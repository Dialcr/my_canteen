using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;
using Microsoft.Extensions.Logging;

namespace Canteen.Services.Services;

public class RequestServices(
    EntitiesContext context,
    ILogger<RequestServices> logger,
    ProductServices services,
    MenuServices menuServices) : CustomServiceBase(context)
{
    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> AddProductsToRequestAsync(
        int requestId,
        List<int> productIds,
        DateTime dateTime)
    {
        var request = await _context.Requests
            .Include(r => r.Order)
            .Include(r => r.RequestProducts)
            .SingleOrDefaultAsync(r =>
                r.Id == requestId &&
                r.Status.Equals(RequestStatus.Planned));

        if (request is null)
        {
            return new ResponseErrorDto
            {
                Status = 400,
                Title = "Request not found",
                Detail = $"The request with id {requestId} was not found or is not in the planned status"
            };
        }

        var dayMenuResult = menuServices.GetMenuByEstablishmentAndDate(requestId, dateTime);

        if (dayMenuResult.IsT1)
        {
            return dayMenuResult.AsT0;
        }

        var dayMenu = dayMenuResult.AsT1;

        var availableProductsResult = services.GetCantneeProductsByMenu(dayMenu);

        if (availableProductsResult.IsT0)
        {
            return availableProductsResult.AsT0;
        }

        var availableProducts = availableProductsResult.AsT1;

        var productsToAdd = new List<Product>();

        foreach (var productId in productIds)
        {
            var product = availableProducts
                .FirstOrDefault(p =>
                    p.Id == productId &&
                    p.Quantity > 0);

            if (product is null)
            {
                return new ResponseErrorDto
                {
                    Status = 400,
                    Title = "Insufficient stock",
                    Detail = $"The product with id {productId} is either out of stock or unavailable"
                };
            }

            productsToAdd.Add(product.Product);
            product.Quantity--;
        }

        request.RequestProducts ??= new List<RequestProduct>();
        productsToAdd.ForEach(x =>
        {
            request.RequestProducts.Add(new RequestProduct()
            {
                ProductId = x.Id,
                RequestId = request.Id
            });
        });
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<OneOf<ResponseErrorDto, CartOutputDto>> CreateRequestAsync(
        CreateRequestInputDto createRequestInputDto,
        int userId)
    {
        var cart = await _context.Carts.Include(x=>x.Requests)
            .FirstOrDefaultAsync(x=> x.EstablishmentId==createRequestInputDto.EstablishmentId
        && x.UserId==userId);
        if (cart is null)
        {
            cart = new CanteenCart()
            {
                CreatedAt = DateTime.Now,
                UserId = userId,
                Requests = new List<CanteenRequest>(),
                EstablishmentId = createRequestInputDto.EstablishmentId
            };
            _context.Carts.Add(cart);
        }

        var estableshimentProducts =_context.Products.Where(x => x.EstablishmentId == createRequestInputDto.EstablishmentId);
        

        var requestProducts = createRequestInputDto.RequestProducts
            .Select(x => new RequestProduct()
        {
            ProductId = x.ProductId,
            Quantity = x.Quantity,
            UnitPrice = estableshimentProducts.FirstOrDefault(p=>p.Id == x.ProductId)!.Price

        }).ToList();
        if (requestProducts.Count()!=createRequestInputDto.RequestProducts.Count)
        {
            return new ResponseErrorDto()
            {
              Status  = 400,
              Title = "Invalid request",
              Detail = "One or more products are not available"
            };
        }
        var request = new CanteenRequest
        {
            UserId = userId,
            CreatedAt = DateTime.Now,
            DeliveryDate = createRequestInputDto.DeliveryDate,
            DeliveryLocation = createRequestInputDto.DeliveryLocation,
            TotalAmount = requestProducts.Sum(x=>x.UnitPrice * x.Quantity),
            DeliveryAmount = createRequestInputDto.DeliveryAmount,
            Status = RequestStatus.Planned,
            DeliveryTimeId = createRequestInputDto.DeliveryTimeId,
            RequestProducts = requestProducts
        };
        
        cart!.Requests!.Add(request);
        await _context.SaveChangesAsync();
        
        return CartingCartExtention.ToCanteenCartDto(cart);
    }
    private OneOf<ResponseErrorDto, CanteenRequest> GetAviableProduct(Menu menuDay, CanteenRequest canteenRequestPlanning)
    {
        CanteenRequest newCanteenRequest = new CanteenRequest()
        {
            RequestProducts = new List<RequestProduct>()
        };
        foreach (var requestProduct in canteenRequestPlanning.RequestProducts!)
        {
            var aviableProduct = menuDay.MenuProducts!.FirstOrDefault(x => x.Product!.Id == requestProduct.ProductId && x.Quantity >= requestProduct.Quantity );
            if (aviableProduct is null)
            {
                return new ResponseErrorDto()
                {
                    Status  = 400,
                    Title = "Insufficient stock",
                    Detail = $"The product with id {requestProduct.ProductId} does not have sufficient stock"
                };
            }
            else
            {
                newCanteenRequest.RequestProducts.Add(new RequestProduct()
                {
                    ProductId = requestProduct.ProductId,
                    RequestId = canteenRequestPlanning.Id,
                    Quantity = requestProduct.Quantity
                });
                aviableProduct.Quantity -= requestProduct.Quantity;
            }
        }

        return newCanteenRequest;
    }

    public async Task<OneOf<ResponseErrorDto, ICollection<CanteenRequest>>> RequestsListAsync(int userId)
    {
        var requests = await _context.Requests
            .Include(x=>x.RequestProducts)
            .ThenInclude(x=>x.Product)
            .Where(x =>
                x.UserId == userId &&
                !x.Status.Equals(RequestStatus.Cancelled))
            .ToListAsync();

        if (requests.Count > 0)
            return requests;

        logger.LogError("The user with id {userId} has no requests", userId);

        return new ResponseErrorDto()
        {
            Status = 400,
            Title = "Requests not found",
            Detail = $"The user with id {userId} has no requests"
        };
    }

    public async Task<OneOf<ResponseErrorDto, List<CanteenRequest>>> HistoryRequestAsync(int userId)
    {
        var requests = await _context.Requests
            .Where(x =>
                x.UserId == userId &&
                (x.Status.Equals(RequestStatus.Cancelled) ||
                 x.Status.Equals(RequestStatus.Delivered)))
            .ToListAsync();

        if (requests.Count > 0)
        {
            return requests;
        }

        logger.LogError("The user with id {userId} has no requests delivered or cancelled", userId);

        return new ResponseErrorDto()
        {
            Status = 400,
            Title = "Requests not found",
            Detail = $"The user with id {userId} has no requests delivered or cancelled"
        };
    }

    
    
    public async Task<OneOf<ResponseErrorDto, IEnumerable<ProductOutputDto>, RequestOutputDto>> MoveRequestIntoOrderAsync(
        int requestId,
        DateTime newDeliveryDate)
    {
        var request = _context.Requests
            .Include(x => x.RequestProducts)!.ThenInclude(requestProduct => requestProduct.Product)
            .Include(x => x.Order)
            .SingleOrDefault(x =>
                x.Id == requestId &&
                x.Status.Equals(RequestStatus.Planned));

        if (request is null)
        {
            return new ResponseErrorDto()
            {
                Status = 400,
                Title = "Request not found",
                Detail = $"Request with id {requestId} and status {RequestStatus.Planned} not found"
            };
        }

        var menuChangeDayResult = menuServices.GetMenuByEstablishmentAndDate(request.Order.EstablishmentId
            , newDeliveryDate);

        if (menuChangeDayResult.TryPickT0(out var error1, out var menuChangeDay))
        {
            return error1;
        }

        var menuOrigineDayResult = menuServices.GetMenuByEstablishmentAndDate(request.Order.EstablishmentId
            , request.DeliveryDate);

        if (menuOrigineDayResult.TryPickT0(out var error2, out var menuOriginDay))
        {
            logger.LogError($"Error status {error2.Status} Detail:{error2.Detail}");
            return error2;
        }

        var productsNotFounfd = request.RequestProducts
            .Where(x =>
                !menuChangeDay.MenuProducts
                    .Any(y =>
                        y.Product.Id == x.Id ||
                        (y.Product.Id == x.Id && y.Quantity < x.Quantity)))
            .Select(x=>x.Product)
            .ToList();
        
        if (productsNotFounfd.Count > 0)
        {
            return productsNotFounfd.Select(x=>x.ToProductOutputDto()).ToList();
        }

        foreach (var dayProdcut in menuOriginDay.MenuProducts)
        {
            var requestproduct = request.RequestProducts!.FirstOrDefault(x =>
                x.ProductId == dayProdcut.CanteenProductId);
            if ( requestproduct is not null)
            {
                dayProdcut.Quantity += requestproduct.Quantity;
            }
        };
        foreach (var dayProdcut in menuChangeDay.MenuProducts)
        {
            
            var requestproduct = request.RequestProducts!.FirstOrDefault(x =>
                x.ProductId == dayProdcut.CanteenProductId);
            if ( requestproduct is not null)
            {
                dayProdcut.Quantity -= requestproduct.Quantity;
            }
        };

        request.DeliveryDate = newDeliveryDate;
        request.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return request.ToCanteenRequestWithProductsDto();
    }
    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> MoveRequestIntoCartAsync(
        int requestId,
        DateTime newDeliveryDate)
    {
        var request =  await _context.Requests
            .SingleOrDefaultAsync(x =>
                x.Id == requestId &&
                x.Status.Equals(RequestStatus.Planned));

        if (request is null)
        {
            return new ResponseErrorDto()
            {
                Status = 400,
                Title = "Request not found",
                Detail = $"Request with id {requestId} and status {RequestStatus.Planned} not found"
            };
        }
        
        request.DeliveryDate = newDeliveryDate;
        request.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return request;
    }

    public OneOf<ResponseErrorDto, RequestOutputDto> GetRequerstInfoById(int requestId)
    {
        var request = _context.Requests
            .Include(x => x.RequestProducts!.OrderBy(y => y.Product.Category)
                .ThenBy(z => z.Product)).ThenInclude(requestProduct => requestProduct.Product)
            .Include(canteenRequest => canteenRequest.DeliveryTime)
            .SingleOrDefault(x => x.Id == requestId);

        if (request is not null)
        {
            
            return request.ToCanteenRequestWithProductsDto();
        }
        return new ResponseErrorDto()
        {
            Status = 400,
            Title = "Request not found",
            Detail = $"Request with id {requestId} not found"
        };
    }
    
    public OneOf<ICollection<RequestInputDto>, CanteenRequest> AllProductsOk(
        CanteenRequest canteenRequest, Menu menu)
    {

        canteenRequest.RequestProducts ??= new List<RequestProduct>();
        var productsOutput  = new List<RequestInputDto>();
        foreach (var product in canteenRequest.RequestProducts)
        {
            var existingProduct = canteenRequest.RequestProducts.FirstOrDefault(p => p.ProductId == product.ProductId);
            
            var aviableProduct = menu.MenuProducts!.SingleOrDefault(x=>x.CanteenProductId == product.ProductId);
            if (aviableProduct is null )
            {
               productsOutput.Add(new RequestInputDto()
               {
                   RequestId = canteenRequest.Id,
                   Product = new ProductDayDto
                   {
                       Product = product.Product,
                       Quantity = product.Quantity,
                       ProductId = product.ProductId
                   }
               });
            }
            else if (aviableProduct.Quantity < product.Quantity)
            {
                productsOutput.Add(new RequestInputDto()
                {
                    RequestId = canteenRequest.Id,
                    Product = new ProductDayDto
                    {
                        Product = product.Product,
                        Quantity = product.Quantity - aviableProduct.Quantity,
                        ProductId = product.ProductId
                    }
                });
                
            }
        }

        if (productsOutput.Count > 0)
        {
            return productsOutput;
        }
        return canteenRequest;
    }
    
    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> DiscountFromInventaryAsync(CanteenRequest canteenRequest, int establishmentId)
    {
        var menu = await _context.Menus.Include(menu => menu.MenuProducts!)
            .FirstOrDefaultAsync(x=>x.EstablishmentId == establishmentId 
                                    && x.Date.Date == canteenRequest.DeliveryDate.Date);
        foreach (var requestProduct in canteenRequest.RequestProducts!)
        {
            var existingProduct = canteenRequest.RequestProducts.FirstOrDefault(p => p.ProductId == requestProduct.ProductId);
            if (existingProduct is null)
            {
                return new ResponseErrorDto()
                {
                    Status = 400,
                    Title = "Product not found",
                };
            }
            var aviableProduct = menu!.MenuProducts!.SingleOrDefault(x=>x.CanteenProductId == requestProduct.ProductId);
            if (existingProduct.Quantity>aviableProduct!.Quantity)
            {
                aviableProduct.Quantity -= existingProduct.Quantity;
            }
            
            await _context.SaveChangesAsync();
        }
        return canteenRequest;
    }
}
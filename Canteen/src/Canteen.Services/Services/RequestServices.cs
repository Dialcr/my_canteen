using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;
using Microsoft.Extensions.Logging;

namespace Canteen.Services.Services;

public class RequestServices : CustomServiceBase
{
    private readonly ILogger<RequestServices> _logger;
    private readonly ProductServices _services;
    private readonly MenuServices _menuServices;
    

    public RequestServices(
        EntitiesContext context,
        ILogger<RequestServices> logger,
        ProductServices services,
        MenuServices menuServices)
        : base(context)
    {
        _logger = logger;
        _services = services;
        _menuServices = menuServices;
    }

    public async Task<OneOf<ResponseErrorDto, Request>> AddProductsToRequest(
        int requestId,
        List<int> productIds,
        DateTime dateTime)
    {
        var request = await _context.Requests
            .Include(r => r.Order)
            .Include(r => r.RequestProducts)
            //.ThenInclude(x=>x.Product)
            .SingleOrDefaultAsync(r =>
                r.Id == requestId &&
                r.Status.Equals(RequestStatus.Planned));

        if (request is null)
        {
            return new ResponseErrorDto
            {
                Status = 404,
                Title = "Request not found",
                Detail = $"The request with id {requestId} was not found or is not in the planned status"
            };
        }

        var dayMenuResult = _menuServices.GetMenuByEstablishmentAndDate(requestId, dateTime);

        if (dayMenuResult.IsT1)
        {
            return dayMenuResult.AsT0;
        }

        var dayMenu = dayMenuResult.AsT1;

        var availableProductsResult = await _services.GetCantneeProductsByMenu(dayMenu);

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
            //request.RequestProducts.Add(x);
            request.RequestProducts.Add(new RequestProduct()
            {
                ProductId = x.Id,
                RequestId = request.Id
            });
        });
        //request.Products.Add(productsToAdd);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<OneOf<ResponseErrorDto, Request>> CreateRequest(
        RequestIntpudDto requestIntpudDto,
        int userId)
    {
        var cart = new CanteenCart()
        {
            CreatedAt = DateTime.Now,
            UserId = userId,
            Requests = new List<Request>(),
            EstablishmentId = requestIntpudDto.EstablishmentId
            
            
        };
        //como los cart se eliminan luego de hacer el checkout, si estableshimentid != de 0 quiere decir que
        //debe haber algun carrito de ese usuario para ese estableciemiento y ese usuario
        if (requestIntpudDto.EstablishmentId != 0)
        {
            cart = _context.Carts.FirstOrDefault(x => x.EstablishmentId == requestIntpudDto.EstablishmentId 
                                                      && x.UserId==userId);
            if (cart is null)
            {
                return new ResponseErrorDto()
                {
                    Status = 404,
                    Title = "Cart not found",
                    Detail = $"The cart with id {requestIntpudDto.EstablishmentId} has not found with status"
                };
            }
        }
        else
        {
            _context.Carts.Add(cart);
        }

        var request = new Request
        {
            UserId = userId,
            CreatedAt = DateTime.Now,
            DeliveryDate = requestIntpudDto.DeliveryDate,
            DeliveryLocation = requestIntpudDto.DeliveryLocation,
            TotalAmount = requestIntpudDto.RequestProducts.Sum(x=>x.Product.Price),
            DeliveryAmount =requestIntpudDto.DeliveryAmount,
            Status = RequestStatus.Planned
        };
        
        cart.Requests!.Add(request);
        //_context.Carts.Add(cart);
        //_context.Requests.Add(request);
        await _context.SaveChangesAsync();

        return request;
    }

    
    
    
    //todo: elimminar este metodo
    private OneOf<ResponseErrorDto, Request> GetAviableProduct(Menu menuDay, Request requestPlanning)
    {
        Request newRequest = new Request()
        {
            RequestProducts = new List<RequestProduct>()
        };
        foreach (var requestProduct in requestPlanning.RequestProducts!)
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
                newRequest.RequestProducts.Add(new RequestProduct()
                {
                    ProductId = requestProduct.ProductId,
                    RequestId = requestPlanning.Id,
                    Quantity = requestProduct.Quantity
                });
                aviableProduct.Quantity -= requestProduct.Quantity;
            }
        }

        return newRequest;
    }

    public async Task<OneOf<ResponseErrorDto, List<Request>>> RequestsList(int userId)
    {
        var requests = await _context.Requests
            .Where(x =>
                x.UserId == userId &&
                !x.Status.Equals(RequestStatus.Cancelled))
            .ToListAsync();

        if (requests.Count > 0)
            return requests;

        _logger.LogError("The user with id {userId} has no requests", userId);

        return new ResponseErrorDto()
        {
            Status = 400,
            Title = "Requests not found",
            Detail = $"The user with id {userId} has no requests"
        };
    }

    public async Task<OneOf<ResponseErrorDto, List<Request>>> HistoryRequest(int userId)
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

        _logger.LogError("The user with id {userId} has no requests delivered or cancelled", userId);

        return new ResponseErrorDto()
        {
            Status = 400,
            Title = "Requests not found",
            Detail = $"The user with id {userId} has no requests delivered or cancelled"
        };
    }

    
    
    public async Task<OneOf<ResponseErrorDto, IEnumerable<Product>, Request>> MoveRequestIntoOrder(
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
                Status = 404,
                Title = "Request not found",
                Detail = $"Request with id {requestId} and status {RequestStatus.Planned} not found"
            };
        }

        var menuChangeDayResult = _menuServices.GetMenuByEstablishmentAndDate(request.Order.EstablishmentId
            , newDeliveryDate);

        if (menuChangeDayResult.TryPickT0(out var error1, out var menuChangeDay))
        {
            return error1;
        }

        var menuOrigineDayResult = _menuServices.GetMenuByEstablishmentAndDate(request.Order.EstablishmentId
            , request.DeliveryDate);

        if (menuOrigineDayResult.TryPickT0(out var error2, out var menuOriginDay))
        {
            _logger.LogError($"Error status {error2.Status} Detail:{error2.Detail}");
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
            return productsNotFounfd;
        }

        foreach (var dayProdcut in menuOriginDay.MenuProducts)
        {
            //if (request.RequestProducts.Contains(dayProdcut.Product)) dayProdcut.Quantity--;
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

        return request;
    }
    public async Task<OneOf<ResponseErrorDto, Request>> MoveRequestIntoCart(
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
                Status = 404,
                Title = "Request not found",
                Detail = $"Request with id {requestId} and status {RequestStatus.Planned} not found"
            };
        }
        
        request.DeliveryDate = newDeliveryDate;
        request.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return request;
    }

    public OneOf<ResponseErrorDto, Request> GetRequerstInfoById(int requestId)
    {
        var request = _context.Requests
            //.Include(x => x.RequestProducts!.OrderByDescending(y => y.Product.Category)
            .Include(x => x.RequestProducts!.OrderBy(y => y.Product.Category)  
                //.Include(x => x.RequestProducts!.OrderBy(y => Enum.Parse<ProductCategory>(y.Product.Category))  
                .ThenBy(z => z.Product))
            .SingleOrDefault(x => x.Id == requestId);

        if (request is not null)
        {
            return request;
        }

        return new ResponseErrorDto()
        {
            Status = 400,
            Title = "Request not found",
            Detail = $"Request with id {requestId} not found"
        };
    }
    
    public OneOf<ICollection<RequestInputDto>, Request> AllProductsOk(
        Request request, Menu menu)
    {

        request.RequestProducts ??= new List<RequestProduct>();
        var productsOutput  = new List<RequestInputDto>();
        foreach (var product in request.RequestProducts)
        {
            var existingProduct = request.RequestProducts.FirstOrDefault(p => p.ProductId == product.ProductId);
            
            var aviableProduct = menu.MenuProducts!.SingleOrDefault(x=>x.CanteenProductId == product.ProductId);
            if (aviableProduct is null )
            {
               productsOutput.Add(new RequestInputDto()
               {
                   RequestId = request.Id,
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
                    RequestId = request.Id,
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
        return request;
    }
    
    //todo: fix the return type
    public async Task<OneOf<ResponseErrorDto, Request>> DiscountFromInventary(Request request, int establishmentId)
    {
        var menu = await _context.Menus.Include(menu => menu.MenuProducts!)
            .FirstOrDefaultAsync(x=>x.EstablishmentId == establishmentId 
                                    && x.Date == request.DeliveryDate);
        foreach (var requestProduct in request.RequestProducts!)
        {
            var existingProduct = request.RequestProducts.FirstOrDefault(p => p.ProductId == requestProduct.ProductId);
            if (existingProduct is null)
            {
                return new ResponseErrorDto()
                {
                    Status = 404,
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
        return request;
    }
}
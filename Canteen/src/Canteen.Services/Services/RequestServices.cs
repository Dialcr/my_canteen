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
    private readonly CanteenOrderServices _orderServices;
    

    public RequestServices(
        EntitiesContext context,
        ILogger<RequestServices> logger,
        ProductServices services,
        MenuServices menuServices, CanteenOrderServices orderServices)
        : base(context)
    {
        _logger = logger;
        _services = services;
        _menuServices = menuServices;
        _orderServices = orderServices;
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
        int userId,
        DateTime deliveryDate,
        string deliveryLocation,
        List<RequestProduct> requestProducts
        ,int establishmentId
        ,decimal deliveryAmount)
    {
        var order = new Order
        {
            CreatedAt = DateTime.Now,
            Status = OrderStatus.Created,
            UserId = userId,
            Requests = new List<Request>(),
            EstablishmentId = establishmentId!
            
            
        };

        if (establishmentId != 0)
        {
            order = _context.Orders.FirstOrDefault(x => x.EstablishmentId == establishmentId 
                                                        && x.Status == OrderStatus.Created
                                                        && x.UserId==userId);
            if (order is null)
            {
                return new ResponseErrorDto()
                {
                    Status = 404,
                    Title = "Order not found",
                    Detail = $"The order with id {establishmentId} has not found with status {OrderStatus.Created}"
                };
            }
        }
        else
        {
            _context.Orders.Add(order);
            
        }

        var request = new Request
        {
            UserId = userId,
            CreatedAt = DateTime.Now,
            DeliveryDate = deliveryDate,
            DeliveryLocation = deliveryLocation,
            TotalAmount = requestProducts.Sum(x=>x.Product.Price),
            DeliveryAmount = deliveryAmount,
            OrderId = order.Id,
        };
        
        order.Requests!.Add(request);
        _context.Orders.Add(order);
        //_context.Requests.Add(request);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<OneOf<ResponseErrorDto, Request>> EditRequest(
        int requestId,
        DateTime deliveryDate,
        string deliveryLocation,
        List<MenuProductInypodDto> productDayDtos)
    {
       
        var request = await _context.Requests
            .Include(r => r.RequestProducts)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
        {
            return new ResponseErrorDto
            {
                Status = 404,
                Title = "Request not found",
                Detail = $"The request with id {requestId} was not found"
            };
        }

        
        if (!request.Status.Equals(RequestStatus.Planned))
        {
            return new ResponseErrorDto
            {
                Status = 400,
                Title = "Invalid request status",
                Detail = "The request status has changed and cannot be edited"
            };
        }

        //var originalProducts = request.Products.ToList();
        request.RequestProducts ??= new List<RequestProduct>();
        var originalProducts = request.RequestProducts;
        
        foreach (var product in productDayDtos)
        {
            //var existingProduct = request.Products.FirstOrDefault(p => p.Id == product.Product.Id);
            var existingProduct = request.RequestProducts.FirstOrDefault(p => p.ProductId == product.ProductId);

            /*
            if (existingProduct is null)
            {
                return new ResponseErrorDto
                {
                    Status = 400,
                    Title = "Invalid product",
                    Detail = $"The product with id {product.Product.Id} is not associated with the request"
                };
            }
            */
            var aviableProduct = _context.MenuProducts.SingleOrDefault(x=>x.Id == product.MenuProductId);
            if (aviableProduct is null)
            {
                return new ResponseErrorDto()
                {
                    Status = 404,
                    Title = "Product not found",
                    Detail = $"The product with id {product.Product.Id} has not been found"
                };
            }
            if (existingProduct is not null)
            {
                //Aqui debo modificar la cantidad de un producto en que ya estaba en el pedido
                if (product.Quantity < existingProduct.Quantity && product.Quantity!=0)
                {
                    existingProduct.Quantity = product.Quantity;
                    aviableProduct.Quantity += existingProduct.Quantity - product.Quantity;
                }
                else if (product.Quantity > existingProduct.Quantity && product.Quantity- existingProduct.Quantity<=aviableProduct.Quantity 
                                                                     && product.Quantity!=0)
                {
                    existingProduct.Quantity = product.Quantity;
                    aviableProduct.Quantity -= product.Quantity-existingProduct.Quantity ;
                }
                else
                {
                    return new ResponseErrorDto
                    {
                        Status = 400,
                        Title = "Insufficient stock",
                        Detail = $"The product with id {product.Product.Id} does not have sufficient stock"
                    };
                }
                
            }
            else
            {
                //Esto es para agregar un nuevo producto al pedido 
                //if (product.Quantity > availableProduct.AsT1.Quantity)
                if (product.Quantity > aviableProduct.Quantity)
                {
                    return new ResponseErrorDto
                    {
                        Status = 400,
                        Title = "Insufficient stock",
                        Detail = $"The product with id {product.Product.Id} does not have sufficient stock"
                    };
                }

                //descuenta la cantidad del producto
                aviableProduct.Quantity -= product.Quantity;
                //agrega la cantidad al pedido
                request.RequestProducts.Add(new RequestProduct()
                {
                    ProductId = product.ProductId,
                    RequestId = request.Id,
                    Quantity = product.Quantity
                });
            }
        }

        request.DeliveryDate = deliveryDate;
        request.DeliveryLocation = deliveryLocation;
        await _orderServices.UpdateTotals(request.OrderId!.Value);
        await _orderServices.ApplyDiscountToOrder(request.OrderId!.Value);
        request.TotalAmount = request.RequestProducts.Sum(x=>x.Product.Price);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<OneOf<ResponseErrorDto, Request>> PlanningRequest(
        int requestId,
        int establishmentId,
        DateTime newDateTime)
    {
        var request = await _context.Requests
            .Include(x => x.Order)
            .Include(request => request.RequestProducts!)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
        {
            return new ResponseErrorDto
            {
                Status = 404,
                Title = "Request not found",
                Detail = $"The request with id {requestId} was not found"
            };
        }

        if (!request.Status.Equals(RequestStatus.Planned))
        {
            return new ResponseErrorDto
            {
                Status = 400,
                Title = "Invalid request status",
                Detail = "The request status has changed and cannot be planned"
            };
        }

        var dayMenuResult = _menuServices.GetMenuByEstablishmentAndDate(establishmentId, newDateTime);
        
        if (dayMenuResult.IsT1)
        {
            return dayMenuResult.AsT0;
        }

        var dayMenu = dayMenuResult.AsT1;

        //var availableProductsResult =  GetAviableProduct(dayMenu,request);
        Request newRequest = new Request()
        {
            DeliveryDate = newDateTime,
            DeliveryLocation = request.DeliveryLocation,
            Status = RequestStatus.Planned,
            OrderId = request.OrderId,
            UserId = request.UserId,
            CreatedAt = DateTimeOffset.Now,
            TotalAmount = request.TotalAmount,
            RequestProducts = new List<RequestProduct>()
        };
        foreach (var requestProduct in request.RequestProducts!)
        {
            var aviableProduct = dayMenu.MenuProducts!.FirstOrDefault(x => x.Product!.Id == requestProduct.ProductId && x.Quantity >= requestProduct.Quantity );
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
                    RequestId = request.Id,
                    Quantity = requestProduct.Quantity
                });
                aviableProduct.Quantity -= requestProduct.Quantity;
            }
        }
        _context.Requests.Add(newRequest);
        var order = await _context.Orders.FirstOrDefaultAsync(x=>x.Id==request.OrderId);
        order!.PrductsTotalAmount += newRequest.TotalAmount;
        order.DeliveryTotalAmount += newRequest.DeliveryAmount;
        
        await _orderServices.ApplyDiscountToOrder(request.OrderId!.Value);
        
        await _context.SaveChangesAsync();

        return newRequest;
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

    public async Task<OneOf<ResponseErrorDto, Request>> CancelRequest(int requestId)
    {
        var request = _context.Requests.Include(x => x.Order)
            .Include(x => x.RequestProducts)
            .Include(x => x.Order)
            .SingleOrDefault(x =>
                x.Id == requestId &&
                x.Status.Equals(RequestStatus.Planned));

        if (request is not null)
        {
            request.Status = RequestStatus.Cancelled;

            Menu originDayMenu = _context.Menus
                .Include(x=>x.MenuProducts)
                .SingleOrDefault(x => x.Date == request.DeliveryDate! && x.EstablishmentId == request.Order!.EstablishmentId)!;
            foreach (var dayProduct in originDayMenu.MenuProducts!)
            {
            
                //if (response.RequestProducts.Contains(dayProduct.CanteenProductId)) dayProduct.Quantity--;
                var requestproduct = request.RequestProducts!.FirstOrDefault(x =>
                    x.ProductId == dayProduct.CanteenProductId);
                if ( requestproduct is not null)
                {
                    dayProduct.Quantity += requestproduct.Quantity;
                }
            };
            
            await _orderServices.UpdateTotals(request.OrderId!.Value);
            await _orderServices.ApplyDiscountToOrder(request.OrderId!.Value);
            await _orderServices.CloseOrderIfAllRequestsClosed(request.OrderId.Value);
            
            await _context.SaveChangesAsync();
            return request;
        }

        return new ResponseErrorDto()
        {
            Status = 404,
            Title = "Request not found",
            Detail = $"Request with id {requestId} and status {RequestStatus.Planned} not found"
        };
    }

    public async Task<OneOf<ResponseErrorDto, IEnumerable<Product>, Request>> MoveRequest(
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
}
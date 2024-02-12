using Canteen.DataAccess;
using Canteen.Services.Dto;
using Microsoft.Extensions.Logging;

namespace Canteen.Services;

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
            .Include(r => r.Products)
            .SingleOrDefaultAsync(r =>
                r.Id == requestId &&
                r.Status.Equals(RequestStatusName.ToStr(RequestStatus.Planned)));

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

        if (availableProductsResult.IsT1)
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

        request.Products.AddRange(productsToAdd);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<Request> CreateRequest(
        int userId,
        DateTime deliveryDate,
        string deliveryLocation,
        decimal totalAmount)
    {
        var order = new Order
        {
            CreatedAt = DateTime.Now,
            Status = "Open"
        };

        var request = new Request
        {
            IdUser = userId,
            CreatedAt = DateTime.Now,
            DeliveryDate = deliveryDate,
            DeliveryLocation = deliveryLocation,
            TotalAmount = totalAmount,
            Order = order
        };

        _context.Orders.Add(order);
        _context.Requests.Add(request);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<OneOf<ResponseErrorDto, Request>> EditRequest(
        int requestId,
        DateTime deliveryDate,
        string deliveryLocation,
        List<ProductDayDto> products)
    {
        var request = await _context.Requests
            .Include(r => r.Products)
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

        if (request.Status != RequestStatusName.ToStr(RequestStatus.Planned))
        {
            return new ResponseErrorDto
            {
                Status = 400,
                Title = "Invalid request status",
                Detail = "The request status has changed and cannot be edited"
            };
        }

        var originalProducts = request.Products.ToList();

        foreach (var product in products)
        {
            var existingProduct = request.Products.FirstOrDefault(p => p.Id == product.Product.Id);

            if (existingProduct is null)
            {
                return new ResponseErrorDto
                {
                    Status = 400,
                    Title = "Invalid product",
                    Detail = $"The product with id {product.Product.Id} is not associated with the request"
                };
            }

            var availableProduct = _services.GetCantneeProductById(product.Product.Id);

            if (availableProduct.IsT0)
            {
                return availableProduct.AsT0;
            }

            if (product.Cantity > availableProduct.AsT1.Cantity)
            {
                return new ResponseErrorDto
                {
                    Status = 400,
                    Title = "Insufficient stock",
                    Detail = $"The product with id {product.Product.Id} does not have sufficient stock"
                };
            }

            existingProduct.Cantity = product.Cantity;

            availableProduct.AsT1.Cantity +=
                (originalProducts.FirstOrDefault(p => p.Id == product.Product.Id)?.Cantity ?? 0) - product.Cantity;
        }

        request.DeliveryDate = deliveryDate;
        request.DeliveryLocation = deliveryLocation;
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<OneOf<ResponseErrorDto, Request>> PlanningRequest(
        int requestId,
        DateTime newDateTime)
    {
        var request = await _context.Requests
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

        if (request.Status != RequestStatusName.ToStr(RequestStatus.Planned))
        {
            return new ResponseErrorDto
            {
                Status = 400,
                Title = "Invalid request status",
                Detail = "The request status has changed and cannot be planned"
            };
        }

        var dayMenuResult = _menuServices.GetMenuByEstablishmentAndDate(requestId, newDateTime);

        if (dayMenuResult.IsT1)
        {
            return dayMenuResult.AsT0;
        }

        var dayMenu = dayMenuResult.AsT1;

        var availableProductsResult = await _services.GetCantneeProductsByMenu(dayMenu);

        if (availableProductsResult.IsT1)
        {
            return availableProductsResult.AsT0;
        }

        var availableProducts = availableProductsResult.AsT1;

        foreach (var product in request.Products)
        {
            var availableProduct = availableProducts.FirstOrDefault(p => p.Id == product.Id);

            if (availableProduct is null || availableProduct.Quantity <= 0)
            {
                return new ResponseErrorDto
                {
                    Status = 400,
                    Title = "Insufficient stock",
                    Detail = $"The product with id {product.Id} is either out of stock or unavailable"
                };
            }
        }

        request.DeliveryDate = newDateTime;
        await _context.SaveChangesAsync();

        return request;
    }

    public OneOf<ResponseErrorDto, List<Request>> RequestsList(int userId)
    {
        var requests = _context.Requests
            .Where(x =>
                x.IdUser == userId &&
                !x.Status.Equals(RequestStatusName.ToStr(RequestStatus.Cancelled)))
            .ToList();

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

    public OneOf<ResponseErrorDto, List<Request>> HistoryRequest(int userId)
    {
        var requests = _context.Requests
            .Where(x =>
                x.IdUser == userId &&
                (x.Status.Equals(RequestStatusName.ToStr(RequestStatus.Cancelled)) ||
                x.Status.Equals(RequestStatusName.ToStr(RequestStatus.Delivered))))
            .ToList();

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
        var response = _context.Requests.Include(x => x.Order)
            .Include(x => x.Products)
            .Include(x => x.Order)
            .SingleOrDefault(x =>
                x.Id == requestId &&
                x.Status.Equals(RequestStatusName.ToStr(RequestStatus.Planned)));

        if (response is not null)
        {
            response.Status = RequestStatusName.ToStr(RequestStatus.Cancelled);

            Menu originDayMenu = _context.Menus.SingleOrDefault(x =>
                x.Date == response.DeliveryDate! && x.IdEstablishment == response.Order.IdEstablishment)!;
            originDayMenu.ProductsDay.ForEach(x =>
            {
                if (response.Products.Contains(x.Product)) x.Quantity--;
            });

            //todo: hacer el descuento del dinero en la orden
            ///summaray
            /// hace falta descontar un porciento del dinero que debe definir negocio en la orden
            /// se debe llamar al metodo de calcular y setear el pago de la orden
            /*
            response.Products.ForEach(x =>
            {
                var originProduct = originDayMenu.ProductsDay.SingleOrDefault(y => x.Id == y.Product.Id);
                originProduct!.Cantity++;
            });
            var negocio = retunrs.SingleOrDefault(x=>x.EstablishmentId == response.Order.IdEstablishment);
            var moneyReturn = response.TotalAmount * negocio.ReturnsProcent;
            */

            await _context.SaveChangesAsync();
            return response;
        }

        return new ResponseErrorDto()
        {
            Status = 400,
            Title = "Request not found",
            Detail = $"Request with id {requestId} and status {RequestStatus.Planned} not found"
        };
    }

    public async Task<OneOf<ResponseErrorDto, List<Product>, Request>> MoveRequest(
        int requestId,
        DateTime NewDeliveryDate)
    {
        var request = _context.Requests
            .Include(x => x.Products)
            .Include(x => x.Order)
            .SingleOrDefault(x =>
                x.Id == requestId &&
                x.Status.Equals(RequestStatusName.ToStr(RequestStatus.Planned)));

        if (request is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Request not found",
                Detail = $"Request with id {requestId} and status {RequestStatusName.ToStr(RequestStatus.Planned)} not found"
            };
        }

        var menuChangeDayResult = _menuServices.GetMenuByEstablishmentAndDate(request.Order.IdEstablishment
            , NewDeliveryDate);

        if (menuChangeDayResult.TryPickT0(out var error1, out var menuChangeDay))
        {
            return error1;
        }

        var menuOrigineDayResult = _menuServices.GetMenuByEstablishmentAndDate(request.Order.IdEstablishment
            , request.DeliveryDate);

        if (menuOrigineDayResult.TryPickT0(out var error2, out var menuOriginDay))
        {
            _logger.LogError($"Error status {error2.Status} Detail:{error2.Detail}");
            return error2;
        }

        var productsNotFounfd = request.Products
            .Where(x =>
                !menuChangeDay.ProductsDay
                    .Any(y =>
                        y.Product.Id == x.Id ||
                        (y.Product.Id == x.Id && y.Quantity <= 0)))
            .ToList();

        if (productsNotFounfd.Count > 0)
        {
            return productsNotFounfd;
        }

        menuOriginDay.ProductsDay.ForEach(x =>
        {
            if (request.Products.Contains(x.Product)) x.Quantity--;
        });

        request.Products.ForEach(x =>
        {
            var changePorduct = menuChangeDay.ProductsDay.SingleOrDefault(y => x.Id == y.Product.Id);
            changePorduct!.Quantity--;

            var originProduct = menuOriginDay.ProductsDay.SingleOrDefault(y => x.Id == y.Product.Id);
            originProduct!.Quantity++;
        });

        request.DeliveryDate = NewDeliveryDate;
        request.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return request;
    }

    public OneOf<ResponseErrorDto, Request> GetRequerstInfoById(int requestId)
    {
        var request = _context.Requests
            .Include(x => x.Products.OrderByDescending(x => x.Category)
                .ThenBy(x => x.Name))
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
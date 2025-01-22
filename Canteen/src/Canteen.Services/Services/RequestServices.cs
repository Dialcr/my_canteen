using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.CanteenRequest;
using Canteen.Services.Dto.Mapper;
using Microsoft.Extensions.Logging;

namespace Canteen.Services.Services;

public class RequestServices(
    EntitiesContext context,
    ILogger<RequestServices> logger,
    IProductServices services,
    IMenuServices menuServices) : CustomServiceBase(context), IRequestServices
{
    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> AddProductsToRequestAsync(
        int requestId,
        List<int> productIds,
        DateTime dateTime)
    {
        var request = await context.Requests
            .Include(r => r.Order)
            .Include(r => r.RequestProducts)
            .Include(x => x.DeliveryTime)
            .SingleOrDefaultAsync(r =>
                r.Id == requestId &&
                r.Status == RequestStatus.Planned);

        if (request is null)
        {
            return Error("Request not found",
                $"The request with id {requestId} was not found or is not in the planned status",
                400);
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
                return Error("Insufficient stock",
                    $"The product with id {productId} is either out of stock or unavailable",
                    400);
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
        await context.SaveChangesAsync();

        return request;
    }

    public async Task<OneOf<ResponseErrorDto, CartOutputDto>> CreateRequestAsync(
        CreateRequestInputDto createRequestInputDto,
        int userId)
    {
        var cart = await context.Carts.Include(x => x.Requests)
            .FirstOrDefaultAsync(x => x.EstablishmentId == createRequestInputDto.EstablishmentId
                                     && x.UserId == userId);
        if (cart is null)
        {
            cart = new CanteenCart()
            {
                CreatedAt = DateTime.Now,
                UserId = userId,
                Requests = new List<CanteenRequest>(),
                EstablishmentId = createRequestInputDto.EstablishmentId
            };
            context.Carts.Add(cart);
        }

        var estableshimentProducts = context.Products.Where(x => x.EstablishmentId == createRequestInputDto.EstablishmentId);


        var requestProducts = createRequestInputDto.RequestProducts
            .Select(x => new RequestProduct()
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitPrice = estableshimentProducts.FirstOrDefault(p => p.Id == x.ProductId)!.Price

            }).ToList();
        if (requestProducts.Count() != createRequestInputDto.RequestProducts.Count)
        {
            return Error("Invalid request",
                "One or more products are not available",
                400);
        }
        var request = new CanteenRequest
        {
            UserId = userId,
            CreatedAt = DateTime.Now,
            DeliveryDate = createRequestInputDto.DeliveryDate,
            DeliveryLocation = createRequestInputDto.DeliveryLocation,
            TotalAmount = requestProducts.Sum(x => x.UnitPrice * x.Quantity),
            DeliveryAmount = createRequestInputDto.DeliveryAmount,
            Status = RequestStatus.Planned,
            //todo add delivery type verification
            DeliveryTimeId = createRequestInputDto.DeliveryTimeId,
            RequestProducts = requestProducts
        };

        cart!.Requests!.Add(request);
        await context.SaveChangesAsync();

        return cart.ToCanteenCartDto();
    }
    private OneOf<ResponseErrorDto, CanteenRequest> GetAviableProduct(Menu menuDay, CanteenRequest canteenRequestPlanning)
    {
        CanteenRequest newCanteenRequest = new CanteenRequest()
        {
            RequestProducts = new List<RequestProduct>()
        };
        foreach (var requestProduct in canteenRequestPlanning.RequestProducts!)
        {
            var aviableProduct = menuDay.MenuProducts!.FirstOrDefault(x => x.Product!.Id == requestProduct.ProductId && x.Quantity >= requestProduct.Quantity);
            if (aviableProduct is null)
            {
                return Error("Insufficient stock",
                    $"The product with id {requestProduct.ProductId} does not have sufficient stock",
                    400);
            }

            newCanteenRequest.RequestProducts.Add(new RequestProduct()
            {
                ProductId = requestProduct.ProductId,
                RequestId = canteenRequestPlanning.Id,
                Quantity = requestProduct.Quantity
            });
            aviableProduct.Quantity -= requestProduct.Quantity;

        }

        return newCanteenRequest;
    }

    public async Task<OneOf<ResponseErrorDto, ICollection<CanteenRequest>>> RequestsListAsync(int userId)
    {
        var requests = await context.Requests
            .Include(x => x.RequestProducts)
                .ThenInclude(x => x.Product)
            .Include(x => x.DeliveryTime)
            .Where(x =>
                x.UserId == userId &&
                x.Status != RequestStatus.Cancelled)
            .ToListAsync();

        if (requests.Count > 0)
            return requests;

        logger.LogError("The user with id {userId} has no requests", userId);

        return Error("Requests not found",
            $"The user with id {userId} has no requests",
            400);
    }

    public async Task<OneOf<ResponseErrorDto, List<CanteenRequest>>> HistoryRequestAsync(int userId)
    {
        var requests = await context.Requests
            .Include(x => x.DeliveryTime)
            .Where(x =>
                x.UserId == userId &&
                (x.Status == RequestStatus.Cancelled ||
                 x.Status == RequestStatus.Delivered))
            .ToListAsync();

        if (requests.Count > 0)
        {
            return requests;
        }

        logger.LogError("The user with id {userId} has no requests delivered or cancelled", userId);

        return Error("Requests not found",
            $"The user with id {userId} has no requests delivered or cancelled",
            400);
    }



    public async Task<OneOf<ResponseErrorDto, IEnumerable<ProductOutputDto>, RequestOutputDto>> MoveRequestIntoOrderAsync(
        int requestId,
        DateTime newDeliveryDate)
    {
        var request = context.Requests
            .Include(x => x.RequestProducts)!.ThenInclude(requestProduct => requestProduct.Product)
            .Include(x => x.Order)
            .Include(x => x.DeliveryTime)
            .SingleOrDefault(x =>
                x.Id == requestId &&
                x.Status == RequestStatus.Planned);

        if (request is null)
        {
            return Error("Request not found",
                $"Request with id {requestId} and status {RequestStatus.Planned} not found",
                400);
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
            .Select(x => x.Product)
            .ToList();

        if (productsNotFounfd.Count > 0)
        {
            return productsNotFounfd.Select(x => x.ToProductOutputDto()).ToList();
        }

        foreach (var dayProdcut in menuOriginDay.MenuProducts)
        {
            var requestproduct = request.RequestProducts!.FirstOrDefault(x =>
                x.ProductId == dayProdcut.CanteenProductId);
            if (requestproduct is not null)
            {
                dayProdcut.Quantity += requestproduct.Quantity;
            }
        }
        ;
        foreach (var dayProdcut in menuChangeDay.MenuProducts)
        {

            var requestproduct = request.RequestProducts!.FirstOrDefault(x =>
                x.ProductId == dayProdcut.CanteenProductId);
            if (requestproduct is not null)
            {
                dayProdcut.Quantity -= requestproduct.Quantity;
            }
        }
        ;

        request.DeliveryDate = newDeliveryDate;
        request.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return request.ToCanteenRequestWithProductsDto();
    }
    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> MoveRequestIntoCartAsync(
        int requestId,
        DateTime newDeliveryDate)
    {
        var request = await context.Requests
            .Include(x => x.DeliveryTime)
            .SingleOrDefaultAsync(x =>
                x.Id == requestId &&
                x.Status == RequestStatus.Planned);

        if (request is null)
        {
            return Error("Request not found",
                $"Request with id {requestId} and status {RequestStatus.Planned} not found",
                400);
        }

        request.DeliveryDate = newDeliveryDate;
        request.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return request;
    }

    public OneOf<ResponseErrorDto, RequestOutputDto> GetRequerstInfoById(int requestId)
    {
        var request = context.Requests
            .Include(x => x.RequestProducts!.OrderBy(y => y.Product.Category)
                .ThenBy(z => z.Product)).ThenInclude(requestProduct => requestProduct.Product)
            .Include(canteenRequest => canteenRequest.DeliveryTime)
            .SingleOrDefault(x => x.Id == requestId);

        if (request is not null)
        {

            return request.ToCanteenRequestWithProductsDto();
        }
        return Error("Request not found",
            $"Request with id {requestId} not found",
            400);
    }

    public ICollection<RequestProductOutputDto>? AllProductsOk(
        CanteenRequest canteenRequest, Menu menu)
    {

        canteenRequest.RequestProducts ??= new List<RequestProduct>();
        var productsOutput = new List<RequestProductOutputDto>();
        foreach (var requestProduct in canteenRequest.RequestProducts)
        {
            var existingProduct = canteenRequest.RequestProducts.FirstOrDefault(p => p.ProductId == requestProduct.ProductId);

            var aviableProduct = menu.MenuProducts!.SingleOrDefault(x => x.CanteenProductId == requestProduct.ProductId);
            if (aviableProduct is null)
            {
                productsOutput.Add(requestProduct.ToRequestProductOutputDto());
            }
            else if (aviableProduct.Quantity < requestProduct.Quantity)
            {
                var requestSuperconductor = requestProduct.ToRequestProductOutputDto();
                requestSuperconductor.Quantity = requestProduct.Quantity - aviableProduct.Quantity;
                productsOutput.Add(requestProduct.ToRequestProductOutputDto());
            }
        }

        if (productsOutput.Count > 0)
        {
            return productsOutput;
        }
        return null;
    }

    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> DiscountFromInventaryAsync(CanteenRequest canteenRequest, int establishmentId)
    {
        var menu = await context.Menus.Include(menu => menu.MenuProducts!)
            .FirstOrDefaultAsync(x => x.EstablishmentId == establishmentId
                                    && x.Date.Date == canteenRequest.DeliveryDate.Date);
        foreach (var requestProduct in canteenRequest.RequestProducts!)
        {
            var existingProduct = canteenRequest.RequestProducts.FirstOrDefault(p => p.ProductId == requestProduct.ProductId);
            if (existingProduct is null)
            {
                return Error("Product not found",
                    "Product not found",
                    400);
            }
            var aviableProduct = menu!.MenuProducts!.SingleOrDefault(x => x.CanteenProductId == requestProduct.ProductId);
            if (existingProduct.Quantity > aviableProduct!.Quantity)
            {
                aviableProduct.Quantity -= existingProduct.Quantity;
            }

            await context.SaveChangesAsync();
        }
        return canteenRequest;
    }
}
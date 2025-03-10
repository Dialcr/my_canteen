using AvangTur.Application.Extensions;
using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.Establishment;
using Canteen.Services.Dto.Establishment.Mapper;
using Canteen.Services.Dto.Responses;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneOf.Types;

namespace Canteen.Services.Services;

public class EstablishmentService(EntitiesContext context) : CustomServiceBase(context), IEstablishmentService
{

    public PagedResponse<EstablishmentOutputDto> GetAllEstablishments(int page, int perPage, int? category = null, bool useInactive = false)
    {
        var allEstablishment = (useInactive)
        ? context.Establishments.Include(x => x.EstablishmentCategories)
            .Include(x => x.DeliveryTimes).AsEnumerable().Where(x => category == null || x.EstablishmentCategories.Any(y => y.Id == category))
        : context.Establishments.Include(x => x.EstablishmentCategories)
            .Include(x => x.DeliveryTimes).Where(x => x.StatusBase == StatusBase.Active)
            .AsEnumerable().Where(x => category == null || x.EstablishmentCategories.Any(y => y.Id == category)).ToList();

        return allEstablishment.Select(x => EstablishmentExtention.ToEstablishmentOutputDtos(x)).ToPagedResult(page, perPage);
    }
    public PagedResponse<EstablishmentOutputDto> GetMustPopularEstablishments(int page, int perPage)
    {
        var allEstablishment = context.Establishments.Where(x => x.StatusBase == StatusBase.Active)
        .Include(x => x.Orders)
        .Include(x => x.EstablishmentCategories)
        .Include(x => x.DeliveryTimes)
        .OrderByDescending(x => x.Orders.Count());

        return allEstablishment.Select(x => EstablishmentExtention.ToEstablishmentOutputDtos(x)).ToPagedResult(page, perPage);
    }

    public async Task<OneOf<ResponseErrorDto, EstablishmentOutputDto>> GetEstablishmentByIdAsync(int id)
    {
        var establish = await context.Establishments
            .Include(x => x.EstablishmentCategories)
            .Where(x => x.Id == id && x.StatusBase == StatusBase.Active)
            .FirstOrDefaultAsync();

        if (establish is null)
        {
            return Error("Establishment not found",
                "Establishment not found",
                400);
        }

        return establish.ToEstablishmentOutputDto();
    }

    public async Task<OneOf<ResponseErrorDto, Response<NoContent>>> CreateEstablishmentAsync(CreateEstablismentDto establishmentDto)
    {
        if (!establishmentDto.EstablishmentCategory.Any())
        {
            return Error("Establishment must have some category", "Establishment must have some category", 400);
        }
        var existingEstablishment = context.Establishments.FirstOrDefault(x => x.Name == establishmentDto.Name && x.StatusBase == StatusBase.Active);
        if (existingEstablishment is not null)
        {
            return Error("Establishment already exists",
                "Establishment already exists",
                400);
        }
        var establishmentCategory = context.EstablishmentsCategory.Where(x => establishmentDto.EstablishmentCategory.Contains(x.Id)
            && x.StatusBase == StatusBase.Active).ToList();
        if (establishmentCategory.Count() != establishmentDto.EstablishmentCategory.Count())
        {
            return Error("Some Category has not found", "Some Category has not found", 400);
        }
        if (establishmentDto.DeliveryTimes.Any(x => x.StartTime > x.EndTime || !Enum.TryParse(x.DeliveryTimeType, out DeliveryTimeType _)))
        {
            return Error("Some errors into delivery times ",
                            "Some errors into delivery times ",
                            400);

        }
        if (establishmentDto.Name.IsNullOrEmpty())
        {
            return Error("Establishment name cannot be empty",
                            "Establishment name cannot be empty",
                            400);

        }
        var establishment = new Establishment
        {
            DeliveryTimes = establishmentDto.DeliveryTimes.Select(x => new DeliveryTime
            {
                DeliveryTimeType = Enum.Parse<DeliveryTimeType>(x.DeliveryTimeType),
                StartTime = x.StartTime,
                EndTime = x.EndTime,

            }).ToList(),
            Name = establishmentDto.Name,
            Description = establishmentDto.Description,
            Address = establishmentDto.Address,
            StatusBase = StatusBase.Active,
            PhoneNumber = establishmentDto.PhoneNumber,
            EstablishmentCategories = establishmentCategory,
            Image = establishmentDto.Image
        };
        context.Establishments.Add(establishment);
        await context.SaveChangesAsync();
        return new Response<NoContent>();

    }
    public async Task<OneOf<ResponseErrorDto, Response<NoContent>>> UpdateEstablishmentAsync(UpdateEstablismentDto establishmentDto)
    {
        var existingEstablishment = context.Establishments.FirstOrDefault(x => x.Name == establishmentDto.Name && x.StatusBase == StatusBase.Active && x.Id != establishmentDto.Id);
        if (existingEstablishment is not null)
        {
            return Error("Establishment with the same name already exists", "Establishment with the same name already exists", 400);
        }
        var establishment = context.Establishments.Where(x => x.StatusBase == StatusBase.Active && x.Id == establishmentDto.Id)
            .Include(x => x.EstablishmentCategories)
            .Include(x => x.DeliveryTimes)
            .FirstOrDefault();
        if (establishment is null)
        {
            return Error("Establishment not found", "not found", 400);
        }
        var establishmentCategory = context.EstablishmentsCategory.Where(x => establishmentDto.EstablishmentCategory.Contains(x.Id)
            && x.StatusBase == StatusBase.Active).ToList();
        if (establishmentCategory.Count() != establishmentDto.EstablishmentCategory.Count())
        {
            return Error("Some Category has not found", "Some Category has not found", 400);
        }
        if (establishmentDto.DeliveryTimes.Any(x => x.StartTime > x.EndTime || !Enum.TryParse(x.DeliveryTimeType, out DeliveryTimeType _)))
        {
            return Error("Some errors into delivery times ",
                            "Some errors into delivery times ",
                            400);

        }
        if (establishmentDto.Name.IsNullOrEmpty())
        {
            return Error("Establishment name cannot be empty",
                            "Establishment name cannot be empty",
                            400);

        }
        var finalDeliveryTime = establishment.DeliveryTimes.Where(x => establishmentDto.DeliveryTimes.Any(y => y.StartTime == x.StartTime && y.EndTime == x.EndTime
            && x.DeliveryTimeType == Enum.Parse<DeliveryTimeType>(y.DeliveryTimeType))).ToList();
        finalDeliveryTime.AddRange(establishmentDto.DeliveryTimes.Where(x => !establishment.DeliveryTimes.Any(y => y.StartTime == x.StartTime && y.EndTime == x.EndTime
            && y.DeliveryTimeType == Enum.Parse<DeliveryTimeType>(x.DeliveryTimeType))).Select(x => new DeliveryTime
            {
                DeliveryTimeType = Enum.Parse<DeliveryTimeType>(x.DeliveryTimeType),
                StartTime = x.StartTime,
                EndTime = x.EndTime,

            }));

        var finalEstablishmentcategory = establishment.EstablishmentCategories.Where(x => establishmentCategory.Any(y => y.Id == x.Id)).ToList();
        finalEstablishmentcategory.AddRange(establishmentCategory.Where(x => !establishment.EstablishmentCategories.Any(y => y.Id == x.Id)));

        establishment.Name = establishmentDto.Name;
        establishment.Description = establishmentDto.Description;
        establishment.Address = establishmentDto.Address;
        establishment.PhoneNumber = establishmentDto.PhoneNumber;

        establishment.EstablishmentCategories = finalEstablishmentcategory;
        establishment.DeliveryTimes = finalDeliveryTime;
        establishment.Image = establishmentDto.Image;

        context.Establishments.Update(establishment);
        await context.SaveChangesAsync();
        return new Response<NoContent>();

    }

    public OneOf<ResponseErrorDto, PagedResponse<DeliveryTimeOupuDto>> GetDeliveryTimesEstablishment(int establishmentId, int page, int perPage)
    {
        var existingEstablishment = context.Establishments.FirstOrDefault(x => x.Id == establishmentId);
        if (existingEstablishment is not null)
        {
            return Error("Establishment already exists", "Establishment already exists", 400);
        }
        var deliveryTimes = context.DeliveryTimes.Where(x => x.EstablishmentId == establishmentId)
        .Select(x => new DeliveryTimeOupuDto
        {
            DeliveryTimeType = x.DeliveryTimeType.ToString(),
            EndTime = x.EndTime,
            StartTime = x.StartTime,
            Id = x.Id
        });
        return deliveryTimes.ToPagedResult(page, perPage);

    }

    public async Task<OneOf<ResponseErrorDto, Response<NoContent>>> ChangeStatusEstablishmentAsync(int id)
    {
        var establishment = context.Establishments.FirstOrDefault(x => x.Id == id);
        if (establishment is null)
        {
            return Error("Establishment not found", "not found", 400);
        }
        establishment.StatusBase = (establishment.StatusBase == StatusBase.Active)
            ? StatusBase.Inactive
            : StatusBase.Active;
        context.Establishments.Update(establishment);
        await context.SaveChangesAsync();
        return new Response<NoContent>();
    }
}
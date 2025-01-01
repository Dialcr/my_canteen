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

namespace Canteen.Services.Services;

public class EstablishmentService(EntitiesContext context) : CustomServiceBase(context), IEstablishmentService
{
    public PagedResponse<EstablishmentOutputDto> GetAllEstablishments(int page, int perPage)
    {
        var allEstablishment = context.Establishments.Select(x => x.ToEstablishmentOutputDto());
        return allEstablishment.ToPagedResult(page, perPage);
    }

    public async Task<OneOf<ResponseErrorDto, EstablishmentOutputDto>> GetEstablishmentByIdAsync(int id)
    {
        var establish = await context.Establishments
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (establish is null)
        {
            return Error("Establishment not found",
                "Establishment not found",
                400);
        }

        return establish.ToEstablishmentOutputDto();
    }

    public async Task<OneOf<ResponseErrorDto, Response<NoContent>>> CreateEstablishmentAsync(CreateEstablismentDto establismentDto)
    {
        var existingEstablishment = context.Establishments.FirstOrDefault(x => x.Name == establismentDto.Name);
        if (existingEstablishment is not null)
        {
            return Error("Establishment already exists",
                "Establishment already exists",
                400);
        }
        if (!establismentDto.DeliveryTimes.Any(x => x.StartTime > x.EndTime || !Enum.TryParse(x.DeliveryTimeType, out DeliveryTimeType _)))
        {
            return Error("Some errors into delivery times ",
                            "Some errors into delivery times ",
                            400);

        }
        if (establismentDto.Name.IsNullOrEmpty())
        {
            return Error("Establishment name cannot be empty",
                            "Establishment name cannot be empty",
                            400);

        }
        var establishment = new Establishment
        {
            DeliveryTimes = establismentDto.DeliveryTimes.Select(x => new DeliveryTime
            {
                DeliveryTimeType = Enum.Parse<DeliveryTimeType>(x.DeliveryTimeType),
                StartTime = x.StartTime,
                EndTime = x.EndTime,

            }).ToList(),
            Name = establismentDto.Name,
            Description = establismentDto.Description,
        };
        context.Establishments.Add(establishment);
        await context.SaveChangesAsync();
        return new Response<NoContent>();

    }

    public OneOf<ResponseErrorDto, PagedResponse<DeliveryTimeOupuDto>> GetDeliveryTimesEstablishment(int establishmentId, int page, int perPage)
    {
        var existingEstablishment = context.Establishments.FirstOrDefault(x => x.Id == establishmentId);
        if (existingEstablishment is not null)
        {
            return Error("Establishment already exists",
                "Establishment already exists",
                400);
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

}
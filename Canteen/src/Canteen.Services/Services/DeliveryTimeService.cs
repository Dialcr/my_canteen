using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.DeliveryTime;
using Canteen.Services.Dto.Responses;
using Google.Protobuf.WellKnownTypes;

namespace Canteen.Services.Services;

public class DeliveryTimeService(EntitiesContext context) : CustomServiceBase(context), IDeliveryTimeService
{
    public OneOf<ResponseErrorDto, DeliveryTimeOutputDto> GetDeliveryTimeById(int id)
    {
        var deliveryTime = context.DeliveryTimes.Find(id);
        if (deliveryTime == null)
        {
            return Error("DeliveryTime not found", $"DeliveryTime with ID {id} not found", 400);
        }
        return deliveryTime.ToDeliveryTimeOutputDto();
    }

    public OneOf<ResponseErrorDto, IEnumerable<DeliveryTimeOutputDto>> GetAllDeliveryTimes(int? establishmentId = null)
    {
        var deliveryTimes = context.DeliveryTimes.ToList();
        var deliveryTimeDtos = deliveryTimes.Where(x => (establishmentId == null || x.EstablishmentId == establishmentId)).Select(dt => dt.ToDeliveryTimeOutputDto()).ToList();
        return deliveryTimeDtos;
    }

    public OneOf<ResponseErrorDto, Response<NoContentData>> CreateDeliveryTime(CreateDeliveryTimeDto deliveryTimeDto)
    {
        if (!System.Enum.TryParse(deliveryTimeDto.DeliveryTimeType, true, out DeliveryTimeType deliveryTimeType))
        {
            return Error("Invalid delivery time type", "Invalid delivery time type", 400);
        }
        var establishment = context.Establishments.FirstOrDefault(x => x.Id == deliveryTimeDto.EstablishmentId);
        if (establishment is null)
        {
            return Error("Establishment not found", $"Establishment with ID {deliveryTimeDto.EstablishmentId} not found", 400);
        }
        var deliveryTime = new DeliveryTime
        {
            EstablishmentId = deliveryTimeDto.EstablishmentId,
            StartTime = deliveryTimeDto.StartTime,
            EndTime = deliveryTimeDto.EndTime,
            DeliveryTimeType = deliveryTimeType
        };
        context.DeliveryTimes.Add(deliveryTime);
        context.SaveChanges();
        return new Response<NoContentData>();
    }

    public OneOf<ResponseErrorDto, Response<NoContentData>> UpdateDeliveryTime(UpdateDeliveryTimeDto deliveryTimeDto)
    {
        if (!System.Enum.TryParse(deliveryTimeDto.DeliveryTimeType, true, out DeliveryTimeType deliveryTimeType))
        {
            return Error("Invalid delivery time type", "Invalid delivery time type", 400);
        }
        var establishment = context.Establishments.FirstOrDefault(x => x.Id == deliveryTimeDto.EstablishmentId);
        if (establishment is null)
        {
            return Error("Establishment not found", $"Establishment with ID {deliveryTimeDto.EstablishmentId} not found", 400);
        }

        var deliveryTime = context.DeliveryTimes.Find(deliveryTimeDto.Id);
        if (deliveryTime == null)
        {
            return Error("DeliveryTime not found", $"DeliveryTime with ID {deliveryTimeDto.Id} not found", 400);
        }
        deliveryTime.EstablishmentId = deliveryTimeDto.EstablishmentId;
        deliveryTime.StartTime = deliveryTimeDto.StartTime;
        deliveryTime.EndTime = deliveryTimeDto.EndTime;
        deliveryTime.DeliveryTimeType = deliveryTimeType;
        context.SaveChanges();
        return new Response<NoContentData>();
    }

    public OneOf<ResponseErrorDto, Response<NoContentData>> DeleteDeliveryTime(int id)
    {
        var deliveryTime = context.DeliveryTimes.Find(id);
        if (deliveryTime == null)
        {
            return Error("DeliveryTime not found", $"DeliveryTime with ID {id} not found", 400);
        }
        context.DeliveryTimes.Remove(deliveryTime);
        context.SaveChanges();
        return new Response<NoContentData>();
    }
}

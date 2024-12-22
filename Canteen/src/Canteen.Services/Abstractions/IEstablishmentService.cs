using System;
using Canteen.Services.Dto;
using Canteen.Services.Dto.Establishment;
using Canteen.Services.Dto.Responses;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Canteen.Services.Abstractions;

public interface IEstablishmentService
{
    IEnumerable<Dto.Establishment.EstablishmentOutputDto> GetAllEstablishments();
    Task<OneOf<Dto.ResponseErrorDto, Dto.Establishment.EstablishmentOutputDto>> GetEstablishmentByIdAsync(int id);
    Task<OneOf<ResponseErrorDto, Response<NoContent>>> CreateEstablishmentAsync(CreateEstablismentDto establismentDto);
    OneOf<ResponseErrorDto, PagedResponse<DeliveryTimeOupuDto>> GetDeliveryTimesEstablishment(int establishmentId, int page, int perPage);

}

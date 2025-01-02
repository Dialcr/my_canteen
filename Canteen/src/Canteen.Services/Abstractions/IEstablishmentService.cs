using System;
using Canteen.Services.Dto;
using Canteen.Services.Dto.Establishment;
using Canteen.Services.Dto.Responses;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Canteen.Services.Abstractions;

public interface IEstablishmentService
{
    PagedResponse<Dto.Establishment.EstablishmentOutputDto> GetAllEstablishments(int page, int perPage, bool useInactive = false);
    Task<OneOf<Dto.ResponseErrorDto, Dto.Establishment.EstablishmentOutputDto>> GetEstablishmentByIdAsync(int id);
    Task<OneOf<ResponseErrorDto, Response<NoContent>>> CreateEstablishmentAsync(CreateEstablismentDto establismentDto);
    Task<OneOf<ResponseErrorDto, Response<NoContent>>> ChangeStatusEstablishmentAsync(int id);
    OneOf<ResponseErrorDto, PagedResponse<DeliveryTimeOupuDto>> GetDeliveryTimesEstablishment(int establishmentId, int page, int perPage);
    Task<OneOf<ResponseErrorDto, Response<NoContent>>> UpdateEstablishmentAsync(UpdateEstablismentDto establishmentDto);

}

using System;
using Canteen.Services.Dto;
using Canteen.Services.Dto.EstablishmentCategory;
using Canteen.Services.Dto.Responses;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Canteen.Services.Abstractions;

public interface IEstablishmentCategoryServices
{
    Task<OneOf<ResponseErrorDto, Response<NoContent>>> CreateEstablishmentCategoryAsync(CreateEstablishmentCategoryDto establishmentCategoryDto);
    PagedResponse<EstablishmentCategoryOutputDto> GetAllEstablishmentsCategory(int page, int perPage, bool useInactive = false);
    Task<OneOf<ResponseErrorDto, Response<NoContent>>> ChangeStatusEstablishmentCategoryAsync(int id);
    Task<OneOf<ResponseErrorDto, Response<NoContent>>> UpdateEstablishmentCategoryAsync(UpdateEstablishmentCategoryDto establishmentCategoryDto);
}

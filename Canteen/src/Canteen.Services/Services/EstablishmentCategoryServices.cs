using System;
using AvangTur.Application.Extensions;
using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.EstablishmentCategory;
using Canteen.Services.Dto.Responses;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Canteen.Services.Services;

public class EstablishmentCategoryServices(EntitiesContext context) : CustomServiceBase(context), IEstablishmentCategoryServices
{
    public async Task<OneOf<ResponseErrorDto, Response<NoContent>>> CreateEstablishmentCategoryAsync(CreateEstablishmentCategoryDto establishmentCategoryDto)
    {
        if (establishmentCategoryDto.Name.IsNullOrEmpty())
        {
            return Error("Establishment category name can't be null or empty", "Establishment category name can't be null or empty", 400);
        }
        var existingEstablishmentCategory = await context.EstablishmentsCategory.FirstOrDefaultAsync(x => x.Name.Equals(establishmentCategoryDto.Name,
        StringComparison.OrdinalIgnoreCase) && x.StatusBase == StatusBase.Active);
        if (existingEstablishmentCategory is not null)
        {
            return Error("Establishment category already exists", "Establishment category already exists", 400);
        }
        var establishmentCategory = new EstablishmentCategory
        {
            Description = establishmentCategoryDto.Description,
            Name = establishmentCategoryDto.Name
        };
        context.EstablishmentsCategory.Add(establishmentCategory);
        await context.SaveChangesAsync();
        return new Response<NoContent>();
    }
    public async Task<OneOf<ResponseErrorDto, Response<NoContent>>> UpdateEstablishmentCategoryAsync(UpdateEstablishmentCategoryDto establishmentCategoryDto)
    {
        if (establishmentCategoryDto.Name.IsNullOrEmpty())
        {
            return Error("Establishment category name can't be null or empty", "Establishment category name can't be null or empty", 400);
        }
        var existingEstablishmentCategory = await context.EstablishmentsCategory.FirstOrDefaultAsync(x => x.Name.Equals(establishmentCategoryDto.Name,
        StringComparison.OrdinalIgnoreCase) && x.Id != establishmentCategoryDto.Id);
        if (existingEstablishmentCategory is not null)
        {
            return Error("Establishment category with the same name already exists", "Establishment category with the same name already exists", 400);
        }

        var establishmentCategory = await context.EstablishmentsCategory.FirstOrDefaultAsync(x => x.Id == establishmentCategoryDto.Id);
        if (establishmentCategory is null)
        {
            return Error("Establishment category not found", "not found", 400);
        }

        establishmentCategory.Name = establishmentCategoryDto.Name;
        establishmentCategory.Description = establishmentCategoryDto.Description;

        context.EstablishmentsCategory.Update(establishmentCategory);
        await context.SaveChangesAsync();

        return new Response<NoContent>();
    }

    public IEnumerable<EstablishmentCategoryOutputDto> GetAllEstablishmentsCategory(bool useInactive = false)
    {
        var allEstablishmentCategory = useInactive ? context.EstablishmentsCategory.Select(x => x.ToEstablishmentCategoryOutputDtos())
        : context.EstablishmentsCategory.Where(x => x.StatusBase == StatusBase.Active).Select(x => x.ToEstablishmentCategoryOutputDtos());
        return allEstablishmentCategory;
    }
    public async Task<OneOf<ResponseErrorDto, Response<NoContent>>> ChangeStatusEstablishmentCategoryAsync(int id)
    {
        var establishmentCategory = context.EstablishmentsCategory.FirstOrDefault(x => x.Id == id);
        if (establishmentCategory is null)
        {
            return Error("Establishment category not found", "not found", 400);
        }
        establishmentCategory.StatusBase = (establishmentCategory.StatusBase == StatusBase.Active)
            ? StatusBase.Inactive
            : StatusBase.Active;
        context.EstablishmentsCategory.Update(establishmentCategory);
        await context.SaveChangesAsync();
        return new Response<NoContent>();
    }



}

using System;
using Microsoft.AspNetCore.Http;

namespace Canteen.Services.Abstractions;

public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile file);
}

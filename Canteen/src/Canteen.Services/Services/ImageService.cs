
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Canteen.Services.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Canteen.Services.Services;

public class ImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public ImageService(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No se proporcionó un archivo válido.");

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Transformation = new Transformation().Quality(80).Crop("scale")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception("Error al subir la imagen a Cloudinary.");

        return uploadResult.SecureUrl.ToString();
    }

}

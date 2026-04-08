using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace backend.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Cloudinary");
        var account = new Account(
            section["CloudName"],
            section["ApiKey"],
            section["ApiSecret"]);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    /// <summary>
    /// Uploads raw image bytes to Cloudinary under the "album-covers" folder.
    /// Returns the permanent secure URL, or null if the upload fails.
    /// </summary>
    public async Task<string?> UploadCoverArtAsync(byte[] imageBytes, string publicId)
{
    using var stream = new MemoryStream(imageBytes);

    var uploadParams = new ImageUploadParams
    {
        File = new FileDescription($"{publicId}.png", stream),
        PublicId = publicId,
        AssetFolder = "album-covers",
        Overwrite = true
    };

    var result = await _cloudinary.UploadAsync(uploadParams);

    if (result.Error != null)
    {
        throw new Exception($"Cloudinary upload failed: {result.Error.Message}");
    }

    return result.SecureUrl?.ToString();
}
}

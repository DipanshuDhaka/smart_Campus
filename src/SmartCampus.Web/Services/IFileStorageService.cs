namespace SmartCampus.Web.Services;

public interface IFileStorageService
{
    Task<(bool Success, string? FileUrl, string? ErrorMessage)> SaveImageAsync(IFormFile file, string subfolder);
}

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<(bool Success, string? FileUrl, string? ErrorMessage)> SaveImageAsync(IFormFile file, string subfolder)
    {
        if (file == null || file.Length == 0)
        {
            return (false, null, "File is empty.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, null, "File exceeds maximum permitted size of 10 MB.");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return (false, null, "Invalid file format. Only JPEG, PNG, and WebP images are allowed.");
        }

        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return (false, null, "Invalid MIME type detected.");
        }

        // Generate safe randomized GUID filename
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", subfolder);

        if (!Directory.Exists(uploadRoot))
        {
            Directory.CreateDirectory(uploadRoot);
        }

        var fullPath = Path.Combine(uploadRoot, fileName);
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/{subfolder}/{fileName}";
        return (true, relativeUrl, null);
    }
}

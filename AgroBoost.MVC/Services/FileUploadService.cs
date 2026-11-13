namespace AgroBoost.MVC.Services;

public interface IFileUploadService
{
    Task<string?> UploadProductImageAsync(IFormFile file, string sellerId);
    bool DeleteProductImage(string imagePath);
    bool IsValidImageFile(IFormFile file);
}

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    public FileUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> UploadProductImageAsync(IFormFile file, string sellerId)
    {
        if (!IsValidImageFile(file))
            return null;

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products", sellerId);
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLower()}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/products/{sellerId}/{fileName}";
    }

    public bool DeleteProductImage(string imagePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
        }
        catch
        {
            // Log error
        }
        return false;
    }

    public bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > _maxFileSize)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLower();
        return _allowedExtensions.Contains(extension);
    }
}
// Services/LocalFileStorageService.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    public LocalFileStorageService(IWebHostEnvironment env) => _env = env;

    public async Task<(string storedFileName, string filePath)> SaveFileAsync(IFormFile file, string subFolder = "claims")
    {
        if (file == null || file.Length == 0) throw new ArgumentException("File is empty");

        var uploadsRoot = Path.Combine(_env.ContentRootPath, "Uploads", subFolder);
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsRoot, storedFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return stored filename and path relative to content root
        return (storedFileName, filePath);
    }

    public Task DeleteFileAsync(string storedFileName, string subFolder = "claims")
    {
        var uploadsRoot = Path.Combine(_env.ContentRootPath, "Uploads", subFolder);
        var filePath = Path.Combine(uploadsRoot, storedFileName);
        if (File.Exists(filePath)) File.Delete(filePath);
        return Task.CompletedTask;
    }
}

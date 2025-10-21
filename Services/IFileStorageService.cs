// Services/IFileStorageService.cs
using Microsoft.AspNetCore.Http;
public interface IFileStorageService
{
    Task<(string storedFileName, string filePath)> SaveFileAsync(IFormFile file, string subFolder = "claims");
    Task DeleteFileAsync(string storedFileName, string subFolder = "claims");
}

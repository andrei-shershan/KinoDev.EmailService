namespace KinoDev.EmailService.WebApi.Services.Abstractions;

public interface IFileService
{
    Task<string> GetFileContentAsync(string fileUrl);

    Task<byte[]> GetFileBytesAsync(string fileUrl);
}
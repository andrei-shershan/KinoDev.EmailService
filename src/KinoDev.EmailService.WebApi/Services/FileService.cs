using KinoDev.EmailService.WebApi.Services.Abstractions;

namespace KinoDev.EmailService.WebApi.Services;

public class FileService : IFileService
{
    private readonly HttpClient _httpClient;

    public FileService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetFileContentAsync(string fileUrl)
    {
        var bytes = await _httpClient.GetByteArrayAsync(fileUrl);
        return Convert.ToBase64String(bytes);
    }

    public async Task<byte[]> GetFileBytesAsync(string fileUrl)
    {
        return await _httpClient.GetByteArrayAsync(fileUrl);
    }
}
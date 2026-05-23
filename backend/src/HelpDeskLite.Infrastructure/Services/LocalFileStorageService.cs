using HelpDeskLite.Application.Configuration;
using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HelpDeskLite.Infrastructure.Services;

public class LocalFileStorageService(
    IOptions<FileStorageSettings> options,
    IHostEnvironment environment) : IFileStorageService
{
    private readonly FileStorageSettings _settings = options.Value;

    public void ValidateFile(string fileName, long fileSizeBytes)
    {
        if (fileSizeBytes <= 0)
        {
            throw new BadRequestException("File is empty.");
        }

        if (fileSizeBytes > _settings.MaxFileSizeBytes)
        {
            var maxMb = _settings.MaxFileSizeBytes / (1024 * 1024);
            throw new BadRequestException($"File '{fileName}' exceeds the maximum size of {maxMb} MB.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new BadRequestException($"File type '{extension}' is not allowed.");
        }
    }

    public async Task<StoredFileResult> SaveAsync(
        Guid ticketId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(fileName, content.Length);

        var root = Path.IsPathRooted(_settings.StoragePath)
            ? _settings.StoragePath
            : Path.Combine(environment.ContentRootPath, _settings.StoragePath);

        var ticketFolder = Path.Combine(root, ticketId.ToString("N"));
        Directory.CreateDirectory(ticketFolder);

        var safeName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(ticketFolder, safeName);

        await using var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, cancellationToken);

        var relativePath = Path.Combine(_settings.StoragePath, ticketId.ToString("N"), safeName)
            .Replace('\\', '/');

        return new StoredFileResult(relativePath, safeName);
    }
}

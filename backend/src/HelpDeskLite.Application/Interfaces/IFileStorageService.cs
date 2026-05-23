namespace HelpDeskLite.Application.Interfaces;

public interface IFileStorageService
{
    Task<StoredFileResult> SaveAsync(Guid ticketId, string fileName, string contentType, Stream content, CancellationToken cancellationToken = default);
    void ValidateFile(string fileName, long fileSizeBytes);
}

public record StoredFileResult(string StoragePath, string SavedFileName);

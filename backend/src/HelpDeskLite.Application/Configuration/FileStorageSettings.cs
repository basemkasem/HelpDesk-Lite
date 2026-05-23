namespace HelpDeskLite.Application.Configuration;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    public string StoragePath { get; set; } = "uploads";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public int MaxFilesPerTicket { get; set; } = 5;
    public string[] AllowedExtensions { get; set; } = [".pdf", ".png", ".jpg", ".jpeg", ".txt", ".docx"];
}

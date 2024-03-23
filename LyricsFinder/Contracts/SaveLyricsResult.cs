namespace LyricsFinder.Contracts;

public record SaveLyricsResult
{
    private SaveLyricsResult(SaveLyricsResultStatusEnum status, string? savePath, string? backupPath, Exception? exception)
    {
        Status = status;
        SavePath = savePath;
        BackupPath = backupPath;
        Exception = exception;
    }

    public SaveLyricsResultStatusEnum Status { get; set; }
    public string? SavePath { get; set; }
    public string? BackupPath { get; set; }
    public Exception? Exception { get; init; }
    public bool Saved => Status == SaveLyricsResultStatusEnum.Saved;
    public bool Overwrite => BackupPath is not null;

    public static SaveLyricsResult Success(string savePath, string? backupPath = null)
    {
        return new SaveLyricsResult(SaveLyricsResultStatusEnum.Saved, savePath, backupPath, null);
    }

    public static SaveLyricsResult Failure(SaveLyricsResultStatusEnum failureStatus, Exception? exception = null)
    {
        return new SaveLyricsResult(failureStatus, null, null, exception);
    }
}
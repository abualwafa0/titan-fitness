using TitanFitness.Application.Abstractions.Storage;

namespace TitanFitness.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _memberPhotosRoot;

    public LocalFileStorageService()
    {
        _memberPhotosRoot = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "members");
    }

    public async Task<string> SaveMemberPhotoAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "File name is required.",
                nameof(fileName));
        }

        Directory.CreateDirectory(_memberPhotosRoot);

        var extension = Path.GetExtension(fileName);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var fullPath = Path.Combine(
            _memberPhotosRoot,
            storedFileName);

        await using var outputStream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true);

        await fileStream.CopyToAsync(
            outputStream,
            cancellationToken);

        return $"/uploads/members/{storedFileName}";
    }

    public Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.CompletedTask;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var relativePath = filePath
            .TrimStart('/', '\\')
            .Replace(
                '/',
                Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativePath));

        var allowedRoot =
            Path.GetFullPath(_memberPhotosRoot) +
            Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                allowedRoot,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The file path is outside the member uploads directory.");
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
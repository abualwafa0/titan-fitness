namespace TitanFitness.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<string> SaveMemberPhotoAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}
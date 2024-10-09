using Tag.KandinskyResult.Repositories.Entities;

namespace Tag.KandinskyResult.Repositories;

internal interface IKandinskyRepository
{
    Task<KandinskyResponseEntity?> GetGenerationStatus(string uuid);
}

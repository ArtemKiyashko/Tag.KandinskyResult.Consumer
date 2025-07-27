using Tag.KandinskyResult.Repositories;

namespace Tag.KandinskyResult.Managers;

internal class KandinskyManager(IKandinskyRepository kandinskyRepository) : IKandinskyManager
{
    private readonly IKandinskyRepository _kandinskyRepository = kandinskyRepository;

    public async Task<string?> GetImageBase64(string uuid)
    {
        var kandinskyResponse = await _kandinskyRepository.GetGenerationStatus(uuid);

        if (kandinskyResponse is not null && kandinskyResponse.Status.Equals("FAIL", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(kandinskyResponse.StatusDescription);

        if (kandinskyResponse is null)
            return default;
            
        if (kandinskyResponse.Result != null && kandinskyResponse.Result.Censored)
            throw new InvalidOperationException("The picture has been censored");

        return kandinskyResponse.Result is null || kandinskyResponse.Result.Files is null ? default : kandinskyResponse.Result.Files[0];
    }
}

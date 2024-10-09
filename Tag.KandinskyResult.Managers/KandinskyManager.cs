using Tag.KandinskyResult.Repositories;

namespace Tag.KandinskyResult.Managers;

internal class KandinskyManager(IKandinskyRepository kandinskyRepository) : IKandinskyManager
{
    private readonly IKandinskyRepository _kandinskyRepository = kandinskyRepository;

    public async Task<string?> GetImageBase64(string uuid)
    {
        var kandinskyResponse = await _kandinskyRepository.GetGenerationStatus(uuid);
        if (kandinskyResponse is null || !kandinskyResponse.Status.Equals("DONE", StringComparison.OrdinalIgnoreCase))
            return default;
        if (kandinskyResponse.Censored)
            throw new InvalidOperationException("The picture has been censored");

        return kandinskyResponse.Images is null ? default : kandinskyResponse.Images[0];
    }
}

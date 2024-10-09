using System;
using System.Net.Http.Json;
using Tag.KandinskyResult.Repositories.Entities;

namespace Tag.KandinskyResult.Repositories;

internal class KandinskyRepository(HttpClient httpClient) : IKandinskyRepository
{
    private readonly HttpClient _httpClient = httpClient;

    public Task<KandinskyResponseEntity?> GetGenerationStatus(string uuid) =>
        _httpClient.GetFromJsonAsync<KandinskyResponseEntity>($"key/api/v1/text2image/status/{uuid}");
}

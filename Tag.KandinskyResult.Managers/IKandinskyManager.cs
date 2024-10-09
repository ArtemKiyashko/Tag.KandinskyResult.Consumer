using System;

namespace Tag.KandinskyResult.Managers;

public interface IKandinskyManager
{
    Task<string?> GetImageBase64(string uuid);
}

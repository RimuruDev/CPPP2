using System;
using System.Collections;
using Internal;
using Internal.Codebase.ProgressModule.Models.Gameplay;

public interface IProgressService : IDisposable
{
    // NOTE:
    // Безопасный API для работы с данными: подписки, изменения.
    //
    public IUserProgressProxy UserProgress { get; }
    public IAudioSettingsProxy AudioSettings { get; }
    public IWorldProgressProxy WorldProgress { get; }

    // NOTE:
    // Упрощенный API, в основнов для WEBGL (YG) !Для WEBGL (YG) крайне рекомендую этот вариант для простоты.
    // Работает точно так же как и варианты ById(string id)
    // Но для Android/IOS/MacOs/Ноутбуки/Планшеты - слишком высокое потребление энергии, если часто вызывать.
    //
    public void SaveAllProgress();
    public void LoadAllProgress();
    public void DeleteAllProgress();

    // NOTE:
    // Более сложный API, не желателем для WEBGL (YG).
    // Предпочтителен для всех остальных сценариев кроме WEBGL (YG).
    //
    public void SaveProgressById(string id);
    public void LoadProgressById(string id);
    public void DeleteProgressById(string id);
    
    // NOTE:
    // Упрощенный API, не желателем для WEBGL (YG).
    // Предпочтителен для всех остальных сценариев кроме WEBGL (YG).
    // Псевдо асинхронность - размазывает операции по кадрам, из-за чего не будет фриза при загрузки или сохранении.
    public IEnumerator SaveAllProgressCoroutine(ProgressOperation operation);
    public IEnumerator LoadAllProgressCoroutine(ProgressOperation operation);
    public IEnumerator DeleteAllProgressCoroutine(ProgressOperation operation);
    
    // NOTE:
    // Более сложный API, не желателем для WEBGL (YG).
    // Предпочтителен для всех остальных сценариев кроме WEBGL (YG).
    // Псевдо асинхронность - размазывает операции по кадрам, из-за чего не будет фриза при загрузки или сохранении.
    public IEnumerator SaveProgressByIdCoroutine(string id, ProgressOperation operation);
    public IEnumerator LoadProgressByIdCoroutine(string id, ProgressOperation operation);
    public IEnumerator DeleteProgressByIdCoroutine(string id, ProgressOperation operation);
}
using System;
using Internal;

public interface IProgressService : IDisposable
{
    public IUserProgressProxy UserProgress { get; }
    public IAudioSettingsProxy AudioSettings { get; }

    public void SaveAllProgress();
    public void LoadProgress();
    public void DeleteAllProgress();

    public void SaveProgressById(string id);
    public void LoadProgressById(string id);
    public void DeleteProgressById(string id);
}
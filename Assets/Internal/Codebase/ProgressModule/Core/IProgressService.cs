using System;
using Internal;
using Internal.Codebase.ProgressModule.Models.Gameplay;

public interface IProgressService : IDisposable
{
    public IUserProgressProxy UserProgress { get; }
    public IAudioSettingsProxy AudioSettings { get; }
    public IWorldProgressProxy WorldProgress { get; }

    public void SaveAllProgress();
    public void LoadAllProgress();
    public void DeleteAllProgress();

    public void SaveProgressById(string id);
    public void LoadProgressById(string id);
    public void DeleteProgressById(string id);
}
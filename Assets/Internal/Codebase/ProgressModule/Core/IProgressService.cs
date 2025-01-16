using System;
using Internal.Codebase.ProgressModule.Models;

namespace Internal.Codebase.ProgressModule.Core
{
    public interface IProgressService : IDisposable
    {
        // Models ====
        public IUserProgressProxy UserProgress { get; }
        public IAudioSettingsProxy AudioSettings { get;  }

        // Classics API ===
        public void SaveAllProgress();
        public void LoadProgress();
        public void DeleteAllProgress();

        // No WEBGL API ===
        // public void SaveProgressById(string id);
        // public void LoadProgressById(string id);
        // public void DeleteProgressById(string id);
    }
}
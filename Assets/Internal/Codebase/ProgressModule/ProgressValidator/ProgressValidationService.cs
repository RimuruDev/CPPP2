using UnityEngine;

namespace Internal
{
    public class ProgressValidationService : IProgressValidator
    {
        public bool IsValid(UserProgress progress)
        {
            if (progress == null)
                return false;

            if (string.IsNullOrWhiteSpace(progress.UserName))
                return false;

            if (progress.Level <= 0)
                return false;

            return true;
        }

        public bool IsValid(AudioSettings progress)
        {
            if (progress.BackgroundMusicVolume < 0) //|| progress.BackgroundMusicVolume > 1)
                return false;

            if (progress.SfxVolume < 0) //|| progress.SfxVolume > 1)
                return false;

            return true;
        }

        public UserProgress ValidateAndFix(UserProgress progress)
        {
            if (progress == null)
            {
                Debug.LogWarning("Progress is null, initializing default progress.");
                return DefaultProgressFactory.CreateDefaultProgress();
            }

            if (progress.Level < 1)
            {
                Debug.LogWarning("Invalid level, resetting to 1.");
                progress.Level = 1;
            }

            if (progress.HardCurrency < 0)
                progress.HardCurrency = 0;

            if (progress.SoftCurrency < 0)
                progress.SoftCurrency = 0;

            return progress;
        }

        public AudioSettings ValidateAndFix(AudioSettings progress)
        {
            if (progress == null)
            {
                Debug.LogWarning("Progress is null, initializing default progress.");
                return DefaultProgressFactory.CreateDefaultAudioSettings();
            }

            // NOTE: Отрубил для тестов валидацию на > 1
            if (progress.BackgroundMusicVolume < 0) //|| progress.BackgroundMusicVolume > 1)
                progress.BackgroundMusicVolume = 1;

            if (progress.SfxVolume < 0) //|| progress.SfxVolume > 1)
                progress.SfxVolume = 1;

            return progress;
        }
    }
}
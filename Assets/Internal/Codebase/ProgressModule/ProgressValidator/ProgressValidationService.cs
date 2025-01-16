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

        public UserProgress ValidateAndFix(UserProgress progress)
        {
            if (progress == null)
            {
                Debug.LogWarning("Progress is null, initializing default progress.");
                return DefaultProgressFactory.CreateDefaultProgress();
            }

            if (progress.Level < 1)
            {
                progress.Level = 1;
                Debug.LogError("Invalid level, resetting to 1.");
            }

            if (progress.HardCurrency < 0)
            {
                progress.HardCurrency = 0;
                Debug.LogError("Invalid hard currency, resetting to 0.");
            }

            if (progress.SoftCurrency < 0)
            {
                progress.SoftCurrency = 0;
                Debug.LogError("Invalid currency level, resetting to 0.");
            }

            return progress;
        }

        public bool IsValid(AudioSettings progress)
        {
            if (progress.BackgroundMusicVolume is < 0 or > 1)
            {
                Debug.LogError("Invalid background music volume value: " + progress.BackgroundMusicVolume);
                return false;
            }

            if (progress.SfxVolume is < 0 or > 1)
            {
                Debug.LogError("Invalid sfx volume value: " + progress.SfxVolume);
                return false;
            }

            return true;
        }

        public AudioSettings ValidateAndFix(AudioSettings progress)
        {
            if (progress == null)
            {
                Debug.LogWarning("Progress is null, initializing default progress.");
                return DefaultProgressFactory.CreateDefaultAudioSettings();
            }

            if (progress.BackgroundMusicVolume is < 0 or > 1)
            {
                progress.BackgroundMusicVolume = 1;
                Debug.LogError("Invalid background music volume. Must be 0 or 1.");
            }

            if (progress.SfxVolume is < 0 or > 1)
            {
                progress.SfxVolume = 1;
                Debug.LogError("Invalid sfx volume. Must be 0 or 1.");
            }

            return progress;
        }
    }
}
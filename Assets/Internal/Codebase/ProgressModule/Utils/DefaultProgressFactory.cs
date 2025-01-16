using Internal.Codebase.ProgressModule.Models.Gameplay;
using UnityEngine;

namespace Internal
{
    public static class DefaultProgressFactory
    {
        public static UserProgress CreateDefaultProgress()
        {
            return new UserProgress
            {
                UserName = "Rimuru",
                Level = 1,
                HardCurrency = 0,
                SoftCurrency = 0,
            };
        }

        public static AudioSettings CreateDefaultAudioSettings()
        {
            return new AudioSettings
            {
                BackgroundMusicVolume = 1,
                SfxVolume = 1
            };
        }

        public static WorldProgress CreateDefaultWorldProgress()
        {
            return new WorldProgress
            {
                CurrentWorldPosition = new Vector3Data(10, 10, 10),
                CurrentWorldRotation = new Vector3Data(0, 0, 0),
                CurrentTime = 30
            };
        }
    }
}
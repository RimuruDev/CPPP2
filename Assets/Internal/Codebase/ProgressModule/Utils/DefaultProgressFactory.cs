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
    }
}
using RimuruDev;

namespace Internal
{
    //
    // Исключительно для теста
    //
    public interface IAudioSettingsProxy
    {
        public AudioSettings Origin { get; }
        public ReactiveProperty<float> BackgroundMusicVolume { get; }
        public ReactiveProperty<float> SfxVolume { get; }
        public void Dispose();
    }
}
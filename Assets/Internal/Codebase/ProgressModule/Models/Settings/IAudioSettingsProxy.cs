using System;
using RimuruDev;

namespace Internal
{
    //
    // Исключительно для теста
    //
    public interface IAudioSettingsProxy : IDisposable
    {
        public AudioSettings Origin { get; }
        public ReactiveProperty<float> BackgroundMusicVolume { get; }
        public ReactiveProperty<float> SfxVolume { get; }
    }
}
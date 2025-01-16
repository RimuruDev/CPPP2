using System;
using System.Collections.Generic;
using RimuruDev;

namespace Internal
{
    //
    // Исключительно для теста
    //
    public class AudioSettingsProxy : IAudioSettingsProxy
    {
        public AudioSettings Origin { get; private set; }

        public ReactiveProperty<float> BackgroundMusicVolume { get; private set; }
        public ReactiveProperty<float> SfxVolume { get; private set; }

        private readonly List<IDisposable> subscriptions = new();

        public AudioSettingsProxy(AudioSettings origin)
        {
            Origin = origin;

            BackgroundMusicVolume = new ReactiveProperty<float>(origin.BackgroundMusicVolume);
            SfxVolume = new ReactiveProperty<float>(origin.SfxVolume);

            subscriptions.Add(BackgroundMusicVolume.Subscribe(value => Origin.BackgroundMusicVolume = value));
            subscriptions.Add(SfxVolume.Subscribe(value => Origin.SfxVolume = value));
        }

        public void Dispose()
        {
            foreach (var subscription in subscriptions)
                subscription.Dispose();

            subscriptions.Clear();

            BackgroundMusicVolume = null;
            SfxVolume = null;
        }
    }
}
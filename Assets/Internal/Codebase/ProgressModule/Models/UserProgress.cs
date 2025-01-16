using System;
using System.Collections;
using System.Collections.Generic;
using RimuruDev;

namespace Internal
{
    [Serializable]
    public class UserProgress
    {
        public string UserName;
        public int Level;
        public int SoftCurrency;
        public int HardCurrency;
    }

    [Serializable]
    public class AudioSettings
    {
        public float BackgroundMusicVolume;
        public float SfxVolume;
    }

    public interface IAudioSettingsProxy
    {
        public AudioSettings Origin { get; }
        public ReactiveProperty<float> BackgroundMusicVolume { get; }
        public ReactiveProperty<float> SfxVolume { get; }
        public void Dispose();
    }

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
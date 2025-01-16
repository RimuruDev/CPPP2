using System;
using System.Collections.Generic;
using RimuruDev;

namespace Internal
{
    public interface IUserProgressProxy : IDisposable
    {
        public UserProgress Origin { get; }
        public ReactiveProperty<string> UserName { get; }
        public ReactiveProperty<int> Level { get; }
        public ReactiveProperty<int> SoftCurrency { get; }
        public ReactiveProperty<int> HardCurrency { get; }
    }

    public class UserProgressProxy : IUserProgressProxy
    {
        public UserProgress Origin { get; private set; }

        public ReactiveProperty<string> UserName { get; private set; }
        public ReactiveProperty<int> Level { get; private set; }
        public ReactiveProperty<int> SoftCurrency { get; private set; }
        public ReactiveProperty<int> HardCurrency { get; private set; }

        private readonly List<IDisposable> subscriptions = new();

        public UserProgressProxy(UserProgress origin)
        {
            Origin = origin;

            UserName = new ReactiveProperty<string>(origin.UserName);
            Level = new ReactiveProperty<int>(origin.Level);
            SoftCurrency = new ReactiveProperty<int>(origin.SoftCurrency);
            HardCurrency = new ReactiveProperty<int>(origin.HardCurrency);

            // === Подписываемся на изменения и сохраняем IDisposable !!!
            subscriptions.Add(UserName.Subscribe(value => Origin.UserName = value));
            subscriptions.Add(Level.Subscribe(value => Origin.Level = value));
            subscriptions.Add(SoftCurrency.Subscribe(value => Origin.SoftCurrency = value));
            subscriptions.Add(HardCurrency.Subscribe(value => Origin.HardCurrency = value));
        }

        public void Dispose()
        {
            // === Отписываемся от всех подписок !!!
            foreach (var subscription in subscriptions)
            {
                subscription.Dispose();
            }

            subscriptions.Clear();

            // === Очищаем ссылки, что бы не повадно было)
            UserName = null;
            Level = null;
            SoftCurrency = null;
            HardCurrency = null;
        }
    }
}
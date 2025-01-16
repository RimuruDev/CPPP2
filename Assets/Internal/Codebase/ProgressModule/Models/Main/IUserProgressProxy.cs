using System;
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
}
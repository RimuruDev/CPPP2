using System;
using System.Collections.Generic;
using RimuruDev;
using UnityEngine;

namespace Internal.Codebase.ProgressModule.Models.Gameplay
{
    public class WorldProgressProxy : IWorldProgressProxy
    {
        public WorldProgress Origin { get; private set; }
        public ReactiveProperty<Vector3Data> CurrentWorldPosition { get; private set; }
        public ReactiveProperty<Vector3Data> CurrentWorldRotation { get; private set; }
        public ReactiveProperty<float> CurrentTime { get; private set; }

        private readonly List<IDisposable> disposables = new();

        public WorldProgressProxy(WorldProgress origin)
        {
            Origin = origin;

            CurrentWorldPosition = new ReactiveProperty<Vector3Data>(origin.CurrentWorldPosition);
            CurrentWorldRotation = new ReactiveProperty<Vector3Data>(origin.CurrentWorldRotation);
            CurrentTime = new ReactiveProperty<float>(origin.CurrentTime);

            disposables.Add(CurrentWorldPosition.Subscribe(value => Origin.CurrentWorldPosition = value));
            disposables.Add(CurrentWorldRotation.Subscribe(value => Origin.CurrentWorldRotation = value));
            disposables.Add(CurrentTime.Subscribe(value => Origin.CurrentTime = value));
        }

        public void Dispose()
        {
            foreach (var disposable in disposables)
                disposable?.Dispose();

            disposables?.Clear();

            CurrentWorldPosition = null;
            CurrentWorldRotation = null;
            CurrentTime = null;
        }
    }
}
using System;
using RimuruDev;
using UnityEngine;

namespace Internal.Codebase.ProgressModule.Models.Gameplay
{
    public interface IWorldProgressProxy : IDisposable
    {
        public WorldProgress Origin { get; }
        public ReactiveProperty<Vector3Data> CurrentWorldPosition { get; }
        public ReactiveProperty<Vector3Data> CurrentWorldRotation { get; }
        public ReactiveProperty<float> CurrentTime { get; }
    }
}
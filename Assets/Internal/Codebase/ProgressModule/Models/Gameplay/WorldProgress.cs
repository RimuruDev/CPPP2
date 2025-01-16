using System;
using UnityEngine;

namespace Internal.Codebase.ProgressModule.Models.Gameplay
{
    [Serializable]
    public class WorldProgress
    {
        public Vector3Data CurrentWorldPosition;
        public Vector3Data CurrentWorldRotation;
        public float CurrentTime;
    }

    [Serializable]
    public struct Vector3Data
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Vector3Data(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 ToVector3() => 
            new(X, Y, Z);

        public Vector3Data ToVector3Data() => 
            new(X, Y, Z);
    }
}
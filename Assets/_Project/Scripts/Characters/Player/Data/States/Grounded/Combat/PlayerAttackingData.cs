using System;
using UnityEngine;

namespace MovementSystem
{
    [Serializable]
    public class PlayerAttackingData
    {
        [field: SerializeField] 
        [field: Range(0, 1)] 
        public float SpeedModifier { get; private set; } = 0f;
    }
}

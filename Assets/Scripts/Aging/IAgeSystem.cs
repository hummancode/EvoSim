using UnityEngine;
using System;

public interface IAgeSystem
{
    float Age { get; }
    float MaxAge { get; }
    bool IsMature { get; }

    event Action OnMatured;
    event Action OnDeath;
}

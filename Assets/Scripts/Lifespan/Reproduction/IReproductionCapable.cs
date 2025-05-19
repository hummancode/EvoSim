using System;
using UnityEngine;
public interface IReproductionCapable
{
    bool CanMate { get; }
    bool IsMating { get; }
    float MatingProximity { get; }
    float LastMatingTime { get; }

    bool CanMateWith(IAgent partner);
    void InitiateMating(IAgent partner);
    void AcceptMating(IAgent partner);

    event Action<Vector3> OnOffspringRequested;
    event Action<IAgent> OnMatingStarted;
    event Action OnMatingCompleted;
}

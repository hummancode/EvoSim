using UnityEngine;

public interface IReproductionCapability
{
    bool IsMating { get; }
    bool CanMateAgain { get; }
    void InitiateMating(GameObject partner);
    event System.Action<GameObject> OnMatingStarted;
    event System.Action OnMatingCompleted;
}
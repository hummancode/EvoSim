using System.Collections.Generic;
using System;
using UnityEngine;

public interface ISensorSystem : IAgentComponent
{
    T GetNearestEntity<T>(float range = -1, Func<T, bool> filter = null) where T : Component;
    List<T> GetEntitiesInRange<T>(float range = -1, Func<T, bool> filter = null) where T : Component;
    bool HasEntityInRange<T>(float range = -1, Func<T, bool> filter = null) where T : Component;
    IEdible GetNearestEdible();
}
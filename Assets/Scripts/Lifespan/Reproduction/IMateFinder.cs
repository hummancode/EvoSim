using System;

public interface IMateFinder
{
    IAgent FindNearestPotentialMate(Func<IAgent, bool> filter = null);
    float GetDistanceTo(IAgent other);
}
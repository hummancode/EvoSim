using UnityEngine;

public class FoodSensorAdapter : ISensorCapability
{
    private readonly SensorSystem sensorSystem;

    public FoodSensorAdapter(SensorSystem sensorSystem)
    {
        this.sensorSystem = sensorSystem ?? throw new System.ArgumentNullException(nameof(sensorSystem));
    }

    public Vector3? GetTargetPosition()
    {
        IEdible edible = sensorSystem.GetNearestEdible();
        if (edible != null && edible is MonoBehaviour mb)
        {
            return mb.transform.position;
        }
        return null;
    }

    public IEdible GetTargetObject()
    {
        return sensorSystem.GetNearestEdible();
    }

    public bool HasTarget()
    {
        return sensorSystem.GetNearestEdible() != null;
    }
}
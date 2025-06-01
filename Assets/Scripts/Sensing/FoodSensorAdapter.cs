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

        // CRITICAL FIX: Check if the object still exists
        if (edible != null && edible is MonoBehaviour mb && mb != null)
        {
            return mb.transform.position;
        }
        return null;
    }

    public IEdible GetTargetObject()
    {
        IEdible edible = sensorSystem.GetNearestEdible();

        // CRITICAL FIX: Validate the object still exists
        if (edible != null && edible is MonoBehaviour mb && mb != null)
        {
            return edible;
        }
        return null;
    }

    public bool HasTarget()
    {
        return GetTargetObject() != null;
    }
}
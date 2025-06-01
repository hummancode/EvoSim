// ============================================================================
// FIXED AgentAdapter.cs - Add null checks for destroyed GameObjects
// ============================================================================

using UnityEngine;

public class AgentAdapter : IAgent
{
    private AgentController controller;

    public AgentAdapter(AgentController controller)
    {
        this.controller = controller;
    }

    // FIXED - Add null checks for GameObject access
    public GameObject GameObject
    {
        get
        {
            // Return null if controller or its GameObject is destroyed
            if (controller == null) return null;
            return controller.gameObject;
        }
    }

    // FIXED - Safe position access
    public Vector3 Position
    {
        get
        {
            if (controller == null) return Vector3.zero;
            return controller.transform.position;
        }
    }

    // FIXED - Safe ReproductionSystem access
    public IReproductionCapable ReproductionSystem
    {
        get
        {
            if (controller == null) return null;

            try
            {
                return controller.GetComponent<ReproductionSystem>() as IReproductionCapable;
            }
            catch (MissingReferenceException)
            {
                // GameObject was destroyed
                return null;
            }
        }
    }

    // FIXED - Safe EnergySystem access  
    public IEnergyProvider EnergySystem
    {
        get
        {
            if (controller == null) return null;

            try
            {
                return controller.GetComponent<EnergySystem>() as IEnergyProvider;
            }
            catch (MissingReferenceException)
            {
                // GameObject was destroyed
                return null;
            }
        }
    }

    // NEW - Helper method to check if this adapter is still valid
    public bool IsValid()
    {
        return controller != null;
    }

    // NEW - Helper method for safe component access
    public T GetComponentSafely<T>() where T : Component
    {
        if (controller == null) return null;

        try
        {
            return controller.GetComponent<T>();
        }
        catch (MissingReferenceException)
        {
            // GameObject was destroyed
            return null;
        }
    }
}
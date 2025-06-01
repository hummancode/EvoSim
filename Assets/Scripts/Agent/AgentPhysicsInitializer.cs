using UnityEngine;

// Remove the RequireComponent attributes since we'll add these components ourselves
public class AgentPhysicsInitializer : MonoBehaviour
{
    [Header("Physics Setup")]
    [SerializeField] private RigidbodyType2D bodyType = RigidbodyType2D.Kinematic;
    [SerializeField] private float colliderRadius = 0.5f;
    [SerializeField] private float colliderHeight = 1.2f;

    [Header("Collider Type")]
    [SerializeField] private ColliderType colliderType = ColliderType.Capsule;

    [Header("Physics Settings")]
    [SerializeField] private bool disableGravity = true;
    [SerializeField] private bool freezeRotation = true;
    [SerializeField] private float linearDrag = 0.5f;

    public enum ColliderType
    {
        Box,
        Circle,
        Capsule
    }

    private Rigidbody2D rb;
    private Collider2D col;

    void Awake()
    {
        // Set layer
        int agentLayerIndex = LayerMask.NameToLayer("Agent");
        if (agentLayerIndex >= 0)
        {
            gameObject.layer = agentLayerIndex;
        }
        else
        {
            Debug.LogWarning("Agent layer not found. Please create an 'Agent' layer in Project Settings.");
        }

        // Initialize Rigidbody2D
        InitializeRigidbody();

        // Initialize Collider2D
        InitializeCollider();

        // Log successful initialization
        Debug.Log($"Agent physics initialized: {gameObject.name}");
    }

    private void InitializeRigidbody()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = bodyType;
        rb.gravityScale = disableGravity ? 0f : 1f;
        rb.drag = linearDrag;
        rb.constraints = freezeRotation ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.None;

        // CRITICAL: Use Continuous collision detection for high speeds
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // CRITICAL: Interpolate for smooth movement at high timescales
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
    private void InitializeCollider()
    {
        // Check for existing colliders
        Collider2D existingCollider = GetComponent<Collider2D>();

        // If we have a collider of the wrong type, remove it
        if (existingCollider != null)
        {
            bool correctType = false;

            switch (colliderType)
            {
                case ColliderType.Box:
                    correctType = existingCollider is BoxCollider2D;
                    break;
                case ColliderType.Circle:
                    correctType = existingCollider is CircleCollider2D;
                    break;
                case ColliderType.Capsule:
                    correctType = existingCollider is CapsuleCollider2D;
                    break;
            }

            if (!correctType)
            {
                Debug.Log($"Replacing existing collider of type {existingCollider.GetType().Name}");
                DestroyImmediate(existingCollider);
                existingCollider = null;
            }
        }

        // If we need to add a collider
        if (existingCollider == null)
        {
            // Add the selected collider type
            switch (colliderType)
            {
                case ColliderType.Box:
                    BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
                    boxCollider.size = new Vector2(colliderRadius * 2, colliderHeight);
                    col = boxCollider;
                    break;

                case ColliderType.Circle:
                    CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
                    circleCollider.radius = colliderRadius;
                    col = circleCollider;
                    break;

                case ColliderType.Capsule:
                    CapsuleCollider2D capsuleCollider = gameObject.AddComponent<CapsuleCollider2D>();
                    capsuleCollider.size = new Vector2(colliderRadius * 2, colliderHeight);
                    col = capsuleCollider;
                    break;
            }

            Debug.Log($"Added {col.GetType().Name} to agent");
        }
        else
        {
            // Use the existing collider
            col = existingCollider;

            // Update collider properties
            switch (colliderType)
            {
                case ColliderType.Box:
                    BoxCollider2D boxCollider = col as BoxCollider2D;
                    if (boxCollider != null)
                    {
                        boxCollider.size = new Vector2(colliderRadius * 2, colliderHeight);
                    }
                    break;

                case ColliderType.Circle:
                    CircleCollider2D circleCollider = col as CircleCollider2D;
                    if (circleCollider != null)
                    {
                        circleCollider.radius = colliderRadius;
                    }
                    break;

                case ColliderType.Capsule:
                    CapsuleCollider2D capsuleCollider = col as CapsuleCollider2D;
                    if (capsuleCollider != null)
                    {
                        capsuleCollider.size = new Vector2(colliderRadius * 2, colliderHeight);
                    }
                    break;
            }
        }

        // Ensure we have a collider
        if (col == null)
        {
            Debug.LogError("Failed to create or find collider!");
            return;
        }

        // Configure collider
        col.isTrigger = false; // Solid collider for agents
    }

    // Rest of the code remains the same...
}
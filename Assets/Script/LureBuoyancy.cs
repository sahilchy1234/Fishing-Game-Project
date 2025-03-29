using UnityEngine;

public class LureBuoyancy : MonoBehaviour
{
    public float waterLevel = 0f;      // Set this to the Y position of the water surface
    public float buoyancyForce = 10f;  // Strength of floating effect
    public float waterDrag = 3f;       // Water resistance (higher = more floaty)
    public float sinkingSpeed = 2f;    // How fast the lure sinks when idle
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        ApplyBuoyancy();
    }

    void ApplyBuoyancy()
    {
        if (transform.position.y < waterLevel) // If lure is underwater
        {
            float depthFactor = Mathf.Clamp01((waterLevel - transform.position.y) / 2f); // Deeper = more buoyant
            rb.AddForce(Vector3.up * buoyancyForce * depthFactor, ForceMode.Acceleration);

            // Simulate water drag
            rb.linearDamping = waterDrag;
        }
        else
        {
            // Reset drag when above water
            rb.linearDamping = 0.5f;
        }

        // Apply slow sinking effect
        if (transform.position.y > waterLevel)
        {
            rb.AddForce(Vector3.down * sinkingSpeed, ForceMode.Acceleration);
        }
    }
}

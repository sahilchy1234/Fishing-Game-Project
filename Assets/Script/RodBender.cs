using UnityEngine;
using UnityEngine.UI;

public class RodBender : MonoBehaviour
{


    [Header("Rod Setup")]
    public Transform[] bones; // Assign from base (0) to tip (last)
    public Slider bendSlider; // Controls the bend amount (0-1)

    [Header("Bend Settings")]
    [Tooltip("Base angle each bone should bend at maximum slider value")]
    public float anglePerBone = 6f; // Base angle value
    public float rotationSmoothness = 10f;

    [Header("Casting Reference")]
    public FishingCast3D castingSystem;
    // public Transform rodTip;

    [Header("Animation Settings")]
    [Tooltip("Maximum animation speed when slider is at full extension")]
    public float maxAnimationSpeed = 1f;

    private Quaternion[] originalRotations;
    private float currentBendFactor;
    private float targetBendFactor;
    private float directionSign = 1f; // 1 for right, -1 for left
    private float previousSliderValue;
    private float sliderMovementDirection;

    void Start()
    {
        originalRotations = new Quaternion[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            originalRotations[i] = bones[i].localRotation;
        }

        bendSlider.onValueChanged.AddListener(OnSliderChanged);
        previousSliderValue = bendSlider.value;
    }

    void Update()
    {
        // Handle casting direction
      

        currentBendFactor = Mathf.Lerp(currentBendFactor, targetBendFactor,
                                      Time.deltaTime * rotationSmoothness);
        ApplyBoneRotations();

        // Update animation parameters
        // UpdateAnimationParameters();
    }


    void OnSliderChanged(float value)
    {
        targetBendFactor = value;
    }

    void ApplyBoneRotations()
    {
        if (bones == null || bones.Length == 0) return;

        for (int i = 0; i < bones.Length; i++)
        {
            // Calculate rotation angle with direction sign applied
            float rotationAngle = anglePerBone * currentBendFactor * ((float)i / bones.Length) ;
            RotateControlledObject();
            // Always bend around the rod's local Z axis (assuming this is the natural bend axis)
            bones[i].localRotation = originalRotations[i] *
                                    Quaternion.AngleAxis(rotationAngle, Vector3.forward);
        }
    }

    public Vector3 rotationAxis = Vector3.up; // Rotation axis (default Z-axis)
    [Header("Character References")]
    public Transform controlledObject; // Object to rotate using slider
    public int maxRotationAngle;

    void RotateControlledObject()
    {
        if (controlledObject == null) return;

        // Calculate rotation angle based on slider value
        float rotationAngle = maxRotationAngle * bendSlider.value;

        // Maintain the initial X rotation (26°) and apply the additional X rotation
        float newXRotation = 26f + rotationAngle;

        // Keep the Y and Z rotation unchanged
        Vector3 newRotation = new Vector3(newXRotation, controlledObject.localRotation.eulerAngles.y, controlledObject.localRotation.eulerAngles.z);

        // Apply rotation smoothly
        controlledObject.localRotation = Quaternion.Euler(newRotation);
    }



}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishingCast3D : MonoBehaviour
{
    [Header("Effects")]
    public GameObject splashPrefab;
    public GameObject ripplePrefab;

    [Header("References")]
    public Animator _animator;
    public Transform lureTransform;
    public Transform castingPoint;
    public LineRenderer trajectoryLine;
    public Transform waterSurface;

    [Header("Casting Settings")]
    public float maxForce = 30f;
    public float minForce = 5f;
    public float dragMultiplier = 0.05f;
    public int trajectoryResolution = 50;
    public float castDuration = 2f;
    public float maxAngle = 60f;
    public float gravity = -9.8f;
    public float airResistance = 0.5f;
    public float timeStep = 0.05f;

    private bool hasHitWater = false;
    public bool isLureInWater = false;
    private Vector3 startPoint;
    public bool isDragging = false;
    private bool isCasting = false;
    private float waterHeight;
    private Camera mainCamera;
    public List<Vector3> trajectoryPoints = new List<Vector3>();
    private Vector3 castVelocity;
    private Vector3 currentCastPosition;
    private float castProgress;
    private Vector3 waterLandingPosition;

    public static FishingCast3D instance;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        mainCamera = Camera.main;
        waterHeight = waterSurface.position.y;
        ResetLurePosition();
    }

    void Update()
    {
        // if (isLureInWater)
        // {
        //     // Keep lure at the landing position
        //     lureTransform.position = waterLandingPosition;
        //     return;
        // }

        if (isCasting)
        {
            UpdateCast();
            return;
        }

        HandleInput();
    }

    void ResetLurePosition()
    {
        lureTransform.position = castingPoint.position;
        lureTransform.rotation = Quaternion.identity;
        hasHitWater = false;
        isLureInWater = false;
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && !isLureInWater)
        {
            startPoint = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 endPoint = Input.mousePosition;
            Vector3 force = CalculateLaunchForce(startPoint, endPoint);
            trajectoryPoints = CalculateTrajectory(castingPoint.position, force);
            DrawTrajectory(trajectoryPoints);
            RotateLureTowards(force);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            _animator.SetBool("throw", true);
            StartCoroutine(StartCastAfterDelay(2f));
            Invoke("false_throw", 3f);
        }
    }
    bool flag = false;
    void false_throw()
    {

        if (flag == false)
        {
            _animator.SetBool("throw", false);
            flag = true;

        }

    }

    IEnumerator StartCastAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCast();
    }

    void StartCast()
    {
        if (trajectoryPoints.Count < 2) return;

        isCasting = true;
        isDragging = false;
        trajectoryLine.positionCount = 0;
        castProgress = 0f;
        currentCastPosition = castingPoint.position;
        castVelocity = (trajectoryPoints[1] - trajectoryPoints[0]) / timeStep;
        hasHitWater = false;
    }

    void UpdateCast()
    {
        castProgress += Time.deltaTime / castDuration;

        // Apply gravity and air resistance to velocity
        castVelocity.y += gravity * Time.deltaTime;
        castVelocity *= (1 - airResistance * Time.deltaTime);

        // Update position
        currentCastPosition += castVelocity * Time.deltaTime;
        lureTransform.position = currentCastPosition;

        // Rotate lure based on movement direction
        if (castVelocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(castVelocity.normalized);
            lureTransform.rotation = Quaternion.Slerp(lureTransform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Check for water impact
        if (!hasHitWater && currentCastPosition.y <= waterHeight)
        {
            WaterImpact();
        }

        // End cast when duration is complete or below water
        if (castProgress >= 1f || currentCastPosition.y < waterHeight - 1f)
        {
            EndCast();
        }
    }

    void WaterImpact()
    {
        hasHitWater = true;
        waterLandingPosition = new Vector3(
            currentCastPosition.x,
            waterHeight,
            currentCastPosition.z
        );
        CreateWaterEffects();
        isLureInWater = true;
    }

    void EndCast()
    {
        isCasting = false;
        if (!hasHitWater)
        {
            ResetLurePosition();
        }
    }

    Vector3 CalculateLaunchForce(Vector3 start, Vector3 end)
    {
        Vector3 screenDirection = start - end;
        float screenDistance = screenDirection.magnitude * dragMultiplier;

        Vector3 worldStart = mainCamera.ScreenToWorldPoint(new Vector3(start.x, start.y, 10f));
        Vector3 worldEnd = mainCamera.ScreenToWorldPoint(new Vector3(end.x, end.y, 10f));
        Vector3 worldDirection = (worldStart - worldEnd).normalized;

        worldDirection.x *= -1; // Reverse only left-right movement

        float forceMagnitude = Mathf.Clamp(screenDistance, minForce, maxForce);
        float launchAngle = Mathf.Lerp(15f, maxAngle, forceMagnitude / maxForce);
        float yForce = Mathf.Tan(launchAngle * Mathf.Deg2Rad) * forceMagnitude;

        return new Vector3(worldDirection.x * forceMagnitude, yForce, worldDirection.z * forceMagnitude);
    }

    List<Vector3> CalculateTrajectory(Vector3 startPosition, Vector3 initialVelocity)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 currentPosition = startPosition;
        Vector3 currentVelocity = initialVelocity;

        for (int i = 0; i < trajectoryResolution; i++)
        {
            points.Add(currentPosition);

            // Apply gravity and air resistance
            currentVelocity.y += gravity * timeStep;
            currentVelocity *= (1 - airResistance * timeStep);

            currentPosition += currentVelocity * timeStep;

            if (currentPosition.y <= waterHeight)
            {
                break;
            }
        }

        return points;
    }

    void DrawTrajectory(List<Vector3> points)
    {
        trajectoryLine.positionCount = points.Count;
        trajectoryLine.SetPositions(points.ToArray());
    }

    void RotateLureTowards(Vector3 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            lureTransform.rotation = Quaternion.Slerp(lureTransform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    void CreateWaterEffects()
    {
        Instantiate(splashPrefab, waterLandingPosition, Quaternion.identity);
        Instantiate(ripplePrefab, waterLandingPosition, Quaternion.Euler(90, 0, 0));
    }
    public RodBender rodController;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "left")
        {
            rodController.anglePerBone = rodController.anglePerBone * 1;
        }
        if (other.gameObject.name == "right")
        {
            rodController.anglePerBone = rodController.anglePerBone * -1;

        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FishingLine : MonoBehaviour
{
    public Transform rodTip;
    public Transform lure;
    public Rigidbody lureRb;
    public LineRenderer lineRenderer;
    public Slider lengthSlider;
    public int lineSegments = 20;
    public float moveSpeed = 5f;

    private float targetRopeLength = 10f;
    private float currentRopeLength;
    private Coroutine foldingCoroutine;

    [Header("Rod Animation")]
    // public RodController rodController;
    public Slider jiggleSlider; // Reference to your UI slider

    void Start()
    {
        if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();
        if (!lureRb) lureRb = lure.GetComponent<Rigidbody>();

        if (lengthSlider) lengthSlider.onValueChanged.AddListener(OnSliderValueChanged);

        lineRenderer.positionCount = lineSegments;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;

        currentRopeLength = targetRopeLength;

        if (jiggleSlider)
        {
            jiggleSlider.onValueChanged.AddListener(OnJiggleValueChanged);
        }

        void OnJiggleValueChanged(float value)
        {
            // if (rodController != null)
            // {
            //     rodController.OnJiggleSliderChanged(value);
            // }
        }
    }

    void Update()
    {
        // Ensure the line renderer updates when throwing or in water
        if (FishingCast3D.instance._animator.GetBool("throw") || FishingCast3D.instance.isLureInWater)
        {
            lureRb.isKinematic = true;

            currentRopeLength = Mathf.Lerp(currentRopeLength, targetRopeLength, Time.deltaTime * moveSpeed);
            Vector3 direction = (lure.position - rodTip.position).normalized;
            Vector3 newLurePosition = rodTip.position + direction * currentRopeLength;

            lureRb.MovePosition(newLurePosition);
        }
        else
        {
            lureRb.isKinematic = false;
        }

        // Always update fishing line so it's visible
        UpdateFishingLine();
    }

    void UpdateFishingLine()
    {
        if (!rodTip || !lure || !lineRenderer) return;

        List<Vector3> linePoints = new List<Vector3>();

        // Always take the updated rod tip position
        Vector3 startPoint = rodTip.position;
        Vector3 endPoint = lure.position;

        for (int i = 0; i < lineSegments; i++)
        {
            float t = i / (float)(lineSegments - 1);
            Vector3 point = BezierCurve(startPoint, endPoint, t);
            linePoints.Add(point);
        }

        lineRenderer.SetPositions(linePoints.ToArray());

        // Ensure line renderer is enabled while throwing
        if (!lineRenderer.enabled)
            lineRenderer.enabled = true;
    }


    Vector3 BezierCurve(Vector3 start, Vector3 end, float t)
    {
        Vector3 middle = (start + end) / 2 + Vector3.down * 1.5f;
        Vector3 p1 = Vector3.Lerp(start, middle, t);
        Vector3 p2 = Vector3.Lerp(middle, end, t);
        return Vector3.Lerp(p1, p2, t);
    }

    void OnSliderValueChanged(float newLength)
    {
        targetRopeLength = newLength;

        FishingCast3D.instance._animator.SetBool("throw", false);

        // Set "folding" to true while slider is moving
        FishingCast3D.instance._animator.SetBool("folding", true);

        // Restart the coroutine to check if the user stops moving the slider
        if (foldingCoroutine != null)
            StopCoroutine(foldingCoroutine);

        foldingCoroutine = StartCoroutine(ResetFoldingAfterDelay());
    }

    IEnumerator ResetFoldingAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        // Check if the slider is still moving, if not, reset "folding" to false
        if (Mathf.Abs(lengthSlider.value - targetRopeLength) < 0.01f)
        {
            FishingCast3D.instance._animator.SetBool("folding", false);
        }
    }
}

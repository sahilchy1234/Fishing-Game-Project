using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class AutoReturnSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 1f;
    
    private Slider slider;
    private bool isPointerDown = false;
    private bool shouldReturn = false;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = minValue;
    }

    private void Update()
    {
        // Only return if pointer is not down and we're not at min value
        if (!isPointerDown && slider.value > minValue)
        {
            slider.value = Mathf.MoveTowards(slider.value, minValue, returnSpeed * Time.deltaTime);
            shouldReturn = true;
        }
        else if (slider.value <= minValue && shouldReturn)
        {
            shouldReturn = false;
            OnReturnComplete();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
    }

    private void OnReturnComplete()
    {
        // Optional: Add any actions you want when fully returned
        // Debug.Log("Slider fully returned to minimum");
    }
}
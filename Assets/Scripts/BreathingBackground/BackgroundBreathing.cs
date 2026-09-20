
using UnityEngine;

public class BackgroundBreathing : MonoBehaviour
{
    [Header("Breathing Settings")]
    [SerializeField] private float breathingAmount = 0.02f;
    [SerializeField] private float breathingSpeed = 0.5f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        float breath = (Mathf.Sin(Time.time * breathingSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

        float scaleMultiplier = 1f + breath * breathingAmount;

        transform.localScale = originalScale * scaleMultiplier;
    }
}

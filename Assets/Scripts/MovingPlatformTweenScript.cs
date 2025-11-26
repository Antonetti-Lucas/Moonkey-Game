using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatformTweenScript : MonoBehaviour
{
    public Transform origem;
    public Transform destino;

    [Tooltip("Units per second")]
    public float velocidade = 3.0f;

    [Tooltip("How close before snapping and switching direction")]
    public float arrivalThreshold = 0.001f;

    private Vector3 origemPos;
    private Vector3 destinoPos;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float elapsed;
    private float duration;

    void Start()
    {
        if (origem == null || destino == null)
        {
            Debug.LogError("Origem or Destino not assigned on " + name);
            enabled = false;
            return;
        }

        origemPos = origem.position;
        destinoPos = destino.position;

        BeginJourney(destinoPos);
    }

    public static float EaseInOutCubic(float x)
    {
        if (x <= 0f) return 0f;
        if (x >= 1f) return 1f;
        return x < 0.5f
            ? 4f * x * x * x
            : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
    }

    private void BeginJourney(Vector3 newTarget)
    {
        startPos = transform.position;
        targetPos = newTarget;
        elapsed = 0f;

        float distance = Vector3.Distance(startPos, targetPos);

        duration = (velocidade > 0f && distance > 0f) ? distance / velocidade : 0f;

        if (duration <= 0f)
        {
            transform.position = targetPos;
            ToggleNextTarget();
        }
    }

    void FixedUpdate()
    {
        if (duration <= 0f)
            return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float eased = EaseInOutCubic(t);
        transform.position = Vector3.Lerp(startPos, targetPos, eased);

        if (Vector3.Distance(transform.position, targetPos) <= arrivalThreshold || t >= 1f)
        {
            transform.position = targetPos;
            ToggleNextTarget();
        }
    }

    private void ToggleNextTarget()
    {
        Vector3 next = (targetPos == destinoPos) ? origemPos : destinoPos;
        BeginJourney(next);
    }
}

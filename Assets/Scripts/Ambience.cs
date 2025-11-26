using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Ambience : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip clip;
    [Range(0f, 1f)] public float maxVolume = 1f;
    public bool loop = true;

    [Header("Range & Blending")]
    [Tooltip("Distance from the zone edge where sound fades out completely.")]
    public float falloffDistance = 10f;
    [Tooltip("Distance from the edge where sound becomes fully 3D.")]
    public float spatialBlendMaxDistance = 10f;
    [Tooltip("How quickly the sound moves/fades.")]
    public float smoothing = 10f;

    [Header("Player Setup")]
    public Transform player;
    public string playerTag = "Player";

    private Collider zoneCollider;
    private AudioSource source;
    private GameObject emitterObject;
    private float currentBlend = 1f;
    private float currentVolume = 0f;

    void Awake()
    {
        zoneCollider = GetComponent<Collider>();

        // IMPORTANT: Ensure the collider is a Trigger so the player doesn't bump into it physically
        zoneCollider.isTrigger = true;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }

        // Create a separate GameObject to hold the audio source
        // This prevents the Zone itself from moving
        emitterObject = new GameObject("Ambience_Emitter_" + gameObject.name);

        emitterObject.transform.SetParent(this.transform);

        source = emitterObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.clip = clip;
        source.loop = loop;

        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 1f;
        source.maxDistance = Mathf.Max(1f, falloffDistance);
    }

    void Start()
    {
        if (source == null) return;
        source.volume = 0f;
        source.Play();
    }

    void Update()
    {
        if (player == null || source == null) return;

        // Find the closest point on the Zone Collider to the player
        Vector3 closestPoint = zoneCollider.ClosestPoint(player.position);

        // Check distance
        float distanceToEdge = Vector3.Distance(player.position, closestPoint);

        // Check if player is inside.
        bool isInside = distanceToEdge < 0.01f;

        // Calculate Target Values
        float targetBlend = isInside ? 0f : Mathf.Clamp01(distanceToEdge / spatialBlendMaxDistance);

        // Volume logic, Inside = Max Volume, Outside = Fade out based on Falloff Distance.
        float targetVolume = isInside ? maxVolume : Mathf.Clamp01(1f - (distanceToEdge / falloffDistance)) * maxVolume;

        // Apply Smoothing
        currentBlend = Mathf.MoveTowards(currentBlend, targetBlend, smoothing * Time.deltaTime);
        currentVolume = Mathf.MoveTowards(currentVolume, targetVolume, smoothing * Time.deltaTime);

        source.spatialBlend = currentBlend;
        source.volume = currentVolume;

        // Move the Emitter
        if (isInside)
        {
            emitterObject.transform.position = player.position;
        }
        else
        {
            emitterObject.transform.position = closestPoint;
        }
    }

    // Cleanup
    void OnDestroy()
    {
        if (emitterObject != null) Destroy(emitterObject);
    }
}
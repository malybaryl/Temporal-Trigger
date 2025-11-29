using UnityEngine;

/// <summary>
/// Bazowy komponent dla obiektów które reagują na TimeController
/// Dodaj do wrogów, pocisków, itp. aby działały tylko gdy czas płynie
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TimeAwareEntity : MonoBehaviour
{
    [Header("Time Aware Settings")]
    [Tooltip("Czy ten obiekt powinien być zamrożony gdy czas się zatrzyma")]
    public bool freezeWhenTimeStopped = true;

    private Rigidbody2D rb;
    private Vector2 storedVelocity;
    private float storedAngularVelocity;
    private bool wasTimeStopped = false;
    private TimeController timeController;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        timeController = FindObjectOfType<TimeController>();
        if (timeController == null)
        {
            Debug.LogWarning($"TimeAwareEntity ({gameObject.name}): Nie znaleziono TimeController!");
        }
    }

    void FixedUpdate()
    {
        if (!freezeWhenTimeStopped || timeController == null) return;

        bool isTimeStopped = !timeController.IsTimeFlowing();

        if (isTimeStopped && !wasTimeStopped)
        {
            // Czas się właśnie zatrzymał - zamroź obiekt
            FreezeEntity();
        }
        else if (!isTimeStopped && wasTimeStopped)
        {
            // Czas się wznowił - odmroź obiekt
            UnfreezeEntity();
        }

        wasTimeStopped = isTimeStopped;
    }

    void FreezeEntity()
    {
        if (rb != null)
        {
            // Zapisz aktualną prędkość
            storedVelocity = rb.velocity;
            storedAngularVelocity = rb.angularVelocity;
            
            // Zatrzymaj ruch
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true; // Wyłącz fizyk ę
        }
    }

    void UnfreezeEntity()
    {
        if (rb != null)
        {
            // Przywróć kinematic
            rb.isKinematic = false;
            
            // Przywróć prędkość
            rb.velocity = storedVelocity;
            rb.angularVelocity = storedAngularVelocity;
        }
    }

    void OnDestroy()
    {
        // Upewnij się że obiekt nie pozostanie zamrożony
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
}

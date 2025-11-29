using UnityEngine;

/// <summary>
/// Kontroluje smugę za pociskiem (Trail Renderer)
/// Długość i zanikanie skalowane przez GameTime.timescale
/// NIE WYMAGA TEKSTUR - używa wbudowanego Trail Renderer
/// </summary>
[RequireComponent(typeof(TrailRenderer))]
public class TrailController : MonoBehaviour
{
    [Header("Trail Settings")]
    [Tooltip("Bazowy czas życia smugi (w sekundach przy pełnej prędkości)")]
    [SerializeField] private float baseTrailTime = 0.3f;
    
    [Tooltip("Minimalna długość smugi (gdy gracz stoi)")]
    [SerializeField] private float minTrailTime = 0.05f;
    
    [Tooltip("Bazowa szerokość smugi na początku")]
    [SerializeField] private float startWidth = 0.15f;
    
    [Tooltip("Szerokość smugi na końcu")]
    [SerializeField] private float endWidth = 0.02f;
    
    [Header("Colors")]
    [Tooltip("Kolor smugi na początku (pełna jasność)")]
    [SerializeField] private Color startColor = new Color(1f, 0.3f, 0.3f, 1f); // Jasno-czerwony
    
    [Tooltip("Kolor smugi na końcu (zanika)")]
    [SerializeField] private Color endColor = new Color(1f, 0f, 0f, 0f); // Przezroczysty czerwony
    
    [Header("Advanced")]
    [Tooltip("Czy smuga ma świecić (emissive)")]
    [SerializeField] private bool emissive = true;
    
    [Tooltip("Moc emisji światła (0-2)")]
    [Range(0f, 2f)]
    [SerializeField] private float emissionIntensity = 0.5f;

    private TrailRenderer trailRenderer;
    private float currentTrailTime;
    private float lastTimescale; // Zapamiętaj poprzednią wartość timescale

    void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        SetupTrailRenderer();
        lastTimescale = GameTime.timescale;
    }

    void Start()
    {
        // Początkowa długość smugi
        UpdateTrailLength();
    }

    void Update()
    {
        // Aktualizuj długość smugi na podstawie GameTime.timescale
        UpdateTrailLength();
    }

    private void SetupTrailRenderer()
    {
        if (trailRenderer == null) return;

        // Podstawowe ustawienia
        trailRenderer.time = baseTrailTime;
        trailRenderer.startWidth = startWidth;
        trailRenderer.endWidth = endWidth;
        trailRenderer.minVertexDistance = 0.1f;
        trailRenderer.autodestruct = false;
        trailRenderer.emitting = true;

        // Materiał - domyślny Unity Sprites/Default
        if (trailRenderer.material == null)
        {
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Kolory (gradient)
        Gradient gradient = new Gradient();
        
        if (emissive)
        {
            // Wersja świecąca (emissive)
            Color emissiveStart = startColor * emissionIntensity;
            Color emissiveEnd = endColor * emissionIntensity;
            
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(emissiveStart, 0.0f), 
                    new GradientColorKey(emissiveEnd, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(startColor.a, 0.0f), 
                    new GradientAlphaKey(endColor.a, 1.0f) 
                }
            );
        }
        else
        {
            // Wersja standardowa
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(startColor, 0.0f), 
                    new GradientColorKey(endColor, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(startColor.a, 0.0f), 
                    new GradientAlphaKey(endColor.a, 1.0f) 
                }
            );
        }

        trailRenderer.colorGradient = gradient;
    }

    private void UpdateTrailLength()
    {
        if (trailRenderer == null) return;

        // KLUCZOWE: Smuga zanika TYLKO gdy timescale > 0 (gdy gra się toczy)
        // Gdy timescale ≈ 0 (gracze stoją) → smuga pozostaje zamrożona
        
        if (GameTime.timescale > 0.01f)
        {
            // Gra się toczy - smuga zanika normalnie
            // Im wyższy timescale, tym szybciej zanika (krótsza smuga)
            currentTrailTime = Mathf.Lerp(baseTrailTime, minTrailTime, GameTime.timescale);
            trailRenderer.time = currentTrailTime;
        }
        else
        {
            // Gra STOI - smuga pozostaje zamrożona (bardzo długi czas życia)
            trailRenderer.time = 999f; // Praktycznie nieskończony czas - smuga nie zanika
        }
    }

    /// <summary>
    /// Wyczyść smugę natychmiast (użyj przy niszczeniu pocisku)
    /// </summary>
    public void ClearTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    /// <summary>
    /// Włącz/wyłącz smugę
    /// </summary>
    public void SetTrailEnabled(bool enabled)
    {
        if (trailRenderer != null)
        {
            trailRenderer.emitting = enabled;
        }
    }

    void OnDestroy()
    {
        // Opcjonalnie: pozostaw smugę po zniszczeniu pocisku
        // Jeśli chcesz aby smuga znikała natychmiast, odkomentuj:
        // ClearTrail();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Aktualizuj w edytorze podczas zmiany parametrów
        if (Application.isPlaying && trailRenderer != null)
        {
            SetupTrailRenderer();
        }
    }
#endif
}

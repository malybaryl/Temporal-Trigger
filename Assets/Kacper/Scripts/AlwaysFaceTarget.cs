using UnityEngine;

public class WeaponOrbit : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Obiekt, wokó³ którego kr¹¿y broñ (np. Player)")]
    [SerializeField] private Transform pivotPoint;

    [Tooltip("Obiekt, na który celujemy (np. Crosshair)")]
    [SerializeField] private Transform aimTarget;

    [Header("Orbit Settings")]
    [Tooltip("Jak daleko od gracza ma byæ broñ (promieñ ko³a)")]
    public float orbitDistance = 1.5f;

    [Tooltip("Korekta obrotu grafiki (zazwyczaj 0, ale czasem sprite jest krzywy)")]
    public float rotationOffset = 0f;

    [Header("Visuals")]
    [Tooltip("Czy broñ ma siê obracaæ góra-dó³ (flip), gdy celujesz w lewo? (¯eby nie by³a do góry nogami)")]
    public bool flipWeaponSprite = true;

    void LateUpdate()
    {
        if (pivotPoint == null || aimTarget == null) return;

        HandleOrbit();
    }

    void HandleOrbit()
    {
        // 1. Oblicz kierunek od Gracza do Celownika
        Vector3 direction = aimTarget.position - pivotPoint.position;

        // Jeœli celownik jest idealnie na graczu, u¿ywamy ostatniego lub domyœlnego kierunku (prawo)
        if (direction.sqrMagnitude < 0.001f) direction = Vector3.right;
        else direction.Normalize(); // Skracamy wektor do d³ugoœci 1

        // 2. Ustaw pozycjê broni (Gracz + Kierunek * Promieñ)
        // To sprawia, ¿e broñ "jeŸdzi" po okrêgu
        transform.position = pivotPoint.position + (direction * orbitDistance);

        // 3. Oblicz k¹t obrotu
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // 4. Zastosuj obrót
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        // 5. (Opcjonalnie) Flipowanie broni, ¿eby nie by³a do góry nogami przy celowaniu w lewo
        if (flipWeaponSprite)
        {
            HandleWeaponFlip(angle);
        }
    }

    void HandleWeaponFlip(float angle)
    {
        // K¹t w Unity (Atan2) idzie od -180 do 180.
        // Prawa strona to okolice 0. Lewa strona to > 90 lub < -90.
        
        Vector3 localScale = transform.localScale;

        if (Mathf.Abs(angle) > 90)
        {
            // Celujemy w lewo - odwróæ broñ w osi Y (lustrzane odbicie w pionie)
            // Dziêki temu lufa jest nadal w stronê celu, ale broñ nie jest "do góry nogami"
            localScale.y = -1f * Mathf.Abs(localScale.y);
        }
        else
        {
            // Celujemy w prawo - normalna skala
            localScale.y = Mathf.Abs(localScale.y);
        }

        transform.localScale = localScale;
    }
}
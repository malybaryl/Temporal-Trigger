using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Prosta mechanika teleportu góra/dó³ (Toggle).
/// U¿ywa statycznej klasy TeleportState do synchronizacji.
/// </summary>
public class TeleportAbility : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("O ile jednostek postaæ ma siê przemieœciæ w osi Y")]
    [SerializeField] private float teleportDistance = 3f;

    [Header("Input Settings")]
    [Tooltip("Który gamepad steruje t¹ postaci¹? (0 = Gracz 1, 1 = Gracz 2)")]
    public int gamepadIndex = 0;
    public bool useGamepadByIndex = true;

    [Header("Co-op Settings")]
    [Tooltip("Przypisz tutaj transform drugiego gracza, aby ruszaæ siê razem")]
    [SerializeField] private Transform additionalTransform;

    // To wykonuje siê raz przy starcie gry
    void Start()
    {
        // NAPRAWA B£ÊDU: Inicjalizacja stanu musi byæ w metodzie, nie w ciele klasy
        // Ustawiamy stan pocz¹tkowy na false (góra)
        TeleportState.changeState(false);
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        bool inputTriggered = false;

#if ENABLE_INPUT_SYSTEM
        // 1. SprawdŸ Gamepada
        Gamepad myGamepad = GetGamepad();

        if (myGamepad != null)
        {
            // X na Xbox / Kwadrat na PS
            if (myGamepad.buttonWest.wasPressedThisFrame)
            {
                inputTriggered = true;
            }
        }

        // 2. SprawdŸ Klawiaturê (tylko Gracz 1)
        if (gamepadIndex == 0 && Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                inputTriggered = true;
            }
        }
#endif

        // Jeœli wykryto wciœniêcie, wykonaj akcjê
        if (inputTriggered)
        {
            PerformTeleport();
        }
    }

    void PerformTeleport()
    {
        // Tworzymy wektor przesuniêcia (np. 0, 3, 0)
        Vector3 moveVector = new Vector3(0, teleportDistance, 0);

        // Pobieramy aktualny stan ze statycznej klasy
        if (TeleportState.get()) 
        {
            // === SYTUACJA: STAN TRUE (JESTEŒMY NA DOLE) -> LECIMY DO GÓRY ===
            
            // Przesuñ gracza w górê (+)
            transform.position += moveVector;
            
            // Przesuñ drugiego gracza w górê (+)
            if (additionalTransform != null) 
                additionalTransform.position += moveVector;
            
            // Zmieñ stan na false (jesteœmy na górze)
            TeleportState.changeState(false);
        }
        else
        {
            // === SYTUACJA: STAN FALSE (JESTEŒMY NA GÓRZE) -> LECIMY W DÓ£ ===
            
            // Przesuñ gracza w dó³ (-)
            transform.position -= moveVector;
            
            // Przesuñ drugiego gracza w dó³ (-)
            if (additionalTransform != null) 
                additionalTransform.position -= moveVector;
            
            // Zmieñ stan na true (jesteœmy na dole)
            TeleportState.changeState(true);
        }
    }

    /// <summary>
    /// Pobiera pada przypisanego do konkretnego indeksu gracza
    /// </summary>
    Gamepad GetGamepad()
    {
#if ENABLE_INPUT_SYSTEM
        if (useGamepadByIndex)
        {
            if (Gamepad.all.Count > gamepadIndex)
            {
                return Gamepad.all[gamepadIndex];
            }
            return null;
        }
        return Gamepad.current;
#endif
        return null;
    }
}
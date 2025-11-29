using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Prosta mechanika teleportu góra/dó³ (Toggle).
/// Przycisk E (Klawiatura) lub X (Gamepad Xbox) / Kwadrat (Gamepad PS).
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

    // Zmienna przechowuj¹ca stan: czy jesteœmy obecnie "na dole"?
    private bool isTeleportedDown = false;

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        bool inputTriggered = false;

#if ENABLE_INPUT_SYSTEM
        // 1. SprawdŸ Gamepada (zachowuj¹c hierarchiê indeksów)
        Gamepad myGamepad = GetGamepad();

        if (myGamepad != null)
        {
            // ButtonWest to "X" na Xboxie lub "Kwadrat" na PlayStation
            if (myGamepad.buttonWest.wasPressedThisFrame)
            {
                inputTriggered = true;
            }
        }

        // 2. SprawdŸ Klawiaturê (Litera E) - tylko dla Gracza 1 (indeks 0)
        // Zapobiega sytuacji, gdzie Gracz 2 naciska E na klawiaturze i teleportuje obu, 
        // lub teleportuje Gracza 2 który ma graæ tylko na padzie.
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
        if (isTeleportedDown)
        {
            // SYTUACJA: Jesteœmy na dole -> Wracamy do GÓRY
            transform.position += new Vector3(0, teleportDistance, 0);
            isTeleportedDown = false;
            // Debug.Log("Teleport UP (Powrót)");
        }
        else
        {
            // SYTUACJA: Jesteœmy na górze -> Lecimy w DÓ£
            transform.position -= new Vector3(0, teleportDistance, 0);
            isTeleportedDown = true;
            // Debug.Log("Teleport DOWN");
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public enum InputSource { Keyboard = 0, GamepadByIndex = 10 } 
    [Header("Input")]
    public bool useNewInputSystem = true; // ustaw false jeœli nie masz New Input System
    public bool autoSelectLastUsedGamepad = true; // jeœli true, wybierze pad który ostatnio go u¿ywa³
    [Tooltip("Je¿eli u¿ywasz GamepadByIndex, wpisz indeks (0 = pierwszy pad w Gamepad.all)")]
    public int selectedGamepadIndex = 0; // u¿ywane jeœli chcesz wybraæ pad rêcznie
    public bool useGamepadByIndex = false; // true = korzystaj z Gamepad.all[selectedGamepadIndex]

    [Header("Movement")]
    public float moveSpeed = 5f;
    public bool smoothMovement = true;
    [Range(0.001f, 1f)]
    public float smoothing = 0.12f;

    Rigidbody2D rb;
    Vector2 velocitySmoothRef = Vector2.zero;
    Vector2 targetVelocity = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // reaguj na pod³¹czenie/od³¹czenie padów
        if (device is Gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    Debug.Log($"Gamepad added: {device.displayName}");
                    break;
                case InputDeviceChange.Removed:
                    Debug.Log($"Gamepad removed: {device.displayName}");
                    break;
                case InputDeviceChange.Disconnected:
                    Debug.Log($"Gamepad disconnected: {device.displayName}");
                    break;
                case InputDeviceChange.Reconnected:
                    Debug.Log($"Gamepad reconnected: {device.displayName}");
                    break;
            }
        }
    }
#endif

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        Vector2 input = Vector2.zero;

        if (useNewInputSystem && (useGamepadByIndex || autoSelectLastUsedGamepad))
        {
#if ENABLE_INPUT_SYSTEM
            // Auto-select ostatnio u¿ywanego gamepada
            if (autoSelectLastUsedGamepad && Gamepad.current != null)
            {
                // Gamepad.current mo¿e byæ aktualizowany przez Input System gdy pad wyœle sygna³
                input = ReadFromGamepad(Gamepad.current);
            }
            else if (useGamepadByIndex)
            {
                // rêczny wybór po indeksie
                if (Gamepad.all.Count > selectedGamepadIndex && selectedGamepadIndex >= 0)
                {
                    input = ReadFromGamepad(Gamepad.all[selectedGamepadIndex]);
                }
                else
                {
                    // jeœli brak pada o takim indeksie -> fallback
                    input = ReadFromKeyboardFallback();
                }
            }
            else
            {
                // fallback: keyboard
                input = ReadFromKeyboardFallback();
            }
#else
            // New Input System nie jest w kompilacji -> fallback
            input = ReadFromKeyboardFallback();
#endif
        }
        else
        {
            // u¿ywamy klasycznego Input Managera
            input = ReadFromKeyboardFallback();
        }

        input = Vector2.ClampMagnitude(input, 1f);
        targetVelocity = input * moveSpeed;
    }

    Vector2 ReadFromKeyboardFallback()
    {
        Vector2 i = Vector2.zero;
        i.x = Input.GetAxisRaw("Horizontal");
        i.y = Input.GetAxisRaw("Vertical");
        return i;
    }

#if ENABLE_INPUT_SYSTEM
    Vector2 ReadFromGamepad(Gamepad gp)
    {
        if (gp == null) return Vector2.zero;

        Vector2 v = gp.leftStick.ReadValue();

        // fallback do d-pad jeœli joystick bliski 0
        if (v.sqrMagnitude < 0.0001f)
        {
            v = gp.dpad.ReadValue();
        }

        // deadzone
        if (Mathf.Abs(v.x) < 0.05f) v.x = 0f;
        if (Mathf.Abs(v.y) < 0.05f) v.y = 0f;

        return v;
    }
#endif

    void FixedUpdate()
    {
        if (smoothMovement)
        {
            Vector2 newVel = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref velocitySmoothRef, smoothing);
            rb.velocity = newVel;
        }
        else
        {
            rb.velocity = targetVelocity;
        }
    }

    // Narzêdzie debuguj¹ce: poka¿ listê padów i status
    void OnGUI()
    {
        int y = 10;
#if ENABLE_INPUT_SYSTEM
        GUI.Label(new Rect(10, y, 400, 20), $"New Input System enabled. Gamepad count: {Gamepad.all.Count}");
        y += 22;
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            var g = Gamepad.all[i];
            string s = $"Index {i}: {g.displayName} (layout: {g.layout})";
            if (useGamepadByIndex && selectedGamepadIndex == i) s += "  <- selected";
            GUI.Label(new Rect(10, y, 600, 20), s);
            y += 20;
        }

        GUI.Label(new Rect(10, y, 400, 20), $"Auto-select last used: {autoSelectLastUsedGamepad}  Use by index: {useGamepadByIndex}  Selected index: {selectedGamepadIndex}");
#else
        GUI.Label(new Rect(10, y, 400, 20), "New Input System not available - using old Input Manager (keyboard).");
#endif
    }

    // Mo¿esz wywo³aæ tê metodê z innego skryptu (np. menu) by ustawiæ pad przez indeks:
    public void SelectGamepadByIndex(int index)
    {
#if ENABLE_INPUT_SYSTEM
        if (index >= 0 && index < Gamepad.all.Count)
        {
            selectedGamepadIndex = index;
            useGamepadByIndex = true;
            autoSelectLastUsedGamepad = false;
            Debug.Log($"Selected gamepad index {index}: {Gamepad.all[index].displayName}");
        }
        else
        {
            Debug.LogWarning($"No gamepad at index {index}. Count={Gamepad.all.Count}");
        }
#else
        Debug.LogWarning("Cannot select gamepad by index: New Input System not enabled in this build.");
#endif
    }

    // pomocnicza metoda zwracaj¹ca listê nazw padów (do UI)
    public List<string> GetGamepadNames()
    {
        List<string> names = new List<string>();
#if ENABLE_INPUT_SYSTEM
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            names.Add($"Index {i}: {Gamepad.all[i].displayName}");
        }
#endif
        if (names.Count == 0) names.Add("No gamepads connected");
        return names;
    }

    // Reset w edytorze
    void Reset()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}

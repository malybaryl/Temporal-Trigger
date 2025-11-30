using UnityEngine;
using TMPro; // Wa¿ne: Biblioteka do obs³ugi tekstów

public class LevelTimer : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Przeci¹gnij tutaj obiekt Text (TMP) z Canvasa")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Settings")]
    [Tooltip("Czy liczyæ czas od startu sceny?")]
    [SerializeField] private bool startOnAwake = true;

    // Zmienna przechowuj¹ca czas
    private float elapsedTime = 0f;
    private bool isRunning = false;

    void Start()
    {
        if (startOnAwake)
        {
            isRunning = true;
        }
    }

    void Update()
    {
        if (isRunning)
        {
            // WA¯NE: U¿ywamy unscaledDeltaTime, ¿eby liczyæ czas REALNY
            // (nawet jak gra jest w slow-motion)
            elapsedTime += Time.unscaledDeltaTime * GameTime.timescale;

            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        // Matematyka czasu
        float minutes = Mathf.FloorToInt(elapsedTime / 60);
        float seconds = Mathf.FloorToInt(elapsedTime % 60);
        float milliseconds = (elapsedTime % 1) * 100; // Dwie cyfry milisekund

        // Formatowanie tekstu: 00:00:00
        // string.Format pozwala ³adnie ustawiæ zera wiod¹ce (np. 05 zamiast 5)
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    // --- METODY PUBLICZNE (do sterowania z innych skryptów) ---

    public void StopTimer()
    {
        isRunning = false;
        // Opcjonalnie: Zapisz wynik, wyœlij do GameManagera itp.
        Debug.Log("Koniec czasu! Wynik: " + elapsedTime);
    }

    public void ResumeTimer()
    {
        isRunning = true;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerUI();
    }

    /// <summary>
    /// Zwraca dok³adny czas w sekundach (np. do zapisu rekordu)
    /// </summary>
    public float GetTime()
    {
        return elapsedTime;
    }
}
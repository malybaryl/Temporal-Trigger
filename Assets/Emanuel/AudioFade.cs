using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFade : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeDuration = 2f; // czas zanikania w sekundach

    void Start()
    {
        // Zakomentowane, żeby nie uruchamiało się automatycznie
        // StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut()
    {
        if (audioSource == null) yield break;
        
        float startVolume = 1f; // zapamiętaj docelową głośność
        float currentVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration && audioSource != null)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(currentVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        // Reset głośności tylko po zakończeniu fade
        if (audioSource != null)
        {
            audioSource.volume = startVolume;
        }
    }

    // opcjonalnie: fade in
    public IEnumerator FadeIn()
    {
        audioSource.volume = 0f;
        audioSource.Play();

        while (audioSource.volume < 1f)
        {
            audioSource.volume += Time.deltaTime / fadeDuration;
            yield return null;
        }
    }
}

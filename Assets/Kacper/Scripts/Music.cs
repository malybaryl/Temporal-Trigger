using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Music : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        // Pobieramy komponent AudioSource
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // NAPRAWA B£ÊDU:
        // Przypisujemy wartoœæ timescale do pitch (wysokoœæ/prêdkoœæ dŸwiêku)
        // Jeœli timescale spadnie do 0.5, muzyka zwolni o po³owê
        if (audioSource != null)
        {
            audioSource.pitch = GameTime.timescale;
        }
    }
}
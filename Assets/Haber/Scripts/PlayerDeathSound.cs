using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathSound : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip deathClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        audioSource.pitch = 1f;
    }

    public void PlayDeath()
    {
        audioSource.PlayOneShot(deathClip);
    }
}

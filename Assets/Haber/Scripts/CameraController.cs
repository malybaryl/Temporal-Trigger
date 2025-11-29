using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const int targetWidth = 640;
    private const int targetHeight = 360;
    [SerializeField] private int pixelsPerUnit = 48;

    private Camera cam;

    private void Awake()
    {
        Camera cam = Camera.main;
        if (cam == null)
            Debug.LogWarning("KAMERA NIE USTAWIONA NA SCENIE - musi mieæ tag MainCamera");

        if (cam == null || !cam.orthographic)
            return;

        cam.orthographicSize = targetHeight / 2f / pixelsPerUnit;
    }
}

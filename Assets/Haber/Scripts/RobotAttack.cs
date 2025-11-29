using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAttack : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;

    private const float defaultFireCooldownMs = 250;
    private float currFireCooldown;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ResetCooldown()
    {
        currFireCooldown = defaultFireCooldownMs;
    }
}

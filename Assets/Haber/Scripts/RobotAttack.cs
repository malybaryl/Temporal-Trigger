using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAttack : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;

    private const float defaultFireCooldownMs = 250f;
    private float currFireCooldownMs;
    private bool allowFire = false;
    void Start()
    {
        currFireCooldownMs = defaultFireCooldownMs;
    }

    // Update is called once per frame
    void Update()
    {
        if(currFireCooldownMs <= 0)
        {
            currFireCooldownMs = defaultFireCooldownMs;
            //fire
            //Debug.Log("FIREEEEEE");
        }
        currFireCooldownMs -= Time.deltaTime * 1000;
    }

    private void SetAllowFire(bool allow)
    {
        allowFire = allow;

        if (!allowFire)
            currFireCooldownMs = defaultFireCooldownMs;
    }
}

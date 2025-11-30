using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugBullet : MonoBehaviour
{
    private float speed = 4f;
    private float angularOffset = 3f;
    private Vector2 direction = Vector2.zero;
    private float lifetime = 5f;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void OnSpawn(Vector2 spawnDirection)
    {
        float randomAngle = Random.Range(-angularOffset, angularOffset);
        Vector2 rotated = Quaternion.AngleAxis(randomAngle, Vector3.forward) * spawnDirection;
        direction = rotated.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        animator.speed = GameTime.timescale;
        transform.position = transform.position + (Vector3)direction * speed * GameTime.timescale * Time.deltaTime;
        lifetime -= Time.deltaTime * GameTime.timescale;

        if (lifetime < 0)
            Destroy(this.gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignoruj kolizje z graczem (pocisk nie powinien trafiæ w³asnego gracza)
        if (collision.CompareTag("Player"))
        {
            Destroy(this.gameObject);
            OnHitPlayer(collision);
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacles") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(this.gameObject);
            return;
        }
    }

    private void OnHitPlayer(Collider2D playerCol)
    {
        Movement player = playerCol.GetComponent<Movement>();
        if (player != null)
            player.Die();
    }

    public float GetSpeed()
    {
        return speed;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private float speed = 5f;
    private float angularOffset = 10f;
    private Vector2 direction = Vector2.zero;
    private float lifetime = 5f;
    
    public void OnSpawn(Vector2 spawnDirection)
    {
        float randomAngle = Random.Range(-angularOffset, angularOffset);
        Vector2 rotated = Quaternion.AngleAxis(randomAngle, Vector3.forward) * spawnDirection;
        direction = rotated.normalized;
    }

    private void Update()
    {
        transform.position = transform.position + (Vector3)direction * speed * GameTime.timescale * Time.deltaTime;
        lifetime -= Time.deltaTime * GameTime.timescale;

        if( lifetime < 0 )
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

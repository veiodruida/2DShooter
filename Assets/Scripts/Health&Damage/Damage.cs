using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the dealing of damage to health components.
/// </summary>
public class Damage : MonoBehaviour
{
    [Header("Team Settings")]
    [Tooltip("The team associated with this damage")]
    public int teamId = 0;

    [Header("Damage Settings")]
    [Tooltip("How much damage to deal")]
    public int damageAmount = 1;
    [Tooltip("Prefab to spawn after doing damage")]
    public GameObject hitEffect = null;
    [Tooltip("Whether or not to destroy the attached game object after dealing damage")]
    public bool destroyAfterDamage = true;
    [Tooltip("Whether or not to apply damage when triggers collide")]
    public bool dealDamageOnTriggerEnter = false;
    [Tooltip("Whether or not to apply damage when triggers stay, for damage over time")]
    public bool dealDamageOnTriggerStay = false;
    [Tooltip("Whether or not to apply damage on non-trigger collider collisions")]
    public bool dealDamageOnCollision = false;
    [Tooltip("Force to apply to the object hit (knockback/repulsion)")]
    public float repulsionForce = 0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (dealDamageOnTriggerEnter)
        {
            DealDamage(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (dealDamageOnTriggerStay)
        {
            DealDamage(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dealDamageOnCollision)
        {
            DealDamage(collision.gameObject);
        }
    }

    private void DealDamage(GameObject collisionGameObject)
    {
        Health collidedHealth = collisionGameObject.GetComponent<Health>();
        if (collidedHealth == null) return;

        bool hitEnemyTeam = collidedHealth.teamId != teamId;

        if (hitEnemyTeam)
        {
            ApplyRepulsion(collisionGameObject);

            if (!collidedHealth.isInvincible)
            {
                collidedHealth.TakeDamage(damageAmount);

                if (hitEffect != null && !CompareTag("Asteroid"))
                {
                    Instantiate(hitEffect, transform.position, transform.rotation, null);
                }
            }

            if (destroyAfterDamage && !CompareTag("Player"))
            {
                if (CompareTag("Asteroid")) return;

                Health myHealth = GetComponent<Health>();
                if (myHealth != null) myHealth.Die();
                else Destroy(gameObject);
            }

            return;
        }

        if ((collisionGameObject.CompareTag("Player") || collisionGameObject.CompareTag("Asteroid")) && teamId != 0)
        {
            Health myHealth = GetComponent<Health>();
            if (myHealth != null)
            {
                myHealth.TakeDamage(damageAmount);
            }
        }
    }

    private void ApplyRepulsion(GameObject collisionGameObject)
    {
        if (repulsionForce <= 0f) return;

        Rigidbody2D rb = collisionGameObject.GetComponentInParent<Rigidbody2D>();
        if (rb == null) rb = collisionGameObject.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector2 forceDirection = (collisionGameObject.transform.position - transform.position).normalized;
        Controller playerCtrl = collisionGameObject.GetComponentInParent<Controller>();
        if (playerCtrl != null)
        {
            playerCtrl.ApplyKnockback(forceDirection * repulsionForce);
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(forceDirection * repulsionForce, ForceMode2D.Impulse);
    }
}

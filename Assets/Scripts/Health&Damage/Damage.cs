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

    /// <summary>
    /// Description: 
    /// Standard Unity function called whenever a Collider2D enters any attached 2D trigger collider
    /// Inputs:
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (dealDamageOnTriggerEnter)
        {
            DealDamage(collision.gameObject);
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called every frame a Collider2D stays in any attached 2D trigger collider
    /// Inputs:
    /// </summary>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (dealDamageOnTriggerStay)
        {
            DealDamage(collision.gameObject);
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called when a Collider2D hits another Collider2D (non-triggers)
    /// Inputs:
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dealDamageOnCollision)
        {
            DealDamage(collision.gameObject);
        }
    }

    /// <summary>
    /// Description:
    /// This function deals damage to a health component if the collided 
    /// with gameobject has a health component attached AND it is on a different team.
    /// Inputs:
    /// </summary>
    private void DealDamage(GameObject collisionGameObject)
    {
        Health collidedHealth = collisionGameObject.GetComponent<Health>();
        if (collidedHealth != null)
        {
            if (collidedHealth.teamId != this.teamId)
            {
                collidedHealth.TakeDamage(damageAmount);

                // Protocolo de Repulsão (Ricochete/Resistência)
                if (repulsionForce > 0)
                {
                    Rigidbody2D rb = collisionGameObject.GetComponentInParent<Rigidbody2D>();
                    if (rb == null) rb = collisionGameObject.GetComponent<Rigidbody2D>();

                    if (rb != null)
                    {
                        // Direção oposta ao ponto de impacto para criar o efeito de ricochete
                        Vector2 forceDirection = (collisionGameObject.transform.position - transform.position).normalized;
                        
                        // Zera a velocidade atual para garantir que o ricochete seja limpo e imediato
                        rb.linearVelocity = Vector2.zero;
                        rb.AddForce(forceDirection * repulsionForce, ForceMode2D.Impulse);
                        
                        Debug.Log($"<color=orange>FÚRIA:</color> Repulsão aplicada em {collisionGameObject.name} com força {repulsionForce}");
                    }
                }

                // PREVENÇÃO DE TIRO FANTASMA...
                if (hitEffect != null && !this.gameObject.CompareTag("Asteroid"))
                {
                    Instantiate(hitEffect, transform.position, transform.rotation, null);
                }

                // Lógica de dano mútuo...
                if ((collisionGameObject.CompareTag("Player") || collisionGameObject.CompareTag("Asteroid")) && this.teamId != 0)
                {
                    Health minhaHealth = GetComponent<Health>();
                    if (minhaHealth != null)
                    {
                        minhaHealth.TakeDamage(damageAmount);
                        return;
                    }
                }

                // Inimigos colidindo com o Escudo...
                if (destroyAfterDamage && !gameObject.CompareTag("Player"))
                {
                    if (gameObject.CompareTag("Asteroid")) return;

                    Health myHealth = GetComponent<Health>();
                    if (myHealth != null) myHealth.Die();
                    else Destroy(this.gameObject);
                }
            }
        }
    }
}


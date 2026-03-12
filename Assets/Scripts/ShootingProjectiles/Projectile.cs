using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to make projectiles move and deal damage
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The distance this projectile will move each second.")]
    public float projectileSpeed = 3.0f;

    [Header("Damage Settings")]
    [Tooltip("The amount of damage this projectile deals to the MotherShip")]
    public int damage = 1;

    private void Update()
    {
        MoveProjectile();
    }

    private void MoveProjectile()
    {
        // Move o projétil na direção em que está apontando
        transform.position = transform.position + transform.up * projectileSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Detecta quando o projétil entra em contato com outro objeto
    /// </summary>
    // Exemplo de como deve ficar a colisão no script do TIRO
    private void OnTriggerEnter2D(Collider2D other)
    {
       // Debug.Log("O tiro encostou em: " + other.gameObject.name + " com a tag: " + other.tag);
        if (other.CompareTag("Boundary"))
        {
           // Debug.Log("<color=red>Bati na borda e morri!</color>");
            Destroy(gameObject);
            return; // Sai da função para não tentar dar dano na borda
        }

        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth == null) targetHealth = other.GetComponentInParent<Health>();

        
        if (targetHealth != null)
        {
            // 1. Se o tiro for do Inimigo (Team 1)
            if (gameObject.CompareTag("EnemyProjectile"))
            {
                // Bate no Player ou no Escudo (ambos Team 0)
                if (targetHealth.teamId == 0)
                {
                    targetHealth.TakeDamage(1);
                    Destroy(gameObject);
                }
            }
            // 2. Se o tiro for do Player (Team 0)
            else if (gameObject.CompareTag("PlayerProjectile"))
            {
                // Bate em qualquer coisa que não seja Team 0
                if (targetHealth.teamId != 0)
                {
                    targetHealth.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }
       // Debug.Log("Bati em: " + other.gameObject.name);
    }

    private void OnBecameInvisible()
    {
        // Quando o tiro sair totalmente da visão da câmera, ele é destruído
        Destroy(gameObject);
       // Debug.Log("<color=red>desaparece e destruiu</color>");

    }
}
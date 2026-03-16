using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Team Settings")]
    public int teamId = 0;

    [Header("Health Settings")]
    public int defaultHealth = 1;
    public int maximumHealth = 1;
    public int currentHealth = 1;

    public float invincibilityTime = 3f;
    public bool isAlwaysInvincible = false;
    public bool isInvincible = false;

    [Header("Configurações Visuais de Dano")]
    public SpriteRenderer characterSprite;
    public float tempoInvisivel = 3f;

    [Header("Lives settings")]
    public bool useLives = false;
    public int currentLives = 3;
    public int maximumLives = 5;

    [Header("Effects & Polish")]
    public GameObject deathEffect;
    public GameObject hitEffect;

    private SpriteRenderer spriteRenderer;
    private bool estaPiscando = false;
    private float timeToBecomeDamagableAgain = 0;
    private Vector3 respawnPosition;

    public int CurrentHealth => currentHealth;

    void Start()
    {
        SetRespawnPoint(transform.position);
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        ConfigurarDificuldadeInicial();
    }

    void ConfigurarDificuldadeInicial()
    {
        var settings = GameSettings.instance;
        if (settings != null && settings.configAtual != null)
        {
            if (gameObject.CompareTag("Player"))
            {
                currentLives = settings.configAtual.vidasIniciais;
            }
        }
    }

    void Update()
    {
        if (isAlwaysInvincible)
        {
            isInvincible = true;
            return;
        }

        if (isInvincible && Time.time >= timeToBecomeDamagableAgain)
        {
            isInvincible = false;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isAlwaysInvincible || isInvincible) return;
        
        if (gameObject.CompareTag("Player") && GameSettings.instance != null)
        {
            float mult = GameSettings.instance.configAtual.multiplicadorDanoRecebido;
            damageAmount = Mathf.RoundToInt(damageAmount * mult);
        }


        if (invincibilityTime > 0)
        {
            isInvincible = true;
            timeToBecomeDamagableAgain = Time.time + invincibilityTime;

            // O AVISO RESOLVE-SE AQUI:
            // Agora estamos a USAR o valor de 'estaPiscando' para decidir se começa o efeito.
            if (characterSprite != null && !estaPiscando)
            {
                StartCoroutine(EfeitoPiscar());
            }
        }

        if (useLives)
        {
            HandleDeathWithLives();
        }
        else
        {
            currentHealth -= damageAmount;
        }

        if (UIManager.instance != null) UIManager.instance.UpdateUI();
        if (hitEffect != null) Instantiate(hitEffect, transform.position, transform.rotation);

        CheckDeath();
    }

    bool CheckDeath()
    {
        if (currentHealth <= 0)
        {
            Die();
            return true;
        }
        return false;
    }

    public void Die()
    {
        MotherShip boss = GetComponent<MotherShip>();
        if (boss != null) boss.FinalizarBoss();

        if (deathEffect != null) Instantiate(deathEffect, transform.position, transform.rotation);

        if (boss != null)
        {
            Destroy(this.gameObject);
            return;
        }

        if (useLives) HandleDeathWithLives();
        else HandleDeathWithoutLives();
    }

    void HandleDeathWithLives()
    {
        currentLives -= 1;
        if (UIManager.instance != null) UIManager.instance.UpdateUI();

        if (currentLives > 0)
        {
            Respawn();
        }
        else
        {
            if (gameObject.CompareTag("Shield"))
            {
                gameObject.SetActive(false);
                currentLives = maximumLives;
                currentHealth = defaultHealth;
            }
            else
            {
                if (gameObject.CompareTag("Player") && GameManager.instance != null)
                {
                    GameManager.instance.GameOver();
                    ShootingController sc = GetComponent<ShootingController>();
                    if (sc != null) sc.weaponLevel = 1;
                }
                Destroy(this.gameObject);
            }
        }
    }

    void HandleDeathWithoutLives()
    {
        if (gameObject.CompareTag("Player") && GameManager.instance != null) GameManager.instance.GameOver();
        if (gameObject.CompareTag("Shield")) { gameObject.SetActive(false); return; }

        Enemy enemyScript = GetComponent<Enemy>();
        if (enemyScript != null) enemyScript.DoBeforeDestroy();

        Destroy(this.gameObject);
    }

    void Respawn()
    {
        currentHealth = defaultHealth;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }
    }

    IEnumerator EfeitoPiscar()
    {
        estaPiscando = true;
        float tempoPassado = 0f;

        while (tempoPassado < invincibilityTime)
        {
            float ratio = tempoPassado / invincibilityTime;
            float intervalo = Mathf.Lerp(0.05f, 0.3f, ratio);

            if (characterSprite != null)
            {
                Color c = characterSprite.color;
                c.a = (c.a == 1f) ? 0.2f : 1f;
                characterSprite.color = c;
            }

            if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(intervalo);
            tempoPassado += intervalo;
        }

        if (characterSprite != null) characterSprite.color = Color.white;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        estaPiscando = false;
    }

    public void ReceiveHealing(int healingAmount)
    {
        currentHealth += healingAmount;
        if (currentHealth > maximumHealth) currentHealth = maximumHealth;
        if (UIManager.instance != null) UIManager.instance.UpdateUI();
    }

    // Essencial para o funcionamento do Escudo e Re-spawns
    private void OnEnable()
    {
        currentHealth = defaultHealth;
        isInvincible = false;
    }

    public void SetRespawnPoint(Vector3 newPos) => respawnPosition = newPos;
}
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
    public AudioSource hitSound;
    public AudioSource deathSound;

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
        Debug.Log($"<color=yellow>TakeDamage() chamado em: {gameObject.name}, isInvincible: {isInvincible}, invincibilityTime: {invincibilityTime}</color>");
        
        if (isAlwaysInvincible || isInvincible)
        {
            Debug.Log($"<color=orange>IGNORADO - Inimigo invencível!</color>");
            return;
        }
        
        Debug.Log($"<color=cyan>Dano recebido: {damageAmount}</color>");
        
        if (gameObject.CompareTag("Player") && GameSettings.instance != null)
        {
            float mult = GameSettings.instance.configAtual.multiplicadorDanoRecebido;
            damageAmount = Mathf.RoundToInt(damageAmount * mult);
        }

        if (invincibilityTime > 0)
        {
            isInvincible = true;
            timeToBecomeDamagableAgain = Time.time + invincibilityTime;

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
        if (hitSound != null) hitSound.Play();

        CheckDeath();
    }

    bool CheckDeath()
    {
        Debug.Log($"<color=blue>CheckDeath - currentHealth: {currentHealth}, name: {gameObject.name}</color>");
        
        if (currentHealth <= 0)
        {
            Debug.Log($"<color=red>INIMIGO MORTO! Chamando Die()</color>");
            Die();
            return true;
        }
        
        Debug.Log($"<color=orange>Inimigo ainda vivo. Health: {currentHealth}</color>");
        return false;
    }

    public void Die()
    {
        Debug.Log($"<color=yellow>Die() chamado em: {gameObject.name}</color>");
        // --- NOVA LÓGICA PARA ASTEROIDES ---
        Asteroid ast = GetComponent<Asteroid>();
        if (ast != null)
        {
            ast.DividirOuExplodir(); // <--- CHAMA AQUI
        }

        MotherShip boss = GetComponent<MotherShip>();
        if (boss != null) boss.FinalizarBoss();

        if (deathEffect != null)
        {
            Debug.Log($"<color=green>Instanciando deathEffect em: {transform.position}</color>");
            Instantiate(deathEffect, transform.position, transform.rotation);
        }
        else
        {
            Debug.Log($"<color=red>deathEffect é NULL no: {gameObject.name}</color>");
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound.clip, transform.position, deathSound.volume);
        }

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

        // Se for Asteroid, executa a lógica normal de divisão/explisão
        if (gameObject.CompareTag("Asteroid"))
        {
            Debug.Log("Asteroide morto - executando DividirOuExplodir");
            Asteroid ast = GetComponent<Asteroid>();
            if (ast != null)
            {
                ast.DividirOuExplodir();
            }
            Destroy(this.gameObject);
            return;
        }

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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// This class handles the health state of a game object.
/// 
/// Implementation Notes: 2D Rigidbodies must be set to never sleep for this to interact with trigger stay damage
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Team Settings")]
    [Tooltip("The team associated with this damage")]
    public int teamId = 0;

    [Header("Health Settings")]
    [Tooltip("The default health value")]
    public int defaultHealth = 1;
    [Tooltip("The maximum health value")]
    public int maximumHealth = 1;
    [Tooltip("The current in game health value")]
    public int currentHealth = 1;
   
    
    [Tooltip("Invulnerability duration, in seconds, after taking damage")]
    public float invincibilityTime = 3f;
    [Tooltip("Whether or not this health is always invincible")]
    public bool isAlwaysInvincible = false;
    public bool isInvincible = false;
    [Header("Configurações Visuais de Dano")]
    public SpriteRenderer characterSprite; // Arraste o Sprite da nave para aqui no Inspector
    public float blinkInterval = 0.1f;    // Velocidade do piscar

    public int CurrentHealth => currentHealth; // Permite que outros scripts leiam a vida

    [Header("Lives settings")]
    [Tooltip("Whether or not to use lives")]
    public bool useLives = false;
    [Tooltip("Current number of lives this health has")]
    public int currentLives = 3;
    [Tooltip("The maximum number of lives this health can have")]
    public int maximumLives = 5;

    [Header("Configurações de Feedback")]
    public float tempoInvisivel = 3f;
    private SpriteRenderer spriteRenderer;
    private bool estaPiscando = false;
    /// <summary>
    /// Description:
    /// Standard unity funciton called before the first frame update
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    void Start()
    {
        SetRespawnPoint(transform.position);
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        ConfigurarDificuldadeInicial();
    }

    void ConfigurarDificuldadeInicial()
    {
        // Usamos a propriedade instance (garante que tenta buscar se for nulo)
        var settings = GameSettings.instance;

        if (settings != null && settings.configAtual != null)
        {
            // SE FOR O JOGADOR: Carrega as vidas
            if (gameObject.CompareTag("Player"))
            {
                currentLives = settings.configAtual.vidasIniciais;
            }
            // SE FOR INIMIGO/BOSS: Podes usar o arquivo para definir a vida aqui também
            else
            {
                // Exemplo opcional: currentHealth = settings.configAtual.vidaInimigoBase;
            }
        }
        else
        {
            // Se ainda for nulo (ordem de execução), não damos erro fatal, 
            // mas o log avisa qual objeto está "órfão"
            Debug.LogWarning($"<color=yellow>Aviso em {name}:</color> GameSettings ainda não pronto no Start. Usando valores do Inspector.");
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once per frame
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    void Update()
    {
        InvincibilityCheck();
    }

    // The specific game time when the health can be damged again
    private float timeToBecomeDamagableAgain = 0;


    /// <summary>
    /// Description:
    /// Checks against the current time and the time when the health can be damaged again.
    /// Removes invicibility if the time frame has passed
    /// Inputs:
    /// None
    /// Returns:
    /// void (no return)
    /// </summary>
    private void InvincibilityCheck()
    {
        // Adicione um log aqui se quiser ter certeza que está rodando
        // NOVIDADE: Se for "Sempre Invencível", não deixa o Update desligar a proteção!
        if (isAlwaysInvincible)
        {
            isInvincible = true;
            return;
        }

        if (isInvincible && Time.time >= timeToBecomeDamagableAgain)
        {
            isInvincible = false;
            Debug.Log("Invencibilidade temporária terminou.");
        }
    }
    IEnumerator EfeitoPiscar()
    {
        estaPiscando = true;
        float tempoPassado = 0f;

        // Enquanto não atingir o tempo total de invencibilidade
        while (tempoPassado < tempoInvisivel)
        {
            // O "ratio" vai de 0 a 1 conforme o tempo passa
            float ratio = tempoPassado / tempoInvisivel;
            float intervalo = Mathf.Lerp(0.05f, 0.3f, ratio);

            // O intervalo começa curto (ex: 0.05s) e termina longo (ex: 0.3s)
            // Usamos o ratio para "desacelerar" a piscada
            // VERIFICAÇÃO DE SEGURANÇA: Só tenta mudar a cor se o sprite existir
            if (characterSprite != null)
            {
                Color c = characterSprite.color;
                c.a = (c.a == 1f) ? 0.2f : 1f;
                characterSprite.color = c;
            }
            ;
            // Liga/Desliga o Sprite
            spriteRenderer.enabled = !spriteRenderer.enabled;

            // Espera o intervalo calculado
            characterSprite.color = new Color(1, 1, 1, 1f);
            yield return new WaitForSeconds(intervalo);
            tempoPassado += intervalo;
        }

        // Garante que o sprite termine VISÍVEL e reseta o estado
        characterSprite.color = Color.white;
        spriteRenderer.enabled = true;
        isInvincible = false; // Resetamos aqui para garantir sincronia com o visual
        estaPiscando = false;
    }

// Chame esta função dentro do seu TakeDamage quando o player levar dano
    public void IniciarInvencibilidade()
    {
        if (!estaPiscando)
        {
            StartCoroutine(EfeitoPiscar());
        }
    }
    // The position that the health's gameobject will respawn at if lives are being used
    private Vector3 respawnPosition;
    /// <summary>
    /// Description:
    /// Changes the respawn position to a new position
    /// Inputs:
    /// Vector3 newRespawnPosition
    /// Returns:
    /// void (no return)
    /// </summary>
    /// <param name="newRespawnPosition">The new position to respawn at</param>
    public void SetRespawnPoint(Vector3 newRespawnPosition)
    {
        respawnPosition = newRespawnPosition;
    }

    /// <summary>
    /// Description:
    /// Repositions the health's game object to the respawn position and resets the health to the default value
    /// Inputs:
    /// None
    /// Returns:
    /// void (no return)
    /// </summary>
    void Respawn()
    {
        // 1. Move para a posição inicial
       // transform.position = respawnPosition;

        // 2. Reseta a vida
        currentHealth = defaultHealth;

        // Zere a velocidade para ela não continuar derivando após o impacto
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
      
    }

    /// <summary>
    /// Description:
    /// Applies damage to the health unless the health is invincible.
    /// Inputs:
    /// int damageAmount
    /// Returns:
    /// void (no return)
    /// </summary>
    /// <param name="damageAmount">The amount of damage to take</param>
    public void TakeDamage(int damageAmount)
    {
        if (isAlwaysInvincible || isInvincible) return;

        // Se o tempo for 0, ela não fica invencível
        if (invincibilityTime > 0)
        {
            isInvincible = true;
            timeToBecomeDamagableAgain = Time.time + invincibilityTime;

            // SÓ PISCA se houver tempo de invencibilidade definido
            if (characterSprite != null) StartCoroutine(EfeitoPiscar());
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

        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, transform.rotation, null);
        }

        CheckDeath();
        Debug.Log($"{gameObject.name} levou dano real! Vida: {currentHealth}");
    }
   
    /// <summary>
    /// Description:
    /// Applies healing to the health, capped out at the maximum health.
    /// Inputs:
    /// int healingAmount
    /// Returns:
    /// void (no return)
    /// </summary>
    /// <param name="healingAmount">How much healing to apply</param>
    public void ReceiveHealing(int healingAmount)
    {
        currentHealth += healingAmount;
        if (UIManager.instance != null) UIManager.instance.UpdateUI();

        if (currentHealth > maximumHealth)
        {
            currentHealth = maximumHealth;
        }
        CheckDeath();
    }

    [Header("Effects & Polish")]
    [Tooltip("The effect to create when this health dies")]
    public GameObject deathEffect;
    [Tooltip("The effect to create when this health is damaged")]
    public GameObject hitEffect;

        /// <summary>
    /// Description:
    /// Handles the death of the health. If a death effect is set, it is created. If lives are being used, the health is respawned.
    /// If lives are not being used or the lives are 0 then the health's game object is destroyed.
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    // No Health.cs
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
        // 1. Tenta encontrar o script da MotherShip no mesmo objeto
        MotherShip boss = GetComponent<MotherShip>();
        if (boss != null)
        {
            boss.FinalizarBoss(); // Roda a pontuação, limpeza e tremor
        }

        // 2. Efeito de explosão grande
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation, null);
        }

        // 3. SE FOR O BOSS, DESTRUIR AGORA!
        if (boss != null)
        {
            Destroy(this.gameObject);
            return; // Sai da função para não rodar as lógicas de "lives" comuns
        }

        // Lógica normal para inimigos comuns e player
        if (useLives) { HandleDeathWithLives(); }
        else { HandleDeathWithoutLives(); }
    }
    // Esta função roda automaticamente sempre que o objeto passa de Desativado para Ativado
    private void OnEnable()
    {
        // Reseta a vida para o padrão ao reaparecer
        currentHealth = defaultHealth;
        isInvincible = false; // Tira a invencibilidade de quando ele "morreu"
    }
    /// <summary>
    /// Description:
    /// Handles the death of the health when lives are being used
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    void HandleDeathWithLives()
    {
        currentLives -= 1;
        if (UIManager.instance != null) UIManager.instance.UpdateUI();
        //Debug.Log($"<color=red>[DANO CRÍTICO]</color> {gameObject.name} Vida: {currentLives}");
        if (currentLives > 0)
        {
            Respawn();
        }
        else
        {
            // SE FOR O ESCUDO, NÃO DESTRUA!
            if (gameObject.name.Contains("shield") || gameObject.CompareTag("Shield"))
            {
                gameObject.SetActive(false);
                // Opcional: resetar a vida para quando ele for reativado
                currentLives = maximumLives;
                currentHealth = defaultHealth;
                if (UIManager.instance != null) UIManager.instance.UpdateUI();

                if (hitEffect != null)
                {
                    Instantiate(hitEffect, transform.position, Quaternion.identity);
                }
            }
            else
            {
                // Se for a nave ou inimigo, aí sim destrói ou dá Game Over
                if (gameObject.tag == "Player" && GameManager.instance != null)
                {
                    GameManager.instance.GameOver();
                    
                    ShootingController sc = GetComponent<ShootingController>();
                    if (sc != null)
                    {
                        sc.weaponLevel = 1; // Reset da arma ao morrer
                    }
                }
                Destroy(this.gameObject);
            }
        }
    }

    /// <summary>
    /// Description:
    /// Handles death when lives are not being used
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    void HandleDeathWithoutLives()
    {
        if (gameObject.tag == "Player" && GameManager.instance != null)
        {
            GameManager.instance.GameOver();
        }

        // SE FOR O ESCUDO
        if (gameObject.CompareTag("Shield"))
        {
            gameObject.SetActive(false); // Apenas desativa
            return; // IMPORTANTE: Impede que o código abaixo destrua o objeto!
        }

        // Garante que a pontuação seja contada antes de sumir
        Enemy enemyScript = GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.DoBeforeDestroy();
        }

        // Se não for o escudo, destrói normalmente
        Destroy(this.gameObject);
    }

}

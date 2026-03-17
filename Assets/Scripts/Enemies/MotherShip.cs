using System.Collections.Generic;
using UnityEngine;

public class MotherShip : MonoBehaviour
{
    private Health minhaSaude;

    [Header("Configurações do Escudo")]
    public Health escudoHealth;
    public int totalNavesParaEnviar = 10;
    private int navesEnviadas = 0;
    public int vidaEscudoEstagio2 = 15;

    private List<GameObject> navesVivas = new List<GameObject>();
    private bool aguardandoLimpezaDeNaves = false;
    private bool estagio2Ativo = false;

    [Header("Configurações de Spawn")]
    public GameObject[] inimigosPrefabs;
    public Transform[] pontosDeSaida;
    public float intervaloSpawn = 5f;

    [Header("Configurações de Ataque")]
    public GameObject bombaPrefab;
    public Transform pontoDisparoBomba;
    public float intervaloBomba = 3f;

    [Header("Configurações de Fúria (Nível 1)")]
    public float intervaloBombaFuria1 = 1.0f;
    public float velocidadeBombaFuria1 = 10f;

    [Header("Configurações de Fúria (Nível 2)")]
    public float intervaloBombaFuria2 = 0.5f;
    public float velocidadeBombaFuria2 = 15f;

    private int nivelFuriaAtual = 0;

    [Header("Configurações de Ataque por Contato")]
    public int danoAoJogador = 1;
    public float forcaRicochete = 10f;

    [Header("Efeitos de Fogo (Animados)")]
    public GameObject[] fogosEffects;

    private bool fogo1Ligado = false;
    private bool fogo2Ligado = false;
    private bool fogo3Ligado = false;
    private bool fogo4Ligado = false;

    void Start()
    {
        minhaSaude = GetComponent<Health>();

        // 1. SINCRONIZAÇÃO COM O DICULT DATA
        if (GameSettings.instance == null || GameSettings.instance.configAtual == null)
        {
            Debug.LogWarning("MotherShip: GameSettings não encontrado! Usando valores do Inspector.");
        }
        else
        {
            var config = GameSettings.instance.configAtual;
            totalNavesParaEnviar = config.navesParaAbrirEscudo;
            vidaEscudoEstagio2 = config.vidaDoEscudoEstagio2;
            intervaloBomba = config.intervaloBombaBoss;
            intervaloBombaFuria1 = config.intervaloFuria1;
            intervaloBombaFuria2 = config.intervaloFuria2;
            velocidadeBombaFuria1 = config.velocidadeFuria1;
            velocidadeBombaFuria2 = config.velocidadeFuria2;
            intervaloSpawn = config.tempoSpawnInimigos;

            // Ajuste do dano de contacto baseado no multiplicador do arquivo
            danoAoJogador = Mathf.RoundToInt(1 * config.multiplicadorDanoRecebido);
        }

        // 2. AJUSTE DE VIDA (CALCULADO PELO GAMEMANAGER)
        if (GameManager.instance != null && minhaSaude != null)
        {
            int vidaFinal = GameManager.instance.CalcularVidaInimigo(minhaSaude.maximumHealth);

            if (GameSettings.instance != null && GameSettings.instance.dificuldadeSelecionada == GameSettings.Dificuldade.Furia)
            {
                vidaFinal = Mathf.RoundToInt(vidaFinal * 1.5f);
                nivelFuriaAtual = 1;
            }

            minhaSaude.maximumHealth = vidaFinal;
            minhaSaude.currentHealth = vidaFinal;

            // Usando a função que já existe no teu GameManager para o escudo
            vidaEscudoEstagio2 = GameManager.instance.CalcularVidaInimigo(vidaEscudoEstagio2);
        }

        // ... resto do teu código (Inicialização Visual, Invokes, etc) ...
        InicializarComponentes();
    }

    // Apenas para manter a organização, movi o resto do teu Start para cá
    void InicializarComponentes()
    {
        foreach (GameObject fogo in fogosEffects) { if (fogo != null) fogo.SetActive(false); }

        if (escudoHealth != null)
        {
            escudoHealth.isAlwaysInvincible = true;
            escudoHealth.isInvincible = true;
            if (minhaSaude != null)
            {
                minhaSaude.isAlwaysInvincible = true;
                minhaSaude.isInvincible = false;
            }
        }

        CancelInvoke("SpawnInimigo");
        CancelInvoke("LancarBomba");

        InvokeRepeating("SpawnInimigo", 2f, intervaloSpawn);
        InvokeRepeating("LancarBomba", 1f, intervaloBomba);
    }

    void Update()
    {
        VerificarLimpezaDeNaves();
        VerificarEstadoIncendio();
        MonitorarEscudo();

        // Feedback Visual do Escudo Vulnerável
        if (estagio2Ativo && escudoHealth != null && escudoHealth.gameObject.activeSelf)
        {
            SpriteRenderer sr = escudoHealth.GetComponent<SpriteRenderer>();
            if (sr != null && sr.color != Color.red) sr.color = Color.red;
        }
    }

    void MonitorarEscudo()
    {
        bool escudoAtivo = (escudoHealth != null && escudoHealth.gameObject.activeSelf && escudoHealth.currentHealth > 0);

        if (escudoAtivo)
        {
            if (minhaSaude != null) minhaSaude.isAlwaysInvincible = true;
        }
        else if (estagio2Ativo)
        {
            if (minhaSaude != null && minhaSaude.isAlwaysInvincible)
            {
                minhaSaude.isAlwaysInvincible = false;
                if (minhaSaude.characterSprite != null) minhaSaude.characterSprite.color = Color.white;
                Debug.Log("<color=red>Atenção:</color> Nave Mãe vulnerável!");
            }
        }
    }

    void VerificarEstadoIncendio()
    {
        bool escudoDestruido = (escudoHealth == null || !escudoHealth.gameObject.activeSelf);

        if (escudoDestruido && minhaSaude != null)
        {
            float pctVida = (float)minhaSaude.currentHealth / minhaSaude.maximumHealth;

            if (pctVida <= 0.80f && !fogo1Ligado) AtivarFogo(0, ref fogo1Ligado);
            if (pctVida <= 0.60f && !fogo2Ligado) AtivarFogo(1, ref fogo2Ligado);
            if (pctVida <= 0.40f && !fogo3Ligado) AtivarFogo(2, ref fogo3Ligado);
            if (pctVida <= 0.25f && !fogo4Ligado) AtivarFogo(3, ref fogo4Ligado);

            // GATILHO MODO FÚRIA
            if (pctVida <= 0.25f && nivelFuriaAtual < 2)
            {
                AtivarModoFuria(2);
            }
            else if (pctVida <= 0.60f && nivelFuriaAtual < 1)
            {
                AtivarModoFuria(1);
            }
        }
    }

    void AtivarFogo(int indice, ref bool flag)
    {
        if (fogosEffects != null && indice < fogosEffects.Length && fogosEffects[indice] != null)
        {
            fogosEffects[indice].SetActive(true);
            flag = true;
        }
    }

    void SpawnInimigo()
    {
        if (navesEnviadas < totalNavesParaEnviar)
        {
            if (inimigosPrefabs.Length > 0 && pontosDeSaida.Length > 0)
            {
                GameObject novoInimigo = Instantiate(inimigosPrefabs[Random.Range(0, inimigosPrefabs.Length)],
                    pontosDeSaida[Random.Range(0, pontosDeSaida.Length)].position, Quaternion.identity);
                navesVivas.Add(novoInimigo);
                navesEnviadas++;

                if (navesEnviadas >= totalNavesParaEnviar)
                {
                    CancelInvoke("SpawnInimigo");
                    aguardandoLimpezaDeNaves = true;
                }
            }
        }
    }

    void VerificarLimpezaDeNaves()
    {
        if (aguardandoLimpezaDeNaves && !estagio2Ativo)
        {
            for (int i = navesVivas.Count - 1; i >= 0; i--)
            {
                if (navesVivas[i] == null) navesVivas.RemoveAt(i);
            }

            if (navesVivas.Count == 0) AtivarEstagio2();
        }
    }

    void AtivarEstagio2()
    {
        if (estagio2Ativo) return;
        estagio2Ativo = true;

        if (escudoHealth != null)
        {
            escudoHealth.isAlwaysInvincible = false;
            escudoHealth.isInvincible = false;
            escudoHealth.maximumHealth = vidaEscudoEstagio2;
            escudoHealth.currentHealth = vidaEscudoEstagio2;

            if (UIManager.instance != null) UIManager.instance.UpdateUI();
        }
    }

    void LancarBomba()
    {
        if (bombaPrefab != null && pontoDisparoBomba != null)
        {
            GameObject novaBomba = Instantiate(bombaPrefab, pontoDisparoBomba.position, Quaternion.identity);
            Bomb scriptBomba = novaBomba.GetComponent<Bomb>();

            if (scriptBomba != null)
            {
                if (nivelFuriaAtual == 2) scriptBomba.velocidade = velocidadeBombaFuria2;
                else if (nivelFuriaAtual == 1) scriptBomba.velocidade = velocidadeBombaFuria1;
                // Caso contrário usa a velocidade base definida no prefab ou script Bomb
            }
        }
    }

    public void AtivarModoFuria(int nivel)
    {
        if (nivel <= nivelFuriaAtual) return;
        nivelFuriaAtual = nivel;

        float novoIntervalo = (nivel == 2) ? intervaloBombaFuria2 : intervaloBombaFuria1;
        CancelInvoke("LancarBomba");
        InvokeRepeating("LancarBomba", 0.2f, novoIntervalo);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Health saudeDoPlayer = collision.GetComponentInParent<Health>();
        if (saudeDoPlayer == null) saudeDoPlayer = collision.GetComponent<Health>();

        if (saudeDoPlayer != null && saudeDoPlayer.teamId == 0)
        {
            saudeDoPlayer.TakeDamage(danoAoJogador);

            Rigidbody2D rbPlayer = collision.GetComponentInParent<Rigidbody2D>();
            if (rbPlayer != null)
            {
                Vector2 direcao = (collision.transform.position - transform.position).normalized;
                rbPlayer.linearVelocity = Vector2.zero;
                rbPlayer.AddForce(direcao * forcaRicochete, ForceMode2D.Impulse);
            }
        }
    }

    public void FinalizarBoss()
    {
        if (GameManager.instance != null && GameManager.instance.gameIsOver) return;

        LimparCenaParaVitoria();

        if (CameraShake.instance != null) CameraShake.instance.Shake(1.2f, 0.7f);

        if (GameManager.instance != null)
        {
            int pontosDoBoss = GameManager.instance.CalcularPontuacaoFinalBoss();
            GameManager.AddScore(pontosDoBoss);
            GameManager.instance.LevelCleared();
        }
    }

    void LimparCenaParaVitoria()
    {
        string[] tagsParaLimpar = { "Items", "EnemyProjectile" };

        foreach (string tag in tagsParaLimpar)
        {
            GameObject[] objetos = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objetos) Destroy(obj);
        }
    }
}
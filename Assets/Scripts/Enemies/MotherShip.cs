using System.Collections.Generic;
using UnityEngine;

public class MotherShip : MonoBehaviour
{
    // Referência ao componente de vida da própria Nave Mãe
    private Health minhaSaude;

    // --- SEÇÃO DE ESTÁGIOS E ESCUDO ---
    [Header("Configurações do Escudo")]
    public Health escudoHealth; // Arraste o Health do objeto BossShield aqui
    public int totalNavesParaEnviar = 10;
    private int navesEnviadas = 0;
    public int vidaEscudoEstagio2 = 15;

    private List<GameObject> navesVivas = new List<GameObject>();
    private bool aguardandoLimpezaDeNaves = false;
    private bool estagio2Ativo = false;

    // --- SEÇÃO DE SPAWN DE INIMIGOS ---
    [Header("Configurações de Spawn")]
    public GameObject[] inimigosPrefabs;
    public Transform[] pontosDeSaida;
    public float intervaloSpawn = 5f;

    // --- SEÇÃO DE ATAQUE (BOMBAS) ---
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

    // --- LÓGICA DE DANO POR CONTATO ---
    [Header("Configurações de Ataque por Contato")]
    public int danoAoJogador = 1;
    public float forcaRicochete = 10f;

    // --- SEÇÃO DE INCÊNDIO (ANIMAÇÕES) ---
    [Header("Efeitos de Fogo (Animados)")]
    [Tooltip("Arraste aqui os 4 objetos de fogo que têm o Animator")]
    public GameObject[] fogosEffects;

    // Flags para garantir que cada animação ligue apenas uma vez
    private bool fogo1Ligado = false;
    private bool fogo2Ligado = false;
    private bool fogo3Ligado = false;
    private bool fogo4Ligado = false;

    void Start()
    {
        minhaSaude = GetComponent<Health>();

        // 1. Verificação de Segurança (Null Check)
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
        }

        // 2. Ajuste de Vida (Com verificação extra)
        if (GameManager.instance != null && minhaSaude != null)
        {
            int vidaFinal = GameManager.instance.CalcularVidaInimigo(minhaSaude.maximumHealth);

            // ATENÇÃO: Verificamos GameSettings aqui também para evitar erro na linha abaixo
            if (GameSettings.instance != null && GameSettings.instance.dificuldadeSelecionada == GameSettings.Dificuldade.Furia)
            {
                vidaFinal = Mathf.RoundToInt(vidaFinal * 1.5f);
                nivelFuriaAtual = 1;
            }

            minhaSaude.maximumHealth = vidaFinal;
            minhaSaude.currentHealth = vidaFinal;
            vidaEscudoEstagio2 = GameManager.instance.CalcularVidaInimigo(vidaEscudoEstagio2);
        }

        // --- INICIALIZAÇÃO DE COMPONENTES ---
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

        // --- INÍCIO DOS ATAQUES ---
        // IMPORTANTE: Cancelamos qualquer repetição anterior para garantir limpeza
        CancelInvoke("SpawnInimigo");
        CancelInvoke("LancarBomba");

        InvokeRepeating("SpawnInimigo", 2f, intervaloSpawn);
        InvokeRepeating("LancarBomba", 1f, intervaloBomba);
    }

    void Update()
    {
        // 1. Checa se os lacaios morreram para abrir o escudo
        VerificarLimpezaDeNaves();

        // 2. Checa a vida para ativar as animações de fogo (APÓS o escudo cair)
        VerificarEstadoIncendio();

        // Monitoriza se o escudo já era
        MonitorarEscudo();
        // NOVIDADE: Forçar cor vermelha no escudo se ele estiver vulnerável
        if (estagio2Ativo && escudoHealth != null && escudoHealth.gameObject.activeSelf)
        {
            SpriteRenderer sr = escudoHealth.GetComponent<SpriteRenderer>();
            if (sr != null && sr.color != Color.red)
            {
                sr.color = Color.red;
            }
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

                // Aqui, em vez de 3 segundos, podemos apenas resetar a cor da nave
                // para mostrar que ela agora leva dano real.
                if (minhaSaude.characterSprite != null) minhaSaude.characterSprite.color = Color.white;

                Debug.Log("<color=red>Atenção:</color> Nave Mãe vulnerável a dano contínuo!");
            }
        }
    }
    // ==========================================
    // LÓGICA DE INCÊNDIO E FEEDBACK VISUAL
    // ==========================================

    void VerificarEstadoIncendio()
    {
        // Só pega fogo se o escudo já tiver sido destruído (objeto desativado ou nulo)
        bool escudoDestruido = (escudoHealth == null || !escudoHealth.gameObject.activeSelf);

        if (escudoDestruido && minhaSaude != null)
        {
            // Calcula a porcentagem de vida atual (ex: 0.5 = 50%)
            float pctVida = (float)minhaSaude.currentHealth / minhaSaude.maximumHealth;

            // Ativa os fogos conforme os limites definidos
            if (pctVida <= 0.80f && !fogo1Ligado) AtivarFogo(0, ref fogo1Ligado);
            if (pctVida <= 0.60f && !fogo2Ligado) AtivarFogo(1, ref fogo2Ligado);
            if (pctVida <= 0.40f && !fogo3Ligado) AtivarFogo(2, ref fogo3Ligado);
            if (pctVida <= 0.25f && !fogo4Ligado) AtivarFogo(3, ref fogo4Ligado);

            // --- NOVIDADE: GATILHO PARA MODO FÚRIA 2 ---
            // Se a vida cair abaixo de 25% e ainda não estivermos no nível 2 de fúria
            if (pctVida <= 0.25f && nivelFuriaAtual < 2)
            {
                AtivarModoFuria(2);
                Debug.Log("<color=purple>BOSS: MODO FÚRIA 2 ATIVADO! Cadência de tiro máxima!</color>");
            }
            // Opcional: Ativar fúria nível 1 aos 60% se já não estiver ativo
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
            fogosEffects[indice].SetActive(true); // Liga o objeto (a animação "fire" começa aqui)
            flag = true;
            Debug.Log($"<color=orange>Alerta:</color> Dano estrutural! Fogo {indice + 1} ativo.");
        }
    }

    // ==========================================
    // LÓGICA DE ATAQUE E ESTÁGIOS
    // ==========================================

    void SpawnInimigo()
    {
        if (navesEnviadas < totalNavesParaEnviar)
        {
            if (inimigosPrefabs.Length > 0 && pontosDeSaida.Length > 0)
            {
                int indiceInimigo = Random.Range(0, inimigosPrefabs.Length);
                int indicePonto = Random.Range(0, pontosDeSaida.Length);

                GameObject novoInimigo = Instantiate(inimigosPrefabs[indiceInimigo], pontosDeSaida[indicePonto].position, Quaternion.identity);
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
            // Limpa a lista de referências nulas (naves que o player já matou)
            for (int i = navesVivas.Count - 1; i >= 0; i--)
            {
                if (navesVivas[i] == null) navesVivas.RemoveAt(i);
            }

            // Se não sobrar ninguém, abre o escudo
            if (navesVivas.Count == 0)
            {
                AtivarEstagio2();
            }
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

            // NOVIDADE: Força o componente Health a reconhecer a nova vida máxima
            // Se o teu script Health tiver uma função de Reset, usa-a aqui.

            if (UIManager.instance != null) UIManager.instance.UpdateUI();

            SpriteRenderer sr = escudoHealth.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.red;
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
                // Ajusta velocidade da bomba baseado no nível de fúria
                if (nivelFuriaAtual == 2) scriptBomba.velocidade = velocidadeBombaFuria2;
                else if (nivelFuriaAtual == 1) scriptBomba.velocidade = velocidadeBombaFuria1;
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
        // Dano por contato direto com o corpo da Nave Mãe
        Health saudeDoPlayer = collision.GetComponentInParent<Health>();
        if (saudeDoPlayer == null) saudeDoPlayer = collision.GetComponent<Health>();

        if (saudeDoPlayer != null && saudeDoPlayer.teamId == 0)
        {
            saudeDoPlayer.TakeDamage(danoAoJogador);

            // Empurrão físico (Ricochete)
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
        // Garante que só roda uma vez
        if (GameManager.instance != null && GameManager.instance.gameIsOver) return;

        // 1. Limpa os itens da cena (a função que criámos antes)
        LimparCenaParaVitoria();

        // 2. Tremor Épico
        if (CameraShake.instance != null)
        {
            CameraShake.instance.Shake(1.2f, 0.7f);
        }

        // 3. Pontuação e Finalização
        if (GameManager.instance != null)
        {
            int pontosDoBoss = GameManager.instance.CalcularPontuacaoFinalBoss();
            GameManager.AddScore(pontosDoBoss);
            GameManager.instance.LevelCleared();
        }

        Debug.Log("<color=red>BOSS DESTRUÍDO!</color>");
    }

    void LimparCenaParaVitoria()
    {
        // Criamos uma lista de tags que queremos remover da tela
        //string[] tagsParaLimpar = { "Items", "PowerUp", "Coin", "EnemyProjectile", "Bomb" };
        string[] tagsParaLimpar = { "Items", "EnemyProjectile" };

        foreach (string tag in tagsParaLimpar)
        {
            GameObject[] objetos = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objetos)
            {
                // Opcional: Criar uma pequena faísca antes de destruir cada item
                // Instantiate(pequenoEfeito, obj.transform.position, Quaternion.identity);

                Destroy(obj);
            }
        }

        Debug.Log("<color=green>Cena limpa:</color> Todos os itens e tiros foram removidos.");
    }

    // 2. Limpa o OnDestroy (deixa-o vazio ou apaga-o)
    private void OnDestroy()
    {
        // Deixa vazio para evitar o erro de leak de memória
    }
}
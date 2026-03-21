using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float velocidade = 3f;
    public float velocidadeRotacaoVisual = 100f;

    [Header("Sistema de Curva (Steering)")]
    [Tooltip("Quão rápido a bomba consegue virar (0 = reto, 10 = curva fechada)")]
    public float forcaDaCurva = 2f;

    private Transform alvo;
    private Vector3 direcaoAtual;

    void Start()
    {
        // 1. Localiza o jogador pelo Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) alvo = player.transform;

        // 2. Define a direção inicial (para baixo)
        direcaoAtual = Vector3.down;

        // 3. Ajusta a dificuldade baseado no GameManager e GameSettings
        AjustarDificuldade();
    }

    /// <summary>
    /// Aplica multiplicadores de velocidade e curva baseados no modo de jogo e nível.
    /// </summary>
    void AjustarDificuldade()
    {
        // Verifica se as instâncias necessárias existem
        if (GameManager.instance != null && GameSettings.instance != null)
        {
            var dif = GameSettings.instance.dificuldadeSelecionada;

            // Lógica específica para o Modo Fúria (Cuba Mode)
            if (dif == GameSettings.Dificuldade.Furia)
            {
                forcaDaCurva *= 2.5f;
                velocidade *= 1.8f;
            }
            else
            {
                // Cálculo genérico para Fácil, Médio e Difícil
                float nivelDificuldade = (int)dif;
                forcaDaCurva *= (1f + (nivelDificuldade * 0.2f));

                // Escalonamento por nível da fase
                velocidade *= (1f + (GameSettings.instance.nivelAtual * 0.1f));
            }
        }
    }

    void Update()
    {
        if (alvo != null)
        {
            // LÓGICA DE CURVA SUAVE (Steering)
            // Normaliza a direção para onde o jogador está
            Vector3 direcaoDesejada = (alvo.position - transform.position).normalized;

            // Interpola a direção atual com a desejada usando Slerp (Spherical Linear Interpolation)
            // Isso cria o efeito de perseguição curva em vez de uma virada instantânea
            direcaoAtual = Vector3.Slerp(direcaoAtual, direcaoDesejada, Time.deltaTime * forcaDaCurva);

            transform.position += direcaoAtual * velocidade * Time.deltaTime;
        }
        else
        {
            // Se o alvo (Player) for nulo/destruído, mantém a última direção calculada
            transform.position += direcaoAtual * velocidade * Time.deltaTime;
        }

        // Rotação visual constante (efeito de girar no próprio eixo Z)
        transform.Rotate(Vector3.forward * velocidadeRotacaoVisual * Time.deltaTime);

        // FÚRIA OVERRIDE: O Unity Physics Matrix quebrou quando mudaste a Layer/Tag para "Bomb" ou porque
        // os prefabs não têm Rigidbody2D. Isto força ativamente a zona espacial.
        Collider2D[] colisoesForcadas = Physics2D.OverlapCircleAll(transform.position, 0.7f);
        foreach (Collider2D col in colisoesForcadas)
        {
            if (col.CompareTag("Asteroid"))
            {
                // Força a explosão dupla (nativa lá em baixo)
                OnTriggerEnter2D(col); 
            }
            else if (col.CompareTag("PlayerProjectile"))
            {
                // Força a Bomba a receber o tiro e apaga o projétil
                Health myHealth = GetComponent<Health>();
                Damage procDamage = col.GetComponent<Damage>();
                int dmg = procDamage != null ? procDamage.damageAmount : 1;
                
                if (myHealth != null) myHealth.TakeDamage(dmg);
                else Destroy(gameObject);

                Destroy(col.gameObject);
            }
        }
    }

    /// <summary>
    /// Otimização: Destrói o projétil quando ele sai da visão da câmera
    /// </summary>
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Colisão com Asteroide - Bomb garante sua morte c/ explosão e ferindo a pedra.
        if (other.CompareTag("Asteroid"))
        {
            // O Spawner e o Unity podem atrasar/excluir eventos simultâneos, forçamos dano daqui.
            Health asteroidHealth = other.GetComponent<Health>();
            if (asteroidHealth != null) asteroidHealth.TakeDamage(999);
            
            Health myHealth = GetComponent<Health>();
            if (myHealth != null) 
            {
                myHealth.Die();
            }
            else 
            {
                Damage myDamage = GetComponent<Damage>();
                if (myDamage != null && myDamage.hitEffect != null)
                {
                    Instantiate(myDamage.hitEffect, transform.position, transform.rotation);
                }
                Destroy(gameObject);
            }
        }
    }
}
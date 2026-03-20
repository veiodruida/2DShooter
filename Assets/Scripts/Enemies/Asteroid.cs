using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("Movimento e Rotação")]
    public float velocidadeMin = 2f;
    public float velocidadeMax = 5f;
    public float rotacaoMin = 30f;
    public float rotacaoMax = 100f;

    [Header("Configurações de Divisão")]
    public int geracaoAtual = 0; // 0 = Grande, 1 = Médio, 2 = Pequeno
    public int limiteGeracoes = 2;
    public int quantidadeFilhos = 2;
    public float multiplicadorEscalaFilho = 0.5f;

    [Header("Efeitos")]
    public GameObject particulasExplosao;

    private float velocidadeReal;
    private float rotacaoReal;
    private float direcaoRotacao;
    private Vector3 direcaoMovimento = Vector3.down;
    private bool estaDestruindo = false;
    void Start()
    {
        // Define valores aleatórios no nascimento
        velocidadeReal = Random.Range(velocidadeMin, velocidadeMax);
        rotacaoReal = Random.Range(rotacaoMin, rotacaoMax);
        direcaoRotacao = (Random.value > 0.5f) ? 1f : -1f;

        // Se for o asteroide original (Geração 0), dá um tamanho aleatório inicial
        if (geracaoAtual == 0)
        {
            float tam = Random.Range(1.5f, 2.5f);
            transform.localScale = new Vector3(tam, tam, 1);
        }

        // Direção levemente aleatória para não descerem todos em linha reta perfeita
        direcaoMovimento = new Vector3(Random.Range(-0.2f, 0.2f), -1, 0).normalized;
    }

    void Update()
    {
        // Move para baixo no espaço global
        transform.Translate(direcaoMovimento * velocidadeReal * Time.deltaTime, Space.World);

        // Gira no próprio eixo
        transform.Rotate(0, 0, direcaoRotacao * rotacaoReal * Time.deltaTime);

        // DESTRUIÇÃO: Só destrói se passar MUITO dos limites da câmera
        // Se sua câmera termina em Y = -10, coloque -15 para garantir que ele sumiu da visão
        if (transform.position.y < -35f || transform.position.y > 35f ||
            transform.position.x > 30f || transform.position.x < -30f)
        {
            Destroy(gameObject);
        }
    }

    // Chamado pelo Health.cs no método Die()
    public void DividirOuExplodir()
    {
        if (estaDestruindo) return;
        estaDestruindo = true;

        if (geracaoAtual < limiteGeracoes)
        {
            SpawnFilhos();
        }
        else
        {
            SoltarParticulas();
        }

        // Opcional: Desativar o colisor imediatamente para não levar "double damage"
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    void SpawnFilhos()
    {
        for (int i = 0; i < quantidadeFilhos; i++)
        {
            GameObject filhoGO = Instantiate(gameObject, transform.position, Quaternion.identity);
            Asteroid filhoScript = filhoGO.GetComponent<Asteroid>();

            // Configura o filho para a próxima geração
            filhoScript.geracaoAtual = geracaoAtual + 1;
            filhoGO.transform.localScale = transform.localScale * multiplicadorEscalaFilho;

            // Faz o filho ser um pouco mais rápido que o pai
            filhoScript.velocidadeReal = velocidadeReal * 1.2f;

            // Dá uma direção lateral para se separarem
            float desvioX = (i == 0) ? -0.5f : 0.5f;
            filhoScript.direcaoMovimento = new Vector3(direcaoMovimento.x + desvioX, direcaoMovimento.y, 0).normalized;
        }
    }

    void SoltarParticulas()
    {
        if (particulasExplosao != null)
        {
            GameObject fx = Instantiate(particulasExplosao, transform.position, Quaternion.identity);
            Destroy(fx, 2f); // Garante limpeza da memória
        }
    }
}
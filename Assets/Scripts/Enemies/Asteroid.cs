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

    [Header("Tamanho (Geração 0)")]
    public float tamanhoMin = 1.5f;
    public float tamanhoMax = 2.5f;

    [HideInInspector] public bool tamanhoDefinido = false;
    [HideInInspector] public float velocidadeReal;
    public Vector3 direcaoMovimento = Vector3.zero;

    private float rotacaoReal;
    private float direcaoRotacao;
    private bool estaDestruindo = false;

    void Start()
    {
        // Define valores aleatórios no nascimento
        if (velocidadeReal <= 0) velocidadeReal = Random.Range(velocidadeMin, velocidadeMax);
        
        rotacaoReal = Random.Range(rotacaoMin, rotacaoMax);
        direcaoRotacao = (Random.value > 0.5f) ? 1f : -1f;

        // Se for o asteroide original (Geração 0) e tamanho não foi definido externamente
        if (geracaoAtual == 0 && !tamanhoDefinido)
        {
            float tam = Random.Range(tamanhoMin, tamanhoMax);
            transform.localScale = new Vector3(tam, tam, 1);
        }

        // Se a direção não foi definida pelo spawner, usa o padrão (baixo)
        if (direcaoMovimento == Vector3.zero)
        {
            direcaoMovimento = new Vector3(Random.Range(-0.2f, 0.2f), -1, 0).normalized;
        }
    }

    void Update()
    {
        // Move no espaço global
        transform.Translate(direcaoMovimento * velocidadeReal * Time.deltaTime, Space.World);

        // Gira no próprio eixo
        transform.Rotate(0, 0, direcaoRotacao * rotacaoReal * Time.deltaTime);

        // DESTRUIÇÃO: Só destrói se passar MUITO dos limites da câmera
        if (transform.position.y < -35f || transform.position.y > 35f ||
            transform.position.x > 35f || transform.position.x < -35f)
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

        // Desativar o colisor imediatamente
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
            if (particulasExplosao.GetComponent<Projectile>() != null)
            {
                Debug.LogWarning("FÚRIA ABSOLUTA: particulasExplosao no Asteroide é um Tiro/Projétil! Ignorando.");
                return;
            }
            GameObject fx = Instantiate(particulasExplosao, transform.position, Quaternion.identity);
            Destroy(fx, 2f); // Garante limpeza da memória
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Shield"))
        {
            Damage otherDamage = other.GetComponent<Damage>() ?? other.GetComponentInParent<Damage>();
            int baseDamage = otherDamage != null ? otherDamage.damageAmount : 1;

            Damage myDamage = GetComponent<Damage>();
            int dealtDamage = myDamage != null ? myDamage.damageAmount : 1;

            Health shieldHealth = other.GetComponent<Health>();
            if (shieldHealth != null) shieldHealth.TakeDamage(dealtDamage);

            Health myHealth = GetComponent<Health>();
            if (myHealth != null) myHealth.TakeDamage(baseDamage);
        }
        else if (other.CompareTag("Boss") || other.CompareTag("Enemy") || other.CompareTag("EnemyProjectile") || other.CompareTag("Bomb"))
        {
            Damage otherDamage = other.GetComponent<Damage>() ?? other.GetComponentInParent<Damage>();
            int baseDamage = otherDamage != null ? otherDamage.damageAmount : 1;

            Damage myDamage = GetComponent<Damage>();
            int dealtDamage = myDamage != null ? myDamage.damageAmount : 1;

            // Asteroide leva dano real do alvo
            Health myHealth = GetComponent<Health>();
            if (myHealth != null) myHealth.TakeDamage(baseDamage);

            // O alvo leva o dano exato do asteroide
            Health targetHealth = other.GetComponent<Health>();
            if (targetHealth != null) 
            {
                targetHealth.TakeDamage(dealtDamage);
            }
        }
    }
}
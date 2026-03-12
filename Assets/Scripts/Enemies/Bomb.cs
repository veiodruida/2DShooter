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
        // 1. Localiza o jogador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) alvo = player.transform;

        // 2. Define a direção inicial (para baixo)
        direcaoAtual = Vector3.down;

        // 3. Ajusta a dificuldade baseado no GameManager
        AjustarDificuldade();
    }

    void AjustarDificuldade()
    {
        // Verifica se AMBAS as instâncias existem antes de fazer qualquer cálculo
        if (GameManager.instance != null && GameSettings.instance != null)
        {
            // 1. Pegamos a dificuldade do GameSettings (que é o nosso padrão agora)
            var dif = GameSettings.instance.dificuldadeSelecionada;

            // Se for Fúria (Cuba Mode), a curva e a velocidade sobem drasticamente
            if (dif == GameSettings.Dificuldade.Furia)
            {
                forcaDaCurva *= 2.5f;
                velocidade *= 1.8f;
            }
            else
            {
                // Usamos o index da dificuldade (0, 1, 2...) para um cálculo genérico
                float nivelDificuldade = (int)dif;
                forcaDaCurva *= (1f + (nivelDificuldade * 0.2f));

                // Usamos o nível da fase vindo do GameSettings ou GameManager
                velocidade *= (1f + (GameSettings.instance.nivelAtual * 0.1f));
            }
        }
        else
        {
            // Debug opcional para te avisar no console se algo faltar durante os testes
            // Debug.LogWarning("Bomb: GameSettings ou GameManager não encontrados na cena.");
        }
    }

    void Update()
    {
        if (alvo != null)
        {
            // LÓGICA DE CURVA SUAVE
            Vector3 direcaoDesejada = (alvo.position - transform.position).normalized;

            // Interpola a direção atual com a desejada para criar o efeito de curva
            direcaoAtual = Vector3.Slerp(direcaoAtual, direcaoDesejada, Time.deltaTime * forcaDaCurva);

            transform.position += direcaoAtual * velocidade * Time.deltaTime;
        }
        else
        {
            // Se o jogador morrer, continua em linha reta para fora da tela
            transform.position += direcaoAtual * velocidade * Time.deltaTime;
        }

        // Rotação visual constante
        transform.Rotate(Vector3.forward * velocidadeRotacaoVisual * Time.deltaTime);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
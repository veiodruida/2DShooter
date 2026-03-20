using UnityEngine;

public class PlayerBomb : MonoBehaviour
{
    [Header("Configurações de Voo")]
    public float velocidade = 8f;
    public float tempoDeVida = 1.5f;
    
    [Header("Perseguição de Alvo")]
    public Transform alvo; // Você arrasta o inimigo/MotherShip no Inspector
    public float forcaDaCurva = 5f; // Quão rápido a bomba vira para o alvo

    [Header("Controlo de Tamanho")]
    public float tamanhoInicial = 0.2f;
    public float tamanhoFinal = 3.0f;

    private ScreenClearBomb scriptPai;
    private float timerCrescimento = 0f;
    private Vector3 direcaoAtual = Vector3.up;

    public void Inicializar(ScreenClearBomb pai)
    {
        scriptPai = pai;
        transform.localScale = new Vector3(tamanhoInicial, tamanhoInicial, 1f);
    }

    void Update()
    {
        // Se houver um alvo, segue ele. Senão, vai para cima
        if (alvo != null && alvo.gameObject.activeSelf)
        {
            Vector3 direcaoDesejada = (alvo.position - transform.position).normalized;
            direcaoAtual = Vector3.Slerp(direcaoAtual, direcaoDesejada, Time.deltaTime * forcaDaCurva);
        }
        else
        {
            // Se o alvo foi destruído, continua na direção atual
            direcaoAtual = Vector3.Slerp(direcaoAtual, Vector3.up, Time.deltaTime * forcaDaCurva);
        }

        // Movimento na direção calculada
        transform.Translate(direcaoAtual * velocidade * Time.deltaTime);

        // Crescimento Linear Lento
        if (timerCrescimento < tempoDeVida)
        {
            timerCrescimento += Time.deltaTime;
            float progresso = timerCrescimento / tempoDeVida;

            // Suaviza o crescimento (opcional, deixa mais orgânico)
            float progressoSuave = Mathf.SmoothStep(0f, 1f, progresso);
            float tamanhoAtual = Mathf.Lerp(tamanhoInicial, tamanhoFinal, progressoSuave);

            transform.localScale = new Vector3(tamanhoAtual, tamanhoAtual, 1f);
        }
        else
        {
            DetonarAgora();
        }
    }

    void DetonarAgora()
    {
        if (scriptPai != null)
        {
            scriptPai.AtivarOndaDeChoque(this.transform.position);
        }
        Destroy(gameObject);
    }
}
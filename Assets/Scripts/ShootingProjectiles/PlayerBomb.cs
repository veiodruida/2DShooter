using UnityEngine;

public class PlayerBomb : MonoBehaviour
{
    [Header("Configurações de Voo")]
    public float velocidade = 8f;
    public float tempoDeVida = 1.5f;

    [Header("Controlo de Tamanho")]
    public float tamanhoInicial = 0.2f;
    public float tamanhoFinal = 3.0f;

    private ScreenClearBomb scriptPai;
    private float timerCrescimento = 0f;

    // Mudamos o tipo do parâmetro para aceitar o script do Player
    public void Inicializar(ScreenClearBomb pai)
    {
        scriptPai = pai;
        transform.localScale = new Vector3(tamanhoInicial, tamanhoInicial, 1f);
    }

    void Update()
    {
        // Movimento para cima
        transform.Translate(Vector3.up * velocidade * Time.deltaTime);

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
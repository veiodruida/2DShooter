using UnityEngine;
using TMPro; // Remova se usar o Text padrao da Unity

public class UIManager_Timer : MonoBehaviour
{
    [Header("Referencias de Texto")]
    public TextMeshProUGUI textoCronometro; // Arraste o TimerText aqui
    public TextMeshProUGUI textoScore;      // Arraste o seu texto de Score aqui (opcional)

    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.gameIsOver)
        {
            AtualizarInterface();
        }
    }

    void AtualizarInterface()
    {
        // 1. FORMATAR O TEMPO (00:00)
        float tempo = GameManager.instance.tempoDaFase;
        string minutos = Mathf.FloorToInt(tempo / 60).ToString("00");
        string segundos = Mathf.FloorToInt(tempo % 60).ToString("00");

        if (textoCronometro != null)
        {
            textoCronometro.text = $"TIMER: {minutos}:{segundos}";
        }

        // 2. ATUALIZAR SCORE (OPCIONAL)
        if (textoScore != null)
        {
            textoScore.text = $"SCORE: {GameManager.score}";
        }
        // Se passar de 2 minutos (120 segundos), o texto fica vermelho
        if (tempo > 120f)
            textoCronometro.color = Color.red;
        else
            textoCronometro.color = Color.white;
    }
}

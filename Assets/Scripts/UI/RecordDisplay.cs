using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RecordsDisplay : MonoBehaviour
{
    public TextMeshProUGUI textoPontosRecorde;
    public TextMeshProUGUI textoTempoRecorde;

    void Start()
    {
        // 1. Carregar os pontos (se não existir, mostra 0)
        int pontos = PlayerPrefs.GetInt("highscore", 0);
        textoPontosRecorde.text = "MAX SCORE: " + pontos.ToString();

        // 2. Carregar o tempo (se não existir, mostra "---")
        if (PlayerPrefs.HasKey("melhor_tempo"))
        {
            float tempo = PlayerPrefs.GetFloat("melhor_tempo");
            textoTempoRecorde.text = "BEST TIME: " + tempo.ToString("F2") + "s";
        }
        else
        {
            textoTempoRecorde.text = "MELHOR TEMPO: ---";
        }
    }

    // Função para o botão "Voltar"
    public void VoltarParaMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Coloque o nome da sua cena de menu
    }
}
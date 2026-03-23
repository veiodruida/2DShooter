using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RecordDisplay : MonoBehaviour
{
    public TextMeshProUGUI textoPontosRecorde;
    public TextMeshProUGUI textoTempoRecorde;

    void Start()
    {
        // 1. Carregar os pontos (se nao existir, mostra 0)
        int pontos = PlayerPrefs.GetInt("highscore", 0);
        textoPontosRecorde.text = "MAX SCORE: " + pontos.ToString();

        // 2. Carregar o tempo (se nao existir, mostra "---")
        if (PlayerPrefs.HasKey("melhor_tempo"))
        {
            float tempo = PlayerPrefs.GetFloat("melhor_tempo");
            textoTempoRecorde.text = "BEST TIME: " + tempo.ToString("F2") + "s";
        }
        else
        {
            textoTempoRecorde.text = "BEST TIME: ---";
        }
    }

    // Funcao para o botao "Voltar"
    public void VoltarParaMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

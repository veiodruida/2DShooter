using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuBridge : MonoBehaviour
{
    [Header("Arraste os objetos do seu Canvas aqui")]
    public TextMeshProUGUI infoDificuldadeTexto;
    public Image imagemBandeira;
    public GameObject botaoDificuldadeExtra;

    private void Start()
    {
        // Assim que o menu abre, ele sincroniza o visual com o que está salvo
        int salva = PlayerPrefs.GetInt("DificuldadeSelecionada", 0);
        AtualizarVisualLocal(salva);
    }

    public void AlterarDificuldade(int indice)
    {
        if (GameSettings.instance != null)
        {
            // 1. Manda a ordem para o GameSettings (que cuida da lógica)
            GameSettings.instance.SetDificuldade(indice);

            // 2. Atualiza a UI do menu imediatamente através da ponte
            AtualizarVisualLocal(indice);
        }
    }

    private void AtualizarVisualLocal(int indice)
    {
        if (infoDificuldadeTexto == null || imagemBandeira == null) return;
        infoDificuldadeTexto.color = Color.white;
      
        // Pegamos as sprites que estão guardadas no GameSettings
        var gs = GameSettings.instance;
        if (gs == null) return;

        switch (indice)
        {
            case 0:
                infoDificuldadeTexto.text = "DIFFICULTY: USA";
                infoDificuldadeTexto.color = new Color(0.2f, 0.5f, 1f);
                imagemBandeira.sprite = gs.flagUSA;
                break;
            case 1:
                infoDificuldadeTexto.text = "DIFFICULTY: RUSSIA";
                infoDificuldadeTexto.color = Color.white;
                imagemBandeira.sprite = gs.flagRussia;
                break;
            case 2:
                infoDificuldadeTexto.text = "DIFFICULTY: NORTH KOREA";
                infoDificuldadeTexto.color = new Color(1f, 0.5f, 0f);
                imagemBandeira.sprite = gs.flagNKorea;
                break;
            case 3:
                infoDificuldadeTexto.text = "!!! CUBA MODE ACTIVATED !!!";
                infoDificuldadeTexto.color = Color.red;
                imagemBandeira.sprite = gs.flagCUBA;
                break;
        }

        if (botaoDificuldadeExtra != null)
            botaoDificuldadeExtra.SetActive(PlayerPrefs.GetInt("JogoFinalizado", 0) == 1);
    }

    public void AbrirPagina(string nomeDaPagina) => UIManager.instance?.GoToPageByName(nomeDaPagina);
    public void IniciarNovoJogo(string nomeDaCena) => SceneManager.LoadScene(nomeDaCena);
    public void SairDoJogo() => Application.Quit();
}
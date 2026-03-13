using UnityEngine;

public class MainMenuBridge : MonoBehaviour
{
    // Funções simples que os botões vão chamar
    // Elas servem apenas para repassar o comando para o UIManager persistente

    public void AbrirPagina(string nomeDaPagina)
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.GoToPageByName(nomeDaPagina);
        }
        else
        {
            Debug.LogError("Ponte: UIManager não encontrado na cena!");
        }
    }

    public void IniciarNovoJogo(string nomeDaCena)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(nomeDaCena);
    }

    public void SairDoJogo()
    {
        Debug.Log("Saindo...");
        Application.Quit();
    }

    public void AlterarDificuldade(int indice)
    {
        // 0=Facil, 1=Medio, 2=Dificil, 3=Furia
        if (GameSettings.instance != null)
        {
            GameSettings.instance.dificuldadeSelecionada = (GameSettings.Dificuldade)indice;

            // Salva para que o GameManager leia ao carregar a fase
            PlayerPrefs.SetInt("DificuldadeSalva", indice);
            PlayerPrefs.Save();

            Debug.Log("<color=yellow>Dificuldade definida para: </color>" + GameSettings.instance.dificuldadeSelecionada);
        }
        else
        {
            Debug.LogError("Erro: GameSettings não encontrado na cena!");
        }
    }
}
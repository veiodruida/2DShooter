using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIButtonFix : MonoBehaviour
{
    public enum ButtonType { Unpause, GoToMenu, StartGame, GoToPage, QuitGame }
    public ButtonType tipo;

    [Tooltip("Nome EXATO do objeto da pagina na Hierarquia (ex: Instructions)")]
    public string nomeAlvo;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn == null) return;

        btn.onClick.RemoveAllListeners();

        switch (tipo)
        {
            case ButtonType.Unpause:
                btn.onClick.AddListener(() => { if (UIManager.instance != null) UIManager.instance.TogglePause(); });
                break;
            case ButtonType.GoToMenu:
                btn.onClick.AddListener(() => { if (UIManager.instance != null) UIManager.instance.VoltarAoMenu(); });
                break;
            case ButtonType.StartGame:
                // Carrega a cena pelo nome definido no nomeAlvo (padrao Level1)
                btn.onClick.AddListener(() => { SceneManager.LoadScene(string.IsNullOrEmpty(nomeAlvo) ? "Level1" : nomeAlvo); });
                break;
            case ButtonType.GoToPage:
                btn.onClick.AddListener(() => { if (UIManager.instance != null) UIManager.instance.GoToPageByName(nomeAlvo); });
                break;
            case ButtonType.QuitGame:
                btn.onClick.AddListener(() => { Application.Quit(); });
                break;
        }
    }
}

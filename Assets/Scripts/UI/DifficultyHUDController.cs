using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyHUDController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI difficultyText;
    public Image flagImage;

    private void Start()
    {
        AtualizarHUD();
    }

    private void OnEnable()
    {
        // Caso a UI seja desativada/ativada, garante que esteja sincronizada
        AtualizarHUD();
    }

    public void AtualizarHUD()
    {
        if (GameSettings.instance == null) return;
        if (difficultyText == null || flagImage == null) return;

        int index = (int)GameSettings.instance.dificuldadeSelecionada;
        var gs = GameSettings.instance;

        // Sincroniza cores e textos com o MainMenuBridge
        switch (index)
        {
            case 0: // Facil / USA
                difficultyText.text = "Dificuldade: USA";
                difficultyText.color = new Color(0.2f, 0.5f, 1f);
                flagImage.sprite = gs.flagUSA;
                break;
            case 1: // Medio / RUSSIA
                difficultyText.text = "Dificuldade: RUSSIA";
                difficultyText.color = Color.white;
                flagImage.sprite = gs.flagRussia;
                break;
            case 2: // Dificil / N. KOREA
                difficultyText.text = "Dificuldade: NORTH KOREA";
                difficultyText.color = new Color(1f, 0.5f, 0f);
                flagImage.sprite = gs.flagNKorea;
                break;
            case 3: // Furia / CUBA
                difficultyText.text = "!!! CUBA MODE !!!";
                difficultyText.color = Color.red;
                flagImage.sprite = gs.flagCUBA;
                break;
        }
    }
}

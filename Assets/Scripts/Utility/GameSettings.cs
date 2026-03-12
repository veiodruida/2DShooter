using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    private static GameSettings _instance;
    public static GameSettings instance
    {
        get { if (_instance == null) _instance = FindFirstObjectByType<GameSettings>(); return _instance; }
    }

    public enum Dificuldade { Facil, Medio, Dificil, Furia }

    [Header("Seleção Atual")]
    public Dificuldade dificuldadeSelecionada = Dificuldade.Medio;
    public int nivelAtual = 1;

    [Header("Assets de Dificuldade")]
    public DifficultyData dificuldadeFacil;
    public DifficultyData dificuldadeNormal;
    public DifficultyData dificuldadeDificil;
    public DifficultyData dificuldadeFuria;

    [Header("UI Feedback (MainMenu)")]
    public TextMeshProUGUI infoDificuldadeTexto;
    public Image imagemBandeira;
    public GameObject botaoDificuldadeExtra;

    [Header("Sprites das Bandeiras")]
    public Sprite flagUSA;
    public Sprite flagRussia;
    public Sprite flagNKorea;
    public Sprite flagCUBA;

    [HideInInspector] public DifficultyData configAtual;

    void Awake()
    {
        if (transform.parent != null) transform.SetParent(null);
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            AtualizarConfigAtual();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() { SceneManager.sceneLoaded += AoCarregarCena; }
    private void OnDisable() { SceneManager.sceneLoaded -= AoCarregarCena; }

    void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        if (cena.name == "MainMenu")
        {
            // Busca as referências de UI pelo nome exato na Hierarchy
            infoDificuldadeTexto = GameObject.Find("DificuldadeTexto")?.GetComponent<TextMeshProUGUI>();
            imagemBandeira = GameObject.Find("BandeiraImagem")?.GetComponent<Image>();

            GameObject btn = GameObject.Find("BotaoCuba");
            if (btn != null) botaoDificuldadeExtra = btn;

            AtualizarTexto();
            VerificarDesbloqueioFuria();
        }
    }

    public void VerificarDesbloqueioFuria()
    {
        int jogoFinalizado = PlayerPrefs.GetInt("JogoFinalizado", 0);
        if (botaoDificuldadeExtra != null)
        {
            botaoDificuldadeExtra.SetActive(jogoFinalizado == 1);
        }
    }

    public void FinalizarJogo()
    {
        PlayerPrefs.SetInt("JogoFinalizado", 1);
        PlayerPrefs.Save();
        VerificarDesbloqueioFuria();
    }

    void AtualizarConfigAtual()
    {
        switch (dificuldadeSelecionada)
        {
            case Dificuldade.Facil: configAtual = dificuldadeFacil; break;
            case Dificuldade.Medio: configAtual = dificuldadeNormal; break;
            case Dificuldade.Dificil: configAtual = dificuldadeDificil; break;
            case Dificuldade.Furia: configAtual = dificuldadeFuria; break;
        }
    }

    public void SetDificuldade(int index)
    {
        dificuldadeSelecionada = (Dificuldade)index;
        AtualizarConfigAtual();
        PlayerPrefs.SetInt("DificuldadeSelecionada", index);
        AtualizarTexto();
    }

    private void AtualizarTexto()
    {
        if (infoDificuldadeTexto == null || imagemBandeira == null) return;

        imagemBandeira.gameObject.SetActive(true);

        switch (dificuldadeSelecionada)
        {
            case Dificuldade.Facil:
                infoDificuldadeTexto.text = "DIFFICULTY: USA";
                infoDificuldadeTexto.color = new Color(0.2f, 0.5f, 1f);
                imagemBandeira.sprite = flagUSA;
                break;
            case Dificuldade.Medio:
                infoDificuldadeTexto.text = "DIFFICULTY: RUSSIA";
                infoDificuldadeTexto.color = Color.white;
                imagemBandeira.sprite = flagRussia;
                break;
            case Dificuldade.Dificil:
                infoDificuldadeTexto.text = "DIFFICULTY: NORTH KOREA";
                infoDificuldadeTexto.color = new Color(1f, 0.5f, 0f);
                imagemBandeira.sprite = flagNKorea;
                break;
            case Dificuldade.Furia:
                infoDificuldadeTexto.text = "!!! CUBA MODE ACTIVATED !!!";
                infoDificuldadeTexto.color = Color.red;
                imagemBandeira.sprite = flagCUBA;
                break;
        }
    }

    public int CalcularVidaInimigo(int vidaBase)
    {
        float mult = (int)dificuldadeSelecionada + 1;
        if (dificuldadeSelecionada == Dificuldade.Furia) mult = 6f;
        return Mathf.RoundToInt(vidaBase * mult * (nivelAtual * 1.2f));
    }

    public float CalcularVelocidadeSpawn(float tempoBase)
    {
        float fator = (int)dificuldadeSelecionada * 0.5f;
        if (dificuldadeSelecionada == Dificuldade.Furia) fator = 3.0f;
        return tempoBase / (nivelAtual * 0.5f + fator);
    }
}
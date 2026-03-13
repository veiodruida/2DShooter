using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    private static GameSettings _instance;
    public static GameSettings instance
    {
        get
        {
            if (_instance == null) _instance = Object.FindFirstObjectByType<GameSettings>();
            return _instance;
        }
    }

    public enum Dificuldade { Facil, Medio, Dificil, Furia }

    [Header("Seleção Atual")]
    public Dificuldade dificuldadeSelecionada = Dificuldade.Facil;
    public int nivelAtual = 0;

    [Header("Assets de Dificuldade (ScriptableObjects)")]
    public DifficultyData dificuldadeFacil;
    public DifficultyData dificuldadeNormal;
    public DifficultyData dificuldadeDificil;
    public DifficultyData dificuldadeFuria;

    [Header("UI Feedback (Encontrados Automaticamente)")]
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

            // Carrega a dificuldade salva logo ao iniciar
            int salva = PlayerPrefs.GetInt("DificuldadeSelecionada", 0);
            dificuldadeSelecionada = (Dificuldade)salva;

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
            // Forçamos a busca sempre que entramos no menu
            infoDificuldadeTexto = GameObject.Find("DificuldadeTexto")?.GetComponent<TextMeshProUGUI>();
            imagemBandeira = GameObject.Find("BandeiraImagem")?.GetComponent<Image>();
            botaoDificuldadeExtra = GameObject.Find("BotaoCuba");

            // IMPORTANTE: Atualiza o visual logo ao entrar para mostrar o que estava salvo
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

    public void SetDificuldade(int index)
    {
        dificuldadeSelecionada = (Dificuldade)index;
        AtualizarConfigAtual(); // Garante que o multiplicador de vida mude!
        PlayerPrefs.SetInt("DificuldadeSelecionada", index);
        PlayerPrefs.Save();
        Debug.Log("GameSettings: Dificuldade lógica alterada para " + dificuldadeSelecionada);
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

        if (configAtual != null)
        {
            Debug.Log($"<color=magenta>Configuração Carregada: {configAtual.name} | Vel. Inimigo: {configAtual.velocidadeInimigoComum} | Vel. Boss: {configAtual.velocidadeBossBase}</color>");
        }
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

    // Mantendo os teus cálculos originais
    public int CalcularVidaInimigo(int vidaBase)
    {
        float mult = (int)dificuldadeSelecionada + 1;
        if (dificuldadeSelecionada == Dificuldade.Furia) mult = 6f;
        return Mathf.RoundToInt(vidaBase * mult * (nivelAtual * 1.2f));
    }
}
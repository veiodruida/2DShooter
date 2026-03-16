using UnityEngine;

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

    [Header("Configuração Ativa")]
    public Dificuldade dificuldadeSelecionada = Dificuldade.Facil;
    public DifficultyData configAtual;
    public int nivelAtual = 1;

    [Header("Arquivos de Dificuldade (.asset)")]
    public DifficultyData dificuldadeFacil;
    public DifficultyData dificuldadeNormal;
    public DifficultyData dificuldadeDificil;
    public DifficultyData dificuldadeFuria;

    [Header("Sprites das Bandeiras (Para o Bridge)")]
    public Sprite flagUSA;
    public Sprite flagRussia;
    public Sprite flagNKorea;
    public Sprite flagCUBA;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            CarregarDificuldadeSalva();
        }
        else { Destroy(gameObject); }
    }

    public void CarregarDificuldadeSalva()
    {
        int salva = PlayerPrefs.GetInt("DificuldadeSelecionada", 0);
        SetDificuldade(salva);
    }

    public void SetDificuldade(int index)
    {
        dificuldadeSelecionada = (Dificuldade)index;
        PlayerPrefs.SetInt("DificuldadeSelecionada", index);
        PlayerPrefs.Save();

        switch (dificuldadeSelecionada)
        {
            case Dificuldade.Facil: configAtual = dificuldadeFacil; break;
            case Dificuldade.Medio: configAtual = dificuldadeNormal; break;
            case Dificuldade.Dificil: configAtual = dificuldadeDificil; break;
            case Dificuldade.Furia: configAtual = dificuldadeFuria; break;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Classe principal que coordena o estado do jogo, pontuação, tempo e vitórias/derrotas.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [Tooltip("Referência ao objeto do jogador na cena")]
    public GameObject player = null;

    [Header("Pontuação e Cronómetro")]
    [SerializeField] private int gameManagerScore = 0;
    public int bonusPerfect = 1000;
    public float tempoRecordeAnterior = 0f;

    public static int score
    {
        get { return instance.gameManagerScore; }
        set { instance.gameManagerScore = value; }
    }

    public static object Dificuldade { get; internal set; }

    [Header("Configurações do Boss e Recordes")]
    public int highScore = 0;
    public float tempoDaFase = 0f;
    public int scoreBaseBoss = 5000;
    public int penalidadePorSegundo = 50;

    [Header("Progresso e Vitória")]
    public bool gameIsWinnable = true;
    public int enemiesToDefeat = 10;
    private int enemiesDefeated = 0;
    public bool printDebugOfWinnableStatus = true;
    public int gameVictoryPageIndex = 0;
    public GameObject victoryEffect;

    [Header("Configurações de Nível de Dificuldade")]
    public int nivelAtual = 1;
    // 0=Facil, 1=Medio, 2=Dificil, 3=Furia
    //public enum Dificuldade { Facil, Medio, Dificil, Furia }
    public GameSettings.Dificuldade dificuldadeSelecionada;

    private int numberOfEnemiesFoundAtStart;

    [HideInInspector]
    public bool gameIsOver = false;

    [Header("Configurações de Game Over")]
    public int gameOverPageIndex = 0;
    public GameObject gameOverEffect;

    private void Awake()
    {
        if (instance == null) instance = this;
        else DestroyImmediate(this);

        if ((player == null) && (Object.FindFirstObjectByType<Controller>() != null))
        {
            player = Object.FindFirstObjectByType<Controller>().gameObject;
        }
    }

    private void Start()
    {
        HandleStartUp();
    }

    private void Update()
    {
        if (!gameIsOver)
        {
            tempoDaFase += Time.deltaTime;
        }
    }

    void HandleStartUp()
    {
        ResetPlayerWeapon();

        if (PlayerPrefs.HasKey("highscore")) highScore = PlayerPrefs.GetInt("highscore");
        if (PlayerPrefs.HasKey("score")) score = PlayerPrefs.GetInt("score");
        if (PlayerPrefs.HasKey("melhor_tempo")) tempoRecordeAnterior = PlayerPrefs.GetFloat("melhor_tempo");

        UpdateUIElements();

        if (printDebugOfWinnableStatus) FigureOutHowManyEnemiesExist();
    }

    private void ResetPlayerWeapon()
    {
        if (player != null)
        {
            ShootingController shooter = player.GetComponent<ShootingController>();
            if (shooter != null) shooter.weaponLevel = 1;
        }
    }

    private void FigureOutHowManyEnemiesExist()
    {
        List<EnemySpawner> enemySpawners = Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None).ToList();
        List<Enemy> staticEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();

        int enemiesFromSpawners = 0;
        foreach (EnemySpawner spawner in enemySpawners)
        {
            if (!spawner.spawnInfinite) enemiesFromSpawners += spawner.maxSpawn;
        }
        numberOfEnemiesFoundAtStart = enemiesFromSpawners + staticEnemies.Count;
    }

    public void IncrementEnemiesDefeated()
    {
        enemiesDefeated++;
        if (enemiesDefeated >= enemiesToDefeat && gameIsWinnable && !GameObject.Find("MotherShip"))
        {
            LevelCleared();
        }
    }

    // --- MÉTODOS DE CÁLCULO DE DIFICULDADE (Adicionados para evitar erros) ---

    /// <summary>
    /// Retorna um multiplicador baseado na dificuldade atual.
    /// Útil para velocidade de inimigos ou dano.
    /// </summary>
    public float GetDificuldadeMultiplier()
    {
        switch (dificuldadeSelecionada)
        {
            case GameSettings.Dificuldade.Facil: return 0.7f;
            case GameSettings.Dificuldade.Medio: return 1.0f;
            case GameSettings.Dificuldade.Dificil: return 1.4f;
            case GameSettings.Dificuldade.Furia: return 2.0f; // Fúria dobra a agressividade
            default: return 1.0f;
        }
    }

    /// <summary>
    /// Calcula a vida que um inimigo deve ter baseado na dificuldade e no nível atual.
    /// </summary>
    public int CalcularVidaInimigo(int vidaBase)
    {
        float mult = GetDificuldadeMultiplier();
        // Aumenta 20% de vida por cada nível da fase
        float fatorNivel = 1f + (nivelAtual - 1) * 0.2f;
        return Mathf.RoundToInt(vidaBase * mult * fatorNivel);
    }

    // --- FIM DOS MÉTODOS DE CÁLCULO ---

    public int CalcularPontuacaoFinalBoss()
    {
        int pontosPerdidos = Mathf.FloorToInt(tempoDaFase * penalidadePorSegundo);
        return Mathf.Max(scoreBaseBoss - pontosPerdidos, 500);
    }

    public int CalcularPontuacaoFinalDaFase()
    {
        int pontuacaoFinal = gameManagerScore;
        int pontosDeTempo = Mathf.Max(0, scoreBaseBoss - Mathf.FloorToInt(tempoDaFase * penalidadePorSegundo));
        pontuacaoFinal += pontosDeTempo;

        Health pHealth = player.GetComponent<Health>();
        if (pHealth != null && pHealth.currentHealth >= pHealth.maximumHealth)
        {
            pontuacaoFinal += bonusPerfect;
        }
        return pontuacaoFinal;
    }

    public static void AddScore(int scoreAmount)
    {
        score += scoreAmount;
        if (score > instance.highScore) SaveHighScore();
        UpdateUIElements();
    }

    public static void ResetScore()
    {
        PlayerPrefs.SetInt("score", 0);
        score = 0;
        UpdateUIElements();
    }

    public static void SaveHighScore()
    {
        if (score > instance.highScore)
        {
            PlayerPrefs.SetInt("highscore", score);
            instance.highScore = score;
        }

        float melhorTempoSalvo = PlayerPrefs.GetFloat("melhor_tempo", 9999f);
        if (instance.tempoDaFase < melhorTempoSalvo && instance.tempoDaFase > 1f)
        {
            PlayerPrefs.SetFloat("melhor_tempo", instance.tempoDaFase);
            instance.tempoRecordeAnterior = instance.tempoDaFase;
        }
        PlayerPrefs.Save();
        UpdateUIElements();
    }

    public static void SalvarNoHistorico(int pontos, float tempo)
    {
        string historicoRaw = PlayerPrefs.GetString("historico_partidas", "");
        string novaEntrada = pontos + "|" + tempo.ToString("F2");

        historicoRaw = string.IsNullOrEmpty(historicoRaw) ? novaEntrada : novaEntrada + "," + historicoRaw;

        string[] entradas = historicoRaw.Split(',');
        if (entradas.Length > 10) historicoRaw = string.Join(",", entradas.Take(10));

        PlayerPrefs.SetString("historico_partidas", historicoRaw);
        PlayerPrefs.Save();
    }

    public static void UpdateUIElements()
    {
        if (UIManager.instance != null) UIManager.instance.UpdateUI();
    }

    public void LevelCleared()
    {
        if (gameIsOver) return;

        score = CalcularPontuacaoFinalDaFase();
        gameIsOver = true;
        PlayerPrefs.SetInt("score", score);

        if (score > highScore) SaveHighScore();

        if (nivelAtual == 3 && GameSettings.instance != null &&
            GameSettings.instance.dificuldadeSelecionada == GameSettings.Dificuldade.Dificil)
        {
            PlayerPrefs.SetInt("JogoFinalizado", 1);
            PlayerPrefs.Save();
        }

        if (UIManager.instance != null)
        {
            if (player != null) player.SetActive(false);
            UIManager.instance.allowPause = false;
            UIManager.instance.UpdateUI();
            UIManager.instance.GoToPage(gameVictoryPageIndex);
            if (victoryEffect != null) Instantiate(victoryEffect, transform.position, transform.rotation);
        }
    }

    public void GameOver()
    {
        gameIsOver = true;
        if (gameOverEffect != null) Instantiate(gameOverEffect, transform.position, transform.rotation);
        if (UIManager.instance != null)
        {
            UIManager.instance.allowPause = false;
            UIManager.instance.GoToPage(gameOverPageIndex);
        }
    }

    public void ReiniciarFase()
    {
        ResetScore();
        tempoDaFase = 0f;
        ResetPlayerWeapon();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnApplicationQuit()
    {
        SaveHighScore();
        ResetScore();
    }
}
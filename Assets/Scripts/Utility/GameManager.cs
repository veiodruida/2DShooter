using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public string nomePaginaVitoria = "VictoryPage"; // Mudado para string
    public GameObject victoryEffect;

    [Header("Configurações de Nível de Dificuldade")]
    public int nivelAtual = 1;
    public GameSettings.Dificuldade dificuldadeSelecionada;

    private int numberOfEnemiesFoundAtStart;

    [HideInInspector]
    public bool gameIsOver = false;

    [Header("Configurações de Game Over")]
    public string nomePaginaGameOver = "GameOverPage"; // Mudado para string
    public GameObject gameOverEffect;

    private void Awake()
    {
        if (instance == null) instance = this;
        else { DestroyImmediate(this); return; }

        if ((player == null) && (Object.FindFirstObjectByType<Controller>() != null))
        {
            player = Object.FindFirstObjectByType<Controller>().gameObject;
        }
    }

    // No GameManager.cs
    void Start()
    {
        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            var cfg = GameSettings.instance.configAtual;
            Debug.Log($"<color=cyan>=== VERIFICAÇÃO DE INÍCIO DE FASE ===</color>");
            Debug.Log($"<color=cyan>Dificuldade Selecionada: {GameSettings.instance.dificuldadeSelecionada}</color>");
            Debug.Log($"<color=cyan>Arquivo de Configuração: {cfg.name}</color>");
            Debug.Log($"<color=cyan>Velocidade Inimigo: {cfg.velocidadeInimigoComum} | Vidas Player: {cfg.vidasIniciais}</color>");
            Debug.Log($"<color=cyan>Velocidade Boss (Base): {cfg.velocidadeBossBase} | Boss Fúria 2: {cfg.velocidadeFuria2}</color>");
        }
        else
        {
            Debug.LogError("CUIDADO: Level 1 iniciado sem GameSettings ou sem arquivo DifficultyData!");
        }
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

    public float GetDificuldadeMultiplier()
    {
        switch (dificuldadeSelecionada)
        {
            case GameSettings.Dificuldade.Facil: return 0.7f;
            case GameSettings.Dificuldade.Medio: return 1.0f;
            case GameSettings.Dificuldade.Dificil: return 1.4f;
            case GameSettings.Dificuldade.Furia: return 2.0f;
            default: return 1.0f;
        }
    }

    public int CalcularVidaInimigo(int vidaBase)
    {
        float mult = GetDificuldadeMultiplier();
        float fatorNivel = 1f + (nivelAtual - 1) * 0.2f;
        return Mathf.RoundToInt(vidaBase * mult * fatorNivel);
    }

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

        Health pHealth = (player != null) ? player.GetComponent<Health>() : null;
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
            UIManager.instance.ConfigurarCursor(true);
            UIManager.instance.UpdateUI();
            UIManager.instance.GoToPageByName(nomePaginaVitoria);
            if (victoryEffect != null) Instantiate(victoryEffect, transform.position, transform.rotation);
        }
    }

    public void GameOver()
    {
        if (gameIsOver) return;
        gameIsOver = true;
        Time.timeScale = 0f; // TRAVA O JOGO

        if (gameOverEffect != null) Instantiate(gameOverEffect, transform.position, transform.rotation);

        if (UIManager.instance != null)
        {
            UIManager.instance.allowPause = false;
            UIManager.instance.ConfigurarCursor(true);
            UIManager.instance.GoToPageByName(nomePaginaGameOver);
        }
    }

    public void ReiniciarFase()
    {
        ResetScore();
        tempoDaFase = 0f;
        ResetPlayerWeapon();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnApplicationQuit()
    {
        SaveHighScore();
        ResetScore();
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "NovaDificuldade", menuName = "Configuracoes/Dificuldade")]
public class DifficultyData : ScriptableObject
{
    [Header("Inimigos e Spawner")]
    public float tempoSpawnItens = 4f;
    public float velocidadeInimigoComum = 5f;
    public float intervaloTiroInimigo = 4.0f; // Adiciona este campo!

    [Header("Player")]
    public int vidasIniciais = 3;
    public float taxaDeTiro = 0.15f;

    [Header("Boss (MotherShip) - Base")]
    public int navesParaAbrirEscudo = 10;
    public int vidaDoEscudoEstagio2 = 15;
    public float intervaloBombaBoss = 3f;
    public float velocidadeBombaBoss = 10f;

    [Header("Boss - Configurações de Fúria")]
    public float intervaloFuria1 = 10f;
    public float velocidadeFuria1 = 5f;
    public float intervaloFuria2 = 7f;
    public float velocidadeFuria2 = 16f;


}
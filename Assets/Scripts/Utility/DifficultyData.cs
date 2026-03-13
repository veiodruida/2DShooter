using UnityEngine;

[CreateAssetMenu(fileName = "NovaDificuldade", menuName = "Configuracoes/Dificuldade")]
public class DifficultyData : ScriptableObject
{
    [Header("Inimigos Comuns")]
    public float velocidadeInimigoComum = 5f;
    public float intervaloTiroInimigo = 4.0f;
    public float tempoSpawnInimigos = 2.0f; // Novo: Controle de spawn de inimigos

    [Header("Spawner de Itens")]
    public float tempoSpawnItens = 4f;

    [Header("Player")]
    public int vidasIniciais = 3;
    public float taxaDeTiro = 0.15f;
    public float velocidadePlayer = 10f; // Novo: Para caso queira mudar a agilidade do player

    [Header("Boss (MotherShip) - Base")]
    public float velocidadeBossBase = 0.1f; // Novo: Velocidade de movimento do Boss
    public int navesParaAbrirEscudo = 10;
    public int vidaDoEscudoEstagio2 = 15;
    public float intervaloBombaBoss = 3f;
    public float velocidadeBombaBoss = 10f;

    [Header("Boss - Modos de Fúria")]
    [Tooltip("Modo Fúria 1 é ativado em certa porcentagem de vida")]
    public float intervaloFuria1 = 10f;
    public float velocidadeFuria1 = 5f;

    [Tooltip("Modo Fúria 2 é o estágio final, mais agressivo")]
    public float intervaloFuria2 = 7f;
    public float velocidadeFuria2 = 16f;

    [Header("Multiplicadores Globais")]
    public float multiplicadorDanoRecebido = 1.0f; // Novo: Para facilitar o balanceamento
}
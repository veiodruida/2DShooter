using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Prefabs dos Itens")]
    public GameObject shieldPrefab;
    public GameObject healthPrefab;
    public GameObject TiroPrefab;
    public GameObject bombaPrefab;

    [Header("Configurações de Dificuldade (Fallback)")]
    [Range(1, 3)]
    public int nivelDificuldade = 1;

    private float tempoEntreSpawn;

    [Header("Área de Spawn")]
    public Vector2 limiteX = new Vector2(-21, 11);
    public Vector2 limiteY = new Vector2(-21, 11);

    void Start()
    {
        // 1. Primeiro define o tempo (do Arquivo ou do Inspector)
        AdjustDifficulty();

        // 2. Só depois inicia o Invoke com o tempo correto
        InvokeRepeating("SpawnRandomItem", 2f, tempoEntreSpawn);
    }

    void AdjustDifficulty()
    {
        // Tenta buscar do arquivo de configuração global
        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            tempoEntreSpawn = GameSettings.instance.configAtual.tempoSpawnItens;
            Debug.Log($"<color=cyan>[CONFIG ARQUIVO]</color> ItemSpawner: tempoEntreSpawn definido para {tempoEntreSpawn}s");
        }
        else
        {
            // Se não houver arquivo, mantém a tua lógica original do Inspector
            if (nivelDificuldade == 1) tempoEntreSpawn = 2f;
            else if (nivelDificuldade == 2) tempoEntreSpawn = 4f;
            else tempoEntreSpawn = 30f;

            Debug.Log($"<color=yellow>[INSPECTOR]</color> ItemSpawner: Usando fallback manual ({tempoEntreSpawn}s)");
        }
    }

    void SpawnRandomItem()
    {
        // Se o jogo acabou ou o Boss foi derrotado, paramos de dar spawn
        if (GameManager.instance != null && GameManager.instance.gameIsOver)
        {
            CancelInvoke("SpawnRandomItem");
            return;
        }

        // 1. Verifica a chance de spawn baseada na dificuldade
        float chance = (GameSettings.instance != null && GameSettings.instance.configAtual != null)
            ? GameSettings.instance.configAtual.chanceSpawnItens
            : 0.5f;

        if (Random.value > chance)
        {
            return; // Não spawna neste tick
        }

        float posX = Random.Range(limiteX.x, limiteX.y);
        float posY = Random.Range(limiteY.x, limiteY.y);
        Vector3 posicaoAleatoria = new Vector3(posX, posY, 0);

        GameObject prefabParaCriar = null;
        float sorteio = Random.value;

        // Mantida a tua lógica de 25% de chance para cada um
        if (sorteio < 0.25f)
        {
            prefabParaCriar = healthPrefab;
           // prefabParaCriar = bombaPrefab;
        }
        else if (sorteio < 0.50f)
        {
            prefabParaCriar = shieldPrefab;
            //prefabParaCriar = bombaPrefab;
        }
        else if (sorteio < 0.75f)
        {
            prefabParaCriar = TiroPrefab;
            //prefabParaCriar = bombaPrefab;
        }
        else
        {
            prefabParaCriar = bombaPrefab;
        }

        if (prefabParaCriar != null)
        {
            Instantiate(prefabParaCriar, posicaoAleatoria, Quaternion.identity);
        }
        else
        {
            Debug.LogError("ERRO: Um dos prefabs no ItemSpawner não foi arrastado no Inspector!");
        }
    }
}
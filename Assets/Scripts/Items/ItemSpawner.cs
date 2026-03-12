using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Prefabs dos Itens")]
    public GameObject shieldPrefab;
    public GameObject healthPrefab;
    public GameObject TiroPrefab;
    public GameObject bombaPrefab; // NOVO: Arraste o Prefab da Bomba aqui

    [Header("Configurações de Dificuldade")]
    [Range(1, 3)]
    public int nivelDificuldade = 1;

    private float tempoEntreSpawn;

    [Header("Área de Spawn")]
    public Vector2 limiteX = new Vector2(-8, 8);
    public Vector2 limiteY = new Vector2(-4, 4);

    void Start()
    {
        AdjustDifficulty();
        // Começa a spawnar após 2 segundos, repetindo conforme a dificuldade
        InvokeRepeating("SpawnRandomItem", 2f, tempoEntreSpawn);
    }

    void AdjustDifficulty()
    {
        // Mantive os teus tempos originais
        if (nivelDificuldade == 1) tempoEntreSpawn = 2f;
        else if (nivelDificuldade == 2) tempoEntreSpawn = 4f;
        else tempoEntreSpawn = 30f;
    }

    void SpawnRandomItem()
    {
        // 1. Escolhe a posição aleatória
        float posX = Random.Range(limiteX.x, limiteX.y);
        float posY = Random.Range(limiteY.x, limiteY.y);
        Vector3 posicaoAleatoria = new Vector3(posX, posY, 0);

        // 2. Lógica de sorteio para 4 itens (25% de chance para cada)
        GameObject prefabParaCriar = null;
        float sorteio = Random.value;

        if (sorteio < 0.25f)
        {
            prefabParaCriar = healthPrefab;
           // prefabParaCriar = bombaPrefab; // Agora a bomba também pode aparecer!

        }
        else if (sorteio < 0.50f)
        {
            prefabParaCriar = shieldPrefab;
            //prefabParaCriar = bombaPrefab; // Agora a bomba também pode aparecer!

        }
        else if (sorteio < 0.75f)
        {
            prefabParaCriar = TiroPrefab;
            //prefabParaCriar = bombaPrefab; // Agora a bomba também pode aparecer!

        }
        else
        {
            prefabParaCriar = bombaPrefab; // Agora a bomba também pode aparecer!
        }

        // 3. Cria o item
        if (prefabParaCriar != null)
        {
            Instantiate(prefabParaCriar, posicaoAleatoria, Quaternion.identity);
            // Debug.Log("<color=yellow>Item criado:</color> " + prefabParaCriar.name);
        }
        else
        {
            Debug.LogError("ERRO: Um dos prefabs no ItemSpawner não foi arrastado no Inspector!");
        }
    }
}
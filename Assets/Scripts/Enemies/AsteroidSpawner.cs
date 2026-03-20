using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Configurações do Prefab")]
    public GameObject asteroidPrefab;

    [Header("Ritmo de Jogo")]
    [Tooltip("Tempo mínimo entre um asteroide e outro")]
    public float intervaloMin = 1.0f;
    [Tooltip("Tempo máximo entre um asteroide e outro")]
    public float intervaloMax = 3.5f;

    [Header("Limites de Spawn (Área de Surgimento)")]
    [Tooltip("Largura total da tela onde podem surgir")]
    public float larguraSpawn = 20f;
    [Tooltip("Altura acima da câmera onde nascem")]
    public float alturaSpawn = 12f;

    [Header("Dificuldade (Opcional)")]
    public bool aumentarFrequenciaNoModoFuria = true;

    private float timer;
    private float tempoParaProximoSpawn;

    void Start()
    {
        CalcularProximoTempo();
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.gameIsOver) return;

        timer += Time.deltaTime;

        if (timer >= tempoParaProximoSpawn)
        {
            SpawnAsteroid();
            timer = 0;
            CalcularProximoTempo();
        }
    }

    void SpawnAsteroid()
    {
        if (asteroidPrefab == null) return;

        // 1. Calcula posição X aleatória em toda a largura definida
        float posX = Random.Range(-larguraSpawn / 2f, larguraSpawn / 2f);

        // 2. Define a posição de nascimento (fora da visão da câmera)
        Vector3 posicaoSpawn = new Vector3(posX, alturaSpawn, 0);

        // 3. Cria o asteroide
        GameObject novoAsteroid = Instantiate(asteroidPrefab, posicaoSpawn, Quaternion.identity);

        // Mantém a hierarquia limpa
        novoAsteroid.transform.parent = this.transform;
    }

    void CalcularProximoTempo()
    {
        tempoParaProximoSpawn = Random.Range(intervaloMin, intervaloMax);

        // Lógica do Modo Fúria 2 (conforme sua configuração)
        if (aumentarFrequenciaNoModoFuria && GameManager.instance != null)
        {
            if (GameSettings.instance != null && GameSettings.instance.dificuldadeSelecionada == GameSettings.Dificuldade.Furia)
            {
                tempoParaProximoSpawn *= 0.4f;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // Desenha a linha de spawn no topo
        Vector3 centroTopo = new Vector3(0, alturaSpawn, 0);
        Gizmos.DrawWireCube(centroTopo, new Vector3(larguraSpawn, 0.5f, 1));

        // Desenha uma caixa sugerindo a área de jogo para ajudar no ajuste
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(larguraSpawn, alturaSpawn * 2, 1));
    }
}
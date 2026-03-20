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
    [Tooltip("Largura total da área de spawn")]
    public float larguraSpawn = 25f;
    [Tooltip("Altura total da área de spawn")]
    public float alturaSpawn = 15f;

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

        Vector3 posicaoSpawn = Vector3.zero;
        Vector3 direcao = Vector3.zero;

        // Escolhe um lado aleatório (0: Topo, 1: Baixo, 2: Esquerda, 3: Direita)
        int lado = Random.Range(0, 4);

        switch (lado)
        {
            case 0: // TOPO
                posicaoSpawn = new Vector3(Random.Range(-larguraSpawn / 2f, larguraSpawn / 2f), alturaSpawn / 2f, 0);
                direcao = new Vector3(Random.Range(-0.5f, 0.5f), -1, 0).normalized;
                break;
            case 1: // BAIXO
                posicaoSpawn = new Vector3(Random.Range(-larguraSpawn / 2f, larguraSpawn / 2f), -alturaSpawn / 2f, 0);
                direcao = new Vector3(Random.Range(-0.5f, 0.5f), 1, 0).normalized;
                break;
            case 2: // ESQUERDA
                posicaoSpawn = new Vector3(-larguraSpawn / 2f, Random.Range(-alturaSpawn / 2f, alturaSpawn / 2f), 0);
                direcao = new Vector3(1, Random.Range(-0.5f, 0.5f), 0).normalized;
                break;
            case 3: // DIREITA
                posicaoSpawn = new Vector3(larguraSpawn / 2f, Random.Range(-alturaSpawn / 2f, alturaSpawn / 2f), 0);
                direcao = new Vector3(-1, Random.Range(-0.5f, 0.5f), 0).normalized;
                break;
        }

        // Cria o asteroide
        GameObject novoAsteroid = Instantiate(asteroidPrefab, posicaoSpawn, Quaternion.identity);
        
        // Define a direção antes do Start do asteroide rodar
        Asteroid astScript = novoAsteroid.GetComponent<Asteroid>();
        if (astScript != null)
        {
            astScript.direcaoMovimento = direcao;
        }

        novoAsteroid.transform.parent = this.transform;
    }

    void CalcularProximoTempo()
    {
        tempoParaProximoSpawn = Random.Range(intervaloMin, intervaloMax);

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
        // Desenha o retângulo da área de spawn
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(larguraSpawn, alturaSpawn, 1));
    }
}
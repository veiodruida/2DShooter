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

    [Header("Área de Spawn")]
    public Vector2 limiteX = new Vector2(-21, 11);
    public Vector2 limiteY = new Vector2(-21, 11);

    [Header("Tamanho dos Asteroides")]
    [Tooltip("Escala mínima dos asteroides (Geração 0)")]
    public float tamanhoMin = 1.5f;
    [Tooltip("Escala máxima dos asteroides (Geração 0)")]
    public float tamanhoMax = 2.5f;

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

        // Pega os limites do jogo via CameraController, mas mantém limiteX e limiteY do Inspector intactos para o Spawn Externo!
        Vector2 alvoCameraX = limiteX;
        Vector2 alvoCameraY = limiteY;

        CameraController cam = FindFirstObjectByType<CameraController>();
        if (cam != null)
        {
            alvoCameraX = new Vector2(cam.minBounds.x, cam.maxBounds.x);
            alvoCameraY = new Vector2(cam.minBounds.y, cam.maxBounds.y);
        }

        Vector3 posicaoSpawn = Vector3.zero;
        Vector3 direcao = Vector3.zero;

        int lado = Random.Range(0, 4);

        // Nasce estritamente na borda Externa definida no Inspector (limiteX e limiteY originais do Spawner)
        switch (lado)
        {
            case 0: // TOPO
                posicaoSpawn = new Vector3(Random.Range(limiteX.x, limiteX.y), limiteY.y, 0);
                break;
            case 1: // BAIXO
                posicaoSpawn = new Vector3(Random.Range(limiteX.x, limiteX.y), limiteY.x, 0);
                break;
            case 2: // ESQUERDA
                posicaoSpawn = new Vector3(limiteX.x, Random.Range(limiteY.x, limiteY.y), 0);
                break;
            case 3: // DIREITA
                posicaoSpawn = new Vector3(limiteX.y, Random.Range(limiteY.x, limiteY.y), 0);
                break;
        }

        // MAGIA FÚRIA: Traçar reta implacável mirando exatamente na área de jogo (os limites da câmara)
        Vector3 alvoAleatorioNaEcra = new Vector3(Random.Range(alvoCameraX.x, alvoCameraX.y), Random.Range(alvoCameraY.x, alvoCameraY.y), 0);
        direcao = (alvoAleatorioNaEcra - posicaoSpawn).normalized;

        GameObject novoAsteroid = Instantiate(asteroidPrefab, posicaoSpawn, Quaternion.identity);

        Asteroid astScript = novoAsteroid.GetComponent<Asteroid>();
        if (astScript != null)
        {
            astScript.direcaoMovimento = direcao;

            float tam = Random.Range(tamanhoMin, tamanhoMax);
            novoAsteroid.transform.localScale = new Vector3(tam, tam, 1);
            astScript.tamanhoDefinido = true;
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
        float largura = limiteX.y - limiteX.x;
        float altura = limiteY.y - limiteY.x;
        Vector3 centro = new Vector3(limiteX.x + largura / 2f, limiteY.x + altura / 2f, 0);
        Gizmos.DrawWireCube(centro, new Vector3(largura, altura, 1));
    }
}
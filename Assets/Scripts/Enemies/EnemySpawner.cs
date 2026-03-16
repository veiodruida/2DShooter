using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which spawns enemies in an area around it.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("GameObject References")]
    [Tooltip("The enemy prefab to use when spawning enemies")]
    public GameObject enemyPrefab = null;
    [Tooltip("The target of the spwaned enemies")]
    public Transform target = null;

    [Header("Spawn Position")]
    [Tooltip("The distance within which enemies can spawn in the X direction")]
    [Min(0)]
    public float spawnRangeX = 10.0f;
    [Tooltip("The distance within which enemies can spawn in the Y direction")]
    [Min(0)]
    public float spawnRangeY = 10.0f;

    [Header("Spawn Variables")]
    [Tooltip("The maximum number of enemies that can be spawned from this spawner")]
    public int maxSpawn = 20;
    [Tooltip("Ignores the max spawn limit if true")]
    public bool spawnInfinite = true;

    // The number of enemies that have been spawned
    private int currentlySpawned = 0;

    [Tooltip("The time delay between spawning enemies")]
    public float spawnDelay = 2.5f;

    // The most recent spawn time
    private float lastSpawnTime = Mathf.NegativeInfinity;

    [Tooltip("The object to make projectiles child objects of.")]
    public Transform projectileHolder = null;

    // --- NOVO: Lógica de Dificuldade ---
    private void Start()
    {
        ConfigurarDificuldade();
    }

    void ConfigurarDificuldade()
    {
        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            float delayAntigo = spawnDelay;
            spawnDelay = GameSettings.instance.configAtual.tempoSpawnInimigos;

            Debug.Log($"<color=cyan>[CONFIG ARQUIVO]</color> Spawner {name}: Intervalo alterado de {delayAntigo}s para {spawnDelay}s");
        }
        else
        {
            Debug.Log($"<color=yellow>[INSPECTOR]</color> Spawner {name}: Usando delay manual de {spawnDelay}s");
        }
    }
    // ----------------------------------

    private void Update()
    {
        CheckSpawnTimer();
    }

    private void CheckSpawnTimer()
    {
        if (Time.timeSinceLevelLoad > lastSpawnTime + spawnDelay && (currentlySpawned < maxSpawn || spawnInfinite))
        {
            Vector3 spawnLocation = GetSpawnLocation();
            SpawnEnemy(spawnLocation);
        }
    }

    private void SpawnEnemy(Vector3 spawnLocation)
    {
        if (enemyPrefab != null)
        {
            GameObject enemyGameObject = Instantiate(enemyPrefab, spawnLocation, enemyPrefab.transform.rotation, null);
            Enemy enemy = enemyGameObject.GetComponent<Enemy>();
            ShootingController[] shootingControllers = enemyGameObject.GetComponentsInChildren<ShootingController>();

            if (enemy != null)
            {
                enemy.followTarget = target;
            }
            foreach (ShootingController gun in shootingControllers)
            {
                gun.projectileHolder = projectileHolder;
            }

            currentlySpawned++;
            lastSpawnTime = Time.timeSinceLevelLoad;
        }
    }

    protected virtual Vector3 GetSpawnLocation()
    {
        float x = Random.Range(0 - spawnRangeX, spawnRangeX);
        float y = Random.Range(0 - spawnRangeY, spawnRangeY);
        return new Vector3(transform.position.x + x, transform.position.y + y, 0);
    }
}
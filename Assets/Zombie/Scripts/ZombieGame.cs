using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class ZombieGame : MonoBehaviour
{
    public int currentLevel = 1;
    public int score = 0;
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    public Transform fenceStart;
    public Transform fenceEnd;
    public GameObject zombiePrefab;
    public int baseZombiesPerLevel = 1;
    public float spawnDurationIncreasePerLevel = 2f;

    [Header("Zombie Randomization")]
    public float minSpeed = .65f;
    public float maxSpeed = 1f;
    public float speedVariation = 0.1f;
    public float timeToSpeedMin = 1f;
    public float timeToSpeedMax = 3f;

    bool gameRunning = false;
    int zombiesToSpawn;
    int zombiesRemaining;
    float currentSpawnDuration;

    private void Awake()
    {
        Zombie.OnZombieDied += HandleZombieDeath;
        Zombie.OnZombieAttacked += GameOver;
        StartGame();
    }

    private void OnDestroy() => Zombie.OnZombieDied -= HandleZombieDeath;

    void StartGame()
    {
        gameRunning = true;
        currentSpawnDuration = spawnDurationIncreasePerLevel;
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        zombiesToSpawn = baseZombiesPerLevel + (currentLevel - 1) * 2; // Increase zombies per level
        zombiesRemaining = zombiesToSpawn;

        for (int i = 0; i < zombiesToSpawn; i++)
        {
            SpawnZombie();
            yield return new WaitForSeconds(currentSpawnDuration / zombiesToSpawn); // Adjust spawn interval
        }
    }

    void SpawnZombie()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnPoint1.position.x, spawnPoint2.position.x),
            spawnPoint1.position.y,
            Random.Range(spawnPoint1.position.z, spawnPoint2.position.z)
        );

        GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
        Zombie zombieScript = zombie.GetComponent<Zombie>();

        if (zombieScript)
        {
            Vector3 destination = GetRandomFencePoint();
        
            // Calculate speed based on the level
            float levelProgression = Mathf.Clamp01(currentLevel / 10f); // Normalized between 0 and 1
            float baseSpeed = Mathf.Lerp(minSpeed, maxSpeed, levelProgression);    // Scales from min (0.4) to max (1.0)
            float randomizedSpeed = Mathf.Clamp(baseSpeed + Random.Range(-speedVariation, speedVariation), minSpeed, 1.0f);

            // Randomize time to speed
            float randomizedTimeToSpeed = Random.Range(timeToSpeedMin, timeToSpeedMax);

            zombieScript.Initialize(randomizedSpeed, randomizedTimeToSpeed, destination);
        }
    }


    Vector3 GetRandomFencePoint()
    {
        float t = Random.Range(0f, 1f); // Random value between 0 and 1
        return Vector3.Lerp(fenceStart.position, fenceEnd.position, t);
    }

    void HandleZombieDeath(Zombie zombie)
    {
        score += 10;
        zombiesRemaining--;

        if (zombiesRemaining <= 0)
        {
            currentLevel++;
            currentSpawnDuration += spawnDurationIncreasePerLevel; // Increase spawn duration for longer waves
            StartCoroutine(SpawnWave());
        }
    }

    public void ZombieReachedFence()
    {
        if (!gameRunning) return;

        gameRunning = false;
        StopAllCoroutines();
        DestroyAllZombies();
        GameOver();
    }

    void DestroyAllZombies()
    {
        Zombie[] zombies = FindObjectsOfType<Zombie>();
        foreach (Zombie z in zombies)
            Destroy(z.gameObject);
    }

 
    void GameOver()
    {
        Debug.Log("Game Over!");
        DestroyAllZombies();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (spawnPoint1 && spawnPoint2)
        {
            Gizmos.DrawSphere(spawnPoint1.position, 0.2f);
            Gizmos.DrawSphere(spawnPoint2.position, 0.2f);
            Gizmos.DrawLine(spawnPoint1.position, spawnPoint2.position);
        }

        Gizmos.color = Color.red;
        if (fenceStart && fenceEnd)
        {
            Gizmos.DrawSphere(fenceStart.position, 0.2f);
            Gizmos.DrawSphere(fenceEnd.position, 0.2f);
            Gizmos.DrawLine(fenceStart.position, fenceEnd.position);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hex.ObjectPooling;
using System;
using DG.Tweening;
using TMPro;

[Serializable]
public class EnemyDictionary
{
    public int enemyIndex;
    public int NumToSpawn;
}
[Serializable]
public class Wave
{
    public List<EnemyDictionary> EnemiesInWave = new List<EnemyDictionary>();
    public int waveDuration = 3;
}
public class Spawner : MonoBehaviour
{
    //public static Spawner spawner;
    [Range(1f, 15f)]
    public float range = 5f;

    public bool canSpawn = true;
    private enum spawnState { Idle, Active, Conclusion }
    [SerializeField] private spawnState battleState;
    private enum spawnerType { Encounter, Arena }
    [SerializeField] private spawnerType spawnerMode;

    //[SerializeField]private TextMeshPro waveText, waveTimeText;
    [SerializeField]private int waveDuration;
    private float waveTimer, waveTimerRate = 1f, emptyWaveRate = 1.6f, defaultWaveRate = 1f;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    [SerializeField] private int waveValue, currentWave;
    [Header("Object Pool")]
    [SerializeField] private GameObject slimePrefab;
    public static ObjectPool<PoolObject> slimePool;
    [SerializeField] private GameObject shrumPrefab;
    public static ObjectPool<PoolObject> shrumPool;
    [SerializeField] private GameObject boarPrefab;
    public static ObjectPool<PoolObject> boarPool;
    [SerializeField] private GameObject bombPrefab;
    public static ObjectPool<PoolObject> bombPool;
    [SerializeField] private GameObject mantisPrefab;
    public static ObjectPool<PoolObject> mantisPool;
    [SerializeField] private GameObject knightPrefab;
    public static ObjectPool<PoolObject> knightPool;
    //Sophie Spawner
    [SerializeField] private float waveStartDelay = 1;
    [SerializeField] private List<Wave> waves = new List<Wave>();
    public List<Vector3> possibleSpawnPoints = new List<Vector3>();

    //private List<Enemy> LivingEnemies = new List<Enemy>();
    private List<Vector3> pointsClaimedPerWave = new List<Vector3>();//this will store the points that each enemy will use for spawning so that no one doubles up.
    private void Awake()
    {
        //spawner = this;
        //create pool instance with prefab reference and push&pull actions
        slimePool = new ObjectPool<PoolObject>(slimePrefab, CallOnPull, CallOnPush);
        shrumPool = new ObjectPool<PoolObject>(shrumPrefab, CallOnPull, CallOnPush);
        boarPool = new ObjectPool<PoolObject>(boarPrefab, CallOnPull, CallOnPush);
        bombPool = new ObjectPool<PoolObject>(bombPrefab, CallOnPull, CallOnPush);
        mantisPool = new ObjectPool<PoolObject>(mantisPrefab, CallOnPull, CallOnPush);
        knightPool = new ObjectPool<PoolObject>(knightPrefab, CallOnPull, CallOnPush);
    }
    private void Start()
    {
        //GenerateWave();
        List<Transform> possibleTransforms = new List<Transform>(GetComponentsInChildren<Transform>());
        foreach (Transform item in possibleTransforms)
        {
            possibleSpawnPoints.Add(item.position);
        }
        //possibleSpawnPoints = new List<Transform>(GetComponentsInChildren<Transform>());
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (battleState == spawnState.Idle)
                ActivateSpawner();
        }
    }

    public void ActivateSpawner()
    {
        canSpawn = true;
        waveTimer = waveStartDelay;
        //StartCoroutine(StartWaves());
        battleState = spawnState.Active;
    }
    public void RestartSpawner()
    {
        canSpawn = true;
        waveTimer = waveStartDelay;
        currentWave = 0;
        battleState = spawnState.Active;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (battleState == spawnState.Conclusion)
            {
                battleState = spawnState.Idle;
            }
        } 
    }
    private void Update()
    {
        if (battleState==spawnState.Active)
        {
            StartWaves();
            //int seconds = Mathf.FloorToInt(waveTimer % 60);
            //waveText.SetText(seconds.ToString());
        }
    }
    private void SummonEnemy(int enemyIndex,Vector3 pos)
    {
       
        GameObject enemy = new GameObject();
        switch (enemyIndex)
        {
            case 0:
                enemy = slimePool.PullGameObject(pos);
                break;
            case 1:
                enemy = shrumPool.PullGameObject(pos);
                break;
            case 2:
                enemy = boarPool.PullGameObject(pos);
                break;
            case 3:
                enemy = bombPool.PullGameObject(pos);
                break;
            case 4:
                enemy = mantisPool.PullGameObject(pos);
                break;
            case 5:
                enemy = knightPool.PullGameObject(pos);
                break;
            default:
                break;
        }
        spawnedEnemies.Add(enemy);
        Enemy spawn = enemy.GetComponent<Enemy>();
        spawn.parent = this;
        
    }
    void StartWaves()
    {
        if (waveTimer <= 0)
        {
            if (currentWave < waves.Count)
            {
                pointsClaimedPerWave.Clear();//clear our list of points grabbed
                for (int i = 0; i < waves[currentWave].EnemiesInWave.Count; i++)//Go through our list of enemies and spawn each one
                {
                    //grab the prefab of that enemy based on the enum type
                    int spawnIndex = (int)waves[currentWave].EnemiesInWave[i].enemyIndex;//get enum index
                    for (int a = 0; a < waves[currentWave].EnemiesInWave[i].NumToSpawn; a++)//spawn as many of that enemy as we need
                    {
                        Vector3 t = GetRandomPosition();
                        SummonEnemy(spawnIndex, t);
                        GameEngine.GlobalPrefab(5, t);//drop shadow
                    }
                }
                currentWave++;
                waveTimer = waves[currentWave].waveDuration;
            }
        }
        else
            waveTimer -= Time.deltaTime*waveTimerRate;
        if (currentWave >= waves.Count)
        {
            EndWaves();
        }
        
    }

    Vector3 GetRandomPosition()//returns a random transform from the list given
    {
        int rand = UnityEngine.Random.Range(0, possibleSpawnPoints.Count);

        if (pointsClaimedPerWave.Contains(possibleSpawnPoints[rand]))//if the point we chose matches one that was already chosen, 
            GetRandomPosition();//recursion! Choose another point
        else//if it's good, we add it to the list and continue
        {
            if (possibleSpawnPoints[rand] == Vector3.zero)
            {
                GetRandomPosition();
            }
            else
                pointsClaimedPerWave.Add(possibleSpawnPoints[rand]); 
        }
        return possibleSpawnPoints[rand];

        //return possibleSpawnPoints[i];
    }
    public void RemoveEnemyFromList(GameObject go)
    {
        if (spawnedEnemies.Contains(go))
        {
            spawnedEnemies.Remove(go);
        }
        if (battleState == spawnState.Active)
        {
            if (spawnedEnemies.Count <= 0)
            {
                waveTimerRate = emptyWaveRate;
                if (!canSpawn)
                {
                    battleState = spawnState.Conclusion;
                }
            }
            else
                waveTimerRate = defaultWaveRate;
        }
    }
    public void EndWaves()
    {
        GameObject[] enemies;
        foreach (GameObject go in spawnedEnemies)
        {
            Enemy enemy = go.GetComponent<Enemy>();
            enemy.DeSpawn();
        }
        canSpawn = false;
        waveTimer = waveStartDelay;
        currentWave = 0;
        //waveText.SetText(currentWave.ToString());
        battleState = spawnState.Conclusion;
    }
    //[System.Serializable]
    //public class Enemy
    //{
    //    public GameObject enemyPrefab;
    //    public int cost;
    //}
    private void CallOnPull(PoolObject poolObject)
    {

    }
    private void CallOnPush(PoolObject poolObject)
    {

    }
}

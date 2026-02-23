using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hex.ObjectPooling;
using System;

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

    [SerializeField]private int waveDuration;
    private float waveTimer;
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
    //Sophie Spawner
    [SerializeField] private float waveStartDelay = 1;
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private List<Transform> possibleSpawnPoints = new List<Transform>();

    //private List<Enemy> LivingEnemies = new List<Enemy>();
    private List<Transform> pointsClaimedPerWave = new List<Transform>();//this will store the points that each enemy will use for spawning so that no one doubles up.
    private void Awake()
    {
        //spawner = this;
        //create pool instance with prefab reference and push&pull actions
        slimePool = new ObjectPool<PoolObject>(slimePrefab, CallOnPull, CallOnPush);
        shrumPool = new ObjectPool<PoolObject>(shrumPrefab, CallOnPull, CallOnPush);
        boarPool = new ObjectPool<PoolObject>(boarPrefab, CallOnPull, CallOnPush);
        bombPool = new ObjectPool<PoolObject>(bombPrefab, CallOnPull, CallOnPush);
    }
    private void Start()
    {
        //GenerateWave();
        possibleSpawnPoints = new List<Transform>(GetComponentsInChildren<Transform>());
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (battleState == spawnState.Idle)
            {
                canSpawn = true;
                waveTimer = waveStartDelay;
                StartCoroutine(StartWaves());
                battleState = spawnState.Active;
            }
        }
        
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
            if (waveTimer > 0)
            {
                waveTimer -= Time.deltaTime;
            }
            else
            {
                //int random = UnityEngine.Random.Range(0, 4);
                //Vector3 position = GetRandomTransform().position;
                //SummonEnemy(random,position);
                waveTimer = waveDuration;
            }
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
            default:
                break;
        }
        spawnedEnemies.Add(enemy);
        Enemy spawn = enemy.GetComponent<Enemy>();
        spawn.parent = this;
        
    }
    IEnumerator StartWaves()
    {
        yield return new WaitForSeconds(waveStartDelay);
        //bState = battleState.active;
        for (int b = 0; b < waves.Count; b++)
        {
            pointsClaimedPerWave.Clear();//clear our list of points grabbed
            for (int i = 0; i < waves[b].EnemiesInWave.Count; i++)//Go through our list of enemies and spawn each one
            {
                //grab the prefab of that enemy based on the enum type
                int spawnIndex = (int)waves[b].EnemiesInWave[i].enemyIndex;//get enum index
                for (int a = 0; a < waves[b].EnemiesInWave[i].NumToSpawn; a++)//spawn as many of that enemy as we need
                {
                    Transform t = GetRandomTransform();
                    SummonEnemy(spawnIndex, t.position);
                    GameEngine.GlobalPrefab(5, t.gameObject);//drop shadow
                }
            }
            yield return new WaitForSeconds(waves[b].waveDuration);
        }
        canSpawn = false;
    }

    Transform GetRandomTransform()//returns a random transform from the list given
    {
        int rand = UnityEngine.Random.Range(0, possibleSpawnPoints.Count);

        if (pointsClaimedPerWave.Contains(possibleSpawnPoints[rand]))//if the point we chose matches one that was already chosen, 
            GetRandomTransform();//recursion! Choose another point
        else
            pointsClaimedPerWave.Add(possibleSpawnPoints[rand]);//if it's good, we add it to the list and continue

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
            if (!canSpawn && spawnedEnemies.Count <= 0)
            {
                battleState = spawnState.Conclusion;
            }
        }
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

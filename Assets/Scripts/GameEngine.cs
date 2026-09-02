using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;


public class GameEngine : MonoBehaviour
{
    public class OnTickEventArgs : EventArgs
    {
        public int tick;
    }
    public static float hitStop;

    public static GameEngine gameEngine;
    [Header("Timer")]
    [SerializeField] private float remainingTime = 15f;
    private float elapsedTime = 0f;
    private bool countDown = false;
    [SerializeField] TextMeshPro timerText;
    private Color timerColor;
    [Header("GameObjects")]
    public Player mainCharacter;

    public GameObject[] globalPrefabs;
    public GameObject mobileHUD;

    public Transform DamagePopup;

    public Spawner spawner;
    public TilemapManager tilemapManager;
    [SerializeField]private List<int> levels = new List<int>();
    private int levelIndex = 0;
    private int tick;
    private float tickTimer;
    private const float tickTimerMax = 1f;
    public static event EventHandler<OnTickEventArgs> OnTick;
    private CinemachineShake cineShake;
    // Use this for initialization
    void Awake ()
    {
        gameEngine = this;
    }
    private void Start()
    {
        mobileHUD.SetActive(Application.isMobilePlatform);
        cineShake = Camera.main.GetComponent<CinemachineShake>();
        timerColor = timerText.color;
        timerText.gameObject.SetActive(false);
        StartGame();
    }

    private void StartGame()
    {
        SetRemainingTime(60);

        tilemapManager.LoadLevel(levels[levelIndex]);
        levelIndex++;
        if (levelIndex>=levels.Count)
        {
            levelIndex = 0;
        }

        spawner.possibleSpawnPoints = new List<Vector3>(tilemapManager.monsterPositions);
        spawner.ActivateSpawner();
    }

    public void ShakeCamera(float _pow, float _time)
    {
        cineShake.ShakeCamera(_pow, _time);
    }
    public static void SetHitPause(float _pow)
    {
        if (_pow > hitStop)
        {
            hitStop = _pow;
        }
    }
    // Update is called once per frame
    private void Update()
    {
        if (hitStop<=0)
        {
            if (countDown) CountdownTimer();
        }
    }

    private void ElapsedTimer()
    {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        SetTimerText(minutes, seconds);
    }
    private void CountdownTimer()
    {
        if (remainingTime > 0)
            remainingTime -= Time.deltaTime;
        else if (remainingTime < 0)
        {
            remainingTime = 0;
            timerText.color = Color.red;
            Debug.Log("TIME OVER");
        }
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        SetTimerText(minutes, seconds);
    }
    public void SetRemainingTime(float seconds)
    {
        timerText.gameObject.SetActive(true);
        timerText.color = timerColor;
        remainingTime = seconds;
        countDown = true;
    }
    public void AddTime(float seconds)
    {
        remainingTime += seconds;
    }
    private void SetTimerText(int minutes, int seconds)
    {
        timerText.SetText(string.Format("{0:00}:{1:00}", minutes, seconds));
    }

    void FixedUpdate ()
    {
        if (hitStop>0)
        {
            hitStop--;
        }
        else
        {
            tickTimer += Time.fixedDeltaTime;
            if (tickTimer>=tickTimerMax)
            {
                tickTimer -= tickTimerMax;
                tick++;
                if (OnTick != null)
                {
                    OnTick(this, new OnTickEventArgs { tick = tick });
                }
            }
        }
	}
    public static void GlobalPrefab(int _index, GameObject _parentObj)
    {
        GameObject nextPrefab = Instantiate(gameEngine.globalPrefabs[_index], _parentObj.transform.position, _parentObj.transform.rotation, _parentObj.transform);
        //nextPrefab.transform.localScale = _parentObj.transform.localScale;
        
        nextPrefab.transform.SetParent(null);
       
    }
    public static void GlobalPrefab(int _index, Vector3 position)
    {
        GameObject nextPrefab = Instantiate(gameEngine.globalPrefabs[_index], position, Quaternion.identity);
        nextPrefab.transform.localScale = Vector3.one;
        
       
    }
}

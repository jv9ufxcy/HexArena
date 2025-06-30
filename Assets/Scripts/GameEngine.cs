using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;


public class GameEngine : MonoBehaviour
{
    public static float hitStop;

    public static GameEngine gameEngine;

    public float deadZone = 0.2f;

    public Player mainCharacter;

    public GameObject[] globalPrefabs;

    public Transform DamagePopup;
    
    // Use this for initialization
    void Awake ()
    {
        gameEngine = this;
    }
    public static void SetHitPause(float _pow)
    {
        if (_pow > hitStop)
        {
            hitStop = _pow;
        }
    }
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (hitStop>0)
        {
            hitStop--;
        }
	}
    public static void GlobalPrefab(int _index, GameObject _parentObj)
    {
        GameObject nextPrefab = Instantiate(gameEngine.globalPrefabs[_index], _parentObj.transform.position, _parentObj.transform.rotation, _parentObj.transform);
        nextPrefab.transform.localScale = _parentObj.transform.localScale;
        
        nextPrefab.transform.SetParent(null);
       
    }
}

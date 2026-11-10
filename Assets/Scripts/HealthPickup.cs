using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healAmt = 60;
    private SpriteRenderer spriteVisual;
    private Vector3 spriteOffset = new Vector3();
    private bool falling = true;
    private float fallingTimer = .5f;
    private void OnEnable()
    {
        if (!GameEngine.gameEngine.pickupObj.Contains(gameObject))
        {
            GameEngine.gameEngine.AddPickup(gameObject, true);
        }
        falling = true;
    }
    //private void OnDisable()
    //{
    //    if (GameEngine.gameEngine.pickupObj.Contains(gameObject))
    //    {
    //        GameEngine.gameEngine.AddPickup(gameObject, false);
    //    }
    //}
    private void Start()
    {
        spriteVisual=GetComponentInChildren<SpriteRenderer>();
        spriteOffset = spriteVisual.transform.position;
        
        spriteVisual.transform.DOPunchScale(Vector3.one*1.25f, fallingTimer/2);
    }
    // Update is called once per frame
    void Update()
    {
        spriteVisual.transform.position = new Vector3(spriteOffset.x, spriteOffset.y + Mathf.Sin(Time.time * 4) * 0.125f, spriteOffset.z);
        if (falling)
        {
            fallingTimer -= Time.deltaTime;
            if (fallingTimer < 0)
            {
                fallingTimer = 0;
                falling = false;
            }
        }
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")&&!falling)
        {
            GameEngine.gameEngine.mainCharacter.DoHeal(healAmt);
            GameEngine.gameEngine.mainCharacter.StartInvul(5f, 30f, Color.white);
            if (GameEngine.gameEngine.pickupObj.Contains(gameObject))
            {
                GameEngine.gameEngine.AddPickup(gameObject,false);
            }
            Destroy(gameObject);
        }
    }
}

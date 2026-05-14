using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ReloadPickup : MonoBehaviour, IHittable
{
    [SerializeField] private int[] bulletArray = {2,2,2,2,2,2};
    [SerializeField] private int numOfBullets = 6, ammoStep;
    [SerializeField] private Sprite[] ammoType;
    [SerializeField] private bool ammoCycle = false;
    [SerializeField] private float ammoTimer = 1f, ammoTimerMax=1f;
    private SpriteRenderer spriteVisual;
    private Vector3 spriteOffset = new Vector3();
    private bool falling = true;
    private float fallingTimer = .5f;
    private void OnEnable()
    {
        falling = true;
    }
    private void Start()
    {
        spriteVisual = GetComponentInChildren<SpriteRenderer>();
        spriteOffset = spriteVisual.transform.position;
        ammoStep = bulletArray[0];
        spriteVisual.transform.DOPunchScale(Vector3.one * 1.25f, fallingTimer / 2);
        if (ammoCycle)
        {
            RandomStartAmmo();
        }
    }
    // Update is called once per frame
    void Update()
    {
        spriteVisual.transform.position = new Vector3(spriteOffset.x, spriteOffset.y+Mathf.Sin(Time.time*4)*0.125f, spriteOffset.z);
        if (ammoCycle && !falling)
        {
            ammoTimer-=Time.deltaTime;
            if (ammoTimer<=0)
            {
                ammoStep++;
                if (ammoStep > 5)
                {
                    ammoStep = 0;
                }
                ChangeAmmo(ammoStep);
                ammoTimer = ammoTimerMax;
            }
        }
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
        if (collision.CompareTag("Player") && !falling)
        {
            collision.GetComponent<Player>().Reload(numOfBullets, bulletArray);
            Destroy(gameObject);
        }
    }
    public void ChangeAmmo(int ammoIndex)
    {
        if (ammoStep>5)
        {
            ammoStep = 0;
        }
        spriteVisual.sprite = ammoType[ammoIndex];
        for (int i = 0; i < bulletArray.Length; i++)
        {
            bulletArray[i] = ammoIndex;
        }
        ammoStep = ammoIndex;
    }
    void RandomStartAmmo()
    {
        int rand = Random.Range(0, 5);
        ChangeAmmo(rand);
    }
    public void Hit(int dam, int effect, int bounceLvl, Vector2 dir)
    {
        ChangeAmmo(effect);
        ammoTimer=ammoTimerMax*2;
    }
}

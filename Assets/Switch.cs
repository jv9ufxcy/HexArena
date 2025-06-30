using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Switch : MonoBehaviour, IHittable
{
    [SerializeField] Transform visualObj;
    [SerializeField] GameObject gateObj, pathObj;
    SpriteRenderer spriteRenderer;
    [SerializeField] Sprite inactiveSprite, activeSprite;
    bool isActive = false;
    // Start is called before the first frame update
    void Start()
    {
        //visualObj = GetComponentInChildren<Transform>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        //inactiveSprite = spriteRenderer.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Hit(int dam, int effect, int bounceLvl, Vector2 dir)
    {
        if (!isActive)
        {
            spriteRenderer.sprite = activeSprite;
            isActive = true;
            if (gateObj!=null)
                gateObj.SetActive(false);
            if (pathObj != null)
                pathObj.SetActive(true);
            visualObj.DOPunchPosition(Vector2.one * 1.125f, 0.167f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpriteVisual : MonoBehaviour
{
    private SpriteRenderer spriteRend;
    private Material spriteMat;
    private Color defaultSpriteCol, defaultOutlineCol = Color.clear;
    // Start is called before the first frame update
    void Start()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        spriteMat = spriteRend.material;
        defaultSpriteCol = spriteRend.color;
        spriteRend.material.SetFloat("_FlashAmt", 0f);
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
    public void SetSpriteColor(Color col)
    {
        spriteRend.material.SetColor("_SpriteColor", col);
    }
    public void SetFlash(float flashAmt, Color flashCol)
    {
        StartCoroutine(FlashWhiteDamage(flashAmt, flashCol));
    }
    private IEnumerator FlashWhiteDamage(float hitFlash, Color flashColor)
    {
        spriteRend.material.SetFloat("_FlashAmt", 0);
        spriteRend.material.SetFloat("_FlashAmt", 1);
        for (int i = 0; i < hitFlash; i++)
        {
            spriteRend.material.SetColor("_SpriteColor", Color.white);
            yield return new WaitForFixedUpdate();
        }
        spriteRend.material.SetFloat("_FlashAmt", 0);
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class DamagePopup : MonoBehaviour
{
    public static DamagePopup Create(Vector3 pos, int amount, int bounce)
    {
        Transform dpTransform = Instantiate(GameEngine.gameEngine.DamagePopup, pos, Quaternion.identity);

        DamagePopup damage = dpTransform.GetComponent<DamagePopup>();
        damage.Setup(amount, bounce);

        return damage;
    }
    private TextMeshPro textMesh;
    private float disappearTimer = 1f;
    private Color textColor;
    private static int sortOrder;
    void Awake()
    {
        textMesh = transform.GetComponent<TextMeshPro>();
    }
    public void Setup(int damAmt, int bounce)
    {
        textMesh.SetText(damAmt.ToString());
        if (bounce<=1)
        {
            textMesh.fontSize = 8;
            textColor = textMesh.color;
        }
        else if(bounce==2)
        {
            textMesh.fontSize = 12;
            textColor = Color.yellow;
        }
        else if (bounce >= 2)
        {
            textMesh.fontSize = 16;
            textColor = Color.red;
        }
        textMesh.color = textColor;
        disappearTimer = 1f;
        sortOrder++;
        textMesh.sortingOrder = sortOrder;
        transform.DOScale(1.25f, 0.25f);
        transform.DOScale(.25f, 1).SetDelay(0.25f);
        float moveYSpeed = 10f;
        transform.DOLocalMoveX(transform.localPosition.x-2, 0.5f);
        transform.DOLocalMoveY(transform.localPosition.y+moveYSpeed, 1).SetDelay(0.5f);
        textMesh.DOFade(0, 0.5f).SetDelay(0.5f);
        Destroy(this.gameObject, 1f);
    }
    //private void FixedUpdate()
    //{
       
    //    //transform.position += new Vector3(0, moveYSpeed) * Time.fixedDeltaTime;
        
    //    disappearTimer -= Time.fixedDeltaTime;
    //    if (disappearTimer<0)
    //    {
            
    //    }
    //}
}

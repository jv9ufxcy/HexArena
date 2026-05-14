using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageDrop : MonoBehaviour,IHittable
{
    [SerializeField]int baseDropChance = 15;
    private void DropItemChance(int dropRate)
    {
        //Player playerChar = GameEngine.gameEngine.mainCharacter;
        //float healthOverMax = playerChar.CurHealth / playerChar.MaxHealth;
        //int dropRate = Mathf.RoundToInt(baseDropChance / healthOverMax);
        int randomChance = UnityEngine.Random.Range(0, 100);
        //Debug.Log("ItemDropRate: " + dropRate + "RandomChance: " + randomChance);
        if (randomChance <= baseDropChance)
        {
            GameEngine.GlobalPrefab(9, this.gameObject);
        }
        //else
        //{
        //    GameEngine.GlobalPrefab(7, this.gameObject);
        //}
    }

    public void Hit(int dam, int effect, int bounceLvl, Vector2 dir)
    {
        DropItemChance(baseDropChance);
        GameEngine.GlobalPrefab(1, this.gameObject);
        Destroy(this.gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DropItemChance(baseDropChance);
            GameEngine.GlobalPrefab(1, this.gameObject);
            Destroy(this.gameObject);
        }
    }
}

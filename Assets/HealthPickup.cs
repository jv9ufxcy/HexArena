using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healAmt = 60;
    [SerializeField] private Transform spriteVisual;

    // Update is called once per frame
    void Update()
    {
        //spriteVisual.position += new Vector3(0f, 1.125f * Mathf.Sin(Time.deltaTime),0f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().DoHeal(healAmt);
            Destroy(gameObject);
        }
    }
}

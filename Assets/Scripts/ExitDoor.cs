using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameEngine.gameEngine.NextLevel(gameObject);
            GameEngine.gameEngine.mainCharacter.StartInvul(5f, 30f, Color.white);
        }
    }
}

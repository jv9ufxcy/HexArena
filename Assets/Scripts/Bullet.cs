using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector2 velocity;
    public int damage = 1;
    public float speed;
    public float rotation;
    public bool canHitSelf = false;
    public float lifetime = 2f;
    private List<Collider2D> enemies = new List<Collider2D>();
    public GameObject owner;
    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 0, rotation);
        Destroy(gameObject, lifetime);
    }
    void Update()
    {
        transform.Translate(velocity * speed * Time.deltaTime);
    }
    public int projectileIndex = 0;
    public string tagToHit = "Player";
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canHitSelf && other.gameObject == owner)
            return;
        IHittable victim = other.transform.root.GetComponent<IHittable>();
        if (victim != null&&!enemies.Contains(other))
        {
            enemies.Add(other);
            victim.Hit(damage, projectileIndex, transform.up);
            Destroy(gameObject, .2f);
        }
    }
}

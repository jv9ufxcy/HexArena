using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class Projectile : MonoBehaviour
{
    public float speed, bounceForce = 0.8f;
    [SerializeField]
    private int damage, bouncePower = 1;
    [SerializeField] private float lifeTime = 3;
    private Vector2 target = new Vector2(), direction = new Vector2();
    [SerializeField] private float punchScaleDur = .125f;
    [SerializeField] private Vector3 punchScale = new Vector3(1.125f, 1.125f, 1);
    [SerializeField]private bool canHitSelf = false;
    private GameObject owner;
    private Rigidbody2D rb;
    private ParticleSystem ps;
    private Vector3 lastVelocity;
    private AudioManager audioManager;
    [SerializeField] private string hitSound = "Bullet", reflectSound = "Reflect";
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioManager = AudioManager.instance;
        ps = GetComponent<ParticleSystem>();
    }
    // Start is called before the first frame update
    void Start()
    {
        //target = Vector2.up;
        direction = transform.up;
        rb.velocity = direction * speed;

        this.transform.DOPunchScale(punchScale, .125f);
        switch (spellEffect)
        {
            case Spell.Wound:
                Destroy(this.gameObject, lifeTime);
                break;
            case Spell.Skewer:
                break;
            case Spell.Guardian:
                break;
            case Spell.Frog:
                break;
            case Spell.Freeze:
                break;
            case Spell.Explosion:
                break;
            default:
                break;
        }
    }

    public void ChangeTarget(Vector2 target, GameObject self)
    {
        this.direction = new Vector2(target.x - transform.position.x, target.y - transform.position.y);
        this.owner = self;
    }
    public void ChangeOwner(GameObject newOwner)
    {
        owner = newOwner;
    }
    public void ChangeDirection(Vector2 newDir)
    {
        direction = newDir;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canHitSelf && other.gameObject == owner)
            return;
        IHittable hit = other.GetComponent<IHittable>();
        if (hit != null)
        {
            
            audioManager.PlaySound(hitSound);
            switch (spellEffect)
            {
                case Spell.Wound:
                    hit.Hit(damage, (int)spellEffect, bouncePower, direction);
                    break;
                case Spell.Skewer:
                    Physics2D.IgnoreCollision(other,GetComponent<Collider2D>(), true);
                    hit.Hit(damage*bouncePower, (int)spellEffect, bouncePower, direction);
                    break;
                case Spell.Guardian:
                    hit.Hit(damage, (int)spellEffect, bouncePower, direction);
                    EndLife();
                    break;
                case Spell.Frog:
                    hit.Hit(damage, (int)spellEffect, bouncePower, direction);
                    EndLife();
                    break;
                case Spell.Freeze:
                    hit.Hit(damage, (int)spellEffect, bouncePower, direction);
                    EndLife();
                    break;
                case Spell.Explosion:
                    hit.Hit(damage, (int)spellEffect, bouncePower, direction);
                    EndLife();
                    break;
                default:
                    break;
            }
        }

    }
    [Range(2,12)]
    [SerializeField]private int bulletParticle= 2;
    private void EndLife()
    {
        transform.DOComplete();
        //transform.DOScale(Vector3.one, 0);
        GameEngine.GlobalPrefab(bulletParticle, this.gameObject);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject != owner)
        {
            Vector2 wallNormal = collision.GetContact(0).normal;
            Vector2 reflectedDirection = Vector2.Reflect(direction, wallNormal);
            direction = reflectedDirection.normalized;
            //Debug.Log(dir);
            audioManager.PlaySound(reflectSound);
            lifeTime -= 1;
            if (lifeTime <= 0)
            {
                EndLife();
            }
            rb.velocity = (direction * Mathf.Max(speed, 0f));
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
            bouncePower++;
            canHitSelf = true;
            BounceEffect();
        }
    }

    private void BounceEffect()
    {
        this.transform.DOComplete();
        this.transform.DOPunchScale(punchScale / 2, .125f);
        var emission = ps.emission;
        if (ps!=null)
        {
            emission.rateOverDistance = emission.rateOverDistance.constant + 2f;
        }
    }

    private enum Spell { Wound, Skewer, Guardian, Frog, Freeze, Explosion }
    [SerializeField]private Spell spellEffect;
    private void SpellEffect()
    {
        switch (spellEffect)
        {
            case Spell.Wound:
                break;
            case Spell.Skewer:
                break;
            case Spell.Guardian:
                break;
            case Spell.Frog:
                break;
            case Spell.Freeze:
                break;
            case Spell.Explosion:
                break;
            default:
                break;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Surface : MonoBehaviour
{
    [SerializeField]
    private int damageOverTime=32;
    [SerializeField]
    [Range(0f, 5f)]
    private int spellEffectIndex=5;
    [SerializeField] private float lifeTime = 3;
    private bool damageTickActivate = false;

    private ParticleSystem ps;
    private AudioManager audioManager;
    private SpriteRenderer rend;
    private Material material;
    [SerializeField] private Vector2 scrollSpeed = new Vector2(1,0), currentPos = new Vector2();
    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        audioManager = AudioManager.instance;
        rend = GetComponentInChildren<SpriteRenderer>();
        material = rend.material;
        currentPos = material.mainTextureOffset;
        //GameEngine.OnTick += delegate (object sender, GameEngine.OnTickEventArgs e)
    }

    // Update is called once per frame
    void Update()
    {
        if (GameEngine.hitStop<=0)
        {
            material.SetTextureOffset("_MainTex", currentPos += scrollSpeed * Time.deltaTime); 
            lifeTime -=Time.deltaTime;
            if (lifeTime<=0)
            {
                damageTickActivate = false;
                ps.enableEmission = false;
                transform.DOScale(0, .16667f);
                Destroy(gameObject, .16667f);
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        IHittable hit = collision.GetComponent<IHittable>();
        if (hit != null)
        {
            Vector3 hitPos = collision.transform.position;
            Vector2 direction = new Vector2(hitPos.x - transform.position.x, hitPos.y - transform.position.y);
            DamageCharacterInSurface(hit, direction);
            GameEngine.OnTick += TickSurface;
        }

    }
    void TickSurface(object sender, GameEngine.OnTickEventArgs e)
    {
        damageTickActivate = true;
        if (ps.emission.enabled==true)
        {
            ps.Play();
        }
    }
    private void DamageCharacterInSurface(IHittable hit, Vector2 direction)
    {
        if (damageTickActivate)
        {
            switch (spellEffectIndex)
            {
                default:
                    hit.Hit(damageOverTime, (int)spellEffectIndex, 0, Vector2.zero/*direction*/);
                    break;
            }
            damageTickActivate = false;
        }
    }
}

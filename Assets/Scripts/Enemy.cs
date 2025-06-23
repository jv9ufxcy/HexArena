using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Playables;
using UnityEditor;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour,IHittable
{
    private Rigidbody2D rb;

    [SerializeField]private Transform player;

    [SerializeField] private SpriteRenderer spriteRend;
    [SerializeField] private Sprite defaultSprite, frozenSprite, frogSprite;
    [SerializeField] private Material defaultMat, flashMat;
    [SerializeField]
    private bool active = false;


    [SerializeField]
    private float maxHealth;
    private float health;

    [SerializeField]
    private float speed;
    [SerializeField]
    private float stoppingDistance;
    [SerializeField]
    private float retreatDistance;

    [Header("Shooting")]
    [SerializeField]
    private GameObject projectileObject;
    [SerializeField] private Transform projectileFirePoint;
    [SerializeField]
    private float startTimeBtwShots;
    private float shotTimer;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private string readyBark = "Ready", attackBark = "Attack", hurtBark = "Hurt", deathBark = "Death", frozenBark = "Frozen", frogBark = "Frog", ghostBark = "Ghost";

    private enum EnemyState { neutral, attacking, frozen, frog, stunned }
    [SerializeField] private EnemyState eState;
    private enum EnemyType { melee, swarmer, ranged, bomber, charger }
    [SerializeField] private EnemyType eType;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        spriteRend = GetComponentInChildren<SpriteRenderer>();
        SetMaxHealth();
    }
    private void OnEnable()
    {
        SetMaxHealth();
        spriteRend.color = Color.white;
        if (defaultMat != null)
            spriteRend.material = defaultMat;
        active = true;
        SpawnIntro();
    }

    private void SpawnIntro()
    {
        spriteRend.color = Color.clear;
        stunTimer = 1f;
        eState = EnemyState.stunned;
        spriteRend.DOColor(Color.white, .8f);
    }

    private void OnDisable()
    {
        Spawner.spawner.RemoveEnemyFromList(gameObject);
    }
    private void SetMaxHealth()
    {
        health = maxHealth;
    }

    private void Start()
    {
        defaultSprite = spriteRend.sprite;
        defaultMat = spriteRend.material;
        shotTimer = startTimeBtwShots;
        audioManager = AudioManager.instance;
        Activate();
    }

    private void PlaySound(string sound)
    {
        if (sound!=null)
        {
            audioManager.PlaySound(sound);
        }
    }

    public void Activate()
    {
        this.player = GameEngine.gameEngine.mainCharacter.transform;
        active = true;
    }

    public void DoDamage(float damage)
    {
        if(active)
             health -= damage;
        
        PlaySound(hurtBark);
        
        spriteRend.transform.DOShakePosition(0.125f, damage, 7, 120);
        StartCoroutine(FlashWhiteDamage(5));
        GameEngine.SetHitPause(4);
        
        stunTimer = .5f;
        eState = EnemyState.stunned;
    }

    public void Hit(int dam, int effect, Vector2 dir)
    {
        if (dam > 0)
        {
            DeSpell();
            DoDamage(dam);
            rb.velocity = dir * 8;
        }
        SpellEffect(effect);
    }
    void SpellEffect(int effect)
    {
        switch (effect)
        {
            case 0://Wound self damages instantly
                break;
            case 1://Skewer damages on contact
                break;
            case 2://Guardians applies shield
                ApplyGuardians();
                break;
            case 3://Polymorph changes movement to grid based for 6 seconds or until damaged
                ApplyPolymorph();
                break;
            case 4://Freeze stops movement for 6 seconds or until damaged
                ApplyFrost();
                break;
            case 5://Explosion knockback
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!active)
            return;

        if (GameEngine.hitStop <= 0)
        {
            CheckDeath();
            CooldownTimers();

            switch (eState)
            {
                case EnemyState.neutral:
                    spriteRend.transform.DOLocalRotate(Vector3.zero, 0);
                    Movement();
                    RotateWeapon();
                    UpdateAnimator();
                    break;
                case EnemyState.attacking:
                    switch (eType)
                    {
                        case EnemyType.melee:
                            MeleeAttack();
                            break;
                        case EnemyType.swarmer:
                            break;
                        case EnemyType.ranged:
                            FireBullet();
                            break;
                        case EnemyType.bomber:
                            MeleeAttack();
                            break;
                        case EnemyType.charger:
                            ChargeAttack();
                            break;
                        default:
                            break;
                    }
                    break;
                case EnemyState.frozen:
                    rb.velocity = Vector2.zero;
                    break;
                case EnemyState.frog:
                    rb.velocity = Vector2.zero;
                    break;
                case EnemyState.stunned:
                    rb.velocity = Vector2.zero;
                    if (stunTimer > 0)
                    {
                        stunTimer -= Time.deltaTime;
                    }
                    else
                        DeSpell();
                    break;
                default:
                    break;
            }
        }
    }

    private void CooldownTimers()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        if (shotTimer > 0)
        {
            shotTimer -= Time.deltaTime;
        }
        if (guardianTimer > 0)
        {
            guardianTimer -= Time.deltaTime;
        }
        else
        {
            if (guardiansCreated.Count > 0)
            {
                foreach (GameObject satellite in guardiansCreated)
                {
                    Destroy(satellite);
                }
                guardiansCreated.Clear();
            }
        }
    }
    [SerializeField] private float shakeDuration = 0.1f, shakeStrength = 0.5f;
    [SerializeField] private int blinkDuration = 1, hitFreezeFrames = 2;
    private void CheckDeath()
    {
        if (health <= 0)
        {
            DeSpell();
            if (guardiansCreated.Count > 0)
            {
                foreach (GameObject satellite in guardiansCreated)
                {
                    Destroy(satellite);
                }
                guardiansCreated.Clear();
            }
            PlaySound(deathBark);
            eState=EnemyState.neutral;
            shotTimer = startTimeBtwShots;
            StartCoroutine(EnemyDeath(shakeDuration, shakeStrength, blinkDuration, hitFreezeFrames));
        }
    }
    private IEnumerator EnemyDeath(float shakeDur, float shakeStr, int blinkDur, int hitFreezeFrames)
    {
        active = false;
        int hitPause = hitFreezeFrames;
        float waitFramesToSeconds = hitPause / 60;//over 60 frames
        GameEngine.SetHitPause(hitPause);

        yield return new WaitForSeconds(waitFramesToSeconds / 2);//start shaking halfway into
        ShakeSprite(shakeDur, shakeStr, 10, 90);
        StartCoroutine(BlinkColor(blinkDur));//blink for 5 frames
        yield return new WaitForSeconds(shakeDur);
        //explode now
        ShakeCamera(1, .25f);
        GameEngine.GlobalPrefab(0, this.gameObject);
        if (eType == EnemyType.bomber)
        {
            //FireArrow(1, 8, 8);
            FireBulletWave(1, 8, 16, 1, 1, 0, 360, 0);
        }
        gameObject.SetActive(false);
    }
    [SerializeField] private int invulFlickerRate = 4;
    private IEnumerator BlinkColor(int intervalsOfFive)
    {
        for (int b = 0; b < intervalsOfFive; b++)
        {
            spriteRend.color = Color.magenta;
            for (int i = 0; i < invulFlickerRate; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            spriteRend.color = Color.white;
            for (int i = 0; i < invulFlickerRate; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }
    }
    private void ShakeSprite(float dur, float str, int vib, float rand)
    {
        spriteRend.transform.DOShakePosition(dur, str, vib, rand, false, true);
    }
    private static void ShakeCamera(float amplitude, float time)
    {
        CinemachineShake.instance.ShakeCamera(amplitude, time);
    }
    private enum AttackingState { standby,attack, recover,}
    [SerializeField]private AttackingState attackState;
    [SerializeField] private int numOfAttacks = 1, chargeSpeed = 24;
    private void MeleeAttack()
    {
        switch (attackState)
        {
            case AttackingState.standby:
                //anticipation Pose
                AnticipationFlash();
                shotTimer = startTimeBtwShots;
                attackState = AttackingState.attack;
                break;
            case AttackingState.attack:
                for (int i = 0; i < numOfAttacks; i++)
                {
                    if (shotTimer <= 0)
                    {
                        //punchy forward
                        AttackAnim();
                        //spawn hitbox
                        GameObject hit = Instantiate(projectileObject, transform.position + (Vector3)direction.normalized, projectileFirePoint.rotation);
                        hit.GetComponent<Projectile>().ChangeDirection(direction);
                        hit.GetComponent<Projectile>().ChangeOwner(this.gameObject);
                        //bulletFX
                        Vector3 punchScale = new Vector3(1.125f, 1.125f, 1);
                        //hit.transform.DOPunchScale(punchScale, .125f);
                        //cooldown
                        cooldownTimer = actionCooldown;
                        attackState = AttackingState.recover;
                    }
                }
                break;
            case AttackingState.recover:
                if (cooldownTimer <= 0)
                {
                    eState = EnemyState.neutral;
                    attackState = AttackingState.standby;
                }
                break;
            default:
                break;
        }
    }
    private void ChargeAttack()
    {
        switch (attackState)
        {
            case AttackingState.standby:
                //anticipation Pose
                AnticipationFlash();
                shotTimer = startTimeBtwShots;
                attackState = AttackingState.attack;
                break;
            case AttackingState.attack:
                if (shotTimer <= 0)
                {
                    //punchy forward
                    AttackAnim();
                    rb.velocity = direction * chargeSpeed;
                    //spawn hitbox
                    GameObject hit = Instantiate(projectileObject, transform.position + (Vector3)direction.normalized, projectileFirePoint.rotation,this.transform);
                    hit.GetComponent<Projectile>().ChangeDirection(direction);
                    hit.GetComponent<Projectile>().ChangeOwner(this.gameObject);
                    //bulletFX
                    Vector3 punchScale = new Vector3(1.125f, 1.125f, 1);
                    //hit.transform.DOPunchScale(punchScale, .125f);
                    //cooldown
                    cooldownTimer = actionCooldown;
                    attackState = AttackingState.recover;
                }
                break;
            case AttackingState.recover:
                if (cooldownTimer <= 0)
                {
                    rb.velocity = Vector2.zero;
                    eState = EnemyState.neutral;
                    attackState = AttackingState.standby;
                }
                break;
            default:
                break;
        }
    }

    private void AttackAnim()
    {
        Vector3 punchPos = new Vector3(1, 0, 0);
        spriteRend.transform.DOPunchPosition(direction, .125f);
        PlaySound(attackBark);
    }

    private void AnticipationFlash()
    {
        Transform spriteTransform = spriteRend.transform;
        Vector3 punchScaleStandby = new Vector3(0, .5f, 0);
        spriteTransform.DOPunchPosition(punchScaleStandby, startTimeBtwShots / 2, 6, .25f);

        PlaySound(readyBark);
        StartCoroutine(FlashWhiteDamage(5));
    }

    private void RotateWeapon()
    {
        Vector2 lookAxis = new Vector2();
        lookAxis = new Vector2(Mathf.Sin(direction.x), Mathf.Sin(direction.y));
        float angle = Mathf.Atan2(lookAxis.y, lookAxis.x) * Mathf.Rad2Deg;
        Quaternion newRot = Quaternion.Euler(0, 0, angle - 90f);
        projectileFirePoint.rotation = Quaternion.Slerp(transform.rotation, newRot, 1);
    }
    Vector2 direction = new Vector2();
    private float cooldownTimer;
    [SerializeField]private float actionCooldown = 3;
    private void Movement()
    {
        float distanceFromPlayer = Vector2.Distance(player.position, transform.position);

        if (distanceFromPlayer > stoppingDistance)//follow Player
        {
            Vector2 target = player.transform.position;
            direction = new Vector2(target.x - transform.position.x, target.y - transform.position.y).normalized;
            rb.velocity = direction * speed;
        }
        else//attack
        { 
            rb.velocity = Vector2.zero;
            eState = EnemyState.attacking;
        }
    }

    private void FireBullet()
    {
        cooldownTimer = actionCooldown;
        if (projectileObject != null)
        {
            if (shotTimer <= 0)
            {
                GameObject proj = Instantiate(projectileObject, projectileFirePoint.position, Quaternion.identity);
                proj.GetComponent<Projectile>().ChangeTarget(player.position, gameObject);
                PlaySound(attackBark);
                shotTimer = startTimeBtwShots;
            }
        }
    }

    [Header("Effects")]
    [SerializeField] private float aniMoveSpeed, yAmp = 0.1f, yFrq = 24f;
    void UpdateAnimator()
    {
        var direction = Mathf.Sign(player.transform.position.x-transform.position.x);
        spriteRend.transform.localScale = new Vector3(direction, 1f, 1f);

        Vector2 latSpeed = rb.velocity;
        aniMoveSpeed = Vector3.SqrMagnitude(latSpeed);
        if (aniMoveSpeed > 0)
        {
            float yPos = Mathf.Sin(Time.time * yFrq) * yAmp;
            spriteRend.gameObject.transform.localPosition = new Vector3(0, yPos, 0);//bounce sprite
        }
        else
            spriteRend.gameObject.transform.localPosition = Vector3.zero;
    }
    public int numOfGuardians = 1;
    private float guardianTimer,stunTimer;
    [SerializeField] private GameObject[] magicObjects;
    private List<GameObject> guardiansCreated = new List<GameObject>();
    void ApplyGuardians()
    {
        PlaySound(ghostBark);
        guardianTimer = 6;
        for (int i = 0; i < numOfGuardians; i++)
        {
            GameObject guardianInstance = Instantiate(magicObjects[0], transform.position, Quaternion.identity);
            GuardianSatellite guardian = guardianInstance.GetComponent<GuardianSatellite>();
            guardian.SetStartingAngle(i, numOfGuardians);//tell it which angle to start at
            guardian.SetQueen(this.gameObject);//tell it which obj to follow
            //guardian.queen = this.gameObject;
            guardiansCreated.Add(guardianInstance);
        }
    }
    void ApplyPolymorph()
    {
        stunTimer = 6;
        eState = EnemyState.frog;
        spriteRend.sprite = frogSprite;
        PlaySound(frogBark);
    }
    void ApplyFrost()
    {
        stunTimer = 6;
        eState = EnemyState.frozen;
        spriteRend.sprite = frozenSprite;
        PlaySound(frozenBark);
    }
    void DeSpell()
    {
        stunTimer = 0;
        spriteRend.sprite = defaultSprite;
        eState = EnemyState.neutral;
    }
    private IEnumerator FlashWhiteDamage(float hitFlash)
    {
        spriteRend.material = defaultMat;
        spriteRend.material = flashMat;
        for (int i = 0; i < hitFlash; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        spriteRend.material = defaultMat;
    }
    float[] rotations;
    private void FireBulletWave(float bulletResource, float numberOfBullets, float speed, float velocityX, float velocityY, float minRot, float maxRot, float isRandom)
    {
        rotations = new float[(int)numberOfBullets];
        Vector2 velocity = new Vector2(velocityX, velocityY);

        if (numberOfBullets > 1)
        {
            if (isRandom != 0)
                RandomRotations((int)numberOfBullets, minRot, maxRot);
            else
                DistributedRotations((int)numberOfBullets, minRot, maxRot);
        }
        else
        {
            for (int i = 0; i < numberOfBullets; i++)
            {
                rotations[i] = minRot;
            }
        }
        SpawnBullets((int)bulletResource, (int)numberOfBullets, speed, velocity);
    }
    private void FireArrow(float bulletResource, float numberOfBullets, float speed)
    {
        var other = GameEngine.gameEngine.mainCharacter;
        Vector3 dir = other.transform.position - transform.position;
        dir = other.transform.InverseTransformDirection(dir);
        float angle = Mathf.Round(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        //minRot = hitbox.transform.parent.transform.localEulerAngles.z-minRot;
        //maxRot = hitbox.transform.parent.transform.localEulerAngles.z+maxRot;
        FireBulletWave(bulletResource, numberOfBullets, speed, 0, -1, 90 + angle, 90 - angle, 0);
    }
    // Select a random rotation from min to max for each bullet
    public float[] RandomRotations(int numberOfBullets, float minRotation, float maxRotation)
    {
        for (int i = 0; i < numberOfBullets; i++)
        {
            rotations[i] = UnityEngine.Random.Range(minRotation, maxRotation);
        }
        return rotations;

    }

    // This will set random rotations evenly distributed between the min and max Rotation.
    public float[] DistributedRotations(int numberOfBullets, float minRotation, float maxRotation)
    {
        for (int i = 0; i < numberOfBullets; i++)
        {
            var fraction = (float)i / ((float)numberOfBullets - 1);
            var difference = maxRotation - minRotation;
            var fractionOfDifference = fraction * difference;
            rotations[i] = fractionOfDifference + minRotation; // We add minRotation to undo Difference
        }
        //foreach (var r in rotations) print(r);
        return rotations;
    }
    public GameObject[] SpawnBullets(int bulletResource, int numberOfBullets, float bulletSpeed, Vector2 bulletVelocity)
    {
        // Spawn Bullets
        GameObject[] spawnedBullets = new GameObject[numberOfBullets];
        for (int i = 0; i < numberOfBullets; i++)
        {
            spawnedBullets[i] = Instantiate(magicObjects[bulletResource], transform.position, Quaternion.identity);

            var b = spawnedBullets[i].GetComponent<Bullet>();
            b.rotation = rotations[i];
            b.speed = bulletSpeed;
            b.velocity = bulletVelocity;
            b.owner = this.gameObject;
        }
        return spawnedBullets;
    }
}

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour,IHittable
{
    Rigidbody2D rb;

    [Header("Stats")]
    [SerializeField]
    private int maxHealth=3;
    [SerializeField]
    private int startingHealth=2;
    private int curHealth=2;

    [Header("Movement")]
    private Vector2 velocity = new Vector2();
    [SerializeField] private float moveSpeed;
    [SerializeField] private float frogSpeed = 0.375f;
    Vector2 moveAxis,lookAxis;
    [SerializeField] private PlayerControls controls;
    private InputAction move, aim, fire, look;
    [SerializeField] private bool lockRotation = false;
    public Transform orbitTarget;
    [SerializeField] private float orbFreq = 1f, orbAmp=.25f;

    [Header("Shooting")]
    [SerializeField] GameObject[] bullets;
    [SerializeField] private Transform weaponOffset,gunObject, firingPoint;
    [SerializeField] private float rotSpeed = 8f, shootRecovery = 0.125f;
    private Ray ray;
    private RaycastHit rayHit;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private float trajectoryMaxLength = 12f;
    [SerializeField] private int reflections = 3;
    [SerializeField] private LayerMask target;
    [Header("Ammo")]
    [Range(0, 5)]
    public List<int> gunChamber = new List<int> { 5, 1, 1, 1, 3, 3 };
    [SerializeField] private int[] ammo = new int[] { 0, 1, 1, 1, 1, 1, 1, 3, 3, 3, 5, 5 };
    [SerializeField] private UIManager uiScript;
    [Header("Spell Effects")]
    private float stunTimer, guardianTimer, fireRate;
    [SerializeField] private GameObject[] magicObjects;
    private enum PlayerState { neutral, frozen,frog,firing,stunned,dead}
    [SerializeField]private PlayerState state;
    [Header("SoundEffects")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private string reloadBark = "Reload", fireBark = "Fire", hurtBark = "Hurt", deathBark = "Death", frozenBark = "Frozen", frogBark = "Frog", ghostBark = "Ghost", healBark="Heal";

    [Header("Effects")]
    [SerializeField] private SpriteRenderer spriteRend;
    [SerializeField] private Sprite defaultSprite,frozenSprite,frogSprite;
    [SerializeField] private Sprite[] healthStateSprite;
    [SerializeField] private Material defaultMat, flashMat;
    [SerializeField] private float yAmp = 0.1f, yFrq = 16f;
    [SerializeField] private float aniMoveSpeed;

    [Header("Invulnerability")]
    [SerializeField] private Color invulColor;
    private bool isInvulnerable;
    [SerializeField] private float invulCooldown, invulFlickerRate = 4f, invulFrames = 90f;

    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        uiScript.AmmoUpdate(gunChamber);
        curHealth = StartingHealth;
        OnHealthChanged();
        controls = new PlayerControls();
        trajectoryLine=GetComponentInChildren<LineRenderer>();
    }
    private void OnEnable()
    {
        move = controls.Player.Move;
        move.Enable();
        look = controls.Player.Look;
        look.Enable();

        fire = controls.Player.Fire;
        fire.Enable();
        fire.performed += Fire;

        aim = controls.Player.Aim;
        aim.Enable();
        aim.performed += Aim;
        aim.canceled += AimRelease;
    }
    private void OnDisable()
    {
        move.Disable();
        look.Disable();
        fire.Disable();
        aim.Disable();
    }
    private void Start()
    {
        defaultSprite = spriteRend.sprite;
        defaultMat = spriteRend.material;
        audioManager = AudioManager.instance;
    }
    // Update is called once per frame
    void Update()
    {
        if (GameEngine.hitStop <= 0)
        {
            if (curHealth <= 0&&state!=PlayerState.dead)
                Death();
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
            
                moveAxis = move.ReadValue<Vector2>();
                lookAxis = look.ReadValue<Vector2>();
            if (state == PlayerState.neutral && !lockRotation)
            {
                if (look.ReadValue<Vector2>() == Vector2.zero)
                {
                    if (moveAxis != Vector2.zero)
                    { 
                        RotateWeapon(CardinalDir(moveAxis));
                        FacingDir(moveAxis); 
                    }
                }
                else
                {
                    if (lookAxis != Vector2.zero)
                    { 
                        RotateWeapon(CardinalDir(lookAxis));
                        FacingDir(lookAxis);
                    }
                }
            }
            UpdateAnimator();
            if (lockRotation)
            {
                LineRend2DReflections(transform.position, weaponOffset.up);
            }
        }
    }
    private void FixedUpdate()
    {
        if (GameEngine.hitStop <= 0)
        {
            switch (state)
            {
                case PlayerState.neutral:
                    if (move.ReadValue<Vector2>() != Vector2.zero)
                        velocity = CardinalDir(moveAxis.normalized) * moveSpeed;
                    break;
                case PlayerState.frozen:
                    if (stunTimer > 0)
                    {
                        stunTimer -= Time.fixedDeltaTime;
                        velocity = Vector2.zero;
                    }
                    else
                        DeSpell();
                    break;
                case PlayerState.frog:
                    if (stunTimer > 0)
                    {
                        if (move.ReadValue<Vector2>() != Vector2.zero)
                            velocity = CardinalDir(moveAxis.normalized) * frogSpeed;
                        //Vector3 destination = transform.position + (Vector3)moveAxis;
                        //StartCoroutine(LeapFrog(destination));
                        stunTimer -= Time.fixedDeltaTime;
                    }
                    else
                        DeSpell();
                    break;
                case PlayerState.firing:
                    if (fireRate > 0)
                    {
                        fireRate -= Time.fixedDeltaTime;
                    }
                    else
                    {
                        fireRate = 0;
                        state = PlayerState.neutral;
                    }
                    break;
                case PlayerState.stunned:
                    if (stunTimer > 0)
                    {
                        stunTimer -= Time.fixedDeltaTime;
                    }
                    else
                        DeSpell();
                    break;
                case PlayerState.dead:
                    velocity = Vector2.zero;
                    break;
                default:
                    break;
            }
            //rb.MovePosition(rb.position + moveAxis.normalized * moveSpeed * Time.fixedDeltaTime);
            OrbitTarget();
            MoveVelocity();
            if (invulCooldown > 0) { invulCooldown--; }
            else
            {
                isInvulnerable = false;
            }
        }
    }
    private void OrbitTarget()
    {
        float x = Mathf.Cos(orbFreq * Time.time) * orbAmp*1.5f;
        float y = Mathf.Sin(orbFreq * Time.time) * orbAmp;
        float z = orbitTarget.transform.position.z;
        orbitTarget.transform.position = new Vector3(x,y,z)+transform.position;
    }
    private void LineRend2DReflections(Vector3 pos, Vector3 dir)
    {
        trajectoryLine.SetPosition(0, transform.position);
        trajectoryLine.positionCount = 1;

        for (int i = 0; i < reflections; i++)
        {
            Debug.DrawRay(pos, dir * trajectoryMaxLength, Color.green);
            Ray2D ray2D = new Ray2D(pos, dir);
            float remainderLength = trajectoryMaxLength;
            RaycastHit2D hit2D = Physics2D.Raycast(ray2D.origin, ray2D.direction, remainderLength, target);
            if (hit2D)
            {
                pos = hit2D.point;
                dir = Vector3.Reflect(dir, hit2D.normal);
                trajectoryLine.positionCount += 1;
                trajectoryLine.SetPosition(trajectoryLine.positionCount - 1, hit2D.point);
                //ray2D = new Ray2D(hit2D.point, Vector3.Reflect(ray2D.direction, hit2D.normal));

                //if (hit2D.collider.tag != "Ground")
                //{
                //    break;
                //}
            }
            else
            {
                trajectoryLine.positionCount += 1;
                trajectoryLine.SetPosition(trajectoryLine.positionCount - 1, ray2D.origin + ray2D.direction * remainderLength);
            }
        }
    }
    private void MoveVelocity()
    {
        rb.velocity = velocity;
        //rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        //rb.MovePosition(rb.position + moveAxis.normalized * moveSpeed * Time.fixedDeltaTime);
        velocity.Scale(new Vector3(0.5f,0.5f,0.5f));
    }
    private void Death()
    {
        StartCoroutine(GameOver());
    }
    private IEnumerator GameOver()
    {
        stunTimer = 999f;
        state = PlayerState.dead;
        GameEngine.SetHitPause(60);
        ScreenShake(3, 1f);
        velocity = Vector3.zero;
        audioManager.PlaySound(deathBark);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Reload(int max, int[] bulletArray)
    {
        fireRate = shootRecovery;
        state = PlayerState.firing;

        gunChamber.Clear();
        for (int i = 0; i < max; i++)
        {
            ;
            gunChamber.Add(bulletArray[Random.Range(0, bulletArray.Length)]);
            uiScript.AmmoUpdate(gunChamber);
            currentBullet = 0;
        }
        gunObject.DOPunchRotation(new Vector3(0, 0, 361), .25f);
        PlaySound(reloadBark);
    }
    private void RotateWeapon(Vector2 rotAxis)
    {
        //lookAxis = new Vector2(rotAxis.x, rotAxis.y);
        float angle = Mathf.Atan2(rotAxis.y, rotAxis.x) * Mathf.Rad2Deg;
        Quaternion newRot = Quaternion.Euler(0, 0, angle - 90f);
        weaponOffset.rotation = Quaternion.Slerp(transform.rotation, newRot, rotSpeed);
    }

    void UpdateAnimator()
    {
        Vector2 latSpeed = velocity;
        aniMoveSpeed = Vector3.SqrMagnitude(latSpeed);
        if (aniMoveSpeed > 1)
        {
            float yPos = Mathf.Sin(Time.time * yFrq) * yAmp;
            spriteRend.gameObject.transform.localPosition = new Vector3(0, yPos, 0);
        }
        else
            spriteRend.gameObject.transform.localPosition = Vector3.zero;
        
        //gunObject.transform.localScale = new Vector3(direction, direction, 1f);

    }

    private void FacingDir(Vector2 dir)
    {
        spriteRend.transform.localScale = new Vector3(Mathf.Sign(CardinalDir(dir).x), 1f, 1f);
    }

    private int currentBullet;
    private void Aim(InputAction.CallbackContext context)
    {
        lockRotation = true;
        trajectoryLine.positionCount = 1;
        if (state == PlayerState.neutral)//if neutral control
        {

        }
    }
    private void AimRelease(InputAction.CallbackContext context)
    {
        lockRotation = false;
        trajectoryLine.positionCount = 0;
    }
    private void Fire(InputAction.CallbackContext context)
    {
        if (GameEngine.hitStop <= 0)
        {
            if (state == PlayerState.neutral)
            {
                if (gunChamber.Count == 0)//reload
                {
                    //int[] bulletArray = new int[] { 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 4, 4, 4, 5, 5 };
                    Reload(6,ammo);
                }
                else//fire
                {
                    //defaults
                    gunObject.transform.DOComplete();
                    gunObject.transform.DOScale(Vector3.one, 0);
                    //weaponOffset.transform.DORotate(Vector3.one, 0);
                    //spawn bullet and remove from inventory
                    GameObject proj = Instantiate(bullets[gunChamber[0]], firingPoint.transform.position, gunObject.rotation);
                    gunChamber.RemoveAt(0);
                    //punch gun
                    gunObject.DOPunchRotation(new Vector3(0, 0, 60f), shootRecovery);
                    Vector3 punchScale = new Vector3(1.025f, 1.025f, 1);
                    gunObject.transform.DOPunchScale(punchScale, .25f,2,shootRecovery);
                    //set bulletDirection
                    proj.GetComponent<Projectile>().ChangeDirection(CardinalDir(lookAxis));
                    proj.GetComponent<Projectile>().ChangeOwner(this.gameObject);
                    //firingState
                    fireRate = shootRecovery;
                    state = PlayerState.firing;
                    uiScript.AmmoUpdate(gunChamber);
                    currentBullet++;
                    PlaySound(fireBark);
                    ScreenShake(1.5f, .25f);
                }
            }
        }
    }
    public void DoHeal(int healthGain)
    {
        curHealth += healthGain;
        curHealth = Mathf.Clamp(curHealth, 0, MaxHealth);
        OnHealthChanged();
        DamagePopup.Create(transform.position, healthGain, -1);
        PlaySound(healBark);
        StartCoroutine(FlashWhiteDamage(2));
    }

    private void OnHealthChanged()
    {
        uiScript.HealthChange((int)curHealth);
        int healthState = Mathf.Clamp(curHealth, 1, healthStateSprite.Length - 1);
        defaultSprite=healthStateSprite[healthState];
        spriteRend.sprite = defaultSprite;
    }

    public void DoDamage(int damage)
    {
        spriteRend.transform.DOComplete();
        curHealth -= damage;
        curHealth = Mathf.Clamp(curHealth, 0, MaxHealth);
        OnHealthChanged();
        GameEngine.SetHitPause(15);
        stunTimer = .15f;
        spriteRend.transform.DOShakePosition(0.125f, 1, 10, 120);
        ScreenShake(2,.5f);
        StartCoroutine(FlashWhiteDamage(5));
        StartInvul(invulFlickerRate, 90f);
        state = PlayerState.stunned;
    }
    private IEnumerator LeapFrog(Vector3 destination)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0;
        while (elapsed<frogSpeed)
        {
            float t = elapsed / frogSpeed;
            aniMoveSpeed = 1;
            rb.MovePosition(Vector2.Lerp(startPos ,destination,t));
            elapsed += Time.deltaTime;
            yield return null;
        }

    }

    public void Hit(int dam,int effect,int bounceLvl, Vector2 dir)
    {
        if (isInvulnerable)
            return;
        DeSpell();
        if (dam>0)
        {
            DoDamage(dam);
            velocity = dir * moveSpeed;
            PlaySound(hurtBark);
        }
        SpellEffect(effect, bounceLvl);
        DamagePopup.Create(transform.position, dam, bounceLvl);
    }
    void SpellEffect(int effect, int level)
    {
        switch (effect)
        {
            case 0://Wound self damages instantly
                
                break;
            case 1://Skewer damages on contact
                break;
            case 2://Guardians applies shield
                ApplyGuardians(level);
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
    //public int numOfGuardians = 1;
    private List<GameObject> guardiansCreated= new List<GameObject>();

    public int CurHealth { get => curHealth; set => curHealth = value; }
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    public int StartingHealth { get => startingHealth; set => startingHealth = value; }

    void ApplyGuardians(int numOfGuardians)
    {
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
        PlaySound(ghostBark);
    }
    void ApplyPolymorph()
    {
        stunTimer = 3;
        state = PlayerState.frog;
        spriteRend.sprite = frogSprite;
        PlaySound(frogBark);
    }
    void ApplyFrost()
    {
        stunTimer = 3;//me being nice
        state = PlayerState.frozen;
        spriteRend.sprite = frozenSprite;
        PlaySound(frozenBark);
    }
    void DeSpell()
    {
        stunTimer = 0;
        spriteRend.sprite = defaultSprite;
        state = PlayerState.neutral;
    }
    public bool GetIsInvulnerable()
    {
        return isInvulnerable;
    }
    public void SetInvulCooldown(float iFrames)
    {
        if (invulCooldown < iFrames)
        {
            invulCooldown = iFrames;
        }
    }
    public void StartInvul(float hitFlash, float invulFrames)
    {
        if (invulCooldown <= 0)
        {
            invulCooldown = invulFrames;
            isInvulnerable = true;
        }
        StartCoroutine(FlashWhiteDamage(hitFlash));
        StartCoroutine(BlinkWhileInvulnerableCoroutine());
    }
    private IEnumerator FlashWhiteDamage(float hitFlash)
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
    private IEnumerator BlinkWhileInvulnerableCoroutine()
    {
        while (isInvulnerable)
        {
            spriteRend.material.SetColor("_SpriteColor", invulColor);
            spriteRend.material.SetFloat("_FlashAmt", 0.5f);
            for (int i = 0; i < invulFlickerRate; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            spriteRend.material.SetColor("_SpriteColor", Color.white);
            spriteRend.material.SetFloat("_FlashAmt", 0f);

            for (int i = 0; i < invulFlickerRate; i++)
            {
                yield return new WaitForFixedUpdate();
            }

        }
    }
    void ScreenShake(float amp, float time)
    {
        GameEngine.gameEngine.ShakeCamera(amp, time);
    }
    private void PlaySound(string sound)
    {
        if (sound != null)
        {
            audioManager.PlaySound(sound);
        }
    }
    public static Vector2 CardinalDir(Vector2 vector)
    {
        float angle = Mathf.Atan2(vector.y, vector.x);
        float hypotenuse = Mathf.Sqrt(vector.x * vector.x) + (vector.y * vector.y);
        float rad = Mathf.Deg2Rad*45;
        float snap = Mathf.Round(angle / rad) * rad;
        //int octant = Mathf.RoundToInt(45 * angle / (2 * Mathf.PI) + 45) % 45;
            Vector2 snappedVector = new Vector2(Mathf.Cos(snap)*hypotenuse,Mathf.Sin(snap)*hypotenuse);
        return snappedVector;
    }
}

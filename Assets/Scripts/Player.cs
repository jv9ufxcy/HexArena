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
    private int maxHealth;
    private int curHealth;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float frogSpeed = 0.375f;
    Vector2 moveAxis,lookAxis;
    [SerializeField] private PlayerControls controls;
    private InputAction move, aim, fire;
    [SerializeField]private bool lockRotation = false;

    [Header("Shooting")]
    [SerializeField] GameObject[] bullets;
    [SerializeField] private Transform weaponOffset,gunObject, firingPoint;
    [SerializeField] private float rotSpeed = 8f;
    private Ray ray;
    private RaycastHit rayHit;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private float trajectoryMaxLength = 12f;
    [SerializeField] private int reflections = 3;
    [SerializeField] private LayerMask target;
    [Header("Ammo")]
    [Range(0, 5)]
    public List<int> gunChamber = new List<int> { 1, 2, 3, 4, 5, 0 };
    [SerializeField] private UIManager uiScript;
    [Header("Spell Effects")]
    private float stunTimer, guardianTimer;
    [SerializeField] private GameObject[] magicObjects;
    private enum PlayerState { neutral, frozen,frog,stunned}
    [SerializeField]private PlayerState state;
    [Header("SoundEffects")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private string reloadBark = "Reload", fireBark = "Fire", hurtBark = "Hurt", deathBark = "Death", frozenBark = "Frozen", frogBark = "Frog", ghostBark = "Ghost";

    [Header("Effects")]
    [SerializeField] private SpriteRenderer spriteRend;
    [SerializeField] private Sprite defaultSprite,frozenSprite,frogSprite;
    [SerializeField] private Material defaultMat, flashMat;
    [SerializeField] private float yAmp = 0.1f, yFrq = 16f;
    [SerializeField] private float aniMoveSpeed;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        uiScript.AmmoUpdate(gunChamber);
        curHealth = maxHealth;
        controls = new PlayerControls();
        trajectoryLine=GetComponentInChildren<LineRenderer>();
    }
    private void OnEnable()
    {
        move = controls.Player.Move;
        move.Enable();

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
            if (curHealth <= 0)
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
            if (moveAxis != Vector2.zero && state == PlayerState.neutral&&!lockRotation)
            {
                RotateWeapon();
            }
            UpdateAnimator();
            if (lockRotation)
            {
                LineRend2DReflections(transform.position, weaponOffset.up);
            }
        }
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
    private static void Death()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Reload(int min,int max)
    {
        gunChamber.Clear();
        for (int i = 0; i < 6; i++)
        {
            int[] bulletArray = new int[]{ 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 4, 4, 4, 5, 5 };
            gunChamber.Add(bulletArray[Random.Range(0, bulletArray.Length)]);
            uiScript.AmmoUpdate(gunChamber);
            currentBullet = 0;
        }
        gunObject.DOPunchRotation(new Vector3(0, 0, 361), .25f);
        PlaySound(reloadBark);
    }
    private void RotateWeapon()
    {
        lookAxis = new Vector2(Mathf.Sin(moveAxis.x), Mathf.Sin(moveAxis.y));
        float angle = Mathf.Atan2(lookAxis.y, lookAxis.x) * Mathf.Rad2Deg;
        Quaternion newRot = Quaternion.Euler(0, 0, angle - 90f);
        weaponOffset.rotation = Quaternion.Slerp(transform.rotation, newRot, rotSpeed);
    }

    void UpdateAnimator()
    {
        Vector2 latSpeed = rb.velocity;
        aniMoveSpeed = Vector3.SqrMagnitude(latSpeed);
        if (aniMoveSpeed>0)
        {
            float yPos = Mathf.Sin(Time.time * yFrq) * yAmp;
            spriteRend.gameObject.transform.localPosition = new Vector3(0, yPos, 0);
        }
        else
            spriteRend.gameObject.transform.localPosition = Vector3.zero;

        var direction = Mathf.Sign(lookAxis.x);
        spriteRend.transform.localScale = new Vector3(direction, 1f, 1f);
        //gunObject.transform.localScale = new Vector3(direction, direction, 1f);

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
                    Reload(0, 6);
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
                    gunObject.DOPunchRotation(new Vector3(0, 0, 60f), 0.12f);
                    Vector3 punchScale = new Vector3(1.025f, 1.025f, 1);
                    gunObject.transform.DOPunchScale(punchScale, .25f,2,.125f);
                    //set bulletDirection
                    proj.GetComponent<Projectile>().ChangeDirection(lookAxis);
                    proj.GetComponent<Projectile>().ChangeOwner(this.gameObject);

                    uiScript.AmmoUpdate(gunChamber);
                    currentBullet++;

                    PlaySound(fireBark);
                }
            }
        }
    }
    public void DoHeal(int healthGain)
    {
        curHealth += healthGain;
        curHealth = Mathf.Clamp(curHealth, 0, maxHealth);
        uiScript.HealthChange((int)curHealth);
        StartCoroutine(FlashWhiteDamage(2));
    }    
    public void DoDamage(int damage)
    {
        spriteRend.transform.DOComplete();
        curHealth -= damage;
        curHealth = Mathf.Clamp(curHealth, 0, maxHealth);
        uiScript.HealthChange((int)curHealth);
        GameEngine.SetHitPause(15);
        stunTimer = .15f;
        spriteRend.transform.DOShakePosition(0.125f, damage, 10, 120);
        StartCoroutine(FlashWhiteDamage(5));
        state = PlayerState.stunned;
    }

    private void FixedUpdate()
    {
        if (GameEngine.hitStop <= 0)
        {
            switch (state)
            {
                case PlayerState.neutral:
                    rb.velocity = moveAxis.normalized * moveSpeed;
                    break;
                case PlayerState.frozen:
                    if (stunTimer > 0)
                    {
                        stunTimer -= Time.fixedDeltaTime;
                        rb.velocity = Vector2.zero;
                    }
                    else
                        DeSpell();
                    break;
                case PlayerState.frog:
                    if (stunTimer > 0)
                    {
                        rb.velocity = moveAxis.normalized * frogSpeed;
                        //Vector3 destination = transform.position + (Vector3)moveAxis;
                        //StartCoroutine(LeapFrog(destination));
                        stunTimer -= Time.fixedDeltaTime;
                    }
                    else
                        DeSpell();
                    break;
                case PlayerState.stunned:
                    if (stunTimer > 0)
                    {
                        stunTimer -= Time.fixedDeltaTime;
                    }
                    else
                        DeSpell();
                    break;
                default:
                    break;
            }
            //rb.MovePosition(rb.position + moveAxis.normalized * moveSpeed * Time.fixedDeltaTime);
        }
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
        DeSpell();
        if (dam>0)
        {
            DoDamage(dam);
            rb.velocity = dir * 8;
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
    private void PlaySound(string sound)
    {
        if (sound != null)
        {
            audioManager.PlaySound(sound);
        }
    }
}

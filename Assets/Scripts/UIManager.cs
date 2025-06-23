using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject player, ammoSlots;
    private Player playerScript;
    public Image healthDie;
    public List<Image> gunChambers, gunAmmoLoaded;
    private Color ammoColor;
    public List<int> gunChamberStorage;
    private int ammoValue, ammoSlot;
    public Sprite emptyChamber;
    [SerializeField]private Sprite[] loadedChamber;
    public List<Sprite> healthDice;

    private string currentSceneName;

    public TMP_Text gameOver;

    public void Start()
    {
        ammoSlot = 0;
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    public void HealthChange(int currentHealth)
    {
        if (currentHealth > 0)
            healthDie.sprite = healthDice[currentHealth - 1];
    }

    public void RotateBarrel(int currentShot)
    {
        gunChambers[currentShot].sprite = emptyChamber;
        ammoSlots.transform.rotation = Quaternion.Euler(0f, 0f, (currentShot + 1) * 60f);
    }

    public void AmmoUpdate(List<int> chamberValues)
    {
        gunChamberStorage = chamberValues;
        foreach (Image chamber in gunChambers)//empty 
        {
            chamber.sprite = emptyChamber;
        }
        foreach (int ammoValue in chamberValues)//load remaining ammo
        {
            gunChambers[ammoSlot].sprite = loadedChamber[ammoValue];
            ammoSlot++;
        }
        ammoSlot = 0;
    }
}

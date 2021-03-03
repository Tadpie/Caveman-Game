using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interact : MonoBehaviour
{

    public Camera playerView;

    public GameObject interactPrompt;
    public Text interactText;

    public GameObject weaponComparePrompt;
    public Text dmgCompareText;
    public Text spdCompareText;
    public Text crtCompareText;

    public Text potionCountText;
    public Text potionCooldownText;
    float defaultPotionCooldown = 6;
    float potionCooldown = 0;

    public LayerMask interactableObjectMask;

    public bool canChangeWeapon = false;
    public GameObject potentialWeapon;
    public GameObject currentWeapon;

    public bool canChangeShield = false;
    public GameObject potentialShield;
    public GameObject currentShield;

    private float reachRange = 8f;

    float currentWeaponStat;
    float potentialWeaponStat;

    int healthPotionCount = 0;

    public float pickupDelay = 0;
    public float defaultPickupDelay = 0.3f;

    GameObject currentArmor;

    PlayerController playerController;
    Armor armor;
    Weapon weapon;
    Door door;

	private void Start()
	{
        playerController = GetComponent<PlayerController>();
    }

	void Update()
    {
        // Potion cooldown text update
        if (pickupDelay > 0) pickupDelay -= Time.deltaTime;
        if (potionCooldown > 0)
        {
            potionCooldown -= Time.deltaTime;
            potionCooldownText.text = potionCooldown.ToString("F2");
        }
        else potionCooldownText.text = "";

        // System to knw what item player is interacting with
        RaycastHit hit;
        if (Physics.Raycast(playerView.transform.position, playerView.transform.forward, out hit, reachRange, interactableObjectMask))
        {
            interactPrompt.SetActive(true);
            interactText.text = "Interact with " + hit.transform.gameObject.name;
            interactText.enabled = true;

            switch (hit.transform.gameObject.tag)
            {
                case "Shortsword":
                    WeaponInteract(hit);
                    break;
                case "Sword":
                    WeaponInteract(hit);
                    break;
                case "Door":
                    DoorInteract(hit);
                    break;
                case "HealthPotion":
                    PotionInteract(hit);
                    break;
                case "LeatherArmor":
                    ArmorInteract(hit);
                    break;
                case "WoodShield":
                    ShieldInteract(hit);
                    break;
            }
        }
        else
        {
            interactPrompt.SetActive(false);
            interactText.enabled = false;
            canChangeWeapon = false;
            canChangeShield = false;
            
            weaponComparePrompt.SetActive(false);
        }

        // Use Health potion
        if (Input.GetKeyDown(KeyCode.H) && playerController.currentHealth < 100 && healthPotionCount > 0 && potionCooldown <= 0)
        {
            StartCoroutine(playerController.Heal(30, 0));
            potionCooldown = defaultPotionCooldown;
            healthPotionCount--;
            potionCountText.text = healthPotionCount.ToString();
        }
    }

    void DecideTextColor(Text text, float item1, float item2) // Changes item stat text color depending if it is better or worse than equipped
    {
        if (item1 > item2)
        {
            text.color = Color.red;
        }
        else if (item1 < item2)
        {
            text.color = Color.green;
        }
        else text.color = Color.black;
    }

    void WeaponInteract(RaycastHit hit)
    {
        // Note that the player is hovering over the potential item
        canChangeWeapon = true;
        potentialWeapon = hit.transform.gameObject;

        if (potentialWeapon != null)
        {
            // Get weapon scripts to view their respective stats, because we chack them multiple times
            Weapon potentialWeaponInfo = potentialWeapon.GetComponent<Weapon>();

            // Weapon damage compare
            if (currentWeapon) currentWeaponStat = currentWeapon.GetComponent<Weapon>().weaponDamage;
            else currentWeaponStat = 0;
            potentialWeaponStat = potentialWeaponInfo.weaponDamage;

            DecideTextColor(dmgCompareText, currentWeaponStat, potentialWeaponStat);
            dmgCompareText.text = " DMG: " + currentWeaponStat + " -> " + potentialWeaponStat;

            // Weapon speed compare
            if (currentWeapon) currentWeaponStat = currentWeapon.GetComponent<Weapon>().weaponSpeed;
            else currentWeaponStat = 0;
            potentialWeaponStat = potentialWeaponInfo.weaponSpeed;

            DecideTextColor(spdCompareText, currentWeaponStat, potentialWeaponStat);
            spdCompareText.text = " SPD: " + currentWeaponStat + " -> " + potentialWeaponStat;

            // Weapon Crit chance compare
            if (currentWeapon) currentWeaponStat = currentWeapon.GetComponent<Weapon>().critChance;
            else currentWeaponStat = 0;
            potentialWeaponStat = potentialWeaponInfo.critChance;

            DecideTextColor(crtCompareText, currentWeaponStat, potentialWeaponStat);
            crtCompareText.text = " CRT: " + currentWeaponStat + "% -> " + potentialWeaponStat + "%";

            // Enable compare prompt
            weaponComparePrompt.SetActive(true);
        }
    }

    void DoorInteract(RaycastHit hit)
    {
        Door door = hit.transform.gameObject.GetComponent<Door>();

        // If door is closed, it can be opened by pressing E
        if(door.openDoor == false)
		{
            if(Input.GetKeyDown(KeyCode.E))
			{
                door.openDoor = true;
			}
		}
    }
    void PotionInteract(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E) && pickupDelay <= 0)
        {
            Destroy(hit.transform.gameObject);
            healthPotionCount++;
            potionCountText.text = healthPotionCount.ToString();
            pickupDelay = defaultPickupDelay;
        }
    }
    void ArmorInteract(RaycastHit hit)
    {
        if (currentArmor) currentWeaponStat = currentArmor.GetComponent<Armor>().defense;
        else currentWeaponStat = 0;
        potentialWeaponStat = hit.transform.gameObject.GetComponent<Armor>().defense;

        // Dsiplay weapon stats and colours if the player is hovering over it
        DecideTextColor(dmgCompareText, currentWeaponStat, potentialWeaponStat);
        dmgCompareText.text = " DEF: " + currentWeaponStat + " -> " + potentialWeaponStat;

        spdCompareText.text = "";
        crtCompareText.text = "";

        weaponComparePrompt.SetActive(true);

        // Equip armor
        if (Input.GetKeyDown(KeyCode.E) && pickupDelay <= 0)
        {
            if (currentArmor != null)
            {
                currentArmor.transform.localPosition = hit.transform.localPosition;
                currentArmor.SetActive(true);
            }
            currentArmor = hit.transform.gameObject;
            currentArmor.SetActive(false);
            playerController.defense = currentArmor.GetComponent<Armor>().defense;
            pickupDelay = defaultPickupDelay;
        }
    }
    void ShieldInteract(RaycastHit hit)
    {
        // Note that the player is hovering over the potential item
        canChangeShield = true;
        potentialShield = hit.transform.gameObject;

        // Dsiplay weapon stats and colours
        if (potentialShield != null)
        {
            if(currentShield) currentWeaponStat = currentShield.GetComponent<Armor>().defense;
            else currentWeaponStat = 0;
            potentialWeaponStat = potentialShield.GetComponent<Armor>().defense;

            DecideTextColor(dmgCompareText, currentWeaponStat, potentialWeaponStat);
            dmgCompareText.text = " DEF: " + currentWeaponStat + " -> " + potentialWeaponStat;

            spdCompareText.text = "";
            crtCompareText.text = "";

            weaponComparePrompt.SetActive(true);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainhandController : MonoBehaviour
{
    private Animator animator;
    private GameObject weapon;
    private bool canChangeWeapon;
    private GameObject potentialWeapon;

    void Start()
    {
        //get weapon animation controller
        animator = GetComponent<Animator>();
        //trailRender = GetComponentInChildren<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get current weapon
        if(gameObject.transform.childCount > 0) 
        {
            weapon = gameObject.transform.GetChild(0).gameObject;
            weapon.GetComponent<Rigidbody>().isKinematic = true;
            weapon.layer = LayerMask.NameToLayer("WeaponEquipped");
            GetComponentInParent<Interact>().currentWeapon = weapon;
        }

        // Drop weapon
        if(Input.GetKey(KeyCode.G) && weapon) 
        {
            DropCurrentWeapon();
        }

        canChangeWeapon = GetComponentInParent<Interact>().canChangeWeapon;
        if (canChangeWeapon) potentialWeapon = GetComponentInParent<Interact>().potentialWeapon;

        if(Input.GetKey(KeyCode.E) && canChangeWeapon && GetComponentInParent<Interact>().pickupDelay <= 0)
		{
            if(weapon) DropCurrentWeapon();
            potentialWeapon.transform.parent = this.gameObject.transform;
            potentialWeapon.transform.localPosition = new Vector3(0, 0, 0);
            potentialWeapon.transform.localRotation = Quaternion.identity;
            GetComponentInParent<Interact>().currentWeapon = potentialWeapon;
            GetComponentInParent<Interact>().pickupDelay = GetComponentInParent<Interact>().defaultPickupDelay;
        }

        // Get the currently playing animation
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        // Attack
        if (info.IsName("Idle") && Input.GetButtonDown("Fire1") && weapon)
        {
            // Attack depending on weapon type
            switch(weapon.tag)
            {
                case "Sword": animator.SetBool("SwordAttack", true);
                    break;
                case "Shortsword": animator.SetBool("DaggerAttack", true);
                    break;
            }
        }
        // Reset attack animation
        else if(info.IsName("Idle")) 
        {
            animator.SetBool("SwordAttack", false);
            animator.SetBool("DaggerAttack", false);
        }
    }

    void DropCurrentWeapon()
	{
        GetComponentInParent<Interact>().currentWeapon = null;
        weapon.layer = LayerMask.NameToLayer("Weapon");
        weapon.GetComponent<Rigidbody>().isKinematic = false;
        weapon.transform.parent = null;
        weapon = null;
    }
}

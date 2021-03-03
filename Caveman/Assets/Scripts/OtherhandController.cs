using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherhandController : MonoBehaviour
{
    private Animator animator;
    private GameObject shield;
    private bool canChangeShield;
    private GameObject potentialShield;

    int finalDefense;
    float ogSpeed;
    float ogSprintSpeed;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.childCount > 0)
        {
            shield = gameObject.transform.GetChild(0).gameObject;
            shield.GetComponent<Rigidbody>().isKinematic = true;
            shield.layer = LayerMask.NameToLayer("ShieldEquipped");
            GetComponentInParent<Interact>().currentShield = shield;
        }

        if (Input.GetKey(KeyCode.G) && shield)
        {
            DropCurrentShield();
        }

        canChangeShield = GetComponentInParent<Interact>().canChangeShield;
        if (canChangeShield) potentialShield = GetComponentInParent<Interact>().potentialShield;

        if (Input.GetKey(KeyCode.E) && canChangeShield && GetComponentInParent<Interact>().pickupDelay <= 0)
        {
            if (shield) DropCurrentShield();
            potentialShield.transform.parent = this.gameObject.transform;
            potentialShield.transform.localPosition = new Vector3(0, 0, 0);
            potentialShield.transform.localRotation = Quaternion.identity;
            GetComponentInParent<Interact>().currentShield = potentialShield;
            GetComponentInParent<Interact>().pickupDelay = GetComponentInParent<Interact>().defaultPickupDelay;
        }

        if(shield)
		{
            // Get the currently playing animation
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

            if (Input.GetButton("Fire2"))
            {
                animator.SetBool("Block", true);
                GetComponentInParent<PlayerController>().shieldActive = true;
            }
            else
            {
                animator.SetBool("Block", false);
                GetComponentInParent<PlayerController>().shieldActive = false;
            }
        }
    }

    void DropCurrentShield()
    {
        GetComponentInParent<Interact>().currentShield = null;
        shield.layer = LayerMask.NameToLayer("Shield");
        shield.GetComponent<Rigidbody>().isKinematic = false;
        shield.transform.parent = null;
        shield = null;
    }
}

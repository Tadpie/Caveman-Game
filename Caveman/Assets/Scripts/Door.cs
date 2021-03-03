using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool openDoor = false;

    Animator animator;

    BoxCollider doorCollider;

    void Start()
    {
        animator = GetComponentInParent<Animator>();
        doorCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (openDoor)
        {
            animator.SetBool("DoorOpen", true);
            gameObject.layer = LayerMask.NameToLayer("Walls or Ground");
            doorCollider.enabled = false;
        }
    }
}

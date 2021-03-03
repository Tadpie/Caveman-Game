using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float weaponDamage;
    public float weaponSpeed;
	public float critChance;

	Animator animator;

	private void Update()
	{
		if (GetComponentInParent<Animator>() != null)
		{
			animator = GetComponentInParent<Animator>();
			animator.speed = weaponSpeed;
		}
	}
}

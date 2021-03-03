using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    public CharacterController controller;

    float speed = 12f;//(default: 12f)
    float sprintSpeed = 16f;
    float jumpHeight = 3f;//(default: 3f)
    float gravity = -50f;//(default: -50f)
    float groundDistance = 0.08f;//(default: 1f)
    float ceilingDistance = 0.04f;//(default: 0.5f)

    //gameobject to check if player is on ground or on the ceiling
    public Transform groundCheck;
    public Transform ceilingCheck;

    //layer mask to determine if the interactable model is ground or a ceiling
    public LayerMask groundMask;
    public LayerMask ceilingMask;
    Vector3 move;

    public bool isGrounded; //boolean to check if player's feet are touching the ground
    
    //check if player is hitting ceiling
    public bool isHittingCeiling;

    //falling velocity
    public Vector3 velocity;
    public Vector3 lastVelocity;

    //variables to help determine if the player is sprinting
    public bool isSprinting = false;

    //initial player values to reset them after changing
    float originalControllerSpeed;
    float originalControllerHeight;

    private Animator animator;

    private readonly int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;
    public int healIterations;
    
    public Image damageSplashImage;

    public GameObject bloodParticle;

    public int defense = 0;

    public bool shieldActive = false;

    float x;
    float z;

    void Start()
    {
        //save the original players values for future reference
        originalControllerHeight = controller.height;
        originalControllerSpeed = speed;
        //get camera animation controller
        animator = GetComponentInChildren<Animator>();
        
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        damageSplashImage.enabled = false;
    }

    void Update()
    {
        InputChecks();
        Jump();
        Sprint();
        Move();
    }

    private void InputChecks()
    {
        //check if player is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        //check if player is hitting the ceiling
        isHittingCeiling = Physics.CheckSphere(ceilingCheck.position, ceilingDistance, ceilingMask);

        //get inputs
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        //define move direction depending on inputs x and z
        move = transform.right * x + transform.forward * z;
    }
    
    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !shieldActive)
        {
            //save your velocity
            lastVelocity = move;
            //jump
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }

        if (!isGrounded)
        {
            //disables stepOffset and slopelimit when not on ground because so we cannot climb up a stair while falling
            controller.slopeLimit = 0f;
            controller.stepOffset = 0f;
            //just to make it feel more natural, disble animation when in the air
            animator.SetBool("Walking", false);
            animator.SetBool("Running", false);
        }
        else
        {
            //when on ground enable step offset and slopelimit
            controller.slopeLimit = 45;
            controller.stepOffset = 0.4f;
        }

        //if player hits ceiling, switch to falling velocity
        if (isHittingCeiling) velocity.y = -2.0f;

        //reset velocity
        if (isGrounded && velocity.y < 0) velocity.y = -2.0f;
    }

    private void Sprint()
    {
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded && !shieldActive && z > 0)
        {
            //sprint
            isSprinting = true;
            speed = sprintSpeed;
            //start playing sprint animation
            if(move.magnitude > 0) 
            {
                animator.SetBool("Running", true);
                animator.SetBool("Walking", false);
            }
            else animator.SetBool("Running", false);
        }
        else if (isGrounded && !shieldActive)
        {
            //stop sprinting
            isSprinting = false;
            speed = originalControllerSpeed;
            //stop playing sprint animation
            //animator.SetBool("IsSprinting", false);
            if(move.magnitude > 0) 
            {
                animator.SetBool("Running", false);
                animator.SetBool("Walking", true);
            }
            else animator.SetBool("Walking", false);
        }
        if(shieldActive)
		{
            animator.SetBool("Walking", false);
            animator.SetBool("Running", false);
        }
    }

    private void Move()
    {
        //always change move speed to its maximum value (to counter diagnal speed problem)
        if (move.magnitude >= 1.0f) move.Normalize();
        if (lastVelocity.magnitude >= 1.0f) lastVelocity.Normalize();

        //move the player, if the player is not touching the ground, you can only control its movements at 70% power, but you also generate an extra 
        //10% more movement from jumping
        if (!isGrounded)
        {
            controller.Move(lastVelocity * 0.4f * speed * Time.deltaTime);
            controller.Move(move * 0.7f * speed * Time.deltaTime);
        }
        else if(!shieldActive) controller.Move(move * speed * Time.deltaTime); // commenting this gives an interesting mechanic, where you only move by jumping
        else controller.Move(move * 0.2f * speed * Time.deltaTime);

        //velocity calculation
        velocity.y += gravity * Time.deltaTime;

        //applying velocity (an extra Time.deltaTime because its based on a physics formula)
        controller.Move(velocity * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        if(shieldActive) defense = defense + GetComponentInChildren<Armor>().defense;

        damage = damage - defense;

        if (shieldActive) defense = defense - GetComponentInChildren<Armor>().defense;

        if (damage < 1) damage = 1;

        currentHealth -= damage;
        if(damageSplashImage != null) StartCoroutine(DamageSplash());
        healthBar.SetHealth(currentHealth);
        if (currentHealth <= 0) Death();
    }
    /*
    public void Heal(int heal)
    {
        currentHealth += heal;
        if (currentHealth > healthBar.GetHealth()) currentHealth = (int)healthBar.GetHealth();
        healthBar.SetHealth(currentHealth);
    }
    */
    IEnumerator DamageSplash()
	{
        damageSplashImage.enabled = true;
        bloodParticle = Instantiate(bloodParticle, transform.position, Quaternion.identity);
        yield return new WaitForFixedUpdate();
        damageSplashImage.enabled = false;
    }

    void Death()
	{
        SceneManager.LoadScene("Menu");
        Cursor.lockState = CursorLockMode.None;
    }

    public IEnumerator Heal(int healAmount, float healthPerHalfSecond)
	{
        // Heal over time as opposed to instantly
        if (healthPerHalfSecond == 0) healthPerHalfSecond = 2.5f;
        healIterations = Mathf.RoundToInt((healAmount / healthPerHalfSecond));

        while (healIterations > 0 && currentHealth != 100)
		{
            healIterations--;
            currentHealth += Mathf.RoundToInt(healthPerHalfSecond);
            if (currentHealth > healthBar.GetHealth()) currentHealth = Mathf.RoundToInt(healthBar.GetHealth());
            healthBar.SetHealth(currentHealth);
            yield return new WaitForSeconds(0.5f);
        }
        healIterations = 0;
	}
}
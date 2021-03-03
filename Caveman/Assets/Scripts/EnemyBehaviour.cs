using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


/// <summary>
/// Answer by Mike 3 at https://answers.unity.com/questions/36255/lookat-to-only-rotate-on-y-axis-how.html helped with object rotations only on y axis.
/// </summary>


public class EnemyBehaviour : MonoBehaviour
{
    public GameObject player;
    public float health = 40f;
    public int damage = 25;
    public GameObject bloodParticle;

    //Boss things
    public GameObject bossWand;
    public GameObject projectile;
    public Slider healthSlider;

    //Drops
    Items items;

    Animator animator;
    NavMeshAgent agent;

    PlayerController playerController;

    public float meleeRadius = 4.5f;
    float extendedMeleeRadius;
    public float lookRadius = 30f;
    float attackDelay;
    bool aggro = false;
    float castDelay;

    // Approximate length of animation until attack
	private float defaultAttackDelay = 0.96f;

    AudioSource audioSource;
    public AudioClip audioClip;
    public GameObject deadBody;

    void Start()
    {
        // Get components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        playerController = player.GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        items = GameObject.Find("RefPad").GetComponent<Items>();

        // Set the attack delay, that way the first attaack doesnt insta-hit
        attackDelay = defaultAttackDelay;

        extendedMeleeRadius = meleeRadius + meleeRadius * 0.8f;
    }

    void Update()
    {
        // Get currently playing animation
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        // Make the asset to always face the player - only rotate on y axis
        var lookPos = player.transform.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1f);

        // Get distance from player
        float distance = Vector3.Distance(player.transform.position, transform.position);

        // Get player direction from goblin
        Vector3 direction = player.transform.position - transform.position;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction.normalized, out hit, lookRadius) && aggro == false)
        {
            //aggro if the player is seen
            //Debug.DrawRay(transform.position, direction.normalized * 100000f, Color.red, 20f);
            if (hit.collider.gameObject == player)
            {
                aggro = true;
            }
        }

        // Calculate by distance whether to walk towards the player or melee attack
        if (aggro && distance > meleeRadius)
        {
            if (gameObject.tag == "Hobgoblin") healthSlider.gameObject.SetActive(true);

            castDelay -= Time.deltaTime;

            agent.SetDestination(player.transform.position);
            animator.SetBool("Walk", true);
            animator.SetBool("Attack", false);
            attackDelay = defaultAttackDelay;

            if (castDelay <= 0)
            {
                ShootProjectile();
                castDelay = 4f;
            }
        }
        else if (distance <= meleeRadius)// || distance <= extendedMeleeRadius && info.IsName("GoblinAttack"))
        {
            attackDelay -= Time.deltaTime;

            if(distance <= meleeRadius * 0.6f) agent.SetDestination(transform.position);
            animator.SetBool("Attack", true);
            animator.SetBool("Walk", false);

            // Shoot raycast forward to deal damage to the player
            RaycastHit hit1;

            //Debug.DrawRay(transform.position + Vector3.down, transform.rotation * Vector3.forward, Color.green, extendedMeleeRadius);
            if (Physics.Raycast(transform.position + Vector3.down, transform.rotation * Vector3.forward, out hit1, extendedMeleeRadius))
            {
                if (hit1.collider.gameObject == player && attackDelay <= 0 && info.IsName("GoblinAttack"))
                {
                    playerController.TakeDamage(damage);
                    attackDelay = defaultAttackDelay;
                }
            }
        }
        else
        {
            attackDelay = defaultAttackDelay;
        }
    }

	private void OnTriggerEnter(Collider collision)
    {
        
        // How many times you can hit per one collision enter
        int hitPossibilities = 1;

        
        // Check if collision invader is a weapon
        if (collision.transform.gameObject.layer == LayerMask.NameToLayer("WeaponEquipped"))
        {
            
            // I don't know either, supposed to check if the player attack animation is playing
            bool playerAttackCondition = collision.transform.gameObject.GetComponentInParent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("DaggerAttack") ||
                collision.transform.gameObject.GetComponentInParent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("SwordAttack");

            if (playerAttackCondition && hitPossibilities > 0)
            {
                // Reset Conditions
                hitPossibilities--;
                playerAttackCondition = false;

                // Get weapon stats
                Weapon weapon = collision.transform.gameObject.GetComponent<Weapon>();

                // Roll a number for crit calculation
                float rand = Random.Range(0, 100);
                // Get weapon damage
                float damage = weapon.weaponDamage;

                // Critical calculator
                if (rand < weapon.critChance)
                {
                    damage = damage * 2;
                }

                TakeDamage(damage);
            }
        }
    }

    void TakeDamage(float damage)
    {
        health = health - damage;
        audioSource.PlayOneShot(audioSource.clip);
        if(gameObject.tag == "Hobgoblin") healthSlider.value = health;
        bloodParticle = Instantiate(bloodParticle, transform.position + Vector3.down, Quaternion.identity);

        // Death
        if (health <= 0)
        {
            Die();
            if (gameObject.tag == "Hobgoblin") healthSlider.gameObject.SetActive(false);
        }
    }

    void Die()
	{
        GameObject body = Instantiate(deadBody, transform.position, Quaternion.identity);
        body.GetComponent<AudioSource>().PlayOneShot(audioClip);
        Destroy(gameObject);

        if (gameObject.tag != "Hobgoblin")
        {
            RollDice(35, items.shortsword);
            RollDice(10, items.healthPotion);
            RollDice(6, items.sword);
            RollDice(4, items.ironShield);
            RollDice(3, items.ironArmor);
        }
        else
        {
            RollDice(70, items.trident);
            RollDice(70, items.katana);
        }
    }

    void RollDice(int chance, GameObject drop)
    {
        float rand = Random.Range(0, 100);
        if (rand <= chance)
        {
            GameObject item = Instantiate(drop, transform.position, Quaternion.LookRotation(Vector3.up));
            item.name = drop.name;
        }
    }

    void ShootProjectile()
	{
        if (gameObject.transform.tag == "Hobgoblin")
        {
            GameObject prj = Instantiate(projectile, bossWand.transform.position, Quaternion.identity);
            prj.GetComponent<Projectile>().speed = 30f;
        }
    }
}

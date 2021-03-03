using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject player;

    public float speed = 20f;
    int damage = 15;

    Transform target;
    Transform startPos;
    
    Vector3 dir;

	void Start()
	{
        // Get player position and direction at the start of projectiles lifetime
        Transform target = player.transform;
        startPos = transform;
        dir = target.position - startPos.position;
    }

	void Update()
    {
        // Make the asset to always face the player - only rotate on y axis
        var lookPos = player.transform.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1f);

        // Normalize resultant vector to unit Vector
        dir = dir.normalized;
        // Move in the direction of the direction vector every frame 
        gameObject.transform.position += dir * Time.deltaTime * speed;
    }

	private void OnTriggerEnter(Collider other)
	{
        // Deal damage to player
        if (other.gameObject.tag != "Hobgoblin")
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                other.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
            }

            Destroy(gameObject);
        }
	}
}

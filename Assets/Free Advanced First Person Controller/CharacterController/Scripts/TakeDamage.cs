using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FPC
{
    public class TakeDamage : MonoBehaviour
    {
        public float damageAmount = 20f; // The amount of damage dealt to the player on collision

        private void OnCollisionEnter(Collision collision)
        {
            // Check if the colliding object is the player
            if (collision.gameObject.CompareTag("Player"))
            {
                // Try to get the PlayerHealth component on the player
                var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // If the player has a PlayerHealth component, apply damage
                    playerHealth.Damage(damageAmount);
                }
            }
        }
    }
}
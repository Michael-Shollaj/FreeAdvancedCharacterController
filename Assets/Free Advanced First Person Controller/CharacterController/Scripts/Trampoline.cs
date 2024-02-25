using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FPC
{
    public class Trampoline : MonoBehaviour
    {
        public float jumpForce = 20f; // The upward force applied when jumping on the trampoline

        private void OnCollisionEnter(Collision collision)
        {
            // Check if the colliding object is the player
            if (collision.gameObject.CompareTag("Player"))
            {
                // Apply an upward force to the player
                Rigidbody playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
                if (playerRigidbody != null)
                {
                    // Set the player's vertical velocity to 0 before applying the trampoline force to ensure consistent jump height
                    Vector3 velocity = playerRigidbody.velocity;
                    velocity.y = 0;
                    playerRigidbody.velocity = velocity;

                    // Apply the jump force
                    playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                }
            }
        }
    }
}
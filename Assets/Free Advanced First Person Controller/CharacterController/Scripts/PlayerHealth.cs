using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace FPC
{
    public class PlayerHealth : MonoBehaviour
    {
        public Slider healthSlider;
        public Image bloodOverlay; // Reference to the blood overlay image
        public float regenRate = 5f;
        public float regenDelay = 5f;
        private float timeSinceLastDamage = 0f;
        public float minimumFallSpeedForDamage = 10f;
        public float fallDamageMultiplier = 2f;

        float health, maxHealth = 100;

        private void Start()
        {
            health = maxHealth;
            InitializeHealthSlider();
            StartCoroutine(HealthRegenCheck());
        }

        void InitializeHealthSlider()
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
            UpdateBloodOverlay(); // Ensure the blood overlay is updated on start
        }

        void Update()
        {
            if (health < maxHealth)
            {
                timeSinceLastDamage += Time.deltaTime;
                UpdateBloodOverlay(); // Call this in Update to continuously check for health changes
            }
        }

        public void Damage(float damagePoints)
        {
            if (health > 0)
            {
                health -= damagePoints;
                timeSinceLastDamage = 0;
                InitializeHealthSlider();
                CheckDeath();
            }
        }

        void CheckDeath()
        {
            if (health <= 0)
            {
                Debug.Log("Player has died.");
                StartCoroutine(RestartGameAfterDelay(1)); // Restart the game after a 2-second delay
            }
        }

        IEnumerator RestartGameAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }


        public void Heal(float healingPoints)
        {
            if (health < maxHealth)
            {
                health += healingPoints;
                InitializeHealthSlider();
            }
        }

        IEnumerator HealthRegenCheck()
        {
            while (true)
            {
                if (timeSinceLastDamage >= regenDelay && health < maxHealth)
                {
                    Heal(regenRate * Time.deltaTime); // Use Heal function to simplify
                }
                yield return null;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Check if the collision is with the ground
            if (collision.gameObject.tag == "Ground")
            {
                // Access the FirstPersonMovement component
                var playerMovement = GetComponent<CharacterController>();
                // Check if the player is not swimming
                if (!playerMovement.isSwimming)
                {
                    var fallSpeed = Mathf.Abs(collision.relativeVelocity.y);
                    if (fallSpeed > minimumFallSpeedForDamage)
                    {
                        var damage = (fallSpeed - minimumFallSpeedForDamage) * fallDamageMultiplier;
                        Damage(damage);
                    }
                }
            }
        }


        void UpdateBloodOverlay()
        {
            // Make the blood overlay more visible as health decreases. Below 15 health, make it fully visible.
            float alpha = health <= 15 ? 1.0f : 1.0f - (health / maxHealth);
            bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, alpha);
        }


    }
}
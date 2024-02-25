using UnityEngine;

namespace FPC
{
    public class ZoneSpeedAffector : MonoBehaviour
    {
        public float speedMultiplier = 0.5f; // Example: half the speed

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) // Make sure the player tag is set correctly
            {
                CharacterController playerMovement = other.GetComponent<CharacterController>();
                if (playerMovement != null)
                {
                    playerMovement.ModifySpeedMultiplier(speedMultiplier);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                CharacterController playerMovement = other.GetComponent<CharacterController>();
                if (playerMovement != null)
                {
                    playerMovement.ResetSpeedMultiplier();
                }
            }
        }
    }
}
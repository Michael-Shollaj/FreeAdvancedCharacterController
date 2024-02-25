using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FPC
{
    public class Water : MonoBehaviour
    {
        public Volume groundVolume;
        public Volume waterVolume;

        CharacterController movement;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && other.GetComponent<CharacterController>() != null)
            {
                movement = other.GetComponent<CharacterController>();
                movement.isSwimming = true;
                movement.swimSpeed = 3;

                // Enable water volume and disable ground volume
                if (waterVolume != null) waterVolume.weight = 1;
                if (groundVolume != null) groundVolume.weight = 0;
            }
            if (other.CompareTag("Head"))
            {
                movement.ResetVelocity();
                other.GetComponentInParent<CharacterController>().isHeadUnderwater = true;
                other.GetComponentInParent<Rigidbody>().useGravity = false;
                RenderSettings.fog = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && other.GetComponent<CharacterController>() != null)
            {
                movement = other.GetComponent<CharacterController>();
                movement.isSwimming = false;

                // Enable ground volume and disable water volume
                if (groundVolume != null) groundVolume.weight = 1;
                if (waterVolume != null) waterVolume.weight = 0;
            }
            if (other.CompareTag("Head"))
            {
                movement.ResetVelocity();
                other.GetComponentInParent<CharacterController>().isHeadUnderwater = false;
                other.GetComponentInParent<Rigidbody>().useGravity = true;
                RenderSettings.fog = false;
            }
        }
    }
}
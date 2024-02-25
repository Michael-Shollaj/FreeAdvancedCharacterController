//Thanks for downloading Advanced Free Character Controller, I hope you find it useful :)
//Mihal Shollaj


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FPC
{
    public class CharacterController : MonoBehaviour
    {
        [Header("Footstep Audio Settings")]
        public float walkFootstepPitch = 1.0f;
        public float sprintFootstepPitch = 1.5f;

        [Header("Audio")]
        public AudioClip footstepClip;
        public AudioClip jumpClip;
        public AudioClip crouchClip;
        public AudioClip dashClip;
        bool isMoving;
        private AudioSource audioSource;

        // Swimming
        [HideInInspector]
        public bool isSwimming;
        public float swimSpeed;
        public Transform target;

        public bool isHeadUnderwater = false;

        public float speed = 5;

        [Header("Climbing")]
        public bool isClimbing = false;
        public float climbSpeed = 3f;
        private bool nearLadder = false;
        private float verticalInput;

        [Header("Stamina")]
        public float maxStamina = 100f;
        public float stamina = 100f;
        public float staminaDecreasePerSecond = 10f;
        public float staminaRegenPerSecond = 5f;
        public Slider staminaSlider; // Assign in the inspector
        private float staminaDepletedTime = -1f; // Time when stamina was last depleted
        private float staminaRegenDelay = 5f; // Delay in seconds before stamina starts regenerating

        private bool isSprinting = false;
        [Header("Running")]
        public bool canRun = true;
        public bool IsRunning { get; private set; }
        public float runSpeed = 9;
        public KeyCode runningKey = KeyCode.LeftShift;

        private Rigidbody rigidbody;

        [Header("Dashing")]
        public KeyCode dashKey = KeyCode.LeftControl; // Example key for dashing
        public float dashSpeed = 20f; // Speed increase during dash
        public float dashDuration = 0.2f; // How long the dash lasts
        public float dashCooldown = 2f; // Cooldown time between dashes in seconds
        private bool isDashing = false; // Is player currently dashing
        private float dashEndTime = -1f; // When the dash should end
        private float lastDashTime = -Mathf.Infinity; // Last time the dash was activated
        private Vector3 dashDirection; // Direction of the dash
        public bool canDash = true;

        [Header("Head Bobbing")]
        public Camera playerCamera;
        public float bobFrequency = 5f;
        public float bobHorizontalAmplitude = 0.1f;
        public float bobVerticalAmplitude = 0.1f;
        private float defaultYPos = 0;
        private float timer = 0;

        [Header("Jumping")]
        public float jumpForce = 5f;
        public float doubleJumpForce = 3f;
        public LayerMask groundLayer; // Layer used to identify the ground
        public Transform groundCheck; // A point where to check if player is grounded
        public float groundDistance = 0.4f; // Distance to check for ground
        private bool canDoubleJump = false; // Tracks if the player can perform a double jump

        private bool isGrounded;

        [Header("Crouching")]
        public bool isCrouching = false;
        public KeyCode crouchKey = KeyCode.C;
        public float crouchScale = 0.5f; // Scale of the player when crouching
        public float crouchSpeed = 2.5f; // Optional: Reduced speed when crouching
        private Vector3 originalScale; // To store the original scale of the player
        public bool canCrouch = true;

        /// <summary> Functions to override movement speed. Will use the last added override. </summary>
        public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            if (playerCamera != null)
            {
                defaultYPos = playerCamera.transform.localPosition.y;
            }
            originalScale = transform.localScale; // Store the original scale
        }

        void Update()
        {
            isMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer, QueryTriggerInteraction.Ignore);

            verticalInput = Input.GetAxis("Vertical");

            // Climbing logic
            if (nearLadder && Mathf.Abs(verticalInput) > 0f)
            {
                isClimbing = true;
            }
            else if (!nearLadder || isGrounded)
            {
                isClimbing = false;
            }

            // Jumping logic
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isGrounded || (isSwimming && !isHeadUnderwater))
                {
                    Jump(jumpForce);
                    canDoubleJump = true;
                }
                else if (canDoubleJump)
                {
                    Jump(jumpForce * doubleJumpForce);
                    canDoubleJump = false;
                }
            }

            HandleCrouching();

            // Dashing logic
            if (Input.GetKeyDown(dashKey) && !isDashing && canDash && Time.time >= lastDashTime + dashCooldown)
            {
                StartDash();
            }
            if (isDashing && Time.time >= dashEndTime)
            {
                isDashing = false;
            }

            // Sprinting and stamina logic
            IsRunning = Input.GetKey(runningKey);
            if (IsRunning && isMoving && stamina > 0)
            {
                isSprinting = true;
                stamina -= staminaDecreasePerSecond * Time.deltaTime;
                if (stamina < 0)
                {
                    stamina = 0;
                }
                staminaDepletedTime = Time.time; // Keep updating while depleting stamina
            }
            else
            {
                isSprinting = false;
            }

            // Stamina regeneration
            if (!isSprinting && Time.time - staminaDepletedTime >= staminaRegenDelay && stamina < maxStamina)
            {
                stamina += staminaRegenPerSecond * Time.deltaTime;
                if (stamina > maxStamina)
                {
                    stamina = maxStamina;
                }
            }

            UpdateStaminaUI();
            HandleFootsteps();
        }

        void HandleFootsteps()
        {
            // Check if the player is moving and grounded
            if (isMoving && isGrounded)
            {
                // If not already playing a footstep sound or if the current clip isn't the footstep clip, set it
                if (!audioSource.isPlaying || audioSource.clip != footstepClip)
                {
                    audioSource.clip = footstepClip;
                    audioSource.loop = true; // Ensure the footstep sound loops
                    audioSource.Play();
                }

                // Adjust the pitch based on whether the player is sprinting
                audioSource.pitch = IsRunning ? sprintFootstepPitch : walkFootstepPitch;
            }
            else
            {
                // If the player stops moving or is not grounded, stop the footstep sound
                if (audioSource.clip == footstepClip)
                {
                    audioSource.loop = false;
                    audioSource.Stop();
                }
            }
        }


        void FixedUpdate()
        {
            if (!isSwimming)
            {
                if (isClimbing)
                {
                    HandleClimbing();
                }
                else
                {
                    HandleMovement();
                }
            }
            else
            {
                HandleSwimming();
            }
        }

        void HandleClimbing()
        {
            rigidbody.useGravity = !isClimbing; // Disable gravity when climbing

            if (isClimbing)
            {
                rigidbody.velocity = new Vector3(0, verticalInput * climbSpeed, 0);
            }
        }

        public void ModifySpeedMultiplier(float multiplier)
        {
            speed *= multiplier; // Adjust speed based on the multiplier
            runSpeed *= multiplier; // Also adjust run speed if necessary
        }

        public void ResetSpeedMultiplier()
        {
            speed = 5; // Reset to default speed
            runSpeed = 9; // Reset to default run speed
        }

        void HandleMovement()
        {
            if (isDashing)
            {
                // Apply dash movement
                rigidbody.velocity = dashDirection * dashSpeed;
            }
            else
            {
                // Update IsRunning based on input and stamina level.
                IsRunning = canRun && Input.GetKey(runningKey) && stamina > 0; // Ensure player has stamina to run

                // Calculate target moving speed based on whether the player is running and has stamina.
                float targetMovingSpeed;
                if (IsRunning && stamina > 0)
                {
                    targetMovingSpeed = runSpeed; // Use run speed if running and has stamina
                }
                else
                {
                    targetMovingSpeed = speed; // Use normal speed otherwise
                }

                if (speedOverrides.Count > 0)
                {
                    targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
                }

                // Get target velocity from input.
                Vector2 targetVelocity = new Vector2(Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);

                // Apply movement.
                rigidbody.velocity = transform.rotation * new Vector3(targetVelocity.x, rigidbody.velocity.y, targetVelocity.y);



                // Apply head bob
                ApplyHeadBob();
            }
        }


        void StartDash()
        {
            PlaySound(dashClip);
            isDashing = true;
            dashEndTime = Time.time + dashDuration;
            dashDirection = transform.forward; // Dash in the direction the player is currently facing
            lastDashTime = Time.time; // Mark the start of the cooldown
        }


        void HandleSwimming()
        {
            // Swimming movement logic
            if (Input.GetAxisRaw("Vertical") > 0)
            {
                transform.position += target.forward * swimSpeed * Time.deltaTime;
            }
            if (Input.GetAxisRaw("Vertical") < 0)
            {
                transform.position -= target.forward * swimSpeed * Time.deltaTime;
            }
        }

        void Jump(float force)
        {
            rigidbody.AddForce(Vector3.up * force, ForceMode.Impulse);
            PlaySound(jumpClip);
        }



        void ApplyHeadBob()
        {
            if (Mathf.Abs(rigidbody.velocity.x) > 0.1f || Mathf.Abs(rigidbody.velocity.z) > 0.1f)
            {
                // Player is moving
                timer += Time.deltaTime * bobFrequency * (IsRunning ? runSpeed : speed);
                float waveSlice = Mathf.Sin(timer);
                playerCamera.transform.localPosition = new Vector3(
                    playerCamera.transform.localPosition.x,
                    defaultYPos + (waveSlice * bobVerticalAmplitude),
                    playerCamera.transform.localPosition.z);
            }
            else
            {
                // Player is not moving
                timer = 0;
                // Smoothly return camera to default position
                playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition,
                    new Vector3(playerCamera.transform.localPosition.x, defaultYPos, playerCamera.transform.localPosition.z),
                    Time.deltaTime * bobFrequency);
            }
        }

        void HandleCrouching()
        {
            // Check for crouch toggle
            if (Input.GetKeyDown(crouchKey) && canCrouch)
            {
                // If attempting to stand up, check for ceiling clearance
                if (isCrouching)
                {
                    // Cast a ray upwards from the player's position to check for obstacles
                    float checkDistance = originalScale.y - crouchScale; // Distance to check above the player
                    RaycastHit hit;
                    bool ceilingAbove = Physics.Raycast(transform.position, Vector3.up, out hit, checkDistance);

                    // If there's no obstacle, or the hit object is not tagged as "Ground", allow standing up
                    if (!ceilingAbove || (hit.collider.gameObject.tag != "Ground"))
                    {
                        ToggleCrouchState();
                    }
                }
                else
                {
                    // If currently standing, allow crouching without checking
                    ToggleCrouchState();
                }
            }
        }

        void ToggleCrouchState()
        {
            isCrouching = !isCrouching;
            AdjustCrouch();
        }

        void AdjustCrouch()
        {
            if (isCrouching)
            {
                PlaySound(crouchClip);
                transform.localScale = new Vector3(transform.localScale.x, crouchScale, transform.localScale.z);
                speed = crouchSpeed; // Optionally adjust speed
            }
            else
            {
                transform.localScale = originalScale;
                speed = 5; // Reset to original speed
            }
        }

        public void ResetVelocity()
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        void UpdateStaminaUI()
        {
            if (staminaSlider)
            {
                staminaSlider.gameObject.SetActive(stamina < maxStamina);
                staminaSlider.value = stamina / maxStamina;
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Ladder"))
            {
                nearLadder = true;
                rigidbody.useGravity = false;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Ladder"))
            {
                nearLadder = false;
                isClimbing = false; // Automatically stop climbing when exiting the ladder
                rigidbody.useGravity = true;
            }
        }

        void PlaySound(AudioClip clip)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }

        void OnDrawGizmos()
        {
            // Set the color of the Gizmo
            Gizmos.color = Color.yellow;

            // Only draw the Gizmo if there's a groundCheck assigned to prevent null reference exceptions
            if (groundCheck != null)
            {
                // Draw a sphere at the groundCheck's position with a radius of groundDistance
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }
        }
    }
}
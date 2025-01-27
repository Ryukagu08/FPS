using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Player/Character Input")]
    public class CharacterInput : MonoBehaviour
    {
        // Toggle for aiming functionality
        public bool toggleAim = false;

        // Toggle for crouching functionality
        public bool toggleCrouch = true;

        // Toggle for leaning functionality
        public bool toggleLean = false;

        // Event triggered when leaning right
        public Action onLeanRight;

        // Event triggered when leaning left
        public Action onLeanLeft;

        /// <summary>
        /// Contains input action definitions and bindings.
        /// </summary>
        public Controls controls;

        /// <summary>
        /// Reference to the CharacterManager component.
        /// </summary>
        public CharacterManager characterManager { get; protected set; }

        /// <summary>
        /// The main camera used for view calculations.
        /// </summary>
        public Camera mainCamera { get; protected set; }

        /// <summary>
        /// Directional input for movement (Forward, Backward, Right, Left).
        /// </summary>
        public Vector2 moveInput { get; protected set; }

        /// <summary>
        /// Raw input values for camera look direction (Up, Down, Right, Left).
        /// </summary>
        public Vector2 rawLookInput { get; protected set; }

        /// <summary>
        /// Adjusted look input values considering sensitivity and other factors.
        /// </summary>
        public Vector2 lookInput { get; protected set; }

        /// <summary>
        /// Indicates if sprinting is currently active.
        /// </summary>
        public bool sprintInput { get; set; }

        /// <summary>
        /// Indicates if tactical sprinting is currently active.
        /// </summary>
        public bool tacticalSprintInput { get; set; }

        /// <summary>
        /// Raw input for tactical sprinting used to detect double clicks.
        /// </summary>
        [HideInInspector] public bool rawTacticalSprintInput;

        /// <summary>
        /// Indicates if jumping input is currently active.
        /// </summary>
        public bool jumpInput { get; set; }

        /// <summary>
        /// Indicates if crouching input is currently active.
        /// </summary>
        public bool crouchInput { get; set; }

        /// <summary>
        /// Indicates if leaning right input is currently active.
        /// </summary>
        public bool leanRightInput { get; set; }

        /// <summary>
        /// Indicates if leaning left input is currently active.
        /// </summary>
        public bool leanLeftInput { get; set; }

        /// <summary>
        /// Additional look value to apply to the camera's rotation.
        /// </summary>
        public Vector2 addedLookValue { get; set; }

        /// <summary>
        /// Raw input for sprinting used for filtering purposes.
        /// </summary>
        [HideInInspector] public bool rawSprintInput;

        private float lastSprintClickTime;

        private void Start()
        {
            // Initialize input actions
            controls = new Controls();
            controls.Player.Enable();

            // Get the CharacterManager component
            characterManager = GetComponent<CharacterManager>();

            // Set up input listeners
            AddInputListeners();
        }

        protected void Update()
        {
            // Read movement input values
            moveInput = controls.Player.Move.ReadValue<Vector2>();

            // Read and process raw look input values
            Vector2 rawLookInput_Unmultiplied = controls.Player.Look.ReadValue<Vector2>();
            rawLookInput = new Vector2(
                rawLookInput_Unmultiplied.x * FPSFrameworkUtility.xSensitivityMultiplier,
                rawLookInput_Unmultiplied.y * FPSFrameworkUtility.ySensitivityMultiplier
            );

            // Update sprint and tactical sprint inputs
            if (moveInput.y < 0 || crouchInput)
            {
                tacticalSprintInput = false;
                sprintInput = false;
            }
            else
            {
                tacticalSprintInput = moveInput.y > 0 ? rawTacticalSprintInput : false;
                sprintInput = moveInput.y > 0 ? rawSprintInput : false;
            }

            if (tacticalSprintInput) sprintInput = false;

            // Check for double clicks on tactical sprint input
            controls.Player.TacticalSprint.HasDoupleClicked(ref rawTacticalSprintInput, ref lastSprintClickTime);

            // Update jump input based on pause state
            if (PauseMenu.Instance)
            {
                jumpInput = !PauseMenu.Instance.paused && controls.Player.Jump.triggered;
            }
            else
            {
                jumpInput = controls.Player.Jump.triggered;
            }

            // Find the main camera if it's not already assigned
            if (mainCamera == null) mainCamera = Camera.main;

            // Calculate processed look input based on camera sensitivity and pause state
            Vector2 processedLookInput = Vector2.zero;
            if (mainCamera)
            {
                float sensitivity = characterManager.character?.sensitivity ?? 1;
                float dynamicSensitivity = Time.fixedDeltaTime * (sensitivity * mainCamera.fieldOfView / 179) / 50;

                if (PauseMenu.Instance && PauseMenu.Instance.paused)
                {
                    dynamicSensitivity = 0;
                }

                processedLookInput = addedLookValue + (rawLookInput * dynamicSensitivity);
                addedLookValue = Vector2.zero;
            }
            else
            {
                float sensitivity = characterManager.character?.sensitivity ?? 1;
                processedLookInput = addedLookValue + ((rawLookInput / 50) * sensitivity);
                addedLookValue = Vector2.zero;
            }

            lookInput = new Vector2(processedLookInput.x, processedLookInput.y);
        }

        private void LateUpdate()
        {
            // Reset leaning inputs if conflicting with other actions
            if (leanRightInput && leanLeftInput || sprintInput || tacticalSprintInput)
            {
                leanRightInput = false;
                leanLeftInput = false;
            }
        }

        protected void AddInputListeners()
        {
            // Handle sprint input
            controls.Player.Sprint.performed += context => rawSprintInput = true;
            controls.Player.Sprint.canceled += context => rawSprintInput = false;

            // Handle crouch input
            controls.Player.Crouch.performed += context =>
            {
                if (!toggleCrouch) crouchInput = true;
            };
            controls.Player.Crouch.canceled += context =>
            {
                crouchInput = toggleCrouch ? !crouchInput : false;
            };

            // Handle lean right input
            controls.Player.LeanRight.performed += context =>
            {
                if (!toggleLean) leanRightInput = true;
            };
            controls.Player.LeanRight.canceled += context =>
            {
                leanRightInput = toggleLean ? !leanRightInput : false;
                if (toggleLean) leanLeftInput = false;
            };

            // Handle lean left input
            controls.Player.LeanLeft.performed += context =>
            {
                if (!toggleLean) leanLeftInput = true;
            };
            controls.Player.LeanLeft.canceled += context =>
            {
                leanLeftInput = toggleLean ? !leanLeftInput : false;
                if (toggleLean) leanRightInput = false;
            };
        }

        /// <summary>
        /// Adds an amount to the current look value for camera rotation.
        /// </summary>
        /// <param name="value">The amount to add to the look value.</param>
        public void AddLookValue(Vector2 value)
        {
            addedLookValue += value;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Player/Item Input")]
    public class ItemInput : MonoBehaviour
    {
        /// <summary>
        /// Indicates whether aiming is toggled.
        /// </summary>
        public bool toggleAim { get; protected set; }

        /// <summary>
        /// Input actions for firearm and throwable controls.
        /// </summary>
        public Controls controls;

        /// <summary>
        /// Reference to the CharacterInput component in the parent.
        /// </summary>
        public CharacterInput characterInput { get; set; }

        /// <summary>
        /// Indicates whether reload input is active.
        /// </summary>
        public bool reloadInput { get; set; }

        /// <summary>
        /// Indicates whether the fire mode switch input is active.
        /// </summary>
        public bool fireModeSwitchInput { get; set; }

        /// <summary>
        /// Indicates whether the sight mode switch input is active.
        /// </summary>
        public bool sightModeSwitchInput { get; set; }

        /// <summary>
        /// Indicates whether aiming input is active.
        /// </summary>
        public bool aimInput { get; set; }

        /// <summary>
        /// Indicates whether dropping the item input is active.
        /// </summary>
        public bool dropInput { get; set; }

        /// <summary>
        /// Indicates whether the fire action was triggered.
        /// </summary>
        public bool triggredFire { get; set; }

        /// <summary>
        /// Indicates whether the fire action is held.
        /// </summary>
        public bool heldFire { get; set; }

        /// <summary>
        /// Action for throwing an item.
        /// </summary>
        public InputAction throwAction { get; set; }

        private void Start()
        {
            // Initialize input actions
            controls = new Controls();
            controls.Firearm.Enable();
            controls.Throwable.Enable();
            throwAction = controls.Throwable.Throw;

            // Get the CharacterInput component from the parent
            characterInput = GetComponentInParent<CharacterInput>();

            // Set up input listeners
            AddInputListeners();
        }

        private void OnEnable()
        {
            // Ensure aim input is reset when the component is enabled
            aimInput = false;
        }

        private void Update()
        {
            // Update input states
            reloadInput = controls.Firearm.Reload.triggered;
            fireModeSwitchInput = controls.Firearm.FireModeSwich.triggered;
            sightModeSwitchInput = controls.Firearm.SightModeSwitch.triggered;
            dropInput = controls.Firearm.Drop.triggered;

            // Update toggleAim from the parent CharacterInput
            toggleAim = characterInput.toggleAim;

            // Update fire input states
            triggredFire = controls.Firearm.Fire.triggered;
            heldFire = controls.Firearm.Fire.IsPressed();
        }

        private void AddInputListeners()
        {
            // Add listener for aiming input
            controls.Firearm.Aim.performed += context =>
            {
                if (toggleAim)
                {
                    aimInput = !aimInput;
                }
                else
                {
                    aimInput = true;
                }
            };

            controls.Firearm.Aim.canceled += context =>
            {
                if (!toggleAim)
                {
                    aimInput = false;
                }
            };
        }
    }
}

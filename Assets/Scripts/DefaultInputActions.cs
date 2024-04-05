//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/ProjectConfig/DefaultInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Cosmobot
{
    public partial class @DefaultInputActions: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @DefaultInputActions()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""DefaultInputActions"",
    ""maps"": [
        {
            ""name"": ""PlayerMovement"",
            ""id"": ""0b76f2c3-3a99-4db1-8a8e-731651b0d56f"",
            ""actions"": [
                {
                    ""name"": ""movement"",
                    ""type"": ""Value"",
                    ""id"": ""583a3c59-aef5-44ec-879b-b8a8420f4413"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""jump"",
                    ""type"": ""Button"",
                    ""id"": ""a52c290c-e873-4832-925d-15d7ebdee8cb"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WSAD"",
                    ""id"": ""fbe0384e-0ed5-40cc-a93c-78162a5e8dea"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""6ab5f737-d48f-4e6b-b15e-75e8ffa13d92"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""a2647e5e-6aa7-4335-bfe9-3587cd75179e"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""1c18233a-d12f-4049-807a-17b1d1216fab"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""12c14194-7110-4cdc-bbff-864a581c40b5"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Arrows"",
                    ""id"": ""a2dec66c-c51a-4a20-ba75-24807b1abf7b"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""9db90531-13f2-426b-9fb6-25a1856daa60"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""0942ce33-1dba-48b3-84cf-171a7675328d"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""8e4b59a7-6e27-431c-ac34-4860c9479eea"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""bab30291-5a9d-4150-bcf8-49468c5fd832"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""cfa0e537-b36d-4304-89a7-d29671c6e0a6"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""PlayerCamera"",
            ""id"": ""8e621584-24a6-434f-afb7-5bbe85d11be2"",
            ""actions"": [
                {
                    ""name"": ""camera"",
                    ""type"": ""Value"",
                    ""id"": ""c4cf1433-09cd-4fab-be87-842ce20e02b4"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""switchView"",
                    ""type"": ""Button"",
                    ""id"": ""104b72e1-39e0-4769-9acd-910a4f80dea7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""zoom"",
                    ""type"": ""Button"",
                    ""id"": ""b298ad49-16e3-4c70-a1e2-79d0cdd79d1e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""3f4b0925-51cd-4cda-b4ae-1b38ca4df841"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""da79b65e-0d14-44f0-ab32-6f2929a5fc37"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""switchView"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b2b55378-1c0d-469a-ac3f-9342f1f91d56"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Minimap"",
            ""id"": ""937d75ae-f092-46fc-ad81-0a2f41e94b59"",
            ""actions"": [
                {
                    ""name"": ""Toggle"",
                    ""type"": ""Button"",
                    ""id"": ""46a81420-c1a6-435b-aeff-bdfb512c6404"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""13d04ef6-9c13-4c3d-ae4d-a4afcabb07e4"",
                    ""path"": ""<Keyboard>/m"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Toggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""PlayerGun"",
            ""id"": ""9d2713aa-b648-490b-b1eb-fb411b4d6512"",
            ""actions"": [
                {
                    ""name"": ""shoot"",
                    ""type"": ""Button"",
                    ""id"": ""0f8c47af-d8db-4bac-ab79-c85a2326ba6d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""pickup"",
                    ""type"": ""Button"",
                    ""id"": ""5b451df9-5ab6-4ed1-bac3-497b0a359349"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""12bf8558-66df-4230-8831-84fd76b16420"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""66420144-d7ce-42f4-916e-7df72986650f"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""pickup"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // PlayerMovement
            m_PlayerMovement = asset.FindActionMap("PlayerMovement", throwIfNotFound: true);
            m_PlayerMovement_movement = m_PlayerMovement.FindAction("movement", throwIfNotFound: true);
            m_PlayerMovement_jump = m_PlayerMovement.FindAction("jump", throwIfNotFound: true);
            // PlayerCamera
            m_PlayerCamera = asset.FindActionMap("PlayerCamera", throwIfNotFound: true);
            m_PlayerCamera_camera = m_PlayerCamera.FindAction("camera", throwIfNotFound: true);
            m_PlayerCamera_switchView = m_PlayerCamera.FindAction("switchView", throwIfNotFound: true);
            m_PlayerCamera_zoom = m_PlayerCamera.FindAction("zoom", throwIfNotFound: true);
            // Minimap
            m_Minimap = asset.FindActionMap("Minimap", throwIfNotFound: true);
            m_Minimap_Toggle = m_Minimap.FindAction("Toggle", throwIfNotFound: true);
            // PlayerGun
            m_PlayerGun = asset.FindActionMap("PlayerGun", throwIfNotFound: true);
            m_PlayerGun_shoot = m_PlayerGun.FindAction("shoot", throwIfNotFound: true);
            m_PlayerGun_pickup = m_PlayerGun.FindAction("pickup", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // PlayerMovement
        private readonly InputActionMap m_PlayerMovement;
        private List<IPlayerMovementActions> m_PlayerMovementActionsCallbackInterfaces = new List<IPlayerMovementActions>();
        private readonly InputAction m_PlayerMovement_movement;
        private readonly InputAction m_PlayerMovement_jump;
        public struct PlayerMovementActions
        {
            private @DefaultInputActions m_Wrapper;
            public PlayerMovementActions(@DefaultInputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @movement => m_Wrapper.m_PlayerMovement_movement;
            public InputAction @jump => m_Wrapper.m_PlayerMovement_jump;
            public InputActionMap Get() { return m_Wrapper.m_PlayerMovement; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(PlayerMovementActions set) { return set.Get(); }
            public void AddCallbacks(IPlayerMovementActions instance)
            {
                if (instance == null || m_Wrapper.m_PlayerMovementActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_PlayerMovementActionsCallbackInterfaces.Add(instance);
                @movement.started += instance.OnMovement;
                @movement.performed += instance.OnMovement;
                @movement.canceled += instance.OnMovement;
                @jump.started += instance.OnJump;
                @jump.performed += instance.OnJump;
                @jump.canceled += instance.OnJump;
            }

            private void UnregisterCallbacks(IPlayerMovementActions instance)
            {
                @movement.started -= instance.OnMovement;
                @movement.performed -= instance.OnMovement;
                @movement.canceled -= instance.OnMovement;
                @jump.started -= instance.OnJump;
                @jump.performed -= instance.OnJump;
                @jump.canceled -= instance.OnJump;
            }

            public void RemoveCallbacks(IPlayerMovementActions instance)
            {
                if (m_Wrapper.m_PlayerMovementActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IPlayerMovementActions instance)
            {
                foreach (var item in m_Wrapper.m_PlayerMovementActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_PlayerMovementActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public PlayerMovementActions @PlayerMovement => new PlayerMovementActions(this);

        // PlayerCamera
        private readonly InputActionMap m_PlayerCamera;
        private List<IPlayerCameraActions> m_PlayerCameraActionsCallbackInterfaces = new List<IPlayerCameraActions>();
        private readonly InputAction m_PlayerCamera_camera;
        private readonly InputAction m_PlayerCamera_switchView;
        private readonly InputAction m_PlayerCamera_zoom;
        public struct PlayerCameraActions
        {
            private @DefaultInputActions m_Wrapper;
            public PlayerCameraActions(@DefaultInputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @camera => m_Wrapper.m_PlayerCamera_camera;
            public InputAction @switchView => m_Wrapper.m_PlayerCamera_switchView;
            public InputAction @zoom => m_Wrapper.m_PlayerCamera_zoom;
            public InputActionMap Get() { return m_Wrapper.m_PlayerCamera; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(PlayerCameraActions set) { return set.Get(); }
            public void AddCallbacks(IPlayerCameraActions instance)
            {
                if (instance == null || m_Wrapper.m_PlayerCameraActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_PlayerCameraActionsCallbackInterfaces.Add(instance);
                @camera.started += instance.OnCamera;
                @camera.performed += instance.OnCamera;
                @camera.canceled += instance.OnCamera;
                @switchView.started += instance.OnSwitchView;
                @switchView.performed += instance.OnSwitchView;
                @switchView.canceled += instance.OnSwitchView;
                @zoom.started += instance.OnZoom;
                @zoom.performed += instance.OnZoom;
                @zoom.canceled += instance.OnZoom;
            }

            private void UnregisterCallbacks(IPlayerCameraActions instance)
            {
                @camera.started -= instance.OnCamera;
                @camera.performed -= instance.OnCamera;
                @camera.canceled -= instance.OnCamera;
                @switchView.started -= instance.OnSwitchView;
                @switchView.performed -= instance.OnSwitchView;
                @switchView.canceled -= instance.OnSwitchView;
                @zoom.started -= instance.OnZoom;
                @zoom.performed -= instance.OnZoom;
                @zoom.canceled -= instance.OnZoom;
            }

            public void RemoveCallbacks(IPlayerCameraActions instance)
            {
                if (m_Wrapper.m_PlayerCameraActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IPlayerCameraActions instance)
            {
                foreach (var item in m_Wrapper.m_PlayerCameraActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_PlayerCameraActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public PlayerCameraActions @PlayerCamera => new PlayerCameraActions(this);

        // Minimap
        private readonly InputActionMap m_Minimap;
        private List<IMinimapActions> m_MinimapActionsCallbackInterfaces = new List<IMinimapActions>();
        private readonly InputAction m_Minimap_Toggle;
        public struct MinimapActions
        {
            private @DefaultInputActions m_Wrapper;
            public MinimapActions(@DefaultInputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @Toggle => m_Wrapper.m_Minimap_Toggle;
            public InputActionMap Get() { return m_Wrapper.m_Minimap; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(MinimapActions set) { return set.Get(); }
            public void AddCallbacks(IMinimapActions instance)
            {
                if (instance == null || m_Wrapper.m_MinimapActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_MinimapActionsCallbackInterfaces.Add(instance);
                @Toggle.started += instance.OnToggle;
                @Toggle.performed += instance.OnToggle;
                @Toggle.canceled += instance.OnToggle;
            }

            private void UnregisterCallbacks(IMinimapActions instance)
            {
                @Toggle.started -= instance.OnToggle;
                @Toggle.performed -= instance.OnToggle;
                @Toggle.canceled -= instance.OnToggle;
            }

            public void RemoveCallbacks(IMinimapActions instance)
            {
                if (m_Wrapper.m_MinimapActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IMinimapActions instance)
            {
                foreach (var item in m_Wrapper.m_MinimapActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_MinimapActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public MinimapActions @Minimap => new MinimapActions(this);

        // PlayerGun
        private readonly InputActionMap m_PlayerGun;
        private List<IPlayerGunActions> m_PlayerGunActionsCallbackInterfaces = new List<IPlayerGunActions>();
        private readonly InputAction m_PlayerGun_shoot;
        private readonly InputAction m_PlayerGun_pickup;
        public struct PlayerGunActions
        {
            private @DefaultInputActions m_Wrapper;
            public PlayerGunActions(@DefaultInputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @shoot => m_Wrapper.m_PlayerGun_shoot;
            public InputAction @pickup => m_Wrapper.m_PlayerGun_pickup;
            public InputActionMap Get() { return m_Wrapper.m_PlayerGun; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(PlayerGunActions set) { return set.Get(); }
            public void AddCallbacks(IPlayerGunActions instance)
            {
                if (instance == null || m_Wrapper.m_PlayerGunActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_PlayerGunActionsCallbackInterfaces.Add(instance);
                @shoot.started += instance.OnShoot;
                @shoot.performed += instance.OnShoot;
                @shoot.canceled += instance.OnShoot;
                @pickup.started += instance.OnPickup;
                @pickup.performed += instance.OnPickup;
                @pickup.canceled += instance.OnPickup;
            }

            private void UnregisterCallbacks(IPlayerGunActions instance)
            {
                @shoot.started -= instance.OnShoot;
                @shoot.performed -= instance.OnShoot;
                @shoot.canceled -= instance.OnShoot;
                @pickup.started -= instance.OnPickup;
                @pickup.performed -= instance.OnPickup;
                @pickup.canceled -= instance.OnPickup;
            }

            public void RemoveCallbacks(IPlayerGunActions instance)
            {
                if (m_Wrapper.m_PlayerGunActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IPlayerGunActions instance)
            {
                foreach (var item in m_Wrapper.m_PlayerGunActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_PlayerGunActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public PlayerGunActions @PlayerGun => new PlayerGunActions(this);
        public interface IPlayerMovementActions
        {
            void OnMovement(InputAction.CallbackContext context);
            void OnJump(InputAction.CallbackContext context);
        }
        public interface IPlayerCameraActions
        {
            void OnCamera(InputAction.CallbackContext context);
            void OnSwitchView(InputAction.CallbackContext context);
            void OnZoom(InputAction.CallbackContext context);
        }
        public interface IMinimapActions
        {
            void OnToggle(InputAction.CallbackContext context);
        }
        public interface IPlayerGunActions
        {
            void OnShoot(InputAction.CallbackContext context);
            void OnPickup(InputAction.CallbackContext context);
        }
    }
}

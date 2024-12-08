//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.11.2
//     from Assets/Scripts/Interface/Input/PlayableCharacterInputs.inputactions
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

public partial class @PlayableCharacterInputs: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayableCharacterInputs()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayableCharacterInputs"",
    ""maps"": [
        {
            ""name"": ""character"",
            ""id"": ""a3170022-aa82-4b0d-95ac-9e9178efdfbc"",
            ""actions"": [
                {
                    ""name"": ""aim"",
                    ""type"": ""Value"",
                    ""id"": ""b00bba34-9aa5-417a-80eb-c0a83e3d4bd6"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""movement"",
                    ""type"": ""PassThrough"",
                    ""id"": ""593ab85e-b581-426f-af0c-3a13e439f93f"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""rush"",
                    ""type"": ""PassThrough"",
                    ""id"": ""58017b8a-e844-4de3-80ec-6258c336ede2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""jump"",
                    ""type"": ""Button"",
                    ""id"": ""03841b82-074c-4841-af00-16aa08a8a2ac"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""dash"",
                    ""type"": ""Button"",
                    ""id"": ""79a8a516-0028-483d-8071-52694c716cae"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""block"",
                    ""type"": ""Button"",
                    ""id"": ""6799233d-eda8-4a5c-a64d-cf710a687472"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""light"",
                    ""type"": ""Button"",
                    ""id"": ""6efbe99c-c453-4ef7-baaf-1eba0682fa37"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""medium"",
                    ""type"": ""Button"",
                    ""id"": ""c8e7466a-1c5c-4cd0-98e3-31a459176f26"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""heavy"",
                    ""type"": ""Button"",
                    ""id"": ""d59f23eb-ad21-4af9-b53d-b78e756c4b72"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""throw"",
                    ""type"": ""Button"",
                    ""id"": ""cd4e2a1f-0f90-456f-97e9-51fb88996b76"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ability1"",
                    ""type"": ""Button"",
                    ""id"": ""ca228c90-9d79-4782-830a-c1d545e07dc8"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ability2"",
                    ""type"": ""Button"",
                    ""id"": ""37c2848a-7965-49a0-82a1-69d9c9a1f8cf"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ability3"",
                    ""type"": ""Button"",
                    ""id"": ""5f3af129-6bd7-4902-abb4-026dfc9373c4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ability4"",
                    ""type"": ""Button"",
                    ""id"": ""dc13de7b-336a-421f-8064-ac2b260bcba3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""special1"",
                    ""type"": ""Button"",
                    ""id"": ""ad3ad947-20e0-40a5-ac4a-b57b1cd5f175"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""special2"",
                    ""type"": ""Button"",
                    ""id"": ""5c04025d-4c8e-4556-a231-f22401670469"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ultimate"",
                    ""type"": ""Button"",
                    ""id"": ""a4c3711a-0f84-4516-9c92-b054a662ffbe"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""8cfb5123-ad66-425e-9e17-c599b1e1a796"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""light"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a3cca5a5-5272-4fb6-a804-d97611060f54"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ca8f9dde-3832-42d7-a3e7-10e08b4d391c"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bffe3d1d-008c-4778-b87a-b8ae499e8f1e"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""throw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""One Modifier"",
                    ""id"": ""8d23d421-599b-4a0a-bd8f-2777818e8f14"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""throw"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""717b62a6-f26b-4ee9-ba86-b6115742da17"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""throw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""c71358eb-0060-4817-b993-06ea8db4d5a7"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""throw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""7bf3f69e-6089-4444-b1b7-e66394ce58b7"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ability1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bc1480b0-0b12-4eb8-b466-99ccc24bf794"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ability2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f1bff09e-5368-47b3-8369-63387b63cbe4"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ability3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""66c67367-3003-4da9-ba71-db3a4b1d3d8a"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ability4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""02ec5e36-e7bd-4392-82f5-220366fdf7ae"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ultimate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""581e2e5c-e670-4a6f-b20f-664ea0e77f72"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""block"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""One Modifier"",
                    ""id"": ""2f28835d-e488-4c39-b977-046ce0baafe1"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special1"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""a3986093-3a30-4d80-ad24-06fe7c1124f6"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""5cad4734-8190-47dd-9717-edd940534e38"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""One Modifier"",
                    ""id"": ""79f00a4c-61c8-454a-a410-ac1b94d34035"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special1"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""1fe25519-afc3-41cb-98e9-05b5e064905d"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""6c82a423-5fdf-4376-8087-8e680ece2064"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""One Modifier"",
                    ""id"": ""475b0d0d-6481-434b-bf08-111a02bbfbb9"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special2"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""4cba9f21-ceeb-4119-988a-9f14346b362f"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""9f6bae7f-a797-44da-a3ed-6a4197dc87c6"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""2a0ae7c2-be64-4b8c-b853-49d7e01ff440"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""medium"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""85c8f57f-9146-4cfd-9d25-e61ecd42c264"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""heavy"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""One Modifier"",
                    ""id"": ""625d659d-3d22-4ee3-92b3-adb729da7ac7"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special2"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""d9bcc0cd-03c3-4ebd-bdb7-28f5e785e4da"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""553ea218-739a-44e7-a314-b1effaece0df"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""special2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""a34993f6-0f2f-4a36-9409-cd5f4654da24"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""aim"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""b4d105cd-ca13-4649-863c-36972f4d07b1"",
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
                    ""id"": ""a1fada33-ca86-487a-bd8c-de94ee950af8"",
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
                    ""id"": ""2f2a2ce6-6cc8-44df-9561-051e48cc2e2c"",
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
                    ""id"": ""99e1aeb5-46db-441d-813a-f23df99fe343"",
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
                    ""id"": ""38a9da30-48e3-4a01-acc5-4f4f70cb4061"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""b6f33cdc-7473-49f8-9f09-db6afcdc457c"",
                    ""path"": ""<Keyboard>/leftAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""rush"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // character
        m_character = asset.FindActionMap("character", throwIfNotFound: true);
        m_character_aim = m_character.FindAction("aim", throwIfNotFound: true);
        m_character_movement = m_character.FindAction("movement", throwIfNotFound: true);
        m_character_rush = m_character.FindAction("rush", throwIfNotFound: true);
        m_character_jump = m_character.FindAction("jump", throwIfNotFound: true);
        m_character_dash = m_character.FindAction("dash", throwIfNotFound: true);
        m_character_block = m_character.FindAction("block", throwIfNotFound: true);
        m_character_light = m_character.FindAction("light", throwIfNotFound: true);
        m_character_medium = m_character.FindAction("medium", throwIfNotFound: true);
        m_character_heavy = m_character.FindAction("heavy", throwIfNotFound: true);
        m_character_throw = m_character.FindAction("throw", throwIfNotFound: true);
        m_character_ability1 = m_character.FindAction("ability1", throwIfNotFound: true);
        m_character_ability2 = m_character.FindAction("ability2", throwIfNotFound: true);
        m_character_ability3 = m_character.FindAction("ability3", throwIfNotFound: true);
        m_character_ability4 = m_character.FindAction("ability4", throwIfNotFound: true);
        m_character_special1 = m_character.FindAction("special1", throwIfNotFound: true);
        m_character_special2 = m_character.FindAction("special2", throwIfNotFound: true);
        m_character_ultimate = m_character.FindAction("ultimate", throwIfNotFound: true);
    }

    ~@PlayableCharacterInputs()
    {
        UnityEngine.Debug.Assert(!m_character.enabled, "This will cause a leak and performance issues, PlayableCharacterInputs.character.Disable() has not been called.");
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

    // character
    private readonly InputActionMap m_character;
    private List<ICharacterActions> m_CharacterActionsCallbackInterfaces = new List<ICharacterActions>();
    private readonly InputAction m_character_aim;
    private readonly InputAction m_character_movement;
    private readonly InputAction m_character_rush;
    private readonly InputAction m_character_jump;
    private readonly InputAction m_character_dash;
    private readonly InputAction m_character_block;
    private readonly InputAction m_character_light;
    private readonly InputAction m_character_medium;
    private readonly InputAction m_character_heavy;
    private readonly InputAction m_character_throw;
    private readonly InputAction m_character_ability1;
    private readonly InputAction m_character_ability2;
    private readonly InputAction m_character_ability3;
    private readonly InputAction m_character_ability4;
    private readonly InputAction m_character_special1;
    private readonly InputAction m_character_special2;
    private readonly InputAction m_character_ultimate;
    public struct CharacterActions
    {
        private @PlayableCharacterInputs m_Wrapper;
        public CharacterActions(@PlayableCharacterInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @aim => m_Wrapper.m_character_aim;
        public InputAction @movement => m_Wrapper.m_character_movement;
        public InputAction @rush => m_Wrapper.m_character_rush;
        public InputAction @jump => m_Wrapper.m_character_jump;
        public InputAction @dash => m_Wrapper.m_character_dash;
        public InputAction @block => m_Wrapper.m_character_block;
        public InputAction @light => m_Wrapper.m_character_light;
        public InputAction @medium => m_Wrapper.m_character_medium;
        public InputAction @heavy => m_Wrapper.m_character_heavy;
        public InputAction @throw => m_Wrapper.m_character_throw;
        public InputAction @ability1 => m_Wrapper.m_character_ability1;
        public InputAction @ability2 => m_Wrapper.m_character_ability2;
        public InputAction @ability3 => m_Wrapper.m_character_ability3;
        public InputAction @ability4 => m_Wrapper.m_character_ability4;
        public InputAction @special1 => m_Wrapper.m_character_special1;
        public InputAction @special2 => m_Wrapper.m_character_special2;
        public InputAction @ultimate => m_Wrapper.m_character_ultimate;
        public InputActionMap Get() { return m_Wrapper.m_character; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CharacterActions set) { return set.Get(); }
        public void AddCallbacks(ICharacterActions instance)
        {
            if (instance == null || m_Wrapper.m_CharacterActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_CharacterActionsCallbackInterfaces.Add(instance);
            @aim.started += instance.OnAim;
            @aim.performed += instance.OnAim;
            @aim.canceled += instance.OnAim;
            @movement.started += instance.OnMovement;
            @movement.performed += instance.OnMovement;
            @movement.canceled += instance.OnMovement;
            @rush.started += instance.OnRush;
            @rush.performed += instance.OnRush;
            @rush.canceled += instance.OnRush;
            @jump.started += instance.OnJump;
            @jump.performed += instance.OnJump;
            @jump.canceled += instance.OnJump;
            @dash.started += instance.OnDash;
            @dash.performed += instance.OnDash;
            @dash.canceled += instance.OnDash;
            @block.started += instance.OnBlock;
            @block.performed += instance.OnBlock;
            @block.canceled += instance.OnBlock;
            @light.started += instance.OnLight;
            @light.performed += instance.OnLight;
            @light.canceled += instance.OnLight;
            @medium.started += instance.OnMedium;
            @medium.performed += instance.OnMedium;
            @medium.canceled += instance.OnMedium;
            @heavy.started += instance.OnHeavy;
            @heavy.performed += instance.OnHeavy;
            @heavy.canceled += instance.OnHeavy;
            @throw.started += instance.OnThrow;
            @throw.performed += instance.OnThrow;
            @throw.canceled += instance.OnThrow;
            @ability1.started += instance.OnAbility1;
            @ability1.performed += instance.OnAbility1;
            @ability1.canceled += instance.OnAbility1;
            @ability2.started += instance.OnAbility2;
            @ability2.performed += instance.OnAbility2;
            @ability2.canceled += instance.OnAbility2;
            @ability3.started += instance.OnAbility3;
            @ability3.performed += instance.OnAbility3;
            @ability3.canceled += instance.OnAbility3;
            @ability4.started += instance.OnAbility4;
            @ability4.performed += instance.OnAbility4;
            @ability4.canceled += instance.OnAbility4;
            @special1.started += instance.OnSpecial1;
            @special1.performed += instance.OnSpecial1;
            @special1.canceled += instance.OnSpecial1;
            @special2.started += instance.OnSpecial2;
            @special2.performed += instance.OnSpecial2;
            @special2.canceled += instance.OnSpecial2;
            @ultimate.started += instance.OnUltimate;
            @ultimate.performed += instance.OnUltimate;
            @ultimate.canceled += instance.OnUltimate;
        }

        private void UnregisterCallbacks(ICharacterActions instance)
        {
            @aim.started -= instance.OnAim;
            @aim.performed -= instance.OnAim;
            @aim.canceled -= instance.OnAim;
            @movement.started -= instance.OnMovement;
            @movement.performed -= instance.OnMovement;
            @movement.canceled -= instance.OnMovement;
            @rush.started -= instance.OnRush;
            @rush.performed -= instance.OnRush;
            @rush.canceled -= instance.OnRush;
            @jump.started -= instance.OnJump;
            @jump.performed -= instance.OnJump;
            @jump.canceled -= instance.OnJump;
            @dash.started -= instance.OnDash;
            @dash.performed -= instance.OnDash;
            @dash.canceled -= instance.OnDash;
            @block.started -= instance.OnBlock;
            @block.performed -= instance.OnBlock;
            @block.canceled -= instance.OnBlock;
            @light.started -= instance.OnLight;
            @light.performed -= instance.OnLight;
            @light.canceled -= instance.OnLight;
            @medium.started -= instance.OnMedium;
            @medium.performed -= instance.OnMedium;
            @medium.canceled -= instance.OnMedium;
            @heavy.started -= instance.OnHeavy;
            @heavy.performed -= instance.OnHeavy;
            @heavy.canceled -= instance.OnHeavy;
            @throw.started -= instance.OnThrow;
            @throw.performed -= instance.OnThrow;
            @throw.canceled -= instance.OnThrow;
            @ability1.started -= instance.OnAbility1;
            @ability1.performed -= instance.OnAbility1;
            @ability1.canceled -= instance.OnAbility1;
            @ability2.started -= instance.OnAbility2;
            @ability2.performed -= instance.OnAbility2;
            @ability2.canceled -= instance.OnAbility2;
            @ability3.started -= instance.OnAbility3;
            @ability3.performed -= instance.OnAbility3;
            @ability3.canceled -= instance.OnAbility3;
            @ability4.started -= instance.OnAbility4;
            @ability4.performed -= instance.OnAbility4;
            @ability4.canceled -= instance.OnAbility4;
            @special1.started -= instance.OnSpecial1;
            @special1.performed -= instance.OnSpecial1;
            @special1.canceled -= instance.OnSpecial1;
            @special2.started -= instance.OnSpecial2;
            @special2.performed -= instance.OnSpecial2;
            @special2.canceled -= instance.OnSpecial2;
            @ultimate.started -= instance.OnUltimate;
            @ultimate.performed -= instance.OnUltimate;
            @ultimate.canceled -= instance.OnUltimate;
        }

        public void RemoveCallbacks(ICharacterActions instance)
        {
            if (m_Wrapper.m_CharacterActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(ICharacterActions instance)
        {
            foreach (var item in m_Wrapper.m_CharacterActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_CharacterActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public CharacterActions @character => new CharacterActions(this);
    public interface ICharacterActions
    {
        void OnAim(InputAction.CallbackContext context);
        void OnMovement(InputAction.CallbackContext context);
        void OnRush(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnDash(InputAction.CallbackContext context);
        void OnBlock(InputAction.CallbackContext context);
        void OnLight(InputAction.CallbackContext context);
        void OnMedium(InputAction.CallbackContext context);
        void OnHeavy(InputAction.CallbackContext context);
        void OnThrow(InputAction.CallbackContext context);
        void OnAbility1(InputAction.CallbackContext context);
        void OnAbility2(InputAction.CallbackContext context);
        void OnAbility3(InputAction.CallbackContext context);
        void OnAbility4(InputAction.CallbackContext context);
        void OnSpecial1(InputAction.CallbackContext context);
        void OnSpecial2(InputAction.CallbackContext context);
        void OnUltimate(InputAction.CallbackContext context);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cosmobot
{
    public class ControllerUI : MonoBehaviour, DefaultInputActions.IProgrammingUIActions
    {

        private DefaultInputActions actions;
        private PlayerCamera playerCamera;

        private Image UIImage;
        private TMP_InputField inputField;

        private RectTransform textArea;


        // Start is called before the first frame update
        void Start()
        {
            UIImage = GetComponent<Image>();
            inputField = GetComponent<TMP_InputField>();
            inputField.textComponent.enableWordWrapping = false;
            inputField.onDeselect.AddListener(IFOnDeselect);
            textArea = inputField.textViewport;
            textArea.gameObject.SetActive(false);
            UIImage.enabled = false;
            inputField.enabled = false;
            playerCamera = FindObjectOfType<PlayerCamera>();
        }

        // Update is called once per frame

        void Update()
        {

        }

        private void OnEnable()
        {
            if (actions is null)
            {
                actions = new DefaultInputActions();
                actions.ProgrammingUI.SetCallbacks(this);
            }

            actions.ProgrammingUI.Enable();
        }

        private void OnDisable()
        {
            actions.ProgrammingUI.Disable();
        }

        public void OnOpen(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            UIImage.enabled = true;
            inputField.enabled = true;
            inputField.textComponent.enableWordWrapping = false;
            textArea.gameObject.SetActive(true);
            Time.timeScale = 0;
            inputField.ActivateInputField();
            actions.PlayerCamera.Disable();
            playerCamera.enabled = false;
        }

        public void OnClose(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            UIImage.enabled = false;
            inputField.enabled = false;
            textArea.gameObject.SetActive(false);
            Time.timeScale = 1;
            playerCamera.enabled = true;

        }

        public void IFOnDeselect(string arg0)
        {
            inputField.ActivateInputField();
        }

        public void OnSave(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

    }
}

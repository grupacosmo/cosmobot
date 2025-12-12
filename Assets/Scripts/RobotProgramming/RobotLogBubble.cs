using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Cosmobot
{
    public class RobotLogBubble : MonoBehaviour
    {
        [SerializeField] private GameObject logItemPrefab;
        [SerializeField] private Transform logContainer;
        [SerializeField] private float logLifeDuration = 4.0f;

        private Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera != null)
            {
                Vector3 directionToCamera = transform.position - mainCamera.transform.position;
                directionToCamera.y = 0;

                if (directionToCamera != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(directionToCamera);
                }
            }
        }

        public void AddLog(string message, Color typeColor)
        {
            GameObject newLog = Instantiate(logItemPrefab, logContainer);

            TMP_Text textComponent = newLog.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = message;
            }
            Image bgImage = newLog.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = typeColor;
            }

            Destroy(newLog, logLifeDuration);
        }

        void Update()
        {
            //testowanie
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AddLog("Traveling to battery factory", new Color(0.655f, 0.773f, 0.827f));
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AddLog("pickItem() Error: \nNo ”battery” to pick up!", new Color(1f, 0.5f, 0.5f));
            }
        }

    }
}

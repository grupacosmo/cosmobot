using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class AutoClicker : MonoBehaviour
    {
        [SerializeField]
        private float clickInterval = 1f;
        private IInteractable target;

        private float currentClickInterval = 1f;

        private void Awake()
        {
            target = GetComponent<IInteractable>();
        }

        private void Update()
        {
            currentClickInterval -= Time.deltaTime;
            if (currentClickInterval <= 0)
            {
                target.Use();
                currentClickInterval = clickInterval;
            }
        }
    }
}
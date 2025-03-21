using System.Collections;
using System.Collections.Generic;
using Codice.Client.Common.TreeGrouper;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Cosmobot
{
    [ExecuteInEditMode]
    public class ResearchNode : MonoBehaviour
    {

        [Header("Unlock Info")]
        public bool unlocked;
        public string title;
        public int researchCost;
        public int playerResearchPoints = 100;
        [Space(25)]
        public ResearchNode[] requiredParentNodes;
        [Space(25)]

        public Color backgroundColorUnlocked = Color.green;
        public Color backgroundColorLocked = Color.black;

        public Color currentColor;

        private RawImage backgroundImage;
        private TMP_Text titleText;
        private TMP_Text costText;
        private Button unlockButton;

        void UpdateView()
        {
            // get parts of the Node
            backgroundImage = GetComponentInChildren<RawImage>();
            unlockButton = GetComponentInChildren<Button>();
            titleText = transform.Find("Title")?.GetComponent<TMP_Text>();
            costText = transform.Find("Cost")?.GetComponent<TMP_Text>();

            // update info viewed by node
            titleText.text = title;
            costText.text = researchCost.ToString();

            // update name
            gameObject.name = "Research" + titleText.text;

            // update color
            currentColor = unlocked ? backgroundColorUnlocked : backgroundColorLocked;
            backgroundImage.color = currentColor;
        }

        void OnValidate()
        {
            UpdateView();
        }

        public bool CanBeUnlocked()
        {
            if (unlocked) return true;
            if (playerResearchPoints < researchCost) return false;

            foreach (ResearchNode node in requiredParentNodes)
            {
                if (!node.unlocked) return false;
            }

            return true;
        }

        public void Unlock()
        {
            Debug.Log("Unlocked");
            unlocked = true;
            UpdateView();
        }

        public void ClickAction()
        {
            if (CanBeUnlocked()) Unlock();
        }
    }
}

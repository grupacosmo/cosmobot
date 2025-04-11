using System.Collections;
using System.Collections.Generic;
using Codice.Client.Common.TreeGrouper;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace Cosmobot.Research
{
    [ExecuteInEditMode]
    public class ResearchNode : MonoBehaviour
    {
        [Header("Unlock Info")]
        public ResearchUnlockable unlockable;


        public bool Unlocked
        {
            get { return unlockable.Unlocked; }
            set { unlockable.Unlocked = value; }
        }

        public static bool testMode = true;

        [Header("Node Info")]
        public ResearchNode[] requiredParentNodes;

        public float playerResearchPoints = 100;

        private Color backgroundColorUnlocked = Color.green;
        private Color backgroundColorLocked = Color.black;
        private Color currentColor;

        private RawImage backgroundImage;
        private TMP_Text titleText;
        private TMP_Text costText;
        private Button unlockButton;

        private LineRenderer lineRenderer;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.startWidth = 5f;
            lineRenderer.endWidth = 5f;

            backgroundImage = GetComponentInChildren<RawImage>();
            unlockButton = GetComponentInChildren<Button>();
            titleText = transform.Find("Title")?.GetComponent<TMP_Text>();
            costText = transform.Find("Cost")?.GetComponent<TMP_Text>();

        }

        void UpdateView()
        {
            // update info viewed by node
            titleText.text = (testMode) ? unlockable.Name + " ID: " + unlockable.Id : unlockable.Name;
            costText.text = unlockable.ResearchCost.ToString();

            // update name
            gameObject.name = "Research" + unlockable.Id;

            // update color
            currentColor = Unlocked ? backgroundColorUnlocked : backgroundColorLocked;
            backgroundImage.color = currentColor;
        }

        void OnValidate()
        {
            Awake();
            UpdateView();
        }

        public bool CanBeUnlocked()
        {
            if (Unlocked) return false;
            if (playerResearchPoints < unlockable.ResearchCost) return false;

            foreach (ResearchNode node in requiredParentNodes)
            {
                if (!node.Unlocked) return false;
            }

            return true;
        }

        public void Unlock()
        {
            Debug.Log("Unlocked unlockable ID: " + unlockable.Id + ".");
            Unlocked = true;
            UpdateView();

            ResearchUnlockedRecipes.Instance.Add(unlockable.Id);
        }

        public void ClickAction()
        {
            if (CanBeUnlocked()) Unlock();
        }

        private List<LineRenderer> connectionLines = new List<LineRenderer>();
        void DrawConnectionLines()
        {
            if (requiredParentNodes == null || requiredParentNodes.Length == 0 || lineRenderer == null)
            {
                lineRenderer.positionCount = 0; // No lines to draw
                return;
            }

            // Set the number of points for the LineRenderer (each connection needs 2 points)
            lineRenderer.positionCount = requiredParentNodes.Length * 2;

            int index = 0;
            foreach (var parent in requiredParentNodes)
            {
                if (parent != null)
                {
                    lineRenderer.SetPosition(index++, parent.transform.position); // Start point (parent)
                    lineRenderer.SetPosition(index++, transform.position); // End point (current node)
                }
            }

            // Optionally, set the color depending on whether the node is unlocked or not
            lineRenderer.startColor = lineRenderer.endColor = unlockable.Unlocked ? Color.green : Color.red;
        }

        void Update()
        {
            DrawConnectionLines();
        }
    }
}

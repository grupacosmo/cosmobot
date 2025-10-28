using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cosmobot.Utils
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(TMP_InputField))]
    public class TMPHorizontalScroll : UIBehaviour, ILayoutSelfController
    {
        private RectTransform inputFieldRt;
        private RectTransform inputFieldTextRt;
        
        private DrivenRectTransformTracker tracker;

        private RectTransform InputFieldRt
        {
            get
            {
                if (inputFieldRt == null)
                {
                    inputFieldRt = GetComponent<RectTransform>();
                } 
                return inputFieldRt;
            }
        }
        
        private RectTransform InputFieldTextRt
        {
            get
            {
                if (inputFieldTextRt == null)
                {
                    inputFieldTextRt = GetComponentInChildren<TMP_Text>().GetComponent<RectTransform>();
                } 
                return inputFieldTextRt;
            }
        }
        
        public void SetLayoutHorizontal()
        {
            UpdateLayout();   
        }

        public void SetLayoutVertical() {}

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(InputFieldRt);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void UpdateLayout()
        {
            tracker.Add(this, InputFieldRt, DrivenTransformProperties.SizeDeltaX);
            float inputFieldWidth = InputFieldRt.rect.size.x;
            float textPreferredWidth = LayoutUtility.GetPreferredWidth(InputFieldTextRt);
            float preferredWidth = Mathf.Max(inputFieldWidth, textPreferredWidth);
            InputFieldRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;
            
            LayoutRebuilder.MarkLayoutForRebuild(InputFieldRt);
        }
    }
}

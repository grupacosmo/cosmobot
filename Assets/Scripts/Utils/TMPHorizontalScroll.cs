using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cosmobot.Utils
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class TMPHorizontalScroll : UIBehaviour, ILayoutSelfController
    {
        [SerializeField]
        private TMP_InputField target;
        
        public bool expandWidthInputFieldToParent = false;
        [Tooltip("This value will be added to RectTransform width to accomodate for future updates")]
        public float additionalExpand = 0;

        [SerializeField]
        [Tooltip("ScrollRect containing this TMPHorizontalScroll")]
        private ScrollRect targetScrollRect;
        
        private float previousCharCaretPositionX = 0;

        private RectTransform parentRt;
        private RectTransform currentRectTransform;
        private RectTransform inputFieldTextRt;

        private DrivenRectTransformTracker tracker;

        private Vector3 CharacterCaretPos => target.textComponent.textInfo.characterInfo[target.caretPosition].topLeft;

        protected RectTransform ParentRt
        {
            get
            {
                if  (parentRt == null)
                    parentRt = transform.parent.GetComponent<RectTransform>();
                return parentRt;
            }
        }

        protected RectTransform CurrentRectTransform
        {
            get
            {
                if (currentRectTransform == null)
                    currentRectTransform = GetComponent<RectTransform>();
                return currentRectTransform;
            }
        }

        protected RectTransform InputFieldTextRt
        {
            get
            {
                if (inputFieldTextRt == null)
                    inputFieldTextRt = target.textComponent.GetComponent<RectTransform>();
                return inputFieldTextRt;
            }
        }

        // == impl ILayoutSelfController

        public void SetLayoutHorizontal()
        {
            UpdateLayout();   
        }

        public void SetLayoutVertical() {}


        // == Unity Events

        protected override void OnEnable()
        {
            base.OnEnable();
            target?.onValueChanged?.AddListener(OnTargetTextChanged);
            SetDirty();
        }

        protected override void OnDisable()
        {
            target?.onValueChanged?.RemoveListener(OnTargetTextChanged);
            tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(CurrentRectTransform);
            base.OnDisable();
        }

        private void Update()
        {
#if UNITY_EDITOR
            // coz [ExecuteAlways]
            if (!Application.isPlaying) return;
#endif
            float currentCaretPosition = CharacterCaretPos.x;
            if (!Mathf.Approximately(previousCharCaretPositionX, currentCaretPosition))
            {
                ScrollToCaret();
                previousCharCaretPositionX = currentCaretPosition;
            }
        }

        // == extends UIBehaviour
        protected override void OnTransformParentChanged()
        {
            SetDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        // ==
        
        protected void SetDirty()
        {
            if (!IsActive())
                return;
            
            LayoutRebuilder.MarkLayoutForRebuild(CurrentRectTransform);
            // if (target) LayoutRebuilder.MarkLayoutForRebuild(InputFieldTextRt);
        }

        private void OnTargetTextChanged(string newText)
        {
            SetDirty();
        }

        private void ScrollToCaret()
        {
            Vector3 caretPositionInTextComponent = CharacterCaretPos;

            Vector3 caretPositionGlobal = target.textComponent.transform.position + caretPositionInTextComponent;
            Vector3 caretPositionInContent = targetScrollRect.content.InverseTransformPoint(caretPositionGlobal);
            Vector3 caretPositionInViewport = targetScrollRect.viewport.InverseTransformPoint(caretPositionGlobal);
            float contentWidth = targetScrollRect.content.rect.width;
            Rect viewportRect = targetScrollRect.viewport.rect;
            
            float offsetCursorAlignment = 0;
            if (caretPositionInViewport.x > (viewportRect.width - additionalExpand))
            {
                offsetCursorAlignment = -viewportRect.xMax + additionalExpand;
            }
            else if (caretPositionInViewport.x < 0)
            {
                offsetCursorAlignment = 0;
            }
            else
            {
                return;
            }
            
            float viewportWidth = viewportRect.width;
            float scrollPosition = (caretPositionInContent.x + offsetCursorAlignment) / (contentWidth - viewportWidth);

            float size = viewportWidth / contentWidth;
            targetScrollRect.horizontalScrollbar.size = size;
            targetScrollRect.horizontalScrollbar.value = scrollPosition;
        }

        private void UpdateLayout()
        {
            if (target == null) return;
            
            tracker.Add(this, CurrentRectTransform, DrivenTransformProperties.SizeDeltaX);
            
            float parentWidth = ParentRt.rect.width;
            float textPreferredWidth = LayoutUtility.GetPreferredWidth(InputFieldTextRt) + additionalExpand;
            float minWidth = expandWidthInputFieldToParent ? parentWidth : 0;
            float preferredWidth = Mathf.Max(textPreferredWidth, minWidth);
            
            CurrentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
        }

#if UNITY_EDITOR
        // == Editor stuff
        
        protected override void OnValidate()
        {
            if (target == null)
            {
                Debug.Log("Target field is required | " + nameof(TMPHorizontalScroll), this);
                return;
            }

            if (!target.transform.IsChildOf(transform))
            {
                Debug.LogError($"The {target.name} does not belong to the transform {transform.name}", this);
                target = null;
            }
            
            SetDirty();
            if (ParentRt == null)
            {
                Debug.LogError("parent does not have a RectTransform", this);
            }
        }
#endif
    }
}

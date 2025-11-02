using System;
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
        // TODO allow TMPHorizontalScroll on any RectTransform 
        //  - will set size of its own RectTransform
        //    based on Text and its parent
        
        [SerializeField]
        private TMP_InputField target;
        
        public bool expandWidthInputFieldToParent = false;
        [Tooltip("This value will be added to RectTransform width to accomodate for future updates")]
        public float additionalExpand = 0;

        [SerializeField]
        private ScrollRect targetScrollRect;
        
        private RectTransform parentRt;
        private RectTransform currentRectTransform;
        private RectTransform inputFieldTextRt;
        
        private DrivenRectTransformTracker tracker;

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
    
        public void SetLayoutHorizontal()
        {
            UpdateLayout();   
        }

        public void SetLayoutVertical() {}

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
        
        protected override void OnTransformParentChanged()
        {
            SetDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }
        
        private void OnTargetTextChanged(string newText)
        {
            SetDirty();
        }

        private float previousCharCaretPositionX = 0;
        
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

        private Vector3 CharacterCaretPos => target.textComponent.textInfo.characterInfo[target.caretPosition].topLeft;
        
        private void ScrollToCaret()
        {
            
            // float scrollPosition = 
            //     (m_TextComponent.textInfo.lineInfo[0].ascender 
            //         + m_TextComponent.margin.y 
            //         + m_TextComponent.margin.w 
            //         - viewportRect.yMax 
            //         + m_TextComponent.rectTransform.anchoredPosition.y)
            //     / ( m_TextComponent.preferredHeight - viewportRect.height);


            RectTransform targetScrollRectRt = (RectTransform)targetScrollRect.transform;
            Vector3 caretPositionInTextComponent = CharacterCaretPos;

            Vector3 caretPositionGlobal = target.textComponent.transform.position + caretPositionInTextComponent;
            Vector3 caretPositionInContent = targetScrollRect.content.InverseTransformPoint(caretPositionGlobal);
            Vector3 caretPositionInViewport = targetScrollRect.viewport.InverseTransformPoint(caretPositionGlobal);
            Debug.Log(caretPositionInViewport.x);
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
            


            //
            // Vector3 caretPositionGlobal = target.textComponent.transform.position + caretPositionInTextComponent;
            // Vector3 caretPositionInScroll = targetScrollRectRt.InverseTransformPoint(caretPositionGlobal);
            // Vector3 contentPositionInScroll = targetScrollRectRt.InverseTransformPoint(targetScrollRect.content.position);
            // Vector3 difference = contentPositionInScroll - caretPositionInScroll;
            //
            // float differenceX = difference.x;
            // float scrollRectWidth = targetScrollRectRt.rect.width;
            // // coz content is wider than actual scroll rect, normalizedDifferenceX can be <0 or >1
            // float normalizedDifferenceX =  differenceX / scrollRectWidth;  
            // if (normalizedDifferenceX < 0 || normalizedDifferenceX > 1) // is outside displayed bounds
            // {
            //     float contentNormalizedDifferenceX = differenceX / targetScrollRect.content.rect.width;
            //     // TODO: check to align left/right
            //     targetScrollRect.horizontalScrollbar.value = 1 - contentNormalizedDifferenceX;
            // }
        }

        private void UpdateLayout()
        {
            if (!target) return;
            
            tracker.Add(this, CurrentRectTransform, DrivenTransformProperties.SizeDeltaX);
            
            float parentWidth = ParentRt.rect.width;
            float textPreferredWidth = LayoutUtility.GetPreferredWidth(InputFieldTextRt) + additionalExpand;
            float minWidth = expandWidthInputFieldToParent ? parentWidth : 0;
            float preferredWidth = Mathf.Max(textPreferredWidth, minWidth);
            
            CurrentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;
            
            LayoutRebuilder.MarkLayoutForRebuild(CurrentRectTransform);
            // if (target) LayoutRebuilder.MarkLayoutForRebuild(InputFieldTextRt);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (!target)
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

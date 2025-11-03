using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cosmobot.Utils.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class TMPScrollable : UIBehaviour, ILayoutSelfController
    {
        [SerializeField]
        private TMP_InputField target;
        
        public bool expandWidthInputFieldToParent = true;
        public bool expandHeightInputFieldToParent = true;
        [Tooltip("This value will be added to RectTransform width to accomodate for future updates")]
        public Vector2 additionalExpand = Vector2.zero;

        [SerializeField]
        [Tooltip("Independent RectTransform to set minimal area for InputField. " +
                 "This should be independent (in object tree) from InputField and " + nameof(TMPScrollable) +
                 " element")]
        private RectTransform minimalSizeRect;
        
        [SerializeField]
        [Tooltip("ScrollRect containing this " + nameof(TMPScrollable))]
        private ScrollRect targetScrollRect;

        [SerializeField]
        [InspectorName("Vertical scroll bars to sync position")]
        private List<Scrollbar> vertScrollBarsToSync;
        [SerializeField]
        [InspectorName("Horizontal scroll bars to sync position")]
        private List<Scrollbar> horScrollBarsToSync;
        
        private Vector2 previousCharCaretPosition = Vector2.zero;

        private RectTransform currentRectTransform;
        private RectTransform inputFieldTextRt;

        private DrivenRectTransformTracker tracker;

        private TMP_CharacterInfo CharacterInfoAtCursor =>
            target.textComponent.textInfo.characterInfo[target.caretPosition];
        private Vector3 CharacterCaretPos => CharacterInfoAtCursor.topLeft;

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

        public void SetLayoutVertical()
        {
            UpdateLayout();
        }
        
        // == Unity Events

        protected override void OnEnable()
        {
            base.OnEnable();
            target?.onValueChanged?.AddListener(OnTargetTextChanged);
            targetScrollRect.horizontalScrollbar.onValueChanged.AddListener(OnHorizontalScroll);
            targetScrollRect.verticalScrollbar.onValueChanged.AddListener(OnVerticalScroll);
            // small change to sync to non 0/1 value
            targetScrollRect.verticalScrollbar.value += 0.001f;
            targetScrollRect.horizontalScrollbar.value += 0.001f;
            SetDirty();
        }

        protected override void OnDisable()
        {
            targetScrollRect.horizontalScrollbar.onValueChanged.RemoveListener(OnHorizontalScroll);
            targetScrollRect.verticalScrollbar.onValueChanged.RemoveListener(OnVerticalScroll);
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
            Vector2 currentCaretPosition = CharacterCaretPos;
            
            if (Vector2.SqrMagnitude(previousCharCaretPosition - currentCaretPosition) > Mathf.Epsilon)
            {
                ScrollToCaret();
                previousCharCaretPosition = currentCaretPosition;
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

        private void OnTargetTextChanged(string newText)
        {
            SetDirty();
        }
        
        protected void SetDirty()
        {
            if (!IsActive())
                return;
            
            LayoutRebuilder.MarkLayoutForRebuild(CurrentRectTransform);
        }
        
        private void OnHorizontalScroll(float _)
        {
            foreach (Scrollbar sb in horScrollBarsToSync)
            {
                sb.size = targetScrollRect.horizontalScrollbar.size;
                sb.value = targetScrollRect.horizontalScrollbar.value;
            }    
        }
        
        private void OnVerticalScroll(float _)
        {
            foreach (Scrollbar sb in vertScrollBarsToSync)
            {
                sb.size = targetScrollRect.verticalScrollbar.size;
                sb.value = targetScrollRect.verticalScrollbar.value;
            }
        }
        
        private void ScrollToCaret()
        {
            Vector3 caretPositionInTextComponent = CharacterCaretPos;
            float caretHeight = CharacterInfoAtCursor.ascender - CharacterInfoAtCursor.descender;

            Vector3 caretPositionGlobal = target.textComponent.transform.position + caretPositionInTextComponent;
            Vector3 caretPositionInContent = targetScrollRect.content.InverseTransformPoint(caretPositionGlobal);
            Vector3 caretPositionInViewport = targetScrollRect.viewport.InverseTransformPoint(caretPositionGlobal);
            Vector2 contentSize = targetScrollRect.content.rect.size;
            Rect viewportRect = targetScrollRect.viewport.rect;

            bool scrollX = true;
            bool scrollY = true;
            Vector2 offsetCursorAlignment = Vector2.zero;
            if (caretPositionInViewport.x > (viewportRect.width - additionalExpand.x))
                offsetCursorAlignment.x = -viewportRect.xMax + additionalExpand.x;
            else if (caretPositionInViewport.x < 0)
                offsetCursorAlignment.x = 0;
            else
                scrollX = false;
            
            if (caretPositionInViewport.y > (viewportRect.height - additionalExpand.y))
                offsetCursorAlignment.y = -viewportRect.yMax + additionalExpand.y + caretHeight;
            else if (caretPositionInViewport.y < 0)
                offsetCursorAlignment.y = 0;
            else
                scrollY = false;

            Vector2 caretPositionInContentV2 = caretPositionInContent;
            // component-wise divide
            Vector2 scrollPosition = (caretPositionInContentV2 + offsetCursorAlignment) / (contentSize - viewportRect.size);
            Vector2 scrollSize = viewportRect.size / contentSize;
            if (!float.IsFinite(scrollPosition.x)) scrollPosition.x = 0;
            if (!float.IsFinite(scrollPosition.y)) scrollPosition.y = 0;
            
            if (scrollX)
            {
                targetScrollRect.horizontalScrollbar.size = scrollSize.x;
                targetScrollRect.horizontalScrollbar.value = scrollPosition.x;
                OnHorizontalScroll(scrollPosition.x);
            }

            if (scrollY)
            {
                targetScrollRect.verticalScrollbar.size = scrollSize.y;
                targetScrollRect.verticalScrollbar.value = scrollPosition.y;
                OnVerticalScroll(scrollPosition.y);
            }
        }

        private void UpdateLayout()
        {
            if (target == null) return;
            
            tracker.Add(this, CurrentRectTransform, DrivenTransformProperties.SizeDelta);

            float lineHeight = CharacterInfoAtCursor.ascender - CharacterInfoAtCursor.descender;
            
            float textPreferredWidth = LayoutUtility.GetPreferredWidth(InputFieldTextRt) + additionalExpand.x;
            float textPreferredHeight = LayoutUtility.GetPreferredHeight(InputFieldTextRt) + additionalExpand.y;
            float minWidth = expandWidthInputFieldToParent ? minimalSizeRect.rect.width : 0;
            float minHeight = expandHeightInputFieldToParent ? minimalSizeRect.rect.height : 0;
            float preferredWidth = Mathf.Max(textPreferredWidth, minWidth);
            float preferredHeight = Mathf.Max(textPreferredHeight + lineHeight, minHeight);
            
            CurrentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
            CurrentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
            OnHorizontalScroll(targetScrollRect.horizontalScrollbar.value);
            OnVerticalScroll(targetScrollRect.verticalScrollbar.value);
        }

#if UNITY_EDITOR
        // == Editor stuff
        
        protected override void OnValidate()
        {
            if (target == null)
            {
                Debug.Log("Target field is required | " + nameof(TMPScrollable), this);
                return;
            }

            if (!target.transform.IsChildOf(transform))
            {
                Debug.LogError($"The {target.name} does not belong to the transform {transform.name}", this);
                target = null;
            }
            
            SetDirty();
            if (minimalSizeRect && target)
            {
                bool minimalTarget = minimalSizeRect.IsChildOf(target.transform) || target.transform.IsChildOf(minimalSizeRect);
                bool minimalThis = minimalSizeRect.IsChildOf(transform) || transform.IsChildOf(minimalSizeRect);
                if (minimalTarget || minimalThis)
                {
                    Debug.LogWarning(
                        $"The {minimalSizeRect.name} belong to the transform {transform.name} or {target.name}. " +
                        $"This can cause unexpected behaviour", this);
                }
            }
        }
#endif
    }
}

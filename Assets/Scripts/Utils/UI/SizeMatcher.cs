using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cosmobot.Utils.UI
{
    [AddComponentMenu("Layout/SizeMatcher", 900)]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class SizeMatcher : UIBehaviour, ILayoutElement, ILayoutIgnorer
    {
        public enum MatchType
        {
            None,
            MinSize,
            PrefferedSize,
            FlexibleSize
        }
        
        [InspectorName("layoutPriority")]
        public int layoutPriorityField = 1;
        
        [SerializeField]
        private RectTransform target;
        
        public MatchType matchWidth;
        public MatchType matchHeight;
        
        public float minWidth => matchWidth == MatchType.MinSize ? (target?.rect.width ?? 0) : -1;
        public float minHeight => matchHeight == MatchType.MinSize ? (target?.rect.height ?? 0) : -1;
        public float preferredWidth => matchWidth == MatchType.PrefferedSize ? (target?.rect.width ?? 0) : -1;
        public float preferredHeight => matchHeight == MatchType.PrefferedSize ? (target?.rect.height ?? 0) : -1;
        public float flexibleWidth => matchWidth == MatchType.FlexibleSize ? (target?.rect.width ?? 0) : -1;
        public float flexibleHeight => matchHeight == MatchType.FlexibleSize ? (target?.rect.height ?? 0) : -1;
        public int layoutPriority => layoutPriorityField;
        
        public bool ignoreLayout => false;

        private Vector2 previousSize;
        
        public void CalculateLayoutInputHorizontal() { 
        }
        public void CalculateLayoutInputVertical() {
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cosmobot.Utils.UI
{
    public class ForwardScrollEvents : UIBehaviour, IScrollHandler
    {
        [SerializeField]
        private ScrollRect target;

        public void OnScroll(PointerEventData eventData)
        {
            target.OnScroll(eventData);
            eventData.Use();
        }
    }
}

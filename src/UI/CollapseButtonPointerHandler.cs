using UnityEngine;
using UnityEngine.EventSystems;

namespace KingdomDistributionPlus.UI
{
    internal sealed class CollapseButtonPointerHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private JunctionCollapseButton owner;

        public void Bind(JunctionCollapseButton collapseOwner)
        {
            owner = collapseOwner;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!owner || eventData == null)
                return;

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                owner.HandleRemoveSingleSlot();
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left)
                owner.HandleCollapseClick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (owner)
                owner.SetCollapseButtonHovered(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (owner)
                owner.SetCollapseButtonHovered(false);
        }
    }
}

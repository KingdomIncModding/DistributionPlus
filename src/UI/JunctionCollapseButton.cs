using KingdomDistributionPlus.Core;
using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomDistributionPlus.UI
{
    internal sealed class JunctionCollapseButton : MonoBehaviour
    {
        private const string CollapseButtonName = "DistributionPlusCollapseButton";

        private WorkspaceObject junction;
        private GameObject collapseButtonRoot;
        private Image collapseBorder;
        private Image collapseIcon;
        private bool buildingHovered;
        private bool collapseButtonHovered;
        private int lastObservedFreeSlots = -1;

        public static JunctionCollapseButton Ensure(Component buildingView, WorkspaceObject junctionModel)
        {
            if (!buildingView || junctionModel == null || !junctionModel.IsJunction)
                return null;

            var controller = buildingView.GetComponent<JunctionCollapseButton>();
            if (!controller)
                controller = buildingView.gameObject.AddComponent<JunctionCollapseButton>();

            controller.Bind(junctionModel);
            return controller;
        }

        private void Update()
        {
            if (!buildingHovered || junction == null || !junction.IsJunction)
                return;

            int freeSlots = junction.TrailingFreeJunctionSlots;
            if (freeSlots != lastObservedFreeSlots)
                RefreshVisualState();
        }

        private void OnDisable()
        {
            buildingHovered = false;
            collapseButtonHovered = false;
            ApplyHoverColors();
        }

        private void OnDestroy()
        {
            DestroyCollapseButton();
        }

        public void Bind(WorkspaceObject junctionModel)
        {
            junction = junctionModel;
            EnsureCollapseButtonCreated();
        }

        public void SetBuildingHovered(bool hovered)
        {
            buildingHovered = hovered;
            RefreshVisualState();
        }

        public void SetCollapseButtonHovered(bool hovered)
        {
            collapseButtonHovered = hovered;
            ApplyHoverColors();
        }

        public void HandleCollapseClick()
        {
            if (junction == null || !junction.IsJunction)
                return;

            if (DistributionPlusPlugin.IsShiftHeld())
            {
                HandleRemoveSingleSlot();
                return;
            }

            if (junction.TrailingFreeJunctionSlots <= 0)
                return;

            junction.CollapseJunctionSlots();
            FoundationApi.UiAccess.PlaySound(GameUiSound.TurnPage);
            RefreshVisualState();
        }

        public void HandleRemoveSingleSlot()
        {
            if (junction == null || !junction.IsJunction)
                return;

            if (junction.TrailingFreeJunctionSlots <= 0)
                return;

            if (junction.SetJunctionSlotCount(junction.JunctionSlotCount - 1))
                FoundationApi.UiAccess.PlaySound(GameUiSound.TurnPage);

            RefreshVisualState();
        }

        public void RefreshVisualState()
        {
            if (junction == null || !junction.IsJunction)
                return;

            SyncNativeButtonIcon();
            EnsureCollapseButtonCreated();

            var plugin = DistributionPlusPlugin.Instance;
            int freeSlots = junction.TrailingFreeJunctionSlots;
            lastObservedFreeSlots = freeSlots;
            bool showButton = plugin != null
                && plugin.Settings.UnlimitedSlots
                && plugin.Settings.ShowCollapseButton
                && freeSlots > 0;

            if (collapseButtonRoot)
            {
                if (collapseButtonRoot.activeSelf != showButton)
                    collapseButtonRoot.SetActive(showButton);

                if (!showButton)
                    collapseButtonHovered = false;
            }

            ApplyHoverColors();
        }

        public void RestoreVanillaState()
        {
            if (junction != null && junction.IsJunction)
            {
                var visuals = FoundationApi.WorkspaceVisuals;
                var nativeIcon = visuals.JunctionButtonIcon(this);
                var addSprite = visuals.JunctionAddIcon(this);
                var collapseSprite = visuals.JunctionCollapseIcon(this);
                if (nativeIcon && addSprite && collapseSprite)
                    nativeIcon.sprite = junction.JunctionSlotCount < 4 ? addSprite : collapseSprite;
            }

            DestroyCollapseButton();
        }

        private void SyncNativeButtonIcon()
        {
            var plugin = DistributionPlusPlugin.Instance;
            if (plugin == null)
                return;

            var visuals = FoundationApi.WorkspaceVisuals;
            var nativeIcon = visuals.JunctionButtonIcon(this);
            var addSprite = visuals.JunctionAddIcon(this);
            var collapseSprite = visuals.JunctionCollapseIcon(this);
            if (!nativeIcon || !addSprite || !collapseSprite)
                return;

            nativeIcon.sprite = plugin.CanAddMoreSlots(junction) ? addSprite : collapseSprite;
        }

        private void EnsureCollapseButtonCreated()
        {
            if (collapseButtonRoot)
                return;

            var visuals = FoundationApi.WorkspaceVisuals;
            var nativeBorder = visuals.JunctionButtonBorder(this);
            var collapseSprite = visuals.JunctionCollapseIcon(this);
            if (!nativeBorder || !collapseSprite)
                return;

            var clone = Instantiate(nativeBorder.gameObject, nativeBorder.transform.parent);
            clone.name = CollapseButtonName;
            FoundationApi.UiAccess.RemoveNativeTooltips(clone);

            PositionOnOppositeCorner(nativeBorder.rectTransform, clone.transform as RectTransform);

            collapseButtonRoot = clone;
            collapseBorder = clone.GetComponent<Image>();

            var button = clone.GetComponentInChildren<Button>(true);
            if (button)
            {
                button.onClick = new Button.ButtonClickedEvent();
                collapseIcon = button.GetComponent<Image>();
                if (collapseIcon)
                    collapseIcon.sprite = collapseSprite;

                var pointerHandler = button.gameObject.AddComponent<CollapseButtonPointerHandler>();
                pointerHandler.Bind(this);
            }

            ApplyHoverColors();
        }

        private static void PositionOnOppositeCorner(RectTransform sourceRect, RectTransform targetRect)
        {
            if (!sourceRect || !targetRect)
                return;

            bool sourceIsLeft = sourceRect.anchorMin.x < 0.5f;
            float anchorX = sourceIsLeft ? 1f : 0f;
            float offsetX = Mathf.Abs(sourceRect.anchoredPosition.x);

            targetRect.anchorMin = new Vector2(anchorX, sourceRect.anchorMin.y);
            targetRect.anchorMax = new Vector2(anchorX, sourceRect.anchorMax.y);
            targetRect.pivot = new Vector2(anchorX, sourceRect.pivot.y);
            targetRect.sizeDelta = sourceRect.sizeDelta;
            targetRect.anchoredPosition = new Vector2(sourceIsLeft ? -offsetX : offsetX, sourceRect.anchoredPosition.y);
        }

        private void ApplyHoverColors()
        {
            if (!collapseButtonRoot || !collapseButtonRoot.activeSelf)
                return;

            Color color = (buildingHovered || collapseButtonHovered) ? Color.white : Color.clear;
            if (collapseBorder)
                collapseBorder.color = color;

            if (collapseIcon)
                collapseIcon.color = color;
        }

        private void DestroyCollapseButton()
        {
            if (collapseButtonRoot)
            {
                Destroy(collapseButtonRoot);
                collapseButtonRoot = null;
            }

            collapseBorder = null;
            collapseIcon = null;
            collapseButtonHovered = false;
        }
    }
}

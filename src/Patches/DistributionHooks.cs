using System;
using System.Collections.Generic;
using KingdomDistributionPlus.Core;
using KingdomDistributionPlus.UI;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomDistributionPlus.Patches
{
    internal sealed class DistributionHooks : IDisposable
    {
        private readonly DistributionPlusPlugin plugin;
        private readonly List<IDisposable> subscriptions = new List<IDisposable>();

        public DistributionHooks(DistributionPlusPlugin plugin)
        {
            this.plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        public void Install()
        {
            subscriptions.Add(FoundationApi.GameHooks.Subscribe(
                DistributionPlusPlugin.PluginId,
                GameHookPoints.JunctionStarted,
                after: OnJunctionStarted));

            subscriptions.Add(FoundationApi.GameHooks.Subscribe(
                DistributionPlusPlugin.PluginId,
                GameHookPoints.JunctionSlotAction,
                before: OnJunctionSlotActionBefore));

            subscriptions.Add(FoundationApi.GameHooks.Subscribe(
                DistributionPlusPlugin.PluginId,
                GameHookPoints.JunctionButtonState,
                after: OnJunctionButtonStateAfter));

            subscriptions.Add(FoundationApi.GameHooks.Subscribe(
                DistributionPlusPlugin.PluginId,
                GameHookPoints.JunctionPointerEnter,
                after: OnJunctionPointerEnter));

            subscriptions.Add(FoundationApi.GameHooks.Subscribe(
                DistributionPlusPlugin.PluginId,
                GameHookPoints.JunctionPointerExit,
                after: OnJunctionPointerExit));

            subscriptions.Add(FoundationApi.GameHooks.Subscribe(
                DistributionPlusPlugin.PluginId,
                GameHookPoints.GhostBound,
                after: OnGhostBound));

            subscriptions.Add(FoundationApi.GameHooks.Subscribe(
                DistributionPlusPlugin.PluginId,
                GameHookPoints.WorkspaceShown,
                after: OnWorkspaceShown));

            subscriptions.Add(FoundationApi.GameHooks.Subscribe(
                DistributionPlusPlugin.PluginId,
                GameHookPoints.PipeBound,
                after: OnPipeBound));
        }

        public void Dispose()
        {
            for (int i = 0; i < subscriptions.Count; i++)
                subscriptions[i]?.Dispose();

            subscriptions.Clear();
        }

        private void OnJunctionStarted(GameHookCall call)
        {
            if (!TryResolveJunction(call.View, out var buildingView, out var junction))
                return;

            var controller = JunctionCollapseButton.Ensure(buildingView, junction);
            controller?.RefreshVisualState();
        }

        private void OnJunctionSlotActionBefore(GameHookCall call)
        {
            if (!plugin.Settings.UnlimitedSlots)
                return;

            if (!TryResolveJunction(call.View, out var buildingView, out var junction))
                return;

            call.Cancel();

            if (plugin.CanAddMoreSlots(junction))
            {
                int target = plugin.ComputeNextSlotCount(junction, DistributionPlusPlugin.IsShiftHeld());
                if (junction.SetJunctionSlotCount(target))
                    FoundationApi.UiAccess.PlaySound(GameUiSound.CraftStep);
            }
            else if (junction.TrailingFreeJunctionSlots > 0)
            {
                junction.CollapseJunctionSlots();
                FoundationApi.UiAccess.PlaySound(GameUiSound.TurnPage);
            }
            else
            {
                FoundationApi.UiAccess.PlaySound(GameUiSound.TurnPage);
            }

            var controller = JunctionCollapseButton.Ensure(buildingView, junction);
            controller?.RefreshVisualState();
        }

        private void OnJunctionButtonStateAfter(GameHookCall call)
        {
            if (!TryResolveJunction(call.View, out var buildingView, out var junction))
                return;

            var controller = JunctionCollapseButton.Ensure(buildingView, junction);
            controller?.RefreshVisualState();
        }

        private void OnJunctionPointerEnter(GameHookCall call)
        {
            if (!TryResolveJunction(call.View, out var buildingView, out var junction))
                return;

            var controller = JunctionCollapseButton.Ensure(buildingView, junction);
            controller?.SetBuildingHovered(true);
        }

        private void OnJunctionPointerExit(GameHookCall call)
        {
            if (!TryResolveJunction(call.View, out var buildingView, out var junction))
                return;

            var controller = buildingView.GetComponent<JunctionCollapseButton>();
            controller?.SetBuildingHovered(false);
        }

        private void OnGhostBound(GameHookCall call)
        {
            if (!plugin.Settings.MatchGhostSize || !call.View)
                return;

            var element = call.Argument<WorkspaceObject>(0);
            if (element == null || !element.IsJunction)
                return;

            float targetHeight = element.Size.y;
            if (targetHeight <= 0f)
                return;

            var rect = call.View.transform as RectTransform;
            if (rect)
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, targetHeight);
        }

        private void OnWorkspaceShown(GameHookCall call)
        {
            plugin.SweepWorkspaceJunctions();
        }

        private void OnPipeBound(GameHookCall call)
        {
            if (!call.View)
                return;

            var pipe = FoundationApi.WorkspaceVisuals.PipeFromView(call.View);
            RefreshPortParent(pipe?.ConnectedInput);
            RefreshPortParent(pipe?.ConnectedOutput);
        }

        private static void RefreshPortParent(WorkspacePort port)
        {
            var parent = port?.Parent;
            if (parent == null || !parent.IsJunction || !parent.View)
                return;

            var controller = parent.View.GetComponent<JunctionCollapseButton>();
            controller?.RefreshVisualState();
        }

        private static bool TryResolveJunction(Component view, out Component buildingView, out WorkspaceObject junction)
        {
            buildingView = null;
            junction = null;
            if (!view)
                return false;

            buildingView = FoundationApi.WorkspaceAccess.FindBuildingView(view.gameObject);
            if (!buildingView)
                return false;

            junction = FoundationApi.WorkspaceAccess.FromView(buildingView);
            return junction != null && junction.IsJunction;
        }
    }
}

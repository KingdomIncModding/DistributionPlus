using System;
using BepInEx;
using KingdomDistributionPlus.Configuration;
using KingdomDistributionPlus.Patches;
using KingdomDistributionPlus.UI;
using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomDistributionPlus.Core
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    [BepInDependency("urakka.kingdominc.foundation", "0.14.7")]
    public sealed class DistributionPlusPlugin : BaseUnityPlugin
    {
        public const string PluginId = "urakka.kingdominc.distributionplus";
        public const string PluginName = "Distribution+";
        public const string PluginVersion = "0.1.0";

        public static DistributionPlusPlugin Instance { get; private set; }
        public DistributionSettings Settings { get; private set; }

        private DistributionHooks hooks;
        private DistributionFoundationUi foundationUi;

        private void Awake()
        {
            Instance = this;
            Settings = new DistributionSettings(Config);

            hooks = new DistributionHooks(this);
            hooks.Install();

            foundationUi = new DistributionFoundationUi(this);
            foundationUi.Register();

            Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void Start()
        {
            SweepWorkspaceJunctions();
        }

        private void OnDestroy()
        {
            hooks?.Dispose();
            hooks = null;

            foundationUi?.Dispose();
            foundationUi = null;

            RestoreAndDestroyAllControllers();

            if (Instance == this)
                Instance = null;
        }

        public static bool IsShiftHeld()
        {
            return Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
        }

        public bool CanAddMoreSlots(WorkspaceObject junction)
        {
            if (junction == null || !junction.IsJunction)
                return false;

            if (!Settings.UnlimitedSlots)
                return junction.JunctionSlotCount < 4;

            int cap = Settings.OptionalMaxSlots;
            return cap <= 0 || junction.JunctionSlotCount < cap;
        }

        public int ComputeNextSlotCount(WorkspaceObject junction, bool shiftHeld)
        {
            int current = junction.JunctionSlotCount;
            int step = shiftHeld ? Settings.ShiftBatchSize : 1;
            int target = current + step;
            int cap = Settings.OptionalMaxSlots;
            if (cap > 0 && target > cap)
                target = cap;

            return target;
        }

        public void SweepWorkspaceJunctions()
        {
            try
            {
                if (!FoundationApi.WorkspaceAccess.IsReady)
                    return;

                var objects = FoundationApi.WorkspaceAccess.AllObjects;
                for (int i = 0; i < objects.Count; i++)
                {
                    var item = objects[i];
                    if (item == null || !item.IsJunction || !item.View)
                        continue;

                    var controller = JunctionCollapseButton.Ensure(item.View, item);
                    controller?.RefreshVisualState();
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug($"SweepWorkspaceJunctions error: {ex.Message}");
            }
        }

        public void RefreshAllJunctions()
        {
            try
            {
                SweepWorkspaceJunctions();
                var controllers = FindObjectsByType<JunctionCollapseButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < controllers.Length; i++)
                {
                    if (controllers[i])
                        controllers[i].RefreshVisualState();
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug($"RefreshAllJunctions error: {ex.Message}");
            }
        }

        private static void RestoreAndDestroyAllControllers()
        {
            var controllers = FindObjectsByType<JunctionCollapseButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                var controller = controllers[i];
                if (!controller)
                    continue;

                controller.RestoreVanillaState();
                if (controller.gameObject.scene.IsValid())
                    Destroy(controller);
            }
        }
    }
}

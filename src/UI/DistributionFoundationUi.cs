using System;
using KingdomDistributionPlus.Configuration;
using KingdomDistributionPlus.Core;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomDistributionPlus.UI
{
    internal sealed class DistributionFoundationUi : IDisposable
    {
        private readonly DistributionPlusPlugin plugin;
        private FoundationModContext context;

        public DistributionFoundationUi(DistributionPlusPlugin plugin)
        {
            this.plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        public void Register()
        {
            try
            {
                context = FoundationApi.ForMod(DistributionPlusPlugin.PluginId);
                RegisterSettings();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            context?.Dispose();
            context = null;
        }

        private void RegisterSettings()
        {
            var settings = plugin.Settings;
            var section = context.Settings(
                "Distribution+",
                "Removes the 4-slot cap on Collectors, Distributors, and Manifolds, adds a companion collapse button, and supports batch slot expansion.");

            section.AddText("General", isHeader: true);

            section.AddToggle(
                "Enable Unlimited Slots",
                settings.UnlimitedSlots,
                val =>
                {
                    settings.SetUnlimitedSlots(val);
                    plugin.RefreshAllJunctions();
                },
                getter: () => settings.UnlimitedSlots);

            section.AddToggle(
                "Show Collapse Button",
                settings.ShowCollapseButton,
                val =>
                {
                    settings.SetShowCollapseButton(val);
                    plugin.RefreshAllJunctions();
                },
                getter: () => settings.ShowCollapseButton);

            section.AddToggle(
                "Match Ghost Size When Copying",
                settings.MatchGhostSize,
                val => settings.SetMatchGhostSize(val),
                getter: () => settings.MatchGhostSize);

            section.AddText("Controls & Limits", isHeader: true);

            section.AddSlider(
                "Shift+Click Add Batch Size",
                DistributionSettings.MinBatchSize,
                DistributionSettings.MaxBatchSize,
                settings.ShiftBatchSize,
                val => settings.SetShiftBatchSize(Mathf.RoundToInt(val)),
                step: 1f,
                getter: () => settings.ShiftBatchSize,
                formatValue: val => $"+{Mathf.RoundToInt(val)} slots");

            section.AddSlider(
                "Optional Slot Cap (0 = Unlimited)",
                0f,
                DistributionSettings.MaxOptionalCap,
                settings.OptionalMaxSlots,
                val =>
                {
                    settings.SetOptionalMaxSlots(Mathf.RoundToInt(val));
                    plugin.RefreshAllJunctions();
                },
                step: 1f,
                getter: () => settings.OptionalMaxSlots,
                formatValue: val =>
                {
                    int rounded = Mathf.RoundToInt(val);
                    if (rounded <= 0)
                        return "Unlimited";

                    int effective = rounded < 4 ? 4 : rounded;
                    return $"{effective} slots";
                });
        }
    }
}

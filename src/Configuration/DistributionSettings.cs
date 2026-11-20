using System;
using BepInEx.Configuration;
using UnityEngine;

namespace KingdomDistributionPlus.Configuration
{
    public sealed class DistributionSettings
    {
        public const int MinBatchSize = 2;
        public const int MaxBatchSize = 10;
        public const int MaxOptionalCap = 64;

        private readonly ConfigEntry<bool> unlimitedSlots;
        private readonly ConfigEntry<bool> showCollapseButton;
        private readonly ConfigEntry<int> shiftBatchSize;
        private readonly ConfigEntry<int> optionalMaxSlots;
        private readonly ConfigEntry<bool> matchGhostSize;

        public DistributionSettings(ConfigFile config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            unlimitedSlots = config.Bind(
                "General",
                "UnlimitedSlots",
                true,
                "Removes the vanilla 4-slot cap on Collectors, Distributors, and Manifolds.");

            showCollapseButton = config.Bind(
                "General",
                "ShowCollapseButton",
                true,
                "Shows a matching collapse button on the opposite bottom corner to shrink unused trailing slots anytime.");

            shiftBatchSize = config.Bind(
                "Controls",
                "ShiftBatchSize",
                5,
                new ConfigDescription(
                    "Number of slots added at once when holding Shift while clicking the + button.",
                    new AcceptableValueRange<int>(MinBatchSize, MaxBatchSize)));

            optionalMaxSlots = config.Bind(
                "General",
                "OptionalMaxSlots",
                0,
                new ConfigDescription(
                    "Optional personal slot limit (0 = unlimited).",
                    new AcceptableValueRange<int>(0, MaxOptionalCap)));

            matchGhostSize = config.Bind(
                "General",
                "MatchGhostSize",
                true,
                "Resizes blueprint copy previews to match the full height of expanded Collectors, Distributors, and Manifolds.");
        }

        public bool UnlimitedSlots => unlimitedSlots.Value;
        public bool ShowCollapseButton => showCollapseButton.Value;
        public int ShiftBatchSize => Mathf.Clamp(shiftBatchSize.Value, MinBatchSize, MaxBatchSize);
        public int OptionalMaxSlots => Mathf.Clamp(optionalMaxSlots.Value, 0, MaxOptionalCap);
        public bool MatchGhostSize => matchGhostSize.Value;

        public void SetUnlimitedSlots(bool value)
        {
            unlimitedSlots.Value = value;
        }

        public void SetShowCollapseButton(bool value)
        {
            showCollapseButton.Value = value;
        }

        public void SetShiftBatchSize(int value)
        {
            shiftBatchSize.Value = Mathf.Clamp(value, MinBatchSize, MaxBatchSize);
        }

        public void SetOptionalMaxSlots(int value)
        {
            int clamped = Mathf.Clamp(value, 0, MaxOptionalCap);
            if (clamped > 0 && clamped < 4)
                clamped = 4;

            optionalMaxSlots.Value = clamped;
        }

        public void SetMatchGhostSize(bool value)
        {
            matchGhostSize.Value = value;
        }
    }
}

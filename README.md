# Distribution+

[![Support on Ko-fi](https://raw.githubusercontent.com/KingdomIncModding/.github/main/assets/kofi-button.png)](https://ko-fi.com/urakkaamyx/tip)

Removes the 4-slot cap on **Collectors**, **Distributors**, and **Manifolds** so you can expand any junction to as many slots as your factory needs.

## Features

- **Unlimited Slots**: Expand Collectors, Distributors, and Manifolds past the vanilla 4-slot limit without chaining multiple junctions together.
- **Dedicated Collapse Button**: Because the `+` button no longer turns into Collapse at 4 slots, a matching vanilla-styled collapse button appears on the opposite bottom corner whenever unused trailing slots can be shrunk.
  - **Left-click** collapses all unused trailing slots down to the last connected slot (minimum 2).
  - **Right-click** or **Shift+Left-click** removes a single unused trailing slot at a time.
- **Batch Expansion**: Hold **Shift** while clicking `+` to add multiple slots at once (configurable, default `+5` slots).
- **Accurate Copy Ghosts**: Blueprint copies of expanded Collectors, Distributors, and Manifolds resize to match their full height before placement.
- **In-Game Settings**: Configure toggles, Shift+Click batch size, or an optional personal slot cap under **Options -> Mods -> Distribution+**.
- **Save-Safe**: Uses the game's native slot count format directly, so saves load cleanly with or without the mod installed.

## Installation

1. Install **BepInEx 5** for Kingdom Inc.
2. Install **Foundation 0.14.7** or newer.
3. Extract the `DistributionPlus` folder into `KingdomInc/BepInEx/plugins/`.
4. Launch the game.

## Compatibility

Distribution+ integrates with Kingdom Inc exclusively through **Foundation** game hooks and does not patch game files directly.

| Class.Method | Hook Type | Impact |
|---|---|---|
| WorldFlowCollector.Start | Foundation After Hook | Attaches the companion collapse button and syncs slot icons on Collectors. |
| WorldFlowCollector.AddInputSlot | Foundation Before Hook | Replaces the 4-slot limit with unlimited (or configured) expansion and Shift+Click batch adding on Collectors. |
| WorldFlowCollector._updateButtonState | Foundation After Hook | Keeps the Collector `+` icon active beyond 4 slots and refreshes the collapse button. |
| WorldFlowCollector.OnPointerEnter | Foundation After Hook | Reveals the Collector companion collapse button on hover. |
| WorldFlowCollector.OnPointerExit | Foundation After Hook | Hides the Collector companion collapse button when hover ends. |
| WorldFlowDistributor.Start | Foundation After Hook | Attaches the companion collapse button and syncs slot icons on Distributors. |
| WorldFlowDistributor.AddOutputSlot | Foundation Before Hook | Replaces the 4-slot limit with unlimited (or configured) expansion and Shift+Click batch adding on Distributors. |
| WorldFlowDistributor._updateButtonState | Foundation After Hook | Keeps the Distributor `+` icon active beyond 4 slots and refreshes the collapse button. |
| WorldFlowDistributor.OnPointerEnter | Foundation After Hook | Reveals the Distributor companion collapse button on hover. |
| WorldFlowDistributor.OnPointerExit | Foundation After Hook | Hides the Distributor companion collapse button when hover ends. |
| WorldFlowManifold.Start | Foundation After Hook | Attaches the companion collapse button and syncs slot icons on Manifolds. |
| WorldFlowManifold.AddOutputSlot | Foundation Before Hook | Replaces the 4-slot limit with unlimited (or configured) expansion and Shift+Click batch adding on Manifolds. |
| WorldFlowManifold._updateButtonState | Foundation After Hook | Keeps the Manifold `+` icon active beyond 4 slots and refreshes the collapse button. |
| WorldFlowManifold.OnPointerEnter | Foundation After Hook | Reveals the Manifold companion collapse button on hover. |
| WorldFlowManifold.OnPointerExit | Foundation After Hook | Hides the Manifold companion collapse button when hover ends. |
| WorldFlowGhost.SetElement | Foundation After Hook | Scales blueprint copy ghosts to match the full height of expanded junctions. |
| WorldFlowManager.ShowFlow | Foundation After Hook | Syncs junction controls when switching between workspaces. |
| WorldFlowPipe.SetFlowPipe | Foundation After Hook | Updates collapse button availability as pipes connect to junction ports. |

## Support

If you enjoy these Kingdom Inc mods and tools, consider supporting development on [**Ko-fi**](https://ko-fi.com/urakkaamyx/tip)!

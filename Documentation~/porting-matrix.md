# ShaderlabVSCode Porting Matrix

Reference source files:

- `Examples/ShaderlabVSCode/Editor/VSCodeBridge.cs`
- `Examples/ShaderlabVSCode/Editor/ShaderlabVSCodeEditor.cs`

## Mapping

| Legacy Component | Action | New Location | Notes |
| --- | --- | --- | --- |
| `OnOpenAsset` shader extension filtering | Reuse + simplify | `Editor/Bridge/RiderShaderAssetOpener.cs` | Keep only bridge-related behavior. |
| External editor process invocation | Reuse + adapt | `Editor/Bridge/RiderOpenService.cs` | Replace VSCode args with Rider args. |
| VSCode path probing | Modify | `Editor/Bridge/RiderOpenService.cs` | Resolve Rider editor path from Unity setting first. |
| VSCode menu links | Drop | N/A | Not part of Rider bridge scope. |
| VSCode extension data update | Drop | N/A | Not in Bridge+Diagnostics scope. |
| Script template installer | Drop | N/A | Not in Bridge+Diagnostics scope. |

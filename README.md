# Unity Shader IDE Bridge (Rider)

`com.clerin.unity.shader-ide-bridge-rider` is a Unity Editor package that improves opening Shader/HLSL files from Unity
in JetBrains Rider (Unity 6.3 baseline).
Keep C# scripts opening in your main IDE (for example Visual Studio 2026), while opening shader-related files in Rider.

## Features

- Automatically intercepts shader-related asset opens (double-click in Project window).
- Opens in Rider with line support when possible.
- Provides a manual menu action to open selected shader-related assets in Rider.
- Provides a diagnostics report to validate Rider setup.

## Supported File Types

- `.shader`
- `.compute`
- `.hlsl`
- `.cginc`
- `.glslinc`
- `.cg`

## Requirements

- Unity `6000.3.x` (Unity 6.3)
- JetBrains Rider installed
- Unity package `com.unity.ide.rider` (declared as a dependency in `package.json`)
- Unity External Script Editor can be your default C# IDE (for example Visual Studio). This package does not change C#.

Optional (only if Rider is not detected automatically):

- Set `RIDER_PATH` (or `JETBRAINS_RIDER_PATH`) to Rider executable path
- Ensure Rider is available via JetBrains Toolbox or on `PATH`

## Installation

### UPM: Git URL

Unity: `Window > Package Manager > + > Add package from git URL...`

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git
```

Tip: for a specific release tag:

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git#v0.1.0
```

### OpenUPM

```bash
openupm add com.clerin.unity.shader-ide-bridge-rider
```

## Usage

### 1) Enable and Configure

Project Settings: `Project/Clerin/Shader IDE Bridge`

- Enable OnOpenAsset Bridge
- Prefer Unity CodeEditor API
- Enable Diagnostics Warnings

### 2) Automatic Open (OnOpenAsset)

Double-click a supported file in Unity Project window. The package will try, in order:

1. Unity `CodeEditor` API (only when Unity is configured to use Rider)
2. Start Rider process with `--line`
3. Unity fallback open only when Unity External Script Editor is already Rider

If Rider cannot be resolved, the bridge logs a warning (when diagnostics are enabled) instead of silently forcing a non-Rider editor path.

### 3) Manual Open

Select a supported asset, then:

- `Tools/Clerin/Shader IDE Bridge/Open In Rider`

### 4) Diagnostics

- `Tools/Clerin/Shader IDE Bridge/Validate Rider Shader Setup`

This prints a report to the Unity Console and also shows it in a dialog.

## What This Package Does Not Do

- It does not build its own include index or force include roots.
- It does not implement Rider-side plugins or custom language services.

Rider is responsible for shader/HLSL navigation and include resolution. This package focuses on reliable Unity-to-Rider
opening and setup validation.

## Troubleshooting

- Rider does not open:
- Run diagnostics: `Tools/Clerin/Shader IDE Bridge/Validate Rider Shader Setup`.
- If your default External Script Editor is Visual Studio (intended), Rider must still be discoverable:
- Install Rider normally, or via JetBrains Toolbox.
- Versioned Windows installs are supported (for example `C:\Program Files\JetBrains\JetBrains Rider 2025.x\bin\rider64.exe`).
- If needed, set `RIDER_PATH` (or `JETBRAINS_RIDER_PATH`) to the Rider executable.
- Run diagnostics and verify `com.unity.ide.rider installed: true`.
- If Rider is configured as Unity's External Script Editor and open is still failing, toggle "Prefer Unity CodeEditor API" off to force the process fallback.
- Line is ignored:
- Some fallbacks only support line, not column; diagnostics will still confirm the path and configuration.
- File is not intercepted:
- Only the listed extensions are handled.

## Repository Layout

- Package root: `package.json`
- Unity Editor implementation: `Editor/`
- Runtime assembly marker: `Runtime/`
- Unity samples: `Samples~/`
- Unity EditMode tests: `Tests/Editor/`
- NuGet shared core: `src/Clerin.UnityShaderIdeBridge.Core/`
- Porting notes: `Documentation~/porting-matrix.md`

## Development

- .NET tests:

```bash
dotnet test Tests/Clerin.UnityShaderIdeBridge.Core.Tests/Clerin.UnityShaderIdeBridge.Core.Tests.csproj --configuration Release
```

- Unity tests:
- Run EditMode tests in the Unity Test Runner.

## Release

- Tag format: `vX.Y.Z`
- Keep versions aligned:
- `package.json` version must match the tag (release workflow enforces this).
- NuGet version is set from the tag during pack/publish.

GitHub Actions secrets (optional):

- `UNITY_LICENSE` to enable Unity EditMode tests in CI.
- `OPENUPM_TOKEN` to publish to OpenUPM on tag.
- `NUGET_API_KEY` to publish NuGet on tag.

## License

MIT (see `LICENSE.md`)

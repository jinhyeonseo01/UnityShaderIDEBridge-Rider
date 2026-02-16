# Unity Shader IDE Bridge for Rider

![Cover](docs/Cover.png)

<p align="center">
  <a href="README.md">English</a> · <a href="docs/README.ko.md">한국어</a> · <a href="docs/README.ja.md">日本語</a> · <a href="docs/README.zh.md">中文</a>
</p>

Open **shader-related files** in **JetBrains Rider**, while keeping **C# scripts** opening in your **primary IDE** (Visual Studio / VS Code / etc.).

> Unity can only select one **External Script Editor**. This package gives you a split workflow:
> - C# → your main IDE  
> - Shader/HLSL/etc. → Rider

**UPM package name:** `com.clerin.unity.shader-ide-bridge-rider`

---

## Installation (UPM / Git URL)

Unity: `Window > Package Manager > + > Add package from git URL...`

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git
```

Specific version tag:

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git#v1.0.0
```

---

## Quick Setup

1. Unity: `Edit > Preferences > External Tools`
2. Set **External Script Editor** to your **C# IDE** (Visual Studio / VS Code / etc.)
3. Open Project Settings: `Project > Clerin > Shader IDE Bridge`

### Settings

| Setting | Meaning |
|---|---|
| **Enable OnOpenAsset Bridge** | Intercept shader files and open them in Rider |
| **Enable Diagnostics Warnings** | Log warnings if Rider cannot be resolved |

**Note:** If your External Script Editor is already **Rider**, the bridge stays **inactive** (Unity default behavior), and the manual menu is hidden/disabled.

---

## Features

- Hooks Unity’s `OnOpenAsset` **only for shader-related extensions**
- **Does not** change Unity’s *External Script Editor* setting
- Finds Rider via **Toolbox / Program Files / PATH / env vars**, then launches with line support (`--line`)
- Includes **Diagnostics** so you can see *why Rider was (not) detected*

---

## Supported File Types

`.shader`, `.compute`, `.hlsl`, `.cginc`, `.glslinc`, `.cg`

---

## Requirements

- Unity **6000.3.x** (Unity 6.3 baseline)
- JetBrains **Rider** installed
- Unity package **`com.unity.ide.rider`** (declared in `package.json`)

### Recommended (for reliability + speed)

Set one of these environment variables to Rider’s executable:

- `RIDER_PATH` *(preferred)*
- `JETBRAINS_RIDER_PATH`
- `JETBRAINS_RIDER`

Windows example:

```text
C:\Program Files\JetBrains\JetBrains Rider 2025.x\bin\rider64.exe
```

---

## Usage

### Automatic (recommended)

- Double-click a supported shader file in the Unity Project window.

### Manual

- Select a supported asset
- `Tools > Clerin > Shader IDE Bridge > Open In Rider`

### Diagnostics

- `Tools > Clerin > Shader IDE Bridge > Validate Rider Shader Setup`
- `Tools > Clerin > Shader IDE Bridge > Clear Rider Cache` *(forces a fresh path scan on next open)*

---

## Limitations (by design)

- No custom include indexing / mirroring
- No extra language service or parsing layer  
  → Rider handles Shader/HLSL parsing, navigation, and include resolution.

---

## Troubleshooting

### Rider doesn’t open / first open is slow

- Run: `Tools > Clerin > Shader IDE Bridge > Validate Rider Shader Setup`
- Set `RIDER_PATH` to avoid path probing on first open.

### File isn’t intercepted

- Only the listed extensions are handled.
- If External Script Editor is Rider, interception is disabled by design.

### It opens in the wrong IDE

- Confirm External Script Editor is **not** Rider (this package is for split-IDE setups).

---

## License

MIT — see `LICENSE.md`

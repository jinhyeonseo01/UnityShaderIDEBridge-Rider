# Unity Shader IDE Bridge for Rider

![Cover](Cover.png)

<p align="center">
  <a href="../README.md">English</a> · <a href="README.ko.md">한국어</a> · <a href="README.ja.md">日本語</a> · <a href="README.zh.md">中文</a>
</p>

在 **JetBrains Rider** 中打开与着色器相关的文件，同时让 **C# 脚本**继续在你的**主 IDE**（Visual Studio / VS Code / etc.）中打开。

> Unity 只能选择一个 **External Script Editor**。此包提供“分离 IDE”工作流：
> - C# → 主 IDE  
> - Shader/HLSL/etc. → Rider

**UPM 包名：** `com.clerin.unity.shader-ide-bridge-rider`

---

## Installation (UPM / Git URL)

Unity: `Window > Package Manager > + > Add package from git URL...`

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git
```

指定版本标签：

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git#v1.0.0
```

---

## Quick Setup

1. Unity: `Edit > Preferences > External Tools`
2. 将 **External Script Editor** 设置为你的 **C# IDE**（Visual Studio / VS Code / etc.）
3. 打开 Project Settings: `Project > Clerin > Shader IDE Bridge`

### Settings

| 设置 | 含义 |
|---|---|
| **Enable OnOpenAsset Bridge** | 拦截着色器文件并在 Rider 中打开 |
| **Enable Diagnostics Warnings** | 当无法解析 Rider 时输出警告日志 |

**注意：** 如果 External Script Editor 已经是 **Rider**，桥接会保持 **非激活**（Unity 默认行为），并且手动菜单会隐藏/禁用。

---

## Features

- 仅对着色器相关扩展名挂钩 Unity 的 `OnOpenAsset`
- **不**更改 Unity 的 *External Script Editor* 设置
- 通过 **Toolbox / Program Files / PATH / 环境变量**查找 Rider，并以行号支持（`--line`）启动
- 内置 **Diagnostics**，便于查看 Rider 为什么（没）被检测到

---

## Supported File Types

`.shader`, `.compute`, `.hlsl`, `.cginc`, `.glslinc`, `.cg`

---

## Requirements

- Unity **6000.3.x**（Unity 6.3 基线）
- 已安装 JetBrains **Rider**
- Unity 包 **`com.unity.ide.rider`**（已在 `package.json` 中声明）

### Recommended（可靠性 + 速度）

将以下任意一个环境变量设置为 Rider 可执行文件路径：

- `RIDER_PATH` *(推荐)*
- `JETBRAINS_RIDER_PATH`
- `JETBRAINS_RIDER`

Windows 示例：

```text
C:\Program Files\JetBrains\JetBrains Rider 2025.x\bin\rider64.exe
```

---

## Usage

### Automatic (recommended)

- 在 Unity Project 窗口双击支持的着色器文件。

### Manual

- 选择支持的资产
- `Tools > Clerin > Shader IDE Bridge > Open In Rider`

### Diagnostics

- `Tools > Clerin > Shader IDE Bridge > Validate Rider Shader Setup`
- `Tools > Clerin > Shader IDE Bridge > Clear Rider Cache` *(下次打开时强制重新扫描路径)*

---

## Limitations (by design)

- 不提供自定义 include 索引/镜像
- 不提供额外语言服务或解析层  
  → Shader/HLSL 的解析、导航与 include 解析由 Rider 负责。

---

## Troubleshooting

### Rider 无法打开 / 首次打开很慢

- 运行：`Tools > Clerin > Shader IDE Bridge > Validate Rider Shader Setup`
- 设置 `RIDER_PATH` 以避免首次打开时的路径探测。

### 文件未被拦截

- 仅处理列表中的扩展名。
- 如果 External Script Editor 是 Rider，则按设计禁用拦截。

### 在错误的 IDE 中打开

- 确认 External Script Editor **不是 Rider**（此包用于分离 IDE 工作流）。

---

## License

MIT — 见 `LICENSE.md`

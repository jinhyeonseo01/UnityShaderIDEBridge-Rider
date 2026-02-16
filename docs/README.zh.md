# Unity Shader IDE Bridge (Rider)

![Cover](Cover.png)

<p align="center">
  <a href="../README.md">English</a> · <a href="README.ko.md">한국어</a> · <a href="README.ja.md">日本語</a> · <a href="README.zh.md">中文</a>
</p>

`com.clerin.unity.shader-ide-bridge-rider` 是一个 Unity Editor 包，用于将与着色器相关的文件在 JetBrains Rider 中打开，
同时保持 C# 脚本在你的主 IDE（例如 Visual Studio 2026）中打开。

当 Unity 只能选择一个 External Script Editor，但你希望如下工作流时可使用此包：

- C# 在主 IDE 中打开
- `.shader` / `.hlsl` / 等在 Rider 中打开

如果 Unity 的 External Script Editor 已经是 Rider，此包会保持非激活状态，不会拦截打开操作。

## What It Does

- 仅拦截与着色器相关资产的双击（Unity `OnOpenAsset`）。
- 不改变 Unity 的 External Script Editor 设置（C# 工作流保持不变）。
- 解析 Rider 可执行文件（Toolbox / Program Files / `PATH` / 环境变量），并使用 `--line` 启动。
- 提供诊断报告，帮助你了解 Rider 未被检测到的原因。

## Supported File Types

- `.shader`
- `.compute`
- `.cginc`
- `.glslinc`
- `.hlsl`
- `.cg`

## Requirements

- Unity `6000.3.x`（Unity 6.3 基线）
- 已安装 JetBrains Rider
- Unity 包 `com.unity.ide.rider`（已在 `package.json` 中声明）

为更可靠与更快速，建议：

- 将 `RIDER_PATH`（或 `JETBRAINS_RIDER_PATH` / `JETBRAINS_RIDER`）设置为 Rider 可执行文件路径。
  - Windows 示例：`C:\Program Files\JetBrains\JetBrains Rider 2025.x\bin\rider64.exe`

## Installation (UPM)

Unity: `Window > Package Manager > + > Add package from git URL...`

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git
```

指定标签：

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git#v0.1.0
```

## Setup

1. Unity: `Edit > Preferences > External Tools`
2. 将 `External Script Editor` 设置为你的 C# IDE（Visual Studio、VS Code 等）
3. Project Settings: `Project/Clerin/Shader IDE Bridge`

设置项：

- `Enable OnOpenAsset Bridge`：启用仅针对着色器的桥接
- `Enable Diagnostics Warnings`：当 Rider 无法解析时输出警告日志

注意：

- 如果 `External Script Editor` 是 Rider，桥接会被禁用（使用 Unity 默认打开流程），手动菜单也会隐藏/禁用。

## Usage

自动：

- 在 Unity Project 窗口双击支持的着色器文件。

手动：

- 选择支持的资产
- `Tools/Clerin/Shader IDE Bridge/Open In Rider`

诊断：

- `Tools/Clerin/Shader IDE Bridge/Validate Rider Shader Setup`
- `Tools/Clerin/Shader IDE Bridge/Clear Rider Cache`（下次打开时强制重新扫描路径）

## Limitations (By Design)

- 不提供自定义 include 索引或镜像，也不提供语言服务。
- Shader/HLSL 的解析、导航与 include 解析由 Rider 负责。

## Troubleshooting

- Rider 无法打开或首次打开很慢：
  - 运行诊断：`Tools/Clerin/Shader IDE Bridge/Validate Rider Shader Setup`。
  - 设置 `RIDER_PATH` 以避免首次打开时的路径探测。
- 文件未被拦截：
  - 仅处理列表中的扩展名。
  - 如果 Unity External Script Editor 是 Rider，则按设计禁用拦截。
- 在错误的 IDE 中打开：
  - 确认 `External Script Editor` 不是 Rider（此包用于双 IDE 工作流）。

## License

MIT（见 `LICENSE.md`）

# Unity Shader IDE Bridge (Rider)

![Cover](Cover.png)

<p align="center">
  <a href="../README.md">English</a> · <a href="README.ko.md">한국어</a> · <a href="README.ja.md">日本語</a> · <a href="README.zh.md">中文</a>
</p>

`com.clerin.unity.shader-ide-bridge-rider` は、シェーダー関連ファイルを JetBrains Rider で開き、
C# スクリプトはメイン IDE（例: Visual Studio 2026）で開き続ける Unity Editor パッケージです。

Unity は External Script Editor を 1 つしか選べませんが、次のようなワークフローに向いています:

- C# はメイン IDE で
- `.shader` / `.hlsl` / などは Rider で

Unity の External Script Editor がすでに Rider の場合、本パッケージは非アクティブになり、ファイルのオープンを介入しません。

## What It Does

- シェーダー関連アセットのみ Unity `OnOpenAsset` のダブルクリックをフックします。
- Unity の External Script Editor 設定は変更しません（C# のワークフローは維持）。
- Rider の実行ファイルを探索（Toolbox / Program Files / `PATH` / 環境変数）し、`--line` 付きで起動します。
- Rider を検出できない理由を確認できる診断レポートを提供します。

## Supported File Types

- `.shader`
- `.compute`
- `.cginc`
- `.glslinc`
- `.hlsl`
- `.cg`

## Requirements

- Unity `6000.3.x`（Unity 6.3 ベースライン）
- JetBrains Rider をインストール済み
- Unity パッケージ `com.unity.ide.rider`（`package.json` に宣言済み）

信頼性と速度のために推奨:

- `RIDER_PATH`（または `JETBRAINS_RIDER_PATH` / `JETBRAINS_RIDER`）に Rider の実行ファイルパスを設定してください。
  - Windows 例: `C:\Program Files\JetBrains\JetBrains Rider 2025.x\bin\rider64.exe`

## Installation (UPM)

Unity: `Window > Package Manager > + > Add package from git URL...`

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git
```

特定タグ:

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git#v0.1.0
```

## Setup

1. Unity: `Edit > Preferences > External Tools`
2. `External Script Editor` を C# IDE（Visual Studio、VS Code など）に設定
3. Project Settings: `Project/Clerin/Shader IDE Bridge`

設定:

- `Enable OnOpenAsset Bridge`: シェーダー専用ブリッジを有効化
- `Enable Diagnostics Warnings`: Rider が解決できない場合に警告ログを出力

注意:

- `External Script Editor` が Rider の場合、ブリッジは無効化されます（Unity の既定のオープン動作を使用）、
  手動メニューも非表示/無効になります。

## Usage

自動:

- Unity Project ウィンドウで対応シェーダーファイルをダブルクリック

手動:

- 対応アセットを選択
- `Tools/Clerin/Shader IDE Bridge/Open In Rider`

診断:

- `Tools/Clerin/Shader IDE Bridge/Validate Rider Shader Setup`
- `Tools/Clerin/Shader IDE Bridge/Clear Rider Cache`（次回オープン時にパスを再スキャン）

## Limitations (By Design)

- カスタム include のインデックス化/ミラーリング、言語サービスはありません。
- Shader/HLSL の解析・ナビゲーション・include 解決は Rider が担当します。

## Troubleshooting

- Rider が開かない、または初回が遅い:
  - 診断を実行: `Tools/Clerin/Shader IDE Bridge/Validate Rider Shader Setup`.
  - 初回のパス探索を減らすために `RIDER_PATH` を設定してください。
- ファイルがフックされない:
  - リストにある拡張子のみ処理されます。
  - Unity External Script Editor が Rider の場合は設計上フックが無効になります。
- 別の IDE で開かれる:
  - `External Script Editor` が Rider ではないことを確認してください（本パッケージは分離 IDE ワークフロー向けです）。

## License

MIT（`LICENSE.md` を参照）

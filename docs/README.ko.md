# Unity Shader IDE Bridge (Rider)

![Cover](Cover.png)

<p align="center">
  <a href="../README.md">English</a> · <a href="README.ko.md">한국어</a> · <a href="README.ja.md">日本語</a> · <a href="README.zh.md">中文</a>
</p>

`com.clerin.unity.shader-ide-bridge-rider`는 Unity Editor 패키지로, 셰이더 관련 파일을 JetBrains Rider에서 열고
C# 스크립트는 주 IDE(예: Visual Studio 2026)에서 열리도록 합니다.

Unity는 External Script Editor를 하나만 선택할 수 있지만, 아래와 같은 워크플로우를 원할 때 사용합니다:

- C#은 주 IDE에서
- `.shader` / `.hlsl` / 등은 Rider에서

Unity External Script Editor가 이미 Rider로 설정되어 있다면, 이 패키지는 비활성화되며 파일 열기를 가로채지 않습니다.

## What It Does

- 셰이더 관련 에셋에 대해서만 Unity `OnOpenAsset` 더블클릭을 가로챕니다.
- Unity의 External Script Editor 설정을 바꾸지 않습니다(C# 워크플로우 유지).
- Rider 실행 파일을 탐색(Toolbox / Program Files / `PATH` / 환경 변수)하고 `--line`으로 실행합니다.
- Rider 감지가 실패한 이유를 확인할 수 있는 진단 리포트를 제공합니다.

## Supported File Types

- `.shader`
- `.compute`
- `.cginc`
- `.glslinc`
- `.hlsl`
- `.cg`

## Requirements

- Unity `6000.3.x` (Unity 6.3 기준)
- JetBrains Rider 설치
- Unity 패키지 `com.unity.ide.rider` (`package.json`에 선언됨)

신뢰성과 속도를 위해 권장:

- `RIDER_PATH`(또는 `JETBRAINS_RIDER_PATH` / `JETBRAINS_RIDER`)에 Rider 실행 파일 경로를 지정하세요.
  - Windows 예시: `C:\Program Files\JetBrains\JetBrains Rider 2025.x\bin\rider64.exe`

## Installation (UPM)

Unity: `Window > Package Manager > + > Add package from git URL...`

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git
```

특정 태그:

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git#v0.1.0
```

## Setup

1. Unity: `Edit > Preferences > External Tools`
2. `External Script Editor`를 C# IDE(Visual Studio, VS Code 등)로 설정
3. Project Settings: `Project/Clerin/Shader IDE Bridge`

설정:

- `Enable OnOpenAsset Bridge`: 셰이더 전용 브리지를 활성화
- `Enable Diagnostics Warnings`: Rider를 찾지 못했을 때 경고 로그 출력

참고:

- `External Script Editor`가 Rider라면 브리지가 비활성화됩니다(기본 Unity 열기 흐름 사용) 그리고 수동 메뉴가 숨김/비활성화됩니다.

## Usage

자동:

- Unity Project 창에서 지원되는 셰이더 파일을 더블클릭

수동:

- 지원되는 에셋 선택
- `Tools/Clerin/Shader IDE Bridge/Open In Rider`

진단:

- `Tools/Clerin/Shader IDE Bridge/Validate Rider Shader Setup`
- `Tools/Clerin/Shader IDE Bridge/Clear Rider Cache` (다음 열기 때 경로 재탐색)

## Limitations (By Design)

- 커스텀 include 인덱싱/미러링 없음, 언어 서비스 없음.
- Shader/HLSL 파싱, 탐색, include 해석은 Rider가 담당합니다.

## Troubleshooting

- Rider가 열리지 않거나 첫 실행이 느림:
  - 진단 실행: `Tools/Clerin/Shader IDE Bridge/Validate Rider Shader Setup`.
  - 첫 실행 시 경로 탐색을 줄이려면 `RIDER_PATH`를 설정하세요.
- 파일이 가로채지지 않음:
  - 목록에 있는 확장자만 처리합니다.
  - Unity External Script Editor가 Rider라면 설계상 가로채기가 비활성화됩니다.
- 다른 IDE로 열림:
  - `External Script Editor`가 Rider가 아닌지 확인하세요(이 패키지는 IDE 분리 워크플로우용입니다).

## License

MIT (`LICENSE.md` 참고)

# Unity Shader IDE Bridge for Rider

![Cover](Cover.png)

<p align="center">
  <a href="../README.md">English</a> · <a href="README.ko.md">한국어</a> · <a href="README.ja.md">日本語</a> · <a href="README.zh.md">中文</a>
</p>

셰이더 관련 파일은 **JetBrains Rider**에서 열고, **C# 스크립트**는 **주 IDE**(Visual Studio / VS Code / etc.)에서 계속 열리도록 합니다.

> Unity는 **External Script Editor**를 하나만 선택할 수 있습니다. 이 패키지는 다음과 같은 분리 워크플로우를 제공합니다:
> - C# → 주 IDE  
> - Shader/HLSL/etc. → Rider

**UPM 패키지 이름:** `com.clerin.unity.shader-ide-bridge-rider`

---

## Installation (UPM / Git URL)

Unity: `Window > Package Manager > + > Add package from git URL...`

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git
```

특정 버전 태그:

```text
https://github.com/jinhyeonseo01/UnityShaderIDEBridge-Rider.git#v1.0.0
```

---

## Quick Setup

1. Unity: `Edit > Preferences > External Tools`
2. **External Script Editor**를 **C# IDE**(Visual Studio / VS Code / etc.)로 설정
3. Project Settings 열기: `Project > Clerin > Shader IDE Bridge`

### Settings

| 설정 | 의미 |
|---|---|
| **Enable OnOpenAsset Bridge** | 셰이더 파일을 가로채 Rider에서 열기 |
| **Enable Diagnostics Warnings** | Rider를 찾지 못했을 때 경고를 로그로 출력 |

**참고:** External Script Editor가 이미 **Rider**라면, 브리지는 **비활성화**(Unity 기본 동작)되며 수동 메뉴가 숨김/비활성화됩니다.

---

## Features

- 셰이더 관련 확장자에 대해서만 Unity `OnOpenAsset`을 후킹
- Unity의 *External Script Editor* 설정을 **변경하지 않음**
- **Toolbox / Program Files / PATH / 환경 변수**로 Rider를 찾고, 라인 지원(`--line`)과 함께 실행
- Rider가 (미)검출된 *이유*를 확인할 수 있는 **Diagnostics** 포함

---

## Supported File Types

`.shader`, `.compute`, `.hlsl`, `.cginc`, `.glslinc`, `.cg`

---

## Requirements

- Unity **6000.3.x** (Unity 6.3 baseline)
- JetBrains **Rider** 설치
- Unity 패키지 **`com.unity.ide.rider`** (`package.json`에 선언됨)

### Recommended (신뢰성 + 속도)

다음 환경 변수 중 하나를 Rider 실행 파일 경로로 설정하세요:

- `RIDER_PATH` *(권장)*
- `JETBRAINS_RIDER_PATH`
- `JETBRAINS_RIDER`

Windows 예시:

```text
C:\Program Files\JetBrains\JetBrains Rider 2025.x\bin\rider64.exe
```

---

## Usage

### Automatic (recommended)

- Unity Project 창에서 지원되는 셰이더 파일을 더블클릭합니다.

### Manual

- 지원되는 에셋을 선택
- `Tools > Clerin > Shader IDE Bridge > Open In Rider`

### Diagnostics

- `Tools > Clerin > Shader IDE Bridge > Validate Rider Shader Setup`
- `Tools > Clerin > Shader IDE Bridge > Clear Rider Cache` *(다음 열기 때 경로를 새로 스캔)*

---

## Limitations (by design)

- 커스텀 include 인덱싱/미러링 없음
- 추가 언어 서비스/파싱 레이어 없음  
  → Shader/HLSL 파싱, 탐색, include 해석은 Rider가 담당합니다.

---

## Troubleshooting

### Rider가 열리지 않음 / 첫 실행이 느림

- 실행: `Tools > Clerin > Shader IDE Bridge > Validate Rider Shader Setup`
- 첫 실행 시 경로 탐색을 줄이려면 `RIDER_PATH`를 설정하세요.

### 파일이 가로채지지 않음

- 목록에 있는 확장자만 처리합니다.
- External Script Editor가 Rider라면 설계상 가로채기가 비활성화됩니다.

### 다른 IDE로 열림

- External Script Editor가 **Rider가 아닌지** 확인하세요(이 패키지는 IDE 분리 워크플로우용입니다).

---

## License

MIT — `LICENSE.md` 참고

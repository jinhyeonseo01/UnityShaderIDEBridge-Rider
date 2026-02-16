# Contributing

## Requirements

- Unity `6000.3.x`
- Rider package enabled in Unity (`com.unity.ide.rider`)
- .NET SDK 8.0+ for shared core and NuGet tests

## Local Development

1. Open project in Unity 6.3.
2. Ensure External Script Editor is Rider.
3. Edit package code under `Editor/`, `Runtime/`, `Tests/`.
4. Run Unity EditMode tests.
5. Run .NET tests for `Tests/Clerin.UnityShaderIdeBridge.Core.Tests`.

## Versioning

- Use semantic versioning.
- Keep UPM and NuGet versions aligned.
- Tag format: `vX.Y.Z`.
- Tagging `vX.Y.Z` triggers the UPM/OpenUPM release workflow (`release-upm.yml`).

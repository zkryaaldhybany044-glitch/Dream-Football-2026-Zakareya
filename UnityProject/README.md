# Dream Football 2026 — Zakareya (Unity Native)

This folder begins a **native Unity Android game**, separate from the legacy browser prototype in the repository root.

## Current status

This is a project foundation, **not a playable football game yet**. It deliberately contains no capsule-player stand-ins, fake screenshots, or unlicensed DLS assets. A real visual prototype requires licensed realistic humanoid models, a rig, animation clips, pitch materials, stadium assets, and a tested Android build.

## Editor baseline
- Unity Editor: 6000.0.32f1
- Render pipeline: Universal Render Pipeline (URP), mobile-oriented
- Input: Unity Input System
- Target: Android ARM64
- Intended offline mode: all match assets and gameplay code packaged locally; no runtime network dependency.

Open this folder (UnityProject) as the Unity project root.

## Planned implementation sequence
1. **Foundation:** project settings, URP renderer, input and Android build configuration.
2. **Visual vertical slice:** licensed realistic humanoid player, humanoid rig, retargeted locomotion/turn/sprint/kick animations, pitch and stadium lighting.
3. **Playable match:** ball control, passing, shooting, goalkeeper, camera, touch controls.
4. **Team simulation:** 11v11 formations, player roles, team tactics, opponent AI, substitutions and match rules.
5. **Offline Android build:** package all required assets, build ARM64 APK in CI, install and profile on the target phone.

## Important build constraint

GitHub Actions cannot activate Unity by guessing credentials. A cloud build needs a Unity Personal/Pro license activation configured as GitHub Actions secrets, or a compatible Unity build service. Never add license files or credentials to the repository.

## Asset and performance policy
- Use original or explicitly licensed models, animations, audio, kits, badges and fonts.
- Do not extract DLS assets or use official club/player likenesses without permission.
- The target phone has 8 GB RAM, but RAM alone does not identify its GPU or CPU. Performance targets must be validated on the actual device.
- Aim for scalable quality settings and a stable 30 FPS baseline before attempting 60 FPS.

## Legacy browser version

The web prototype remains in the repository root and is not the native Unity project. It will not be presented as the requested finished game.

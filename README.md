# Sliding Block Escape

A sliding-block puzzle game for Android, built in Unity 6.

Blocks on a grid slide only along their own axis. Clear a path so the target block can exit the board. Later levels add one-way tiles, linked blocks that move together, and move limits.

> Status: in development.

## Tech stack

| Area | Choice |
| --- | --- |
| Engine | Unity 6 (6000.6), URP 2D renderer |
| Dependency injection | VContainer |
| Async | UniTask |
| Tweens | LitMotion |
| Content loading | Addressables |
| Remote config | Unity Remote Config (UGS) |
| Testing | Unity Test Framework (Edit Mode) |
| Input | Input System (touch drag) |

## Project layout

Each folder under `Assets/_Project/` is its own assembly, so the puzzle core cannot depend on Unity.

```
Assets/_Project/
  Core/        Pure C#: board, blocks, moves, rules, BFS solver (no UnityEngine)
  Core.Tests/  Edit Mode unit tests for Core
  Game/        MonoBehaviour views, input, presenters, VContainer scopes
  UI/          Screens: views + presenters (MVP)
  Data/        LevelData ScriptableObjects, level packs
  Editor/      Level editor window, validation tools
  Services/    Save, remote config, analytics wrappers behind interfaces
```

## Getting started

1. Install Unity 6000.6.2f1 with Android Build Support.
2. Install [Git LFS](https://git-lfs.com) and run `git lfs install` once.
3. Clone the repo and open the folder in Unity Hub.

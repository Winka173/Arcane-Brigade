# Arcane Brigade — Project Layout

Code for the MVP vertical slice. See [`Docs/MVP_Implementation_Plan.md`](../../Docs/MVP_Implementation_Plan.md)
for the design slice and execution roadmap.

## Folders

```
Assets/ArcaneBrigade/
  ArcaneBrigade.asmdef          Runtime assembly (refs Input System, URP runtime)
  Editor/                       Editor-only tooling
    ArcaneBrigade.Editor.asmdef
    ArcaneBrigadeMenu.cs        Tools menu (scene creation, etc.)
  Scripts/
    Core/                       GameManager, GameState, RunContext, enums
    Entities/                   Unit base class (Champion/Ally/Enemy inherit)
    Data/                       ScriptableObject definitions (Weapon, Champion, Ally,
                                Enemy, Blessing, Relic, Wave)
    Input/                      PlayerInputReader (New Input System)
  Scenes/                       Game.unity is created by the menu utility
```

## First-time setup (chapter 1 deliverable)

1. Open the project in Unity 6 (6000.4.8f1 or matching).
2. Let it import — all scripts in `Assets/ArcaneBrigade/` should compile without errors.
3. Menu: **Tools → Arcane Brigade → Create Game Scene** to generate `Assets/ArcaneBrigade/Scenes/Game.unity`.
4. Open the scene; press **Play**. You'll see the dark background and a `GameManager` object —
   no gameplay yet. This is the chapter 1 checkpoint.

Subsequent chapters layer content on top:
- Ch 2 — Aldric champion, movement, camera follow
- Ch 3 — Damage pipeline
- ...

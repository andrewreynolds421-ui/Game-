# Transformation FPS

An open-world, first-person shooter prototype in the Destiny 2 mold — fast
movement, a shootable primary weapon, and a Destiny-style subclass kit where
the "super" is a full-body transformation into a more powerful form.

## Status

This is the first playable slice: **the transformation ability system**,
plus enough FPS scaffolding (movement, shooting, health/shields, a couple
enemy dummies) to actually feel it out. It is not open-world yet — it's a
small flat test arena.

## Requirements

- Unity **2022.3 LTS** (the project targets `2022.3.50f1`; a nearby 2022.3.x
  patch version will also work — Unity will offer to upgrade in place).

## Setup

1. Open Unity Hub → **Add** → select this repo folder.
2. Open the project. Unity will import packages and regenerate `Library/`
   on first open — this can take a few minutes.
3. Create or open any scene (File → New Scene → Basic (Built-in)) and press
   **Play**.

There is intentionally no hand-authored `.unity` scene file in this repo.
A `GameBootstrap` script (`Assets/Scripts/Bootstrap/GameBootstrap.cs`) runs
automatically on scene load and procedurally builds the test arena, player,
and enemies from code. Hand-writing Unity's scene YAML/GUID format outside
the Editor is fragile and can't be verified without opening the Editor, so
the whole test environment is generated at runtime instead — open any blank
scene and press Play, no manual GameObject wiring required.

## Controls

| Input | Action |
|---|---|
| WASD | Move |
| Mouse | Look |
| Space | Jump |
| Left Shift (+ W) | Sprint |
| C | Crouch |
| Left Click | Fire |
| Right Click | Aim down sights |
| R | Reload |
| F | Melee |
| G | Morph Bolt (grenade-equivalent) |
| Q | Adaptation (class-ability-equivalent) |
| X | Ultimate — full-body Transformation |

## Architecture

```
Assets/Scripts/
  Core/         IDamageable — shared damage interface for player + enemies
  Player/       FirstPersonController, PlayerStats (health/shield), WeaponController
  Abilities/    AbilityDefinition, TransformationForm (ScriptableObjects),
                TransformationManager, SampleFormLibrary
  Enemies/      EnemyDummy — simple respawning combat target
  UI/           SimpleHud — OnGUI debug readout (health/ammo/cooldowns)
  Bootstrap/    GameBootstrap — procedurally builds the test scene at Play time
```

### The Transformation ability system (Destiny 2 subclass equivalent)

- **`TransformationForm`** (ScriptableObject) is the equivalent of a Destiny
  subclass: it bundles a Melee, Morph Bolt (grenade), Adaptation (class
  ability), and Ultimate ability, plus passive stat modifiers and the
  visual/scale change applied while transformed.
- **`AbilityDefinition`** (ScriptableObject) is a single ability's data:
  cooldown, energy cost, damage, radius, duration.
- **`TransformationManager`** (MonoBehaviour on the player) reads input,
  tracks per-ability cooldowns and Ultimate energy (0–100, filled by melee
  hits, Morph Bolt hits, and kills — mirroring supers charging off combat),
  and applies the Ultimate's full-body transformation: scale, tint, and
  temporary speed/damage multipliers for its duration.
- Three sample forms — **Beastkin** (melee/mobility), **Emberkin**
  (elemental/AoE), **Wraithkin** (utility/survivability) — are built in code
  by `SampleFormLibrary` so the arena has content without hand-authored
  asset files. In a full content pipeline these would instead be `.asset`
  files created via the Editor's `Create > TransformationFPS > ...` menus
  (the `[CreateAssetMenu]` attributes are already on both classes).

### Design intent for "transformation"

Transformation is treated as a **fantasy/sci-fi ability system** — a
creature/elemental morph mechanic that stands in for Destiny's Light
subclasses (Arc/Solar/Void/etc. → Beast/Elemental/Spectral). It is not
sexual content, and this project won't be extended in that direction.

## Not yet built

- Open-world traversal (streaming terrain, loading zones) — current arena
  is a small flat test space.
- Real weapon variety / loadout system.
- Enemy AI (current dummies are stationary targets).
- Real UI (current HUD is `OnGUI` debug text).
- Adaptation ability effects (cooldown/event wiring exists; per-form effects
  like phase-step or regen fields are not implemented yet).

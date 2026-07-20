# Transformation FPS

An open-world, first-person shooter prototype in the Destiny 2 mold — fast
movement, a shootable primary weapon, and a Destiny-style subclass kit where
the "super" is a full-body transformation into a more powerful form.

## Status

Four playable slices so far:

- **The transformation ability system** — Destiny-style melee/grenade/class-
  ability/super kit where the super is a full-body transformation.
- **Open-world terrain streaming** — procedurally generated Terrain chunks
  that load/unload around the player as they move, replacing the earlier
  small flat test arena.
- **Hover vehicle (Sparrow-equivalent)** — summon/mount/dismount a
  physics-based hover vehicle for covering the streamed world quickly.
- **On-foot movement kit** — a per-Form aerial move (double-jump / glide /
  blink, matching Beastkin / Emberkin / Wraithkin), sprint-slide, and
  auto-mantling over ledges.

Real content (weapon variety, enemy AI, world decoration) is not built yet —
see "Not yet built" below.

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
| Space (while airborne) | Aerial move — multi-jump / glide / blink, depending on equipped Form |
| Left Shift (+ W) | Sprint |
| C | Crouch |
| C (while sprinting) | Slide |
| (walk into a ledge within reach) | Auto-mantle |
| Left Click | Fire |
| Right Click | Aim down sights |
| R | Reload |
| F | Melee |
| G | Morph Bolt (grenade-equivalent) |
| Q | Adaptation (class-ability-equivalent) |
| X | Ultimate — full-body Transformation |
| V | Summon/mount, or dismount, the hover vehicle |
| A/D (while mounted) | Steer vehicle |
| W/S (while mounted) | Throttle / brake-reverse |
| Left Shift (while mounted) | Boost |
| Mouse (while mounted) | Free-look, independent of vehicle heading |

## Architecture

```
Assets/Scripts/
  Core/         IDamageable — shared damage interface for player + enemies
  Player/       FirstPersonController, PlayerStats (health/shield), WeaponController,
                VehicleMountController, AerialMobilityController, LedgeMantleController
  Abilities/    AbilityDefinition, TransformationForm (ScriptableObjects),
                TransformationManager, SampleFormLibrary
  Enemies/      EnemyDummy — simple respawning combat target
  World/        TerrainNoiseProfile, TerrainChunkBuilder, TerrainStreamingManager
  Vehicles/     HoverVehicleController, VehicleFactory
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

### Open-world terrain streaming

- **`TerrainNoiseProfile`** is a multi-octave (fractal) Perlin noise sampler,
  evaluated in **world-space** coordinates rather than per-chunk local ones.
  Because every chunk queries the same continuous function, adjacent chunks'
  heights match exactly at their shared edge — no explicit seam-stitching
  needed for the heightmap itself.
- **`TerrainChunkBuilder`** turns one grid coordinate into a real Unity
  `Terrain` GameObject: builds a heightmap from the noise profile, assigns a
  runtime-generated `TerrainLayer` (a flat-color texture, since no art
  assets exist yet), and positions it in the world.
- **`TerrainStreamingManager`** tracks the player's current chunk and keeps
  a square of chunks loaded around them (`viewDistanceInChunks`, default 2 →
  a 5×5 grid). It loads the starting chunks synchronously in `Start()` so
  the ground exists before the first physics step, then throttles further
  loads to `maxChunkLoadsPerFrame` (default 1) as the player moves, to avoid
  hitches. Chunks outside a one-chunk buffer past view distance are
  unloaded (destroyed) — since terrain is procedural, nothing needs to be
  saved; walking back regenerates identical terrain from the same noise
  profile. Neighboring chunks are linked via `Terrain.SetNeighbors` so
  normals blend cleanly across seams.
- Tune `chunkSize`, `heightmapResolution`, `viewDistanceInChunks` and the
  noise profile's `scale` / `octaves` / `heightMultiplier` on the
  `TerrainStreamingManager` component (find it under the "Terrain
  Streaming" GameObject at Play time, or adjust the defaults in
  `GameBootstrap.BuildTerrainStreaming`).

### Hover vehicle (Sparrow-equivalent)

- **`HoverVehicleController`** is a `Rigidbody`-based hover craft: a downward
  raycast drives a spring/damper force that holds it at `hoverHeight` above
  whatever's beneath it (terrain or anything else with a collider), forward
  thrust and yaw torque from drive input move it, and a separate upright
  stabilization torque keeps it level regardless of terrain slope — without
  fighting the steering torque, since it only corrects tilt, not heading.
  When nobody's driving it (`IsPlayerControlled = false`), it just idles,
  hovering in place, which is also its resting state after a dismount.
- **`VehicleFactory`** builds the vehicle from primitives at runtime, same
  approach as `GameBootstrap` uses for the rest of the scene.
- **`VehicleMountController`** (on the player) handles summon/mount/dismount
  on **V**. The key design choice: on mount, the *player* is reparented
  under the vehicle (not just the camera). Since `Transform.position` always
  reports world space regardless of parent, everything that already reads
  the player's transform — `TerrainStreamingManager.target` for chunk
  streaming, `EnemyDummy`'s `FindGameObjectWithTag("Player")` — keeps
  working automatically while riding, with no extra per-frame sync code.
  On-foot movement and the weapon are disabled while mounted; the mouse
  free-looks independently of the vehicle's heading (rotating only the
  camera pivot), and A/D steering applies torque to the vehicle itself.
  Dismounting drops the player just behind the vehicle, raycast down to the
  actual ground height, facing the direction the vehicle was heading.
- Pressing **V** again always teleports the same vehicle instance to just
  in front of the player (matching Destiny's Sparrow-summon behavior)
  rather than spawning duplicates.

### On-foot movement kit

- **`FirstPersonController`** now exposes a small public API instead of
  keeping all movement state private, so other systems can drive or
  momentarily take over movement without duplicating its velocity/gravity/
  collision handling: `VerticalVelocity` (get/set), `FallSpeedClamp` (a max
  fall speed other systems can clamp temporarily), `PerformJumpImpulse`
  (the shared jump-arc formula, used by both the ground jump and per-Form
  air jumps), and `MovementSuppressed` (skips its own WASD/gravity/jump/
  slide logic entirely — mouse look keeps running — so e.g. ledge mantling
  can drive the `CharacterController` directly for a few frames).
- **Sprint-slide** lives directly in `FirstPersonController` since it
  already owns crouch/sprint state: pressing **C** while sprinting captures
  current horizontal velocity as a slide direction, boosts it to
  `slideSpeed`, and decays it over `slideDuration`; releasing **C** early
  or jumping cancels it.
- **`AerialMobilityController`** reads the equipped `TransformationForm`'s
  `aerialMoveType` and, on Space while airborne, performs whichever move
  that Form defines:
  - **MultiJump** (Beastkin) — extra jumps in the air, via
    `PerformJumpImpulse`, refilled on landing.
  - **Glide** (Emberkin) — clamps `FallSpeedClamp` for `glideDuration` and
    adds forward drift along the camera's look direction each frame.
  - **Blink** (Wraithkin) — an instant forward teleport (`CharacterController.Move`),
    clamped short of any obstacle hit by a forward raycast, on a cooldown.
- **`LedgeMantleController`** auto-mantles while walking forward into a
  ledge within reach: a chest-height raycast finds a wall (rejected if its
  surface normal is too close to "up," so gentle terrain slopes don't
  misfire as walls), a raycast down from above the wall finds the ledge
  height, and if it's within `[minMantleHeight, maxMantleHeight]` with
  headroom to stand, the player is lerped up onto it over `mantleDuration`
  via `MovementSuppressed` + direct `CharacterController.Move` calls.
- `GameBootstrap.BuildTraversalTestCourse` places a small staircase (to
  test mantling) and a gap platform (to test the aerial move) near spawn
  at `(15, ~, 15)`, purely so these systems have something to try out
  against. To test Glide or Blink instead of the default MultiJump, swap
  `SampleFormLibrary.CreateBeastForm()` for `CreateElementalForm()` /
  `CreateSpectralForm()` in `GameBootstrap.BuildPlayer`.

### Design intent for "transformation"

Transformation is treated as a **fantasy/sci-fi ability system** — a
creature/elemental morph mechanic that stands in for Destiny's Light
subclasses (Arc/Solar/Void/etc. → Beast/Elemental/Spectral). It is not
sexual content, and this project won't be extended in that direction.

## Not yet built

- Other traversal aids: grapple, fast travel / loading zones.
- World decoration (trees, rocks, points of interest) — terrain is
  currently bare rolling hills with no scattered props.
- Real weapon variety / loadout system.
- Enemy AI (current dummies are stationary targets, and are placed near the
  world origin rather than distributed across streamed chunks).
- Real UI (current HUD is `OnGUI` debug text).
- Adaptation ability effects (cooldown/event wiring exists; per-form effects
  like phase-step or regen fields are not implemented yet).

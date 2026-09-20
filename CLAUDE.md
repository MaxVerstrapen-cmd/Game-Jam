# Head Over Heels — Project Notes for Claude

A 2-player local versus fighting game made for a game jam. Unity **2022.1.5f1**, 2D, sprite-based.

Full pitch/story is in [README.md](README.md). Short version: two fighters battle best-of-5 rounds over a caged girl; as they fight they end up falling for each other. Theme: "Head Over Heels" (romance + literally tumbling around, also "Head" and "Heel" are the two fighters' dialogue-system names).

## Repo quirk: don't be surprised by drift

This project has been actively developed from at least two different machine checkouts (`C:\Users\dunphy3777\_GameJam\Game-Jam` and `D:\GAMEJAM\Game-Jam`) plus at least one other collaborator pushing directly. Scripts you looked at earlier in a session can be meaningfully different by the time you look again — always re-read a file before editing it rather than trusting your memory of it from earlier in the conversation, and check `git status`/`git log` at the start of a session rather than assuming the working tree matches what's described below. This file will go stale; treat it as a map of *where things are and why*, not a byte-for-byte guarantee of current file contents.

Merge conflicts on `Assets/playerMovement.cs` have already happened once; resolve conflicts by comparing which side has the more complete/newer feature set, not just "ours" vs "theirs" blindly. A duplicate `EventSystem` GameObject also once survived a scene merge (git can't detect two independently-added GameObjects are semantically the same thing) — if "There are 2 event systems in the scene" ever reappears, search the Hierarchy for duplicates and delete one.

## Players

There are two player characters, each built the same way but with separate assets:

| | Player 1 | Player 2 |
|---|---|---|
| Prefab | `Assets/Player.prefab` | `Assets/Player2.prefab` |
| Move / Jump | WASD / `W` | Arrow keys / `↑` |
| Attack (dash) | `Left Shift` | `Keypad0` or `Alpha0` |
| Parry | `Q` | `/` (Slash) |
| Character art | "Max" — `Assets/Sprites/Art/MaxSprite/` | "Samurai" — `Assets/Sprites/Art/Samurai/` |
| Animator Controller | `Assets/Animations/player1.controller` | `Assets/Animations/samurai.controller` |
| `playerNumber` | `1` | `2` |

Both prefabs are **near-identical copies** with the same internal component `fileID`s (deliberately, so scene-level `PrefabInstance` overrides — position, collider size, `playerNumber`, etc. — stay valid no matter which prefab an instance points at). If you need a third character, duplicate one of these prefabs the same way rather than building from scratch: copy the `.prefab` file, keep every `fileID` identical, and only change the `SpriteRenderer`'s default sprite, the `Animator`'s `m_Controller` GUID, and `playerNumber`.

Both players are placed once each in `Assets/Scenes/mainScene.unity`, named exactly `player1` and `player2` in the Hierarchy — `playerMovement.Start()` finds them with `GameObject.Find("player1")`/`GameObject.Find("player2")`, and `healthBarUpdater.cs` does the same, so don't rename those Hierarchy objects without updating both. Each player also has a **child GameObject with a trigger collider** — the attack hitbox, wired via the `[SerializeField] attackHitbox` field on `playerMovement` and a `HitboxScript` component on the child (see below). This hitbox child is scene-specific (not baked into the shared prefab), so if you add a new scene with players in it, remember to add and wire this child too, or `Start()` will null-ref on `attackHitbox.GetComponent<HitboxScript>()`.

Other scenes: `MainMenu` (the only one in Build Settings right now), `DialogueScene`/`DialogueTest`, `HowToPlayScene`, `SampleScene`, `TestScene`. `StartGame.cs` currently sends the Start button to `DialogueScene`, not `mainScene` — the game is mid-integration between the fight scene and a separate dialogue/story scene.

## Scripts

### `Assets/playerMovement.cs` — the "brain"
Movement, jump, attack/dash, parry, health, and pass-through-during-attack physics. Shared by both players (differentiated entirely by `playerNumber`, checked with `if`/`else if` everywhere — there is no subclassing).

- `playerNumber` is a **per-instance** `public int`, NOT static — it used to be `static` early on, which meant both player objects fought over one shared value. If you ever see both players moving with the same keys, check this didn't regress.
- **Damage flow** (this has been reworked since earlier in the project — don't trust an older description of it):
  1. Each player has a body `Collider2D` (solid, for floor/physics) and a separate child hitbox `Collider2D` set as a trigger, referenced via `attackHitbox`.
  2. `HitboxScript.cs` sits on that child, and on `OnTriggerEnter2D` calls `owner.Hitbox(otherPlayer)` back up on the parent's `playerMovement`.
  3. `Hitbox()` only does anything if `isAttacking` is true (the hitbox is otherwise disabled anyway) and the target hasn't already been hit by *this* dash (`hitThisDash` HashSet — a single dash can otherwise register 2–3 hits through both the body collider and the trigger hitbox overlapping in the same frame).
  4. If the target is parrying, the **attacker** gets stunned (`setIstunned`) instead of the target taking damage.
  5. Otherwise `otherPlayer.health -= 1`, and (currently player-2/samurai only) `otherPlayer.anim.SetTrigger("Hurt")` fires the hurt animation on the player who got hit. **Player 1 doesn't have a Hurt animation yet** — widen that `if (otherPlayer.playerNumber == 2)` check once one exists for Max.
  6. `OnCollisionEnter2D` also calls `Hitbox()` when the two *solid* bodies collide while attacking — this is the only way a **parried** dash registers as a hit at all, because `updatePassThrough()` keeps the bodies solid against each other specifically when the target is parrying (see next point), so the trigger hitbox's shape gets physically pushed apart and never actually overlaps in that case.
- **Pass-through physics**: `updatePassThrough()` (called every `FixedUpdate`) uses `Physics2D.IgnoreCollision` on the two players' *body* colliders — passable exactly when one of them is dashing and the target *isn't* parrying it. Both players independently compute the same answer from shared state, so there's no ownership fight over whose dash "wins" the IgnoreCollision call. It only calls into the physics engine when the pass-through state actually changes (`passingThrough` bool) — calling `IgnoreCollision` every frame resets contact state and would spam spurious collision events.
- Parry: blocks damage and stuns the attacker instead if timed right (`isParrying`, `setIstunned`).
- `player1`/`player2` fields (references to the *other* `playerMovement` instances, found via `GameObject.Find` in `Start()`) are used both for pass-through and for reaching across to set the other player's Animator trigger on hit.

### `Assets/HitboxScript.cs`
Thin trigger listener on the attack-hitbox child object — see above. Ignores anything without a `playerMovement` in its parent chain (floor/walls/tilemap) and ignores self-hits.

### `Assets/PlayerAnimation.cs`
Drives the Animator's `IsRunning` bool and left/right sprite flip, purely from each player's own movement keys via its own `GetHorizontalInput()` (never reads the shared `Input.GetAxisRaw("Horizontal")` — that axis is bound to *both* A/D and arrow keys by Unity's default Input Manager, which caused both players to animate off either player's keys before this was fixed). `Attack`, `Hurt`, and `IsJumping` are all triggered from `playerMovement.cs` instead, at the exact moments those things actually happen in game logic (see Animator Controllers below for why that matters).

### `Assets/healthBarUpdater.cs`
Watches both players' health (via `playerMovement.getHealth()`) and shrinks/destroys the corresponding health bar UI object (`healthbarP1`/`healthbarP2`, found by name).

### `Assets/StartGame.cs`
Wires a UI Button to load `DialogueScene`.

### `Assets/Scripts/Dialogue/*` and `Assets/Scripts/Menu/*`
A separate floating-combat-dialogue system (`CombatDialogue.cs`) that pops speech bubbles above each fighter ("Head"/"Heel") at health thresholds (`p1_2Triggered`, `p1_1Triggered`, `p1_0Triggered` etc. — one-shot flags per threshold per player), plus `DialogueManager`/`DialogueData`/`DialogueTrigger`/`OpeningDialogueTransition` for the narrative side, and small menu helpers (`BackToMainMenu`, `OpenHowToPlay`). Haven't been touched as part of the animation/physics work this file otherwise documents — go read them directly if you need to change dialogue behavior.

### `Assets/Scripts/Player/TimerScript.cs`
Contains an unused `MinimalTimer` struct — nothing currently references it (the actual timers in `playerMovement.cs` just compare raw `Time.time` floats against cooldown fields directly). Likely dead code / a leftover from an earlier refactor.

## Animator Controllers

`player1.controller` and `samurai.controller` follow the same shape, built up incrementally — check both since one may have states the other doesn't yet:

| State | player1.controller | samurai.controller |
|---|---|---|
| Idle | `IdlePlayer` | `SamuraiIdle` |
| Running | `IsRunning` | `SamuraiRun` |
| Attack | `Attack` | `AttackMax2` |
| Jump | `JumpingKnight` | *(none yet)* |
| Hurt | *(none yet)* | `HurtSamurai` |

- **Parameters**: `IsRunning` (Bool) and `Attack` (Trigger) exist on both. `IsJumping` (Bool) only exists on `player1.controller`. `Hurt` (Trigger) only exists on `samurai.controller`.
- **Attack** and **Hurt** are both reachable from **Any State** via their Trigger, and both exit back to Idle/Running using **`Has Exit Time = 1`** (waits for the clip to finish) rather than any input condition — this guarantees the full animation always plays even if the key was only tapped for a frame, since the code fires these with `GetKeyDown`/at a single instant, not `GetKey`.
- **Jump** exits using the `IsJumping` bool going false (no exit-time wait) — jump is a *state* (as long as airborne) not a one-shot event, unlike Attack/Hurt. It's driven directly from `playerMovement.cs`: `SetBool("IsJumping", true)` when the jump impulse fires, `SetBool("IsJumping", false)` in `OnCollisionEnter2D` when they land on something tagged `floor`.
- **Critical rule for this project: every transition's `Transition Duration` must be `0`** (instant cut), never a crossfade. All animations here are sprite-swap (`m_PPtrCurves`/`m_Sprite`), and Unity cannot interpolate between two different Sprite references — a nonzero blend duration doesn't produce a smooth crossfade, it produces visible flicker/popping between frames of both clips. This has bitten the project three times now (Jump, the samurai's Attack, and it was caught early for Hurt) — if you add a new state, set its transitions' duration to `0` from the start, don't leave it at the Editor's default.
- New animation clips must have **`Loop Time` off** if they're a one-shot action (Attack, Hurt) — a short looping clip left on by accident (as happened with the first version of `JumpingKnight.anim`) rapid-fires through its frames the whole time the state is active instead of playing once and holding.

## Sprite import conventions (easy to get wrong — has caused the most bugs)

- **Texture Type**: `Sprite (2D and UI)`, **Sprite Mode**: `Multiple`, sliced into equal-size grid frames named `<SheetName>_<index>`.
- **Filter Mode must be `Point (no filter)`** for every character sheet. `Bilinear` (the Unity default for new imports) blurs pixel art — this has already caused a "why is the attack/jump/hurt blurry" bug on four separate sheets (`Attack1.png`, `Jump.png` for Max; `ATTACK 1.png`, `HURT.png` for the samurai) where Idle/Run were correctly set to Point but every newer sheet wasn't. **Check this on every new sheet before anything else.**
- **Pivot must be Custom, not the default Bottom/Center**, and must match where the character's feet *actually* are within the tile — not the tile's edge. Padding inside the transparent canvas means the visual feet usually sit well above the tile's bottom edge. To find the right value: measure the lowest opaque pixel's distance from the tile's bottom edge, divide by tile height. Established values so far:
  - Max (`MaxSprite/`, 180×180 tiles): pivot Y = **0.3722** (Idle, Run, Attack, Jump — a Hurt sheet doesn't exist for Max yet).
  - Samurai (`Samurai/`, 96×96 tiles): pivot Y = **0.1667** (Idle, Run, Attack, Hurt).
  - **Every frame in a sheet must use the same pivot value.** Multiple bugs so far were caused by only some frames in a sheet getting the correct custom pivot while others were left at the default — this causes the character to visibly pop/sink on specific frames instead of a smooth animation. Always spot-check every frame's `alignment`/`pivot` in the `.meta`, not just the first one.
- The player's `BoxCollider2D` offset is set so its *bottom* edge lines up with the sprite pivot (feet), not centered on it (Unity's default). For a collider of `size {1,1}`, that means `offset: {0, 0.5}` — get this wrong and the character floats above or sinks into platforms.

## Known Unity Editor gotcha in this project

Hand-editing `.controller`/`.anim`/`.meta` files (or scripted tools editing them) can leave Unity's **import cache stale** — the Animator window shows an empty/old state machine, a texture's pivot doesn't visually update, etc., even though the file on disk is correct. The fix is always: right-click the asset in the Project window → **Reimport**. The reverse can also happen — changes made live in the Editor (e.g. building a new Animator state through the UI) aren't necessarily flushed to disk until you explicitly save (Ctrl+S / File > Save Project); if you're about to read/edit an asset file directly, ask whoever's in the Editor to save first so you're not working from a stale copy.

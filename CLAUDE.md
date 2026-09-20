# Head Over Heels — Project Notes for Claude

A 2-player local versus fighting game made for a game jam. Unity **2022.1.5f1**, 2D, sprite-based.

Full pitch/story is in [README.md](README.md). Short version: two fighters battle best-of-5 rounds over a caged girl; as they fight they end up falling for each other. Theme: "Head Over Heels" (romance + literally tumbling around).

## Repo quirk: don't be surprised by two working directories

This project has been actively developed from at least two different machine checkouts (`C:\Users\dunphy3777\_GameJam\Game-Jam` and `D:\GAMEJAM\Game-Jam` have both shown up in this project's history). They can drift out of sync with each other and with `origin/main` — always check `git status`/`git log` at the start of a session rather than assuming the working tree matches what you remember from a prior conversation. Merge conflicts on `Assets/playerMovement.cs` have already happened once; resolve conflicts by comparing which side has the more complete/newer feature set, not just "ours" vs "theirs" blindly.

## Players

There are two player characters, each built the same way but with separate assets:

| | Player 1 | Player 2 |
|---|---|---|
| Prefab | `Assets/Player.prefab` | `Assets/Player2.prefab` |
| Controls | WASD, Shift = attack, Q = parry | Arrow keys, Keypad0/Alpha0 = attack, `/` = parry |
| Character art | "Max" — `Assets/Sprites/Art/MaxSprite/` | "Samurai" — `Assets/Sprites/Art/Samurai/` |
| Animator Controller | `Assets/Animations/player1.controller` | `Assets/Animations/samurai.controller` |
| `playerNumber` | `1` | `2` |

Both prefabs are **near-identical copies** with the same internal component `fileID`s (deliberately, so scene-level `PrefabInstance` overrides — position, collider size, `playerNumber`, etc. — stay valid no matter which prefab an instance points at). If you need a third character, duplicate one of these prefabs the same way rather than building from scratch: copy the `.prefab` file, keep every `fileID` identical, and only change the `SpriteRenderer`'s default sprite, the `Animator`'s `m_Controller` GUID, and `playerNumber`.

Both players are placed once each in `Assets/Scenes/mainScene.unity`, named `player1` and `player2` in the Hierarchy — that's the only scene with actual gameplay currently wired up. Other scenes: `MainMenu` (the only one in Build Settings right now), `DialogueScene`/`DialogueTest`, `HowToPlayScene`, `SampleScene`, `TestScene`.

## Scripts

- **`Assets/playerMovement.cs`** — the "brain": movement, jump, attack, parry, health, and pass-through-during-attack physics. Shared by both players (differentiated entirely by `playerNumber`, checked with `if`/`else if` everywhere — there is no subclassing).
  - `playerNumber` is a **per-instance** `public int`, NOT static — it used to be `static` early on, which meant both player objects fought over one shared value. If you ever see both players moving with the same keys, check this didn't regress.
  - Damage detection uses a **trigger collider** (`OnTriggerEnter2D` → `playerHitbox`), separate from the main solid collider used for physical movement/floor collision. This is intentional: it lets `Physics2D.IgnoreCollision` make the two players pass through each other during an attack/dash (see `attackInput`/`FixedUpdate`) while damage detection keeps working regardless, since it doesn't depend on solid-collision events.
  - Parry: `Q`/`Slash`, blocks damage and stuns the attacker instead if timed right (`isParrying`, `setIstunned`).
  - `player1`/`player2` fields are found via `GameObject.Find("player1")`/`GameObject.Find("player2")` in `Start()` — so those exact Hierarchy names matter, don't rename the scene objects without updating this.
- **`Assets/PlayerAnimation.cs`** — drives the Animator's `IsRunning` bool and left/right sprite flip, purely from each player's own movement keys (never reads the shared `Input.GetAxisRaw("Horizontal")` — that axis is bound to *both* A/D and arrow keys by Unity's default Input Manager, which caused both players to animate off either player's keys before this was fixed).
- **`Assets/healthBarUpdater.cs`** — watches both players' health (via `playerMovement.getHealth()`) and shrinks/destroys the corresponding health bar UI object (`healthbarP1`/`healthbarP2`, found by name).
- **`Assets/StartGame.cs`** — wires a UI Button to load `DialogueScene` (not `mainScene` directly — currently mid-integration with a separate dialogue system).

## Animator Controllers

Both `player1.controller` and `samurai.controller` follow the same shape:

- **Parameters**: `IsRunning` (Bool), `Attack` (Trigger). `player1.controller` additionally has `IsJumping` (Bool) — the samurai doesn't have a jump animation yet.
- **States**: `IdlePlayer`/`SamuraiIdle` ↔ `IsRunning`/`SamuraiRun` (bool-driven, default state machine loop), plus an `Attack` state reachable from **Any State** via the `Attack` trigger, and (player1 only) a `JumpingKnight` state reachable from Idle/Running via the `IsJumping` bool.
- **Attack** exits back to Idle/Running using `Has Exit Time = 1` (waits for the clip to finish) rather than any input condition — this guarantees the full attack animation always plays even if the key was only tapped for a frame, since `attackInput()` uses `GetKeyDown` not `GetKey`.
- **Jump** exits using the `IsJumping` bool going false (no exit-time wait) — jump is a *state* (as long as airborne) not a one-shot event, unlike Attack.
- **Critical rule for this project: every transition's `Transition Duration` must be `0`** (instant cut), never a crossfade. All animations here are sprite-swap (`m_PPtrCurves`/`m_Sprite`), and Unity cannot interpolate between two different Sprite references — a nonzero blend duration doesn't produce a smooth crossfade, it produces visible flicker/popping between frames of both clips. This has bitten the project twice already (Jump and the samurai's Attack both had to be fixed from `0.25` back to `0`). If you add a new state, set its transitions' duration to `0` from the start.

## Sprite import conventions (easy to get wrong)

- **Texture Type**: `Sprite (2D and UI)`, **Sprite Mode**: `Multiple`, sliced into equal-size grid frames named `<SheetName>_<index>`.
- **Filter Mode must be `Point (no filter)`** for every character sheet. `Bilinear` (the Unity default for new imports) blurs pixel art — this has already caused a "why is the attack/jump blurry" bug on three separate sheets (`Attack1.png`, `Jump.png` for Max; `ATTACK 1.png` for the samurai) where Idle/Run were correctly set to Point but the newer sheets weren't.
- **Pivot must be Custom, not the default Bottom/Center**, and must match where the character's feet *actually* are within the tile — not the tile's edge. Padding inside the transparent canvas means the visual feet usually sit well above the tile's bottom edge. To find the right value: measure the lowest opaque pixel's distance from the tile's bottom edge, divide by tile height. Established values so far:
  - Max (`MaxSprite/`, 180×180 tiles): pivot Y = **0.3722** (used consistently across Idle, Run, Attack, Jump).
  - Samurai (`Samurai/`, 96×96 tiles): pivot Y = **0.1667** (used consistently across Idle, Run, Attack).
  - **Every frame in a sheet must use the same pivot value.** Two bugs so far were caused by only some frames in a sheet getting the correct custom pivot while others were left at the default — this causes the character to visibly pop/sink on specific frames instead of a smooth animation.
- The player's `BoxCollider2D` offset is set so its *bottom* edge lines up with the sprite pivot (feet), not centered on it (Unity's default). For a collider of `size {1,1}`, that means `offset: {0, 0.5}` — get this wrong and the character floats above or sinks into platforms.

## Known Unity Editor gotcha in this project

Hand-editing `.controller`/`.anim`/`.meta` files (or scripted tools editing them) can leave Unity's **import cache stale** — the Animator window shows an empty/old state machine, a texture's pivot doesn't visually update, etc., even though the file on disk is correct. The fix is always: right-click the asset in the Project window → **Reimport**. The reverse can also happen — changes made live in the Editor (e.g. building a new Animator state through the UI) aren't necessarily flushed to disk until you explicitly save (Ctrl+S / File > Save Project); if you're about to read/edit an asset file directly, ask whoever's in the Editor to save first so you're not working from a stale copy.

## Git merge history note

There was a real merge conflict in `Assets/playerMovement.cs` (two branches both extended the `attackInput`/`FixedUpdate` attack-trigger logic slightly differently) — resolved by keeping the version that fired `anim.SetTrigger("Attack")` for both `playerNumber == 1` and `playerNumber == 2`. There was also a **duplicate `EventSystem`** GameObject left in `mainScene.unity` after a different merge (git can't detect that two independently-added GameObjects are semantically the same thing) — if "There are 2 event systems in the scene" ever reappears, it's the same class of bug: search the Hierarchy for duplicates and delete one.

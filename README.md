# What Hides Inside 🌒

**An atmospheric 2D puzzle-platformer built in Unity — a Software Engineering diploma project.**

---

## 👁️ About the Project

*What Hides Inside* is a graduation project for the Software Engineering program at
**Astana IT University**. The deliverable is a playable game, but the heart of the project
is a software-engineering case study: applying enterprise-grade **SOLID principles** and an
**interface-driven architecture** to small-team game development — to reduce coupling, cut
technical debt, and eliminate version-control conflicts.

The game drops a silhouette character into an atmospheric, physics-driven world where
progress depends on precise platforming and on manipulating the environment to get past
hazards — building toward a dark cave section where visibility drops and the player moves
by sound and faint light.

## 🎮 Gameplay Features

- **Physics manipulation** — drag crates to build steps, bridge gaps, and disarm traps.
- **Traversal toolkit** — ladders, swinging ropes, moving and floating platforms, and
  Limbo-style ledge climbing (hang, pull up, drop, or jump off).
- **Hazards** — rotating and shuttle saws, bear traps, and charging turrets.
- **Instant-death + checkpoints** — no health bar; any lethal contact returns you to the
  last checkpoint.
- **Precision platforming** — tight, predictable movement with coyote time, jump buffering,
  and asymmetric fall gravity.
- **A dark cave section** — a low-visibility "secret level" where the atmosphere shifts and
  navigation relies on sound and minimal light.
- **Environmental storytelling** — no text or dialogue; mood and meaning come from space,
  light, and sound.

## 🛠️ Technical Stack & Architecture

- **Engine:** Unity 6 LTS
- **Rendering:** Universal Render Pipeline (URP) with 2D lighting and post-processing
- **Architecture:**
  - **Interface-driven, decoupled design** — `IDamageable`, `IInteractable`,
    `IInputProvider`, and the `TriggerableObject` abstract class are the only cross-system
    boundaries, so hazards, the player controller, and switch chains stay independent.
  - **Custom physics controller** — velocity-based movement, coyote time, jump buffering,
    asymmetric fall gravity, and continuous collision detection to prevent tunnelling.
  - **Single-responsibility components** — movement, ledge climbing, hazards, and the cave
    lighting system are split into focused scripts rather than one monolithic controller.
  - **Additive Scene Workflow + Git LFS** — environment and systems are split across scenes
    to eliminate merge conflicts in two-person collaborative development.
  - **Data-driven decisions** — engineering priorities were set by a pilot survey of 50
    respondents.

## 👥 Authors

- **Aldibayeva Akku** ([@Akku-2325](https://github.com/Akku-2325))
- **Nurmukhammed Seitkhan** ([@NurNurzhanuly](https://github.com/NurNurzhanuly))

**Scientific Supervisor:** Akhmetzhanova Shynar
**University:** Astana IT University (AITU)
**Year:** 2025–2026

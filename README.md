# What Hides Inside 🌒
**An atmospheric 2D puzzle-platformer built in Unity, developed as a Software Engineering diploma project.**

---

### 👁️ About the Project

"What Hides Inside" is a graduation project for the Software Engineering program at
Astana IT University. While the deliverable is a playable game, the core of the project
is a software-engineering case study: applying enterprise-grade **SOLID design principles**
and an **interface-driven architecture** to small-team game development, in order to reduce
coupling, cut technical debt, and eliminate version-control conflicts.

The game places a silhouette character in an atmospheric, physics-driven world where
progress depends on precise platforming and manipulating the environment to get past hazards.

### 🎮 Gameplay Features

- **Physics manipulation:** Move crates and objects to solve environmental puzzles and open paths.
- **Interactive mechanics:** Ladders, ropes, rotating-saw hazards, and elevators.
- **Instant-death + checkpoints:** No health bar — any lethal contact returns you to the last checkpoint.
- **Precision platforming:** Tight, predictable movement with coyote time and jump buffering.
- **Environmental storytelling:** No text or dialogue. Mood and meaning come from space and sound.
- **Spatial-audio navigation:** A sensory-deprivation level navigated largely by sound.

### 🛠️ Technical Stack & Architecture

- **Engine:** Unity 6 LTS
- **Rendering:** Universal Render Pipeline (URP) with 2D lighting
- **Architecture:**
  - **Interface-driven, decoupled design** — `IDamageable`, `IInteractable`, `IInputProvider`,
    and the `TriggerableObject` abstract class as the only cross-system boundaries.
  - **Custom physics controller** — velocity-based movement, coyote time, jump buffering,
    asymmetric fall gravity; continuous collision detection to prevent tunnelling.
  - **Additive Scene Workflow + Git LFS** — environment and systems split across scenes to
    eliminate merge conflicts in two-person collaborative development.
  - **Data-driven decisions** — engineering priorities set by a pilot survey of 50 respondents.

### 👥 Authors

- **Aldibayeva Akku** (@Akku-2325)
- **Nurmukhammed Seitkhan** (@NurNurzhanuly)

**Scientific Supervisor:** Akhmetzhanova Shynar
**University:** Astana IT University (AITU)
**Year:** 2025–2026
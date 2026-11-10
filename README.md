# Ballroom Legends: Road to Blackpool

A 2D ballroom dancing competition game inspired by the Blackpool Dance Festival in England.
Pick your dancer, step onto a floor inspired by the Winter Gardens, dance the Waltz in time
with the music, and impress the judges round after round to win the championship.

> Status: early development (MVP in progress)

## MVP scope

- [ ] Main menu
- [ ] Character selection (boy or girl)
- [ ] One 2D ballroom (Blackpool Ballroom)
- [ ] Basic Waltz rhythm mechanic (3/4 time, timing windows: Perfect / Good / Miss)
- [ ] Simple tournament: judges' scores, ranking, advancing through rounds
- [ ] Results screen

Later: outfit and appearance customization, more dances (Tango, Foxtrot, Quickstep,
Viennese Waltz), Skating System judging, partner AI, music library.

## Tech stack

| Area | Choice |
| --- | --- |
| Engine | Unity 6.3 LTS (6000.3.x), Universal 2D template (URP) |
| Language | C# |
| Editor | Visual Studio Code + Unity extension (C# Dev Kit) |
| Version control | Git + GitHub |
| Tests | Unity Test Framework (NUnit), EditMode tests |
| Target | Desktop (macOS first, Windows later) |

## Getting started

1. Install Unity Hub and Unity **6000.3.x LTS** (the exact version is in
   `ProjectSettings/ProjectVersion.txt`).
2. Clone the repo: `git clone https://github.com/lilaibikesha/ballroom-legends.git`
3. In Unity Hub: **Add > Add project from disk** and pick the cloned folder.
4. First time only: menu **Ballroom Legends > Create Initial Scenes** (creates the 4 scenes).
5. Open `Assets/_Project/Scenes/MainMenu.unity` and press Play.
6. Run tests: **Window > General > Test Runner > EditMode > Run All**.

## Project structure

```
Assets/
  _Project/                 Everything we create (keeps it apart from imported assets)
    Scenes/                 MainMenu, CharacterSelection, BlackpoolBallroom, Results
    Scripts/                Runtime code (BallroomLegends.asmdef)
      Core/                 GameManager, GameState, SceneNames
      Characters/           CharacterSelection, DancerCharacter
      Dance/                DanceManager, HitResult
      Tournament/           TournamentManager, Couple, RoundType
    Editor/                 Editor-only tools (BallroomLegends.Editor.asmdef)
    Tests/EditMode/         Unit tests (BallroomLegends.Tests.EditMode.asmdef)
    Sprites/                Characters, Ballroom, UI
    Animations/
    Audio/                  Music, SFX
    Prefabs/
Packages/                   Unity package manifest (tracked)
ProjectSettings/            Unity project settings (tracked)
```

## Architecture

| Class | Responsibility |
| --- | --- |
| `GameManager` | Persistent singleton. Owns the game state machine, scene flow and the chosen character. |
| `CharacterSelection` | Character Selection screen: highlight and confirm boy or girl. |
| `DanceManager` | Waltz rhythm: tracks the beat from the audio clock and rates each step. |
| `TournamentManager` | Couples, judges' scores, rankings and round progression. |

Game flow: `MainMenu -> CharacterSelection -> BlackpoolBallroom (Dancing) -> Results`

Principles: one responsibility per class, pure logic (scoring, timing) kept free of Unity
APIs so it can be unit tested, and no magic strings (scene names live in `SceneNames`).

## Conventions

- C#: PascalCase for types, methods and properties; `_camelCase` for private fields;
  `[SerializeField] private` instead of public fields for Inspector values.
- One class per file; file name matches class name; namespaces follow folders
  (`BallroomLegends.Dance`, ...).
- Commits: short imperative messages, e.g. `Add Waltz timing windows`.
- Branches: `main` stays playable; work on `feature/<name>` branches and merge when done.

## Credits

Created by [lilaibikesha](https://github.com/lilaibikesha).

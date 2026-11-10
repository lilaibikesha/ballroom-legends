# Ballroom Legends: Road to Blackpool

A 2D ballroom dancing competition game inspired by the Blackpool Dance Festival in England.
Pick your dancer, step onto a floor inspired by the Winter Gardens, dance the Waltz in time
with the music, and impress the judges round after round to win the championship.

> Status: early development (MVP in progress)

## MVP scope

- [x] Main menu (Start Game)
- [x] Create your dancer: type a name, pick boy or girl
- [x] Dance Guide: Standard vs Latin, the 5 dances of each, a detail page per dance
      (history, characteristics, skills, tempo, video placeholder)
- [x] Choose a path: Standard only or Latin only
- [x] Ballroom Training Hall (home base)
- [x] One 2D ballroom (Blackpool Ballroom, placeholder art)
- [x] Basic Waltz rhythm mechanic (3/4 time, timing windows: Perfect / Good / Miss)
- [x] Simple tournament: 48 couples, 5 judges, recalls through Heat, Quarter-final,
      Semi-final and Final
- [x] Results screen

Code is written but still needs a first run in Unity: see `docs/COMPETITION.md` > Try it.

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
4. First time only: menu **Ballroom Legends > Create Initial Scenes** (creates the 6 scenes).
   Already ran it before the competition milestone? Run **Ballroom Legends > Rebuild Competition Scenes** once.
5. Open `Assets/_Project/Scenes/MainMenu.unity` and press Play.
6. Run tests: **Window > General > Test Runner > EditMode > Run All**.

## Project structure

```
Assets/
  _Project/                 Everything we create (keeps it apart from imported assets)
    Scenes/                 MainMenu, CharacterSelection, DanceGuide, TrainingHall,
                            BlackpoolBallroom, Results
    Data/                   DanceLibrary.asset + Dances/ (10 DanceInfo assets)
    Scripts/                Runtime code (BallroomLegends.asmdef)
      Core/                 GameManager, GameState, SceneNames
      Characters/           CharacterSelection (name + dancer), DancerCharacter
      Dance/                DanceStyle, DanceInfo, DanceLibrary, DanceManager, HitResult,
                            RhythmMath, PerformanceTracker, MetronomeClip
      Player/               PlayerNameRules
      UI/                   MainMenuScreen, DanceGuideScreen, ResultsScreen
      Training/             TrainingHallScreen
      Tournament/           TournamentManager, TournamentRules, JudgePanel, CompetitorField,
                            Couple, RoundType, RandomUtil
      Competition/          CompetitionController (ballroom scene), CompetitionResult,
                            RoundReport, RankingEntry, TextFormat
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
| `CharacterSelection` | Create-your-dancer screen: name input, highlight and confirm boy or girl. |
| `DanceInfo` / `DanceLibrary` | ScriptableObject data for the 10 dances (edited in the Inspector, read by code). |
| `DanceGuideScreen` | Dance Guide panels: style overview, dance list, dance detail, style choice. |
| `TrainingHallScreen` | The Ballroom Training Hall, the player's home base. Opens the competition. |
| `DanceManager` | Rhythm mechanic: click track on the audio clock, judges each step, raises events. |
| `RhythmMath` / `PerformanceTracker` | Pure timing math, and the Perfect / Good / Miss counts and 0-100 score. |
| `TournamentManager` | Registers couples, stores judges' marks, ranks, recalls the best to the next round. |
| `JudgePanel` / `CompetitorField` | Turns a dance into judges' marks; creates the 47 AI couples. |
| `CompetitionController` | Ballroom scene director: connects the dance, the tournament and the UI. |
| `ResultsScreen` | Shows the competition, round by round. |

Game flow: `MainMenu -> CharacterSelection -> DanceGuide -> TrainingHall -> BlackpoolBallroom (Dancing) -> Results`
(new-game flow: [docs/GAME_FLOW.md](docs/GAME_FLOW.md), competition: [docs/COMPETITION.md](docs/COMPETITION.md))

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

# New-game flow

```
MainMenu ──Start Game──> CharacterSelection ──Confirm──> DanceGuide ──Choose style──> TrainingHall
                         (name + boy/girl)              (preview the 10 dances)        (home base)
```

| Step | Scene | Script | What the player does |
| --- | --- | --- | --- |
| 1 | MainMenu | `MainMenuScreen` | Clicks **Start Game** |
| 2 | CharacterSelection | `CharacterSelection` | Types a name, picks boy or girl, **Confirm** |
| 3 | DanceGuide | `DanceGuideScreen` | Explores Standard and Latin, opens each dance |
| 4 | DanceGuide (last panel) | `DanceGuideScreen` | Picks **Standard only** or **Latin only** |
| 5 | TrainingHall | `TrainingHallScreen` | Starting point of the career |

`GameManager` remembers the choices across scenes: `PlayerName`, `SelectedCharacter`,
`ChosenStyle`.

## The Dance Guide (one scene, four panels)

Only one panel is visible at a time. `DanceGuideScreen.ShowOnly(panel)` switches them.

```
[1 Style overview]  Standard | Latin | I'm ready to choose
        │                │
        ▼                ▼
[2 Dance list]   5 buttons (Waltz, Tango, ...)   Back
        │
        ▼
[3 Dance detail] name, origin, history, music + tempo, characteristics, skills,
                 [Video coming soon] button, Back
[4 Style choice] Standard only | Latin only  ──> TrainingHall
```

| Standard (in competition order) | Latin (in competition order) |
| --- | --- |
| Waltz | Samba |
| Tango | Cha Cha Cha |
| Viennese Waltz | Rumba |
| Slow Foxtrot | Paso Doble |
| Quickstep | Jive |

The content (history, characteristics, skills, tempo, difficulty) is already written in
`Assets/_Project/Data/Dances/*.asset`. Click one in the Project window to read or edit it in
the Inspector. To add a video later: import an .mp4 into `Assets/_Project/Videos/` and drag
it into the dance's **Video** slot. The button switches from "Video coming soon" to
"Watch video" once you implement that check.

Why ScriptableObjects? The dance text is data, not logic. Keeping it in assets means you can
fix a typo or add a dance without touching code, and every screen reads the same data.

## Try it

1. Open the project in Unity. If the Console shows red errors, fix those first.
2. Menu **Ballroom Legends > Create Initial Scenes**. It imports the TextMeshPro essentials
   if needed, creates the 6 scenes, **builds the UI for the four new-game screens**, wires
   every button and Inspector slot, and plugs the DanceLibrary into the Dance Guide.
3. Open `Assets/_Project/Scenes/MainMenu.unity` and press **Play**.
   You can also press Play in any other scene: the GameManager creates itself on startup.
4. **Window > General > Test Runner > EditMode > Run All**. Expect everything green except
   the one ignored DanceManager test.

The generated UI is a plain placeholder layout (purple boxes, gold titles). Move, resize and
restyle anything in the Scene view; the code only cares about the Inspector references.
To rebuild a screen from scratch, delete its .unity file and run the menu again.

## How it works (read the code in this order)

1. `Core/GameManager.cs`: singleton, `CreateOnStartup` (auto-creation), the state machine
   (`CanTransition` is a tuple `switch` table, `SceneFor` / `StateForScene` map states and
   scenes), and the `Set...` methods that validate before saving.
2. `Player/PlayerNameRules.cs`: pure rules, one `GetError` method that both the UI and
   `IsValid` use. Tests: `PlayerNameRulesTests` (note the boundary tests).
3. `UI/MainMenuScreen.cs` and `Characters/CharacterSelection.cs`: guard clauses, then act.
4. `Dance/DanceLibrary.cs` + `Data/`: ScriptableObject data, `GetDances(style)`.
5. `UI/DanceGuideScreen.cs`: panel switching, buttons wired from code (and the
   loop-variable lambda trap), `StringBuilder` bullet lists, the disabled video button.
6. `Training/TrainingHallScreen.cs`: reads the choices back from the GameManager.
7. `Editor/UiBuilder.cs` + `Editor/ScreenBuilders.cs`: how the UI is generated.

Exercises if you want to practice on this code:
- Add a 6th test case to `GameStateTransitionTests` for a transition you think is illegal.
- Make the boy/girl cards show a picture (Image component + a sprite in `Sprites/Characters`).
- Show difficulty as filled and empty circles instead of "3 / 5".
- Videos: add a `VideoPlayer` + `RawImage` to the detail panel and play `_currentDance.Video`
  in `DanceGuideScreen.PlayVideo`.

## Next milestone

`DanceManager` (Waltz rhythm: `EvaluateStep` test-first, beat clock with
`AudioSettings.dspTime`) and `TournamentManager`, reached from the Training Hall's
"Go to competition" button.

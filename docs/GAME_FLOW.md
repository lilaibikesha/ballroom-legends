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

## Build the UI (in Unity)

Run **Ballroom Legends > Create Initial Scenes** first. It creates the 6 scenes, adds the
screen scripts, and plugs the DanceLibrary into `DanceGuideScreen` for you.

General recipe for every screen:
1. **GameObject > UI > Canvas**. Unity also adds an **EventSystem** (needed for clicks).
   On the Canvas Scaler set *UI Scale Mode = Scale With Screen Size*, 1920 x 1080.
2. Add text with **GameObject > UI > Text - TextMeshPro**. The first time, Unity asks to
   import *TMP Essentials*: click **Import TMP Essentials**.
3. Add buttons with **GameObject > UI > Button - TextMeshPro**.
4. Wire a button: select it > **On Click ()** > **+** > drag the screen object
   (e.g. `DanceGuideScreen`) into the slot > choose the method.
5. Drag UI objects into the screen script's Inspector slots.

Per screen:
- **MainMenu**: title text, *Start Game* button -> `MainMenuScreen.StartGame`.
- **CharacterSelection**: *Input Field - TextMeshPro* (-> `nameInput`), an error text
  (-> `errorText`), *Boy* / *Girl* buttons (-> `SelectBoy` / `SelectGirl`), two preview
  objects (-> `boyPreview` / `girlPreview`, any image for now), *Confirm* -> `ConfirmSelection`.
- **DanceGuide**: under the Canvas create 4 empty child objects named `StyleOverviewPanel`,
  `DanceListPanel`, `DanceDetailPanel`, `StyleChoicePanel` (right-click Canvas > Create Empty,
  then set the RectTransform to stretch). Put the buttons and texts from the diagram above
  inside them, and drag everything into `DanceGuideScreen`'s slots. Tip: put the long history
  text inside a **Scroll View** so it fits.
- **TrainingHall**: a welcome text (-> `welcomeText`). Background art comes later.

## Your coding tasks for this feature (in order)

1. `PlayerNameRules` test-first (`Tests/EditMode/PlayerNameRulesTests.cs`).
2. `GameManager.ChangeState` with the new states, plus the `Set...` validation.
3. `MainMenuScreen.StartGame` and `CharacterSelection.ConfirmSelection`.
4. `DanceLibrary.GetDances(style)`.
5. `DanceGuideScreen.ShowDanceList` (watch out for the loop-variable lambda trap).
6. `DanceGuideScreen.ShowDanceDetail`, including the "Video coming soon" state.
7. `DanceGuideScreen.ChooseStyle` and `TrainingHallScreen.Start`.

Test the whole flow by pressing Play in **MainMenu** (that's where the GameManager lives).

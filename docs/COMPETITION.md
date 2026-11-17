# Competition: five dances a round, judges, recalls, results

```
TrainingHall --Go to competition--> BlackpoolBallroom --(knocked out or Final done)--> Results --> TrainingHall
                                    (one scene, every round)
```

Like the real Blackpool Dance Festival, **every round has all five dances of your style**.
Inside the ballroom, every round goes the same way:

```
[Intro panel]   "Quarter-final: Standard. 24 couples in 2 heats. The best 12 are recalled."
      |          Dance! | Withdraw
      v
[HUD]           dance 1 of 5: Waltz. Count-in (just listen), then press an ARROW key on every lit beat
      |         "ONE  two  three" lights up, Perfect / Good / Miss, score, combo
      v         the judges hold up their marks for you
[Dance break]   "Waltz: 82/100", your marks, your place so far     Next: Tango | Withdraw
      |
      v         ... Tango, Viennese Waltz, Slow Foxtrot, Quickstep ...
[Round result]  every dance's score and marks, the round total, your place, the top 6
      |
      +-- recalled  -> next round's intro
      +-- not recalled, or the Final is over -> Results scene
```

| Round | Couples | On the floor | Recalled |
| --- | --- | --- | --- |
| First Round (Heat) | 48 | 4 heats of 12 | 24 |
| Quarter-final | 24 | 2 heats of 12 | 12 |
| Semi-final | 12 | everyone | 6 |
| Final | 6 | everyone | placed 1st to 6th |

A field of 4 couples or fewer has nobody to knock out, so it skips straight to the Final
(`TournamentRules.FirstRound`). The game always enters 48, so this matters for tests and
future small events.

| Style | Dances (in competition order) |
| --- | --- |
| Standard | Waltz, Tango, Viennese Waltz, Slow Foxtrot, Quickstep |
| Latin | Samba, Cha Cha Cha, Rumba, Paso Doble, Jive |

The judges mark every couple after every dance (3 judges, 1-10 each). Your round total is
the sum of all of them: 5 dances x 3 judges = 15 marks, so a perfect round is 150.
Ties are broken by the highest single mark, then by name (A to Z): see `TournamentRanking`.
A great Waltz can't save a weak Quickstep: you need to be an all-rounder.

## Steps: not every beat is a step

Each dance's tempo and time signature come from its `.asset` file. Pressing on every beat
of a Quickstep (204 beats a minute!) would be button mashing, so fast dances step on fewer
beats. The rule (`RhythmMath.StepInterval`): step on every beat, unless that's more than
140 steps a minute; then every 2nd beat; in 3/4, once a bar.

| Dance | Beats a minute | You step on | Count shows |
| --- | --- | --- | --- |
| Waltz | 87 | every beat | ONE two three |
| Tango | 64 | every beat | ONE two |
| Viennese Waltz | 177 | beat ONE only | ONE · · |
| Slow Foxtrot | 116 | every beat | ONE two three four |
| Quickstep | 204 | every 2nd beat | ONE · three · |
| Samba | 102 | every beat | ONE two |
| Cha Cha Cha | 124 | every beat | ONE two three four |
| Rumba | 104 | every beat | ONE two three four |
| Paso Doble | 122 | every beat | ONE two |
| Jive | 172 | every 2nd beat | ONE · three · |

You still hear the in-between beats as soft ticks, so you can feel the tempo. Every dance
lasts about 24 seconds (`Routine Seconds`): slow dances get fewer bars, fast ones more.

## On the floor

`BallroomFloor` draws your heat: up to 12 couples seen from above (her skirt, his
shoulders), the three judges along the front and the audience in the side galleries.

- **Standard**: couples travel anticlockwise around the floor (the "line of dance"). Your
  couple (gold dress, gold ring, "You") only moves forward when you dance the step. A Miss
  makes you wobble and flash red; a Perfect gives a gold glow.
- **Latin**: couples dance on the spot, opening out and closing in on every step.
- AI couples wobble now and then; weaker couples more often.
- After each dance, the judges hold up paddles with **your** marks.
- When you're recalled (or make the podium) the audience cheers.

All of it is placeholder art made in code (`FloorShapes` draws a circle into a texture).
Real sprites can replace it later without touching the competition logic.

## Try it

1. Open the project. Fix any red Console errors first (and paste them to me if they're
   confusing).
2. First time? **Ballroom Legends > Create Initial Scenes** builds everything. Already
   created the scenes before this update? Run **Ballroom Legends > Rebuild Competition
   Scenes** once: it regenerates the Ballroom (new floor, dance break panel) and Results.
3. Open `MainMenu.unity`, press **Play**, go through the new-game flow, then
   **Go to competition** in the Training Hall. You can also press Play directly in
   `BlackpoolBallroom.unity` (it uses Standard).
4. Turn the sound on. Listen to the count-in of low clicks, then press any **arrow key**
   on every lit beat (Space and a gamepad also work). The high click is beat ONE.
5. Testing and five dances take too long? Select **CompetitionController** and set
   `Dances Per Round` to 1 or 2.
6. **Test Runner > EditMode > Run All**. Everything should be green.

Tuning lives in the Inspector, no code needed:

- `DanceManager`: `Routine Seconds` (default 24), `Max Steps Per Minute` (140), timing
  windows (Perfect 0.05 s, Good 0.12 s), `Input Offset Seconds` if your presses are always
  judged late.
- `CompetitionController`: `Dances Per Round` (1-5), AI skill range (45-88), how much
  judges disagree (0.8), and `Random Seed` (any number other than 0 replays the exact same
  competition: great for debugging).
- `BallroomFloor` (on FloorStage): floor size, couples per heat (12), steps per lap,
  how often AI couples wobble, judge and audience positions.

Difficulty, from a simulation of 2,000 competitions per average dance score (five dances a
round). Adding up five dances makes luck matter less than with one dance. (Measured with 5
judges; with 3 judges each total has a little more luck in it, so expect small changes.)

| Your average score | Past the First Round | Reach the Semi-final | Reach the Final | Win |
| --- | --- | --- | --- | --- |
| 65 | 35% | 0% | 0% | 0% |
| 70 | 84% | 1% | 0% | 0% |
| 75 | 100% | 23% | 0% | 0% |
| 80 | 100% | 81% | 16% | 0% |
| 85 | 100% | 100% | 82% | 8% |
| 90 | 100% | 100% | 100% | 74% |

## How it works (read the code in this order)

1. `Dance/RhythmMath.cs`: pure timing math. Beat times, nearest beat, early/late offset,
   beat-in-bar with a true modulo (C#'s `%` is negative for negative numbers). The beat
   clock: `SongPositionInBeats` and `SecondsFromNearestBeat` (doubles, because `dspTime`
   is a double). The step part: `StepInterval`, `RoutineBars`, `CountInBeats`, `NearestStep`.
2. `Dance/DanceManager.cs`:
   - **Why `AudioSettings.dspTime`?** It's the audio hardware's clock, the same clock the
     music plays on. `Time.time` is the frame clock: it drifts from the audio and jumps
     on slow frames. `PlayScheduled(startDsp)` starts the music on an exact audio-clock
     time, so beat 0 and the first click line up.
   - `Update` does four things: announce beats that have passed (a `while` loop, because
     a slow frame can skip several), judge an arrow-key press against the nearest **step**,
     turn steps whose Good window has closed into Misses, and finish.
   - Pressing twice on one step, or during the count-in, is ignored: no points for mashing.
     Pressing on an in-between beat of a fast dance counts as a badly-timed press for the
     nearest step.
   - It talks to the rest of the game through C# **events** (`BeatTicked`, `StepJudged`,
     `DanceFinished`). That's the Observer pattern: the rhythm code doesn't know the UI
     exists.
3. `Dance/PerformanceTracker.cs`: Perfect = 1 point, Good = 0.5, Miss = 0, out of all the
   steps, so the score is 0-100. Plus combo and best combo.
4. `Dance/MetronomeClip.cs`: sound is an array of numbers. Each click is a fading sine wave
   written into a `float[]`, then into an `AudioClip`. Beats without a step get a soft tick.
5. `Tournament/JudgePanel.cs`: mark = skill / 10 + a bell-curve "opinion" (Box-Muller, in
   `RandomUtil`), rounded and clamped to 1-10. The `System.Random` is passed in
   (dependency injection), so tests use a fixed seed and get the same marks every time.
6. `Tournament/TournamentManager.cs`: `AddDanceScores` adds one dance's marks to a couple;
   guard clauses on every input; registration closes when the first marks arrive.
   `Tournament/TournamentRanking.cs`: the pure ranking rules with LINQ
   (`OrderByDescending(...).ThenByDescending(...).ThenBy(...)`): the total of all dances,
   ties broken by the best single mark and then the name.
7. `Competition/RoundProgramme.cs`, `DanceReport.cs`, `RoundReport.cs`: which dances a round
   has, and what you scored in each one. `RoundReport.AddDance` keeps the round totals.
8. `Competition/CompetitionController.cs`: the only class that knows the dance, the
   tournament and the floor. It runs the five dances, has the judges mark every couple after
   each one (AI couples vary a bit from dance to dance), and saves a `CompetitionResult` in
   the GameManager before loading the Results scene.
9. `Floor/FloorLayout.cs` (pure geometry: ellipse, heats, who's in your heat),
   `Floor/CoupleView.cs` (one couple: glides to its target, bounces, wobbles) and
   `Floor/BallroomFloor.cs` (the whole floor, judges and audience).
10. `UI/ResultsScreen.cs`: reads that result back.

## Your turn (exercises, in rough order of difficulty)

1. **Calibrate.** Play a round and watch the feedback. If you see "a bit late" all the
   time, set `Input Offset Seconds` to about 0.03. Why does Bluetooth audio make this
   worse?
2. **Space to start.** Let SPACE also press "Dance!" and "Next: Tango". Careful: the same
   press must not count as a step (hint: which `Update` runs first?).
3. **Hold the beat.** Show a short "Combo x10!" message every 10 steps in a row, and make
   the audience cheer for it (`BallroomFloor.Cheer`).
4. **Your colors.** Pass the player's chosen character (boy or girl) to the floor and put
   the "You" label and the gold color on the right partner.
5. **Real music.** Import a Waltz track into `Assets/_Project/Audio/Music`, assign it to the
   AudioSource, turn off `Use Generated Click Track`, and add a `firstBeatSeconds` field so
   the beat grid starts where the music's first beat is.
6. **Real rhythms.** Tango isn't "every beat": it's slow, slow, quick-quick-slow. Give
   `DanceInfo` a step pattern (e.g. `"S S Q Q S"`) and make `DanceManager` use it instead
   of `StepInterval`.
7. **Smoother clock.** `dspTime` only updates once per audio buffer (every ~10-20 ms).
   Research how rhythm games smooth it with `Time.unscaledDeltaTime` between updates.
8. **The Skating System.** Real judges don't give marks out of 10 in a Final, they place
   couples 1st to 6th in each dance, and a couple's place is decided by the majority of
   judges. Then the five dance places are added up. Implement it in a new pure class
   (`SkatingSystem.Place(int[,] placements)`), test-first. It's a classic algorithms
   exercise.

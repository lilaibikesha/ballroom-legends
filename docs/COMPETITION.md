# Competition: rhythm, judges, rounds, results

```
TrainingHall --Go to competition--> BlackpoolBallroom --(knocked out or Final done)--> Results --> TrainingHall
                                    (one scene, every round)
```

Inside the ballroom, every round goes the same way:

```
[Intro panel]  "First Round: Waltz. 48 couples, the best 24 are recalled."   Dance! | Withdraw
      |
      v
[HUD]          count-in (just listen) -> 48 steps, press SPACE on every beat
      |        "ONE  two  three" lights up, Perfect / Good / Miss, score, combo
      v
[Round result] your dance score, the 5 judges' marks, your place, the top 6
      |
      +-- recalled  -> next round's intro
      +-- not recalled, or the Final is over -> Results scene
```

| Round | Couples | Recalled |
| --- | --- | --- |
| First Round (Heat) | 48 | 24 |
| Quarter-final | 24 | 12 |
| Semi-final | 12 | 6 |
| Final | 6 | placed 1st to 6th |

Your competition dance is the first dance of your style: **Waltz** for Standard
(3/4, 29 bars a minute = 87 BPM) and **Samba** for Latin (2/4, 51 bars = 102 BPM).
The tempo and time signature come from the dance's `.asset` file, not from code.

## Try it

1. Pull the latest `main` and open the project. Fix any red Console errors first (and
   paste them to me if they're confusing).
2. If you already ran **Create Initial Scenes** before, run
   **Ballroom Legends > Rebuild Competition Scenes** once. The Ballroom and Results scenes
   used to be empty; this regenerates just those two. First time? **Create Initial Scenes**
   builds everything.
3. Open `MainMenu.unity`, press **Play**, go through the new-game flow, then
   **Go to competition** in the Training Hall. You can also press Play directly in
   `BlackpoolBallroom.unity`.
4. Turn the sound on. One count-in bar of low clicks, then press **SPACE** on every click.
   The high click is beat ONE.
5. **Test Runner > EditMode > Run All**. Everything should be green (no ignored tests now).

Tuning lives in the Inspector, no code needed:

- `DanceManager`: routine length (`Routine Bars`, default 16), timing windows (Perfect
  0.05 s, Good 0.12 s), `Input Offset Seconds` if your presses are always judged late.
- `CompetitionController`: AI skill range (45-88), how much judges disagree (0.8), and
  `Random Seed` (any number other than 0 replays the exact same competition: great for
  debugging).

Difficulty (from a simulation of 2,000 competitions per score): a dance score around 70
usually gets you past the first round, about 80 reaches the Semi-final, the mid 80s
usually make the Final, and 90+ gives you a real chance to win (95+ wins most of the time).

## How it works (read the code in this order)

1. `Dance/RhythmMath.cs`: pure timing math. Beat times, nearest beat, early/late offset,
   beat-in-bar with a true modulo (C#'s `%` is negative for negative numbers).
2. `Dance/DanceManager.cs`:
   - **Why `AudioSettings.dspTime`?** It's the audio hardware's clock, the same clock the
     music plays on. `Time.time` is the frame clock: it drifts from the audio and jumps
     on slow frames. `PlayScheduled(startDsp)` starts the music on an exact audio-clock
     time, so beat 0 and the first click line up.
   - `Update` does four things: announce beats that have passed (a `while` loop, because
     a slow frame can skip several), judge a SPACE press against the nearest beat, turn
     beats whose Good window has closed into Misses, and finish.
   - Pressing twice on one beat, or during the count-in, is ignored: no points for mashing.
   - It talks to the rest of the game through C# **events** (`BeatTicked`, `StepJudged`,
     `DanceFinished`). That's the Observer pattern: the rhythm code doesn't know the UI
     exists.
3. `Dance/PerformanceTracker.cs`: Perfect = 1 point, Good = 0.5, Miss = 0, out of all the
   steps, so the score is 0-100. Plus combo and best combo.
4. `Dance/MetronomeClip.cs`: sound is an array of numbers. Each click is a fading sine wave
   written into a `float[]`, then into an `AudioClip`. Replace it with real music later
   by assigning a clip to the DanceManager's AudioSource (see the exercises).
5. `Tournament/JudgePanel.cs`: mark = skill / 10 + a bell-curve "opinion" (Box-Muller, in
   `RandomUtil`), rounded and clamped to 1-10. The `System.Random` is passed in
   (dependency injection), so tests use a fixed seed and get the same marks every time.
6. `Tournament/TournamentManager.cs`: guard clauses on every input, ranking with LINQ
   (`OrderByDescending(...).ThenByDescending(...)`), ties broken by best single mark and
   then start number (LINQ's sort is *stable*, so the original order survives as the last
   tie-breaker). That's why the player gets a random start number.
7. `Competition/CompetitionController.cs`: the only class that knows both the dance and the
   tournament. It turns your dance score into your skill for the round, has the judges mark
   all couples (AI couples vary a bit from round to round), and saves a
   `CompetitionResult` in the GameManager before loading the Results scene (the ballroom's
   objects are destroyed when a new scene loads).
8. `UI/ResultsScreen.cs`: reads that result back.

## Your turn (exercises, in rough order of difficulty)

1. **Calibrate.** Play a round and watch the feedback. If you see "a bit late" all the
   time, set `Input Offset Seconds` to about 0.03. Why does Bluetooth audio make this
   worse?
2. **Space to start.** Let SPACE also press "Dance!" on the intro panel. Careful: the same
   press must not count as a step (hint: which `Update` runs first?).
3. **Hold the beat.** Show a short "Combo x10!" message every 10 steps in a row.
4. **Real music.** Import a Waltz track into `Assets/_Project/Audio/Music`, assign it to the
   AudioSource, turn off `Use Generated Click Track`, and add a `firstBeatSeconds` field so
   the beat grid starts where the music's first beat is.
5. **Smoother clock.** `dspTime` only updates once per audio buffer (every ~10-20 ms).
   Research how rhythm games smooth it with `Time.unscaledDeltaTime` between updates.
6. **The Skating System.** Real judges don't give marks out of 10 in a Final, they place
   couples 1st to 6th, and a couple's place is decided by the majority of judges. Implement
   it in a new pure class (`SkatingSystem.Place(int[,] placements)`), test-first. It's a
   classic algorithms exercise.

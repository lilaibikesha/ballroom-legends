using BallroomLegends.Characters;
using BallroomLegends.Dance;
using BallroomLegends.Player;
using BallroomLegends.Training;
using BallroomLegends.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BallroomLegends.EditorTools
{
    /// <summary>
    /// Builds the placeholder UI for each screen of the new-game flow and wires it to the
    /// screen scripts. It's the same result as building it by hand (GameObject > UI > ...,
    /// dragging references into the Inspector, wiring On Click ()), just repeatable.
    /// Restyle freely afterwards: nothing in the code depends on positions or colors.
    /// </summary>
    internal static class ScreenBuilders
    {
        private const string DanceLibraryPath = "Assets/_Project/Data/DanceLibrary.asset";

        public static void BuildMainMenu()
        {
            var screen = new GameObject("MainMenuScreen").AddComponent<MainMenuScreen>();
            RectTransform canvas = UiBuilder.CreateCanvas();

            UiBuilder.CreateText(canvas, "Title", "Ballroom Legends", 120, new Vector2(0, 260), new Vector2(1600, 170)).color = UiBuilder.Gold;
            UiBuilder.CreateText(canvas, "Subtitle", "Road to Blackpool", 56, new Vector2(0, 140), new Vector2(1200, 90));
            UiBuilder.CreateButton(canvas, "StartGameButton", "Start Game", new Vector2(0, -60), new Vector2(520, 120), screen.StartGame, 44);
            UiBuilder.CreateButton(canvas, "QuitButton", "Quit", new Vector2(0, -210), new Vector2(520, 100), screen.QuitGame);
        }

        public static void BuildCharacterSelection()
        {
            var screen = new GameObject("CharacterSelection").AddComponent<CharacterSelection>();
            RectTransform canvas = UiBuilder.CreateCanvas();

            UiBuilder.CreateText(canvas, "Title", "Create your dancer", 80, new Vector2(0, 430), new Vector2(1600, 120)).color = UiBuilder.Gold;
            UiBuilder.CreateText(canvas, "NameLabel", "Your name", 40, new Vector2(0, 330), new Vector2(800, 60));
            TMP_InputField nameInput = UiBuilder.CreateInputField(canvas, "NameInput", "Type your name...",
                new Vector2(0, 250), new Vector2(720, 90), PlayerNameRules.MaxLength);
            TMP_Text errorText = UiBuilder.CreateText(canvas, "ErrorText", "", 32, new Vector2(0, 175), new Vector2(1200, 50));
            errorText.color = UiBuilder.ErrorColor;

            // Gold frames behind the cards: CharacterSelection switches one on when highlighted.
            RectTransform boyFrame = UiBuilder.CreateBox(canvas, "BoyHighlight", new Vector2(-300, -110), new Vector2(380, 440), UiBuilder.Gold);
            RectTransform girlFrame = UiBuilder.CreateBox(canvas, "GirlHighlight", new Vector2(300, -110), new Vector2(380, 440), UiBuilder.Gold);
            boyFrame.gameObject.SetActive(false);
            girlFrame.gameObject.SetActive(false);

            UiBuilder.CreateButton(canvas, "BoyButton", "Boy", new Vector2(-300, -110), new Vector2(340, 400), screen.SelectBoy, 48);
            UiBuilder.CreateButton(canvas, "GirlButton", "Girl", new Vector2(300, -110), new Vector2(340, 400), screen.SelectGirl, 48);
            UiBuilder.CreateButton(canvas, "ConfirmButton", "Confirm", new Vector2(0, -430), new Vector2(460, 100), screen.ConfirmSelection, 40);

            UiBuilder.SetField(screen, "nameInput", nameInput);
            UiBuilder.SetField(screen, "errorText", errorText);
            UiBuilder.SetField(screen, "boyPreview", boyFrame.gameObject);
            UiBuilder.SetField(screen, "girlPreview", girlFrame.gameObject);
        }

        public static void BuildDanceGuide()
        {
            var screen = new GameObject("DanceGuideScreen").AddComponent<DanceGuideScreen>();
            RectTransform canvas = UiBuilder.CreateCanvas();

            var library = AssetDatabase.LoadAssetAtPath<DanceLibrary>(DanceLibraryPath);
            if (library == null)
            {
                Debug.LogWarning($"[Setup] {DanceLibraryPath} not found. Assign the library by hand.");
            }
            else
            {
                UiBuilder.SetField(screen, "library", library);
            }

            // ----- Panel 1: style overview -----
            RectTransform overview = UiBuilder.CreateContainer(canvas, "StyleOverviewPanel");
            UiBuilder.CreateText(overview, "Title", "Two styles, ten dances", 76, new Vector2(0, 420), new Vector2(1600, 110)).color = UiBuilder.Gold;
            UiBuilder.CreateText(overview, "Intro", "Ballroom competitions have two styles with five dances each. Pick one to meet its dances.",
                34, new Vector2(0, 330), new Vector2(1500, 90));

            BuildStyleCard(overview, "StandardCard", new Vector2(-420, 20), "Standard",
                "Smooth, elegant dances in a closed hold that travel around the floor.\n\nWaltz, Tango, Viennese Waltz, Slow Foxtrot, Quickstep",
                "Explore Standard", screen.ShowStandardDances);
            BuildStyleCard(overview, "LatinCard", new Vector2(420, 20), "Latin",
                "Rhythmic, expressive dances with hip action and open positions.\n\nSamba, Cha Cha Cha, Rumba, Paso Doble, Jive",
                "Explore Latin", screen.ShowLatinDances);

            UiBuilder.CreateButton(overview, "ReadyButton", "I'm ready to choose", new Vector2(0, -420), new Vector2(620, 100), screen.ShowStyleChoice, 40);

            // ----- Panel 2: dance list -----
            RectTransform list = UiBuilder.CreateContainer(canvas, "DanceListPanel");
            TMP_Text listTitle = UiBuilder.CreateText(list, "Title", "Dances", 76, new Vector2(0, 420), new Vector2(1600, 110));
            listTitle.color = UiBuilder.Gold;

            var danceButtons = new Object[5];
            for (int i = 0; i < danceButtons.Length; i++)
            {
                // No click listener here: DanceGuideScreen adds them from code, because which
                // dance a button opens depends on the style the player picked.
                danceButtons[i] = UiBuilder.CreateButton(list, $"DanceButton{i + 1}", "Dance", new Vector2(0, 250 - i * 125), new Vector2(640, 105), null, 40);
            }

            UiBuilder.CreateButton(list, "BackButton", "Back", new Vector2(0, -420), new Vector2(360, 90), screen.ShowStyleOverview);

            // ----- Panel 3: dance detail -----
            RectTransform detail = UiBuilder.CreateContainer(canvas, "DanceDetailPanel");
            TMP_Text danceName = UiBuilder.CreateText(detail, "DanceName", "Dance", 84, new Vector2(0, 440), new Vector2(1600, 110));
            danceName.color = UiBuilder.Gold;
            TMP_Text origin = UiBuilder.CreateText(detail, "Origin", "Origin", 34, new Vector2(0, 360), new Vector2(1600, 60));

            UiBuilder.CreateBox(detail, "HistoryBox", new Vector2(-440, 20), new Vector2(920, 580), UiBuilder.Panel);
            TMP_Text history = UiBuilder.CreateText(detail, "History", "History", 30, new Vector2(-440, 20), new Vector2(860, 530), TextAlignmentOptions.TopLeft);
            history.enableAutoSizing = true;
            history.fontSizeMin = 20;
            history.fontSizeMax = 32;

            UiBuilder.CreateBox(detail, "InfoBox", new Vector2(500, 20), new Vector2(800, 580), UiBuilder.Panel);
            TMP_Text music = UiBuilder.CreateText(detail, "Music", "Music", 28, new Vector2(500, 225), new Vector2(740, 140), TextAlignmentOptions.TopLeft);
            TMP_Text characteristics = UiBuilder.CreateText(detail, "Characteristics", "Characteristics", 28, new Vector2(500, 50), new Vector2(740, 200), TextAlignmentOptions.TopLeft);
            TMP_Text skills = UiBuilder.CreateText(detail, "Skills", "Skills", 28, new Vector2(500, -150), new Vector2(740, 200), TextAlignmentOptions.TopLeft);

            Button videoButton = UiBuilder.CreateButton(detail, "VideoButton", "Video coming soon", new Vector2(-440, -390), new Vector2(560, 100), screen.PlayVideo);
            UiBuilder.CreateButton(detail, "BackButton", "Back to dances", new Vector2(500, -390), new Vector2(460, 100), screen.BackToDanceList);

            // ----- Panel 4: style choice -----
            RectTransform choice = UiBuilder.CreateContainer(canvas, "StyleChoicePanel");
            UiBuilder.CreateText(choice, "Title", "Choose your path", 80, new Vector2(0, 380), new Vector2(1600, 120)).color = UiBuilder.Gold;
            UiBuilder.CreateText(choice, "Intro", "Which style do you want to train and compete in?", 38, new Vector2(0, 270), new Vector2(1500, 70));
            UiBuilder.CreateButton(choice, "StandardOnlyButton", "Standard only", new Vector2(-360, 0), new Vector2(600, 180), screen.ChooseStandard, 52);
            UiBuilder.CreateButton(choice, "LatinOnlyButton", "Latin only", new Vector2(360, 0), new Vector2(600, 180), screen.ChooseLatin, 52);
            UiBuilder.CreateButton(choice, "BackButton", "Back to the guide", new Vector2(0, -320), new Vector2(500, 100), screen.ShowStyleOverview);

            // Wire everything into DanceGuideScreen's Inspector slots.
            UiBuilder.SetField(screen, "styleOverviewPanel", overview.gameObject);
            UiBuilder.SetField(screen, "danceListPanel", list.gameObject);
            UiBuilder.SetField(screen, "danceDetailPanel", detail.gameObject);
            UiBuilder.SetField(screen, "styleChoicePanel", choice.gameObject);
            UiBuilder.SetField(screen, "danceListTitle", listTitle);
            UiBuilder.SetArray(screen, "danceButtons", danceButtons);
            UiBuilder.SetField(screen, "danceNameText", danceName);
            UiBuilder.SetField(screen, "originText", origin);
            UiBuilder.SetField(screen, "historyText", history);
            UiBuilder.SetField(screen, "musicText", music);
            UiBuilder.SetField(screen, "characteristicsText", characteristics);
            UiBuilder.SetField(screen, "skillsText", skills);
            UiBuilder.SetField(screen, "videoButton", videoButton);
            UiBuilder.SetField(screen, "videoButtonLabel", videoButton.GetComponentInChildren<TMP_Text>());

            // Only the overview is visible when the scene opens (the script also enforces this).
            list.gameObject.SetActive(false);
            detail.gameObject.SetActive(false);
            choice.gameObject.SetActive(false);
        }

        public static void BuildTrainingHall()
        {
            var screen = new GameObject("TrainingHallScreen").AddComponent<TrainingHallScreen>();
            RectTransform canvas = UiBuilder.CreateCanvas();

            UiBuilder.CreateText(canvas, "Title", "Ballroom Training Hall", 84, new Vector2(0, 400), new Vector2(1600, 120)).color = UiBuilder.Gold;
            TMP_Text welcome = UiBuilder.CreateText(canvas, "WelcomeText", "Welcome!", 44, new Vector2(0, 170), new Vector2(1500, 220));

            UiBuilder.CreateButton(canvas, "CompetitionButton", "Go to competition", new Vector2(0, -90), new Vector2(560, 100), screen.GoToCompetition);
            UiBuilder.CreateButton(canvas, "DanceGuideButton", "Dance Guide", new Vector2(0, -220), new Vector2(560, 100), screen.OpenDanceGuide);
            UiBuilder.CreateButton(canvas, "MainMenuButton", "Main menu", new Vector2(0, -350), new Vector2(560, 100), screen.BackToMainMenu);

            UiBuilder.SetField(screen, "welcomeText", welcome);
        }

        private static void BuildStyleCard(Transform parent, string name, Vector2 position, string title,
            string description, string buttonLabel, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform card = UiBuilder.CreateBox(parent, name, position, new Vector2(700, 520), UiBuilder.Panel);
            UiBuilder.CreateText(card, "Title", title, 64, new Vector2(0, 180), new Vector2(640, 90)).color = UiBuilder.Gold;
            UiBuilder.CreateText(card, "Description", description, 32, new Vector2(0, 10), new Vector2(620, 240));
            UiBuilder.CreateButton(card, "ExploreButton", buttonLabel, new Vector2(0, -180), new Vector2(440, 100), onClick);
        }
    }
}

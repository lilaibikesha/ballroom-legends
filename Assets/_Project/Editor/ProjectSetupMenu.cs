using System.Collections.Generic;
using System.IO;
using BallroomLegends.Core;
using BallroomLegends.Dance;
using BallroomLegends.Tournament;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BallroomLegends.EditorTools
{
    /// <summary>
    /// Editor-only helper (never ships in the game). Adds a "Ballroom Legends" menu to the
    /// Unity menu bar that creates the game's scenes and registers them in Build Settings.
    /// Safe to run more than once: existing scenes are never overwritten.
    /// </summary>
    public static class ProjectSetupMenu
    {
        private const string ScenesFolder = "Assets/_Project/Scenes";

        [MenuItem("Ballroom Legends/Create Initial Scenes")]
        public static void CreateInitialScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            // The screens use TextMeshPro, which needs its essential resources first.
            if (!UiBuilder.EnsureTmpEssentials())
            {
                return;
            }

            Directory.CreateDirectory(ScenesFolder);
            var buildScenes = new List<EditorBuildSettingsScene>();

            foreach (string sceneName in SceneNames.All)
            {
                string path = $"{ScenesFolder}/{sceneName}.unity";

                if (File.Exists(path))
                {
                    Debug.Log($"[Setup] {path} already exists, skipped.");
                }
                else
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    CreateCamera();
                    AddSceneObjects(sceneName);
                    EditorSceneManager.SaveScene(scene, path);
                    Debug.Log($"[Setup] Created {path}");
                }

                buildScenes.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene($"{ScenesFolder}/{SceneNames.MainMenu}.unity");
            Debug.Log($"[Setup] Done. Build Settings now list: {string.Join(", ", SceneNames.All)}.");
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.07f, 0.15f);

            cameraObject.AddComponent<AudioListener>();
        }

        private static void AddSceneObjects(string sceneName)
        {
            // No GameManager here: it creates itself when you press Play (see
            // GameManager.CreateOnStartup), so every scene can be played on its own.
            switch (sceneName)
            {
                case SceneNames.MainMenu:
                    ScreenBuilders.BuildMainMenu();
                    break;
                case SceneNames.CharacterSelection:
                    ScreenBuilders.BuildCharacterSelection();
                    break;
                case SceneNames.DanceGuide:
                    ScreenBuilders.BuildDanceGuide();
                    break;
                case SceneNames.TrainingHall:
                    ScreenBuilders.BuildTrainingHall();
                    break;
                case SceneNames.BlackpoolBallroom:
                    new GameObject("DanceManager").AddComponent<DanceManager>();
                    new GameObject("TournamentManager").AddComponent<TournamentManager>();
                    break;
                case SceneNames.Results:
                    // Empty for now: you'll build the results UI here.
                    break;
            }
        }
    }
}

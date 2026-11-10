using System.Collections.Generic;
using System.IO;
using BallroomLegends.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BallroomLegends.EditorTools
{
    /// <summary>
    /// Editor-only helper (never ships in the game). Adds a "Ballroom Legends" menu to the
    /// Unity menu bar that creates the game's scenes and registers them in Build Settings.
    ///
    ///  - Create Initial Scenes: creates any scene that doesn't exist yet. Safe to run
    ///    more than once: existing scenes are never overwritten.
    ///  - Rebuild Competition Scenes: replaces BlackpoolBallroom and Results with freshly
    ///    generated ones (use it once if you created the scenes before the competition
    ///    milestone, when those two scenes were still empty).
    /// </summary>
    public static class ProjectSetupMenu
    {
        private const string ScenesFolder = "Assets/_Project/Scenes";

        [MenuItem("Ballroom Legends/Create Initial Scenes")]
        public static void CreateInitialScenes()
        {
            if (!PrepareToBuild())
            {
                return;
            }

            foreach (string sceneName in SceneNames.All)
            {
                string path = PathFor(sceneName);

                if (File.Exists(path))
                {
                    Debug.Log($"[Setup] {path} already exists, skipped.");
                }
                else
                {
                    CreateScene(sceneName);
                }
            }

            FinishBuild(SceneNames.MainMenu);
        }

        [MenuItem("Ballroom Legends/Rebuild Competition Scenes")]
        public static void RebuildCompetitionScenes()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Rebuild competition scenes?",
                "This replaces BlackpoolBallroom.unity and Results.unity with freshly generated ones. " +
                "Anything you changed by hand in those two scenes will be lost.",
                "Rebuild",
                "Cancel");

            if (!confirmed || !PrepareToBuild())
            {
                return;
            }

            // Leave the scenes we're about to replace, so neither file is open.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            foreach (string sceneName in new[] { SceneNames.BlackpoolBallroom, SceneNames.Results })
            {
                string path = PathFor(sceneName);
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }

                CreateScene(sceneName);
            }

            FinishBuild(SceneNames.BlackpoolBallroom);
        }

        private static string PathFor(string sceneName) => $"{ScenesFolder}/{sceneName}.unity";

        /// <summary>Asks to save open changes and makes sure TextMeshPro is ready.</summary>
        private static bool PrepareToBuild()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return false;
            }

            // The screens use TextMeshPro, which needs its essential resources first.
            if (!UiBuilder.EnsureTmpEssentials())
            {
                return false;
            }

            Directory.CreateDirectory(ScenesFolder);
            return true;
        }

        private static void CreateScene(string sceneName)
        {
            string path = PathFor(sceneName);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            AddSceneObjects(sceneName);
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[Setup] Created {path}");
        }

        /// <summary>Lists every scene in Build Settings (in SceneNames.All order) and opens one.</summary>
        private static void FinishBuild(string sceneToOpen)
        {
            var buildScenes = new List<EditorBuildSettingsScene>();
            foreach (string sceneName in SceneNames.All)
            {
                string path = PathFor(sceneName);
                if (File.Exists(path))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(path, true));
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(PathFor(sceneToOpen));
            Debug.Log($"[Setup] Done. Build Settings list {buildScenes.Count} scenes.");
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

            // The AudioListener is the game's "ears": without one you'd hear no music.
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
                    ScreenBuilders.BuildBlackpoolBallroom();
                    break;
                case SceneNames.Results:
                    ScreenBuilders.BuildResults();
                    break;
            }
        }
    }
}

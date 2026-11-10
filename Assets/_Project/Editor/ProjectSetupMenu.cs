using System.Collections.Generic;
using System.IO;
using BallroomLegends.Characters;
using BallroomLegends.Core;
using BallroomLegends.Dance;
using BallroomLegends.Tournament;
using BallroomLegends.Training;
using BallroomLegends.UI;
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
        private const string DanceLibraryPath = "Assets/_Project/Data/DanceLibrary.asset";

        [MenuItem("Ballroom Legends/Create Initial Scenes")]
        public static void CreateInitialScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
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
            switch (sceneName)
            {
                case SceneNames.MainMenu:
                    new GameObject("GameManager").AddComponent<GameManager>();
                    new GameObject("MainMenuScreen").AddComponent<MainMenuScreen>();
                    break;
                case SceneNames.CharacterSelection:
                    new GameObject("CharacterSelection").AddComponent<CharacterSelection>();
                    break;
                case SceneNames.DanceGuide:
                {
                    var guide = new GameObject("DanceGuideScreen").AddComponent<DanceGuideScreen>();
                    AssignDanceLibrary(guide);
                    break;
                }
                case SceneNames.TrainingHall:
                    new GameObject("TrainingHallScreen").AddComponent<TrainingHallScreen>();
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

        /// <summary>
        /// Fills the private [SerializeField] "library" field, exactly like dragging the
        /// DanceLibrary asset into the Inspector slot by hand. SerializedObject is how editor
        /// code edits serialized fields.
        /// </summary>
        private static void AssignDanceLibrary(DanceGuideScreen guide)
        {
            var library = AssetDatabase.LoadAssetAtPath<DanceLibrary>(DanceLibraryPath);
            if (library == null)
            {
                Debug.LogWarning($"[Setup] {DanceLibraryPath} not found. Assign the library by hand.");
                return;
            }

            var serialized = new SerializedObject(guide);
            serialized.FindProperty("library").objectReferenceValue = library;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

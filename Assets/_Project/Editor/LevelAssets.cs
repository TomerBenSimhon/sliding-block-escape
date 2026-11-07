using System;
using SlidingBlock.Data;
using UnityEditor;
using UnityEngine;

namespace SlidingBlock.Editor
{
    // File operations on level assets: create, duplicate, save.
    // Editing a level's contents is LevelDataEditing's job; asking the user for a path is the window's.
    public static class LevelAssets
    {
        public const string LevelsFolder = "Assets/_Project/Levels";

        // Creates an empty level asset (default size, no blocks) at a project path like "Assets/.../Level_02.asset".
        public static LevelData CreateNew(string path)
        {
            ValidatePath(path);

            LevelData level = ScriptableObject.CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(level, path);
            AssetDatabase.SaveAssets();

            return level;
        }

        // Saves the level first (CopyAsset copies the file on disk, so unsaved edits would be lost),
        // then copies it next to itself with a unique name and returns the copy.
        public static LevelData Duplicate(LevelData level)
        {
            ValidateLevel(level);
            Save(level);

            string path = AssetDatabase.GetAssetPath(level);
            string copyPath = AssetDatabase.GenerateUniqueAssetPath(path);

            if (!AssetDatabase.CopyAsset(path, copyPath))
                throw new InvalidOperationException($"Could not copy '{path}' to '{copyPath}'");

            return AssetDatabase.LoadAssetAtPath<LevelData>(copyPath);
        }

        public static void Save(LevelData level)
        {
            ValidateLevel(level);
            AssetDatabase.SaveAssetIfDirty(level);
        }

        public static bool HasUnsavedChanges(LevelData level)
        {
            ValidateLevel(level);

            return EditorUtility.IsDirty(level);
        }

        private static void ValidateLevel(LevelData level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));
        }

        private static void ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/") || !path.EndsWith(".asset"))
                throw new ArgumentException($"Path must look like 'Assets/.../Name.asset', but was '{path}'", nameof(path));
        }
    }
}

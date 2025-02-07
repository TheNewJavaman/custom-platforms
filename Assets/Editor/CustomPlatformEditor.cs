using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace CustomFloorPlugin {
    [CustomEditor(typeof(CustomPlatform))]
    public class CustomPlatformEditor : Editor {
        CustomPlatform customPlat;

        private void OnEnable() {
            customPlat = (CustomPlatform)target;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (GUILayout.Button("Export")) {
                string path = EditorUtility.SaveFilePanel("Save Platform file", "", customPlat.platName + ".zip", "zip");

                if (path != "") {
                    string fileName = Path.GetFileName(path);
                    string folderPath = Path.GetDirectoryName(path);
                    string tempPath = Path.GetTempPath();

                    PrefabUtility.CreatePrefab("Assets/_CustomPlatform.prefab", customPlat.gameObject);
                    AssetBundleBuild assetBundleBuild = default(AssetBundleBuild);
                    assetBundleBuild.assetNames = new string[] {
                    "Assets/_CustomPlatform.prefab"
                        };

                    assetBundleBuild.assetBundleName = fileName.Replace(".zip", ".plat");

                    BuildTargetGroup selectedBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                    BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

                    BuildPipeline.BuildAssetBundles(tempPath, new AssetBundleBuild[] { assetBundleBuild }, 0, EditorUserBuildSettings.activeBuildTarget);

                    EditorPrefs.SetString("currentBuildingAssetBundlePath", folderPath);

                    EditorUserBuildSettings.SwitchActiveBuildTarget(selectedBuildTargetGroup, activeBuildTarget);

                    AssetDatabase.DeleteAsset("Assets/_CustomPlatform.prefab");

                    if (File.Exists(path)) {
                        File.Delete(path);
                    }

                    if (Directory.Exists(Path.Combine(tempPath, "CustomPlatforms"))) {
                        DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(tempPath, "CustomPlatforms"));
                        foreach (FileInfo file in dirInfo.GetFiles()) {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in dirInfo.GetDirectories()) {
                            dir.Delete(true);
                        }
                    }

                    Directory.CreateDirectory(Path.Combine(tempPath, "CustomPlatforms"));
                    Directory.CreateDirectory(Path.Combine(tempPath, "CustomPlatforms", "CustomPlatforms"));
                    Directory.CreateDirectory(Path.Combine(tempPath, "CustomPlatforms", "CustomPlatforms", "Scripts"));
                    File.Move(tempPath + "/" + fileName.Replace(".zip", ".plat"), Path.Combine(tempPath, "CustomPlatforms", "CustomPlatforms" + "/" + fileName.Replace(".zip", ".plat")));
                    List<string> scriptPaths = new List<string>(Directory.GetFiles(Path.Combine(Application.dataPath, "_Scripts")));
                    scriptPaths.Remove(Path.Combine(Application.dataPath, "_Scripts", "CustomFloorPlugin.dll"));
                    foreach (string scriptPath in scriptPaths) {
                        if (scriptPath.EndsWith(".dll")) {
                            File.Copy(scriptPath, Path.Combine(tempPath, "CustomPlatforms", "CustomPlatforms", "Scripts") + "/" + Path.GetFileName(scriptPath));
                        }
                    }
                    ZipFile.CreateFromDirectory(Path.Combine(tempPath, "CustomPlatforms"), Path.Combine(folderPath, fileName));

                    AssetDatabase.Refresh();

                    EditorUtility.DisplayDialog("Exportation Successful!", "Exportation Successful!", "OK");
                }
                else {
                    EditorUtility.DisplayDialog("Exportation Failed!", "Path is invalid.", "OK");
                }

            }
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Cosmobot.Editor.Standalone
{
    public class BuildInstaller : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private string[] bundlesPath = {"Assets/Scripts/Editor/EditorStandalone/BuildHelper/Bundles" };

        public void OnPostprocessBuild(BuildReport report)
        {
            List<BuildBundle> buildBundles = AssetDatabase.FindAssets("t:BuildBundle", bundlesPath)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<BuildBundle>(path))
                .Where(bundle => bundle.includeInBuild)
                .ToList();

            string buildPath = report.summary.outputPath;

            foreach (BuildBundle bundle in buildBundles)
            {
                foreach (Object file in bundle.Files)
                {
                    string filePath = AssetDatabase.GetAssetPath(file);

                    if (File.Exists(filePath))
                    {
                        filePath = filePath.Substring("Assets/".Length);

                        string outputPath = Path.Combine(Path.GetDirectoryName(buildPath), Path.GetFileNameWithoutExtension(buildPath) + "_Data/", filePath);
                        string outputDirPath = Path.GetDirectoryName(outputPath);

                        if (!Directory.Exists(outputDirPath))
                        {
                            Directory.CreateDirectory(outputDirPath);
                        }

                        File.Copy(Path.Combine(Application.dataPath, filePath), outputPath, true);
                        Debug.Log($"Copied file to: {outputPath}");
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Cosmobot.Editor
{
    public class LibImporter
    {
        //https://www.nuget.org/api/v2/package/{libName}/{version}
        private const string NugetAddres = "https://www.nuget.org/api/v2/package";
        private const string LibFolder = "Plugins";
        private const string LibFile = "Assets/Plugins/Libs.json";
        private const string NugetPackageLibDir = "lib";

        private const string ProgressBarName = "Lib Importer";

        // will take first found
        private static readonly string[] nugetPackageLibStandardsDirs = { "netstandard2.1", "netstandard2.0" };

        [MenuItem("Cosmo/Import Libs")]
        public static void OpenLibImporter()
        {
            try
            {
                ImportLibs(LibFile);
                EditorUtility.ClearProgressBar();
            }
            catch
            {
                EditorUtility.ClearProgressBar();
                throw;
            }
        }

        public static void ImportLibs(string assetPath)
        {
            EditorUtility.DisplayProgressBar(ProgressBarName, "Reading lib file", 0f);
            LibToImportAsset libToImportAsset;
            try
            {
                libToImportAsset = JsonUtility.FromJson<LibToImportAsset>(File.ReadAllText(assetPath));
                if (libToImportAsset == null)
                {
                    Debug.LogError($"LibImporter: Asset is not a valid LibToImportAsset, skipping import. {assetPath}");
                    EditorUtility.ClearProgressBar();
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"LibImporter: Error reading {assetPath}: {e.Message}");
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
                return;
            }

            List<LibToImport> libs = libToImportAsset.nuget ?? new List<LibToImport>();
            int progress = 1;
            Debug.Log("Detected libs: " + string.Join(", ", libs));
            foreach (LibToImport libToImport in libs)
            {
                EditorUtility.DisplayProgressBar(ProgressBarName,
                    $"Importing libs {libToImport.name} {progress}/{libs.Count}", progress / (libs.Count + 1f));
                try
                {
                    ProcessLib(libToImport);
                }
                catch (Exception e)
                {
                    Debug.LogError($"LibImporter: Error processing {libToImport.name}: {e.Message}");
                    Debug.LogException(e);
                }

                progress++;
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private static void ProcessLib(LibToImport libToImport)
        {
            string dllPath = $"{Application.dataPath}/{LibFolder}/{libToImport.name}.dll";
            if (!IsDownloadRequired(dllPath, libToImport.version))
            {
                Debug.Log($"{libToImport.name} already in correct version");
                return;
            }

            Uri downloadUri = CreateNugetDownloadUri(libToImport.name, libToImport.version);
            string downloadTargetPath = Path.GetTempFileName();
            using WebClient client = new WebClient();
            client.DownloadFile(downloadUri, downloadTargetPath);

            ZipArchive archive = ZipFile.Open(downloadTargetPath, ZipArchiveMode.Read);
            ZipArchiveEntry libEntry = null;
            foreach (string nugetLibStandard in nugetPackageLibStandardsDirs)
            {
                if (libEntry is not null) break; // take first
                libEntry = archive.GetEntry($"{NugetPackageLibDir}/{nugetLibStandard}/{libToImport.name}.dll");
            }

            if (libEntry is null)
            {
                string standards = string.Join(',', nugetPackageLibStandardsDirs);
                Debug.LogWarning($"LibImporter: Lib {libToImport.name} zip does not contain {standards} library.");
                return;
            }

            libEntry.ExtractToFile(dllPath, true);
            Debug.Log("LibImporter: Downloaded and extracted " + libToImport.name + " to " + dllPath);
        }


        private static bool IsDownloadRequired(string libPath, string version)
        {
            if (!File.Exists(libPath))
            {
                return true;
            }

            FileVersionInfo info = FileVersionInfo.GetVersionInfo(libPath);
            return $"{info.ProductMajorPart}.{info.ProductMinorPart}.{info.ProductBuildPart}" != version;
        }

        private static Uri CreateNugetDownloadUri(string name, string version)
        {
            return new Uri($"{NugetAddres}/{name}/{version}");
        }
    }

    [Serializable]
    public class LibToImportAsset
    {
        [SerializeField]
        public List<LibToImport> nuget;
    }

    [Serializable]
    public class LibToImport
    {
        [SerializeField]
        public string name;

        [SerializeField]
        public string version;

        public override string ToString()
        {
            return $"{name} {version}";
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace Cosmobot
{
    public class EngineLogicTemplateGenerator
    {
        const string path = "Assets/Templates/EngineLogicTemplate.cs.txt";
        [MenuItem("Assets/Create/Cosmobot/EngineLogic Script")]
        public static void CreateEngineLogicTemplate()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "DefaultEngineLogic.cs");
        }
    }
}

using UnityEditor;

namespace Cosmobot.Editor
{
    public class EngineLogicTemplateGenerator
    {
        const string Path = "Assets/Templates/EngineLogicTemplate.cs.txt";
        [MenuItem("Assets/Create/Cosmobot/EngineLogic Script")]
        public static void CreateEngineLogicTemplate()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path, "DefaultEngineLogic.cs");
        }
    }
}

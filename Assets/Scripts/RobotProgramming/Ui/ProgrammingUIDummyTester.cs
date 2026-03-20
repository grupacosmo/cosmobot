using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cosmobot
{
    public class ProgrammingUIDummyTester : MonoBehaviour
    {

        [SerializeField]
        private ProgrammingFileManager fileManager;
        [SerializeField]
        private ProgrammingUiLogManager logManager;
        [SerializeField]
        private ProgrammingUiManager uiManager;

        private readonly string[] files = new [] { "main.js", "robot.js", "mySuperSystem.js", "api.js", "<internal>" };
        private readonly string[] functionNames = new[] { "foo", "main", "findIron", "findItem", "travelToMine", "goAround" };
        private readonly string[] dummyErrors = new[] {
            "Uncaught ReferenceError: myVar is not defined",
            "TypeError: Cannot read properties of undefined (reading 'length')",
            "RangeError: Maximum call stack size exceeded",
            "URIError: Malformed URI sequence"
        };

        private readonly string[] dummyWarnings = new[] {
            "Warning: Deprecated API usage: 'escape()' is deprecated",
            "Warning: 'var' declarations are function-scoped, consider using 'let' or 'const'",
            "Warning: Possible loss of precision in number conversion",
            "Warning: Unexpected console statement",
            "Warning: Assignment to constant variable"
        };

        private readonly string[] dummyTraces = new[] {
            "entering function initApp()",
            "found 'Battery' item at (10, 1, 15)",
            "Construction completed",
            "Digging 'copper ore'...",
        };


        private float currentDelay = 0;
        private void Update()
        {
            currentDelay -= Time.deltaTime;
            if (currentDelay < 0)
            {
                currentDelay = Random.Range(1, 3);
                GenerateDummyLog();
            }
        }

        public void CreateDummyFile()
        {
            fileManager.CreateNewFile(files[Random.Range(0, files.Length - 1)]);
        }

        public void ConfirmRemoveOpenFile()
        {
            fileManager.ConfirmRemoveOpenFile();
        }

        public void RemoveOpenFile()
        {
            fileManager.RemoveOpenFile();
        }

        public void SaveFile()
        {
            fileManager.SaveFile();
        }

        public void CloseUI()
        {
            uiManager.Close();
        }

        public void RunActiveFile()
        {
            fileManager.RunActiveFile();
        }

        public void GenerateDummyLog()
        {
            LogLevel level = RandomLevel();
            string message = GetBaseMessageFor(level);
            if (level == LogLevel.Error)
            {
                message += "\n" + GenerateStackTrace();
            }
            logManager.CreateLog(level, message);
        }

        private string GenerateStackTrace()
        {
            int stackSize = Random.Range(2, 6);
            StringBuilder stackTraceSb = new StringBuilder();
            for (int i = 0; i < stackSize; i++)
            {
                stackTraceSb.Append(RandomElem(functionNames) + " @ " + RandomElem(files) + ":" + Random.Range(1, 1000));
                stackTraceSb.Append('\n');
            }

            return stackTraceSb.ToString();
        }

        private string GetBaseMessageFor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info: return RandomElem(dummyTraces);
                case LogLevel.Warn: return RandomElem(dummyWarnings);
                case LogLevel.Error: return RandomElem(dummyErrors);
                default: return "Unknown level";
            }
        }

        private LogLevel RandomLevel()
        {
            return RandomElem(new[] { LogLevel.Info, LogLevel.Warn, LogLevel.Error });
        }

        private T RandomElem<T>(T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cosmobot
{
    // Only Ui functionality
    public class ProgrammingUiLogManager : MonoBehaviour
    {
        [SerializeField]
        private int maxLogEntries = 50;

        [SerializeField]
        private ScrollRect consoleScrollView;

        [Header("Canvas UI Prefabs")]
        [SerializeField]
        private GameObject uiLogEntryPrefab;

        private readonly Queue<ProgrammingUiLogEntry> logs = new();

        public void CreateLog(LogLevel level, string message)
        {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();
            CreateLog(now, level, message);
        }

        public void CreateLog(long time, LogLevel level, string message)
        {
            bool scrollToBottom = consoleScrollView.verticalNormalizedPosition <= 0.01f;

            LogEntry entry = new LogEntry(time, level, message);
            GameObject uiLogEntry = Instantiate(uiLogEntryPrefab, transform);
            ProgrammingUiLogEntry uiLog = uiLogEntry.GetComponent<ProgrammingUiLogEntry>();
            uiLog.SetLog(entry);

            if (scrollToBottom)
                StartCoroutine(ScrollToBottomCoroutine());


            Enqueue(uiLog);
        }

        private void Enqueue(ProgrammingUiLogEntry entry)
        {
            // TODO: use object pooling
            logs.Enqueue(entry);
            if (logs.Count > maxLogEntries)
                Destroy(logs.Dequeue().gameObject);
        }

        private IEnumerator ScrollToBottomCoroutine()
        {
            consoleScrollView.velocity = new Vector2(0, 1000);
            yield return new WaitForSeconds(0.1f);
            for (int i = 0; i < 10; i++)
            {
                consoleScrollView.velocity *= 0.5f;
                yield return new WaitForSeconds(0.05f);
                if (consoleScrollView.verticalNormalizedPosition <= 0.01f)
                {
                    consoleScrollView.velocity = Vector2.zero;
                    consoleScrollView.verticalNormalizedPosition = 0;
                    break;
                }
            }
        }
    }
}

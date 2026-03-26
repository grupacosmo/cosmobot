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

        [SerializeField]
        private GameObject logInstantiationTarget;

        [Header("Canvas UI Prefabs")]
        [SerializeField]
        private GameObject uiLogEntryPrefab;

        private readonly Queue<ProgrammingUiLogEntry> logs = new();

        public void CreateLog(ProgrammableData data, LogEntry logEntry)
        {
            bool scrollToBottom = consoleScrollView.verticalNormalizedPosition <= 0.01f;

            LogEntry entry = new LogEntry(logEntry.timestamp, logEntry.level, logEntry.message);
            GameObject uiLogEntry = Instantiate(uiLogEntryPrefab, logInstantiationTarget.transform);
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

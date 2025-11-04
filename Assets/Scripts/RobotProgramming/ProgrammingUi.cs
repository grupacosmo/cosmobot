using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cosmobot
{
    public class ProgrammingUi : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private TMP_Text codeDisplay;
        
        [SerializeField]
        private TMP_Text lineNumbersText;
        
        private string bufferedText = "";
        private bool dirty = false;
        private int bufferedLineCount = 1;
        private int visibleLineCount;
        private float previousLineNumberContainerHeight;

        private RectTransform lineNumberTextParent;
        
        private void OnEnable()
        {
            if (inputField != null && codeDisplay != null)
            {
                inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
                codeDisplay.text = inputField.text;
            }
        }

        private void OnDisable()
        {
            if (inputField != null)
            {
                inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            }
        }
        
        private void Start()
        {
            lineNumberTextParent = (RectTransform)lineNumbersText.transform.parent;
            bufferedLineCount = 1; 
            RecalculateVisibleLineCount();
            UpdateLineNumbers();
            
        }

        private void Update()
        {
            float currentLineNumberContainerHeight = lineNumberTextParent.rect.size.y;
            if (!Mathf.Approximately(currentLineNumberContainerHeight, previousLineNumberContainerHeight))
            {
                RecalculateVisibleLineCount();
                UpdateLineNumbers();
                previousLineNumberContainerHeight = currentLineNumberContainerHeight;
            }
            
            if (dirty)
            {
                UpdateOutputField();
                dirty = false;
            }
        }

        private void RecalculateVisibleLineCount()
        {
            TMP_LineInfo[] lineInfo = lineNumbersText.textInfo.lineInfo;
            if (lineInfo.Length == 0)
            {
                Debug.LogError("no line in line numbers text. Is it even possible?");
            }

            float fullLineHeight = lineInfo[0].lineHeight + lineNumbersText.lineSpacing;
            float containerHeight = lineNumberTextParent.rect.size.y;
            if (fullLineHeight < Mathf.Epsilon)
            {
                Debug.LogWarning("fullLineHeight is too small");
                fullLineHeight = containerHeight / 10;
            }
            
            int lineCount = (int)Mathf.Ceil(containerHeight / fullLineHeight);
            visibleLineCount = lineCount + 1; // just to be sure
        }
        
        private void UpdateOutputField()
        {
            int lineCount = CountLines();
            if (lineCount != bufferedLineCount)
            {
                bufferedLineCount = lineCount;
                UpdateLineNumbers();
            }

            UpdateText();
            dirty = false;
        }

        private int CountLines()
        {
            int lineCount = 1;
            foreach (char c in bufferedText)
            {
                if (c == '\n') lineCount++;
            }
            return lineCount;
        }
        
        private void UpdateLineNumbers()
        {
            // well with more than 9999 lines unity will probably die processing TMPro 
            // simple and stupid approximation
            int stringSize = Mathf.Min(bufferedLineCount, visibleLineCount) + 1; // '\n'
            if (bufferedLineCount < 1000)
                stringSize += bufferedLineCount * 3; // 001-999 
            else 
                stringSize += 3000 + (bufferedLineCount - 1000) * 4; // (001-999) + (1000-9999)
            
            StringBuilder lineNumbers = new StringBuilder(stringSize);
            int i = 0;
            for (; i < bufferedLineCount; i++)
            {
                lineNumbers.Append(i+1).Append('\n');
            }

            // for (; i < visibleLineCount; i++) // fill to bottom
            // {
            //     lineNumbers.Append('\n');
            // }
            // lineNumbers.Append('\n');
            
            lineNumbersText.text = lineNumbers.ToString();
        }

        private void UpdateText()
        {
            codeDisplay.text = Format(bufferedText);
        }

        private void OnInputFieldValueChanged(string newValue)
        {
            bufferedText = newValue;
            dirty = true;
        }

        private string Format(string input)
        {
            // Debug.Log("Format: cp" + caretPosition + " sel: "+ selStart + " to " + selEnd + "; " + input);
            const char ZWS = '\u200B'; // zero-width space

            return input
                    .Replace("<", "<" + ZWS)
                    .Replace(">", ZWS + ">")
                    .Replace("public", "<color=#ff0000>public</color>")
                    .Replace("void",  "<color=#00ff00>void</color>")
                    .Replace("int",  "<color=#00ff00>int</color>")
                    .Replace("6",  "<color=#0000ff>6</color>")
                // + "<color=#00000000></mspace=0.01>.</mspace></color>"
                ;
        }
    }
}

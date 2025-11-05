using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                lineNumbers.Append(i + 1).Append('\n');
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

        private static readonly Regex parsingRegex = PrepareApiTypes();
        private static readonly Regex richTextFixerRegex = new Regex(@"(<|>)");
        private static readonly Dictionary<string, Color> syntaxColorStyle = new Dictionary<string, Color>
        {
            ["comment"]     = ColorHex(0x6A9955), // greenish
            ["string"]      = ColorHex(0xCE9178), // light red/orange
            ["number"]      = ColorHex(0xB5CEA8), // pale green
            ["keyword"]     = ColorHex(0x569CD6), // blue
            ["literal"]     = ColorHex(0xDCDCAA), // yellowish
            ["builtin"]     = ColorHex(0x4EC9B0), // teal
            ["function"]    = ColorHex(0xDCDCAA), // yellow
            ["operator"]    = ColorHex(0xD4D4D4), // light gray
            ["punctuation"] = ColorHex(0xD4D4D4), // same as operator
            ["apiTypes"]    = ColorHex(0x8DDDCD), // light teal
        };

        private static Color ColorHex(uint color)
        {
            float r = (color       & 0xFF) / 255f;
            float g = (color >> 8  & 0xFF) / 255f;
            float b = (color >> 16 & 0xFF) / 255f;
            return new Color(r, g, b);
        }
        
        private static string ColorToHex(Color color)
        {
            int r = (int)(color.r * 255);
            int g = (int)(color.g * 255);
            int b = (int)(color.b * 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        
        private static Regex PrepareApiTypes()
        {
            Type apiVec2Type = typeof(Cosmobot.Api.Types.Vec2);
            string apiNamespace = apiVec2Type.Namespace;
            string apiTypesNames = 
                apiVec2Type.Assembly.GetTypes()
                    .Where(t => t.Namespace == apiNamespace)
                    .Select(t => t.Name)
                    .Aggregate((total, next) =>  total + "|" + next);
            
            string pattern = string.Join("|", new[]
            {
                @"(?<comment>\/\/[^\n]*|\/\*[\s\S]*?\*\/)",
                @"(?<string>(['""`])(?:\\.|(?!\1).)*\1)",
                @"(?<number>\b(?:0[xX][\dA-Fa-f]+|\d+(?:\.\d+)?(?:[eE][+-]?\d+)?)|0[bB][\d0-1]+\b)",
                @"(?<keyword>\b(?:if|else|for|while|do|break|continue|return|switch|case|default|function|class|extends|super|import|export|new|try|catch|finally|throw|const|let|var|of|in|instanceof|typeof|delete|await|async|yield|with|this)\b)",
                @"(?<literal>\b(?:true|false|null|undefined|NaN|Infinity)\b)",
                @"(?<builtin>\b(?:Array|Object|String|Number|Boolean|Date|Math|JSON|Promise|Symbol|BigInt|Map|Set|WeakMap|WeakSet|Error|RegExp|console|window|document)\b)",
                @"(?<function>\b[a-zA-Z_]\w*(?=\s*\())",
                @"(?<operator>[+\-*/%=&|^!?<>]=?|={1,3}|:{1,2}|\?|\.\.\.|~|\*\*)",
                @"(?<punctuation>[{}()[\];,.])",
                @"(?<apiTypes>" + apiTypesNames + @")"
            });
            
            return new Regex(pattern, RegexOptions.Compiled);
        }

        private string Format(string input)
        {
            // Debug.Log("Format: cp" + caretPosition + " sel: "+ selStart + " to " + selEnd + "; " + input);
            const char ZWS = '\u200B'; // zero-width space
            input = richTextFixerRegex.Replace(input, match =>
            {
                if (match.Value == "<")
                    return "<" + ZWS;
                return ZWS + ">";
            });
            
            StringBuilder output = new StringBuilder(input.Length);
            int lastIndex = 0;
            foreach (Match match in parsingRegex.Matches(input))
            {
                if (match.Index > lastIndex)
                    output.Append(input.Substring(lastIndex, match.Index - lastIndex));

                foreach (Group group in match.Groups)
                {
                    // string regex -> two groups
                    if (group.Success && group.Name != "0" && group.Name != "1")
                    {
                        if (syntaxColorStyle.TryGetValue(group.Name, out Color value))
                        {
                            string color = ColorToHex(value);
                            output.Append($"<color={color}>{group.Value}</color>");
                        }
                    }
                }
                lastIndex = match.Index + match.Length;
            }
            
            if (lastIndex < input.Length)
                output.Append(input.Substring(lastIndex));
            
            return output.ToString();
        }
    }
}

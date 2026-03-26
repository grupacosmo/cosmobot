using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cosmobot.Utils;
using TMPro;
using UnityEngine;

namespace Cosmobot
{
    public class ProgrammingUi : MonoBehaviour
    {
        public string Code { get => bufferedText; set => HandleFileLoad(value); }
        public event Action OnCodeChanged;
        
        [SerializeField]
        private float fontSize = 36;

        [SerializeField]
        private TMP_InputField inputField;

        [SerializeField]
        private TMP_Text codeDisplay;

        [SerializeField]
        private TMP_Text lineNumbersText;

        [SerializeField]
        private TMP_Text fileStatusText;

        public SerializableDictionary<Programmable, ProgrammingUiFileEntry> robotActiveFiles = new();
        
        // syntax highlight
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

        // render
        private string bufferedText = "";
        private bool dirty = false;
        private int bufferedLineCount = 1;
        private int visibleLineCount;
        private float previousLineNumberContainerHeight;
        private int prevCaretPos = 0;
        private int prevSelectAnchorPos = 0;
        private int prevSelectFocusPos = 0;

        private RectTransform lineNumberTextParent;

        // == Unity's event functions

        private void OnEnable()
        {
            bool hasComponents = true;
            hasComponents &= ComponentUtils.RequireNotNull(inputField, "inputField", this);
            hasComponents &= ComponentUtils.RequireNotNull(lineNumbersText, "lineNumbersText", this);
            hasComponents &= ComponentUtils.RequireNotNull(codeDisplay, "codeDisplay", this);
            if (!hasComponents)
            {
                Debug.LogWarning($"{nameof(ProgrammingUi)} has unset references. Check logs for more info", this);
                enabled = false;
                return;
            }

            UpdateFontSize();

            inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            dirty = true;
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
            if (prevCaretPos != inputField.caretPosition
                || prevSelectAnchorPos != inputField.selectionAnchorPosition
                || prevSelectFocusPos != inputField.selectionFocusPosition)
            {
                UpdateFileStatus();
                prevCaretPos = inputField.caretPosition;
                prevSelectAnchorPos = inputField.selectionAnchorPosition;
                prevSelectFocusPos = inputField.selectionFocusPosition;
            }

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
                UpdateFileStatus();
                dirty = false;
            }
        }

        // ==

        private void HandleFileLoad(string fileContents)
        {
            fileContents ??= "";

            bufferedText = fileContents;
            inputField.SetTextWithoutNotify(fileContents);
            
            bufferedLineCount = CountLines();
            RecalculateVisibleLineCount();
            UpdateLineNumbers(); 
            
            dirty = true;
        }
        
        private void UpdateFileStatus()
        {
            int caretPos = inputField.caretPosition;
            TMP_TextInfo textInfo = inputField.textComponent.textInfo;
            TMP_CharacterInfo charAtCaret = textInfo.characterInfo[caretPos];
            int caretLine = charAtCaret.lineNumber + 1;
            int caretChar = charAtCaret.index - textInfo.lineInfo[charAtCaret.lineNumber].firstCharacterIndex + 1;
            int selectionAnchorPos = inputField.selectionAnchorPosition;
            int selectionFocusPos = inputField.selectionFocusPosition;
            int selectedChars = Mathf.Abs(selectionFocusPos - selectionAnchorPos);

            string status = $"{caretLine}:{caretChar}";
            if (selectedChars != 0)
            {
                int selectionAnchorLine = textInfo.characterInfo[selectionAnchorPos].lineNumber;
                int selectionFocusLine = textInfo.characterInfo[selectionFocusPos].lineNumber;
                int selectedLines =  Mathf.Abs(selectionFocusLine - selectionAnchorLine);

                status += $" ({selectedChars} chars";
                if (selectedLines > 0)
                    status += $", {selectedLines} line break" + (selectedLines == 1 ? "" : "s");
                status += ")";
            }

            status += $" | file: {textInfo.lineCount} ({textInfo.characterCount})";

            fileStatusText.text = status;
        }

        private void UpdateFontSize()
        {
            inputField.pointSize = fontSize;
            codeDisplay.fontSize = fontSize;
            lineNumbersText.fontSize = fontSize;
        }

        private void RecalculateVisibleLineCount()
        {
            TMP_LineInfo[] lineInfo = lineNumbersText.textInfo.lineInfo;
            if (lineInfo.Length == 0)
            {
                Debug.LogError("no line in line numbers text. Is it even possible?");
                visibleLineCount = 1;
                return;
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

            for (; i < visibleLineCount; i++) // fill to bottom
            {
                lineNumbers.Append('\n');
            }
            lineNumbers.Append('\n');

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
            
            OnCodeChanged?.Invoke();
        }

        private static Color ColorHex(uint color)
        {
            float r = (color >> 16 & 0xFF) / 255f;
            float g = (color >> 8  & 0xFF) / 255f;
            float b = (color       & 0xFF) / 255f;
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

            IEnumerable<string> apiTypeNamesEnumerable =
                apiVec2Type.Assembly.GetTypes()
                    .Where(t => t.Namespace == apiNamespace)
                    .Select(t => t.Name);
            string apiTypesNamesRegex =  string.Join("|", apiTypeNamesEnumerable);

            string pattern = string.Join("|", new[]
            {
                @"(?<comment>\/\/[^\n]*|\/\*[\s\S]*?\*\/)",
                @"(?<string>(['""`])(?:\\.|(?!\1).)*\1)",
                @"(?<number>\b(?:0[xX][\dA-Fa-f]+|\d+(?:\.\d+)?(?:[eE][+-]?\d+)?)|0[bB][0-1]+\b)",
                @"(?<keyword>\b(?:if|else|for|while|do|break|continue|return|switch|case|default|function|class|extends|super|import|export|new|try|catch|finally|throw|const|let|var|of|in|instanceof|typeof|delete|await|async|yield|with|this)\b)",
                @"(?<literal>\b(?:true|false|null|undefined|NaN|Infinity)\b)",
                @"(?<builtin>\b(?:Array|Object|String|Number|Boolean|Date|Math|JSON|Promise|Symbol|BigInt|Map|Set|WeakMap|WeakSet|Error|RegExp|console|window|document)\b)",
                @"(?<function>\b[a-zA-Z_]\w*(?=\s*\())",
                @"(?<operator>[+\-*/%=&|^!?<>]=?|={1,3}|:{1,2}|\?|\.\.\.|~|\*\*)",
                @"(?<punctuation>[{}()[\];,.])",
                @"(?<apiTypes>" + apiTypesNamesRegex + @")"
            });

            return new Regex(pattern, RegexOptions.Compiled);
        }

        private string Format(string input)
        {
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
                    output.Append(input, lastIndex, match.Index - lastIndex);

                foreach (Group group in match.Groups)
                {
                    // string regex -> two groups
                    bool validGroup = group.Name != "0" && group.Name != "1";
                    bool syntaxColorExists = syntaxColorStyle.TryGetValue(group.Name, out Color value);
                    if (group.Success && validGroup && syntaxColorExists)
                    {
                        string color = ColorToHex(value);
                        output.Append($"<color={color}>{group.Value}</color>");
                    }
                }
                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < input.Length)
                output.Append(input, lastIndex, input.Length - lastIndex);

            return output.ToString();
        }

#if UNITY_EDITOR
        // == editor only 
        private float previousFontSize = 36;
        private void OnValidate()
        {
            if (previousFontSize != fontSize) // safe float comparison
            {
                UpdateFontSize();
                previousFontSize = fontSize;
            }
        }
#endif
    }
}

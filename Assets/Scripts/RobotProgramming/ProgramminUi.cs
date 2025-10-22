using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cosmobot
{
    public class ProgramminUi : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private TMP_Text codeDisplay;
        
        [SerializeField]
        private float blinkInterval = 0.55f;
        [SerializeField]
        private float blinkDelayAfterEdit = 0.8f;
        
        private int lastCaretPosition = 0;
        
        private string bufferedText;
        private bool dirty = false;
        private bool cursorBlink = false;
        private float lastBlink = 0;
        private bool wasBlinkUpdate = false;
        
        
        private void OnEnable()
        {
            if (inputField != null && codeDisplay != null)
            {
                inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
                inputField.onTextSelection.AddListener(OnTextSelection);
                codeDisplay.text = inputField.text;
            }
        }

        private void OnDisable()
        {
            if (inputField != null)
            {
                inputField.onTextSelection.RemoveListener(OnTextSelection);
                inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            }
        }
        

        private void Update()
        {
            if (inputField.caretPosition != lastCaretPosition)
            {
                lastCaretPosition = inputField.caretPosition;
                dirty = true;
            }
            lastBlink -= Time.deltaTime;
            if (lastBlink <= 0)
            {
                cursorBlink = !cursorBlink;
                dirty = true;
                wasBlinkUpdate = true;
            }
            
            if (dirty)
            {
                if (!wasBlinkUpdate)
                {
                    cursorBlink = true;
                    lastBlink = blinkDelayAfterEdit;
                }
                else
                {
                    lastBlink = blinkInterval;
                    wasBlinkUpdate = false;
                }
                
                UpdateText();
                dirty = false;
            }
        }

        private void UpdateText()
        {
            int selectStart = inputField.selectionStringAnchorPosition;
            int selectEnd = inputField.selectionStringFocusPosition;
            int carretPosition = inputField.caretPosition;
            int selStart = Math.Min(selectStart, selectEnd);
            int selEnd = Math.Max(selectStart, selectEnd);
            codeDisplay.text = Format(bufferedText, carretPosition, selStart, selEnd);
        }

        private void OnInputFieldValueChanged(string newValue)
        {
            bufferedText = newValue;
            dirty = true;
        }
        
        private void OnTextSelection(string text, int selectionStart, int selectionEnd)
        {
            bufferedText = text;
            dirty = true;
        }
        

        private string Format(string input, int caretPosition, int selStart, int selEnd)
        {
            Debug.Log("Format: cp" + caretPosition + " sel: "+ selStart + " to " + selEnd + "; " + input);
            int startOffset = 0;
            int endOffset = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != '<' && input[i] != '>') continue;
                if (i < selStart + startOffset) startOffset++;
                if (i < selEnd + endOffset) endOffset++;
            }
            
            const char ZWS = '\u200B'; // zero-width space
            const string SelectStartMarkTag = "<mark=#4444ff55>";
            const string SelectEndMarkTag = "</mark>";


            int offsetSelStart = selStart + startOffset;
            int offsetSelEnd = selEnd + endOffset + SelectStartMarkTag.Length;

            int offsetCaretPos =
                caretPosition
                + (caretPosition == selStart ? startOffset : endOffset)
                + SelectStartMarkTag.Length
                + (selStart == selEnd ? SelectEndMarkTag.Length : 0);


            const char ZWJ = '\u200D';
            const string monospaceStartTag = "<mspace=0.7em>";
            // const string cursorTag = "</mspace><rotate=15>\u0338</rotate>" + monospaceStartTag;

            string cursorBlinked = cursorBlink ? "|" : "";
            string cursorTag = $"<mspace=-0.01>{cursorBlinked}</mspace>" + monospaceStartTag;
            
            return monospaceStartTag + input
                    .Replace("<", "<" + ZWS)
                    .Replace(">", ZWS + ">")
                    .Insert(offsetSelStart, SelectStartMarkTag)
                    .Insert(offsetSelEnd, SelectEndMarkTag)
                    .Insert(offsetCaretPos, cursorTag) 
                // .Replace("<", "<" + zws)
                // .Replace(">", zws + ">" )
                    .Replace("public", "<color=#ff0000>public</color>")
                    .Replace("void",  "<color=#00ff00>void</color>")
                    .Replace("int",  "<color=#00ff00>int</color>")
                    .Replace("6",  "<color=#0000ff>6</color>")
                                     + "</mspace>";
        }
    }
}

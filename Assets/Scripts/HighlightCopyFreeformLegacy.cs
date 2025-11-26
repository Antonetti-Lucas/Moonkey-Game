using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChristinaCreatesGames.UI
{
    public class HighlightCopyFreeformLegacy : MonoBehaviour, IPointerDownHandler, IPointerMoveHandler, IPointerUpHandler
    {
        [Header("Highlight Settings")]
        [SerializeField] private Color selectionColor = Color.red;
        private string _hexValueOfColor;

        private Canvas _canvas;
        private Camera _cameraToUse;

        private string StartTag => $"<color=#" + _hexValueOfColor + ">";
        private string _endTag = "</color>";

        private Text _textField;
        private string _originalText;
        private string _originalTextWithoutTags;
        private string _selectedText;
        private int _startPos;
        private bool _mouseDown;

        private void Awake()
        {
            _textField = GetComponent<Text>();
            _canvas = GetComponentInParent<Canvas>();

            if (_canvas == null)
            {
                Debug.LogError("Script must be inside a Canvas!");
                enabled = false;
                return;
            }

            _originalText = _textField.text;
            _originalTextWithoutTags = Regex.Replace(_originalText, @"<.*?>", string.Empty);

            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                _cameraToUse = null;
            else
                _cameraToUse = _canvas.worldCamera;

            _hexValueOfColor = ColorUtility.ToHtmlStringRGBA(selectionColor);
        }

        public void SetNewText(string newText)
        {
            _textField.text = newText;
            _originalText = newText;
            _originalTextWithoutTags = Regex.Replace(newText, @"<.*?>", string.Empty);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _textField.text = _originalTextWithoutTags;

            _startPos = GetCharacterIndexFromPosition(eventData.position);

            _mouseDown = true;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!_mouseDown)
                return;

            int currentPos = GetCharacterIndexFromPosition(eventData.position);

            if (currentPos == -1 || currentPos <= _startPos)
                return;

            int length = currentPos - _startPos + 1;

            if (_startPos + length > _originalTextWithoutTags.Length)
                length = _originalTextWithoutTags.Length - _startPos;

            _selectedText = _originalTextWithoutTags.Substring(_startPos, length);

            var markedText = MarkSelection(length);

            _textField.text = markedText;
        }

        private string MarkSelection(int length)
        {
            var markedText = new StringBuilder(_originalTextWithoutTags);
            markedText.Insert(_startPos, StartTag);

            if (_startPos + length + StartTag.Length <= markedText.Length)
                markedText.Insert(_startPos + length + StartTag.Length, _endTag);
            else
            {
                markedText.Insert(markedText.Length, _endTag);
            }

            return markedText.ToString();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_mouseDown == false)
                return;

            _mouseDown = false;
            CopyToClipboard();
        }

        public void CopyToClipboard()
        {
            if (!string.IsNullOrEmpty(_selectedText))
            {
                _selectedText = _selectedText.Trim();
                GUIUtility.systemCopyBuffer = _selectedText;
                Debug.Log("Copied to clipboard: " + _selectedText);
            }

            _textField.text = _originalText;
        }

        // This calculates which character is closest to the mouse.
        private int GetCharacterIndexFromPosition(Vector2 screenPosition)
        {
            TextGenerator gen = _textField.cachedTextGenerator;
            RectTransform rect = _textField.rectTransform;

            // Convert screen mouse position to local position inside the text box
            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPosition, _cameraToUse, out localMousePos);

            int closestIndex = -1;
            float minDistance = float.MaxValue;

            // Loop through every character Unity has generated
            for (int i = 0; i < gen.characterCount; i++)
            {
                UICharInfo charInfo = gen.characters[i];

                Vector2 charPos = charInfo.cursorPos;

                charPos.y /= _canvas.scaleFactor;

                float dist = Vector2.Distance(localMousePos, charPos);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
    }
}
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace VitrivrVR.Input.Text
{
  /// <summary>
  /// Static class to allow routing of non-system text input to the currently selected registered text input field.
  /// This applies mostly to custom implemented XR based text input.
  /// </summary>
  public static class TextInputManager
  {
    public static Dictionary<int, KeyValuePair<string,int>> textfieldLastInput = new Dictionary<int, KeyValuePair<string, int>>();
    /// <summary>
    /// Inputs the given text into the currently selected text input field.
    /// </summary>
    /// <param name="text"></param>
    public static void InputText(string text)
    {
      var inputField = GetSelectedInputField();

      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      foreach (var keyEvent in text.Select(character => new Event {character = character}))
      {
        inputField.ProcessEvent(keyEvent);
      }

      inputField.ForceLabelUpdate();
    }

    /// <summary>
    /// Inputs the given input event into the currently selected text input field.
    /// </summary>
    /// <param name="inputEvent">Input event as event</param>
    public static void InputEvent(Event inputEvent)
    {
      var inputField = GetSelectedInputField();

      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      inputField.ProcessEvent(inputEvent);
      inputField.ForceLabelUpdate();
      textfieldLastInput[inputField.GetInstanceID()] = new KeyValuePair<string, int>("", 1);
    }

    /// <summary>
    /// Inputs the given keyboard event into the currently selected text input field.
    /// </summary>
    /// <param name="eventString">Keyboard event string</param>
    public static void InputKeyboardEvent(string eventString)
    {
      InputEvent(Event.KeyboardEvent(eventString));
    }

    /// <summary>
    /// Inputs a backspace into the currently selected text input field.
    /// </summary>
    public static void InputBackspace()
    {
      InputKeyboardEvent("backspace");
    }

    /// <summary>
    /// Inputs a return into the currently selected text input field.
    /// </summary>
    public static void InputReturn()
    {
      InputKeyboardEvent('\n'.ToString());
    }

    /// <summary>
    /// Inputs a left arrow navigation event into the currently selected text input field.
    /// </summary>
    public static void InputLeftArrow()
    {
      InputKeyboardEvent("LeftArrow");
    }

    /// <summary>
    /// Inputs a right arrow navigation event into the currently selected text input field.
    /// </summary>
    public static void InputRightArrow()
    {
      InputKeyboardEvent("RightArrow");
    }

    /// <summary>
    /// Inputs a tab character into the currently selected text input field.
    /// </summary>
    public static void InputTabulator()
    {
      InputKeyboardEvent('\t'.ToString());
    }

    /// <summary>
    /// Inputs the given text into the currently selected text input field and additionally inputs spaces.
    /// </summary>
    /// /// <param name="text">Text to input</param>
    public static void InputWord(string text)
    {
      var inputField = GetSelectedInputField();

      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      if (!textfieldLastInput.ContainsKey(inputField.GetInstanceID()))
      {
        textfieldLastInput[inputField.GetInstanceID()] = new KeyValuePair<string, int>("", 1);    
      }

      int noWhitespaceLength = textfieldLastInput[inputField.GetInstanceID()].Key.Trim().Length;

      if (!(text.Trim().Length == 0) && ((text.Trim().Length > 1 && noWhitespaceLength > 0) || (text.Trim().Length == 1 && noWhitespaceLength > 1)) && inputField.caretPosition == inputField.text.Length && inputField.text.Length > 0) 
      {
        text = " " + text;
      }

      foreach (var keyEvent in text.Select(character => new Event {character = character}))
      {
        inputField.ProcessEvent(keyEvent);
      }

      inputField.ForceLabelUpdate();

      textfieldLastInput[inputField.GetInstanceID()] = new KeyValuePair<string, int>(text, text.Length);
    }

    /// <summary>
    /// Deletes the last inputted word.
    /// </summary>
    public static void DeleteWord()
    {
      var inputField = GetSelectedInputField();

      if (inputField == null || !textfieldLastInput.ContainsKey(inputField.GetInstanceID()))
      {
        return; // No text field selected, nothing to do
      }

      int neededBackspaces = textfieldLastInput[inputField.GetInstanceID()].Key.Trim().Length;
      if (neededBackspaces == 0 || inputField.caretPosition != inputField.text.Length) 
      {
        neededBackspaces = 1;
      }
      int textLength = inputField.text.Length;
      for (int i = 0; i < neededBackspaces; i++)
      {
        inputField.ProcessEvent(Event.KeyboardEvent("backspace"));
        if (textLength - neededBackspaces >= inputField.text.Length)
        { 
          break;
        }
      }
      inputField.ForceLabelUpdate();
      textfieldLastInput[inputField.GetInstanceID()] = new KeyValuePair<string, int>("", 1);
      Debug.Log("NEEDEDBACKSPACES: " + neededBackspaces);
    }

    /// <summary>
    /// Retrieves the currently selected text input field.
    /// Returns null in case no text input field is currently selected.
    /// </summary>
    /// <returns>The currently selected text input field or null</returns>
    private static TMP_InputField GetSelectedInputField()
    {
      var selectedObject = EventSystem.current.currentSelectedGameObject;

      return selectedObject != null && selectedObject.TryGetComponent<TMP_InputField>(out var inputField)
        ? inputField
        : null;
    }
  }
}
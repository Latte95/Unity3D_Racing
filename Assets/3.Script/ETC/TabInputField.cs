using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TabInputField : MonoBehaviour
{
    private List<InputField> inputFields = new List<InputField>();
    public Button login;

    private void Awake()
    {
        InputField[] Fields = transform.GetComponentsInChildren<InputField>();
        foreach (InputField f in Fields)
        {
            inputFields.Add(f);
        }
    }

    private void Update()
    {
        for (int i = 0; i < inputFields.Count; i++)
        {
            if (inputFields[i].isFocused && Keyboard.current[Key.Tab].wasPressedThisFrame)
            {
                if (i == inputFields.Count - 1)
                {
                    inputFields[0].Select();
                }
                else
                {
                    inputFields[i + 1].Select();
                }
                break;
            }
        }
    }
}

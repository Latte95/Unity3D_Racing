using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TabInputField : MonoBehaviour
{
    private const int InputFieldSize = 2;

    private InputField[] inputFields = new InputField[InputFieldSize];
    public Button login;

    private void Awake()
    {
        InputField[] Fields = transform.GetComponentsInChildren<InputField>();
        for (int i = 0; i < Fields.Length; i++)
        {
            inputFields[i] = Fields[i];
        }
    }

    private void Update()
    {
        int inputFieldLength = inputFields.Length;

        for (int i = 0; i < inputFieldLength; i++)
        {
            // ��Ű �Է½� ���� �Է����� �ʵ� �������� �Ѿ
            if (inputFields[i].isFocused && Keyboard.current[Key.Tab].wasPressedThisFrame)
            {
                inputFields[(i + 1) % inputFieldLength].Select();
            }
        }
    }
}

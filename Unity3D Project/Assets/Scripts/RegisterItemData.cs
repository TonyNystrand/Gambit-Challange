using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RegisterItemData
{
    public enum RegisterFormats
    {
        REAL4 = 0,
        LONG = 1,
        BCD = 2,
        INTEGER = 3,
        ERROR = 4,
        LANGUAGE = 5,
        SIGNAL = 6,
        DATE = 7
    }

    public string title = "";
    public int registerStart = 0;
    public int registerNumber = 2;

    public RegisterFormats registerFormat = RegisterFormats.REAL4;
    public string unit = "";


}

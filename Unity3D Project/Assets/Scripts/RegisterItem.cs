using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterItem : MonoBehaviour 
{
    public Text titleLabel;
    public Text valueLabel;

    public string Title
    {
        get 
        {
            if (titleLabel)
                return titleLabel.text;
            else
                return "";
        }
        set
        {
            if (titleLabel)
                titleLabel.text = value;
        }
    }

    public string Value
    {
        get
        {
            if (valueLabel)
                return valueLabel.text;
            else
                return "";
        }
        set
        {
            if (valueLabel)
                valueLabel.text = value;
        }
    }
}

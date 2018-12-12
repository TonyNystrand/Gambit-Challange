using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ModbusParser : MonoBehaviour 
{
    public GameObject registerItemPrefab;
    public List<RegisterItemData> items = new List<RegisterItemData>();


    private string mDataLocation = "http://tuftuf.gambitlabs.fi/feed.txt";
    private Dictionary<int, ushort> mRegister = new Dictionary<int, ushort>();
    private string mTimestamp = "";

 

    // Use this for initialization
    void Start ()
    {
        UpdateData();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void UpdateData()
    {
        //Stop coroutine in case it is active
        StopCoroutine("ReadData");
        //Start new
        StartCoroutine("ReadData");
    }


    public IEnumerator ReadData()
    {
        //Empty cached register
        mRegister.Clear();

        //Read data
        UnityWebRequest myRequest = UnityWebRequest.Get(mDataLocation+"?noCache="+ Random.Range(1000,5000).ToString());
        myRequest.timeout = 1;
        yield return myRequest.SendWebRequest();

        if (myRequest.isNetworkError || myRequest.isHttpError)
        {

        }
        else
        {
            //Parse lines in data file
            string[] lines = myRequest.downloadHandler.text.Split('\n');

            //Store timestamp
            mTimestamp = lines[0];

            //Add dictionary items
            for (int i = 1; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                    mRegister.Add(int.Parse(lines[i].Split(':')[0]), ushort.Parse(lines[i].Split(':')[1]));
            }
            //Update labels
            UpdateUI();

        }
    }

    private void UpdateUI()
    {
        //Destroy old labels
        while (this.transform.childCount > 0)
            DestroyImmediate(this.transform.GetChild(0).gameObject);

        //Exit if no prefab for the label is assigned
        if (registerItemPrefab == null)
            return;

        //Create timestamp label
        RegisterItem _regItem = Instantiate(registerItemPrefab, this.transform).GetComponent<RegisterItem>();
        if (_regItem)
        {
            _regItem.Title = "Timestamp";
            _regItem.Value = mTimestamp;
        }

        //Create labels for each register
        foreach (RegisterItemData item in items)
        {
            _regItem = Instantiate(registerItemPrefab, this.transform).GetComponent<RegisterItem>();
            if(_regItem)
            {
                _regItem.Title = item.title;
                switch(item.registerFormat)
                {
                    case RegisterItemData.RegisterFormats.REAL4:
                        _regItem.Value = GetReal4Value(item.registerStart, item.registerNumber).ToString("0.00000");
                        if (!string.IsNullOrEmpty(item.unit))
                            _regItem.Value += " " + item.unit;
                        break;
                    case RegisterItemData.RegisterFormats.LONG:
                        _regItem.Value = GetLongValue(item.registerStart, item.registerNumber).ToString();
                        if (!string.IsNullOrEmpty(item.unit))
                            _regItem.Value += " " + item.unit;
                        break;
                    case RegisterItemData.RegisterFormats.INTEGER:
                        _regItem.Value = GetLongValue(item.registerStart, item.registerNumber).ToString();
                        if (!string.IsNullOrEmpty(item.unit))
                            _regItem.Value += " " + item.unit;
                        break;
                    case RegisterItemData.RegisterFormats.BCD:
                        _regItem.Value = GetBcdValue(item.registerStart, item.registerNumber);
                        if (!string.IsNullOrEmpty(item.unit))
                            _regItem.Value += " " + item.unit;
                        break;
                    case RegisterItemData.RegisterFormats.DATE:
                        _regItem.Value = GetDateValue(item.registerStart, item.registerNumber).ToString("yyyy-MM-dd HH:mm:ss");

                        //_regItem.Value = GetDateValue(item.registerStart, item.registerNumber).ToShortDateString() + " - "+ GetDateValue(item.registerStart, item.registerNumber).ToShortTimeString();
                        if (!string.IsNullOrEmpty(item.unit))
                            _regItem.Value += " " + item.unit;
                        break;
                    case RegisterItemData.RegisterFormats.SIGNAL:
                        _regItem.Value = GetLowerByte(mRegister[item.registerStart]) + " (Step " + GetUpperByte(mRegister[item.registerStart]) + ")";
                        break;
                    case RegisterItemData.RegisterFormats.LANGUAGE:
                        _regItem.Value = GetLongValue(item.registerStart, item.registerNumber) == 0 ? "English" : "Chinese";
                        break;
                    case RegisterItemData.RegisterFormats.ERROR:
                        switch(GetLongValue(item.registerStart, item.registerNumber))
                        {
                            case 0:
                                _regItem.Value = "no received signal";
                                break;
                            case 1:
                                _regItem.Value = "low received signal";
                                break;
                            case 2:
                                _regItem.Value = "poor received signal";
                                break;
                            case 3:
                                _regItem.Value = "pipe empty";
                                break;
                            case 4:
                                _regItem.Value = "hardware failure";
                                break;
                            case 5:
                                _regItem.Value = "receiving circuits gain in adjusting";
                                break;
                            case 6:
                                _regItem.Value = "frequency at the frequency output over flow";
                                break;
                            case 7:
                                _regItem.Value = "current at 4-20mA over flow";
                                break;
                            case 8:
                                _regItem.Value = "RAM check-sum error";
                                break;
                            case 9:
                                _regItem.Value = "main clock or timer clock error";
                                break;
                            case 10:
                                _regItem.Value = "parameters check-sum error";
                                break;
                            case 11:
                                _regItem.Value = "ROM check-sum error";
                                break;
                            case 12:
                                _regItem.Value = "temperature circuits error";
                                break;
                            case 13:
                                _regItem.Value = "reserved";
                                break;
                            case 14:
                                _regItem.Value = "internal timer over flow";
                                break;
                            case 15:
                                _regItem.Value = "analog input over range";
                                break;
                            default:
                                _regItem.Value = "N/A";
                                break;
                        }
                        break;
                    default:
                        _regItem.Value = "N/A";
                        break;
                }
            }
        }
    }

    private float GetReal4Value(int registerStart, int registerNumber)
    {
        return System.BitConverter.ToSingle(GetBytes(registerStart, registerNumber), 0);
    }

    private int GetLongValue(int registerStart, int registerNumber)
    {
        return System.BitConverter.ToInt16(GetBytes(registerStart, registerNumber), 0);
    }

    private string GetBcdValue(int registerStart, int registerNumber)
    {
        return System.BitConverter.ToString(GetBytes(registerStart, registerNumber), 0);
    }

    private System.DateTime GetDateValue(int registerStart, int registerNumber)
    {
        byte[] bytes = GetBytes(registerStart, registerNumber);
        return new System.DateTime(1980+bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], bytes[0]);
    }

    private byte[] GetBytes(int registerStart, int registerNumber)
    {
        MemoryStream memoryStream = new MemoryStream();
        for (int i = 0; i < registerNumber; i++)
        {
            byte[] _bytes = System.BitConverter.GetBytes(mRegister[registerStart + i]);
            memoryStream.Write(_bytes, 0, _bytes.Length);
        }
        return memoryStream.ToArray();
    }


    private byte GetUpperByte(ushort word)
    {
        return (byte)(word >> 8);
    }

    private byte GetLowerByte(ushort word)
    {
        return (byte)(word & 255);
    }


}

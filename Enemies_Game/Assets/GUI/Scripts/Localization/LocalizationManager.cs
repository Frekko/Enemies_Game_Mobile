using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UniRx.Async;


public class LocalizationManager : MonoBehaviour 
{
	public SystemLanguage defaultLocalization = SystemLanguage.English;
	public SystemLanguage forceLocalization = SystemLanguage.Unknown;
	public bool saveUnknownKeyword = true;

	Dictionary<string, LocalizationStringElement> _localizationData;
	List<string> _unknownKeys;
	static LocalizationManager _instance;
	string _currentLocalizationName = "Unknown";

	public static LocalizationManager instance
	{
		get
		{
			return _instance;
		}
	}

	static string _localizationDataSubfolder = "Localization/";

	// Use this for initialization
	void Awake () 
	{
		_instance = this;

		string localizationName="";

		if (forceLocalization != SystemLanguage.Unknown)
		{
			localizationName = forceLocalization.ToString();
		}
		else if (Application.systemLanguage != SystemLanguage.Unknown)
		{
			localizationName = Application.systemLanguage.ToString();
		}
		else
		{
			localizationName =SystemLanguage.Unknown.ToString();
		}

		LoadLocalization (localizationName);
	}

	async UniTaskVoid LoadLocalization (string localizationFileName)
	{
		_currentLocalizationName = localizationFileName;
		string resourcePath = Application.streamingAssetsPath + "/" + _localizationDataSubfolder + localizationFileName + ".po";
		
		
		

//		if (!File.Exists(resourcePath))
//		{
//			Debug.LogError ("Can't load localization from " + resourcePath);
//			_localizationData = new Dictionary<string, LocalizationStringElement>();
//			return;
//		}
//
//		var txt = File.ReadAllText(resourcePath, Encoding.UTF8);

		var txt = await WebRequestHelper.GetTextAsync(resourcePath);
		_localizationData = ParsePOFile.Parse(txt);
	}
	

	public static string Localize(string keyText)
	{
		if (_instance == null)
		{
			//Debug.Log ("LocalizationManager.instance == null");
			return keyText;
		}

		if (_instance._localizationData == null )
		{
			Debug.Log ("There is no localization for " + Application.systemLanguage.ToString());
			return keyText;
		}

		if (_instance._localizationData.ContainsKey(keyText))
		{
			return _instance._localizationData[keyText].translation;
		}
		else if (_instance.saveUnknownKeyword)
		{
			_instance.SaveUnknownKey (keyText);
		}

		return keyText;	
	}

    public static string LocalizeCombinedString(string[] combinedString)
    {
        if (combinedString.Length <= 0)
        {
            return "";
        }

        if (combinedString.Length == 1)
        {
            return Localize(combinedString[0]);
        }


        string format = combinedString[0];
        object[] localizedStrParams = new object[combinedString.Length - 1];

        for (int i = 1; i < combinedString.Length; i++)
        {
            localizedStrParams[i-1] = Localize(combinedString[i]);
        }


        return string.Format(Localize(format), localizedStrParams);
    }

	public void SaveUnknownKey (string key)
	{
		#if UNITY_EDITOR
		string unknownKeysFilePath = Application.persistentDataPath+"/unknownKeys.po";

		if (_unknownKeys == null)
		{
			_unknownKeys = new List<string>();
		}

		if (!_unknownKeys.Contains(key))
		{
			_unknownKeys.Add(key);
		}
		else
		{
			return;
		}

		string strToSave = string.Format("# unknown localization keys for {0} \n\n", _currentLocalizationName);

		foreach (var savedKey in _unknownKeys)
		{
			strToSave += string.Format("msgid \"{0}\"\nmsgstr \"{0}\"\n\n", savedKey);
		}

		TextWriter tw = new StreamWriter(unknownKeysFilePath);
		tw.Write(strToSave);
		tw.Close();

		Debug.Log (string.Format("Unknown key {0} added to {1}", key, unknownKeysFilePath));
		#endif
	}
}

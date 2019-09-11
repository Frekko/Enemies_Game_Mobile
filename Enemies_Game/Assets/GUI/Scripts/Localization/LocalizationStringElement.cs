using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocalizationStringElement
{
	public LocalizationStringElement(string msgstr)
	{
		_translation = msgstr;
	}

	string _translation = "";
	List<string> _pluralTranslations;

	public string translation
	{
		get
		{
			return _translation;
		}
	}

	public void AddPlural(string translString)
	{
		if(_pluralTranslations == null)
		{
			_pluralTranslations = new List<string>();
		}
		_pluralTranslations.Add(translString);
	}
}

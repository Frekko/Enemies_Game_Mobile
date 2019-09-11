using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TextLocalizationOnAwake : MonoBehaviour 
{

	void Awake()
	{
		Text textComponent = GetComponent<Text> ();

		if (textComponent != null) 
		{
			textComponent.text = LocalizationManager.Localize (textComponent.text);
		}
		
		var tmTextComponent = GetComponent<TMPro.TextMeshProUGUI> ();

		if (tmTextComponent != null) 
		{
			tmTextComponent.text = LocalizationManager.Localize (tmTextComponent.text);
		}
	}
}

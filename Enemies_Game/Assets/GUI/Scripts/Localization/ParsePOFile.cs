using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ParsePOFile 
{
	public static Dictionary <string, LocalizationStringElement> parsedData;

	static int _lineNum=0;

	public static Dictionary<string, LocalizationStringElement> Parse (string txtFromFile)
	{
		parsedData = new Dictionary<string, LocalizationStringElement>();



		string msgid = null;
		string msgstr = null;

		string[] lines = txtFromFile.Split('\n');
		string trimmedLine ="";

		foreach (var line in lines)
		{
			_lineNum++;

			trimmedLine = line.Trim();

			if (trimmedLine.Length == 0 || line[0] == '#') //empty or comment
			{
				continue;
			}

			if (trimmedLine.StartsWith("msgid ")) 
			{
				if (msgid == null && msgstr != null)
					Debug.LogWarning("Found 2 consecutive msgid. Line: " + _lineNum);

				// A new msgid has been encountered, so commit the last one
				if (msgid != null && msgstr != null) 
				{
					AddLocalization(msgid, msgstr);
				}

				msgid = GetValue(trimmedLine);
				msgstr = null;

				continue;
			}

			if (trimmedLine.StartsWith("msgstr ")) 
			{
				if (msgid == null)
					Debug.LogWarning("msgstr with no msgid. Line: " + _lineNum);

				msgstr = GetValue(trimmedLine);
				continue;
			}

			if (trimmedLine[0] == '"') 
			{
				if (msgid == null && msgstr == null)
					Debug.LogWarning("Invalid format. Line: " + _lineNum);

				if (msgstr == null) 
				{
					msgid += GetValue(trimmedLine).Replace("\\r", "\r").Replace("\\n", "\n");
				} 
				else 
				{
					msgstr += GetValue(trimmedLine);
				}
				continue;
			}
		}

		if (msgid != null) 
		{
			if (msgstr == null)
				Debug.LogWarning("Expecting msgstr. Line: " + _lineNum);

			AddLocalization(msgid, msgstr);
		}

		return parsedData;
	}

	static void AddLocalization (string msgid, string msgstr)
	{
		if (string.IsNullOrEmpty(msgid)) 
		{
			Debug.LogWarning("Error: Found empty msgid - will skip it. Line: " + _lineNum);
		} 
		else 
		{
			LocalizationStringElement item = new LocalizationStringElement(msgstr);

			if (parsedData.ContainsKey(msgid)) 
			{
				Debug.LogWarning(string.Format("Error: Found duplicate msgid {0} at line {1} - will overwrite the value from earlier instances.", msgid, _lineNum));
			}
			parsedData[msgid] = item;
		}
	}

	#region string utils
	static string Unescape(string unescapedCString)
	{
		StringBuilder result = new StringBuilder();

		// System.Text.RegularExpressions.Regex.Unescape(result) would unescape many chars that
		// .po files don't escape (it escapes \, *, +, ?, |, {, [, (,), ^, $,., #, and white space), so I'm
		// doing it manually.

		char lastChar = '\0';
		bool escapeCompleted = false;
		for (int i = 0; i < unescapedCString.Length; i++) {

			char currentChar = unescapedCString[i];

			if (lastChar == '\\') {

				escapeCompleted = true;

				switch (currentChar) {
					case '\\': result.Append("\\"); break;
					case '"': result.Append("\""); break;
					case 'r': result.Append("\r"); break;
					case 'n': result.Append("\n"); break;
					case 't': result.Append("\t"); break;
					default:
						escapeCompleted = false;

						result.Append(lastChar);
						result.Append(currentChar);                            
						break;
				}
			} else if (currentChar != '\\') {
				result.Append(currentChar);
			}

			if (escapeCompleted) {
				lastChar = '\0';
				escapeCompleted = false;
			} else {
				lastChar = currentChar;
			}
		}

		return result.ToString();
	}


	static string GetValue(string line)
	{
		int begin = line.IndexOf('"');
		if (begin == -1)
			Debug.LogWarning(string.Format("No begin quote at line {0}: {1}", _lineNum, line));

		int end = line.LastIndexOf('"');
		if (end == -1)
			Debug.LogWarning(string.Format("No closing quote at line {0}: {1}", _lineNum, line));

		return Unescape(line.Substring(begin + 1, end - begin - 1));
	}
	#endregion
}

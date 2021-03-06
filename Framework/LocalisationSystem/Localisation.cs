using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using System;
	using System.Collections.Generic;
	using Utils;
	using Serialization;

	namespace LocalisationSystem
	{
		public static class Localisation
		{
			#region Public Data
			public static readonly string kDefaultLocalisationFilePath = "Localisation/Localisation";
			#endregion

			#region Private Data
			private static LocalisationMap _localisationMap;
			private static SystemLanguage _currentLanguage = SystemLanguage.English;
			private static SystemLanguage _fallBackLanguage = SystemLanguage.English;
			private static Dictionary<string, string> _variables = new Dictionary<string, string>();
#if UNITY_EDITOR
			private static string[] _editorKeys;
#endif
			#endregion

#if UNITY_EDITOR
			#region Menu Functions
			[MenuItem("Localisation/Reload Strings")]
			private static void MenuReloadStrings()
			{
				LoadStrings();
			}

			[MenuItem("Localisation/Save Strings")]
			private static void MenuSaveStrings()
			{
				SaveStrings();
			}
			#endregion
#endif
			
			#region Public Interface
			public static void LoadStrings()
			{
				_localisationMap = null;

				TextAsset asset = Resources.Load(kDefaultLocalisationFilePath) as TextAsset;

				if (asset != null)
				{
					_localisationMap = SerializeConverter.FromTextAsset<LocalisationMap>(asset);
				}				
				else
				{
					_localisationMap = new LocalisationMap();
				}
#if UNITY_EDITOR
				RefreshEditorKeys();
#endif
			}
			
			public static KeyValuePair<string, string> VariablePair(string key, string value)
			{
				return new KeyValuePair<string, string>(key, value);
			}

			public static string GetString(string key, params KeyValuePair<string, string>[] variables)
			{
				return GetString(key, _currentLanguage, variables);
			}

			public static SystemLanguage GetCurrentLanguage()
			{
				return _currentLanguage;
			}

			public static void SetVaraiable(string key, string value)
			{
				_variables[key] = value;
			}

			public static void ClearVaraiable(string key)
			{
				_variables.Remove(key);
			}

#if UNITY_EDITOR
			public static void UpdateString(string key, SystemLanguage language, string text)
			{
				if (_localisationMap == null)
					LoadStrings();

				_localisationMap.UpdateString(key, language, text);

				RefreshEditorKeys();
			}

			public static void SaveStrings()
			{
				if (_localisationMap!= null)
				{
					string path;

					TextAsset asset = Resources.Load(kDefaultLocalisationFilePath) as TextAsset;
					if (asset != null)
					{
						path = AssetDatabase.GetAssetPath(asset);
					}
					else
					{
						path = Application.dataPath + "/Resources/" + kDefaultLocalisationFilePath + ".xml";
					}

					SerializeConverter.ToFile(_localisationMap, path);
				}				
			}

			public static string[] GetStringKeys()
			{
				if (_localisationMap == null)
					LoadStrings();
						
				return _editorKeys;
			}

			public static bool IsValidKey(string key)
			{
				if (_localisationMap == null)
					LoadStrings();

				return _localisationMap.IsValidKey(key);
			}
#endif
			#endregion

			public static readonly string kVariableStartChars = "${";
			public static readonly string kVariableEndChars = "}";

			#region Private Functions
			private static string GetString(string key, SystemLanguage language, params KeyValuePair<string, string>[] variables)
			{
				if (_localisationMap == null)
					LoadStrings();

				string text = _localisationMap.GetString(key, language, _fallBackLanguage);
				text = ReplaceVariables(text, variables);

				return text;
			}

			//As well as passed in variables store a table of stored variables
			private static string ReplaceVariables(string text, params KeyValuePair<string, string>[] variables)
			{
				string fullText = "";
				int index = 0;

				while (index < text.Length)
				{
					int variableStartIndex = text.IndexOf(kVariableStartChars, index);

					if (variableStartIndex != -1)
					{
						int variableEndIndex = text.IndexOf(kVariableEndChars, variableStartIndex);
						if (variableEndIndex == -1)
							throw new Exception("Can't find matching end bracket for variable in localised string");

						fullText += text.Substring(index, variableStartIndex - index);

						int variableKeyStartIndex = variableStartIndex + kVariableEndChars.Length + 1;
						string variableKey = text.Substring(variableKeyStartIndex, variableEndIndex - variableKeyStartIndex);

						bool foundKey = false;

						//First check provided variables
						foreach (KeyValuePair<string, string> variable in variables)
						{
							if (variable.Key == variableKey)
							{
								fullText += variable.Value;
								foundKey = true;
								break;
							}
						}

						//If not found in there check global variables
						if (!foundKey)
						{
							string value;
							if (_variables.TryGetValue(variableKey, out value))
							{
								fullText += value;
							}
							else
							{
								Debug.LogError("Can't find variable to replace key '" + variableKey + "'");
							}
						}

						index = variableEndIndex + kVariableEndChars.Length;
					}
					else
					{
						fullText += text.Substring(index, text.Length - index);
						break;
					}
				}

				return fullText;
			}

#if UNITY_EDITOR
			private static void RefreshEditorKeys()
			{
				_editorKeys = _localisationMap.GetStringKeys();
				ArrayUtils.Insert(ref _editorKeys, "(new key)", 0);
			}
#endif
			#endregion
		}
	}
}
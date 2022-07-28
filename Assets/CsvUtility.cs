using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using System.Reflection;

// https://bravenewmethod.com/2014/09/13/lightweight-csv-reader-for-unity/
// https://dskjal.com/unity/load-csv.html
public class CsvUtility
{
	static string SPLIT_RE = @"\s*,\s*(?=(?:[^""]*""[^""]*"")*[^""]*$)";// http://www.atmarkit.co.jp/ait/articles/1702/15/news024.html
	static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";

	public static List<List<string>> FromCsv(string csvText)
    {
		//csvText = csvText.Trim(new char[] { '\r', '\n' });
		var t_lines = Regex.Split(csvText, LINE_SPLIT_RE);

		List<string> lineList = new List<string>();
		for (int i = 0; i < t_lines.Length; i++)
		{
			string line = t_lines[i];
			int count = CountString(line, "\"");
			
			if ((count % 2) == 1)
			{
				// " が奇数の時
				bool end = false;
				while (!end)
				{
					i++;
					string _line = t_lines[i];
					count = CountString(_line, "\"");
					line += "\n" + _line;
					end = (count % 2) == 1;
				}
				lineList.Add(line);
			}
			else
			{
				// " が偶数の時
				lineList.Add(line);
			}
		}

		var lines = lineList.ToArray();


		if (lines.Length <= 0) return null;

		List<List<string>> list = new List<List<string>>();

		// ヘッダの名前を元に、T のメンバーに値を入れていく。
		for (var i = 0; i < lines.Length; i++)
		{
			//Debug.LogWarning (lines[i]);
			string[] values = Regex.Split(lines[i], SPLIT_RE);
			if (values.Length == 0 || values[0] == "") continue;

			List<string> entry = new List<string>();
			for (var j = 0; j < values.Length; j++)
			{
				string value = values[j];
				value = FormatString(value);
				entry.Add(value);
			}
			list.Add(entry);
		}
		return list;
	}

	public static T[] FromCsv<T>(string csvText)
	{
		//csvText = csvText.Trim(new char[] {'\r', '\n'});
		var t_lines = Regex.Split (csvText, LINE_SPLIT_RE);

		List<string> lineList = new List<string> ();
		for (int i = 0; i < t_lines.Length; i++) {
			string line = t_lines[i];
			int count = CountString (line, "\"");
			//Debug.Log (line + " / " + ((count % 2) == 1));
			if ((count % 2) == 1) {
				// " が奇数の時
				bool end = false;
				while(!end){
					i++;
					string _line = t_lines[i];
					count = CountString (_line, "\"");
					line += "\n"+_line;
					end = (count % 2) == 1;
				}
				lineList.Add (line);
			}else{
				// " が偶数の時
				lineList.Add (line);
			}
		}

		var lines = lineList.ToArray ();

		if(lines.Length <= 0) return null;
		
		List<T> list = new List<T>();

		// ヘッダの名前を元に、T のメンバーに値を入れていく。
		var header = Regex.Split(lines[0], SPLIT_RE);
		for (var i = 1; i < lines.Length; i++) {

			//Debug.LogWarning (lines[i]);
			string[] values = Regex.Split(lines[i], SPLIT_RE);
			if(values.Length == 0 ||values[0] == "") continue;

			// http://pgnote.net/?p=854
			T entry = (T)Activator.CreateInstance<T> ();
			for(var j = 0; j < header.Length && j < values.Length; j++ ) {
				string value = values[j];
				//Debug.LogWarning ("---" + value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS));
				//value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
				string headerProp = header [j];
				if(headerProp == null || string.IsNullOrEmpty(headerProp)) {
					Debug.LogWarning("No header: "+i+" / "+j);
					continue; }

				// プロパティ情報の取得
				System.Reflection.FieldInfo field = typeof(T).GetField(headerProp, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (!field.FieldType.IsArray)
                {
                    if (field.FieldType == typeof(Int32))
                    {
                        int n;
                        if (int.TryParse(value, out n))
                        {
                            field.SetValue(entry, n);
                        }
                    }
                    else if (field.FieldType == typeof(Single))
                    {
                        float f;
                        if (float.TryParse(value, out f))
                        {
                            field.SetValue(entry, f);
                        }
                    }
                    else if (field.FieldType == typeof(String))
                    {
						value = FormatString(value);
						field.SetValue(entry, value.ToString());
                    }else{
                        Debug.LogWarning("Error not support type.");
                    }
                }
                else {
                    Debug.LogWarning("Error: not support Array type." + field.FieldType);
                }
			}
			list.Add ((T)entry);
		}
		return list.ToArray();
	}


	/// <summary>
	/// 指定された文字列内にある文字列が幾つあるか数える
	/// </summary>
	/// <param name="strInput">strFindが幾つあるか数える文字列</param>
	/// <param name="strFind">数える文字列</param>
	/// <returns>strInput内にstrFindが幾つあったか</returns>
	public static int CountString(string strInput, string strFind)
	{
		int foundCount = 0;
		int sPos = strInput.IndexOf(strFind);
		while (sPos > -1)
		{
			foundCount++;
			sPos = strInput.IndexOf(strFind, sPos + 1);
		}

		return foundCount;
	}

	public static string ToCSV<T>(T[] data)
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder ();
		
		FieldInfo[] fInfos = typeof(T).GetFields ();
        // header
        for (int i = 0; i < fInfos.Length; i++) {
			var fInfo = fInfos[i];
			sb.Append (fInfo.Name);
            if(i < fInfos.Length - 1){
                sb.Append(",");
            }
        }
		sb.AppendLine ();
		//
		for (int n = 0; n < data.Length; n++) {
			var d = data [n];

			string[] strs = new string[fInfos.Length];
			for (int i = 0; i < fInfos.Length; i++) {
				var fInfo = fInfos[i];
				string cellStr = fInfo.GetValue (d).ToString ();
				strs [i] = CorrectFormat(cellStr);
			}
			string lineStr = string.Join (",", strs);
			sb.AppendLine (lineStr);
		}

		return sb.ToString ();
	}

	public static string CorrectFormat(string str)
	{
        //if (str.IndexOf(",") > -1 || str.IndexOf("\n") > -1 || str.IndexOf("\r") > -1){
        str = str.Replace("\n", "\r");

        return str;
	}

	public static string FormatString(string str)
    {
		str = str.TrimStart("\""[0]);
		str = str.TrimEnd("\""[0]);
		str = str.Replace("\"\"", "\"");
		return str;
	}

	// -----
	// csv変換、string変換
	// -----

	public static readonly string SPLIT = ",";

	public static string ToCsv(Vector3 v)
	{
		return v.x + SPLIT + v.y + SPLIT + v.z;
	}
	public static string ToCsv(Quaternion v)
	{
		return v.x + SPLIT + v.y + SPLIT + v.z + SPLIT + v.w;
	}
	public static string ToCsv(bool v)
	{
		return (v ? 1 : 0).ToString();
	}


	public static float[] ToFloat(string[] csvStrs)
	{
		int num = csvStrs.Length;
		float[] csv = new float[num];
		for (int i = 0; i < num; i++)
		{
			bool success = float.TryParse(csvStrs[i], out csv[i]);
			if (!success)
			{
				Debug.LogWarning("error float: " + csvStrs[i]);
			}
		}
		return csv;
	}

	public static Vector3 ToVector3(float x, float y, float z)
	{
		return new Vector3(x, y, z);
	}

	public static Quaternion ToQuaternion(float x, float y, float z, float w)
	{
		return new Quaternion(x, y, z, w);
	}
	public static Vector3 ToVector3(string x, string y, string z)
	{
		return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
	}
	public static Quaternion ToQuaternion(string x, string y, string z, string w)
	{
		return new Quaternion(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
	}
	public static Vector3 ToVector3(float[] csv)
	{
		return new Vector3(csv[0], csv[1], csv[2]);
	}
	public static Quaternion ToQuaternion(float[] csv)
	{
		return new Quaternion(csv[0], csv[1], csv[2], csv[3]);
	}
	public static bool ToBool(float csv)
	{
		return csv == 1;
	}

	public static Vector3 ToVector3(string[] csv)
	{
		float[] data = ToFloat(csv);
		return ToVector3(data);
	}
	public static Quaternion ToQuaternion(string[] csv)
	{
		float[] data = ToFloat(csv);
		return ToQuaternion(data);
	}
	public static bool ToBool(string csv)
	{
		return csv == "1";
	}
}


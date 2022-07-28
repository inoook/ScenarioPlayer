using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CsvToScenario : MonoBehaviour
{

    // Load
    public List<KeyTimePlayer.KeyTime> LoadCsv(string path)
    {
        List<KeyTimePlayer.KeyTime> log = new List<KeyTimePlayer.KeyTime>();

        Encoding encoder = Encoding.GetEncoding("Shift_JIS");
        string csv = FileUtils.LoadStream(path, encoder);
        List<List<string>> csvData = CsvUtility.FromCsv(csv);

        // data
        // 一行目はヘッダー
        for (int i = 1; i < csvData.Count; i++)
        {
            List<string> line = csvData[i];
            log.Add(FromCsvLine(line));
        }
        Debug.Log("log: " + log.Count);

        return log;
    }

    KeyTimePlayer.KeyTime FromCsvLine(List<string> data)
    {
        KeyTimePlayer.KeyTime s = new KeyTimePlayer.KeyTime();
        // time は時刻形式 0:00:00.000
        string time = data[0];
        s.timeSec = (float)(System.TimeSpan.Parse(time).TotalSeconds);
        return s;
    }
}

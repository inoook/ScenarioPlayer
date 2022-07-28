﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class FileUtils
{
    public static string LoadStream(string filePath, Encoding encoding = null)
    {
        if (encoding == null)
        {
            encoding = Encoding.UTF8;
        }
        if (File.Exists(filePath))
        {
            StreamReader reader = new StreamReader(filePath, encoding);
            string str = reader.ReadToEnd();
            reader.Close();

            return str;
        }
        else
        {
            Debug.LogError("file not exist");
            return "";
        }
    }
}

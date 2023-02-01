using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CsvReader : MonoBehaviour
{
    public static string[][] ReadCSV(string filePath)
    {
        if (!File.Exists(filePath)){
            Debug.LogError("There is no file on " + Path.GetFullPath(filePath));
            return null;
        }
        string[] fileContent = File.ReadAllLines(filePath);
        var columnLength = fileContent.Length;
        string[][] table = new string[columnLength][];
        var count = 0;
        foreach (string column in fileContent)
        {
            string[] cells = column.Split(',');
            table[count] = cells;
            count++;
        }
        return table;
        
    }
}

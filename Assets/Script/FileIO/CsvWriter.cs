using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class CsvWriter
{
    public static void WriteCSV(string[][] content, string filePath)
    {
        string fullFilePath = Path.GetFullPath(filePath);
        string directoryPath = Path.GetDirectoryName(fullFilePath);
        FileChecker.AssureDirectoryExist(directoryPath);
        string fileContent = ConvertTableToString(content);
        FileStream file = new FileStream(filePath, FileMode.Create);
        StreamWriter writer = new StreamWriter(file);
        writer.Write(fileContent);
        writer.Close();
        file.Close();

    }
    public static void WriteCSV(string[,] content, string filePath)
    {
        string[][] table = ConvertMultiDimensionToTable(content);
        WriteCSV(table, filePath);
    }
    private static string ConvertTableToString(string[][] table)
    {
        var csv = new StringBuilder();
        foreach(string[] column in table)
        {
            var newLine = new StringBuilder();
            foreach(string cell in column)
            {
                if(newLine.Length == 0)
                {
                    newLine.Append(cell);
                }
                else
                {
                    newLine.AppendFormat(",{0}", cell);
                }
            }
            if(csv.Length == 0)
            {
                csv.Append(newLine);
            }
            else
            {
                csv.AppendFormat("\n{0}", newLine);
            }
        }
        return csv.ToString();
    }
    private static string[][] ConvertMultiDimensionToTable(string[,] multi)
    {
        string[][] table = new string[multi.GetLength(0)][];
        for(int i = 0; i < multi.GetLength(0); i++)
        {
            table[i] = new string[multi.GetLength(1)];
            for(int j = 0; j < multi.GetLength(1); j++)
            {
                table[i][j] = multi[i, j];
            }
        }
        return table;
    }
}

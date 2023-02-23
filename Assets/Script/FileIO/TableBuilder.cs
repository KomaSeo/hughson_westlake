using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableBuilder
{
    public enum AddMode
    {
        row,
        column
    }
    private Dictionary<(int, int), string> tableDictionary = new Dictionary<(int, int), string>();
    private int currentIndex;
    public AddMode mode { get; private set; }
    public TableBuilder(AddMode mode) { 
        this.mode = mode;
        currentIndex = 0;
    }
    public void Add(params object[] content)
    {
        int opposeIndex = 0;
        foreach(object cell in content)
        {
            (int, int) addMatrix;
            if(mode == AddMode.row)
            {
                addMatrix = (currentIndex, opposeIndex);
            }
            else
            {
                addMatrix = (opposeIndex, currentIndex);
            }
            if(cell == null)
            {
                tableDictionary.Add(addMatrix, "NULL");
            }
            else
            {
                tableDictionary.Add(addMatrix, cell.ToString());
            }
            opposeIndex++;
        }
        currentIndex++;
    }
    public string[,] GetTable()
    {
        (int, int) matrixSize = GetMatrixSize();
        string[,] table = new string[matrixSize.Item1, matrixSize.Item2];
        foreach(var key in tableDictionary.Keys)
        {
            table[key.Item1, key.Item2] = tableDictionary[key];
        }
        return table;
    }
    private (int, int) GetMatrixSize()
    {
        int maxRow = -1;
        int maxColumn = -1;
        foreach(var key in tableDictionary.Keys)
        {
            maxRow = ((maxRow < key.Item1) ? key.Item1 : maxRow);
            maxColumn = ((maxColumn < key.Item2) ? key.Item2 : maxColumn);
        }
        return (maxRow+1, maxColumn+1);
    }
}

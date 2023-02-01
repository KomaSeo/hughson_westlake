using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileChecker : MonoBehaviour
{
    public static void AssureDirectoryExist(string directoryPath)
    {
        if(directoryPath == null)
        {
            return;
        }
        AssureDirectoryExist(Path.GetDirectoryName(directoryPath));
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        else
        {
            return;
        }
    }
}

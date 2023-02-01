using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
namespace EXP.ScriptTest
{
    public class CsvTest : MonoBehaviour
    {
        void Start()
        {
            string filePath = "./test1.csv";
            string[] content = { "hello", "This is", "Test" };
            string[] content2 = { "hmmm", "IT is" };
            string[] content3 = { "test", "for jagged", "array" };
            string[][] testContent = new string[][]
            {
            content,
            content2,
            content3
            };
            CsvWriter.WriteCSV(testContent, filePath);
            string[][] testContent2;
            testContent2 = CsvReader.ReadCSV(filePath);
            foreach (string[] column in testContent2)
            {
                foreach (string cell in column)
                {
                    Debug.Log(cell);
                }
                Debug.Log("column Divider");
            }


            string filePath2 = "./test2.csv";
            string[,] testContent3 =
            {
            {"test0_0", "test0_1","test0_2" },
            {"test1_0", "test1_1", "test1_2" },
            {"test2_0", "test2_1", "test2_2" }
        };
            CsvWriter.WriteCSV(testContent3, filePath2);
            string[][] testContent4;
            testContent4 = CsvReader.ReadCSV(filePath2);
            foreach (string[] column in testContent4)
            {
                foreach (string cell in column)
                {
                    Debug.Log(cell);
                }
                Debug.Log("column Divider");
            }

        }

        void Update()
        {

        }
    }


}
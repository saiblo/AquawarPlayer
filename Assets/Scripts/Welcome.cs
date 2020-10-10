using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Welcome : MonoBehaviour
{
    public GameObject openFileDialogPrefab;

    public void OpenFile()
    {
        var openFileDialog = Instantiate(openFileDialogPrefab);
        Dialog dialog = openFileDialog.GetComponent<Dialog>();

        // Choose a file
        dialog.OpenFileDialog("Try Choosing a txt file", @"C:\StartingLocationHere", ".txt", OnDialogComplete);
    }
    static string Show(string args)
    {
        try
        {
            // 创建一个 StreamReader 的实例来读取文件 
            // using 语句也能关闭 StreamReader
            using (StreamReader sr = new StreamReader(args))
            {
                string line;
                string total = "";
                // 从文件读取并显示行，直到文件的末尾 
                while ((line = sr.ReadLine()) != null)
                {
                    total = total + line;
                    total = total + "\n";
                }
                return total;
            }
        }
        catch (Exception e)
        {
            // 向用户显示出错消息
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
            return "The file could not be read:.";
        }
        Console.ReadKey();
        return "The file could not be read:.";
    }
    void OnDialogComplete(bool isSucessful, string path)
    {
        GameObject textGameObject = GameObject.Find("TextResult");
        if (isSucessful)
        {
            Debug.Log("Path : " + path);
            textGameObject.GetComponent<Text>().text = Show(path);
        }
        else
        {
            Debug.Log("No File/Folder Chosen, Cancel was pressed or something else happened.");
            textGameObject.GetComponent<Text>().text = "No File was selected. Press this button to try again.";
        }
    }
}

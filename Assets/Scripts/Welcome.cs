using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void OnDialogComplete(bool isSucessful, string path)
    {
        if (isSucessful)
        {
            Debug.Log("Path : " + path);
        }
        else
        {
            Debug.Log("No File/Folder Chosen, Cancel was pressed or something else happened.");
        }
    }
}

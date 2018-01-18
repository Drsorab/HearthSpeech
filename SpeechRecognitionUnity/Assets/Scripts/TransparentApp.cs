using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices; // Pro and Free!!!
using System.IO;
using System.Collections.Generic;

//WARNING!! Running this code inside Unity when not in a build will make the entire development environment transparent
//Restarting Unity would however resolve

public class TransparentApp : MonoBehaviour
{
    [HideInInspector]
    public Structs.DataCollection loadedData = new Structs.DataCollection();
    SpeechRecognitionEngine speechEngine;
    [DllImport("user32.dll", EntryPoint = "SetWindowLongA")]
    static extern int SetWindowLong(int hwnd, int nIndex, long dwNewLong);
    [DllImport("user32.dll")]
    static extern bool ShowWindowAsync(int hWnd, int nCmdShow);
    [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
    static extern int SetLayeredWindowAttributes(int hwnd, int crKey, byte bAlpha, int dwFlags);
    [DllImport("user32.dll", EntryPoint = "GetActiveWindow")]
    private static extern int GetActiveWindow();
    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern long GetWindowLong(int hwnd, int nIndex);
    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern int SetWindowPos(int hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

    int mode = 0;

    private void Start()
    {
        speechEngine = GetComponent<SpeechRecognitionEngine>();
        loadedData=LoadData();
    }


    void SwitchMode(int which)
    {
        int handle = GetActiveWindow();
        int fWidth = Screen.width;
        int fHeight = Screen.height;

        //Remove title bar
        long lCurStyle = GetWindowLong(handle, -16);     // GWL_STYLE=-16
        int a = 12582912;   //WS_CAPTION = 0x00C00000L
        int b = 1048576;    //WS_HSCROLL = 0x00100000L
        int c = 2097152;    //WS_VSCROLL = 0x00200000L
        int d = 524288;     //WS_SYSMENU = 0x00080000L
        int e = 16777216;   //WS_MAXIMIZE = 0x01000000L

        lCurStyle &= ~(a | b | c | d);
        lCurStyle &= e;
        SetWindowLong(handle, -16, lCurStyle);// GWL_STYLE=-16

        // Transparent windows with click through
        if (which == 1)
            SetWindowLong(handle, -20, 524288 | 0x10000000);//GWL_EXSTYLE=-20; WS_EX_LAYERED=524288=&h80000, WS_EX_TRANSPARENT=32=0x00000020L
        else
            SetWindowLong(GetActiveWindow(), -20, 524288 | 32);//GWL_EXSTYLE=-20; WS_EX_LAYERED=524288=&h80000, WS_EX_TRANSPARENT=32=0x00000020L

        SetLayeredWindowAttributes(handle, 0, 51, 2);// Transparency=51=20%, LWA_ALPHA=2

        SetWindowPos(handle, 0, 0, 0, fWidth, fHeight, 32 | 64); //SWP_FRAMECHANGED = 0x0020 (32); //SWP_SHOWWINDOW = 0x0040 (64)
        ShowWindowAsync(handle, 3); //Forces window to show in case of unresponsive app    // SW_SHOWMAXIMIZED(3)

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            mode = 1;
            SwitchMode(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            mode = 2;
            SwitchMode(2);
        }
    }

    public void SaveData()
    {
        if(loadedData == null)
            loadedData = new Structs.DataCollection();

        if (loadedData.Hand==null)
        {
            loadedData.Hand = new List<Structs.ListWrapper>();
            for (int i = 0; i < 10; i++) {
                loadedData.Hand.Add(new Structs.ListWrapper());
                loadedData.Hand[i].myList = new List<Vector2>();
            }
        }

        loadedData.Hand[speechEngine.formation].myList = new List<Vector2>();
        foreach (Transform t in speechEngine.hand)
        {
            loadedData.Hand[speechEngine.formation].myList.Add(t.position);
        }

        loadedData.FriendlyBoard = new List<Vector2>();
        foreach (Transform t in speechEngine.friendlyBoard) {
            loadedData.FriendlyBoard.Add(t.position);
        }

        loadedData.EnemyBoard = new List<Vector2>();
        foreach (Transform t in speechEngine.enemyBoard)
        {
            loadedData.EnemyBoard.Add(t.position);
        }

        loadedData.extra = new List<Vector2>();
        foreach (Transform t in speechEngine.extras)
        {
            loadedData.extra.Add(t.position);
        }

        string path = Application.persistentDataPath + "/Data.json";
        string json = JsonUtility.ToJson(loadedData);
        Debug.Log("json:" + json);
        File.WriteAllText(path, json);

    }

    Structs.DataCollection LoadData()
    {
        Debug.Log(Application.persistentDataPath);
        if (CheckJsonExistance())
        {
            string json = File.ReadAllText(Application.persistentDataPath + "/Data.json");
            loadedData = JsonUtility.FromJson<Structs.DataCollection>(json);
            if (loadedData == null)
                return null;

            speechEngine.PopulateMe();
            return loadedData;
        }
        else
        {
            System.IO.File.WriteAllText(Application.persistentDataPath + "/Data.json", "");
            return null;
        }
    }

    bool CheckJsonExistance()
    {
        string filePath = Application.persistentDataPath + "/Data.json";

        if (System.IO.File.Exists(filePath))
        {
            // The file exists -> run event
            return true;
        }
        // The file does not exist -> run event
        return false;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;


public class SpeechRecognitionEngine : MonoBehaviour
{
    public string[] keywords = new string[] { "up", "down", "left", "right" };
    public ConfidenceLevel confidence = ConfidenceLevel.Medium;
    public float speed = 1;
    public Camera cam;
    public Text results;
    public Image target;
    public List<Transform> hand = new List<Transform>();
    public List<Transform> friendlyBoard = new List<Transform>();
    public List<Transform> enemyBoard = new List<Transform>();
    public List<Transform> extras = new List<Transform>();
    [HideInInspector]
    public int formation = 0;
    protected PhraseRecognizer recognizer;
    protected string word = "right";
    bool even = false;
    Vector2 lastPos;
    TransparentApp transApp;

    private void Start()
    {
        transApp = GetComponent<TransparentApp>();
        if (keywords != null)
        {
            recognizer = new KeywordRecognizer(keywords, confidence);
            recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
            recognizer.Start();
        }
    }

    private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        word = args.text;
        results.text = "You said: <b>" + word + "</b>";
    }

    void HandleHand(int idx) {
        GoForIt(hand[idx].position.x, hand[idx].position.y);
    }

    private void Update()
    {
        //var x = target.transform.position.x;
        //var y = target.transform.position.y;

        if (word.Contains("hand"))
        {
            int result = int.Parse(Regex.Match(word, @"\d+").Value);
            HandleHand(result - 1);
        }
        else if (word.Contains("cards")) {
            int result = int.Parse(Regex.Match(word, @"\d+").Value);
            FixFormation(result - 1);
        }
        else if (word.Contains("discard"))
        {
            int result = int.Parse(Regex.Match(word, @"\d+").Value);
            GoForIt(extras[result+3].position.x, extras[result+3].position.y);
        }
        else if (word.Contains("friend"))
        {
            int result = int.Parse(Regex.Match(word, @"\d+").Value);
            float offset = 0;
            if (word.Contains("right"))
                offset += (friendlyBoard[0].position.x + friendlyBoard[1].position.x)/2;
            
            GoForIt(friendlyBoard[result-1].position.x+offset, friendlyBoard[result -1].position.y);
        }
        else if (word.Contains("enemy"))
        {
            int result = int.Parse(Regex.Match(word, @"\d+").Value);
            GoForIt(enemyBoard[result - 1].position.x, enemyBoard[result - 1].position.y);
        }

        switch (word)
        {
            case "break":
                Win32.RightMouseClick((uint)lastPos.x, (uint)lastPos.y);
                Win32.RightMouseUp((uint)lastPos.x, (uint)lastPos.y);
                break;
            case "mouse down":
                Win32.LeftMouseClick((uint)lastPos.x, (uint)lastPos.y);
                break;
            case "mouse up":
                Win32.LeftMouseUp((uint)lastPos.x, (uint)lastPos.y);
                break;
            case "f shift":
                ShiftBoard(friendlyBoard);
                break;
            case "e shift":
                ShiftBoard(enemyBoard);
                break;
            case "m shift":
                extras[5].position = new Vector3(extras[5].position.x-70, extras[5].position.y, extras[5].position.z);
                break;
            case "next":
                GoForIt(extras[3].position.x, extras[3].position.y);
                break;
            case "me":
                GoForIt(extras[0].position.x, extras[0].position.y);
                break;
            case "hero power":
                GoForIt(extras[1].position.x, extras[1].position.y);
                break;
            case "face":
                GoForIt(extras[2].position.x, extras[2].position.y);
                break;

        }
        word = "";
        //target.transform.position = new Vector3(x, y, 0);
    }

    void GoForIt(float x , float y) {
        Win32.POINT p = new Win32.POINT();
        p.x = Convert.ToInt16(x);
        p.y = Convert.ToInt16(y);
        Vector3 screenPos = cam.WorldToScreenPoint(new Vector3(p.x, p.y, 0));
        //Win32.ClientToScreen(this.Handle, ref p);
        Win32.SetCursorPos(p.x - 5, Screen.height - p.y + 25);
        lastPos = new Vector2(p.x - 5, Screen.height - p.y + 25);
        StartCoroutine(MouseIt());
    }

    IEnumerator MouseIt() {
        Win32.LeftMouseClick((uint)lastPos.x, (uint)lastPos.y);
        yield return new WaitForSeconds(0.5f);
        Win32.LeftMouseUp((uint)lastPos.x, (uint)lastPos.y);
    }

    void ShiftBoard(List<Transform> mylist) {
        int offset = 0;
        if (even)
        {
            even = false;
            offset = -70;
        }
        else {
            even = true;
            offset = 70;
        }

        foreach (Transform t in mylist)
        {
            t.position = new Vector3 (t.position.x + offset, t.position.y, t.position.z) ;
        }
    }

    void FixFormation(int idx) {
        formation = idx;
        int count = 0;
        foreach (Transform t in hand) {
            if(transApp.loadedData!=null && transApp.loadedData.Hand.Count>0 && transApp.loadedData.Hand[formation].myList.Count>0)
                t.position = new Vector3(transApp.loadedData.Hand[formation].myList[count].x, transApp.loadedData.Hand[formation].myList[count].y);
            if (count <= idx)
            {                
                t.gameObject.SetActive(true);
            }
            else
                t.gameObject.SetActive(false);
            count++;
        }
    }

    void FixOthersFormation()
    {
        int count = 0;
        foreach (Transform t in friendlyBoard)
        {
            if (transApp.loadedData.FriendlyBoard.Count > 0)
                t.position = new Vector3(transApp.loadedData.FriendlyBoard[count].x, transApp.loadedData.FriendlyBoard[count].y);
            else
                break;
            count++;
        }
        count = 0;
        foreach (Transform t in enemyBoard)
        {
            if (transApp.loadedData.EnemyBoard.Count > 0)
                t.position = new Vector3(transApp.loadedData.EnemyBoard[count].x, transApp.loadedData.EnemyBoard[count].y);
            else
                break;
            count++;
        }
        count = 0;
        foreach (Transform t in extras)
        {        
            if (transApp.loadedData.extra.Count > 0)
                t.position = new Vector3(transApp.loadedData.extra[count].x, transApp.loadedData.extra[count].y);
            else
                break;
            count++;
        }
    }

    private void OnApplicationQuit()
    {
        if (recognizer != null && recognizer.IsRunning)
        {
            recognizer.OnPhraseRecognized -= Recognizer_OnPhraseRecognized;
            recognizer.Stop();
        }
    }

    public void PopulateMe() {
        FixFormation(9);
        FixOthersFormation();
    }
}

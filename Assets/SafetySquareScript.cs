using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KModkit;
using System.Text.RegularExpressions;

public class SafetySquareScript : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo Bomb;

    public KMSelectable water;
    public KMSelectable powder;
    public KMSelectable foam;
    public KMSelectable chem;
    public KMSelectable co2;
    public TextMesh redText;
    public TextMesh blueText;
    public TextMesh yellowText;
    public TextMesh whiteText;
    public TextMesh whitestrike;
    public GameObject barTop;
    public GameObject barBottom;
    public Material[] ledOptions;
    public Renderer led;
    public GameObject ledLight;
    public KMSelectable redButton;
    public KMSelectable blueButton;
    public KMSelectable yellowButton;
    public KMSelectable whiteButton;
    public KMHighlightable redHL;
    public KMHighlightable blueHL;
    public KMHighlightable yellowHL;
    public KMHighlightable whiteHL;
    public string[] buttonNames;
    public KMSelectable[] buttons;

    //logging
    static int moduleIdCounter = 1;
    int moduleID;
    private bool moduleSolved;
    bool stageTwo;

    int redNum;
    int blueNum;
    int yellowNum;
    int answer;
    int stage;
    string fire;
    string ans1;
    string ans2;
    string ans3;
    string ans4;

    private readonly string[] table1col1 = new string[] { "R", "W", "Y", "B", "Y" };
    private readonly string[] table1col2 = new string[] { "W", "R", "W", "Y", "B" };
    private readonly string[] table1col3 = new string[] { "Y", "B", "R", "R", "W" };
    private readonly string[] table2red1 = new string[] { "R", "W", "Y", "B" };
    private readonly string[] table2red2 = new string[] { "R", "W", "R", "W" };
    private readonly string[] table2white1 = new string[] { "B", "R", "W", "Y" };
    private readonly string[] table2white2 = new string[] { "Y", "W", "Y", "Y" };
    private readonly string[] table2blue1 = new string[] { "Y", "W", "R", "B" };
    private readonly string[] table2blue2 = new string[] { "B", "R", "Y", "R" };
    private readonly string[] table2yellow1 = new string[] { "W", "Y", "R", "R" };
    private readonly string[] table2yellow2 = new string[] { "B", "W", "Y", "B" };

    void Awake()
    {
        moduleID = moduleIdCounter++;

        water.OnInteract += delegate () { PressWater(); return false; };
        powder.OnInteract += delegate () { PressPowder(); return false; };
        foam.OnInteract += delegate () { PressFoam(); return false; };
        chem.OnInteract += delegate () { PressChem(); return false; };
        co2.OnInteract += delegate () { PressCo2(); return false; };
        redButton.OnInteract += delegate () { RedPress(); return false; };
        blueButton.OnInteract += delegate () { BluePress(); return false; };
        yellowButton.OnInteract += delegate () { YellowPress(); return false; };
        whiteButton.OnInteract += delegate () { WhitePress(); return false; };
    }

    // Use this for initialization
    void Start()
    {
        Disables();
        GetAns();
    }


    void GetAns()
    {

        //square generation
        int redNum = UnityEngine.Random.Range(0, 5);
        int blueNum = UnityEngine.Random.Range(0, 5);
        int yellowNum = UnityEngine.Random.Range(0, 5);
        int whiteNum = UnityEngine.Random.Range(0, 4);
        redText.text = redNum.ToString();
        blueText.text = blueNum.ToString();
        yellowText.text = yellowNum.ToString();
        whitestrike.text = " ";
        if (whiteNum == 0)
        { whiteText.text = " "; Debug.LogFormat("[Safety Square #{0}] No special white rules", moduleID); }
        else if (whiteNum == 1)
        { whiteText.text = "W"; whitestrike.text = "_"; Debug.LogFormat("[Safety Square #{0}] Special rule(W): don't use water", moduleID); }
        else if (whiteNum == 2)
        { whiteText.text = "OX"; Debug.LogFormat("[Safety Square #{0}] Special rule(OX): don't use foam", moduleID); }
        else if (whiteNum == 3)
        { whiteText.text = "SA"; Debug.LogFormat("[Safety Square #{0}] Special rule(SA): don't use co2", moduleID); }

        int numSum = redNum + blueNum + yellowNum;
        //eeeeeee
        Debug.LogFormat("[Safety Square #{0}] This module is using manual version 1.1, you may not get correct answers with an outdated manual. check on page 4 to verify.", moduleID);
        //eeeeeee
        //CALCULATING FIRE TYPE
        if (numSum < 5)
        {
            Debug.LogFormat("[Safety Square #{0}] Sum is less than 6, using table A", moduleID);
            //left table
            if (redNum == 0)
            { fire = "B"; }
            else if (yellowNum > redNum)
            { fire = "A"; }
            else if (whiteNum == 0)
            { fire = "C"; }
            else if (GetComponent<KMBombInfo>().IsIndicatorOn("FRK") || GetComponent<KMBombInfo>().IsIndicatorOn("IND"))
            { fire = "D"; }
            else { fire = "K"; }
        }
        else if (numSum == 5 || numSum == 6)
        {
            Debug.LogFormat("[Safety Square #{0}] Sum is 5 or 6, Using table B", moduleID);
            //middle
            if (blueNum < 3)
            { fire = "A"; }
            else if (yellowNum == blueNum)
            { fire = "B"; }
            else if (redNum == GetComponent<KMBombInfo>().GetPortPlateCount())
            { fire = "K"; }
            else if (GetComponent<KMBombInfo>().IsIndicatorOn("CAR") || GetComponent<KMBombInfo>().IsIndicatorOn("BOB"))
            { fire = "D"; }
            else { fire = "C"; }
        }
        else
        {
            Debug.LogFormat("[Safety Square #{0}] Sum is larger than 6, using table C", moduleID);
            //right
            if (GetComponent<KMBombInfo>().GetPortCount() > 4)
            { fire = "C"; }
            else if (numSum > 9)
            { fire = "D"; }
            else if (yellowNum == 4)
            { fire = "K"; }
            else if (GetComponent<KMBombInfo>().GetSerialNumberLetters().Any(x => x == 'A' || x == 'B' || x == 'C' || x == 'D' || x == 'K'))
            { fire = "A"; }
            else { fire = "B"; }
        }
        //CALCULATING CORRECT EXTINGIUSHER
        //NO VOWEL

        if (GetComponent<KMBombInfo>().GetSerialNumberLetters().All(x => x != 'A' && x != 'E' && x != 'I' && x != 'O' && x != 'U'))
        {
            //NO VOWEL
            if (fire == "A")
            {
                if (whiteNum != 1)
                { answer = 1; }
                else { answer = 3; }
            }
            else if (fire == "B")
            {
                if (whiteNum != 2) { answer = 3; }
                else if (whiteNum != 2) { answer = 5; }
                else { answer = 2; }
            }
            else if (fire == "C")
            {
                if (whiteNum != 2) { answer = 5; }
                else { answer = 2; }
            }
            else if (fire == "D")
            { answer = 2; }
            else { answer = 4; }
        }
        else
        {
            //VOWEL           
            if (fire == "A")
            {
                if (whiteNum != 2) { answer = 3; }
                else { answer = 1; }
            }
            else if (fire == "B")
            {
                if (whiteNum != 3) { answer = 5; }
                else if (whiteNum != 2) { answer = 3; }
                else { answer = 2; }
            }
            else if (fire == "C")
            {
                if (whiteNum != 3) { answer = 5; }
                else { answer = 2; }
            }
            else if (fire == "D")
            { answer = 2; }
            else { answer = 4; }
        }
        //LOGGING
        Debug.LogFormat("[Safety Square #{0}] Type {1} fire present", moduleID, fire);
        Debug.LogFormat("[Safety Square #{0}] Stage one correct extinguisher is: button {1}, {2}", moduleID, answer, buttonNames[answer]);

    }
    //Button Presses
    void PressWater()
    {
        if (moduleSolved) { return; }
        water.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Safety Square #{0}] You Pressed Water", moduleID);
        if (answer == 1 && stageTwo == false) { audio.PlaySoundAtTransform("fireSound", transform); Debug.LogFormat("safteySquare #{0}: That is correct", moduleID); StageTwo(); }
        else { WrongButton(); }
    }
    void PressPowder()
    {
        if (moduleSolved) { return; }
        powder.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Safety Square #{0}] You Pressed Powder", moduleID);
        if (answer == 2 && stageTwo == false) { audio.PlaySoundAtTransform("fireSound", transform); Debug.LogFormat("safteySquare #{0}: That is correct", moduleID); StageTwo(); }
        else { WrongButton(); }
    }
    void PressFoam()
    {
        if (moduleSolved) { return; }
        foam.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Safety Square #{0}] You Pressed Foam", moduleID);
        if (answer == 3 && stageTwo == false) { audio.PlaySoundAtTransform("fireSound", transform); Debug.LogFormat("safteySquare #{0}: That is correct", moduleID); StageTwo(); }
        else { WrongButton(); }
    }
    void PressChem()
    {
        if (moduleSolved) { return; }
        chem.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Safety Square #{0}] You Pressed Wet Chemical", moduleID);
        if (answer == 4 && stageTwo == false) { audio.PlaySoundAtTransform("fireSound", transform); Debug.LogFormat("safteySquare #{0}: That is correct", moduleID); StageTwo(); }
        else { WrongButton(); }
    }
    void PressCo2()
    {
        if (moduleSolved) { return; }
        co2.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Safety Square #{0}] You Pressed Co2", moduleID);
        if (answer == 5 && stageTwo == false) { audio.PlaySoundAtTransform("fireSound", transform); Debug.LogFormat("safteySquare #{0}: That is correct", moduleID); StageTwo(); }
        else { WrongButton(); }
    }

    void Disables()  //DISABILING STAGE TWO STUFF FOR STAGE ONE
    {
        ledLight.SetActive(false);
        redHL.gameObject.SetActive(false);
        blueHL.gameObject.SetActive(false);
        yellowHL.gameObject.SetActive(false);
        whiteHL.gameObject.SetActive(false);
    }

    void StageTwo() // STAGE TWO ACTIVATED
    {
        stageTwo = true;
        Debug.LogFormat("[Safety Square #{0}] Stage one passed!", moduleID);
        barTop.SetActive(false);
        int ledCol = UnityEngine.Random.Range(1, 4);//LED COLOR
        led.material = ledOptions[ledCol];
        ledLight.SetActive(true);
        yellowText.text = " "; //remove square text
        redText.text = " ";
        blueText.text = " ";
        whiteText.text = " ";
        whitestrike.text = " ";
        redHL.gameObject.SetActive(true);
        blueHL.gameObject.SetActive(true);
        yellowHL.gameObject.SetActive(true);
        whiteHL.gameObject.SetActive(true);
        stage = 1; //part two stage 1
        redButton.gameObject.transform.localPosition = new Vector3(-.0017f, 0.01f, 0.0317f);
        blueButton.gameObject.transform.localPosition = new Vector3(-.0017f, 0.01f, -0.0017f);
        yellowButton.gameObject.transform.localPosition = new Vector3(0.0317f, 0.01f, 0.0317f);
        whiteButton.gameObject.transform.localPosition = new Vector3(0.0317f, 0.01f, -0.0017f);
        answer = answer - 1;
        //calculate answer
        //ans1
        if (ledCol == 3)
        { ans1 = table1col1[answer]; }
        else if (ledCol == 1) { ans1 = table1col2[answer]; }
        else { ans1 = table1col3[answer]; }
        //ans2
        if (GetComponent<KMBombInfo>().IsIndicatorOff("BOB") || GetComponent<KMBombInfo>().IsIndicatorOff("IND"))
        { ans2 = "W"; }
        else if (GetComponent<KMBombInfo>().GetBatteryCount() > 3)
        { ans2 = "Y"; }
        else if (GetComponent<KMBombInfo>().GetPortCount(Port.DVI) > 0 || GetComponent<KMBombInfo>().GetPortCount(Port.RJ45) > 0)
        { ans2 = "R"; }
        else { ans2 = "B"; }
        //ans3+4
        int col;
        if (ans2 == "R") { col = 0; }
        else if (ans2 == "Y") { col = 1; }
        else if (ans2 == "W") { col = 2; }
        else { col = 3; }
        if (ans1 == "R") { ans3 = table2red1[col]; ans4 = table2red2[col]; }
        else if (ans1 == "W") { ans3 = table2white1[col]; ans4 = table2white2[col]; }
        else if (ans1 == "B") { ans3 = table2blue1[col]; ans4 = table2blue2[col]; }
        else { ans3 = table2yellow1[col]; ans4 = table2yellow2[col]; }
        //logging
        Debug.LogFormat("[Safety Square #{0}] Answer for stage two is: {1}, {2}, {3}, {4}", moduleID, ans1, ans2, ans3, ans4);
        answer = answer++;
        StartCoroutine(GrayOut());
    }

    private IEnumerator GrayOut()
    {
        foreach (KMSelectable button in buttons)
        {
            yield return new WaitForSeconds(0.15f);
            button.gameObject.GetComponentInChildren<MeshRenderer>().material = ledOptions[4];
            button.gameObject.transform.localPosition = button.gameObject.transform.localPosition + new Vector3(0f, -.0026f, 0f);
            button.GetComponentInChildren<KMHighlightable>().gameObject.SetActive(false);
        }
    }
    void RedPress()                 //STAGE TWO BUTTONS
    {
        if (moduleSolved) { return; }
        redButton.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Safety Square #{0}] You Pressed Red", moduleID);
        if (ans1 == "R" && stage == 1) { stage = 2; }
        else if (ans2 == "R" && stage == 2) { stage = 3; }
        else if (ans3 == "R" && stage == 3) { stage = 4; }
        else if (ans4 == "R" && stage == 4) { Solve(); }
        else { WrongButton(); stage = 1; }
    }
    void BluePress()
    {
        if (moduleSolved) { return; }
        blueButton.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Safety Square #{0}] You Pressed Blue", moduleID);
        if (ans1 == "B" && stage == 1) { stage = 2; }
        else if (ans2 == "B" && stage == 2) { stage = 3; }
        else if (ans3 == "B" && stage == 3) { stage = 4; }
        else if (ans4 == "B" && stage == 4) { Solve(); }
        else { WrongButton(); stage = 1; }
    }
    void YellowPress()
    {
        if (moduleSolved) { return; }
        yellowButton.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Safety Square #{0}] You Pressed Yellow", moduleID);
        if (ans1 == "Y" && stage == 1) { stage = 2; }
        else if (ans2 == "Y" && stage == 2) { stage = 3; }
        else if (ans3 == "Y" && stage == 3) { stage = 4; }
        else if (ans4 == "Y" && stage == 4) { Solve(); }
        else { WrongButton(); stage = 1; }
    }
    void WhitePress()
    {
        if (moduleSolved) { return; }
        whiteButton.AddInteractionPunch(.5f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Safety Square #{0}] You Pressed White", moduleID);
        if (ans1 == "W" && stage == 1) { stage = 2; }
        else if (ans2 == "W" && stage == 2) { stage = 3; }
        else if (ans3 == "W" && stage == 3) { stage = 4; }
        else if (ans4 == "W" && stage == 4) { Solve(); }
        else { WrongButton(); stage = 1; }
    }

    void WrongButton()
    {
        GetComponent<KMBombModule>().HandleStrike();
        Debug.LogFormat("[Safety Square #{0}] That is incorrect", moduleID);
    }

    void Solve()                    //MODULE SOLVED
    {
        moduleSolved = true;
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        barBottom.SetActive(false);
        ledLight.SetActive(false);
        led.material = ledOptions[0];
        Debug.LogFormat("[Safety Square #{0}] Module Solved!", moduleID);
        GetComponent<KMBombModule>().HandlePass();
    }

    //twitch plays
    private bool cmdIsValid(string param)
    {
        string[] parameters = param.Split(' ', ',');
        for (int i = 1; i < parameters.Length; i++)
        {
            if (!parameters[i].EqualsIgnoreCase("co2") && !parameters[i].EqualsIgnoreCase("water") && !parameters[i].EqualsIgnoreCase("foam") && !parameters[i].EqualsIgnoreCase("dry") && !parameters[i].EqualsIgnoreCase("chem")
                && !parameters[i].EqualsIgnoreCase("red") && !parameters[i].EqualsIgnoreCase("white") && !parameters[i].EqualsIgnoreCase("blue") && !parameters[i].EqualsIgnoreCase("yellow") 
                && !parameters[i].EqualsIgnoreCase("powder") && !parameters[i].EqualsIgnoreCase("drypowder") && !parameters[i].EqualsIgnoreCase("wet") && !parameters[i].EqualsIgnoreCase("chemical") && !parameters[i].EqualsIgnoreCase("wetchemical") && !parameters[i].EqualsIgnoreCase("carbon")
                 && !parameters[i].EqualsIgnoreCase("r") && !parameters[i].EqualsIgnoreCase("b") && !parameters[i].EqualsIgnoreCase("y") && !parameters[i].EqualsIgnoreCase("w") )
            {
                return false;
            }
        }
        return true;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <button> [Presses the specified button] | !{0} press <button> <button> [Example of button chaining] | !{0} check [Outputs all made inputs in chat (this is here cause of stage 2)] | Valid buttons are CO2, Water, Dry(Or: Powder, Drypowder), Foam, Chem(Or: Wet, Chemical, Wetchemical), Red(R), Blue(B), Yellow(Y), and White(W)";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*check\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (stageTwo == true)
            {
                yield return null;
                string press = "sendtochat The following buttons have been pressed so far: ";
                if (stage == 1)
                {
                    press += "none";
                }
                else if (stage == 2)
                {
                    press += ans1;
                }
                else if (stage == 3)
                {
                    press += ans1 + ", " + ans2;
                }
                else if (stage == 4)
                {
                    press += ans1 + ", " + ans2 + ", " + ans3;
                }
                yield return press;
                yield break;
            }
            else
            {
                yield return "sendtochaterror You cannot check presses on Stage 1!";
                yield break;
            }
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length > 1)
            {
                if (cmdIsValid(command))
                {
                    if (stageTwo == false)
                    {
                        if (parameters.Length != 2)
                        {
                            yield return "sendtochaterror You may only interact with one button on Stage 1!";
                            yield break;
                        }
                        for (int i = 1; i < parameters.Length; i++)
                        {
                            if (parameters[i].EqualsIgnoreCase("white") || parameters[i].EqualsIgnoreCase("red") || parameters[i].EqualsIgnoreCase("yellow") || parameters[i].EqualsIgnoreCase("blue")
                                || parameters[i].EqualsIgnoreCase("r") || parameters[i].EqualsIgnoreCase("b") || parameters[i].EqualsIgnoreCase("y") || parameters[i].EqualsIgnoreCase("w"))
                            {
                                yield return "sendtochaterror You may not interact with Stage 2 buttons yet!";
                                yield break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 1; i < parameters.Length; i++)
                        {
                            if (parameters[i].EqualsIgnoreCase("co2") || parameters[i].EqualsIgnoreCase("dry") || parameters[i].EqualsIgnoreCase("foam") || parameters[i].EqualsIgnoreCase("chem") || parameters[i].EqualsIgnoreCase("water")
                                || parameters[i].EqualsIgnoreCase("powder") || parameters[i].EqualsIgnoreCase("drypowder") || parameters[i].EqualsIgnoreCase("wet") || parameters[i].EqualsIgnoreCase("chemical") || parameters[i].EqualsIgnoreCase("wetchemical") || parameters[i].EqualsIgnoreCase("carbon") )
                            {
                                yield return "sendtochaterror You may not interact with Stage 1 buttons anymore!";
                                yield break;
                            }
                        }
                    }
                    yield return null;
                    for (int i = 1; i < parameters.Length; i++)
                    {
                        if (parameters[i].EqualsIgnoreCase("co2") || parameters[i].EqualsIgnoreCase("carbon"))
                        {
                            co2.OnInteract();
                        }
                        else if (parameters[i].EqualsIgnoreCase("water"))
                        {
                            water.OnInteract();
                        }
                        else if (parameters[i].EqualsIgnoreCase("dry") || parameters[i].EqualsIgnoreCase("powder") || parameters[i].EqualsIgnoreCase("drypowder"))
                        {
                            powder.OnInteract();
                        }
                        else if (parameters[i].EqualsIgnoreCase("chem") || parameters[i].EqualsIgnoreCase("wet") || parameters[i].EqualsIgnoreCase("chemical") || parameters[i].EqualsIgnoreCase("wetchemical"))
                        {
                            chem.OnInteract();
                        }
                        else if (parameters[i].EqualsIgnoreCase("foam"))
                        {
                            foam.OnInteract();
                        }
                        else if (parameters[i].EqualsIgnoreCase("white") || parameters[i].EqualsIgnoreCase("w"))
                        {
                            whiteButton.OnInteract();
                        }
                        else if (parameters[i].EqualsIgnoreCase("red") || parameters[i].EqualsIgnoreCase("r"))
                        {
                            redButton.OnInteract();
                        }
                        else if (parameters[i].EqualsIgnoreCase("yellow") || parameters[i].EqualsIgnoreCase("y"))
                        {
                            yellowButton.OnInteract();
                        }
                        else if (parameters[i].EqualsIgnoreCase("blue") || parameters[i].EqualsIgnoreCase("b"))
                        {
                            blueButton.OnInteract();
                        }
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
            yield break;
        }
    }
}

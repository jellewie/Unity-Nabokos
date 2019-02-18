using UnityEngine;
using UnityEngine.UI;                                                               //Required when Using UI elements.                                                          //Required to read write files with the 
using System.IO;                                                                    //Required to read write files with the streamreader and streamwriter

public class ShowDebugLog : MonoBehaviour
{
    public InputField TheTextfield;
    FileInfo FI;
    private string emthy;

    void Start()
    {                                                                               //Use this for initialization
        UpdateText();
    }
    public void ClearDebugFile()
    {
        string TheFile = Path.Combine(Application.persistentDataPath, "Debug.TXT");
        if (System.IO.File.Exists(TheFile))
        {
            FI = new FileInfo(TheFile);
            FI.Delete();
            StreamWriter SW = FI.CreateText();
            SW.Close();
            UpdateText();
        }
    }
    public void UpdateText()
    {
        string TheFile = Path.Combine(Application.persistentDataPath, "Debug.TXT");
        if (System.IO.File.Exists(TheFile))
        {
            TheTextfield.text = "";
            StreamReader SR = File.OpenText(TheFile);
            string info = SR.ReadToEnd();
            SR.Close();
            TheTextfield.text = info;
        }
    }
    public void SendEmail()
    {
        UpdateText();

        string email = "minecraftjellewie@gmail.com";                               //Mailadres to send to
        string subject = "Debug log of the game";                                   //Subject of the email
        string body = "\r\n================\r\nYou can write seomthing above this line\r\n================\r\n" + TheTextfield.text;                                  //text in the mail
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }
}

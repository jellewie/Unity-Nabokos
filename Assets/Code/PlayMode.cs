using System;                                                                       //To be able to catch errors and using StringSplitOptions and do math
using System.Collections;                                                           //To use IEnumerator (coroutine)
using UnityEngine;
using UnityEngine.SceneManagement;                                                  //To be able to switch scenes
using UnityEngine.UI;                                                               //Required when Using UI elements.
using System.IO;                                                                    //Required to read write files with the streamreader and streamwriter

//+ System.Convert.ToChar(9)
// #if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE || UNITY_EDITOR 

//Data file format level
//"Level",LevelName,NextLevel,Minsteps,CurrentZoomLevel,PlayerX,Y,Z,Steps,TimeSpend,StepsTaken

public class PlayMode : MonoBehaviour
{
    /*
     
     Android: Add  external write permission when needed???
     <uses-permission android:name=”android.permission.WRITE_EXTERNAL_STORAGE” /> to your manifest.
    
    Dataform data file:
    DataBlock,DataID,DataX,DataY,DataZ,

    'DataBlock';
        a=Stationary/wall
        b=MoveAble/Pushable box (if on marker then it will be converted to a good one)
        c=MoveAbleGood
        d=MarkerEnd
        e=Door
        f=Button
        g=Teleport
	    h=If
	    i=Player
        j=Splitter
        k=Belt

    'CCCC';
    	with f;L(X,Y,Z) |OR| D(ID)      (Toggle state of door of that block)
        with g;L(X,Y,Z) |OR| D(ID)      (tp the block to the location or block with ID[first with that ID])
        with h;A,B,C,D,E                
		        A= A(X,Y,Z) |OR| a(ID) 
		        B= = |OR| !=
		        C= B(X,Y,Z) |OR| D(ID) |OR| T(Block)
		        D= C(X,Y,Z) |OR| D(ID) 
		        E= D(X,Y,Z) |OR| D(ID) |OR| T(Block)
	        Example for g;
		        0,0,0,=,Th,D1,2,2,2
		        if block at '0,0,0' is of 'T'ype 'h'(player) then Block with i'D''1' is the same as the block at '2,2,2'
        with j;X+,Y+,Z+,X-,Y-,Z-        (bools, if true then its open on that side like in the game 'ZeGame' by 'JesperTheEnd'

        do not leave blank spaces like 'a,1,,0'
        
     "#ERROR001 - Somehow you've won while you already where in the win state"      not used
     "#ERROR002 - Ignored Data in LEVEL file"
     "#ERROR003 - To few variables in the Blockdata to make up a block"
     "#ERROR004 - I can't seem to find that (custom) level"
     "#ERROR005 - That level number or name doesn't exist"
     "#ERROR006 - That slider value isn't a value"
     "#ERROR007 - An error occured when loading the settings"                       not used
     "#ERROR008 - An error occured when saving the settings"                        not used
     "#ERROR009 - An error occured when saving the log file "
     "#ERROR010 - Files out of date, please try an update"
     "#ERROR011 - Error while loading the level"
     */
    //from settings
    public float AnimationTime;  		                                            //time in seconds for the animation
    public int StepsBuffer;                                                         //the amount of inputs that can be in the buffer to execute

    //debug
    public InputField InputLevelName;                                               //
    public InputField InputNextLevelName;                                           //
    public Text TextDebug;															//Text that is for debug purposes
    public Text TextStepsMinimal;                                                   //Text that shows steps minimal (EXTERN!)
    //other public
    public Text TextPopUp;                                                          //The text in the middle of the screen, to determen on startup what mode we are in
    public Text TextSteps;															//Text that shows the amount of moves
    public Text TextLevel;															//Text that shows the current level
    public Text TextStepsTaken;                                                     //Text that shows steps taken
    public Text TextTimeSpend;                                                      //Text that shows the time spend
    public TextAsset FileLevelData;                                                 //The file where to get the data from
    public Transform BlockA;                                                        //prefab of Stationary/wall
    public Transform BlockB;                                                        //prefab of MoveAble
    public Transform BlockC;                                                        //prefab of MoveAbleGood
    public Transform BlockD;                                                        //prefab of End point
    public Transform BlockK;                                                        //prefab of Belt
    public Transform BlocksFolder;                                                  //The folder with the blocks
    public GameObject Player;														//to select the player
    public GameObject BlocksFolderToClear;                                          //The folder to clear on reset
    public GameObject UndoButton;                                                   //To hide the button when no move is made
    public GameObject LoadAnimationBlackScreen;									    //to select the black screen
    public GameObject LoadAnimationBar;                                             //To enable and rotate the rotatingbar
    public GameObject FolderFollowPlayer;                                           //The objects to move with the player (like camera and light)
    public GameObject FolderSplashScreen;                                           //The objects to show after a level (splashscreen)

    //other private
    private GameObject ColidedWith;
    private GameObject[] ListWithObjects;                                           //TEMP, an array with objects, to select them temp (changes)
    private int Undos = 0;                                                          //The amount of undos that have happened
    private int StepsMinimal;                                                       //The amount of steps to finisch the level (current record)
    private int StepsHighscore;                                                     //The amount of steps what the player used last time (if anny)
    private int MaskMoveAble = ~(1 << 9);
    private int temp;
    private string LevelName;                                                       //The current level name
    private string NextLevelName;                                                   //The next level to load after this level is done
    private bool EditMode = false;                                                  //If in edit mode
    private bool IsMoving = false;          									    //true while a move is being 0
    private bool Vertical = false;          										//If a Vertical key is pressed and registered
    private bool Horizontal = false;                                                //If a horizontal key is pressed and registered
    private int AbleToPushBlock = 0;                                                //flag that tells if the player can push a block 0=false 1=true 2=going to
    private int AbleToPullBlock = 0;                                                //flag that tells if the player can pull a block 0=false 1=true 2=going to
    private bool GameHasBegun;                                                      //After this the time start time has be set
    private bool GameWon = false;                                                   //If the game has been won 
    private bool GamePaused = true;                                                 //If the game is paused and the timer should stop
    private bool NewHighscore = false;                                              //If we made a new highscore
    private float LoadingTime = 0.3f;                                               //The time to show the loading animation
    private float InputVertical;                                                    //If a vertical input is registred (keyboard)
    private float InputHorizontal;                                                  //If a Horizontal input is registred (keyboard)
    private float StartTime;                                                        //The time when the game has started
    private float PlayTime;                                                         //The time the player has spend in the game
    private string MovesBuffer = "";        										//list of next moves
    private string StepsTaken;                                                      //The steps that have been taken
    private string ApplyMove = "";                                                  //the move that is been extracted and need to be shown 
    private Vector2 touchOrigin = -Vector2.one;                                     //To store location of screen touch origin for touch controls.
    private Vector3 PlayerStartPosition;                                            //The start position of the player (for the animation)
    private Vector3 PlayerEndPosition;                                              //The end position of the player (for the animation)
    private Vector3 Offset;                                                         //Offset between player and camera
    private FileInfo FI;

    //Splashscreen
    public Text TextSplashLevelName;
    public Text TextSplashSteps;
    public Text TextSplashTime;
    public Text TextSplashUndo;
    public Text TextSplashNewHighscore;
    public Text TextSplashMinimal;
    public GameObject Star1;
    public GameObject Star2;
    public GameObject Star3;
    public GameObject Star4;
    public GameObject Star5;

    //Backdoor settings
    private readonly int MaxZoomIn = 10;                                            //The max level to let the user zoom in
    private readonly int MaxZoomout = 150;                                          //The max level to let the user zoom out
    private readonly int ZoomSpeed = 18;                                            //The zoom speed
    private readonly int CurrentVersionFile = 3;                                    //

    void Start()														            //Is called when initialization
    {
        StartCoroutine(LoadingAnimationIn());                                       //Show start animation
        FolderSplashScreen.SetActive(false);                                        //Hide the folder with all the splashscreen stuff
        Offset = FolderFollowPlayer.transform.position - Player.transform.position; //Set the offset
        LoadSettings();                                                             //Load the player prefs settings
        if (TextPopUp.text == "PlayMode")
        {
            TextPopUp.text = "";
            string TheFile = Path.Combine(Application.persistentDataPath, "ResumeWorld.TXT");
            if (System.IO.File.Exists(TheFile) == true)                             //If there is a last save game
            {
                CreateLevel("", TheFile);                                           //Load the world the player left last time
                FI = new FileInfo(TheFile);                                         //Select the file so we can delete it
                FI.Delete();                                                        //Delete the save file
                CheckBelowAllBoxed();                                               //check if boxes are on top of points and change them, also check if won
                if (FolderSplashScreen.activeSelf)                                  //If loadingscreen is on
                    Button_SplashScreenOff();                                       //Hide Splashscreen and select next level
            }
            else
            {
                PlayerStartPosition = Player.transform.position;                    //start pos of the player for the animation
                PlayerEndPosition = Player.transform.position;                      //end pos of the player for the animation
                LoadLevel("1");                                                     //load the above level
            }
        }
        else if (TextPopUp.text == "EditMode")
        {
            TextPopUp.text = "";
            EditMode = true;
            PlayerStartPosition = Player.transform.position;                        //start pos of the player for the animation
            PlayerEndPosition = Player.transform.position;                          //end pos of the player for the animation
        }
    }
    void Update()                                                                   //Is called once per frame
    {
        U0Timer();                                                                  //Control the timer
        U1GetInputs();                                                              //Get input and convert these to a string
        U2GetNextMoveIfNeeded();                                                    //Get next move to execute, if it has a new move check if its valid 
    }
    void LateUpdate()                                                               //Is called once after the frame
    {
        FolderFollowPlayer.transform.position = Player.transform.position + Offset; //Set camera to the player + offset
        Zoom();                                                                     //Call the zoom code that enables zoom
    }
    void OnApplicationQuit()                                                        //Is called when leaving the app
    {
        MovesBuffer = "";                                                           //Clear the buffer
        while (IsMoving)                                                            //if movement animation sn't finisched
        {
            PlayTime = StartTime - AnimationTime;           //Add time since we are skipping the animation
            AnimationTime = 1;                              //Make the animation time smal, so we would be done fast
                                                            //TODO FIXME, Make it in  better way, and check if above code works, it should give you an penalty time when leaving the game
        }
        if (!FolderSplashScreen.activeSelf)                                         //If game isn't finisched
            SaveWorld("ResumeWorld.TXT");                                           //Autosave the level
    }

    void Zoom()                                                                     //Check for zoom input and zoom if found
    {
        if (Input.touchCount == 2)                                                  //If there are two touches on the device
        {
            touchOrigin.x = -1;                                                     //Cancel register touch as a valid move
            Touch touchZero = Input.GetTouch(0);                                    //Store first touch
            Touch touchOne = Input.GetTouch(1);                                     //Store second touch
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;//calculate where the first touch started
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;   //calculate where the second touch started
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;//Distance of touches
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;//Distance of touches
            float touchOffset = prevTouchDeltaMag - touchDeltaMag;
            if (PlayerPrefs.GetInt("InverseZoom", 0) == 0)
                Camera.main.fieldOfView += touchOffset / ZoomSpeed;                 //change field of view
            else
                Camera.main.fieldOfView -= touchOffset / ZoomSpeed;                 //change field of view
            TestFieldOfViewMinMax();
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0 && Application.isFocused)      //if there is a scoll wheel moving
        {
            //if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 || Input.mousePosition.x > Screen.width || Input.mousePosition.y > Screen.height)    //Check if mouse is outside the game
            if (PlayerPrefs.GetInt("InverseZoom", 0) == 0)
                Camera.main.fieldOfView -= (Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed);   //change field of view
            else
                Camera.main.fieldOfView += (Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed);   //change field of view
            TestFieldOfViewMinMax();
        }
    }
    void LoadSettings()                                                             //To load some player pref settings
    {
        AnimationTime = PlayerPrefs.GetInt("AnimationSpeed", 250);  		        //time in miliseconds for the animation (+ conversion from miliseconds to seconds)
        AnimationTime = AnimationTime / 1000;
        StepsBuffer = PlayerPrefs.GetInt("StepsBuffer", 3);                         //the amount of inputs that can be in the buffer to execute
    }
    void LoadLevel(string LevelNameToLoad)                                          //Start loading an level
    {
        if (LevelNameToLoad.Substring(0, 1) == "c")
        {
            string TheFile = Path.Combine(Application.persistentDataPath, Path.Combine("Custom", LevelNameToLoad + ".TXT"));
            if (System.IO.File.Exists(TheFile) == true) //Check if the file exist
                CreateLevel("CustomLevel", TheFile); //Create the level from the file
            else
                Debug.LogError("#ERROR004 - I can't seem to find that (custom) level; '" + LevelNameToLoad + "' ");  //Show an error message
        }
        else
            CreateLevel(LevelNameToLoad, "LevelData");                              //Load the data from the Datafile and create the selected level
    }
    void U0Timer()                                                                  //To record the time spend in the level
    {
        if (Application.isFocused && !EditMode)                                     //If aplication is in focus
        {
            if (!GamePaused)                                                        //If the game isn't paused
            {
                if (GameWon)                                                        //If the game is won
                {
                    PlayTime = Time.fixedTime - StartTime;                          //Set the playtime
                    GamePaused = true;                                              //Pause the game
                }
                else
                    PlayTime = Time.fixedTime - StartTime;                          //Set the playtime
            }
            else if (GameHasBegun && !GameWon)                                      //If returning to the game
            {
                StartTime = Time.fixedTime - PlayTime;
                GamePaused = false;
            }
        }
        else if (!GamePaused && GameHasBegun && !EditMode && !GameWon)              //If the game isn't yet being paused
        {
            PlayTime = Time.fixedTime - StartTime;                                  //Pause the timer
            GamePaused = true;
        }
        if (!EditMode && PlayerPrefs.GetInt("Konami", 0) == 1)                      //If we are not in edit mode (and in playmode)
            TextTimeSpend.text = "Timer: " + System.Convert.ToInt32(PlayTime);      //Show time spend
    }
    void U1GetInputs()                                                              //Get input and convert these to a string
    {
        if (Input.GetKeyDown(KeyCode.Escape))                                       //If player pressed back of ESC
            Button_BackToMenu();                                                    //Go back to menu
        InputVertical = Input.GetAxisRaw("Vertical");                               //Set input controls to keys
        InputHorizontal = Input.GetAxisRaw("Horizontal");                           //Set input controls to keys
        if (Input.touchCount == 1)                                                  //Check if Input has registered a touch
        {
            Touch myTouch = Input.touches[0];                                       //Store the first touch detected.
            if (myTouch.phase == TouchPhase.Began)                                  //Check if the phase of that touch equals Began
            {
                touchOrigin = myTouch.position;                                     //If so, set touchOrigin to the position of that touch
            }
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)       //If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
            {
                Vector2 touchEnd = myTouch.position;                                //Set touchEnd to equal the position of this touch
                float x = touchEnd.x - touchOrigin.x;                               //Calculate the difference between the beginning and end of the touch on the x axis.
                float y = touchEnd.y - touchOrigin.y;                               //Calculate the difference between the beginning and end of the touch on the y axis.
                touchOrigin.x = -1;                                                 //Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
                if (Mathf.Abs(x) > Mathf.Abs(y))                                    //Check if the difference along the x axis is greater than the difference along the y axis.
                    InputHorizontal = x;                                            //x is set horizontal
                else
                    InputVertical = y;                                              //y is set horizontal
            }
        }
        if (PlayerPrefs.GetInt("InverseMove", 0) == 1)                              //If controls are backwards
        {
            InputHorizontal *= -1;
            InputVertical *= -1;
        }
        if (InputVertical != 0 || InputHorizontal != 0 || Vertical == true || Horizontal == true)   //Put input into buffer
        {
            if (MovesBuffer.Length < StepsBuffer)                                   //Check if string is to long (prevent hackers and spam moving)
            {
                if (Vertical == true)                                               //If forward and backwards are not pressed & are triggered before
                {
                    if (InputVertical == 0)
                        Vertical = false;                                           //Reset so forward and backwards can be triggered again
                }
                else if (InputVertical < 0)                                         //if button forward is pressed & not handled before
                {
                    Vertical = true;                                                //Set this press is being handled already
                    MovesBuffer = MovesBuffer + "A";                                //Handle the press
                }
                else if (InputVertical > 0)                                         //If button backwards is pressed & not handled before
                {
                    Vertical = true;                                                //Set this press is being handled already
                    MovesBuffer = MovesBuffer + "B";                                //Handle the press
                }
                if (Horizontal == true)                                             //If left and right are not pressed & are triggered before
                {
                    if (InputHorizontal == 0)
                        Horizontal = false;                                         //Reset so left and right can be triggered again
                }
                else if (InputHorizontal < 0)                                       //if button left is pressed & not handled before
                {
                    Horizontal = true;                                              //Set this press is being handled already
                    MovesBuffer = MovesBuffer + "C";                                //Handle the press
                }
                else if (InputHorizontal > 0)                                       //If button right is pressed & not handled before
                {
                    Horizontal = true;                                              //Set this press is being handled already
                    MovesBuffer = MovesBuffer + "D";                                //Handle the press
                }
            }
        }
    }
    void U2GetNextMoveIfNeeded()                                                    //Get next move to execute, if it has a new move check if its valid 
    {
        if (IsMoving == false && MovesBuffer != "")                                 //If no move is currently shown, and there is a move available
        {
            ApplyMove = MovesBuffer.Substring(0, 1);                                //Get the first move to execute
            MovesBuffer = MovesBuffer.Substring(1);                                 //Remove this move from the list
            if (ApplyMove == "R")                                                   //If an softreset
            {
                ResetLevel();                                                       //Do an reset
                return;
            }
            else if (FolderSplashScreen.activeSelf)                               //Check if we are still showing the splashscreen
            {
                touchOrigin.x = -1;                                                         //Cancel register touch as a valid move
                Button_SplashScreenOff();                                           //Turn the splashscreen off
                return;                                                             //Cancel the current move
            }
            IsMoving = true;													    //Set the move is being shown already
            PlayerStartPosition = Player.transform.position;                        //Set start position to move away from this position
            if (ApplyMove == "A")                                                   //If forward
                PlayerEndPosition = PlayerStartPosition + new Vector3(0, 0, 1f);    //Set move to coordinate
            else if (ApplyMove == "B")                                              //If backwards
                PlayerEndPosition = PlayerStartPosition + new Vector3(0, 0, -1f);   //Set move to coordinate
            else if (ApplyMove == "C")                                              //If Left
                PlayerEndPosition = PlayerStartPosition + new Vector3(1f, 0, 0);    //Set move to coordinate
            else if (ApplyMove == "D")                                              //If Right
                PlayerEndPosition = PlayerStartPosition + new Vector3(-1f, 0, 0);   //Set move to coordinate
            else if (ApplyMove == "U")                                              //If undo
                Undo();                                                             //Do an undo
            else if (ApplyMove == "a")                                              //If place block a
                Place_a();
            else if (ApplyMove == "b")                                              //If place block b
                Place_b();
            else if (ApplyMove == "c")                                              //If place block c
                Place_c();
            else if (ApplyMove == "d")                                              //If place block d
                Place_d();
            else if (ApplyMove == "r")                                              //If place block d
                Place_Remove();
            if (EditMode == false && PlayerEndPosition != PlayerStartPosition)      //If not in edit mode and player moved
            {
                AbleToPushBlock = 2;                                                //We need to check if we are pushing and if thats a valid move
                TestIfMoveIsValid();                                                //Test if move is valid
            }
            else                                                                    //If in edit mode
            {
                Debug.Log("Do we need this one? a");
                MovePlayer(Player, PlayerEndPosition);                              //Show the animation
            }
        }
    }
    void TestIfMoveIsValid()                                                        //Test if move is valid (and call animation if it's calid)
    {
        RaycastHit hit;
        if (AbleToPullBlock == 2)                                                   //If puling box
        {
            if (Physics.Raycast(PlayerStartPosition, (PlayerStartPosition - PlayerEndPosition), out hit, 1) == true)    //if for obstacles ahead
            {
                ColidedWith = hit.collider.gameObject;                              //Select the object thats hit
                if (ColidedWith.gameObject.CompareTag("b") || ColidedWith.gameObject.CompareTag("c")) //Check if the object is MoveAble
                {
                    AbleToPullBlock = 1;                                            //It need to pul the box (its able to do so)
                    MovePlayer(ColidedWith.gameObject, PlayerStartPosition);        //Show the animation
                }
                else
                {
                    IsMoving = false;                                               //Move is invaled, reset so the next move can be tried (player is blocked)
                    ColidedWith = null;                                             //reset colided with something tag
                }
            }
        }
        else if (AbleToPushBlock == 2)
        {
            AbleToPushBlock = 0;
            if (Physics.Raycast(PlayerStartPosition, -1 * (PlayerStartPosition - PlayerEndPosition), out hit, 1) == true)    //if for obstacles ahead
            {
                ColidedWith = hit.collider.gameObject;                              //Select the object thats hit
                if (ColidedWith.gameObject.CompareTag("b") || ColidedWith.gameObject.CompareTag("c")) //Check if the object is MoveAble
                {
                    if (Physics.Raycast(ColidedWith.transform.position, -1 * (PlayerStartPosition - PlayerEndPosition), out hit, 1) == false)   //if the object has no obstacles ahead
                    {
                        AbleToPushBlock = 1;                                        //It need to push the box (its able to do so)
                        StartCoroutine(AnimationMoveObject(ColidedWith, ColidedWith.transform.position + (-1 * (PlayerStartPosition - PlayerEndPosition))));                          //Show the animation
                    }
                    else
                    {
                        IsMoving = false;                                           //reset so the next move can be tried (moveable box is blocked)
                        ColidedWith = null;                                         //reset colided with something tag.
                    }
                }
                else
                {
                    IsMoving = false;                                               //Move is invaled, reset so the next move can be tried (player is blocked)
                    ColidedWith = null;                                             //reset colided with something tag
                }
            }
        }
        if (IsMoving)               //If the player needs to walk
            MovePlayer(Player, PlayerEndPosition);                          //Show the animation
        if (IsMoving && GameHasBegun == false)                                      //If this is the first move in the game
        {
            GameHasBegun = true;                                                    //Flag that the game has began
            UndoButton.SetActive(true);                                             //Set undo button active
        }
    }
    void CreateLevel(string LoadLevel, string LoadFrom)
    {
        //
        //would next code be usefull? if a number only pick the number part, would speed code up?
        //SplitBlocks = SplitLevels[LoadLevel].Split(new string[] { "\n" }, StringSplitOptions.None);
        //
        string TextFromTheFile = "";
        if (LoadFrom == "LevelData")
            TextFromTheFile = FileLevelData.text;
        else
        {
            string TheFile = LoadFrom;
            if (System.IO.File.Exists(TheFile))
            {
                StreamReader ReaderThing = File.OpenText(TheFile);
                TextFromTheFile = ReaderThing.ReadToEnd();
                ReaderThing.Close();
            }
            else
            {
                Debug.LogError("File doesn't exist, canceling load"); //JELLE TODO FIXME DO WE NEED THESE LINES??
                return;
            }
        }
        string[] DataLevel = TextFromTheFile.Split(new string[] { "\nNext" }, StringSplitOptions.None); //Cut the file into pieces and cut it on "\nNext"
        string[] FileVersion = DataLevel[0].Split(new string[] { "," }, StringSplitOptions.None);
        if (System.Convert.ToInt32(FileVersion[0]) == CurrentVersionFile)           //If the file version is the same as we are looking for
        {
            if (LoadFrom == "LevelData")                                            //If we need to load the data from the levels file
            {
                for (int A = 1; A < DataLevel.Length; A++)                          //Search and select the level we need
                {
                    string[] temp = DataLevel[A].Split(new string[] { "\r" }, StringSplitOptions.None); //Split the cut files on "\r" (reset, \n is new line)
                    string[] temp2 = temp[0].Split(new string[] { "," }, StringSplitOptions.None);  //Split those cut files on ","
                    if (LoadLevel == System.Convert.ToString(temp2[1]))             //If the level in the file is the one we want
                    {
                        CreateLevelFromData(DataLevel, A);                          //Load the selected level
                        A = DataLevel.Length;                                       //Stop the loop
                    }
                }
            }
            else
            {
                string[] temp = DataLevel[1].Split(new string[] { "\r" }, StringSplitOptions.None);   //Split the cut files on "\r" (reset, \n is new line)
                string[] temp2 = temp[0].Split(new string[] { "," }, StringSplitOptions.None);  //Split those cut files on ","
                LoadLevel = temp2[1];                                               //Set loadlevel name (since we havn't any and this is used to display level name)
                int A = 1;                                                          //Load from the first cut (select first level in the array)
                PlayerPrefs.SetInt("DontResetZoom", 1); //#HACK this will fix the zoom level when returning to an level while 'Dont reset zoom' is on (it doesn't know the start position of the zoom level)
                CreateLevelFromData(DataLevel, A);                                  //Create the level
                PlayerPrefs.SetInt("DontResetZoom", 0); //#HACK sorta fix this hack
            }
        }
        else
            Debug.LogError("#ERROR010 - Files out of date, please try an update; Looked for '" + CurrentVersionFile + "' Found '" + System.Convert.ToInt32(FileVersion[0]) + "'");
        if (EditMode)                                                               //If we are in edit mode
            return;                                                                 //Stop the code here so we dont enter playmodes
        if (StepsTaken.Length > 0)
            GameHasBegun = true;
        else
        {
            PlayTime = 0;                                                           //Reset the timer
            GamePaused = true;                                                      //Stop the timer
            GameWon = false;                                                        //Reset
            Undos = 0;                                                              //Reset
            UndoButton.SetActive(false);                                            //Reset Hide undo button
            GameHasBegun = false;                                                   //Reset Game hasn't began
            ColidedWith = null;                                                     //Reset Clear selecting of object that be moved in to
            AbleToPushBlock = 0;                                                    //Reset 
            AbleToPullBlock = 0;                                                    //Reset
            IsMoving = false;                                                       //Reset we dont need to show an animation
            MovesBuffer = "";                                                       //Reset
            PlayerStartPosition = Player.transform.position;                        //start pos of the player for the animation
            PlayerEndPosition = Player.transform.position;                          //end pos of the player for the animation
            StepsTaken = "";                                                        //Clear
        }
        TextStepsMinimal.text = "Minimal steps: " + StepsMinimal;                   //Update text
        TextLevel.text = "Level: " + LevelName;                                     //Update text
        TextStepsTaken.text = StepsTaken;                                           //Update text
        TextSteps.text = "Steps: " + System.Convert.ToString(StepsTaken.Length);    //Update text
    }
    void GetHighscore(string LoadLevel, int NewScore)
    {
        string Source = "";
        if (LoadLevel.Substring(0, 1) != "c")
            Source = "Save.TXT";
        else
            Debug.LogError("Custom level, custom highscores not implented yet");
        if (Source != "")
        {
            string TextFromRecordFile = "";
            NewHighscore = false;
            string TextBefore = "";
            string TextAfter = "";
            string TheFile = Path.Combine(Application.persistentDataPath, Source);
            if (System.IO.File.Exists(TheFile))
            {
                StreamReader SR = File.OpenText(TheFile);
                TextFromRecordFile = SR.ReadToEnd();
                SR.Close();
                string[] DataLevel = TextFromRecordFile.Split(new string[] { "\r" }, StringSplitOptions.None);    //Cut the file into pieces and cut it on "\nNext"
                for (int A = 0; A < DataLevel.Length + 1; A++)
                {
                    if (A == DataLevel.Length)                                      //If we reached the end of the string (and did not found the level)
                        NewHighscore = true;                                        //Highscore - Level not played before
                    else
                    {
                        string[] temp2 = DataLevel[A].Split(new string[] { "," }, StringSplitOptions.None);  //Split those cut files on ","
                        if (LoadLevel == System.Convert.ToString(temp2[0]))         //If the level in the file is the one we want
                        {
                            StepsHighscore = System.Convert.ToInt16(temp2[2]);      //Get the current highscore
                            if (StepsHighscore > NewScore)                          //If we have a new record
                                NewHighscore = true;                                //Highscore - Score beaten
                            for (int B = A + 1; B < DataLevel.Length; B++)          //Loop to get data after the level (to put back later)
                                TextAfter += "\r" + DataLevel[B];                   //Get the data (and temp save the data)
                            A = DataLevel.Length;                                   //Stop the loop
                        }
                        else
                        {
                            if (A == 0)                                               //if this is the first line (line 0)
                                TextBefore += DataLevel[A];                         //Dont add an enter before this line (and temp save the data)
                            else
                                TextBefore += "\r" + DataLevel[A];                  //Add an enter before the line (and temp save the data)
                        }
                    }
                }
            }
            else
                NewHighscore = true;                                                //Highscore - Save file doesnt yet exist, so new highscore
            if (NewHighscore)                                                       //If we have a new highscore
            {
                if (TextBefore == "" || TextAfter == "")                            //If file was emthy
                    TextFromRecordFile = "";                                        //Clear string
                if (TextBefore != "")                                               //If there is data before the level
                    TextFromRecordFile = TextBefore + "\r";                         //Add the temp stored data
                string Temp = LevelName + "," + GameWon + "," + StepsTaken.Length + "," + AnimationTime + "," + PlayTime + "," + StepsTaken; //create log
                int CRC = GetCRC(Temp);                                             //Create a checksom
                Temp += "," + CRC;                                                  //Add the checksom to the log
                TextFromRecordFile += Temp;                                         //Add this highscore to the data
                if (TextAfter != "")                                                //If there is data after the level
                    TextFromRecordFile += TextAfter;                                //Add the temp stored data
                StreamWriter SW;
                FI = new FileInfo(Path.Combine(Application.persistentDataPath, "Save.TXT"));
                FI.Delete();
                SW = FI.CreateText();
                SW.Write(TextFromRecordFile);
                SW.Close();
            }
        }
    }
    void CreateLevelFromData(string[] DataLevel, int A)
    {
        Player.transform.position = new Vector3(0, 0, 0);                           //reset player position
        foreach (Transform child in BlocksFolderToClear.transform)                  //For each object in the "Blocks" folder
            Destroy(child.gameObject);                                              //Remove the object
        StepsTaken = "";                                                            //Reset (will triger code to tell this is a new level)
        string DataBlock;                                                           //The type of block
        string DataID;                                                              //The ID of the block
        int DataX;                                                                  //x coordinate
        int DataY;                                                                  //y coordinate
        int DataZ;                                                                  //z coordinate
        string[] SplitBlocks = DataLevel[A].Split(new string[] { "\n" }, StringSplitOptions.None);
        for (int B = 0; B < SplitBlocks.Length; B++)
        {
            if (SplitBlocks[B] != "")                                               //If the line isn't emthy
            {
                string[] SplitBlockData = SplitBlocks[B].Split(new string[] { "," }, StringSplitOptions.None);
                if (SplitBlockData[0] == "Level")
                {
                    LevelName = SplitBlockData[1];
                    NextLevelName = SplitBlockData[2];
                    StepsMinimal = System.Convert.ToInt32(SplitBlockData[3]);
                    if (PlayerPrefs.GetInt("DontResetZoom", 0) == 0)                //If there is a zoom value and 'DontResetZoom = false'
                    {
                        float ZoomTo = System.Convert.ToSingle(SplitBlockData[4]);
                        if (ZoomTo > 0)
                        {
                            Camera.main.fieldOfView = ZoomTo;
                        }
                    }
                    if (SplitBlockData.Length >= 10)
                    {
                        Player.transform.position = new Vector3(System.Convert.ToInt32(SplitBlockData[5]), System.Convert.ToInt32(SplitBlockData[6]), System.Convert.ToInt32(SplitBlockData[7]));
                        //Steps = System.Convert.ToInt32(SplitBlockData[8]);
                        PlayTime = System.Convert.ToSingle(SplitBlockData[9]);
                        StepsTaken = SplitBlockData[10];
                    }
                }
                else if (SplitBlockData.Length > 3)
                {
                    DataBlock = SplitBlockData[0];
                    DataX = System.Convert.ToInt32(SplitBlockData[1]);
                    DataY = System.Convert.ToInt32(SplitBlockData[2]);
                    DataZ = System.Convert.ToInt32(SplitBlockData[3]);
                    if (SplitBlockData.Length > 4)
                        DataID = SplitBlockData[4];                                 //set DataID
                    else
                        DataID = "";                                                //Clear data ID so it doesn't get copied from last time
                    if (DataBlock == "a")
                    {
                        var a = Instantiate(BlockA, new Vector3(DataX, DataY, DataZ), Quaternion.identity); //Create object and select it
                        a.transform.SetParent(BlocksFolder);                        //Sort the object in to the Blocks folder
                        if (DataID != "")                                           //If there is a BlockDataID (else keep the clone name
                            a.name = DataID;                                        //Set the BlockDataID
                    }
                    else if (DataBlock == "b")
                    {
                        var a = Instantiate(BlockB, new Vector3(DataX, DataY, DataZ), Quaternion.identity); //Create object and select it
                        a.transform.SetParent(BlocksFolder);                        //Sort the object in to the Blocks folder
                        if (DataID != "")                                           //If there is a BlockDataID (else keep the clone name
                            a.name = DataID;                                        //Set the BlockDataID
                    }
                    else if (DataBlock == "c")
                    {
                        var a = Instantiate(BlockC, new Vector3(DataX, DataY, DataZ), Quaternion.identity); //Create object and select it
                        a.transform.SetParent(BlocksFolder);                        //Sort the object in to the Blocks folder
                        if (DataID != "")                                           //If there is a BlockDataID (else keep the clone name
                            a.name = DataID;                                        //Set the BlockDataID
                    }
                    else if (DataBlock == "d")
                    {
                        var a = Instantiate(BlockD, new Vector3(DataX, DataY, DataZ), Quaternion.identity); //Create object and select it
                        a.transform.SetParent(BlocksFolder);                        //Sort the object in to the Blocks folder
                        if (DataID != "")                                           //If there is a BlockDataID (else keep the clone name
                            a.name = DataID;                                        //Set the BlockDataID
                    }
                }
                else
                    Debug.LogError("#ERROR003 - Missing variables to create a block, there are only '" + SplitBlockData.Length + "' variables B='" + B + "' Data='" + SplitBlocks[B] + "'");
            }
            else
                Debug.LogError("#ERROR002 - Ignored an emthy line at = '" + (B + 2));
        }
        A = DataLevel.Length;                                                       //Stop the loop
    }
    void TestFieldOfViewMinMax()                                                    //Test if field of view is out of limits
    {
        if (Camera.main.fieldOfView < MaxZoomIn)                                    //If player is zoomed in too far in
            Camera.main.fieldOfView = MaxZoomIn;
        else if (Camera.main.fieldOfView > MaxZoomout)                              //If player is zoomed in too far away
            Camera.main.fieldOfView = MaxZoomout;
    }
    void Undo()                                                                     //Undo asked find what to undo
    {
        if (StepsTaken.Length > 0)
        {
            ApplyMove = StepsTaken.Substring(StepsTaken.Length - 1);                //Get the last executed move
            if (ApplyMove == "A")                                                   //If forward
            {
                PlayerEndPosition = PlayerStartPosition + new Vector3(0, 0, -1f);   //Set move to coordinate
            }
            else if (ApplyMove == "B")                                              //If backwards
            {
                PlayerEndPosition = PlayerStartPosition + new Vector3(0, 0, 1f);    //Set move to coordinate
            }
            else if (ApplyMove == "C")                                              //If Left
            {
                PlayerEndPosition = PlayerStartPosition + new Vector3(-1f, 0, 0);   //Set move to coordinate
            }
            else if (ApplyMove == "D")                                              //If Right
            {
                PlayerEndPosition = PlayerStartPosition + new Vector3(1f, 0, 0);    //Set move to coordinate
            }
            else if (ApplyMove == "a")                                              //If forward
            {
                AbleToPullBlock = 2;                                                //It need to pull the box (its able to do so)
                PlayerEndPosition = PlayerStartPosition + new Vector3(0, 0, -1f);   //Set move to coordinate
            }
            else if (ApplyMove == "b")                                              //If backwards
            {
                AbleToPullBlock = 2;                                                //It need to pull the box (its able to do so)
                PlayerEndPosition = PlayerStartPosition + new Vector3(0, 0, 1f);    //Set move to coordinate
            }
            else if (ApplyMove == "c")                                              //If Left
            {
                AbleToPullBlock = 2;                                                //It need to pull the box (its able to do so)
                PlayerEndPosition = PlayerStartPosition + new Vector3(-1f, 0, 0);   //Set move to coordinate
            }
            else if (ApplyMove == "d")                                              //If Right
            {
                AbleToPullBlock = 2;                                                //It need to pull the box (its able to do so)
                PlayerEndPosition = PlayerStartPosition + new Vector3(1f, 0, 0);    //Set move to coordinate
            }
            StepsTaken = StepsTaken.Substring(0, StepsTaken.Length - 1);            //Remove last move from the steps taken
            TextStepsTaken.text = StepsTaken;                                       //Display what moves have been done
            ApplyMove = "";                                                         //Dont write a move to the log}
            if (StepsTaken.Length == 0)                                             //If back at start position
                MovesBuffer = "R";                                                  //Ask for an reset, so timer etc will be reseted
        }
        else
        {
            //This is only called when the last undo is done and there is still an undo to do in the buffer
            ApplyMove = "";                                                         //Dont write a move to the log
            IsMoving = false;                                                       //Move is invaled
        }
    }
    void GameEnd()                                                                  //Game has ended
    {
        GameWon = true;                                                             //Set the gamewon flag
        GamePaused = false;                                                         //Tell the timer it isnt paused
        StartTime = Time.fixedTime - PlayTime;                                      //Set the start time
        WriteDebug(true);                                                           //Write to the debug log file
        GetHighscore(LevelName, StepsTaken.Length);                                 //Check if we have a new highscore
        Star1.SetActive(false);                                                     //Hide star 1 (game finisched)
        Star2.SetActive(false);                                                     //Hide star 2 (Done with < (minimal steps * 1.2)
        Star3.SetActive(false);                                                     //Hide star 3 Done with minimal steps
        Star4.SetActive(false);                                                     //Hide star 4 Done with < minimal steps
        Star5.SetActive(false);                                                     //Hide star 5 Undo star
        TextSplashMinimal.text = "";                                                //Hide SplashMinimal that shows if you did it with the minimal amounts of steps
        TextSplashNewHighscore.text = "";                                           //Hide SplashNewHighscore that shows if you have made a percenal record
        FolderSplashScreen.SetActive(true);                                         //Show the folder with all the splashscreen stuff
        TextSplashLevelName.text = "Level " + LevelName + " Completed";             //Set the level name text
        TextSplashSteps.text = "with " + StepsTaken.Length + " of the minimal " + StepsMinimal + " steps";  //Show minimal text 
        float Thinktime = PlayTime - (StepsTaken.Length * AnimationTime);           //Caculate Think Time
        int min = System.Convert.ToInt32(Mathf.Floor(Thinktime / 60));              //Extract min of think time (only full ones)
        int sec = System.Convert.ToInt32(Mathf.Floor(Thinktime - min * 60));        //Extract sec of think time (exclude minutes)
        int msec = System.Convert.ToInt32((Thinktime - min * 60 - sec) * 100);      //Extract miliseconds
        string a = "";                                                              //Create a new string to put time in (mm:ss:??)
        if (min < 10)
            a = "0";
        a += min + ":";
        if (sec < 10)
            a += "0";
        a += sec + ".";
        if (msec < 10)
            a += "0";
        a += msec;
        TextSplashTime.text = a;                                                    //Show the time spend text
        TextSplashUndo.text = System.Convert.ToString(Undos);                       //Show amounts of undo's used
        StartCoroutine(AnimationSplashScreen());
    }
    void CheckBelowAllBoxed()                                                       //Check below all boxes, and check if won
    {
        RaycastHit hit;                                                    	        //make a temp value for raycast named hit
        ListWithObjects = GameObject.FindGameObjectsWithTag("c");		            //make a temp, and put all MoveAbleGood boxes in to that list
        for (int i = 0; i < ListWithObjects.Length; i++)                            //Run though this full list
        {
            if (Physics.Raycast(ListWithObjects[i].transform.position - new Vector3(0, 1, 0), new Vector3(0, 1, 0), out hit, 1, MaskMoveAble) == false)   //if no MarkerEnd
            {
                var a = Instantiate(BlockB, ListWithObjects[i].transform.position, Quaternion.identity); //Add a new MoveAbleGood box at the place the MoveAble is
                a.transform.SetParent(BlocksFolder);                                //Sort the object in to the Blocks folder
                a.name = ListWithObjects[i].name;                                   //Set the name of the , thats the ID
                Destroy(ListWithObjects[i]);                                     	//remove the MoveAble box
            }
        }
        ListWithObjects = GameObject.FindGameObjectsWithTag("b");                   //put all MoveAble boxes in to the list
        for (int i = 0; i < ListWithObjects.Length; i++)                         	//Run though this full list
        {
            if (Physics.Raycast(ListWithObjects[i].transform.position - new Vector3(0, 1, 0), new Vector3(0, 1, 0), out hit, 1, MaskMoveAble) && hit.transform.tag == "d")   //if on MarkerEnd
            {
                var a = Instantiate(BlockC, ListWithObjects[i].transform.position, Quaternion.identity); //Add a new MoveAbleGood box at the place the MoveAble is
                a.transform.SetParent(BlocksFolder);                                //Sort the object in to the Blocks folder
                a.name = ListWithObjects[i].name;                                   //Set the name of the , thats the ID
                Destroy(ListWithObjects[i]);                                     	//remove the MoveAble box
                if (ListWithObjects.Length <= 1)                                    //if this was the last one
                    GameEnd();                                                      //Game has ended
            }
        }
        if (ListWithObjects.Length == 0)                                            //if the game is won
            GameEnd();                                                              //Game has ended
    }
    void WriteDebug(bool GameWon)                                                   //To write what has been done to a file
    {
        if (StepsTaken.Length < 5)
            return;
        try
        {
            string Temp = LevelName + "," + GameWon + "," + StepsTaken.Length + "," + AnimationTime + "," + PlayTime + "," + StepsTaken; //create log
            int CRC = GetCRC(Temp);                                                 //Create a checksom
            Temp += "," + CRC;                                                      //Add the checksom to the log
            StreamWriter SW = new StreamWriter(Path.Combine(Application.persistentDataPath, "Debug.TXT"), true);
            SW.WriteLine(Temp);                                                     //Write the text to the file
            SW.Close();                                                             //Close the stream so the file isn't locked anymore
        }
        catch (Exception e)
        {
            Debug.LogError("#ERROR009 - An error occured when saving the log file " + e);
        }
    }
    int GetCRC(String In)                                                           //To calculate the Checksum
    {
        int CRC = 241;
        int B = 4;
        int GroupCRC = 0;
        for (int A = 0; A < In.Length; A++)
        {
            string SingleCharacter = In.Substring(A, 1);
            int SingleInt = -1;
            try
            {
                SingleInt = ConvertToInt(SingleCharacter);
            }
            catch (Exception)
            {
            }
            if (SingleInt >= 0)
            {
                GroupCRC = GroupCRC + SingleInt;
                if (B < 0)
                {
                    if (System.Convert.ToString(CRC).Length > 5)
                    {
                        CRC = System.Convert.ToInt32(System.Convert.ToString(CRC).Substring(System.Convert.ToString(CRC).Length - 6, 6)) * GroupCRC;
                    }
                    else
                    {
                        CRC = System.Convert.ToInt32(CRC) * GroupCRC;
                    }
                    B = 4;
                }
                B--;
            }
        }
        CRC = CRC + GroupCRC;
        return CRC;
    }
    int ConvertToInt(string In)                                                     //For the CRC to convert letters to numbers
    {
        temp = -2;
        try
        {
            temp = System.Convert.ToInt16(In);
        }
        catch (Exception)
        {
            if (In == "a" || In == "A")
                temp = 1;
            else if (In == "b" || In == "B")
                temp = 2;
            else if (In == "c" || In == "C")
                temp = 3;
            else if (In == "d" || In == "D")
                temp = 4;
            else if (In == "e" || In == "E")
                temp = 5;
            else if (In == "f" || In == "F")
                temp = 6;
            else if (In == "g" || In == "G")
                temp = 7;
            else if (In == "h" || In == "H")
                temp = 8;
            else if (In == "i" || In == "I")
                temp = 9;
            else if (In == "j" || In == "J")
                temp = 10;
            else if (In == "k" || In == "K")
                temp = 11;
            else if (In == "l" || In == "L")
                temp = 12;
            else if (In == "m" || In == "M")
                temp = 13;
            else if (In == "n" || In == "N")
                temp = 14;
            else if (In == "o" || In == "O")
                temp = 15;
            else if (In == "p" || In == "P")
                temp = 16;
            else if (In == "q" || In == "Q")
                temp = 17;
            else if (In == "r" || In == "R")
                temp = 18;
            else if (In == "s" || In == "S")
                temp = 19;
            else if (In == "t" || In == "T")
                temp = 20;
            else if (In == "u" || In == "U")
                temp = 21;
            else if (In == "v" || In == "V")
                temp = 22;
            else if (In == "w" || In == "W")
                temp = 23;
            else if (In == "x" || In == "X")
                temp = 24;
            else if (In == "y" || In == "Y")
                temp = 25;
            else if (In == "z" || In == "Z")
                temp = 26;
        }
        return temp;
    }
    void SaveWorld(string File)                                                     //To save the world to disc
    {
        StreamWriter SW;
        FI = new FileInfo(Path.Combine(Application.persistentDataPath, File));
        FI.Delete();
        SW = FI.CreateText();
        string NewLine = CurrentVersionFile + ",I'm JelleWho the writer of these codes";
        SW.WriteLine(NewLine);
        NewLine = "NextLevel," + LevelName + "," + NextLevelName + "," + StepsMinimal + "," + Camera.main.fieldOfView + "," + System.Convert.ToString(Player.transform.position.x) + "," + System.Convert.ToString(Player.transform.position.y) + "," + System.Convert.ToString(Player.transform.position.z) + "," + StepsTaken.Length + "," + PlayTime + "," + StepsTaken + ",";
        SW.Write(NewLine);                                                          //Write the text to the file
        int TotalBlocks = BlocksFolder.childCount;
        for (int A = 0; A < TotalBlocks; A++)                                    //Do for each block in game
        {
            Transform a = BlocksFolder.gameObject.transform.GetChild(A);
            if (a.tag != "Untagged")                                                //Do only for tagged blocks (the tags tell us the type of prefab to use to rebuild
            {
                SW.WriteLine("");
                NewLine = System.Convert.ToString(a.tag) + "," + a.position.x + "," + a.position.y + "," + a.position.z + "," + a.name;
                SW.Write(NewLine);                                                  //Write the text to the file
            }
        }
        SW.Close();                                                                 //Close the stream so the file isn't locked anymore
    }
    void ResetLevel()                                                               //Reset current level (or load selected one)
    {
        FolderSplashScreen.SetActive(false);                                        //Hide the folder with all the splashscreen stuff
        MovesBuffer = "";                                                           //clear buffer since we dont need to wait for the animation timer
        if (InputLevelName.text != "")                                              //Check if a level number is given, and if so set it as level number to load
        {
            LoadLevel(InputLevelName.text);                                         //Load the level set in the InputLevelName
            InputLevelName.text = "";                                               //Clear the InputLevelName
        }
        else if (EditMode)
        {
            foreach (Transform child in BlocksFolderToClear.transform)              //For each object in the "Blocks" folder
                Destroy(child.gameObject);                                          //Remove the object
            Player.transform.position = new Vector3(0, 0, 0);                            //make sure player is at the end position
        }
        else
            LoadLevel(LevelName);                                                   //Reload the current level
    }
    void Place_a()                                                                  //[EditMode] Place a block
    {
        if (Physics.Raycast(Player.transform.position - new Vector3(0, 1, 0), new Vector3(0, 1, 0), 1, ~(1 << 8)) == false) //Check if there isn't a block already
        {
            var a = Instantiate(BlockA, Player.transform.position, Quaternion.identity); //Add a new MoveAbleGood box at the place the MoveAble is
            a.transform.SetParent(BlocksFolder);                                    //Sort the object in to the Blocks folder
            a.name = InputLevelName.text;                                           //Set the ID
            InputLevelName.text = "";                                               //Clear the textfield
        }
    }
    void Place_b()                                                                  //[EditMode] Place a block
    {
        if (Physics.Raycast(Player.transform.position - new Vector3(0, 1, 0), new Vector3(0, 1, 0), 1, ~(1 << 8)) == false) //Check if there isn't a block already
        {
            var a = Instantiate(BlockB, Player.transform.position, Quaternion.identity); //Add a new MoveAbleGood box at the place the MoveAble is
            a.transform.SetParent(BlocksFolder);                                    //Sort the object in to the Blocks folder
            a.name = InputLevelName.text;                                           //Set the ID
            InputLevelName.text = "";                                               //Clear the textfield
        }
    }
    void Place_c()                                                                  //[EditMode] Place a block
    {
        if (Physics.Raycast(Player.transform.position - new Vector3(0, 1, 0), new Vector3(0, 1, 0), 1, ~(1 << 8)) == false) //Check if there isn't a block already
        {
            var a = Instantiate(BlockC, Player.transform.position, Quaternion.identity); //Add a new MoveAbleGood box at the place the MoveAble is
            a.transform.SetParent(BlocksFolder);                                    //Sort the object in to the Blocks folder
            a.name = InputLevelName.text;                                           //Set the ID
            a = Instantiate(BlockD, Player.transform.position, Quaternion.identity); //Add a new MoveAbleGood box at the place the MoveAble is
            a.transform.SetParent(BlocksFolder);                                    //Sort the object in to the Blocks folder
            a.name = InputLevelName.text;                                           //Set the ID
            InputLevelName.text = "";                                               //Clear the textfield
        }
    }
    void Place_d()                                                                  //[EditMode] Place a block
    {
        if (Physics.Raycast(Player.transform.position - new Vector3(0, 1, 0), new Vector3(0, 1, 0), 1, ~(1 << 8)) == false) //Check if there isn't a block already
        {
            var a = Instantiate(BlockD, Player.transform.position, Quaternion.identity); //Add a new MoveAbleGood box at the place the MoveAble is
            a.transform.SetParent(BlocksFolder);                                    //Sort the object in to the Blocks folder
            a.name = InputLevelName.text;                                           //Set the ID
            InputLevelName.text = "";                                               //Clear the textfield
        }
    }
    void Place_Remove()                                                             //[EditMode] remove a block
    {
        RaycastHit hit;                                                    	        //make a temp value for raycast named hit
        if (Physics.Raycast(Player.transform.position - new Vector3(0, 1, 0), new Vector3(0, 1, 0), out hit, 1, ~(1 << 8)))
            Destroy(hit.transform.gameObject);                                      //remove the MoveAble box
    }
    void MovePlayer(GameObject TheBlock, Vector3 ToPos)                             //Called when the player needs to move
    {
        StartCoroutine(AnimationMoveObject(TheBlock, ToPos));                       //Show the animation
        if (!EditMode)                                                              //If we are not in the editmode (and thus playmode)
        {
            if (AbleToPushBlock == 1)                                               //If the player has moved a block
            {
                StepsTaken = StepsTaken + System.Convert.ToString(ApplyMove.ToLower());   //Record what moves have been done (pushed a block so lower case)
                AbleToPushBlock = 0;                                                //Reset able to push block so it doesnt keep pushing etc
            }
            else
                StepsTaken = StepsTaken + System.Convert.ToString(ApplyMove);       //Record what moves have been done (not pushed a block so higher case)
            TextStepsTaken.text = StepsTaken;                                       //Display what moves have been done
            TextSteps.text = "Steps: " + System.Convert.ToString(StepsTaken.Length);//Reload text to be the right number
            if (AbleToPullBlock == 1)                                               //If there is a block pushed make sure its on the position
                AbleToPullBlock = 0;                                                //Reset able to push block so it doesnt keep pushing etc
        }
    }

    IEnumerator AnimationMoveObject(GameObject Object, Vector3 ToPos)               //Called form the animation to move a block
    {
        Vector3 FromPos = Object.transform.position;                                //Get starting position
        float AnimationCurrentTime = 0;                                             //Where the animation is at
        while (AnimationCurrentTime < AnimationTime)                                //While there is an animation to show
        {
            IsMoving = true;        //TODO FIXME; add an counter for each animation, and when 0 and this code ends set moving to false
            float perc = AnimationCurrentTime / AnimationTime;                      //Calculate the percentage where the cube is on its way
            Object.transform.position = Vector3.Lerp(FromPos, ToPos, perc);         //move the block
            AnimationCurrentTime += Time.deltaTime;                                 //Move counter forward
            yield return null;                                                      //Animation not done, keep on going
        }
        Object.transform.position = (ToPos);                                        //make sure block is at the end position
        IsMoving = false;                                                           //Reset so it can start showing the next move
        if (Object.tag == "c" || Object.tag == "b")
        {
            RaycastHit hit;                                                         //make a temp value for raycast named hit
            if (Object.tag == "c")
            {
                if (Physics.Raycast(Object.transform.position - new Vector3(0, 1, 0), new Vector3(0, 1, 0), out hit, 1, MaskMoveAble) == false)   //if no MarkerEnd
                {
                    var a = Instantiate(BlockB, Object.transform.position, Quaternion.identity); //Add a new MoveAbleGood box at the place the MoveAble is
                    a.transform.SetParent(BlocksFolder);                            //Sort the object in to the Blocks folder
                    a.name = Object.name;                                           //Set the name of the , thats the ID
                    Destroy(Object);                                                //remove the MoveAble box
                }
            }
            else if (Object.tag == "b")
            {
                if (Physics.Raycast(Object.transform.position - new Vector3(0, 1, 0), new Vector3(0, 1, 0), out hit, 1, MaskMoveAble) && hit.transform.tag == "d")   //if on MarkerEnd
                {
                    var a = Instantiate(BlockC, Object.transform.position, Quaternion.identity); //Add a new MoveAbleGood box at the place the MoveAble is
                    a.transform.SetParent(BlocksFolder);                            //Sort the object in to the Blocks folder
                    a.name = Object.name;                                           //Set the name of the , thats the ID
                    Destroy(Object);                                                //remove the MoveAble box
                    ListWithObjects = GameObject.FindGameObjectsWithTag("b");       //put all MoveAble boxes in to the list
                    if (ListWithObjects.Length <= 1)                                //if there are boxed that are not at an end position
                    {
                        GameEnd();                                                  //Game has ended
                    }
                }
            }
        }
    }
    IEnumerator AnimationSplashScreen()                                             //Called when splashscreen is on for the animations
    {
        float waitfor = 0.5f;                                                       //Time to wait after an animation
        yield return new WaitForSeconds(waitfor);
        Star1.SetActive(true);
        if (StepsTaken.Length < (StepsMinimal * 1.2f))
        {
            yield return new WaitForSeconds(waitfor);
            Star2.SetActive(true);
            if (StepsTaken.Length <= StepsMinimal)                                  //If it is the minimal amount of steps
            {
                yield return new WaitForSeconds(waitfor);
                Star3.SetActive(true);
                if (StepsTaken.Length < StepsMinimal)                               //If the player beated the game highscore
                {
                    TextSplashSteps.text += "\nYou are better then me, Please contact me :D";
                    yield return new WaitForSeconds(waitfor);
                    Star4.SetActive(true);
                }
            }
        }
        if (Undos == 0)                                                             //If no undos are used
        {
            yield return new WaitForSeconds(waitfor);
            Star5.SetActive(true);
        }
        if (StepsTaken.Length <= StepsMinimal)                                      //If we need to show the minimalsteps splash
        {
            yield return new WaitForSeconds(waitfor);
            TextSplashMinimal.text = "Minimal steps!";                              //Show it
            StartCoroutine(BouceObject(TextSplashMinimal, 300, 0.25f));             //Let it bounce
        }
        if (NewHighscore)
        {
            yield return new WaitForSeconds(waitfor);
            TextSplashNewHighscore.text = "New Highscore!";                         //Show it
            StartCoroutine(BouceObject(TextSplashNewHighscore, 300, 0.25f));        //Let it bounce
        }
    }
    IEnumerator BouceObject(Text BounceText, int BounceTime, float BounceDistance)
    {
        //BounceTime = (x/60= seconds)
        //BounceDistance = The distance to bound up and down (scale -+ X)
        int b = BounceTime / 2;                                                     //We need this for the timer, Also the start position (start at 100%)
        bool Toggle = false;                                                        //If the animation text need to zoom in or out
        while (BounceText.isActiveAndEnabled)                                       //Show animation while the splashscreen is shown
        {
            if (Toggle)                                                             //If going up
            {
                if (b >= BounceTime - 1)                                            //If we bounce out all the way 
                    Toggle = false;                                                 //Toggle so we are going in again
                b += 1;                                                             //Add 1 (we are expanding)
            }
            else                                                                    //Else we are going down
            {
                if (b <= 1)                                                         //If we bounce in all the way
                    Toggle = true;                                                  //Toggle so we are going out again
                b -= 1;                                                             //Remove 1 (we are shrinking)
            }
            float x = (BounceDistance * b * 2) / BounceTime + (1 - BounceDistance); //calculate the current zoom factor (y*x*2/w +(1-y)) Thanks kim!
            BounceText.transform.localScale = new Vector3(x, x, x);  //Execute new scale (to let it bounce)
            yield return null;
        }
    }
    IEnumerator LoadingAnimationIn()                                                //Called when leaving this scene
    {
        int Size = System.Convert.ToInt32(Math.Round((Math.Sqrt(Math.Pow((Screen.width + Screen.height) / 2, 2) * 2)), 0));    //Calculate size of the loading screen to cover the complete screen
        var rectTransform = LoadAnimationBlackScreen.GetComponent<RectTransform>(); //Select it so we can change it size
        rectTransform.sizeDelta = new Vector2(Size, Size);                          //Set the size
        Vector2 from = new Vector2((-Screen.width + -Screen.height) / 2 - 10, Screen.height / 2);  //Set the 'from' position
        Vector2 to = new Vector2(Screen.width / 2, Screen.height / 2);              //End position of LoadAnimationBlackScreen
        LoadAnimationBar.SetActive(false);
        bool ShowAnimation = true;                                                  //Start to show animation
        float AnimationCurrentTime = 0;                                             //Where the animation is at
        while (ShowAnimation == true)                                               //While animation isn't done
        {
            LoadAnimationBlackScreen.transform.position = Vector2.Lerp(to, from, AnimationCurrentTime / LoadingTime);   //move the screen to the percentage
            AnimationCurrentTime += Time.deltaTime;                                 //Move counter forward
            if (AnimationCurrentTime > LoadingTime)                                 //If at end of animation
            {
                LoadAnimationBlackScreen.transform.position = from;                 //make sure player is at the end position
                if (AnimationCurrentTime > (LoadingTime + 0.1f))                    //If at end of animation
                {
                    AnimationCurrentTime = 0;                                       //Reset
                    ShowAnimation = false;                                          //Reset
                }
            }
            yield return null;
        }
        LoadAnimationBar.SetActive(false);
        LoadAnimationBlackScreen.SetActive(false);
    }
    IEnumerator LoadingAnimationOutAndLoadLevel(string SceneToLoad)                 //Called when entering this scene
    {
        int Size = System.Convert.ToInt32(Math.Round((Math.Sqrt(Math.Pow((Screen.width + Screen.height) / 2, 2) * 2)), 0));    //Calculate size of the loading screen to cover the complete screen
        var rectTransform = LoadAnimationBlackScreen.GetComponent<RectTransform>(); //Select it so we can change it size
        rectTransform.sizeDelta = new Vector2(Size, Size);                          //Set the size
        Vector2 from = new Vector2((-Screen.width + -Screen.height) / 2 - 10, Screen.height / 2);  //Set the 'from' position
        Vector2 to = new Vector2(Screen.width / 2, Screen.height / 2);              //End position of LoadAnimationBlackScreen
        LoadAnimationBlackScreen.SetActive(true);
        LoadAnimationBar.SetActive(true);
        bool ShowAnimation = true;                                                  //Start to show animation
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneToLoad);
        asyncLoad.allowSceneActivation = false;
        float AnimationCurrentTime = 0;                                             //Where the animation is at
        while (asyncLoad.progress < 0.9f || ShowAnimation == true)
        {
            LoadAnimationBlackScreen.transform.position = Vector2.Lerp(from, to, AnimationCurrentTime / LoadingTime);   //move the screen to the percentage
            AnimationCurrentTime += Time.deltaTime;                                 //Move counter forward
            LoadAnimationBar.transform.Rotate(new Vector3(0, 0, 400) * asyncLoad.progress);
            if (AnimationCurrentTime > LoadingTime)                                 //If at end of animation
            {
                LoadAnimationBlackScreen.transform.position = to;                   //make sure player is at the end position
                if (AnimationCurrentTime > (LoadingTime + 0.1f))                    //If at end of animation
                {
                    AnimationCurrentTime = 0;                                       //Reset
                    ShowAnimation = false;                                          //Reset
                }
            }
            yield return null;
        }
        asyncLoad.allowSceneActivation = true;                                      //Go to the screen
    }

    public void Button_BackToMenu()
    {
        touchOrigin.x = -1;                                                         //Cancel register touch as a valid move
        if (EditMode == false)
            SaveWorld("ResumeWorld.TXT");                                           //Autosave the level
        StartCoroutine(LoadingAnimationOutAndLoadLevel("MainMenu"));                //Start loading screen in background, and go to it when done//
    }
    public void Button_Undo()
    {
        touchOrigin.x = -1;                                                         //Cancel register touch as a valid move
        Undos += 1;                                                                 //Add one to the undo counter
        if (MovesBuffer.Length < StepsBuffer)                                       //Check if string is to long (prevent hackers and spam moving)
            MovesBuffer = MovesBuffer + "U";                                        //Put an undo in the next move buffer
    }
    public void Button_Reset()
    {
        touchOrigin.x = -1;                                                         //Cancel register touch as a valid move
        WriteDebug(false);                                                          //Write debug (game isn't won)
        if (MovesBuffer == "R")                                                     //If already tried a soft reset
            ResetLevel();                                                           //lets try it with force
        else
            MovesBuffer = "R";                                                      //Set buffer to execute an Reset (also clears pref commands)
    }
    public void Button_CRC()
    {
        if (InputLevelName.text != "")
            Debug.Log(GetCRC(InputLevelName.text));
    }
    public void Button_ExecuteSteps()
    {
        if (InputLevelName.text != "")
        {
            MovesBuffer = InputLevelName.text.ToUpper();
            InputLevelName.text = "";
        }
    }
    public void Button_LoadCustomWorld()
    {
        if (InputLevelName.text != "")                                              //Check if a level name (or rather file name) is given
        {
            string TheFile = Path.Combine(Application.persistentDataPath, Path.Combine("Custom", InputLevelName.text + ".TXT"));
            if (System.IO.File.Exists(TheFile) == true)                             //Check if the file exist
                CreateLevel("CustomLevel", TheFile);                                //Create the level from the file
        }
    }
    public void Button_SaveCustomLevel()
    {
        if (InputLevelName.text != "")                                              //Check if a level name (or rather file name) is given
        {
            string TheFile = Path.Combine(Application.persistentDataPath, "Custom");
            if (System.IO.File.Exists(TheFile) == false)                            //Check if there is already a custom folder
                Directory.CreateDirectory(TheFile);                                 //Create the custom folder
            NextLevelName = InputNextLevelName.text;
            SaveWorld(Path.Combine(TheFile, InputLevelName.text + ".TXT"));         //Save the world to the file
        }
    }
    public void Button_Place_a()
    {
        MovesBuffer = "a";
    }
    public void Button_Place_b()
    {
        MovesBuffer = "b";
    }
    public void Button_Place_c()
    {
        MovesBuffer = "c";
    }
    public void Button_Place_d()
    {
        MovesBuffer = "d";
    }
    public void Button_Place_Remove()
    {
        MovesBuffer = "r";
    }
    public void Button_SplashScreenOff()                                            //Hide Splashscreen
    {
        FolderSplashScreen.SetActive(false);                                        //Hide the folder with all the splashscreen stuff
        LoadLevel(NextLevelName);                                                   //Load the next level
    }
}
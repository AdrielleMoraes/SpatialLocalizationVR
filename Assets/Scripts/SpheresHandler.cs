using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class _UnityEventGO : UnityEvent<GameObject> { }
public class SpheresHandler : MonoBehaviour {

    //test settings
    public GameObject infoPanel;
    public GameObject button;
    public GameObject tutorial;
    public int nSeriesTrials = 3; //number of series to be reproduced in total
    public bool visualFeedback = false;
    
    //which test will be conducted
    public enum TypeOfTest { MouseTest, HeadPointingTest, PointerPointingTest };
    public TypeOfTest TestType = TypeOfTest.MouseTest;

    //GET all sound sources
    GameObject[] soundObjects;// object with spheres
    AudioSource[] audioData;
    SoundSource sphere; //gameobject to be manipulated

    //Private variables
    private bool isPlayed; //track if the sound was reproduced and theres no input from the user yet 
    private int _lastPlayed = 0; //index of the last sphere
    private bool isTesting = false; //track if its the tutorial or testing phase
    private List<int> availablePos; //avalable positions for the next stimuli

    private DataTable soundDT;//Datatable to store stimuli locati
    private DateTime playTime;//store when the sound was triggered. 

    private int tutorialTrials = 0;// use this to indicate that the test has begun

    private bool feedbackRunning; //waiting for the user to confirm feedback


    //EYE TRACKING 
    private Tobii.Research.Unity.VRSaveData saveEyeTrackingData;


    // Use this for initialization
    void Start () {
        //Game settings
        GetSpheres();
        
        availablePos = new List<int>();

        GenerateDT();//Create a new datatable to store data from user

        RestartTrials(); //restart number of trials before testing

        InitializeEyeTracking();

        //start tutorial
        StartTrialButtonOnClick();
    }
    private void InitializeEyeTracking()
    {
        //Eye tracking
        saveEyeTrackingData = FindObjectOfType<Tobii.Research.Unity.VRSaveData>();
        // Start saving eye gaze data 
        try
        {
            saveEyeTrackingData.SaveData = true;
        }
        catch (Exception)
        {
            Debug.LogWarning("Not saving eye tracking data");
        }
        
    }

    private void GetSpheres()
    {
        soundObjects = GameObject.FindGameObjectsWithTag("SoundSource"); //get all sound sources available inside the scene
        audioData = soundObjects[0].GetComponents<AudioSource>();

        if (TestType == TypeOfTest.PointerPointingTest)
        {
            FindObjectOfType<Pointer>().laserEnabled = true;
        }
            foreach (var ss in soundObjects)
        {
            if (TestType == TypeOfTest.HeadPointingTest)
            {

                //add selection events
                ss.AddComponent<SelectWithGaze>();
                ss.GetComponent<SelectWithGaze>().InitializeEvent();
                ss.GetComponent<SelectWithGaze>().OnSelected.AddListener(SphereSelection);

                button.GetComponent<UITouchpadGazeButton>().enabled = true;
            }
            if (TestType == TypeOfTest.PointerPointingTest)
            {               
                //add selection events
                ss.AddComponent<SelectWithPointer>();
                ss.GetComponent<SelectWithPointer>().InitializeEvent();
                ss.GetComponent<SelectWithPointer>().OnSelected.AddListener(SphereSelection);

                button.GetComponent<SelectWithPointer>().enabled = true;           
            }
            if (TestType == TypeOfTest.MouseTest)
            {
                //add selection events
                ss.AddComponent<SelectWithMouse>();
                ss.GetComponent<SelectWithMouse>().InitializeEvent();
                ss.GetComponent<SelectWithMouse>().OnSelected.AddListener(SphereSelection);
            }
        }
    }


    #region Datatable Management
    //TODO: put data for head tracking or eye tracking (if needed)
    /// <summary>
    /// Function to generate a new DataTable and add its headers
    /// </summary>
    public void GenerateDT()
    {
        soundDT = new DataTable();
        //Add columns 
        soundDT.Columns.Add("Stimulus ID", typeof(int));
        soundDT.Columns.Add("Type of stimulus", typeof(string));
        soundDT.Columns.Add("Real Location X", typeof(double));
        soundDT.Columns.Add("Real Location Y", typeof(double));
        soundDT.Columns.Add("Real Location Z", typeof(double));
        soundDT.Columns.Add("User Location X", typeof(double));
        soundDT.Columns.Add("User Location Y", typeof(double));
        soundDT.Columns.Add("User Location Z", typeof(double));
        soundDT.Columns.Add("TimeStamp", typeof(long));
        soundDT.Columns.Add("Time (ms)", typeof(double));      
    }

    /// <summary>
    /// Add new variable to current database
    /// </summary>
    /// <param name="type"></param>
    /// <param name="realLocation"></param>
    /// <param name="userLocation"></param>
    private void DTAddData(string type, Vector3 realLocation, Vector3 userLocation,long timeStamp, double time)
    {
        soundDT.Rows.Add(soundDT.Rows.Count, type, Math.Round(realLocation.x, 3), Math.Round(realLocation.y, 3), Math.Round(realLocation.z, 3),
                         Math.Round(userLocation.x, 3), Math.Round(userLocation.y, 3), Math.Round(userLocation.z, 3),timeStamp, time);
    }

    /// <summary>
    /// Save datatable to .csv file
    /// </summary>
    private void SaveDTData()
    {
        //new variable with data converted to string
        StringBuilder sb = new StringBuilder();

        //get column names and join them together to insert into the .csv file
        string[] columnNames = soundDT.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
        sb.AppendLine(string.Join(",", columnNames));

        //append data from each dt row to the stringbuilder variable
        foreach (DataRow row in soundDT.Rows)
        {
            string[] fields = row.ItemArray.Select(field => string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\"")).ToArray();
            sb.AppendLine(string.Join(",", fields));
        }

        //write data to .csv file 
        long time = ToUnixTimestamp(GetTimeStamp());
        var fileName = string.Format("Data/Test_{0}.csv", time.ToString());
        File.WriteAllText(fileName, sb.ToString());
    }

    #endregion
    #region Clock
    [System.Security.SuppressUnmanagedCodeSecurity, System.Runtime.InteropServices.DllImport("kernel32.dll")]
    static extern void GetSystemTimePreciseAsFileTime(out FileTime pFileTime);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct FileTime
    {
        public const long FILETIME_TO_DATETIMETICKS = 504911232000000000;   // 146097 = days in 400 year Gregorian calendar cycle. 504911232000000000 = 4 * 146097 * 86400 * 1E7
        public uint TimeLow;    // least significant digits
        public uint TimeHigh;   // most sifnificant digits
        public long TimeStamp_FileTimeTicks { get { return TimeHigh * 4294967296 + TimeLow; } }     // ticks since 1-Jan-1601 (1 tick = 100 nanosecs). 4294967296 = 2^32
        public DateTime dateTime { get { return new DateTime(TimeStamp_FileTimeTicks + FILETIME_TO_DATETIMETICKS); } }
    }

    public static DateTime GetTimeStamp()
    {
        FileTime ft;
        GetSystemTimePreciseAsFileTime(out ft);
        return ft.dateTime;
    }
    #endregion
    #region Buttons
    /// <summary>
    /// Button to start the trials
    /// </summary>
    public void StartTrialButtonOnClick()
    {
        if (!isTesting)
        {
            //Start Tutorial
            PlayTutorial();
            return;
        }
        //start test phase
        StartCoroutine(ReproduceTrial());
    }

    //Function to handle events
    public void SphereSelection(GameObject G)
    {
        G.GetComponent<SoundSource>().StartFlick(false);
        // For the tutorial phase
        if (!isTesting)
        {
            sphere.StopFlickering();
            tutorialTrials++;
            tutorial.SetActive(false);           
            if (tutorialTrials == 3)
            {
                infoPanel.SetActive(true);
            }
            NextButtonOnClick();
            return;
        }

        //For the testing phase
        //this if checks if the sound source is currently reproducing a sound and no feedback is running
        if (isPlayed && !audioData[0].isPlaying && !feedbackRunning)
        {     
            CheckInput(G);

            //if the listener selected the wrong sphere
            if (G.name != sphere.name)
            {               
                //wait until the user selects a new sphere
                //todo:(change colour to blue(?))
                feedbackRunning = true;
                if (visualFeedback)
                {
                    //start animation
                    sphere.StartFlick(true);
                }
                else
                {
                    //play sound without loop
                    PlayAudio(_lastPlayed, true);
                }
            }
        }

        //go to the next stimulus once the correct sphere is selected and the feedback is running
        if(G.name == sphere.name)
        {
            //stop animation
            sphere.StopFlickering(); //for the visual feedback
            StopAudio();//for the audio feedback
            feedbackRunning = false;

            //Go to next stimulus
            NextButtonOnClick();
        }       
        
    }

    /// <summary>
    /// Go to next stimulus
    /// </summary>
    private void NextButtonOnClick()
    {
        //if we are doing the tutorial part
        if (!isTesting)
        {
            PlayTutorial();
            return;
        }
        //play audio
        StartCoroutine(ReproduceTrial());        
    }

    public void StopTutorialOnClick()
    {
        //hide panel with instructions
        infoPanel.SetActive(false);

        sphere.StopFlickering();

        //start testing
        isTesting = true;
        StartTrialButtonOnClick();
    }

    #endregion

    /// <summary>
    /// Function to blink a random sphere 
    /// </summary>
    private void PlayTutorial()
    {
        //get a random sphere
        isPlayed = true; 
        var random = new System.Random();
        int index = random.Next(soundObjects.Length);
        if (tutorialTrials == 0)
        {
            index = 0;
        }
        //trigger the sphere for it to start blinking
        sphere = (SoundSource)soundObjects[index].GetComponent(typeof(SoundSource));
        sphere.StartFlick(true);
    }

    /// <summary>
    /// Restart array with containing positions to play stimuli
    /// </summary>
    private void RestartTrials()
    {
        availablePos = new List<int>();
        availablePos.Clear();

        //set list with possible values
        for (int i = 0; i < soundObjects.Length; i++)
        {
            availablePos.Add(i);
        }

        nSeriesTrials--;
    }

    /// <summary>
    /// Check if the input from the user represents a valid location in space
    /// </summary>
    public void CheckInput(GameObject selection)
    {
        isPlayed = false; 

        //for the testing phase
        if (isTesting)
        {
            //get time to complete task
            TimeSpan interval = DateTime.Now - playTime;
            DateTime date = DateTime.Now;
            string usec = date.ToString("ffffff");
            long time = ToUnixTimestamp(date);

            //add data to datatable
            DTAddData(string.Format("{0}", _lastPlayed), soundObjects[_lastPlayed].transform.position, selection.transform.position, time, interval.TotalMilliseconds);
        }

        //finish if the number of series is equal to 0
        if (nSeriesTrials < 0)
        {
            //display message
            infoPanel.gameObject.SetActive(true);

            // save any game data here
            saveEyeTrackingData.SaveData = false;

            isPlayed = false;

            //exit app
            UnityEditor.EditorApplication.isPlaying = false;
        }        
    }

    public static long ToUnixTimestamp(DateTime d)
    {
        var epoch = d - new DateTime(1970, 1, 1, 0, 0, 0);
        string usec = d.ToString("ffffff");
        long t = (long)epoch.TotalMilliseconds * 1000 + Convert.ToInt32(usec);
        return t;
    }

    /// <summary>
    /// Function to wait a random value between 0.5 and 1 sec before reproducing the audio file
    /// </summary>
    /// <returns></returns>
    IEnumerator ReproduceTrial()
    {
        Debug.Log("Reproducing Audio");
        yield return new WaitForSeconds(0.3f);
        //get a new index
        var random = new System.Random();

        int next = 0;

        if (availablePos.Count == soundObjects.Length) //first time
        {
            //get a random position 
            _lastPlayed = random.Next(soundObjects.Length - 1);
            next = _lastPlayed;
        }
        //if its not the first time
        else
        {
            List<int> choices = new List<int> { 0, 1, 2 };
            while (true)
            {
                //get a random position for the three possibilities : i-1, i+1, opposite
                int i = random.Next(choices.Count);
                int choice = choices[i];
                choices.Remove(choice);
                switch (choice)
                {
                    //i-1 side
                    case 0:
                        next = _lastPlayed == 0 ? soundObjects.Length - 1 : _lastPlayed - 1;
                        break;

                    //i+1 side
                    case 1:
                        next = _lastPlayed == soundObjects.Length - 1 ? 0 : _lastPlayed + 1;
                        break;

                    //opposite side
                    case 2:
                        //1st or 2nd quadrant
                        if (_lastPlayed < soundObjects.Length/2)
                        {
                            //1st
                            if (_lastPlayed < soundObjects.Length / 4)
                            {
                                next = random.Next(soundObjects.Length * 3 / 4,soundObjects.Length - 1 );
                            }
                            //2nd
                            else
                            {
                                next = random.Next(soundObjects.Length / 2, soundObjects.Length * 3 / 4 - 1);
                            }
                        }
                        //3rd or 4th quadrant
                        else
                        {
                            //3rd
                            if (_lastPlayed < soundObjects.Length * 3 / 4)
                            {
                                next = random.Next(soundObjects.Length / 4, soundObjects.Length / 2 - 1);
                            }
                            //4th
                            else
                            {
                                next = random.Next(0, soundObjects.Length / 4 - 1);
                            }
                        }
                        break;
                }
                //loop until find available pos
                if (availablePos.Contains(next))
                {
                    break;
                }

                if (choices.Count == 0)
                {
                    //make a copy of the list
                    List<int> copy = availablePos;
                    //remove current index
                    copy.Remove(_lastPlayed);

                    int index = random.Next(copy.Count); //get a new value in array
                    next = copy[index];
                    break;
                }              
            }   
        }

        //remove actual
        availablePos.Remove(_lastPlayed);
        _lastPlayed = next;

        //restart trials if there's no other sphere left
        if (availablePos.Count == 1 && nSeriesTrials >= 0)
        {
            RestartTrials();
            Debug.Log("Restarting trials");
        }
        //update sphere
        sphere = (SoundSource)soundObjects[_lastPlayed].GetComponent(typeof(SoundSource));
        //play sound without loop
        PlayAudio(_lastPlayed, false);

        Debug.Log("Source: " + sphere.name);

        //start to count the user reaction time
        playTime = DateTime.Now;
        isPlayed = true;
    }

    //play audio according to the test phase
    private void PlayAudio(int source, bool loop)
    {
        //get audio data from source    
        audioData = soundObjects[source].GetComponents<AudioSource>();

        //set loop property
        audioData[0].loop = loop;

        //Stop any audio playing
        audioData[0].Stop();

        audioData[0].Play(0); //which audiofile will be played

        //Reproduce distractors (if applicable)
        if (nSeriesTrials == 1)
        {
            //0 degrees           
            audioData = soundObjects[source].GetComponents<AudioSource>();
            //set loop property
            audioData[1].loop = loop;

            //Stop any audio playing
            audioData[1].Stop();

            audioData[1].Play(0); //which audiofile will be played
        }

        if (nSeriesTrials == 0)
        {
            //the step is defined as 90 degrees apart from the target
            int step = 90 * soundObjects.Length / 360;

            //+90 degrees
            audioData = soundObjects[(source + step) % soundObjects.Length].GetComponents<AudioSource>();
            //set loop property
            audioData[1].loop = loop;

            //Stop any audio playing
            audioData[1].Stop();

            audioData[1].Play(0); //which audiofile will be played

            //-90 degrees
            audioData = soundObjects[(source - step + soundObjects.Length) % soundObjects.Length].GetComponents<AudioSource>();
            //set loop property
            audioData[1].loop = loop;

            //Stop any audio playing
            audioData[1].Stop();
            audioData[1].Play(0); //which audiofile will be played
        }
    }


    //stop all audio from playing
    private void StopAudio()
    {
        audioData[0].Stop();
        audioData[1].Stop();
    }

    private void OnApplicationQuit()
    {        
        SaveDTData();
    }
}

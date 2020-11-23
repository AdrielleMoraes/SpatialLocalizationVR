using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;

[RequireComponent(typeof(LineRenderer))]
public class ReadEyeTrackingFile : MonoBehaviour {

    private LineRenderer _lineRenderer;
    public int raySize = 20;

    public GameObject endPoint_prefab; //the child game object

    [HideInInspector]
    public Vector3 hitPoint;

    public Transform eye;

    string[] previousValue;
    int pointer = 0;
    List<GazeData> data;
    List<Vector3> collisionPoints;
    GazeData gazeData;

    /// <summary>
    /// Use this to classify data columns
    /// </summary>
    enum ColumnsList
    {
        TimeStamp = 1,
        HeadPosX = 2,
        HeadPosY = 3,
        HeadPosZ = 4,
        HeadRotX = 5,
        HeadRotY = 6,
        HeadRotZ = 7,
        HeadRotW = 8,
        HeadDirX = 9,
        HeadDirY = 10,
        HeadDirZ = 11,
        GazeOriX = 12,
        GazeOriY = 13,
        GazeOriZ = 14,
        GazeDirX = 15,
        GazeDirY = 16,
        GazeDirZ = 17,
        RWGazeOriX = 18,
        RWGazeOriY = 19,
        RWGazeOriZ = 20,
        RWGazeDirX = 21,
        RWGazeDirY = 22,
        RWGazeDirZ = 23,
        Pupil = 24
    }

    private string[] match_selections = new string[] { "1575542229268596", "1575542237133555", "1575542245731689", "1575542252205284", "1575542260478488" };
    int resetValue = 16000;
    int matchi = 0;

    // Use this for initialization
    void Start()
    {
        //save collision points
        collisionPoints = new List<Vector3>();
        //get line renderer component
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        ReadFile();
    }

    // Update is called once per frame
    void Update()
    {
        if (pointer < data.Count)
        {
            //head pose
            transform.position = data.ElementAt(pointer).headPose.position;
            transform.rotation = data.ElementAt(pointer).headPose.rotation;

            //eye data
            eye.localPosition = data.ElementAt(pointer).eye.rayWorldOrigin;
            //raycast
            CreateRaycast(raySize, eye.position, data.ElementAt(pointer).eye.rayWorldDirection);

            pointer++;

            if (pointer >= data.Count)
            {
                //save data
                WriteData();

                Debug.Log("Saved...");
            }
        }
    }

    private void ReadFile()
    {
        data = new List<GazeData>();
        

        string path = EditorUtility.OpenFilePanel("Choose file", "", "csv");
        if (path.Length != 0)
        {
            string selectedFileName = path;
            using (var reader = new StreamReader(selectedFileName))
            {
                //ignore first line (headers)
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    //head pose
                    Vector3 head_pos = new Vector3(
                        float.Parse(values[(int)ColumnsList.HeadPosX]),
                        float.Parse(values[(int)ColumnsList.HeadPosY]),
                        float.Parse(values[(int)ColumnsList.HeadPosZ]));

                    Quaternion head_rot = new Quaternion(
                        float.Parse(values[(int)ColumnsList.HeadRotX]),
                        float.Parse(values[(int)ColumnsList.HeadRotY]),
                        float.Parse(values[(int)ColumnsList.HeadRotZ]),
                        float.Parse(values[(int)ColumnsList.HeadRotW]));

                    Vector3 head_dir = new Vector3(
                        float.Parse(values[(int)ColumnsList.HeadDirX]),
                        float.Parse(values[(int)ColumnsList.HeadDirY]),
                        float.Parse(values[(int)ColumnsList.HeadDirZ]));

                    //eye
                    Vector3 eye_origin = new Vector3(
                        float.Parse(values[(int)ColumnsList.GazeOriX]),
                        float.Parse(values[(int)ColumnsList.GazeOriY]),
                        float.Parse(values[(int)ColumnsList.GazeOriZ]));

                    Vector3 eye_dir = new Vector3(
                        float.Parse(values[(int)ColumnsList.GazeDirX]),
                        float.Parse(values[(int)ColumnsList.GazeDirY]),
                        float.Parse(values[(int)ColumnsList.GazeDirZ]));

                    Vector3 RWeye_origin = new Vector3(
                        float.Parse(values[(int)ColumnsList.RWGazeOriX]),
                        float.Parse(values[(int)ColumnsList.RWGazeOriY]),
                        float.Parse(values[(int)ColumnsList.RWGazeOriZ])); ;

                    Vector3 RWeye_dir = new Vector3(
                        float.Parse(values[(int)ColumnsList.RWGazeDirX]),
                        float.Parse(values[(int)ColumnsList.RWGazeDirY]),
                        float.Parse(values[(int)ColumnsList.RWGazeDirZ]));

                    //add data to list
                    gazeData = new GazeData();

                    gazeData.timestamp = values[(int)ColumnsList.TimeStamp];
                    gazeData.headPose.position = head_pos;
                    gazeData.headPose.rotation = head_rot;
                    gazeData.headPose.direction = head_dir;

                    gazeData.eye.origin = eye_origin;
                    gazeData.eye.direction = eye_dir;

                    gazeData.eye.rayWorldOrigin = RWeye_origin;
                    gazeData.eye.rayWorldDirection = RWeye_dir;

                    data.Add(gazeData);
                }
                Debug.Log("Finish all reading...");
            }
        }
        else
        {
            Debug.Log("Quit");
        }
    }

    /// Create a raycast and a laser pointer
    /// </summary>
    /// <param name="defaultLenght">Default lenght for the pointer when it doesnt hit anything</param>
    private void CreateRaycast(int defaultLenght, Vector3 pos, Vector3 dir)
    {
        //default end if we dont hit anything
        Vector3 endPosition = pos + (dir * defaultLenght);

        RaycastHit hit;
        //create a ray going from the gameobject and forward
        Ray ray = new Ray(pos, dir);

        if (Physics.Raycast(ray, out hit))
        {
            endPosition = hit.point;
            //make dot visible
            endPoint_prefab.SetActive(true);
            endPoint_prefab.transform.position = endPosition;
            collisionPoints.Add(endPosition);
        }
        else
        {
            endPoint_prefab.SetActive(false);
            collisionPoints.Add(endPosition);
        }
            
        //set the position of the line renderer
        _lineRenderer.SetPosition(0, pos);
        _lineRenderer.SetPosition(1, endPosition);

    }

    private void WriteData()
    {
        //get path to save
        string path = EditorUtility.OpenFolderPanel("Select a folder", "Open", "Open folder" );
        // Write each directory name to a file.
        using (StreamWriter sw = new StreamWriter(path+"/collisions.csv"))
        {
            sw.WriteLine("collision_x,collision_y,collision_z");
            for (int i = 0; i < collisionPoints.Count; i++)
            {
                float cy = collisionPoints.ElementAt(i).y < 0 ? 0 : collisionPoints.ElementAt(i).y;
                string points = collisionPoints.ElementAt(i).x.ToString() + "," + cy.ToString() + "," + collisionPoints.ElementAt(i).z.ToString();
                sw.WriteLine(points);
            }
        }
    }
}

public class GazeData: MonoBehaviour
{
    public struct HeadPose
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 direction;

        public HeadPose(Vector3 _position, Quaternion _rotation, Vector3 _direction)
        {
            position = _position;
            rotation = _rotation;
            direction = _direction;
        }
    }
    public struct EyeData
    {
        public float pupilDiameter;
        public Vector3 origin;
        public Vector3 direction;
        public Vector3 rayWorldOrigin;
        public Vector3 rayWorldDirection;

        public EyeData(float _pupilDiameter, Vector3 _origin, Vector3 _direction, Vector3 _rayWorldOrigin, Vector3 _rayWorldDirection)
        {
            pupilDiameter = _pupilDiameter;
            origin = _origin;
            direction = _direction;
            rayWorldOrigin = _rayWorldOrigin;
            rayWorldDirection = _rayWorldDirection;
        }

    }

    public string timestamp;
    public EyeData eye;
    public HeadPose headPose;

    public GazeData()
    {
        
    }
}

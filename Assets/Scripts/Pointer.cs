using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.VR;

/// <summary>
/// Pointer class used to create a laser beam with trigger and to save clicks to a .csv file
/// </summary>
public class Pointer : MonoBehaviour {

    private DataTable clickDT;

    public GameObject dot; //the child game object
    public bool laserEnabled = true;

    private readonly int maxLength = 13;

    //create laser
    private LineRenderer _lineRenderer;
    [HideInInspector]
    public Vector3 hitPoint;

    private void Start()
    {
        CreateDataTable();
        //for the trigger
        _lineRenderer = GetComponent<LineRenderer>();       
    }

    private void CreateDataTable()
    {
        clickDT = new DataTable();
        clickDT.Columns.Add("Click Location X", typeof(double));
        clickDT.Columns.Add("Click Location Y", typeof(double));
        clickDT.Columns.Add("Click Location Z", typeof(double));
        clickDT.Columns.Add("TimeStamp", typeof(long));
    }

    // Update is called once per frame
    void Update () {

        //if pressing the button 
        if (SteamVR_Actions._default.Teleport.GetStateUp(SteamVR_Input_Sources.Any))
        {
            //save click
            clickDT.Rows.Add(Math.Round(dot.transform.position.x, 3), Math.Round(dot.transform.position.y, 3), Math.Round(dot.transform.position.z, 3), ToUnixTimestamp(GetTimeStamp()));
        }

        //for the trigger
        if (laserEnabled)
        {
            GenerateLaser();
        }
    }

    private void GenerateLaser()
    {
        //action corresponding to pressing the trigger
        float triggerValue = SteamVR_Actions._default.Squeeze.GetAxis(SteamVR_Input_Sources.Any);
        //if pressing the trigger draw the laser to the spheres
        if (triggerValue > 0)
        {
            //do raycast
            CreateRaycast(maxLength);
        }
        //not pressing the button, so disable laser point
        else
        {
            _lineRenderer.SetPosition(1, transform.position);
            _lineRenderer.SetPosition(0, transform.position);
            dot.SetActive(false);
        }
    }

    /// <summary>
    /// Create a raycast and a laser pointer
    /// </summary>
    /// <param name="defaultLenght">Default lenght for the pointer when it doesnt hit anything</param>
    private void CreateRaycast(int defaultLenght)
    {
        //default end if we dont hit anything
        Vector3 endPosition = transform.position + (transform.forward * defaultLenght);

        RaycastHit hit;
        //create a ray going from the gameobject and forward
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit))
        {
            endPosition = hit.point;
            //make dot visible
            dot.SetActive(true);
            dot.transform.position = endPosition;
        }
        else
            dot.SetActive(false);
        //set the position of the line renderer
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, endPosition);
    }

    /// <summary>
    /// Save datatable to .csv file
    /// </summary>
    private void SaveDTData()
    {
        //new variable with data converted to string
        StringBuilder sb = new StringBuilder();

        //get column names and join them together to insert into the .csv file
        string[] columnNames = clickDT.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
        sb.AppendLine(string.Join(",", columnNames));

        //append data from each dt row to the stringbuilder variable
        foreach (DataRow row in clickDT.Rows)
        {
            string[] fields = row.ItemArray.Select(field => string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\"")).ToArray();
            sb.AppendLine(string.Join(",", fields));
        }

        //write data to .csv file 
        long time = ToUnixTimestamp(GetTimeStamp());
        var fileName = string.Format("Data/Clicks_{0}.csv", time.ToString());
        File.WriteAllText(fileName, sb.ToString());
    }

    #region clock
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

    public static long ToUnixTimestamp(DateTime d)
    {
        var epoch = d - new DateTime(1970, 1, 1, 0, 0, 0);
        string usec = d.ToString("ffffff");
        long t = (long)epoch.TotalMilliseconds * 1000 + Convert.ToInt32(usec);
        return t;
    }
    #endregion



    private void OnApplicationQuit()
    {
        SaveDTData();
    }

}

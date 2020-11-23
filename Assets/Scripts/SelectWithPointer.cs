using UnityEngine;
using Valve.VR;

public class SelectWithPointer : MonoBehaviour {

    // Event called when the button is clicked.
    public UIButtonEvent OnSelected;

    private bool collisionDetected = false;

    public void InitializeEvent()
    {
        // Initialize click event.
        if (OnSelected == null)
        {
            OnSelected = new UIButtonEvent();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SteamVR_Actions._default.Squeeze.GetAxis(SteamVR_Input_Sources.Any) <= 0)
        {
            collisionDetected = false;
        }
        if (collisionDetected)
        {     
            if (SteamVR_Actions._default.Teleport.GetStateDown(SteamVR_Input_Sources.Any))
            {
                collisionDetected = false;
                Debug.Log("click");
                //Invoke click event.
                if (OnSelected != null)
                {
                    OnSelected.Invoke(gameObject);                    
                }
            }
            if (GetComponent<SoundSource>() != null)
            {
                //change color
                GetComponent<SoundSource>().ChangeColor();
            }
        }
        else
        {
            if (GetComponent<SoundSource>() != null)
                GetComponent<SoundSource>().SetOriginalColor();
        }       
    }

    private void OnTriggerEnter(Collider col)
    {
        collisionDetected = true;
    }

    private void OnTriggerExit(Collider col)
    {
        collisionDetected = false;
    }

    private void OnTriggerStay(Collider other)
    {
        collisionDetected = true;
    }
}

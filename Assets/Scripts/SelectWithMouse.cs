using UnityEngine;

public class SelectWithMouse : MonoBehaviour {

    // Event called when the button is clicked.
    public UIButtonEvent OnSelected;

    //Selection
    private SoundSource _sphere;

    public void InitializeEvent()
    {
        // Initialize click event.
        if (OnSelected == null)
        {
            OnSelected = new UIButtonEvent();
        }
    }
	
	// Update is called once per frame
	void Update () {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            //if the user inputs is the same as the 
            if (hit.collider.gameObject.name == gameObject.name )
            {
                Debug.Log("Selected: " + gameObject.name);
                //update selection 
                gameObject.GetComponent<SoundSource>().ChangeColor();

                //fire event
                if (OnSelected != null && Input.GetButtonDown("Fire1"))
                {
                    OnSelected.Invoke(gameObject);
                }
            }        
        }
        else
        {
            //set original color when exiting the gameobject
            gameObject.GetComponent<SoundSource>().SetOriginalColor();
        }
    }
}

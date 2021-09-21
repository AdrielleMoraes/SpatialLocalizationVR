using UnityEngine;

public class SelectWithMouse : MonoBehaviour {

	// Update is called once per frame
	void Update () {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            //if the user inputs is the same as the 
            if (hit.collider.gameObject.name == gameObject.name )
            {
                //update selection 
                gameObject.GetComponent<SoundSource>().ChangeColor();

                //fire event
                if (Input.GetButtonDown("Fire1"))
                {
                    EventManager.TriggerEvent("Selection", gameObject);
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

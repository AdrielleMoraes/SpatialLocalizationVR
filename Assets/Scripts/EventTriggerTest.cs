using UnityEngine;
using System.Collections;

public class EventTriggerTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            EventManager.TriggerEvent("test", gameObject);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            //if the user inputs is the same as the 
            if (hit.collider.gameObject.name == gameObject.name)
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class SphereEvent : UnityEvent<GameObject>
{
}
public class EventManager : MonoBehaviour
{
    private Dictionary<string, SphereEvent> eventDictionary;

    private static EventManager eventManager;

    public static EventManager instance
    {
        get
        {
            // look for a event manager instance. The scene only starts if there's one atached to a gameobject
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                {
                    Debug.LogError("there needs to be one active EventManager script on a Gameobject in your scene");
                }
                else
                {
                    eventManager.Init();
                }
            }
            return eventManager;
        }
    }

    void  Init()
    {
        if(eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, SphereEvent>();
        }
    }
   
    public static void StartListening(string eventName, UnityAction<GameObject> listener)
    {
        SphereEvent thisEvent = null;
        if(instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new SphereEvent();
            thisEvent.AddListener(listener);
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction<GameObject> listener)
    {
        if (eventManager == null)
            return;
        SphereEvent thisEvent = null;
        if(instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(string eventName, GameObject sphereObject)
    {
        SphereEvent thisEvent = null;
        if(instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(sphereObject);
        }
    }
}

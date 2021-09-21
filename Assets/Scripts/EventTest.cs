using UnityEngine;
using UnityEngine.Events;

public class EventTest : MonoBehaviour
{

    private UnityAction<GameObject> someListener;

    void Awake()
    {
        someListener = new UnityAction<GameObject>(OnSelection);
    }

    void OnEnable()
    {
        EventManager.StartListening("test", someListener);
        EventManager.StartListening("Selection", OnSelection);

    }

    void OnDisable()
    {
        EventManager.StopListening("test", someListener);
        EventManager.StopListening("Selection", OnSelection);
    }

    void OnSelection(GameObject sphere)
    {
        gameObject.GetComponent<SpheresManager>().SphereSelection(sphere);
    }

}
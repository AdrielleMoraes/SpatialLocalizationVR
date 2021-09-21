using System.Collections;
using UnityEngine;


public class SoundSource : MonoBehaviour
{

    Color HighlightColor = Color.green;
    public float AnimationTime = 0.1f;

    private Renderer _renderer;
    private Color _originalColor;
    private Color _targetColor;

    private bool blinkSphere = true;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.material.color;
        _targetColor = _originalColor;
    }

    public void StartFlick(bool loop)
    {
        //start blinking coroutine
        if (loop)
        {
            blinkSphere = true;
        }
        else
        {
            blinkSphere = false;
        }
        StartCoroutine(Blink());

    }
    private IEnumerator Blink()
    {
        //object blinks
        while (true)
        {
            for (int i = 0; i < 2; i++)
            {
                gameObject.GetComponent<Renderer>().enabled = false;
                yield return new WaitForSeconds(0.2f);
                gameObject.GetComponent<Renderer>().enabled = true;
                yield return new WaitForSeconds(0.2f);
            }
            if (blinkSphere == false)
            {
                break;
            }
        }
    }

    public void StopFlickering()
    {
        blinkSphere = false;
    }

    private void Update()
    {
        _renderer.material.color = Color.Lerp(_renderer.material.color, _targetColor, Time.deltaTime * (1 / AnimationTime));
    }

    public void ChangeColor()
    {
        _targetColor = HighlightColor;
    }

    public void SetOriginalColor()
    {
        _targetColor = _originalColor;
    }

}

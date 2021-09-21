using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// A gaze aware button that is interacted with the touchpad button on the Vive controller.
/// </summary>
public class SelectWithGaze : MonoBehaviour
{
    private SoundSource _sphere;

    public void InitializeEvent()
    {
        // Initialize click event.

    }

    private void Start()
    {
        _sphere = GetComponent<SoundSource>();
    }

    private void Update()
    {

    }
}

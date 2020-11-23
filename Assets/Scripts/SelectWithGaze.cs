using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.G2OM;
using Valve.VR;
using System;

/// <summary>
/// A gaze aware button that is interacted with the touchpad button on the Vive controller.
/// </summary>
/// //Monobehaviour which implements the "IGazeFocusable" interface, meaning it will be called on when the object receives focus
public class SelectWithGaze : MonoBehaviour, IGazeFocusable
{
    // Event called when the button is clicked.
    public UIButtonEvent OnSelected;

    private SoundSource _sphere;


    // The state the button is currently  in.
    private ButtonState _currentButtonState = ButtonState.Idle;

    // Private fields.
    private bool _hasFocus;


    public void InitializeEvent()
    {
        // Initialize click event.
        if (OnSelected == null)
        {
            OnSelected = new UIButtonEvent();
        }
    }

    private void Start()
    {
        _sphere = GetComponent<SoundSource>();
    }

    private void Update()
    {
        // When the button is being focused and the interaction button is pressed down, set the button to the PressedDown state.
        if (_currentButtonState == ButtonState.Focused && SteamVR_Actions._default.Teleport.GetStateDown(SteamVR_Input_Sources.Any))
        {
            UpdateState(ButtonState.PressedDown);
        }
        // When the button is pressed down and the interaction button is released, call the click method and update the state.
        else if (_currentButtonState == ButtonState.PressedDown && SteamVR_Actions._default.Teleport.GetStateUp(SteamVR_Input_Sources.Any))
        {
            // Invoke click event.
            if (OnSelected != null)
            {
                OnSelected.Invoke(gameObject);               
            }
            // Set the state depending on if it has focus or not.
            UpdateState(_hasFocus ? ButtonState.Focused : ButtonState.Idle);
        }
    }

    /// <summary>
    /// Updates the button state and starts an animation of the button.
    /// </summary>
    /// <param name="newState">The state the button should transition to.</param>
    private void UpdateState(ButtonState newState)
    {
        var oldState = _currentButtonState;
        _currentButtonState = newState;

        // Variables for when the button is pressed or clicked.
        var buttonPressed = newState == ButtonState.PressedDown;
        var buttonClicked = (oldState == ButtonState.PressedDown && newState == ButtonState.Focused);
    }

    /// <summary>
    /// Method called by Tobii XR when the gaze focus changes by implementing <see cref="IGazeFocusable"/>.
    /// </summary>
    /// <param name="hasFocus"></param>
    public void GazeFocusChanged(bool hasFocus)
    {
        // Don't use this method if the component is disabled.
        if (!enabled) return;

        _hasFocus = hasFocus;

        //If this object received focus, fade the object's color to highlight color
        if (hasFocus)
        {
            _sphere.ChangeColor();
            UpdateState(ButtonState.Focused);
        }
        //If this object lost focus, fade the object's color to it's original color
        else
        {
            _sphere.SetOriginalColor();
            UpdateState(ButtonState.Idle);
        }
    }
}

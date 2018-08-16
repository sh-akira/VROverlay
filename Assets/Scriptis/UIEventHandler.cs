using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using sh_akira.VROverlay;

public class UIEventHandler : MonoBehaviour
{

    public VROverlay VrOverlay;
    public VROverlay GuiVrOverlay;
    public VRGUIOverlayCursor VrGuiOverlayCursor;

    public Transform GlobalPosition;
    public Transform LocalPosition;
    public Transform ControllerPosition;

    public void GlobalButton_Click()
    {
        Debug.Log("GlobalButton_Click");

        VrOverlay.RenderPosition = GlobalPosition;
        VrOverlay.WidthInMeters = 1.0f;
        VrOverlay.PositionOffset = new Vector3(0.1f, 0f, 0f);
        GuiVrOverlay.RenderPosition = GlobalPosition;
        GuiVrOverlay.WidthInMeters = 0.16875f;
        GuiVrOverlay.PositionOffset = new Vector3(-0.59f, 0f, 0f);
        VrGuiOverlayCursor.WidthInMeters = 0.1f;

    }

    public void LocalButton_Click()
    {
        Debug.Log("LocalButton_Click");

        VrOverlay.RenderPosition = LocalPosition;
        VrOverlay.WidthInMeters = 1.0f;
        VrOverlay.PositionOffset = new Vector3(0.1f, 0f, 0f);
        GuiVrOverlay.RenderPosition = LocalPosition;
        GuiVrOverlay.WidthInMeters = 0.16875f;
        GuiVrOverlay.PositionOffset = new Vector3(-0.59f, 0f, 0f);
        VrGuiOverlayCursor.WidthInMeters = 0.1f;
    }

    public void ControllerButton_Click()
    {
        Debug.Log("ControllerButton_Click");

        VrOverlay.RenderPosition = ControllerPosition;
        VrOverlay.WidthInMeters = 0.25f;
        VrOverlay.PositionOffset = new Vector3(0.025f, 0f, 0f);
        GuiVrOverlay.RenderPosition = ControllerPosition;
        GuiVrOverlay.WidthInMeters = 0.0421875f;
        GuiVrOverlay.PositionOffset = new Vector3(-0.1475f, 0f, 0f);
        VrGuiOverlayCursor.WidthInMeters = 0.025f;
    }
}

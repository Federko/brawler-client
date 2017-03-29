using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraActionState : ICameraState
{

    public CameraActionState(ActionCamera camera)
    {
        this.ActionCamera = camera;
    }

    public ActionCamera ActionCamera
    {
        get; set;
    }

    private float newDistance { get; set; }
    private float HorizontalCameraSpeed { get { return ActionCamera.actionHorizntalCameraSpeed; } }
    private float VerticalCameraSpeed { get { return ActionCamera.actionVerticalCameraSpeed; } }
    private float MinOrbitY { get { return ActionCamera.actionMinOrbitY; } }
    private float MaxOrbitY { get { return ActionCamera.actionMaxOrbitY; } }
    private float ActionDistance { get { return ActionCamera.actionDistance; } }
    private float ActionLerpSpeed { get { return ActionCamera.actionLerpSpeed; } }
    private float ActionHeight { get { return ActionCamera.actionHeight; } }
    private Transform Transform { get { return ActionCamera.transform; } }
    private Transform Target { get { return ActionCamera.target; } }
    private Repositioning Repositioning { get { return ActionCamera.repositioning; } }


    public void Enter()
    {
        newDistance = ActionDistance;

        //update orbits with new camera position
        ActionCamera.orbitX = ActionCamera.transform.rotation.eulerAngles.y;
        ActionCamera.orbitY = ActionCamera.transform.rotation.eulerAngles.x;

    }

    public void Exit()
    {
    }

    public void Update()
    {
        float lookX = FixValue(Input.GetAxis("4th"));
        float lookY = FixValue(Input.GetAxis("5th")) * (ActionCamera.invertCameraY ? -1 : 1);
        //Debug.Log(lookX);
        //Debug.Log(lookY);

        ActionCamera.orbitX += lookX * HorizontalCameraSpeed * Time.deltaTime;
        ActionCamera.orbitY += lookY * VerticalCameraSpeed * Time.deltaTime;
        ActionCamera.orbitY = ClampAngle(ActionCamera.orbitY);

        Quaternion rotation = Quaternion.Euler(ActionCamera.orbitY, ActionCamera.orbitX, 0);

        Transform.rotation = rotation;

        Vector3 position = rotation * new Vector3(0, ActionCamera.actionHeight, ActionDistance) + Target.position;

        if (Repositioning.active)
        {
            RaycastHit hit;
            if (Physics.Linecast(Target.position, position, out hit, ~(LayerMask.GetMask(Repositioning.ignoredLayers))))
            {
                newDistance = -hit.distance;
                Camera.main.nearClipPlane = Repositioning.onRepositionNearClipPlane;
            }
            else
            {
                newDistance = Mathf.Lerp(newDistance, ActionDistance, ActionLerpSpeed * Time.deltaTime);
                Camera.main.nearClipPlane = Repositioning.onNormalNearClipPlane;
            }
        }
        Transform.position = Vector3.Lerp(Transform.position, rotation * new Vector3(0, ActionHeight, newDistance) + Target.position, Time.deltaTime * 19f);

        if (!ActionCamera.ManageInputFromOtherScript)
        {
            //must be removed, use ActionCamera methods for switching states.
            if (Input.GetKeyDown(KeyCode.M))
            {
                ActionCamera.Lock();
            }
        }
    }


    /// <summary>
    /// If value is between max and min, ignore itself and return 0, else return real value.
    /// </summary>
    /// <param name="value">Value that be fixed</param>
    /// <param name="max"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    public float FixValue(float value, float max = 0.2f, float min = -0.2f)
    {
        if (value < max && value > min)
            return 0;
        return value;
    }

    private float ClampAngle(float angle)
    {
        if (angle < MinOrbitY)
            return MinOrbitY;
        if (angle > MaxOrbitY)
            return MaxOrbitY;
        return angle;
    }

    
}

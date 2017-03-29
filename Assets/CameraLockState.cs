using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLockState : ICameraState
{

    public CameraLockState(ActionCamera camera)
    {
        ActionCamera = camera;
    }
    public ActionCamera ActionCamera
    {
        get; set;
    }

    private float newDistance { get; set; }
    private float LockDistance { get { return ActionCamera.lockDistance; } }
    private float LockMaxDistanceFromTarget { get { return ActionCamera.lockMaxDistanceFromTarget; } }
    private float LockHeight { get { return ActionCamera.lockHeight; } }
    private float LockLerpSpeed { get { return ActionCamera.lockLerpSpeed; } }
    private float LockWidthOfSight { get { return ActionCamera.lockWidthOfSight; } }
    private float LockSight { get { return ActionCamera.lockSight; } }
    private Transform Transform { get { return ActionCamera.transform; } }
    private Transform Target { get { return ActionCamera.target; } }
    private Repositioning Repositioning { get { return ActionCamera.repositioning; } }

    public ITargettable focusedTarget { get; set; }

    public void Enter()
    {
        newDistance = LockDistance;
    }

    public void Exit()
    {
        focusedTarget = null;
    }

    public void Update()
    {

        //if is too far away return in default state
        if (Vector3.Distance(focusedTarget.Transform.position, Transform.position) > LockMaxDistanceFromTarget)
        {
            focusedTarget.OnTargetLosed();
            ActionCamera.Unlock();
            return;
        }

        Vector3 newFor = (focusedTarget.Transform.position - Transform.position).normalized;

        Transform.rotation = Quaternion.LookRotation(Vector3.Lerp(Transform.forward, newFor, Time.deltaTime * 8f));


        Vector3 position = Transform.rotation * new Vector3(0, LockHeight, LockDistance) + Target.position;
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
                newDistance = Mathf.Lerp(newDistance, LockDistance, LockLerpSpeed * Time.deltaTime);
                Camera.main.nearClipPlane = Repositioning.onNormalNearClipPlane;
            }

        }
        position = Transform.rotation * new Vector3(0, LockHeight, newDistance) + Target.position;
        position.y = LockHeight;
        Transform.position = Vector3.Lerp(Transform.position, position, Time.deltaTime * 10f);

        #region TO DELETE
        //must be removed, use ActionCamera methods for switching states.
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            focusedTarget.OnTargetReleased();
            ActionCamera.Unlock();
        }


        if (Input.GetKeyDown(KeyCode.Z))
        {
            ITargettable t = ActionCamera.GetTargetFromSide(-30);
            if (t != null)
                focusedTarget = t;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            ITargettable t = ActionCamera.GetTargetFromSide(30);
            if (t != null)
                focusedTarget = t;
        }
        #endregion
    }






}

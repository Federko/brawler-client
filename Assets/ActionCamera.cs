using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionCamera : MonoBehaviour
{
    [HideInInspector]
    public float distanceDelta;

    private void OnDrawGizmos()
    {
        if (!debugSight) return;

        Vector3 direction = transform.forward;
        //Vector3 direction = Quaternion.AngleAxis(-30, Vector3.up) * transform.forward;

        if (direction.y < 0) direction.y = 0;

        Gizmos.color = Color.red;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawSphere(target.position + direction * lockWidthOfSight, lockWidthOfSight);
        Gizmos.DrawSphere(target.position + direction * lockSight, lockWidthOfSight);
    }

    //generic
    [Tooltip("Target must be looked from camera.")]
    public Transform target;
    public bool invertCameraY;

    //action
    [Tooltip("Distance on z axis from target, used in action state.")]
    public float actionDistance;
    [Tooltip("Distance on y axis from target, used in action state.")]
    public float actionHeight;
    public float actionHorizntalCameraSpeed;
    public float actionVerticalCameraSpeed;
    [Tooltip("The min angle that can be reached from camera.")]
    public float actionMinOrbitY;
    [Tooltip("The max angle that can be reached from camera.")]
    public float actionMaxOrbitY;
    [Tooltip("The linear interpolation speed of camera. Used in action stae.")]
    public float actionLerpSpeed;

    //lock
    [Tooltip("Distance on z axis from target, used in lock state.")]
    public float lockDistance;
    [Tooltip("Distance on y axis from target, used in lock state.")]
    public float lockHeight;
    [Tooltip("The linear interpolation speed of camera. Used in lock state.")]
    public float lockLerpSpeed;
    [Tooltip("The maximum distance supported from lock camera, over this value camera return in action state.")]
    public float lockMaxDistanceFromTarget;
    [Tooltip("The distance of player sight, start from camera. Used when player try to get target for lock camera.")]
    public float lockSight;
    [Tooltip("The width of player sight. Used in lock state.")]
    public float lockWidthOfSight;
    [Tooltip("")]
    public string[] sightIgnoredLayers;
    [Tooltip("Show a sphere Gizmos in tha start point of spherecast and at the end point of spherecast. Sight trigger enemy between this 2 points.")]
    public bool debugSight;

    public Repositioning repositioning;

    [HideInInspector]
    public float orbitX;
    [HideInInspector]
    public float orbitY;



    protected Dictionary<string, ICameraState> states;
    protected ICameraState current;


    /// <summary>
    /// Identify tha camera state
    /// </summary>
    public bool Locked
    {
        get
        {
            return (Get("Lock") == current) ? true : false;
        }
    }

    public void Awake()
    {
        states = new Dictionary<string, ICameraState>();
        AddState("Action", new CameraActionState(this));
        AddState("Lock", new CameraLockState(this));
        current = Get("Action");
        current.Enter();
    }

    // Use this for initialization
    public void Start()
    {
        if (current == null) //no states
        {
            throw new UnityException("STATE MACHINE IS EMPTY");
        }
        current.Enter();

        if (target == null) //no player selected
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                target = go.transform;
                return;
            }
        }
    }


    void Update()
    {


        
    }
    // Update is called once per frame
    public void LateUpdate()
    {
        current.Update();

        //update hitted effect

        #region TO DELETE

        Vector3 newFor = Camera.main.transform.forward * Input.GetAxis("Vertical") + Camera.main.transform.right * Input.GetAxis("Horizontal");
        newFor.y = 0;
        target.position += newFor * 3 * Time.deltaTime;
        #endregion
    }


    /// <summary>
    /// Lock camera on a single target. Disable all controls.
    /// </summary>
    public void Lock()
    {
        ITargettable target = GetTargetCameraBased();
        if(target != null)
        {
            (states["Lock"] as CameraLockState).focusedTarget = target;
            Switch("Lock");
        }
    }
    /// <summary>
    /// Unlock camera and controls will be activated.
    /// </summary>
    public void Unlock()
    {
        Switch("Action");
    }

    /// <summary>
    /// 
    /// </summary>
    public void InvertState()
    {
        if (current == Get("Action"))
            Switch("Lock");
        else
            Switch("Action");
    }

    /// <summary>
    /// The first enabled camera tagged "MainCamera" with ActionCamera component attached. Null if there isn't.
    /// </summary>
    /// <returns></returns>
    public static ActionCamera Main
    {
        get
        {
            if (Camera.main != null)
                return Camera.main.GetComponent<ActionCamera>();
            return null;
        }
    }

    //focusing
    private ITargettable GetTargetCameraBased()
    {

        Vector3 direction = transform.forward;
        if (direction.y < 0) direction.y = 0;

        RaycastHit[] hits = Physics.SphereCastAll(target.position + direction * lockWidthOfSight, lockWidthOfSight, direction, lockSight,~LayerMask.GetMask(sightIgnoredLayers));

        if (hits != null && hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                ITargettable targettable = hit.collider.GetComponent<ITargettable>();
                if (targettable != null)
                {
                    targettable.OnTargetFocused();
                    return targettable;
                }
            }
        }
        return null;
    }
    public ITargettable GetTargetFromSide(float deg)
    {

        Vector3 direction = Quaternion.AngleAxis(deg,Vector3.up) * transform.forward;
        if (direction.y < 0) direction.y = 0;

        RaycastHit[] hits = Physics.SphereCastAll(target.position + direction * lockWidthOfSight, lockWidthOfSight, direction, lockSight, ~LayerMask.GetMask(sightIgnoredLayers));

        if (hits != null && hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                ITargettable targettable = hit.collider.GetComponent<ITargettable>();
                if (targettable != null)
                {
                    targettable.OnTargetFocused();
                    return targettable;
                }
            }
        }
        return null;
    }
    #region STATE MACHINE UTILITIES
    private ICameraState Get(string stateName)
    {
        if (states.ContainsKey(stateName))
        {
            return states[stateName];
        }
        return null;
    }
    private void Switch(string newStateName)
    {
        current.Exit();
        current = states[newStateName];
        current.Enter();
    }
    public void AddState(string stateName, ICameraState state)
    {
        if (!states.ContainsKey(stateName) && !states.ContainsValue(state))
        {
            states.Add(stateName, state);
        }
    }
    #endregion


    public override string ToString()
    {
        return "ActionCamera attached to " + gameObject.ToString();
    }
}

[Serializable]
public struct Repositioning
{
    public bool active;
    public float onRepositionNearClipPlane;
    public float onNormalNearClipPlane;

    [Tooltip("Names of layers that will be ignored during repositioning.")]
    public string[] ignoredLayers;
}


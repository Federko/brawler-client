using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeTarget : MonoBehaviour,ITargettable {
    public Transform Transform
    {
        get { return transform; }
    }

    //public event EventHandler OnTargetFocused;
    //public event EventHandler OnTargetLosed;
    //public event EventHandler OnTargetReleased;

    public void OnTargetFocused()
    {
        Debug.Log("TARGET FOCUSED");
    }

    public void OnTargetLosed()
    {
        Debug.Log("TARGET LOSED");
    }

    public void OnTargetReleased()
    {
        Debug.Log("TARGET RELEASED");
    }

    // Use this for initialization
    void Start () {
		
	}

    public Transform target;
	// Update is called once per frame
	void Update () {

        transform.RotateAround(target.position,Vector3.up, 100 * Time.deltaTime);
	}
}

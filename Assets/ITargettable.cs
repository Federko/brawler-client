using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ITargettable
{

    void OnTargetFocused();
    void OnTargetReleased();
    void OnTargetLosed();

    Transform Transform { get; }
}

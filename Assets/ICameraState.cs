using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICameraState {

    void Enter();
    void Update();
    void Exit();
    ActionCamera ActionCamera { get; set; }
}

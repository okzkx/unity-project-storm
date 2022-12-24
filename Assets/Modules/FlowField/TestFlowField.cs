using System.Collections;
using System.Collections.Generic;
using Modules.FlowField;
using Unity.Entities;
using UnityEngine;

public class TestFlowField : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        World defaultWorld = World.DefaultGameObjectInjectionWorld;
        defaultWorld.AddSystemManaged(defaultWorld.GetOrCreateSystemManaged<FlowFieldGroup>());
    }
}
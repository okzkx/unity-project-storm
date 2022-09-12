using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class AutoMoveSystem : SystemBase
{
    public int CountX = 100;
    public int CountY = 100;

    

    protected override void OnUpdate()
    {
        // Debug.Log("Update");
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngineInternal;


public struct CharacterTag : IComponentData {
}

[DisableAutoCreation]
public partial class CharacterMoveSystem : SystemBase {
    public float speed = 10;

    protected override void OnUpdate() {
        var flowField = TestFlowField.flowField;
        float deltaTime = Time.DeltaTime;
        float speed = this.speed;
        //
        // Entities
        //     .WithReadOnly(flowField)
        //     .WithAll<CharacterTag>()
        //     .ForEach((ref Translation translation) => {
        //         int2 rowCol = TranslationToRowCol(translation);
        //         float2 flowVector = flowField[TestFlowField.RowCol(rowCol)];
        //
        //         translation.Value += deltaTime * speed * new float3(flowVector.x, 0, flowVector.y);
        //
        //     }).ScheduleParallel();

        Dependency.Complete();

        Entities
            .WithReadOnly(flowField)
            .WithAll<CharacterTag>()
            .ForEach((ref PhysicsVelocity velocity, ref Rotation rotation, ref Translation translation) => {
                int2 rowCol = TranslationToRowCol(translation);
                if (TestFlowField.IsInArea(rowCol)) {
                    float2 flowVector = flowField[TestFlowField.RowCol(rowCol)];

                    velocity.Linear += new float3(flowVector.x, 0, flowVector.y);
                    velocity.Linear = speed * math.normalizesafe(velocity.Linear);
                    velocity.Angular = float3.zero;
                    velocity.Linear.y = 0;

                    rotation.Value = quaternion.identity;
                    float3 position = translation.Value;
                    position.y = 0;
                    translation.Value = position;
                }
            }).ScheduleParallel();
    }

    static int2 TranslationToRowCol(Translation translation) {
        return (int2) math.floor(translation.Value.zx);
    }
}
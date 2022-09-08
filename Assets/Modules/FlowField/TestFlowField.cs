using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class TestFlowField : MonoBehaviour
{
    public GameObject groundGridPrefab;
    public int CountX = 100;
    public int CountY = 100;

    // Start is called before the first frame update
    void Start()
    {
        World world = World.DefaultGameObjectInjectionWorld;
        AutoMoveSystem autoMoveSystem = world.CreateSystem<AutoMoveSystem>();
        SimulationSystemGroup simulationSystemGroup = world.GetExistingSystem<SimulationSystemGroup>();
        simulationSystemGroup.AddSystemToUpdateList(autoMoveSystem);


        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(groundGridPrefab, settings);
        EntityManager entityManager = world.EntityManager;

        for (var x = 0; x < CountX; x++)
        {
            for (var y = 0; y < CountY; y++)
            {
                // Efficiently instantiate a bunch of entities from the already converted entity prefab
                var instance = entityManager.Instantiate(prefab);

                // Place the instantiated entity in a grid with some noise
                var position = transform.TransformPoint(new float3(x * 1.3F, 0, y * 1.3F));
                entityManager.SetComponentData(instance, new Translation {Value = position});
            }
        }
    }
}
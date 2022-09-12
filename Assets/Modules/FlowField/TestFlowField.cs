using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class TestFlowField : MonoBehaviour
{
    public GameObject groundGridPrefab;
    public int CountX = 100;
    public int CountY = 100;
    public float heat = 100;

    // Start is called before the first frame update
    private void Start()
    {
        OnCreate();

        World world = World.DefaultGameObjectInjectionWorld;
        AutoMoveSystem autoMoveSystem = world.CreateSystem<AutoMoveSystem>();
        SimulationSystemGroup simulationSystemGroup = world.GetExistingSystem<SimulationSystemGroup>();
        simulationSystemGroup.AddSystemToUpdateList(autoMoveSystem);


        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(groundGridPrefab, settings);
        EntityManager entityManager = world.EntityManager;

        for (var row = 0; row < CountX; row++)
        {
            for (var col = 0; col < CountY; col++)
            {
                // Efficiently instantiate a bunch of entities from the already converted entity prefab
                var instance = entityManager.Instantiate(prefab);

                // Place the instantiated entity in a grid with some noise
                var position = transform.TransformPoint(new float3(col + 0.5F, 0, row + 0.5F));
                entityManager.SetComponentData(instance, new Translation {Value = position});
                float h = hotMap[RowCol(row, col)] / heat;
                float r = math.max(h, 0);
                float b = math.max(0, -h);
                // r = r * 0.5f + 0.5f;
                float g = 0;
                if (map[RowCol(row, col)] == 1)
                {
                    g = 1;
                    r = 0;
                    b = 0;
                }

                entityManager.AddComponentData(instance,
                    new HDRPMaterialPropertyBaseColor {Value = new float4(r, g, b, 1)});
            }
        }
    }

    NativeArray<float> hotMap;
    private NativeArray<int> map;

    protected void OnCreate()
    {
        map = new NativeArray<int>(CountX * CountY, Allocator.Persistent);
        hotMap = new NativeArray<float>(CountX * CountY, Allocator.Persistent);

        for (int i = 0; i < hotMap.Length; i++)
        {
            hotMap[i] = float.MinValue;
        }

        for (int col = 5; col < 70; col++)
        {
            map[RowCol(30, col)] = 1;
        }

        Queue<(int, int)> hotStarts = new Queue<(int, int)>();
        var firstHot = (15, 30);
        hotStarts.Enqueue(firstHot);
        hotMap[RowCol(firstHot)] = heat;

        while (hotStarts.Count > 0)
        {
            (int, int) hotStart = hotStarts.Dequeue();
            // hotStarts.RemoveAt(hotStarts.Count - 1);

            float newHeat = hotMap[RowCol(hotStart)] - 1;

            TryAddNextHotStart((hotStart.Item1 + 1, hotStart.Item2), hotStarts, newHeat);
            TryAddNextHotStart((hotStart.Item1 - 1, hotStart.Item2), hotStarts, newHeat);
            TryAddNextHotStart((hotStart.Item1, hotStart.Item2 + 1), hotStarts, newHeat);
            TryAddNextHotStart((hotStart.Item1, hotStart.Item2 - 1), hotStarts, newHeat);

            // if (hotStart.Item1 + 1 < CountY && newHeat > hotMap[RowCol(hotStart.Item1 + 1, hotStart.Item2)])
            // {
            // }

            // hotMap[RowCol(hotStart.Item1 + 1,hotStart.Item2 )] = 
            //     hotMap[RowCol(hotStart.Item1 + 1,hotStart.Item2 )] = 
            //         hotMap[RowCol(hotStart.Item1 + 1,hotStart.Item2 )] = 
        }

        // for (int row = 0; row < 100; row++)
        // {
        //     for (int col = 0; col < 100; col++)
        //     {
        //         hotMap[RowCol(row, col)] = 0.0f;
        //     }
        // }
    }

    private void TryAddNextHotStart((int, int) nextHotStart, Queue<(int, int)> hotStarts, float newHeat)
    {
        if (IsInArea(nextHotStart) &&
            newHeat > hotMap[RowCol(nextHotStart)]
            && map[RowCol(nextHotStart)] < 1
           )
        {
            hotMap[RowCol(nextHotStart)] = newHeat;
            hotStarts.Enqueue(nextHotStart);
        }
    }

    bool IsInArea((int, int) rowcol)
    {
        return rowcol.Item1 >= 0 &&
               rowcol.Item2 >= 0 &&
               rowcol.Item1 < CountY &&
               rowcol.Item2 < CountX;
    }

    void Destroy()
    {
        OnDestroy();
    }

    protected void OnDestroy()
    {
        map.Dispose();
        hotMap.Dispose();
    }

    private int RowCol(int row, int col)
    {
        return row * CountY + col;
    }

    private int RowCol((int, int) rowcol)
    {
        return rowcol.Item1 * CountY + rowcol.Item2;
    }
}
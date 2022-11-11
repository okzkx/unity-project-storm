using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


public class TestFlowField : MonoBehaviour {
    public GameObject groundGridPrefab;
    public int CountX = 100;
    public int CountY = 100;
    public float heat = 100;
    public bool isHeatMap = false;

    // Start is called before the first frame update
    private void Start() {
        World world = World.DefaultGameObjectInjectionWorld;
        // SystemHandle simulationSystemGroup = world.GetExistingSystem<SimulationSystemGroup>();
        // simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<CharacterMoveSystem>());
        // simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<FlowFieldSystem>());

        CreateFlowField(CountX,CountY,heat);
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, new BlobAssetStore());
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(groundGridPrefab, settings);
        EntityManager entityManager = world.EntityManager;

        for (var row = 0; row < CountX; row++) {
            for (var col = 0; col < CountY; col++) {
                // Efficiently instantiate a bunch of entities from the already converted entity prefab
                var instance = entityManager.Instantiate(prefab);

                // Place the instantiated entity in a grid with some noise
                float3 position = transform.TransformPoint(new float3(col + 0.5F, 0, row + 0.5F));
                // entityManager.SetComponentData(instance, new Translation {Value = position});
                TransformAspect transformAspect = entityManager.GetAspect<TransformAspect>(instance);
                transformAspect.Position = position; 
                
                continue;

                float h = hotMap[RowCol(row, col)] / heat;
                float r = math.max(h, 0);
                float b = math.max(0, -h);
                float g = 0;
                if (map[RowCol(row, col)] == 1) {
                    g = 1;
                    r = 0;
                    b = 0;
                }

                entityManager.AddComponentData(instance,
                    new URPMaterialPropertyBaseColor {Value = new float4(r, g, b, 1)});

                if (!isHeatMap) {
                    Color color = DebugDouble(flowField[RowCol(row, col)]);

                    entityManager.AddComponentData(instance,
                        new URPMaterialPropertyBaseColor {Value = new float4(color.r, color.g, color.b, color.a)});
                }
            }
        }
    }

    public static Color DebugDouble(float2 v) {
        float angle = Vector2.Angle(new float2(1, 0), v);
        if (v.y < 0) {
            angle = 360 - angle;
        }

        return Color.HSVToRGB(angle / 360, 1, 1);
    }

    public static NativeArray<float> hotMap;
    public static NativeArray<int> map;
    public static NativeArray<float2> flowField;

    public static void CreateFlowField(int CountX, int CountY, float heat) {
        map = new NativeArray<int>(CountX * CountY, Allocator.Persistent);
        hotMap = new NativeArray<float>(CountX * CountY, Allocator.Persistent);

        for (int i = 0; i < hotMap.Length; i++) {
            hotMap[i] = float.MinValue;
        }

        for (int col = 5; col < 70; col++) {
            map[RowCol(30, col)] = 1;
        }

        NativeQueue<int2> hotStarts = new NativeQueue<int2>(Allocator.TempJob);
        var firstHot = new int2(15, 30);
        hotStarts.Enqueue(firstHot);
        hotMap[RowCol(firstHot)] = heat;

        while (hotStarts.Count > 0) {
            var hotStart = hotStarts.Dequeue();
            float newHeat = hotMap[RowCol(hotStart)] - 1;

            TryAddNextHotStart(new int2(hotStart.x + 1, hotStart.y), newHeat);
            TryAddNextHotStart(new int2(hotStart.x - 1, hotStart.y), newHeat);
            TryAddNextHotStart(new int2(hotStart.x, hotStart.y + 1), newHeat);
            TryAddNextHotStart(new int2(hotStart.x, hotStart.y - 1), newHeat);

            newHeat = hotMap[RowCol(hotStart)] - math.SQRT2;

            TryAddNextHotStart(new int2(hotStart.x + 1, hotStart.y - 1), newHeat);
            TryAddNextHotStart(new int2(hotStart.x - 1, hotStart.y - 1), newHeat);
            TryAddNextHotStart(new int2(hotStart.x + 1, hotStart.y + 1), newHeat);
            TryAddNextHotStart(new int2(hotStart.x - 1, hotStart.y + 1), newHeat);

            void TryAddNextHotStart(int2 nextHotStart, float newHeat) {
                if (IsInArea(nextHotStart) &&
                    newHeat > hotMap[RowCol(nextHotStart)]
                    && map[RowCol(nextHotStart)] < 1
                   ) {
                    hotMap[RowCol(nextHotStart)] = newHeat;
                    hotStarts.Enqueue(nextHotStart);
                }
            }
        }

        hotStarts.Dispose();

        flowField = new NativeArray<float2>(CountX * CountY, Allocator.Persistent);
        for (int row = 0; row < CountY; row++) {
            for (int col = 0; col < CountX; col++) {
                int2 current = new int2(row, col);


                float2 vector = flowField[RowCol(current)];
                if (IsRoad(current)) {
                    AppendOrientation(new(row + 1, col), 1, new float2(0, 1));
                    AppendOrientation(new(row - 1, col), 1, new float2(0, -1));
                    AppendOrientation(new(row, col + 1), 1, new float2(1, 0));
                    AppendOrientation(new(row, col - 1), 1, new float2(-1, 0));

                    AppendOrientation(new(row + 1, col - 1), math.SQRT2, new float2(-1, 1));
                    AppendOrientation(new(row - 1, col - 1), math.SQRT2, new float2(-1, -1));
                    AppendOrientation(new(row + 1, col + 1), math.SQRT2, new float2(1, 1));
                    AppendOrientation(new(row - 1, col + 1), math.SQRT2, new float2(1, -1));

                    void AppendOrientation(int2 orientation, float magnitude, float2 map_ori) {
                        if (IsInArea(orientation) && IsRoad(orientation)) {
                            vector += (hotMap[RowCol(orientation)] - hotMap[RowCol(current)]) / magnitude * map_ori;
                        }
                    }
                } else {
                    vector = RowColToPos2D(firstHot) - RowColToPos2D(current);
                }

                flowField[RowCol(current)] = math.normalizesafe(vector);
            }
        }
    }

    private static float2 RowColToPos2D(int2 rowCol) {
        return new float2(rowCol.y, rowCol.x);
    }

    private static bool IsRoad(int2 orientation) {
        return map[RowCol(orientation)] < 1;
    }

    public static bool IsInArea(int2 rowcol) {
        return rowcol.x >= 0 &&
               rowcol.y >= 0 &&
               rowcol.x < 100 &&
               rowcol.y < 100;
    }

    void Destroy() {
        OnDestroy();
    }

    protected void OnDestroy() {
        map.Dispose();
        hotMap.Dispose();
        flowField.Dispose();
    }

    public static int RowCol(int row, int col) {
        return row * 100 + col;
    }

    public static int RowCol(int2 rowcol) {
        return rowcol.x * 100 + rowcol.y;
    }
}
//
// [DisableAutoCreation]
// public partial class FlowFieldSystem : SystemBase {
//     BuildPhysicsWorld m_BuildPhysicsWorld;
//
//     protected override void OnCreate() {
//         m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
//     }
//
//     protected override void OnUpdate() {
//         var collisionWorld = m_BuildPhysicsWorld.PhysicsWorld.CollisionWorld;
//         Vector2 mousePosition = Input.mousePosition;
//         UnityEngine.Ray unityRay = Camera.main.ScreenPointToRay(mousePosition);
//         var rayInput = new RaycastInput {
//             Start = unityRay.origin,
//             End = unityRay.origin + unityRay.direction * 1000,
//             Filter = CollisionFilter.Default,
//         };
//
//         if (Input.GetMouseButtonDown(0)) {
//             collisionWorld.CastRay(rayInput, out var raycastHit);
//             Debug.Log(raycastHit.Position);
//         }
//     }
// }
//

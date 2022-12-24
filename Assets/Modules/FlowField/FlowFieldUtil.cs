using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class FlowFieldUtil {
    public static Color DebugDouble(float2 v) {
        float angle = Vector2.Angle(new float2(1, 0), v);
        if (v.y < 0) {
            angle = 360 - angle;
        }

        return Color.HSVToRGB(angle / 360, 1, 1);
    }

    public static (NativeArray<int> map, NativeArray<float> hotMap, NativeArray<float2> flowField) CreateFlowField(int CountX, int CountY, float heat) {
        var map = new NativeArray<int>(CountX * CountY, Allocator.TempJob);
        var hotMap = new NativeArray<float>(CountX * CountY, Allocator.TempJob);
        var flowField = new NativeArray<float2>(CountX * CountY, Allocator.TempJob);

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
        for (int row = 0; row < CountY; row++) {
            for (int col = 0; col < CountX; col++) {
                int2 current = new int2(row, col);


                float2 vector = flowField[RowCol(current)];
                if (IsRoad(map, current)) {
                    AppendOrientation(new(row + 1, col), 1, new float2(0, 1));
                    AppendOrientation(new(row - 1, col), 1, new float2(0, -1));
                    AppendOrientation(new(row, col + 1), 1, new float2(1, 0));
                    AppendOrientation(new(row, col - 1), 1, new float2(-1, 0));

                    AppendOrientation(new(row + 1, col - 1), math.SQRT2, new float2(-1, 1));
                    AppendOrientation(new(row - 1, col - 1), math.SQRT2, new float2(-1, -1));
                    AppendOrientation(new(row + 1, col + 1), math.SQRT2, new float2(1, 1));
                    AppendOrientation(new(row - 1, col + 1), math.SQRT2, new float2(1, -1));

                    void AppendOrientation(int2 orientation, float magnitude, float2 map_ori) {
                        if (IsInArea(orientation) && IsRoad(map, orientation)) {
                            vector += (hotMap[RowCol(orientation)] - hotMap[RowCol(current)]) / magnitude * map_ori;
                        }
                    }
                } else {
                    vector = RowColToPos2D(firstHot) - RowColToPos2D(current);
                }

                flowField[RowCol(current)] = math.normalizesafe(vector);
            }
        }

        return (map, hotMap, flowField);
    }

    private static float2 RowColToPos2D(int2 rowCol) {
        return new float2(rowCol.y, rowCol.x);
    }

    private static bool IsRoad(NativeArray<int> map, int2 orientation) {
        return map[RowCol(orientation)] < 1;
    }

    public static bool IsInArea(int2 rowcol) {
        return rowcol.x >= 0 &&
               rowcol.y >= 0 &&
               rowcol.x < 100 &&
               rowcol.y < 100;
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
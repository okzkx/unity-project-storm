using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Modules.FlowField {
    using static FlowFieldUtil;

    public struct GridsBlock : IComponentData {
        public NativeArray<Entity> grids;
    }

    [BurstCompile]
    [UpdateInGroup(typeof(FlowFieldGroup))]
    internal partial struct GroundGridSpawnSystem : ISystem {
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<Config>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            Debug.Log("Spawn");
            var config = SystemAPI.GetSingleton<Config>();
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            NativeArray<Entity> vehicles = CollectionHelper.CreateNativeArray<Entity>(config.Count, Allocator.Persistent);
            ecb.Instantiate(config.groundGridPrefab, vehicles);

            for (var row = 0; row < config.CountX; row++) {
                for (var col = 0; col < config.CountY; col++) {
                    Entity instance = vehicles[row * config.CountX + col];

                    ecb.AddComponent<GroundGrid>(instance);
                    ecb.SetComponent(instance, new LocalTransform {
                        Position = new float3(col + 0.5F, 0, row + 0.5F),
                        Scale = 1,
                        Rotation = quaternion.identity,
                    });
                }
            }

            var gridsBlockEntity = ecb.CreateEntity();
            ecb.AddComponent(gridsBlockEntity, new GridsBlock() {
                grids = vehicles,
            });

            var requiredFlowFieldEntity = ecb.CreateEntity();
            ecb.AddComponent<RequiredFlowField>(requiredFlowFieldEntity);

            state.Enabled = false;
        }
    }

    internal struct GroundGrid : IComponentData {
    }

    internal struct RequiredFlowField : IComponentData {
    }

    [UpdateInGroup(typeof(FlowFieldGroup))]
    public partial class FlowFieldSystem : SystemBase {
        protected override void OnCreate() {
            RequireForUpdate<RequiredFlowField>();
        }

        protected override void OnUpdate() {
            // var gridsBlock = GetSingleton<GridsBlock>();
            EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<RequiredFlowField>());

            var config = SystemAPI.GetSingleton<Config>();
            var (map, hotMap, flowField) = CreateFlowField(config.CountX, config.CountY, config.heat);

            var query = GetEntityQuery(typeof(GroundGrid));
            var grids = query.ToEntityArray(Allocator.Temp);
            if (grids.Length == 0) {
                return;
            }

            for (var row = 0; row < config.CountX; row++) {
                for (var col = 0; col < config.CountY; col++) {
                    Entity grid = grids[row * config.CountX + col];

                    float h = hotMap[RowCol(row, col)] / config.heat;
                    float r = math.max(h, 0);
                    float b = math.max(0, -h);
                    float g = 0;
                    if (map[RowCol(row, col)] == 1) {
                        g = 1;
                        r = 0;
                        b = 0;
                    }

                    EntityManager.AddComponentData(grid,
                        new URPMaterialPropertyBaseColor {Value = new float4(r, g, b, 1)});

                    if (!config.isHeatMap) {
                        Color color = DebugDouble(flowField[RowCol(row, col)]);

                        EntityManager.AddComponentData(grid,
                            new URPMaterialPropertyBaseColor {Value = new float4(color.r, color.g, color.b, color.a)});
                    }
                }
            }

            map.Dispose();
            hotMap.Dispose();
            flowField.Dispose();
        }
    }
}
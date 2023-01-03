using Unity.Entities;
using UnityEngine;

namespace Modules.FlowField {
    public struct Config : IComponentData {
        public Entity groundGridPrefab;
        public int CountX;
        public int CountY;
        public float heat;
        public bool isHeatMap;
        public int Count => CountX * CountY;
    }

    public class ConfigAuthoring : MonoBehaviour {
        public GameObject groundGridPrefab;
        public int CountX = 100;
        public int CountY = 100;
        public float heat = 100;
        public bool isHeatMap = false;

        class Baker : Baker<ConfigAuthoring> {
            public override void Bake(ConfigAuthoring authoring) {
                AddComponent(new Config {
                    groundGridPrefab = GetEntity(authoring.groundGridPrefab),
                    CountX = authoring.CountX,
                    CountY = authoring.CountY,
                    heat = authoring.heat,
                    isHeatMap = authoring.isHeatMap,
                });
            }
        }
    }
}
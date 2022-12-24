using Unity.Entities;

namespace Modules.EntitySample {
    internal class ConfigAuthoring : UnityEngine.MonoBehaviour {
        public UnityEngine.GameObject TankPrefab;
        public int TankCount = 20;
        public float SafeZoneRadius = 15;
    }

    internal class ConfigBaker : Baker<ConfigAuthoring> {
        public override void Bake(ConfigAuthoring authoring) {
            AddComponent(new Config {
                TankPrefab = GetEntity(authoring.TankPrefab),
                TankCount = authoring.TankCount,
                SafeZoneRadius = authoring.SafeZoneRadius
            });
        }
    }
}
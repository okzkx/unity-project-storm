using Unity.Entities;

namespace Modules.EntitySample {
    internal struct Config : IComponentData {
        public Entity TankPrefab;
        public int TankCount;
        public float SafeZoneRadius;
    }
}
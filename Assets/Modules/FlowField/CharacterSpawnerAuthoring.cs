using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct CharacterSpawner : IComponentData {
    public Entity prefab;
}

public class CharacterSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
    public GameObject soliderPrefab;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var prefab = conversionSystem.GetPrimaryEntity(soliderPrefab);

        dstManager.AddComponentData(entity, new CharacterSpawner() {
            prefab = prefab,
        });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
        referencedPrefabs.Add(soliderPrefab);
    }
}

public partial class CharacterSpawnSystem : SystemBase {
    protected override void OnCreate() {
        RequireSingletonForUpdate<CharacterSpawner>();
    }

    protected override void OnUpdate() {
        EntityQuery query = GetEntityQuery(typeof(CharacterSpawner));
        var spawnEntity = query.GetSingletonEntity();
        
        var prefab = EntityManager.GetComponentData<CharacterSpawner>(spawnEntity).prefab;

        for (int row = 0; row < 50; row++) {
            for (int col = 0; col < 50; col++) {
                Entity instance = EntityManager.Instantiate(prefab);
                EntityManager.AddComponentData(instance, new CharacterTag());
                EntityManager.AddComponentData(instance, new Translation {Value = new float3(50 + col + 0.5f, 0, 50 + row + 0.5f)});
            }
        }

        EntityManager.DestroyEntity(spawnEntity);
    }
}
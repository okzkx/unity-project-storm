using Unity.Entities;
using Unity.Scenes;
using UnityEngine;

public abstract class SceneSelectorGroup : ComponentSystemGroup {
    protected override void OnCreate() {
        base.OnCreate();
        var subScene = Object.FindObjectOfType<SubScene>();
        if (subScene != null)
            Enabled = SceneName == subScene.gameObject.scene.name;
        else
            Enabled = false;
    }

    protected abstract string SceneName { get; }
}
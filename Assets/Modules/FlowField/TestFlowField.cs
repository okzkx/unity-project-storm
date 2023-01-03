using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Modules.FlowField {
    using static FlowFieldUtil;

    public class TestFlowField : MonoBehaviour {
        public int CountX = 100;
        public int CountY = 100;
        public float heat = 100;
        public bool isHeatMap = false;

        private void Start() {
            var (map, hotMap, flowField) = CreateFlowField(CountX, CountY, heat, Allocator.Temp);

            for (var row = 0; row < CountX; row++) {
                for (var col = 0; col < CountY; col++) {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.position = new Vector3(col + 0.5F, 0, row + 0.5F);

                    float h = hotMap[RowCol(row, col)] / heat;
                    float r = math.max(h, 0);
                    float b = math.max(0, -h);
                    float g = 0;
                    if (map[RowCol(row, col)] == 1) {
                        g = 1;
                        r = 0;
                        b = 0;
                    }

                    Color color = new Color(r, g, b, 1);
                    if (!isHeatMap) {
                        color = DebugDouble(flowField[RowCol(row, col)]);
                    }

                    MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
                    var block = new MaterialPropertyBlock();
                    block.SetColor("_BaseColor", color);
                    meshRenderer.SetPropertyBlock(block);
                }
            }
        }
    }
}
# unity-project-storm

代号：风暴

## Untiy 版本

- 2022.2.0b8 或者更高的版本
- 下载地址：[Beta Program](https://unity3d.com/unity/beta) , 或直接从 UnityHub beta 区域下载

## 示例\模块

地址：`'Assets\Modules\'`

### EntitySample

Dots v1.0 EntitySample

1. Entity 生成
2. Entity 渲染和材质参数变化
3. System 修改 Aspect 来修改 ComponentData 数据
4. System 修改 ComponentData 数量来控制 Entity 行为
5. System 对 GameObject 和 Entity 进行交互

### FlowField

群体寻路算法

1. 根据目标点和地图生成热度图 (HeatMap)
2. 根据热度图生成向量场 (FlowField)
3. 根据向量场对于群体士兵单位进行物理位移，寻路到目标点
4. 通过物理碰撞解决重叠问题

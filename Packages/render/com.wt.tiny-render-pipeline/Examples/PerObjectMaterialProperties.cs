using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 特性：不允许同一物体挂多个该组件
[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    // 获取名为"_BaseColor"的Shader属性（全局）
    private static int s_BaseColorId = Shader.PropertyToID("_BaseColor");

    private static int s_CutOffId = Shader.PropertyToID("_CutOff");
    // 每个物体自己的颜色
    [SerializeField] private Color m_BaseColor = Color.white;

    [SerializeField, Range(0.0f, 1.0f)] private float m_CutOff = 0.5f;
    // MaterialPropertyBlock用于给每个物体设置材质属性，将其设置为静态，所有物体使用同一个block
    private static MaterialPropertyBlock s_MaterialPropertyBlock;

    private void OnValidate()
    {
        s_MaterialPropertyBlock ??= new MaterialPropertyBlock();
        // 设置block中的baseColor属性(通过baseCalorId索引)为baseColor
        s_MaterialPropertyBlock.SetColor(s_BaseColorId, m_BaseColor);
        s_MaterialPropertyBlock.SetFloat(s_CutOffId, m_CutOff);
        // 将物体的Renderer中的颜色设置为block中的颜色
        GetComponent<Renderer>().SetPropertyBlock(s_MaterialPropertyBlock);
    }

    // Runtime时也执行
    private void Awake()
    {
        OnValidate();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���ԣ�������ͬһ����Ҷ�������
[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    // ��ȡ��Ϊ"_BaseColor"��Shader���ԣ�ȫ�֣�
    private static int s_BaseColorId = Shader.PropertyToID("_BaseColor");

    private static int s_CutOffId = Shader.PropertyToID("_CutOff");
    // ÿ�������Լ�����ɫ
    [SerializeField] private Color m_BaseColor = Color.white;

    [SerializeField, Range(0.0f, 1.0f)] private float m_CutOff = 0.5f;
    // MaterialPropertyBlock���ڸ�ÿ���������ò������ԣ���������Ϊ��̬����������ʹ��ͬһ��block
    private static MaterialPropertyBlock s_MaterialPropertyBlock;

    private void OnValidate()
    {
        s_MaterialPropertyBlock ??= new MaterialPropertyBlock();
        // ����block�е�baseColor����(ͨ��baseCalorId����)ΪbaseColor
        s_MaterialPropertyBlock.SetColor(s_BaseColorId, m_BaseColor);
        s_MaterialPropertyBlock.SetFloat(s_CutOffId, m_CutOff);
        // �������Renderer�е���ɫ����Ϊblock�е���ɫ
        GetComponent<Renderer>().SetPropertyBlock(s_MaterialPropertyBlock);
    }

    // RuntimeʱҲִ��
    private void Awake()
    {
        OnValidate();
    }
}

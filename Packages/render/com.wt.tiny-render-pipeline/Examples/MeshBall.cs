using UnityEngine;
using Random = UnityEngine.Random;

public class MeshBall : MonoBehaviour
{
    // 和之前一样，使用int类型的PropertyId代替属性名称
    private int s_BaseColorId = Shader.PropertyToID("_BaseColor");
    // GPU Instancing使用的Mesh
    [SerializeField] Mesh m_Mesh = default;
    // GPU Instancing使用的Material
    [SerializeField] Material m_Material = default;

    // 我们可以new 1000个GameObject，但是我们也可以直接通过每实例数据去绘制GPU Instancing的物体
    // 创建每实例数据
    private Matrix4x4[] m_Matrices = new Matrix4x4[1023];
    private Vector4[] m_BaseColors = new Vector4[1023];

    private MaterialPropertyBlock m_MaterialPropertyBlock;

    private void Awake()
    {
        for (int i = 0; i < m_Matrices.Length; i++)
        {
            // 在半径10米的球空间内随机实例小球的位置
            m_Matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10f,
                Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f),
                Vector3.one * Random.Range(0.5f, 1.5f));
            m_BaseColors[i] = new Vector4(Random.value, Random.value, Random.value, Random.Range(0.5f, 1f));
        }
    }

    private void Update()
    {
        if (m_MaterialPropertyBlock == null)
        {
            m_MaterialPropertyBlock = new MaterialPropertyBlock();
            // 设置向量属性数组
            m_MaterialPropertyBlock.SetVectorArray(s_BaseColorId, m_BaseColors);
        }
        // 一帧绘制多个网格，并且没有创建不必要的游戏对象的开销（一次最多只能绘制1023个实例），材质必须支持GPU Instancing
        Graphics.DrawMeshInstanced(m_Mesh, 0, m_Material, m_Matrices, 1023, m_MaterialPropertyBlock);
    }
}

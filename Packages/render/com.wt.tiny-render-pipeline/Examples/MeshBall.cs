using UnityEngine;
using Random = UnityEngine.Random;

public class MeshBall : MonoBehaviour
{
    // ��֮ǰһ����ʹ��int���͵�PropertyId������������
    private int s_BaseColorId = Shader.PropertyToID("_BaseColor");
    // GPU Instancingʹ�õ�Mesh
    [SerializeField] Mesh m_Mesh = default;
    // GPU Instancingʹ�õ�Material
    [SerializeField] Material m_Material = default;

    // ���ǿ���new 1000��GameObject����������Ҳ����ֱ��ͨ��ÿʵ������ȥ����GPU Instancing������
    // ����ÿʵ������
    private Matrix4x4[] m_Matrices = new Matrix4x4[1023];
    private Vector4[] m_BaseColors = new Vector4[1023];

    private MaterialPropertyBlock m_MaterialPropertyBlock;

    private void Awake()
    {
        for (int i = 0; i < m_Matrices.Length; i++)
        {
            // �ڰ뾶10�׵���ռ������ʵ��С���λ��
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
            // ����������������
            m_MaterialPropertyBlock.SetVectorArray(s_BaseColorId, m_BaseColors);
        }
        // һ֡���ƶ�����񣬲���û�д�������Ҫ����Ϸ����Ŀ�����һ�����ֻ�ܻ���1023��ʵ���������ʱ���֧��GPU Instancing
        Graphics.DrawMeshInstanced(m_Mesh, 0, m_Material, m_Matrices, 1023, m_MaterialPropertyBlock);
    }
}

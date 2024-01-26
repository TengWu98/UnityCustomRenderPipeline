using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace TinyRender.Pipeline
{
    public partial class CameraRenderer
    {
        //存放当前渲染上下文
        private ScriptableRenderContext m_Context;
        //存放摄像机渲染器当前应该渲染的摄像机
        private Camera m_Camera;
        // 命令缓冲区
        private const string m_BufferName = "Render Camera";
        private CommandBuffer m_CommandBuffer = new CommandBuffer
        {
            name = m_BufferName
        };
        // 剔除结果
        private CullingResults m_CullingResults;
        // 获取SRPDefaultUnlit过程的着色器标记ID
        static ShaderTagId s_UnlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

        string m_SampleName { get; set; }

        // 定义分部函数
        partial void DrawUnsupportedShaders();
        partial void DrawGizmos();
        partial void PrepareForSceneWindow();
        partial void PrepareBuffer();
#if UNITY_EDITOR
        // 取Unity默认的shaderTagId
        static ShaderTagId[] s_LegacyShaderTagIds = {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
        };

        // 错误材质
        static Material s_ErrorMaterial = null;

        partial void DrawUnsupportedShaders()
        {
            // 获取错误材质
            if (s_ErrorMaterial == null)
            {
                s_ErrorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
            }

            var drawingSettings = new DrawingSettings(s_LegacyShaderTagIds[0], new SortingSettings(m_Camera))
            {
                // 设置覆写的材质
                overrideMaterial = s_ErrorMaterial
            };

            // 设置所有不支持的ShaderPass
            for (int i = 1; i < s_LegacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, s_LegacyShaderTagIds[i]);
            }

            var filteringSettings = FilteringSettings.defaultValue;
            m_Context.DrawRenderers(
                m_CullingResults, ref drawingSettings, ref filteringSettings
            );
        }

        partial void DrawGizmos()
        {
            // 在Scene窗口中绘制Gizmos
            if (Handles.ShouldRenderGizmos())
            {
                // 后处理之前绘制
                m_Context.DrawGizmos(m_Camera, GizmoSubset.PreImageEffects);
                // 后处理之后绘制
                m_Context.DrawGizmos(m_Camera, GizmoSubset.PostImageEffects);
            }
        }

        partial void PrepareForSceneWindow()
        {
            // 绘制Scene窗口下的UI
            if (m_Camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(m_Camera);
            }
        }

        partial void PrepareBuffer()
        {
            UnityEngine.Profiling.Profiler.BeginSample("Editor Only");
            // 对每个摄像机使用不同的Sample Name
            m_CommandBuffer.name = m_SampleName = m_Camera.name;
            UnityEngine.Profiling.Profiler.EndSample();
        }
#else
        const string m_SampleName => bufferName;
#endif

        // 摄像机渲染器的渲染函数，在当前渲染上下文的基础上渲染当前摄像机
        public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing)
        {
            this.m_Context = context;
            this.m_Camera = camera;

            PrepareBuffer();
            PrepareForSceneWindow();

            if (!Cull())
            {
                return;
            }

            Setup();
            DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
            DrawUnsupportedShaders();
            DrawGizmos();
            Submit();
        }

        private void Setup()
        {
            // 把当前摄像机的信息告诉上下文，这样shader中就可以获取到当前帧下摄像机的信息，比如VP矩阵等
            m_Context.SetupCameraProperties(m_Camera);
            // 获取相机的clearFlags
            CameraClearFlags flags = m_Camera.clearFlags;
            //清除当前摄像机Render Target中的内容,包括深度和颜色，ClearRenderTarget内部会Begin/EndSample(buffer.name)
            m_CommandBuffer.ClearRenderTarget(
                flags <= CameraClearFlags.Depth,
                flags == CameraClearFlags.Color,
                flags == CameraClearFlags.Color ?
                    m_Camera.backgroundColor.linear : Color.clear
            );
            // 在Profiler和Frame Debugger中开启对Command buffer的监测
            m_CommandBuffer.BeginSample(m_SampleName);
            // 提交CommandBuffer并且清空它，在Setup中做这一步的作用应该是确保在后续给CommandBuffer添加指令之前，其内容是空的。
            ExecuteCommandBuffer();
        }

        private void ExecuteCommandBuffer()
        {
            // 要执行缓冲区，需要将缓冲区作为参数在当前上下文中调用ExecuteCommandBuffer函数，它执行缓冲区，但不清空它
            m_Context.ExecuteCommandBuffer(m_CommandBuffer);
            // 我们在CommandBuffer执行之后要立刻清空它
            // 这样如果我们想要重用CommandBuffer，需要针对它再单独操作（不使用ExecuteCommandBuffer）
            m_CommandBuffer.Clear();
        }

        private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
        {
            /***************************** 绘制不透明物体 ****************************/
            // 决定物体绘制顺序是正交排序还是基于距离排序的配置
            // 设置排序方式为不透明物体的绘制顺序（由近到远渲染）
            var sortingSettings = new SortingSettings(m_Camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            // 决定摄像机支持的Shader Pass和绘制顺序等的配置
            // 允许的Shader Pass：Unlit Shader
            // 绘制顺序：sortingSettings
            var drawingSettings = new DrawingSettings(s_UnlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing
            };
            // 决定过滤哪些Visible Objects的配置，包括支持的RenderQueue等
            // 允许的渲染队列：不透明物体
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            // 渲染CullingResults内的所有不透明的VisibleObjects
            m_Context.DrawRenderers(
                m_CullingResults, ref drawingSettings, ref filteringSettings
            );

            /***************************** 绘制天空盒 ******************************/
            m_Context.DrawSkybox(m_Camera);

            /***************************** 绘制透明物体 ****************************/
            // 设置排序方式为透明物体的绘制顺序（由远到近渲染）
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            // 允许的渲染队列：透明物体
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            // 渲染CullingResults内的所有透明的VisibleObjects
            m_Context.DrawRenderers(
                m_CullingResults, ref drawingSettings, ref filteringSettings
            );
        }

        private void Submit()
        {
            // 在Proiler和Frame Debugger中结束对Command buffer的监测
            m_CommandBuffer.EndSample(m_SampleName);
            // 提交CommandBuffer并且清空它
            ExecuteCommandBuffer();
            // 提交当前上下文中缓存的指令队列，并依次执行指令队列
            m_Context.Submit();
        }

        private bool Cull()
        {
            if (m_Camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                m_CullingResults = m_Context.Cull(ref p);
                return true;
            }
            return false;
        }

        
    }
}

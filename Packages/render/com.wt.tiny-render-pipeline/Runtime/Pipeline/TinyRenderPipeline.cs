using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TinyRender.Pipeline
{
    public partial class TinyRenderPipeline : RenderPipeline
    {
        private readonly CameraRenderer m_CameraRenderer;

        private bool m_UseDynamicBatching;
        private bool m_UseGPUInstacing;

        // 重写Render函数，实现自定义的渲染
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(var camera in cameras)
            {
                m_CameraRenderer.Render(context, camera, m_UseDynamicBatching, m_UseGPUInstacing);
            }
        }

        // 构造函数
        public TinyRenderPipeline(bool useDynamicBatching, bool useGPUInstacing, bool useSRPBatcher)
        {
            m_CameraRenderer = new CameraRenderer();

            this.m_UseDynamicBatching = useDynamicBatching;
            this.m_UseGPUInstacing = useGPUInstacing;
            // 配置SRP Batch
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        }
    }
}

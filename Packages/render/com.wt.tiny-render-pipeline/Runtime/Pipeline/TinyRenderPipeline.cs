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

        // ��дRender������ʵ���Զ������Ⱦ
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(var camera in cameras)
            {
                m_CameraRenderer.Render(context, camera, m_UseDynamicBatching, m_UseGPUInstacing);
            }
        }

        // ���캯��
        public TinyRenderPipeline(bool useDynamicBatching, bool useGPUInstacing, bool useSRPBatcher)
        {
            m_CameraRenderer = new CameraRenderer();

            this.m_UseDynamicBatching = useDynamicBatching;
            this.m_UseGPUInstacing = useGPUInstacing;
            // ����SRP Batch
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        }
    }
}

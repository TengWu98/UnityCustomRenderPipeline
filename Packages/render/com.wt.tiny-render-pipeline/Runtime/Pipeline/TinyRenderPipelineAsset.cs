using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TinyRender.Pipeline
{
    [CreateAssetMenu(menuName = "Rendering/Create TinyRenderPipelineAsset")]
    public class TinyRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] bool m_UseDynamicBatching = true;
        [SerializeField] bool m_UseGPUInstacing = true;
        [SerializeField] bool m_UseSRPBatcher = true;
        protected override RenderPipeline CreatePipeline() => new TinyRenderPipeline(
                m_UseDynamicBatching, m_UseGPUInstacing, m_UseSRPBatcher
            ); 
    }
}

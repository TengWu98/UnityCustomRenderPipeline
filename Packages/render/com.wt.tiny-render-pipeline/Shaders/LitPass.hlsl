#ifndef TINY_RENDER_LIT_PASS_INCLUDED
#define TINY_RENDER_LIT_PASS_INCLUDED

#include "Packages/render/com.wt.tiny-render-pipeline/Shaders/ShaderLibrary/Common.hlsl"

// 在Shader的全局变量区定义纹理的句柄和其采样器，通过名字来匹配
TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

// 使用Core RP Library的CBUFFER宏指令包裹材质属性，让Shader支持SRP Batcher，同时在不支持SRP Batcher的平台自动关闭它。
// CBUFFER_START后要加一个参数，参数表示该C buffer的名字(Unity内置了一些名字，如UnityPerMaterial，UnityPerDraw)。

// CBUFFER_START(UnityPerMaterial)
// 	float4 _BaseColor;
// CBUFFER_END

// 为了使用GPU Instancing，每实例数据要构建成数组,使用UNITY_INSTANCING_BUFFER_START(END)来包裹每实例数据
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
	//纹理坐标的偏移和缩放可以是每实例数据
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
	UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

// 顶点着色器的输入
struct a2v
{
	float3 positionOS : POSITION;
	float2 baseUV:TEXCOORD0;
	// 定义GPU Instancing使用的每个实例的ID，告诉GPU当前绘制的是哪个Object
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

// 片元着色器的输入
struct v2f
{
	float4 positionCS : SV_POSITION;
	float2 baseUV:VAR_BASE_UV;
	// 定义每一个片元对应的object的唯一ID
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2f vert(a2v v)
{
	v2f o;

	// 从input中提取实例的ID并将其存储在其他实例化宏所依赖的全局静态变量中
    UNITY_SETUP_INSTANCE_ID(v);
    // 将实例ID传递给output
    UNITY_TRANSFER_INSTANCE_ID(v,o);

	float3 positionWS = TransformObjectToWorld(v.positionOS);
	o.positionCS = TransformWorldToHClip(positionWS);

	//应用纹理ST变换
    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    o.baseUV = v.baseUV * baseST.xy + baseST.zw;
	return o;
}

float4 frag(v2f i) : SV_TARGET
{
	// 从input中提取实例的ID并将其存储在其他实例化宏所依赖的全局静态变量中
    UNITY_SETUP_INSTANCE_ID(i);
    // 获取采样纹理颜色
    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.baseUV);
    // 通过UNITY_ACCESS_INSTANCED_PROP获取每实例数据
    float4 baseColor =  UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 base = baseMap * baseColor;
    #if defined(_CLIPPING)
    clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
    #endif
	return base;
}

#endif
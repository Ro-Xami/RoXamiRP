using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public enum RenderPassEvent
    {
        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> before rendering any other passes in the pipeline.
        /// Camera matrices and stereo rendering are not setup this point.
        /// You can use this to draw to custom input textures used later in the pipeline, f.ex LUT textures.
        /// </summary>
        BeforeRendering = 0,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> before rendering shadowmaps.
        /// Camera matrices and stereo rendering are not setup this point.
        /// </summary>
        BeforeRenderingShadows = 20,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> after rendering shadowmaps.
        /// Camera matrices and stereo rendering are not setup this point.
        /// </summary>
        AfterRenderingShadows = 40,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> before rendering prepasses, f.ex, depth prepass.
        /// Camera matrices and stereo rendering are already setup at this point.
        /// </summary>
        BeforeRenderingPrePasses = 60,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> after rendering prepasses, f.ex, depth prepass.
        /// Camera matrices and stereo rendering are already setup at this point.
        /// </summary>
        AfterRenderingPrePasses = 80,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> before rendering gbuffer pass.
        /// </summary>
        BeforeRenderingGbuffer = 100,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> after rendering gbuffer pass.
        /// </summary>
        AfterRenderingGbuffer = 120,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> before rendering deferred shading pass.
        /// </summary>
        BeforeRenderingDeferredDiffuse = 140,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> after rendering deferred shading pass.
        /// </summary>
        AfterRenderingDeferredDiffuse = 160,
        
        BeforeRenderingDeferredGI = 180,
        AfterRenderingDeferredGI = 200,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> before rendering opaque objects.
        /// </summary>
        BeforeRenderingOpaques = 220,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> after rendering opaque objects.
        /// </summary>
        AfterRenderingOpaques = 240,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> before rendering the sky.
        /// </summary>
        BeforeRenderingSkybox = 260,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> after rendering the sky.
        /// </summary>
        AfterRenderingSkybox = 280,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> before rendering transparent objects.
        /// </summary>
        BeforeRenderingTransparents = 300,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> after rendering transparent objects.
        /// </summary>
        AfterRenderingTransparents = 320,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> before rendering post-processing effects.
        /// </summary>
        BeforeRenderingPostProcessing = 340,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> after rendering post-processing effects but before final blit, post-processing AA effects and color grading.
        /// </summary>
        AfterRenderingPostProcessing = 360,

        /// <summary>
        /// Executes a <c>ScriptableRenderPass</c> after rendering all effects.
        /// </summary>
        AfterRendering = 380,
    }

    public abstract class RoXamiRenderPass
    {
        public RenderPassEvent renderPassEvent { get; set; }


        public virtual void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        public virtual void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

        }

        public virtual void CleanUp()
        {

        }

        protected void ExecuteCommandBuffer(ScriptableRenderContext context, CommandBuffer cmd)
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        protected void ClearRenderTargetWithUnityAPI(CommandBuffer cmd, Camera camera)
        {
            CameraClearFlags flags = camera.clearFlags;

            cmd.ClearRenderTarget(
                flags <= CameraClearFlags.Depth,
                flags <= CameraClearFlags.Color,
                flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear
            );
        }
        
        protected void DrawDontCareDontCare(
            CommandBuffer cmd, RenderTargetIdentifier from, RenderTargetIdentifier to, 
            Material mat, int passIndex)
        {
            cmd.SetGlobalTexture(ShaderDataID.TempRtSource0ID, from);
            cmd.SetRenderTarget(
                to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare
            );
            cmd.DrawProcedural(
                Matrix4x4.identity, mat, passIndex,
                MeshTopology.Triangles, 3
            );
        }
        
        protected void DrawDontCareStore(
            CommandBuffer cmd, RenderTargetIdentifier from, RenderTargetIdentifier to, 
            Material mat, int passIndex)
        {
            cmd.SetGlobalTexture(ShaderDataID.TempRtSource0ID, from);
            cmd.SetRenderTarget(
                to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
            );
            cmd.DrawProcedural(
                Matrix4x4.identity, mat, passIndex,
                MeshTopology.Triangles, 3
            );
        }

        protected void DrawFullScreenTriangles(CommandBuffer cmd, Material mat, int passIndex)
        {
            cmd.DrawProcedural(
                Matrix4x4.identity, mat, passIndex,
                MeshTopology.Triangles, 3
            );
        }
        
        protected void UpdateGlobalSHColor()
        {
            CoreRpToRoXamiRP.SHUtility.UploadToShader();
        }

        protected void RoXamiRPCopyTexture(CommandBuffer cmd, RTHandle from, RTHandle to, int nameID)
        {
            cmd.CopyTexture(from, to);
            cmd.SetGlobalTexture(nameID, to);
        }

        public static bool operator <(RoXamiRenderPass lhs, RoXamiRenderPass rhs)
        {
            return lhs.renderPassEvent < rhs.renderPassEvent;
        }

        public static bool operator >(RoXamiRenderPass lhs, RoXamiRenderPass rhs)
        {
            return lhs.renderPassEvent > rhs.renderPassEvent;
        }
    }
}
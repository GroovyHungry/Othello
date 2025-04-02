using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelationRenderFeature : ScriptableRendererFeature
{
	class PixelationPass : ScriptableRenderPass
	{
		private Material material;
		private RTHandle cameraColorTarget;
		public float pixelSize = 100f;

		public PixelationPass(Material material)
		{
			this.material = material;
			renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
		}

		public void SetTarget(RTHandle cameraColorTargetHandle)
		{
			cameraColorTarget = cameraColorTargetHandle;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (material == null)
				return;

			CommandBuffer cmd = CommandBufferPool.Get("Pixelation Pass");

			material.SetFloat("_PixelSize", pixelSize);

			Blitter.BlitCameraTexture(cmd, cameraColorTarget, cameraColorTarget, material, 0);
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}

	[SerializeField] Shader pixelationShader;
	[SerializeField] float pixelSize = 100f;

	private Material pixelationMaterial;
	private PixelationPass pixelationPass;

	public override void Create()
	{
		if (pixelationShader == null)
		{
			Debug.LogError("Pixelation shader missing");
			return;
		}

		pixelationMaterial = CoreUtils.CreateEngineMaterial(pixelationShader);
		pixelationPass = new PixelationPass(pixelationMaterial)
		{
			pixelSize = pixelSize
		};
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		pixelationPass.SetTarget(renderer.cameraColorTargetHandle);
		renderer.EnqueuePass(pixelationPass);
	}
}

using System;
using UnityEngine;

namespace Kayac
{
	[CreateAssetMenu(fileName = "LightPostProcessorAsset", menuName = "Kayac/LightPostProcessorAsset", order = 1)]
	public class LightPostProcessorAsset : ScriptableObject
	{
		public Shader BloomCombineShader
		{
			get
			{
				return this.bloomCombineShader;
			}
		}

		public Shader BrightnessExtractionShader
		{
			get
			{
				return this.brightnessExtractionShader;
			}
		}

		public Shader CompositionShader
		{
			get
			{
				return this.compositionShader;
			}
		}

		public Shader GaussianBlurShader
		{
			get
			{
				return this.gaussianBlurShader;
			}
		}

		[SerializeField]
		private Shader bloomCombineShader;

		[SerializeField]
		private Shader brightnessExtractionShader;

		[SerializeField]
		private Shader compositionShader;

		[SerializeField]
		private Shader gaussianBlurShader;
	}
}

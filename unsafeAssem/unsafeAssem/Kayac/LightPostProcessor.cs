using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace Kayac
{
	public class LightPostProcessor : M2CameraInterrupt
	{
		public float BloomPixelThreshold
		{
			set
			{
				this.bloomPixelThreshold = value;
			}
		}

		public float BloomStrength
		{
			get
			{
				return this.bloomStrength;
			}
			set
			{
				this.bloomStrength = value;
			}
		}

		public int BloomCombineStartLevel
		{
			get
			{
				return this.bloomCombineStartLevel;
			}
			set
			{
				this.bloomCombineStartLevel = value;
			}
		}

		public IEnumerable<RenderTexture> EnumerateRenderTexturesForDebug()
		{
			yield return this.brightness;
			yield return this.bloomX;
			yield return this.bloomXY;
			yield return this.bloomCombined;
			yield break;
		}

		public LightPostProcessor(MTI Mti = null)
		{
			if (Mti == null)
			{
				this.blurMaterial = new Material(Shader.Find("Hidden/LightPostProcessorGaussianBlur"));
				this.extractionMaterial = new Material(Shader.Find("Hidden/LightPostProcessorBrightnessExtraction"));
				this.combineMaterial = new Material(Shader.Find("Hidden/LightPostProcessorBloomCombine"));
				this.compositionMaterial = new Material(Shader.Find("Hidden/LightPostProcessorComposition"));
			}
			else
			{
				this.blurMaterial = new Material(Mti.LoadShader("Hidden/LightPostProcessorGaussianBlur"));
				this.extractionMaterial = new Material(Mti.LoadShader("Hidden/LightPostProcessorBrightnessExtraction"));
				this.combineMaterial = new Material(Mti.LoadShader("Hidden/LightPostProcessorBloomCombine"));
				this.compositionMaterial = new Material(Mti.LoadShader("Hidden/LightPostProcessorComposition"));
			}
			this.SetColorTransform();
			this.bloomSamples = new LightPostProcessor.BloomSample[4];
		}

		public void destruct()
		{
			IN.DestroyOne(this.blurMaterial);
			IN.DestroyOne(this.extractionMaterial);
			IN.DestroyOne(this.combineMaterial);
			IN.DestroyOne(this.compositionMaterial);
		}

		public void RenderImage(RenderTexture source, RenderTexture destination)
		{
			this.maxBloomLevelCount = Math.Min(this.maxBloomLevelCount, 7);
			this.SetupRenderTargets(source);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.LoadOrtho();
			bool flag = this.first;
			int num = source.width >> this.bloomStartLevel;
			int num2 = source.height >> this.bloomStartLevel;
			int num3 = (this.brightness.width - num) / 2;
			int num4 = (this.brightness.height - num2) / 2;
			this.ExtractBrightness(source, num3, num4, num, num2, true);
			this.CalcGaussianSamples(this.bloomSigmaInPixel);
			this.BlurX(num3, num4, num, num2);
			this.BlurY();
			if (this.bloomCombineStartLevel < this.bloomRects.Count + this.bloomStartLevel)
			{
				this.CombineBloom();
			}
			this.Composite(source, destination);
			GL.PopMatrix();
			this.first = false;
		}

		private void ExtractBrightness(RenderTexture source, int brightnessOffsetX, int brightnessOffsetY, int brightnessNetWidth, int brightnessNetHeight, bool clear)
		{
			this.extractionMaterial.SetTexture("_MainTex", source);
			if (this.bloomPixelThreshold <= 0f)
			{
				this.extractionMaterial.EnableKeyword("PASS_THROUGH");
			}
			else
			{
				this.extractionMaterial.DisableKeyword("PASS_THROUGH");
				Vector4 vector = new Vector4(1f / (1f - this.bloomPixelThreshold), -this.bloomPixelThreshold / (1f - this.bloomPixelThreshold), 0f, 0f);
				this.extractionMaterial.SetVector("_ColorTransform", vector);
			}
			this.extractionMaterial.SetPass(0);
			this.Blit(source, 0, 0, source.width, source.height, this.brightness, brightnessOffsetX, brightnessOffsetY, brightnessNetWidth, brightnessNetHeight, clear, this.clearColor);
		}

		private void BlurX(int brightnessOffsetX, int brightnessOffsetY, int brightnessNetWidth, int brightnessNetHeight)
		{
			this.brightness.filterMode = FilterMode.Bilinear;
			this.blurMaterial.SetTexture("_MainTex", this.brightness);
			this.blurMaterial.SetFloat("_InvertOffsetScale01", this.bloomSamples[0].offset * 2f / Mathf.Abs(this.bloomSamples[0].offset - this.bloomSamples[1].offset));
			this.blurMaterial.SetPass(0);
			this.bloomX.DiscardContents();
			Graphics.SetRenderTarget(this.bloomX);
			GL.Clear(false, true, this.clearColor);
			int num = this.brightness.width;
			GL.Begin(7);
			for (int i = 0; i < this.bloomRects.Count; i++)
			{
				LightPostProcessor.BloomRect bloomRect = this.bloomRects[i];
				this.AddBlurQuads(this.brightness, brightnessOffsetX, brightnessOffsetY, brightnessNetWidth, brightnessNetHeight, 1f / (float)num, this.bloomX, bloomRect.x, bloomRect.y, bloomRect.width, bloomRect.height, true);
				num /= 2;
			}
			GL.End();
		}

		private void BlurY()
		{
			this.bloomXY.DiscardContents();
			Graphics.SetRenderTarget(this.bloomXY);
			GL.Clear(false, true, this.clearColor);
			this.bloomX.filterMode = FilterMode.Bilinear;
			this.blurMaterial.SetTexture("_MainTex", this.bloomX);
			this.blurMaterial.SetPass(0);
			GL.Begin(7);
			for (int i = 0; i < this.bloomRects.Count; i++)
			{
				LightPostProcessor.BloomRect bloomRect = this.bloomRects[i];
				this.AddBlurQuads(this.bloomX, bloomRect.x, bloomRect.y, bloomRect.width, bloomRect.height, 1f / (float)this.bloomX.height, this.bloomXY, bloomRect.x, bloomRect.y, bloomRect.width, bloomRect.height, false);
			}
			GL.End();
		}

		private void CombineBloom()
		{
			this.bloomCombined.DiscardContents();
			Graphics.SetRenderTarget(this.bloomCombined);
			GL.Clear(false, true, this.clearColor);
			this.bloomXY.filterMode = FilterMode.Bilinear;
			int num = this.bloomRects.Count + this.bloomStartLevel - this.bloomCombineStartLevel;
			num = Mathf.Clamp(num, 0, this.bloomRects.Count);
			if ((num & 4) != 0)
			{
				this.combineMaterial.EnableKeyword("SAMPLE_4");
			}
			else
			{
				this.combineMaterial.DisableKeyword("SAMPLE_4");
			}
			if ((num & 2) != 0)
			{
				this.combineMaterial.EnableKeyword("SAMPLE_2");
			}
			else
			{
				this.combineMaterial.DisableKeyword("SAMPLE_2");
			}
			if ((num & 1) != 0)
			{
				this.combineMaterial.EnableKeyword("SAMPLE_1");
			}
			else
			{
				this.combineMaterial.DisableKeyword("SAMPLE_1");
			}
			this.combineMaterial.SetTexture("_MainTex", this.bloomXY);
			int num2 = this.bloomRects.Count - num;
			LightPostProcessor.BloomRect bloomRect = this.bloomRects[num2];
			float num3 = 1f;
			float num4 = 0f;
			for (int i = 0; i < num; i++)
			{
				num4 += num3;
				num3 *= this.bloomStrengthMultiplier;
			}
			float num5 = 1f / num4;
			num3 = 1f;
			for (int j = 0; j < num; j++)
			{
				int num6 = num2 + j;
				LightPostProcessor.BloomRect bloomRect2 = this.bloomRects[num6];
				this.combineMaterial.SetFloat("_Weight" + j.ToString(), num3 * num5);
				Vector4 vector = default(Vector4);
				vector.x = (float)bloomRect2.width / (float)bloomRect.width;
				vector.y = (float)bloomRect2.height / (float)bloomRect.height;
				vector.z = ((float)bloomRect2.x - (float)bloomRect.x * vector.x) / (float)this.bloomXY.width;
				vector.w = ((float)bloomRect2.y - (float)bloomRect.y * vector.y) / (float)this.bloomXY.height;
				this.combineMaterial.SetVector("_UvTransform" + j.ToString(), vector);
				num3 *= this.bloomStrengthMultiplier;
			}
			this.combineMaterial.SetPass(0);
			float num7 = (float)bloomRect.x / (float)this.bloomCombined.width;
			float num8 = (float)(bloomRect.x + bloomRect.width) / (float)this.bloomCombined.width;
			float num9 = (float)bloomRect.y / (float)this.bloomCombined.height;
			float num10 = (float)(bloomRect.y + bloomRect.height) / (float)this.bloomCombined.height;
			GL.Begin(7);
			GL.Vertex3(num7, num9, 0f);
			GL.Vertex3(num7, num10, 0f);
			GL.Vertex3(num8, num10, 0f);
			GL.Vertex3(num8, num9, 0f);
			GL.End();
		}

		private void Composite(RenderTexture source, RenderTexture destination)
		{
			if (destination != null)
			{
				destination.DiscardContents();
			}
			Graphics.SetRenderTarget(destination);
			if (this.colorOffset == Vector3.zero && this.colorScale == Vector3.one && this.saturation == 1f)
			{
				this.compositionMaterial.DisableKeyword("COLOR_FILTER");
			}
			else
			{
				this.compositionMaterial.EnableKeyword("COLOR_FILTER");
			}
			int num = this.bloomRects.Count + this.bloomStartLevel - this.bloomCombineStartLevel;
			num = Mathf.Clamp(num, 0, this.bloomRects.Count);
			int num2 = this.bloomRects.Count - num;
			if (num > 0)
			{
				this.compositionMaterial.EnableKeyword("BLOOM_COMBINED");
			}
			else
			{
				this.compositionMaterial.DisableKeyword("BLOOM_COMBINED");
			}
			if ((num2 & 4) != 0)
			{
				this.compositionMaterial.EnableKeyword("BLOOM_4");
			}
			else
			{
				this.compositionMaterial.DisableKeyword("BLOOM_4");
			}
			if ((num2 & 2) != 0)
			{
				this.compositionMaterial.EnableKeyword("BLOOM_2");
			}
			else
			{
				this.compositionMaterial.DisableKeyword("BLOOM_2");
			}
			if ((num2 & 1) != 0)
			{
				this.compositionMaterial.EnableKeyword("BLOOM_1");
			}
			else
			{
				this.compositionMaterial.DisableKeyword("BLOOM_1");
			}
			source.filterMode = FilterMode.Point;
			this.compositionMaterial.SetTexture("_MainTex", source);
			this.bloomXY.filterMode = FilterMode.Bilinear;
			this.compositionMaterial.SetTexture("_BloomTex", this.bloomXY);
			this.bloomCombined.filterMode = FilterMode.Bilinear;
			this.compositionMaterial.SetTexture("_BloomCombinedTex", this.bloomCombined);
			float num3 = 1f;
			float num4 = 0f;
			for (int i = 0; i < this.bloomRects.Count; i++)
			{
				num4 += num3;
				num3 *= this.bloomStrengthMultiplier;
			}
			num3 = this.bloomStrength / num4;
			for (int j = 0; j < num2; j++)
			{
				LightPostProcessor.BloomRect bloomRect = this.bloomRects[j];
				this.compositionMaterial.SetFloat(bloomRect.weightShaderPropertyId, num3);
				Vector4 vector;
				vector.x = (float)bloomRect.width / (float)this.bloomXY.width;
				vector.y = (float)bloomRect.height / (float)this.bloomXY.height;
				vector.z = (float)bloomRect.x / (float)this.bloomXY.width;
				vector.w = (float)bloomRect.y / (float)this.bloomXY.height;
				this.compositionMaterial.SetVector(bloomRect.uvTransformShaderPropertyId, vector);
				num3 *= this.bloomStrengthMultiplier;
			}
			if (num > 0)
			{
				float num5 = 1f;
				float num6 = 0f;
				for (int k = 0; k < num; k++)
				{
					num6 += num5;
					num5 *= this.bloomStrengthMultiplier;
				}
				LightPostProcessor.BloomRect bloomRect2 = this.bloomRects[this.bloomRects.Count - num];
				float num7 = num3 * num6;
				this.compositionMaterial.SetFloat("_BloomWeightCombined", num7);
				Vector4 vector2;
				vector2.x = (float)bloomRect2.width / (float)this.bloomXY.width;
				vector2.y = (float)bloomRect2.height / (float)this.bloomXY.height;
				vector2.z = (float)bloomRect2.x / (float)this.bloomXY.width;
				vector2.w = (float)bloomRect2.y / (float)this.bloomXY.height;
				this.compositionMaterial.SetVector("_BloomUvTransformCombined", vector2);
			}
			this.compositionMaterial.SetPass(0);
			GL.Begin(7);
			GL.Vertex3(0f, 0f, 0f);
			GL.Vertex3(0f, 1f, 0f);
			GL.Vertex3(1f, 1f, 0f);
			GL.Vertex3(1f, 0f, 0f);
			GL.End();
		}

		private void AddBlurQuads(RenderTexture from, int fromX, int fromY, int fromWidth, int fromHeight, float offsetScale, RenderTexture to, int toX, int toY, int toWidth, int toHeight, bool forX)
		{
			float num = (float)toX / (float)to.width;
			float num2 = (float)(toX + toWidth) / (float)to.width;
			float num3 = (float)toY / (float)to.height;
			float num4 = (float)(toY + toHeight) / (float)to.height;
			float num5 = (float)fromX / (float)from.width;
			float num6 = (float)(fromX + fromWidth) / (float)from.width;
			float num7 = (float)fromY / (float)from.height;
			float num8 = (float)(fromY + fromHeight) / (float)from.height;
			float num9 = this.bloomSamples[0].offset * offsetScale;
			float num10 = this.bloomSamples[0].offset * offsetScale;
			float num11 = this.bloomSamples[1].offset * offsetScale;
			float num12 = this.bloomSamples[1].offset * offsetScale;
			float num13 = this.bloomSamples[2].offset * offsetScale;
			float num14 = this.bloomSamples[2].offset * offsetScale;
			float num15 = this.bloomSamples[3].offset * offsetScale;
			float num16 = this.bloomSamples[3].offset * offsetScale;
			if (forX)
			{
				num12 = (num10 = (num14 = (num16 = 0f)));
			}
			else
			{
				num11 = (num9 = (num13 = (num15 = 0f)));
			}
			GL.MultiTexCoord3(0, num5 + num9, num7 + num10, this.bloomSamples[0].weight);
			GL.MultiTexCoord3(1, num5 + num11, num7 + num12, this.bloomSamples[1].weight);
			GL.MultiTexCoord3(2, num5 + num13, num7 + num14, this.bloomSamples[2].weight);
			GL.MultiTexCoord3(3, num5 + num15, num7 + num16, this.bloomSamples[3].weight);
			GL.Vertex3(num, num3, 0f);
			GL.MultiTexCoord3(0, num5 + num9, num8 + num10, this.bloomSamples[0].weight);
			GL.MultiTexCoord3(1, num5 + num11, num8 + num12, this.bloomSamples[1].weight);
			GL.MultiTexCoord3(2, num5 + num13, num8 + num14, this.bloomSamples[2].weight);
			GL.MultiTexCoord3(3, num5 + num15, num8 + num16, this.bloomSamples[3].weight);
			GL.Vertex3(num, num4, 0f);
			GL.MultiTexCoord3(0, num6 + num9, num8 + num10, this.bloomSamples[0].weight);
			GL.MultiTexCoord3(1, num6 + num11, num8 + num12, this.bloomSamples[1].weight);
			GL.MultiTexCoord3(2, num6 + num13, num8 + num14, this.bloomSamples[2].weight);
			GL.MultiTexCoord3(3, num6 + num15, num8 + num16, this.bloomSamples[3].weight);
			GL.Vertex3(num2, num4, 0f);
			GL.MultiTexCoord3(0, num6 + num9, num7 + num10, this.bloomSamples[0].weight);
			GL.MultiTexCoord3(1, num6 + num11, num7 + num12, this.bloomSamples[1].weight);
			GL.MultiTexCoord3(2, num6 + num13, num7 + num14, this.bloomSamples[2].weight);
			GL.MultiTexCoord3(3, num6 + num15, num7 + num16, this.bloomSamples[3].weight);
			GL.Vertex3(num2, num3, 0f);
		}

		private void Blit(RenderTexture from, int fromX, int fromY, int fromWidth, int fromHeight, RenderTexture to, int toX, int toY, int toWidth, int toHeight, bool clear, Color clearColor)
		{
			float num = (float)toX / (float)to.width;
			float num2 = (float)(toX + toWidth) / (float)to.width;
			float num3 = (float)toY / (float)to.height;
			float num4 = (float)(toY + toHeight) / (float)to.height;
			float num5 = (float)fromX / (float)from.width;
			float num6 = (float)(fromX + fromWidth) / (float)from.width;
			float num7 = (float)fromY / (float)from.height;
			float num8 = (float)(fromY + fromHeight) / (float)from.height;
			if (RenderTexture.active != to)
			{
				to.DiscardContents();
				Graphics.SetRenderTarget(to);
			}
			if (clear)
			{
				GL.Clear(false, true, clearColor);
			}
			GL.Begin(7);
			GL.TexCoord2(num5, num7);
			GL.Vertex3(num, num3, 0f);
			GL.TexCoord2(num5, num8);
			GL.Vertex3(num, num4, 0f);
			GL.TexCoord2(num6, num8);
			GL.Vertex3(num2, num4, 0f);
			GL.TexCoord2(num6, num7);
			GL.Vertex3(num2, num3, 0f);
			GL.End();
		}

		public void SetColorFilter(Vector3 colorOffset, Vector3 colorScale, float saturation)
		{
			this.colorOffset = colorOffset;
			this.colorScale = colorScale;
			this.saturation = saturation;
			this.SetColorTransform();
		}

		private void SetupRenderTargets(RenderTexture source)
		{
			if (this.prevSource != null && source != null && source.width == this.prevSource.width && source.height == this.prevSource.height)
			{
				return;
			}
			if (this.maxBloomLevelCount == 0)
			{
				return;
			}
			RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;
			if (this.useARGB2101010 && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB2101010))
			{
				renderTextureFormat = RenderTextureFormat.ARGB2101010;
			}
			int num = source.width >> this.bloomStartLevel;
			int num2 = source.height >> this.bloomStartLevel;
			this.brightness = new RenderTexture(X.beki_min(num), X.beki_min(num2), 0, renderTextureFormat);
			this.brightness.name = "brightness";
			this.brightness.useMipMap = true;
			this.brightness.filterMode = FilterMode.Bilinear;
			this.bloomRects = new List<LightPostProcessor.BloomRect>();
			int num3;
			int num4;
			this.CalcBloomRenderTextureArrangement(out num3, out num4, this.bloomRects, num, num2, 16, this.maxBloomLevelCount);
			Debug.Log(string.Concat(new string[]
			{
				"LightPostProcessor.SetupRenderTargets(): create RTs. ",
				this.brightness.width.ToString(),
				"x",
				this.brightness.height.ToString(),
				" + ",
				num3.ToString(),
				"x",
				num4.ToString(),
				" levels:",
				this.bloomRects.Count.ToString()
			}));
			this.bloomX = new RenderTexture(num3, num4, 0, renderTextureFormat);
			this.bloomX.name = "bloomX";
			this.bloomX.filterMode = FilterMode.Bilinear;
			this.bloomXY = new RenderTexture(num3, num4, 0, renderTextureFormat);
			this.bloomXY.name = "bloomXY";
			this.bloomXY.filterMode = FilterMode.Bilinear;
			this.bloomCombined = new RenderTexture(num3, num4, 0, renderTextureFormat);
			this.bloomCombined.name = "bloomCombined";
			this.prevSource = source;
		}

		private void CalcGaussianSamples(float sigma)
		{
			float num = LightPostProcessor.Gauss(sigma, 0f) * 0.5f;
			float num2 = LightPostProcessor.Gauss(sigma, 1f);
			float num3 = LightPostProcessor.Gauss(sigma, 2f);
			float num4 = LightPostProcessor.Gauss(sigma, 3f);
			float num5 = LightPostProcessor.Gauss(sigma, 4f);
			float num6 = LightPostProcessor.Gauss(sigma, 5f);
			float num7 = LightPostProcessor.Gauss(sigma, 6f);
			float num8 = LightPostProcessor.Gauss(sigma, 7f);
			float num9 = num + num2;
			float num10 = 0f + num2 / num9;
			float num11 = num3 + num4;
			float num12 = 2f + num4 / num11;
			float num13 = num5 + num6;
			float num14 = 4f + num6 / num13;
			float num15 = num7 + num8;
			float num16 = 6f + num8 / num15;
			float num17 = (num9 + num11 + num13 + num15) * 2f;
			num9 /= num17;
			num11 /= num17;
			num13 /= num17;
			num15 /= num17;
			this.SetGaussSample(0, num10, num9);
			this.SetGaussSample(1, num12, num11);
			this.SetGaussSample(2, num14, num13);
			this.SetGaussSample(3, num16, num15);
		}

		private void SetGaussSample(int index, float offset, float weight)
		{
			LightPostProcessor.BloomSample bloomSample = this.bloomSamples[index];
			bloomSample.offset = offset;
			bloomSample.weight = weight;
			this.bloomSamples[index] = bloomSample;
		}

		private static float Gauss(float sigma, float x)
		{
			float num = sigma * sigma;
			return Mathf.Exp(-(x * x) / (2f * num));
		}

		private void CalcBloomRenderTextureArrangement(out int widthOut, out int heightOut, List<LightPostProcessor.BloomRect> rects, int width, int height, int padding, int levelCount)
		{
			bool flag = height > width;
			int num = padding;
			int num2 = padding;
			int num3 = 0;
			int num4 = 0;
			while (levelCount > 0 && width > 0 && height > 0)
			{
				LightPostProcessor.BloomRect bloomRect;
				bloomRect.x = num;
				bloomRect.y = num2;
				bloomRect.width = width;
				bloomRect.height = height;
				bloomRect.uvTransformShaderPropertyId = Shader.PropertyToID("_BloomUvTransform" + rects.Count.ToString());
				bloomRect.weightShaderPropertyId = Shader.PropertyToID("_BloomWeight" + rects.Count.ToString());
				rects.Add(bloomRect);
				num3 = Math.Max(num3, num + width + padding);
				num4 = Math.Max(num4, num2 + height + padding);
				if (flag)
				{
					num += width + padding;
				}
				else
				{
					num2 += height + padding;
				}
				flag = !flag;
				if (width % 4 == 0)
				{
					int num5 = height % 4;
				}
				width /= 2;
				height /= 2;
				if (width < this.minBloomLevelSize || height < this.minBloomLevelSize)
				{
					break;
				}
				levelCount--;
			}
			widthOut = num3;
			heightOut = num4;
			this.first = true;
		}

		private void SetColorTransform()
		{
			if (this.compositionMaterial == null)
			{
				return;
			}
			Matrix4x4 matrix4x = Matrix4x4.Translate(new Vector3(this.colorOffset.x, this.colorOffset.y, this.colorOffset.z)) * Matrix4x4.Scale(new Vector3(this.colorScale.x, this.colorScale.y, this.colorScale.z));
			Matrix4x4 matrix4x2 = default(Matrix4x4);
			matrix4x2.SetRow(0, new Vector4(0.299f, 0.587f, 0.114f, 0f));
			matrix4x2.SetRow(1, new Vector4(-0.169f, -0.331f, 0.5f, 0f));
			matrix4x2.SetRow(2, new Vector4(0.5f, -0.419f, -0.081f, 0f));
			matrix4x2.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
			Matrix4x4 matrix4x3 = Matrix4x4.Scale(new Vector3(1f, this.saturation, this.saturation));
			Matrix4x4 matrix4x4 = default(Matrix4x4);
			matrix4x4.SetRow(0, new Vector4(1f, 0f, 1.402f, 0f));
			matrix4x4.SetRow(1, new Vector4(1f, -0.344f, -0.714f, 0f));
			matrix4x4.SetRow(2, new Vector4(1f, 1.772f, 0f, 0f));
			matrix4x4.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
			Matrix4x4 matrix4x5 = matrix4x * matrix4x4 * matrix4x3 * matrix4x2;
			this.compositionMaterial.SetVector("_ColorTransformR", matrix4x5.GetRow(0));
			this.compositionMaterial.SetVector("_ColorTransformG", matrix4x5.GetRow(1));
			this.compositionMaterial.SetVector("_ColorTransformB", matrix4x5.GetRow(2));
		}

		public Vector3 colorOffset = new Vector3(0f, 0f, 0f);

		public Vector3 colorScale = new Vector3(1f, 1f, 1f);

		public float saturation = 1f;

		public float bloomPixelThreshold;

		public float bloomStrength = 1f;

		public float bloomStrengthMultiplier = 2f;

		public int bloomStartLevel = 2;

		public int bloomCombineStartLevel = 1;

		public int maxBloomLevelCount = 7;

		public int minBloomLevelSize = 16;

		public float bloomSigmaInPixel = 3f;

		public bool useARGB2101010;

		private Material extractionMaterial;

		private Material blurMaterial;

		private Material combineMaterial;

		private Material compositionMaterial;

		private RenderTexture prevSource;

		private RenderTexture brightness;

		private RenderTexture bloomX;

		private RenderTexture bloomXY;

		private RenderTexture bloomCombined;

		private List<LightPostProcessor.BloomRect> bloomRects;

		private LightPostProcessor.BloomSample[] bloomSamples;

		private bool first = true;

		private readonly Color clearColor = new Color(0f, 0f, 0f, 1f);

		private struct BloomSample
		{
			public float offset;

			public float weight;
		}

		private struct BloomRect
		{
			public int x;

			public int y;

			public int width;

			public int height;

			public int uvTransformShaderPropertyId;

			public int weightShaderPropertyId;
		}
	}
}

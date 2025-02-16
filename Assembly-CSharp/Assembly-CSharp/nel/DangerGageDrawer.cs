using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class DangerGageDrawer
	{
		public DangerGageDrawer(MeshDrawer Md, MeshRenderer Mrd = null, bool auto_connect_mrd = false)
		{
			if (Md != null)
			{
				this.initMesh(Md, Mrd, auto_connect_mrd, 1f, -1);
			}
		}

		public void initMesh(MeshDrawer Md, MeshRenderer Mrd = null, bool auto_connect_mrd = false, float scale = 1f, int stencil_ref = -1)
		{
			if (this.MtrF == null)
			{
				this.MtrF = MTR.newMtr("Hachan/RakuraiGlitch");
				this.PFBack = MTRX.getPF("nel_danger_gage_back");
				PxlFrame pf = MTRX.getPF("gage_filled");
				this.BaseImg = this.PFBack.getLayer(0).Img;
				this.FillImg = pf.getLayer(0).Img;
				MTRX.setMaterialST(this.MtrF, "_MainTex", this.FillImg, 2f);
				this.MtrF.SetFloat("_RGBNoiseScaleX", 6f);
				this.MtrF.SetFloat("_RGBNoiseScaleY", 0.44f);
			}
			this.MtrF.SetFloat("_GlitchXScale", 0.021639999f * scale);
			this.MtrF.SetFloat("_GlitchYScale", 0.014079999f * scale);
			if (stencil_ref >= 0)
			{
				this.MtrF.SetFloat("_StencilRef", (float)stencil_ref);
				this.MtrF.SetFloat("_StencilComp", 3f);
			}
			else
			{
				this.MtrF.SetFloat("_StencilComp", 8f);
			}
			this.ran0 = X.xors() & 16777215U;
			Md.clear(false, false);
			Md.chooseSubMesh(1, false, true);
			Md.setMaterial(MTR.MIiconL.getMtr(BLEND.NORMAL, stencil_ref), false);
			Md.base_z = 0.0001f;
			Md.chooseSubMesh(2, false, true);
			Md.setMaterial(MTR.MIiconL.getMtr(BLEND.SUB, stencil_ref), false);
			Md.chooseSubMesh(3, false, true);
			Md.base_z = -0.0001f;
			Md.setMaterial(this.MtrF, false);
			Md.chooseSubMesh(4, false, true);
			Md.base_z = -0.0002f;
			Md.setMaterial((stencil_ref >= 0) ? MTRX.getMtr(stencil_ref) : MTRX.MtrMeshNormal, false);
			if (Mrd != null)
			{
				if (auto_connect_mrd)
				{
					Md.connectRendererToTriMulti(Mrd);
					return;
				}
				Mrd.sharedMaterials = Md.getMaterialArray(false);
			}
		}

		public void destruct()
		{
			if (this.MtrF != null)
			{
				IN.DestroyOne(this.MtrF);
			}
		}

		public Vector2 getPos(int i)
		{
			int num = i % 3;
			int num2 = i / 3;
			Vector2 vector = new Vector2((float)num2 * 64f - 160f, 0f);
			if (num == 0)
			{
				return vector;
			}
			vector.x += 32f;
			vector.y += 32f * (float)X.MPF(num == 1);
			return vector;
		}

		private C32 getColor(C32 C, int i)
		{
			int num = 0;
			int num2 = this.Acol.Length;
			bool flag = false;
			int j;
			int num3;
			for (;;)
			{
				for (j = 1; j <= num2; j++)
				{
					num3 = this.Acol_separat[j - 1];
					if (num3 >= i)
					{
						goto Block_1;
					}
					num = num3;
				}
				flag = true;
				i -= 30;
				num -= 30;
			}
			Block_1:
			C.Set(this.Acol[j - 1]).blend(this.Acol[j % num2], (float)(i - num) / (float)(num3 - num));
			C = (flag ? C.blend(4294901760U, 0.4f) : C);
			return C.multiply(C.C, false);
		}

		public Mesh Redraw(MeshDrawer Md, float af, bool force = false, float alpha = 1f)
		{
			if (this.t == af && !force)
			{
				return Md.getMesh();
			}
			if (this.t == -2f || this.t > af)
			{
				this.ran0 = X.xors() & 16777215U;
			}
			this.t = af;
			if (this.auto_clear)
			{
				Md.resetVertexCount();
			}
			Md.chooseSubMesh(3, false, true);
			Md.chooseSubMesh(2, false, true);
			Md.chooseSubMesh(4, false, true);
			Md.chooseSubMesh(1, false, true);
			float num = (float)this.BaseImg.width * 0.5f * 0.015625f;
			float num2 = (float)this.BaseImg.height * 0.5f * 0.015625f;
			float num3 = -0.125f;
			Md.uvRect(-num + Md.base_x, -num2 + Md.base_y + num3, num * 2f, num2 * 2f, this.BaseImg, true, false);
			Md.Col = C32.MulA(4283780170U, alpha);
			Md.RectBL(-num + Md.base_x, -num2 + num3, num * X.ZPOWN(this.t, 44f, 3f) * 2f, num2 * 2f, true);
			if (this.t >= 20f)
			{
				float num4 = 0.6567164f;
				Color32 color = C32.MulA(4282992969U, alpha);
				int i = 0;
				while (i < 16)
				{
					int num5 = i;
					float num6 = this.t - 20f - (float)this.appear_delay - (float)(this.daia_delay * i);
					float num7 = ((i < this.already_show) ? ((float)(this.daia_delay * 16 * 20 + 256)) : num6);
					if (num7 < 0f)
					{
						goto IL_059D;
					}
					for (int j = num5 + 16; j < this.value_; j += 16)
					{
						float num8 = this.t - (float)(this.daia_delay * j);
						if (num8 < 0f)
						{
							break;
						}
						num7 = num8;
						num5 = j;
					}
					Vector2 vector = this.getPos(i);
					uint ran = X.GETRAN2((int)(this.ran0 + (uint)(num5 * 17) + (uint)(i * 9)), (int)(this.ran0 % 6U + (uint)(num5 % 7)));
					if (num5 >= this.value_)
					{
						goto IL_059D;
					}
					Md.chooseSubMesh(3, false, false);
					Md.allocUv2(4, false).allocUv3(4, false);
					Md.Uv2(256f * this.speed_ratio / (24f + 8f * X.RAN(ran, 550)), 256f * this.speed_ratio / (4f + 7f * X.RAN(ran, 1592)), false);
					Md.Uv3(256f * this.speed_ratio / (200f + 40f * X.RAN(ran, 2705)), 256f * this.speed_ratio / (165f + 30f * X.RAN(ran, 2964)), false);
					float num9 = num4;
					float num10 = num4;
					if (num7 < 7f)
					{
						vector.x += (X.RAN(ran, 573) - 0.5f) * 2f * 8f;
						vector.y += (X.RAN(ran, 1696) - 0.5f) * 2f * 8f;
						num9 *= 1f + (X.RAN(ran, 1067) - 0.5f) * 2f * 0.11f;
						num10 *= 1f + (X.RAN(ran, 2374) - 0.5f) * 2f * 0.25f;
					}
					C32 color2 = this.getColor(Md.ColGrd, num5);
					float num11 = X.MMX(0f, 1f - X.ZSIN(num7, 10f) + 0.125f * X.SINI(num7 + X.RAN(ran, 2306) * 400f, 8.4f + X.RAN(ran, 1581) * 2.7f) + 0.125f * X.SINI(num7 + X.RAN(ran, 1372) * 400f, 19.4f + X.RAN(ran, 2913) * 4.1f), 1f);
					Md.Col = color2.blend(uint.MaxValue, num11).mulA(alpha).C;
					float num12 = ((float)this.FillImg.width * 0.5f - 2f) * 0.015625f * num9;
					float num13 = ((float)this.FillImg.height * 0.5f - 2f) * 0.015625f * num10;
					vector *= 0.015625f;
					Md.uvRect(vector.x - num12 - 0.03125f * num9 + Md.base_x, vector.y - num13 - 0.03125f * num10 + Md.base_y, num12 * 2f + 0.0625f * num9, num13 * 2f + 0.0625f * num10, this.FillImg, true, false);
					Md.RectBL(vector.x - num12, vector.y - num13, num12 * 2f, num13 * 2f, true);
					Md.allocUv2(0, true).allocUv3(0, true);
					Md.chooseSubMesh(2, false, false);
					Md.Col = C32.MulA(4285558896U, alpha);
					Md.RectBL(vector.x - num12, vector.y - num13, num12 * 2f, num13 * 2f, true);
					IL_069A:
					i++;
					continue;
					IL_059D:
					if (this.already_show != 0)
					{
						num7 = 128f;
					}
					else
					{
						num7 = this.t - 20f - (float)(this.daia_delay * i);
						if (num7 < 0f)
						{
							goto IL_069A;
						}
					}
					Vector2 pos = this.getPos(i);
					uint ran2 = X.GETRAN2((int)(this.ran0 + (uint)(num5 * 17) + (uint)(i * 9)), (int)(this.ran0 % 6U + (uint)(num5 % 7)));
					Md.chooseSubMesh(4, false, false);
					Md.Col = color;
					if (num7 < 4f)
					{
						pos.x += (X.RAN(ran2, 1172) - 0.5f) * 2f * 8f;
						pos.y += (X.RAN(ran2, 2754) - 0.5f) * 2f * 8f;
					}
					Md.Daia3(pos.x, pos.y, 44f, 44f, 7f, 7f, false);
					goto IL_069A;
				}
			}
			Md.updateForMeshRenderer(true);
			return Md.getMesh();
		}

		public void fixAppearTime(int i, float fix_time = -1f)
		{
			if (fix_time >= 0f)
			{
				this.t = fix_time;
			}
			this.appear_delay = (int)(this.t - 20f - (float)(this.daia_delay * i));
		}

		public int val
		{
			get
			{
				return this.value_;
			}
			set
			{
				if (this.value_ != value)
				{
					this.value_ = value;
					this.t = X.Mn(this.t, -1f);
				}
			}
		}

		private const int MID_BASE = 1;

		private const int MID_SUB = 2;

		private const int MID_FILL = 3;

		public const int MID_EMPTY = 4;

		private Material MtrF;

		private PxlImage BaseImg;

		private PxlFrame PFBack;

		private PxlImage FillImg;

		private const float gage_w = 320f;

		private const float cell_wh = 44f;

		public int daia_delay = 3;

		public float speed_ratio = 1f;

		private float t = -2f;

		public const int max_memori = 16;

		private const float delay_memori = 20f;

		private int value_;

		private uint ran0;

		private const float marg = 2f;

		public int already_show;

		public int appear_delay = 60;

		public bool auto_clear = true;

		public uint[] Acol = new uint[] { 4290052078U, 4294951553U, 4282865663U, 4288589740U, 4292845411U, 4287236529U, 4280806229U, 4286874803U, 4292477760U };

		public int[] Acol_separat = new int[] { 7, 10, 13, 17, 20, 23, 27, 30, 33 };
	}
}

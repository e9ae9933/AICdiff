using System;
using Better;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2CImgDrawerLight : M2CImgDrawerWithEffect
	{
		public M2CImgDrawerLight(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, false)
		{
			if (M2CImgDrawerLight.OMesh == null)
			{
				M2CImgDrawerLight.OMesh = new BDic<uint, PxlMeshDrawer>(4);
			}
			M2CImgDrawerLight.OMesh.Clear();
			this.no_basic_draw = true;
			this.effect_2_ed = true;
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer PMesh)
		{
			if (base.Mp.apply_chip_effect)
			{
				return false;
			}
			M2CImgDrawerLight.OMesh.Clear();
			base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, PMesh);
			return false;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.Lg != null)
			{
				base.Mp.remLight(this.Lg);
			}
			this.PMesh = X.Get<uint, PxlMeshDrawer>(M2CImgDrawerLight.OMesh, this.Cp.Img.chip_id);
			bool flag = false;
			int num2;
			Vector3[] array5;
			Vector2[] array6;
			if (this.PMesh == null)
			{
				this.PMesh = (M2CImgDrawerLight.OMesh[this.Cp.Img.chip_id] = new PxlMeshDrawer(false));
				string[] array = base.Meta.Get("light_tri");
				string[] array2 = base.Meta.Get("light_mesh");
				string[] array3 = base.Meta.Get("light_col");
				float nm = base.Meta.GetNm("light_mesh_multiple", 1f, 0);
				int num = array.Length;
				int[] array4 = new int[num];
				flag = true;
				num2 = array2.Length / 2;
				array5 = new Vector3[num2];
				array6 = new Vector2[num2];
				Color32[] array7 = null;
				C32 colb = MTRX.colb;
				if (array3 != null)
				{
					int num3 = array3.Length;
					if (num3 != 0)
					{
						array7 = new Color32[num2];
						for (int i = 0; i < num2; i++)
						{
							string text = array3[i % num3];
							if (REG.match(text, REG.RegSmallNumberWhole))
							{
								Color32 brightColor = base.Mp.Dgn.BrightColor;
								brightColor.a = (byte)(255f * X.Nm(text, 0f, false));
								array7[i] = brightColor;
							}
							else
							{
								array7[i] = C32.d2c(X.NmUI(text, 0U, true, true));
							}
						}
					}
				}
				if (array7 == null)
				{
					array7 = new Color32[num2];
					for (int j = 0; j < num2; j++)
					{
						array7[j] = base.Mp.Dgn.BrightColor;
					}
				}
				for (int k = 0; k < num2; k++)
				{
					array5[k] = new Vector3((float)TX.eval(array2[k * 2], "") * nm * 0.015625f, (float)TX.eval(array2[k * 2 + 1], "") * nm * 0.015625f, 0f);
				}
				for (int l = 0; l < num; l++)
				{
					array4[l] = X.NmI(array[l], 0, false, false);
				}
				this.PMesh.setRawVerticesAndTriangles(array6, array7, array5, array4, -1, -1);
			}
			else
			{
				array5 = this.PMesh.getRawVerticeArray(out num2);
				array6 = this.PMesh.getRawUvArray(out num2);
			}
			for (int m = 0; m < num2; m++)
			{
				Vector3 vector = array5[m] * 64f / base.Mp.CLENB;
				if (m == 0)
				{
					this.lx = vector.x;
					this.rx = vector.x;
					this.ty = vector.y;
					this.by = vector.y;
				}
				else
				{
					this.lx = X.Mn(vector.x, this.lx);
					this.ty = X.Mn(vector.y, this.ty);
					this.rx = X.Mx(vector.x, this.rx);
					this.by = X.Mx(vector.y, this.by);
				}
			}
			if (flag)
			{
				float num4 = this.rx - this.lx;
				float num5 = this.by - this.ty;
				PxlImage iconWhite = MTRX.IconWhite;
				for (int n = 0; n < num2; n++)
				{
					Vector3 vector2 = array5[n] * 64f / base.Mp.CLENB;
					array6[n] = new Vector2((num4 == 0f) ? iconWhite.RectIUv.x : (iconWhite.RectIUv.x + iconWhite.RectIUv.width * (vector2.x - this.lx) / num4), (num5 == 0f) ? iconWhite.RectIUv.y : (iconWhite.RectIUv.y + iconWhite.RectIUv.height * (vector2.y - this.ty) / num5));
				}
			}
			this.light_effect_level = (this.light_apply_level = 1f);
			string[] array8 = base.Meta.Get("light_effect_level");
			if (array8 != null)
			{
				this.light_effect_level = X.Mn(base.Meta.GetNm("light_effect_level", 1f, 0), 1f);
				if (array8.Length > 1)
				{
					this.light_apply_level = X.Mn(base.Meta.GetNm("light_effect_level", 1f, 1), 1f);
				}
			}
			base.Mp.addLight((this.Lg = new M2LightFn(base.Mp, new M2LightFn.FnDrawLight(this.fnDrawLight), null, null)).Pos(this.Cp.mapcx, this.Cp.mapcy));
			this.Lg.showing_delay = 50;
			string s = base.Meta.GetS("ptcst");
			if (TX.valid(s))
			{
				this.Ptc = base.Mp.PtcSTsetVar("cx", (double)this.Cp.mapcx).PtcSTsetVar("cy", (double)this.Cp.mapcy).PtcSTsetVar("rotation", (double)((this.Cp is M2Picture) ? ((float)this.Cp.rotation / 180f * 3.1415927f) : ((float)this.Cp.rotation * 1.5707964f)))
					.PtcSTsetVar("flip", (double)(this.Cp.flip ? 1 : 0))
					.PtcST(s, null, PTCThread.StFollow.NO_FOLLOW);
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (this.Lg != null)
			{
				base.Mp.remLight(this.Lg);
				this.Lg = null;
			}
			if (this.Ptc != null)
			{
				this.Ptc.destruct();
				this.Ptc = null;
			}
		}

		private void fnDrawLight(MeshDrawer Md, M2LightFn Lt, float x, float y, float scale, float alpha)
		{
			Md.Col = Md.ColGrd.White().multiply(this.light_apply_level, this.light_apply_level, this.light_apply_level, alpha).C;
			Md.RotaMesh(x, y, scale / base.Mp.base_scale, scale / base.Mp.base_scale, this.Cp.draw_rotR, this.PMesh, this.Cp.flip, true);
			Md.Identity();
		}

		protected override bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.Cp.mapcx;
			Ef.y = this.Cp.mapcy;
			if (Ed.isinCamera(Ef.x + (this.lx + this.rx) / 2f, Ef.y + (this.ty + this.by) / 2f, (this.rx - this.lx) / 2f + 54f, (this.by - this.ty) / 2f + 54f))
			{
				MeshDrawer mesh = Ef.GetMesh("", C32.c2d(base.Mp.Dgn.BrightColor), BLEND.ADD, true);
				mesh.Identity();
				mesh.Col = C32.MulA(uint.MaxValue, this.light_effect_level);
				mesh.RotaMesh(0f, 0f, 1f, 1f, this.Cp.draw_rotR, this.PMesh, this.Cp.flip, true);
			}
			return true;
		}

		private M2LightFn Lg;

		private static BDic<uint, PxlMeshDrawer> OMesh;

		private PxlMeshDrawer PMesh;

		private float lx;

		private float ty;

		private float rx;

		private float by;

		private float light_effect_level = 1f;

		private float light_apply_level = 1f;

		private PTCThread Ptc;
	}
}

using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerPentachoron4D : M2CImgDrawerWithEffect, IRunAndDestroy
	{
		public M2CImgDrawerPentachoron4D(MeshDrawer Md, int lay, M2Puts _Cp)
			: base(Md, lay, _Cp, false)
		{
			this.set_effect_on_initaction = true;
			this.effect_2_ed = false;
			this.no_basic_draw = true;
			this.ATDr = new M2CImgDrawerPentachoron4D.P4D[4];
			Dim4DDrawer.FnDrawPoint fnDrawPoint = new Dim4DDrawer.FnDrawPoint(this.DrawPoint);
			Dim4DDrawer.FnDrawLine fnDrawLine = new Dim4DDrawer.FnDrawLine(this.DrawLine);
			Dim4DDrawer.FnDrawSurface fnDrawSurface = new Dim4DDrawer.FnDrawSurface(this.DrawSurface);
			this.tdr_i = 0;
			while (this.tdr_i < 4)
			{
				this.ATDr[this.tdr_i] = new M2CImgDrawerPentachoron4D.P4D(new Pentachoron4D2Drawer
				{
					level_tale = 0.45f,
					FD_DrawPoint = fnDrawPoint,
					FD_DrawLine = fnDrawLine,
					FD_DrawSurface = fnDrawSurface,
					Rz_length = 80f,
					Rw_length = 11f
				});
				this.tdr_i++;
			}
		}

		public void destruct()
		{
		}

		public bool run(float fcnt)
		{
			float base_floort = base.Mp.base_floort;
			if (this.floort_fine <= base_floort)
			{
				this.tdr_i = 0;
				while (this.tdr_i < 4)
				{
					M2CImgDrawerPentachoron4D.P4D p4D = this.ATDr[this.tdr_i];
					if (this.floort_fine == 0f)
					{
						p4D.ywagR = X.XORSPS() * 3.1415927f;
						p4D.yzagR = X.XORSPS() * 3.1415927f;
						p4D.xyagR = X.XORSPS() * 3.1415927f;
						p4D.d_xz_intv = X.NI(300, 450, X.XORSP());
					}
					p4D.d_ywagR = 6.2831855f / X.NI(320, 520, X.XORSP());
					p4D.d_yzagR = 6.2831855f / X.NI(655, 950, X.XORSP());
					p4D.d_xyagR = 6.2831855f / X.NI(1030, 1520, X.XORSP());
					this.ATDr[this.tdr_i] = p4D;
					this.tdr_i++;
				}
				this.floort_fine = base_floort + X.NI(380, 990, X.XORSP());
			}
			this.tdr_i = 0;
			while (this.tdr_i < 4)
			{
				uint ran = X.GETRAN2(this.Cp.index + 13 + this.tdr_i * 7, 3 + this.tdr_i);
				M2CImgDrawerPentachoron4D.P4D p4D2 = this.ATDr[this.tdr_i];
				p4D2.ywagR += fcnt * p4D2.d_ywagR;
				p4D2.yzagR += fcnt * p4D2.d_yzagR;
				p4D2.xyagR += fcnt * p4D2.d_xyagR;
				p4D2.xzagR = X.NI(10, 18, X.RAN(ran, 1811)) / 180f * 3.1415927f * X.COSI(base_floort + X.RAN(ran, 1628) * 320f, p4D2.d_xz_intv);
				this.tdr_i++;
			}
			return true;
		}

		public override void initAction(bool normal_map)
		{
			this.floort_fine = 0f;
			base.Mp.addRunnerObject(this);
			base.initAction(normal_map);
			this.calpha = 1f;
			if (this.Ed.Con.SubMapData != null)
			{
				this.calpha = ((this.Ed.Con.SubMapData.camera_length >= 1f) ? X.saturate(1f - (this.Ed.Con.SubMapData.camera_length - 1f) / 1.44f) : X.NIL(1f, 0.33f, X.Abs(this.Ed.Con.SubMapData.camera_length - 1f), 0.77f));
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.Mp.remRunnerObject(this);
			base.closeAction(when_map_close);
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		protected override bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.Cp.mapcx;
			Ef.y = this.Cp.mapcy;
			if (this.floort_fine == 0f || !Ed.isinCamera(Ef, 162.5f, 227.5f))
			{
				return true;
			}
			M2CImgDrawerPentachoron4D.MdS = Ef.GetMesh("p4d", MTR.MtrNDSub, true);
			M2CImgDrawerPentachoron4D.MdA = Ef.GetMesh("p4d", MTR.MtrNDAdd, false);
			M2CImgDrawerPentachoron4D.MdI = Ef.GetMeshImg("p4d", MTRX.MIicon, BLEND.ADD, false);
			if (M2CImgDrawerPentachoron4D.MdA.getTriMax() > 0)
			{
				M2CImgDrawerPentachoron4D.MdA.base_z += 0.15f;
			}
			this.tdr_i = 0;
			while (this.tdr_i < 4)
			{
				ref M2CImgDrawerPentachoron4D.P4D ptr = this.ATDr[this.tdr_i];
				Matrix4x4 matrix4x = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, (float)(this.tdr_i * 90))) * Matrix4x4.Translate(new Vector3(0f, -4.405753f, 0f));
				if (this.tdr_i % 2 == 1)
				{
					matrix4x = Matrix4x4.Scale(new Vector3(0.68f, 0.443f, 1f)) * matrix4x;
				}
				else
				{
					matrix4x = Matrix4x4.Scale(new Vector3(0.7f, 1f, 1f)) * matrix4x;
				}
				M2CImgDrawerPentachoron4D.MdS.setCurrentMatrix(matrix4x, false);
				M2CImgDrawerPentachoron4D.MdA.setCurrentMatrix(matrix4x, false);
				M2CImgDrawerPentachoron4D.MdI.setCurrentMatrix(matrix4x, false);
				ptr.Dr.drawTo(M2CImgDrawerPentachoron4D.MdS, 130f);
				this.tdr_i++;
			}
			return true;
		}

		private void DrawLine(MeshDrawer _Md, Vector4 P0, Vector4 P1)
		{
			M2CImgDrawerPentachoron4D.MdI.Col = M2CImgDrawerPentachoron4D.MdI.ColGrd.White().mulA(0.5f * this.calpha).C;
			M2CImgDrawerPentachoron4D.MdI.ColGrd.mulA(0f);
			float num = X.NI(P0.x, P1.x, 0.5f) * 0.015625f;
			float num2 = X.NI(P0.y, P1.y, 0.5f) * 0.015625f;
			float num3 = X.Abs(P0.x - P1.x) * 1.25f * 0.015625f;
			float num4 = X.Abs(P0.y - P1.y) * 1.25f * 0.015625f;
			M2CImgDrawerPentachoron4D.MdI.uvRect(M2CImgDrawerPentachoron4D.MdI.base_x + num - num3 * 0.5f, M2CImgDrawerPentachoron4D.MdI.base_y + num2 - num4 * 0.5f, num3, num4, MTRX.IconWhite, true, false);
			M2CImgDrawerPentachoron4D.MdI.BlurLine(P0.x, P0.y, P1.x, P1.y, 20f, 0, 20f, false);
		}

		private void DrawPoint(MeshDrawer _Md, Vector4 P0)
		{
			M2CImgDrawerPentachoron4D.MdI.Col = M2CImgDrawerPentachoron4D.MdI.ColGrd.White().mulA(1f * this.calpha).C;
			M2CImgDrawerPentachoron4D.MdI.initForImg(MTRX.EffBlurCircle245, 0);
			float num = 12f * P0.w;
			M2CImgDrawerPentachoron4D.MdI.Rect(P0.x, P0.y, num, num, false);
		}

		private void DrawSurface(MeshDrawer _Md, int i, Vector4 P0, Vector4 P1, Vector4 P2)
		{
			float t = this.Ed.t;
			float num = ((this.Ed.Con.SubMapData != null) ? (1f / X.Mx(0.2f, this.Ed.Con.SubMapData.scalex)) : 1f);
			uint ran = X.GETRAN2(this.tdr_i * 13 + this.Cp.index + 13 + i, 11 + this.Cp.index % 3 + this.tdr_i * 3);
			float num2 = X.NI(0.3f, 0.15f, X.RAN(ran, 1057)) * num - 1f;
			float num3 = X.NI(0.3f, 0.15f, X.RAN(ran, 1304)) * num - 1f;
			EffectItemNel.GhostDrawerUv(M2CImgDrawerPentachoron4D.MdS, true, ran, num2, -1000f, -1f, -1000f, 120, 133);
			M2CImgDrawerPentachoron4D.MdS.Col = M2CImgDrawerPentachoron4D.MdS.ColGrd.Set1(0.8f + 0.2f * X.COSI(t + X.RAN(ran, 916) * 400f, X.NI(240, 320, X.RAN(ran, 948))), 0.2f + 0.13f * X.COSI(t + X.RAN(ran, 1831) * 400f, X.NI(240, 320, X.RAN(ran, 3133))), 0.2f + 0.13f * X.COSI(t + X.RAN(ran, 3155) * 400f, X.NI(240, 320, X.RAN(ran, 1914))), 0.52f + 0.32f * X.COSI(t + X.RAN(ran, 437) * 400f, X.NI(240, 320, X.RAN(ran, 1982)))).multiply(this.calpha, false).C;
			M2CImgDrawerPentachoron4D.MdS.Tri(0, 2, 1, false);
			M2CImgDrawerPentachoron4D.MdS.PosD(P0.x, P0.y, null).PosD(P1.x, P1.y, null).PosD(P2.x, P2.y, null);
			EffectItemNel.GhostDrawerUv(M2CImgDrawerPentachoron4D.MdS, false, ran, num2, -1000f, -1f, num3, 240, 350);
			num2 = X.NI(0.69f, 0.05f, X.RAN(ran, 1057)) * num - 1f;
			num3 = X.NI(0.29f, 0.05f, X.RAN(ran, 1219)) * num - 1f;
			EffectItemNel.GhostDrawerUv(M2CImgDrawerPentachoron4D.MdA, true, ran, num2, -1000f, -1f, -1000f, 120, 133);
			M2CImgDrawerPentachoron4D.MdA.Col = M2CImgDrawerPentachoron4D.MdA.ColGrd.Set1(0.25f + 0.2f * X.COSI(t + X.RAN(ran, 1745) * 400f, X.NI(620, 911, X.RAN(ran, 1086))), 0.5f + 0.23f * X.COSI(t + X.RAN(ran, 2661) * 400f, X.NI(620, 911, X.RAN(ran, 2320))), 0.6f + 0.33f * X.COSI(t + X.RAN(ran, 712) * 400f, X.NI(620, 911, X.RAN(ran, 1244))), 0.52f + 0.42f * X.COSI(t + X.RAN(ran, 2440) * 400f, X.NI(620, 911, X.RAN(ran, 2424)))).multiply(0.3f * this.calpha, false).C;
			M2CImgDrawerPentachoron4D.MdA.Tri(0, 1, 2, false);
			M2CImgDrawerPentachoron4D.MdA.PosD(P0.x, P0.y, null).PosD(P1.x, P1.y, null).PosD(P2.x, P2.y, null);
			EffectItemNel.GhostDrawerUv(M2CImgDrawerPentachoron4D.MdA, false, ran, num2, -1000f, -1f, num3, 320, 310);
		}

		private const float width_px = 130f;

		private readonly M2CImgDrawerPentachoron4D.P4D[] ATDr;

		private const int MAX_TDR = 4;

		private int tdr_i;

		private float calpha;

		private float floort_fine;

		private static MeshDrawer MdS;

		private static MeshDrawer MdA;

		private static MeshDrawer MdI;

		private struct P4D
		{
			public P4D(Pentachoron4D2Drawer _TDr)
			{
				this.Dr = _TDr;
				this.d_ywagR = 0f;
				this.d_yzagR = 0f;
				this.d_xyagR = 0f;
				this.d_xz_intv = 60f;
			}

			public float ywagR
			{
				get
				{
					return this.Dr.ywagR;
				}
				set
				{
					this.Dr.ywagR = value;
				}
			}

			public float yzagR
			{
				get
				{
					return this.Dr.yzagR;
				}
				set
				{
					this.Dr.yzagR = value;
				}
			}

			public float xyagR
			{
				get
				{
					return this.Dr.xyagR;
				}
				set
				{
					this.Dr.xyagR = value;
				}
			}

			public float xzagR
			{
				get
				{
					return this.Dr.xzagR;
				}
				set
				{
					this.Dr.xzagR = value;
				}
			}

			public readonly Pentachoron4D2Drawer Dr;

			public float d_ywagR;

			public float d_yzagR;

			public float d_xyagR;

			public float d_xz_intv;
		}
	}
}

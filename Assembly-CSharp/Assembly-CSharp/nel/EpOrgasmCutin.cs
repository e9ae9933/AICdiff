using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class EpOrgasmCutin
	{
		public EpOrgasmCutin(PR _Pr)
		{
			this.Pr = _Pr;
			this.ALay = new List<EpOrgasmCutin.CtLay>();
			this.ALayEf = new List<EpOrgasmCutin.CtLay>();
			this.ALayEfDraw = new List<EpOrgasmCutin.CtLay>();
			this.MtrMask = MTRX.getMtr(BLEND.MASK, 20);
			this.FD_drawCutin = new FnEffectRun(this.drawCutin);
		}

		public bool Clear()
		{
			if (this.Efi != null)
			{
				this.Efi.destruct();
			}
			this.ALay.Clear();
			this.ALayEf.Clear();
			this.Efi = null;
			return false;
		}

		public void setE(IEffectSetter EF, float x, float y, bool _cutin_is_left)
		{
			this.Clear();
			this.cutin_is_left = _cutin_is_left;
			this.is_down = (this.Pr.isWormTrapped() ? (X.XORSP() < 0.5f) : (this.Pr.isPoseDown(false) || this.Pr.isPoseManguri(false)));
			PxlSequence sequence = MTR.NoelUiPic.getPoseByName("_orgasm").getSequence(0);
			int num = this.Pr.getAnimator().orgasm_frame_index;
			if (num < 0)
			{
				num = (this.is_down ? 1 : 0);
			}
			this.sF = sequence.getFrame(num);
			if (this.MtrPr == null)
			{
				this.MtrPr = MTRX.getMtr(BLEND.NORMALST, this.sF, 20);
				this.MtrPr.SetFloat("_UseAddColor", 1f);
			}
			int num2 = this.sF.countLayers();
			if (this.ALay.Capacity < num2)
			{
				this.ALay.Capacity = num2;
				this.ALayEf.Capacity = num2;
				this.ALayEfDraw.Capacity = num2;
			}
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				PxlLayer layer = this.sF.getLayer(i);
				EpOrgasmCutin.CtLay ctLay;
				if (TX.isStart(layer.name, "effect", 0))
				{
					ctLay = new EpOrgasmCutin.CtLay(layer, EpOrgasmCutin.LAY.EFFECT, false);
					this.ALayEf.Add(ctLay);
				}
				else if (TX.isStart(layer.name, "enemy", 0))
				{
					ctLay = new EpOrgasmCutin.CtLay(layer, EpOrgasmCutin.LAY.ENEMY, false);
					num3++;
				}
				else if (TX.isStart(layer.name, "sensitive", 0))
				{
					ctLay = new EpOrgasmCutin.CtLay(layer, EpOrgasmCutin.LAY.SENSITIVE, true);
				}
				else
				{
					ctLay = new EpOrgasmCutin.CtLay(layer, EpOrgasmCutin.LAY.NORMAL, true);
				}
				this.ALay.Add(ctLay);
			}
			this.absorbed_weight = 0;
			if (num3 > 0)
			{
				int num4 = X.Mn(this.Pr.isWormTrapped() ? X.IntR(X.NIXP(2.2f, (float)num3)) : X.IntR(this.Pr.getAbsorbedTotalWeight()), num3);
				if (num4 > 0)
				{
					this.absorbed_weight = num4;
					int num5 = 0;
					List<int> combinationI = X.getCombinationI(num3, num4, X.xors(), null);
					for (int j = 0; j < num2; j++)
					{
						EpOrgasmCutin.CtLay ctLay2 = this.ALay[j];
						if (ctLay2.type == EpOrgasmCutin.LAY.ENEMY && (combinationI == null || combinationI.IndexOf(num5++) >= 0))
						{
							ctLay2.visible = true;
						}
						this.ALay[j] = ctLay2;
					}
				}
			}
			this.Efi = EF.setEffectWithSpecificFn("ep_orgasm_cutin", x, y, 150f, 0, 0, this.FD_drawCutin);
		}

		private bool drawCutin(EffectItem E)
		{
			if (E.af >= E.z)
			{
				this.Clear();
				return false;
			}
			MeshDrawer mesh = E.GetMesh("", this.MtrMask, true);
			mesh.base_z += 0.06f;
			float num = X.ZSIN(E.af, 23f);
			mesh.Col = mesh.ColGrd.Set(4290361785U).blend(4285490793U, X.ZSIN((float)(this.absorbed_weight - 1), 3f)).C;
			mesh.Col = mesh.ColGrd.White().blend(mesh.Col, num).C;
			mesh.ColGrd.Set(4289174177U).blend(4282653484U, X.ZSIN((float)this.absorbed_weight, 5f));
			mesh.RectGradation(0f, 0f, 420f * (num - X.ZLINE(E.af - 110f, E.z - 110f)), IN.h + 44f, GRD.BOTTOM2TOP, false);
			MeshDrawer mesh2 = E.GetMesh("", this.MtrPr, true);
			mesh2.base_z -= 0.018f;
			if (E.time <= IN.totalframe)
			{
				E.time = IN.totalframe + 5;
				if (this.ALayEf.Count > 0)
				{
					this.ALayEfDraw.Clear();
					X.getCombination<EpOrgasmCutin.CtLay>(this.ALayEf, 3, X.xors(), this.ALayEfDraw, false);
					for (int i = this.ALayEf.Count - 1; i >= 0; i--)
					{
						EpOrgasmCutin.CtLay ctLay = this.ALayEf[i];
						ctLay.visible = false;
						this.ALayEf[i] = ctLay;
					}
					for (int j = this.ALayEfDraw.Count - 1; j >= 0; j--)
					{
						EpOrgasmCutin.CtLay ctLay2 = this.ALayEfDraw[j];
						ctLay2.visible = true;
						this.ALayEfDraw[j] = ctLay2;
					}
				}
			}
			float num2 = 0f;
			float num3 = (float)(this.sF.pSq.height / 2);
			mesh2.setMtrColor("_AddColor", mesh2.ColGrd.Set(16751598).multiply(1f - X.ZSIN(E.af, 30f), true).blend(16093361U, 0.06f + 0.02f * X.COSIT(23.7f) + 0.02f * X.COSIT(7.38f))
				.C);
			mesh2.Col = MTRX.ColWhite;
			float num4 = (-IN.hh + num3 - 400f * (1f - X.ZSIN2(E.af, 23f) * 0.75f - X.ZSIN(E.af, E.z) * 0.25f)) * (float)X.MPF(this.is_down);
			num2 += X.COSIT(74f) * 3.3f;
			num4 += X.COSIT(111f) * 5.5f;
			int count = this.ALay.Count;
			for (int k = 0; k < count; k++)
			{
				EpOrgasmCutin.CtLay ctLay3 = this.ALay[k];
				if ((ctLay3.type != EpOrgasmCutin.LAY.SENSITIVE || X.SENSITIVE) && ctLay3.visible)
				{
					mesh2.initForImg(ctLay3.L.Img, 0);
					PxlLayer l = ctLay3.L;
					mesh2.RotaL(num2, num4, l, false, false, 0);
				}
			}
			return true;
		}

		public bool isRightCutinActive()
		{
			return this.Efi != null && !this.cutin_is_left;
		}

		public bool isLeftCutinActive()
		{
			return this.Efi != null && this.cutin_is_left;
		}

		public readonly PR Pr;

		private EffectItem Efi;

		private PxlFrame sF;

		private int absorbed_weight;

		private bool is_down;

		public const float dw = 420f;

		public const float dwh = 210f;

		private List<EpOrgasmCutin.CtLay> ALay;

		private List<EpOrgasmCutin.CtLay> ALayEf;

		private List<EpOrgasmCutin.CtLay> ALayEfDraw;

		private Material MtrMask;

		private Material MtrPr;

		private FnEffectRun FD_drawCutin;

		private bool cutin_is_left;

		private struct CtLay
		{
			public CtLay(PxlLayer _L, EpOrgasmCutin.LAY _type, bool _visible = true)
			{
				this.type = _type;
				this.L = _L;
				this.visible = _visible;
			}

			public EpOrgasmCutin.LAY type;

			public bool visible;

			public PxlLayer L;
		}

		private enum LAY
		{
			NORMAL,
			SENSITIVE,
			ENEMY,
			EFFECT
		}
	}
}

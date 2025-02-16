using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class FallenCutin : IRunAndDestroy, IPauseable
	{
		public FallenCutin(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.Gob = IN.CreateGob(null, "FallenCutin");
			this.MMRD = this.Gob.AddComponent<MultiMeshRenderer>();
			IN.setZAbs(this.Gob.transform, 54.97f);
		}

		public void initCameraAfter()
		{
			this.Gob.transform.SetParent(this.M2D.Cam.getGameObject().transform, false);
			int finalRenderedLayer = this.M2D.Cam.getFinalRenderedLayer();
			this.MMRD.setLayer(finalRenderedLayer);
			this.Gob.layer = finalRenderedLayer;
			this.need_fine_layer = true;
		}

		public void endS()
		{
			if (this.Efi != null)
			{
				this.Efi.destruct();
			}
			this.Efi = null;
		}

		public void Clear(bool do_not_rem_runner = false)
		{
			if (this.stt >= FallenCutin.STATE.ACTIVE)
			{
				this.endS();
				this.Pr = null;
				this.changeState(FallenCutin.STATE.OFFLINE);
			}
			this.M2D.DeassignPauseable(this);
			if (!do_not_rem_runner)
			{
				IN.remRunner(this);
			}
			this.Gob.SetActive(false);
		}

		public void destruct()
		{
			if (this.Mti != null)
			{
				this.Mti.Dispose();
			}
			IN.DestroyE(this.Gob);
			this.MI = null;
		}

		private void changeState(FallenCutin.STATE _stt)
		{
			this.stt = _stt;
			this.t = 0f;
		}

		public void initPxl(bool force = false)
		{
			if (this.Pxl != null)
			{
				return;
			}
			if (!force && X.sensitive_level == 0)
			{
				return;
			}
			this.Pxl = MTRX.loadMtiPxc(out this.Mti, "noel_fallen_uipic", "PxlNoel/noel_fallen_uipic.pxls", "NEL", false, true);
		}

		public bool setE(PR _Pr)
		{
			if (X.sensitive_level >= 2 || (X.sensitive_level >= 1 && _Pr.is_alive))
			{
				if (this.stt != FallenCutin.STATE.ACTIVE)
				{
					this.initPxl(true);
					this.Pr = _Pr;
					IN.addRunner(this);
					this.t = 0f;
					this.t0 = 0f;
					this.ran0 = X.xors();
					if (this.stt == FallenCutin.STATE.NOT_PREPARED)
					{
						this.MMRD.use_valotile = true;
						this.MdB = this.MMRD.Make(MTR.MtrNDSub);
						this.MdB.chooseSubMesh(1, false, false);
						this.MdB.setMaterial(MTRX.MtrMeshSub, false);
						this.MdB.chooseSubMesh(2, false, false);
						this.MdB.setMaterial(MTRX.MtrMeshNormal, false);
						this.MdB.chooseSubMesh(3, false, false);
						this.MdB.setMaterial(MTRX.MtrMeshStriped, false);
						this.MdB.connectRendererToTriMulti(this.MMRD.GetMeshRenderer(this.MdB));
						this.MdNoel = this.MMRD.Make(MTRX.MtrMeshNormal);
						this.MdT = this.MMRD.Make(MTRX.MtrMeshSub);
						this.MdT.chooseSubMesh(1, false, false);
						this.MdT.setMaterial(MTR.MtrNDSub, false);
						this.MdT.connectRendererToTriMulti(this.MMRD.GetMeshRenderer(this.MdT));
					}
					this.MdB.clear(false, false);
					this.MdNoel.clear(false, false);
					this.MdT.clear(false, false);
					if (this.need_fine_layer)
					{
						this.need_fine_layer = false;
						int length = this.MMRD.Length;
						for (int i = 0; i < length; i++)
						{
							this.MMRD.Get(i).draw_gl_only = true;
							ValotileRenderer valotileRenderer = this.MMRD.GetValotileRenderer(i);
							valotileRenderer.ReleaseBinding(true, true, false);
							this.M2D.Cam.connectToBinder(valotileRenderer);
						}
					}
					this.stt = FallenCutin.STATE.ACTIVE;
					this.Gob.SetActive(true);
					this.M2D.AssignPauseableP(this);
					this.t_recheck = 80f;
				}
				return true;
			}
			return false;
		}

		public void Pause()
		{
			this.Gob.SetActive(false);
			if (this.stt == FallenCutin.STATE.FADEOUT)
			{
				if (this.t >= 0f)
				{
					this.t = X.Mx(this.t, 80f);
					return;
				}
			}
			else if (this.t >= 0f)
			{
				this.t = -1f - this.t;
			}
		}

		public void Resume()
		{
			this.Gob.SetActive(true);
			if (this.t < 0f)
			{
				this.t = -1f - this.t;
			}
		}

		private bool isLoadFinished()
		{
			if (this.MI != null)
			{
				return true;
			}
			if (!this.Pxl.isLoadCompleted())
			{
				return false;
			}
			if (this.Mti.isAsyncLoadFinished())
			{
				this.MI = this.Mti.MI;
				this.MMRD.setMaterial(this.MdNoel, this.MI.getMtr(BLEND.NORMALP3, -1), false);
				return true;
			}
			return false;
		}

		public void progressNextPhase(PR _Pr)
		{
			if (this.stt == FallenCutin.STATE.ACTIVE && this.Pr == _Pr)
			{
				this.changeState(FallenCutin.STATE.FALLING);
			}
		}

		public bool run(float fcnt)
		{
			if (this.t < 0f)
			{
				return true;
			}
			if (!this.runInner(fcnt))
			{
				this.Clear(true);
				return false;
			}
			return true;
		}

		private bool runInner(float fcnt)
		{
			if (this.isHiddenState())
			{
				return false;
			}
			this.t += fcnt;
			this.t0 += fcnt;
			if (this.isActiveState())
			{
				if (X.sensitive_level >= 2 && this.t >= (float)(this.Pr.is_alive ? 80 : 200))
				{
					this.M2D.Iris.ForceWakeupInput(false);
					this.t_recheck = 0f;
				}
				this.t_recheck -= fcnt;
				if (this.t_recheck <= 0f)
				{
					this.t_recheck = 20f;
					if (!this.M2D.Iris.isWaiting(this.Pr))
					{
						this.changeState(FallenCutin.STATE.FALLING);
					}
				}
			}
			return !X.D_EF || this.drawCutin();
		}

		private bool drawCutin()
		{
			if (this.Pr.Mp == null)
			{
				return true;
			}
			this.MdB.clear(false, false);
			this.MdNoel.clear(false, false);
			this.MdT.clear(false, false);
			M2Camera cam = this.Pr.M2D.Cam;
			float scaleRev = cam.getScaleRev();
			if (this.stt < FallenCutin.STATE.FADEOUT)
			{
				float num = X.ZLINE(this.t0 - 12f, 68f);
				float num2;
				if (num > 0f)
				{
					num2 = IN.w + 120f * scaleRev;
					float num3 = IN.h + 230f * scaleRev;
					if (num >= 1f)
					{
						this.MdB.chooseSubMesh(1, false, false);
						this.MdB.Col = MTRX.ColWhite;
						this.MdB.Rect(0f, 0f, num2, num3, false);
					}
					else
					{
						this.MdB.chooseSubMesh(0, false, false);
						this.MdB.allocUv2(16, false);
						this.MdB.Col = this.MdB.ColGrd.blend3(4286190918U, 4293288866U, uint.MaxValue, num).C;
						float num4 = (1f - X.ZSIN(num, 0.6f)) * 0.6f;
						this.MdB.ColGrd.mulA(X.ZLINE(num - 0.6f, 0.39999998f));
						EffectItemNel.GhostDrawerUv(this.MdB, true, this.ran0, 0.88f, -1000f, -1f, -1000f, 120, 133);
						this.MdB.InnerCircle(0f, 0f, num2, num3, 0f, 0f, num2 * 0.8f, num3 * 0.8f, num2 * num4, num3 * num4, false, false, 0f, 1f);
						EffectItemNel.GhostDrawerUv(this.MdB, false, this.ran0, 0.88f, -1000f, -1f, -1000f, 120, 133);
					}
				}
				this.MdB.chooseSubMesh(2, false, false);
				num2 = IN.w + 260f * scaleRev;
				for (int i = 0; i < 3; i++)
				{
					float num5 = (float)(20 + i * 11);
					float num6 = (float)(17 + i * 9);
					if (((this.stt == FallenCutin.STATE.ACTIVE) ? X.ZLINE(this.t, num6) : (num6 * 8f)) <= 0f)
					{
						break;
					}
					float num7 = 0f;
					if (this.stt == FallenCutin.STATE.FALLING)
					{
						num7 = -(IN.hh + 430f) * X.ZPOW(this.t - (float)(11 + i * 4), 35f);
					}
					float num3 = (float)((i == 0) ? 14 : ((i == 1) ? 0 : (-30))) + 240f * scaleRev;
					uint ran = X.GETRAN2(((IN.totalframe / 13) & 127) * 13 + i * 31, i);
					float num8 = (2.44f - (float)i * 0.06f + 0.15f * X.RANS(ran, 1226)) / 180f * 3.1415927f;
					this.MdB.Identity();
					this.MdB.Rotate(num8, false).TranslateP(0f, num7, false);
					this.MdB.Col = C32.d2c((i == 0) ? 4286742183U : ((i == 1) ? 4288773799U : 4288773799U));
					num3 *= ((i == 0) ? X.ZSIN2(num) : ((i == 1) ? X.ZSIN(num) : X.ZCOS(num)));
					this.MdB.Rect(0f, 0f, num2, num3, false);
					if (i == 2)
					{
						float num9 = X.ZLINE(this.t - num5 - (num6 - 36f), 40f);
						if (num9 > 0f && num9 < 1f)
						{
							this.MdB.chooseSubMesh(3, false, false);
							this.MdB.Col = this.MdB.ColGrd.Set(4291571066U).blend(4292598747U, num9).C;
							this.MdB.uvRectN(X.Cos(0.7853982f), X.Sin(-0.7853982f));
							this.MdB.allocUv2(6, false).Uv2(1f, 0.95f * (1f - num9), false);
							this.MdB.Rect(0f, 0f, num2, num3, false);
							this.MdB.allocUv2(0, true);
						}
					}
				}
				if (this.isLoadFinished())
				{
					num = ((this.stt == FallenCutin.STATE.ACTIVE) ? X.ZLINE(this.t - 30f, 38f) : X.ZLINE(this.t, 70f));
					if (num > 0f)
					{
						float num10;
						float num11;
						if (this.stt == FallenCutin.STATE.ACTIVE)
						{
							num10 = 1f - X.ZSIN2(num);
							num11 = 8f * (1f - X.ZSIN(num)) / 180f * 3.1415927f;
						}
						else
						{
							num10 = -X.ZPOW(num) * 1.4f;
							num11 = 10f * X.ZCOS(num) / 180f * 3.1415927f;
						}
						this.MdNoel.Scale(0.5f, 0.5f, false).Rotate(num11, false).TranslateP(0f, num10 * (IN.hh + 200f), false)
							.Scale(scaleRev, scaleRev, false);
						PxlSequence sequence = this.Pxl.getPoseByName("stand").getSequence(0);
						PxlFrame frame = sequence.getFrame((int)((ulong)this.ran0 % (ulong)((long)sequence.countFrames())));
						this.MdNoel.Col = MTRX.ColWhite;
						this.MdNoel.Uv23(MTRX.ColTrnsp, false);
						Matrix4x4 currentMatrix = this.MdNoel.getCurrentMatrix();
						this.Pr.getAnimator().copyNormal3Attribute(this.MdNoel, false, 0.4f, 0.12f);
						this.MdNoel.RotaL(0f, 0f, frame.getLayer(0), false, false, 0);
						this.MdNoel.setCurrentMatrix(currentMatrix, false);
						this.Pr.getAnimator().copyNormal3Attribute(this.MdNoel, false, 2f, 0.3f);
						this.MdNoel.RotaL(0f, 0f, frame.getLayer(1), false, false, 0);
						this.MdNoel.allocUv23(6, true);
						this.Pr.getAnimator().copyNormal3Attribute(this.MdNoel, true, 0f, 0f);
						this.MdNoel.RotaL(0f, 0f, frame.getLayer(2), false, false, 0);
						this.MdNoel.allocUv23(0, true);
					}
				}
			}
			if (this.stt >= FallenCutin.STATE.FALLING)
			{
				float num12 = 1f;
				float num13 = 0f;
				float num14 = -0.6f;
				float num15 = -0.2f;
				if (this.stt == FallenCutin.STATE.FALLING)
				{
					num12 = X.ZSINV(this.t, 40f);
					this.MdT.chooseSubMesh((num12 >= 1f) ? 0 : 1, false, false);
					this.MdT.Col = this.MdT.ColGrd.Set(4291571066U).blend(uint.MaxValue, X.ZLINE(this.t, 30f)).mulA(X.ZLINE(this.t - 30f, 30f))
						.C;
					this.MdT.ColGrd.Set(4291571066U).blend(uint.MaxValue, X.ZLINE(this.t - 30f, 30f));
					if (this.t >= 65f)
					{
						this.changeState(FallenCutin.STATE.FADEOUT);
					}
					float num16 = 2f;
					float num17 = 1.5f;
					num14 = num16;
					num15 = num17;
				}
				else if (this.stt == FallenCutin.STATE.FADEOUT)
				{
					num13 = X.ZSIN(this.t - 20f, 40f);
					if (num13 >= 1f)
					{
						return false;
					}
					this.MdT.chooseSubMesh(1, false, false);
					this.MdT.Col = this.MdT.ColGrd.Set(uint.MaxValue).blend(4291571066U, X.ZLINE(this.t - 30f, 30f)).C;
					this.MdT.ColGrd.Set(uint.MaxValue).blend(4291571066U, X.ZLINE(this.t - 30f, 30f)).mulA(1f - X.ZSIN(this.t, 20f));
					float num18 = 1.5f;
					float num17 = 1.8f;
					num14 = num18;
					num15 = num17;
				}
				float num19 = scaleRev * (float)((num12 == 1f && num13 == 0f) ? 4 : 1);
				num12 = -IN.hh + IN.h * num12;
				num13 = -IN.hh + IN.h * num13;
				this.MdT.allocUv23(4, false);
				this.MdT.Scale(1.15f * num19, 1.15f * num19, false).RectBLGradation(-IN.wh, num13, IN.w, num12 - num13, GRD.TOP2BOTTOM, false);
				this.MdT.Uv2(X.RAN(this.ran0, 995) + X.ANMPT(120, 1f), X.RAN(this.ran0, 769) + X.ANMPT(133, 1f), false);
				this.MdT.Uv3(num14, num14, false).Uv3(num15, num15, false).Uv3(num15, num15, false)
					.Uv3(num14, num14, false);
			}
			if (this.Efi == null && this.Pr.Mp != null)
			{
				this.Efi = this.Pr.Mp.getEffectTop().setEffectWithSpecificFn("FCUTIN", 0f, 0f, 0f, 0, 0, (EffectItem Ef) => true);
			}
			if (this.Efi != null && (CFG.sp_dmgcounter_position & CFG.UIPIC_DMGCNT.MAP) != CFG.UIPIC_DMGCNT.NONE)
			{
				this.Efi.x = cam.x * this.Pr.Mp.rCLEN;
				this.Efi.y = cam.y * this.Pr.Mp.rCLEN;
				this.Pr.Mp.DmgCntCon.drawDmgCounterForSpecificMv(this.Pr, this.Efi, 1.8f, 1f, -0.65f, false);
			}
			this.MdB.updateForMeshRenderer(true);
			this.MdNoel.updateForMeshRenderer(true);
			this.MdT.updateForMeshRenderer(true);
			return true;
		}

		public override string ToString()
		{
			return "FallenCutin";
		}

		public bool isHiddenState()
		{
			return this.stt < FallenCutin.STATE.ACTIVE;
		}

		public bool isActiveState()
		{
			return this.stt == FallenCutin.STATE.ACTIVE;
		}

		public bool isActive(PR Pr)
		{
			return (this.stt == FallenCutin.STATE.ACTIVE || this.stt == FallenCutin.STATE.FALLING) && Pr == this.Pr;
		}

		public readonly NelM2DBase M2D;

		private GameObject Gob;

		private FallenCutin.STATE stt = FallenCutin.STATE.NOT_PREPARED;

		public readonly MultiMeshRenderer MMRD;

		private const int LAY_UWORM = 0;

		private const int LAY_CWORM = 1;

		private const int LAY_NOEL = 2;

		private const int B_MID_NDSUB = 0;

		private const int B_MID_SUB = 1;

		private const int B_MID_FILL = 2;

		private const int B_MID_STRIPE = 3;

		private const int T_MID_SUB = 0;

		private const int T_MID_NDSUB = 1;

		private PR Pr;

		private PxlCharacter Pxl;

		private MTIOneImage Mti;

		private MImage MI;

		private MeshDrawer MdB;

		private MeshDrawer MdNoel;

		private MeshDrawer MdT;

		private EffectItem Efi;

		private uint ran0;

		private bool pause;

		private float t;

		private float t0;

		private float t_recheck;

		private bool need_fine_layer;

		private const string pxl_name = "noel_fallen_uipic";

		private enum STATE
		{
			NOT_PREPARED = -1,
			OFFLINE,
			ACTIVE,
			FALLING,
			FADEOUT
		}
	}
}

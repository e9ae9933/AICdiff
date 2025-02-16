using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class NelChipBarrierLit : NelChipCheckPoint, IRunAndDestroy
	{
		public NelChipBarrierLit(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			base.arrangeable = true;
		}

		public override int entryChipMesh(MeshDrawer MdB, MeshDrawer MdG, MeshDrawer MdT, MeshDrawer MdLB, MeshDrawer MdLT, MeshDrawer MdTT, float sx, float sy, float _zm, float _rotR = 0f)
		{
			int num = base.entryChipMesh(MdB, MdG, MdT, MdLB, MdLT, MdTT, sx, sy, _zm, _rotR);
			this.DaG = (this.DaL = (this.DaT = null));
			return num;
		}

		public override bool can_touch_checkpoint
		{
			get
			{
				return PUZ.IT.barrier_active;
			}
		}

		public override int getCheckPointPriority()
		{
			return 500;
		}

		public override void returnChcekPoint(PR Pr)
		{
			if (PUZ.IT.barrier_active)
			{
				PUZ.IT.revertGimmickActivation();
			}
		}

		public override void activateCheckPoint()
		{
			if (!M2CImgDrawerBarrierLit.stop_snapshot_on_checkpoint && (this.Mp.M2D as NelM2DBase).CheckPoint.get_CurCheck() == this)
			{
				M2CImgDrawerBarrierLit m2CImgDrawerBarrierLit = this.DaB as M2CImgDrawerBarrierLit;
				Vector2 crystalMapPoint = new Vector2(this.mapcx, this.mapcy);
				if (m2CImgDrawerBarrierLit != null)
				{
					if (m2CImgDrawerBarrierLit.touchPlayer(false))
					{
						PUZ.IT.FineLitConnect();
					}
					crystalMapPoint = m2CImgDrawerBarrierLit.getCrystalMapPoint(-1f);
				}
				PUZ.IT.createSnapShot(false);
				this.Mp.PtcSTsetVar("x", (double)crystalMapPoint.x).PtcSTsetVar("y", (double)crystalMapPoint.y).PtcST("puzz_barrier_lit_snapped", null, PTCThread.StFollow.NO_FOLLOW);
			}
			base.activateCheckPoint();
		}

		public bool isTouched()
		{
			M2CImgDrawerBarrierLit m2CImgDrawerBarrierLit = this.DaB as M2CImgDrawerBarrierLit;
			return m2CImgDrawerBarrierLit != null && m2CImgDrawerBarrierLit.isTouched();
		}

		public override Color32 getDrawEffectPositionAndColor(ref int pixel_x, ref int pixel_y)
		{
			return PlayerCheckPoint.defaultAssignPositionAndColor(this, ref pixel_x, ref pixel_y);
		}

		public override bool drawCheckPoint(EffectItem Ef, float pixel_x, float pixel_y, Color32 Col)
		{
			return PlayerCheckPoint.defaultDrawChipPC(Ef, this, pixel_x, pixel_y, Col);
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			this.Ed = this.Mp.remED(this.Ed);
			this.need_fine_lit_connect_ = true;
			this.destruct();
			base.closeAction(when_map_close, do_not_remove_drawer);
		}

		public void destruct()
		{
			this.Ed = this.Mp.remED(this.Ed);
			this.closeEvent();
			if (this.MvEvent != null)
			{
				this.MvEvent.destruct();
				this.MvEvent = null;
				this.WarpTo = null;
				this.need_fine_lit_connect_ = false;
			}
		}

		public void closeEvent()
		{
			if (this.MvEvent != null)
			{
				this.MvEvent.setExecutable(M2EventItem.CMD.TALK, false);
				this.Mp.remRunnerObject(this);
			}
			this.Ed = this.Mp.remED(this.Ed);
			this.lit_jump_t = -33f;
		}

		public static void eventWarp(int id, int index_lay, int index_src)
		{
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			PR pr = nelM2DBase.Cam.getBaseMover() as PR;
			if (pr == null)
			{
				X.de("ワープするプレイヤーインスタンスが不明", null);
				return;
			}
			NelChipBarrierLit nelChipBarrierLit = null;
			bool flag = false;
			if (id == -1)
			{
				nelChipBarrierLit = nelM2DBase.CheckPoint.get_CurCheck() as NelChipBarrierLit;
				if (nelChipBarrierLit == null)
				{
					X.de("ワープするチップインスタンスが不明", null);
					return;
				}
				flag = true;
			}
			if (id == -2)
			{
				if (index_lay < 0 || index_src < 0)
				{
					X.de("-2 ワープ先として指定されたチップインスタンスが不明", null);
					return;
				}
				NelChipBarrierLit nelChipBarrierLit2 = nelM2DBase.curMap.getLayer(index_lay).getChipByIndex(index_src) as NelChipBarrierLit;
				if (nelChipBarrierLit2 == null)
				{
					X.de("-2 ワープ先として指定されたチップインスタンスが不明", null);
					return;
				}
				if (nelChipBarrierLit2.WarpTo != null)
				{
					nelChipBarrierLit = nelChipBarrierLit2.WarpTo;
				}
			}
			if (nelChipBarrierLit != null)
			{
				NelChipWarp.reduce_release_effect = pr.Mp.floort + 2f;
				float num = nelChipBarrierLit.mapcy - 0.6f;
				NelEnemy.warpParticleSetting(pr.Mp, pr.x, pr.y, nelChipBarrierLit.mapcx, num, "puzz_warp_star", 2.25f);
				nelM2DBase.curMap.PtcSTsetVar("sx", (double)pr.x).PtcSTsetVar("sy", (double)pr.y).PtcSTsetVar("dx", (double)nelChipBarrierLit.mapcx)
					.PtcSTsetVar("dy", (double)num)
					.PtcST("puzz_warp_execute", null, PTCThread.StFollow.NO_FOLLOW);
				pr.executeWarpToChip(nelChipBarrierLit);
				if (flag)
				{
					PUZ.IT.revertGimmickActivation();
				}
			}
		}

		public float warp_target_s0
		{
			get
			{
				return this.warp_target_s0_;
			}
			set
			{
				if (this.warp_target_s0_ != value)
				{
					this.warp_target_s0_ = value;
					this.setEffectBinder();
				}
			}
		}

		private void setEffectBinder()
		{
			if (this.Ed == null)
			{
				if (this.FD_fnDrawOnEffectWarpTarget == null)
				{
					this.FD_fnDrawOnEffectWarpTarget = new M2DrawBinder.FnEffectBind(this.fnDrawOnEffectWarpTarget);
				}
				this.Ed = this.Mp.setED("BarrierLit", this.FD_fnDrawOnEffectWarpTarget, 0f);
				this.Ed.t = 0f;
				this.Ed.f0 = IN.totalframe;
				if (this.DrwWarp == null)
				{
					this.DrwWarp = new WarpDoorDrawer(50f, 50f, 2);
				}
			}
		}

		private bool fnDrawOnEffectWarpTarget(EffectItem Ef, M2DrawBinder Ed)
		{
			M2CImgDrawerBarrierLit m2CImgDrawerBarrierLit = this.DaB as M2CImgDrawerBarrierLit;
			if (m2CImgDrawerBarrierLit == null)
			{
				this.Ed = null;
				return false;
			}
			Vector2 crystalMapPoint = m2CImgDrawerBarrierLit.getCrystalMapPoint(-1f);
			crystalMapPoint.y += M2CImgDrawerBarrierLit.getFloatingMapShiftY();
			Ef.x = crystalMapPoint.x;
			Ef.y = crystalMapPoint.y;
			if (!Ed.isinCamera(Ef, 2f, 4f))
			{
				return true;
			}
			bool flag = false;
			bool flag2 = true;
			if (this.WarpTo != null)
			{
				flag = true;
				if (this.lit_jump_t < 40f)
				{
					PxlSequence crystalSq = NelChipWarp.getCrystalSq();
					MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
					meshImg.base_z += 1.5E-05f;
					meshImg.Col = meshImg.ColGrd.White().mulA((this.lit_jump_t > 0f) ? (1f - X.ZLINE(this.lit_jump_t - 18f, 22f)) : X.ZLINE(-this.lit_jump_t, 28f)).C;
					PxlFrame frame = crystalSq.getFrame(1 + X.ANMT(6, 8f));
					meshImg.RotaPF(0f, 0f, this.Mp.base_scale, this.Mp.base_scale, this.draw_rotR, frame, this.flip, false, false, uint.MaxValue, false, 0);
				}
				if (this.lit_jump_t > -33f)
				{
					flag2 = false;
					Ef.x = base.M2D.Cam.x * this.Mp.rCLEN;
					Ef.y = base.M2D.Cam.y * this.Mp.rCLEN;
					NelChipWarp.fnDrawOnEffectWarp(this, new Vector2((crystalMapPoint.x - Ef.x) * this.Mp.CLENB, -(crystalMapPoint.y - Ef.y) * this.Mp.CLENB), this.lit_jump_t, 674124U, Ef, Ed);
					Ef.x = crystalMapPoint.x;
					Ef.y = crystalMapPoint.y;
				}
			}
			if (flag2 && this.warp_target_s0 > this.Mp.floort)
			{
				flag = true;
				float num = X.ZLINE(this.warp_appear_t - 18f, 22f);
				if (num > 0f)
				{
					float num2 = X.ZPOWV(this.warp_target_s0_ - this.Mp.floort, 30f);
					Material material = null;
					WarpDoorDrawer warpDrawer = NelChipWarp.getWarpDrawer(ref material);
					NelChipWarp.need_fine_color = true;
					MeshDrawer meshDrawer = null;
					RenderTexture targetTexture = base.M2D.Cam.GetCameraCollecter(0).Cam.targetTexture;
					if (targetTexture != null)
					{
						material.SetTexture("_MainTex", targetTexture);
						float num3 = 0.125f + 0.0625f * X.COSI(this.Mp.floort, 38f) + 0.0625f * X.COSI(this.Mp.floort, 91f);
						material.SetColor("_Color", MTRX.cola.Set(base.M2D.Cam.transparent_color).multiply(num3, false).setA1(num2)
							.C);
						meshDrawer = Ef.GetMesh("", material, true);
						meshDrawer.base_z += 1E-05f;
					}
					MeshDrawer mesh = Ef.GetMesh("", MTRX.getMtr(BLEND.ADD, -1), true);
					this.DrwWarp.BaseColor = mesh.ColGrd.White().mulA(X.ZLINE(1f - num * num2)).C;
					this.DrwWarp.LineColorIn = mesh.ColGrd.Set(warpDrawer.LineColorIn).mulA(num2).C;
					this.DrwWarp.LineColorOut = mesh.ColGrd.Set(warpDrawer.LineColorOut).mulA(num2).C;
					this.DrwWarp.Radius(18f, 22f).WH(46f * num2, 52f * num2);
					this.DrwWarp.ripple_thick = 2.5f;
					int num4 = this.DrwWarp.drawTo(mesh, this.Mp.floort, 0f, 0f);
					if (meshDrawer != null && num4 > 0)
					{
						NelChipWarp.shakeWarpBehindTexture(this.Mp, mesh, meshDrawer, targetTexture, this.DrwWarp, 0f, 0f, num4, 0.5f, num2, this.index);
					}
				}
			}
			if (!flag)
			{
				this.Ed = null;
			}
			return flag;
		}

		protected override void initCheckPoint(M2MoverPr Mv)
		{
			base.initCheckPoint(Mv);
			this.FineLitConnect(false);
			if (Mv is PR && this.MvEvent != null)
			{
				this.NearPR = Mv;
			}
		}

		public bool run(float fcnt)
		{
			if (!base.NM2D.Puz.barrier_active)
			{
				this.closeEvent();
				return false;
			}
			bool flag = this.WarpTo != null && NelChipWarp.isPlayerFooting(this.NearPR, this);
			if (flag)
			{
				this.WarpTo.warp_target_s0 = this.Mp.floort + 60f;
				if (this.lit_jump_t >= 0f)
				{
					this.WarpTo.warp_appear_t = this.lit_jump_t;
				}
			}
			NelChipWarp.runWarpPortal(ref this.lit_jump_t, this, fcnt, flag);
			return true;
		}

		public void FineLitConnect(bool force = false)
		{
			M2CImgDrawerBarrierLit m2CImgDrawerBarrierLit = this.DaB as M2CImgDrawerBarrierLit;
			if (m2CImgDrawerBarrierLit == null || (!force && !this.need_fine_lit_connect_))
			{
				return;
			}
			this.need_fine_lit_connect_ = false;
			this.WarpTo = (m2CImgDrawerBarrierLit.isTouched() ? PUZ.IT.getLitWorpDestination(this, m2CImgDrawerBarrierLit) : null);
			if (this.WarpTo != null)
			{
				if (this.MvEvent == null || this.MvEvent.destructed)
				{
					this.lit_jump_t = -33f;
					M2EventItem m2EventItem = (this.MvEvent = this.Mp.getEventContainer().CreateAndAssign(base.unique_key));
					float num = this.mtop - 0.5f;
					m2EventItem.setToArea(this.mleft, num, this.mright - this.mleft, this.mbottom - num);
					m2EventItem.assign("TALK", NelChipWarp.getEventContent(this.mapcx, (int)(this.mbottom + 0.25f), "-2 " + this.Lay.index.ToString() + " " + this.index.ToString()), true);
					m2EventItem.check_desc_name = "EV_access_warp";
				}
				this.Mp.addRunnerObject(this);
				this.MvEvent.setExecutable(M2EventItem.CMD.TALK, true);
				this.setEffectBinder();
				return;
			}
			this.closeEvent();
		}

		public bool hasEvent()
		{
			return this.MvEvent != null;
		}

		private float warp_target_s0_;

		public M2DrawBinder Ed;

		public float warp_appear_t;

		public const float T_FADEIN = 40f;

		public const float T_FADEOUT = 33f;

		public const float extending_h_time = 18f;

		public float lit_jump_t = -33f;

		private WarpDoorDrawer DrwWarp;

		private bool need_fine_lit_connect_;

		private M2EventItem MvEvent;

		private M2Attackable NearPR;

		private M2DrawBinder.FnEffectBind FD_fnDrawOnEffectWarpTarget;

		public NelChipBarrierLit WarpTo;
	}
}

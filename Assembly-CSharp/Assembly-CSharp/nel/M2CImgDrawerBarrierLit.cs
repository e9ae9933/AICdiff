using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerBarrierLit : M2CImgDrawerWithEffect, IActivatable, IPuzzActivationListener
	{
		public M2CImgDrawerBarrierLit(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, _lay == 0)
		{
			this.CpB = this.Cp as NelChipBarrierLit;
			this.use_color = (this.rewriteable_alpha = true);
			this.use_shift = true;
			this.set_effect_on_initaction = false;
			this.start_af_min = (this.start_af_max = 0);
			this.Cp.light_alpha = 0f;
			this.Cp.arrangeable = true;
			M2CImgDrawerBarrierLit.SqAnim = NelChipWarp.getCrystalSq();
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (this.layer == 1 || this.layer == 3)
			{
				return false;
			}
			base.Set(true);
			PxlMeshDrawer pxlMeshDrawer = this.Cp.Img.getSrcMesh(3);
			if (pxlMeshDrawer != null)
			{
				Md.RotaMesh(meshx, meshy, _zmx, _zmy, _rotR, pxlMeshDrawer, false, false);
			}
			this.ver_top = Md.getVertexMax();
			pxlMeshDrawer = this.Cp.Img.getSrcMesh(1);
			if (pxlMeshDrawer != null)
			{
				Md.RotaMesh(meshx, meshy, _zmx, _zmy, _rotR, pxlMeshDrawer, false, false);
			}
			this.ver_bottom = Md.getVertexMax();
			Md.RotaMesh(meshx, meshy, _zmx, _zmy, _rotR, Ms, false, false);
			base.Set(false);
			base.initMdArray();
			if (this.Cp.active_removed)
			{
				base.repositActiveRemoveFlag();
			}
			return true;
		}

		public override void initAction(bool f)
		{
			base.initAction(f);
			this.player_touched = COOK.getSF("BLT_" + base.unique_key) != 0;
			M2LpPuzzManageArea m2LpPuzzManageArea = PUZ.IT.isBelongTo(this);
			if (m2LpPuzzManageArea != null && m2LpPuzzManageArea.auto_touch_barrier_lit)
			{
				this.touchPlayer(false);
			}
			if (PUZ.IT.barrier_active)
			{
				this.activate();
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			PUZ.IT.removePaListener(this);
		}

		public void changePuzzleActivation(bool activated)
		{
			if (activated)
			{
				this.activate();
				this.CpB.FineLitConnect(true);
				return;
			}
			this.deactivate();
			this.CpB.closeEvent();
		}

		public bool touchPlayer(bool assign_checkpoint)
		{
			bool flag = false;
			if (!this.player_touched)
			{
				this.player_touched = true;
				flag = true;
				COOK.setSF("BLT_" + base.unique_key, 1);
			}
			if (assign_checkpoint)
			{
				M2CImgDrawerBarrierLit.stop_snapshot_on_checkpoint = true;
				(base.Mp.M2D as NelM2DBase).CheckPoint.setCheckPointManual(this.CpB);
			}
			return flag;
		}

		public void activate()
		{
			if (!this.player_touched || this.start_t > 0f)
			{
				return;
			}
			this.start_t = X.Mx(1f, base.Mp.floort);
			int num = this.ver_top - this.index_first;
			for (int i = 0; i < num; i++)
			{
				this.ACol[i].a = 0;
			}
			this.Cp.light_alpha = 1f;
			this.need_reposit_flag = true;
			base.Mp.addUpdateMesh(base.redraw(0f), false);
			M2CImgDrawerBarrierLit.Mtr = MTRX.MIicon.getMtr(BLEND.NORMALP2, -1);
			if (this.Ptc == null)
			{
				this.Ptc = new EfParticleLooper("puzzle_lit_star");
			}
			this.Ed = base.Mp.remED(this.Ed);
			base.setEffect();
			Vector2 vector = this.Cp.PixelToMapPoint(16f, -9f);
			base.Mp.PtcSTsetVar("x", (double)vector.x).PtcSTsetVar("y", (double)vector.y).PtcST("puzzle_lit_activate", null, PTCThread.StFollow.NO_FOLLOW);
		}

		public void deactivate()
		{
			if (this.start_t > 0f)
			{
				this.start_t = -base.Mp.floort;
				int num = this.ver_top - this.index_first;
				for (int i = 0; i < num; i++)
				{
					this.ACol[i].a = byte.MaxValue;
				}
				this.Cp.light_alpha = 0f;
				this.need_reposit_flag = true;
				base.Mp.addUpdateMesh(base.redraw(0f), false);
				this.Ed = base.Mp.remED(this.Ed);
				Vector2 vector = this.Cp.PixelToMapPoint(16f, -9f);
				base.Mp.PtcSTsetVar("x", (double)vector.x).PtcSTsetVar("y", (double)vector.y).PtcST("puzzle_lit_deactivate", null, PTCThread.StFollow.NO_FOLLOW);
			}
			(base.Mp.M2D as NelM2DBase).CheckPoint.removeCheckPointManual(this.CpB);
		}

		public override int redraw(float fcnt)
		{
			M2CImgDrawerBarrierLit.stop_snapshot_on_checkpoint = false;
			if (this.start_t != 0f)
			{
				NelChipWarp.fineWarpColor();
				float num = base.Mp.floort - X.Abs(this.start_t);
				if (num < 35f)
				{
					float num2 = X.ZPOW(num, 30f);
					if (this.start_t < 0f)
					{
						num2 = 1f - num2;
					}
					int num3 = this.index_last - this.index_first;
					int num4 = this.ver_bottom - this.index_first;
					int num5 = this.ver_top - this.index_first;
					for (int i = 0; i < num3; i++)
					{
						if (i >= num4)
						{
							this.ASh[i].y = 8f * num2 * (float)((i >= num5) ? 2 : 1);
						}
					}
					this.need_reposit_flag = true;
				}
			}
			return base.redraw(fcnt);
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		public bool isTouched()
		{
			return this.player_touched;
		}

		public static float getFloatingMapShiftY()
		{
			return 0.15f * X.COSIT_floating(0.5f);
		}

		protected override bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.Ptc == null)
			{
				return false;
			}
			if (this.start_t <= 0f)
			{
				return false;
			}
			float num = X.ZSIN(Ed.t, 34f);
			Vector2 crystalMapPoint = this.getCrystalMapPoint(num);
			Ef.x = crystalMapPoint.x;
			Ef.y = crystalMapPoint.y + M2CImgDrawerBarrierLit.getFloatingMapShiftY();
			if (!Ed.isinCamera(Ef, 2f, 4f))
			{
				return true;
			}
			Color32 c = MTRX.cola.Set(4288256409U).blend(4278190080U, num - 0.12f + 0.06f * X.COSI(Ed.t, 7.93f) + 0.06f * X.COSI(Ed.t, 11.83f)).C;
			MeshDrawer mesh = Ef.GetMesh("", M2CImgDrawerBarrierLit.Mtr, true);
			mesh.Col = MTRX.ColWhite;
			mesh.initForImg(M2CImgDrawerBarrierLit.SqAnim.getImage(0, 0), 0);
			mesh.allocUv23(4, false).Uv23(c, false);
			mesh.RotaGraph(0f, 0f, 1f, this.Cp.draw_rotR, null, this.Cp.flip);
			mesh.allocUv23(0, true);
			if ((base.M2D as NelM2DBase).CheckPoint.get_CurCheck() == this.CpB)
			{
				Ef.x = crystalMapPoint.x;
				Ef.y = crystalMapPoint.y;
				this.Ptc.Draw(Ef, this.Ptc.total_delay);
			}
			return true;
		}

		public Vector2 getCrystalMapPoint(float tz = -1f)
		{
			float num = 0f;
			if (this.start_t != 0f)
			{
				tz = ((tz < 0f) ? ((this.start_t < 0f) ? (1f - X.ZSIN(base.Mp.floort + this.start_t, 34f)) : X.ZSIN(base.Mp.floort - this.start_t, 34f)) : tz);
				num = 8f * tz;
			}
			return this.Cp.PixelToMapPoint(16f, -9f - num);
		}

		public float start_t;

		private const float sh_y = 8f;

		private const float pixel_crystal_x = 16f;

		private const float pixel_crystal_y = -9f;

		private static Material Mtr;

		private EfParticleLooper Ptc;

		private int ver_top;

		private int ver_bottom;

		private bool player_touched;

		private static PxlSequence SqAnim;

		public readonly NelChipBarrierLit CpB;

		public static bool stop_snapshot_on_checkpoint;
	}
}

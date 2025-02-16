using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelChipFootSwitch : NelChip, IActivatable, IPuzzActivationListener, IRunAndDestroy, IBCCFootListener
	{
		public NelChipFootSwitch(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			if (this.Abuf == null)
			{
				this.Abuf = new List<M2Puts>(1);
			}
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			this.t_actv = -40f;
			this.aim = base.Meta.getDirsI("foot_switch", this.rotation, this.flip, 0, -1);
			this.foot_shift = base.Meta.GetNm("foot_switch", 0f, 1) * base.rCLEN;
			this.ManageArea = PUZ.IT.isBelongTo(this);
			if (this.ManageArea != null)
			{
				PUZ.IT.addPaListener(this);
			}
			this.ARideFootD = new List<IMapDamageListener>(2);
			this.ARideBcc = new List<M2BlockColliderContainer.BCCLine>(2);
			this.Abuf.Clear();
			M2BlockColliderContainer m2BlockColliderContainer = ((this.AttachCM != null) ? this.AttachCM.getBCCCon() : null);
			if (m2BlockColliderContainer == null)
			{
				this.Mp.addBCCFootListener(this);
			}
			else
			{
				m2BlockColliderContainer.addBCCFootListener(this);
			}
			this.Mp.getPointMetaPutsTo(this.mapx + CAim._XD(this.aim, 1), this.mapy - CAim._YD(this.aim, 1), this.Abuf, "puzzle_rail_start");
			this.puzzle_id = -1;
			this.t_deactivate_reserve = 0f;
			int num = (int)CAim.get_opposite((AIM)this.aim);
			for (int i = this.Abuf.Count - 1; i >= 0; i--)
			{
				M2Chip m2Chip = this.Abuf[i] as M2Chip;
				if (m2Chip != null && m2Chip.getMeta().getDirsI("puzzle_rail_start", m2Chip.rotation, m2Chip.flip, 1, -1) == num)
				{
					this.puzzle_id = m2Chip.getMeta().GetI("puzzle_rail_start", -1, 0);
					if (this.puzzle_id >= 0)
					{
						break;
					}
				}
			}
			this.box_checked = false;
			this.runner_assigned = this.need_runner_assign;
		}

		public bool need_runner_assign
		{
			get
			{
				return this.t_actv > -40f || ((this.ManageArea == null || PUZ.IT.barrier_active) && (!this.box_checked || this.ABCCNeedCheck != null));
			}
		}

		public bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (this.runner_assigned == value)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (value)
				{
					this.Mp.addRunnerObject(this);
					this.t_fine = 0f;
					return;
				}
				this.Mp.remRunnerObject(this);
			}
		}

		private void checkAnotherBCCInitialize()
		{
			this.box_checked = true;
			for (int i = this.Mp.count_carryable_bcc - 1; i >= 0; i--)
			{
				M2BlockColliderContainer carryableBCCByIndex = this.Mp.getCarryableBCCByIndex(i);
				Rect rect = carryableBCCByIndex.getBoundsShifted();
				if (carryableBCCByIndex.BelongTo is IBCCCarriedMover)
				{
					rect = (carryableBCCByIndex.BelongTo as IBCCCarriedMover).getBccAppliedArea(rect);
				}
				if (this.isCovering(rect))
				{
					if (this.ABCCNeedCheck == null)
					{
						this.ABCCNeedCheck = new List<M2BlockColliderContainer>(1);
					}
					this.ABCCNeedCheck.Add(carryableBCCByIndex);
				}
			}
		}

		public void destruct()
		{
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			this.runner_assigned = false;
			PUZ.IT.removePaListener(this);
			this.Mp.remBCCFootListener(this);
			this.t_actv = -40f;
			this.Ed = this.Mp.remED(this.Ed);
		}

		public void activate()
		{
			this.t_deactivate_reserve = 0f;
			if (this.isActive())
			{
				return;
			}
			this.t_actv = 0f;
			if (this.puzzle_id >= 0)
			{
				PuzzLiner.activateFootSwitch(this, this.puzzle_id, false);
			}
			if (this.Mp.floort >= 10f)
			{
				this.defineParticlePreVariable();
				this.Mp.PtcST("puzz_foot_switch_on", null, PTCThread.StFollow.NO_FOLLOW);
			}
			if (this.FD_fnDrawFootingLight == null)
			{
				this.FD_fnDrawFootingLight = new M2DrawBinder.FnEffectBind(this.fnDrawFootingLight);
			}
			if (this.Ed == null)
			{
				this.Ed = this.Mp.setED("FootingLight", this.FD_fnDrawFootingLight, 0f);
			}
			else
			{
				this.Ed.t = 0f;
			}
			this.runner_assigned = true;
		}

		public void deactivate()
		{
			if (!this.isActive())
			{
				return;
			}
			this.t_actv = -1f;
			this.ARideBcc.Clear();
			this.ARideFootD.Clear();
			if (this.puzzle_id >= 0)
			{
				PuzzLiner.activateFootSwitch(this, this.puzzle_id, true);
			}
			if (this.Mp.floort >= 10f)
			{
				this.defineParticlePreVariable();
				this.Mp.PtcST("puzz_foot_switch_off", null, PTCThread.StFollow.NO_FOLLOW);
			}
			this.t_deactivate_reserve = 0f;
			this.Ed = this.Mp.remED(this.Ed);
			this.runner_assigned = true;
		}

		public void defineParticlePreVariable()
		{
			float num;
			float num2;
			this.getFootPos(out num, out num2);
			this.Mp.PtcSTsetVar("cx", (double)num).PtcSTsetVar("cy", (double)num2).PtcSTsetVar("agR", (double)CAim.get_agR(CAim.get_opposite((AIM)this.aim), 0f));
		}

		public uint getFootableAimBits()
		{
			return 1U << this.aim;
		}

		public DRect getMapBounds(DRect BufRc)
		{
			float num;
			float num2;
			this.getFootPos(out num, out num2);
			float num3 = ((CAim._XD(this.aim, 1) == 0) ? ((float)this.clms * 0.5f - 0.25f) : 0.125f);
			float num4 = ((CAim._YD(this.aim, 1) == 0) ? ((float)this.rows * 0.5f - 0.25f) : 0.125f);
			return BufRc.Set(num - num3, num2 - num4, num3 * 2f, num4 * 2f);
		}

		public bool footedInit(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener FootD)
		{
			if (this.ManageArea != null && !PUZ.IT.barrier_active)
			{
				return false;
			}
			if (this.ARideFootD.IndexOf(FootD) < 0)
			{
				this.ARideFootD.Add(FootD);
				this.activate();
			}
			return true;
		}

		public bool footedQuit(IMapDamageListener Fd, bool from_jump_init = false)
		{
			this.ARideFootD.Remove(Fd);
			if (this.ARideFootD.Count == 0)
			{
				this.t_fine = 0f;
				this.runner_assigned = this.need_runner_assign;
			}
			return true;
		}

		public bool run(float fcnt)
		{
			if (!this.box_checked)
			{
				this.checkAnotherBCCInitialize();
			}
			bool flag = true;
			this.t_fine -= fcnt;
			if (this.t_fine <= 0f)
			{
				if (this.t_actv >= 0f && this.t_deactivate_reserve < 5f)
				{
					this.t_fine = (float)(base.isinCamera(0f) ? 8 : 1) * 6f;
					bool flag2 = this.ARideFootD.Count > 0;
					if (!flag2 && this.ARideBcc.Count > 0)
					{
						float num;
						float num2;
						this.getFootPos(out num, out num2);
						float num3 = ((CAim._XD(this.aim, 1) == 0) ? ((float)this.clms * 0.5f - 0.1f) : 0.125f);
						float num4 = ((CAim._YD(this.aim, 1) == 0) ? ((float)this.rows * 0.5f - 0.1f) : 0.125f);
						int num5 = (int)CAim.get_opposite((AIM)this.aim);
						for (int i = this.ARideBcc.Count - 1; i >= 0; i--)
						{
							if (this.ARideBcc[i].isNear(num, num2, num3, num4, num5, true))
							{
								flag = (flag2 = true);
								break;
							}
							this.ARideBcc.RemoveAt(i);
						}
					}
					if (!flag2)
					{
						if (this.t_deactivate_reserve == 0f)
						{
							this.t_deactivate_reserve = 1f;
						}
						else
						{
							this.t_deactivate_reserve += fcnt;
						}
						this.t_fine = 0f;
					}
					else if (this.ARideBcc.Count == 0 && this.t_actv >= 40f)
					{
						flag = false;
					}
				}
				else
				{
					this.t_fine = (float)(base.isinCamera(0f) ? 9 : 3) * 6f;
					bool flag3 = false;
					if (this.ABCCNeedCheck != null)
					{
						float num6;
						float num7;
						this.getFootPos(out num6, out num7);
						float num8 = ((CAim._XD(this.aim, 1) == 0) ? ((float)this.clms * 0.5f - 0.1f) : 0.125f);
						float num9 = ((CAim._YD(this.aim, 1) == 0) ? ((float)this.rows * 0.5f - 0.1f) : 0.125f);
						int num10 = (int)CAim.get_opposite((AIM)this.aim);
						for (int j = this.ABCCNeedCheck.Count - 1; j >= 0; j--)
						{
							M2BlockColliderContainer.BCCLine near = this.ABCCNeedCheck[j].getNear(num6, num7, num8, num9, num10, null, false, true, 0f);
							if (near != null)
							{
								this.ARideBcc.Add(near);
								flag3 = true;
								break;
							}
						}
					}
					if (flag3)
					{
						this.activate();
					}
					else if (this.t_actv <= -40f)
					{
						flag = this.need_runner_assign;
					}
					else if (this.isActive())
					{
						this.deactivate();
					}
				}
			}
			else
			{
				flag = true;
			}
			if (this.t_actv >= 0f)
			{
				if (this.t_actv < 40f)
				{
					this.t_actv += fcnt;
					NelChipFootSwitch.M2CImgDrawerFootSwitch m2CImgDrawerFootSwitch = base.CastDrawer<NelChipFootSwitch.M2CImgDrawerFootSwitch>();
					if (m2CImgDrawerFootSwitch != null)
					{
						m2CImgDrawerFootSwitch.need_reposit_flag = true;
					}
				}
			}
			else if (this.t_actv > -40f)
			{
				this.t_actv -= fcnt;
				NelChipFootSwitch.M2CImgDrawerFootSwitch m2CImgDrawerFootSwitch2 = base.CastDrawer<NelChipFootSwitch.M2CImgDrawerFootSwitch>();
				if (m2CImgDrawerFootSwitch2 != null)
				{
					m2CImgDrawerFootSwitch2.need_reposit_flag = true;
				}
			}
			if (!flag)
			{
				this.runner_assigned_ = false;
			}
			return flag;
		}

		protected override M2CImgDrawer CreateDrawer(ref MeshDrawer Md, int lay, METACImg Meta, M2CImgDrawer Pre_Drawer)
		{
			if (lay < 3)
			{
				return new NelChipFootSwitch.M2CImgDrawerFootSwitch(Md, lay, this);
			}
			return null;
		}

		public bool fnDrawFootingLight(EffectItem Ef, M2DrawBinder Ed)
		{
			if (!this.isActive() || this.puzzle_id < 0)
			{
				this.Ed = null;
				return false;
			}
			int num = CAim._XD(this.aim, 1);
			int num2 = CAim._YD(this.aim, 1);
			float num3;
			float num4;
			this.getFootPos(out num3, out num4);
			Ef.x = num3 - 90f * base.rCLENB * 0.5f * (float)num;
			Ef.y = num4 + 90f * base.rCLENB * 0.5f * (float)num2;
			if (!((num != 0) ? Ed.isinCamera(Ef, 90f * base.rCLEN + 1f, 1f) : Ed.isinCamera(Ef, 1f, 90f * base.rCLEN + 1f)))
			{
				return true;
			}
			MeshDrawer mesh = Ef.GetMesh("", MTRX.MtrMeshNormal, false);
			mesh.Rotate(CAim.get_agR((AIM)this.aim, 0f), false);
			float num5 = (float)((num != 0) ? this.iheight : this.iwidth) * this.Mp.base_scale;
			int num6 = X.ANMT(2, 15f);
			mesh.Col = mesh.ColGrd.Set(((num6 == 0) ? MTR.Apuzzle_switch_col : MTR.Apuzzle_switch_col_dark)[this.puzzle_id]).mulA((num6 == 0) ? 0.8f : 0.4f).C;
			mesh.ColGrd.setA(0f);
			mesh.RectGradation(0f, 0f, 90f, num5, GRD.RIGHT2LEFT, false);
			return true;
		}

		public void getFootPos(out float foot_x, out float foot_y)
		{
			foot_x = this.mapcx + (float)CAim._XD(this.aim, 1) * this.foot_shift;
			foot_y = this.mapcy - (float)CAim._YD(this.aim, 1) * this.foot_shift;
		}

		public bool isCovering(Rect Rc)
		{
			return X.isCovering((float)this.mapx, (float)(this.mapx + this.clms), Rc.x, Rc.xMax, 1f) && X.isCovering((float)this.mapy, (float)(this.mapy + this.rows), Rc.y, Rc.yMax, 1f);
		}

		public void changePuzzleActivation(bool activated)
		{
			if (activated)
			{
				this.runner_assigned = true;
			}
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		public bool isActive()
		{
			return this.t_actv >= 0f;
		}

		private int aim;

		private int puzzle_id;

		private float foot_shift;

		private M2LpPuzzManageArea ManageArea;

		private float t_actv = -40f;

		private float t_deactivate_reserve;

		private bool box_checked;

		private const float T_FADE = 40f;

		private float t_fine;

		private const float FINE_INTV = 6f;

		private List<IMapDamageListener> ARideFootD;

		private List<M2BlockColliderContainer.BCCLine> ARideBcc;

		private readonly List<M2Puts> Abuf;

		private M2DrawBinder Ed;

		private List<M2BlockColliderContainer> ABCCNeedCheck;

		private bool runner_assigned_;

		private M2DrawBinder.FnEffectBind FD_fnDrawFootingLight;

		public class M2CImgDrawerFootSwitch : M2CImgDrawer
		{
			public M2CImgDrawerFootSwitch(MeshDrawer Md, int _lay, M2Puts _Cp)
				: base(Md, _lay, _Cp, true)
			{
				this.use_shift = true;
				this.CpSw = this.Cp as NelChipFootSwitch;
			}

			public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
			{
				base.Set(false);
				Md.RotaMesh(meshx, meshy, _zmx, _zmy, _rotR, Ms, false, false);
				if (this.layer <= 2)
				{
					PxlMeshDrawer srcMesh = this.Cp.Img.getSrcMesh(4);
					Md.RotaMesh(meshx, meshy, _zmx, _zmy, _rotR, srcMesh, false, false);
					this.spring_height = 6f;
				}
				base.Set(false);
				if ((this.redraw_flag || this.use_shift || this.use_color) && this.index_last > this.index_first)
				{
					base.initMdArray();
				}
				if (this.Cp.active_removed)
				{
					base.repositActiveRemoveFlag();
				}
				return this.redraw_flag;
			}

			public override int redraw(float fcnt)
			{
				if (this.need_reposit_flag)
				{
					float num = ((this.CpSw.t_actv >= 0f) ? X.ZSIN(this.CpSw.t_actv, 11f) : (1f - (X.ZSIN(-this.CpSw.t_actv, 9f) * 1.375f - X.ZCOS(-this.CpSw.t_actv - 8f, 12f) * 0.5f + X.ZSINV(-this.CpSw.t_actv - 20f, 19f) * 0.125f)));
					for (int i = 0; i < 7; i++)
					{
						if (i != 4)
						{
							this.ASh[i] = new Vector3(0f, (float)X.IntR(-num * this.spring_height), 0f);
						}
					}
				}
				return base.redraw(fcnt);
			}

			private readonly NelChipFootSwitch CpSw;

			public float spring_height;
		}
	}
}

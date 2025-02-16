using System;
using System.Collections.Generic;
using Better;
using m2d;
using XX;

namespace nel
{
	public class NelChipThunderShooter : NelChip, IRunAndDestroy, NearManager.INearLsnObject, ILinerReceiver
	{
		public virtual string areasnd_cue
		{
			get
			{
				return "areasnd_thunder";
			}
		}

		public virtual string main_tag_name
		{
			get
			{
				return "thunder_shooter";
			}
		}

		public NelChipThunderShooter(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			if (NelChipThunderShooter.Ptc == null)
			{
				NelChipThunderShooter.Ptc = new EfParticleOnce("map_thunder_shooter", EFCON_TYPE.NORMAL);
				NelChipThunderShooter.PtcFoot = new EfParticleLooper("map_thunder_foot");
				NelChipThunderShooter.PtcFootC = new EfParticleLooper("map_thunder_foot_c");
				NelChipThunderShooter.PtcHeadC = new EfParticleLooper("map_thunder_head_c");
			}
		}

		public void assignBoundsLp(M2LabelPointThunder Lp, bool pre_connected = false, bool checking = false)
		{
			if (checking)
			{
				this.aim = this.Img.Meta.getDirsI("thunder_shooter", this.rotation, this.flip, 0, -1);
				if (!Lp.isinMapP((float)this.mapx + 0.5f + (float)CAim._XD(this.aim, 1), (float)this.mapy + 0.5f - (float)CAim._YD(this.aim, 1), 0f, 0f))
				{
					return;
				}
			}
			this.LpArea = Lp;
			if (pre_connected && Lp != null)
			{
				this.pre_connected = true;
			}
			Lp.assignChip(this);
		}

		public void assignAnotherShooter(NelChipThunderShooter _StAnother)
		{
			this.StAnother = _StAnother;
			_StAnother.StAnotherO = this;
			this.assignBoundsLp(this.StAnother.LpArea, this.StAnother.pre_connected, false);
			if (this.Ext != null && !this.Ext.initted)
			{
				this.Ext.reachable_max = X.Mn(this.Ext.reachable_max, this.StAnother.Ext.reachable_max);
			}
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.Mp.NM == null)
			{
				return;
			}
			(base.M2D as NelM2DBase).prepareSvTexture("damage_horse", false);
			this.aim = this.Img.Meta.getDirsI(this.main_tag_name, this.rotation, this.flip, 0, -1);
			if (this.aim < 0)
			{
				return;
			}
			this.t_effect_running = 0f;
			M2ExtenderChecker m2ExtenderChecker = new M2ExtenderChecker(base.unique_key, this.Mp, (AIM)this.aim, 0f, 0f, 0.2f);
			m2ExtenderChecker.ChipCheckingFn = (int mapx, int mapy, M2Pt Pt) => CCON.mistPassable(Pt.cfg, 2);
			this.Ext = m2ExtenderChecker;
			this.Ext.camera_clip = true;
			this.Ext.ActiveCarryCM = this.AttachCM;
			this.Ext.mist_passable = true;
			float num;
			float num2;
			this.Ext.calcShotStartPos(this, out num, out num2, 0);
			this.fineShotPos();
			if (this.LpArea != null)
			{
				switch (this.aim)
				{
				case 0:
					this.Ext.reachable_max = (float)((int)num - this.LpArea.mapx);
					break;
				case 1:
					this.Ext.reachable_max = (float)((int)num2 - this.LpArea.mapy);
					break;
				case 2:
					this.Ext.reachable_max = (float)(this.LpArea.mapx + this.LpArea.mapw - (int)num);
					break;
				case 3:
					this.Ext.reachable_max = (float)(this.LpArea.mapy + this.LpArea.maph - (int)num2);
					break;
				}
			}
			this.Ext.fnReachFined = new M2ExtenderChecker.FnReachFined(this.fnReachFined);
		}

		public void fineShotPos()
		{
			float num;
			float num2;
			this.Ext.calcShotStartPos(this, out num, out num2, 0);
			this.Ext.SetPos(num, num2);
			if (this.Ray != null)
			{
				this.Ray.PosMap(num, num2);
			}
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			if (this.LpArea != null && this.StAnother == null && !this.pre_connected)
			{
				this.LpArea.closeAction(true);
				this.Lay.LP.Rem(this.LpArea.key);
				this.Lay.LP.reindex();
			}
			if (this.NearTicket != null && this.Mp.NM != null)
			{
				this.Mp.NM.Deassign(this);
			}
			this.NearTicket = null;
			this.LpArea = null;
			this.pre_connected = false;
			this.StAnother = (this.StAnotherO = null);
			this.MDI = base.M2D.MDMGCon.Release(this.MDI);
			this.Ray = base.NM2D.MGC.destructRay(this.Ray);
			if (this.Ext != null)
			{
				this.Ext.destruct();
				this.Ext = null;
			}
			this.runner_assigned = false;
		}

		public virtual void fnReachFined(bool initted, bool changed)
		{
			if (this.Ext == null)
			{
				return;
			}
			if (this.Ext.reachable_max == 0f)
			{
				return;
			}
			if (initted)
			{
				this.Mp.setDangerousFlag(this.Ext);
				if (this.LpArea == null)
				{
					if (this.StAnother != null)
					{
						if (this.StAnother.LpArea == null)
						{
							return;
						}
						this.LpArea = this.StAnother.LpArea;
					}
					this.LpArea = new M2LabelPointThunder("Thunder_" + base.unique_key, -1, this.Lay, this);
					this.LpArea.index = this.Lay.LP.Length;
					this.LpArea.fineSize(this.Ext, this.aim);
					this.Lay.LP.Add(this.LpArea);
				}
				int num = (int)this.Ext.shot_dx + ((this.aim == 0) ? (-1) : 0);
				int num2 = (int)this.Ext.shot_dy + ((this.aim == 1) ? (-1) : 0);
				List<M2Puts> allPointMetaPutsTo = this.Mp.getAllPointMetaPutsTo(num, num2, 1, 1, null, Map2d.LayArray2Bits(this.Lay), this.main_tag_name);
				if (allPointMetaPutsTo != null)
				{
					for (int i = allPointMetaPutsTo.Count - 1; i >= 0; i--)
					{
						NelChipThunderShooter nelChipThunderShooter = allPointMetaPutsTo[i] as NelChipThunderShooter;
						if (nelChipThunderShooter == this || nelChipThunderShooter == this.StAnother)
						{
							return;
						}
						if (nelChipThunderShooter != this.StAnotherO && nelChipThunderShooter != null && nelChipThunderShooter.aim == (int)CAim.get_opposite((AIM)this.aim) && nelChipThunderShooter.StAnother == null)
						{
							nelChipThunderShooter.assignAnotherShooter(this);
							break;
						}
					}
				}
				if (this.StAnother == null)
				{
					this.LpArea.fineEnable();
				}
				if (this.Mp.NM != null)
				{
					bool flag = this.AttachCM != null || this.Ext.need_check_other_bcc;
					this.NearTicket = this.Mp.NM.AssignForMv(this, flag, true);
					if (this.AttachCM != null)
					{
						this.NearTicket.moving_always = true;
					}
				}
			}
			if (changed || initted)
			{
				this.fined_floort = this.Mp.floort;
				this.runner_assigned = this.Ext.reachable_len > 0f;
				if (this.NearTicket != null && !this.NearTicket.moving_always)
				{
					this.Mp.NM.recheckMovingAllMover();
				}
				if (this.Ray != null)
				{
					this.Ray.PosMap(this.Ext.shot_sx, this.Ext.shot_sy);
					this.Ray.LenM(X.LENGTHXYS(this.Ext.shot_sx, this.Ext.shot_sy, this.Ext.shot_dx, this.Ext.shot_dy));
				}
			}
		}

		public void SetEnable(bool v)
		{
			if (this.Ext == null || this.Ext.auto_ascend == v)
			{
				return;
			}
			this.Ext.auto_ascend = v;
			if (v)
			{
				this.fineShotPos();
			}
			else
			{
				this.Ext.reachable_len = 0f;
			}
			this.Ext.Fine(0f, false);
		}

		public override void translateByChipMover(float ux, float uy, C32 AddCol, int drawx0, int drawy0, int move_drawx = 0, int move_drawy = 0, bool stabilize_move_map = false)
		{
			base.translateByChipMover(ux, uy, AddCol, drawx0, drawy0, move_drawx, move_drawy, stabilize_move_map);
			if (this.Ext != null && this.Ext.auto_ascend)
			{
				this.fineShotPos();
				this.Ext.need_fine_map_bcc = true;
				this.Ext.Fine(3f, false);
			}
		}

		public void destruct()
		{
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
					if (this.FD_fnDrawThunderEffect == null)
					{
						this.FD_fnDrawThunderEffect = new M2DrawBinder.FnEffectBind(this.fnDrawThunderEffect);
					}
					if (this.Ed == null)
					{
						this.Ed = this.Mp.setED("Thunder", this.FD_fnDrawThunderEffect, 0f);
						return;
					}
				}
				else
				{
					this.Mp.remRunnerObject(this);
					if (this.Ed != null)
					{
						this.Mp.remED(this.Ed);
					}
					this.Ed = null;
				}
			}
		}

		public bool run(float fcnt)
		{
			if (this.Ext == null || this.Ext.reachable_len == 0f)
			{
				return false;
			}
			if (this.another_full)
			{
				return true;
			}
			if (this.Ray != null)
			{
				this.Ray.clearReflectBuffer().Cast(false, null, true);
			}
			if (this.t_effect_running > 20f)
			{
				this.t_effect_running = 0f;
				float num;
				float num2;
				if (CAim._XD(this.aim, 1) != 0)
				{
					num = 1.5707964f;
					num2 = this.Ext.w * this.Mp.base_scale * 0.5f;
				}
				else
				{
					num = 0f;
					num2 = this.Ext.h * this.Mp.base_scale * 0.5f;
				}
				this.Mp.PtcN("map_thunder_shooter_moving", this.Ext.cx * this.Mp.rCLEN, this.Ext.cy * this.Mp.rCLEN, num, (int)num2, 2);
			}
			return true;
		}

		public bool another_full
		{
			get
			{
				return this.StAnother != null && this.StAnother.Ext.reachable_len == this.StAnother.Ext.reachable_max;
			}
		}

		public bool another_full_o
		{
			get
			{
				return this.StAnotherO != null && this.StAnotherO.Ext.reachable_len == this.StAnotherO.Ext.reachable_max;
			}
		}

		public DRect getBounds(M2Mover Mv, DRect Dest)
		{
			if (this.Ext != null && (this.Ext.w != 0f || this.Ext.h != 0f))
			{
				return Dest.Set(this.Ext.x - 1f, this.Ext.y - 1f, this.Ext.w + 2f, this.Ext.h + 2f);
			}
			return null;
		}

		public bool nearCheck(M2Mover Mv, NearTicket NTk)
		{
			M2Attackable m2Attackable = Mv as M2Attackable;
			if (m2Attackable == null || this.Ext == null || this.LpArea == null)
			{
				return false;
			}
			if (Mv == this.Mp.Pr)
			{
				this.t_effect_running += Map2d.TS;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = CAim._XD(this.aim, 1) != 0;
			if (this.Ext.reachable_len > 0f)
			{
				if (flag3)
				{
					float num = this.Ext.x;
					float num2 = this.Ext.right;
					if (this.AttachCM != null && this.AttachCM.vx > 0f)
					{
						num -= X.Mx(Mv.vx, this.AttachCM.vx) + 0.4f;
					}
					else if (this.AttachCM != null && this.AttachCM.vx < 0f)
					{
						num2 += -X.Mn(Mv.vx, this.AttachCM.vx) + 0.4f;
					}
					else if (Mv.vx > 0f && Mv.mleft <= this.Ext.right)
					{
						num -= Mv.vx + 0.3f;
					}
					else if (Mv.vx < 0f && Mv.mright >= this.Ext.x)
					{
						num2 += -Mv.vx + 0.3f;
					}
					float cy = this.Ext.cy;
					flag = Mv.isCovering(num, num2, cy - 0.2f, cy + 0.2f, 0f);
					flag2 = Mv.isCovering(num, num2, cy - 0.2f - 0.7f, cy + 0.2f + 0.7f, 0f);
				}
				else
				{
					float num3 = this.Ext.y;
					float num4 = this.Ext.bottom;
					float cx = this.Ext.cx;
					if (this.AttachCM != null && this.AttachCM.vy > 0f)
					{
						num3 -= X.Mx(Mv.vy, this.AttachCM.vy) + 0.4f;
					}
					else if (this.AttachCM != null && this.AttachCM.vy < 0f)
					{
						num4 += -X.Mx(Mv.vy, this.AttachCM.vy) + 0.4f;
					}
					else if (Mv.vy > 0f && Mv.mtop <= this.Ext.bottom)
					{
						num3 -= Mv.vy + 0.3f;
					}
					else if (Mv.vy < 0f && Mv.mbottom <= this.Ext.y)
					{
						num4 += -Mv.vy + 0.3f;
					}
					flag = Mv.isCovering(cx - 0.2f, cx + 0.2f, num3, num4, 0f);
					flag2 = Mv.isCovering(cx - 0.2f - 0.7f, cx + 0.2f + 0.7f, num3, num4, 0f);
				}
				if (flag)
				{
					M2FootManager footManager = Mv.getFootManager();
					if (footManager != null && footManager.get_FootBCC() != null && !footManager.get_FootBCC().is_map_bcc)
					{
						M2BlockColliderContainer.BCCLine footBCC = footManager.get_FootBCC();
						int num5 = 0;
						while (num5 < 4 && flag)
						{
							if (((ulong)footManager.footable_bits & (ulong)(1L << (num5 & 31))) != 0UL && footBCC.foot_aim == (AIM)num5 && CAim.get_opposite((AIM)this.aim) == (AIM)num5)
							{
								AIM aim = (AIM)num5;
								if (aim == AIM.L || aim == AIM.R)
								{
									if (X.BTW(footBCC.shifted_y - 0.15f, Mv.y, footBCC.shifted_bottom + 0.15f))
									{
										flag = false;
									}
								}
								else if (X.BTW(footBCC.shifted_x - 0.15f, Mv.x, footBCC.shifted_right + 0.15f))
								{
									flag = false;
								}
							}
							num5++;
						}
					}
				}
			}
			float num7;
			if (flag)
			{
				if (this.OHitLock == null)
				{
					this.OHitLock = new BDic<int, float>(1);
				}
				float num6;
				if (this.OHitLock.TryGetValue(m2Attackable.index, out num6) && this.Mp.floort < num6)
				{
					flag = false;
				}
			}
			else if (this.OHitLock != null && this.OHitLock.TryGetValue(m2Attackable.index, out num7) && this.Mp.floort < num7)
			{
				if (flag2)
				{
					this.OHitLock[m2Attackable.index] = num7 - Map2d.TS * 2f;
				}
				else
				{
					this.OHitLock.Remove(m2Attackable.index);
				}
			}
			if (flag)
			{
				if (this.MDI == null)
				{
					this.createMapDamageInstance();
				}
				if (flag3)
				{
					this.MDI.Set(this.Ext.shot_sx, this.Ext.shot_sy, 0f, 0f);
				}
				else
				{
					this.MDI.Set(this.Ext.shot_sx, this.Ext.y, 0f, this.Ext.height);
				}
				float num8;
				float num9;
				AttackInfo atk = this.GetAtk(m2Attackable, out num8, out num9, true, true);
				if (atk != null)
				{
					int num10 = (int)m2Attackable.get_hp();
					if (m2Attackable.applyDamageFromMap(this.MDI, atk, num8, num9, true) != null && (int)m2Attackable.get_hp() < num10 && this.LpArea.hitlock_maxt > 0f)
					{
						this.afterDamageApplied(m2Attackable);
					}
				}
			}
			return true;
		}

		protected virtual void createMapDamageInstance()
		{
			this.MDI = base.M2D.MDMGCon.Create((this.LpArea.hit_aim >= 0) ? MAPDMG.THUNDER_STATIC : MAPDMG.THUNDER, this.Ext.cx, this.Ext.cy, 0f, 0f, null);
		}

		protected virtual AttackInfo GetAtk(M2Attackable MvA, out float efx, out float efy, bool set_burst_vx = true, bool set_burst_vy = true)
		{
			AttackInfo atk = this.MDI.GetAtk(null, MvA, out efx, out efy);
			bool flag = CAim._XD(this.aim, 1) != 0;
			if (atk != null)
			{
				if (set_burst_vx)
				{
					if (this.LpArea.hit_aim >= 0)
					{
						atk.burst_vx = (float)CAim._XD(this.LpArea.hit_aim, 1) * X.Abs(atk.burst_vx);
					}
					else if (this.another_full_o && this.StAnotherO.Ext.reachable_len == this.Ext.reachable_len && flag)
					{
						bool flag2 = X.Abs(this.Ext.shot_sx - MvA.x) < X.Mx(2f, this.Ext.reachable_max * 0.33f);
						bool flag3 = X.Abs(this.StAnotherO.Ext.shot_sx - MvA.x) < X.Mx(2f, this.StAnotherO.Ext.reachable_max * 0.33f);
						if (flag2 != flag3)
						{
							atk.burst_vx = X.Abs(atk.burst_vx) * (float)CAim._XD(flag2 ? this.aim : this.StAnotherO.aim, 1) * (float)X.MPF(X.XORSP() < 0.8f);
						}
						else if (MvA.getPhysic() != null && MvA.getPhysic().walk_xspeed != 0f)
						{
							atk.burst_vx = X.Abs(atk.burst_vx) * (float)X.MPF(MvA.getPhysic().walk_xspeed > 0f) * (float)X.MPF(X.XORSP() < 0.7f);
						}
						else
						{
							atk.burst_vx = X.Abs(atk.burst_vx) * (float)X.MPF(X.XORSP() < 0.5f);
						}
					}
				}
				if (set_burst_vy && flag && MvA is M2MoverPr && MvA.is_alive)
				{
					atk.burst_vy = 0.012f;
				}
			}
			return atk;
		}

		protected virtual void afterDamageApplied(M2Attackable MvA)
		{
			if (MvA.is_alive)
			{
				if (this.OHitLock == null)
				{
					this.OHitLock = new BDic<int, float>(1);
				}
				this.OHitLock[MvA.index] = this.Mp.floort + this.LpArea.hitlock_maxt;
			}
		}

		public void activateLiner(bool immediate)
		{
			if (this.LpArea != null)
			{
				this.LpArea.linerActivating(true);
			}
		}

		public void deactivateLiner(bool immediate)
		{
			if (this.LpArea != null)
			{
				this.LpArea.linerActivating(false);
			}
		}

		public bool initEffect(bool activating, ref DRect RcEffect)
		{
			RcEffect = this.Ext;
			return true;
		}

		private bool fnDrawThunderEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.LpArea == null || this.Ext == null)
			{
				this.Ed = null;
				return false;
			}
			if (this.no_draw_on_reach_len_is_zero && this.Ext.reachable_len == 0f)
			{
				return true;
			}
			if (this.another_full)
			{
				return true;
			}
			NelChipThunderShooter.Ptc.x = (Ef.x = this.Ext.cx);
			NelChipThunderShooter.Ptc.y = (Ef.y = this.Ext.cy);
			float num = this.Ext.w * 0.5f;
			float num2 = this.Ext.h * 0.5f;
			return !Ed.isinCamera(Ef, num + this.draw_margin_mapx, num2 + this.draw_margin_mapy) || this.fnDrawThunderEffectInner(Ef, Ed, num, num2);
		}

		protected virtual bool fnDrawThunderEffectInner(EffectItem Ef, M2DrawBinder Ed, float dw, float dh)
		{
			MeshDrawer mesh = Ef.GetMesh("", MTRX.MtrMeshAdd, false);
			MeshDrawer mesh2 = Ef.GetMesh("", MTRX.MtrMeshSub, false);
			mesh2.Col = mesh2.ColGrd.Set(4287450745U).mulA(0.7f + 0.2f * X.COSIT(8.8f) + 0.1f * X.COSIT(17.13f)).C;
			mesh2.ColGrd.setA(0f);
			mesh.Col = mesh.ColGrd.Set(4293525375U).C;
			mesh.ColGrd.setA(0f);
			C32 c = MTRX.cola.Set(mesh.Col).setA(90f);
			dw *= this.Mp.CLENB;
			dh *= this.Mp.CLENB;
			float num = 0.8f + 0.2f * X.COSIT(19.8f) + 0.1f * X.COSIT(23.13f);
			if (CAim._XD(this.aim, 1) != 0)
			{
				mesh2.BlurLine(-dw, 0f, dw, 0f, 42f, 0, 0f, false);
				mesh.BlurLineW(-dw, 0f, dw, 0f, 2f, (float)X.IntR(20f * num), 0, 0f, 0f, 1f, c, mesh.ColGrd, mesh.ColGrd);
				NelChipThunderShooter.Ptc.z = 1.5707964f;
				NelChipThunderShooter.Ptc.time = (int)dw;
			}
			else
			{
				mesh2.BlurLine(0f, -dh, 0f, dh, 42f, 0, 0f, false);
				mesh.BlurLineW(0f, -dh, 0f, dh, 2f, (float)X.IntR(20f * num), 0, 0f, 0f, 1f, c, mesh.ColGrd, mesh.ColGrd);
				NelChipThunderShooter.Ptc.z = 0f;
				NelChipThunderShooter.Ptc.time = (int)dh;
			}
			if (this.Ext.ActiveCarryCM != null)
			{
				Ef.x = this.Ext.shot_sx;
				Ef.y = this.Ext.shot_sy;
				NelChipThunderShooter.PtcHeadC.Draw(Ef, 50f);
			}
			else if (!NelChipThunderShooter.Ptc.drawTo(mesh, (float)(IN.totalframe - NelChipThunderShooter.Ptc.f0), 0f))
			{
				NelChipThunderShooter.Ptc.restart();
			}
			Ef.x = this.Ext.shot_dx_cur(-1f);
			Ef.y = this.Ext.shot_dy_cur(-1f);
			if (Ed.isinCamera(Ef, 3f, 3f))
			{
				NelChipThunderShooter.PtcFoot.Draw(Ef, 30f);
				NelChipThunderShooter.PtcFootC.Draw(Ef, 50f);
			}
			return true;
		}

		public BDic<int, float> OHitLock
		{
			get
			{
				return this.LpArea.OHitLock;
			}
			set
			{
				this.LpArea.OHitLock = value;
			}
		}

		private bool pre_connected;

		protected M2LabelPointThunder LpArea;

		protected int aim;

		private const float hit_radius = 0.2f;

		private NelChipThunderShooter StAnother;

		private NelChipThunderShooter StAnotherO;

		protected M2ExtenderChecker Ext;

		private bool runner_assigned_;

		private M2DrawBinder Ed;

		protected float fined_floort;

		private static EfParticleOnce Ptc;

		private static EfParticleLooper PtcFoot;

		private static EfParticleLooper PtcHeadC;

		private static EfParticleLooper PtcFootC;

		protected M2MapDamageContainer.M2MapDamageItem MDI;

		private NearManager.NearTicketSrc NearTicket;

		public const float HITLOCK_T = 95f;

		private const float WIDE_HIT_REDUCE_SPD = 3f;

		private M2Ray Ray;

		protected float draw_margin_mapx = 1f;

		protected float draw_margin_mapy = 1f;

		protected bool no_draw_on_reach_len_is_zero = true;

		private float t_effect_running;

		private M2DrawBinder.FnEffectBind FD_fnDrawThunderEffect;
	}
}

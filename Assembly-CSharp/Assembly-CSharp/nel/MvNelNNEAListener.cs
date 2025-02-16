using System;
using System.Collections.Generic;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class MvNelNNEAListener : M2EventItem
	{
		public MvNelNNEAListener prepareCreateNNEA<T>(string _pxc_name, bool use_enemy_dir = false) where T : MvNelNNEAListener.NelNNpcEventAssign
		{
			this.FD_Create = (MvNelNNEAListener Mv) => Mv.Mp.createMover<T>("Assigned_" + Mv.pxc_name, Mv.x, Mv.y, false, false);
			return this.prepareCreateNNEA(_pxc_name, this.FD_Create, use_enemy_dir);
		}

		public MvNelNNEAListener prepareCreateNNEA(string _pxc_name, MvNelNNEAListener.FnCreateNNEA _FD_Create, bool use_enemy_dir = false)
		{
			this.pxc_name = _pxc_name;
			this.FD_Create = _FD_Create;
			if (use_enemy_dir)
			{
				EnemyData.loadPxl(this.Mp.M2D as NelM2DBase, this.pxc_name);
				this.pxc_name = "N_" + this.pxc_name;
			}
			else
			{
				this.Mp.M2D.loadMaterialPxl(this.pxc_name, "MapChars/" + this.pxc_name + ".pxls", true, true);
			}
			return this;
		}

		public override void runPre()
		{
			base.runPre();
			if (this.FD_Create != null)
			{
				PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter(this.pxc_name);
				if (pxlCharacter == null)
				{
					this.destruct();
					return;
				}
				if (this.Mp.update_mesh_flag_has_initaction)
				{
					return;
				}
				if (pxlCharacter.isLoadCompleted())
				{
					this.Assigned = this.FD_Create(this);
					if (this.Assigned == null)
					{
						this.destruct();
						return;
					}
					this.Assigned.first_mp_ratio = 1f;
					this.Assigned.AttachEvent = this;
					this.Mp.assignMover(this.Assigned, false);
					this.FD_Create = null;
					if (this.sync_enemy_position_first)
					{
						MvNelNNEAListener.stop_sync_another = true;
						this.setTo(this.Assigned.x, this.Assigned.y);
						MvNelNNEAListener.stop_sync_another = false;
						this.FirstPos = new Vector2(this.Assigned.x, this.Assigned.y);
					}
				}
			}
		}

		public override M2Mover setAim(AIM n, bool sprite_force_aim_set = false)
		{
			base.setAim(n, sprite_force_aim_set);
			if (this.Assigned != null && !MvNelNNEAListener.stop_sync_another)
			{
				MvNelNNEAListener.stop_sync_another = true;
				this.Assigned.setAim(n, sprite_force_aim_set);
				MvNelNNEAListener.stop_sync_another = false;
			}
			return this;
		}

		public override M2Mover setTo(float _x, float _y)
		{
			base.setTo(_x, _y);
			if (!MvNelNNEAListener.stop_sync_another)
			{
				this.FirstPos.Set(_x, _y);
				if (this.Assigned != null)
				{
					MvNelNNEAListener.stop_sync_another = true;
					this.Assigned.setTo(_x, _y);
					MvNelNNEAListener.stop_sync_another = false;
				}
			}
			return this;
		}

		public override M2Mover assignMoveScript(string str, bool soft_touch = false)
		{
			if (this.Assigned != null)
			{
				this.Assigned.assignMoveScript(str, soft_touch);
			}
			else
			{
				this.assignMoveScript(str, soft_touch);
			}
			return this;
		}

		public NelEnemy getAssignedEnemy()
		{
			return this.Assigned;
		}

		public static void ReadEvtS(EvReader ER, StringHolder rER, Map2d curMap)
		{
			MvNelNNEAListener mvNelNNEAListener = curMap.getMoverByName(rER._1, true) as MvNelNNEAListener;
			if (mvNelNNEAListener != null && mvNelNNEAListener.getAssignedEnemy() == null)
			{
				curMap.removeMover(mvNelNNEAListener);
				mvNelNNEAListener = null;
			}
			if (mvNelNNEAListener == null)
			{
				M2EventContainer eventContainer = curMap.getEventContainer();
				if (rER._2 != "INIT" || rER.clength <= 3)
				{
					X.de("ENGINE_NNEA の始動は INIT <pxl_key> で行うこと", null);
				}
				string _ = rER._1;
				if (_ != null && _ == "Barten")
				{
					mvNelNNEAListener = eventContainer.CreateAndAssignT<MvNelNNEAListener>(rER._1, false);
					mvNelNNEAListener.prepareCreateNNEA<NelNEvBarten>(rER._3, false);
				}
			}
			if (rER._2 == "INIT" && mvNelNNEAListener != null && M2EventCommand.EvME != null)
			{
				mvNelNNEAListener.setTo(M2EventCommand.EvME.x, M2EventCommand.EvME.y);
				mvNelNNEAListener.setAim(M2EventCommand.EvME.aim, false);
			}
		}

		private static bool stop_sync_another;

		public string pxc_name;

		public bool lock_first_position = true;

		public bool sync_enemy_size;

		public bool sync_enemy_position_first;

		private MvNelNNEAListener.NelNNpcEventAssign Assigned;

		private MvNelNNEAListener.FnCreateNNEA FD_Create;

		public Vector2 FirstPos;

		public delegate MvNelNNEAListener.NelNNpcEventAssign FnCreateNNEA(MvNelNNEAListener _Mv);

		public class NelNNpcEventAssign : NelEnemy, EnemySummoner.ISummonActivateListener
		{
			public override void appear(Map2d _Mp)
			{
				this.Mp = _Mp;
				this.anim_chara_name = this.AttachEvent.pxc_name;
				NOD.BasicData basicData = this.initializeFirst(null);
				base.appear(_Mp, basicData);
				base.weight = (this.weight0 = -1f);
				if (!this.mover_hitable)
				{
					this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, -1f);
				}
				if (!this.wall_hitable)
				{
					this.Phy.addLockWallHitting(this, -1f);
				}
				this.Anm.showToFront(false, false);
				this.Nai.add_enemy_target_count = false;
				this.Nai.awake_length = 5f;
				this.Nai.fnSleepLogic = new NAI.FnNaiLogic(this.fnSleepConsider);
				this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.fnAwakeLogic);
				base.addF((NelEnemy.FLAG)5242880);
				this.TeCon.setFadeIn(20f, 0f);
				EnemySummoner.addSummonerListener(this);
			}

			public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
			{
			}

			protected virtual NOD.BasicData initializeFirst(string npc_key)
			{
				this.kind = ENEMYKIND.DEVIL;
				this.id = ENEMYID.WANDER_PUPPET_NPC;
				return NOD.getBasicData(npc_key ?? "NPC_ENEMY");
			}

			public override void destruct()
			{
				if (base.destructed)
				{
					return;
				}
				base.destruct();
				EnemySummoner.remSummonerListener(this);
			}

			public override RAYHIT can_hit(M2Ray Ray)
			{
				if ((Ray.hittype & HITTYPE.GUARD_IGNORE) != HITTYPE.NONE || (this.throw_ray_normal_attack && Ray.projectile_power <= 0) || (this.throw_ray_magic_attack && Ray.projectile_power > 0) || Ray.Caster != this.Mp.Pr || EnemySummoner.isActiveBorder())
				{
					return RAYHIT.NONE;
				}
				if (this.throw_ray_when_fighting && EnemySummoner.isActiveBorder())
				{
					return RAYHIT.NONE;
				}
				return base.can_hit(Ray);
			}

			public override bool cannotHitTo(M2Mover Mv)
			{
				return true;
			}

			public override void runPre()
			{
				if (base.destructed)
				{
					return;
				}
				base.runPre();
				if (this.return_back_length >= 0f && this.AttachEvent.lock_first_position && X.LENGTHXYS(base.x, base.y, this.AttachEvent.FirstPos.x, this.AttachEvent.FirstPos.y) >= this.return_back_length)
				{
					this.setToDefaultPosition(false, null);
				}
			}

			protected override bool initDeathEffect()
			{
				if (base.destructed)
				{
					return false;
				}
				this.AttachEvent.destruct();
				return base.initDeathEffect();
			}

			public override void finePositionFromTransform()
			{
				if (this.Mp == null)
				{
					return;
				}
				base.finePositionFromTransform();
				if (!MvNelNNEAListener.stop_sync_another)
				{
					MvNelNNEAListener.stop_sync_another = true;
					this.AttachEvent.setTo(base.x, base.y);
					MvNelNNEAListener.stop_sync_another = false;
				}
			}

			public override void IntPositionChanged(int prex, int prey)
			{
				base.IntPositionChanged(prex, prey);
				if (this.Mp.Pr != null)
				{
					this.Mp.Pr.need_check_event = (this.Mp.Pr.need_check_talk_event_target = true);
				}
			}

			public override void positionChanged(float prex, float prey)
			{
				base.positionChanged(prex, prey);
				if (this.AttachEvent == this.Mp.TalkTarget)
				{
					base.M2D.changeTalkTarget(this.Mp.TalkTarget);
				}
			}

			public override M2Mover setAim(AIM n, bool sprite_force_aim_set = false)
			{
				base.setAim(n, sprite_force_aim_set);
				if (this.AttachEvent != null && !MvNelNNEAListener.stop_sync_another)
				{
					MvNelNNEAListener.stop_sync_another = true;
					this.AttachEvent.setAim(n, sprite_force_aim_set);
					MvNelNNEAListener.stop_sync_another = false;
				}
				return this;
			}

			public override M2Mover SizeW(float px_w = -1000f, float px_h = -1000f, ALIGN align = ALIGN.CENTER, ALIGNY aligny = ALIGNY.MIDDLE)
			{
				base.SizeW(px_w, px_h, align, aligny);
				if (this.AttachEvent != null && this.AttachEvent.sync_enemy_size)
				{
					base.Size(px_w / 2f, px_h / 2f, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				}
				return this;
			}

			public override M2Mover setTo(float _x, float _y)
			{
				base.setTo(_x, _y);
				if (this.AttachEvent != null && !MvNelNNEAListener.stop_sync_another)
				{
					MvNelNNEAListener.stop_sync_another = true;
					this.AttachEvent.setTo(_x, _y);
					MvNelNNEAListener.stop_sync_another = false;
				}
				return this;
			}

			public override bool fineFootType()
			{
				if (base.disappearing || base.hasF(NelEnemy.FLAG.NO_AUTO_LAND_EFFECT))
				{
					this.FootD.footstamp_type = FOOTSTAMP.NONE;
					return false;
				}
				this.FootD.footstamp_type = FOOTSTAMP.BARE;
				return true;
			}

			public override M2Mover setToDefaultPosition(bool no_set_camera = false, Map2d TargetMap = null)
			{
				this.setTo(this.AttachEvent.FirstPos.x, this.AttachEvent.FirstPos.y);
				if (this.Phy != null)
				{
					this.Phy.killSpeedForce(true, true, true, false, false);
				}
				return this;
			}

			public void openSummoner(EnemySummoner Smn, bool is_active_border)
			{
				if (this.invisible_when_fighting)
				{
					this.Anm.alpha = 0f;
				}
			}

			public void closeSummoner(EnemySummoner Smn, bool defeated)
			{
				if (this.invisible_when_fighting)
				{
					this.Anm.alpha = 1f;
				}
				this.setToDefaultPosition(false, null);
			}

			protected virtual bool fnSleepConsider(NAI Nai)
			{
				if (!Map2d.can_handle || this.isMoveScriptActive(false))
				{
					Nai.AddTicket(NAI.TYPE.GAZE, 128, true).SetAim(-2);
					return true;
				}
				return Nai.AddTicketB(NAI.TYPE.WAIT, 128, true);
			}

			protected virtual bool fnAwakeLogic(NAI Nai)
			{
				return this.fnSleepConsider(Nai) || Nai.AddTicketB(NAI.TYPE.WAIT, 128, true);
			}

			public override bool readTicket(NaTicket Tk)
			{
				if (Tk.type == NAI.TYPE.GAZE)
				{
					Tk.initProgress(this);
					if (Tk.prog == PROG.ACTIVE && this.t >= 120f)
					{
						if (Map2d.can_handle && !this.isMoveScriptActive(false))
						{
							return false;
						}
						this.t = X.NIXP(0f, 60f);
					}
					return true;
				}
				return base.readTicket(Tk);
			}

			public MvNelNNEAListener AttachEvent;

			public bool throw_ray_when_fighting = true;

			public bool invisible_when_fighting = true;

			public bool throw_ray_normal_attack = true;

			public bool throw_ray_magic_attack = true;

			public bool mover_hitable;

			public bool wall_hitable;

			public float return_back_length = 3f;

			protected bool killable;
		}
	}
}

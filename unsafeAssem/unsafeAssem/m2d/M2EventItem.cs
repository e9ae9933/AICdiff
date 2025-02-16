using System;
using evt;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2EventItem : M2Mover, IM2TalkableObject
	{
		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			this.ACmd = new M2EventCommand[12];
			this.floating = true;
			this.Mp.setTag(base.gameObject, "Mover");
			this.sp_shift_pixel_x = this.sp_shift_default_pixel_x;
			this.sp_shift_pixel_y = this.sp_shift_default_pixel_y;
			base.base_gravity = 0.5f;
			base.gameObject.layer = IN.LAY("Ignore Raycast");
			base.gameObject.isStatic = true;
		}

		public void destructMovePat()
		{
			if (this.MPat != null)
			{
				this.MPat.destruct();
				this.MPat = null;
			}
		}

		public M2Phys initPhysics()
		{
			bool flag = false;
			base.gameObject.isStatic = false;
			if (this.Phy == null)
			{
				this.floating = false;
				Rigidbody2D orAdd = IN.GetOrAdd<Rigidbody2D>(base.gameObject);
				this.Phy = new M2Phys(this, orAdd);
				base.M2D.AssignPauseableP(this.Phy);
				this.Phy.walk_xspeed_manageable_air = -1f;
				this.FootD = this.Phy.getFootManager();
				this.FootD.footstamp_type = FOOTSTAMP.SHOES;
				this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, -1f);
				flag = true;
			}
			if (this.CC == null)
			{
				this.CC = new M2MvColliderCreatorAtk(this);
			}
			if (flag)
			{
				this.fineTransformToPos(true, true);
				this.Phy.appear(this.Mp);
			}
			return this.Phy;
		}

		public override IFootable canFootOn(IFootable F)
		{
			if (this.MPat != null)
			{
				F = this.MPat.canFootOn(F);
				if (F == null)
				{
					return null;
				}
			}
			return base.canFootOn(F);
		}

		public M2MoverPr FollowPr
		{
			get
			{
				return this.Mp.Pr;
			}
		}

		public M2EventCommand assign(string _key, string str, bool do_not_announce = false)
		{
			bool flag = true;
			if (TX.isStart(_key, "!", 0))
			{
				flag = false;
				_key = TX.slice(_key, 1);
			}
			_key = _key.ToUpper();
			M2EventItem.CMD cmd;
			if (!FEnum<M2EventItem.CMD>.TryParse(_key, out cmd, true))
			{
				this.Mp.getEventContainer().setSpecialCommand(_key, new M2EventCommand(-1, null, base.key + "__" + _key, str, this.Mp));
				X.dl("EVC の特殊コマンド " + _key + " として登録します。", null, false, false);
				return null;
			}
			M2EventCommand m2EventCommand = this.assign(cmd, str, do_not_announce);
			if (!flag)
			{
				this.setExecutable(cmd, false);
			}
			return m2EventCommand;
		}

		public M2EventCommand assign(M2EventItem.CMD cmd, string str, bool do_not_announce = false)
		{
			M2EventCommand m2EventCommand = (this.ACmd[(int)cmd] = new M2EventCommand((int)cmd, this, base.key + "__" + FEnum<M2EventItem.CMD>.ToStr(cmd), str, this.Mp));
			m2EventCommand.do_not_announce = do_not_announce;
			if (cmd == M2EventItem.CMD.LOAD_ONCE)
			{
				this.Mp.cmd_loadevent_execute |= 2U;
			}
			if (cmd == M2EventItem.CMD.LOAD)
			{
				this.Mp.cmd_loadevent_execute |= 2U;
			}
			return m2EventCommand;
		}

		public override void runPre()
		{
			if (this.first_x == -1000f)
			{
				this.first_x = base.x;
			}
			if (this.lp_set_pos_randw != null && !this.Mp.need_update_collider)
			{
				Vector3 randomFootable = this.Mp.getEventContainer().getRandomFootable(this, TX.valid(this.lp_set_pos_randw) ? this.Mp.getLabelPoint(this.lp_set_pos_randw) : null);
				this.lp_set_pos_randw = null;
				if (randomFootable.z > 0f)
				{
					this.setTo(randomFootable.x, randomFootable.y - this.sizey);
				}
			}
			base.runPre();
			if (this.mtrtype != M2EventItem.PXMTR.NORMAL)
			{
				this.runMaterialUpdate();
			}
			if (this.TeCon != null)
			{
				this.TeCon.runDrawOrRedrawMesh(X.D_EF, (float)X.AF_EF, this.TS);
			}
			if (this.Phy != null)
			{
				M2FootManager footManager = this.Phy.getFootManager();
				if (footManager != null && this.no_foot_sound)
				{
					footManager.lockPlayFootStamp(4);
				}
			}
			if (this.first_anm_frame >= -1)
			{
				if (this.Anm != null && this.Anm.getCurrentTexture() != null && this.Anm.getCurrentSequence() != null)
				{
					this.Anm.animReset((this.first_anm_frame < 0) ? X.xors(this.Anm.getCurrentSequence().countFrames()) : this.first_anm_frame);
					this.first_anm_frame = -2;
				}
				if (this.Mobg != null && this.Mobg.MSq != null)
				{
					this.Mobg.animReset((this.first_anm_frame < 0) ? X.xors(this.Mobg.MSq.countFrames()) : this.first_anm_frame);
					this.first_anm_frame = -2;
				}
			}
			float num = this.TS * this.animator_TS;
			bool flag = false;
			if (this.MPat != null)
			{
				flag = this.MPat.run(num);
			}
			if (flag && this.Mp.TalkTarget == this)
			{
				M2MoverPr pr = this.Mp.Pr;
				if (pr != null)
				{
					pr.need_check_event = (pr.need_check_talk_event_target = true);
				}
			}
			if (this.Mobg != null)
			{
				this.Mobg.updateAnimator(num);
			}
			if (this.talk_target_ttt_ != (this.EfTalkTarget != null))
			{
				if (this.talk_target_ttt_)
				{
					if (M2EventItem.PFTalkTarget == null)
					{
						X.de("PFTalkTarget 未設定", null);
						return;
					}
					if (this.Get(M2EventItem.CMD.TALK) == null)
					{
						this.talk_target_ttt_ = false;
						this.EfTalkTarget = null;
						return;
					}
					this.EfTalkTarget = this.Mp.getEffect().setEffectWithSpecificFn("talk_target_ttt", 0f, 0f, (float)((this.Mp.floort < 5f) ? 2 : 1), 0, 0, delegate(EffectItem Ef)
					{
						if (!this.FnDrawTalkTarget(Ef))
						{
							if (Ef == this.EfTalkTarget)
							{
								this.EfTalkTarget = null;
							}
							return false;
						}
						return true;
					});
					return;
				}
				else
				{
					this.EfTalkTarget = null;
				}
			}
		}

		public bool need_load_event_check
		{
			get
			{
				return this.ACmd[5] != null || this.ACmd[0] != null;
			}
		}

		public void evLoadExecute()
		{
			if (this.ACmd[5] != null)
			{
				this.Mp.executeLoadOnceCommand(this);
				this.ACmd[5] = null;
			}
			if (this.ACmd[0] != null)
			{
				this.execute(M2EventItem.CMD.LOAD, null);
			}
		}

		public bool SubmitTalkable(M2MoverPr SubmitFrom)
		{
			if (SubmitFrom.isSubmitO(0) && (SubmitFrom.isMenuPD(1) || SubmitFrom.isRunO() || (SubmitFrom.isJumpO(0) && (SubmitFrom.isLO(0) || SubmitFrom.isRO(0)))))
			{
				return false;
			}
			if (this.talk_with_magic_key_)
			{
				if (!SubmitFrom.isMagicPD(1))
				{
					return false;
				}
			}
			else
			{
				if (base.M2D.t_lock_check_push_up > 0f)
				{
					return false;
				}
				KEY currentKeyAssignObject = IN.getCurrentKeyAssignObject();
				if (currentKeyAssignObject.isDupe(KEY.IPT.CHECK, KEY.IPT.JUMP, 2) || currentKeyAssignObject.isDupe(KEY.IPT.CHECK, KEY.IPT.A, 2) || currentKeyAssignObject.isDupe(KEY.IPT.CHECK, KEY.IPT.S, 2) || currentKeyAssignObject.isDupe(KEY.IPT.CHECK, KEY.IPT.D, 2))
				{
					if (!SubmitFrom.isCheckPD(1))
					{
						return false;
					}
				}
				else if (!SubmitFrom.isCheckU(0))
				{
					return false;
				}
			}
			if (this.hasCheck() && this.canExecutable(M2EventItem.CMD.CHECK))
			{
				this.execute(M2EventItem.CMD.CHECK, SubmitFrom);
				Map2d.can_handle = !EV.isStoppingGameHandle();
				return true;
			}
			if (this.hasTalk() && this.canExecutable(M2EventItem.CMD.TALK))
			{
				this.execute(M2EventItem.CMD.TALK, SubmitFrom);
				Map2d.can_handle = !EV.isStoppingGameHandle();
				return true;
			}
			return false;
		}

		public virtual void FocusTalkable()
		{
			this.execute(M2EventItem.CMD.FOCUS, null);
		}

		public virtual void BlurTalkable(bool event_init)
		{
		}

		public int canTalkable(bool when_battle_busy)
		{
			if (base.destructed || !this.hasTalkOrCheck() || !this.trigger_visible_)
			{
				return -1;
			}
			if (!when_battle_busy)
			{
				return 1;
			}
			return 0;
		}

		public M2EventItem clear()
		{
			if (this.ACmd != null)
			{
				X.ALLN<M2EventCommand>(this.ACmd);
			}
			this.trigger_visible_ = true;
			return this;
		}

		public M2EventItem remove(string _key)
		{
			_key = _key.ToUpper();
			M2EventItem.CMD cmd;
			if (FEnum<M2EventItem.CMD>.TryParse(_key, out cmd, true))
			{
				this.ACmd[(int)cmd] = null;
			}
			return this;
		}

		public M2EventItem remove(int i)
		{
			this.ACmd[i] = null;
			return this;
		}

		public M2Light setLight(uint col, float base_radius = -1f)
		{
			if ((col & 4278190080U) > 0U)
			{
				if (this.Lig == null)
				{
					this.Lig = new M2Light(this.Mp, this);
					this.Lig.follow_speed = 0.4f;
					this.Mp.addLight(this.Lig);
				}
				this.Lig.Col.Set(col);
				if (base_radius >= 0f)
				{
					this.Lig.radius = base_radius;
				}
				this.Lig.radius = 40f + this.sizey * base.CLEN * 2f;
			}
			else if (this.Lig != null)
			{
				this.Mp.remLight(this.Lig);
				this.Lig = null;
			}
			return this.Lig;
		}

		public float anm_timescale
		{
			get
			{
				if (this.Anm != null)
				{
					return this.Anm.timescale;
				}
				if (this.Mobg == null)
				{
					return 0f;
				}
				return this.Mobg.timeScale;
			}
			set
			{
				if (this.Anm != null)
				{
					this.Anm.timescale = value;
				}
				if (this.Mobg != null)
				{
					this.Mobg.timeScale = value;
				}
			}
		}

		public void setPxlMtrWithLight(bool flag = true)
		{
			if (flag)
			{
				if ((this.mtrtype & M2EventItem.PXMTR.WITHLIGHT) == M2EventItem.PXMTR.NORMAL)
				{
					this.mtrtype |= M2EventItem.PXMTR.P_WITHLIGHT;
					return;
				}
			}
			else
			{
				if ((this.mtrtype & M2EventItem.PXMTR.WITHLIGHT) != M2EventItem.PXMTR.NORMAL)
				{
					if (this.Anm != null)
					{
						MImage mi = MTRX.getMI(this.Anm.getCurrentCharacter());
						if (mi != null)
						{
							this.Anm.setRendererMaterial(mi.getMtr(BLEND.NORMALP3, -1));
						}
					}
					if (this.Mobg != null)
					{
						this.Mobg.fineMaterial();
					}
				}
				this.mtrtype &= (M2EventItem.PXMTR)4294967292U;
			}
		}

		public void setPxlMtrUseMask(bool flag = true)
		{
			if (flag)
			{
				if ((this.mtrtype & M2EventItem.PXMTR.USEMASK) == M2EventItem.PXMTR.NORMAL)
				{
					this.setPxlMtrWithLight(true);
					this.mtrtype |= M2EventItem.PXMTR.P_USEMASK;
					return;
				}
			}
			else
			{
				if ((this.mtrtype & M2EventItem.PXMTR.USEMASK) != M2EventItem.PXMTR.NORMAL)
				{
					this.fineAnimatorMaterialUseMask();
				}
				this.mtrtype &= (M2EventItem.PXMTR)4294967283U;
			}
		}

		private void runMaterialUpdate()
		{
			if (base.destructed)
			{
				return;
			}
			if ((this.mtrtype & M2EventItem.PXMTR.P_WITHLIGHT) != M2EventItem.PXMTR.NORMAL)
			{
				if (this.Anm != null)
				{
					MImage mimage = ((this.Anm != null) ? MTRX.getMI(this.Anm.getCurrentCharacter()) : null);
					if (mimage == null)
					{
						return;
					}
					this.Anm.setRendererMaterial(this.Mp.M2D.getWithLightTextureMaterial(mimage));
				}
				else if (this.Mobg != null)
				{
					MImage mi = this.Mobg.MI;
					this.Mobg.fineMaterial(this.Mp.M2D.getWithLightTextureMaterial(mi));
				}
				this.mtrtype = (this.mtrtype & (M2EventItem.PXMTR)4294967294U) | M2EventItem.PXMTR.WITHLIGHT;
			}
			if ((this.mtrtype & M2EventItem.PXMTR.P_USEMASK) != M2EventItem.PXMTR.NORMAL && this.fineAnimatorMaterialUseMask())
			{
				this.mtrtype = (this.mtrtype & (M2EventItem.PXMTR)4294967291U) | M2EventItem.PXMTR.USEMASK;
			}
		}

		private bool fineAnimatorMaterialUseMask()
		{
			if (this.Anm != null)
			{
				Material rendererMaterial = this.Anm.getRendererMaterial();
				MImage mi = MTRX.getMI(this.Anm.getCurrentCharacter());
				if (mi != null)
				{
					this.Mp.Dgn.setBorderMask(ref rendererMaterial, mi, true, null);
					this.Anm.setRendererMaterial(rendererMaterial);
					return true;
				}
			}
			if (this.Mobg != null)
			{
				Material rendererMaterial2 = this.Mobg.getRendererMaterial();
				MImage mi2 = MTRX.getMI(this.Anm.getCurrentCharacter());
				if (mi2 != null)
				{
					this.Mp.Dgn.setBorderMask(ref rendererMaterial2, mi2, true, null);
					this.Anm.setRendererMaterial(rendererMaterial2);
					return true;
				}
			}
			return false;
		}

		public bool hasStand()
		{
			return this.ACmd[3] != null;
		}

		public bool hasTouch()
		{
			return this.ACmd[4] != null;
		}

		public bool hasStandRelease()
		{
			return this.ACmd[6] != null;
		}

		public bool hasTalk()
		{
			return this.ACmd[1] != null;
		}

		public bool hasCheck()
		{
			return this.ACmd[2] != null;
		}

		public bool hasTalkOrCheck()
		{
			return this.ACmd[1] != null || this.ACmd[2] != null;
		}

		public void fineCheckDescName()
		{
			this.check_desc_name_ = null;
		}

		public void setCheckDescNameRaw(STB StbRaw, bool use_kd = true)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				if (use_kd)
				{
					this.addCheckDescKD(stb);
				}
				stb.Add(StbRaw);
				this.check_desc_name_ = stb.ToString();
			}
		}

		private void addCheckDescKD(STB Stb)
		{
			Stb.Add((this.hasCheck() ? this.canExecutable(M2EventItem.CMD.CHECK) : this.canExecutable(M2EventItem.CMD.TALK)) ? "<key " : "<key_s ");
			Stb.Add(this.talk_with_magic_key_ ? "x" : "check");
			Stb.Add("/>");
		}

		public void setCheckDescName(string tx_key, bool raw_content_flag = false, bool use_kd = true)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				if (use_kd)
				{
					this.addCheckDescKD(stb);
				}
				if (TX.valid(tx_key))
				{
					stb.AddTxA(tx_key, true);
				}
				else if (this.hasCheck())
				{
					stb.AddTxA("EV_access_default_check", false);
				}
				else if (this.hasTalk())
				{
					stb.AddTxA("EV_access_default_talk", false);
				}
				this.check_desc_name_ = stb.ToString();
			}
		}

		public string check_desc_name
		{
			get
			{
				if (this.check_desc_name_ == null)
				{
					bool flag = true;
					string text = "";
					if (TX.valid(this.check_desc_name0))
					{
						text = this.check_desc_name0;
						if (TX.isStart(this.check_desc_name0, "!", 0))
						{
							text = TX.slice(this.check_desc_name0, 1);
							flag = false;
						}
					}
					this.setCheckDescName(text, false, flag);
				}
				return this.check_desc_name_;
			}
			set
			{
				this.check_desc_name0 = value;
				this.check_desc_name_ = null;
			}
		}

		public M2EventCommand Get(string _key)
		{
			_key = _key.ToUpper();
			M2EventItem.CMD cmd;
			if (FEnum<M2EventItem.CMD>.TryParse(_key, out cmd, true))
			{
				return this.ACmd[(int)cmd];
			}
			X.de("M2EventItem::Get - 不明なコマンド実行パターン指定: " + _key, null);
			return null;
		}

		public M2EventCommand Get(M2EventItem.CMD cm)
		{
			return this.ACmd[(int)cm];
		}

		public bool trigger_visible
		{
			get
			{
				return this.trigger_visible_;
			}
			set
			{
				if (this.trigger_visible == value)
				{
					return;
				}
				this.trigger_visible_ = value;
				if (value && this.Mp != null && this.Mp.Pr != null)
				{
					this.Mp.Pr.need_check_event = true;
				}
			}
		}

		public M2EventItem setMovePattern(StringHolder rER)
		{
			M2MovePat.createMovePattern(rER, this, ref this.MPat);
			return this;
		}

		public M2EventItem setMovePattern(string key)
		{
			M2MovePat.createMovePattern(null, key, this, ref this.MPat);
			return this;
		}

		public void runMoveFollow()
		{
		}

		public override M2Mover assignMoveScript(string str, bool soft_touch = false)
		{
			if (EV.isStoppingGame())
			{
				this.event_stop = true;
			}
			if (this.MPat != null)
			{
				this.MPat.assignMoveScript();
			}
			return base.assignMoveScript(str, soft_touch);
		}

		public override bool isMoveScriptActive(bool only_moved_by_event = false)
		{
			return base.isMoveScriptActive(only_moved_by_event) && (!only_moved_by_event || this.event_stop);
		}

		public virtual M2EventCommand execute(M2EventItem.CMD _cmd, IM2EvTrigger executed_by)
		{
			M2EventCommand m2EventCommand = this.Get(_cmd);
			this.Mp.stackEventCommand(m2EventCommand, executed_by);
			if (_cmd == M2EventItem.CMD.TALK && this.EfTalkTarget != null)
			{
				if (this.EfTalkTarget.z > 0f)
				{
					this.EfTalkTarget.af = 0f;
				}
				this.EfTalkTarget.z = -2f;
			}
			return m2EventCommand;
		}

		public override void evInit()
		{
			base.evInit();
			if (this.MPat != null)
			{
				this.MPat.evStart(M2EventCommand.Ev0 == this);
			}
		}

		public override void evQuit()
		{
			base.evQuit();
			this.fix_pose = null;
			this.no_foot_sound = false;
			if (this.MPat != null)
			{
				this.MPat.evQuit();
			}
			if (this.EfTalkTarget != null && this.EfTalkTarget.z == -2f)
			{
				this.EfTalkTarget.z = -1f;
			}
		}

		public void setExecutable(M2EventItem.CMD _cm, bool flag = true)
		{
			if (this.canExecutable(_cm) == flag)
			{
				return;
			}
			if (flag)
			{
				this.executable |= 1U << (int)_cm;
			}
			else
			{
				this.executable &= ~(1U << (int)_cm);
			}
			if (_cm == M2EventItem.CMD.CHECK || _cm == M2EventItem.CMD.TALK)
			{
				this.check_desc_name_ = null;
			}
			if (this.Mp.TalkTarget as M2EventItem == this)
			{
				this.Mp.M2D.changeTalkTarget(this.Mp.TalkTarget);
			}
		}

		public void setExecutableAll(bool flag = true)
		{
			this.executable = (flag ? 4095U : 0U);
		}

		public bool canExecutable(M2EventItem.CMD _cm)
		{
			return ((ulong)this.executable & (ulong)(1L << (int)(_cm & (M2EventItem.CMD)31))) > 0UL;
		}

		public void spriteSetted()
		{
			base.savePreFloat();
		}

		public static void assignShadowIndex(string[] Aname, int index)
		{
		}

		public static void assignWeight(string[] Aname, float weight)
		{
		}

		public static void assignSize(string[] Aname, float sizex, float sizey = -1000f)
		{
		}

		public float event_cx
		{
			get
			{
				return base.x_shifted;
			}
		}

		public float event_cy
		{
			get
			{
				return base.y_shifted + this.event_center_shift_y - base.getSpTop() / base.CLEN * 0.66f;
			}
			set
			{
				this.event_center_shift_y = value - (base.y_shifted - base.getSpTop() / base.CLEN * 0.66f);
			}
		}

		public override bool SpSetShift(float _x = -1000f, float _y = -1000f)
		{
			this.sp_shift_pixel_x = ((_x == -1000f) ? this.sp_shift_default_pixel_x : _x);
			this.sp_shift_pixel_y = ((_y == -1000f) ? this.sp_shift_default_pixel_y : _y);
			return true;
		}

		public override float getSpShiftX()
		{
			return this.sp_shift_pixel_x + base.getSpShiftX();
		}

		public override float getSpShiftY()
		{
			return this.sp_shift_pixel_y + base.getSpShiftY();
		}

		public M2PxlAnimator setPxlChara(string s, string file_with_dir, string pose_name = "stand")
		{
			if (TX.noe(s) || (this.Anm != null && this.Anm.chara_title == s))
			{
				return this.Anm;
			}
			if (file_with_dir == null)
			{
				file_with_dir = "MapChars/" + s;
			}
			base.gameObject.isStatic = false;
			this.first_anm_frame = -1;
			this.Mp.M2D.loadMaterialPxl(s, file_with_dir + ".pxls", true, true);
			this.Anm = this.Mp.M2D.createBasicPxlAnimatorForRenderTicket(this, s, pose_name, true, M2Mover.DRAW_ORDER.PR0);
			this.Anm.TeCon = this.initAnimTeCon(this.Anm, this.Anm, null);
			this.Anm.auto_assign_tecon = false;
			this.SpSetPose(pose_name, -1000, null, false);
			this.Anm.setAim((int)this.aim, 1);
			return this.Anm;
		}

		private TransEffecter initAnimTeCon(ITeShift AnmPos = null, ITeColor AnmCol = null, ITeScaler AnmScl = null)
		{
			this.TeCon = new TransEffecter("M2m-" + this.ToString(), null, 10, 0, 0, EFCON_TYPE.NORMAL);
			this.TeCon.layer_effect_bottom = ((this.Mp.Dgn != null) ? this.Mp.Dgn.effect_layer_bottom : base.gameObject.layer);
			this.TeCon.layer_effect_top = ((this.Mp.Dgn != null) ? this.Mp.Dgn.effect_layer_top : base.gameObject.layer);
			if (AnmCol != null)
			{
				this.TeCon.RegisterCol(AnmCol, false);
			}
			if (AnmPos != null)
			{
				this.TeCon.RegisterPos(AnmPos);
			}
			if (AnmScl != null)
			{
				this.TeCon.RegisterScl(AnmScl);
			}
			if (this.Lig == null)
			{
				this.setLight(2295647944U, -1f);
			}
			return this.TeCon;
		}

		public M2MobGAnimator setMobgChara(string s, string pose_name = "stand")
		{
			if (TX.noe(s))
			{
				return this.Mobg;
			}
			if (this.Mobg == null)
			{
				base.gameObject.isStatic = false;
				this.first_anm_frame = -1;
				this.Mobg = new M2MobGAnimator(this, s);
				this.initAnimTeCon(this.Mobg, this.Mobg, this.Mobg);
			}
			else
			{
				this.Mobg.sklt_name = s;
			}
			if (TX.valid(pose_name))
			{
				this.SpSetPose(pose_name, -1000, null, false);
			}
			this.Mobg.setAim(this.aim, 1);
			return this.Mobg;
		}

		public bool setDod(M2Mover.DRAW_ORDER dod)
		{
			if (this.Anm != null)
			{
				this.Anm.order = dod;
				return true;
			}
			if (this.Mobg != null)
			{
				this.Mobg.order = dod;
				return true;
			}
			return false;
		}

		public bool anim_available
		{
			get
			{
				return this.Anm != null || this.Mobg != null;
			}
		}

		public override void SpSetPose(string nPose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
			if (!this.anim_available)
			{
				return;
			}
			if (fix_change == "")
			{
				this.fix_pose = null;
				fix_change = null;
			}
			if (fix_change != null)
			{
				nPose = (this.fix_pose = ((this.pose_prefix != null) ? (this.pose_prefix + fix_change) : fix_change));
			}
			else
			{
				if (this.fix_pose != null)
				{
					return;
				}
				nPose = ((this.pose_prefix != null) ? (this.pose_prefix + nPose) : nPose);
			}
			if (nPose != null)
			{
				if (this.Anm != null)
				{
					this.Anm.setPose(nPose, reset_anmf);
				}
				if (this.Mobg != null)
				{
					this.Mobg.SpSetPose(nPose, reset_anmf);
				}
			}
		}

		public override void SpMotionReset(int set_to_frame = 0)
		{
			if (this.Anm != null)
			{
				this.Anm.animReset(set_to_frame);
			}
			if (this.Mobg != null)
			{
				this.Mobg.animReset(set_to_frame);
			}
		}

		public override void breakPoseFixOnWalk(int i)
		{
			this.fix_pose = null;
		}

		public override M2Mover setAim(AIM n, bool sprite_force_aim_set = false)
		{
			if (this.aim != n || sprite_force_aim_set)
			{
				this.aim = n;
				if (!base.fix_aim || sprite_force_aim_set)
				{
					if (this.Anm != null)
					{
						this.Anm.setAim((int)n, 0);
					}
					if (this.Mobg != null)
					{
						this.Mobg.setAim(n, 0);
					}
				}
			}
			return this;
		}

		public bool MobgClipArea(M2LabelPoint Lp)
		{
			if (this.Mobg != null)
			{
				this.Mobg.ClipArea(Lp);
				return true;
			}
			return false;
		}

		public bool talk_with_magic_key
		{
			get
			{
				return this.talk_with_magic_key_;
			}
			set
			{
				if (this.talk_with_magic_key == value)
				{
					return;
				}
				this.talk_with_magic_key_ = value;
				this.check_desc_name_ = null;
			}
		}

		public bool talk_target_ttt
		{
			get
			{
				return this.talk_target_ttt_;
			}
			set
			{
				if (this.talk_target_ttt_ == value)
				{
					if (this.EfTalkTarget != null)
					{
						this.EfTalkTarget.time = 0;
					}
					return;
				}
				this.talk_target_ttt_ = value;
			}
		}

		private bool FnDrawTalkTarget(EffectItem Ef)
		{
			if (base.destructed)
			{
				return false;
			}
			if (Ef.time == 0)
			{
				Ef.time++;
				if (this.Get(M2EventItem.CMD.TALK) == null)
				{
					return false;
				}
			}
			if (!base.M2D.Cam.isCoveringMp(base.x, base.mtop, base.x, base.mtop, 60f))
			{
				return true;
			}
			if (Ef != this.EfTalkTarget)
			{
				if (Ef.z > 0f)
				{
					Ef.af = 0f;
				}
				Ef.z = 0f;
			}
			else if (Ef.z > 0f)
			{
				if (this.Mp.TalkTarget == this)
				{
					Ef.z = -1f;
					Ef.af = 0f;
				}
			}
			else if (Ef.z == -1f && this.Mp.TalkTarget != this)
			{
				Ef.z = 1f;
				Ef.af = 0f;
			}
			Ef.x = base.x;
			Ef.y = base.y;
			float num;
			if (Ef.z > 0f)
			{
				num = ((Ef.z == 2f) ? 1f : X.ZSIN2(Ef.af, 15f));
			}
			else
			{
				num = 1f - X.ZSIN(Ef.af, 15f);
				if (num <= 0f)
				{
					return Ef.z < 0f;
				}
			}
			MeshDrawer mesh = Ef.GetMesh("", 4278190080U, BLEND.MUL, false);
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
			mesh.Col = mesh.ColGrd.White().mulA(num).C;
			float num2 = X.NI(0.5f, 1f, num);
			float num3 = (this.sizey * base.CLENM + 20f) * num2 + 16f;
			float num4 = 58f * num;
			float num5 = 36f * num;
			float num6 = 10f * num;
			mesh.KadomaruRect(0f, num3, num4 + 2f, num5 + 2f, 7f * num6, 1.5f, false, 0f, 0f, false);
			mesh.Col = mesh.ColGrd.Black().mulA(num * 0.7f).C;
			mesh.KadomaruRect(0f, num3, num4, num5, 7f * num6, 0f, false, 0f, 0f, false);
			float num7 = num3 - num5 * 0.5f;
			mesh.Triangle(-5f * num, num7, 5f * num, num7, 0f, num7 - 8f * num, false);
			meshImg.Col = mesh.ColGrd.White().mulA(num * 0.5f).C;
			meshImg.RotaPF(0f, num3, 1f, 1f, 0f, M2EventItem.PFTalkTarget, false, false, false, uint.MaxValue, false, 0);
			return true;
		}

		public void addSndIntv(string pos_snd_key, string cue_name, float _interval, float _saf = 0f, int _play_count = -1)
		{
			if (TX.noe(pos_snd_key))
			{
				pos_snd_key = this.snd_key;
			}
			base.M2D.Snd.createInterval(pos_snd_key, cue_name, _interval, this, _saf, _play_count);
		}

		public void clearSndIntv(string pos_snd_key, string cue_name)
		{
			if (TX.noe(pos_snd_key))
			{
				pos_snd_key = this.snd_key;
			}
			if (TX.noe(cue_name))
			{
				base.M2D.Snd.stopP(pos_snd_key);
				return;
			}
			base.M2D.Snd.stop(cue_name, pos_snd_key);
		}

		public void closeAllStacked()
		{
			int num = this.ACmd.Length;
			for (int i = 0; i < num; i++)
			{
				if (this.ACmd[i] != null)
				{
					EV.unstackReader(this.ACmd[i]);
				}
			}
		}

		public Vector4 getMapPosAndSizeTalkableObject()
		{
			float num = (this.hasTalk() ? 1.5f : 0.3f);
			return new Vector4(base.x, base.y, this.sizex + num, this.sizey);
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			if (this.Lig != null)
			{
				this.Mp.remLight(this.Lig);
				this.Lig = null;
			}
			if (this.EfTalkTarget != null)
			{
				this.EfTalkTarget.destruct();
				this.EfTalkTarget = null;
			}
			Map2d mp = this.Mp;
			if (this.TeCon != null)
			{
				this.TeCon.clearRegistered();
				this.TeCon = null;
			}
			base.destruct();
			if (mp != null)
			{
				mp.destructEvent(this);
			}
		}

		public const bool auto_hide_ttt = true;

		internal bool appear_first = true;

		protected M2EventCommand[] ACmd;

		public static PxlFrame PFTalkTarget;

		private string check_desc_name_;

		private string check_desc_name0;

		public bool create_from_evc;

		public float stand_extend_map_w;

		private M2MovePat MPat;

		public bool no_foot_sound;

		public string pose_prefix;

		public string lp_set_pos_randw;

		public bool event_is_active;

		public bool event_stop;

		public Vector2[] APt;

		public float walkspeed = 0.082f;

		public bool stand_executable_on_event;

		public bool do_not_map_assign;

		public M2PxlAnimatorRT Anm;

		public M2MobGAnimator Mobg;

		public TransEffecter TeCon;

		private string fix_pose;

		protected M2Light Lig;

		public int first_anm_frame = -2;

		private bool talk_with_magic_key_;

		private M2EventItem.PXMTR mtrtype;

		public float first_x = -1000f;

		public static readonly float STAND_LENXY = 1.6f;

		public static readonly float TALK_LENXY = 2.4f;

		public static readonly float CHECK_LENXY = 1.4f;

		public float sp_shift_pixel_x;

		public float sp_shift_pixel_y;

		public readonly float sp_shift_default_pixel_x;

		public readonly float sp_shift_default_pixel_y = 10.5f;

		private bool talk_target_ttt_;

		public float event_center_shift_y;

		private EffectItem EfTalkTarget;

		private bool trigger_visible_ = true;

		private uint executable = 4095U;

		public const float LIGHT_BASE_RADIUS = 40f;

		public const float LIGHT_RADIUS_SIZE_MUL = 2f;

		public enum CMD
		{
			LOAD,
			TALK,
			CHECK,
			STAND,
			FOCUS,
			LOAD_ONCE,
			STAND_RELEASE,
			TRIGGER_A,
			TRIGGER_B,
			TRIGGER_C,
			TRIGGER_D,
			TRIGGER_E,
			_MAX
		}

		internal enum MOV_PAT
		{
			NONE,
			SEE,
			SEE_AROUND,
			WALKAROUND_LR,
			WALKAROUND_MAP,
			_MOVPAT_TYPE_ALL = 1048575,
			_NO_PLAY_FOOTSND = 16777216,
			_RIGHT = 33554432,
			_OFFLINE = 67108864
		}

		private enum PXMTR : uint
		{
			NORMAL,
			P_WITHLIGHT,
			WITHLIGHT,
			P_USEMASK = 4U,
			USEMASK = 8U
		}
	}
}

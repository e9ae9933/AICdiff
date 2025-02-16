using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class M2FootManager : ICarryable, IMapDamageListener
	{
		public M2FootManager(M2Phys _Phy)
		{
			this.Phy = _Phy;
			this.Mv = this.Phy.Mv;
			if (M2FootManager.ABccFootBuffer == null)
			{
				M2FootManager.ABccFootBuffer = new List<IBCCFootListener>(8);
				M2FootManager.ABufCp = new List<M2Puts>(2);
			}
		}

		public void initS()
		{
			this.Cur = null;
			this.CurBCC = (this.LadderBCC = null);
			this.foottype = null;
			this.t_foot = -1f;
			this.pre_search_x = (this.pre_search_y = -100f);
			this.t_lock_foot_fix = 0f;
			this.stamp_state = FOOTRES.KEEP_FOOT;
			this.need_recheck_current_pos = (this.need_recheck_bcc_cache = (this.need_recheck_foottype = true));
			this.need_recheck_bcc_foot_ = 6;
			this.no_change_shift_pixel = false;
			this.recheck_side = false;
			this.t_footstamp = -20;
			if (this.foottype == null)
			{
				this.foottype = this.Mp.Dgn.foot_type;
			}
			this.cache_lgt = 0;
			this.pivot = ALIGN.RIGHT;
			this.ABccl.Clear();
			M2FootManager.foot_fine_id = 1U;
			this.ABccFoot.Clear();
		}

		public void destruct()
		{
			if (this.Cur != null)
			{
				this.rideInitTo(null, false);
			}
		}

		public void positionChanged(float px, float py)
		{
			if (this.CurBCC != null)
			{
				if (this.CurBCC.BCC != this.Mp.BCC)
				{
					this.need_recheck_bcc_foot_ = X.Mx(0, this.need_recheck_bcc_foot_) + 1;
					return;
				}
				if (this.ABccFoot.Count > 0 && this.need_recheck_bcc_foot_ <= 0)
				{
					this.need_recheck_bcc_foot_--;
				}
			}
		}

		public void IntPositionChanged(float px, float py)
		{
			this.need_recheck_bcc_foot_ = 6;
			if (X.LENGTHXYN(this.pre_search_x, this.pre_search_y, px, py) >= 4f)
			{
				this.pre_search_x = px;
				this.pre_search_y = py;
				this.need_recheck_bcc_cache = true;
				this.cache_lgt = X.Mx(5 + X.IntC(X.Mx(this.sizex, this.sizey)), X.IntR(X.LENGTHXYN(0f, 0f, this.Phy.pre_force_velocity_x, this.Phy.pre_force_velocity_y)));
				if (this.Mv.NCM != null)
				{
					this.Mv.NCM.grid4_updated = true;
				}
				if (this.Mv == this.Mp.M2D.Cam.getBaseMover())
				{
					this.Mp.M2D.mainMvIntPosChanged(true);
				}
			}
			if (this.CurBCC != null && this.CurBCC.is_map_bcc)
			{
				this.need_recheck_foottype = true;
			}
			if (this.auto_search_ladder && !this.need_recheck_bcc_cache)
			{
				this.searchLadder();
			}
			if (this.hasFoot())
			{
				this.need_recheck_foottype = true;
			}
		}

		public M2BlockColliderContainer.BCCLine searchLadder()
		{
			if (this.CurBCC != null && this.CurBCC.is_ladder)
			{
				return this.LadderBCC = this.CurBCC;
			}
			this.LadderBCC = null;
			this.recheckCache();
			float num = 2f;
			for (int i = this.ABccl.Count - 1; i >= 0; i--)
			{
				M2BlockColliderContainer.BCCLine bccline = this.ABccl[i];
				if (bccline.is_ladder)
				{
					float num2 = X.Abs(bccline.shifted_cx - this.Mv.x);
					if (num2 < num && X.Abs(bccline.shifted_cy - this.Mv.y) < bccline.height * 0.5f + this.Mv.sizey + 1f)
					{
						this.LadderBCC = bccline;
						num = num2;
					}
				}
			}
			return this.LadderBCC;
		}

		public bool canFootOnCurrentFoot()
		{
			return this.Cur != null && this.Cur.isCarryable(this) != null;
		}

		public void recheckCache()
		{
			if (!this.need_recheck_bcc_cache || this.Mp.BCC == null || !this.Mp.BCC.is_prepared)
			{
				return;
			}
			this.need_recheck_bcc_cache = false;
			this.need_recheck_current_pos = true;
			this.foottype = null;
			this.ABccl.Clear();
			this.Mp.BCC.cacheNearFoot(this, this.ABccl, this.Mv.mleft, this.Mv.mtop, this.Mv.mright, this.Mv.mbottom, 1.5f + (float)this.cache_lgt);
			if (this.auto_search_ladder)
			{
				this.searchLadder();
			}
		}

		public IFootable checkFootObject(float pre_fall_y)
		{
			this.recheckCache();
			if (this.Mp.BCC != null)
			{
				M2BlockColliderContainer.BCCLine bccline = this.Mp.BCC.checkFootCarryable(this, this.ABccl, pre_fall_y);
				if (bccline != null && this.rideInitTo(bccline, false) != null)
				{
					return this.Cur;
				}
			}
			for (int i = this.Mp.count_carryable_bcc - 1; i >= 0; i--)
			{
				M2BlockColliderContainer.BCCLine bccline2 = this.Mp.getCarryableBCCByIndex(i).checkFootCarryable(this);
				if (bccline2 != null && this.rideInitTo(bccline2, false) != null)
				{
					return this.Cur;
				}
			}
			return null;
		}

		public IFootable FixPX(IFootable F, float _x)
		{
			return F;
		}

		public IFootable FixPY(IFootable F, float _y)
		{
			return F;
		}

		public IFootable rideInitTo(IFootable F, bool from_jump_init = false)
		{
			if (F == this.Cur)
			{
				return this.Cur;
			}
			bool flag = false;
			if (this.Cur != null)
			{
				flag = true;
				this.Cur.quitCarry(this);
			}
			if (F != null)
			{
				F = this.Mv.canFootOn(F);
			}
			if (F != null)
			{
				this.shift_pixel_x = (this.shift_pixel_y = 0f);
			}
			this.quitFootListener(true, from_jump_init);
			this.Cur = ((F != null) ? F.initCarry(this) : null);
			this.vague_foot = true;
			FOOTRES footres;
			if (this.Cur != null)
			{
				this.CurBCC = this.Cur as M2BlockColliderContainer.BCCLine;
				this.need_recheck_foottype = true;
				this.need_recheck_bcc_foot_ = 6;
				this.recheck_side = true;
				if (this.CurBCC != null)
				{
					if (!this.CurBCC.is_ladder)
					{
						this.LastBCC = this.CurBCC;
					}
					else
					{
						this.LadderBCC = this.CurBCC;
					}
				}
				if (!flag)
				{
					footres = FOOTRES.FOOTED;
					this.playFootStampEffect(true);
					this.foottype = null;
					this.t_foot = 0f;
				}
				else
				{
					footres = FOOTRES.KEEP_FOOT;
				}
			}
			else
			{
				this.CurBCC = null;
				if (flag)
				{
					this.t_foot = -1f;
					footres = (from_jump_init ? FOOTRES.JUMPED : FOOTRES.JUMPED);
				}
				else
				{
					footres = FOOTRES.KEEP_AIR;
				}
			}
			if (this.Phy.getFootManager() != this)
			{
				return this.Cur;
			}
			this.Phy.changeRiding(F, footres);
			return this.Cur;
		}

		private void quitFootListener(bool recheck_evnet = false, bool from_jump_init = false)
		{
			if (recheck_evnet && this.need_recheck_bcc_foot_ >= 6)
			{
				this.BccFootCheck();
			}
			for (int i = this.ABccFoot.Count - 1; i >= 0; i--)
			{
				this.ABccFoot[i].footedQuit(this, from_jump_init);
			}
			this.ABccFoot.Clear();
		}

		public void remFootListener(IBCCFootListener Lsn)
		{
			this.ABccFoot.Remove(Lsn);
			this.need_recheck_foottype = true;
		}

		public void initJump(bool recheck_foot = false, bool no_footstamp_snd = false, bool remain_foot_margin = false)
		{
			int num = this.t_footstamp;
			if (this.Cur != null)
			{
				if (!no_footstamp_snd)
				{
					this.fineFootStampType();
				}
				this.t_footstamp = 1;
			}
			this.rideInitTo(null, true);
			this.t_foot = X.Mn(-1f, this.t_foot);
			if (!remain_foot_margin)
			{
				this.vague_foot = false;
			}
			this.Phy.clearFric();
			if (recheck_foot)
			{
				this.Phy.recheckFoot(0f);
			}
			if (no_footstamp_snd)
			{
				this.t_footstamp = num;
				return;
			}
			this.stamp_state = FOOTRES.JUMPED;
		}

		public void playFootStampEffect(bool rideinit)
		{
			if (this.t_footstamp == 0)
			{
				this.t_footstamp = 1;
				this.stamp_state = FOOTRES.KEEP_FOOT;
			}
			if (this.t_footstamp == 1)
			{
				FOOTRES footres = (rideinit ? FOOTRES.FOOTED : FOOTRES.KEEP_FOOT);
				if (footres > this.stamp_state)
				{
					this.stamp_state = footres;
				}
			}
		}

		public void lockPlayFootStamp(int lock_t)
		{
			this.t_footstamp = X.Mn(-lock_t, this.t_footstamp);
		}

		public void fineFootStampType()
		{
			this.recheckCache();
			if (this.need_recheck_bcc_foot_ >= 6)
			{
				this.BccFootCheck();
			}
			if (this.CurBCC != null && (this.need_recheck_foottype || !this.CurBCC.is_map_bcc))
			{
				float x = this.Mv.x;
				float sizex = this.Mv.sizex;
				int bcc_xd = this.bcc_xd;
				float y = this.Mv.y;
				float sizey = this.Mv.sizey;
				int bcc_yd = this.bcc_yd;
				this.need_recheck_foottype = false;
				string text = this.foottype;
				if (CCON.isWater(this.Mp.getConfig((int)this.Phy.tstacked_x, (int)(this.Phy.tstacked_y + this.Mv.sizey - 0.1f))))
				{
					this.foottype = "water";
				}
				else if (this.footstamp_type == FOOTSTAMP.NONE || this.footstamp_type == FOOTSTAMP.SHOES || this.footstamp_type == FOOTSTAMP.BAREFOOT)
				{
					M2BlockColliderContainer.BCCPos footStampChip = this.CurBCC.getFootStampChip((int)this.pivot, this.Phy.tstacked_x, this.Phy.tstacked_y, this.Mv.sizex, this.Mv.sizey);
					if (footStampChip.valid)
					{
						this.foottype = footStampChip.foot_type;
						if (TX.noe(this.foottype))
						{
							this.foottype = this.Mp.Dgn.foot_type;
						}
					}
				}
				else
				{
					this.foottype = "ground";
				}
				if (this.foottype == "ice" && text != "ice" && text != null)
				{
					this.Phy.setWalkXSpeed(this.Phy.walk_xspeed * 0.5f, false, false);
					this.Phy.addOnIce();
				}
				int count = this.ABccFoot.Count;
				for (int i = 0; i < count; i++)
				{
					this.ABccFoot[i].rewriteFootType(this.CurBCC, this, ref this.foottype);
				}
			}
		}

		private void playFootStampEffectExecute()
		{
			this.fineFootStampType();
			if (TX.noe(this.foottype))
			{
				return;
			}
			float num = this.Mv.x + this.Mv.sizex * (float)this.bcc_xd;
			float num2 = this.Mv.y - this.Mv.sizey * (float)this.bcc_yd;
			string text = this.foottype;
			if (this.foottype == "water")
			{
				text = this.Mp.M2D.checkWaterFootSound(this.Mv, num, num2, ref this.foottype);
				int num3 = (int)num;
				int num4 = (int)(num2 + (float)this.bcc_yd * 0.02f);
				if (CCON.isWater(this.Mp.getConfig(num3, num4)))
				{
					M2FootManager.ABufCp.Clear();
					if (this.Mp.getPointMetaPutsTo(num3, num4, M2FootManager.ABufCp, "water") > 0)
					{
						IActivatableByMv activatableByMv = M2FootManager.ABufCp[0] as IActivatableByMv;
						if (activatableByMv != null)
						{
							activatableByMv.activate(this.Mv);
						}
					}
				}
			}
			string text2 = null;
			switch (this.footstamp_type)
			{
			case FOOTSTAMP.BIG:
				if (this.foottype == "water")
				{
					text2 = "dive_to_water";
				}
				else
				{
					if (this.stamp_state == FOOTRES.FOOTED)
					{
						this.Mp.PtcSTsetVar("sizex", (double)(this.sizex * this.Mp.CLENB)).PtcSTsetVar("bx", (double)num).PtcSTsetVar("by", (double)num2)
							.PtcSTsetVar("bagR", (double)this.bcc_agR)
							.PtcST("enemy_ground_bump_s", null, PTCThread.StFollow.NO_FOLLOW);
						return;
					}
					this.Mp.PtcSTsetVar("bx", (double)num).PtcSTsetVar("by", (double)num2).PtcST("enemy_footsound_big_walk", null, PTCThread.StFollow.NO_FOLLOW);
					return;
				}
				break;
			case FOOTSTAMP.OVERDRIVE:
				if (!(this.foottype == "water"))
				{
					this.Mp.PtcSTsetVar("sizex", (double)(this.sizex * this.Mp.CLENB)).PtcSTsetVar("bx", (double)num).PtcSTsetVar("by", (double)num2)
						.PtcSTsetVar("bagR", (double)this.bcc_agR)
						.PtcST("enemy_ground_bump", null, PTCThread.StFollow.NO_FOLLOW);
					return;
				}
				text2 = "dive_to_water";
				break;
			case FOOTSTAMP.HEEL:
				if (this.foottype == "metal" || this.foottype == "normal" || this.foottype == "ground")
				{
					text2 = "foot_heel";
				}
				break;
			}
			string text3 = this.foottype;
			if (text3 != null)
			{
				uint num5 = <PrivateImplementationDetails>.ComputeStringHash(text3);
				if (num5 <= 1219850847U)
				{
					if (num5 != 265927825U)
					{
						if (num5 != 642305365U)
						{
							if (num5 == 1219850847U)
							{
								if (text3 == "void")
								{
									text = null;
								}
							}
						}
						else if (text3 == "leaf")
						{
							this.Mp.PtcN("foot_leaf", num, num2, 0f, (int)(5U + X.xors() % 4U), 0);
						}
					}
					else if (text3 == "web")
					{
						this.Mp.PtcSTsetVar("cx", (double)num).PtcSTsetVar("cy", (double)(num2 + 0.15f)).PtcST("foot_web", this.Mv as IEfPInteractale, PTCThread.StFollow.NO_FOLLOW);
						text = null;
					}
				}
				else
				{
					if (num5 <= 1927346304U)
					{
						if (num5 != 1640418574U)
						{
							if (num5 != 1927346304U)
							{
								goto IL_0419;
							}
							if (!(text3 == "ice"))
							{
								goto IL_0419;
							}
						}
						else
						{
							if (!(text3 == "leafground"))
							{
								goto IL_0419;
							}
							goto IL_0419;
						}
					}
					else if (num5 != 3522460157U)
					{
						if (num5 != 4060326187U)
						{
							goto IL_0419;
						}
						if (!(text3 == "glass"))
						{
							goto IL_0419;
						}
					}
					else
					{
						if (!(text3 == "woodbridge"))
						{
							goto IL_0419;
						}
						goto IL_0419;
					}
					if (this.footstamp_type == FOOTSTAMP.BAREFOOT)
					{
						text2 = (text = "foot_bare");
					}
					else
					{
						text = "glass";
					}
				}
			}
			IL_0419:
			if (TX.valid(text))
			{
				if (text2 == null)
				{
					if (!M2FootManager.HashFoot.Has(text))
					{
						M2FootManager.HashFoot.Set(text, "foot_" + text);
					}
					text2 = M2FootManager.HashFoot.Get(text);
				}
				this.Mv.playFootSound(text2, 1);
			}
		}

		public void runPre()
		{
			if (this.t_footstamp > 0)
			{
				if (this.Phy.no_play_footsnd_animator || this.footstamp_type == FOOTSTAMP.NONE)
				{
					this.t_footstamp = 0;
				}
				else
				{
					this.playFootStampEffectExecute();
					this.t_footstamp = -15;
					this.stamp_state = FOOTRES.KEEP_FOOT;
				}
			}
			if (this.t_footstamp < 0)
			{
				this.t_footstamp++;
			}
			if (this.t_foot >= 0f)
			{
				if (this.need_recheck_foottype)
				{
					this.fineFootStampType();
				}
				if (this.foottype == "ice")
				{
					this.Phy.addOnIce();
				}
				if (!this.no_change_shift_pixel)
				{
					if (this.shift_pixel_x != 0f)
					{
						this.shift_pixel_x = X.VALWALK(this.shift_pixel_x, 0f, Map2d.TS * 2f);
					}
					if (this.shift_pixel_y != 0f)
					{
						this.shift_pixel_y = X.VALWALK(this.shift_pixel_y, 0f, Map2d.TS * 2f);
					}
				}
				this.t_foot += 1f;
			}
			else if (this.Phy.fineGravityScale() > 0f)
			{
				this.t_foot -= 1f;
			}
			if (this.need_recheck_bcc_foot_ >= 6)
			{
				this.BccFootCheck();
				return;
			}
			if (this.need_recheck_bcc_foot_ <= -6)
			{
				this.need_recheck_bcc_foot_ = 0;
				this.need_recheck_foottype = M2FootManager.BccFootRunDL(this, this.CurBCC, this.ABccFoot, this.footable_bits_) || this.need_recheck_foottype;
			}
		}

		private void BccFootCheck()
		{
			this.need_recheck_bcc_foot_ = 0;
			this.need_recheck_foottype = M2FootManager.BccFootCheckDL(this, this.CurBCC, this.ABccFoot, this.footable_bits_) || this.need_recheck_foottype;
		}

		public static bool BccFootCheckDL(IMapDamageListener Lsn, M2BlockColliderContainer.BCCLine CurBCC, List<IBCCFootListener> ABccFoot, uint footable_bits)
		{
			M2FootManager.ABccFootBuffer.Clear();
			M2FootManager.ABccFootBuffer.AddRange(ABccFoot);
			ABccFoot.Clear();
			if (CurBCC != null && CurBCC.AFootLsnRegistered != null)
			{
				DRect mapBounds = Lsn.getMapBounds(M2BlockColliderContainer.BufRc);
				if (mapBounds != null)
				{
					float left = mapBounds.left;
					float right = mapBounds.right;
					float top = mapBounds.top;
					float bottom = mapBounds.bottom;
					List<IBCCFootListener> afootLsnRegistered = CurBCC.AFootLsnRegistered;
					for (int i = afootLsnRegistered.Count - 1; i >= 0; i--)
					{
						IBCCFootListener ibccfootListener = afootLsnRegistered[i];
						if ((ibccfootListener.getFootableAimBits() & footable_bits) != 0U)
						{
							DRect mapBounds2 = ibccfootListener.getMapBounds(M2BlockColliderContainer.BufRc);
							if (mapBounds2 != null && mapBounds2.active && mapBounds2.isCoveringXy(left, top, right, bottom, 0.0625f, -1000f))
							{
								int num = M2FootManager.ABccFootBuffer.IndexOf(ibccfootListener);
								if (num < 0)
								{
									if (!ibccfootListener.footedInit(CurBCC, Lsn))
									{
										goto IL_00F2;
									}
								}
								else
								{
									M2FootManager.ABccFootBuffer.RemoveAt(num);
								}
								ABccFoot.Add(ibccfootListener);
							}
						}
						IL_00F2:;
					}
				}
			}
			bool flag = M2FootManager.ABccFootBuffer.Count > 0;
			for (int j = M2FootManager.ABccFootBuffer.Count - 1; j >= 0; j--)
			{
				M2FootManager.ABccFootBuffer[j].footedQuit(Lsn, false);
			}
			M2FootManager.ABccFootBuffer.Clear();
			return flag;
		}

		public static bool BccFootRunDL(IMapDamageListener Lsn, M2BlockColliderContainer.BCCLine CurBCC, List<IBCCFootListener> ABccFoot, uint footable_bits)
		{
			if (ABccFoot.Count == 0)
			{
				return false;
			}
			DRect mapBounds = Lsn.getMapBounds(M2BlockColliderContainer.BufRc);
			float left = mapBounds.left;
			float right = mapBounds.right;
			float top = mapBounds.top;
			float bottom = mapBounds.bottom;
			bool flag = false;
			for (int i = ABccFoot.Count - 1; i >= 0; i--)
			{
				IBCCFootListener ibccfootListener = ABccFoot[i];
				bool footableAimBits = ibccfootListener.getFootableAimBits() != 0U;
				bool flag2 = false;
				if (((footableAimBits ? 1U : 0U) & footable_bits) != 0U)
				{
					DRect mapBounds2 = ibccfootListener.getMapBounds(M2BlockColliderContainer.BufRc);
					flag2 = mapBounds2 != null && mapBounds2.active && mapBounds2.isCoveringXy(left, top, right, bottom, 0.0625f, -1000f);
				}
				if (!flag2)
				{
					ibccfootListener.footedQuit(Lsn, false);
					ABccFoot.RemoveAt(i);
					flag = true;
				}
			}
			return flag;
		}

		public void runPostAttakable(M2Attackable MvA)
		{
			this.recheckCache();
			bool flag = Map2d.can_handle && MvA.check_dangerous_bcc;
			if (this.need_recheck_current_pos)
			{
				this.need_recheck_current_pos = false;
				for (int i = this.ABccl.Count - 1; i >= 0; i--)
				{
					M2BlockColliderContainer.BCCLine bccline = this.ABccl[i];
					float num;
					float num2;
					bccline.BCC.getBaseShift(out num, out num2);
					MvA.checkBCCEvent(bccline, ref flag, num, num2);
				}
			}
			else if (this.CurBCC != null && this.CurBCC.BCC == this.Mp.BCC)
			{
				MvA.checkBCCEvent(this.CurBCC, ref flag, this.CurBCC.BCC.base_shift_x, this.CurBCC.BCC.base_shift_y);
			}
			for (int j = this.Mp.count_carryable_bcc - 1; j >= 0; j--)
			{
				M2BlockColliderContainer carryableBCCByIndex = this.Mp.getCarryableBCCByIndex(j);
				if (carryableBCCByIndex != null && carryableBCCByIndex.isBoundsCovering(MvA.mleft, MvA.mtop, MvA.mright, MvA.mbottom, 0.25f))
				{
					carryableBCCByIndex.checkBCCEvent(MvA, ref flag);
				}
			}
		}

		public void runPhysics(float fcnt)
		{
			this.expandCacheLgt(X.IntR(X.LENGTHXYN(0f, 0f, this.Phy.pre_force_velocity_x, this.Phy.pre_force_velocity_y)), true);
			if (this.Cur != null && this.t_lock_foot_fix == 0f && this.auto_fix_to_foot)
			{
				float num2;
				float num3;
				float num = this.Cur.fixToFootPos(this, this.Phy.move_depert_tstack_x, this.Phy.move_depert_tstack_y, out num2, out num3);
				if (num > 0f && (X.LENGTHXYN(num2, num3, 0f, 0f) >= 0.004f || num3 < 0f || this.t_foot < 40f))
				{
					this.Phy.addTranslateStack(X.absMn(num2, num), X.absMn(num3, num));
				}
			}
			if (this.t_lock_foot_fix > 0f)
			{
				this.t_lock_foot_fix = X.Mx(0f, this.t_lock_foot_fix - fcnt);
			}
			if (this.recheck_side && !this.Phy.isLockWallHittingActive())
			{
				this.recheck_side = false;
				float num4;
				if ((num4 = 0.15f - this.Mv.canGoToSideL(AIM.R, 0.15f, -0.55f, false, false, false)) > 0.006f)
				{
					this.recheck_side = true;
					this.Phy.addTranslateStack(-X.Mn(num4, 0.1f), 0f);
					return;
				}
				if ((num4 = 0.15f - this.Mv.canGoToSideL(AIM.L, 0.15f, -0.55f, false, false, false)) > 0.006f)
				{
					this.recheck_side = true;
					this.Phy.addTranslateStack(X.Mn(num4, 0.1f), 0f);
				}
			}
		}

		public void setShiftPixel(IFootable F, float pixel_x, float pixel_y)
		{
			if (F != this.Cur && (this.CurBCC == null || this.CurBCC.BCC.BelongTo != F))
			{
				return;
			}
			this.shift_pixel_x = pixel_x;
			this.shift_pixel_y = pixel_y;
		}

		public bool moveWithFoot(float dx, float dy, Collider2D _Collider, M2BlockColliderContainer BCCCarrier, bool no_collider_lock = false)
		{
			if (this.Mv.isDestructed())
			{
				return false;
			}
			this.Mv.moveWithFoot(dx, dy, _Collider, BCCCarrier, true, no_collider_lock);
			return true;
		}

		public Vector2 getFootingMapPos()
		{
			AIM aim = AIM.B;
			float num = 0.5f;
			if (this.CurBCC != null)
			{
				aim = this.CurBCC.aim;
				if (this.CurBCC.is_naname)
				{
					num = 0f;
				}
			}
			else
			{
				int num2 = 4;
				int num3 = 0;
				while (num3 < num2 && ((ulong)this.footable_bits & (ulong)(1L << (int)(aim & (AIM)31U))) == 0UL)
				{
					aim = CAim.get_clockwise2(aim, false);
					num3++;
				}
			}
			return new Vector2(this.Mv.x + (this.Mv.sizex + num) * (float)CAim._XD(aim, 1), this.Mv.y - (this.Mv.sizey + num) * (float)CAim._YD(aim, 1));
		}

		public bool reverseVelocityApplied(float vx, float vy)
		{
			if (this.CurBCC == null)
			{
				return false;
			}
			if (this.CurBCC.is_ladder)
			{
				return X.Abs(vx) > 0.01f;
			}
			AIM aim = this.CurBCC.aim;
			if (aim == AIM.L)
			{
				return vx > 0.01f;
			}
			if (aim != AIM.R)
			{
				return X.Abs(vy) > 0.01f && CAim._YD(this.CurBCC.aim, 1) > 0 == vy > 0f;
			}
			return vx < -0.01f;
		}

		public void CliffStopCrop(ref float force_velocity_x)
		{
			if (force_velocity_x == 0f)
			{
				return;
			}
			M2BlockColliderContainer.BCCLine footBCC = this.get_FootBCC();
			if (footBCC != null && !footBCC.isWall())
			{
				M2BlockColliderContainer.BCCLine bccline = ((force_velocity_x < 0f) ? footBCC.SideL : footBCC.SideR);
				if (bccline == null || !bccline.isUseableDir(this))
				{
					float num = force_velocity_x;
					float num2;
					float num3;
					footBCC.BCC.getBaseShift(out num2, out num3);
					if (force_velocity_x < 0f)
					{
						force_velocity_x = X.Mx(force_velocity_x, footBCC.x - num2 - this.mleft);
					}
					else
					{
						force_velocity_x = X.Mn(force_velocity_x, footBCC.right - num2 - this.mright);
					}
					if (force_velocity_x != num)
					{
						bool is_lift = footBCC.is_lift;
						M2BlockColliderContainer.BCCLine bccline2;
						footBCC.BCC.isFallable((float)X.MPF(num > 0f) * (this.Mv.sizex + 0.35f) + this.Mv.x, this.Mv.mbottom, 0.001f, 0.25f, out bccline2, is_lift, !is_lift, -1f, null);
						if (bccline2 != null && bccline2 != footBCC)
						{
							force_velocity_x = num;
						}
					}
				}
			}
		}

		public void lockFootFix(int _time)
		{
			this.t_lock_foot_fix = X.Mx(this.t_lock_foot_fix, (float)_time);
		}

		public M2BlockColliderContainer.BCCLine findBcc(Func<M2BlockColliderContainer.BCCLine, bool> Fn)
		{
			int count = this.ABccl.Count;
			for (int i = 0; i < count; i++)
			{
				M2BlockColliderContainer.BCCLine bccline = this.ABccl[i];
				if (Fn(bccline))
				{
					return bccline;
				}
			}
			if (this.CurBCC != null && this.CurBCC.BCC != this.Mp.BCC)
			{
				return this.CurBCC.BCC.findBcc(Fn);
			}
			return null;
		}

		public void holdFallingFootMargin()
		{
			if (this.t_foot < 0f && this.t_foot > -7f)
			{
				this.t_foot = -1f;
			}
		}

		public DRect getMapBounds(DRect Buf)
		{
			Buf = Buf ?? M2BlockColliderContainer.BufRc;
			return Buf.Set(this.mleft, this.mtop, this.Mv.sizex * 2f, this.Mv.sizey * 2f);
		}

		public void applyMapDamage(M2MapDamageContainer.M2MapDamageItem MapDmg, M2BlockColliderContainer.BCCLine Bcc)
		{
			M2Attackable m2Attackable = this.Mv as M2Attackable;
			if (m2Attackable == null || this.Mv.destructed)
			{
				return;
			}
			float num;
			float num2;
			AttackInfo atk = MapDmg.GetAtk(Bcc, m2Attackable, out num, out num2);
			if (atk != null)
			{
				m2Attackable.applyDamageFromMap(MapDmg, atk, num, num2, true);
			}
		}

		public bool isCenterPr()
		{
			return this.Mp.M2D.isCenterPlayer(this.Mv);
		}

		public void expandCacheLgt(int lgt, bool recheck_cache_execute = true)
		{
			if (lgt > this.cache_lgt)
			{
				this.cache_lgt = lgt;
				this.need_recheck_bcc_cache = true;
			}
			if (recheck_cache_execute)
			{
				this.recheckCache();
			}
		}

		public uint footable_bits
		{
			get
			{
				return this.footable_bits_;
			}
			set
			{
				if (this.footable_bits == value)
				{
					return;
				}
				this.footable_bits_ = value;
				if (this.CurBCC != null && !this.CurBCC.isUseableDir(this.footable_bits_))
				{
					this.initJump(false, false, false);
				}
			}
		}

		public int bcc_xd
		{
			get
			{
				if (this.CurBCC != null)
				{
					return CAim._XD(this.CurBCC.aim, 1);
				}
				return 0;
			}
		}

		public int bcc_yd
		{
			get
			{
				if (this.CurBCC != null)
				{
					return CAim._YD(this.CurBCC.aim, 1);
				}
				return -1;
			}
		}

		public float bcc_agR
		{
			get
			{
				if (this.CurBCC != null)
				{
					return CAim.get_agR(this.CurBCC.aim, 0f);
				}
				return -1.5707964f;
			}
		}

		public bool is_on_web
		{
			get
			{
				return this.hasFoot() && this.foottype == "web";
			}
		}

		public void addOnIce()
		{
			if (this.hasFoot())
			{
				this.Phy.addOnIce();
			}
		}

		public List<M2BlockColliderContainer.BCCLine> getCachedBccVector()
		{
			this.recheckCache();
			return this.ABccl;
		}

		public IFootable get_Foot()
		{
			return this.Cur;
		}

		public M2BlockColliderContainer.BCCLine get_FootBCC()
		{
			return this.CurBCC;
		}

		public M2BlockColliderContainer.BCCLine get_LastBCC()
		{
			return this.LastBCC;
		}

		public bool hasFootHard(float f)
		{
			return this.t_foot >= f;
		}

		public bool bccAimHas(AIM a)
		{
			if (this.CurBCC == null)
			{
				return false;
			}
			int num = CAim._XD(a, 1);
			int num2 = CAim._YD(a, 1);
			return (num == 0 || num == CAim._XD(this.CurBCC.aim, 1)) && (num2 == 0 || num2 == CAim._YD(this.CurBCC.aim, 1));
		}

		public void applyVelocity(FOCTYPE type, float velocity_x, float velocity_y)
		{
			if (velocity_x != 0f)
			{
				this.Phy.addFoc(type | FOCTYPE._RELEASE, velocity_x, 0f, -2f, 0, 3, 0, -1, 0);
			}
			if (velocity_y < 0f && this.Phy.base_gravity > 0f)
			{
				this.Phy.addFoc(type | FOCTYPE._GRAVITY_LOCK, 0f, velocity_y, -2f, 0, 2, X.IntC(-velocity_y / M2Phys.getGravityApplyVelocity(this.Mp, this.Phy.base_gravity, 1f)), -1, 0);
				return;
			}
			if (velocity_y != 0f)
			{
				this.Phy.addFoc(type | FOCTYPE._RELEASE, 0f, velocity_y, -2f, 0, 3, 0, -1, 0);
			}
		}

		public bool FootIs(IFootable F)
		{
			return this.Cur == F;
		}

		public bool FootIsLift()
		{
			return this.CurBCC != null && this.CurBCC.is_lift && !this.CurBCC.is_ladder;
		}

		public bool FootIsLadder()
		{
			return this.CurBCC != null && this.CurBCC.is_ladder;
		}

		public bool FootTargetIsBcc(M2BlockColliderContainer _BCC)
		{
			return this.CurBCC != null && this.CurBCC.BCC == _BCC;
		}

		public bool canJump()
		{
			return this.hasFoot() || (this.t_foot > -7f && this.vague_foot);
		}

		public bool canStartRunning()
		{
			return this.t_foot > -7f;
		}

		public bool hasFoot()
		{
			return this.t_foot >= 0f;
		}

		public float mleft
		{
			get
			{
				return this.Mv.mleft;
			}
		}

		public float mtop
		{
			get
			{
				return this.Mv.mtop;
			}
		}

		public float mright
		{
			get
			{
				return this.Mv.mright;
			}
		}

		public float mbottom
		{
			get
			{
				return this.Mv.mbottom;
			}
		}

		public float rgdleft
		{
			get
			{
				return this.Phy.rgdleft;
			}
		}

		public float rgdtop
		{
			get
			{
				return this.Phy.rgdtop;
			}
		}

		public float rgdright
		{
			get
			{
				return this.Phy.rgdright;
			}
		}

		public float rgdbottom
		{
			get
			{
				return this.Phy.rgdbottom;
			}
		}

		public float t_mleft
		{
			get
			{
				return this.Phy.tstacked_x - this.Mv.sizex;
			}
		}

		public float t_mtop
		{
			get
			{
				return this.Phy.tstacked_y - this.Mv.sizey;
			}
		}

		public float t_mright
		{
			get
			{
				return this.Phy.tstacked_x + this.Mv.sizex;
			}
		}

		public float t_mbottom
		{
			get
			{
				return this.Phy.tstacked_y + this.Mv.sizey;
			}
		}

		public float sizex
		{
			get
			{
				return this.Mv.sizex;
			}
		}

		public float sizey
		{
			get
			{
				return this.Mv.sizey;
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.Mv.Mp;
			}
		}

		public readonly M2Mover Mv;

		public readonly M2Phys Phy;

		private const int FOOT_INIT_TIME_MAX = 20;

		private const int FOOT_RELEASE_MARGIN = 7;

		public bool auto_search_ladder;

		private int t_footstamp = -20;

		private FOOTRES stamp_state;

		private float t_foot = -7f;

		private IFootable Cur;

		private M2BlockColliderContainer.BCCLine CurBCC;

		private M2BlockColliderContainer.BCCLine LastBCC;

		public M2BlockColliderContainer.BCCLine LadderBCC;

		private List<M2BlockColliderContainer.BCCLine> ABccl = new List<M2BlockColliderContainer.BCCLine>(8);

		private List<IBCCFootListener> ABccFoot = new List<IBCCFootListener>(8);

		private static List<IBCCFootListener> ABccFootBuffer;

		private static List<M2Puts> ABufCp;

		public bool auto_fix_to_foot = true;

		public ALIGN pivot = ALIGN.RIGHT;

		private float t_lock_foot_fix;

		private float pre_search_x;

		private float pre_search_y;

		private int cache_lgt;

		public bool need_recheck_current_pos = true;

		private int need_recheck_bcc_foot_;

		private const int FT_CHECK = 6;

		public bool need_recheck_bcc_cache = true;

		public bool need_recheck_foottype;

		public bool vague_foot;

		private uint footable_bits_ = 8U;

		public FOOTSTAMP footstamp_type = FOOTSTAMP.BARE;

		public string foottype;

		private static uint foot_fine_id;

		public bool no_change_shift_pixel;

		public float shift_pixel_x;

		public float shift_pixel_y;

		public float fix_mapx = -1000f;

		public float fix_mapy = -1000f;

		public bool recheck_side;

		private static HashP HashFoot = new HashP(8);

		public const string FOOTTYPE_ICE = "ice";

		public const string FOOTTYPE_WEB = "web";
	}
}

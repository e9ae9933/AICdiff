using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel.mgm.sneaking
{
	public class PrRunMGSneaking : ISpecialPrRunner
	{
		public Map2d Mp
		{
			get
			{
				return this.Con.Mp;
			}
		}

		public PrRunMGSneaking(M2LpSneakingMG _Con)
		{
			this.Con = _Con;
			this.nM2D = this.Con.nM2D;
			this.BoxNoteTop = new GameObject("NoteT").AddComponent<M2BoxOneLine>().Init(this.nM2D, false);
			this.BoxNoteBtm = new GameObject("NoteB").AddComponent<M2BoxOneLine>().Init(this.nM2D, false);
			this.BoxNoteLR = new GameObject("NoteLR").AddComponent<M2BoxOneLine>().Init(this.nM2D, false);
			this.FD_fnInjectStateForPr = new PrStateInjector.fnInitState(this.fnInjectStateForPr);
			this.PeBgmLower = new EffectHandlerPE(1);
			this.PeBgmLower.Set(PostEffect.IT.setPE(POSTM.BGM_LOWER, 30f, 0f, 0));
		}

		public PrRunMGSneaking activate()
		{
			this.BoxNoteTop.deactivate();
			this.BoxNoteBtm.deactivate();
			this.BoxNoteLR.deactivate();
			PR pr = this.Mp.Pr as PR;
			this.PrPreFoot = null;
			pr.initSpRunner(this);
			this.EfPrGrd = this.Mp.getEffectTop().setEffectWithSpecificFn("sneaking_top_grd", this.Mp.Pr.x, this.Mp.Pr.y, -1f, 0, 0, new FnEffectRun(this.fnDrawSneakingPrGradation));
			this.SpPrRefine(pr, false);
			this.TEfDarken = pr.TeCon.setEventDarken(0f, 0, 360f);
			pr.getFootManager().footstamp_type = FOOTSTAMP.HEEL;
			return this;
		}

		public void initAware()
		{
			if (this.EfPrGrd != null)
			{
				this.EfPrGrd.z = X.Mn(-1f + this.EfPrGrd.z, -0.01f);
				this.EfPrGrd = null;
			}
			if (this.TEfDarken != null)
			{
				this.TEfDarken.destruct();
				this.TEfDarken = null;
			}
		}

		public void removeEffect()
		{
			if (this.EfPrGrd != null)
			{
				this.EfPrGrd.destruct();
				this.EfPrGrd = null;
			}
			if (this.TEfDarken != null)
			{
				this.TEfDarken.destruct();
				this.TEfDarken = null;
			}
		}

		public void destruct()
		{
			this.removeEffect();
			this.AttachPr = null;
			this.PeBgmLower.release(false);
			BGM.remHalfFlag("SNEAK_MG");
			if (this.BoxNoteTop != null)
			{
				this.BoxNoteTop.destruct();
				IN.DestroyE(this.BoxNoteTop.gameObject);
				this.BoxNoteBtm.destruct();
				IN.DestroyE(this.BoxNoteBtm.gameObject);
				this.BoxNoteLR.destruct();
				IN.DestroyE(this.BoxNoteLR.gameObject);
				this.BoxNoteTop = (this.BoxNoteBtm = (this.BoxNoteLR = null));
			}
		}

		public void deactivateUI()
		{
			if (this.BoxNoteTop != null)
			{
				this.BoxNoteTop.deactivate();
				this.BoxNoteBtm.deactivate();
				this.BoxNoteLR.deactivate();
			}
		}

		private PR AttachPr
		{
			get
			{
				return this.AttachPr_;
			}
			set
			{
				if (this.AttachPr_ == value)
				{
					return;
				}
				if (this.AttachPr_ != null && this.AttachPr_.SttInjector != null)
				{
					this.AttachPr_.SttInjector.Rem(PR.STATE.SP_RUN, this.FD_fnInjectStateForPr);
				}
				this.AttachPr_ = value;
				if (this.AttachPr_ != null && this.AttachPr_.SttInjector != null)
				{
					this.AttachPr_.SttInjector.Add(PR.STATE.SP_RUN, 1, 2, this.FD_fnInjectStateForPr);
				}
			}
		}

		private bool fnInjectStateForPr(PR.STATE state, ref float t_state, ref bool execute_change_state, ref PR.STATE target_state)
		{
			if (state == PR.STATE.NORMAL && this.AttachPr_ != null)
			{
				execute_change_state = false;
				this.AttachPr_.initSpRunner(this);
				return true;
			}
			return false;
		}

		internal void SpPrRefine(PR Pr, bool fine_ef_z = false)
		{
			M2LpSneakingMG.LigArea prLigTarget = this.PrLigTarget;
			this.PrLigTarget = null;
			M2LabelPoint prStairTop = this.PrStairTop;
			this.PrStairTop = null;
			M2LabelPoint prStairBtm = this.PrStairBtm;
			this.PrStairBtm = null;
			this.AttachPr = Pr;
			M2BlockColliderContainer.BCCLine footBCC = Pr.getFootManager().get_FootBCC();
			if (footBCC == null || !footBCC.is_naname)
			{
				foreach (KeyValuePair<int, M2LpSneakingMG.LigAreaContainer> keyValuePair in this.Con.OAWalkArea)
				{
					M2LpSneakingMG.LigAreaContainer value = keyValuePair.Value;
					if (X.BTW((float)(keyValuePair.Key - 5), Pr.y, (float)(keyValuePair.Key + 1)))
					{
						int count = value.Count;
						for (int i = 0; i < count; i++)
						{
							M2LpSneakingMG.LigArea ligArea = value[i];
							if ((i == 0) ? (Pr.x < ligArea.right) : ((i == count - 1) ? (Pr.x >= ligArea.x) : X.BTW(ligArea.x, Pr.x, ligArea.right)))
							{
								this.PrLigTarget = ligArea;
								break;
							}
						}
					}
					if (this.PrLigTarget != null)
					{
						break;
					}
				}
			}
			if (this.PrLigTarget != prLigTarget || fine_ef_z)
			{
				if (this.PrLigTarget == null || this.PrLigTarget is M2LpSneakingMG.BrightArea)
				{
					Pr.remD(M2MoverPr.DECL.CANNOT_EXECUTE_CHECKTARGET);
				}
				else
				{
					Pr.addD(M2MoverPr.DECL.CANNOT_EXECUTE_CHECKTARGET);
				}
				if (this.EfPrGrd != null)
				{
					if (this.PrLigTarget is M2LpSneakingMG.DarkArea)
					{
						if (this.EfPrGrd.z < 0f)
						{
							this.EfPrGrd.z = X.Mx(1f + this.EfPrGrd.z, 0f);
							BGM.addHalfFlag("SNEAK_MG");
						}
					}
					else if (this.EfPrGrd.z >= 0f)
					{
						this.EfPrGrd.z = X.Mn(-1f + this.EfPrGrd.z, -0.01f);
						BGM.remHalfFlag("SNEAK_MG");
					}
				}
			}
			bool flag = this.PrPreFoot != footBCC || fine_ef_z;
			if (footBCC != null)
			{
				float shifted_bottom = footBCC.shifted_bottom;
				float num = X.Mn(Pr.mbottom, shifted_bottom - 0.01f);
				List<M2LabelPoint> astair = this.Con.AStair;
				int num2 = astair.Count - 1;
				while (num2 >= 0 && (this.PrStairBtm == null || this.PrStairTop == null))
				{
					M2LabelPoint m2LabelPoint = astair[num2];
					if (m2LabelPoint.isContainingMapXy(Pr.x, num, Pr.x, num, 0f) && (float)m2LabelPoint.mapy < shifted_bottom && shifted_bottom <= (float)(m2LabelPoint.mapy + m2LabelPoint.maph))
					{
						if (TX.headerIs(m2LabelPoint.key, "StairB", 0, '_', true))
						{
							this.PrStairBtm = m2LabelPoint;
						}
						else
						{
							this.PrStairTop = m2LabelPoint;
						}
					}
					num2--;
				}
			}
			if (this.PrStairBtm != prStairBtm || flag)
			{
				if (this.PrStairBtm != null)
				{
					this.BoxNoteBtm.setPos(this.PrStairBtm.mapfocx, this.note_btm_pos_y(Pr, this.PrStairBtm), 160f, -30f);
					this.BoxNoteBtm.make(TX.Get("sneaking_MG_KD_btm", ""), true);
					if ((Pr.isJumpO(0) || Pr.isBO(0)) && this.PrStairBtm != prStairBtm)
					{
						this.lock_stair_btm_input = true;
					}
				}
				else
				{
					this.BoxNoteBtm.deactivate();
				}
			}
			if (this.PrStairTop != prStairTop || flag)
			{
				if (this.PrStairTop != null)
				{
					this.BoxNoteTop.setPos(this.PrStairTop.mapfocx, this.note_top_pos_y(Pr, this.PrStairTop), 160f, -30f);
					this.BoxNoteTop.make(TX.Get("sneaking_MG_KD_top", ""), true);
					if ((Pr.isJumpO(0) || Pr.isTO(0)) && this.PrStairTop != prStairTop)
					{
						this.lock_stair_top_input = true;
					}
				}
				else
				{
					this.BoxNoteTop.deactivate();
				}
			}
			if (this.PrPreFoot != footBCC)
			{
				this.PrPreFoot = footBCC;
				if (this.BoxNoteBtm.visible && this.PrStairBtm != null)
				{
					this.BoxNoteBtm.setPos(this.PrStairBtm.mapfocx, this.note_btm_pos_y(Pr, this.PrStairBtm), 160f, -30f);
				}
				if (this.BoxNoteTop.visible && this.PrStairTop != null)
				{
					this.BoxNoteTop.setPos(this.PrStairTop.mapfocx, this.note_top_pos_y(Pr, this.PrStairTop), 160f, -30f);
				}
			}
		}

		public float note_top_pos_y(PR Pr, M2LabelPoint StairLp)
		{
			M2BlockColliderContainer.BCCLine lastBCC = Pr.getFootManager().get_LastBCC();
			float num = (float)this.Mp.rows;
			if (lastBCC != null)
			{
				num = X.Mn(num, lastBCC.shifted_bottom);
			}
			if (StairLp != null)
			{
				num = X.Mn(num, StairLp.bottom * this.Mp.rCLEN);
			}
			return num;
		}

		public float note_btm_pos_y(PR Pr, M2LabelPoint StairLp)
		{
			M2BlockColliderContainer.BCCLine lastBCC = Pr.getFootManager().get_LastBCC();
			float num = (float)this.Mp.rows;
			if (lastBCC != null)
			{
				num = X.Mn(num, lastBCC.shifted_bottom);
			}
			if (StairLp != null)
			{
				num = X.Mn(num, StairLp.bottom * this.Mp.rCLEN);
			}
			return num;
		}

		bool ISpecialPrRunner.runPreSPPR(PR Pr, float fcnt, ref float t_state)
		{
			if (this.Con.isAware() || this.Con.isDestructed())
			{
				return false;
			}
			this.PeBgmLower.fine(100);
			if (this.Mp.Pr != Pr)
			{
				return true;
			}
			Pr.addD(M2MoverPr.DECL.NO_USE_ITEM);
			if (this.EfPrGrd != null)
			{
				M2Mover m2Mover = this.nM2D.Cam.getBaseMover() ?? Pr;
				this.EfPrGrd.x = m2Mover.x;
				this.EfPrGrd.y = m2Mover.y;
				if (this.EfPrGrd.z >= 0f)
				{
					this.EfPrGrd.z = X.VALWALK(this.EfPrGrd.z, 1f, 0.02f * fcnt);
					this.PeBgmLower.setXLevel(POSTM.BGM_LOWER, this.EfPrGrd.z * 0.66f);
				}
				else
				{
					this.EfPrGrd.z = X.VALWALK(this.EfPrGrd.z, -1f, 0.032f * fcnt);
					this.PeBgmLower.setXLevel(POSTM.BGM_LOWER, (1f + this.EfPrGrd.z) * 0.66f);
				}
				if (this.TEfDarken != null)
				{
					this.TEfDarken.x = ((this.EfPrGrd.z >= 0f) ? this.EfPrGrd.z : (1f + this.EfPrGrd.z)) * 0.66f;
					this.TEfDarken.af = 0f;
				}
			}
			if (!Map2d.can_handle)
			{
				return true;
			}
			M2MoverPr.MOVEK movek = Pr.getMoveKey() & (M2MoverPr.MOVEK)10;
			bool flag = true;
			bool flag2 = false;
			bool flag3 = false;
			M2BlockColliderContainer.BCCLine footBCC = Pr.getFootManager().get_FootBCC();
			M2BlockColliderContainer.BCCLine bccline = footBCC;
			if (footBCC != null && (this.PrStairTop != null || this.PrStairBtm != null))
			{
				bool flag4 = false;
				if (Pr.isJumpO(0) || Pr.isTO(0))
				{
					if (!this.lock_stair_top_input)
					{
						flag4 = true;
						if (this.PrStairTop != null)
						{
							flag = false;
							flag2 = true;
						}
						if (footBCC.is_naname)
						{
							movek = ((footBCC.aim == AIM.BL) ? M2MoverPr.MOVEK.KEYD_L : M2MoverPr.MOVEK.KEYD_R);
						}
						else
						{
							this.checkConnectStair(Pr, this.PrStairTop, true, ref movek, true, true);
						}
					}
				}
				else
				{
					this.lock_stair_top_input = false;
				}
				if (Pr.isBO(0))
				{
					if (!flag4 && !this.lock_stair_btm_input)
					{
						if (this.PrStairBtm != null)
						{
							flag = false;
							flag3 = true;
						}
						if (footBCC.is_naname)
						{
							movek = ((footBCC.aim == AIM.BL) ? M2MoverPr.MOVEK.KEYD_R : M2MoverPr.MOVEK.KEYD_L);
						}
						else
						{
							this.checkConnectStair(Pr, this.PrStairBtm, false, ref movek, true, true);
						}
					}
				}
				else
				{
					this.lock_stair_btm_input = false;
				}
			}
			else
			{
				this.lock_stair_top_input = false;
				this.lock_stair_btm_input = false;
			}
			this.BoxNoteBtm.hilighted = flag3;
			this.BoxNoteTop.hilighted = flag2;
			if (flag)
			{
				if ((movek & M2MoverPr.MOVEK.KEYD_R) != (M2MoverPr.MOVEK)0 && this.PrLigTarget is M2LpSneakingMG.DarkArea && this.PrLigTarget.isOnBorder(Pr, AIM.R) && !Pr.isRP(8))
				{
					Pr.setAim(AIM.R, false);
					movek &= (M2MoverPr.MOVEK)(-3);
				}
				if ((movek & M2MoverPr.MOVEK.KEYD_L) != (M2MoverPr.MOVEK)0 && this.PrLigTarget is M2LpSneakingMG.DarkArea && this.PrLigTarget.isOnBorder(Pr, AIM.L) && !Pr.isLP(8))
				{
					Pr.setAim(AIM.L, false);
					movek &= (M2MoverPr.MOVEK)(-9);
				}
			}
			if (movek == (M2MoverPr.MOVEK)10)
			{
				movek = (M2MoverPr.MOVEK)0;
			}
			NoelAnimator animator = Pr.getAnimator();
			if (movek == (M2MoverPr.MOVEK)0)
			{
				if (this.PrLigTarget is M2LpSneakingMG.DarkArea && this.PrLigTarget.isOnBorder(Pr, Pr.aim))
				{
					animator.timescale = 1f;
					Pr.setDefaultPose("stealth_hide");
					if (!this.BoxNoteLR.isActive())
					{
						this.BoxNoteLR.setPos((Pr.aim == AIM.R) ? this.PrLigTarget.right : this.PrLigTarget.x, this.PrLigTarget.bottom, 160f, -30f);
						this.BoxNoteLR.make((Pr.aim == AIM.R) ? TX.Get("sneaking_MG_KD_goout_right", "") : TX.Get("sneaking_MG_KD_goout_left", ""), true);
					}
				}
				else
				{
					if (this.BoxNoteLR.isActive())
					{
						this.BoxNoteLR.deactivate();
					}
					if (!animator.poseIs("stealth_walk", true))
					{
						Pr.setDefaultPose("stealth_walk");
					}
					if (animator.cframe != 0 && animator.cframe != 4)
					{
						animator.animReset((animator.cframe >= 4) ? 0 : 4);
					}
					animator.timescale = 0f;
				}
				t_state = 0f;
			}
			else
			{
				animator.timescale = 1f;
				if (movek == M2MoverPr.MOVEK.KEYD_R)
				{
					Pr.setAim(AIM.R, false);
				}
				else if (movek == M2MoverPr.MOVEK.KEYD_L)
				{
					Pr.setAim(AIM.L, false);
				}
				Pr.getPhysic().walk_xspeed = 0.07f * Pr.mpf_is_right;
				Pr.setDefaultPose("stealth_walk");
				if (t_state >= 8f && bccline != null && !bccline.is_naname)
				{
					t_state = 0f;
					this.SpPrRefine(Pr, false);
				}
				if ((!(this.PrLigTarget is M2LpSneakingMG.DarkArea) || !this.PrLigTarget.isOnBorder(Pr, Pr.aim)) && this.BoxNoteLR.isActive())
				{
					this.BoxNoteLR.deactivate();
				}
			}
			return true;
		}

		private void checkConnectStair(PR Pr, M2LabelPoint LpStair, bool to_top, ref M2MoverPr.MOVEK mvd, bool check_left = true, bool check_right = true)
		{
			if (LpStair == null)
			{
				return;
			}
			M2BlockColliderContainer.BCCLine footBCC = Pr.getFootManager().get_FootBCC();
			if (X.Abs(Pr.x - LpStair.mapfocx) < 0.75f)
			{
				using (BList<M2BlockColliderContainer.BCCHitInfo> blist = ListBuffer<M2BlockColliderContainer.BCCHitInfo>.Pop(0))
				{
					int num = 0;
					while (num < 3 && blist.Count == 0)
					{
						M2BlockColliderContainer.BCCLine bccline = ((num == 0) ? footBCC : ((num == 1) ? footBCC.SideL : footBCC.SideR));
						if (bccline != null)
						{
							footBCC.BCC.getConnectedBcc(bccline, LpStair.mapfocx, 0.75f, blist, true, true, false, check_left, check_right);
							for (int i = blist.Count - 1; i >= 0; i--)
							{
								if (!blist[i].Hit.is_naname)
								{
									blist.RemoveAt(i);
								}
							}
						}
						num++;
					}
					M2BlockColliderContainer.BCCLine bccline2 = ((blist.Count > 0) ? blist[0].Hit : null);
					if (bccline2 == null || !bccline2.is_naname)
					{
						mvd = ((Pr.x > LpStair.mapfocx) ? M2MoverPr.MOVEK.KEYD_L : M2MoverPr.MOVEK.KEYD_R);
						return;
					}
					float num2 = ((bccline2.aim == AIM.BL == to_top) ? bccline2.shifted_right : bccline2.shifted_x);
					if (X.Abs(Pr.x - num2) < 0.15f)
					{
						Pr.getFootManager().rideInitTo(bccline2, false);
						return;
					}
					mvd = ((Pr.x > num2) ? M2MoverPr.MOVEK.KEYD_L : M2MoverPr.MOVEK.KEYD_R);
					return;
				}
			}
			mvd = ((Pr.x > LpStair.mapfocx) ? M2MoverPr.MOVEK.KEYD_L : M2MoverPr.MOVEK.KEYD_R);
		}

		void ISpecialPrRunner.quitSPPR(PR Pr, PR.STATE aftstate)
		{
			Pr.getAnimator().timescale = 1f;
			Pr.fineFootStampType();
		}

		internal void checkBrightArea(M2LpSneakingMG.BrightArea Area)
		{
			if (Area == this.PrLigTarget && this.EfPrGrd != null)
			{
				Area.ScrBright(this.Mp, ((this.EfPrGrd.z >= 0f) ? this.EfPrGrd.z : (1f + this.EfPrGrd.z)) * 0.28f);
			}
		}

		private bool fnDrawSneakingPrGradation(EffectItem E)
		{
			if (this.Con.isDestructed())
			{
				this.EfPrGrd = null;
				return false;
			}
			float num = ((E.z >= 0f) ? E.z : (1f + E.z)) * X.ZLINE(E.af, 40f);
			if (num > 0f)
			{
				MeshDrawer mesh = E.GetMesh("lg_grd_top", 4278190080U, BLEND.NORMAL, false);
				Vector3 posMainTransform = this.nM2D.Cam.PosMainTransform;
				mesh.base_x = posMainTransform.x;
				mesh.base_y = posMainTransform.y;
				mesh.base_z -= 1f;
				mesh.Col = mesh.ColGrd.Set(4278190080U).mulA(num).C;
				mesh.ColGrd.setA(0f);
				float h = IN.h;
				float num2 = (E.x * this.Mp.CLEN - this.nM2D.Cam.x) * this.Mp.base_scale;
				float num3 = -(E.y * this.Mp.CLEN - this.nM2D.Cam.y) * this.Mp.base_scale;
				mesh.RectDoughnut(num2, num3, UIBase.gamew, IN.h, num2, num3, UIBase.gamew * 0.45f, IN.h * 0.45f, false, 0f, 1f, false);
				mesh.RectDoughnut(num2, num3, UIBase.gamew, IN.h, num2, num3, UIBase.gamew * 0.15f, IN.h * 0.15f, false, 0f, 1f, false);
				mesh.RectDoughnut(num2, num3, UIBase.gamew * 2f, IN.h * 2f, num2, num3, UIBase.gamew, IN.h, false, 0f, 0f, false);
			}
			else if (E != this.EfPrGrd)
			{
				return false;
			}
			return true;
		}

		internal void EvtOpen()
		{
			if (this.EfPrGrd != null && this.EfPrGrd.z >= 0f)
			{
				this.EfPrGrd.z = X.Mn(-1f + this.EfPrGrd.z, -0.01f);
			}
			this.deactivateUI();
		}

		public bool isEffectActive()
		{
			return this.EfPrGrd != null;
		}

		public readonly M2LpSneakingMG Con;

		public readonly NelM2DBase nM2D;

		private EffectItem EfPrGrd;

		private M2BoxOneLine BoxNoteTop;

		private M2BoxOneLine BoxNoteBtm;

		private M2BoxOneLine BoxNoteLR;

		private TransEffecterItem TEfDarken;

		private PR AttachPr_;

		private readonly PrStateInjector.fnInitState FD_fnInjectStateForPr;

		private const float pr_walk_speed = 0.07f;

		internal const string lp_stair_area = "Stair";

		internal const string lp_stair_area_btm = "StairB";

		private readonly EffectHandlerPE PeBgmLower;

		private M2LpSneakingMG.LigArea PrLigTarget;

		private M2BlockColliderContainer.BCCLine PrPreFoot;

		private M2LabelPoint PrStairTop;

		private M2LabelPoint PrStairBtm;

		private bool lock_stair_top_input;

		private bool lock_stair_btm_input;

		private const float box_note_shift_top = 160f;

		private const float box_note_shift_btm = -30f;
	}
}

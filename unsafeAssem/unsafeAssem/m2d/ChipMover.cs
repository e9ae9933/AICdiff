using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public class ChipMover : M2Mover, IPresserBehaviour, ITeShift, ITeColor
	{
		public bool attractChips(ChipMover.FnAttractChip FnAttract, M2MapLayer LayOnly = null, DRect Rc = null)
		{
			List<M2Puts> list = new List<M2Puts>();
			int count_layers = this.Mp.count_layers;
			if (Rc != null)
			{
				int num = (int)(Rc.x / base.CLEN);
				int num2 = (int)(Rc.y / base.CLEN);
				int num3 = X.IntC(Rc.right / base.CLEN);
				int num4 = X.IntC(Rc.bottom / base.CLEN);
				for (int i = num; i < num3; i++)
				{
					for (int j = num2; j < num4; j++)
					{
						M2Pt pointPuts = this.Mp.getPointPuts(i, j, false, false);
						if (pointPuts != null && pointPuts.count != 0)
						{
							int count = pointPuts.count;
							for (int k = 0; k < count; k++)
							{
								M2Puts m2Puts = pointPuts[k];
								if ((LayOnly == null || LayOnly == m2Puts.Lay) && list.IndexOf(m2Puts) < 0 && FnAttract(m2Puts, list))
								{
									list.Add(m2Puts);
								}
							}
						}
					}
				}
			}
			else
			{
				for (int l = ((LayOnly != null) ? (-1) : 0); l < count_layers; l++)
				{
					M2MapLayer m2MapLayer = ((l == -1) ? LayOnly : this.Mp.getLayer(l));
					if (l < 0 || m2MapLayer != LayOnly)
					{
						int count_chips = m2MapLayer.count_chips;
						for (int m = 0; m < count_chips; m++)
						{
							M2Puts chipByIndex = m2MapLayer.getChipByIndex(m);
							if (list.IndexOf(chipByIndex) < 0 && FnAttract(chipByIndex, list))
							{
								list.Add(chipByIndex);
							}
						}
					}
				}
			}
			return this.attractChips(list, false);
		}

		public bool attractChips(List<M2Puts> APt, bool centerize = false)
		{
			int count = APt.Count;
			if (count == 0)
			{
				return false;
			}
			if (this.APuts == null)
			{
				this.APuts = new ChipMover.ExMapChip[count];
				this.BoundsRc = new DRect("_");
			}
			for (int i = 0; i < count; i++)
			{
				M2Puts m2Puts = APt[i];
				m2Puts.AttachCM = this;
				if (!this.do_not_bind_BCC_)
				{
					m2Puts.addActiveRemoveKey(base.key, true);
				}
				M2Chip m2Chip = m2Puts as M2Chip;
				if (m2Chip != null)
				{
					this.BoundsRc.Expand((float)m2Chip.mapx, (float)m2Chip.mapy, (float)m2Chip.clms, (float)m2Chip.rows, false);
				}
				this.APuts[i] = new ChipMover.ExMapChip
				{
					Pt = m2Puts,
					drawx0 = m2Puts.drawx,
					drawy0 = m2Puts.drawy
				};
			}
			if (this.Mp == null)
			{
				APt[0].Mp.assignMover(this, false);
			}
			if (!this.BoundsRc.isEmpty())
			{
				M2MvColliderCreatorCM m2MvColliderCreatorCM = this.CC as M2MvColliderCreatorCM;
				if (m2MvColliderCreatorCM == null)
				{
					m2MvColliderCreatorCM = new M2MvColliderCreatorCM(this, this.APuts, false);
					this.CC = m2MvColliderCreatorCM;
				}
				m2MvColliderCreatorCM.FnCanStand = this.FnColliderBlockCanStand;
				m2MvColliderCreatorCM.do_not_bind_BCC = this.do_not_bind_BCC_;
				base.gameObject.isStatic = false;
				if (centerize)
				{
					this.setTo(this.BoundsRc.cx, this.BoundsRc.cy);
					this.drawx0 = this.drawx;
					this.drawy0 = this.drawy;
					base.Size(this.BoundsRc.width * base.CLEN, this.BoundsRc.height * base.CLEN, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				}
				else
				{
					base.Size(X.Mx(base.x - this.BoundsRc.left, this.BoundsRc.right - base.x) * base.CLENM, X.Mx(base.y - this.BoundsRc.top, this.BoundsRc.bottom - base.y) * base.CLENM, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				}
				base.carryable_other_object = true;
				this.Mp.considerConfig4((int)this.BoundsRc.left, (int)this.BoundsRc.top, X.IntC(this.BoundsRc.right), X.IntC(this.BoundsRc.bottom));
				this.Mp.need_update_collider = true;
				if (this.CC != null && this.CC.need_recreate)
				{
					this.CC.fineRecreate();
				}
			}
			else
			{
				this.floating = true;
				base.Size(1f, 1f, ALIGN.CENTER, ALIGNY.MIDDLE, false);
			}
			return true;
		}

		public override void appear(Map2d Mp)
		{
			base.base_gravity = 0f;
			base.appear(Mp);
			base.gameObject.layer = base.M2D.map_object_layer;
			Mp.setTag(base.gameObject, "Block");
			base.fineDrawPosition();
			this.drawx0 = this.drawx;
			this.drawy0 = this.drawy;
			if (this.TeCon != null)
			{
				this.TeCon.layer_effect_bottom = ((Mp.Dgn != null) ? Mp.Dgn.effect_layer_bottom : base.gameObject.layer);
				this.TeCon.layer_effect_top = ((Mp.Dgn != null) ? Mp.Dgn.effect_layer_top : base.gameObject.layer);
			}
		}

		public TransEffecter initTransEffecter()
		{
			this.TeCon = new TransEffecter("M2m-" + this.ToString(), null, 10, 0, 0, EFCON_TYPE.NORMAL);
			if (!this.deny_coloring_tecon_)
			{
				this.TeCon.RegisterCol(this, false);
			}
			this.TeCon.RegisterPos(this);
			if (this.Mp != null)
			{
				this.TeCon.layer_effect_bottom = ((this.Mp.Dgn != null) ? this.Mp.Dgn.effect_layer_bottom : base.gameObject.layer);
				this.TeCon.layer_effect_top = ((this.Mp.Dgn != null) ? this.Mp.Dgn.effect_layer_top : base.gameObject.layer);
			}
			return this.TeCon;
		}

		public Color32 Col
		{
			get
			{
				if (this.AddCol == null)
				{
					return C32.d2c(4286545791U);
				}
				return this.AddCol.C;
			}
			set
			{
				if (value.Equals(C32.d2c(4286545791U)) && this.TeCon == null)
				{
					this.AddCol = null;
					this.pos_reset = true;
					return;
				}
				if (this.AddCol == null)
				{
					this.AddCol = new C32();
				}
				this.AddCol.Set(value);
				this.pos_reset = true;
			}
		}

		public bool deny_coloring_tecon
		{
			get
			{
				return this.deny_coloring_tecon_;
			}
			set
			{
				if (this.deny_coloring_tecon == value)
				{
					return;
				}
				this.deny_coloring_tecon_ = value;
				if (this.TeCon == null)
				{
					return;
				}
				if (value)
				{
					this.TeCon.UnregisterCol(this, false);
					return;
				}
				this.TeCon.RegisterCol(this, false);
			}
		}

		public bool disappearing
		{
			get
			{
				return this.disappearing_;
			}
			set
			{
				if (this.disappearing == value)
				{
					return;
				}
				this.disappearing_ = value;
				if (this.CC != null)
				{
					if (this.disappearing_collider_trigger)
					{
						this.CC.Cld.isTrigger = value;
					}
					else
					{
						this.CC.Cld.enabled = !value;
					}
				}
				this.pos_reset = true;
				if (value)
				{
					this.deny_coloring_tecon = true;
					if (this.do_not_bind_BCC_)
					{
						if (this.APuts == null)
						{
							return;
						}
						int num = this.APuts.Length;
						for (int i = 0; i < num; i++)
						{
							this.APuts[i].Pt.addActiveRemoveKey(base.key, false);
						}
					}
					else
					{
						base.carryable_other_object = false;
					}
					this.Col = C32.d2c(8355711U);
				}
				else
				{
					this.deny_coloring_tecon = false;
					if (this.do_not_bind_BCC_)
					{
						if (this.APuts == null)
						{
							return;
						}
						int num2 = this.APuts.Length;
						for (int j = 0; j < num2; j++)
						{
							this.APuts[j].Pt.remActiveRemoveKey(base.key, false);
						}
					}
					else
					{
						base.carryable_other_object = !this.floating;
					}
					this.Col = C32.d2c(4286545791U);
				}
				if (this.do_not_bind_BCC_)
				{
					this.reconsiderConfig();
				}
			}
		}

		public bool do_not_bind_BCC
		{
			get
			{
				return this.do_not_bind_BCC_;
			}
			set
			{
				if (this.do_not_bind_BCC_ == value)
				{
					return;
				}
				this.do_not_bind_BCC_ = value;
				M2MvColliderCreatorCM m2MvColliderCreatorCM = this.CC as M2MvColliderCreatorCM;
				if (m2MvColliderCreatorCM == null || this.Mp == null)
				{
					return;
				}
				m2MvColliderCreatorCM.do_not_bind_BCC = this.do_not_bind_BCC_;
			}
		}

		public Vector2 getTranslatedMp()
		{
			float num = (this.drawx - this.drawx0) * this.Mp.rCLEN;
			float num2 = (this.drawy - this.drawy0) * this.Mp.rCLEN;
			return new Vector2(num, num2);
		}

		public List<M2BlockColliderContainer.BCCLine> getPressableBccLineNear(float mapx, float mapy, float sizex, float sizey, AIM a, List<M2BlockColliderContainer.BCCLine> ARet)
		{
			if (this.CC is M2MvColliderCreatorCM)
			{
				(this.CC as M2MvColliderCreatorCM).BCC.getNear(mapx, mapy, sizex, sizey, (int)a, ARet, false, true, 1f);
			}
			return ARet;
		}

		public Vector2 getTranslatedDelta()
		{
			if (this.Phy == null)
			{
				return Vector2.zero;
			}
			return new Vector2(base.x - this.Phy.prex, base.y - this.Phy.prey);
		}

		public ChipMover stabilize()
		{
			base.fineDrawPosition();
			float drawx = this.drawx;
			float drawy = this.drawy;
			float num = this.drawx0 / base.CLEN;
			float num2 = this.drawy0 / base.CLEN;
			int num3 = X.IntR(base.x - X.frac(num));
			int num4 = X.IntR(base.y - X.frac(num2));
			int num5 = num3 - (int)num;
			int num6 = num4 - (int)num2;
			int num7;
			if (num5 != 0 || num6 != 0)
			{
				num7 = this.APuts.Length;
				float num8 = ((float)num5 * base.CLEN + this.shift_x_) / 64f;
				float num9 = -((float)num6 * base.CLEN - this.shift_y_) / 64f;
				for (int i = 0; i < num7; i++)
				{
					ChipMover.ExMapChip exMapChip = this.APuts[i];
					exMapChip.Pt.translateByChipMover(num8, num9, this.AddCol, exMapChip.drawx0, exMapChip.drawy0, num5, num6, true);
				}
			}
			if (this.chips_reinputted)
			{
				return this;
			}
			this.chips_reinputted = true;
			num7 = this.APuts.Length;
			for (int j = 0; j < num7; j++)
			{
				this.APuts[j].Pt.remActiveRemoveKey(base.key, false);
			}
			if (base.carryable_other_object)
			{
				if (this.CC is M2MvColliderCreatorCM)
				{
					this.Mp.recheckFootObject((this.CC as M2MvColliderCreatorCM).BCC, null);
				}
				this.reconsiderConfig();
			}
			return this;
		}

		public ChipMover unstabilize(bool fine_mappos_immediately = false)
		{
			if (this.chips_reinputted)
			{
				this.chips_reinputted = false;
				int num = this.APuts.Length;
				for (int i = 0; i < num; i++)
				{
					ChipMover.ExMapChip exMapChip = this.APuts[i];
					exMapChip.Pt.addActiveRemoveKey(base.key, true);
					exMapChip.Pt.translateByChipMover(0f, 0f, this.AddCol, exMapChip.drawx0, exMapChip.drawy0, 0, 0, true);
				}
				if (fine_mappos_immediately)
				{
					this.resetChipsDrawPosition(0f, 0f);
				}
				if (base.carryable_other_object)
				{
					this.reconsiderConfig();
					if (this.CC is M2MvColliderCreatorCM)
					{
						this.Mp.recheckFootObject((this.CC as M2MvColliderCreatorCM).BCC, null);
					}
				}
			}
			return this;
		}

		private void reconsiderConfig()
		{
			int num = X.IntR((this.drawx - this.drawx0) / base.CLEN);
			int num2 = X.IntR((this.drawy - this.drawy0) / base.CLEN);
			this.Mp.considerConfig4((int)this.BoundsRc.x + num - 1, (int)this.BoundsRc.y + num2 - 1, (int)this.BoundsRc.right + num + 1, (int)this.BoundsRc.bottom + num2 + 1);
			this.Mp.need_update_collider = true;
		}

		public Vector2 getStabilizeDepertPos()
		{
			float num = this.drawx0 / base.CLEN;
			float num2 = this.drawy0 / base.CLEN;
			return new Vector2((float)X.IntR(base.x - X.frac(num)) + X.frac(num), (float)X.IntR(base.y - X.frac(num2)) + X.frac(num2));
		}

		public override void moveBy(float map_dx, float map_dy, bool recheck_foot = true)
		{
			if (this.chips_reinputted && (map_dx != 0f || map_dy != 0f))
			{
				this.unstabilize(false);
			}
			base.moveBy(map_dx, map_dy, recheck_foot);
		}

		public override M2Mover setTo(float depx, float depy)
		{
			if (this.chips_reinputted && (base.x != depx || base.y != depy))
			{
				this.unstabilize(false);
			}
			base.setTo(depx, depy);
			this.pos_reset = true;
			return this;
		}

		public override void positionChanged(float prex, float prey)
		{
			this.pos_reset = true;
		}

		public override void runPre()
		{
			base.runPre();
		}

		public override void runPost()
		{
			if (this.pos_reset)
			{
				this.resetChipsDrawPosition(0f, 0f);
			}
			if (this.particle_auto_setting_)
			{
				this.effect_t++;
				ChipMover.ExMapChipPtcEffector.setMovingEffect(this.APtcEffector, this.Mp, this.effect_t);
			}
			if (this.TeCon != null)
			{
				this.TeCon.runDrawOrRedrawMesh(X.D_EF, (float)X.AF_EF, this.TS);
			}
			base.runPost();
		}

		public Color32 getColorTe()
		{
			if (this.AddCol == null)
			{
				this.AddCol = new C32(4286545791U);
			}
			return MTRX.ColWhite;
		}

		public void setColorTe(C32 Buf, C32 CMul, C32 CAdd)
		{
			this.AddCol.Set(4286545791U).multiply255((float)CMul.r, (float)CMul.g, (float)CMul.b, (float)CMul.a).add(CAdd.C, true, 2f);
			this.pos_reset = true;
		}

		public Vector2 getShiftTe()
		{
			return new Vector2(this.shift_x_, this.shift_y_);
		}

		public void setShiftTe(Vector2 Pixel)
		{
			this.setShift(Pixel.x, Pixel.y);
		}

		public ChipMover setShift(float pixel_x, float pixel_y)
		{
			this.shift_x_ = pixel_x;
			this.shift_y_ = pixel_y;
			this.pos_reset = true;
			return this;
		}

		public override float getSpShiftX()
		{
			return base.getSpShiftX() + this.shift_x_;
		}

		public override float getSpShiftY()
		{
			return base.getSpShiftY() + this.shift_y_;
		}

		public override Vector2 calcHitUPosFromRay(float raymapx, float raymapy, Vector3 DirU)
		{
			if (this.CC is M2MvColliderCreatorCM)
			{
				Vector3 vector = (this.CC as M2MvColliderCreatorCM).BCC.crosspoint(raymapx, raymapy, raymapx + DirU.x, raymapy - DirU.y, 0.25f, 0.25f, null, true, null);
				if (vector.z >= 2f)
				{
					return vector;
				}
			}
			return base.calcHitUPosFromRay(raymapx, raymapy, DirU);
		}

		public bool particle_auto_setting
		{
			get
			{
				return this.particle_auto_setting_;
			}
			set
			{
				if (value == this.particle_auto_setting_ || this.APuts == null)
				{
					return;
				}
				this.particle_auto_setting_ = value;
				if (value && this.APtcEffector == null)
				{
					this.APtcEffector = ChipMover.ExMapChipPtcEffector.createList(this.APuts);
				}
				if (value)
				{
					if (this.Mp.floort >= 10f)
					{
						ChipMover.ExMapChipPtcEffector.initEffect(this.APtcEffector, this.Mp);
					}
					this.effect_t = 0;
				}
			}
		}

		public void sendActivateToDrawer()
		{
			if (this.APuts == null)
			{
				return;
			}
			int num = this.APuts.Length;
			for (int i = 0; i < num; i++)
			{
				this.APuts[i].Pt.activateToDrawer();
			}
		}

		public void sendDeactivateToDrawer()
		{
			if (this.APuts == null)
			{
				return;
			}
			int num = this.APuts.Length;
			for (int i = 0; i < num; i++)
			{
				this.APuts[i].Pt.deactivateToDrawer();
			}
		}

		public override M2Mover setToDefaultPosition(bool no_set_camera = false, Map2d TargetMap = null)
		{
			return this;
		}

		public virtual void resetChipsDrawPosition(float add_shiftx = 0f, float add_shifty = 0f)
		{
			int num = this.APuts.Length;
			this.pos_reset = false;
			float num2 = this.drawx + this.shift_x_ + add_shiftx - this.drawx0;
			float num3 = this.drawy - this.shift_y_ + add_shifty - this.drawy0;
			int num4 = (int)num2;
			int num5 = (int)num3;
			float num6 = num2 * 0.015625f;
			float num7 = -num3 * 0.015625f;
			for (int i = 0; i < num; i++)
			{
				ChipMover.ExMapChip exMapChip = this.APuts[i];
				exMapChip.Pt.translateByChipMover(num6, num7, this.AddCol, exMapChip.drawx0, exMapChip.drawy0, num4, num5, false);
			}
		}

		public void revertChips()
		{
			if (this.APuts == null)
			{
				return;
			}
			if (this.AddCol != null)
			{
				this.AddCol.Set(4286545791U);
			}
			int num = this.APuts.Length;
			for (int i = 0; i < num; i++)
			{
				ChipMover.ExMapChip exMapChip = this.APuts[i];
				exMapChip.Pt.translateByChipMover(0f, 0f, this.AddCol, exMapChip.drawx0, exMapChip.drawy0, 0, 0, true);
				if (!this.chips_reinputted)
				{
					exMapChip.Pt.remActiveRemoveKey(base.key, this.close_action_destruction);
				}
				if (exMapChip.Pt.AttachCM == this)
				{
					exMapChip.Pt.AttachCM = null;
				}
			}
			this.chips_reinputted = true;
		}

		public virtual bool publishPress(M2Attackable MvTarget, List<M2BlockColliderContainer.BCCLine> ATargetBcc, out bool stop_carrier)
		{
			bool flag = MvTarget.applyPressDamage(this, this.getPressAim(MvTarget), out stop_carrier);
			if ((flag && ATargetBcc != null && this.Phy != null) & stop_carrier)
			{
				Vector2 zero = Vector2.zero;
				for (int i = ATargetBcc.Count - 1; i >= 0; i--)
				{
					M2BlockColliderContainer.BCCLine bccline = ATargetBcc[i];
					if (bccline.BCC.BelongTo as ChipMover == this)
					{
						switch (bccline.aim)
						{
						case AIM.L:
							zero.x = X.Mn(zero.x, MvTarget.mleft - bccline.shifted_x);
							break;
						case AIM.T:
							zero.y = X.Mn(zero.y, MvTarget.mtop - bccline.shifted_y);
							break;
						case AIM.R:
							zero.x = X.Mx(zero.x, MvTarget.mright - bccline.shifted_x);
							break;
						case AIM.B:
							zero.y = X.Mx(zero.y, MvTarget.mbottom - bccline.shifted_y);
							break;
						case AIM.LT:
						case AIM.TR:
						{
							float num = X.Mx(bccline.slopeBottomY(bccline.x_MMX_shifted(MvTarget.mleft)), bccline.slopeBottomY(bccline.x_MMX_shifted(MvTarget.mright)));
							zero.y = X.Mn(zero.y, MvTarget.mtop - num);
							break;
						}
						case AIM.BL:
						case AIM.RB:
						{
							float num = X.Mn(bccline.slopeBottomY(bccline.x_MMX_shifted(MvTarget.mleft)), bccline.slopeBottomY(bccline.x_MMX_shifted(MvTarget.mright)));
							zero.y = X.Mx(zero.y, MvTarget.mbottom - num);
							break;
						}
						}
					}
				}
				this.Phy.addTranslateStack(zero.x, zero.y);
			}
			return flag;
		}

		public float dif_map_x
		{
			get
			{
				return base.x - this.drawx0 / base.CLEN;
			}
		}

		public float dif_map_y
		{
			get
			{
				return base.y - this.drawy0 / base.CLEN;
			}
		}

		public DRect getBoundsRect()
		{
			return this.BoundsRc;
		}

		public bool isCarryingAttacker()
		{
			if (this.ACarry == null)
			{
				return false;
			}
			int count = this.ACarry.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.ACarry[i] is M2FootManager && (this.ACarry[i] as M2FootManager).Mv is M2Attackable)
				{
					return true;
				}
			}
			return false;
		}

		public M2BlockColliderContainer getBCCCon()
		{
			if (this.CC is M2MvColliderCreatorCM)
			{
				return (this.CC as M2MvColliderCreatorCM).BCC;
			}
			return null;
		}

		public override void fineHittingLayer()
		{
			base.gameObject.layer = base.M2D.map_object_layer;
		}

		public virtual int getPressAim(M2Mover Mv)
		{
			bool floating = this.floating;
			return -2;
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			if (this.TeCon != null)
			{
				this.TeCon.destruct();
				this.TeCon = null;
			}
			this.revertChips();
			this.particle_auto_setting = false;
			base.destruct();
		}

		protected float drawx0;

		protected float drawy0;

		protected bool pos_reset;

		private int effect_t;

		public ChipMover.ExMapChip[] APuts;

		private float shift_x_;

		private float shift_y_;

		public float collider_margin;

		private DRect BoundsRc;

		private C32 AddCol;

		protected TransEffecter TeCon;

		private bool chips_reinputted;

		private List<ChipMover.ExMapChipPtcEffector> APtcEffector;

		private bool particle_auto_setting_;

		public bool close_action_destruction;

		private bool deny_coloring_tecon_;

		private bool do_not_bind_BCC_;

		public Func<int, bool> FnColliderBlockCanStand;

		public bool disappearing_collider_trigger;

		public bool disappearing_;

		public class ExMapChip
		{
			public M2Puts Pt;

			public int drawx0;

			public int drawy0;
		}

		private class ExMapChipPtcEffector
		{
			public static List<ChipMover.ExMapChipPtcEffector> createList(ChipMover.ExMapChip[] APuts)
			{
				List<ChipMover.ExMapChipPtcEffector> list = new List<ChipMover.ExMapChipPtcEffector>();
				int num = APuts.Length;
				for (int i = 0; i < num; i++)
				{
					M2Puts pt = APuts[i].Pt;
					string s = pt.getMeta().GetS("chipmover_activating_effect");
					string s2 = pt.getMeta().GetS("chipmover_activating_init_effect");
					if (TX.valid(s) || TX.valid(s2))
					{
						list.Add(new ChipMover.ExMapChipPtcEffector
						{
							Pt = pt,
							ptc_key_moving = (TX.valid(s) ? s : null),
							ptc_key_init = (TX.valid(s2) ? s2 : null),
							intv = pt.getMeta().GetI("chipmover_activating_effect", 1, 1)
						});
					}
				}
				return list;
			}

			public static void initEffect(List<ChipMover.ExMapChipPtcEffector> APtcEffector, Map2d Mp)
			{
				int count = APtcEffector.Count;
				for (int i = 0; i < count; i++)
				{
					ChipMover.ExMapChipPtcEffector exMapChipPtcEffector = APtcEffector[i];
					string text = exMapChipPtcEffector.ptc_key_init;
					if (text != null)
					{
						Mp.PtcSTsetVar("x", (double)exMapChipPtcEffector.Pt.mapcx).PtcSTsetVar("y", (double)exMapChipPtcEffector.Pt.mapcy).PtcST(text, null, PTCThread.StFollow.NO_FOLLOW);
					}
				}
			}

			public static void setMovingEffect(List<ChipMover.ExMapChipPtcEffector> APtcEffector, Map2d Mp, int effect_t)
			{
				int count = APtcEffector.Count;
				for (int i = 0; i < count; i++)
				{
					ChipMover.ExMapChipPtcEffector exMapChipPtcEffector = APtcEffector[i];
					string text = exMapChipPtcEffector.ptc_key_moving;
					if (text != null && effect_t % exMapChipPtcEffector.intv == 0)
					{
						Mp.PtcSTsetVar("x", (double)exMapChipPtcEffector.Pt.mapcx).PtcSTsetVar("y", (double)exMapChipPtcEffector.Pt.mapcy).PtcST(text, null, PTCThread.StFollow.NO_FOLLOW);
					}
				}
			}

			public M2Puts Pt;

			public int intv = 10;

			public string ptc_key_moving;

			public string ptc_key_init;
		}

		public delegate bool FnAttractChip(M2Puts Cp, List<M2Puts> APt);
	}
}

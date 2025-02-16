using System;
using System.Collections.Generic;
using evt;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2Mover : MonoBehaviour, IFootable, IM2EvTrigger, IM2DebugTarget, IPosLitener, IHkdsFollowable
	{
		protected virtual void Start()
		{
			Rigidbody2D rigidbody2D;
			if (base.TryGetComponent<Rigidbody2D>(out rigidbody2D) && this.Mp == null)
			{
				rigidbody2D.isKinematic = true;
			}
		}

		public static void clearIndexCount()
		{
			M2Mover.index_cnt = 0;
		}

		public virtual void appear(Map2d _Mp)
		{
			this.index = ++M2Mover.index_cnt;
			this.snd_key_ = null;
			this.Mp = _Mp;
			if (this.divide_clen_size)
			{
				this.sizex /= this.CLENM;
				this.sizey /= this.CLENM;
				this.divide_clen_size = false;
			}
			this.snd_key_ = null;
			this.fix_aim = false;
			base.enabled = true;
			this.createPhys(null);
			if (this.Phy != null)
			{
				this.Phy.appear(this.Mp);
			}
			this.finePositionFromTransform();
		}

		protected void createPhys(Func<M2Mover, Rigidbody2D, M2Phys> FD_CreatePhys = null)
		{
			if (this.Phy == null)
			{
				if (FD_CreatePhys == null)
				{
					FD_CreatePhys = M2Phys.FD_createPhys;
				}
				Rigidbody2D rigidbody2D;
				if (base.TryGetComponent<Rigidbody2D>(out rigidbody2D))
				{
					this.Phy = FD_CreatePhys(this, rigidbody2D);
					this.FootD = this.Phy.getFootManager();
					if (this.CC == null)
					{
						this.CC = new M2MvColliderCreatorAtk(this).fineRecreate();
					}
				}
			}
		}

		public M2Mover Size(float wmap, float hmap = -1000f, ALIGN align = ALIGN.CENTER, ALIGNY aligny = ALIGNY.MIDDLE, bool resize_moveby = false)
		{
			float num = this.sizex;
			float num2 = this.sizey;
			this.sizex = X.Mx(0f, wmap);
			this.sizey = ((hmap == -1000f) ? this.sizex : X.Mx(0f, hmap));
			if (this.Mp != null)
			{
				this.sizex /= this.CLENM;
				this.sizey /= this.CLENM;
				this.divide_clen_size = false;
			}
			else
			{
				this.divide_clen_size = true;
			}
			if (align != ALIGN.CENTER || aligny != ALIGNY.MIDDLE)
			{
				float num3 = this.sizex - num;
				float num4 = this.sizey - num2;
				if (num3 != 0f || num4 != 0f)
				{
					num3 *= (float)((align == ALIGN.LEFT) ? 1 : ((align == ALIGN.RIGHT) ? (-1) : 0));
					num4 *= (float)((aligny == ALIGNY.TOP) ? 1 : ((aligny == ALIGNY.BOTTOM) ? (-1) : 0));
					if (this.Phy != null && !resize_moveby)
					{
						this.Phy.addFoc(FOCTYPE.RESIZE, num3, num4, -1f, -1, 1, 0, -1, 0);
					}
					else
					{
						this.moveBy(num3, num4, true);
					}
				}
			}
			if (this.Phy != null || this.CC != null)
			{
				if (this.CC == null)
				{
					this.CC = new M2MvColliderCreatorAtk(this);
				}
				this.CC.need_recreate = true;
			}
			return this;
		}

		public virtual void runPre()
		{
			if (this.Phy != null)
			{
				if (this.Phy.main_updated_count > 0)
				{
					this.Phy.memoryPrePosition();
				}
				else
				{
					this.Phy.runPre();
				}
			}
			this.fix_transform_position_flag = false;
			this.finePositionFromTransform();
			if (this.ACarry != null)
			{
				int count = this.ACarry.Count;
				for (int i = 0; i < count; i++)
				{
					this.ACarry[i].setShiftPixel(this, this.getSpShiftX(), this.getSpShiftY());
				}
			}
			if (this.Phy == null)
			{
				bool flag = false;
				if (this.vx_ != 0f || this.vy_ != 0f)
				{
					this.setTo(this.x + this.vx_ * this.TS, this.y + this.vy_ * this.TS);
					flag = true;
				}
				if (this.MScr != null)
				{
					this.runMoveScript();
					flag = true;
				}
				if (flag && this == this.M2D.Cam.getBaseMover())
				{
					this.M2D.Cam.fine_focus_area = true;
				}
			}
			if (this.draw_assist > 0)
			{
				this.draw_assist--;
				return;
			}
			this.fineDrawPosition();
		}

		public void killSpeedForce(bool x_flag = true, bool y_flag = true, bool kill_phy_translate_stack = false)
		{
			if (x_flag)
			{
				this.vx_ = 0f;
			}
			if (y_flag)
			{
				this.vy_ = 0f;
			}
			if (this.Phy != null)
			{
				this.Phy.killSpeedForce(x_flag, y_flag, true, false, kill_phy_translate_stack);
			}
		}

		public void setVelocityForce(float _vx, float _vy)
		{
			this.vx_ = _vx;
			this.vy_ = _vy;
		}

		public virtual void runPost()
		{
			if (this.CC != null && this.CC.need_recreate)
			{
				this.CC.fineRecreate();
			}
			if (this.Phy != null)
			{
				this.Phy.runPost(this.vx, this.vy);
			}
			if (this.NCM != null)
			{
				this.NCM.run();
			}
		}

		public virtual void runPhysics(float fcnt)
		{
			if (this.MScr != null)
			{
				this.runMoveScript();
			}
			if (this.Phy.main_updated_count >= 1 && this.hit_wall_ != (HITWALL)0)
			{
				this.hit_wall_ = (HITWALL)0;
			}
			this.Phy.runPhysics(this.TS, ref this.vx_, ref this.vy_, ref this.drawx_, ref this.drawy_, this.ACarry);
		}

		public virtual void walkBy(FOCTYPE foctype, float map_dx, float map_dy, bool checkwall = false)
		{
			if (map_dx == 0f && map_dy == 0f)
			{
				return;
			}
			if (this.hasFoot() && map_dy < 0f && (foctype & FOCTYPE.WALK) != (FOCTYPE)0U)
			{
				this.FootD.rideInitTo(null, false);
			}
			if (this.Phy != null)
			{
				if (this.walk_auto_assisting > 0 && (this.hasFoot() || this.walk_auto_assisting == 2))
				{
					if (this.walk_auto_assisting == 1)
					{
						this.walk_auto_assisting = 2;
					}
					this.initDrawAssist(10, true);
				}
				this.Phy.addFoc(foctype | (checkwall ? FOCTYPE._CHECK_WALL : ((FOCTYPE)0U)), map_dx, map_dy, -1f, -1, 1, 0, -1, 0);
				return;
			}
			this.moveBy(map_dx, map_dy, true);
		}

		public virtual IFootable checkFootObject(float pre_fall_y)
		{
			if (this.Mp == null || this.FootD == null)
			{
				return null;
			}
			return this.FootD.checkFootObject(pre_fall_y);
		}

		public virtual IFootable canFootOn(IFootable F)
		{
			return F;
		}

		public virtual void changeRiding(IFootable _PD, FOOTRES footres)
		{
			if (this.FootD == null)
			{
				return;
			}
			this.FootD.pivot = (this.FootD.FootIsLadder() ? ALIGN.CENTER : ALIGN.RIGHT);
			switch (footres)
			{
			case FOOTRES.KEEP_AIR:
			case FOOTRES.JUMPED:
			case FOOTRES.JUMPED_IJ:
				if (this.walk_auto_assisting == 2)
				{
					this.fineDrawPosition();
				}
				if (footres == FOOTRES.JUMPED || footres == FOOTRES.JUMPED_IJ)
				{
					this.Phy.fineGravityScale();
					return;
				}
				break;
			case FOOTRES.FOOTED:
				this.Phy.fineGravityScale();
				break;
			default:
				return;
			}
		}

		public virtual IFootable checkSkipLift(M2BlockColliderContainer.BCCLine _P)
		{
			if (this.Phy != null && this.Phy.pre_force_velocity_y > 0f && _P != null && !_P.is_ladder)
			{
				if ((this.Phy.pre_y_attached & (FOCTYPE.WALK | FOCTYPE.JUMP)) != (FOCTYPE)0U)
				{
					return _P;
				}
				if ((this.Phy.pre_y_attached & (FOCTYPE.KNOCKBACK | FOCTYPE._NO_CONSIDER_WATER)) == (FOCTYPE)0U)
				{
					return null;
				}
			}
			return _P;
		}

		public virtual void moveByHitCheck(M2Phys AnotherPhy, FOCTYPE foctype, float map_dx, float map_dy)
		{
			if (this.Phy != null)
			{
				this.Phy.moveByHitCheck(foctype, map_dx, map_dy);
				return;
			}
			this.walkBy(foctype, map_dx, map_dy, true);
		}

		public virtual bool export_other_mover_right(M2Mover Other)
		{
			return this.x < Other.x;
		}

		public void killSpeedFromPhysicsAbsolutely(bool x_flag = true, bool y_flag = true)
		{
			if (x_flag)
			{
				this.vx_ = 0f;
			}
			if (y_flag)
			{
				this.vy_ = 0f;
			}
		}

		protected void moveByInner(M2MoveTicket Tk, float map_dx, float map_dy, bool recheck_foot = true)
		{
			if (Tk != null && !Tk.init(this))
			{
				return;
			}
			this.x_ += map_dx;
			this.y_ += map_dy;
			this.fineTransformToPos(recheck_foot, false);
			if (this.ACarry != null && this.Phy == null)
			{
				int count = this.ACarry.Count;
				for (int i = 0; i < count; i++)
				{
					this.ACarry[i].moveWithFoot(map_dx, map_dy, (this.CC != null) ? this.CC.Cld : null, (this.Phy != null) ? this.Phy.MyBCC : null, true);
				}
			}
			if (Tk != null)
			{
				Tk.quit(this);
			}
		}

		public virtual void moveBy(float map_dx, float map_dy, bool recheck_foot = true)
		{
			this.moveByInner((this.ACarry != null || M2MoveTicket.IT.isActive()) ? M2MoveTicket.IT : null, map_dx, map_dy, recheck_foot);
		}

		public void moveByTranslateStack(float map_dx, float map_dy)
		{
			if (this.Phy != null)
			{
				this.Phy.addTranslateStack(map_dx, map_dy);
				this.x_ += map_dx;
				this.y_ += map_dy;
				return;
			}
			this.moveBy(map_dx, map_dy, false);
		}

		public bool getMovableLen(ref float dx, ref float dy, float margin_side = -0.1f, bool make_hitwall_flag = false, bool check_other_bcc = false)
		{
			bool flag = true;
			if (dx != 0f)
			{
				M2BlockColliderContainer.BCCLine bccline;
				float num;
				this.canGoToSideLB(out bccline, out num, (dx > 0f) ? AIM.R : AIM.L, X.Abs(dx), margin_side, make_hitwall_flag, check_other_bcc, false);
				if (bccline != null)
				{
					flag = false;
					dx = X.VALWALK(dx, 0f, X.Abs(num));
				}
			}
			return flag;
		}

		public virtual void walkByAim(int aim, float speed = 0f)
		{
			if (aim != -1)
			{
				float num = (float)(-(float)CAim._YD(aim, 1));
				this.walkBy(FOCTYPE.WALK, (float)CAim._XD(aim, 1) * speed * this.TS, num * speed * this.TS, false);
			}
		}

		public virtual void walkBySpeed(float __vx, float __vy)
		{
			if (this.Phy != null)
			{
				this.walkBy(FOCTYPE.WALK, __vx, __vy, false);
				return;
			}
			this.vx_ = __vx;
			this.vy_ = __vy;
		}

		public virtual void addFocFallWaterVelocity(float __vy, int duration)
		{
			if (this.Phy != null)
			{
				this.Phy.addFoc(FOCTYPE.KNOCKBACK | FOCTYPE._NO_CONSIDER_WATER, 0f, __vy, -1f, 0, duration, 0, -1, 0);
			}
		}

		public void moveToStandablePoint(float depx, float depy)
		{
			int num = X.IntC(X.LENGTHXYS(this.x, this.y, depx, depy) / 0.25f);
			float num2 = 1f / (float)num;
			int i = 0;
			while (i < num)
			{
				float num3 = (float)i * num2;
				float num4 = X.NI(this.x, depx, num3);
				float num5 = X.NI(this.y, depy, num3);
				if (this.canStand((int)num4, (int)num5) && this.canStand((int)(num4 - this.sizex), (int)(num5 - this.sizex)) && this.canStand((int)(num4 + this.sizex), (int)(num5 - this.sizex)) && this.canStand((int)(num4 + this.sizex), (int)(num5 + this.sizex)) && this.canStand((int)(num4 - this.sizex), (int)(num5 + this.sizex)))
				{
					if (i == 0)
					{
						return;
					}
					this.moveBy(num4 - this.x, num5 - this.y, true);
					return;
				}
				else
				{
					i++;
				}
			}
			this.moveBy(depx - this.x, depy - this.y, true);
		}

		public bool checkHitTo(M2Mover Mv)
		{
			if (this.Phy == null || Mv.Phy == null)
			{
				return false;
			}
			this.Phy.checkHitTo(Mv.Phy);
			return true;
		}

		public virtual bool cannotHitTo(M2Mover Mv)
		{
			return false;
		}

		public virtual void moveWithFoot(float dx, float dy, Collider2D _Collider, M2BlockColliderContainer BCCCarrier, bool add_to_velocity, bool no_collider_lock = false)
		{
			if (!this.canApplyCarryVelocity())
			{
				return;
			}
			if (this.Phy != null && add_to_velocity)
			{
				this.Phy.moveWithFoot(dx, dy, _Collider, BCCCarrier, ref this.vx_, ref this.vy_, no_collider_lock);
				return;
			}
			if (this.FootD == null)
			{
				return;
			}
			this.moveBy(dx, dy, true);
			this.fineDrawPosition();
		}

		public virtual IFootable isCarryable(M2FootManager FootD)
		{
			return this;
		}

		public virtual bool isCarrying(M2Mover Mv)
		{
			return false;
		}

		public bool carryable_other_object
		{
			get
			{
				return this.ACarry != null;
			}
			set
			{
				if (value == (this.ACarry != null))
				{
					return;
				}
				if (value)
				{
					this.ACarry = new List<ICarryable>();
					return;
				}
				if (this.ACarry != null)
				{
					for (int i = this.ACarry.Count - 1; i >= 0; i--)
					{
						this.ACarry[i].initJump(false, true, false);
					}
					this.ACarry = null;
				}
			}
		}

		public virtual IFootable initCarry(ICarryable FootD)
		{
			if (this.ACarry == null)
			{
				return null;
			}
			if (this.ACarry.IndexOf(FootD) == -1)
			{
				this.ACarry.Add(FootD);
			}
			return this;
		}

		public virtual void quitCarry(ICarryable FootD)
		{
			if (this.ACarry == null)
			{
				return;
			}
			this.ACarry.Remove(FootD);
		}

		public virtual float fixToFootPos(M2FootManager FootD, float x, float y, out float dx, out float dy)
		{
			dx = (dy = 0f);
			return 0f;
		}

		public virtual void finePositionFromTransform()
		{
			if (this.Mp == null)
			{
				return;
			}
			Vector2 vector = base.transform.localPosition;
			this.x_ = this.Mp.uxToMapx(vector.x);
			this.y_ = this.Mp.uyToMapy(vector.y);
		}

		public virtual void fineTransformToPos(bool recheck_foot = true, bool force_transform_fix = false)
		{
			if (this.Phy != null && !this.Phy.isPausing())
			{
				this.Phy.fineTransformToPos(recheck_foot);
				if (!force_transform_fix && !this.fix_transform_position_flag)
				{
					return;
				}
			}
			this.fix_transform_position_flag = true;
			Vector3 localPosition = base.transform.localPosition;
			localPosition.x = this.Mp.map2meshx(this.x_) * 0.015625f;
			localPosition.y = this.Mp.map2meshy(this.y_) * 0.015625f;
			base.transform.localPosition = localPosition;
			if (this.FootD != null)
			{
				this.FootD.need_recheck_bcc_cache = true;
			}
		}

		public void OnDestroy()
		{
			if (!this.destructed)
			{
				this.destruct();
			}
		}

		public virtual void deactivateFromMap()
		{
			if (this.destructed)
			{
				return;
			}
			try
			{
				base.transform.SetParent(null, false);
			}
			catch
			{
			}
			base.gameObject.SetActive(false);
			this.quitMoveScript(false);
			if (this.Mp != null)
			{
				this.M2D.DeassignPauseable(this);
			}
			if (this.Phy != null)
			{
				M2FootManager footManager = this.Phy.getFootManager();
				if (footManager != null)
				{
					footManager.initJump(false, true, false);
				}
				this.Phy.Pause();
				if (this.Mp != null)
				{
					this.M2D.DeassignPauseable(this.Phy);
				}
			}
		}

		public virtual void destruct()
		{
			if (this.destructed)
			{
				return;
			}
			this.index = -1;
			if (this.Mp != null)
			{
				this.M2D.DeassignPauseable(this);
			}
			if (this.Phy != null)
			{
				if (this.Mp != null)
				{
					this.M2D.DeassignPauseable(this.Phy);
				}
				this.Phy.destruct();
			}
			if (this.CC != null)
			{
				this.CC.destruct();
			}
			this.CC = null;
			this.carryable_other_object = false;
			this.FootD = null;
			this.Phy = null;
			this.NCM = null;
			if (this.Mp != null)
			{
				this.Mp.removeMover(this);
			}
			else
			{
				try
				{
					IN.DestroyOne(base.gameObject);
				}
				catch
				{
				}
			}
			this.Mp = null;
		}

		protected virtual void OnCollisionEnter2D(Collision2D col)
		{
			if (this.Mp.getTag(col.gameObject) == "Ground")
			{
				this.hit_wall_collider = true;
			}
			if (this.Phy != null)
			{
				this.Phy.OnCollisionEnter2D(col);
			}
		}

		protected virtual void OnCollisionStay2D(Collision2D col)
		{
			if (this.Phy == null)
			{
				return;
			}
			string tag = this.Mp.getTag(col.gameObject);
			M2Phys m2Phys;
			if (tag == "Ground")
			{
				this.Phy.fineWallStuck(false);
				if (!this.floating && !this.Phy.hasFoot() && X.BTWS(0f, this.Phy.gravity_added_velocity, 0.1f))
				{
					M2FootManager footManager = this.Phy.getFootManager();
					if (footManager != null)
					{
						footManager.need_recheck_bcc_cache = true;
						return;
					}
				}
			}
			else if (tag == "Block" && this.Phy.MyBCC == null && col.gameObject != this.Mp.gameObject && this.Mp.Gob2Phys(col.gameObject, out m2Phys))
			{
				this.Phy.addBCCCheck(m2Phys.MyBCC, 60f);
			}
		}

		public virtual float getSpShiftX()
		{
			if (this.FootD == null)
			{
				return 0f;
			}
			return this.FootD.shift_pixel_x;
		}

		public virtual float getSpShiftY()
		{
			if (this.FootD == null)
			{
				return 0f;
			}
			return this.FootD.shift_pixel_y;
		}

		public float getSpLeft()
		{
			return 0f;
		}

		public float getSpTop()
		{
			return 0f;
		}

		protected virtual bool noHitableAttack()
		{
			return true;
		}

		public virtual void fineHittingLayer()
		{
			if (this.Phy == null)
			{
				return;
			}
			base.gameObject.layer = (this.Phy.isLockWallHittingActive() ? LayerMask.NameToLayer((!this.Phy.isLockMoverHittingActive()) ? "Water" : "TransparentFX") : ((!this.Phy.isLockMoverHittingActive()) ? this.Phy.default_layer : 2));
		}

		public virtual M2Mover setTo(float _x, float _y)
		{
			this.x_ = _x;
			this.y_ = _y;
			if (this.Mp == null)
			{
				return this;
			}
			bool flag = false;
			if (this.Phy != null)
			{
				if (!this.Phy.isPausing())
				{
					this.Phy.Pause();
				}
				else
				{
					flag = true;
				}
				if (this.FootD != null)
				{
					this.FootD.need_recheck_bcc_cache = true;
				}
			}
			this.Mp.M2D.Cam.blurCenterIfFocusing(this);
			this.fineDrawPosition().fineTransformToPos(true, true);
			if (this.Phy != null && !flag)
			{
				this.Phy.Resume();
				this.Phy.recheckFoot(0f);
			}
			if (this.NCM != null)
			{
				this.NCM.fine_all = true;
				this.NCM.checkCachedObject(true, NearManager.NCK._PARENT_TYPE);
			}
			this.Mp.MovRenderer.need_clip_check = true;
			return this;
		}

		public virtual M2Mover setToDefaultPosition(bool no_set_camera = false, Map2d TargetMap = null)
		{
			Map2d map2d = TargetMap ?? this.Mp;
			float num = (float)(map2d.rows - map2d.crop) + 0.25f;
			if (EV.isActive(false) && this.mtop > num)
			{
				this.setTo(this.x, num + this.sizey);
				return this;
			}
			if (map2d == null)
			{
				return this;
			}
			M2LabelPoint labelPoint = map2d.getLabelPoint("start");
			if (labelPoint != null)
			{
				this.setTo(labelPoint.mapfocx, labelPoint.mapfocy - this.sizey);
			}
			else
			{
				this.setTo((float)(map2d.clms / 2), (float)(map2d.rows / 2));
			}
			if (!no_set_camera && map2d.M2D.Cam.getBaseMover() == this)
			{
				this.M2D.Cam.fineImmediately();
			}
			return this;
		}

		public M2Mover setToPt(Vector2 P, float _shx = 0f, float _shy = 0f)
		{
			this.x_ = P.x;
			this.y_ = P.y;
			this.setTo(this.x_ + _shx, this.y_ + _shy);
			return this;
		}

		public M2Mover setToArea(M2LabelPoint P)
		{
			if (P != null)
			{
				this.setToArea((float)P.mapx, (float)P.mapy, (float)P.mapw, (float)P.maph);
			}
			return this;
		}

		public M2Mover setToArea(float mapx, float mapy, float mapw, float maph)
		{
			if (this.CC is M2MvColliderCreatorAtk)
			{
				(this.CC as M2MvColliderCreatorAtk).setToRect();
			}
			this.Size(mapw * this.CLEN, maph * this.CLEN, ALIGN.CENTER, ALIGNY.MIDDLE, false);
			this.setTo(mapx + this.sizex, mapy + this.sizey);
			return this;
		}

		public M2Mover setToLabelPt(string label, float _shx = 0f, float _shy = 0f)
		{
			Vector2 pos = this.Mp.getPos(label, _shx, _shy, this);
			this.setToPt(pos, 0f, 0f);
			return this;
		}

		public virtual int checkStuckInWall(ref M2BlockColliderContainer.BCCLine PreStackBcc_, bool extending = false)
		{
			if (!Map2d.can_handle && this.Mp.floort >= 80f)
			{
				return -1;
			}
			M2BlockColliderContainer.BCCLine bccline = PreStackBcc_;
			PreStackBcc_ = null;
			float num = ((!extending) ? (this.sizex * 0.5f) : (-1.25f));
			float num2 = num;
			float num3 = ((!extending) ? (this.sizey * 0.5f) : (-1.25f));
			float num4 = num3;
			if (extending && this.FootD != null)
			{
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				if (footBCC != null)
				{
					if (-footBCC._yd > 0)
					{
						num4 = 0.1f;
					}
					if (-footBCC._yd < 0)
					{
						num3 = 0.1f;
					}
					if (footBCC._xd < 0)
					{
						num = 0.1f;
					}
					if (footBCC._xd > 0)
					{
						num2 = 0.1f;
					}
				}
			}
			Vector3 vector = new Vector3(0f, 0f, -1f);
			M2BlockColliderContainer.BCCLine bccline2 = null;
			int i = 0;
			while (i < 2)
			{
				int num5 = 15;
				if (i != 0)
				{
					if (bccline != null)
					{
						num5 &= ~(1 << (int)bccline.foot_aim);
					}
					bccline = null;
					goto IL_0107;
				}
				if (bccline != null)
				{
					num5 = 1 << (int)bccline.foot_aim;
					goto IL_0107;
				}
				IL_04D4:
				i++;
				continue;
				IL_0107:
				int num6 = (int)(this.mleft + num);
				int num7 = (int)(this.mtop + num3);
				int num8 = X.Mx(num6 + 1, X.IntC(this.mright - num2));
				int num9 = X.Mx(num7 + 1, X.IntC(this.mbottom - num4));
				if (bccline == null || bccline._yd == 0)
				{
					int num10 = 0;
					while (num10 < 2 && (vector.z == -1f || i == 1))
					{
						float num11 = ((num10 == 0) ? (this.mtop + num3) : (this.mbottom - num4));
						for (int j = num6; j < num8; j++)
						{
							float num12 = X.MMX(this.mleft + num, (float)j + 0.5f, this.mright - num2);
							M2BlockColliderContainer.BCCLine bccline3 = bccline;
							Vector3 vector2 = M2BlockColliderContainer.extractFromStuck(this.Mp, num12, num11, ref bccline3, num5, 0f, 0f, false);
							if (vector2.z >= 0f)
							{
								if (i == 0)
								{
									if (CAim.get_aim_tetra(0f, 0f, vector2.x - num12, -(vector2.y - num11)) == CAim.get_opposite(bccline3.foot_aim))
									{
										vector.Set(vector2.x - num12, vector2.y - num11, 1f);
										vector.z = X.Mx(0f, bccline3.stuckInWall_depert_len(this.x, this.y, this.sizex, this.sizey, 1.15f));
										bccline2 = bccline3;
										break;
									}
								}
								else if (vector.z == -1f || bccline3.stuckInWall_depert_len(this.x, this.y, this.sizex, this.sizey, 1.15f) < vector.z - 3.5f)
								{
									PreStackBcc_ = bccline3;
									this.Phy.checkStuckInWall(vector2.x - num12, vector2.y - num11, ref this.vy_, extending);
									return 1;
								}
							}
						}
						num10++;
					}
				}
				if (bccline == null || bccline._yd != 0)
				{
					int num13 = 0;
					while (num13 < 2 && (vector.z == -1f || i == 1))
					{
						float num14 = ((num13 == 0) ? (this.mleft + num) : (this.mright - num2));
						for (int k = num7; k < num9; k++)
						{
							float num15 = X.MMX(this.mtop + num3, (float)k + 0.5f, this.mbottom - num4);
							M2BlockColliderContainer.BCCLine bccline4 = bccline;
							Vector3 vector3 = M2BlockColliderContainer.extractFromStuck(this.Mp, num14, num15, ref bccline4, num5, 0f, 0f, false);
							if (vector3.z >= 0f)
							{
								if (i == 0)
								{
									if (CAim.get_aim_tetra(0f, 0f, vector3.x - num14, -(vector3.y - num15)) == CAim.get_opposite(bccline4.foot_aim))
									{
										vector.Set(vector3.x - num14, vector3.y - num15, 1f);
										vector.z = X.Mx(0f, bccline4.stuckInWall_depert_len(this.x, this.y, this.sizex, this.sizey, 1.15f));
										bccline2 = bccline4;
										break;
									}
								}
								else if (vector.z == -1f || bccline4.stuckInWall_depert_len(this.x, this.y, this.sizex, this.sizey, 1.15f) < vector.z - 3.5f)
								{
									PreStackBcc_ = bccline4;
									this.Phy.checkStuckInWall(vector3.x - num14, vector3.y - num15, ref this.vy_, extending);
									return 1;
								}
							}
						}
						num13++;
					}
					goto IL_04D4;
				}
				goto IL_04D4;
			}
			if (vector.z > 0f)
			{
				PreStackBcc_ = bccline2;
				this.Phy.checkStuckInWall(vector.x, vector.y, ref this.vy_, extending);
				return 1;
			}
			return 0;
		}

		public virtual bool stuckExtractFailure(Rect RcMap)
		{
			float num = 0f;
			float num2 = 0f;
			if (!X.isCovering(RcMap.x, RcMap.xMax, this.mleft, this.mright, 0f))
			{
				if (RcMap.xMax - this.sizex * 2f < this.mleft)
				{
					num = RcMap.xMax - this.sizex * 2f - this.mleft;
				}
				if (this.mright < RcMap.x + this.sizex * 2f)
				{
					num = RcMap.x + this.sizex * 2f - this.mright;
				}
			}
			if (!this.isCovering(RcMap.x, RcMap.xMax, RcMap.yMin, RcMap.yMax, 0f))
			{
				if (RcMap.yMax - this.sizey * 2f < this.mtop)
				{
					num2 = RcMap.yMax - this.sizex * 2f - this.mtop;
				}
				if (this.mbottom < RcMap.y + this.sizey * 2f)
				{
					num2 = RcMap.y + this.sizey * 2f - this.mbottom;
				}
			}
			num *= 0.7f;
			num2 *= 0.7f;
			if (this.FootD != null)
			{
				M2BlockColliderContainer.BCCLine lastBCC = this.FootD.get_LastBCC();
				if (lastBCC != null && lastBCC.BCC == this.Mp.BCC && lastBCC._yd != 0)
				{
					float num3 = this.x + num;
					float num4 = this.y + num2;
					float num5 = X.VALWALK(num3, lastBCC.cx, 0.35f);
					float num6 = X.VALWALK(num4, lastBCC.slopeBottomY(lastBCC.cx) - this.sizey, 0.35f);
					num += num5 - num3;
					num2 += num6 - num4;
				}
			}
			if (num != 0f || num2 != 0f)
			{
				this.moveBy(num, num2, true);
				return true;
			}
			return false;
		}

		public bool isStuckInWall(bool secure_mode = true)
		{
			if (this.hasFoot())
			{
				return false;
			}
			float num = X.Mx(0f, this.sizex - 0.2f);
			float num2 = X.Mx(0f, this.sizey - 0.2f);
			int num3 = (int)(this.x - num);
			int num4 = (int)(this.y - num2);
			int num5 = X.Mx(num3 + 1, X.IntC(this.x + num));
			int num6 = X.Mx(num4 + 1, X.IntC(this.y + num2));
			if (!secure_mode)
			{
				for (int i = num3; i < num5; i++)
				{
					for (int j = num4; j < num6; j++)
					{
						if (!this.Mp.canStand(i, j))
						{
							return true;
						}
					}
				}
				return false;
			}
			bool flag = false;
			for (int k = num3; k < num5; k++)
			{
				flag = true;
				for (int l = num4; l < num6; l++)
				{
					if (this.Mp.canStand(k, l))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				return true;
			}
			flag = false;
			for (int m = num4; m < num6; m++)
			{
				flag = true;
				for (int n = num3; n < num5; n++)
				{
					if (this.Mp.canStand(n, m))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			return flag;
		}

		public virtual bool isRingOut()
		{
			if (this.Mp == null || this.y < (float)(this.Mp.rows - this.Mp.crop))
			{
				return false;
			}
			if (EV.isActive(false))
			{
				return this.mtop > (float)(this.Mp.rows - this.Mp.crop) + 0.55f;
			}
			return this.mbottom > (float)this.Mp.rows - 0.5f;
		}

		public bool initDrawAssist(int f = 1, bool reset_start = false)
		{
			bool flag = this.draw_assist != 0;
			this.draw_assist = X.Mx(f, 0);
			if (this.draw_assist == 0)
			{
				this.fineDrawPosition();
			}
			else if (flag || reset_start)
			{
				this.drawx_ = this.x * this.CLEN;
				this.drawy_ = this.y * this.CLEN;
				return true;
			}
			return false;
		}

		public M2Mover fineDrawPosition()
		{
			if (this.Mp != null)
			{
				this.drawx_ = this.x * this.CLEN;
				this.drawy_ = this.y * this.CLEN;
				if (this.walk_auto_assisting == 2)
				{
					this.walk_auto_assisting = 1;
				}
				this.draw_assist = 0;
			}
			return this;
		}

		public M2Mover setDrawPositionShift(float shift_pixel_x, float shift_pixel_y, int foc_time = 4)
		{
			this.drawx_ = this.x * this.CLEN + shift_pixel_x;
			this.drawy_ = this.y * this.CLEN + shift_pixel_y;
			this.draw_assist = X.Mx(foc_time, 0);
			return this;
		}

		public M2Mover endDrawAssist(int f = 1)
		{
			this.initDrawAssist(0, false);
			return this;
		}

		public void SpSetPoseMulti(string[] APose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
		}

		public virtual void SpSetPose(string nPose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
		}

		public bool destructed
		{
			get
			{
				return this.index < 0;
			}
		}

		public virtual bool SpSetShift(float _x = -1000f, float _y = -1000f)
		{
			return false;
		}

		public virtual bool SpPoseIs(string pose)
		{
			return false;
		}

		public bool SpPoseIs(string _a, string _b)
		{
			return this.SpPoseIs(_a) || this.SpPoseIs(_b);
		}

		public bool SpPoseIs(string _a, string _b, string _c)
		{
			return this.SpPoseIs(_a) || this.SpPoseIs(_b) || this.SpPoseIs(_c);
		}

		public bool SpPoseIs(string _a, string _b, string _c, string _d)
		{
			return this.SpPoseIs(_a) || this.SpPoseIs(_b) || this.SpPoseIs(_c) || this.SpPoseIs(_d);
		}

		public virtual PxlLayer[] SpGetPointsData(ref M2PxlAnimator MyAnimator, ref ITeScaler Scl, ref float rotation_plusR)
		{
			return null;
		}

		public AIM SpAimGet()
		{
			return this.aim;
		}

		public virtual float getSpWidth()
		{
			return 0f;
		}

		public virtual float getSpHeight()
		{
			return 0f;
		}

		public M2SoundPlayerItem playSndAbs(string t)
		{
			if (this.Mp == null)
			{
				return null;
			}
			return this.Mp.playSnd(t);
		}

		public virtual M2SoundPlayerItem playSndPos(string t, byte voice_priority_manual = 1)
		{
			if (this.Mp == null)
			{
				return null;
			}
			return this.Mp.playSnd(t, this.snd_key, this.x, this.y, voice_priority_manual);
		}

		public virtual M2SoundPlayerItem playFootSound(string t, byte voice_priority_manual = 1)
		{
			if (this.Mp == null)
			{
				return null;
			}
			return this.Mp.playSnd(t, this.snd_key, this.x, this.y, voice_priority_manual);
		}

		public virtual string snd_key
		{
			get
			{
				if (this.snd_key_ == null)
				{
					this.snd_key_ = "m2d.mover." + this.index.ToString();
				}
				return this.snd_key_;
			}
		}

		public float talkable_y_extend
		{
			get
			{
				if (this.Phy != null && this.hasFoot())
				{
					return 1.2f;
				}
				return 1f;
			}
		}

		public bool SpIsTetra()
		{
			return false;
		}

		public virtual M2Mover setAim(AIM n, bool sprite_force_aim_set = false)
		{
			this.aim = n;
			if (!this.fix_aim_ || sprite_force_aim_set)
			{
				this.SpSetPose(null, -1, null, sprite_force_aim_set);
			}
			return this;
		}

		public bool fix_aim
		{
			get
			{
				return this.fix_aim_;
			}
			set
			{
				if (value == this.fix_aim_)
				{
					return;
				}
				this.fix_aim_ = value;
				if (value)
				{
					this.setAim(this.aim, true);
				}
			}
		}

		public virtual void SpMotionReset(int set_to_frame = 0)
		{
		}

		public virtual void evInit()
		{
		}

		public virtual void evQuit()
		{
			this.move_script_attached = false;
		}

		public virtual M2Mover assignMoveScript(string str, bool soft_touch = false)
		{
			if (this.MScr == null)
			{
				this.MScr = new M2MoveScript(this, str);
			}
			else
			{
				this.MScr.addScript(str);
			}
			this.move_script_attached = true;
			return this;
		}

		public void initJump()
		{
			if (this.FootD != null)
			{
				this.FootD.initJump(false, false, false);
			}
		}

		public virtual void jumpInit(float xlen, float ypos, float high_y, bool release_x_velocity = false)
		{
			this.FootD.initJump(false, false, false);
			float num = ((this.Phy != null) ? this.Phy.gravity_apply_velocity(1f) : M2Phys.getGravityApplyVelocity(this.Mp, this.base_gravity_, 1f));
			Vector4 jumpVelocity = M2Mover.getJumpVelocity(this.Mp, xlen, ypos, high_y, this.base_gravity_, num);
			this.jumpInit(jumpVelocity, ypos, release_x_velocity);
		}

		public virtual void jumpInit(Vector4 JumpVelocity, float ypos, bool release_x_velocity = false)
		{
			if (this.Phy != null)
			{
				int num = X.IntR(JumpVelocity.z);
				int num2 = X.IntR(JumpVelocity.w);
				this.Phy.addLockGravityFrame(num2);
				this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._GRAVITY_LOCK, 0f, JumpVelocity.y, -1f, 0, 1, num2, -1, 0);
				if (JumpVelocity.x != 0f)
				{
					if (release_x_velocity)
					{
						this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._RELEASE, JumpVelocity.x, 0f, -1f, -1, 1, 0, -1, 0);
						return;
					}
					this.Phy.addFoc(FOCTYPE.WALK, JumpVelocity.x, 0f, -1f, 0, num, num, 1, 0);
					return;
				}
			}
			else
			{
				this.vx_ += JumpVelocity.x;
				this.vy_ = JumpVelocity.y;
			}
		}

		public static Vector4 getJumpVelocity(Map2d Mp, float xlen, float ypos, float high_y, float gravity_scale, float ac = 0f)
		{
			high_y = X.Mx(high_y, 0f);
			ypos = X.Mx(-high_y, ypos);
			if (ac == 0f)
			{
				ac = M2Phys.getGravityApplyVelocity(Mp, gravity_scale, 1f);
			}
			float num = 2f * high_y * ac;
			float num2 = -Mathf.Sqrt(num);
			float num3 = (-num2 + Mathf.Sqrt(X.Mx(0f, num + ac * 2f * ypos))) / ac;
			float num4 = -num2 / ac;
			return new Vector4(xlen / num3, num2, num3, num4);
		}

		public static Vector4 getJumpVelocityT(Map2d Mp, float xlen, float ypos, float time, float gravity_scale, float ac = 0f)
		{
			if (ac == 0f)
			{
				ac = M2DropObject.getGravityVelocity(Mp, gravity_scale);
			}
			time = X.Mx(time, 1f);
			float num = ypos / time - 0.5f * ac * time;
			float num2 = -num / ac;
			return new Vector4(xlen / time, num, time, num2);
		}

		public bool jumpByMoveScript(float height, float t, float maxt)
		{
			if (t == 0f)
			{
				this.jumpInit(0f, 0f, height / this.CLEN, false);
			}
			return false;
		}

		public void savePreFloat()
		{
			if (this.MScr != null)
			{
				this.MScr.savePreFloat();
			}
		}

		public void quitMoveScript(bool run_once = false)
		{
			if (this.MScr != null)
			{
				this.MScr.quitStack(run_once, true, false);
			}
		}

		public virtual void runMoveScript()
		{
			if (!this.MScr.run(false) && (!this.MScr.lock_remove_ms || !EV.isActive(false)))
			{
				this.MScr = null;
			}
		}

		public virtual void breakPoseFixOnWalk(int level)
		{
		}

		public float LENGTHXYRECTS2(M2DrawItem Mv)
		{
			return X.LENGTHXYRECTS2(this.mleft, this.mtop, this.mright, this.mbottom, Mv.mleft, Mv.mtop, Mv.mright, Mv.mbottom);
		}

		public bool isCovering(float l2, float r2, float t2, float b2, float expand = 0f)
		{
			return X.isCovering(this.mleft, this.mright, l2 - expand, r2 + expand, 0f) && X.isCovering(this.mtop, this.mbottom, t2 - expand, b2 + expand, 0f);
		}

		public bool isCovering(float l2, float r2, float t2, float b2, float expand_x, float expand_y)
		{
			return X.isCovering(this.mleft, this.mright, l2 - expand_x, r2 + expand_x, 0f) && X.isCovering(this.mtop, this.mbottom, t2 - expand_y, b2 + expand_y, 0f);
		}

		public bool isContaining(float l2, float r2, float t2, float b2, float expand_x, float expand_y)
		{
			return X.isContaining(this.mleft, this.mright, l2 - expand_x, r2 + expand_x, 0f) && X.isContaining(this.mtop, this.mbottom, t2 - expand_y, b2 + expand_y, 0f);
		}

		public bool isCoveringMv(M2Mover Mva, float margin_x = 0f, float margin_y = 0f)
		{
			return X.isCovering(this.mleft, this.mright, Mva.mleft, Mva.mright, margin_x) && X.isCovering(this.mtop, this.mbottom, Mva.mtop, Mva.mbottom, margin_y);
		}

		public bool isContainingMv(M2Mover Mva, float margin_x = 0f, float margin_y = 0f)
		{
			return X.isContaining(this.mleft, this.mright, Mva.mleft, Mva.mright, margin_x) && X.isContaining(this.mtop, this.mbottom, Mva.mtop, Mva.mbottom, margin_y);
		}

		public virtual void getCameraCenterPos(ref float posx, ref float posy, float shiftx, float shifty, bool immediate, ref float follow_speed)
		{
			posx = this.drawx_tstack + this.getSpShiftX() - ((this.FootD != null) ? this.FootD.shift_pixel_x : 0f) + shiftx;
			posy = this.drawy_tstack - this.getSpShiftY() + ((this.FootD != null) ? this.FootD.shift_pixel_y : 0f) + shifty;
		}

		public virtual void positionChanged(float prex, float prey)
		{
			if (this.NCM != null)
			{
				this.NCM.fine_float_pos = true;
			}
		}

		public virtual void IntPositionChanged(int prex, int prey)
		{
			this.Mp.MovRenderer.need_clip_check = true;
			if (!this.destructed && this == this.Mp.M2D.Cam.getBaseMover())
			{
				this.M2D.mainMvIntPosChanged(false);
			}
			if (this.NCM != null)
			{
				this.NCM.grid1_updated = true;
			}
			if (this == this.M2D.Cam.getBaseMover())
			{
				this.M2D.Cam.fine_focus_area = true;
			}
		}

		public bool canGoToSide(AIM aim, float margin = 0f, float marginy = -0.1f, bool make_hitwall_flag = false, bool check_other_bcc = false, bool refix_position = false)
		{
			M2BlockColliderContainer.BCCLine bccline;
			float num;
			this.canGoToSideLB(out bccline, out num, aim, margin, marginy, make_hitwall_flag, check_other_bcc, refix_position);
			return num == 0f;
		}

		public float canGoToSideL(AIM aim, float margin = 0f, float marginy = -0.1f, bool make_hitwall_flag = false, bool check_other_bcc = false, bool refix_position = false)
		{
			M2BlockColliderContainer.BCCLine bccline;
			float num;
			this.canGoToSideLB(out bccline, out num, aim, margin, marginy, make_hitwall_flag, check_other_bcc, refix_position);
			return margin - X.Abs(num);
		}

		public float canGoToSideLS(float mv_shiftx, float mv_shifty, AIM aim, float margin = 0f, float marginy = -0.1f, bool make_hitwall_flag = false, bool check_other_bcc = false, bool refix_position = false)
		{
			M2BlockColliderContainer.BCCLine bccline;
			float num;
			this.canGoToSideLB(out bccline, out num, mv_shiftx, mv_shifty, aim, margin, marginy, make_hitwall_flag, check_other_bcc, refix_position);
			return margin - X.Abs(num);
		}

		public M2BlockColliderContainer.BCCLine canGoToSideB(AIM aim, float margin = 0f, float marginy = -0.1f, bool make_hitwall_flag = false, bool check_other_bcc = false, bool refix_position = false)
		{
			M2BlockColliderContainer.BCCLine bccline;
			float num;
			this.canGoToSideLB(out bccline, out num, aim, margin, marginy, make_hitwall_flag, check_other_bcc, refix_position);
			return bccline;
		}

		public bool canGoToSideLB(out M2BlockColliderContainer.BCCLine RetBcc, out float backlen, AIM aim, float margin = 0f, float marginy = -0.1f, bool make_hitwall_flag = false, bool check_other_bcc = false, bool refix_position = false)
		{
			return this.canGoToSideLB(out RetBcc, out backlen, 0f, 0f, aim, margin, marginy, make_hitwall_flag, check_other_bcc, refix_position);
		}

		public bool canGoToSideLB(out M2BlockColliderContainer.BCCLine RetBcc, out float backlen, float mv_shiftx, float mv_shifty, AIM aim, float margin = 0f, float marginy = -0.1f, bool make_hitwall_flag = false, bool check_other_bcc = false, bool refix_position = false)
		{
			if (this.floating)
			{
				RetBcc = null;
				backlen = 0f;
				return true;
			}
			if (this.FootD != null)
			{
				this.FootD.expandCacheLgt(X.IntR(X.Mx(marginy, margin)), true);
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				bool flag = false;
				bool flag2 = false;
				if (footBCC != null)
				{
					flag = footBCC.L_is_270;
					flag2 = footBCC.R_is_270;
				}
				float num = -1000f;
				float num2 = 0f;
				List<M2BlockColliderContainer.BCCLine> cachedBccVector = this.FootD.getCachedBccVector();
				for (int i = cachedBccVector.Count - 1; i >= 0; i--)
				{
					M2BlockColliderContainer.BCCLine bccline = cachedBccVector[i];
					if (!bccline.is_lift && bccline != footBCC)
					{
						if (bccline.is_naname && footBCC != null && !this.FootD.FootIsLadder())
						{
							AIM foot_aim = bccline.foot_aim;
							if (((aim != AIM.T && aim != AIM.B) || footBCC._xd == 0) && (foot_aim == AIM.B || aim == AIM.R || aim == AIM.L) && aim != AIM.T)
							{
								bool flag3 = false;
								if (aim == AIM.R || aim == AIM.L)
								{
									float num3 = (float)bccline._yd;
									float num4 = (float)footBCC._yd;
									if (num3 * num4 < 0f)
									{
										float num5 = margin + this.sizex;
										if (num == -1000f)
										{
											num = footBCC.BCC.base_shift_x;
											num2 = footBCC.BCC.base_shift_y;
										}
										float num6 = this.x + mv_shiftx + (float)CAim._XD(aim, 1) * num5;
										float num7 = footBCC.slopeBottomY(X.MMX(footBCC.x - num, num6, footBCC.y - num2));
										float num8 = bccline.slopeBottomY(X.MMX(bccline.x, num6, bccline.right));
										if (num3 > 0f && num4 < 0f)
										{
											flag3 = X.BTWM(-this.sizey * 2f, num7 - num8, this.sizey * 2f);
										}
										else
										{
											flag3 = X.BTWM(-this.sizey * 2f, num8 - num7, this.sizey * 2f);
										}
									}
								}
								if (!flag3)
								{
									goto IL_0265;
								}
							}
						}
						if ((footBCC == null || ((!flag || footBCC.SideL != bccline) && (!flag2 || footBCC.SideR != bccline))) && (backlen = bccline.canGoToSide(this.FootD, mv_shiftx, mv_shifty, aim, margin, marginy, refix_position)) != 0f)
						{
							if (make_hitwall_flag)
							{
								this.setWallHitted(aim);
							}
							RetBcc = bccline;
							return false;
						}
					}
					IL_0265:;
				}
				if (check_other_bcc)
				{
					for (int j = this.Mp.count_carryable_bcc - 1; j >= 0; j--)
					{
						M2BlockColliderContainer carryableBCCByIndex = this.Mp.getCarryableBCCByIndex(j);
						if ((backlen = carryableBCCByIndex.canGoToSide(out RetBcc, this.FootD, mv_shiftx, mv_shifty, aim, margin, marginy, this.hasFoot(), refix_position)) != 0f)
						{
							if (make_hitwall_flag)
							{
								this.setWallHitted(aim);
							}
							return false;
						}
					}
				}
				RetBcc = null;
				backlen = 0f;
			}
			else
			{
				RetBcc = null;
				backlen = 0f;
				if (!this.canThroughRectR(aim, margin, marginy))
				{
					if (make_hitwall_flag)
					{
						this.setWallHitted(aim);
					}
					backlen = 0.01f;
					return false;
				}
			}
			return true;
		}

		public bool cannotRideAnyFloorTo(AIM aim = AIM.B, float margin = 0f, float marginy = -0.1f, bool make_hitwall_flag = false)
		{
			if (this.floating)
			{
				return true;
			}
			if (aim == AIM.T || aim == AIM.B)
			{
				int num = (int)(this.y - (this.sizey + margin) * (float)CAim._YD(aim, 1));
				int i = (int)(this.x - this.sizex - marginy);
				int num2 = (int)(this.x + this.sizex + marginy);
				while (i <= num2)
				{
					if (this.Mp.canFootOn(i, num))
					{
						if (make_hitwall_flag)
						{
							this.hit_wall_ |= (HITWALL)(1 << (int)aim);
						}
						return false;
					}
					i++;
				}
			}
			return true;
		}

		public bool canThroughRectR(AIM aim, float margin = 0f, float marginy = -0.1f)
		{
			if (aim == AIM.L || aim == AIM.R)
			{
				return this.Mp.canThroughRectR(this.x, this.y, (this.sizex - 0.0625f) * 2f, (this.sizey + marginy) * 2f, margin + 0.0625f, aim);
			}
			return this.Mp.canThroughRectR(this.x, this.y, (this.sizex + marginy) * 2f, (this.sizey - 0.0625f) * 2f, margin + 0.0625f, aim);
		}

		public bool checkCornerConfig(float marginx, float marginy, Func<int, bool> FnConfigCheck)
		{
			int num = (int)(this.mleft - marginx);
			int num2 = (int)(this.mright + marginx);
			int num3 = (int)(this.mtop - marginy);
			int num4 = (int)(this.mbottom + marginy);
			for (int i = num; i <= num2; i++)
			{
				for (int j = num3; j <= num4; j++)
				{
					if (FnConfigCheck(this.Mp.getConfig(i, j)))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static Vector3 checkDirectionWalkable(M2Mover Mv, float mvx, float footbottomy, int len, bool reverse_choose = false)
		{
			Map2d mp = Mv.Mp;
			int num = (int)mvx;
			Vector3 vector = new Vector3(mvx, footbottomy, 0f);
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			for (int i = 0; i < 2; i++)
			{
				AIM aim = ((i == 0) ? AIM.L : AIM.R);
				int num2 = (int)(footbottomy + 0.05f);
				int num3 = X.Mx(1, X.IntC(Mv.sizey * 2f));
				bool flag = true;
				int num4 = num;
				int num5 = CAim._XD(aim, 1);
				Vector2 vector2 = new Vector2((float)num4, (float)num2);
				int j;
				for (j = 0; j < len; j++)
				{
					num4 += num5;
					int num6 = 0;
					for (int k = num3; k >= 1; k--)
					{
						num6 = mp.getConfig(num4, num2 - k);
						if (!CCON.canStand(num6))
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						break;
					}
					if (CCON.isBlockSlope(num6) && CCON.getSlopeLevel(num6, num5 == -1) == 0f)
					{
						num2--;
					}
					else if (Mv.FootD != null)
					{
						num6 = mp.getConfig(num4, num2);
						if (!CCON.canFootOn(num6, Mv.FootD))
						{
							flag = false;
							break;
						}
						float slopeLevel = CCON.getSlopeLevel(num6, num5 == 1);
						if (CCON.isSlope(num6) && slopeLevel == 0f)
						{
							num2++;
						}
					}
					vector2.Set((float)num4, (float)num2);
				}
				if (flag)
				{
					vector.z += (float)(1 << (int)aim);
				}
				if (i == 0)
				{
					zero.Set(vector2.x + 0.5f, vector2.y, (float)j);
				}
				if (i == 1)
				{
					zero2.Set(vector2.x + 0.5f, vector2.y, (float)j);
				}
			}
			if (zero.z == zero2.z)
			{
				Vector2 vector3 = ((Mv.is_right == !reverse_choose) ? zero2 : zero);
				vector.x = vector3.x;
				vector.y = vector3.y;
			}
			else
			{
				Vector2 vector4 = ((zero2.z > zero.z) ? zero2 : zero);
				vector.x = vector4.x;
				vector.y = vector4.y;
			}
			return vector;
		}

		public virtual Vector2 calcHitUPosFromRay(float raymapx, float raymapy, Vector3 Dir)
		{
			Dir.y *= -1f;
			Vector2 vector = default(Vector2);
			float num;
			float num2;
			float num3;
			if (Dir.x == 0f)
			{
				num = 0f;
				num2 = 1f;
				num3 = -raymapx;
			}
			else
			{
				float num4 = Dir.y / Dir.x;
				num = num4;
				num2 = -1f;
				num3 = raymapy - num4 * raymapx;
			}
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			Vector2 vector3;
			if (Dir.y < 0f)
			{
				Vector2 vector2;
				if (X.crosspointGH(num, num2, num3, X.MMX(this.y, raymapy, this.mbottom), out vector2) && X.BTW(this.mleft, vector2.x, this.mright))
				{
					zero2 = new Vector3(vector2.x, vector2.y, 1f);
				}
			}
			else if (Dir.y > 0f && X.crosspointGH(num, num2, num3, X.MMX(this.mtop, raymapy, this.y), out vector3) && X.BTW(this.mleft, vector3.x, this.mright))
			{
				zero2 = new Vector3(vector3.x, vector3.y, 1f);
			}
			Vector2 vector5;
			if (Dir.x < 0f)
			{
				Vector2 vector4;
				if (X.crosspointGV(num, num2, num3, X.MMX(this.x, raymapx, this.mright), out vector4) && X.BTW(this.mtop, vector4.y, this.mbottom))
				{
					zero = new Vector3(vector4.x, vector4.y, 1f);
				}
			}
			else if (Dir.x > 0f && X.crosspointGV(num, num2, num3, X.MMX(this.mleft, raymapx, this.x), out vector5) && X.BTW(this.mtop, vector5.y, this.mbottom))
			{
				zero = new Vector3(vector5.x, vector5.y, 1f);
			}
			if (zero.z == zero2.z)
			{
				if (zero.z == 1f)
				{
					vector = ((X.LENGTHXYS(raymapx, raymapy, zero.x, zero.y) < X.LENGTHXYS(raymapx, raymapy, zero2.x, zero2.y)) ? zero : zero2);
				}
				else
				{
					vector = new Vector2(X.MMX2(this.mleft + 0.25f, raymapx, this.mright - 0.25f), X.MMX2(this.mtop + 0.25f, raymapy, this.mbottom - 0.25f));
				}
			}
			else
			{
				vector = ((zero.z == 1f) ? zero : zero2);
			}
			return vector;
		}

		public M2DBase M2D
		{
			get
			{
				return this.Mp.M2D;
			}
		}

		public float weight
		{
			get
			{
				if (this.Phy == null)
				{
					return 1f;
				}
				return this.Phy.mass;
			}
			set
			{
				if (this.Phy != null)
				{
					this.Phy.mass = value;
				}
			}
		}

		public M2MvColliderCreator getColliderCreator()
		{
			return this.CC;
		}

		public bool canStand(int msx = 0, int msy = 0)
		{
			if (!this.floating)
			{
				return CCON.canStand(this.Mp.getConfig(msx, msy), this);
			}
			return X.BTW(0f, (float)msx, (float)this.Mp.clms) && X.BTW(0f, (float)msy, (float)this.Mp.rows);
		}

		public float mleft
		{
			get
			{
				return this.x - this.sizex;
			}
		}

		public float mright
		{
			get
			{
				return this.x + this.sizex;
			}
		}

		public float mtop
		{
			get
			{
				return this.y - this.sizey;
			}
		}

		public float mbottom
		{
			get
			{
				return this.y + this.sizey;
			}
		}

		public float footbottom
		{
			get
			{
				return this.y + this.sizey + ((X.Abs(this.vy_) < 0.08f) ? 0.03f : 0f);
			}
		}

		public virtual bool considerFricOnVelocityCalc()
		{
			return this.FootD != null && this.FootD.hasFoot();
		}

		public virtual bool canJump()
		{
			return this.FootD == null || this.FootD.canJump();
		}

		public bool hasFoot()
		{
			return this.FootD != null && this.FootD.hasFoot();
		}

		public bool on_ladder
		{
			get
			{
				return this.FootD != null && this.FootD.FootIsLadder();
			}
		}

		public float x
		{
			get
			{
				return this.x_;
			}
		}

		public float y
		{
			get
			{
				return this.y_;
			}
		}

		public float vx
		{
			get
			{
				return this.vx_;
			}
		}

		public float vy
		{
			get
			{
				return this.vy_;
			}
		}

		public float CLEN
		{
			get
			{
				Map2d map2d = this.Mp ?? M2DBase.Instance.curMap;
				if (map2d == null)
				{
					return 28f;
				}
				return map2d.CLEN;
			}
		}

		public float CLENM
		{
			get
			{
				Map2d map2d = this.Mp ?? M2DBase.Instance.curMap;
				if (map2d == null)
				{
					return 56f;
				}
				return map2d.CLEN * map2d.mover_scale;
			}
		}

		public float get_sizex()
		{
			return this.sizex;
		}

		public float get_sizey()
		{
			return this.sizey;
		}

		public float getSizeWH()
		{
			return (this.sizex + this.sizey) * 0.5f;
		}

		public Rigidbody2D getRigidbody()
		{
			if (this.Phy == null)
			{
				return null;
			}
			return this.Phy.getRigidbody();
		}

		public M2Phys getPhysic()
		{
			return this.Phy;
		}

		public M2FootManager getFootManager()
		{
			return this.FootD;
		}

		public virtual float drawx
		{
			get
			{
				if (this.draw_assist != 0)
				{
					return this.drawx_;
				}
				return this.x * (this.Mp ?? M2DBase.Instance.curMap).CLEN;
			}
		}

		public virtual float drawy
		{
			get
			{
				if (this.draw_assist != 0)
				{
					return this.drawy_;
				}
				return this.y * (this.Mp ?? M2DBase.Instance.curMap).CLEN;
			}
		}

		public float drawx_tstack
		{
			get
			{
				return this.drawx + ((this.Phy != null) ? (this.Phy.translate_stack_x * (this.Mp ?? M2DBase.Instance.curMap).CLEN) : 0f);
			}
		}

		public float drawy_tstack
		{
			get
			{
				return this.drawy + ((this.Phy != null) ? (this.Phy.translate_stack_y * (this.Mp ?? M2DBase.Instance.curMap).CLEN) : 0f);
			}
		}

		public float get_carry_vx()
		{
			return this.vx;
		}

		public float get_carry_vy()
		{
			return this.vy;
		}

		public float x_shifted
		{
			get
			{
				return this.x + this.getSpShiftX() * (this.Mp ?? M2DBase.Instance.curMap).rCLEN;
			}
		}

		public float y_shifted
		{
			get
			{
				return this.y - this.getSpShiftY() * (this.Mp ?? M2DBase.Instance.curMap).rCLEN;
			}
		}

		public float drawx_map
		{
			get
			{
				return (this.drawx + this.getSpShiftX()) * (this.Mp ?? M2DBase.Instance.curMap).rCLEN;
			}
		}

		public float drawy_map
		{
			get
			{
				return (this.drawy - this.getSpShiftY()) * (this.Mp ?? M2DBase.Instance.curMap).rCLEN;
			}
		}

		public AIM aim_behind
		{
			get
			{
				return CAim.get_aim2(0f, 0f, (float)(-(float)CAim._XD(this.aim, 1)), (float)CAim._YD(this.aim, 1), false);
			}
		}

		public virtual float TS
		{
			get
			{
				return Map2d.TS;
			}
		}

		public virtual float animator_TS
		{
			get
			{
				float num = 1f;
				if (this.Phy != null && this.Phy.isin_water)
				{
					num = (this.Phy.water_speed_scale - 1f) * 0.4f + 1f;
				}
				return ((this.MScr != null) ? this.MScr.ms_timescale : 1f) * num;
			}
		}

		public bool using_draw_assist
		{
			get
			{
				return this.draw_assist > 0;
			}
		}

		public float base_gravity
		{
			get
			{
				return this.base_gravity_;
			}
			set
			{
				if (this.base_gravity_ == value)
				{
					return;
				}
				this.base_gravity_ = value;
				if (this.Phy != null)
				{
					this.Phy.base_gravity = this.base_gravity;
				}
			}
		}

		public virtual bool isMoveScriptActive(bool only_moved_by_event = false)
		{
			return this.MScr != null && this.MScr.isActive();
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"<Mover.",
				this.index.ToString(),
				">-",
				this.key,
				" @",
				X.spr_after(this.x, 1),
				",",
				X.spr_after(this.y, 1)
			});
		}

		public static bool isNull(M2Mover Mv)
		{
			try
			{
				if (Mv.key_ == null || Mv.key_ != null)
				{
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		public bool is_right
		{
			get
			{
				return CAim._XD(this.aim, 1) > 0;
			}
			set
			{
				this.setAim(value ? AIM.R : AIM.L, false);
			}
		}

		public string key
		{
			get
			{
				if (this.key_ != null)
				{
					return this.key_;
				}
				try
				{
					this.key_ = base.gameObject.name;
				}
				catch
				{
				}
				return this.key_;
			}
			set
			{
				base.gameObject.name = value;
				this.key_ = value;
			}
		}

		public float mpf_is_right
		{
			get
			{
				return (float)(this.is_right ? 1 : (-1));
			}
		}

		public bool isinWater()
		{
			return this.Phy != null && this.Phy.isin_water;
		}

		public virtual bool canApplyCarryVelocity()
		{
			return true;
		}

		public bool wallHitted(AIM a)
		{
			return (this.hit_wall_ & (HITWALL)(1 << (int)a)) > (HITWALL)0;
		}

		public bool wallHittedA()
		{
			return this.wallHittedVX(this.mpf_is_right);
		}

		public bool wallHittedVX(float vx)
		{
			return (this.hit_wall_ & ((vx > 0f) ? HITWALL.SIM_R : HITWALL.SIM_L)) > (HITWALL)0;
		}

		public bool wallHittedVY(float vy)
		{
			return (this.hit_wall_ & ((vy > 0f) ? HITWALL.SIM_B : HITWALL.SIM_T)) > (HITWALL)0;
		}

		public void setWallHitted(AIM a)
		{
			this.hit_wall_ |= (HITWALL)(1 << (int)a);
		}

		public bool hit_wall_collider
		{
			get
			{
				return (this.hit_wall_ & HITWALL.COLLIDER) > (HITWALL)0;
			}
			set
			{
				if (value)
				{
					this.hit_wall_ |= HITWALL.COLLIDER;
				}
			}
		}

		public Vector3 getHkdsDepertPos()
		{
			if (this.Mp == null || M2DBase.Instance.curMap == null)
			{
				return Vector3.zero;
			}
			Vector2 vector = this.M2D.Cam.PosMainTransform;
			float scale = this.M2D.Cam.getScale(true);
			return new Vector3((this.Mp.ux2effectScreenx(this.Mp.map2ux(this.drawx_map)) - vector.x) * 64f * scale + this.M2D.ui_shift_x, (this.Mp.uy2effectScreeny(this.Mp.map2uy(this.drawy_map)) - vector.y) * 64f * scale, 1f);
		}

		public bool checkPositionMoved(IMessageContainer Msg)
		{
			return this.Phy == null || this.Phy.prex != this.x || this.Phy.prey != this.y;
		}

		public bool getPosition(out float x, out float y)
		{
			x = this.x;
			y = this.y;
			return true;
		}

		public float VALWALK(float var_value, float depvar, float speed)
		{
			return X.VALWALK(var_value, depvar, speed * this.TS);
		}

		public float VALWALKANGLER(float var_value, float depvar, float speed)
		{
			return X.VALWALKANGLER(var_value, depvar, speed * this.TS);
		}

		public bool isDestructed()
		{
			return this.destructed;
		}

		public float sizex;

		public float sizey;

		public bool divide_clen_size;

		public AIM aim = AIM.B;

		public AIM gravity_aim = AIM.B;

		public bool floating;

		public int walk_auto_assisting = 1;

		public bool slip_slope = true;

		public Map2d Mp;

		private string key_;

		private float x_;

		private float y_;

		private float vx_;

		private float vy_;

		public int index;

		protected float drawx_;

		protected float drawy_;

		protected int draw_assist;

		protected HITWALL hit_wall_;

		protected List<ICarryable> ACarry;

		private float base_gravity_ = 0.6f;

		private const int CHECK_SLIDE_INTERVAL = 4;

		protected M2Phys Phy;

		protected M2FootManager FootD;

		protected M2MvColliderCreator CC;

		public NearCheckerM NCM;

		public bool ringoutable = true;

		public bool do_not_destruct_when_remove;

		public bool move_script_attached;

		private bool fix_aim_;

		protected M2MoveScript MScr;

		private static int index_cnt;

		private bool fix_transform_position_flag;

		private string snd_key_;

		public enum DRAW_ORDER : byte
		{
			_NO_USE,
			MASK_B,
			MASK_G,
			MASK_T,
			N_BACK0,
			N_BACK1,
			N_BACK_EF0,
			N_BACK_EF1,
			BUF_0,
			BUF_1,
			BUF_2,
			PR0,
			PR1,
			PR2,
			N_TOP0,
			N_TOP1,
			N_TOP2,
			N_TOP_EF0,
			N_TOP_EF1,
			CM0,
			CM1,
			CM2,
			_ALL
		}
	}
}

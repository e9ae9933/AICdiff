using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelPlayerCursor
	{
		public NelPlayerCursor(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.MtrBorder = MTRX.newMtr(MTR.MIiconL.getMtr(BLEND.NORMALBORDER8, -1));
			this.MtrBorder.SetFloat("_BorderBit", 15f);
			this.FD_edDraw = new M2DrawBinder.FnEffectBind(this.edDraw);
			this.Ray = new M2Ray();
			this.RaySC = new NelPlayerCursor.M2RayChecker();
			this.AHitRT = new List<M2Ray.M2RayHittedItem>();
		}

		public void destruct()
		{
			if (this.MtrBorder != null)
			{
				IN.DestroyOne(this.MtrBorder);
			}
			this.MtrBorder = null;
		}

		public void init(Map2d _Mp, PR _Pr)
		{
			this.Mp = _Mp;
			this.Pr = _Pr;
			this.Skill = this.Pr.Skill;
			this.M2D.MGC.quitRayCohitable(this.Ray);
			this.Ed = this.Mp.setED("NelCursor", this.FD_edDraw, 0f);
			this.StCalc = new ShotCalcurater(this.Pr);
		}

		public void initMagic(MagicItem _Mg, bool from_sleep = false, bool initialize_run = false)
		{
			this.t_autoaim = -3;
			int num = 0;
			int num2 = 0;
			if (this.Pr.isRO(0))
			{
				num++;
				this.t_autoaim = (from_sleep ? (-4) : (-15));
			}
			if (this.Pr.isLO(0))
			{
				num--;
				this.t_autoaim = (from_sleep ? (-4) : (-15));
			}
			if (this.Pr.isBO(0))
			{
				num2--;
				this.t_autoaim = (from_sleep ? (-4) : (-15));
			}
			if (this.Pr.isTO(0))
			{
				num2++;
				this.t_autoaim = (from_sleep ? (-4) : (-15));
			}
			if (num == 0 && num2 == 0)
			{
				num = CAim._XD(this.Pr.aim, 1);
			}
			this.Ed.t = 0f;
			this.hit_rt_max = 0;
			this.M2D.MGC.quitRayCohitable(this.Ray);
			this.Ray.Set(this.Mp, this.Pr, 0.1f, HITTYPE.EN | HITTYPE.WALL);
			this.Ray.ACohitableCheck = this.M2D.MGC.getCohitableRayList();
			this.RaySC.Set(this.Mp, this.Pr, 0.1f, HITTYPE.EN | HITTYPE.WALL);
			this.RaySC.ACohitableCheck = this.M2D.MGC.getCohitableRayList();
			this.TargetHit = null;
			this.thit_mapx = this.Pr.x;
			this.thit_mapy = this.Pr.y;
			this.t_mn_check_collider = 0;
			int num3 = 255;
			if (_Mg.Mn != null)
			{
				num3 = (int)_Mg.Mn.getManipulateTargetting(_Mg, this.Pr, ref num, ref num2, true);
			}
			int num4 = CAim.fixDirectionByBits(ref num, ref num2, num3);
			if (num4 >= 0)
			{
				this.aim = (AIM)num4;
			}
			this.aim_vector_setted_ = (int)this.aim;
			if (_Mg.Mn != null)
			{
				if (_Mg.Ray != null)
				{
					this.Ray.CopyFrom(_Mg.Ray);
					this.RaySC.CopyFrom(_Mg.Ray);
				}
				this.Ray.hittype |= HITTYPE.AUTO_TARGET;
				this.RaySC.hittype |= HITTYPE.AUTO_TARGET;
				this.Ray.hittype_to_week_projectile = (this.RaySC.hittype_to_week_projectile = HITTYPE.NONE);
				this.Ray.hit_target_max = (this.RaySC.hit_target_max = 32);
				this.Ray.HitLock(0f, null);
				this.RaySC.HitLock(0f, null);
				if (this.AHitBuf == null || this.AHitBuf.Length != this.Ray.hit_target_max)
				{
					this.AHitBuf = new RaycastHit2D[this.Ray.hit_target_max];
				}
				this.StCalc.AHitBuf = this.AHitBuf;
				_Mg.runPre(0f);
				Vector2 aimInitPos = _Mg.getAimInitPos();
				this.Aim.x = aimInitPos.x + (float)CAim._XD(this.aim, 1) * 4f;
				this.Aim.y = aimInitPos.y - (float)CAim._YD(this.aim, 1) * 4f;
			}
			if (initialize_run)
			{
				this.run();
			}
			_Mg.runPre(0f);
			if (_Mg.isPreparingCircle)
			{
				this.StCalc.Clear();
				this.t_camera = 0;
				_Mg.calcAimPos(false);
				_Mg.da = _Mg.aim_agR;
			}
		}

		public void initExplode(MagicItem _Mg)
		{
		}

		public void run()
		{
			MagicItem curMagic = this.getCurMagic();
			this.hit_rt_max = 0;
			if (curMagic == null || curMagic.Mn == null || !this.Skill.showMagicChantingTimeForEffect())
			{
				this.M2D.MGC.quitRayCohitable(this.Ray);
				this.Ed.t = -1f;
				this.t_camera = 0;
				return;
			}
			this.M2D.MGC.initRayCohitable(this.Ray);
			for (int i = this.Ray.getHittedMax() - 1; i >= 0; i--)
			{
				M2Ray.M2RayHittedItem hitted = this.Ray.GetHitted(i);
				if ((hitted.type & HITTYPE._TEMP_REFLECT) != HITTYPE.NONE)
				{
					if (this.hit_rt_max >= this.AHitRT.Count)
					{
						this.AHitRT.Add(new M2Ray.M2RayHittedItem(null));
					}
					List<M2Ray.M2RayHittedItem> ahitRT = this.AHitRT;
					int num = this.hit_rt_max;
					this.hit_rt_max = num + 1;
					ahitRT[num].Set(hitted);
				}
			}
			bool flag = false;
			bool flag2 = false;
			int num2 = 0;
			int num3 = 0;
			MagicNotifiear.TARGETTING targetting = curMagic.Mn.getManipulateTargetting(curMagic, this.Pr, ref num2, ref num3, false);
			if (!this.Skill.cursor_allow_input_aim())
			{
				targetting &= (MagicNotifiear.TARGETTING)(-256);
			}
			if ((targetting & MagicNotifiear.TARGETTING._AIM_ALL) != (MagicNotifiear.TARGETTING)0)
			{
				if (this.Ed.t < 0f)
				{
					this.Ed.t = 0f;
				}
				int num4 = (this.Pr.isLO(0) ? 1 : 0) | (this.Pr.isTO(0) ? 2 : 0) | (this.Pr.isRO(0) ? 4 : 0) | (this.Pr.isBO(0) ? 8 : 0);
				if ((this.Pr.isLP(1) || this.Pr.isMagicLPD(0)) | this.Pr.isTP(1) | (this.Pr.isLP(1) || this.Pr.isMagicLPD(0)) | this.Pr.isBP())
				{
					if (this.t_autoaim < 0)
					{
						this.t_autoaim = 0;
					}
					this.t_naname_hold = 0;
				}
				if (num4 != 0 && this.t_autoaim >= 0)
				{
					this.t_autoaim = 10;
					for (int j = 0; j < 4; j++)
					{
						if ((num4 & (1 << j)) != 0)
						{
							num2 += CAim._XD(j, 1);
							num3 += CAim._YD(j, 1);
						}
					}
				}
				if ((num2 != 0 || num3 != 0) && CAim.fixDirectionByBits(ref num2, ref num3, (int)targetting) != -1)
				{
					flag = true;
					if (num2 == 0 || num3 == 0)
					{
						if (this.t_naname_hold > 0)
						{
							int num = this.t_naname_hold - 1;
							this.t_naname_hold = num;
							if (num > 0)
							{
								num2 += ((num2 == 0) ? CAim._XD(this.aim, 1) : 0);
								num3 += ((num3 == 0) ? CAim._YD(this.aim, 1) : 0);
							}
						}
					}
					else
					{
						this.t_naname_hold = 10;
					}
					this.aim = CAim.get_aim2(0f, 0f, (float)num2, (float)num3, false);
				}
				if (num2 == 0 && num3 == 0)
				{
					if (this.t_autoaim != 0)
					{
						this.t_autoaim = X.VALWALK(this.t_autoaim, 0, 1);
						if (this.t_autoaim == 0)
						{
							if (this.Skill.canUseAutoTargettingMagic())
							{
								this.findAutoTarget();
							}
							if (this.aim_vector_setted_ >= 0)
							{
								this.t_mn_check_collider = 0;
							}
							this.aim_vector_setted_ = -1;
							if (this.TargetHit == null)
							{
								this.t_autoaim = 20;
								this.aim_vector_setted_ = -2;
							}
						}
					}
					if (this.TargetHit == null)
					{
						flag2 = true;
					}
					this.t_naname_hold = 0;
				}
			}
			bool flag3 = false;
			if (curMagic.Mn != null && this.t_mn_check_collider == 0)
			{
				this.t_mn_check_collider = 2;
				flag3 = true;
				if (!curMagic.Mn._0.no_reset_at_cursor_recast)
				{
					curMagic.runPre(0f);
				}
				this.fineRayPositionAndAim(curMagic, 0f);
			}
			if (this.t_autoaim == 0 && this.TargetHit != null && (targetting & MagicNotifiear.TARGETTING._AUTO_TARGET) != (MagicNotifiear.TARGETTING)0 && this.Skill.canUseAutoTargettingMagic())
			{
				if (this.TargetHit is M2Mover)
				{
					M2Mover m2Mover = this.TargetHit as M2Mover;
					this.thit_mapx = m2Mover.x;
					this.thit_mapy = m2Mover.y;
				}
				else if (this.TargetHit is MonoBehaviour)
				{
					Vector3 localPosition = (this.TargetHit as MonoBehaviour).transform.localPosition;
					this.thit_mapx = this.Mp.uxToMapx(localPosition.x);
					this.thit_mapy = this.Mp.uyToMapy(localPosition.y);
				}
				float num5 = this.thit_mapx - this.Pr.x;
				float num6 = this.thit_mapy - this.Pr.y;
				X.Abs(num5);
				X.Abs(num6);
				if (curMagic.Mn != null && flag3 && curMagic.Mn.use_flexible_fix && this.t_camera >= 4)
				{
					Vector2 aimInitPos = curMagic.getAimInitPos();
					this.RaySC.Pos = this.Ray.Pos;
					this.RaySC.Dir = this.Ray.Dir;
					if (this.StCalc.Set(curMagic, this.RaySC, this.Ray, this.AHitRT, this.hit_rt_max, this.TargetHit, aimInitPos).calcProgress(this.Mp, this.thit_mapx, this.thit_mapy))
					{
						this.Pr.target_calced = 2;
						flag3 = false;
					}
				}
				if (this.StCalc.calc_finished)
				{
					this.Aim = this.StCalc.CalcedAimPos;
				}
				else if (curMagic.Mn == null || curMagic.Mn._0.fnFixTargetMoverPos == null || !curMagic.Mn._0.fnFixTargetMoverPos(curMagic, this.Pr, this.Pr.x + num5, this.Pr.y + num6, ref this.Aim, -1))
				{
					this.Aim.x = this.Pr.x + num5;
					this.Aim.y = this.Pr.y + num6;
				}
				if (this.t_mn_check_collider > 0)
				{
					this.t_mn_check_collider--;
				}
			}
			else if (this.t_autoaim < 0 || flag || flag2)
			{
				Vector2 aimInitPos2 = curMagic.getAimInitPos();
				float num7 = aimInitPos2.x + (float)CAim._XD(this.aim, 1) * 4f;
				float num8 = aimInitPos2.y - (float)CAim._YD(this.aim, 1) * 4f;
				if (curMagic.Mn == null || curMagic.Mn._0.fnFixTargetMoverPos == null || !curMagic.Mn._0.fnFixTargetMoverPos(curMagic, this.Pr, num7, num8, ref this.Aim, (int)this.aim))
				{
					this.Aim.x = num7;
					this.Aim.y = num8;
				}
				this.TargetHit = null;
				this.StCalc.Clear();
				if (this.t_mn_check_collider > 0)
				{
					this.t_mn_check_collider--;
				}
				if ((this.t_autoaim < 0 || flag) && this.aim_vector_setted_ != (int)this.aim)
				{
					this.aim_vector_setted = (int)this.aim;
					SND.Ui.play("tool_wand", false);
				}
			}
			if (flag3 && curMagic.Mn != null)
			{
				float num9 = (curMagic.isPreparingCircle ? 0f : curMagic.t);
				this.Pr.target_calced = 2;
				curMagic.Mn.drawTo(null, this.Mp, this.Ray.getMapPos(0f), num9, this.Ray.getAngleR(), !this.Ray.hit_pr, this.Ed.t, this.Ray, this.AHitBuf);
			}
			this.t_camera++;
		}

		private void fineRayPositionAndAim(MagicItem Mg, float aim_ij_R = 0f)
		{
			Mg.calcAimPos(false);
			float aim_agR = Mg.aim_agR;
			Mg.setRayStartPos(this.Ray);
			this.Ray.AngleR(aim_agR + aim_ij_R);
		}

		public float getCameraShiftLevel()
		{
			return X.ZSIN((float)this.t_camera, 44f);
		}

		public Vector3 getTargetPosition()
		{
			if (this.t_autoaim != 0)
			{
				return Vector3.zero;
			}
			return new Vector3(this.thit_mapx, this.thit_mapy, 1f);
		}

		public void blurTargetting(IM2RayHitAble _TargetHit)
		{
			if (this.t_autoaim == 0 && this.TargetHit == _TargetHit)
			{
				this.t_autoaim = 1;
				this.TargetHit = null;
			}
		}

		public int getAim()
		{
			if (this.TargetHit != null)
			{
				return -1;
			}
			return (int)this.aim;
		}

		public int getMagicManipulatorCursorAim(MagicItem Mg)
		{
			return this.aim_vector_setted_;
		}

		private int aim_vector_setted
		{
			get
			{
				return this.aim_vector_setted_;
			}
			set
			{
				if (value != this.aim_vector_setted_)
				{
					this.aim_vector_setted_ = value;
					this.t_mn_check_collider = 0;
				}
			}
		}

		private float getAngleShiftR(int t, float addition)
		{
			return ((float)((t + 1) / 2) + addition) * (float)X.MPF(t % 2 == 1) * 0.104719765f;
		}

		private void findAutoTarget()
		{
			MagicItem curMagic = this.getCurMagic();
			if (curMagic == null)
			{
				this.TargetHit = null;
				return;
			}
			this.cur_shift_len = 4f;
			bool flag = false;
			float num = 0.04f;
			if (curMagic.Mn != null)
			{
				int count = curMagic.Mn.Count;
				for (int i = 0; i < count; i++)
				{
					MagicNotifiear.MnHit hit = curMagic.Mn.GetHit(i);
					if (hit.len > 0f)
					{
						this.cur_shift_len = X.Mn(hit.len, this.cur_shift_len);
						if (hit.wall_hit)
						{
							flag = true;
						}
						num = hit.thick;
						break;
					}
				}
			}
			Vector2 aimInitPos = curMagic.getAimInitPos();
			float num2 = CAim.get_agR(this.aim, 0f);
			float weather_lock_radius = this.weather_lock_radius;
			M2Ray.M2RayHittedItem m2RayHittedItem = M2Ray.findAutoTarget(this.Pr, aimInitPos.x, aimInitPos.y, weather_lock_radius * X.Mx(this.cur_shift_len, 8f), num, (flag ? HITTYPE.WALL : HITTYPE.NONE) | HITTYPE.EN | HITTYPE.OTHER | HITTYPE.AUTO_TARGET, curMagic.projectile_power, weather_lock_radius * X.Mx(1.6f, this.cur_shift_len), num2, this.Pr, curMagic.Caster as M2Mover);
			if (m2RayHittedItem == null)
			{
				this.TargetHit = null;
				return;
			}
			if (this.TargetHit != m2RayHittedItem.Hit)
			{
				this.TargetHit = m2RayHittedItem.Hit;
				this.Ed.t = 0f;
			}
			this.thit_mapx = m2RayHittedItem.mv_mapx;
			this.thit_mapy = m2RayHittedItem.mv_mapy;
			this.t_mn_check_collider = 0;
		}

		public bool need_recalc_len
		{
			set
			{
				this.t_mn_check_collider = 0;
			}
		}

		public Vector2 getAimPos()
		{
			return this.Aim;
		}

		private bool edDraw(EffectItem Ef, M2DrawBinder Ed)
		{
			if (Ed != this.Ed)
			{
				return false;
			}
			MagicItem curMagic = this.getCurMagic();
			if (curMagic == null)
			{
				return true;
			}
			if (curMagic.Mn == null || !this.Skill.showMagicChantingTimeForEffect())
			{
				return true;
			}
			if (this.Ray != null)
			{
				float num = (curMagic.isPreparingCircle ? 0f : curMagic.t);
				float num2;
				if (curMagic.isPreparingCircle)
				{
					Vector2 aimInitPos = curMagic.getAimInitPos();
					num2 = this.Mp.GAR(aimInitPos.x, aimInitPos.y, this.Aim.x, this.Aim.y);
				}
				else
				{
					num2 = curMagic.aim_agR;
				}
				MeshDrawer mesh = Ef.GetMesh("", uint.MaxValue, BLEND.NORMAL, false);
				if (mesh.getTriMax() == 0)
				{
					mesh.base_z -= 0.1f;
				}
				Vector2 vector = curMagic.Mn.drawTo(mesh, this.Mp, curMagic.getRayStartPos(), num, num2, !this.Ray.hit_pr, Ed.t, null, this.AHitBuf);
				if (this.StCalc.is_searching)
				{
					float num3 = this.Mp.map2meshx(vector.x) * this.Mp.base_scale;
					float num4 = this.Mp.map2meshy(vector.y) * this.Mp.base_scale;
					mesh.Line(num3 - 28f, num4 - 28f, num3 + 28f, num4 + 28f, 1f, false, 0f, 0f).Line(num3 - 28f, num4 + 28f, num3 + 28f, num4 - 28f, 1f, false, 0f, 0f);
				}
				this.StCalc.drawDebug(mesh);
			}
			if (this.TargetHit != null && this.t_autoaim == 0 && this.Skill.canUseAutoTargettingMagic())
			{
				Ef.x = this.thit_mapx;
				Ef.y = this.thit_mapy;
				MeshDrawer mesh2 = Ef.GetMesh("", this.MtrBorder, false);
				mesh2.allocUv23(64, false);
				mesh2.Uv23(C32.d2c(2002541660U), false);
				mesh2.base_z -= 0.2f;
				float num5 = X.ZLINE(Ed.t, 34f);
				mesh2.Col = MTRX.cola.Set(4294152183U).blend(4289986425U, X.ANMP((int)Ed.t, 15, 1f)).C;
				float num6 = ((Ed.t >= 30f) ? (-X.SINI(Ed.t - 30f, 90f) * 4.4f) : 0f);
				float num7 = X.ZSIN(num5, 0.4f) * 1.25f - X.ZCOS(num5 - 0.4f, 0.6f) * 0.25f;
				float num8 = X.ZSIN(num5, 0.28f) * 1.5f - X.ZCOS(num5 - 0.26f, 0.24f) * 0.7f + X.ZCOS(num5 - 0.5f, 0.5f) * 0.2f;
				float num9 = 1.5f - 0.5f * X.ZSIN3(num5, 0.8f) * 0.5f;
				PxlFrame pf = MTRX.getPF("lockon_cursor");
				for (int i = 0; i < 2; i++)
				{
					mesh2.Scale(num7, num8, false);
					mesh2.TranslateP(0f, -(X.Mx((this.TargetHit is M2Mover) ? X.Mn((this.TargetHit as M2Mover).sizey, 2.5f) : 0f, 1.5f) * this.Mp.CLENB + 12f + num6), false);
					mesh2.Scale(num9, num9 * (float)X.MPF(i == 0), false);
					mesh2.RotaPF(0f, 0f, 1f, 1f, 0f, pf, false, false, false, uint.MaxValue, false, 0);
					mesh2.RotaPF(0f, 0f, 1f, 1f, 0f, pf, true, false, false, uint.MaxValue, false, 0);
					mesh2.Identity();
				}
				mesh2.allocUv23(0, true);
			}
			return true;
		}

		public float weather_lock_radius
		{
			get
			{
				return X.NI(this.M2D.NightCon.PlayerLockonRadius(), 1f, (float)(EnemySummoner.isActiveBorder() ? 1 : 0));
			}
		}

		public MagicItem getCurMagic()
		{
			return this.Skill.getCurMagicForCursor();
		}

		private Map2d Mp;

		private PR Pr;

		public readonly NelM2DBase M2D;

		private M2PrSkill Skill;

		private Vector2 Aim;

		private M2DrawBinder Ed;

		private const float map_search_radius = 8f;

		private const float map_search_shift_len = 4f;

		private AIM aim;

		private int aim_vector_setted_;

		private IM2RayHitAble TargetHit;

		private float thit_mapx;

		private float thit_mapy;

		private int t_autoaim;

		private int t_naname_hold;

		private float cur_shift_len;

		private M2Ray Ray;

		private NelPlayerCursor.M2RayChecker RaySC;

		private int t_mn_check_collider;

		private int t_camera;

		private List<M2Ray.M2RayHittedItem> AHitRT;

		private int hit_rt_max;

		private Material MtrBorder;

		private const int CHANGE_AUTOAIM_DELAY = 10;

		private const float CAM_MAX_SHIFT_X = 5f;

		private const float CAM_MAX_SHIFT_Y = 4f;

		private RaycastHit2D[] AHitBuf;

		private ShotCalcurater StCalc;

		private M2DrawBinder.FnEffectBind FD_edDraw;

		public class M2RayChecker : M2Ray
		{
			public M2RayChecker()
			{
				base.HitLock(0f, null);
			}

			public override HITTYPE Cast(bool sort = true, RaycastHit2D[] AHit = null, bool ignore_hitlock = false)
			{
				base.clearReflectBuffer();
				if (NelPlayerCursor.M2RayChecker.ABufChk != null && NelPlayerCursor.M2RayChecker.buf_frexible == -1)
				{
					NelPlayerCursor.M2RayChecker.buf_frexible = 0;
					HITTYPE hittype = base.Cast(sort, AHit, ignore_hitlock);
					NelPlayerCursor.M2RayChecker.buf_frexible = this.hit_i;
					for (int i = 0; i < NelPlayerCursor.M2RayChecker.buf_frexible; i++)
					{
						while (NelPlayerCursor.M2RayChecker.ABufChk.Count <= i)
						{
							NelPlayerCursor.M2RayChecker.ABufChk.Add(new M2Ray.M2RayHittedItem(null));
						}
						NelPlayerCursor.M2RayChecker.ABufChk[i].Set(this.AHitted[i]);
					}
					return hittype;
				}
				return base.Cast(sort, AHit, ignore_hitlock);
			}

			public static List<M2Ray.M2RayHittedItem> ABufChk;

			public static int buf_frexible = -2;
		}
	}
}

using System;
using evt;
using m2d;
using XX;
using XX.mobpxl;

namespace nel.mgm.sneaking
{
	public class M2MovePatSneaker : M2MovePatWalker
	{
		public float dep_bottomy { get; private set; }

		public M2MovePatSneaker(M2LpSneakingMG _Con, M2EventItem _Mv, M2LabelPoint _Lp)
			: base(_Mv, M2EventItem.MOV_PAT._OTHER)
		{
			this.Mv.setMovePattern(this);
			this.Con = _Con;
			this.Mv.setLight(0U, -1f);
			this.wait_time_min = 100f;
			this.wait_time_max = 210f;
			this.dep_stand_pose = "stand_light";
			this.dep_walk_pose = "walk_light";
			this.ManpuAwake = new M2ManpuAwake(this.Mv);
			this.switchLp(_Lp, true);
		}

		public void switchLp(M2LabelPoint _Lp, bool immediate = true)
		{
			this.Mv.quitMoveScript(false);
			this.Mv.breakPoseFixOnWalk(0);
			this.Lp = _Lp;
			this.near_aware = 1.15f;
			this.fix_aim = -1;
			this.t_turn_pr_approaching = 0f;
			this.foot_snd = "foot_normal";
			if (TX.valid(this.Lp.comment))
			{
				META meta = new META(this.Lp.comment);
				this.near_aware = meta.GetNm("near_aware", this.near_aware, 0);
				this.fix_aim = CAim.parseString(meta.GetS("fix_aim"), this.fix_aim);
				this.foot_snd = meta.GetS("foot_snd") ?? this.foot_snd;
			}
			this.dep_bottomy = this.Mp.getFootableY(this.Lp.mapcx, (int)this.Lp.mapcy, 12, true, -1f, true, true, true, 0f);
			if (immediate)
			{
				this.randomizePosition();
				return;
			}
			if (this.movtf < 0f)
			{
				this.movtf = -2f;
			}
		}

		public override void destruct()
		{
			if (this.Light != null)
			{
				this.Mp.remLight(this.Light);
				this.Light = null;
			}
			M2MobGAnimator mobg = this.Mv.Mobg;
			if (mobg != null)
			{
				mobg.order = M2Mover.DRAW_ORDER.N_BACK0;
			}
			this.ManpuAwake.destruct();
			base.destruct();
		}

		public void randomizePosition()
		{
			this.Mv.setTo((float)this.Lp.mapx + (float)this.Lp.mapw * X.NIXP(0.3f, 0.7f), this.dep_bottomy - this.Mv.sizey);
			this.Mv.setAim((X.XORSP() < 0.5f) ? AIM.L : AIM.R, false);
			this.movtf = -1f;
			this.walkInit();
		}

		protected override void walkInit()
		{
			if (this.movtf >= 0f)
			{
				AIM aim;
				if (this.fix_aim >= 0)
				{
					if (this.fix_aim == 2)
					{
						aim = ((this.Mv.x < (float)this.Lp.mapx + (float)this.Lp.mapw * 0.15f) ? AIM.R : AIM.L);
					}
					else
					{
						aim = ((this.Mv.x < (float)this.Lp.mapx + (float)this.Lp.mapw * 0.85f) ? AIM.R : AIM.L);
					}
					if (this.fix_aim == (int)aim)
					{
						this.movtf = -100f;
						return;
					}
				}
				else
				{
					aim = ((this.Mv.x < this.Lp.mapcx) ? AIM.R : AIM.L);
				}
				this.Mv.setAim(aim, false);
			}
			this.movtf = 0f;
			float num = X.NIXP(0f, 0.15f);
			if (this.Mv.mpf_is_right > 0f)
			{
				this.dep_x = (float)this.Lp.mapw * (1f - num);
			}
			else
			{
				this.dep_x = (float)this.Lp.mapw * num;
			}
			this.t_turn = 1f;
		}

		public override bool run(float fcnt)
		{
			if (this.Light == null)
			{
				M2MobGAnimator mobg = this.Mv.Mobg;
				if (mobg == null || mobg.Sklt == null)
				{
					return false;
				}
				this.Mv.setLight(0U, -1f);
				this.Light = new M2LightFn(this.Mp, new M2LightFn.FnDrawLight(this.fnDrawLightFront), this.Mv, new Func<float, float, bool>(this.fnIsInCameraLight));
				this.Light.Col.Set(MTRX.ColWhite);
				this.Light.alpha_rgb = 0.7f;
				this.Mp.addLight(this.Light, 0);
				MobSklt sklt = mobg.Sklt;
				sklt.prepareAnimType("stand_light", M2MobGenerator.Instance, "man_elf_normal_stand_light");
				sklt.prepareAnimType("walk_light", M2MobGenerator.Instance, "man_elf_young_walk_dojo_light");
				mobg.order = M2Mover.DRAW_ORDER.CM0;
			}
			if (this.foot_cframe >= 0)
			{
				M2MobGAnimator mobg2 = this.Mv.Mobg;
				M2Mover baseMover = this.Mp.M2D.Cam.getBaseMover();
				if (this.foot_cframe != mobg2.cframe && X.Abs(this.Mv.mbottom - baseMover.mbottom) < 5f)
				{
					this.foot_cframe = mobg2.cframe;
					if (this.foot_cframe == 1 || this.foot_cframe == 4)
					{
						this.Mv.playSndPos(this.foot_snd, 1);
					}
				}
			}
			bool flag = base.run(fcnt);
			if (this.need_fine_light_flag)
			{
				this.need_fine_light_flag = false;
			}
			this.ManpuAwake.run(fcnt);
			bool flag2 = false;
			if (this.t_turn_pr_approaching > 0f)
			{
				flag2 = true;
				this.t_turn_pr_approaching = X.Mx(0f, this.t_turn_pr_approaching - fcnt);
			}
			float num = (flag2 ? 1.5f : 1f);
			if (this.movtf >= 100f && !flag2)
			{
				num *= 0.33f;
			}
			this.t_turn = X.VALWALK(this.t_turn, 100f, fcnt * num);
			return flag;
		}

		protected override bool walkInner(float fcnt, ref bool moved, ref string dep_walk_pose)
		{
			if (this.movtf >= 2000f)
			{
				base.setVelocityX(0f);
				return true;
			}
			if (this.movtf < 100f)
			{
				if ((this.Mv.mpf_is_right > 0f) ? (this.Mv.x >= (float)this.Lp.mapx + this.dep_x) : (this.Mv.x <= (float)this.Lp.mapx + this.dep_x))
				{
					this.quitSightCheck();
					if (this.fix_aim >= 0)
					{
						base.setAim((AIM)this.fix_aim, false);
					}
					return false;
				}
				base.setVelocityX(0.06f * this.Mv.mpf_is_right);
				moved = (this.need_fine_light_flag = true);
			}
			else if (this.movtf < 160f)
			{
				base.setVelocityX(0f);
				this.movtf = X.VALWALK(this.movtf, 160f, fcnt * DIFF.sneaking_TS_npc_checking_prepare);
			}
			else
			{
				if ((this.Mv.mpf_is_right > 0f) ? (this.Mv.x >= this.dep_x) : (this.Mv.x <= this.dep_x))
				{
					this.quitSightCheck();
					this.movtf = -60f;
					return false;
				}
				base.setVelocityX(0.06f * this.Mv.mpf_is_right);
				moved = (this.need_fine_light_flag = true);
			}
			return true;
		}

		internal void checkSightPrCheck(M2MoverPr Pr, DRect Area, float fcnt)
		{
			if (this.movtf >= 2000f || (base.event_stop_waiting && this.event_stop_continue))
			{
				return;
			}
			float mpf_is_right = this.Mv.mpf_is_right;
			if (((mpf_is_right > 0f) ? (Pr.x < this.Mv.x - this.near_aware * 1.25f) : (Pr.x > this.Mv.x + this.near_aware * 1.25f)) || X.Abs(Pr.x - this.Mv.x) > 7f)
			{
				return;
			}
			if (X.Abs(Pr.x - this.Mv.x) < this.near_aware)
			{
				this.t_turn = 65f;
			}
			if ((Pr.x < this.Mv.x) ? (Pr.vx > 0f) : (Pr.vx < 0f))
			{
				this.t_turn_pr_approaching = 4f;
			}
			bool flag = false;
			if (this.movtf < 100f)
			{
				this.movtf = 100f;
				base.setAim((this.Mv.x < Pr.x) ? AIM.R : AIM.L, false);
				this.Con.initCheck(this);
				flag = true;
			}
			float num = Pr.x + 1.25f * (float)X.MPF(this.Mv.x < Pr.x);
			float num2 = X.MMX2(Area.x + 1.85f, num, Area.right - 1.85f);
			if (mpf_is_right > 0f)
			{
				num2 = X.Mx(this.Mv.x, num2);
				this.dep_x = (flag ? num2 : X.Mx(this.dep_x, num2));
			}
			else
			{
				num2 = X.Mn(this.Mv.x, num2);
				this.dep_x = (flag ? num2 : X.Mn(this.dep_x, num2));
			}
			float num3 = X.Mn(X.Abs(this.Mv.x + 0.7f * mpf_is_right - Pr.x), X.Mn(X.Abs(this.Mv.x + 1.4f * mpf_is_right - Pr.x), X.Abs(this.Mv.x + 2.8f * mpf_is_right - Pr.x)));
			float num4 = X.Mx(0.015f, 1f - X.ZSIN2(num3 - this.near_aware, 2.45f)) * X.ZSINV(this.t_turn, 65f);
			this.ManpuAwake.activateCheck(num4, fcnt * DIFF.sneaking_TS_npc_checking);
			if (this.ManpuAwake.is_awared)
			{
				this.movtf = 2000f;
				this.Con.initAware(this);
				this.Mv.setLight(1979711487U, 90f);
			}
		}

		private void quitSightCheck()
		{
			if (this.movtf >= 100f)
			{
				this.movtf = 0f;
				this.Con.quitCheck(this);
			}
			this.ManpuAwake.deactivate();
		}

		public bool is_aware
		{
			get
			{
				return this.movtf >= 2000f;
			}
		}

		internal void StopFromEvent()
		{
			this.SpSetPose(this.dep_stand_pose, -1, null, false);
			base.setVelocityX(0f);
			if (!this.is_aware)
			{
				this.quitSightCheck();
				this.movtf = -1200f;
			}
		}

		public override void SpSetPose(string nPose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
			if (nPose == this.dep_walk_pose)
			{
				if (this.foot_cframe < 0)
				{
					this.foot_cframe = 0;
				}
			}
			else
			{
				this.foot_cframe = -1;
			}
			base.SpSetPose(nPose, reset_anmf, fix_change, sprite_force_aim_set);
		}

		private void fnDrawLightFront(MeshDrawer Md, M2LightFn Lt, float x, float y, float scale, float alpha)
		{
			float num = 1.0367256f;
			float num2 = -num / 8f;
			float num3 = 7f * this.Mp.CLEN - 10f;
			Md.Identity().TranslateP(24f, 0f, false).Scale(this.Mv.mpf_is_right, 1f, false)
				.TranslateP(x, y, false);
			Md.uvRect(-num3 * 0.015625f, -num3 * 1.5f * 0.015625f, num3 * 4f * 0.015625f, num3 * 3f * 0.015625f, MTRX.IconWhite, false, true);
			Md.Col = Md.ColGrd.Set(Lt.Col).mulA(Lt.alpha_rgb * alpha * 0.33f).C;
			Md.PosD(0f, -5f, null).PosD(0f, 5f, null);
			int num4 = -1;
			for (int i = 0; i < 8; i++)
			{
				if (i == 4)
				{
					num4--;
					Md.Tri(-2, -1, 4, false);
				}
				Md.Tri(num4, i, i + 1, false);
			}
			float num5 = num * 0.5f;
			for (int j = 0; j <= 8; j++)
			{
				Md.PosD(num3 * X.Cos(num5), num3 * X.Sin(num5), null);
				num5 += num2;
			}
			Md.Col = Md.ColGrd.Set(Lt.Col).mulA(Lt.alpha_rgb * alpha).C;
			Md.ColGrd.mulA(0f);
			num *= 1.15f;
			Md.Scale(0.8f, 1f, true);
			Md.ArcCGrd(-num3 * 0.64f, 0f, num3 * 2.55f, num3 * 1.5f, -num * 0.5f, num * 0.5f, 1f, 0f, 0);
			Md.Identity();
		}

		private bool fnIsInCameraLight(float meshx, float meshy)
		{
			float num = this.Mv.x + this.Mv.mpf_is_right * 7f * 0.5f;
			return this.Mp.M2D.Cam.isCoveringMp(num - 7f, this.Mv.y - 5f, num + 7f, this.Mv.y + 5f, 0f);
		}

		internal bool isSightCovering(M2LpSneakingMG.BrightArea Area)
		{
			float num = this.Mv.x + this.Mv.mpf_is_right * 7f * 0.5f;
			return Area.isCoveringXy(num - 3.5f, this.Mv.y, num + 3.5f, this.Mv.y + 0.01f, 0f, -1000f);
		}

		internal float getBrightLevel(M2LpSneakingMG.BrightArea Area)
		{
			float mpf_is_right = this.Mv.mpf_is_right;
			float num = this.Mv.x + mpf_is_right * 7f * 0.6f;
			float num2 = this.Mv.x + mpf_is_right * 7f * 0.3f;
			float num3;
			if (Area.isin(num, Area.cy, 0f) || Area.isin(num2, Area.cy, 0f))
			{
				num3 = 1f;
			}
			else
			{
				float num4 = X.Mn(X.Abs(num - Area.left), X.Abs(num - Area.right));
				float num5 = X.Mn(X.Abs(num2 - Area.left), X.Abs(num2 - Area.right));
				float num6 = X.Mn(num4, num5);
				num3 = 1f - X.saturate(num6 * 0.32467532f);
			}
			if (num3 > 0f)
			{
				float num7;
				float num8;
				if (mpf_is_right > 0f)
				{
					num7 = this.Mv.x;
					num8 = this.Mv.x + 7f;
				}
				else
				{
					num7 = this.Mv.x - 7f;
					num8 = this.Mv.x;
				}
				num7 = X.Mx(num7, Area.x);
				num8 = X.Mn(num8, Area.right);
				num3 *= X.ZLINE(num8 - num7, X.Mn(Area.w, 7f));
			}
			return num3;
		}

		protected override bool event_stop_continue
		{
			get
			{
				return base.event_stop_continue || EV.isActive(false);
			}
		}

		private M2LabelPoint Lp;

		private readonly M2LpSneakingMG Con;

		public float dep_x;

		public int fix_aim = -1;

		private const float near_aware_default = 1.15f;

		public float near_aware = 1.15f;

		private M2LightFn Light;

		private float t_turn;

		private float t_turn_pr_approaching;

		private const float walk_speed = 0.06f;

		private const string anim_source_stand = "man_elf_normal_stand_light";

		private const string anim_source_walk = "man_elf_young_walk_dojo_light";

		public const float sight_xlen = 7f;

		private const float MT_CHECK = 100f;

		private const float MT_CHECK_DELAY = 60f;

		private const float turn_back_aware_level_time = 65f;

		private const float MT_AWARE = 2000f;

		private bool need_fine_light_flag;

		private string foot_snd = "foot_normal";

		public int foot_cframe = -1;

		private M2ManpuAwake ManpuAwake;
	}
}

using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class M2CastableEventBase : M2AttackableEventManipulatable, ITortureListener, M2MagicCaster
	{
		public override void appear(Map2d Mp)
		{
			base.appear(Mp);
			this.FD_drawEffectMagicElec = new M2DrawBinder.FnEffectBind(this.drawEffectMagicElecInner);
			this.FD_ElecGetPos = new Func<Vector2>(this.getTargetPos);
			this.OAPointData = new BDic<PxlFrame, M2CastableEventBase.FrameData>();
			string voice_family_name = this.get_voice_family_name();
			if (voice_family_name != null)
			{
				this.VoSrc = NEL.prepareVoiceController(voice_family_name);
				this.Vo = new M2VoiceController(this, this.VoSrc, this.snd_key + ".voice");
			}
			this.casting_time_scale = 0.86f;
			this.publish_hp_damage_ratio = 0.45f;
			if (M2CastableEventBase.MtrAlphaClipMask == null)
			{
				M2CastableEventBase.MtrAlphaClipMask = MTRX.newMtr(MTRX.ShaderAlphaSplice);
			}
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			if (this.Anm != null && this.Anm.Mp != null)
			{
				this.Anm.timescale = 1f;
				this.Anm.check_torture = false;
				this.Anm.auto_replace_mesh = true;
			}
			if (this.VoSrc != null)
			{
				this.VoSrc.destruct();
				this.VoSrc = null;
			}
			if (this.Vo != null)
			{
				this.Vo.destruct();
				this.Vo = null;
			}
			base.destruct();
		}

		protected virtual bool initTarget()
		{
			return false;
		}

		protected bool initRenderTicket()
		{
			if (this.Anm == null)
			{
				this.Anm = this.Mp.getPxlAnimator(this.Par) as M2PxlAnimatorRT;
			}
			this.Anm.FnReplaceRender = new M2RenderTicket.FnPrepareMd(this.RenderPrepareMesh);
			this.Anm.order = M2Mover.DRAW_ORDER.PR0;
			Texture currentTexture = this.Anm.getCurrentTexture();
			if (currentTexture == null)
			{
				return false;
			}
			this.Anm.auto_replace_mesh = false;
			M2CastableEventBase.MtrAlphaClipMask.mainTexture = currentTexture;
			return true;
		}

		public void initTorturePose(string t)
		{
			this.Par.SpSetPose(t, -1, null, false);
			this.Anm.timescale = 0f;
			if (!this.Anm.check_torture)
			{
				this.Anm.check_torture = true;
				this.Mp.addTortureListener(this);
			}
		}

		public void setToTortureFix(float x, float y)
		{
			this.ParPhy.addFoc(FOCTYPE.RESIZE, x - this.Par.x, y - this.Par.y, -1f, -1, 1, 0, -1, 0);
			this.ParPhy.addLockMoverHitting(HITLOCK.ABSORB, 20f);
			this.ParPhy.addLockGravity(this, 0f, 20f);
		}

		protected void quitTorturePose(M2Mover.DRAW_ORDER set_order = M2Mover.DRAW_ORDER.N_BACK0)
		{
			this.Anm.timescale = 1f;
			if (this.Anm.check_torture)
			{
				this.Anm.check_torture = false;
				this.Mp.remTortureListener(this);
			}
			this.Anm.order = set_order;
			this.ParPhy.remLockMoverHitting(HITLOCK.ABSORB);
			this.ParPhy.remLockGravity(this);
		}

		public override void runPre()
		{
			base.runPre();
			if (!this.target_defined && !this.initTarget())
			{
				return;
			}
			if (!this.Par.destructed)
			{
				this.setTo(this.Par.x, this.Par.y);
			}
		}

		public void runPostTorture()
		{
			if (this.Anm == null || this.Par.destructed || this.Target == null || this.Target.destructed)
			{
				this.Mp.remTortureListener(this);
				return;
			}
			if (this.Anm.check_torture)
			{
				float num;
				float num2;
				AbsorbManager.syncTorturePositionS(this.Target, this, this.ParPhy, this.Anm.pose_title, out num, out num2, false);
			}
		}

		public bool check_torture
		{
			get
			{
				return this.Anm.check_torture;
			}
			set
			{
				this.Anm.check_torture = value;
			}
		}

		private M2CastableEventBase.FrameData fineFrameData(PxlFrame F)
		{
			if (this.CurF != F)
			{
				if (!this.OAPointData.TryGetValue(F, out this.CurFD))
				{
					this.OAPointData[F] = (this.CurFD = new M2CastableEventBase.FrameData(this.Mp, F, M2CastableEventBase.MtrAlphaClipMask));
				}
				this.MdMain = this.Anm.getMainMeshDrawer();
				if (this.MdClip == null)
				{
					this.MdClip = new MeshDrawer(null, 4, 6);
					this.MdClip.draw_gl_only = true;
					this.MdClip.activate("", MTRX.newMtr(MTRX.ShaderAlphaSplice), false, MTRX.ColWhite, null);
				}
				this.MdClip.initForImgAndTexture(this.MdMain.getMaterial().mainTexture);
				this.MdMain.clearSimple();
				this.MdMain.Uv23(this.Anm.CAdd, false);
				this.MdMain.Col = this.Anm.color;
				this.MdMain.RotaPF(0f, 0f, 1f, 1f, 0f, F, false, false, false, this.CurFD.main_layer_bits, false, 0);
				this.MdMain.allocUv23(0, true);
				if (this.CurFD.enemy_layer_bits != 0U)
				{
					this.MdClip.clearSimple();
					this.MdClip.RotaPF(0f, 0f, 1f, 1f, 0f, F, false, true, true, this.CurFD.enemy_layer_bits, false, 0);
				}
				this.CurF = F;
			}
			return this.CurFD;
		}

		public override PxlLayer[] SpGetPointsData(ref M2PxlAnimator MyAnimator, ref ITeScaler Scl, ref float rotation_plusR)
		{
			MyAnimator = this.Anm;
			return this.fineFrameData(this.Anm.getCurrentDrawnFrame()).APoints;
		}

		public void setTutoTX(string tx_key)
		{
			this.hideTuto();
			this.tuto_setted = true;
			EV.TutoBox.AddText(TX.Get(tx_key, ""), -1, null);
		}

		public void hideTuto()
		{
			if (this.tuto_setted)
			{
				this.tuto_setted = false;
				EV.TutoBox.RemText(true, false);
			}
		}

		public override Vector2 getTargetPos()
		{
			return new Vector2(base.x, base.y) + ((this.Anm == null) ? Vector2.zero : this.fineFrameData(this.Anm.getCurrentDrawnFrame()).RodPos);
		}

		public override Vector2 getAimPos(MagicItem Mg)
		{
			if (this.Target == null)
			{
				return new Vector2(this.Par.x + this.Par.mpf_is_right * 5f, this.Par.y);
			}
			return new Vector2(this.Target.x, (X.Abs(this.Target.y - this.Par.y) < 0.8f) ? this.Par.y : (this.Target.y - 0.25f));
		}

		protected MagicItem initChant()
		{
			if (this.CurMg != null)
			{
				if (this.CurMg.is_sleep)
				{
					this.CurMg.kill(-1f);
				}
				this.CurMg = null;
			}
			if (this.CurMg == null)
			{
				MGKIND magic_kind = this.get_magic_kind();
				this.CurMg = base.nM2D.MGC.setMagic(this, magic_kind, MGHIT.PR);
				if (this.mp_hold > 0f)
				{
					this.CurMg.t = this.CurMg.casttime * this.mp_hold / this.CurMg.reduce_mp;
				}
			}
			return this.CurMg;
		}

		public bool progressChant()
		{
			if (this.CurMg != null && this.CurMg.casttime > 0f && !this.CurMg.is_sleep)
			{
				float num = X.Mn(this.CurMg.casttime, this.CurMg.t + this.TS * this.casting_time_scale) * this.CurMg.reduce_mp / this.CurMg.casttime;
				this.mp_hold = num;
				return true;
			}
			return false;
		}

		protected bool explodeMagic()
		{
			if (this.CurMg == null)
			{
				return false;
			}
			MagicItem magicItem = this.CurMg.explode(false);
			if (this.CurMg == null || magicItem == null)
			{
				return false;
			}
			magicItem.reduce_mp = 0f;
			this.CurMg = null;
			this.mp_hold = 0f;
			this.CurMg = magicItem;
			return true;
		}

		protected void sleepMagic()
		{
			if (this.CurMg != null && !this.CurMg.is_sleep)
			{
				if (!this.CurMg.isPreparingCircle)
				{
					this.mp_hold = 0f;
					this.CurMg = null;
					return;
				}
				if (this.mp_hold == 0f)
				{
					this.fineHoldMagicTime();
					return;
				}
				this.ParPhy.quitSoftFall(0f);
				this.CurMg.Sleep();
			}
		}

		protected void abortMagic()
		{
			if (this.CurMg != null)
			{
				if (this.CurMg.isPreparingCircle)
				{
					this.CurMg.close();
				}
				this.mp_hold = 0f;
				this.CurMg = null;
			}
		}

		public override void runPost()
		{
			if (this.TeCon == null)
			{
				if (this.Mp != null)
				{
					this.Mp.removeMover(this);
				}
				return;
			}
			base.runPost();
			this.progressChant();
			NoelAnimator.prepareHoldElecAndEd(this.Mp, this.CurMg != null && this.CurMg.isPreparingCircle && this.CurMg.chant_finished, ref this.HoldMagicElec, ref this.HoldMagicEd, this.FD_drawEffectMagicElec, true);
		}

		private bool drawEffectMagicElecInner(EffectItem E, M2DrawBinder Ed)
		{
			if (this.HoldMagicElec == null || this.CurMg == null || this.CurMg.Ray == null)
			{
				if (Ed == this.HoldMagicEd)
				{
					this.HoldMagicEd = null;
				}
				return false;
			}
			return NoelAnimator.drawEffectMagicElecS(this.Mp, E, Ed, this.HoldMagicElec, this.FD_ElecGetPos, true);
		}

		public virtual bool has_chantable_mp()
		{
			return true;
		}

		public bool magic_explodable()
		{
			return this.CurMg != null && this.mp_hold > 0f && this.CurMg.casttime <= this.CurMg.t;
		}

		public override int applyMpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			val = base.applyMpDamage(val, force, Atk);
			if (val > 0)
			{
				this.mp_hold -= (float)val;
				this.fineHoldMagicTime();
			}
			return val;
		}

		protected bool fineHoldMagicTime()
		{
			if (this.CurMg == null || !this.CurMg.isPreparingCircle)
			{
				this.CurMg = null;
				return true;
			}
			if (this.mp_hold <= 0f)
			{
				this.ParPhy.quitSoftFall(0f);
				this.CurMg.close();
				this.CurMg.kill(-1f);
				this.CurMg = null;
				if (this.HoldMagicEd != null)
				{
					this.HoldMagicElec.release();
					this.HoldMagicEd = this.Mp.remED(this.HoldMagicEd);
				}
				return true;
			}
			this.CurMg.castedTimeResetTo(this.CurMg.casttime * this.mp_hold / this.CurMg.reduce_mp);
			return false;
		}

		public bool isExistMagic()
		{
			return this.CurMg != null;
		}

		public bool basicAbsorbProgress(float threshold_t)
		{
			if (this.t >= threshold_t)
			{
				float num = X.NIXP(16f, 38f) * this.fasten;
				this.t = threshold_t - num;
				this.absorbDamageEffect();
				if (X.XORSP() < 0.07f)
				{
					this.fasten = X.Mn(1f, 0.5f + X.XORSP() * 0.78f);
					this.Target.getAnimator().timescale = X.Mx(1f, 0.8f + X.XORSP() * 1.88f);
				}
				return true;
			}
			return false;
		}

		public void absorbDamageEffect()
		{
			this.TeCon.setDmgBlink(MGATTR.ABSORB, 10f, 1f, 0f, 0);
			base.nM2D.Mana.AddMulti(base.x, base.y, X.NIXP(4f, 8f) * 4f, MANA_HIT.FROM_DAMAGE_SPLIT | MANA_HIT.FALL | MANA_HIT.FALL_EN, 1f);
			this.playVo("dmga");
			if (X.XORSP() < 0.63f)
			{
				this.TeCon.setQuake(3f, 7, 1f, 4);
			}
			if (X.XORSP() < 0.38f)
			{
				this.TeCon.setQuake(3f, 7, 1f, 4);
			}
			if (X.XORSP() < 0.45f)
			{
				this.Target.absorbEffect();
				this.playSndPos("absorb_guchu", 1);
			}
			base.PtcVar("x", (double)this.Par.x).PtcVar("y", (double)this.Par.y).PtcVar("hit_x", (double)(this.Par.x + X.XORSPS() * 0.125f))
				.PtcVar("hit_y", (double)(this.Par.y + X.XORSPS() * 0.55f))
				.PtcST("player_absorbed_basic", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			if (X.XORSP() < 0.4f)
			{
				base.PtcST("absorb_atk_basic", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			if (this.Target != null && X.XORSP() < 0.5f)
			{
				this.Target.getAnimator().randomizeFrame(0.5f, 0.5f);
			}
		}

		protected bool RenderPrepareMesh(Camera Cam, M2RenderTicket Tk, bool need_redraw, int draw_id, out MeshDrawer MdOut, ref bool color_one_overwrite)
		{
			MdOut = null;
			if (this.disappearing)
			{
				return false;
			}
			if (draw_id == 0)
			{
				this.fineFrameData(this.Anm.getCurrentDrawnFrame());
				MdOut = this.MdMain;
				return true;
			}
			if (draw_id != 1)
			{
				return false;
			}
			if (!this.Anm.check_torture)
			{
				return false;
			}
			if (this.CurFD.enemy_layer_bits == 0U)
			{
				return false;
			}
			MdOut = this.MdClip;
			return true;
		}

		public bool setTortureAnimation(string pose_name, int cframe, int loop_to)
		{
			if (this.Anm.poseIs(pose_name, false))
			{
				this.Anm.animReset(cframe);
				return true;
			}
			return false;
		}

		public void playVo(string v)
		{
			if (this.Vo != null)
			{
				this.Vo.play(v, false);
			}
		}

		protected virtual MGKIND get_magic_kind()
		{
			return MGKIND.FIREBALL;
		}

		public virtual string get_voice_family_name()
		{
			return "ixia";
		}

		public override string get_parent_name()
		{
			return "ixia";
		}

		protected NelEnemy Target;

		protected bool target_defined;

		private PxlFrame CurF;

		private M2CastableEventBase.FrameData CurFD;

		protected BDic<PxlFrame, M2CastableEventBase.FrameData> OAPointData;

		protected static Material MtrAlphaClipMask;

		protected MeshDrawer MdBuf;

		private M2VoiceController Vo;

		protected float fasten = 1f;

		private VoiceController VoSrc;

		private M2DrawBinder.FnEffectBind FD_drawEffectMagicElec;

		private MeshDrawer MdMain;

		private MeshDrawer MdClip;

		public bool disappearing;

		protected bool tuto_setted;

		private Func<Vector2> FD_ElecGetPos;

		protected MagicItem CurMg;

		protected float mp_hold;

		private ElecTraceDrawer HoldMagicElec;

		private M2DrawBinder HoldMagicEd;

		private M2DrawBinder ExpectationEd;

		private M2Ray RayForDraw;

		private float expect_collider_t;

		protected class FrameData
		{
			public FrameData(Map2d Mp, PxlFrame F, Material MtrAlphaClipMask)
			{
				List<PxlLayer> list = null;
				int num = F.countLayers();
				bool flag = false;
				for (int i = 0; i < num; i++)
				{
					PxlLayer layer = F.getLayer(i);
					if (TX.isStart(layer.name, "point_", 0))
					{
						layer.alpha = 0f;
						if (list == null)
						{
							list = new List<PxlLayer>(1);
						}
						list.Add(layer);
					}
					if (TX.isStart(layer.name, "enemy", 0))
					{
						this.main_layer_bits &= ~(1U << i);
						this.enemy_layer_bits |= 1U << i;
					}
					if (!flag && TX.isStart(layer.name, "cane", 0))
					{
						flag = true;
						Vector3 vector = layer.getTransformMatrix().MultiplyPoint3x4(new Vector3((float)(-(float)layer.Img.width) * 0.5f + 6f, (float)layer.Img.height * 0.5f - 11f, 0f) * 0.015625f);
						this.RodPos = new Vector2(vector.x, -vector.y) * 64f * Mp.rCLENB;
					}
				}
				this.APoints = ((list != null) ? list.ToArray() : null);
			}

			public PxlLayer[] APoints;

			public Vector2 RodPos;

			public uint enemy_layer_bits;

			public uint main_layer_bits = uint.MaxValue;
		}
	}
}

using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class AbsorbManager : IRunAndDestroy
	{
		public AbsorbManager(AbsorbManagerContainer _Con)
		{
			this.Con = _Con;
			this.Gacha = new PrGachaItem(this);
			this.FD_notTortureEnable = new Func<AbsorbManager, bool>(this.notTortureEnable);
		}

		public bool InitTP(NelM2Attacker T, NelM2Attacker P, int _id)
		{
			if (this.isActive())
			{
				return false;
			}
			this.TargetM = T as M2Attackable;
			this.PublishM = P as M2Attackable;
			if (this.PublishM == null || this.TargetM == null)
			{
				return false;
			}
			this.Target = T;
			this.Publish = P;
			this.id = _id;
			this.Listener = null;
			this.PosT = (this.PosT0 = new Vector2(this.TargetM.x, this.TargetM.y));
			this.PosP = (this.PosP0 = new Vector2(this.PublishM.x, this.PublishM.y) - this.PosT);
			if (X.Abs(this.PosT0.x - this.TargetM.x) >= 1.5f)
			{
				this.DEBUG();
			}
			float num = 0.8f;
			this.use_torture = 0;
			this.PublishKiss.Set(-this.TargetM.sizex * num, -this.TargetM.sizey * num, this.TargetM.sizex * num * 2f, this.TargetM.sizey * num * 2f);
			this.is_down_ = AbsorbManager.ABM_POSE._NOT_INITTED;
			this.kiss_af = 0f;
			this.no_change_release_in_dead_ = (this.no_shuffleframe_on_applydamage_ = false);
			this.kirimomi_release = (this.publish_float = (this.penetrate_decline = (this.cannot_apply_mist_damage_ = false)));
			this.need_translate_first = true;
			this.use_cam_zoom2_ = false;
			this.Con.use_torture = false;
			this.Con.no_clamp_speed = false;
			this.release_from_publish_count = false;
			this.no_ser_burned_effect_ = false;
			this.mouth_is_covered_ = false;
			this.emstate_attach_ = UIPictureBase.EMSTATE.NORMAL;
			this.normal_UP_fade_injectable_ = 0.66f;
			this.Con.normal_UP_fade_injectable = -1f;
			this.pose_priority_ = 1;
			this.no_clamp_speed_ = (this.cannot_move_ = false);
			this.breath_key_ = null;
			this.target_pose_ = "";
			this.Gacha.clear(this.TargetM as PR).activate(PrGachaItem.TYPE.CANNOT_RELEASE, 0, 63U);
			this.gacha_releaseable = false;
			this.Con.need_fine_gacha_release = false;
			this.uipicture_fade_key_ = "";
			return true;
		}

		public bool run(float fcnt)
		{
			if (!this.isActive())
			{
				this.destruct();
				return false;
			}
			if (!this.Gacha.run(fcnt))
			{
				if (!this.gacha_releaseable)
				{
					this.gacha_releaseable = true;
					this.Con.need_fine_gacha_release = true;
				}
			}
			else
			{
				this.gacha_releaseable = false;
				if (this.Gacha.isActive() && !this.Gacha.isErrorInput())
				{
					this.Con.gacha_active_count = true;
				}
			}
			return true;
		}

		private void DEBUG()
		{
			Vector2 posT = this.PosT;
		}

		public AbsorbManager fineFirstPos(AbsorbManager Abm0)
		{
			if (Abm0 != this && Abm0 != null)
			{
				this.PosT.x = this.PosT.x + (Abm0.PosT0.x - this.PosT0.x);
				this.PosT.y = this.PosT.y + (Abm0.PosT0.y - this.PosT0.y);
				this.PosT0 = Abm0.PosT0;
				if (X.Abs(this.PosT0.x - this.TargetM.x) >= 1.5f)
				{
					this.DEBUG();
				}
				this.need_translate_first = false;
			}
			else if (this.need_translate_first)
			{
				this.PosT.x = this.PosT.x + (this.TargetM.x - this.PosT0.x);
				this.PosT.y = this.PosT.y + (this.TargetM.y - this.PosT0.y);
				this.PosT0.Set(this.TargetM.x, this.TargetM.y);
				this.need_translate_first = false;
			}
			return this;
		}

		public void resetKissAf()
		{
			if (this.use_torture == 0)
			{
				this.kiss_af = 0f;
			}
		}

		public bool isActive()
		{
			return this.Target != null && this.Publish != null;
		}

		public bool isActive(NelM2Attacker _Target, NelM2Attacker _Publ, bool check_torture)
		{
			return this.isActive() && this.Target == _Target && this.Publish == _Publ && (!check_torture || this.use_torture <= 0 || (this.PublishM.SpPoseIs(this.target_pose) && this.TargetM.SpPoseIs(this.target_pose)));
		}

		public void releaseFromPublish(NelM2Attacker _Publ)
		{
			if (this.Publish == _Publ)
			{
				this.destruct();
			}
		}

		public bool releaseFromTarget(NelM2Attacker _Target)
		{
			if (this.Target == _Target)
			{
				this.destruct();
				return true;
			}
			return this.Target == null;
		}

		public bool checkPublisher(NelM2Attacker _Publ)
		{
			return this.Publish != null && this.Publish == _Publ;
		}

		public bool checkTarget(NelM2Attacker _Target)
		{
			return this.Target != null && this.Target == _Target;
		}

		public bool checkTargetAndLength(NelM2Attacker _Target, float map_margin = 3f)
		{
			return this.Target != null && this.Target == _Target && this.TargetM != null && this.PublishM != null && this.TargetM.isCoveringMv(this.PublishM, map_margin, map_margin);
		}

		public void translateTarget(NelM2Attacker _Target, float dx, float dy)
		{
			if (this.Target != null && this.Target == _Target)
			{
				this.PosT0.x = this.PosT0.x + dx;
				this.PosT.x = this.PosT.x + dx;
				this.PosT0.y = this.PosT0.y + dy;
				this.PosT.y = this.PosT.y + dy;
			}
		}

		public void destruct()
		{
			if (this.Listener != null)
			{
				this.Listener.absorbFinished(this.Gacha != null && !this.Gacha.isFinished());
				this.Con.removeListener(this.Listener);
			}
			this.Listener = null;
			if (this.TargetM != null)
			{
				this.TargetM.getPhysic();
				if (this.release_from_publish_count)
				{
					this.Con.releaseFromCounter(this.Publish);
				}
			}
			if (this.Publish is M2Mover)
			{
				M2Phys physic = (this.Publish as M2Mover).getPhysic();
				if (physic != null)
				{
					physic.remLockWallHitting(this);
				}
			}
			this.Con.finePosePriority(this.pose_priority_);
			if (this.use_torture > 0)
			{
				this.TargetM.quitTortureAbsorb();
			}
			if (this.cannot_apply_mist_damage_)
			{
				this.Con.cannot_apply_mist_damage = false;
				this.cannot_apply_mist_damage_ = false;
			}
			if (this.emstate_attach_ != UIPictureBase.EMSTATE.NORMAL)
			{
				this.Con.emstate_attach = (UIPictureBase.EMSTATE)(-1);
			}
			this.emstate_attach_ = UIPictureBase.EMSTATE.NORMAL;
			if (this.no_clamp_speed_)
			{
				this.Con.no_clamp_speed = false;
				this.no_clamp_speed_ = false;
			}
			this.no_change_release_in_dead_ = false;
			this.no_ser_burned_effect_ = false;
			if (this.no_shuffle_aim_)
			{
				this.no_shuffle_aim_ = false;
				this.Con.no_shuffle_aim = false;
			}
			if (this.cannot_move_)
			{
				this.cannot_move_ = false;
				this.Con.cannot_move = false;
			}
			if (this.mouth_is_covered_)
			{
				this.mouth_is_covered_ = false;
				this.Con.mouth_is_covered = false;
			}
			if (this.no_shuffleframe_on_applydamage_)
			{
				this.no_shuffleframe_on_applydamage_ = false;
				this.Con.no_shuffleframe_on_applydamage = false;
			}
			if (this.uipicture_fade_key_ != "")
			{
				this.uipicture_fade_key = "";
			}
			this.breath_key = null;
			this.Con.normal_UP_fade_injectable = -1f;
			this.Con.removeMoveByVector(this.PublishM);
			this.Target = (this.Publish = null);
			if (this.Gacha.isUseable())
			{
				this.Con.need_fine_gacha_effect = true;
				this.Con.need_fine_gacha_release = true;
			}
			this.use_torture = 0;
			this.Con.total_weight = -1f;
			this.gacha_releaseable = true;
			this.use_cam_zoom2_ = false;
			this.Gacha.clear(null);
			this.Con.FlgCamZoom.Rem(this);
			this.TargetM = (this.PublishM = null);
		}

		public void changeTorturePose(string p)
		{
			if (this.TargetM == null || this.PublishM == null)
			{
				return;
			}
			this.changeTorturePose(p, CAim._XD(this.TargetM.aim, 1) != CAim._XD(this.PublishM.aim, 1), false, -1, -1);
		}

		public void changeTorturePose(string p, bool opposite_aim, bool clear_tecon = false, int set_frame = -1, int reset_animf = -1)
		{
			if (this.TargetM == null || this.PublishM == null)
			{
				return;
			}
			if (clear_tecon)
			{
				this.TargetM.TeCon.clear();
				this.PublishM.TeCon.clear();
			}
			if (opposite_aim)
			{
				this.use_torture = 2;
			}
			else
			{
				this.use_torture = 1;
			}
			this.Con.use_torture = true;
			this.target_pose = p;
			this.pose_priority_ = 99;
			this.TargetM.initJump();
			this.PublishM.initTortureAbsorbPoseSet(p, set_frame, reset_animf);
			this.TargetM.initTortureAbsorbPoseSet(p, set_frame, reset_animf);
			this.syncTorturePosition(p);
			this.Con.resetKissAf();
		}

		public void setKirimomiReleaseDir(int a = -1)
		{
			if (this.use_torture > 0)
			{
				this.Con.kirimomi_release_dir = a;
			}
		}

		public bool syncTorturePosition(string pose_title)
		{
			if (this.use_torture == 0 || this.TargetM == null || this.PublishM == null)
			{
				return false;
			}
			if (this.PublishM.SpPoseIs(pose_title))
			{
				float num = 0f;
				float num2 = 0f;
				if (AbsorbManager.syncTorturePositionS(this.PublishM, this.TargetM, this.TargetM.getPhysic(), pose_title, ref num, ref num2, this.use_torture == 2))
				{
					return true;
				}
			}
			return false;
		}

		public static bool syncTorturePositionS(M2Attackable PublishM, M2Attackable TargetM, M2Phys PhyT, string post_title, ref float dx, ref float dy, bool opposite_aim = false)
		{
			ITortureListener tortureListener = TargetM as ITortureListener;
			M2PxlAnimator m2PxlAnimator = null;
			M2PxlAnimator m2PxlAnimator2 = null;
			ITeScaler teScaler = null;
			ITeScaler teScaler2 = null;
			float num = 0f;
			float num2 = 0f;
			if (tortureListener == null)
			{
				X.de("TargetM は TortureListenerではない", null);
				return false;
			}
			Bench.P("PtGet-Publish");
			PxlLayer[] array = PublishM.SpGetPointsData(ref m2PxlAnimator2, ref teScaler2, ref num2);
			Bench.Pend("PtGet-Publish");
			if (array == null || m2PxlAnimator2 == null)
			{
				return false;
			}
			Bench.P("setAim-Target");
			TargetM.setAim(CAim.get_aim2(0f, 0f, (float)(CAim._XD(PublishM.aim, 1) * X.MPF(!opposite_aim)), (float)CAim._YD(PublishM.aim, 1), false), true);
			Bench.Pend("setAim-Target");
			Bench.P("setTortureAnimation-Target");
			bool flag = tortureListener.setTortureAnimation(post_title, m2PxlAnimator2.cframe, m2PxlAnimator2.getCurrentSequence().loop_to);
			Bench.Pend("setTortureAnimation-Target");
			if (!flag)
			{
				X.de("torture ポーズの設定に失敗しました", null);
				return false;
			}
			Bench.P("PtGet-Target");
			PxlLayer[] array2 = TargetM.SpGetPointsData(ref m2PxlAnimator, ref teScaler, ref num);
			Bench.Pend("PtGet-Target");
			if (array2 == null || m2PxlAnimator == null)
			{
				X.de("ターゲット側に torture のための正しい PointData が指定されていない", null);
				return false;
			}
			Map2d mp = PublishM.Mp;
			TargetM.transform.localRotation = PublishM.transform.localRotation;
			int num3 = array2.Length;
			int num4 = array.Length;
			m2PxlAnimator.setShiftTe(m2PxlAnimator2.getShiftTe());
			Vector2 vector = teScaler2.getScaleTe() * m2PxlAnimator2.ViewScale;
			Vector2 vector2 = ((teScaler != null) ? teScaler.getScaleTe() : Vector2.one) * m2PxlAnimator.ViewScale;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < num3; j++)
				{
					PxlLayer pxlLayer = array2[j];
					for (int k = 0; k < num4; k++)
					{
						PxlLayer pxlLayer2 = array[k];
						if (i == 1 || pxlLayer2.name == pxlLayer.name)
						{
							Bench.P("Positioning");
							Vector2 vector3 = new Vector2((pxlLayer2.x + m2PxlAnimator2.offsetPixelX + PublishM.getSpShiftX() * mp.base_scale) * vector.x, (-pxlLayer2.y + m2PxlAnimator2.offsetPixelY + PublishM.getSpShiftY() * mp.base_scale) * vector.y);
							vector3 = X.ROTV2e(vector3, num2);
							M2Phys physic = PublishM.getPhysic();
							float num5 = mp.map2meshx(physic.move_depert_tstack_x) + vector3.x / mp.base_scale;
							float num6 = mp.map2meshy(physic.move_depert_tstack_y) + vector3.y / mp.base_scale;
							Vector2 vector4 = new Vector2((pxlLayer.x + m2PxlAnimator.offsetPixelX + TargetM.getSpShiftX() * mp.base_scale) * vector2.x, (-pxlLayer.y + m2PxlAnimator.offsetPixelY + TargetM.getSpShiftY() * mp.base_scale) * vector2.y);
							vector4 = X.ROTV2e(vector4, num);
							float num7 = mp.map2meshx(PhyT.move_depert_tstack_x) + vector4.x / mp.base_scale;
							float num8 = mp.map2meshy(PhyT.move_depert_tstack_y) + vector4.y / mp.base_scale;
							dx = (num5 - num7) * mp.rCLEN;
							dy = -(num6 - num8) * mp.rCLEN;
							tortureListener.setToTortureFix(PhyT.move_depert_tstack_x + dx, PhyT.move_depert_tstack_y + dy);
							Bench.Pend("Positioning");
							return true;
						}
					}
				}
			}
			return false;
		}

		public void resize(float _x, float _y)
		{
			if (this.use_torture > 0)
			{
				return;
			}
			this.PosP.x = this.PosP.x * _x;
			this.PosP.y = this.PosP.y * _y;
			this.PosP0.x = this.PosP0.x * _x;
			this.PosP0.y = this.PosP0.y * _y;
		}

		public void walkPublishPos(float tcnt, float maxt)
		{
			if (this.PublishM.hasFoot())
			{
				this.PublishM.initJump();
			}
			if (this.Target is PR && !this.abm_pose_fixed)
			{
				PR pr = this.Target as PR;
				AbsorbManager.ABM_POSE abm_POSE = (pr.isPoseDown(false) ? AbsorbManager.ABM_POSE.DOWN : (pr.isPoseCrouch(false) ? AbsorbManager.ABM_POSE.CROUCH : AbsorbManager.ABM_POSE.STAND));
				this.setAbmPose(abm_POSE, false);
			}
			M2Phys physic = this.PublishM.getPhysic();
			M2Phys physic2 = this.TargetM.getPhysic();
			float num = this.PublishM.drawx_tstack * this.Mp.rCLEN;
			float num2 = this.PublishM.drawy_tstack * this.Mp.rCLEN;
			float num3 = ((this.kiss_af >= maxt) ? tcnt : (tcnt / (1f + maxt - this.kiss_af)));
			float num4 = X.absMn((this.TargetM.drawx_tstack * this.Mp.rCLEN + this.PosP.x - num) * num3, 0.4f);
			float num5 = X.absMn((this.TargetM.drawy_tstack * this.Mp.rCLEN + this.PosP.y - num2) * num3, 0.4f);
			float num6 = num4;
			float num7 = num5;
			this.PublishM.getMovableLen(ref num6, ref num7, -0.1f, false, false);
			physic.addTranslateStack(num6, num7);
			physic.addLockWallHitting(this, 5f);
			if (this.kiss_af < maxt)
			{
				this.kiss_af += tcnt;
			}
			if (!this.Con.cannot_move)
			{
				float num8 = num4 - num6;
				float num9 = num5 - num7;
				physic2.addTranslateStack(-num8 * 0.5f, -num9 * 0.5f);
			}
		}

		public AbsorbManager.ABM_POSE abm_pose
		{
			get
			{
				return this.is_down_;
			}
		}

		public bool abm_pose_fixed
		{
			get
			{
				return (this.is_down_ & AbsorbManager.ABM_POSE._FIXED) > AbsorbManager.ABM_POSE.STAND;
			}
		}

		public void setAbmPose(AbsorbManager.ABM_POSE p, bool fixing = false)
		{
			if (!this.isActive() || (this.is_down_ & (AbsorbManager.ABM_POSE)767) == p)
			{
				return;
			}
			this.is_down_ = (p | (fixing ? AbsorbManager.ABM_POSE._FIXED : AbsorbManager.ABM_POSE.STAND)) & (AbsorbManager.ABM_POSE)(-513);
			if (p != AbsorbManager.ABM_POSE.DOWN)
			{
				if (p != AbsorbManager.ABM_POSE.CROUCH)
				{
					this.PublishKiss.Set(-20f / this.Mp.CLENB, -6f / this.Mp.CLENB, 40f / this.Mp.CLENB, 22f / this.Mp.CLENB);
				}
				else
				{
					this.PublishKiss.Set(-15f / this.Mp.CLENB, -15f / this.Mp.CLENB, 30f / this.Mp.CLENB, 30f / this.Mp.CLENB);
				}
			}
			else
			{
				this.PublishKiss.Set(-20f / this.Mp.CLENB, -6f / this.Mp.CLENB, 40f / this.Mp.CLENB, 22f / this.Mp.CLENB);
			}
			this.PosP = this.randomisePos(11f / this.Mp.CLENB);
			this.kiss_af = X.Mn(this.kiss_af, 4f);
		}

		public Vector2 randomisePos(float margin)
		{
			Vector2 vector = default(Vector2);
			float num = margin * 2f;
			uint ran = X.GETRAN2(this.id, this.id & 63);
			if (num >= this.PublishKiss.width)
			{
				vector.x = this.PublishKiss.x + this.PublishKiss.width * 0.5f;
			}
			else
			{
				vector.x = this.PublishKiss.x + margin + X.RAN(ran, 2329) * (this.PublishKiss.width - num);
			}
			if (num >= this.PublishKiss.height)
			{
				vector.y = this.PublishKiss.y + this.PublishKiss.height * 0.5f;
			}
			else
			{
				vector.y = this.PublishKiss.y + margin + X.RAN(ran, 2609) * (this.PublishKiss.height - num);
			}
			if (X.Abs(this.PosT0.x - this.TargetM.x) >= 1.5f)
			{
				this.DEBUG();
			}
			return vector;
		}

		public bool use_cam_zoom2
		{
			get
			{
				return this.use_cam_zoom2_;
			}
			set
			{
				if (this.use_cam_zoom2_ == value)
				{
					return;
				}
				this.use_cam_zoom2_ = value;
				if (this.use_cam_zoom2_)
				{
					this.Con.FlgCamZoom.Add(this);
					return;
				}
				this.Con.FlgCamZoom.Rem(this);
			}
		}

		public int getPublishWeight()
		{
			if (this.Publish == null)
			{
				return 0;
			}
			return this.Publish.getAbsorbWeight();
		}

		public float getPublishXD()
		{
			return (float)X.MPF(this.PosP.x > 0f);
		}

		public float getPublish0XD()
		{
			return (float)X.MPF(this.PosP0.x > 0f);
		}

		public Vector2 getPosT0()
		{
			return this.PosT0;
		}

		public Map2d Mp
		{
			get
			{
				return this.Con.Mv.Mp;
			}
		}

		public PrGachaItem get_Gacha()
		{
			return this.Gacha;
		}

		public M2Attackable getPublishMover()
		{
			return this.PublishM;
		}

		public M2Attackable getTargetMover()
		{
			return this.TargetM;
		}

		public string uipicture_fade_key
		{
			get
			{
				return this.uipicture_fade_key_;
			}
			set
			{
				this.uipicture_fade_key_ = value;
				this.Con.uipicture_fade_key = null;
			}
		}

		public UIPictureBase.EMSTATE emstate_attach
		{
			get
			{
				return this.emstate_attach_;
			}
			set
			{
				if (value == this.emstate_attach_)
				{
					return;
				}
				this.emstate_attach_ = value;
				this.Con.emstate_attach = (UIPictureBase.EMSTATE)(-1);
				PR pr = this.Con.Mv as PR;
				if (pr != null)
				{
					pr.recheck_emot = true;
				}
			}
		}

		public string breath_key
		{
			get
			{
				return this.breath_key_;
			}
			set
			{
				if (this.breath_key_ == value)
				{
					return;
				}
				this.breath_key_ = value;
				this.Con.fine_breath_key = true;
			}
		}

		public bool no_clamp_speed
		{
			get
			{
				return this.no_clamp_speed_;
			}
			set
			{
				this.no_clamp_speed_ = value;
				this.Con.no_clamp_speed = value;
			}
		}

		public bool cannot_move
		{
			get
			{
				return this.cannot_move_;
			}
			set
			{
				this.cannot_move_ = value;
				this.Con.cannot_move = value;
			}
		}

		public bool mouth_is_covered
		{
			get
			{
				return this.mouth_is_covered_;
			}
			set
			{
				this.mouth_is_covered_ = value;
				this.Con.mouth_is_covered = value;
			}
		}

		public bool no_ser_burned_effect
		{
			get
			{
				return this.no_ser_burned_effect_;
			}
			set
			{
				this.no_ser_burned_effect_ = value;
				this.Con.no_ser_burned_effect = value;
			}
		}

		public bool no_change_release_in_dead
		{
			get
			{
				return this.no_change_release_in_dead_;
			}
			set
			{
				this.no_change_release_in_dead_ = value;
				if (value)
				{
					this.Con.no_change_release_in_dead = value;
				}
			}
		}

		public bool no_shuffleframe_on_applydamage
		{
			get
			{
				return this.no_shuffleframe_on_applydamage_;
			}
			set
			{
				this.no_shuffleframe_on_applydamage_ = value;
				if (value)
				{
					this.Con.no_shuffleframe_on_applydamage = value;
				}
			}
		}

		public bool no_shuffle_aim
		{
			get
			{
				return this.no_shuffle_aim_;
			}
			set
			{
				this.no_shuffle_aim_ = value;
				if (value)
				{
					this.Con.no_shuffle_aim = value;
				}
			}
		}

		public bool cannot_apply_mist_damage
		{
			get
			{
				return this.cannot_apply_mist_damage_;
			}
			set
			{
				this.cannot_apply_mist_damage_ = value;
				if (value)
				{
					this.Con.cannot_apply_mist_damage = value;
				}
			}
		}

		public float normal_UP_fade_injectable
		{
			get
			{
				return this.normal_UP_fade_injectable_;
			}
			set
			{
				this.normal_UP_fade_injectable_ = value;
				this.Con.normal_UP_fade_injectable = -1f;
			}
		}

		public int pose_priority
		{
			get
			{
				return this.pose_priority_;
			}
			set
			{
				this.Con.finePosePriority(this.pose_priority_);
				this.pose_priority_ = value;
			}
		}

		public string target_pose
		{
			get
			{
				return this.target_pose_;
			}
			set
			{
				this.target_pose_ = value;
				this.Con.finePosePriority(this.pose_priority_);
			}
		}

		public bool isKeyPD(string k, bool break_flag = true, bool inputted_flag = true)
		{
			KEY.SIMKEY simkey = M2MoverPr.key2SIMKEY(k);
			if (simkey == (KEY.SIMKEY)0)
			{
				return this.Con.MvPr != null && Map2d.can_handle && IN.isKeyPD(k);
			}
			if ((this.Con.gacha_in_pd & simkey) != (KEY.SIMKEY)0)
			{
				if (break_flag)
				{
					this.Con.gacha_in_pd &= ~simkey;
					if (inputted_flag)
					{
						this.Con.gachaInput(true);
					}
				}
				return true;
			}
			return false;
		}

		public bool isKeyPO(string k, bool break_flag = true, bool inputted_flag = true)
		{
			KEY.SIMKEY simkey = M2MoverPr.key2SIMKEY(k);
			if (simkey == (KEY.SIMKEY)0)
			{
				return this.Con.MvPr != null && Map2d.can_handle && IN.isKeyO(k, 0);
			}
			if ((this.Con.gacha_in_o & simkey) != (KEY.SIMKEY)0)
			{
				if (break_flag)
				{
					this.Con.gacha_in_o &= ~simkey;
					if (inputted_flag)
					{
						this.Con.gachaInput(true);
					}
				}
				return true;
			}
			return false;
		}

		public void KeyIsSafe(string k)
		{
			this.Con.safe_key |= M2MoverPr.key2SIMKEY(k);
		}

		public bool isTortureUsing()
		{
			return this.use_torture > 0;
		}

		public bool isNotTortureEnable()
		{
			return this.Con.countTortureItem(this.FD_notTortureEnable, false) == 0;
		}

		private bool notTortureEnable(AbsorbManager Abm)
		{
			return Abm != this && Abm.pose_priority_ >= this.pose_priority_;
		}

		public readonly AbsorbManagerContainer Con;

		private NelM2Attacker Target;

		private NelM2Attacker Publish;

		private M2Attackable TargetM;

		private M2Attackable PublishM;

		private PrGachaItem Gacha;

		public IGachaListener Listener;

		public int id;

		public Vector2 PosT;

		private Vector2 PosT0;

		public Vector2 PosP;

		private Vector2 PosP0;

		private Vector2 PosP0_Map;

		public Rect PublishKiss;

		private AbsorbManager.ABM_POSE is_down_;

		private bool need_translate_first;

		private byte use_torture;

		public bool publish_float;

		public bool penetrate_decline;

		public bool gacha_releaseable = true;

		public bool release_from_publish_count;

		private bool use_cam_zoom2_;

		private string uipicture_fade_key_ = "";

		private float normal_UP_fade_injectable_;

		private bool no_ser_burned_effect_;

		private bool no_shuffle_aim_;

		private bool no_change_release_in_dead_;

		private bool no_shuffleframe_on_applydamage_;

		private bool mouth_is_covered_;

		public bool kirimomi_release;

		private bool cannot_apply_mist_damage_;

		private bool no_clamp_speed_;

		private bool cannot_move_;

		public float kiss_af;

		private string breath_key_;

		private UIPictureBase.EMSTATE emstate_attach_;

		private int pose_priority_;

		private string target_pose_;

		private Func<AbsorbManager, bool> FD_notTortureEnable;

		public const int POSEPRI_TORTURE = 99;

		public const int POSEPRI_BOSS = 60;

		public const int POSEPRI_VORE = 40;

		public const int POSEPRI_BIG = 20;

		public const int POSEPRI_NORMAL = 1;

		public enum ABM_POSE
		{
			STAND,
			DOWN,
			CROUCH,
			_POSE_TYPE = 255,
			_FIXED,
			_NOT_INITTED = 512
		}
	}
}

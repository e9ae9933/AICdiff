using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class AbsorbManagerContainer : RBase<AbsorbManager>, IEventWaitListener
	{
		public AbsorbManagerContainer(int cnt, M2Mover _Mv)
			: base(cnt, true, false, false)
		{
			this.Achecked = new NelM2Attacker[cnt];
			this.ASpEvent = new List<IGachaListener>(2);
			this.Mv = _Mv;
			if (this.Mv is M2MoverPr)
			{
				this.MvPr = this.Mv as M2MoverPr;
			}
			this.OMoveReserve = new BDic<M2Attackable, List<Vector3>>();
			this.FlgCamZoom = new FlaggerC<AbsorbManager>(delegate(FlaggerC<AbsorbManager> V)
			{
				if (this.EfCamZoom == null)
				{
					this.EfCamZoom = PostEffect.IT.setPEfadeinout(POSTM.ZOOM2, 70f, -1f, 1f, 0);
				}
			}, delegate(FlaggerC<AbsorbManager> V)
			{
				if (this.EfCamZoom != null)
				{
					this.EfCamZoom.deactivate(false);
					this.EfCamZoom = null;
				}
			});
			this.FD_fnDrawGachaB = (EffectItem Ef) => this.fnDrawGachaB(Ef, false);
			this.FD_fnDrawGachaBEv = (EffectItem Ef) => this.fnDrawGachaB(Ef, true);
		}

		public void removeListener(IGachaListener Listener)
		{
			this.ASpEvent.Remove(Listener);
		}

		public void releaseIndividualSpEvent()
		{
			if (this.ASpEvent.Count > 0)
			{
				int num = this.LEN;
				for (int i = this.ASpEvent.Count - 1; i >= 0; i--)
				{
					IGachaListener gachaListener = this.ASpEvent[i];
					if (gachaListener.individual)
					{
						AbsorbManager absorbManager = this.Get(gachaListener);
						if (absorbManager != null)
						{
							absorbManager.destruct();
							num--;
						}
					}
				}
				if (num == 0)
				{
					this.clear();
				}
			}
		}

		public AbsorbManager GetOrPop(NelM2Attacker Target, NelM2Attacker Publ, ref bool first, bool penetrate_absorb)
		{
			int num = 0;
			this.total_weight_ = -1f;
			if (this.LEN == 0)
			{
				first = true;
				this.release_type = AbsorbManagerContainer.RELEASE_TYPE.NORMAL;
			}
			int absorbWeight = Publ.getAbsorbWeight();
			AbsorbManager absorbManager = null;
			for (int i = 0; i < this.LEN; i++)
			{
				AbsorbManager absorbManager2 = this.AItems[i];
				if (absorbManager2.isActive(Target, Publ, false))
				{
					return null;
				}
				if (!absorbManager2.isActive() && absorbManager2.release_from_publish_count)
				{
					absorbManager = absorbManager2;
				}
				if (absorbManager2.penetrate_decline)
				{
					penetrate_absorb = false;
				}
				num += absorbManager2.getPublishWeight();
			}
			if (num + absorbWeight > this.AItems.Length || this.AItems.Length <= this.LEN)
			{
				int i = 0;
				absorbManager = null;
				while (i < this.LEN && (num + absorbWeight > this.AItems.Length || this.AItems.Length <= this.LEN))
				{
					AbsorbManager absorbManager3 = this.AItems[i];
					int publishWeight = absorbManager3.getPublishWeight();
					if (!penetrate_absorb && absorbManager3.checkPublisher(Target))
					{
						i++;
					}
					else if (publishWeight < absorbWeight || penetrate_absorb)
					{
						base.clearAt(i);
						num -= publishWeight;
					}
					else
					{
						if (!absorbManager3.isActive() && absorbManager3.release_from_publish_count)
						{
							absorbManager = absorbManager3;
						}
						i++;
					}
				}
				if (this.LEN <= 0)
				{
					this.add_cnt = 0;
					first = true;
				}
			}
			if ((this.AItems.Length <= this.LEN && absorbManager == null) || (this.MvPr != null && this.MvPr.is_alive && absorbWeight == 1 && this.add_cnt > this.AItems.Length) || num + absorbWeight > this.AItems.Length)
			{
				return null;
			}
			AbsorbManager absorbManager4 = absorbManager ?? base.Pop(64);
			AbsorbManager absorbManager5 = absorbManager4;
			int num2 = this.items_id;
			this.items_id = num2 + 1;
			if (absorbManager5.InitTP(Target, Publ, num2))
			{
				if (first)
				{
					this.current_pose_priority_ = 0;
				}
				return absorbManager4;
			}
			return null;
		}

		public void countAdd(NelM2Attacker Publish)
		{
			X.pushToEmptyS<NelM2Attacker>(ref this.Achecked, Publish, ref this.add_cnt, 4);
		}

		public void releaseFromCounter(NelM2Attacker Publish)
		{
			int num = X.isinC<NelM2Attacker>(this.Achecked, Publish, this.add_cnt);
			if (num >= 0)
			{
				X.shiftEmpty<NelM2Attacker>(this.Achecked, 1, num, -1);
				this.add_cnt--;
			}
		}

		public AbsorbManagerContainer clearTextInstance()
		{
			if (this.AItems == null)
			{
				return this;
			}
			int num = this.AItems.Length;
			for (int i = 0; i < num; i++)
			{
				AbsorbManager absorbManager = this.AItems[i];
				if (absorbManager != null)
				{
					absorbManager.get_Gacha().clearTextInstance(this.Mv);
				}
			}
			return this;
		}

		public override void clear()
		{
			if (this.EfCamZoom != null)
			{
				this.EfCamZoom.deactivate(false);
				this.EfCamZoom = null;
			}
			if (this.AItems == null)
			{
				return;
			}
			for (int i = 0; i < this.LEN; i++)
			{
				try
				{
					this.AItems[i].destruct();
				}
				catch
				{
				}
			}
			base.clear();
			this.has_gacha_lrtb = false;
			this.renderable_on_evt_stop_ghandle = false;
			this.kirimomi_release_dir_last_ = this.kirimomi_release_dir;
			this.kirimomi_release_dir = -1;
			this.no_error_on_miss_input = false;
			this.use_torture_ = 0;
			this.cannot_move_ = (this.mouth_is_covered_ = 0);
			this.no_ser_burned_effect_ = 0;
			this.current_pose_priority_ = 0;
			this.no_clamp_speed_ = (this.no_shuffleframe_on_applydamage_ = (this.no_shuffle_aim_ = (this.cannot_apply_mist_damage_ = 0)));
			this.total_weight_ = 0f;
			this.emstate_attach_ = UIPictureBase.EMSTATE.NORMAL;
			this.breath_key = null;
			this.fine_breath_key = false;
			this.timeout = 100;
			this.normal_UP_fade_injectable_ = -1f;
			this.gacha_inputted_t = -2f;
			this.uipicture_fade_key_ = null;
			this.ev_assign = false;
			this.no_change_release_in_dead = false;
			this.FlgCamZoom.Clear();
			this.OMoveReserve.Clear();
			this.need_fine_gacha_effect = true;
			this.ASpEvent.Clear();
			this.releaseCameraBinder();
			this.add_cnt = 0;
		}

		private void releaseCameraBinder()
		{
			if (this.CbGachaT != null)
			{
				IN.getGUICamera();
				CameraBidingsBehaviour.UiBind.deassignPostRenderFunc(this.CbGachaT);
			}
			if (this.EfGachaB != null)
			{
				this.EfGachaB.destruct();
			}
			this.EfGachaB = null;
			this.CbGachaT = null;
		}

		public bool checkTarget(NelM2Attacker _Target)
		{
			int num = 0;
			for (int i = 0; i < this.LEN; i++)
			{
				AbsorbManager absorbManager = this.AItems[i];
				if (!absorbManager.checkTarget(_Target))
				{
					if (absorbManager.isActive())
					{
						absorbManager.destruct();
					}
					num++;
				}
				else
				{
					num += (absorbManager.isActive() ? 0 : 1);
				}
			}
			return num < this.LEN;
		}

		public bool hasTarget(NelM2Attacker _Target)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				if (this.AItems[i].checkTarget(_Target))
				{
					return true;
				}
			}
			return false;
		}

		public bool hasPublisher(NelM2Attacker _Publ)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				if (this.AItems[i].checkPublisher(_Publ))
				{
					return true;
				}
			}
			return false;
		}

		public bool releaseFromTarget(NelM2Attacker Target)
		{
			int num = 0;
			for (int i = 0; i < this.LEN; i++)
			{
				num += (this.AItems[i].releaseFromTarget(Target) ? 1 : 0);
			}
			if (num >= this.LEN)
			{
				this.clear();
				return true;
			}
			return num >= this.LEN;
		}

		public void releaseAll()
		{
			this.clearTextInstance();
			this.clear();
		}

		public bool runAbsorbPr(PR _Target, float t, float fcnt)
		{
			Bench.P("runAbsorb");
			bool flag = this.runAbsorbPrInner(_Target, t, fcnt);
			Bench.Pend("runAbsorb");
			return flag;
		}

		private bool runAbsorbPrInner(PR _Target, float t, float fcnt)
		{
			if (this.LEN == 0)
			{
				if (this.EfCamZoom != null)
				{
					this.clear();
				}
				else
				{
					this.add_cnt = 0;
				}
				return false;
			}
			this.gacha_renderable = true;
			this.release_type = AbsorbManagerContainer.RELEASE_TYPE.NORMAL;
			AbsorbManager absorbManager = null;
			if (this.MvPr != null && this.Mv.Mp.Pr == this.MvPr)
			{
				this.gacha_in_pd = this.MvPr.getKeyPDState();
				this.gacha_in_o = this.MvPr.getKeyPOState();
				if (this.CbGachaT != null)
				{
					this.CbGachaT.need_redraw = true;
				}
			}
			else
			{
				this.gacha_in_pd = (this.gacha_in_o = (KEY.SIMKEY)0);
				this.releaseCameraBinder();
			}
			this.safe_key = (KEY.SIMKEY)0;
			KEY.SIMKEY simkey = this.gacha_in_pd;
			KEY.SIMKEY simkey2 = this.gacha_in_o;
			this.gacha_active_count = false;
			if (this.fine_breath_key)
			{
				this.breath_key = null;
			}
			int num = 0;
			int num2 = this.current_pose_priority_;
			string text = null;
			for (int i = 0; i < this.LEN; i++)
			{
				AbsorbManager absorbManager2 = this.AItems[i];
				if (!absorbManager2.checkTarget(_Target))
				{
					if (absorbManager2.isActive())
					{
						absorbManager2.destruct();
					}
				}
				else
				{
					this.release_type |= (absorbManager2.kirimomi_release ? AbsorbManagerContainer.RELEASE_TYPE.KIRIMOMI : AbsorbManagerContainer.RELEASE_TYPE.NORMAL);
					string text2 = Bench.P(absorbManager2.getPublishMover().key);
					absorbManager2.fineFirstPos(absorbManager).run(fcnt);
					if (absorbManager == null)
					{
						absorbManager = absorbManager2;
					}
					if (num2 < 99)
					{
						if (absorbManager2.isTortureUsing())
						{
							num2 = (this.current_pose_priority_ = 99);
							text = null;
						}
						else if (absorbManager2.target_pose != null && num2 < absorbManager2.pose_priority)
						{
							text = absorbManager2.target_pose;
							num2 = absorbManager2.pose_priority;
						}
					}
					if (this.fine_breath_key)
					{
						int publishWeight = absorbManager2.getPublishWeight();
						if (publishWeight > num && absorbManager2.breath_key != null)
						{
							num = publishWeight;
							this.breath_key = absorbManager2.breath_key;
						}
					}
					Bench.Pend(text2);
				}
			}
			if (this.fine_breath_key)
			{
				this.fine_breath_key = false;
				if (this.MvPr is PR)
				{
					(this.MvPr as PR).EpCon.breath_key = this.breath_key;
				}
			}
			if (this.gacha_active_count)
			{
				if (this.gacha_inputted_t == -1f || (!this.no_error_on_miss_input && !DIFF.gacha_failure_decline && this.EfGachaB != null && (this.gacha_inputted_t != -2f || this.EfGachaB.af > 40f) && (this.gacha_in_pd & ~this.safe_key) > (KEY.SIMKEY)0))
				{
					for (int j = 0; j < this.LEN; j++)
					{
						this.AItems[j].get_Gacha().ErrorOccured(false, !DIFF.gacha_failure_no_reset);
					}
					this.gacha_inputted_t = -2f;
					if (this.EfGachaB != null)
					{
						this.EfGachaB.af = X.Mx(this.EfGachaB.af, 40f);
					}
				}
				else if (this.timeout > 0 && this.gacha_inputted_t >= 0f)
				{
					float num3 = this.gacha_inputted_t;
					this.gacha_inputted_t = num3 + 1f;
					if (num3 >= (float)this.timeout)
					{
						for (int k = 0; k < this.LEN; k++)
						{
							this.AItems[k].get_Gacha().ErrorOccured(true, true);
						}
						this.gacha_inputted_t = -2f;
					}
				}
			}
			else
			{
				this.gacha_inputted_t = -2f;
			}
			if (text != null)
			{
				this.current_pose_priority_ = X.Mx(this.current_pose_priority_, num2);
				if (this.MvPr != null)
				{
					this.MvPr.setAbsorbAnimation(text);
				}
				else
				{
					this.Mv.SpSetPose(text, -1, null, false);
				}
			}
			if (this.need_fine_gacha_release)
			{
				this.need_fine_gacha_release = false;
				if (this.CbGachaT != null)
				{
					bool flag = true;
					bool flag2 = false;
					for (int l = 0; l < this.LEN; l++)
					{
						AbsorbManager absorbManager3 = this.AItems[l];
						if (absorbManager3.get_Gacha().isUseable())
						{
							flag2 = true;
							if (!absorbManager3.gacha_releaseable)
							{
								flag = false;
								break;
							}
						}
					}
					if (flag && flag2)
					{
						for (int m = this.LEN - 1; m >= 0; m--)
						{
							AbsorbManager absorbManager4 = this.AItems[m];
							if (absorbManager4.get_Gacha().isUseable())
							{
								absorbManager4.get_Gacha().releaseEffect();
								absorbManager4.destruct();
							}
						}
						this.release_type |= AbsorbManagerContainer.RELEASE_TYPE.GACHA;
						this.need_fine_gacha_release = false;
					}
				}
			}
			if (!this.checkTarget(_Target) || !this.isActive())
			{
				this.add_cnt = 0;
				this.need_fine_gacha_effect = true;
				if (this.release_type == AbsorbManagerContainer.RELEASE_TYPE.NORMAL && !this.no_change_release_in_dead && !_Target.is_alive && X.XORSP() < 0.4f)
				{
					this.release_type = AbsorbManagerContainer.RELEASE_TYPE.KIRIMOMI;
				}
				this.clear();
				this.Mv.endDrawAssist(1);
				return false;
			}
			return true;
		}

		public void resetKissAf()
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.AItems[i].resetKissAf();
			}
		}

		public bool appliedTortureEmotion(string emo)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				if (this.AItems[i].uipicture_fade_key == emo)
				{
					return true;
				}
			}
			return false;
		}

		public bool isAlreadyAbsorbed(NelM2Attacker Publish)
		{
			for (int i = 0; i < this.add_cnt; i++)
			{
				if (this.Achecked[i] == Publish)
				{
					return true;
				}
			}
			return false;
		}

		public override bool run(float fcnt)
		{
			if (this.LEN == 0)
			{
				this.need_fine_gacha_release = false;
				if (this.need_fine_gacha_effect)
				{
					this.need_fine_gacha_effect = false;
					this.releaseCameraBinder();
				}
				return false;
			}
			if (this.Mv.vx != 0f || this.Mv.vy != 0f)
			{
				for (int i = 0; i < this.LEN; i++)
				{
					AbsorbManager absorbManager = this.AItems[i];
					if (!absorbManager.isTortureUsing())
					{
						absorbManager.translateTarget(absorbManager.getTargetMover() as NelM2Attacker, this.Mv.vx, this.Mv.vy);
					}
				}
				M2Phys physic = this.Mv.getPhysic();
				if (physic != null && !this.no_clamp_speed)
				{
					physic.clampSpeed(FOCTYPE.HIT, X.Abs(this.Mv.vx) * 0.6f, (this.Mv.vy < 0f) ? (X.Abs(this.Mv.vy) * 0.6f) : (physic.ySpeedMax * 0.4f), 0.02f);
				}
			}
			if (this.EfCamZoom != null)
			{
				this.EfCamZoom.fine(120);
			}
			this.attachBinder();
			return true;
		}

		private bool attachBinder()
		{
			if (this.need_fine_gacha_effect)
			{
				this.need_fine_gacha_effect = false;
				if (PrGachaItem.checkDrawPoints(this.Mv, this.AItems, this.LEN))
				{
					if (this.CbGachaT == null)
					{
						this.CbGachaT = new CameraRenderBinderGachaItem(this, false);
						if (!this.ev_assign)
						{
							this.EfGachaB = this.Mv.Mp.getEffect().setEffectWithSpecificFn("gachaB", 0f, 0f, 0f, 0, 0, this.FD_fnDrawGachaB);
						}
						else
						{
							this.EfGachaB = UIBase.Instance.getEffect().setEffectWithSpecificFn("_gacha_ev_attach", 0f, 0f, 0f, 0, 0, this.FD_fnDrawGachaBEv);
						}
						IN.getGUICamera();
						CameraBidingsBehaviour.UiBind.assignPostRenderFunc(this.CbGachaT);
						return true;
					}
					if (this.ev_assign && this.EfGachaB != null && this.EfGachaB.FnDef == this.FD_fnDrawGachaB)
					{
						X.dl("event のためのBinderアサインのため、Binderを貼り直します", null, false, false);
						this.releaseCameraBinder();
						this.need_fine_gacha_effect = true;
						return this.attachBinder();
					}
				}
				else
				{
					this.releaseCameraBinder();
				}
			}
			return false;
		}

		public override void destruct()
		{
			base.destruct();
		}

		public void resize(float _x, float _y)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.AItems[i].resize(_x, _y);
			}
		}

		public void initDeath()
		{
			for (int i = 0; i < this.LEN; i++)
			{
				PrGachaItem gacha = this.AItems[i].get_Gacha();
				if (gacha.isActive())
				{
					gacha.releaseEffect();
					gacha.activate(PrGachaItem.TYPE.CANNOT_RELEASE, 0, 63U);
				}
			}
		}

		public void reserveMoveBy(M2Attackable Mv, float dx, float dy, float weight)
		{
			if (Mv == null || (dx == 0f && dy == 0f))
			{
				return;
			}
			List<Vector3> list = X.Get<M2Attackable, List<Vector3>>(this.OMoveReserve, Mv);
			if (list == null)
			{
				list = (this.OMoveReserve[Mv] = new List<Vector3>(1));
			}
			list.Add(new Vector3(dx, dy, weight));
		}

		public void removeMoveByVector(M2Attackable Mv)
		{
			if (Mv == null)
			{
				return;
			}
			this.OMoveReserve.Remove(Mv);
		}

		public bool CorruptGacha(float level)
		{
			bool flag = false;
			for (int i = 0; i < this.LEN; i++)
			{
				flag = this.AItems[i].get_Gacha().CorruptGacha(level) || flag;
			}
			return flag;
		}

		public AbsorbManager initSpecialGacha(NelM2Attacker _Mv, IGachaListener Lsn, PrGachaItem.TYPE type, int tap_count, KEY.SIMKEY simkey = KEY.SIMKEY.Z, bool tap_count_abs = true)
		{
			return this.initSpecialGacha(_Mv, Lsn, type, tap_count, false, simkey, tap_count_abs);
		}

		public AbsorbManager initSpecialGachaNotDiffFix(NelM2Attacker _Mv, IGachaListener Lsn, PrGachaItem.TYPE type, int tap_count, KEY.SIMKEY simkey = KEY.SIMKEY.Z, bool tap_count_abs = true)
		{
			return this.initSpecialGacha(_Mv, Lsn, type, tap_count, true, simkey, tap_count_abs);
		}

		private AbsorbManager initSpecialGacha(NelM2Attacker _Mv, IGachaListener Lsn, PrGachaItem.TYPE type, int tap_count, bool do_not_fix_by_difficulty, KEY.SIMKEY simkey = KEY.SIMKEY.Z, bool tap_count_abs = true)
		{
			bool flag = false;
			if (Lsn.individual && this.ASpEvent.Count == 0 && this.LEN > 0)
			{
				this.releaseFromTarget(this.Mv as NelM2Attacker);
			}
			AbsorbManager orPop = this.GetOrPop(_Mv, _Mv, ref flag, false);
			if (orPop != null)
			{
				orPop.target_pose = null;
				orPop.Listener = Lsn;
				this.ASpEvent.Add(Lsn);
				PrGachaItem gacha = orPop.get_Gacha();
				if (!do_not_fix_by_difficulty)
				{
					gacha.activate(type, tap_count, (uint)simkey);
				}
				else
				{
					gacha.activateNotDiffFix(type, tap_count, (uint)simkey);
				}
				gacha.SoloPositionPixel = new Vector3(0f, -20f, 0f);
				if (tap_count_abs)
				{
					gacha.gacha_hit_abs = true;
				}
			}
			return orPop;
		}

		public void runPost()
		{
			if (this.ASpEvent.Count > 0 && this.LEN > 0)
			{
				bool flag = false;
				for (int i = this.ASpEvent.Count - 1; i >= 0; i--)
				{
					if (this.ASpEvent[i].canAbsorbContinue())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.clear();
				}
			}
			if (this.OMoveReserve.Count == 0)
			{
				return;
			}
			foreach (KeyValuePair<M2Attackable, List<Vector3>> keyValuePair in this.OMoveReserve)
			{
				List<Vector3> value = keyValuePair.Value;
				int count = value.Count;
				if (count != 0)
				{
					float num = 0f;
					float num2 = 0f;
					float num3 = 0f;
					for (int j = 0; j < count; j++)
					{
						Vector3 vector = value[j];
						num += vector.z;
						num2 += vector.x * vector.z;
						num3 += vector.y * vector.z;
					}
					if (num > 0f)
					{
						M2Phys physic = keyValuePair.Key.getPhysic();
						num2 /= num;
						num3 /= num;
						if (physic != null && (!(keyValuePair.Key is M2MoverPr) || X.LENGTHXYS(0f, 0f, num2, num3) >= 0.003f))
						{
							physic.addFoc(FOCTYPE.ABSORB | ((keyValuePair.Key is M2MoverPr) ? FOCTYPE._CHECK_WALL : ((FOCTYPE)0U)), num2 / num, num3 / num, -1f, -1, 1, 0, -1, 0);
						}
					}
					value.Clear();
				}
			}
		}

		public bool eventAbsorbBind(StringHolder rER)
		{
			if (this.LEN == 0)
			{
				return false;
			}
			PrGachaItem.TYPE type;
			if (!FEnum<PrGachaItem.TYPE>.TryParse(rER._1, out type, true))
			{
				rER.tError("1:TYPEが不明: " + rER._1);
				return false;
			}
			int num = rER.Int(2, 10);
			if (num <= 0)
			{
				rER.tError("2:countが0");
				return false;
			}
			for (int i = 0; i < this.LEN; i++)
			{
				PrGachaItem gacha = this.AItems[i].get_Gacha();
				if (gacha == null || !gacha.isUseable())
				{
					this.ev_assign = true;
					gacha.activate(type, num, 63U);
					if (rER.clength > 3)
					{
						Vector4 vector;
						if (!TalkDrawer.getDefinedPosition(rER._3, out vector))
						{
							X.de("フィックス先 vp_pos 不明:" + rER._3, null);
						}
						else
						{
							gacha.SoloPositionPixel = new Vector3(vector.x, vector.y, 1f);
						}
					}
					if (this.CbGachaT == null)
					{
						this.need_fine_gacha_effect = true;
						EV.initWaitFn(this, 0);
					}
					EV.addAllocEvHandleKey(KEY.SIMKEY._RANDOMISE, true);
					this.attachBinder();
					return true;
				}
			}
			return false;
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || this.LEN > 0;
		}

		private bool fnDrawGachaB(EffectItem Ef, bool event_bind)
		{
			if (this.LEN == 0)
			{
				return false;
			}
			if (!this.gacha_renderable || (!this.renderable_on_evt_stop_ghandle && EV.isStoppingGameHandle() && !EV.handle_randamise_key_enabled))
			{
				return true;
			}
			MeshDrawer meshDrawer = null;
			for (int i = 0; i < this.LEN; i++)
			{
				PrGachaItem gacha = this.AItems[i].get_Gacha();
				if (gacha != null && gacha.isUseable())
				{
					if (meshDrawer == null)
					{
						meshDrawer = Ef.GetMesh("", MTRX.MtrMeshNormal, !event_bind);
					}
					gacha.drawGachaB(meshDrawer, event_bind);
				}
			}
			return true;
		}

		public string uipicture_fade_key
		{
			get
			{
				if (this.LEN == 0)
				{
					this.uipicture_fade_key_ = null;
					return "";
				}
				if (this.uipicture_fade_key_ == null)
				{
					int num = 0;
					for (int i = 0; i < this.LEN; i++)
					{
						AbsorbManager absorbManager = this.AItems[i];
						if (absorbManager.uipicture_fade_key != "" && num < absorbManager.getPublishWeight())
						{
							this.uipicture_fade_key_ = absorbManager.uipicture_fade_key;
							num = absorbManager.getPublishWeight();
						}
					}
					if (this.uipicture_fade_key_ == null)
					{
						this.uipicture_fade_key_ = "";
					}
				}
				return this.uipicture_fade_key_;
			}
			set
			{
				this.uipicture_fade_key_ = null;
			}
		}

		public float total_weight
		{
			get
			{
				if (this.total_weight_ < 0f)
				{
					this.total_weight_ = 0f;
					for (int i = 0; i < this.LEN; i++)
					{
						AbsorbManager absorbManager = this.AItems[i];
						if (absorbManager.isActive())
						{
							this.total_weight_ += (float)absorbManager.getPublishWeight();
						}
					}
				}
				return this.total_weight_;
			}
			set
			{
				this.total_weight_ = -1f;
			}
		}

		public bool redraw_gacha_mesh
		{
			set
			{
				if (this.CbGachaT != null)
				{
					this.CbGachaT.need_redraw = true;
				}
			}
		}

		public bool use_torture
		{
			get
			{
				if (this.use_torture_ == 2)
				{
					this.use_torture_ = ((this.getTorturePublisher() != null) ? 1 : 0);
				}
				return this.use_torture_ > 0;
			}
			set
			{
				if (!value)
				{
					this.use_torture_ = 2;
					return;
				}
				this.use_torture_ = 1;
			}
		}

		public bool cannot_move
		{
			get
			{
				if (this.use_torture)
				{
					return true;
				}
				if (this.cannot_move_ == 2)
				{
					this.cannot_move_ = 0;
					for (int i = 0; i < this.LEN; i++)
					{
						if (this.AItems[i].cannot_move)
						{
							this.cannot_move_ = 1;
							break;
						}
					}
				}
				return this.cannot_move_ > 0;
			}
			set
			{
				if (!value)
				{
					this.cannot_move_ = 2;
					return;
				}
				this.cannot_move_ = 1;
			}
		}

		public bool mouth_is_covered
		{
			get
			{
				if (this.mouth_is_covered_ == 2)
				{
					this.mouth_is_covered_ = 0;
					for (int i = 0; i < this.LEN; i++)
					{
						if (this.AItems[i].mouth_is_covered)
						{
							this.mouth_is_covered_ = 1;
							break;
						}
					}
				}
				return this.mouth_is_covered_ > 0;
			}
			set
			{
				if (!value)
				{
					this.mouth_is_covered_ = 2;
					return;
				}
				this.mouth_is_covered_ = 1;
			}
		}

		public bool no_clamp_speed
		{
			get
			{
				if (this.no_clamp_speed_ == 2)
				{
					this.no_clamp_speed_ = 0;
					for (int i = 0; i < this.LEN; i++)
					{
						if (this.AItems[i].no_clamp_speed)
						{
							this.no_clamp_speed_ = 1;
							break;
						}
					}
				}
				return this.no_clamp_speed_ > 0;
			}
			set
			{
				if (!value)
				{
					this.no_clamp_speed_ = 2;
					return;
				}
				this.no_clamp_speed_ = 1;
			}
		}

		public bool no_ser_burned_effect
		{
			get
			{
				if (this.no_ser_burned_effect_ == 2)
				{
					this.no_ser_burned_effect_ = 0;
					for (int i = 0; i < this.LEN; i++)
					{
						if (this.AItems[i].no_ser_burned_effect)
						{
							this.no_ser_burned_effect_ = 1;
							break;
						}
					}
				}
				return this.no_ser_burned_effect_ > 0;
			}
			set
			{
				if (!value)
				{
					this.no_ser_burned_effect_ = 2;
					return;
				}
				this.no_ser_burned_effect_ = 1;
			}
		}

		public bool no_shuffleframe_on_applydamage
		{
			get
			{
				if (this.use_torture)
				{
					return true;
				}
				if (this.no_shuffleframe_on_applydamage_ == 2)
				{
					this.no_shuffleframe_on_applydamage_ = 0;
					for (int i = 0; i < this.LEN; i++)
					{
						if (this.AItems[i].no_shuffleframe_on_applydamage)
						{
							this.no_shuffleframe_on_applydamage_ = 1;
							break;
						}
					}
				}
				return this.no_shuffleframe_on_applydamage_ > 0;
			}
			set
			{
				if (!value)
				{
					this.no_shuffleframe_on_applydamage_ = 2;
					return;
				}
				this.no_shuffleframe_on_applydamage_ = 1;
			}
		}

		public bool no_shuffle_aim
		{
			get
			{
				if (this.use_torture)
				{
					return true;
				}
				if (this.no_shuffle_aim_ == 2)
				{
					this.no_shuffle_aim_ = 0;
					for (int i = 0; i < this.LEN; i++)
					{
						AbsorbManager absorbManager = this.AItems[i];
						if (absorbManager.isActive() && absorbManager.no_shuffle_aim)
						{
							this.no_shuffle_aim_ = 1;
							break;
						}
					}
				}
				return this.no_shuffle_aim_ > 0;
			}
			set
			{
				if (!value)
				{
					this.no_shuffle_aim_ = 2;
					return;
				}
				this.no_shuffle_aim_ = 1;
			}
		}

		public bool cannot_apply_mist_damage
		{
			get
			{
				if (this.cannot_apply_mist_damage_ == 2)
				{
					this.cannot_apply_mist_damage_ = 0;
					for (int i = 0; i < this.LEN; i++)
					{
						AbsorbManager absorbManager = this.AItems[i];
						if (absorbManager.isActive() && absorbManager.cannot_apply_mist_damage)
						{
							this.cannot_apply_mist_damage_ = 1;
							break;
						}
					}
				}
				return this.cannot_apply_mist_damage_ > 0;
			}
			set
			{
				if (!value)
				{
					this.cannot_apply_mist_damage_ = 2;
					return;
				}
				this.cannot_apply_mist_damage_ = 1;
			}
		}

		public float normal_UP_fade_injectable
		{
			get
			{
				if (this.normal_UP_fade_injectable_ < 0f)
				{
					if (this.LEN == 0)
					{
						return 1f;
					}
					this.normal_UP_fade_injectable_ = 1f;
					for (int i = 0; i < this.LEN; i++)
					{
						AbsorbManager absorbManager = this.AItems[i];
						if (absorbManager.isActive())
						{
							this.normal_UP_fade_injectable_ = X.Mn(this.normal_UP_fade_injectable_, absorbManager.normal_UP_fade_injectable);
						}
					}
				}
				return this.normal_UP_fade_injectable_;
			}
			set
			{
				if (value < 0f)
				{
					this.normal_UP_fade_injectable_ = -1f;
				}
			}
		}

		public UIPictureBase.EMSTATE emstate_attach
		{
			get
			{
				if (this.emstate_attach_ < UIPictureBase.EMSTATE.NORMAL)
				{
					this.emstate_attach_ = UIPictureBase.EMSTATE.NORMAL;
					for (int i = 0; i < this.LEN; i++)
					{
						AbsorbManager absorbManager = this.AItems[i];
						this.emstate_attach_ |= absorbManager.emstate_attach;
					}
				}
				return this.emstate_attach_;
			}
			set
			{
				if (value < UIPictureBase.EMSTATE.NORMAL)
				{
					this.emstate_attach_ = value;
				}
			}
		}

		public int kirimomi_release_dir
		{
			get
			{
				return this.kirimomi_release_dir_;
			}
			set
			{
				this.kirimomi_release_dir_ = value;
			}
		}

		public AIM kirimomi_release_dir_last
		{
			get
			{
				if (this.kirimomi_release_dir_last_ >= 0)
				{
					return (AIM)this.kirimomi_release_dir_last_;
				}
				if (X.xors(2) != 0)
				{
					return AIM.R;
				}
				return AIM.L;
			}
		}

		public void gachaInput(bool inputted_or_failure)
		{
			if (inputted_or_failure)
			{
				this.gacha_inputted_t = 0f;
				return;
			}
			if (this.gacha_inputted_t != -2f)
			{
				this.gacha_inputted_t = -1f;
			}
		}

		public AbsorbManager getTorturePublisher()
		{
			for (int i = 0; i < this.LEN; i++)
			{
				AbsorbManager absorbManager = this.AItems[i];
				if (absorbManager.isActive() && absorbManager.isTortureUsing())
				{
					return absorbManager;
				}
			}
			return null;
		}

		public bool isTortureByOther(M2Mover P)
		{
			AbsorbManager torturePublisher = this.getTorturePublisher();
			return torturePublisher != null && torturePublisher.getPublishMover() != P;
		}

		public AbsorbManager getSpecificPublisher(Func<AbsorbManager, bool> FnChecker)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				AbsorbManager absorbManager = this.AItems[i];
				if (absorbManager.isActive() && FnChecker(absorbManager))
				{
					return absorbManager;
				}
			}
			return null;
		}

		public int countTortureItem(Func<AbsorbManager, bool> FnChecker, bool destruct_target = false)
		{
			int num = 0;
			for (int i = 0; i < this.LEN; i++)
			{
				AbsorbManager absorbManager = this.AItems[i];
				if (absorbManager.isActive() && FnChecker(absorbManager))
				{
					num++;
					if (destruct_target)
					{
						absorbManager.destruct();
					}
				}
			}
			return num;
		}

		public bool syncTorturePosition(string pose_title)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				if (this.AItems[i].syncTorturePosition(pose_title))
				{
					return true;
				}
			}
			return false;
		}

		public bool isConfisionGacha
		{
			get
			{
				return this.has_gacha_lrtb && this.MvPr is PR && (this.MvPr as PR).Ser.has(SER.CONFUSE);
			}
		}

		public int current_pose_priority
		{
			get
			{
				if (!this.isActive())
				{
					return 0;
				}
				if (this.current_pose_priority_ < 0)
				{
					return 99;
				}
				return this.current_pose_priority_;
			}
			set
			{
				this.current_pose_priority_ = -1;
			}
		}

		public bool is_event_rendering
		{
			get
			{
				return this.ev_assign;
			}
		}

		public void finePosePriority(int p)
		{
			if (this.current_pose_priority_ > 0 && p >= this.current_pose_priority_)
			{
				this.current_pose_priority_ = -1;
			}
		}

		public bool isFilled()
		{
			return this.total_weight >= 5f;
		}

		public AbsorbManager GetManagerItem(int i)
		{
			return this.AItems[i];
		}

		public AbsorbManager Get(IGachaListener Listener)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				AbsorbManager absorbManager = this.AItems[i];
				if (absorbManager.isActive() && absorbManager.Listener == Listener)
				{
					return absorbManager;
				}
			}
			return null;
		}

		public override AbsorbManager Create()
		{
			return new AbsorbManager(this);
		}

		public readonly M2Mover Mv;

		public readonly M2MoverPr MvPr;

		private int add_cnt;

		private int items_id;

		private NelM2Attacker[] Achecked;

		public FlaggerC<AbsorbManager> FlgCamZoom;

		private string uipicture_fade_key_;

		private PostEffectItem EfCamZoom;

		private CameraRenderBinderGachaItem CbGachaT;

		public bool need_fine_gacha_effect;

		public bool need_fine_gacha_release;

		public bool no_error_on_miss_input;

		public bool gacha_active_count;

		public KEY.SIMKEY gacha_in_pd;

		public KEY.SIMKEY gacha_in_o;

		public KEY.SIMKEY safe_key;

		public AbsorbManagerContainer.RELEASE_TYPE release_type;

		private float total_weight_;

		private float gacha_inputted_t = -2f;

		public const int timeout_default = 100;

		public int timeout = 100;

		private byte use_torture_ = 2;

		private byte cannot_move_ = 2;

		private byte no_clamp_speed_ = 2;

		private byte no_ser_burned_effect_;

		private byte no_shuffleframe_on_applydamage_ = 2;

		private byte mouth_is_covered_ = 2;

		private byte no_shuffle_aim_ = 2;

		private byte cannot_apply_mist_damage_ = 2;

		public bool no_change_release_in_dead;

		public UIPictureBase.EMSTATE emstate_attach_;

		public bool fine_breath_key;

		public bool has_gacha_lrtb;

		public string breath_key;

		public bool renderable_on_evt_stop_ghandle;

		public bool gacha_renderable = true;

		private BDic<M2Attackable, List<Vector3>> OMoveReserve;

		public EffectItem EfGachaB;

		private float normal_UP_fade_injectable_ = -1f;

		private int kirimomi_release_dir_ = -1;

		private int kirimomi_release_dir_last_ = -1;

		private int current_pose_priority_;

		public const int POSEPRI_TORTURE = 99;

		public const int POSEPRI_BOSS = 60;

		public const int POSEPRI_VORE = 40;

		public const int POSEPRI_BIG = 20;

		public const int POSEPRI_NORMAL = 1;

		public bool ev_assign;

		private List<IGachaListener> ASpEvent;

		private FnEffectRun FD_fnDrawGachaB;

		private FnEffectRun FD_fnDrawGachaBEv;

		public enum RELEASE_TYPE
		{
			NORMAL,
			GACHA,
			KIRIMOMI
		}
	}
}

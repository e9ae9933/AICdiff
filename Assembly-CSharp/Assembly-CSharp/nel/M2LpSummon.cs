using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using evt;
using m2d;
using nel.gm;
using UnityEngine;
using XX;

namespace nel
{
	public class M2LpSummon : NelLpRunner, ICheckPointListener, IM2WeedManager, IStatDangerousDescriptor, IEventListener
	{
		public M2LpSummon(string _key, int _index, M2MapLayer _Lay, EnemySummoner _Reader)
			: base(_key, _index, _Lay)
		{
			this.Reader = _Reader;
			this.FD_drawBd = new M2DrawBinder.FnEffectBind(this.drawBd);
			if (M2LpSummon.ALpOutSide == null)
			{
				M2LpSummon.ALpOutSide = new M2LpCamDecline[4];
			}
		}

		public void assignWeed(M2ManaWeed _Weed)
		{
			if (this.ABelongWeed == null)
			{
				this.ABelongWeed = new List<M2ManaWeed>();
				this.AActiveWeed = new List<M2ManaWeed>();
			}
			this.ABelongWeed.Add(_Weed);
			this.AActiveWeed.Add(_Weed);
		}

		public void deassignWeed(M2ManaWeed _Weed)
		{
			if (this.ABelongWeed != null)
			{
				this.ABelongWeed.Remove(_Weed);
				this.AActiveWeed.Remove(_Weed);
			}
			if (this.ASafeWeed != null)
			{
				this.ASafeWeed.Remove(_Weed);
			}
		}

		public virtual MANA_HIT deassignActiveWeed(M2ManaWeed _Weed, AttackInfo AtkHitExecute = null)
		{
			if (this.AActiveWeed != null)
			{
				this.AActiveWeed.Remove(_Weed);
				if (AtkHitExecute != null)
				{
					return this.openSummoner(AtkHitExecute.AttackFrom, _Weed, false);
				}
			}
			return MANA_HIT.NOUSE;
		}

		public void assignActiveWeed(M2ManaWeed _Weed)
		{
			if (this.AActiveWeed != null)
			{
				this.AActiveWeed.Add(_Weed);
			}
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.ABelongWeed = null;
			this.AActiveWeed = null;
			this.ASafeWeed = null;
			this.initActionPreSummon(ref this.NInfo, ref this.defeated_count, out this.is_sudden);
			this.AFirstShownWeed = null;
			M2LpSummon.NearLpSmn = null;
			if (base.nM2D.GM != null)
			{
				base.nM2D.GM.addStatDangerDescriptor(this);
			}
			EV.addListener(this);
		}

		protected virtual void initActionPreSummon(ref NightController.SummonerData NInfo, ref int defeated_count, out M2LpSummon.SUDDEN is_sudden)
		{
			QuestTracker.QuestProgress questProgress;
			this.Reader.QEntry = base.nM2D.QUEST.getSummonerEntryFor(this.Reader, out questProgress);
			if (this.Reader.QEntry.valid && this.Reader.QEntry.fix_enemykind > 0)
			{
				ENEMYID fix_enemykind = (ENEMYID)this.Reader.QEntry.fix_enemykind;
				NDAT.getResources(base.nM2D, fix_enemykind.ToString());
			}
			NInfo = base.nM2D.NightCon.GetLpInfo(this, false);
			NInfo.summoner_is_night = this.Reader.only_night;
			defeated_count = NInfo.defeat_count;
			is_sudden = ((this.Reader.QEntry.valid && questProgress != null && !questProgress.aborted) ? M2LpSummon.SUDDEN.SUDDEN_QUEST : (base.nM2D.NightCon.isSudden(NInfo) ? M2LpSummon.SUDDEN.SUDDEN : M2LpSummon.SUDDEN.NORMAL));
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			this.Ed = this.Mp.remED(this.Ed);
			this.need_recheck_config_after = -2f;
			if (normal_map)
			{
				this.type = M2LpSummon.TYPE.MAIN;
				this.Meta = new META(this.comment);
				int[] dirs = this.Meta.getDirs("border", 0, false, 0);
				if (dirs != null && dirs.Length != 0)
				{
					int num = dirs.Length;
					this.border = 0;
					for (int i = 0; i < num; i++)
					{
						this.border |= 1 << dirs[i];
					}
				}
				else
				{
					this.border = 15;
				}
				string[] array = this.Meta.Get("sudden_margin");
				if (array != null)
				{
					if (array.Length != 0)
					{
						this.sudden_margin_x = X.Nm(array[0], 0f, false);
					}
					if (array.Length > 1)
					{
						this.sudden_margin_y = X.Nm(array[1], 0f, false);
					}
				}
				if (this.Meta.GetB("border_lock_after", false))
				{
					this.need_recheck_config_after = -1f;
				}
				this.bottom_clip = this.Meta.GetNm("bottom_clip", this.bottom_clip, 0);
				this.fallable_ringout = false;
				string[] array2 = this.Meta.Get("under_fill");
				if (array2 != null && array2.Length != 0)
				{
					this.under_extend = (byte)X.Mx(0, X.NmI(array2[0], 4, false, false));
					this.fallable_ringout = this.under_extend > 0;
				}
				if (this.wm_icon_enabled)
				{
					this.WmIco = new WMIconCreator(this, WMIcon.TYPE.ENEMY, this.cleared_sf_key);
				}
				for (int j = 3; j >= 0; j--)
				{
					M2LpSummon.ALpOutSide[j] = null;
				}
			}
			this.t_ui = -40f;
			this.Reader.flush_memory(true, false, false);
			if (this.Reader.has_puppetrevenge_replace && PuppetRevenge.isRevengeEnabled(base.nM2D, this.Reader, this))
			{
				this.is_sudden = M2LpSummon.SUDDEN.SUDDEN_PUPPETREVENGE;
				this.Reader.loadMaterialPuppetRevenge(this);
			}
			if (normal_map)
			{
				if (this.need_set_effect)
				{
					this.Ed = this.Mp.setED(base.unique_key, this.FD_drawBd, 0f);
				}
				this.Mp.addRunnerObject(this);
			}
			this.deactivateBoxes();
		}

		public override void closeAction(bool when_map_close)
		{
			BGM.remBattleTransition(base.unique_key);
			if (M2LpSummon.NearLpSmn == this)
			{
				M2LpSummon.NearLpSmn = null;
			}
			this.ABelongWeed = (this.AActiveWeed = null);
			this.ASafeWeed = null;
			this.destructBox();
			this.deactivateLog();
			this.need_recheck_config_after = -3f;
			this.Ed = this.Mp.remED(this.Ed);
			this.Meta = null;
			this.showRemovedChip(false);
			base.closeAction(when_map_close);
			if (this.Reader != null)
			{
				if (when_map_close && this.Reader.isActive())
				{
					this.Reader.close(true, false);
				}
				this.Reader.releaseLp();
			}
			if (when_map_close)
			{
				for (int i = 3; i >= 0; i--)
				{
					M2LpSummon.ALpOutSide[i] = null;
				}
				EV.remListener(this);
			}
			if (base.nM2D.GM != null)
			{
				base.nM2D.GM.remStatDangerDescriptor(this);
			}
		}

		public bool close_action_finished
		{
			get
			{
				return this.Meta == null;
			}
		}

		public string safe_area_alert_string(int rest_sec = -1)
		{
			return TX.Get("Alert_summoner_defeated", "");
		}

		public void deactivateLog()
		{
			if (this.RowSafeArea != null)
			{
				this.RowSafeArea.hideProgress();
				this.RowSafeArea = null;
			}
			this.ASafeWeed = null;
		}

		public bool fnClickStartSummonImmediate(aBtn B)
		{
			this.openSummoner(null, null, false);
			return true;
		}

		public MANA_HIT openSummoner(M2Mover MvFrom, M2ManaWeed _Weed = null, bool sudden_effect = false)
		{
			if (!this.Reader.isActive())
			{
				if (MvFrom != null)
				{
					if (_Weed != null && this.ASafeWeed != null)
					{
						int num = this.ASafeWeed.IndexOf(_Weed);
						if (num >= 0)
						{
							this.ASafeWeed.RemoveAt(num);
							if (this.ASafeWeed.Count == 0)
							{
								this.ASafeWeed = null;
							}
							return MANA_HIT.PR | MANA_HIT.EN | MANA_HIT.IMMEDIATE_COLLECTABLE | MANA_HIT.SPECIAL;
						}
					}
					if (!this.type_no_border && !base.isContainingMapXy(MvFrom.x, MvFrom.y, MvFrom.x, MvFrom.y, 0f))
					{
						X.dl("外側に MvFrom がいるためリーダーはオープンされません", null, false, false);
						return MANA_HIT.EN | MANA_HIT.FROM_GAGE_SPLIT;
					}
				}
				this.deactivateBoxes();
				if (this.validate)
				{
					this.checkValidate();
				}
				this.deactivateLog();
				this.AShowingChipsD = null;
				UiBenchMenu.initBattle();
				this.fineUsingDifficultyRestrict(true);
				if ((this.type & M2LpSummon.TYPE.NO_EFFECT) == M2LpSummon.TYPE.MAIN)
				{
					if (this.Ed == null)
					{
						this.Ed = this.Mp.setED(base.unique_key, this.FD_drawBd, 0f);
					}
					this.Ed.f0 = IN.totalframe;
					this.Ed.t = 0f;
					M2LpSummon.summonerActivateEffect(this.Mp, this.getActvEffectType(sudden_effect), base.mapfocx, base.mapfocy, (float)this.mapw);
				}
				bool flag;
				this.Reader.activate(this, out flag);
				this.showRemovedChip(true);
				if ((this.type & M2LpSummon.TYPE.NO_EFFECT) == M2LpSummon.TYPE.MAIN)
				{
					if (!flag)
					{
						BGM.addBattleTransition(base.unique_key);
					}
					else
					{
						BGM.fadeout(0f, 40f, false);
					}
				}
				base.nM2D.CheckPoint.setCheckPointManual(this);
				if (!this.type_no_hide_layer)
				{
					this.hideSummonHideChip(false);
				}
				if (this.WmIco != null)
				{
					this.WmIco.notice();
				}
				if (this.ABelongWeed != null)
				{
					int count = this.ABelongWeed.Count;
					for (int i = 0; i < count; i++)
					{
						this.ABelongWeed[i].safe_after_battle = false;
					}
				}
				if (this.need_recheck_config_after >= -1f)
				{
					this.need_recheck_config_after = -1f;
				}
				this.t_ui = 0f;
				if (!this.type_no_border || !this.type_no_hide_layer)
				{
					this.checkConfigExecute();
				}
				X.dl("Opened summoner reader: " + this.Reader.key, null, false, false);
			}
			return MANA_HIT.ALL;
		}

		protected virtual M2LpSummon.ACTV_EFFECT getActvEffectType(bool sudden_effect)
		{
			M2LpSummon.ACTV_EFFECT actv_EFFECT;
			switch (this.is_sudden & M2LpSummon.SUDDEN._TYPE)
			{
			case M2LpSummon.SUDDEN.SUDDEN:
				actv_EFFECT = (sudden_effect ? M2LpSummon.ACTV_EFFECT.SUDDEN : M2LpSummon.ACTV_EFFECT.NORMAL);
				break;
			case M2LpSummon.SUDDEN.SUDDEN_PUPPETREVENGE:
				actv_EFFECT = M2LpSummon.ACTV_EFFECT.SUDDEN_PUPPETREVENGE;
				break;
			case M2LpSummon.SUDDEN.SUDDEN_QUEST:
				actv_EFFECT = (sudden_effect ? M2LpSummon.ACTV_EFFECT.SUDDEN : M2LpSummon.ACTV_EFFECT.NORMAL);
				break;
			default:
				actv_EFFECT = M2LpSummon.ACTV_EFFECT.NORMAL;
				break;
			}
			return actv_EFFECT;
		}

		public static void summonerActivateEffect(Map2d Mp, M2LpSummon.ACTV_EFFECT _actv, float mapfocx, float mapfocy, float mapw)
		{
			Mp.M2D.Cam.Qu.Vib(5f, 8f, 2f, 0);
			PostEffect it = PostEffect.IT;
			it.setSlow(17f, 0.5f, 0);
			if ((_actv & M2LpSummon.ACTV_EFFECT.SUDDEN) != M2LpSummon.ACTV_EFFECT.NORMAL)
			{
				float footableY = Mp.getFootableY(mapfocx, (int)mapfocy, 9, false, -1f, false, true, true, 0f);
				it.addTimeFixedEffect(it.setPEabsorbed(POSTM.SUMMONER_ACTIVATE, 10f, 45f, 1f, -5), 1f);
				it.addTimeFixedEffect(it.setPEabsorbed(POSTM.SUMMONER_ACTIVATE, 2f, 60f, 1f, 30), 1f);
				it.addTimeFixedEffect(it.setPEabsorbed(POSTM.FINAL_ALPHA, 2f, 40f, 0.55f, -5), 1f);
				it.addTimeFixedEffect(it.setPEfadeinout(POSTM.FINAL_ALPHA, 80f, 100f, 0.55f, 0), 1f);
				Mp.PtcSTsetVar("x", (double)mapfocx).PtcSTsetVar("y", (double)mapfocy).PtcSTsetVar("fy", (double)footableY)
					.PtcSTsetVar("wh", (double)(Mp.CLENB * mapw * 0.5f))
					.PtcSTsetVar("w", (double)mapw);
				if (_actv == M2LpSummon.ACTV_EFFECT.SUDDEN_PUPPETREVENGE)
				{
					it.addTimeFixedEffect(it.setPEfadeinout(POSTM.JAMMING, 140f, 120f, -99.24f, 0), 1f);
					Mp.PtcSTsetVar("phase", (double)PuppetRevenge.getRevengeBattlePhase());
				}
				Mp.PtcST2Base("summoner_activate_" + _actv.ToString().ToLower(), 1f, PTCThread.StFollow.NO_FOLLOW);
				return;
			}
			it.addTimeFixedEffect(it.setPEabsorbed(POSTM.SUMMONER_ACTIVATE, 10f, 35f, 1f, -5), 1f);
			if (_actv == M2LpSummon.ACTV_EFFECT.EVENT)
			{
				Mp.PtcSTsetVar("x", (double)mapfocx).PtcSTsetVar("y", (double)mapfocy);
				Mp.PtcST2Base("summoner_activate_event", 1f, PTCThread.StFollow.NO_FOLLOW);
				return;
			}
			if (_actv != M2LpSummon.ACTV_EFFECT.SMNC)
			{
				Mp.PtcSTsetVar("x", (double)mapfocx).PtcSTsetVar("y", (double)mapfocy);
				Mp.PtcST2Base("summoner_activate", 1f, PTCThread.StFollow.NO_FOLLOW);
				return;
			}
			Mp.PtcST2Base("summoner_activate_smnc", 1f, PTCThread.StFollow.NO_FOLLOW);
		}

		public virtual string summoned_prepare_ptc_key
		{
			get
			{
				return "en_summoned_prepare";
			}
		}

		public virtual string summoned_appear_ptc_key
		{
			get
			{
				return "en_summoned";
			}
		}

		public bool type_no_effect
		{
			get
			{
				return (this.type & M2LpSummon.TYPE.NO_EFFECT) > M2LpSummon.TYPE.MAIN;
			}
			set
			{
				if (value == this.type_no_effect)
				{
					return;
				}
				if (value)
				{
					this.type |= M2LpSummon.TYPE.NO_EFFECT;
					return;
				}
				this.type &= (M2LpSummon.TYPE)(-3);
				if (this.isActive())
				{
					if (this.Ed == null)
					{
						this.Ed = this.Mp.setED(base.unique_key, this.FD_drawBd, 0f);
					}
					this.Ed.f0 = IN.totalframe;
					this.Ed.t = 0f;
					BGM.addBattleTransition(base.unique_key);
				}
			}
		}

		public bool type_no_revert
		{
			get
			{
				return (this.type & M2LpSummon.TYPE.NO_REVERT) > M2LpSummon.TYPE.MAIN;
			}
			set
			{
				if (value == this.type_no_revert)
				{
					return;
				}
				if (value)
				{
					this.type |= M2LpSummon.TYPE.NO_REVERT;
					return;
				}
				this.type &= (M2LpSummon.TYPE)(-17);
			}
		}

		public bool type_no_weedsecure_after_defeat
		{
			get
			{
				return (this.type & M2LpSummon.TYPE.NO_WEEDSECURE_AFTER_DEFEAT) > M2LpSummon.TYPE.MAIN;
			}
			set
			{
				if (value == this.type_no_weedsecure_after_defeat)
				{
					return;
				}
				if (value)
				{
					this.type |= M2LpSummon.TYPE.NO_WEEDSECURE_AFTER_DEFEAT;
					return;
				}
				this.type &= (M2LpSummon.TYPE)(-33);
			}
		}

		public bool type_no_border
		{
			get
			{
				return (this.type & M2LpSummon.TYPE.NO_BORDER) > M2LpSummon.TYPE.MAIN;
			}
		}

		public bool type_no_hide_layer
		{
			get
			{
				return (this.type & M2LpSummon.TYPE.NO_HIDE_LAYER) > M2LpSummon.TYPE.MAIN;
			}
		}

		public bool setTypeNoBorder(bool value, bool fine_config = true)
		{
			if (value == this.type_no_border)
			{
				return false;
			}
			if (value)
			{
				this.type |= M2LpSummon.TYPE.NO_BORDER;
			}
			else
			{
				this.type &= (M2LpSummon.TYPE)(-2);
				if (this.Reader.fatal_key != null)
				{
					UILog.Instance.AddAlertTX("Fatal_exists_in_this_battle", UILogRow.TYPE.ALERT);
				}
				if (this.Reader.isActive())
				{
					this.Reader.getPlayer().fineEnemyDropItem();
				}
			}
			if (fine_config && this.isActive())
			{
				this.checkConfigExecute();
			}
			return true;
		}

		public bool setTypeNoHideLayer(bool value, bool fine_config = true)
		{
			if (value == this.type_no_hide_layer)
			{
				return false;
			}
			if (value)
			{
				this.type |= M2LpSummon.TYPE.NO_HIDE_LAYER;
				if (this.isActive())
				{
					this.showRemovedChip(false);
				}
			}
			else
			{
				this.type &= (M2LpSummon.TYPE)(-9);
				if (this.isActive())
				{
					this.hideSummonHideChip(false);
				}
			}
			if (fine_config && this.isActive())
			{
				this.checkConfigExecute();
			}
			return true;
		}

		public bool type_event_enemy
		{
			get
			{
				return (this.type & M2LpSummon.TYPE.EVENT_ENEMY) > M2LpSummon.TYPE.MAIN;
			}
			set
			{
				if (value == this.type_event_enemy)
				{
					return;
				}
				if (value)
				{
					this.type |= M2LpSummon.TYPE.EVENT_ENEMY;
				}
				else
				{
					this.type &= (M2LpSummon.TYPE)(-5);
				}
				if (this.isActive())
				{
					this.Reader.getPlayer().fineEventEnemyFlag();
				}
			}
		}

		public void checkConfigExecute()
		{
			this.Mp.considerConfig4(0, 0, this.Mp.clms, this.Mp.rows);
			this.Mp.need_update_collider = true;
			if (!this.isActive() || this.type_no_border)
			{
				base.nM2D.FlgWarpEventNotInjectable.Rem(base.unique_key);
				base.nM2D.Ui.QuestBox.FlgHide.Rem("BATTLE");
				for (int i = 3; i >= 0; i--)
				{
					M2LpCamDecline m2LpCamDecline = M2LpSummon.ALpOutSide[i];
					if (m2LpCamDecline != null)
					{
						base.nM2D.Cam.remCropping(m2LpCamDecline);
					}
				}
				return;
			}
			for (int j = 3; j >= 0; j--)
			{
				M2LpCamDecline m2LpCamDecline2 = M2LpSummon.ALpOutSide[j];
				if (m2LpCamDecline2 == null || m2LpCamDecline2.Lay != this.Lay)
				{
					m2LpCamDecline2 = (M2LpSummon.ALpOutSide[j] = new M2LpCamDecline("LpSummonDecline " + j.ToString(), -1, this.Lay));
				}
				base.nM2D.Cam.addCropping(m2LpCamDecline2);
			}
			this.fineCropPosition();
			base.nM2D.FlgWarpEventNotInjectable.Add(base.unique_key);
			base.nM2D.Ui.QuestBox.FlgHide.Add("BATTLE");
		}

		public void fineCropPosition()
		{
			if (this.isActive() && !this.type_no_border)
			{
				float num = 1f - X.ZSIN(this.t_ui, 70f);
				float num2 = this.Mp.CLEN * 1.5f;
				float num3 = this.Mp.CLEN * 1.25f;
				float num4 = 200f;
				float wh = IN.wh;
				float hh = IN.hh;
				base.nM2D.Cam.blurCenterForce();
				for (int i = 3; i >= 0; i--)
				{
					M2LpCamDecline m2LpCamDecline = M2LpSummon.ALpOutSide[i];
					if (m2LpCamDecline != null)
					{
						switch (i)
						{
						case 0:
							m2LpCamDecline.Set(-IN.wh * num + this.x - wh - num2, base.y - num4, wh, base.h + num4 * 2f);
							break;
						case 1:
							m2LpCamDecline.Set(this.x - num4, -IN.hh * num + base.y - hh - num3, base.w + num4 * 2f, hh);
							break;
						case 2:
							m2LpCamDecline.Set(IN.wh * num + base.right + num2, base.y - num4, wh, base.h + num4 * 2f);
							break;
						case 3:
							m2LpCamDecline.Set(this.x - num4, IN.hh * num + base.bottom + this.bottom_clip * this.Mp.CLEN, base.w + num4 * 2f, hh);
							break;
						}
					}
				}
			}
		}

		public void parseType(string t)
		{
			M2LpSummon.TYPE type = M2LpSummon.TYPE.MAIN;
			if (TX.valid(t) && t != "0")
			{
				string[] array = TX.split(t, new Regex("[| ]+"));
				for (int i = array.Length - 1; i >= 0; i--)
				{
					M2LpSummon.TYPE type2;
					if (!FEnum<M2LpSummon.TYPE>.TryParse(array[i].ToUpper(), out type2, true))
					{
						X.de("SUMMONER_TYPE タイプエラー: " + array[i], null);
					}
					else
					{
						type |= type2;
					}
				}
			}
			M2LpSummon.TYPE type3 = this.type;
			this.type_no_effect = (type & M2LpSummon.TYPE.NO_EFFECT) > M2LpSummon.TYPE.MAIN;
			this.type_no_revert = (type & M2LpSummon.TYPE.NO_REVERT) > M2LpSummon.TYPE.MAIN;
			bool flag = this.setTypeNoBorder((type & M2LpSummon.TYPE.NO_BORDER) > M2LpSummon.TYPE.MAIN, false);
			flag = this.setTypeNoHideLayer((type & M2LpSummon.TYPE.NO_HIDE_LAYER) > M2LpSummon.TYPE.MAIN, false) || flag;
			this.type_event_enemy = (type & M2LpSummon.TYPE.EVENT_ENEMY) > M2LpSummon.TYPE.MAIN;
			X.dl(string.Concat(new string[]
			{
				"SUMMONER_TYPE (act:",
				(EnemySummoner.ActiveScript != null).ToString(),
				") タイプを ",
				type3.ToString(),
				" => ",
				this.type.ToString(),
				" に設定"
			}), null, false, false);
			if (this.isActive() && flag)
			{
				this.checkConfigExecute();
			}
		}

		public override void considerConfig4(int _l, int _t, int _r, int _b, M2Pt[,] AAPt)
		{
			if (!this.isActive() || this.type_no_border)
			{
				return;
			}
			M2EventContainer eventContainer = this.Mp.getEventContainer();
			if (eventContainer != null)
			{
				eventContainer.FlgStandDecline.Add("ENEMYSUMMONER");
			}
			int num = this.mapy + this.maph + (int)this.under_extend;
			int num2 = this.mapx + this.mapw;
			for (int i = _t; i < _b; i++)
			{
				if (X.BTW(0f, (float)i, (float)this.Mp.rows))
				{
					for (int j = _l; j < _r; j++)
					{
						if (X.BTW(0f, (float)j, (float)this.Mp.clms))
						{
							CCON.calcConfigManual(ref AAPt[j, i], (i < this.mapy || i >= num || j < this.mapx || j >= num2) ? 128 : 4);
						}
					}
				}
			}
		}

		private bool hideSummonHideChip(bool only_summon_show_mana_plants = false)
		{
			if (this.AHiddenChips != null || this.AShowingChips != null)
			{
				return false;
			}
			int count_layers = this.Mp.count_layers;
			bool flag = false;
			for (int i = 0; i < count_layers; i++)
			{
				M2MapLayer layer = this.Mp.getLayer(i);
				int summon_hide = layer.summon_hide;
				int summon_show = layer.summon_show;
				if (summon_hide > 0)
				{
					if (!only_summon_show_mana_plants)
					{
						int num = layer.count_chips;
						if (this.AHiddenChips == null)
						{
							this.AHiddenChips = new List<M2Puts>(layer.count_chips);
						}
						else
						{
							this.AHiddenChips.Capacity += layer.count_chips;
						}
						for (int j = 0; j < num; j++)
						{
							M2Puts chipByIndex = layer.getChipByIndex(j);
							this.AHiddenChips.Add(chipByIndex);
							flag = true;
							chipByIndex.addActiveRemoveKey(base.unique_key, false);
						}
						if (summon_hide >= 2)
						{
							num = ((layer.LP != null) ? layer.LP.Length : 0);
							if (this.AHiddenLp == null)
							{
								this.AHiddenLp = new List<M2LabelPoint>(num);
							}
							for (int k = 0; k < num; k++)
							{
								M2LabelPoint m2LabelPoint = layer.LP.Get(k);
								m2LabelPoint.deactivate();
								this.AHiddenLp.Add(m2LabelPoint);
							}
						}
					}
				}
				else if (summon_show > 0)
				{
					int num2 = layer.count_chips;
					if (!only_summon_show_mana_plants)
					{
						if (this.AShowingChips == null)
						{
							this.AShowingChips = new List<M2Puts>(layer.count_chips);
						}
						else
						{
							this.AShowingChips.Capacity += layer.count_chips;
						}
					}
					int l = 0;
					while (l < num2)
					{
						M2Puts chipByIndex2 = layer.getChipByIndex(l);
						if (only_summon_show_mana_plants)
						{
							NelChipManaWeed nelChipManaWeed = chipByIndex2 as NelChipManaWeed;
							if (nelChipManaWeed != null && !CCON.canStandAndNoBlockSlope(this.Mp.getConfig((int)nelChipManaWeed.mapcx, (int)(nelChipManaWeed.mbottom + 0.25f))))
							{
								if (this.AFirstShownWeed == null)
								{
									this.AFirstShownWeed = new List<NelChipManaWeed>(8);
								}
								this.AFirstShownWeed.Add(nelChipManaWeed);
								goto IL_021C;
							}
						}
						else if (this.AFirstShownWeed == null || !(chipByIndex2 is NelChipManaWeed) || this.AFirstShownWeed.IndexOf(chipByIndex2 as NelChipManaWeed) < 0)
						{
							this.AShowingChips.Add(chipByIndex2);
							goto IL_021C;
						}
						IL_022B:
						l++;
						continue;
						IL_021C:
						flag = true;
						chipByIndex2.remActiveRemoveKey("SUMMON_SHOW", false);
						goto IL_022B;
					}
					if (summon_show >= 2)
					{
						num2 = ((layer.LP != null) ? layer.LP.Length : 0);
						if (this.AShowingLp == null)
						{
							this.AShowingLp = new List<M2LabelPoint>(num2);
						}
						for (int m = 0; m < num2; m++)
						{
							M2LabelPoint m2LabelPoint2 = layer.LP.Get(m);
							m2LabelPoint2.activate();
							this.AShowingLp.Add(m2LabelPoint2);
						}
					}
				}
			}
			return flag;
		}

		private void showRemovedChip(bool no_reconsider_area = false)
		{
			bool flag = false;
			if (this.AHiddenChips != null)
			{
				int count = this.AHiddenChips.Count;
				for (int i = 0; i < count; i++)
				{
					this.AHiddenChips[i].remActiveRemoveKey(base.unique_key, this.close_action_finished);
				}
				this.AShowingChipsD = this.AHiddenChips;
				this.AHiddenChips = null;
				flag = true;
			}
			if (this.AShowingChips != null)
			{
				int count2 = this.AShowingChips.Count;
				for (int j = 0; j < count2; j++)
				{
					this.AShowingChips[j].addActiveRemoveKey("SUMMON_SHOW", false);
				}
				this.AHiddenChipsD = this.AShowingChips;
				this.AShowingChips = null;
				flag = true;
			}
			if (this.AShowingLp != null)
			{
				for (int k = this.AShowingLp.Count - 1; k >= 0; k--)
				{
					this.AShowingLp[k].deactivate();
				}
				this.AShowingLp = null;
			}
			if (this.AHiddenLp != null)
			{
				for (int l = this.AHiddenLp.Count - 1; l >= 0; l--)
				{
					this.AHiddenLp[l].activate();
				}
				this.AHiddenLp = null;
			}
			if (flag && !no_reconsider_area)
			{
				this.checkConfigExecute();
			}
		}

		public virtual void closeSummoner(bool defeated, out bool is_first_defeat)
		{
			BGM.remBattleTransition(base.unique_key);
			is_first_defeat = false;
			if (this.Reader.isActive())
			{
				this.Reader.close(false, defeated);
				return;
			}
			M2EventContainer eventContainer = this.Mp.getEventContainer();
			if (eventContainer != null)
			{
				eventContainer.FlgStandDecline.Rem("ENEMYSUMMONER");
			}
			X.dl("Summoner " + this.Reader.key + " is " + (defeated ? "defeated" : "closed"), null, false, false);
			base.nM2D.CheckPoint.removeCheckPointManual(this);
			if (this.Ui != null)
			{
				this.Ui.releaseTextCache();
			}
			this.deactivateLog();
			base.nM2D.MGC.clearPoolHitLink();
			if (!this.type_no_hide_layer)
			{
				this.showRemovedChip(true);
			}
			if (defeated)
			{
				if (this.defeated_count == 0)
				{
					this.defeated_count = 1;
					this.Mp.fineSaveFlag();
				}
				this.NInfo = base.nM2D.NightCon.DefeatSummonAreaLp(this, out is_first_defeat, this.NInfo);
				if (this.is_sudden_puppetrevenge)
				{
					base.nM2D.IMNG.battleFinishProgress(4, false);
					PuppetRevenge.progressRevengeBattlePhase();
				}
				else
				{
					if (!this.no_item_progress)
					{
						base.nM2D.IMNG.battleFinishProgress(this.Reader.grade, false);
					}
					if (!this.no_gq_progress)
					{
						base.nM2D.QUEST.summonerFinished(this.Reader, false, false);
					}
				}
				if (!this.no_item_progress)
				{
					base.nM2D.NightCon.clearPuppetRevengeCache(this.is_sudden_puppetrevenge, this.Mp.key);
				}
				this.is_sudden = M2LpSummon.SUDDEN.NORMAL;
				this.t_ui = -30f;
				if (this.ABelongWeed != null && !this.type_no_weedsecure_after_defeat)
				{
					this.ASafeWeed = new List<M2ManaWeed>(this.ABelongWeed.Count);
					for (int i = this.ABelongWeed.Count - 1; i >= 0; i--)
					{
						M2ManaWeed m2ManaWeed = this.ABelongWeed[i];
						if (!m2ManaWeed.destructed)
						{
							this.ASafeWeed.Add(m2ManaWeed);
							m2ManaWeed.safe_after_battle = true;
						}
					}
				}
				this.defeated_pre_frame = true;
			}
			else
			{
				this.t_ui = 0f;
			}
			if (this.need_set_effect)
			{
				if (this.Ed == null)
				{
					this.Ed = this.Mp.setED(base.unique_key, this.FD_drawBd, 0f);
				}
			}
			else
			{
				this.Ed = this.Mp.remED(this.Ed);
			}
			if (this.Ed != null)
			{
				this.Ed.f0 = IN.totalframe;
				this.Ed.t = 0f;
			}
			if (!this.type_no_border || !this.type_no_hide_layer)
			{
				if (this.need_recheck_config_after >= -1f)
				{
					this.need_recheck_config_after = 0f;
					return;
				}
				if (this.need_recheck_config_after == -2f)
				{
					this.checkConfigExecute();
				}
			}
		}

		private bool drawBd(EffectItem Ef, M2DrawBinder Ed)
		{
			Bench.P("Summoner LP ED");
			Ef.x = base.mapfocx;
			Ef.y = base.mapfocy;
			if (this.Reader.isActive())
			{
				PR pr = this.Mp.M2D.Cam.getBaseMover() as PR;
				if (!this.type_no_effect)
				{
					if (pr != null && pr.is_alive && !this.type_no_border)
					{
						float num = 0f;
						if (pr.x < (float)(this.mapx + 4) && (this.border & 1) != 0)
						{
							num = X.Mx(num, 0.25f + 0.75f * (1f - X.ZLINE(pr.x - (float)this.mapx, 4f)));
						}
						if (pr.x >= (float)(this.mapx + this.mapw - 4) && (this.border & 4) != 0)
						{
							num = X.Mx(num, 0.25f + 0.75f * (1f - X.ZLINE((float)(this.mapx + this.mapw) - pr.x, 4f)));
						}
						if (pr.y < (float)(this.mapy + 4) && (this.border & 2) != 0)
						{
							num = X.Mx(num, 0.25f + 0.75f * (1f - X.ZLINE(pr.y - (float)this.mapy, 4f)));
						}
						if (pr.y >= (float)(this.mapy + this.maph - 4) && (this.border & 8) != 0)
						{
							num = X.Mx(num, 0.25f + 0.75f * (1f - X.ZLINE((float)(this.mapy + this.maph) - pr.y, 4f)));
						}
						if (num > 0f)
						{
							Ef.x = (float)this.mapx + (float)this.mapw * 0.5f;
							Ef.y = (float)this.mapy + (float)this.maph * 0.5f;
							float num2 = (float)this.mapw * this.Mp.CLENB;
							float num3 = (float)this.maph * this.Mp.CLENB;
							float num4 = this.Mp.CLENB * 2f;
							MeshDrawer mesh = Ef.GetMesh("", MTR.MtrFireWall, false);
							mesh.Col = mesh.ColGrd.Set(4293140802U).mulA(num).C;
							mesh.ColGrd.setA(0f);
							mesh.allocUv2(8, false);
							mesh.RectDoughnut(0f, 0f, num2 + num4, num3 + num4, 0f, 0f, num2, num3, false, 1f, 0f, false);
							mesh.Uv2(30f, 60f, false);
							mesh.allocUv2(0, true);
						}
					}
					if (Ed.t < 100f)
					{
						if (this.AHiddenChips != null)
						{
							this.drawChipsAnimation(Ef, Ed, X.ZLINE(Ed.t, 90f), this.AHiddenChips, false);
						}
						if (this.AShowingChips != null)
						{
							this.drawChipsAnimation(Ef, Ed, X.ZLINE(Ed.t, 90f), this.AShowingChips, true);
						}
					}
				}
			}
			else
			{
				if (Ed.isinCamera(Ef, 6f, 8f))
				{
					if (this.WmIco != null)
					{
						this.WmIco.notice();
					}
					if (!this.cannot_open_summoner)
					{
						Bench.P("1");
						float num5 = X.ZSIN(Ef.af, 25f);
						MeshDrawer meshDrawer = Ef.GetMesh("", MTRX.MtrMeshMul, true);
						MeshDrawer meshDrawer2 = Ef.GetMesh("", MTRX.MtrMeshAdd, true);
						float num6 = X.COSIT(140f) * this.Mp.CLEN * 0.015625f;
						meshDrawer.base_y += num6;
						meshDrawer2.base_y += num6;
						C32 c = MTRX.cola.White().blend(4293628819U, 0.5f + 0.5f * X.COSIT(70f)).mulA(0.7f + 0.3f * X.COSIT(2.7f));
						meshDrawer.Identity();
						if (num5 < 1f)
						{
							meshDrawer.Scale(num5, 1f + (1f - num5) * 2f, false);
							meshDrawer2.Col = c.mulA(num5).C;
						}
						else
						{
							meshDrawer2.Col = c.C;
						}
						Bench.Pend("1");
						Bench.P("2");
						meshDrawer.Col = MTRX.cola.Black().mulA(0.85f).C;
						meshDrawer.ColGrd.Black().setA(0f);
						meshDrawer.BlurPoly2(0f, 0f, 85f, 0f, 4, 100f, 40f, MTRX.cola, meshDrawer.ColGrd);
						NightController.drawGradeDaia(meshDrawer2, 0f, 0f, this.Reader.grade);
						int num7 = 10;
						for (int i = 0; i < num7; i++)
						{
							int num8 = (int)Ef.af + i * 8;
							float num9 = X.ZLINE((float)(num8 % 65), 65f);
							uint ran = X.GETRAN2(i, num8 / 65 % 7 + 4);
							float num10 = 70f * X.RAN(ran, 725);
							float num11 = 70f - num10 + num9 * X.NI(80, 150, X.RAN(ran, 604));
							float num12 = 1f - X.ZLINE(num9 - 0.5f, 0.5f);
							meshDrawer.BlurPoly2(num10 * (float)X.MPF(X.RAN(ran, 1024) >= 0.5f), num11, 4f * num12, 0f, 8, 100f, 20f * num12, MTRX.cola, meshDrawer.ColGrd);
						}
						Bench.Pend("2");
						Bench.P("3");
						float num13 = X.ANMP((int)Ef.af, 89, 1f);
						meshDrawer.Poly(0f, 0f, 80f + 66f * num13, 0f, 4, 1f, false, 0f, 0f);
						if (this.NInfo != null && this.NInfo.defeated_in_session)
						{
							meshDrawer = Ef.GetMesh("_lpsummon", MTRX.MtrMeshNormal, true);
							meshDrawer.base_z -= 0.01f;
							meshDrawer.Col = meshDrawer.ColGrd.Set(4290690750U).C;
							meshDrawer.CheckMark(0f, -10f, 120f, 8f, false);
							meshDrawer.Col = meshDrawer.ColGrd.Set(4284440415U).C;
							meshDrawer.CheckMark(0f, -12f, 120f, 8f, false);
						}
						Bench.Pend("3");
					}
				}
				if (this.ASafeWeed != null)
				{
					MeshDrawer meshDrawer2 = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
					int count = this.ASafeWeed.Count;
					meshDrawer2.Col = meshDrawer2.ColGrd.Set(4288462193U).C;
					meshDrawer2.Identity();
					for (int j = 0; j < count; j++)
					{
						Vector3 localPosition = this.ASafeWeed[j].transform.localPosition;
						meshDrawer2.base_x = localPosition.x * this.Mp.base_scale;
						meshDrawer2.base_y = localPosition.y * this.Mp.base_scale;
						PlayerCheckPoint.DrawBubble(meshDrawer2, 0f, 0f);
					}
				}
				if (this.AShowingChipsD != null && Ed.t < 80f && !this.drawChipsAnimation(Ef, Ed, X.ZLINE(Ed.t, 70f), this.AShowingChipsD, true))
				{
					this.AShowingChipsD = null;
				}
				if (this.AHiddenChipsD != null && Ed.t < 80f && !this.drawChipsAnimation(Ef, Ed, X.ZLINE(Ed.t, 70f), this.AHiddenChipsD, false))
				{
					this.AHiddenChipsD = null;
				}
			}
			Bench.Pend("Summoner LP ED");
			return true;
		}

		private bool drawChipsAnimation(EffectItem Ef, M2DrawBinder Ed, float tz, List<M2Puts> ACp, bool showing)
		{
			if (tz >= 1f)
			{
				return false;
			}
			Ef.x = 0f;
			Ef.y = 0f;
			MeshDrawer meshImg = Ef.GetMeshImg("", this.Mp.M2D.MIchip, BLEND.ADD, false);
			float num = 1f;
			MeshDrawer meshDrawer = ((tz < 0.5f) ? Ef.GetMesh("", MTRX.MtrMeshAdd, false) : null);
			M2Camera cam = this.Mp.M2D.Cam;
			meshImg.Col = meshImg.ColGrd.Set(showing ? 4285513983U : 4294905655U).mulA(1f - tz).C;
			if (meshDrawer != null)
			{
				num = tz / 0.5f;
				meshDrawer.Col = meshImg.ColGrd.setA1(1f - num).C;
			}
			int count = ACp.Count;
			for (int i = 0; i < count; i++)
			{
				M2Puts m2Puts = ACp[i];
				if (!m2Puts.Img.Meta.invisible && cam.isCoveringMp(m2Puts.mleft, m2Puts.mtop, m2Puts.mright, m2Puts.mbottom, (float)(X.Mx(m2Puts.iwidth, m2Puts.iheight) + 40)))
				{
					float num2 = m2Puts.mapcx * base.CLENB;
					float num3 = -m2Puts.mapcy * base.CLENB;
					float draw_rotR = m2Puts.draw_rotR;
					if (m2Puts.Img.initAtlasMd(meshImg, 0U))
					{
						meshImg.RotaGraph(num2, num3, this.Mp.base_scale, draw_rotR, null, m2Puts.flip);
					}
					if (meshDrawer != null)
					{
						float num4 = (float)m2Puts.iwidth * this.Mp.base_scale;
						float num5 = (float)m2Puts.iheight * this.Mp.base_scale;
						int num6 = X.IntC(X.NI(12, 2, X.ZLINE((float)i, 16f)));
						for (int j = 0; j < num6; j++)
						{
							uint ran = X.GETRAN2(m2Puts.index + j * 7, 2 + j % 5);
							float num7 = X.RAN(ran, 2381) * 1.5707964f;
							float num8 = X.NI(30, 50, X.RAN(ran, 1353)) * num;
							float num9 = X.NI(4, 8, X.RAN(ran, 2174)) * X.ZSINV(1f - num);
							float num10 = num4 * X.NI(-0.5f, 0.5f, X.RAN(ran, 1853));
							float num11 = num5 * X.NI(-0.5f, 0.5f, X.RAN(ran, 2882));
							num10 += num8 * X.Cos(num7) * (float)X.MPF(num10 >= 0f);
							num11 += num8 * X.Sin(num7) * (float)X.MPF(num11 >= 0f);
							meshDrawer.Circle(num10 + num2, num3 + num11, num9, 0f, false, 0f, 0f);
						}
					}
				}
			}
			return true;
		}

		public void createBox()
		{
		}

		private void deactivateBoxes()
		{
			if (M2LpSummon.NearLpSmn == this)
			{
				M2LpSummon.NearLpSmn = null;
			}
			if (this.Ui != null)
			{
				this.Ui.deactivate();
			}
		}

		private void destructBox()
		{
			if (this.Ui != null)
			{
				this.Ui.destruct();
				this.Ui = null;
			}
		}

		public virtual string getTabIconKey(UiGMCStat Gmc, out uint color)
		{
			color = 4293010506U;
			if (!this.isActiveBorder() && this.t_ui <= 0f)
			{
				return null;
			}
			return "itemrow_category.29";
		}

		public virtual bool createGMDesc(UiGMCStat Gmc, Designer Ds, out aBtn FirstBtn, out aBtn LastBtn)
		{
			FirstBtn = null;
			LastBtn = null;
			uint num;
			string tabIconKey = this.getTabIconKey(Gmc, out num);
			if (tabIconKey == null)
			{
				return false;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddIconHtml(tabIconKey, num, num == 0U).Add(" ");
				this.addTitleStringForGM(Gmc, stb);
				UiBoxDesigner.addPTo(Ds, stb, ALIGN.LEFT, Ds.use_w, true, 0f, "", -1);
				Ds.Br();
				DsnDataP dsnDataP = new DsnDataP("", false)
				{
					Col = NEL.ColText,
					text = (this.isActiveBorder() ? TX.Get("Smnc_title_enemy", "") : TX.Get("Summoner_descryption_enemies", "")),
					size = 15f,
					alignx = ALIGN.LEFT,
					swidth = 180f,
					text_auto_wrap = true,
					text_margin_x = 12f,
					text_margin_y = 6f,
					sheight = 1f
				};
				FillBlock fillBlock = Ds.addP(dsnDataP, false);
				DsnDataButtonMulti dsnDataButtonMulti = new DsnDataButtonMulti();
				dsnDataButtonMulti.name = "gmdesc_enemy";
				dsnDataButtonMulti.skin = "row";
				dsnDataButtonMulti.margin_h = 0f;
				dsnDataButtonMulti.clms = 1;
				dsnDataButtonMulti.h = 32f;
				dsnDataButtonMulti.w = Ds.use_w - 24f;
				dsnDataButtonMulti.fnClick = (aBtn B) => false;
				DsnDataButtonMulti dsnDataButtonMulti2 = dsnDataButtonMulti;
				using (BList<string> blist = ListBuffer<string>.Pop(0))
				{
					using (BList<int> blist2 = ListBuffer<int>.Pop(0))
					{
						dsnDataButtonMulti2.titlesL = blist;
						int num2;
						if (this.isActiveBorder())
						{
							using (BList<Vector2Int> blist3 = ListBuffer<Vector2Int>.Pop(0))
							{
								this.Reader.getPlayer().listupRestEnemy(blist3);
								num2 = blist3.Count;
								for (int i = 0; i < num2; i++)
								{
									Vector2Int vector2Int = blist3[i];
									if (this.isAvailableInStat((ENEMYID)vector2Int.x))
									{
										blist.Add(vector2Int.x.ToString());
										blist2.Add(vector2Int.y);
									}
								}
								goto IL_0262;
							}
						}
						List<int> appearEnemyAbout = this.Reader.getAppearEnemyAbout();
						num2 = appearEnemyAbout.Count;
						for (int j = 0; j < num2; j++)
						{
							if (this.isAvailableInStat((ENEMYID)appearEnemyAbout[j]))
							{
								blist.Add(appearEnemyAbout[j].ToString());
								blist2.Add(-1);
							}
						}
						IL_0262:
						BtnContainer<aBtn> btnContainer = Ds.addButtonMultiT<aBtnFDRow>(dsnDataButtonMulti2);
						dsnDataButtonMulti2.titlesL = null;
						num2 = blist.Count;
						if (num2 > 0)
						{
							FirstBtn = btnContainer.Get(0);
							LastBtn = btnContainer.Get(num2 - 1);
						}
						for (int k = 0; k < num2; k++)
						{
							stb.Clear();
							aBtn aBtn = btnContainer.Get(k);
							ButtonSkinFDItemRow buttonSkinFDItemRow = aBtn.get_Skin() as ButtonSkinFDItemRow;
							ENEMYID enemyid = (ENEMYID)X.NmI(aBtn.title, 0, false, false);
							stb.Add(NDAT.getEnemyName(enemyid, true));
							int num3 = blist2[k];
							if (num3 > 0)
							{
								stb.Add(" x ", num3, "");
							}
							buttonSkinFDItemRow.setItem(null, base.nM2D, new UiFieldGuide.FDR(enemyid, 1), stb);
						}
						fillBlock.heightPixel = btnContainer.get_sheight_px();
						fillBlock.fineWH(true);
						Ds.RowRemakeHeightRecalc(fillBlock, null);
					}
				}
				Ds.Br();
				ReelManager.ItemReelDrop[] array = null;
				List<ReelManager.ItemReelDrop> list = null;
				if (this.isActiveBorder())
				{
					list = this.Reader.getPlayer().getRestReelDrop();
				}
				else
				{
					array = this.Reader.getItemReelVector();
				}
				if (array == null && list == null)
				{
					dsnDataP.TxCol = NEL.ColText;
					dsnDataP.Col = MTRX.ColTrnsp;
					dsnDataP.text = TX.Get("Summoner_no_reward", "");
					dsnDataP.swidth = Ds.use_w;
					fillBlock = Ds.addP(dsnDataP, false);
				}
				else
				{
					dsnDataP.text = TX.Get("Summoner_reward", "");
					fillBlock = Ds.addP(dsnDataP, false);
					int num4 = ((array != null) ? array.Length : list.Count);
					dsnDataButtonMulti2.titles = X.makeToStringed<int>(X.makeCountUpArray(num4, 0, 1));
					BtnContainer<aBtn> btnContainer2 = Ds.addButtonMultiT<aBtnFDRow>(dsnDataButtonMulti2);
					dsnDataButtonMulti2.titles = null;
					for (int l = 0; l < num4; l++)
					{
						ButtonSkinFDItemRow buttonSkinFDItemRow2 = btnContainer2.Get(l).get_Skin() as ButtonSkinFDItemRow;
						ReelManager.ItemReelDrop itemReelDrop = ((array != null) ? array[l] : list[l]);
						buttonSkinFDItemRow2.setItem(null, base.nM2D, new UiFieldGuide.FDR(itemReelDrop.IR.GReelItem, 0), null);
					}
					fillBlock.heightPixel = btnContainer2.get_sheight_px();
					fillBlock.fineWH(true);
					Ds.RowRemakeHeightRecalc(fillBlock, null);
					if (num4 > 0)
					{
						if (FirstBtn == null)
						{
							btnContainer2.Get(0);
						}
						if (LastBtn != null)
						{
							LastBtn.setNaviB(btnContainer2.Get(0), true, true);
						}
						LastBtn = btnContainer2.Get(num4 - 1);
					}
				}
			}
			if (base.nM2D.IMNG.has_recipe_collection)
			{
				Ds.Br();
				Ds.addP(new DsnDataP("", false)
				{
					html = true,
					text = TX.Get("KD_show_catalog", ""),
					text_margin_x = 20f,
					text_margin_y = 8f,
					size = 12f,
					alignx = ALIGN.RIGHT,
					swidth = Ds.use_w,
					TxCol = NEL.ColText
				}, false);
			}
			if (this.isActiveBorder())
			{
				string text = this.abortButtonTXKeyForGM(Gmc);
				if (TX.valid(text))
				{
					Ds.Br();
					Ds.addTab("_Abort", Ds.use_w, 46f, Ds.use_w, 46f, false).alignx = ALIGN.CENTER;
					aBtnNel aBtnNel = Ds.addButtonT<aBtnNel>(new DsnDataButton
					{
						name = "summoner_abort",
						title = "summoner_abort",
						skin_title = text,
						skin = "normal",
						w = 240f,
						h = 26f,
						fnClick = new FnBtnBindings(this.fnAbortSummonerFromGM)
					});
					Ds.endTab(true);
					Ds.Br();
					if (LastBtn != null)
					{
						LastBtn.setNaviB(aBtnNel, true, true);
					}
					FirstBtn = FirstBtn ?? aBtnNel;
					LastBtn = aBtnNel;
				}
			}
			return true;
		}

		private bool isAvailableInStat(ENEMYID id)
		{
			NOD.BasicData basicData = NOD.getBasicData(id);
			return basicData != null && (basicData.hide_in_FG_list & 4) == 0;
		}

		public virtual void addTitleStringForGM(UiGMCStat Gmc, STB Stb)
		{
			Stb.AddTxA("Status_Tab_current_battle", false);
			Stb.TxRpl(this.Reader.name_localized);
		}

		public virtual string abortButtonTXKeyForGM(UiGMCStat Gmc)
		{
			return "&&Status_Tab_battle_abort";
		}

		public virtual string abortEventFromGM()
		{
			return "__M2D_abort_summoner";
		}

		protected virtual bool fnAbortSummonerFromGM(aBtn B)
		{
			if (!this.isActive() || EV.isActive(false))
			{
				return false;
			}
			string text = this.abortEventFromGM();
			if (TX.valid(text))
			{
				EV.stack(text, 0, -1, new string[] { (base.nM2D.GameOver != null && base.nM2D.GameOver.isActive()) ? "1" : "0" }, null);
			}
			return true;
		}

		public override bool run(float fcnt)
		{
			if (this.defeated_pre_frame)
			{
				this.defeated_pre_frame = false;
				base.nM2D.Mana.transformMana(base.mapfocx, base.mapfocy, 28f, MANA_HIT.PR | MANA_HIT.NO_FARE_HIT | MANA_HIT.IMMEDIATE_COLLECTABLE | MANA_HIT.SPECIAL);
				if (!this.type_no_revert && !this.type_no_weedsecure_after_defeat)
				{
					this.RowSafeArea = UILog.Instance.AddAlert(this.safe_area_alert_string(-1), UILogRow.TYPE.ALERT);
				}
				Vector2 returnPos = this.getReturnPos();
				this.Mp.PtcSTsetVar("cx", (double)returnPos.x).PtcSTsetVar("cy", (double)returnPos.y).PtcST("summoner_defeated", null, PTCThread.StFollow.NO_FOLLOW);
			}
			if (this.need_recheck_config_after >= 0f)
			{
				this.need_recheck_config_after += fcnt;
				bool flag = false;
				PR pr = this.Mp.Pr as PR;
				if (pr == null)
				{
					flag = true;
				}
				else if (pr.isNormalState() && (pr.hasFoot() || this.need_recheck_config_after >= 120f))
				{
					flag = true;
				}
				if (flag)
				{
					this.need_recheck_config_after = -1f;
					this.checkConfigExecute();
				}
			}
			if (this.is_sudden != M2LpSummon.SUDDEN.NORMAL && (this.is_sudden & M2LpSummon.SUDDEN._CHIP_FINED) == M2LpSummon.SUDDEN.NORMAL)
			{
				this.is_sudden |= M2LpSummon.SUDDEN._CHIP_FINED;
				if (!this.cannot_open_summoner && !this.Meta.GetB("no_sudden_appear_weed", false))
				{
					this.hideSummonHideChip(true);
				}
			}
			if (this.RowSafeArea != null)
			{
				if (Map2d.can_handle)
				{
					this.RowSafeArea.hold();
				}
				else if (this.RowSafeArea.isHiding())
				{
					this.RowSafeArea = null;
				}
			}
			if (!this.Reader.isActive())
			{
				if (this.t_ui != 0f)
				{
					this.t_ui += (float)((this.t_ui > 0f) ? (-1) : 1);
				}
				if (this.t_ui == 0f)
				{
					if (this.validate)
					{
						this.validate = false;
						this.checkValidate();
					}
					if (this.cannot_open_summoner)
					{
						this.t_ui = -300f;
					}
					else
					{
						bool flag2 = false;
						if (Map2d.can_handle && this.Mp.Pr is PR && this.Mp.Pr.isManipulateState())
						{
							float mapfocx = base.mapfocx;
							float num = X.Mn((float)(this.mapy + this.maph - 3), base.mapfocy);
							if (this.is_sudden == M2LpSummon.SUDDEN.NORMAL)
							{
								flag2 = X.BTW(mapfocx - 3.3f, this.Mp.Pr.x, mapfocx + 3.3f) && !base.nM2D.NightCon.isUiActive();
							}
							else
							{
								flag2 = (this.Mp.Pr.vx != 0f || this.Mp.Pr.vy != 0f) && X.BTW((float)this.mapx + this.sudden_margin_x, this.Mp.Pr.x, (float)(this.mapx + this.mapw) - this.sudden_margin_x);
							}
							if (flag2 && X.BTW(base.mapfocy - 0.5f, this.Mp.Pr.y, (float)(this.mapy + this.maph)))
							{
								float num2 = X.Mn((float)(this.mapy + this.maph), this.Mp.getFootableY(base.mapfocx, (int)base.mapfocy, 14, false, -1f, false, true, true, 0f)) + 0.5f;
								flag2 = X.BTW(num, this.Mp.Pr.mbottom, num2);
							}
							else
							{
								flag2 = false;
							}
							if (flag2)
							{
								(this.Mp.Pr as PR).EpCon.lockOazuke(0f);
							}
						}
						if (!flag2)
						{
							this.t_ui = (float)(this.Reader.isActive() ? (-120) : (-15));
							if (M2LpSummon.NearLpSmn == this)
							{
								M2LpSummon.NearLpSmn = null;
							}
						}
						else
						{
							M2LpSummon.NearLpSmn = this;
							if (this.is_sudden == M2LpSummon.SUDDEN.NORMAL)
							{
								this.t_ui = 30f;
							}
							else
							{
								this.t_ui = -15f;
								if (X.XORSP() < 0.5f)
								{
									this.openSummoner(null, null, true);
								}
							}
						}
					}
				}
				if (this.t_ui > 0f)
				{
					if (this.Reader.need_fine)
					{
						this.Reader.loadMaterial(this.Mp, false);
					}
					bool flag3 = this.Ui == null || !this.Ui.isActive();
					if (flag3)
					{
						byte b = (byte)TX.getCurrentFamilyIndex();
						if (b != this.eng)
						{
							this.eng = b;
							this.destructBox();
							flag3 = true;
						}
						if (this.Ui == null)
						{
							this.Ui = new UILpSummon(this);
						}
						this.Ui.activate();
					}
					if (this.Ui.run(fcnt, flag3))
					{
						SND.Ui.play("enter_enemy_summon", false);
						this.openSummoner(null, null, false);
					}
				}
				else
				{
					this.deactivateBoxes();
				}
			}
			else if (this.t_ui < 70f)
			{
				this.t_ui += Map2d.TS;
				this.fineCropPosition();
			}
			return true;
		}

		public void fineUsingDifficultyRestrict(bool resetting = false)
		{
			if (!resetting && this.skill_difficulty_restrict <= DIFF.I)
			{
				return;
			}
			this.skill_difficulty_restrict = DIFF.I;
			X.dl("skill_difficulty_restrict is " + this.skill_difficulty_restrict.ToString(), null, false, false);
		}

		public virtual bool EvtTargetIsMe(string t)
		{
			return TX.valid(t) && TX.isEnd(this.key, t) && this.key.Length == "Summon_".Length + t.Length;
		}

		public virtual bool EvtRead(EvReader ER, StringHolder rER, int skipping)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				if (!(cmd == "SUMMONER_TYPE"))
				{
					if (!(cmd == "SUMMONER_RECHECK_HR_BUFFER"))
					{
						if (!(cmd == "SUMMONER_ACTIVATE"))
						{
							if (cmd == "SUMMONER_ACTIVATE_EFFECT")
							{
								if (this.Mp.Pr != null)
								{
									M2LpSummon.ACTV_EFFECT actv_EFFECT = M2LpSummon.ACTV_EFFECT.NORMAL;
									if (rER._1 != "" && !FEnum<M2LpSummon.ACTV_EFFECT>.TryParse(rER._1, out actv_EFFECT, true))
									{
										return rER.tError("不明なM2LpSummon.ACTV_EFFECT: " + rER._1);
									}
									M2LpSummon.summonerActivateEffect(this.Mp, actv_EFFECT, this.Mp.Pr.x, this.Mp.Pr.y, 16f);
								}
								return true;
							}
							if (!(cmd == "ENEMY_ADD_TICKET"))
							{
								if (cmd == "SUMMONER_DEFEAT_NIGHT_PROGRESS")
								{
									EnemySummoner enemySummoner = EnemySummoner.Get(rER._1, true);
									if (enemySummoner == null)
									{
										return rER.tError("不明な Summoner : " + rER._1);
									}
									if (enemySummoner == this.Reader)
									{
										if (base.nM2D.NightCon.SummonerDefeated(enemySummoner, rER.Int(3, -1)) > 0 && rER.Nm(2, 1f) != 0f)
										{
											base.nM2D.NightCon.showNightLevelAdditionUI();
										}
										return true;
									}
								}
							}
							else
							{
								if (!(M2EventCommand.EvMV is NelEnemy))
								{
									return rER.tError("EvMV が NelEnemy ではない");
								}
								(M2EventCommand.EvMV as NelEnemy).createTicketFromEvent(rER);
								return true;
							}
						}
						else if (this.EvtTargetIsMe(rER._1))
						{
							X.dl("OPEN SUMMONER FROM EVENT!", null, false, false);
							this.openSummoner(M2EventCommand.EvMV, null, false);
							return true;
						}
					}
					else if (this.EvtTargetIsMe(rER._1) && this.Reader.getPlayer() != null)
					{
						this.Reader.getPlayer().recheckHrBuffer();
						return true;
					}
				}
				else if (this.EvtTargetIsMe(rER._1))
				{
					this.parseType(rER.slice_join(2, " ", ""));
					return true;
				}
			}
			return false;
		}

		bool IEventListener.EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		public virtual bool EvtClose(bool is_first_or_end)
		{
			return true;
		}

		int IEventListener.EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		bool IEventListener.EvtMoveCheck()
		{
			return true;
		}

		public void checkValidate()
		{
			this.validate = false;
		}

		public string cleared_sf_key
		{
			get
			{
				return this.Reader.getClearedSfKey(this.Mp);
			}
		}

		public string reader_key
		{
			get
			{
				return this.Reader.key;
			}
		}

		public bool need_set_effect
		{
			get
			{
				return this.Reader.isActive() || (this.is_sudden == M2LpSummon.SUDDEN.NORMAL && !this.noon_donot_show);
			}
		}

		public bool noon_donot_show
		{
			get
			{
				return !this.Reader.QEntry.valid && !this.Reader.do_not_open && (this.Reader.only_night && !base.nM2D.NightCon.isLastBattled(this)) && !base.nM2D.NightCon.isNight();
			}
		}

		public virtual bool cannot_open_summoner
		{
			get
			{
				return (this.type_no_revert && this.defeated_count > 0) || (this.noon_donot_show && !this.is_sudden_puppetrevenge) || this.Reader.do_not_open;
			}
		}

		public bool is_sudden_puppetrevenge
		{
			get
			{
				return (this.is_sudden & M2LpSummon.SUDDEN._TYPE) == M2LpSummon.SUDDEN.SUDDEN_PUPPETREVENGE;
			}
		}

		public virtual bool wm_icon_enabled
		{
			get
			{
				return !this.Reader.do_not_open;
			}
		}

		public virtual Vector2 RevertPosPr
		{
			get
			{
				return new Vector2(base.mapcx, base.mapcy);
			}
		}

		public NightController.SummonerData getInfo()
		{
			return this.NInfo;
		}

		public int get_defeated_count()
		{
			return this.defeated_count;
		}

		public bool isActive()
		{
			return this.Reader != null && this.Reader.isActive();
		}

		public bool isActiveBorder()
		{
			return this.Reader != null && this.Reader.isActive() && !this.type_no_border;
		}

		public void returnChcekPoint(PR Pr)
		{
		}

		public int getCheckPointPriority()
		{
			return 1000;
		}

		public Color32 getDrawEffectPositionAndColor(ref int pixel_x, ref int pixel_y)
		{
			return MTRX.ColTrnsp;
		}

		public void activateCheckPoint()
		{
		}

		public bool drawCheckPoint(EffectItem Ef, float pixe_x, float pixel_y, Color32 Col)
		{
			return true;
		}

		public Vector2 getReturnPos()
		{
			return new Vector2(base.mapfocx, this.Mp.getFootableY(base.mapfocx, (int)base.mapfocy, 12, true, -1f, false, true, true, 0f));
		}

		bool IM2WeedManager.get_destructed()
		{
			return base.destructed;
		}

		public readonly EnemySummoner Reader;

		public bool no_item_progress;

		public bool no_gq_progress;

		private M2DrawBinder Ed;

		public int border = 15;

		public float t_ui;

		private int defeated_count;

		public List<M2Puts> AHiddenChips;

		public List<M2Puts> AShowingChips;

		public List<M2LabelPoint> AHiddenLp;

		public List<M2LabelPoint> AShowingLp;

		public List<M2Puts> AHiddenChipsD;

		public List<M2Puts> AShowingChipsD;

		public WMIconCreator WmIco;

		public float need_recheck_config_after = -2f;

		public byte eng = byte.MaxValue;

		private const float MAXT_CROP = 70f;

		private M2LpSummon.SUDDEN is_sudden;

		public float sudden_margin_x = 2f;

		public float sudden_margin_y;

		public int skill_difficulty_restrict;

		public float bottom_clip = 3f;

		public const byte under_extend_default = 4;

		protected byte under_extend = 4;

		public bool fallable_ringout;

		private UILpSummon Ui;

		public List<M2ManaWeed> ASafeWeed;

		public List<M2ManaWeed> ABelongWeed;

		public List<M2ManaWeed> AActiveWeed;

		public List<NelChipManaWeed> AFirstShownWeed;

		private static M2LpCamDecline[] ALpOutSide;

		private UILogRow RowSafeArea;

		private bool defeated_pre_frame;

		private NightController.SummonerData NInfo;

		public static M2LpSummon NearLpSmn;

		public const string evt_abort = "__M2D_abort_summoner";

		private bool validate;

		private M2LpSummon.TYPE type;

		private M2DrawBinder.FnEffectBind FD_drawBd;

		public enum TYPE
		{
			MAIN,
			NO_BORDER,
			NO_EFFECT,
			EVENT_ENEMY = 4,
			NO_HIDE_LAYER = 8,
			NO_REVERT = 16,
			NO_WEEDSECURE_AFTER_DEFEAT = 32
		}

		protected enum SUDDEN
		{
			NORMAL,
			SUDDEN,
			SUDDEN_PUPPETREVENGE,
			SUDDEN_QUEST,
			_TYPE = 127,
			_CHIP_FINED
		}

		public enum ACTV_EFFECT
		{
			NORMAL,
			EVENT,
			SMNC,
			SUDDEN = 64,
			SUDDEN_PUPPETREVENGE
		}
	}
}

using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class NightController : IValotileSetable, IEventWaitListener
	{
		public float dLevelToNightLevel(int _dlevel)
		{
			int num = _dlevel % 16;
			if (num < 3)
			{
				return 0f;
			}
			if (num < 9)
			{
				return 0.5f + 0.25f * global::XX.X.ZPOW((float)(num - 4), 5f);
			}
			num -= 9;
			return 1f + global::XX.X.ZLINE((float)(num - 3), 4f) * 0.5f;
		}

		public NightController(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.OSmnData = new BDic<string, NightController.SummonerData>();
			this.OSmnDataThisSession = new BDic<string, NightController.SummonerData>();
			this.AWeather = new WeatherItem[0];
			this.ALockPuppetRevenge = new List<string>(4);
			NightController.Xors.init(false, 0U, 0U, 0U, 0U);
		}

		public void destruct()
		{
			this.destructRemovingMist();
			if (this.GobUi != null)
			{
				this.MdUi.destruct();
				this.EF.destruct();
			}
		}

		public void newGame()
		{
			this.deactivate(true);
			NightController.Xors.init(false, 0U, 0U, 0U, 0U);
			this.clear();
		}

		public void clearWithoutNightLevel()
		{
			this.pre_dlevel = (this.dlevel = (this.dlevel_add = (this.dlevel_add_stock = 0)));
			this.fade_add = 0f;
			this.redraw_dgd_flag = false;
			this.battle_count = 0;
			this.reel_obtained = 0;
			this.first_battle_dlevel = 0;
			this.current_ui_obtainable = 0;
			this.cur_weather_ = 0;
			this.last_battle_lp_key = "";
			foreach (KeyValuePair<string, NightController.SummonerData> keyValuePair in this.OSmnDataThisSession)
			{
				keyValuePair.Value.clearSession();
			}
			this.OSmnDataThisSession.Clear();
			for (int i = this.AWeather.Length - 1; i >= 0; i--)
			{
				this.AWeather[i].destruct();
			}
			this.AWeather = new WeatherItem[0];
		}

		public void clear()
		{
			this.destructRemovingMist();
			this.fade_deplevel = (this.night_level = 0f);
			this.clearWithoutNightLevel();
			this.clearPuppetRevenge();
			this.OSmnData.Clear();
		}

		public void clearPuppetRevenge()
		{
			this.lock_puppetrevenge = 0;
			this.ALockPuppetRevenge.Clear();
		}

		public void initS(Map2d Mp)
		{
			this.deactivate(true);
			this.destructRemovingMist();
			for (int i = this.AWeather.Length - 1; i >= 0; i--)
			{
				this.AWeather[i].initS(Mp, true);
			}
		}

		public void destructRemovingMist()
		{
			if (this.ARemovingMst != null)
			{
				for (int i = this.ARemovingMst.Count - 1; i >= 0; i--)
				{
					M2FillingMistDrawer m2FillingMistDrawer = this.ARemovingMst[i];
					m2FillingMistDrawer.cache_gob = false;
					m2FillingMistDrawer.destruct();
				}
				this.ARemovingMst = null;
			}
		}

		public NightController.SummonerData GetLpInfo(M2LpSummon Lp, bool create_and_assigning = false)
		{
			return this.GetLpInfo(Lp.cleared_sf_key, Lp.Reader, create_and_assigning);
		}

		public NightController.SummonerData GetLpInfo(EnemySummoner Smn, Map2d Mp, bool create_and_assigning = false)
		{
			return this.GetLpInfo(Smn.getClearedSfKey(Mp), Smn, create_and_assigning);
		}

		public NightController.SummonerData GetLpInfo(string key, EnemySummoner Reader, bool create_and_assigning = false)
		{
			NightController.SummonerData summonerData = global::XX.X.Get<string, NightController.SummonerData>(this.OSmnData, key);
			if (summonerData == null)
			{
				summonerData = new NightController.SummonerData(null, 0);
				if (create_and_assigning)
				{
					this.OSmnData[key] = summonerData;
				}
			}
			if (Reader == null)
			{
				Reader = summonerData.getSummoner(key);
				if (Reader == null)
				{
					return summonerData;
				}
			}
			else
			{
				summonerData.SmnCache = Reader;
			}
			int num;
			if (summonerData.summoner_is_night && summonerData.defeat_count == 0)
			{
				summonerData.night_calced = 0;
				summonerData.sudden_level = byte.MaxValue;
			}
			else if (this.suddenEnable() && this.last_battle_lp_key != key && (!summonerData.defeated_in_session || (this.dlevel >= 32 && this.getObtainableGrade(Reader.grade, summonerData) >= Reader.grade)) && SCN.isSuddenBattleEnable(Reader.key, out num))
			{
				byte b = (byte)(this.getDayCount(false) + 1);
				if (summonerData.night_calced < b)
				{
					summonerData.night_calced = b;
					summonerData.sudden_level = (byte)((num < 0) ? NightController.xors(256) : num);
				}
			}
			else
			{
				summonerData.sudden_level = 0;
			}
			return summonerData;
		}

		public NightController.SummonerData DefeatSummonAreaLp(M2LpSummon Lp, out bool is_first_defeat, NightController.SummonerData Ni = null)
		{
			string cleared_sf_key = Lp.cleared_sf_key;
			Ni = (this.OSmnData[cleared_sf_key] = (this.OSmnDataThisSession[cleared_sf_key] = Ni ?? this.GetLpInfo(Lp, false)));
			is_first_defeat = Ni.defeat_count == 0;
			if (!Lp.is_sudden_puppetrevenge)
			{
				Ni.defeat_count++;
				Ni.defeated_in_session = true;
			}
			Ni.night_calced = (byte)(this.getDayCount(false) + 1);
			if (Ni.last_battle_index <= 0 || !(this.last_battle_lp_key == cleared_sf_key))
			{
				NightController.SummonerData summonerData = Ni;
				int num = this.battle_count + 1;
				this.battle_count = num;
				summonerData.last_battle_index = num;
			}
			this.last_battle_lp_key = cleared_sf_key;
			Ni.sudden_level = 0;
			return Ni;
		}

		public bool isUiActive()
		{
			return this.t_actv >= 0 && this.state != NightController.UIST.OFFLINE;
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || this.isUiActive();
		}

		public int summoner_enemy_count_addition(EnemySummoner Smn)
		{
			int num = this.dlevel + this.dlevel_add;
			int num2 = num % 16;
			int num3 = global::XX.X.Mn(5, num / 16);
			int num4;
			if (!this.isNight())
			{
				if (num3 == 0)
				{
					num4 = (num2 + 1) / 3;
				}
				else
				{
					num4 = global::XX.X.IntC((float)num3 * 1.77f) + ((num2 >= 4) ? 1 : 0);
				}
			}
			else
			{
				num4 = 2 + global::XX.X.IntC((float)num3 * 2.5f);
			}
			return num4 / this.diff_divide(Smn);
		}

		public int summoner_max_thunder_odable_appear(EnemySummoner Smn)
		{
			int num = global::XX.X.Mn(10, this.dlevel / 16);
			int num2;
			if (Smn.skill_difficulty_restrict == 0)
			{
				num2 = ((num >= 3) ? 1 : 0);
			}
			else
			{
				num2 = ((num >= 5) ? 2 : ((num >= 2) ? 1 : 0));
			}
			if (!this.isNight())
			{
				num2--;
			}
			return global::XX.X.Mx(0, num2);
		}

		public int summoner_enemy_countmax_addition(EnemySummoner Smn)
		{
			int num = this.dlevel + this.dlevel_add;
			int num2 = num % 16;
			int num3 = global::XX.X.Mn(5, num / 16);
			if (!this.isNight())
			{
				return num3 * 2 / this.diff_divide(Smn) + ((num2 > 3) ? 1 : 0);
			}
			return (int)((float)num3 * 2.5f + 2f) / this.diff_divide(Smn);
		}

		public float summoner_delayonesecond_ratio(EnemySummoner Smn)
		{
			int num = this.dlevel + this.dlevel_add;
			int num2 = num % 16;
			int num3 = global::XX.X.Mn(10, num / 16);
			if (num3 >= 1)
			{
				return 0.9f;
			}
			if (num3 >= 2)
			{
				return 0.82f;
			}
			if (num3 >= 3)
			{
				return 0.76f;
			}
			if (num3 >= 4)
			{
				return 0.66f;
			}
			if (num3 >= 5)
			{
				return 0.5f;
			}
			if (num3 >= 6)
			{
				return 0.3f;
			}
			return 1f;
		}

		public int diff_divide(EnemySummoner Smn)
		{
			if (Smn.skill_difficulty_restrict != 0)
			{
				return 1;
			}
			return 2;
		}

		public int summoner_max_addition(EnemySummoner Smn)
		{
			int num = this.dlevel + this.dlevel_add;
			int num2 = num % 16;
			int num3 = global::XX.X.Mn(10, num / 16);
			int num4;
			if (!this.isNight())
			{
				num4 = global::XX.X.IntC((float)global::XX.X.Mx(0, num3 - 1) * 0.5f);
			}
			else
			{
				num4 = num3 + 1;
			}
			for (int i = this.AWeather.Length - 1; i >= 0; i--)
			{
				num4 += this.AWeather[i].summoner_max_addition(Smn);
			}
			num4 = (this.isNight() ? global::XX.X.Mx(num4, 0) : num4);
			if (num4 < 0)
			{
				return num4;
			}
			return num4 / this.diff_divide(Smn);
		}

		public float summoner_mp_addition(EnemySummoner Smn)
		{
			int num = this.dlevel + this.dlevel_add;
			int num2 = global::XX.X.Mn(10, num / 16);
			if (!this.isNight())
			{
				int num3 = num2;
				if (num3 != 0)
				{
					if (num3 != 1)
					{
						return (float)num2 * 2.2f - 1f + global::XX.X.NI(0f, 2.2f, global::XX.X.ZLINE(global::XX.X.frac((float)num2), 0.6f));
					}
					return global::XX.X.NI(1f, 1.8f, global::XX.X.ZLINE(global::XX.X.frac((float)num2), 0.6f));
				}
				else
				{
					if ((float)num2 < 0.25f)
					{
						return 0f;
					}
					return 0.3f + global::XX.X.NI(0f, 0.6f, global::XX.X.ZLINE((float)num2 - 0.25f, 0.4f));
				}
			}
			else
			{
				if (num2 == 0)
				{
					return global::XX.X.NI(0f, 0.125f, global::XX.X.ZLINE((float)num2 - 0.75f, 0.25f));
				}
				return 0.125f * (float)num2 + (float)((num2 >= 2) ? 2 : 0) + global::XX.X.NI(0f, 0.125f, global::XX.X.ZLINE(global::XX.X.frac((float)num2) - 0.6f, 0.4f));
			}
		}

		public float summoner_drop_od_enemy_box_ratio(EnemySummoner Smn)
		{
			int num = this.dlevel + this.dlevel_add;
			return global::XX.X.NIL(0.2f, 0.5f, (float)num, 80f);
		}

		public int summoner_drop_od_enemy_box_max(EnemySummoner Smn)
		{
			return global::XX.X.Mn(global::XX.X.IntR((float)(this.dlevel + this.dlevel_add) / 16f), 3);
		}

		public void run(float fcnt)
		{
			if (this.fade_add != 0f)
			{
				this.night_level = global::XX.X.VALWALK(this.night_level, this.fade_deplevel, this.fade_add * fcnt);
				bool flag = false;
				if (this.night_level == this.fade_deplevel)
				{
					this.fade_add = 0f;
					flag = true;
				}
				this.pre_drawn = false;
				this.fineLevel(flag);
			}
			if (this.state != NightController.UIST.OFFLINE)
			{
				this.runUi();
			}
		}

		public void runWeather(float fcnt, Map2d Mp)
		{
			for (int i = this.AWeather.Length - 1; i >= 0; i--)
			{
				this.AWeather[i].run(fcnt, Mp);
			}
		}

		public void fineLevel(bool fix_night_to_morning = false)
		{
			if (fix_night_to_morning && this.fade_deplevel >= 2f)
			{
				int num = (int)(this.fade_deplevel / 2f) * 2;
				this.fade_deplevel -= (float)num;
				this.night_level -= (float)num;
			}
			this.M2D.fineMaterialColor(false);
		}

		public void changeTo(float dep_level, int _fade_maxt)
		{
			if (dep_level < this.fade_deplevel)
			{
				dep_level = 2f + dep_level;
			}
			if (dep_level == this.fade_deplevel && this.fade_add == 0f && this.night_level == this.fade_deplevel)
			{
				return;
			}
			this.fade_deplevel = dep_level;
			bool flag = false;
			if (_fade_maxt <= 0)
			{
				this.fade_add = 0f;
				this.night_level = dep_level;
				flag = true;
			}
			else
			{
				this.night_level = this.GetLevel(true);
				this.fade_add = global::XX.X.Abs(dep_level - this.night_level) / (float)_fade_maxt;
			}
			this.fineLevel(flag);
		}

		public void SummonerInited(EnemySummoner Smn, Map2d Mp, out int thunder_overdrive)
		{
			int num = 0;
			this.reserved_obtainable_grade = this.getObtainableGrade(Smn, Mp, false);
			for (int i = this.AWeather.Length - 1; i >= 0; i--)
			{
				num += this.AWeather[i].getThunderOverdriveCount(this, Smn);
			}
			thunder_overdrive = num;
		}

		public int SummonerDefeated(EnemySummoner Smn, Map2d Mp, int ob_add = -1)
		{
			if (ob_add < 0)
			{
				ob_add = this.getReservedObtainableGrade(Smn, Mp, true, true);
			}
			foreach (KeyValuePair<string, NightController.SummonerData> keyValuePair in this.OSmnDataThisSession)
			{
				EnemySummoner smnCache = keyValuePair.Value.SmnCache;
				if (smnCache != null)
				{
					smnCache.flush_memory(false, false);
				}
			}
			if (ob_add > 0)
			{
				this.dlevel += ob_add;
			}
			if (this.dlevel_add_stock > 0)
			{
				this.dlevel_add += this.dlevel_add_stock;
				ob_add += this.dlevel_add_stock;
				this.dlevel_add_stock = 0;
			}
			this.reserved_obtainable_grade = 0;
			return ob_add;
		}

		public void fineMeterShowLevel()
		{
			this.pre_dlevel = global::XX.X.Mn(this.dlevel + this.dlevel_add, 192);
		}

		public void showNightLevelAdditionUI()
		{
			this.activate(NightController.UIST.DLEVEL_SHOW);
			this.changeTo(this.dLevelToNightLevel(this.dlevel), 260);
		}

		public void weatherShuffle()
		{
			int num = this.cur_weather_;
			this.cur_weather_ = 0;
			int num2 = this.dlevel;
			List<WeatherItem> list = new List<WeatherItem>(this.AWeather);
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].checkQuit(num2, true))
				{
					list.RemoveAt(i);
				}
			}
			WeatherItem.weatherShuffle(list, num2, num);
			for (int j = list.Count - 1; j >= 0; j--)
			{
				this.cur_weather_ |= 1 << (int)list[j].weather;
			}
			this.AWeather = list.ToArray();
		}

		public int getReservedObtainableGrade(EnemySummoner Smn, Map2d Mp, bool consider_restrict = true, bool consider_debug_lock_danger = false)
		{
			if (Smn == null || (consider_debug_lock_danger && this.debug_lock_dangerousness && this.M2D.debug_listener_created) || this.M2D.isGameOverActive())
			{
				return 0;
			}
			if (this.reserved_obtainable_grade >= 0)
			{
				return this.reserved_obtainable_grade;
			}
			return this.getObtainableGrade(Smn, Mp, false);
		}

		public int getObtainableGrade(EnemySummoner Smn, Map2d Mp, bool consider_restrict = true)
		{
			if (Smn == null)
			{
				return 0;
			}
			int grade = Smn.grade;
			NightController.SummonerData lpInfo = this.GetLpInfo(Smn, Mp, false);
			return this.getObtainableGrade(Smn.grade, lpInfo);
		}

		public int getObtainableGrade(int base_grade, NightController.SummonerData Info)
		{
			if (Info == null)
			{
				return 0;
			}
			if (Info.last_battle_index == 0)
			{
				return base_grade;
			}
			return global::XX.X.Mx(0, global::XX.X.Mn(base_grade, this.battle_count - Info.last_battle_index - ((this.getDayCount(true) >= 3 || this.first_battle_dlevel >= 16) ? 1 : 0)));
		}

		public bool alreadyCleardInThisSession(M2LpSummon Lp)
		{
			return (Lp.getInfo() ?? this.GetLpInfo(Lp, false)).defeated_in_session;
		}

		public bool alreadyCleardInThisSession(EnemySummoner Smn, Map2d Mp)
		{
			return this.GetLpInfo(Smn, Mp, false).defeated_in_session;
		}

		public bool alreadyCleardAtLeastOnce(EnemySummoner Smn, Map2d Mp)
		{
			return this.GetLpInfo(Smn, Mp, false).defeat_count > 0;
		}

		public void cameraFinedImmediately()
		{
			for (int i = this.AWeather.Length - 1; i >= 0; i--)
			{
				this.AWeather[i].fine_pos = true;
			}
		}

		public bool isNoelJuiceExplodable()
		{
			return !this.M2D.isSafeArea();
		}

		public bool addAdditionalDangerLevel(ref int count, int grade, bool show_ui)
		{
			if (!this.isNoelJuiceExplodable())
			{
				return false;
			}
			int num = 45 - this.dlevel_add;
			if (num <= 0)
			{
				return true;
			}
			int num2 = global::XX.X.Mn(count, global::XX.X.IntC((float)num / (float)(grade + 1)));
			count -= num2;
			this.dlevel_add = global::XX.X.Mn(this.dlevel_add + (grade + 1) * num2, 45);
			if (show_ui)
			{
				if (!this.isUiActive())
				{
					this.activate(NightController.UIST.DLEVEL_SHOW);
				}
				else
				{
					this.fineCurrentUiObtainable();
				}
			}
			return true;
		}

		public bool addAdditionalDangerLevelStock(int count)
		{
			if (!this.isNoelJuiceExplodable() || count <= 0)
			{
				return false;
			}
			if (EnemySummoner.isActiveBorder())
			{
				this.dlevel_add_stock += count;
			}
			else
			{
				this.addAdditionalDangerLevel(ref count, 0, true);
			}
			return true;
		}

		public void setAdditionalDangerLevelManual(int v)
		{
			this.dlevel_add = global::XX.X.Mn(v, 45);
		}

		private void changeState(NightController.UIST st)
		{
			this.state = st;
			this.t_state = 0;
			this.s_phase = 0;
		}

		private void tempActivation(bool flag)
		{
			if (this.state == NightController.UIST.OFFLINE)
			{
				return;
			}
			this.GobUi.SetActive(flag);
		}

		private void digestObtainableCount()
		{
			while (this.current_ui_obtainable > this.reel_obtained)
			{
				this.reel_obtained += 1;
				this.M2D.IMNG.getReelManager().obtainProgress((int)this.reel_obtained);
			}
		}

		public void deactivate(bool immediate = false)
		{
			if (this.t_actv >= 0)
			{
				this.digestObtainableCount();
			}
			if (immediate)
			{
				this.t_actv = -40;
				if (this.state != NightController.UIST.OFFLINE)
				{
					this.state = NightController.UIST.OFFLINE;
					this.redraw_dgd_flag = false;
					this.fineMeterShowLevel();
					this.M2D.remValotAddition(this);
				}
				if (this.GobUi != null)
				{
					this.GobUi.SetActive(false);
				}
				if (this.UiReel != null)
				{
					this.M2D.IMNG.getReelManager().destructGob();
					this.UiReel = null;
					return;
				}
			}
			else if (this.t_actv >= 0)
			{
				this.t_actv = global::XX.X.Mn(-1, -40 + this.t_actv);
				if (this.UiReel != null)
				{
					this.UiReel.deactivate();
				}
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.EF != null && !this.EF.no_graphic_render;
			}
			set
			{
				if (this.EF == null)
				{
					return;
				}
				this.EF.no_graphic_render = (this.EF.draw_gl_only = !global::XX.X.DEBUGSTABILIZE_DRAW && value);
				ValotileRenderer.RecheckUse(this.MdUi, this.GobUi.GetComponent<MeshRenderer>(), ref this.ValotUi, value);
			}
		}

		public void repositGob(bool force = false)
		{
			if (!force && (this.state == NightController.UIST.OFFLINE || this.GobUi == null))
			{
				return;
			}
			float num = -3.875f;
			IN.PosP(this.GobUi.transform, this.M2D.ui_shift_x, 0f, num);
		}

		public void activate(NightController.UIST st)
		{
			this.changeState(st);
			this.t_actv = 0;
			bool flag = true;
			if (this.GobUi == null)
			{
				this.GobUi = new GameObject("NightCon");
				this.MdUi = MeshDrawer.prepareMeshRenderer(this.GobUi, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
				this.Ma = new MdArranger(this.MdUi);
				this.Dgd = new DangerGageDrawer(this.MdUi, this.GobUi.GetComponent<MeshRenderer>(), true);
				this.GobUi.layer = IN.gui_layer;
				this.EF = new PTCThreadRunnerOnce<EffectItemNel>(this.MdUi, this.GobUi, 16);
				this.EF.initEffect("nightcon", IN.getGUICamera(), new Effect<EffectItemNel>.FnCreateEffectItem(EffectItemNel.fnCreateOneNel), EFCON_TYPE.UI);
			}
			else
			{
				flag = !this.GobUi.activeSelf;
			}
			if (flag)
			{
				this.MdUi.base_z = 0f;
				this.GobUi.SetActive(true);
				this.M2D.addValotAddition(this);
				this.repositGob(true);
			}
			this.Dgd.auto_clear = false;
			this.Dgd.appear_delay = ((this.pre_dlevel == 0) ? 0 : 256);
			this.Dgd.val = this.pre_dlevel;
			this.Dgd.already_show = this.pre_dlevel;
			this.Dgd.daia_delay = 3;
			if (this.UiReel != null)
			{
				this.M2D.IMNG.getReelManager().destructGob();
				this.UiReel = null;
			}
			this.UiReel = this.M2D.IMNG.getReelManager().initUiState(ReelManager.MSTATE.NIGHTCON_ADDING, this.GobUi.transform, true);
			this.fineCurrentUiObtainable();
			this.UiReel.no_draw_hidescreen = true;
		}

		private void runUi()
		{
			this.redraw_dgd_flag = false;
			if (this.t_actv >= 0)
			{
				int num = this.t_actv;
				this.t_actv = num + 1;
				if (num < 40)
				{
					this.redraw_dgd_flag = true;
				}
				if (this.state == NightController.UIST.DLEVEL_SHOW)
				{
					if (this.t_state >= 70 && this.pre_dlevel < this.dlevel + this.dlevel_add)
					{
						this.t_state = 50;
						this.Dgd.val = this.pre_dlevel + 1;
						this.Dgd.fixAppearTime(this.pre_dlevel, (float)this.t_actv);
						DangerGageDrawer dgd = this.Dgd;
						num = this.pre_dlevel;
						this.pre_dlevel = num + 1;
						Vector2 vector = dgd.getPos(num % 16) * 0.015625f * 2f;
						vector.y += 0.65625f;
						this.EF.PtcSTsetVar("cx", (double)vector.x).PtcSTsetVar("cy", (double)vector.y).PtcST("dangerlv_add", this.M2D.PlayerNoel, PTCThread.StFollow.NO_FOLLOW, null);
						this.redraw_dgd_flag = true;
					}
					if (this.t_state >= 50 && this.t_state <= 80)
					{
						this.redraw_dgd_flag = true;
					}
					if (this.t_state >= 110 && this.s_phase < 1)
					{
						if (this.reel_obtained < this.current_ui_obtainable)
						{
							this.reel_obtained += 1;
							this.t_state = 90;
							this.M2D.IMNG.getReelManager().obtainProgress((int)this.reel_obtained);
						}
						else
						{
							this.s_phase = 2;
						}
					}
					if (this.t_state > 150 && this.UiReel != null && this.s_phase < 2)
					{
						if (this.UiReel.ReelObtainAnimating())
						{
							this.t_state = 140;
						}
						else
						{
							this.s_phase = 2;
						}
					}
					if (this.t_state >= 250)
					{
						this.deactivate(false);
					}
				}
				else if (this.t_state >= 60)
				{
					this.deactivate(false);
				}
			}
			else
			{
				int num = this.t_actv - 1;
				this.t_actv = num;
				if (num < -40)
				{
					this.deactivate(true);
					return;
				}
				this.redraw_dgd_flag = true;
			}
			this.t_state++;
			if (global::XX.X.D)
			{
				this.repositGob(false);
				bool flag = false;
				if (this.redraw_dgd_flag)
				{
					this.redrawUi((this.t_actv >= 0) ? global::XX.X.ZLINE((float)this.t_actv, 40f) : global::XX.X.ZLINE((float)(40 + this.t_actv), 40f));
					flag = true;
				}
				else if (this.EF.Length > 0)
				{
					this.MdUi.chooseSubMesh(4, false, false);
					this.Ma.revertVerAndTriIndexSaved(false);
					flag = true;
				}
				if (flag || this.pre_drawn)
				{
					this.MdUi.chooseSubMesh(4, false, false);
					this.EF.runDrawOrRedrawMesh(global::XX.X.D, (float)global::XX.X.AF, 1f);
					this.MdUi.updateForMeshRenderer(true);
					this.MdUi.base_z = 0f;
					this.pre_drawn = flag;
				}
			}
		}

		private void redrawUi(float alpha)
		{
			this.redraw_dgd_flag = false;
			float num = 0f;
			float num2 = 42f;
			float num3 = 1f;
			float num4;
			if (this.state == NightController.UIST.DLEVEL_SHOW)
			{
				num4 = alpha;
				num3 = 2f;
				num2 -= 180f * global::XX.X.ZPOW(1f - alpha);
			}
			else
			{
				num4 = 0f;
			}
			this.pre_drawn = false;
			this.MdUi.clear(false, false);
			this.MdUi.base_x = (this.MdUi.base_y = 0f);
			this.MdUi.chooseSubMesh(0, false, true);
			this.MdUi.Col = this.MdUi.ColGrd.Black().mulA(0.25f * alpha).C;
			this.MdUi.Rect(0f, 0f, IN.w + 32f, IN.h + 32f, false);
			this.MdUi.chooseSubMesh(4, false, false);
			this.Ma.Set(true);
			if (num4 >= 0f)
			{
				this.Dgd.Redraw(this.MdUi, (float)((this.t_actv >= 0) ? this.t_actv : 2048), true, num4);
				this.MdUi.chooseSubMesh(4, false, false);
				this.Ma.Set(false);
				this.Ma.scaleAll(num3, num3, 0f, 0f, false);
				this.Ma.translateAll(num, num2, false);
				if (alpha != 1f)
				{
					this.Ma.mulAlpha(alpha);
					return;
				}
			}
			else
			{
				this.Ma.Set(false);
			}
		}

		public ByteArray writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(8);
			Ba.writeFloat(this.fade_deplevel);
			Ba.writeUShort((ushort)this.pre_dlevel);
			Ba.writeUShort((ushort)this.dlevel);
			int num = this.OSmnData.Count;
			Ba.writeUShort((ushort)num);
			foreach (KeyValuePair<string, NightController.SummonerData> keyValuePair in this.OSmnData)
			{
				Ba.writePascalString(keyValuePair.Key, "utf-8");
				keyValuePair.Value.writeBinaryTo(Ba);
			}
			Ba.writePascalString(this.last_battle_lp_key, "utf-8");
			Ba.writePascalString(null, "utf-8");
			Ba.writeByte((int)this.reel_obtained);
			num = this.AWeather.Length;
			Ba.writeUShort((ushort)num);
			for (int i = 0; i < num; i++)
			{
				this.AWeather[i].writeBinaryTo(Ba);
			}
			Ba.writeByte(this.dlevel_add);
			Ba.writeByte(this.first_battle_dlevel);
			NightController.Xors.writeBinaryTo(Ba);
			Ba.writeByte((int)this.lock_puppetrevenge);
			num = global::XX.X.Mn(255, this.ALockPuppetRevenge.Count);
			Ba.writeByte(num);
			for (int j = 0; j < num; j++)
			{
				Ba.writePascalString(this.ALockPuppetRevenge[j], "utf-8");
			}
			return Ba;
		}

		public ByteArray readBinaryFrom(ByteArray Ba)
		{
			this.clear();
			int num = Ba.readByte();
			this.night_level = Ba.readFloat();
			this.fade_deplevel = this.night_level;
			if (this.fade_deplevel != this.night_level)
			{
				this.fade_add = 1f;
			}
			if (num >= 1)
			{
				this.pre_dlevel = (int)Ba.readUShort();
				this.dlevel = (int)Ba.readUShort();
				int num2 = (int)Ba.readUShort();
				if (num == 1)
				{
					for (int i = 0; i < num2; i++)
					{
						Ba.readPascalString("utf-8", false);
					}
				}
				else
				{
					for (int j = 0; j < num2; j++)
					{
						string text = Ba.readPascalString("utf-8", false);
						NightController.SummonerData summonerData = (this.OSmnData[text] = new NightController.SummonerData(Ba, num));
						if (summonerData.defeated_in_session)
						{
							this.OSmnDataThisSession[text] = summonerData;
							this.battle_count = global::XX.X.Mx(summonerData.last_battle_index, this.battle_count);
						}
					}
					this.last_battle_lp_key = Ba.readPascalString("utf-8", false);
					Ba.readPascalString("utf-8", false);
					this.reel_obtained = (byte)Ba.readByte();
				}
				num2 = (int)Ba.readUShort();
				this.AWeather = new WeatherItem[num2];
				int num3 = 0;
				for (int k = 0; k < num2; k++)
				{
					WeatherItem weatherItem = WeatherItem.readFrom(Ba, num);
					if (weatherItem != null)
					{
						if (num < 4)
						{
							weatherItem.start_dlevel = global::XX.X.Mx(0, weatherItem.start_dlevel - this.dlevel_add);
						}
						this.AWeather[num3++] = weatherItem;
						this.cur_weather_ |= 1 << (int)weatherItem.weather;
					}
				}
				if (num3 < num2)
				{
					Array.Resize<WeatherItem>(ref this.AWeather, num3);
				}
				if (num >= 3)
				{
					this.dlevel_add = Ba.readByte();
					this.dlevel_add = global::XX.X.Mn(this.dlevel_add, 45);
					if (num >= 5)
					{
						this.first_battle_dlevel = Ba.readByte();
						if (num >= 7)
						{
							NightController.Xors.readBinaryFrom(Ba, false);
							if (num >= 8)
							{
								this.lock_puppetrevenge = (byte)Ba.readByte();
								num2 = Ba.readByte();
								for (int l = 0; l < num2; l++)
								{
									this.ALockPuppetRevenge.Add(Ba.readPascalString("utf-8", false));
								}
							}
						}
					}
				}
			}
			this.deactivate(true);
			this.fineLevel(true);
			return Ba;
		}

		public void syncPosToWMIcon()
		{
			foreach (KeyValuePair<string, NightController.SummonerData> keyValuePair in this.OSmnData)
			{
				int num = keyValuePair.Key.IndexOf("..");
				if (num >= 0)
				{
					Map2d map2d = this.M2D.Get(TX.slice(keyValuePair.Key, 0, num), false);
					if (map2d != null)
					{
						WholeMapItem wholeFor = this.M2D.WM.GetWholeFor(map2d, true);
						if (wholeFor != null)
						{
							List<WMIcon> iconVectorFor = wholeFor.GetIconVectorFor(map2d);
							if (iconVectorFor != null)
							{
								for (int i = iconVectorFor.Count - 1; i >= 0; i--)
								{
									WMIcon wmicon = iconVectorFor[i];
									if (wmicon.type == WMIcon.TYPE.ENEMY && wmicon.sf_key == null)
									{
										wmicon.sf_key = keyValuePair.Key;
										break;
									}
								}
							}
						}
					}
				}
			}
		}

		public bool hasWeather(WeatherItem.WEATHER w)
		{
			return (this.cur_weather_ & (1 << (int)w)) != 0;
		}

		public bool applyWeatherDebug(string s)
		{
			WeatherItem.WEATHER weather = WeatherItem.WEATHER.NORMAL;
			if (s == "NORMAL")
			{
				this.cur_weather_ = 0;
				for (int i = this.AWeather.Length - 1; i >= 0; i--)
				{
					this.AWeather[i].destruct();
				}
				this.AWeather = new WeatherItem[0];
				return true;
			}
			bool flag = TX.isStart(s, '!');
			if (flag)
			{
				s = TX.slice(s, 1);
			}
			if (!FEnum<WeatherItem.WEATHER>.TryParse(s.ToUpper(), out weather, true))
			{
				weather = WeatherItem.WEATHER.NORMAL;
			}
			if (weather > WeatherItem.WEATHER.NORMAL)
			{
				if (!flag)
				{
					if ((this.cur_weather_ & (1 << (int)weather)) == 0)
					{
						List<WeatherItem> list = new List<WeatherItem>(this.AWeather);
						WeatherItem weatherItem = new WeatherItem(weather, this.dlevel + this.dlevel_add).initS(null, false);
						if (weatherItem.get_conflict() != 0U)
						{
							for (int j = list.Count - 1; j >= 0; j--)
							{
								if (((1U << (int)list[j].weather) & weatherItem.get_conflict()) != 0U)
								{
									this.cur_weather_ &= ~(1 << (int)list[j].weather);
									list[j].destruct();
									list.RemoveAt(j);
								}
							}
						}
						list.Add(weatherItem);
						this.AWeather = list.ToArray();
						this.cur_weather_ |= 1 << (int)weather;
						weatherItem.showLog();
						return true;
					}
				}
				else if ((this.cur_weather_ & (1 << (int)weather)) != 0)
				{
					WeatherItem weather2 = this.getWeather(weather);
					int num;
					if (weather2 != null && (num = global::XX.X.isinC<WeatherItem>(this.AWeather, weather2)) >= 0)
					{
						weather2.destruct();
						global::XX.X.splice<WeatherItem>(ref this.AWeather, num, 1);
						this.cur_weather_ &= ~(1 << (int)weather);
						return true;
					}
				}
			}
			return false;
		}

		public bool applyDangerousFromEvent(int v, bool immediate, bool do_not_change_meter_cache = false)
		{
			v = global::XX.X.Mx(0, v);
			this.dlevel = v;
			if (!do_not_change_meter_cache)
			{
				this.fineMeterShowLevel();
			}
			global::XX.X.dl("危険度を " + v.ToString() + " に設定", null, false, false);
			this.changeTo(this.dLevelToNightLevel(this.dlevel), immediate ? 0 : 260);
			return true;
		}

		public void setBattleCount(int v)
		{
			this.battle_count = v;
		}

		public float WindSpeed()
		{
			if ((this.cur_weather_ & 2) == 0)
			{
				return 1f;
			}
			return 1.5f;
		}

		public float PlayerChantSpeed()
		{
			return (float)(((this.cur_weather_ & 2) != 0) ? 2 : 1);
		}

		public float ManaWeedRatio()
		{
			return (float)(((this.cur_weather_ & 32) != 0) ? 3 : (((this.cur_weather_ & 8) != 0) ? 2 : 1)) * (((this.cur_weather_ & 16) != 0) ? 0.25f : 1f);
		}

		public bool hasMistWeather()
		{
			return (this.cur_weather_ & 40) != 0;
		}

		public int add_overdriveable()
		{
			if ((this.cur_weather_ & 32) != 0)
			{
				return 2;
			}
			if ((this.cur_weather_ & 8) == 0)
			{
				return 0;
			}
			return 1;
		}

		public float SpilitMpRatioEn()
		{
			if ((this.cur_weather_ & 16) == 0)
			{
				return 1f;
			}
			return 1.5f;
		}

		public float SpilitMpRatioPr()
		{
			if ((this.cur_weather_ & 16) == 0)
			{
				return 1f;
			}
			return 2f;
		}

		public string add_overdriveable_str(EnemySummoner Smn)
		{
			if ((this.cur_weather_ & 4) != 0)
			{
				WeatherItem weather = this.getWeather(WeatherItem.WEATHER.THUNDER);
				if (weather != null)
				{
					return string.Concat(new string[]
					{
						" <img mesh=\"weather.",
						2.ToString(),
						"\" /><font color=\"0x",
						C32.codeToCodeText(4294966715U),
						"\">+",
						weather.getThunderOverdriveCount(this, Smn).ToString(),
						"</font>"
					});
				}
			}
			return "";
		}

		public int add_overdriveable_count(EnemySummoner Smn)
		{
			if ((this.cur_weather_ & 4) != 0)
			{
				WeatherItem weather = this.getWeather(WeatherItem.WEATHER.THUNDER);
				if (weather != null)
				{
					return weather.getThunderOverdriveCount(this, Smn);
				}
			}
			return 0;
		}

		public float fixEnemyFirstMpRatio(float v)
		{
			if (v >= 0.96f)
			{
				return v;
			}
			if ((this.cur_weather_ & 16) != 0)
			{
				v = global::XX.X.Scr(v, 0.5f);
			}
			return global::XX.X.Mn(v, 0.96f);
		}

		public float applySerRatio(bool is_en)
		{
			if ((this.cur_weather_ & 64) == 0)
			{
				return 1f;
			}
			if (!is_en)
			{
				return 3f;
			}
			return 2.2f;
		}

		public float applyEggRatio()
		{
			if ((this.cur_weather_ & 64) != 0)
			{
				return 4f;
			}
			return 1f;
		}

		public bool isSuddenOnMap(NightController.SummonerData NInfo, Map2d Mp)
		{
			return this.M2D.curMap == Mp && this.isSudden(NInfo);
		}

		public bool suddenEnable()
		{
			return this.isNight() || this.getDayCount(false) >= 3;
		}

		public bool isSudden(NightController.SummonerData NInfo)
		{
			if (!this.suddenEnable())
			{
				return false;
			}
			float num;
			if (this.hasMistWeather())
			{
				num = global::XX.X.NIL(0.4f, 0.75f, (float)this.getDayCount(false), 4f);
			}
			else
			{
				num = global::XX.X.NIL(0.2f, 0.375f, (float)this.getDayCount(false), 4f);
			}
			return (float)NInfo.sudden_level / 255f > 1f - num;
		}

		public static void drawGradeDaia(MeshDrawer Md, float px, float py, int grade)
		{
			if (grade <= 0)
			{
				return;
			}
			int num = 0;
			int num2;
			switch (grade)
			{
			case 5:
			case 6:
			case 7:
			case 8:
				num = 1;
				num2 = grade - 4;
				break;
			case 9:
				num = 3;
				num2 = 2;
				break;
			case 10:
			case 11:
				num = 6;
				num2 = grade - 7;
				break;
			case 12:
			case 13:
				num = 7 | ((grade == 13) ? 8 : 0);
				num2 = 4;
				break;
			default:
				num2 = grade;
				break;
			}
			if (num2 == 1)
			{
				NightController.drawDaia1(Md, px, py, 1.5f, num != 0);
				return;
			}
			if (num2 == 2)
			{
				NightController.drawDaia1(Md, px, py + 30f, 1f, (num & 1) != 0);
				NightController.drawDaia1(Md, px, py - 30f, 1f, (num & 2) != 0);
				return;
			}
			NightController.drawDaia1(Md, px - 30f, py, 1f, (num & 4) != 0);
			NightController.drawDaia1(Md, px, py - 30f, 1f, (num & 2) != 0);
			NightController.drawDaia1(Md, px + 30f, py, 1f, (num & 1) != 0);
			if (num2 == 4)
			{
				NightController.drawDaia1(Md, px, py + 34f, 1f, (num & 8) != 0);
			}
		}

		private static void drawDaia1(MeshDrawer Md, float px, float py, float size_rate = 1f, bool enlarge = false)
		{
			Md.Poly(px, py, 22f * size_rate, 0f, 4, 8f * size_rate, false, 0f, 0f);
			if (enlarge)
			{
				Md.Poly(px, py, 14f * size_rate, 0f, 4, 0f, false, 0f, 0f);
			}
		}

		public float GetLevel(bool real_value = false)
		{
			if (!real_value)
			{
				return this.fade_deplevel;
			}
			if (this.night_level < 2f)
			{
				return this.night_level;
			}
			return this.night_level - (float)((int)(this.night_level / 2f));
		}

		public WeatherItem[] getWeatherArray()
		{
			return this.AWeather;
		}

		public WeatherItem getWeather(WeatherItem.WEATHER wtype)
		{
			for (int i = this.AWeather.Length - 1; i >= 0; i--)
			{
				WeatherItem weatherItem = this.AWeather[i];
				if (weatherItem.weather == wtype)
				{
					return weatherItem;
				}
			}
			return null;
		}

		public int getNoelJuiceQuality(float dlevel_multiple = 1.35f)
		{
			return global::XX.X.Mn(4, (int)((float)this.dlevel * dlevel_multiple / 16f));
		}

		public bool isLockPuppetRevenge(string map_key)
		{
			if (this.ALockPuppetRevenge.Count == 0)
			{
				this.ALockPuppetRevenge.Add("");
			}
			string text = this.ALockPuppetRevenge[0];
			if (text == map_key)
			{
				return false;
			}
			bool flag = true;
			if (this.ALockPuppetRevenge.IndexOf(map_key) < 0)
			{
				if (text == "")
				{
					if (this.lock_puppetrevenge == 0 || NightController.XORSP() < global::XX.X.Mn(0.07f + (float)(this.lock_puppetrevenge - 1) * 0.18f, 0.5f))
					{
						this.ALockPuppetRevenge[0] = map_key;
						flag = false;
					}
				}
				else if (this.lock_puppetrevenge == 0 && NightController.XORSP() < 0.3f)
				{
					this.ALockPuppetRevenge[0] = map_key;
					flag = false;
				}
				this.ALockPuppetRevenge.Add(map_key);
				if (this.ALockPuppetRevenge.Count > 4)
				{
					this.ALockPuppetRevenge.RemoveRange(1, this.ALockPuppetRevenge.Count - 4);
					if (flag)
					{
						this.ALockPuppetRevenge[0] = "";
					}
				}
			}
			return flag;
		}

		public void clearPuppetRevengeCache(bool is_puppet_revenge, string map_key)
		{
			if (this.ALockPuppetRevenge.Count == 0)
			{
				return;
			}
			this.ALockPuppetRevenge.Clear();
			this.ALockPuppetRevenge.Add("");
			if (TX.valid(map_key))
			{
				this.ALockPuppetRevenge.Add(map_key);
			}
			if (is_puppet_revenge)
			{
				this.lock_puppetrevenge = 1;
				return;
			}
			if (this.lock_puppetrevenge >= 1 && this.lock_puppetrevenge < 10)
			{
				this.lock_puppetrevenge += 1;
			}
		}

		public static uint xors()
		{
			return NightController.Xors.get0();
		}

		public static int xors(int i)
		{
			return (int)(NightController.Xors.get0() % (uint)i);
		}

		public static int xorsi()
		{
			return (int)(NightController.Xors.get0() & 2147483647U);
		}

		public static uint randA(uint i)
		{
			return NightController.Xors.randA[(int)(i & 127U)] & 134217727U;
		}

		public static float ranNm(float f1, float r)
		{
			return f1 + r * (-1f + 2f * NightController.XORSP());
		}

		public static float NIXP(float v, float v2)
		{
			return global::XX.X.NI(v, v2, NightController.XORSP());
		}

		public static float XORSP()
		{
			return NightController.Xors.getP();
		}

		public static float XORSPS()
		{
			return (NightController.XORSP() - 0.5f) * 2f;
		}

		public static void shuffle<T>(T[] A, int arraymax = -1)
		{
			arraymax = ((arraymax < 0) ? A.Length : arraymax);
			int num = (arraymax - 1) * 23;
			for (int i = 0; i < num; i++)
			{
				int num2 = (int)((ulong)NightController.Xors.get0() % (ulong)((long)arraymax));
				int num3 = (int)((ulong)NightController.Xors.get0() % (ulong)((long)arraymax));
				int num4 = num2;
				int num5 = num3;
				T t = A[num3];
				T t2 = A[num2];
				A[num4] = t;
				A[num5] = t2;
			}
		}

		public static void shuffle<T>(List<T> A, int arraymax = -1)
		{
			arraymax = ((arraymax < 0) ? A.Count : arraymax);
			int num = (arraymax - 1) * 23;
			for (int i = 0; i < num; i++)
			{
				int num2 = (int)((ulong)NightController.Xors.get0() % (ulong)((long)arraymax));
				int num3 = (int)((ulong)NightController.Xors.get0() % (ulong)((long)arraymax));
				int num4 = num2;
				int num5 = num3;
				T t = A[num3];
				T t2 = A[num2];
				A[num4] = t;
				A[num5] = t2;
			}
		}

		public void addRemovingMst(M2FillingMistDrawer Mst)
		{
			if (this.ARemovingMst == null)
			{
				this.ARemovingMst = new List<M2FillingMistDrawer>(1);
			}
			this.ARemovingMst.Add(Mst);
		}

		public int current_weather_bit
		{
			get
			{
				return this.cur_weather_;
			}
		}

		public bool isLastBattled(M2LpSummon Lp)
		{
			return this.last_battle_lp_key == Lp.cleared_sf_key;
		}

		public float getDangerLevel()
		{
			return global::XX.X.Mn(10f, (float)(this.dlevel + this.dlevel_add) / 16f);
		}

		public int getDangerMeterVal(bool real = false)
		{
			return global::XX.X.Mn(this.dlevel + (real ? 0 : this.dlevel_add), 160);
		}

		public int getDangerAddedVal()
		{
			return this.dlevel_add;
		}

		public bool isNight()
		{
			return this.dlevel % 16 >= 9;
		}

		public int getBattleCount()
		{
			return this.battle_count;
		}

		public int getDayCount(bool limit = true)
		{
			int num = this.dlevel / 16;
			if (!limit)
			{
				return num;
			}
			return global::XX.X.Mn(num, 10);
		}

		private void fineCurrentUiObtainable()
		{
			int num = global::XX.X.Mn(this.reelObtainableCount(false), this.battle_count) + global::XX.X.Mn(this.reelObtainableCount(true), 2 + this.battle_count);
			this.current_ui_obtainable = (byte)global::XX.X.Mx(0, global::XX.X.Mn(255, num));
		}

		private int reelObtainableCount(bool calc_juice_add)
		{
			int num = (calc_juice_add ? (this.dlevel_add / 3) : this.dlevel);
			return (num + 2 + ((3 < num && num < 8) ? 1 : 0)) / 5;
		}

		public readonly NelM2DBase M2D;

		private float fade_deplevel;

		public float night_level;

		private float fade_add;

		private readonly BDic<string, NightController.SummonerData> OSmnData;

		private readonly BDic<string, NightController.SummonerData> OSmnDataThisSession;

		private int battle_count;

		private string last_battle_lp_key = "";

		public const float DANGER_MAX = 10f;

		public const float DANGER_CALC_MAX = 5f;

		private const float meter_def_shift_px_y = 42f;

		public const int LEVEL_ONE_DAY = 16;

		public const int LEVEL_EVENING = 9;

		public const int REEL_GET_INTERVAL = 5;

		public const int REEL_GET_INTERVAL_JUICE_DIV = 3;

		public const int REEL_GET_FIRST = 3;

		public const int DLEVEL_ADD_MAX = 45;

		public static XorsMaker Xors = new XorsMaker(0U, true);

		private const int T_FADE = 40;

		private NightController.UIST state;

		private int t_actv = -40;

		private int t_state;

		private DangerGageDrawer Dgd;

		private bool redraw_dgd_flag;

		private bool pre_drawn;

		private int dlevel;

		private int dlevel_add;

		private int dlevel_add_stock;

		private int pre_dlevel;

		public int first_battle_dlevel;

		private byte reel_obtained;

		private PTCThreadRunnerOnce<EffectItemNel> EF;

		public int reserved_obtainable_grade = -1;

		private int cur_weather_;

		private WeatherItem[] AWeather;

		public const int suddon_reload_day_count = 2;

		public const int noon_sudden_allow_day = 3;

		private byte lock_puppetrevenge;

		private readonly List<string> ALockPuppetRevenge;

		public bool debug_lock_dangerousness;

		public bool debug_allow_night_travel;

		public bool debug_lock_weather;

		private List<M2FillingMistDrawer> ARemovingMst;

		private MeshDrawer MdUi;

		private ValotileRenderer ValotUi;

		private GameObject GobUi;

		private UiReelManager UiReel;

		private MdArranger Ma;

		private int s_phase;

		private byte current_ui_obtainable;

		public sealed class SummonerData
		{
			public SummonerData(ByteArray Ba = null, int vers = 0)
			{
				if (Ba != null)
				{
					this.readBinaryFrom(Ba, vers);
				}
			}

			public void clearSession()
			{
				this.sudden_level = 0;
				this.last_battle_index = 0;
				this.defeated_in_session = false;
				this.SmnCache = null;
				this.night_calced = 0;
			}

			public EnemySummoner getSummoner(string sf_key)
			{
				if (this.SmnCache == null)
				{
					int num = sf_key.IndexOf("..");
					if (num >= 0)
					{
						this.SmnCache = EnemySummoner.Get(TX.slice(sf_key, num + 2), false);
					}
				}
				return this.SmnCache;
			}

			public bool summoner_is_night
			{
				get
				{
					if (this.SmnCache != null && this.SmnCache.only_night)
					{
						this.summoner_is_night_ = true;
					}
					return this.summoner_is_night_;
				}
				set
				{
					this.summoner_is_night_ = value;
				}
			}

			public bool summoner_is_night_real
			{
				get
				{
					return this.summoner_is_night_;
				}
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeByte((this.defeated_in_session ? 1 : 0) | (this.summoner_is_night ? 2 : 0));
				Ba.writeByte((int)this.sudden_level);
				Ba.writeInt(this.fd_favorite ? (-1 - this.defeat_count) : this.defeat_count);
				Ba.writeInt(this.last_battle_index);
				Ba.writeByte((int)this.night_calced);
			}

			public void readBinaryFrom(ByteArray Ba, int vers)
			{
				int num = Ba.readByte();
				this.defeated_in_session = (num & 1) != 0;
				this.summoner_is_night_ = (num & 2) != 0;
				this.sudden_level = (byte)Ba.readByte();
				this.defeat_count = Ba.readInt();
				if (this.defeat_count < 0)
				{
					this.defeat_count = -this.defeat_count - 1;
					this.fd_favorite = true;
				}
				else
				{
					this.fd_favorite = false;
				}
				this.last_battle_index = Ba.readInt();
				this.night_calced = (byte)Ba.readByte();
			}

			public byte sudden_level;

			public int defeat_count;

			public bool fd_favorite;

			public int last_battle_index;

			public bool defeated_in_session;

			private bool summoner_is_night_;

			public byte calced_night_sudden;

			public byte night_calced;

			public EnemySummoner SmnCache;
		}

		public enum UIST
		{
			OFFLINE = -1,
			NONE,
			DLEVEL_SHOW
		}
	}
}

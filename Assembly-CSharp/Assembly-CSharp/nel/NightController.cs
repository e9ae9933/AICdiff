using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using nel.smnp;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public sealed class NightController : IEventWaitListener
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
				return 0.5f + 0.25f * X.ZPOW((float)(num - 4), 5f);
			}
			num -= 9;
			return 1f + X.ZLINE((float)(num - 3), 4f) * 0.5f;
		}

		public NightController(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.OSmnData = new BDic<string, NightController.SummonerData>();
			this.OSmnDataThisSession = new BDic<string, NightController.SummonerData>();
			this.UiDg = new UiDangerousViewer(this.M2D, this);
			this.AWeather = new WeatherItem[0];
			this.ALockPuppetRevenge = new List<string>(4);
			this.FlgTemporaryWeather = new Flagger(delegate(FlaggerT<string> _)
			{
				this.initTemporaryWeather(true);
			}, delegate(FlaggerT<string> _)
			{
				this.initTemporaryWeather(false);
			});
			NightController.Xors.init(false, 0U, 0U, 0U, 0U);
		}

		public void destruct()
		{
			this.destructRemovingMist();
			this.UiDg.destruct();
		}

		public void newGame()
		{
			this.UiDg.deactivate(true);
			NightController.Xors.init(false, 0U, 0U, 0U, 0U);
			this.clear();
		}

		public void clearWithoutNightLevel()
		{
			this.dlevel = (this.dlevel_add = (this.dlevel_add_stock = 0));
			this.fade_add = 0f;
			this.battle_count = 0;
			this.first_battle_dlevel = 0;
			this.UiDg.clearWithoutNightLevel();
			this.cur_weather_ = 0;
			this.FlgTemporaryWeather.Clear();
			this.temporary_weather_back = -1;
			this.last_battle_lp_key = "";
			foreach (KeyValuePair<string, NightController.SummonerData> keyValuePair in this.OSmnDataThisSession)
			{
				keyValuePair.Value.clearSession();
			}
			this.OSmnDataThisSession.Clear();
			this.clearWeather();
		}

		public void clearWeather()
		{
			this.cur_weather_ = 0;
			for (int i = this.AWeather.Length - 1; i >= 0; i--)
			{
				try
				{
					this.AWeather[i].destruct();
				}
				catch
				{
				}
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
			this.UiDg.deactivate(true);
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
			NightController.SummonerData summonerData = X.Get<string, NightController.SummonerData>(this.OSmnData, key);
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
			return this.UiDg.isActive();
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || this.isUiActive();
		}

		private int cur_dlevel(EnemySummoner Smn, bool consider_questentry = true, float dla_ratio = 1f)
		{
			float num = (float)this.dlevel + (float)this.dlevel_add * dla_ratio;
			if (consider_questentry && Smn != null && Smn.QEntry.valid)
			{
				num += (float)Smn.QEntry.add_dlevel;
			}
			return X.IntR(num);
		}

		private int cur_dlevel_nattr(EnemySummoner Smn, float dla_ratio = 1f)
		{
			bool flag = true;
			if (Smn != null && Smn.QEntry.valid)
			{
				flag = Smn.QEntry.nattr == ENATTR.NORMAL;
			}
			return this.cur_dlevel(Smn, flag, dla_ratio);
		}

		public int summoner_enemy_count_addition(EnemySummoner Summoner)
		{
			int num = this.cur_dlevel(Summoner, true, 1f);
			int num2 = num % 16;
			int num3 = X.Mn(5, num / 16);
			int num4;
			if (!this.isNight())
			{
				if (num3 == 0)
				{
					num4 = (num2 + 1) / 3;
				}
				else
				{
					num4 = X.IntC((float)num3 * 1.77f) + ((num2 >= 4) ? 1 : 0);
				}
			}
			else
			{
				num4 = 2 + X.IntC((float)num3 * 2.5f);
			}
			return num4 / this.diff_divide(Summoner);
		}

		public int summoner_max_thunder_odable_appear(SummonerPlayer Smn)
		{
			int num = X.Mn(10, this.cur_dlevel(Smn.Summoner, true, 0f) / 16);
			int num2;
			if (Smn.Summoner.skill_difficulty_restrict == 0)
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
			return X.Mx(0, num2);
		}

		public int summoner_enemy_countmax_addition(SummonerPlayer Smn)
		{
			int num = this.cur_dlevel(Smn.Summoner, true, 1f);
			int num2 = num % 16;
			int num3 = X.Mn(5, num / 16);
			if (!this.isNight())
			{
				return num3 * 2 / this.diff_divide(Smn.Summoner) + ((num2 > 3) ? 1 : 0);
			}
			return (int)((float)num3 * 2.5f + 2f) / this.diff_divide(Smn.Summoner);
		}

		public int summoner_enemy_for_qentry_special_count_min(SummonerPlayer Smn, out float mp_min, out float mp_max)
		{
			float num = (float)(this.cur_dlevel(Smn.Summoner, true, 1f) + 7) / 16f;
			if (!this.isNight())
			{
				mp_min = 0.3f;
				mp_max = 0.55f;
				return X.Mn(2 + (int)(num * 0.66f), 5);
			}
			mp_min = 0.08f;
			mp_max = 0.12f;
			return X.Mn(2 + (int)(num * 1.12f), 7);
		}

		public float summoner_delayonesecond_ratio(SummonerPlayer Smn)
		{
			int num = this.cur_dlevel(Smn.Summoner, true, 1f);
			int num2 = num % 16;
			int num3 = X.Mn(10, num / 16);
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

		public int diff_divide(EnemySummoner Summoner)
		{
			if (Summoner.skill_difficulty_restrict != 0)
			{
				return 1;
			}
			return 2;
		}

		public int summoner_max_addition(SummonerPlayer Smn)
		{
			int num = this.cur_dlevel(Smn.Summoner, true, 1f);
			int num2 = X.Mn(10, num / 16);
			int num3;
			if (!this.isNight())
			{
				num3 = X.IntC((float)X.Mx(0, num2 - 1) * 0.5f);
			}
			else
			{
				num3 = num2 + 1;
			}
			for (int i = this.AWeather.Length - 1; i >= 0; i--)
			{
				num3 += this.AWeather[i].summoner_max_addition();
			}
			num3 = (this.isNight() ? X.Mx(num3, 0) : num3);
			if (num3 < 0)
			{
				return num3;
			}
			return num3 / this.diff_divide(Smn.Summoner);
		}

		public float summoner_mp_addition(SummonerPlayer Smn)
		{
			int num = this.cur_dlevel(Smn.Summoner, true, 1f);
			int num2 = X.Mn(10, num / 16);
			if (!this.isNight())
			{
				int num3 = num2;
				if (num3 != 0)
				{
					if (num3 != 1)
					{
						return 2f + 0.75f * (float)(num2 - 2) + X.NI(0f, 0.45f, X.ZLINE(X.frac((float)num2), 0.7f));
					}
					return X.NI(1f, 1.8f, X.ZLINE(X.frac((float)num2), 0.6f));
				}
				else
				{
					if ((float)num2 < 0.25f)
					{
						return 0f;
					}
					return 0.3f + X.NI(0f, 0.6f, X.ZLINE((float)num2 - 0.25f, 0.4f));
				}
			}
			else
			{
				if (num2 == 0)
				{
					return X.NI(0f, 0.125f, X.ZLINE((float)num2 - 0.75f, 0.25f));
				}
				return 0.125f * (float)num2 + (float)((num2 >= 2) ? 2 : 0) + X.NI(0f, 0.125f, X.ZLINE(X.frac((float)num2) - 0.6f, 0.4f));
			}
		}

		public float summoner_drop_od_enemy_box_ratio(SummonerPlayer Smn)
		{
			int num = this.cur_dlevel(Smn.Summoner, true, 1f);
			return X.NIL(0.2f, 0.5f, (float)num, 80f);
		}

		public int summoner_drop_od_enemy_box_max(SummonerPlayer Smn)
		{
			return X.Mn(X.IntR((float)this.cur_dlevel(Smn.Summoner, true, 1f) / 16f), 3);
		}

		public int summoner_attachable_nattr_max(SummonerPlayer Smn)
		{
			int num = this.cur_dlevel_nattr(Smn.Summoner, 1f);
			bool flag = false;
			if (!flag && num < 16)
			{
				return 0;
			}
			float num2 = (float)(num - 16) * 1.2f / 16f + 1f;
			if (this.isNight())
			{
				num2 = (num2 - 3f) * 0.7f;
			}
			if ((float)num >= 80f)
			{
				num2 += (float)(this.isNight() ? 1 : 3);
			}
			return (int)X.Mx((float)(flag ? 1 : 0), num2);
		}

		public int summoner_attachable_nattr_kind_max(SummonerPlayer Smn)
		{
			int num = this.cur_dlevel_nattr(Smn.Summoner, 1f);
			if (num >= 96)
			{
				return 3;
			}
			if ((float)num < 56f)
			{
				return 1;
			}
			return 2;
		}

		public bool summoner_nattr_attach(SummonerPlayer Smn, SmnEnemyKind K, float smn_xorsp, int nattr_lvl)
		{
			if (nattr_lvl == 255)
			{
				return false;
			}
			int num = this.cur_dlevel_nattr(Smn.Summoner, 1f);
			if (!false && (nattr_lvl > num || NightController.XORSP() < 0.25f))
			{
				return false;
			}
			int num2 = this.summoner_attachable_nattr_kind_max(Smn);
			ENATTR enattr = ~K.EnemyDesc.nattr_decline;
			if ((K.nattr & ENATTR._MATTR) != ENATTR.NORMAL)
			{
				enattr &= ~(ENATTR.FIRE | ENATTR.ICE | ENATTR.THUNDER | ENATTR.SLIMY | ENATTR.ACME);
			}
			ENATTR enattr2 = EnemyAttr.attachKind(enattr, smn_xorsp, num2);
			if (enattr2 > ENATTR.NORMAL)
			{
				K.nattr |= enattr2;
				return true;
			}
			return false;
		}

		public void summoner_nattr_attach_after(SummonerPlayer Smn, SmnEnemyKind K)
		{
			K.nattr &= ~K.EnemyDesc.nattr_decline;
			if ((K.nattr & ~(ENATTR._MAX_RANDOMIZE | ENATTR._FIXED)) == ENATTR.NORMAL)
			{
				return;
			}
			int num = this.cur_dlevel_nattr(Smn.Summoner, 1f);
			int num2 = EnemyAttr.attrKindCount(K.nattr);
			float num3 = X.NIL(0f, 1f, (float)(num - 16) - (float)(num2 - 1) * 8f, 48f);
			K.mp_ratio_addable_max = num3;
			if (num3 <= 0f)
			{
				K.mp_add_weight = 0f;
				return;
			}
			if (this.isNight() ? (num <= 96) : ((float)num <= 54.4f))
			{
				K.mp_add_weight *= 0.3f;
			}
		}

		public float summoner_nattr_atk_add_experience(NelEnemy En)
		{
			int num = this.cur_dlevel(En.Summoner.Summoner, true, 1f);
			return X.NIL(0.8f, 2.5f, (float)(num - 16), 64f);
		}

		public float summoner_nattr_def_add_experience(NelEnemy En)
		{
			int num = this.cur_dlevel(En.Summoner.Summoner, true, 1f);
			return X.NIL(1f, 2.5f, (float)(num - 16), 64f);
		}

		public void summoner_nattr_extend_mp(NelEnemy En, out int extend_maxhp, out int extend_maxmp)
		{
			extend_maxhp = 0;
			extend_maxmp = 0;
			if ((En.nattr & (ENATTR.ATK | ENATTR.DEF | ENATTR.MP_STABLE | ENATTR.FIRE | ENATTR.ICE | ENATTR.THUNDER | ENATTR.SLIMY | ENATTR.ACME)) != ENATTR.NORMAL)
			{
				this.cur_dlevel(En.Summoner.Summoner, true, 1f);
				extend_maxmp = (int)X.NIL(60f, 180f, -16f, 64f);
			}
		}

		public float summoner_drop_item_base_ratio(EnemySummoner Smn)
		{
			if (!Smn.dropable_special_item)
			{
				return 0f;
			}
			int reservedObtainableGrade = this.getReservedObtainableGrade(Smn, true, false);
			float num = 1f;
			int num2 = this.AWeather.Length;
			for (int i = 0; i < num2; i++)
			{
				num *= this.AWeather[i].enemydrop_ratio;
			}
			if (reservedObtainableGrade < Smn.grade)
			{
				num = X.Mn(num, 1f) * 0.5f * X.Mx(0.01f, X.ZLINE((float)reservedObtainableGrade, (float)Smn.grade));
			}
			return num;
		}

		public void run(float fcnt)
		{
			if (this.fade_add != 0f)
			{
				this.night_level = X.VALWALK(this.night_level, this.fade_deplevel, this.fade_add * fcnt);
				bool flag = false;
				if (this.night_level == this.fade_deplevel)
				{
					this.fade_add = 0f;
					flag = true;
				}
				this.fineLevel(flag);
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
				this.fade_add = X.Abs(dep_level - this.night_level) / (float)_fade_maxt;
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

		public int SummonerDefeated(EnemySummoner Smn, int ob_add = -1)
		{
			if (ob_add < 0)
			{
				ob_add = this.getReservedObtainableGrade(Smn, true, true);
			}
			foreach (KeyValuePair<string, NightController.SummonerData> keyValuePair in this.OSmnDataThisSession)
			{
				EnemySummoner smnCache = keyValuePair.Value.SmnCache;
				if (smnCache != null)
				{
					smnCache.flush_memory(false, false, true);
				}
			}
			if (ob_add > 0)
			{
				this.dlevel += ob_add;
				this.M2D.GUILD.progressDangerousness(ob_add);
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

		public void showNightLevelAdditionUI()
		{
			this.UiDg.activateDLevelShow();
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

		public int getReservedObtainableGrade(EnemySummoner Smn, bool consider_restrict = true, bool consider_debug_lock_danger = false)
		{
			M2LpSummon summonedArea = Smn.getSummonedArea();
			if (summonedArea.is_sudden_puppetrevenge)
			{
				return 4;
			}
			if (Smn == null || (consider_debug_lock_danger && this.debug_lock_dangerousness && this.M2D.debug_listener_created) || this.M2D.isGameOverActive())
			{
				return 0;
			}
			if (this.reserved_obtainable_grade >= 0)
			{
				return this.reserved_obtainable_grade;
			}
			return this.getObtainableGrade(Smn, summonedArea.Mp, false);
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
			return X.Mx(0, X.Mn(base_grade, this.battle_count - Info.last_battle_index - ((this.getDayCount(true) >= 3 || this.first_battle_dlevel >= 16) ? 1 : 0)));
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
			int num2 = X.Mn(count, X.IntC((float)num / (float)(grade + 1)));
			count -= num2;
			this.dlevel_add = X.Mn(this.dlevel_add + (grade + 1) * num2, 45);
			if (show_ui)
			{
				if (!this.isUiActive())
				{
					this.UiDg.activateDLevelShow();
				}
				else
				{
					this.UiDg.fineCurrentUiObtainable();
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
			this.dlevel_add = X.Mn(v, 45);
		}

		public ByteArray writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(8);
			Ba.writeFloat(this.fade_deplevel);
			this.UiDg.writeBinaryTo(Ba);
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
			this.UiDg.writeBinaryTo2(Ba);
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
			num = X.Mn(255, this.ALockPuppetRevenge.Count);
			Ba.writeByte(num);
			for (int j = 0; j < num; j++)
			{
				Ba.writePascalString(this.ALockPuppetRevenge[j], "utf-8");
			}
			return Ba;
		}

		public ByteReader readBinaryFrom(ByteReader Ba)
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
				this.UiDg.readBinaryFrom(Ba, num);
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
							this.battle_count = X.Mx(summonerData.last_battle_index, this.battle_count);
						}
					}
					this.last_battle_lp_key = Ba.readPascalString("utf-8", false);
					Ba.readPascalString("utf-8", false);
					this.UiDg.readBinaryFrom2(Ba, num);
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
							weatherItem.start_dlevel = X.Mx(0, weatherItem.start_dlevel - this.dlevel_add);
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
					this.dlevel_add = X.Mn(this.dlevel_add, 45);
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
			this.UiDg.deactivate(true);
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

		public void initTemporaryWeather(string flag_key, int _weather)
		{
			if (_weather < 0)
			{
				this.FlgTemporaryWeather.Rem(flag_key);
				return;
			}
			this.FlgTemporaryWeather.Add(flag_key);
			if (_weather > 0)
			{
				for (int i = 0; i < 7; i++)
				{
					int num = 1 << i;
					if ((_weather & num) != 0)
					{
						WeatherItem.WEATHER weather = (WeatherItem.WEATHER)i;
						if (!this.hasWeather(weather))
						{
							this.applyWeatherDebug(weather.ToString());
						}
					}
				}
			}
		}

		private void initTemporaryWeather(bool flag)
		{
			if (!flag)
			{
				if (this.temporary_weather_back >= 0)
				{
					uint num = (uint)this.temporary_weather_back;
					List<WeatherItem> list = new List<WeatherItem>(X.bit_count(num));
					for (int i = 0; i < 7; i++)
					{
						WeatherItem.WEATHER weather = (WeatherItem.WEATHER)i;
						if ((num & (1U << i)) != 0U)
						{
							WeatherItem weatherItem = this.getWeather(weather);
							if (weatherItem == null)
							{
								weatherItem = new WeatherItem(weather, this.dlevel + this.dlevel_add).initS(null, false);
								weatherItem.showLog();
							}
							list.Add(weatherItem);
						}
						else
						{
							WeatherItem weather2 = this.getWeather(weather);
							if (weather2 != null)
							{
								weather2.destruct();
							}
						}
					}
					this.AWeather = list.ToArray();
					this.cur_weather_ = (int)num;
					return;
				}
			}
			else
			{
				this.temporary_weather_back = (short)this.cur_weather_;
			}
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
					if (weather2 != null && (num = X.isinC<WeatherItem>(this.AWeather, weather2)) >= 0)
					{
						weather2.destruct();
						X.splice<WeatherItem>(ref this.AWeather, num, 1);
						this.cur_weather_ &= ~(1 << (int)weather);
						return true;
					}
				}
			}
			return false;
		}

		public bool applyDangerousFromEvent(int v, bool immediate, bool do_not_change_meter_cache = false, bool no_set_nightlevel = false)
		{
			v = X.Mx(0, v);
			this.dlevel = v;
			if (!do_not_change_meter_cache)
			{
				this.UiDg.fineMeterShowLevel();
			}
			X.dl("危険度を " + v.ToString() + " に設定", null, false, false);
			if (!no_set_nightlevel)
			{
				this.changeTo(this.dLevelToNightLevel(this.dlevel), immediate ? 0 : 260);
			}
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

		public float PlayerLockonRadius()
		{
			if ((this.cur_weather_ & 32) != 0)
			{
				return 0.5f;
			}
			if ((this.cur_weather_ & 8) == 0)
			{
				return 1f;
			}
			return 0.75f;
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
				v = X.Scr(v, 0.5f);
			}
			return X.Mn(v, 0.96f);
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
				num = X.NIL(0.4f, 0.75f, (float)this.getDayCount(false), 4f);
			}
			else
			{
				num = X.NIL(0.2f, 0.375f, (float)this.getDayCount(false), 4f);
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
			return X.Mn(4, (int)((float)this.dlevel * dlevel_multiple / 16f));
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
					if (this.lock_puppetrevenge == 0 || NightController.XORSP() < X.Mn(0.07f + (float)(this.lock_puppetrevenge - 1) * 0.18f, 0.5f))
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
			return X.NI(v, v2, NightController.XORSP());
		}

		public static float XORSP()
		{
			return NightController.Xors.XORSP();
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

		public int getDefeatedCount(string summoner_key)
		{
			NightController.SummonerData summonerData;
			if (this.OSmnData.TryGetValue(summoner_key, out summonerData))
			{
				return summonerData.defeat_count;
			}
			return 0;
		}

		public bool isLastBattled(M2LpSummon Lp)
		{
			return this.last_battle_lp_key == Lp.cleared_sf_key;
		}

		public float getDangerLevel()
		{
			return X.Mn(10f, (float)(this.dlevel + this.dlevel_add) / 16f);
		}

		public int getDangerMeterVal(bool real = false, bool raw = false)
		{
			if (raw)
			{
				return this.dlevel + (real ? 0 : this.dlevel_add);
			}
			return X.Mn(this.dlevel + (real ? 0 : this.dlevel_add), 160);
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
			return X.Mn(num, 10);
		}

		public int reelObtainableCount(bool calc_juice_add)
		{
			int num = (calc_juice_add ? (this.dlevel_add / 3) : this.dlevel);
			return (num + 2 + ((3 < num && num < 8) ? 1 : 0)) / 5;
		}

		public readonly NelM2DBase M2D;

		public readonly UiDangerousViewer UiDg;

		private float fade_deplevel;

		public float night_level;

		private float fade_add;

		private readonly BDic<string, NightController.SummonerData> OSmnData;

		private readonly BDic<string, NightController.SummonerData> OSmnDataThisSession;

		private int battle_count;

		private string last_battle_lp_key = "";

		public const float DANGER_MAX = 10f;

		public const float DANGER_CALC_MAX = 5f;

		public const int LEVEL_ONE_DAY = 16;

		public const int LEVEL_EVENING = 9;

		public const int REEL_GET_INTERVAL = 5;

		public const int REEL_GET_INTERVAL_JUICE_DIV = 3;

		public const int REEL_GET_FIRST = 3;

		public const int DLEVEL_ADD_MAX = 45;

		public static XorsMaker Xors = new XorsMaker(0U, true);

		private int dlevel;

		private int dlevel_add;

		private int dlevel_add_stock;

		public int first_battle_dlevel;

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

		private Flagger FlgTemporaryWeather;

		private short temporary_weather_back = -1;

		private List<M2FillingMistDrawer> ARemovingMst;

		public sealed class SummonerData
		{
			public SummonerData(ByteReader Ba = null, int vers = 0)
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

			public void readBinaryFrom(ByteReader Ba, int vers)
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
	}
}

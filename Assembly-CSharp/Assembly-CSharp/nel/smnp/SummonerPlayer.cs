using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel.smnp
{
	public class SummonerPlayer : IRunAndDestroy
	{
		public SummonerPlayer(EnemySummoner _Summoner, EfParticleFuncCalc _FuncBase, CsvReaderA _CR, out bool bgm_replaced, bool auto_prepare_content = true)
		{
			this.Summoner = _Summoner;
			this.FuncBase = _FuncBase;
			this.CR = _CR;
			this.Lp = this.Summoner.getSummonedArea();
			this.Mp = this.Lp.Mp;
			this.runner_key_ = "SummonerPlayer:" + this.key;
			this.Mp.addRunnerObject(this);
			this.ran_session = SummonerPlayer.xors(16777215);
			if (SummonerPlayer.VPTemp == null)
			{
				SummonerPlayer.VPTemp = new VariableP(1);
			}
			this.max_enemy_appear_whole = 99;
			this.Adecline_enemyid_buf = new List<string>(1);
			this.AASummoned = new List<List<NelEnemy>>();
			this.AOverdrived = new List<NelEnemy>(4);
			this.OFirstPos = new BDic<NelEnemy, SummonedInfo>();
			this.OMaxEnemyAppear = new BDic<string, EnAppearMax>();
			this.OFnCheckEnemy = new BDic<NelEnemy.FnCheckEnemy, int>();
			this.Asplitter_title = new List<SummonerPlayer.SplitterTerm>(1);
			this.AReel = this.Summoner.getItemReelVector();
			this.AReelSecretSrc = this.Summoner.getItemReelVectorForSecret();
			this.replace_reel_secret_to_lower = this.Summoner.replace_secret_to_lower;
			SummonerPlayer.golemtoy_lock_delay = 0f;
			this.golemtoy_lock_delay_maxt = 300f;
			bgm_replaced = false;
			COOK.setSF("_BATTLE_STARTED", COOK.getSF("_BATTLE_STARTED") + 1);
			UIPicture.Recheck(30);
			this.enemy_index = 0;
			this.M2D.areaTitleHideProgress();
			this.OFirstPos.Clear();
			this.AASummoned.Clear();
			this.delay = 50f;
			if (this.Summoner.QEntry.valid && this.Summoner.QEntry.weather > WeatherItem.WEATHER.NORMAL)
			{
				this.M2D.NightCon.initTemporaryWeather(this.runner_key_, 1 << (int)this.Summoner.QEntry.weather);
			}
			this.AOverdrived.Clear();
			this.Summoner.fatal_key = null;
			this.bgm_replace_when_close = true;
			this.current_splitter_id = 0;
			this.bgm_replace = null;
			this.bgm_block_to = null;
			this.delay_one_second = (this.delay_one = -1f);
			this.weight_add = 0f;
			if (auto_prepare_content)
			{
				this.prepareEnemyConent(ref bgm_replaced);
			}
		}

		protected virtual void prepareEnemyConentFromScript(List<SmnEnemyKind> AKindL, List<SmnPoint> ASmnPosL, out BDic<string, int[]> OEnemyCountMax, ref int appear_add, out int max_enemy_count, ref int count_add, out int splitter_id, out int countadd_priority, out bool drop_all_reels_after, out bool force_can_get_whole_reels, out bool odable_enemy_exist)
		{
			CsvReaderA csvReaderA = this.CR;
			csvReaderA.VarCon.removeTemp();
			csvReaderA.VarCon.define("_here", this.key, true);
			csvReaderA.VarCon.define("_map", this.Lp.Mp.key, true);
			OEnemyCountMax = null;
			max_enemy_count = 99;
			splitter_id = 0;
			countadd_priority = 1;
			ENATTR enattr = ENATTR.NORMAL;
			drop_all_reels_after = false;
			force_can_get_whole_reels = false;
			odable_enemy_exist = false;
			NightController nightCon = this.Lp.nM2D.NightCon;
			if (this.QEntry.valid && this.QEntry.fix_enemykind > 0)
			{
				EnemySummonerManager manager = this.Summoner.GetManager();
				if (manager != null)
				{
					bool flag = (this.QEntry.fix_enemykind & int.MinValue) != 0;
					string text = FEnum<ENEMYID>.ToStr((ENEMYID)(this.QEntry.fix_enemykind & int.MaxValue));
					Vector2 enemyPower = manager.getEnemyPower(text);
					int num = X.Mx(1, X.IntR(3.6f - X.Abs(enemyPower.x) - (float)(flag ? 3 : 0)));
					this.OMaxEnemyAppear[(flag ? "OD_" : "") + text] = new EnAppearMax(X.Mx(1, X.IntR((float)num) + appear_add), (ENEMYID)0U, false);
				}
			}
			bool flag2 = false;
			while (csvReaderA.read())
			{
				flag2 = false;
				string cmd = csvReaderA.cmd;
				if (cmd != null)
				{
					uint num2 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
					if (num2 <= 1205246472U)
					{
						if (num2 <= 441099668U)
						{
							if (num2 <= 208699951U)
							{
								if (num2 != 113502404U)
								{
									if (num2 == 208699951U)
									{
										if (cmd == "%BGM_OVERRIDE_KEY")
										{
											this.bgm_block_ovr = csvReaderA._2;
										}
									}
								}
								else if (cmd == "%INITIAL_DELAY")
								{
									this.delay = this.Calc(csvReaderA, 1, "0", "ZLINE");
								}
							}
							else if (num2 != 278310300U)
							{
								if (num2 != 430789690U)
								{
									if (num2 == 441099668U)
									{
										if (cmd == "%DELAY_ONE")
										{
											this.delay_one = this.Calc(csvReaderA, 1, "0", "ZLINE");
											this.delay_one_second = this.delay_one * nightCon.summoner_delayonesecond_ratio(this);
										}
									}
								}
								else if (cmd == "%MAX_CNT")
								{
									if (csvReaderA.clength == 3)
									{
										if (!this.QEntry.kindMatch(csvReaderA._1))
										{
											if (OEnemyCountMax == null)
											{
												OEnemyCountMax = new BDic<string, int[]>(1);
											}
											float num3 = this.CalcF(csvReaderA, ref flag2, 2, "99", "ZLINE");
											if (!flag2)
											{
												num3 += (float)nightCon.summoner_enemy_countmax_addition(this);
											}
											Dictionary<string, int[]> dictionary = OEnemyCountMax;
											string _ = csvReaderA._1;
											int[] array = new int[2];
											array[0] = X.IntR(num3);
											dictionary[_] = array;
										}
									}
									else
									{
										float num4 = this.CalcF(csvReaderA, ref flag2, 1, "99", "ZLINE");
										if (!flag2)
										{
											num4 += (float)nightCon.summoner_enemy_countmax_addition(this);
										}
										max_enemy_count = X.IntR(num4);
									}
								}
							}
							else if (cmd == "%ENATTR")
							{
								enattr = ENATTR.NORMAL;
								for (int i = 1; i < csvReaderA.clength; i++)
								{
									string text2 = csvReaderA.getIndex(i);
									if (TX.isStart(text2, '!'))
									{
										text2 = TX.slice(text2, 1);
										enattr |= ENATTR._FIXED;
									}
									ENATTR enattr2;
									if (!FEnum<ENATTR>.TryParse(text2, out enattr2, true))
									{
										csvReaderA.tError("不明な ENATTR ID: " + csvReaderA.getIndex(i));
									}
									else
									{
										enattr |= enattr2;
									}
								}
							}
						}
						else if (num2 <= 943963272U)
						{
							if (num2 != 667981029U)
							{
								if (num2 == 943963272U)
								{
									if (cmd == "%DELAY_FILLED")
									{
										this.delay_filled = this.Calc(csvReaderA, 1, "0", "ZLINE");
									}
								}
							}
							else if (cmd == "%BGM_REPLACE_WHEN_CLOSE")
							{
								this.bgm_replace_when_close = TX.eval(csvReaderA._1, "") != 0.0;
							}
						}
						else if (num2 != 1015927979U)
						{
							if (num2 != 1066748529U)
							{
								if (num2 == 1205246472U)
								{
									if (cmd == "%PUPPETREVENGE")
									{
										if (this.Lp.is_sudden_puppetrevenge)
										{
											CsvReaderA csvReaderA2 = this.Summoner.createCRPuppetRevenge(false);
											this.Summoner.fineReelData(csvReaderA._1, ref this.AReel, ref this.AReelSecretSrc, ref this.replace_reel_secret_to_lower);
											csvReaderA = csvReaderA2;
											csvReaderA.seek_set(0);
											force_can_get_whole_reels = (drop_all_reels_after = true);
										}
									}
								}
							}
							else if (cmd == "%REPLACE_BGM")
							{
								if (csvReaderA.clength < 3 || TX.eval(csvReaderA.slice_join(2, " ", ""), "") != 0.0)
								{
									this.bgm_replace = csvReaderA._1;
								}
							}
						}
						else if (cmd == "%ADD_COUNT")
						{
							string text3 = csvReaderA.slice_join(1, " ", "");
							if (TX.isStart(text3, "!", 0))
							{
								text3 = TX.slice(text3, 1);
								count_add = 0;
							}
							count_add += (int)this.CalcScript(text3, "0", "ZLINE");
						}
					}
					else
					{
						if (num2 <= 1843910277U)
						{
							if (num2 <= 1684797004U)
							{
								if (num2 != 1242513950U)
								{
									if (num2 != 1684797004U)
									{
										continue;
									}
									if (!(cmd == "%GOLEMTOY_DIVIDE"))
									{
										continue;
									}
									SummonerPlayer.golemtoy_divide_count = (int)this.Calc(csvReaderA, 1, SummonerPlayer.golemtoy_divide_count.ToString(), "ZLINE");
									continue;
								}
								else
								{
									if (!(cmd == "%MAX_APPEAR"))
									{
										continue;
									}
									if (csvReaderA.clength != 3)
									{
										float num5 = this.CalcF(csvReaderA, ref flag2, 1, "99", "ZLINE");
										if (!flag2)
										{
											num5 = X.Mx(1f, num5 + (float)appear_add);
										}
										this.max_enemy_appear_whole = X.Mx(2, X.IntR(num5));
										continue;
									}
									if (!this.QEntry.kindMatch(csvReaderA._1))
									{
										float num6 = this.CalcF(csvReaderA, ref flag2, 2, "99", "ZLINE");
										if (!flag2)
										{
											num6 = X.Mx(X.Mn(2f, num6), num6 + (float)appear_add);
										}
										string text4 = csvReaderA._1;
										bool flag3 = false;
										if (TX.isStart(text4, "OD_", 0))
										{
											text4 = TX.slice(text4, 3);
											flag3 = true;
										}
										this.OMaxEnemyAppear[text4] = new EnAppearMax(X.Mx(1, X.IntR(num6)), (ENEMYID)0U, flag3);
										continue;
									}
									continue;
								}
							}
							else if (num2 != 1717652112U)
							{
								if (num2 != 1728610943U)
								{
									if (num2 != 1843910277U)
									{
										continue;
									}
									if (!(cmd == "%POS_LN"))
									{
										continue;
									}
								}
								else
								{
									if (!(cmd == "%EN_OD"))
									{
										continue;
									}
									goto IL_086B;
								}
							}
							else
							{
								if (!(cmd == "%BGM_BLOCK"))
								{
									continue;
								}
								this.bgm_block_to = csvReaderA._1;
								continue;
							}
						}
						else if (num2 <= 2619452609U)
						{
							if (num2 != 2064310418U)
							{
								if (num2 != 2459156378U)
								{
									if (num2 != 2619452609U)
									{
										continue;
									}
									if (!(cmd == "%NEXT_SCRIPT"))
									{
										continue;
									}
									this.next_script_key = csvReaderA._1;
									continue;
								}
								else
								{
									if (!(cmd == "%FATAL"))
									{
										continue;
									}
									if (csvReaderA.clength < 3 || TX.eval(csvReaderA.slice_join(2, " ", ""), "") != 0.0)
									{
										this.Summoner.fatal_key = csvReaderA._1;
										X.dl("フェイタルキー " + this.Summoner.fatal_key, null, false, false);
										continue;
									}
									continue;
								}
							}
							else
							{
								if (!(cmd == "%EN_HR"))
								{
									continue;
								}
								splitter_id++;
								this.Asplitter_title.Add(new SummonerPlayer.SplitterTerm(csvReaderA, (int)this.delay_one_second));
								continue;
							}
						}
						else if (num2 != 3719324707U)
						{
							if (num2 != 4074695482U)
							{
								if (num2 != 4090079685U)
								{
									continue;
								}
								if (!(cmd == "%DELAY_ONE_FIRST"))
								{
									continue;
								}
								this.delay_one = this.Calc(csvReaderA, 1, "0", "ZLINE");
								continue;
							}
							else if (!(cmd == "%POS"))
							{
								continue;
							}
						}
						else
						{
							if (!(cmd == "%EN"))
							{
								continue;
							}
							goto IL_086B;
						}
						M2LabelPoint lp = this.Lp;
						int num7 = 0;
						if (csvReaderA.cmd == "%POS_LN")
						{
							M2LabelPoint m2LabelPoint = this.Mp.getPoint(csvReaderA._1, false) ?? lp;
							num7 = 1;
						}
						SmnPoint smnPoint = new SmnPoint(this, csvReaderA.Nm(1 + num7, 0f), csvReaderA.Nm(2 + num7, 0f), this.Calc(csvReaderA, 3 + num7, "1", "ZLINE"), SummonerPlayer.XORSP(), csvReaderA.slice(4 + num7, -1000));
						ASmnPosL.Add(smnPoint);
						continue;
						IL_086B:
						if (csvReaderA.IntE(5, 1) != 0)
						{
							ENEMYID enemyid;
							if (!FEnum<ENEMYID>.TryParse(csvReaderA._1U, out enemyid, true))
							{
								csvReaderA.tError("不明なID: " + csvReaderA._1);
							}
							else
							{
								bool flag4 = csvReaderA.cmd == "%EN_OD";
								int num8;
								flag2 = this.Summoner.getEnemyCountFromScript(csvReaderA, out num8, ref count_add, nightCon);
								if (num8 > 0)
								{
									SmnEnemyKind smnEnemyKind = new SmnEnemyKind(csvReaderA._1U, num8, splitter_id, this.Calc(csvReaderA, 3, "0.5", "ZLINE"), this.Calc(csvReaderA, 4, "0", "ZLINE"), csvReaderA._5, this.Calc(csvReaderA, 6, "1", "ZLINE"), this.Calc(csvReaderA, 7, "1", "ZLINE"), enattr);
									if (!smnEnemyKind.EnemyDesc.no_replacable_by_quest && this.QEntry.valid && this.QEntry.fix_enemykind >= 0)
									{
										ENEMYID fix_enemykind = (ENEMYID)this.QEntry.fix_enemykind;
										string text5 = fix_enemykind.ToString();
										NDAT.EnemyDescryption typeAndId = NDAT.getTypeAndId(text5);
										if (typeAndId.valid && (!flag4 || typeAndId.overdriveable))
										{
											smnEnemyKind.enemyid = text5;
											smnEnemyKind.EnemyDesc = typeAndId;
										}
									}
									AKindL.Add(smnEnemyKind);
									if (flag2)
									{
										smnEnemyKind.count_add_weight = 0f;
									}
									else
									{
										countadd_priority = X.Mn(countadd_priority, num8);
									}
									if (flag4)
									{
										smnEnemyKind.pre_overdrive = true;
									}
									else if (smnEnemyKind.EnemyDesc.overdriveable)
									{
										odable_enemy_exist = true;
									}
								}
							}
						}
					}
				}
			}
			if (ASmnPosL.Count == 0)
			{
				ASmnPosL.Add(new SmnPoint(this, 0f, 0f, 1f, SummonerPlayer.XORSP(), null));
			}
		}

		protected virtual void prepareEnemyConent(ref bool bgm_replaced)
		{
			if (this.CR == null)
			{
				return;
			}
			this.CR.seek_set(this.Summoner.get_main_data_line());
			this.prepareEnemyConentInner(ref bgm_replaced);
		}

		protected void prepareEnemyConentInner(ref bool bgm_replaced)
		{
			NelM2DBase nelM2DBase = this.Lp.Mp.M2D as NelM2DBase;
			List<SmnEnemyKind> list = new List<SmnEnemyKind>(4);
			List<SmnPoint> list2 = new List<SmnPoint>();
			this.Asplitter_title.Clear();
			this.Asplitter_title.Add(default(SummonerPlayer.SplitterTerm));
			NightController nightCon = this.Lp.nM2D.NightCon;
			int num;
			nelM2DBase.NightCon.SummonerInited(this.Summoner, this.Lp.Mp, out num);
			int num2 = nightCon.summoner_enemy_count_addition(this.Summoner);
			int num3 = nightCon.summoner_max_addition(this);
			int num4 = nightCon.summoner_attachable_nattr_max(this);
			this.max_enemy_appear_whole = X.Mx(2, 3 + num3);
			this.thunder_odable = this.Summoner.get_thunder_odable_def() + nightCon.summoner_max_thunder_odable_appear(this);
			this.bgm_block_ovr = null;
			this.Summoner.GetManager();
			BDic<string, int[]> bdic;
			int num5;
			int num6;
			int num7;
			bool flag;
			bool flag2;
			this.prepareEnemyConentFromScript(list, list2, out bdic, ref num3, out num5, ref num2, out num6, out num7, out this.drop_all_reels_after, out flag, out flag2);
			List<SmnEnemyKind> list3 = new List<SmnEnemyKind>(list.Count);
			list3.Clear();
			list3.AddRange(list);
			int num8 = 0;
			float num9 = 0f;
			list.Clear();
			List<int[]> list4 = new List<int[]>(2);
			for (int i = list3.Count - 1; i >= 0; i--)
			{
				SmnEnemyKind smnEnemyKind = list3[i];
				list4.Clear();
				int num10 = 999;
				if (bdic != null)
				{
					foreach (KeyValuePair<string, int[]> keyValuePair in bdic)
					{
						if (smnEnemyKind.isSame(keyValuePair.Key))
						{
							list4.Add(keyValuePair.Value);
							num10 = X.Mn(keyValuePair.Value[0] - keyValuePair.Value[1], num10);
						}
					}
				}
				int num11 = X.Mn(num10, smnEnemyKind.def_count);
				if (num10 <= smnEnemyKind.def_count)
				{
					smnEnemyKind.count_add_weight = 0f;
				}
				for (int j = list4.Count - 1; j >= 0; j--)
				{
					list4[j][1] += num11;
				}
				if (!smnEnemyKind.count_fix)
				{
					num9 += smnEnemyKind.count_add_weight;
				}
				if (smnEnemyKind.pre_overdrive)
				{
					num8 += num11;
				}
				while (--num11 >= 0)
				{
					list.Add(new SmnEnemyKind(smnEnemyKind));
				}
			}
			if (num9 > 0f && list3.Count > 0)
			{
				SummonerPlayer.shuffle<SmnEnemyKind>(list3, -1);
				int num12 = 0;
				while (num2 > 0 && num9 > 0f && ++num12 < 300)
				{
					SmnEnemyKind smnEnemyKind2 = null;
					if (num7 <= 0)
					{
						int num13 = 1;
						for (int k = list3.Count - 1; k >= 0; k--)
						{
							SmnEnemyKind smnEnemyKind3 = list3[k];
							if (!smnEnemyKind3.count_fix)
							{
								num13 = X.Mn(num13, smnEnemyKind3.def_count);
								if (smnEnemyKind3.def_count == num7)
								{
									smnEnemyKind2 = smnEnemyKind3;
									smnEnemyKind2.def_count = 1;
									break;
								}
							}
						}
						num7 = num13;
					}
					if (smnEnemyKind2 == null && num7 > 0)
					{
						float num14 = SummonerPlayer.XORSP() * num9;
						for (int k = list3.Count - 1; k >= 0; k--)
						{
							SmnEnemyKind smnEnemyKind4 = list3[k];
							if (smnEnemyKind4.count_add_weight > 0f)
							{
								num14 -= smnEnemyKind4.count_add_weight;
								if (num14 <= 0.0002f)
								{
									smnEnemyKind2 = smnEnemyKind4;
									break;
								}
							}
						}
					}
					if (smnEnemyKind2 != null)
					{
						num2--;
						SmnEnemyKind smnEnemyKind5 = new SmnEnemyKind(smnEnemyKind2);
						smnEnemyKind5.pre_overdrive = false;
						list.Add(smnEnemyKind5);
						if (bdic != null)
						{
							bool flag3 = false;
							foreach (KeyValuePair<string, int[]> keyValuePair2 in bdic)
							{
								if (smnEnemyKind5.isSame(keyValuePair2.Key))
								{
									int[] value = keyValuePair2.Value;
									int num15 = 1;
									int num16 = value[num15] + 1;
									value[num15] = num16;
									if (num16 >= keyValuePair2.Value[0])
									{
										flag3 = true;
									}
								}
							}
							if (flag3)
							{
								num9 -= smnEnemyKind2.count_add_weight;
								smnEnemyKind2.count_add_weight = 0f;
							}
						}
					}
				}
			}
			list3.Clear();
			for (int l = 0; l < 2; l++)
			{
				if (flag2 && num > 0)
				{
					int count = list.Count;
					int[] array = X.makeCountUpArray(count, 0, 1);
					SummonerPlayer.shuffle<int>(array, count);
					for (int m = 0; m < count; m++)
					{
						int num17 = array[m];
						SmnEnemyKind smnEnemyKind6 = list[num17];
						NDAT.EnemyDescryption enemyDesc = smnEnemyKind6.EnemyDesc;
						if (!smnEnemyKind6.pre_overdrive && !smnEnemyKind6.thunder_overdrive && enemyDesc.overdriveable)
						{
							smnEnemyKind6.thunder_overdrive = true;
							list3.Add(smnEnemyKind6);
							num8++;
							if (--num <= 0)
							{
								break;
							}
						}
					}
				}
				if (l != 0)
				{
					break;
				}
				if (list.Count > num5)
				{
					for (int n = list3.Count - 1; n >= 0; n--)
					{
						list.Remove(list3[n]);
					}
					SummonerPlayer.shuffle<SmnEnemyKind>(list, -1);
					int num18 = list.Count - (num5 + list3.Count);
					if (num18 > 0)
					{
						list.RemoveRange(num5 + list3.Count, num18);
					}
					for (int num19 = list3.Count - 1; num19 >= 0; num19--)
					{
						list.Add(list3[num19]);
					}
				}
				else
				{
					SummonerPlayer.shuffle<SmnEnemyKind>(list, -1);
				}
				if (!this.QEntry.valid || this.QEntry.fix_enemykind < 0)
				{
					break;
				}
				float num21;
				float num22;
				int num20 = nightCon.summoner_enemy_for_qentry_special_count_min(this, out num21, out num22);
				ENEMYID fix_enemykind = (ENEMYID)this.QEntry.fix_enemykind;
				int num23 = 0;
				for (int num24 = list.Count - 1; num24 >= 0; num24--)
				{
					SmnEnemyKind smnEnemyKind7 = list[num24];
					if (!this.is_follower(smnEnemyKind7, null))
					{
						num23 = X.Mx(num23, smnEnemyKind7.splitter_id);
					}
					if (smnEnemyKind7.isSame(fix_enemykind))
					{
						num20--;
					}
				}
				if (num20 <= 0)
				{
					break;
				}
				while (--num20 >= 0)
				{
					SmnEnemyKind smnEnemyKind8 = new SmnEnemyKind(FEnum<ENEMYID>.ToStr(fix_enemykind & (ENEMYID)2147483647U), 1, num6, num21, num22, "", 0f, 3f, ENATTR.NORMAL)
					{
						pre_overdrive = ((fix_enemykind & (ENEMYID)2147483648U) > (ENEMYID)0U)
					};
					list.Add(smnEnemyKind8);
				}
			}
			if (num6 >= 1)
			{
				list.Sort(new Comparison<SmnEnemyKind>(this.fnSortKind));
			}
			list3.Clear();
			this.prepareEnemyEnAttr(list, list3, num4);
			for (int num25 = list.Count - 1; num25 >= 0; num25--)
			{
				nightCon.summoner_nattr_attach_after(this, list[num25]);
			}
			list3.Clear();
			float num26 = 0f;
			for (int num27 = list.Count - 1; num27 >= 0; num27--)
			{
				num26 += list[num27].mp_add_weight;
			}
			if (num26 > 0f)
			{
				float num28 = nightCon.summoner_mp_addition(this);
				list3.AddRange(list);
				int num29 = list3.Count;
				int num30 = 0;
				int num31 = X.Mx(1, X.IntC((float)(nightCon.getDangerMeterVal(false, false) / 16)));
				while (num28 > 0f && num29 > 0 && num26 > 0f && ++num30 < 300)
				{
					float num32 = SummonerPlayer.XORSP() * num26;
					int num33 = 0;
					while (num33 < num29)
					{
						SmnEnemyKind smnEnemyKind9 = list3[num33];
						num32 -= smnEnemyKind9.mp_add_weight;
						if (num32 <= 0.0002f)
						{
							float num34 = X.MMX(0f, 1f - smnEnemyKind9.mp_ratio_min, smnEnemyKind9.mp_ratio_addable_max);
							if (num34 > 0f)
							{
								float num35 = X.Mn(X.MMX(0.125f, num34 * SummonerPlayer.NIXP(0.5f, 1f), num34), num28);
								smnEnemyKind9.mp_ratio_min = X.saturate(smnEnemyKind9.mp_ratio_min + num35);
								smnEnemyKind9.mp_ratio_max = X.saturate(smnEnemyKind9.mp_ratio_max + num35);
								smnEnemyKind9.mp_ratio_addable_max -= num35;
								num28 -= num35;
								SmnEnemyKind smnEnemyKind10 = smnEnemyKind9;
								smnEnemyKind10.mp_added += 1;
							}
							else
							{
								smnEnemyKind9.mp_added = (byte)num31;
							}
							if ((int)smnEnemyKind9.mp_added >= num31 || smnEnemyKind9.mp_ratio_addable_max <= 0f)
							{
								list3.RemoveAt(num33);
								num29--;
								num26 -= smnEnemyKind9.mp_add_weight;
								break;
							}
							break;
						}
						else
						{
							num33++;
						}
					}
				}
			}
			this.AReelAfterWholeDefeat = null;
			if (this.AReel != null && list.Count > 0 && (!nightCon.alreadyCleardInThisSession(this.Lp) || flag))
			{
				int num36 = list.Count;
				list3.Clear();
				List<int> list5 = new List<int>(X.makeCountUpArray(num36, 0, 1));
				SummonerPlayer.shuffle<int>(list5, num36);
				if (num8 > 0 && this.AReelSecretSrc != null)
				{
					int num37 = (this.Lp.is_sudden_puppetrevenge ? 3 : nightCon.summoner_drop_od_enemy_box_max(this));
					if (num37 > 0)
					{
						float num38 = nightCon.summoner_drop_od_enemy_box_ratio(this);
						for (int num39 = num36 - 1; num39 >= 0; num39--)
						{
							int num40 = list5[num39];
							SmnEnemyKind smnEnemyKind11 = list[num40];
							if ((smnEnemyKind11.pre_overdrive || smnEnemyKind11.thunder_overdrive) && SummonerPlayer.XORSP() < num38)
							{
								ReelManager.ItemReelContainer itemReelContainer = this.AReelSecretSrc[SummonerPlayer.xors(this.AReelSecretSrc.Length)];
								itemReelContainer.GReelItem.touchObtainCount();
								if (SummonerPlayer.XORSP() < this.replace_reel_secret_to_lower)
								{
									itemReelContainer = ReelManager.ReplaceToLowerGrade(itemReelContainer);
								}
								smnEnemyKind11.SpecialDropReel = new ReelManager.ItemReelDrop(itemReelContainer, -1);
								if (--num37 <= 0)
								{
									break;
								}
							}
						}
					}
				}
				for (int num41 = this.AReel.Length - 1; num41 >= 0; num41--)
				{
					ReelManager.ItemReelDrop itemReelDrop = this.AReel[num41];
					if (itemReelDrop != null)
					{
						itemReelDrop.IR.GReelItem.touchObtainCount();
					}
				}
				this.AReelAfterWholeDefeat = new List<ReelManager.ItemReelDrop>(this.AReel);
				SummonerPlayer.shuffle<ReelManager.ItemReelDrop>(this.AReelAfterWholeDefeat, -1);
				for (int num42 = this.AReelAfterWholeDefeat.Count - 1; num42 >= 0; num42--)
				{
					this.AReelAfterWholeDefeat[num42].fineQuestTargetItem(nelM2DBase.QUEST);
				}
				this.AReelAfterWholeDefeat.Sort(new Comparison<ReelManager.ItemReelDrop>(SummonerPlayer.fnSortReel));
				if (!this.drop_all_reels_after)
				{
					num36 = list5.Count;
					if (this.Asplitter_title.Count > 1)
					{
						for (int num43 = num36 - 1; num43 >= 0; num43--)
						{
							if (this.is_follower(list[list5[num43]], null))
							{
								num36--;
								list5.RemoveAt(num43);
							}
						}
					}
					int num44 = this.AReel.Length;
					if (num36 > 0)
					{
						for (int num45 = 0; num45 < 3; num45++)
						{
							int num46 = 0;
							while (num46 < num36 && num44 > 0)
							{
								list[list5[num46]].drop_reel++;
								num44--;
								num46++;
							}
							if (num44 == 0)
							{
								break;
							}
							SummonerPlayer.shuffle<int>(list5, -1);
						}
					}
				}
			}
			this.ASmnPos = list2.ToArray();
			this.AKindRest = list;
			this.enemy_rest = (this.summoned_count = 0);
			this.battleable_enemy_ = 1;
			int num47 = 0;
			this.enemy_respawn_timescale_when_exists = 1f;
			if (this.Asplitter_title.Count > 1)
			{
				for (int num48 = this.AKindRest.Count - 1; num48 >= 0; num48--)
				{
					if (!this.is_follower(this.AKindRest[num48], null))
					{
						this.enemy_rest++;
					}
					else
					{
						num47++;
					}
				}
			}
			else
			{
				this.enemy_rest = this.AKindRest.Count;
			}
			if (this.bgm_replace != null)
			{
				BGM.load(this.bgm_replace, null, false);
				BGM.fadeout(0f, 120f, false);
				bgm_replaced = true;
			}
			if (num6 >= 1)
			{
				this.AKindRest.Sort(new Comparison<SmnEnemyKind>(this.fnSortKind));
			}
			if (this.delay_one < 0f)
			{
				this.delay_one = 0f;
			}
			if (this.delay_filled < 0f)
			{
				this.delay_filled = 90f;
			}
			if (this.delay_one_second < 0f)
			{
				this.delay_one_second = this.delay_one;
			}
			if (this.Lp.type_event_enemy)
			{
				this.delay = 0f;
			}
			for (int num49 = this.AKindRest.Count - 1; num49 >= 0; num49--)
			{
				NDAT.SummonerOpened(this.AKindRest[num49].enemyid);
			}
		}

		protected virtual void prepareEnemyEnAttr(List<SmnEnemyKind> AKindL, List<SmnEnemyKind> AKindBuf, int nattr_addable_count)
		{
			NightController nightCon = this.Lp.nM2D.NightCon;
			EnemySummonerManager manager = this.Summoner.GetManager();
			float num = (float)((this.QEntry.valid && this.QEntry.nattr > ENATTR.NORMAL) ? this.QEntry.nattr_addable_max : 0);
			int count = AKindL.Count;
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				SmnEnemyKind smnEnemyKind = AKindL[i];
				if ((smnEnemyKind.nattr & ENATTR._FIXED) == ENATTR.NORMAL)
				{
					if (num > 0.125f && !flag)
					{
						float num2 = X.Abs(manager.getEnemyPower(smnEnemyKind.enemyid).x);
						num2 = 0.4f + num2 * 0.6f;
						if (num >= num2)
						{
							num -= num2;
							ENATTR nattr = smnEnemyKind.nattr;
							smnEnemyKind.nattr = this.QEntry.nattr;
							if ((nattr & ENATTR._AATTR) != ENATTR.NORMAL && (this.QEntry.nattr & ENATTR._AATTR) == ENATTR.NORMAL)
							{
								smnEnemyKind.nattr |= nattr & ENATTR._AATTR;
							}
							if ((nattr & ENATTR._MATTR) != ENATTR.NORMAL && (this.QEntry.nattr & ENATTR._MATTR) == ENATTR.NORMAL)
							{
								smnEnemyKind.nattr |= nattr & ENATTR._MATTR;
								goto IL_0196;
							}
							goto IL_0196;
						}
					}
					if (nattr_addable_count > 0)
					{
						byte b;
						if (smnEnemyKind.thunder_overdrive || smnEnemyKind.pre_overdrive)
						{
							b = smnEnemyKind.EnemyDesc.nattr_addable_od;
							if (b == 0)
							{
								b = 32;
							}
						}
						else
						{
							b = smnEnemyKind.EnemyDesc.nattr_addable;
							if (b == 0)
							{
								b = 16;
							}
						}
						if (nightCon.summoner_nattr_attach(this, smnEnemyKind, smnEnemyKind.smn_xorsp, (int)b))
						{
							nattr_addable_count--;
						}
					}
				}
				IL_0196:;
			}
		}

		public static int fnSortReel(ReelManager.ItemReelDrop A, ReelManager.ItemReelDrop B)
		{
			if (A.quest_target_item == B.quest_target_item)
			{
				return 0;
			}
			if (!A.quest_target_item)
			{
				return 1;
			}
			return -1;
		}

		public void destruct()
		{
			this.close(false);
		}

		public void close(bool defeated)
		{
			if (this.QEntry.valid && this.QEntry.weather > WeatherItem.WEATHER.NORMAL)
			{
				this.M2D.NightCon.initTemporaryWeather(this.runner_key_, -1);
			}
			if (defeated && this.AReelAfterWholeDefeat != null && this.drop_all_reels_after)
			{
				while (this.progressStackedDropReel(SummonerPlayer.XORSP(), this.Lp.mapfocx, this.Lp.mapfocy))
				{
				}
			}
			if (this.bgm_replace == "" && this.bgm_replace_when_close)
			{
				BGM.replace(120f, 160f, true, false);
				this.bgm_replace = null;
			}
			if (defeated && !this.Lp.is_sudden_puppetrevenge)
			{
				this.touchAllReelsObtain();
			}
			this.AReelAfterWholeDefeat = null;
			this.Mp.remRunnerObject(this);
		}

		private bool progressStackedDropReel(float xorsp, float depx, float depy)
		{
			if (this.AReelAfterWholeDefeat != null && this.AReelAfterWholeDefeat.Count > 0)
			{
				if (xorsp < DIFF.box_drop_ratio(this.Lp.skill_difficulty_restrict))
				{
					this.Lp.nM2D.IMNG.dropMBoxReel(this.AReelAfterWholeDefeat[0], depx, depy, 0f, 0f);
					this.AReelAfterWholeDefeat.RemoveAt(0);
				}
				else
				{
					this.AReelAfterWholeDefeat.RemoveAt(this.AReelAfterWholeDefeat.Count - 1);
				}
				return true;
			}
			return false;
		}

		public void clearEnemy()
		{
			for (int i = this.AASummoned.Count - 1; i >= 0; i--)
			{
				List<NelEnemy> list = this.AASummoned[i];
				int count = list.Count;
				for (int j = 0; j < count; j++)
				{
					NelEnemy nelEnemy = list[j];
					if (nelEnemy != null)
					{
						try
						{
							nelEnemy.destruct();
						}
						catch
						{
						}
					}
				}
			}
		}

		public bool run(float fcnt = 0f)
		{
			if (this.Mp.M2D.isMaterialLoading(false) || !PxlsLoader.isLoadCompletedAll())
			{
				return true;
			}
			if (SummonerPlayer.golemtoy_lock_delay > 0f)
			{
				SummonerPlayer.golemtoy_lock_delay = X.Mx(SummonerPlayer.golemtoy_lock_delay - fcnt, 0f);
			}
			if (this.AASummoned.Count >= this.max_enemy_appear_current_total)
			{
				return true;
			}
			if (this.delay > 0f)
			{
				this.delay = X.Mx((this.count_battleable_enemy <= 0) ? (X.Mn(this.delay, 150f) - fcnt) : (this.delay - this.enemy_respawn_timescale_when_exists * fcnt), 0f);
			}
			else if (this.AKindRest.Count > 0)
			{
				for (int i = this.AKindRest.Count - 1; i >= 0; i--)
				{
					SmnEnemyKind smnEnemyKind = this.AKindRest[i];
					if (smnEnemyKind.splitter_id > this.current_splitter_id)
					{
						SummonerPlayer.SplitterTerm splitterTerm = this.Asplitter_title[smnEnemyKind.splitter_id];
						bool flag = splitterTerm.can_go_through(smnEnemyKind.splitter_id, this);
						this.Asplitter_title[smnEnemyKind.splitter_id] = splitterTerm;
						if (!flag)
						{
							this.delay = 5f;
							this.delay_one = this.delay_one_second;
							break;
						}
						this.current_splitter_id = smnEnemyKind.splitter_id;
						if (splitterTerm.delay > 0)
						{
							this.delay = (float)splitterTerm.delay;
							break;
						}
					}
					if (this.Adecline_enemyid_buf.IndexOf(smnEnemyKind.check_str) < 0)
					{
						int num = this.max_enemy_appear_whole;
						int num2 = 0;
						if (smnEnemyKind.thunder_overdrive)
						{
							int num3 = 0;
							int num4 = X.Mx(1, this.thunder_odable);
							foreach (KeyValuePair<NelEnemy, SummonedInfo> keyValuePair in this.OFirstPos)
							{
								if (keyValuePair.Value.K.thunder_overdrive)
								{
									num3++;
								}
							}
							if (num4 <= num3)
							{
								this.Adecline_enemyid_buf.Add(smnEnemyKind.check_str);
								goto IL_037C;
							}
						}
						if (num > num2)
						{
							foreach (KeyValuePair<string, EnAppearMax> keyValuePair2 in this.OMaxEnemyAppear)
							{
								if (keyValuePair2.Value.isSame(smnEnemyKind, keyValuePair2.Key))
								{
									num = X.Mn(num, keyValuePair2.Value.max);
									num2 = X.Mx(num2, keyValuePair2.Value.appeared);
								}
							}
						}
						if (num <= num2)
						{
							this.Adecline_enemyid_buf.Add(smnEnemyKind.check_str);
						}
						else if (!(this.summonEnemy(smnEnemyKind, null, false) == null))
						{
							if (!this.is_follower(smnEnemyKind, null))
							{
								this.enemy_rest--;
							}
							this.AKindRest.RemoveAt(i);
							if (this.AASummoned.Count >= this.max_enemy_appear_current_total)
							{
								this.delay_one = this.delay_one_second;
								this.delay = X.Mx(1f, this.delay_filled);
							}
							else
							{
								this.delay = X.Mx(1f, this.delay_one);
							}
							if (this.bgm_replace == null || !(this.bgm_replace != ""))
							{
								break;
							}
							BGM.replace(30f, 0f, !this.bgm_replace_when_close, false);
							this.bgm_replace = "";
							if (TX.valid(this.bgm_block_ovr))
							{
								BGM.setOverrideKey(this.bgm_block_ovr, false);
							}
							if (TX.valid(this.bgm_block_to))
							{
								BGM.GotoBlock(this.bgm_block_to, true);
								break;
							}
							break;
						}
					}
					IL_037C:;
				}
				if (this.delay <= 0f)
				{
					if (this.enemy_count == 0f)
					{
						this.enemy_rest = 0;
					}
					else
					{
						this.delay_one = this.delay_one_second;
						this.delay = 60f;
					}
				}
			}
			if (this.enemy_rest <= 0 && this.enemy_count <= 0f)
			{
				if (this.next_script_key != null)
				{
					if (this.delay != 0f)
					{
						return true;
					}
					EnemySummoner enemySummoner = EnemySummoner.Get(this.next_script_key, true);
					this.next_script_key = null;
					if (enemySummoner != null)
					{
						bool flag2 = false;
						this.CR = enemySummoner.getCsvReader();
						this.prepareEnemyConent(ref flag2);
						return true;
					}
				}
				this.Summoner.close(false, true);
				this.Mp.remRunnerObject(this);
				return true;
			}
			return true;
		}

		public NelEnemy summonEnemyFromOther(SmnEnemyKind K, SmnPoint Target = null)
		{
			return this.summonEnemy(K, Target, true);
		}

		private NelEnemy summonEnemy(SmnEnemyKind K, SmnPoint Target, bool add_rest_count = false)
		{
			bool flag = false;
			int num = this.ASmnPos.Length;
			NightController nightCon = this.Lp.nM2D.NightCon;
			Vector2 vector = Vector2.zero;
			int num2 = 0;
			NelEnemy nelEnemy = null;
			List<NelEnemy> list = new List<NelEnemy>(4);
			for (;;)
			{
				nelEnemy = NDAT.createByKey(this.Lp.Mp, K.enemyid, "-Summonned-" + this.Lp.key + "-" + this.enemy_index.ToString());
				if (nelEnemy == null)
				{
					break;
				}
				float num3 = this.weight_add;
				if (num > 0 && Target == null)
				{
					List<SmnPoint> list2 = new List<SmnPoint>(2);
					List<SmnPoint> list3 = new List<SmnPoint>(2);
					if (!flag)
					{
						for (int i = 0; i < 2; i++)
						{
							float num4 = this.weight_add + (float)i;
							list2.Clear();
							for (int j = 0; j < num; j++)
							{
								SmnPoint smnPoint = this.ASmnPos[j];
								if (smnPoint.available(num4))
								{
									if (smnPoint.idMatch(K.enemyid))
									{
										smnPoint.AddTo(list2, num4);
									}
									else if (smnPoint.Aalloc_id == null)
									{
										smnPoint.AddTo(list3, num4);
									}
								}
							}
							if (list2.Count > 0)
							{
								Target = list2[(int)(K.NCXORSP(141) * (float)list2.Count)];
								num3 = num4;
								break;
							}
							if (list3.Count > 0)
							{
								Target = list3[(int)(K.NCXORSP(67) * (float)list3.Count)];
								num3 = num4;
								break;
							}
						}
					}
					if (Target == null)
					{
						int i = 0;
						while (Target == null)
						{
							float num5 = this.weight_add + (float)i;
							list2.Clear();
							for (int k = 0; k < num; k++)
							{
								SmnPoint smnPoint2 = this.ASmnPos[k];
								if (smnPoint2.available(num5))
								{
									smnPoint2.AddTo(list2, num5);
								}
							}
							if (list2.Count > 0)
							{
								Target = list2[(int)(K.NCXORSP(93) * (float)list2.Count)];
								num3 = num5;
								break;
							}
							i++;
						}
					}
				}
				nelEnemy.Summoner = this;
				nelEnemy.first_mp_ratio = X.ZLINE(X.NI(K.mp_ratio_min, K.mp_ratio_max, K.NCXORSP(323)));
				if (K.mp_ratio_min < 1f)
				{
					nelEnemy.first_mp_ratio = this.Lp.nM2D.NightCon.fixEnemyFirstMpRatio(nelEnemy.first_mp_ratio);
				}
				int num7;
				if (Target != null)
				{
					float num6 = 0f;
					if (Target.pos_cnt < 0)
					{
						if (Target.pos_cnt == -1)
						{
							vector = Vector2.zero;
							Target.pos_cnt--;
						}
						else
						{
							vector.Set(1.4f * X.Cos(Target.pos_agR), 1.2f * X.Sin(Target.pos_agR));
							Target.pos_agR += 2.2242477f;
							SmnPoint smnPoint3 = Target;
							num7 = smnPoint3.pos_cnt - 1;
							smnPoint3.pos_cnt = num7;
							if (num7 < -6)
							{
								Target.pos_cnt = -1;
							}
						}
						vector *= Target.shuffle_ratio;
						vector.x += Target.x;
						vector.y += Target.y;
					}
					else
					{
						if (Target.pos_cnt == 0)
						{
							vector = Vector2.zero;
							Target.pos_cnt++;
						}
						else
						{
							vector.Set(1.4f * X.Cos(Target.pos_agR), 1.2f * X.Sin(Target.pos_agR));
							vector *= Target.shuffle_ratio;
							num6 = 0.7f;
							Target.pos_agR += 2.5132742f;
							SmnPoint smnPoint4 = Target;
							num7 = smnPoint4.pos_cnt + 1;
							smnPoint4.pos_cnt = num7;
							if (num7 > 5)
							{
								Target.pos_cnt = 0;
							}
						}
						float num8 = Target.Lp.mapfocx;
						float num9 = Target.Lp.mapfocy;
						if (Target.Lp is M2LpPuzzCarrier)
						{
							Vector2 moverTranslatedMp = (Target.Lp as M2LpPuzzCarrier).getMoverTranslatedMp();
							num8 += moverTranslatedMp.x;
							num9 += moverTranslatedMp.y;
							vector.x += Target.x + num8;
							vector.y += Target.y + num9;
						}
						else
						{
							vector.x += Target.x + num8;
							vector.y += Target.y + num9;
							vector = Target.Lp.getWalkable(this.Mp, X.MMX((float)Target.Lp.mapx, vector.x, (float)(Target.Lp.mapx + Target.Lp.mapw)), X.MMX((float)Target.Lp.mapy, vector.y, (float)(Target.Lp.mapy + Target.Lp.maph)));
						}
					}
					Target.use_weight += 1f;
					this.OFirstPos[nelEnemy] = new SummonedInfo(K, vector, Target);
					this.weight_add = num3;
					if (num6 != 0f)
					{
						vector.x += X.NI(-num6, num6, K.NCXORSP(47));
						vector.y += X.NI(-num6, num6, K.NCXORSP(39)) * 0.5f;
					}
				}
				Vector3 localPosition = nelEnemy.transform.localPosition;
				localPosition.x = this.Mp.map2ux(localPosition.x);
				localPosition.y = this.Mp.map2uy(localPosition.y);
				nelEnemy.transform.localPosition = localPosition;
				FEnum<ENEMYID>.TryParse(K.enemyid, out nelEnemy.id, true);
				M2Mover m2Mover = nelEnemy;
				string enemyid = K.enemyid;
				string text = "_";
				num7 = this.enemy_index;
				this.enemy_index = num7 + 1;
				m2Mover.key = enemyid + text + num7.ToString();
				nelEnemy.smn_xorsp = K.smn_xorsp;
				if (K.nattr != ENATTR.NORMAL)
				{
					nelEnemy.nattr = K.nattr & ~(ENATTR._MAX_RANDOMIZE | ENATTR._FIXED);
				}
				this.Mp.assignMover(nelEnemy, false);
				if (Target == null)
				{
					vector = this.Lp.getWalkable(nelEnemy, new Vector2(X.NIXP((float)this.Lp.mapx + (float)this.Lp.mapw * 0.25f, (float)this.Lp.mapx + (float)this.Lp.mapw * 0.75f), X.NIXP((float)this.Lp.mapy + (float)this.Lp.maph * 0.25f, (float)this.Lp.mapy + (float)this.Lp.maph * 0.75f)));
					vector.y += nelEnemy.sizey;
					this.OFirstPos[nelEnemy] = new SummonedInfo(K, vector, null);
				}
				nelEnemy.setTo(vector.x, vector.y + 0.5f - nelEnemy.sizey);
				bool flag2 = (Target != null && Target.sudden_appear) || this.Lp.type_event_enemy;
				nelEnemy.initSummoned(K, flag2, num2);
				this.battleable_enemy_ = -1;
				this.enemy_respawn_timescale_when_exists = DIFF.enemy_respawn_timescale_when_exists;
				if (flag2)
				{
					this.fineEventEnemyFlag(nelEnemy);
					nelEnemy.quitSummonAndAppear(false);
				}
				list.Add(nelEnemy);
				this.summoned_count++;
				if (nelEnemy is IOtherKillListener)
				{
					if (this.AOtherLsn == null)
					{
						this.AOtherLsn = new List<IOtherKillListener>(1);
					}
					this.AOtherLsn.Add(nelEnemy as IOtherKillListener);
				}
				foreach (KeyValuePair<string, EnAppearMax> keyValuePair in this.OMaxEnemyAppear)
				{
					if (keyValuePair.Value.isSame(K, keyValuePair.Key))
					{
						keyValuePair.Value.appeared++;
					}
				}
				this.current_splitter_id = X.Mx(K.splitter_id, this.current_splitter_id);
				if (K.temporary_adding_count)
				{
					this.max_enemy_appear_whole_temp++;
				}
				if (this.Summoner.dropable_special_item)
				{
					UiEnemyDex.addDefeatCount(nelEnemy, nelEnemy.id, 0, true);
				}
				OverDriveManager odManager = nelEnemy.getOdManager();
				if (odManager != null)
				{
					if (K.pre_overdrive)
					{
						odManager.pre_overdrive = true;
					}
					else if (K.thunder_overdrive)
					{
						odManager.thunder_overdrive = true;
					}
				}
				if (K.DupeConnect == null)
				{
					goto IL_08A5;
				}
				num2++;
				K = K.DupeConnect;
				if (num2 >= 1 && K.NCXORSP(78) < 0.4f)
				{
					Target = null;
				}
			}
			X.de("エネミー召喚に失敗(" + this.key + ")", null);
			return null;
			IL_08A5:
			this.AASummoned.Add(list);
			this.OFnCheckEnemy.Clear();
			return nelEnemy;
		}

		public void killedEnemy(NelEnemy En)
		{
			int num = -1;
			int num2 = -1;
			for (int i = this.AASummoned.Count - 1; i >= 0; i--)
			{
				num2 = X.isinC<NelEnemy>(this.AASummoned[i], En);
				if (num2 >= 0)
				{
					num = i;
					break;
				}
			}
			SummonedInfo summonedInfo = X.Get<NelEnemy, SummonedInfo>(this.OFirstPos, En);
			this.battleable_enemy_ = -1;
			if (summonedInfo != null)
			{
				this.OFirstPos.Remove(En);
				if (summonedInfo.K.SpecialDropReel != null && summonedInfo.K.NCXORSP(94) < DIFF.box_drop_ratio(this.Lp.skill_difficulty_restrict))
				{
					this.Lp.nM2D.IMNG.dropMBoxReel(summonedInfo.K.SpecialDropReel, En.x, En.y, 0f, 0f);
				}
				while (summonedInfo.K.drop_reel > 0)
				{
					this.progressStackedDropReel(summonedInfo.K.NCXORSP(194 + summonedInfo.K.drop_reel * 1189), En.x, En.y);
					summonedInfo.K.drop_reel--;
				}
				if (summonedInfo.K.temporary_adding_count && this.max_enemy_appear_whole_temp > 0)
				{
					this.max_enemy_appear_whole_temp--;
				}
				int num3 = X.Mx(this.current_splitter_id, summonedInfo.K.splitter_id);
				for (int j = this.Asplitter_title.Count - 1; j >= num3 + 1; j--)
				{
					SummonerPlayer.SplitterTerm splitterTerm = this.Asplitter_title[j];
					if (splitterTerm.clearBuffer())
					{
						this.Asplitter_title[j] = splitterTerm;
					}
				}
				foreach (KeyValuePair<string, EnAppearMax> keyValuePair in this.OMaxEnemyAppear)
				{
					if (keyValuePair.Value.isSame(summonedInfo.K, keyValuePair.Key))
					{
						keyValuePair.Value.appeared--;
					}
				}
			}
			this.enemyOverDriveQuit(En);
			if (num >= 0)
			{
				this.OFnCheckEnemy.Clear();
				List<NelEnemy> list = this.AASummoned[num];
				list.RemoveAt(num2);
				if (list.Count == 0)
				{
					this.AASummoned.RemoveAt(num);
				}
				if (this.AOtherLsn != null)
				{
					if (En is IOtherKillListener)
					{
						this.AOtherLsn.Remove(En as IOtherKillListener);
					}
					for (int k = this.AOtherLsn.Count - 1; k >= 0; k--)
					{
						this.AOtherLsn[k].otherEnemyKilled(En);
					}
				}
			}
			this.Adecline_enemyid_buf.Clear();
		}

		public List<List<NelEnemy>> getAASummoned()
		{
			return this.AASummoned;
		}

		public float mana_desire_multiple
		{
			get
			{
				if (this.AASummoned.Count == 0)
				{
					return 1f;
				}
				float num = 0f;
				foreach (KeyValuePair<NelEnemy, SummonedInfo> keyValuePair in this.OFirstPos)
				{
					num += X.Mx(1f - keyValuePair.Key.mp_ratio, 0.125f);
				}
				return 1f / (X.Mx(0f, num - 1f) * 0.28f + 1f);
			}
		}

		public bool checkRingOut(NelEnemy En)
		{
			bool fallable_ringout = this.Lp.fallable_ringout;
			if (this.Lp == null)
			{
				return false;
			}
			if (!En.ringoutable || (En.hasFoot() && !fallable_ringout) || !(fallable_ringout ? (En.mbottom >= (float)(this.Lp.mapy + this.Lp.maph) + 0.5f) : (En.y >= (float)(this.Lp.mapy + this.Lp.maph))))
			{
				return true;
			}
			SummonedInfo summonedInfo;
			if (!this.OFirstPos.TryGetValue(En, out summonedInfo))
			{
				return false;
			}
			Vector2 vector = summonedInfo.FirstPos;
			vector = this.Lp.getWalkable(this.Mp, X.MMX((float)this.Lp.mapx, vector.x, (float)(this.Lp.mapx + this.Lp.mapw)), X.MMX((float)this.Lp.mapy, vector.y, (float)(this.Lp.mapy + this.Lp.maph)));
			En.setTo(vector.x, vector.y);
			En.initRingOut(false);
			return true;
		}

		public bool is_follower(NelEnemy En, string check_prefix = null)
		{
			SummonedInfo summonedInfo = this.getSummonedInfo(En);
			return summonedInfo != null && this.is_follower(summonedInfo.K, check_prefix);
		}

		public bool is_follower(SmnEnemyKind K, string check_prefix = null)
		{
			if (K.splitter_id >= 0)
			{
				SummonerPlayer.SplitterTerm splitterTerm = this.Asplitter_title[X.Mn(this.Asplitter_title.Count - 1, K.splitter_id)];
				if (TX.valid(splitterTerm.title) && TX.isStart(splitterTerm.title, "_FOLLOW_", 0))
				{
					return check_prefix == null || TX.isStart(splitterTerm.title, check_prefix, 0);
				}
			}
			return false;
		}

		private int fnSortKind(SmnEnemyKind Ka, SmnEnemyKind Kb)
		{
			if (Ka.splitter_id == Kb.splitter_id)
			{
				return 0;
			}
			if (Ka.splitter_id >= Kb.splitter_id)
			{
				return -1;
			}
			return 1;
		}

		public void enemyOverDriveInit(NelEnemy N)
		{
			if (this.AOverdrived.IndexOf(N) >= 0)
			{
				return;
			}
			this.AOverdrived.Add(N);
		}

		public void enemyOverDriveQuit(NelEnemy N)
		{
			this.AOverdrived.Remove(N);
		}

		public bool hasOverDrivedEnemy(int thresh = 1)
		{
			return this.AOverdrived.Count >= thresh;
		}

		public SummonedInfo getSummonedInfo(NelEnemy N)
		{
			return X.Get<NelEnemy, SummonedInfo>(this.OFirstPos, N);
		}

		public int callFollowerEnemy(ref int splitter_id, int set_delay = 0, int summonable = -1)
		{
			splitter_id = X.Mx(this.current_splitter_id, splitter_id);
			int count = this.AKindRest.Count;
			if (count == 0)
			{
				return 0;
			}
			bool flag = false;
			int num = 1;
			for (int i = count - 1; i >= 0; i--)
			{
				SmnEnemyKind smnEnemyKind = this.AKindRest[i];
				if (smnEnemyKind.splitter_id >= splitter_id)
				{
					if (!flag)
					{
						flag = true;
						this.current_splitter_id = (splitter_id = smnEnemyKind.splitter_id);
						this.delay = X.Mx(this.delay, (float)set_delay);
						this.Asplitter_title.Insert(splitter_id, this.Asplitter_title[X.MMX(0, splitter_id, this.Asplitter_title.Count)]);
					}
					if (summonable == 0 || smnEnemyKind.splitter_id > splitter_id)
					{
						smnEnemyKind.splitter_id++;
					}
					else
					{
						summonable--;
						num = 2;
					}
				}
			}
			return num;
		}

		public int countSplitterEnemy(int splitter_id, bool count_active = true, bool count_rest = true, bool count_lesser_id = false)
		{
			int num = 0;
			if (count_active)
			{
				for (int i = this.AASummoned.Count - 1; i >= 0; i--)
				{
					List<NelEnemy> list = this.AASummoned[i];
					for (int j = list.Count - 1; j >= 0; j--)
					{
						NelEnemy nelEnemy = list[j];
						if (nelEnemy.battleable_enemy)
						{
							SummonedInfo summonedInfo = this.getSummonedInfo(nelEnemy);
							if (summonedInfo != null && (count_lesser_id ? (summonedInfo.K.splitter_id <= splitter_id) : (summonedInfo.K.splitter_id == splitter_id)))
							{
								num++;
							}
						}
					}
				}
			}
			if (count_rest)
			{
				for (int k = this.AKindRest.Count - 1; k >= 0; k--)
				{
					if (this.AKindRest[k].splitter_id == splitter_id)
					{
						num++;
					}
				}
			}
			return num;
		}

		public int countActiveEnemy(NelEnemy.FnCheckEnemy Fn, bool memory = true)
		{
			int num;
			if (memory && this.OFnCheckEnemy.TryGetValue(Fn, out num))
			{
				return num;
			}
			int num2 = 0;
			for (int i = this.AASummoned.Count - 1; i >= 0; i--)
			{
				List<NelEnemy> list = this.AASummoned[i];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					try
					{
						if (Fn(list[j]))
						{
							num2++;
						}
					}
					catch
					{
					}
				}
			}
			if (memory)
			{
				this.OFnCheckEnemy[Fn] = num2;
			}
			return num2;
		}

		public int countActiveEnemy(string enemy_id, bool count_active = true, bool count_rest = true, bool count_rest_only_current_splitter = true)
		{
			int num = 0;
			if (count_active)
			{
				for (int i = this.AASummoned.Count - 1; i >= 0; i--)
				{
					List<NelEnemy> list = this.AASummoned[i];
					for (int j = list.Count - 1; j >= 0; j--)
					{
						NelEnemy nelEnemy = list[j];
						if (nelEnemy.battleable_enemy)
						{
							SummonedInfo summonedInfo = this.getSummonedInfo(nelEnemy);
							if (summonedInfo != null && summonedInfo.K.isSame(enemy_id))
							{
								num++;
							}
						}
					}
				}
			}
			if (count_rest)
			{
				for (int k = this.AKindRest.Count - 1; k >= 0; k--)
				{
					SmnEnemyKind smnEnemyKind = this.AKindRest[k];
					if (!count_rest_only_current_splitter && smnEnemyKind.splitter_id <= this.current_splitter_id && smnEnemyKind.isSame(enemy_id))
					{
						num++;
					}
				}
			}
			return num;
		}

		public int count_battleable_enemy
		{
			get
			{
				if (this.battleable_enemy_ < 0)
				{
					this.battleable_enemy_ = 0;
					for (int i = this.AASummoned.Count - 1; i >= 0; i--)
					{
						List<NelEnemy> list = this.AASummoned[i];
						for (int j = list.Count - 1; j >= 0; j--)
						{
							if (list[j].battleable_enemy)
							{
								this.battleable_enemy_++;
							}
						}
					}
				}
				return this.battleable_enemy_;
			}
			set
			{
				if (value < 0)
				{
					this.battleable_enemy_ = -1;
				}
			}
		}

		public void LockGolemToyMax()
		{
			SummonerPlayer.golemtoy_lock_delay = this.golemtoy_lock_delay_maxt;
		}

		public int getGolemToyCreatable(out int exists_toy)
		{
			int num = SummonerPlayer.golemtoy_divide_count;
			exists_toy = 0;
			if (num <= 0)
			{
				return 0;
			}
			int num2 = this.countActiveEnemy(EnemySummoner.FD_countGolem, true) / num + (this.Summoner.puppetrevenge_enable ? 1 : 0);
			exists_toy = this.countActiveEnemy(EnemySummoner.FD_countGolemToy, true);
			return num2;
		}

		public NelEnemy searchActiveEnemy(NelEnemy.FnCheckEnemy Fn)
		{
			int count = this.AASummoned.Count;
			for (int i = 0; i < count; i++)
			{
				List<NelEnemy> list = this.AASummoned[i];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					NelEnemy nelEnemy = list[j];
					if (Fn(nelEnemy))
					{
						return nelEnemy;
					}
				}
			}
			return null;
		}

		public void fineEventEnemyFlag(NelEnemy En)
		{
			En.event_enemy_flag = this.Lp.type_event_enemy;
		}

		public void fineEventEnemyFlag()
		{
			for (int i = this.AASummoned.Count - 1; i >= 0; i--)
			{
				List<NelEnemy> list = this.AASummoned[i];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					this.fineEventEnemyFlag(list[j]);
				}
			}
		}

		public void fineEnemyDropItem()
		{
			for (int i = this.AASummoned.Count - 1; i >= 0; i--)
			{
				List<NelEnemy> list = this.AASummoned[i];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					list[j].fineDropItem();
				}
			}
		}

		public void recheckHrBuffer()
		{
			if (this.Asplitter_title != null)
			{
				for (int i = this.Asplitter_title.Count - 1; i >= 0; i--)
				{
					SummonerPlayer.SplitterTerm splitterTerm = this.Asplitter_title[i];
					if (splitterTerm.clearBuffer())
					{
						this.Asplitter_title[i] = splitterTerm;
					}
				}
			}
		}

		public bool AreaContainsMv(M2Mover Mv, float extend_px = 0f)
		{
			return this.Lp.isContainingMover(Mv, extend_px);
		}

		public bool AreaCoveringMv(M2Mover Mv, float extend_px = 0f)
		{
			return this.Lp.isCoveringMover(Mv, extend_px);
		}

		public float CLEN
		{
			get
			{
				return this.Lp.Mp.CLEN;
			}
		}

		public void touchAllReelsObtain()
		{
			if (this.AReel != null)
			{
				for (int i = this.AReel.Length - 1; i >= 0; i--)
				{
					ReelManager.ItemReelDrop itemReelDrop = this.AReel[i];
					if (itemReelDrop != null && itemReelDrop.IR != null)
					{
						itemReelDrop.IR.GReelItem.touchObtainCount();
					}
				}
			}
		}

		public void listupRestEnemy(List<Vector2Int> Aid_cnt)
		{
			if (!this.isActive())
			{
				return;
			}
			int count = this.AASummoned.Count;
			for (int i = 0; i < count; i++)
			{
				List<NelEnemy> list = this.AASummoned[i];
				if (list.Count > 0)
				{
					SummonerPlayer.addCount(Aid_cnt, (int)(list[0].id | (list[0].isOverDrive() ? ((ENEMYID)2147483648U) : ((ENEMYID)0U))), list.Count);
				}
			}
			for (int j = this.AKindRest.Count - 1; j >= 0; j--)
			{
				SmnEnemyKind smnEnemyKind = this.AKindRest[j];
				ENEMYID enemyid;
				if (!this.is_follower(smnEnemyKind, null) && FEnum<ENEMYID>.TryParse(smnEnemyKind.enemyid, out enemyid, true))
				{
					SummonerPlayer.addCount(Aid_cnt, (int)(enemyid | (smnEnemyKind.pre_overdrive ? ((ENEMYID)2147483648U) : ((ENEMYID)0U))), 1);
				}
			}
		}

		private static void addCount(List<Vector2Int> Aid_cnt, int id, int cnt = 1)
		{
			int count = Aid_cnt.Count;
			for (int i = 0; i < count; i++)
			{
				Vector2Int vector2Int = Aid_cnt[i];
				if (vector2Int.x == id)
				{
					vector2Int.y += cnt;
					Aid_cnt[i] = vector2Int;
					return;
				}
			}
			Aid_cnt.Add(new Vector2Int(id, cnt));
		}

		public static int xors(int i)
		{
			return NightController.xors(i);
		}

		public static float NIXP(float v, float v2)
		{
			return X.NI(v, v2, SummonerPlayer.XORSP());
		}

		public static float XORSP()
		{
			return NightController.XORSP();
		}

		public static float XORSPS()
		{
			return (SummonerPlayer.XORSP() - 0.5f) * 2f;
		}

		public T RanSelect<T>(T[] A, int seedA = 1389, int seedB = 51)
		{
			int num = (int)(X.RAN((uint)(this.ran_session + seedA + (this.ran_session & 63) * 11 + this.summoned_count * 43), 5 + this.summoned_count * seedB) * (float)A.Length);
			return A[num];
		}

		public static void shuffle<T>(T[] A, int arraymax = -1)
		{
			NightController.shuffle<T>(A, arraymax);
		}

		public static void shuffle<T>(List<T> A, int arraymax = -1)
		{
			NightController.shuffle<T>(A, arraymax);
		}

		public int max_enemy_appear_current_total
		{
			get
			{
				return this.max_enemy_appear_whole + this.max_enemy_appear_whole_temp;
			}
		}

		public float enemy_count
		{
			get
			{
				return (float)this.AASummoned.Count;
			}
		}

		private float getDangerLevel()
		{
			return (M2DBase.Instance as NelM2DBase).NightCon.getDangerLevel();
		}

		private float CalcScript(string t, string default_str = "0", string defaut_fn = "ZLINE")
		{
			this.FuncBase.Remake(TX.noe(t) ? default_str : t, defaut_fn, 5f);
			return this.FuncBase.Get(-1f, this.getDangerLevel(), false);
		}

		private float Calc(CsvReaderA CR, int index, string default_str = "0", string defaut_fn = "ZLINE")
		{
			return this.CalcScript(CR.getIndex(index), default_str, defaut_fn);
		}

		private float CalcF(CsvReaderA CR, ref bool fix_flag, int index, string default_str = "0", string defaut_fn = "ZLINE")
		{
			string text = CR.getIndex(index);
			if (TX.isStart(text, "!", 0))
			{
				fix_flag = true;
				text = TX.slice(text, 1);
			}
			return this.CalcScript(text, default_str, defaut_fn);
		}

		public List<ReelManager.ItemReelDrop> getRestReelDrop()
		{
			return this.AReelAfterWholeDefeat;
		}

		public bool isEnemyEventUsing()
		{
			return this.Lp.type_event_enemy;
		}

		public bool isActive()
		{
			return this.Summoner.isActive();
		}

		public int getRestCount()
		{
			return this.enemy_rest;
		}

		public bool isActiveBorder()
		{
			return EnemySummoner.isActiveBorder();
		}

		public override string ToString()
		{
			return this.runner_key_;
		}

		public NelM2DBase M2D
		{
			get
			{
				return this.Lp.nM2D;
			}
		}

		public string key
		{
			get
			{
				return this.Summoner.key;
			}
		}

		public QuestTracker.SummonerEntry QEntry
		{
			get
			{
				return this.Summoner.QEntry;
			}
			set
			{
				this.Summoner.QEntry = value;
			}
		}

		public readonly EnemySummoner Summoner;

		private CsvReaderA CR;

		public readonly Map2d Mp;

		public readonly M2LpSummon Lp;

		private static VariableP VPTemp;

		private readonly EfParticleFuncCalc FuncBase;

		public string bgm_replace;

		public string bgm_block_ovr;

		public string bgm_block_to;

		private bool drop_all_reels_after;

		public const string csv_enemy_entry = "%EN";

		public const string csv_od_enemy_entry = "%EN_OD";

		public const string csv_puppetrevenge_entry = "%PUPPETREVENGE";

		public const string follower_splitter_title = "_FOLLOW_";

		internal int enemy_index;

		private SmnPoint[] ASmnPos;

		private List<SmnEnemyKind> AKindRest;

		public int summoned_count;

		private int enemy_rest;

		public readonly int ran_session;

		private int max_enemy_appear_whole_temp;

		protected int max_enemy_appear_whole;

		protected BDic<string, EnAppearMax> OMaxEnemyAppear;

		protected float delay_one;

		protected float delay_one_second;

		protected float delay_filled;

		private float delay;

		private float enemy_respawn_timescale_when_exists = 1f;

		private float weight_add;

		private string next_script_key;

		private int thunder_odable = 1;

		public bool bgm_replace_when_close = true;

		private BDic<NelEnemy, SummonedInfo> OFirstPos;

		private BDic<NelEnemy.FnCheckEnemy, int> OFnCheckEnemy;

		private int battleable_enemy_ = -1;

		private readonly List<NelEnemy> AOverdrived;

		private readonly List<List<NelEnemy>> AASummoned;

		private const int INITIAL_DELAY = 50;

		private const int DELAY_FILLED_DEFAULT = 90;

		public int current_overdrive_rest;

		private List<IOtherKillListener> AOtherLsn;

		private List<string> Adecline_enemyid_buf;

		private List<SummonerPlayer.SplitterTerm> Asplitter_title;

		public static int golemtoy_divide_count;

		public static float golemtoy_lock_delay;

		public float golemtoy_lock_delay_maxt = 300f;

		public const float GOLEMTOY_LOCK_AT_FULL_DEFAULT = 300f;

		public int current_splitter_id = -1;

		private ReelManager.ItemReelDrop[] AReel;

		private ReelManager.ItemReelContainer[] AReelSecretSrc;

		private List<ReelManager.ItemReelDrop> AReelAfterWholeDefeat;

		private float replace_reel_secret_to_lower;

		private string runner_key_;

		private struct SplitterTerm
		{
			public SplitterTerm(CsvReader CR = null, int delay_one_second = 0)
			{
				this.can_go_through_ = 2;
				this.delay = CR.Int(3, delay_one_second);
				if (CR == null)
				{
					this.title = "";
					this.Term = null;
					return;
				}
				this.title = CR._1;
				this.Term = null;
				if (TX.valid(CR._2))
				{
					if (CR._2 == "1" || CR._2 == "true")
					{
						this.can_go_through_ = 3;
						return;
					}
					this.Term = new EvalP(null).Parse(CR._2);
				}
			}

			public bool can_go_through(int index, SummonerPlayer Con)
			{
				if (this.can_go_through_ == 2)
				{
					if (this.Term == null)
					{
						this.can_go_through_ = ((Con.countSplitterEnemy(index - 1, true, true, true) > 0) ? 0 : 1);
					}
					else
					{
						this.can_go_through_ = ((this.Term.getValue(SummonerPlayer.VPTemp) != 0.0) ? 1 : 0);
					}
				}
				return this.can_go_through_ > 0;
			}

			public bool clearBuffer()
			{
				if (this.can_go_through_ <= 1)
				{
					this.can_go_through_ = 2;
					return true;
				}
				return false;
			}

			public readonly string title;

			public readonly EvalP Term;

			public readonly int delay;

			private byte can_go_through_;
		}
	}
}

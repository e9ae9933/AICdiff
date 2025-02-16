using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using m2d;
using XX;

namespace nel
{
	public static class NOD
	{
		public static void reloadNODScript()
		{
			if (NOD.OTackle == null)
			{
				NOD.OTackle = new BDic<string, NOD.TackleInfo>();
				NOD.OShockwave = new BDic<string, NOD.ShockwaveInfo>();
				NOD.OBasic = new BDic<string, NOD.BasicData>();
				NOD.OConsume = new BDic<string, NOD.MpConsume>();
			}
			List<string> list = new List<string>(20);
			NOD.load_count = (NOD.load_count + 1U) & 65535U;
			using (MTI mti = new MTI("Enemies/_nodd", "_"))
			{
				CsvReaderA csvReaderA = new CsvReaderA(mti.LoadText("_nodd", ".csv", true), true);
				csvReaderA.tilde_replace = true;
				NOD.TackleInfo tackleInfo = null;
				NOD.ShockwaveInfo shockwaveInfo = null;
				NOD.BasicData basicData = null;
				NOD.MpConsume mpConsume = null;
				int num = 0;
				while (csvReaderA.read())
				{
					if (csvReaderA.cmd.IndexOf("##") == 0)
					{
						num = 0;
						tackleInfo = null;
						mpConsume = null;
						basicData = null;
						shockwaveInfo = null;
						string cmd = csvReaderA.cmd;
						if (cmd != null)
						{
							if (cmd == "##TACKLE")
							{
								num = 1;
								continue;
							}
							if (cmd == "##BASIC")
							{
								num = 0;
								continue;
							}
							if (cmd == "##CONSUME")
							{
								num = 2;
								continue;
							}
							if (cmd == "##SHOCKWAVE")
							{
								num = 3;
								continue;
							}
						}
						csvReaderA.tError("不明な##指定子: " + csvReaderA._1);
					}
					else if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
					{
						tackleInfo = null;
						mpConsume = null;
						basicData = null;
						shockwaveInfo = null;
						string text = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
						int num2 = num;
						if (REG.match(text, NOD.RegTackle))
						{
							text = REG.rightContext;
							num2 = 1;
						}
						if (REG.match(text, NOD.RegConsume))
						{
							text = REG.rightContext;
							num2 = 2;
						}
						if (REG.match(text, NOD.RegShockwave))
						{
							text = REG.rightContext;
							num2 = 3;
						}
						if (num2 == 1)
						{
							tackleInfo = X.Get<string, NOD.TackleInfo>(NOD.OTackle, text);
							if (tackleInfo != null)
							{
								tackleInfo.copyFrom(new NOD.TackleInfo());
							}
							else
							{
								tackleInfo = (NOD.OTackle[text] = new NOD.TackleInfo());
							}
						}
						else if (num2 == 0)
						{
							basicData = X.Get<string, NOD.BasicData>(NOD.OBasic, text);
							if (basicData != null)
							{
								basicData.copyFrom(new NOD.BasicData());
							}
							else
							{
								basicData = (NOD.OBasic[text] = new NOD.BasicData());
							}
						}
						else if (num2 == 2)
						{
							mpConsume = X.Get<string, NOD.MpConsume>(NOD.OConsume, text);
							if (mpConsume != null)
							{
								mpConsume.copyFrom(new NOD.MpConsume());
							}
							else
							{
								mpConsume = (NOD.OConsume[text] = new NOD.MpConsume());
							}
						}
						else if (num2 == 3)
						{
							shockwaveInfo = X.Get<string, NOD.ShockwaveInfo>(NOD.OShockwave, text);
							if (shockwaveInfo != null)
							{
								shockwaveInfo.copyFrom(new NOD.ShockwaveInfo());
							}
							else
							{
								shockwaveInfo = (NOD.OShockwave[text] = new NOD.ShockwaveInfo());
							}
						}
						string text2 = num2.ToString() + "." + text;
						if (list.IndexOf(text2) == -1)
						{
							list.Add(text2);
						}
						else
						{
							X.dl("重複:" + text, null, false, false);
						}
					}
					else if (basicData != null)
					{
						basicData.readBasicDataCR(csvReaderA);
					}
					else if (tackleInfo != null)
					{
						tackleInfo.readTaclkeInfoCR(csvReaderA);
					}
					else if (mpConsume != null)
					{
						mpConsume.readMpConsumeCR(csvReaderA);
					}
					else if (shockwaveInfo != null)
					{
						shockwaveInfo.readShockwaveCR(csvReaderA);
					}
				}
			}
		}

		public static NOD.TackleInfo getTackle(string key)
		{
			NOD.TackleInfo tackleInfo = X.Get<string, NOD.TackleInfo>(NOD.OTackle, key);
			if (tackleInfo == null)
			{
				X.de("TackleInfo " + key + " が不明", null);
			}
			return tackleInfo;
		}

		public static NOD.ShockwaveInfo getShockwave(string key)
		{
			NOD.ShockwaveInfo shockwaveInfo = X.Get<string, NOD.ShockwaveInfo>(NOD.OShockwave, key);
			if (shockwaveInfo == null)
			{
				X.de("ShockwaveInfo " + key + " が不明", null);
			}
			return shockwaveInfo;
		}

		public static NOD.ShockwaveInfo[] getShockwaveA(params string[] key)
		{
			int num = key.Length;
			List<NOD.ShockwaveInfo> list = new List<NOD.ShockwaveInfo>(num);
			for (int i = 0; i < num; i++)
			{
				NOD.ShockwaveInfo shockwave = NOD.getShockwave(key[i]);
				if (shockwave != null)
				{
					list.Add(shockwave);
				}
			}
			return list.ToArray();
		}

		public static NOD.TackleInfo[] getTackleA(params string[] key)
		{
			int num = key.Length;
			List<NOD.TackleInfo> list = new List<NOD.TackleInfo>(num);
			for (int i = 0; i < num; i++)
			{
				NOD.TackleInfo tackle = NOD.getTackle(key[i]);
				if (tackle != null)
				{
					list.Add(tackle);
				}
			}
			return list.ToArray();
		}

		public static NOD.BasicData getBasicData(string key)
		{
			NOD.BasicData basicData = X.Get<string, NOD.BasicData>(NOD.OBasic, key);
			if (basicData == null)
			{
				X.de("BasicData " + key + " が不明", null);
			}
			return basicData;
		}

		public static NOD.MpConsume getMpConsume(string key)
		{
			NOD.MpConsume mpConsume = X.Get<string, NOD.MpConsume>(NOD.OConsume, key);
			if (mpConsume == null)
			{
				X.de("MpConsume " + key + " が不明", null);
			}
			return mpConsume;
		}

		private const string data_path = "Enemies/_nodd";

		private const string data_basename = "_nodd";

		public static readonly Regex RegTackle = new Regex("^TACKLE\\.");

		public static readonly Regex RegConsume = new Regex("^CONSUME\\.");

		public static readonly Regex RegShockwave = new Regex("^SHOCKWAVE\\.");

		private static BDic<string, NOD.TackleInfo> OTackle;

		private static BDic<string, NOD.ShockwaveInfo> OShockwave;

		private static BDic<string, NOD.BasicData> OBasic;

		private static BDic<string, NOD.MpConsume> OConsume;

		public static uint load_count;

		public class BasicData
		{
			public NOD.BasicData Set0(NelEnemy En)
			{
				return En.Set0(this);
			}

			public NOD.BasicData Set1(NelEnemy En)
			{
				return En.Set1(this);
			}

			public NOD.BasicData copyFrom(NOD.BasicData Src)
			{
				if (Src == null)
				{
					return this;
				}
				this.maxhp = Src.maxhp;
				this.maxmp = Src.maxmp;
				this.drop_mp_min = Src.drop_mp_min;
				this.drop_mp_max = Src.drop_mp_max;
				this.weight = Src.weight;
				this.anim_chara_name = Src.anim_chara_name;
				this.sizew_x = Src.sizew_x;
				this.sizew_y = Src.sizew_y;
				this.knockback_time0 = Src.knockback_time0;
				this.foot_bump_effect_enlarge_level = Src.foot_bump_effect_enlarge_level;
				this.od_damage_ratio = Src.od_damage_ratio;
				this.apply_damage_ratio_max_divide = Src.apply_damage_ratio_max_divide;
				this.od_mp_multiple = Src.od_mp_multiple;
				this.od_hp_multiple = Src.od_hp_multiple;
				this.od_killed_mana_splash = Src.od_killed_mana_splash;
				this.mana_desire_multiple = Src.mana_desire_multiple;
				this.killed_add_exp = Src.killed_add_exp;
				this.killed_add_exp_od = Src.killed_add_exp_od;
				this.flashbang_time_ratio = Src.flashbang_time_ratio;
				this.enlarge_publish_damage_ratio = Src.enlarge_publish_damage_ratio;
				this.enlarge_anim_scale_max = Src.enlarge_anim_scale_max;
				this.enlarge_od_anim_scale_min = Src.enlarge_od_anim_scale_min;
				this.enlarge_od_anim_scale_max = Src.enlarge_od_anim_scale_max;
				this.stun_time = Src.stun_time;
				this.basic_stun_ratio = Src.basic_stun_ratio;
				this.ser_resist_key = Src.ser_resist_key;
				this.drop_ratio_normal100 = Src.drop_ratio_normal100;
				this.drop_ratio_od100 = Src.drop_ratio_od100;
				this.DropItemNormal = Src.DropItemNormal;
				this.DropItemOd = Src.DropItemOd;
				return this;
			}

			public void readBasicDataCR(CsvReaderA CR)
			{
				string cmd = CR.cmd;
				if (cmd != null)
				{
					uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
					if (num > 1794232352U)
					{
						if (num <= 2934347616U)
						{
							if (num <= 2301162767U)
							{
								if (num <= 1933455812U)
								{
									if (num != 1800876423U)
									{
										if (num != 1933455812U)
										{
											goto IL_0797;
										}
										if (!(cmd == "knockback_time"))
										{
											goto IL_0797;
										}
										goto IL_0588;
									}
									else
									{
										if (!(cmd == "itemdrop_od"))
										{
											goto IL_0797;
										}
										if (TX.noe(CR._1))
										{
											this.DropItemOd = null;
											return;
										}
										NelItem byId = NelItem.GetById(CR._1, false);
										if (byId != null)
										{
											this.DropItemOd = byId;
											this.drop_ratio_od100 = (byte)CR.Int(2, 0);
										}
										return;
									}
								}
								else if (num != 2209446351U)
								{
									if (num != 2301162767U)
									{
										goto IL_0797;
									}
									if (!(cmd == "od_mp_multiple"))
									{
										goto IL_0797;
									}
									goto IL_05D8;
								}
								else
								{
									if (!(cmd == "enlarge_publish_damage_ratio"))
									{
										goto IL_0797;
									}
									this.enlarge_publish_damage_ratio = CR.Nm(1, this.enlarge_publish_damage_ratio);
									return;
								}
							}
							else if (num <= 2640708647U)
							{
								if (num != 2484692502U)
								{
									if (num != 2640708647U)
									{
										goto IL_0797;
									}
									if (!(cmd == "enlarge_anim_scale_max"))
									{
										goto IL_0797;
									}
									this.enlarge_anim_scale_max = CR.Nm(1, this.enlarge_anim_scale_max);
									return;
								}
								else
								{
									if (!(cmd == "od_killed_mana_splash"))
									{
										goto IL_0797;
									}
									this.od_killed_mana_splash = CR.Int(1, this.od_killed_mana_splash);
									return;
								}
							}
							else if (num != 2726189239U)
							{
								if (num != 2934347616U)
								{
									goto IL_0797;
								}
								if (!(cmd == "apply_damage_ratio_max_divide"))
								{
									goto IL_0797;
								}
								this.apply_damage_ratio_max_divide = CR.Nm(1, this.apply_damage_ratio_max_divide);
								return;
							}
							else if (!(cmd == "overdrive_hp_multiple"))
							{
								goto IL_0797;
							}
						}
						else if (num <= 3652794935U)
						{
							if (num <= 3379424283U)
							{
								if (num != 3014181560U)
								{
									if (num != 3379424283U)
									{
										goto IL_0797;
									}
									if (!(cmd == "itemdrop"))
									{
										goto IL_0797;
									}
									if (TX.noe(CR._1))
									{
										this.DropItemNormal = null;
										this.DropItemOd = null;
										return;
									}
									NelItem byId2 = NelItem.GetById(CR._1, false);
									if (byId2 != null)
									{
										this.DropItemNormal = byId2;
										this.drop_ratio_normal100 = (byte)CR.Int(2, 0);
										if (CR.clength >= 4)
										{
											this.DropItemOd = byId2;
											this.drop_ratio_od100 = (byte)CR.Int(3, 0);
										}
									}
									return;
								}
								else if (!(cmd == "od_hp_multiple"))
								{
									goto IL_0797;
								}
							}
							else if (num != 3547911330U)
							{
								if (num != 3652794935U)
								{
									goto IL_0797;
								}
								if (!(cmd == "overdrive_damage_ratio"))
								{
									goto IL_0797;
								}
								goto IL_05C4;
							}
							else
							{
								if (!(cmd == "anim_chara_name"))
								{
									goto IL_0797;
								}
								this.anim_chara_name = CR._1;
								return;
							}
						}
						else if (num <= 3817715362U)
						{
							if (num != 3762698377U)
							{
								if (num != 3817715362U)
								{
									goto IL_0797;
								}
								if (!(cmd == "mana_desire_multiple"))
								{
									goto IL_0797;
								}
								this.mana_desire_multiple = CR.Nm(1, this.mana_desire_multiple);
								return;
							}
							else
							{
								if (!(cmd == "flashbang_time_ratio"))
								{
									goto IL_0797;
								}
								this.flashbang_time_ratio = CR.Nm(1, this.flashbang_time_ratio);
								return;
							}
						}
						else if (num != 3951692174U)
						{
							if (num != 4214761112U)
							{
								goto IL_0797;
							}
							if (!(cmd == "ser_resist_key"))
							{
								goto IL_0797;
							}
							this.ser_resist_key = CR._1;
							return;
						}
						else
						{
							if (!(cmd == "foot_bump_effect_enlarge_level"))
							{
								goto IL_0797;
							}
							this.foot_bump_effect_enlarge_level = CR.Nm(1, this.foot_bump_effect_enlarge_level);
							return;
						}
						this.od_hp_multiple = CR.Nm(1, this.od_hp_multiple);
						return;
					}
					if (num <= 879568002U)
					{
						if (num <= 597743964U)
						{
							if (num <= 457430236U)
							{
								if (num != 74459250U)
								{
									if (num != 457430236U)
									{
										goto IL_0797;
									}
									if (!(cmd == "killed_add_exp"))
									{
										goto IL_0797;
									}
									this.killed_add_exp = CR.Nm(1, this.killed_add_exp);
									return;
								}
								else
								{
									if (!(cmd == "killed_add_exp_od"))
									{
										goto IL_0797;
									}
									this.killed_add_exp_od = CR.Nm(1, this.killed_add_exp_od);
									return;
								}
							}
							else if (num != 495286539U)
							{
								if (num != 597743964U)
								{
									goto IL_0797;
								}
								if (!(cmd == "size"))
								{
									goto IL_0797;
								}
								this.sizew_x = CR.Nm(1, this.sizew_x);
								this.sizew_y = CR.Nm(2, this.sizew_y);
								return;
							}
							else
							{
								if (!(cmd == "drop_mp_min"))
								{
									goto IL_0797;
								}
								this.drop_mp_min = CR.Int(1, this.drop_mp_min);
								return;
							}
						}
						else if (num <= 826051549U)
						{
							if (num != 603081627U)
							{
								if (num != 826051549U)
								{
									goto IL_0797;
								}
								if (!(cmd == "%CLONE"))
								{
									goto IL_0797;
								}
								NOD.BasicData basicData = NOD.getBasicData(CR._1);
								if (basicData != null)
								{
									this.copyFrom(basicData);
								}
								return;
							}
							else
							{
								if (!(cmd == "stun_time"))
								{
									goto IL_0797;
								}
								this.stun_time = CR.Int(1, this.stun_time);
								return;
							}
						}
						else if (num != 863114229U)
						{
							if (num != 879568002U)
							{
								goto IL_0797;
							}
							if (!(cmd == "od_damage_ratio"))
							{
								goto IL_0797;
							}
							goto IL_05C4;
						}
						else
						{
							if (!(cmd == "drop_mp_max"))
							{
								goto IL_0797;
							}
							this.drop_mp_max = CR.Int(1, this.drop_mp_max);
							return;
						}
					}
					else if (num <= 1352703673U)
					{
						if (num <= 1315530935U)
						{
							if (num != 1014813721U)
							{
								if (num != 1315530935U)
								{
									goto IL_0797;
								}
								if (!(cmd == "enlarge_od_anim_scale_max"))
								{
									goto IL_0797;
								}
								this.enlarge_od_anim_scale_max = CR.Nm(1, this.enlarge_od_anim_scale_max);
								return;
							}
							else
							{
								if (!(cmd == "enlarge_od_anim_scale_min"))
								{
									goto IL_0797;
								}
								this.enlarge_od_anim_scale_min = CR.Nm(1, this.enlarge_od_anim_scale_min);
								return;
							}
						}
						else if (num != 1326753129U)
						{
							if (num != 1352703673U)
							{
								goto IL_0797;
							}
							if (!(cmd == "weight"))
							{
								goto IL_0797;
							}
							this.weight = CR.Nm(1, this.weight);
							return;
						}
						else
						{
							if (!(cmd == "maxhp"))
							{
								goto IL_0797;
							}
							this.maxhp = CR.Int(1, this.maxhp);
							return;
						}
					}
					else if (num <= 1592304412U)
					{
						if (num != 1461632862U)
						{
							if (num != 1592304412U)
							{
								goto IL_0797;
							}
							if (!(cmd == "knockback_time0"))
							{
								goto IL_0797;
							}
						}
						else
						{
							if (!(cmd == "basic_stun_ratio"))
							{
								goto IL_0797;
							}
							this.basic_stun_ratio = CR.Nm(1, this.basic_stun_ratio);
							return;
						}
					}
					else if (num != 1629294724U)
					{
						if (num != 1794232352U)
						{
							goto IL_0797;
						}
						if (!(cmd == "overdrive_mp_multiple"))
						{
							goto IL_0797;
						}
						goto IL_05D8;
					}
					else
					{
						if (!(cmd == "maxmp"))
						{
							goto IL_0797;
						}
						this.maxmp = CR.Int(1, this.maxmp);
						return;
					}
					IL_0588:
					this.knockback_time0 = CR.Int(1, this.knockback_time0);
					return;
					IL_05C4:
					this.od_damage_ratio = CR.Nm(1, this.od_damage_ratio);
					return;
					IL_05D8:
					this.od_mp_multiple = CR.Nm(1, this.od_mp_multiple);
					return;
				}
				IL_0797:
				CR.tError("readBasicDataCR: 不明なコマンド: " + CR.cmd);
			}

			public int maxhp = -1;

			public int maxmp = -1;

			public int drop_mp_min = -1;

			public int drop_mp_max = -1;

			public float weight = -1000f;

			public string anim_chara_name;

			public float sizew_x = -1f;

			public float sizew_y = -1f;

			public int knockback_time0 = -1;

			public float foot_bump_effect_enlarge_level = -1f;

			public float apply_damage_ratio_max_divide = -1f;

			public float od_damage_ratio = -1f;

			public float od_mp_multiple = -1f;

			public float od_hp_multiple = -1f;

			public int od_killed_mana_splash = -1;

			public float mana_desire_multiple = -1f;

			public float killed_add_exp = -1f;

			public float killed_add_exp_od = -1f;

			public float enlarge_publish_damage_ratio = -1f;

			public float enlarge_anim_scale_max = -1f;

			public float enlarge_od_anim_scale_min = -1f;

			public float enlarge_od_anim_scale_max = -1f;

			public float basic_stun_ratio = -1f;

			public string ser_resist_key;

			public NelItem DropItemNormal;

			public byte drop_ratio_normal100;

			public NelItem DropItemOd;

			public byte drop_ratio_od100;

			public float flashbang_time_ratio = -1f;

			public int stun_time = -1;
		}

		public class TackleInfo
		{
			public bool abs_pos_flag
			{
				get
				{
					return this.radius < 0f;
				}
				set
				{
					this.radius = X.Abs(this.radius) * (float)X.MPF(!value);
				}
			}

			public float calc_difx_map(NelEnemy En)
			{
				return this.difx_map * (this.mul_sizex ? En.sizex : 1f) + (this.add_sizex ? En.sizex : 0f);
			}

			public float calc_dify_map(NelEnemy En)
			{
				return this.dify_map * (this.mul_sizey ? En.sizey : 1f) + (this.add_sizey ? En.sizey : 0f);
			}

			public float x_reachable
			{
				get
				{
					return this.difx_map + X.Abs(this.radius);
				}
			}

			public float y_reachable
			{
				get
				{
					return X.Abs(this.dify_map) + X.Abs(this.radius);
				}
			}

			public NOD.TackleInfo copyFrom(NOD.TackleInfo Src)
			{
				if (Src == null)
				{
					return this;
				}
				this.cur_load_count = Src.cur_load_count;
				this.difx_map = Src.difx_map;
				this.dify_map = Src.dify_map;
				this.projectile_power = Src.projectile_power;
				this.radius = Src.radius;
				this.no_consider_size = Src.no_consider_size;
				this.no_break_at_hit = Src.no_break_at_hit;
				this.reflect = Src.reflect;
				this.add_sizex = Src.add_sizex;
				this.add_sizey = Src.add_sizey;
				this.mul_sizex = Src.mul_sizex;
				this.mul_sizey = Src.mul_sizey;
				this.hitlock = Src.hitlock;
				this.Mcs = Src.Mcs;
				this.kill_on_target_hit = Src.kill_on_target_hit;
				this.consider_rotate = Src.consider_rotate;
				this._start_delay = Src._start_delay;
				this._prepare_delay = Src._prepare_delay;
				this._hold = Src._hold;
				this._after_delay = Src._after_delay;
				this._count = Src._count;
				this.hit_other = Src.hit_other;
				this.shape = Src.shape;
				return this;
			}

			public NOD.TackleInfo resetLoadedVariables()
			{
				return this.copyFrom(new NOD.TackleInfo());
			}

			public void readTaclkeInfoCR(CsvReaderA CR)
			{
				string cmd = CR.cmd;
				if (cmd != null)
				{
					uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
					if (num <= 2506343768U)
					{
						if (num <= 926677940U)
						{
							if (num <= 230313139U)
							{
								if (num != 182036190U)
								{
									if (num != 184732674U)
									{
										if (num != 230313139U)
										{
											goto IL_07C1;
										}
										if (!(cmd == "radius"))
										{
											goto IL_07C1;
										}
									}
									else
									{
										if (!(cmd == "_start_delay"))
										{
											goto IL_07C1;
										}
										this._start_delay = CR.Nm(1, this._start_delay);
										return;
									}
								}
								else
								{
									if (!(cmd == "x*="))
									{
										goto IL_07C1;
									}
									float num2 = CR.Nm(1, 0f);
									this.difx_map *= num2;
									return;
								}
							}
							else if (num <= 709362235U)
							{
								if (num != 455087854U)
								{
									if (num != 709362235U)
									{
										goto IL_07C1;
									}
									if (!(cmd == "abs"))
									{
										goto IL_07C1;
									}
									this.abs_pos_flag = CR.Nm(1, 1f) != 0f;
									return;
								}
								else
								{
									if (!(cmd == "y*sizey"))
									{
										goto IL_07C1;
									}
									this.dify_map = CR.Nm(1, 0f);
									this.add_sizey = false;
									this.mul_sizey = true;
									return;
								}
							}
							else if (num != 826051549U)
							{
								if (num != 926677940U)
								{
									goto IL_07C1;
								}
								if (!(cmd == "_after_delay"))
								{
									goto IL_07C1;
								}
								this._after_delay = CR.Nm(1, this._after_delay);
								return;
							}
							else
							{
								if (!(cmd == "%CLONE"))
								{
									goto IL_07C1;
								}
								NOD.TackleInfo tackle = NOD.getTackle(CR._1);
								if (tackle != null)
								{
									this.copyFrom(tackle);
								}
								return;
							}
						}
						else if (num <= 1755842327U)
						{
							if (num <= 1682070957U)
							{
								if (num != 1482915802U)
								{
									if (num != 1682070957U)
									{
										goto IL_07C1;
									}
									if (!(cmd == "_count"))
									{
										goto IL_07C1;
									}
									this._count = CR.Int(1, this._count);
									return;
								}
								else
								{
									if (!(cmd == "xy"))
									{
										goto IL_07C1;
									}
									this.difx_map = CR.Nm(1, 0f);
									this.dify_map = CR.Nm(2, 0f);
									this.add_sizex = (this.mul_sizex = (this.add_sizey = (this.mul_sizey = false)));
									return;
								}
							}
							else if (num != 1716095973U)
							{
								if (num != 1755842327U)
								{
									goto IL_07C1;
								}
								if (!(cmd == "xy*="))
								{
									goto IL_07C1;
								}
								float num2 = CR.Nm(1, 0f);
								this.difx_map *= num2;
								this.dify_map *= num2;
								return;
							}
							else
							{
								if (!(cmd == "hitlock"))
								{
									goto IL_07C1;
								}
								this.hitlock = CR.Int(1, -2);
								return;
							}
						}
						else if (num <= 2379146479U)
						{
							if (num != 2076077999U)
							{
								if (num != 2379146479U)
								{
									goto IL_07C1;
								}
								if (!(cmd == "kill_on_target_hit"))
								{
									goto IL_07C1;
								}
								this.kill_on_target_hit = CR.Nm(1, 1f) != 0f;
								return;
							}
							else
							{
								if (!(cmd == "_hold"))
								{
									goto IL_07C1;
								}
								this._hold = CR.Nm(1, this._hold);
								return;
							}
						}
						else if (num != 2462546090U)
						{
							if (num != 2506343768U)
							{
								goto IL_07C1;
							}
							if (!(cmd == "consider_rotate"))
							{
								goto IL_07C1;
							}
							this.consider_rotate = CR.Nm(1, 1f) != 0f;
							return;
						}
						else
						{
							if (!(cmd == "reflect"))
							{
								goto IL_07C1;
							}
							FEnum<HITTYPE>.TryParse(CR._1U, out this.reflect, true);
							return;
						}
					}
					else if (num <= 3031831110U)
					{
						if (num <= 2640172229U)
						{
							if (num != 2507958825U)
							{
								if (num != 2607378027U)
								{
									if (num != 2640172229U)
									{
										goto IL_07C1;
									}
									if (!(cmd == "y*="))
									{
										goto IL_07C1;
									}
									float num2 = CR.Nm(1, 0f);
									this.dify_map *= num2;
									return;
								}
								else
								{
									if (!(cmd == "hit_other"))
									{
										goto IL_07C1;
									}
									this.hit_other = CR.Nm(1, 1f) != 0f;
									return;
								}
							}
							else
							{
								if (!(cmd == "_prepare_delay"))
								{
									goto IL_07C1;
								}
								this._prepare_delay = CR.Nm(1, this._prepare_delay);
								return;
							}
						}
						else if (num <= 2841936065U)
						{
							if (num != 2646858022U)
							{
								if (num != 2841936065U)
								{
									goto IL_07C1;
								}
								if (!(cmd == "consume"))
								{
									goto IL_07C1;
								}
								this.Mcs = ((CR._1 == "") ? null : NOD.getMpConsume(CR._1));
								return;
							}
							else
							{
								if (!(cmd == "shape"))
								{
									goto IL_07C1;
								}
								this.shape = FEnum<RAYSHAPE>.Parse(CR._1, RAYSHAPE.CIRCLE);
								return;
							}
						}
						else if (num != 2959455711U)
						{
							if (num != 3031831110U)
							{
								goto IL_07C1;
							}
							if (!(cmd == "square"))
							{
								goto IL_07C1;
							}
							if (CR.Nm(1, 1f) != 0f)
							{
								this.shape = RAYSHAPE.RECT;
							}
							return;
						}
						else
						{
							if (!(cmd == "x+sizex"))
							{
								goto IL_07C1;
							}
							this.difx_map = CR.Nm(1, 0f);
							this.add_sizex = true;
							this.mul_sizex = false;
							return;
						}
					}
					else if (num <= 3951675832U)
					{
						if (num <= 3111723784U)
						{
							if (num != 3042668706U)
							{
								if (num != 3111723784U)
								{
									goto IL_07C1;
								}
								if (!(cmd == "no_consider_size"))
								{
									goto IL_07C1;
								}
								this.no_consider_size = CR.Nm(1, 1f) != 0f;
								return;
							}
							else
							{
								if (!(cmd == "x*sizex"))
								{
									goto IL_07C1;
								}
								this.difx_map = CR.Nm(1, 0f);
								this.add_sizex = false;
								this.mul_sizex = true;
								return;
							}
						}
						else if (num != 3507993648U)
						{
							if (num != 3951675832U)
							{
								goto IL_07C1;
							}
							if (!(cmd == "no_break_at_hit"))
							{
								goto IL_07C1;
							}
							this.no_break_at_hit = CR.Nm(1, 1f) != 0f;
							return;
						}
						else
						{
							if (!(cmd == "projectile_power"))
							{
								goto IL_07C1;
							}
							this.projectile_power = CR.Int(1, 0);
							return;
						}
					}
					else if (num <= 4162494471U)
					{
						if (num != 4144776981U)
						{
							if (num != 4162494471U)
							{
								goto IL_07C1;
							}
							if (!(cmd == "y+sizey"))
							{
								goto IL_07C1;
							}
							this.dify_map = CR.Nm(1, 0f);
							this.add_sizey = true;
							this.mul_sizey = false;
							return;
						}
						else if (!(cmd == "r"))
						{
							goto IL_07C1;
						}
					}
					else if (num != 4228665076U)
					{
						if (num != 4245442695U)
						{
							goto IL_07C1;
						}
						if (!(cmd == "x"))
						{
							goto IL_07C1;
						}
						this.difx_map = CR.Nm(1, 0f);
						this.add_sizex = (this.mul_sizex = false);
						return;
					}
					else
					{
						if (!(cmd == "y"))
						{
							goto IL_07C1;
						}
						this.dify_map = CR.Nm(1, 0f);
						this.add_sizey = (this.mul_sizey = false);
						return;
					}
					this.radius = CR.Nm(1, 0f);
					return;
				}
				IL_07C1:
				CR.tError("readTaclkeInfoCR: 不明なコマンド: " + CR.cmd);
			}

			public int mp_consume
			{
				get
				{
					if (this.Mcs == null)
					{
						return 0;
					}
					return this.Mcs.consume;
				}
			}

			public uint cur_load_count = NOD.load_count;

			public float difx_map;

			public float dify_map;

			public float radius = 0.8f;

			public bool no_consider_size;

			public bool no_break_at_hit;

			public bool add_sizex;

			public bool add_sizey;

			public bool mul_sizex;

			public bool mul_sizey;

			public HITTYPE reflect = HITTYPE.BREAK;

			public RAYSHAPE shape;

			public int projectile_power = -1;

			public int hitlock = -1;

			public bool kill_on_target_hit;

			public bool consider_rotate;

			public bool hit_other = true;

			public bool square;

			public NOD.MpConsume Mcs;

			public float _start_delay;

			public float _prepare_delay;

			public float _after_delay;

			public float _hold;

			public int _count;
		}

		public class ShockwaveInfo
		{
			public float calc_x_far_abs(NelEnemy En)
			{
				return this.x_far_abs * (this.mul_sizex ? En.sizex : 1f) + (this.add_sizex ? En.sizex : 0f);
			}

			public NOD.ShockwaveInfo copyFrom(NOD.ShockwaveInfo Src)
			{
				this.x_far_abs = Src.x_far_abs;
				this.map_high = Src.map_high;
				this.spd = Src.spd;
				this.forward = Src.forward;
				this.Mcs = Src.Mcs;
				this.add_sizex = Src.add_sizex;
				this.mul_sizex = Src.mul_sizex;
				return this;
			}

			public void readShockwaveCR(CsvReaderA CR)
			{
				string cmd = CR.cmd;
				if (cmd != null)
				{
					uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
					if (num <= 1821360658U)
					{
						if (num <= 826051549U)
						{
							if (num != 61612962U)
							{
								if (num == 826051549U)
								{
									if (cmd == "%CLONE")
									{
										NOD.ShockwaveInfo shockwave = NOD.getShockwave(CR._1);
										if (shockwave != null)
										{
											this.copyFrom(shockwave);
										}
										return;
									}
								}
							}
							else if (cmd == "x_far_abs+sizex")
							{
								this.x_far_abs = CR.Nm(1, 0f);
								this.add_sizex = true;
								this.mul_sizex = false;
								return;
							}
						}
						else if (num != 1734159300U)
						{
							if (num == 1821360658U)
							{
								if (cmd == "map_high")
								{
									this.map_high = CR.Nm(1, 0f);
									return;
								}
							}
						}
						else if (cmd == "backward")
						{
							this.forward = CR.Nm(1, 1f) == 0f;
							return;
						}
					}
					else if (num <= 3544013702U)
					{
						if (num != 2841936065U)
						{
							if (num == 3544013702U)
							{
								if (cmd == "x_far_abs")
								{
									this.x_far_abs = CR.Nm(1, 0f);
									this.add_sizex = false;
									this.mul_sizex = false;
									return;
								}
							}
						}
						else if (cmd == "consume")
						{
							this.Mcs = NOD.getMpConsume(CR._1);
							return;
						}
					}
					else if (num != 3628164822U)
					{
						if (num == 4273367263U)
						{
							if (cmd == "x_far_abs*sizex")
							{
								this.x_far_abs = CR.Nm(1, 0f);
								this.add_sizex = false;
								this.mul_sizex = true;
								return;
							}
						}
					}
					else if (cmd == "spd")
					{
						this.spd = CR.Nm(1, 0f);
						return;
					}
				}
				CR.tError("readShockwaveCR: 不明なコマンド: " + CR.cmd);
			}

			public int mp_consume
			{
				get
				{
					if (this.Mcs == null)
					{
						return 0;
					}
					return this.Mcs.consume;
				}
			}

			public float x_far_abs;

			public float map_high = 1f;

			public float spd = 0.34f;

			public bool forward = true;

			public NOD.MpConsume Mcs;

			public bool add_sizex;

			public bool mul_sizex;
		}

		public class MpConsume
		{
			public NOD.MpConsume copyFrom(NOD.MpConsume Src)
			{
				this.consume = Src.consume;
				this.release = Src.release;
				this.neutral_ratio = Src.neutral_ratio;
				return this;
			}

			public void readMpConsumeCR(CsvReaderA CR)
			{
				string cmd = CR.cmd;
				if (cmd != null)
				{
					if (cmd == "%CLONE")
					{
						NOD.MpConsume mpConsume = NOD.getMpConsume(CR._1);
						if (mpConsume != null)
						{
							this.copyFrom(mpConsume);
						}
						return;
					}
					if (cmd == "consume")
					{
						this.consume = CR.Int(1, this.consume);
						if (this.release < 0)
						{
							this.release = (int)((float)this.consume * 0.5f);
						}
						return;
					}
					if (cmd == "release")
					{
						this.release = CR.Int(1, this.release);
						return;
					}
					if (cmd == "cr")
					{
						this.consume = CR.Int(1, this.consume);
						this.release = CR.Int(2, this.release);
						return;
					}
					if (cmd == "neutral_ratio")
					{
						this.neutral_ratio = CR.Nm(1, this.neutral_ratio);
						return;
					}
					if (cmd == "c*r")
					{
						this.consume = CR.Int(1, this.consume);
						this.release = (int)((float)this.consume * CR.Nm(2, 0.5f));
						return;
					}
				}
				CR.tError("readMpConsumeCR: 不明なコマンド: " + CR.cmd);
			}

			public int consume;

			public int release = -1;

			public float neutral_ratio = 0.25f;

			public const float default_release_ratio = 0.5f;
		}
	}
}

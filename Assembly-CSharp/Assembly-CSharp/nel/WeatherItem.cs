using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class WeatherItem : IEfPInteractale
	{
		public static void initWeather()
		{
			WeatherItem.ADesc = new WeatherItem.WeatherDescription[7];
			WeatherItem.OCalcMax = new BDic<string, EfParticleFuncCalc>(2);
			WeatherItem.ODescSp = new BDic<string, WeatherItem.WeatherDescription>(1);
			WeatherItem.CalcItem = new EfParticleFuncCalc(null, "ZLINE", 1f);
			for (int i = 6; i >= 0; i--)
			{
				WeatherItem.ADesc[i] = new WeatherItem.WeatherDescription((WeatherItem.WEATHER)i, null);
			}
			CsvReader csvReader = new CsvReader(TX.getResource("Data/weather", ".csv", false), CsvReader.RegSpace, true);
			WeatherItem.WeatherDescription weatherDescription = null;
			bool flag = false;
			while (csvReader.read())
			{
				if (csvReader.cmd == "#MAX_COUNT")
				{
					WeatherItem.OCalcMax["_"] = new EfParticleFuncCalc(csvReader.slice_join(1, "|", ""), "Z0", 5f);
				}
				else if (csvReader.cmd == "#MAX_COUNT_SP")
				{
					WeatherItem.OCalcMax[csvReader._1] = new EfParticleFuncCalc(csvReader.slice_join(2, "|", ""), "Z0", 5f);
				}
				else
				{
					if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
					{
						string index = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
						WeatherItem.WEATHER weather;
						if (FEnum<WeatherItem.WEATHER>.TryParse(index.ToUpper(), out weather, true))
						{
							weatherDescription = WeatherItem.ADesc[(int)weather];
							flag = false;
						}
						else
						{
							flag = true;
							weatherDescription = (WeatherItem.ODescSp[index] = new WeatherItem.WeatherDescription(WeatherItem.WEATHER.NORMAL, null));
							weatherDescription.sp_key = index;
						}
					}
					if (weatherDescription != null)
					{
						string cmd = csvReader.cmd;
						if (cmd != null)
						{
							uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
							if (num <= 1414982785U)
							{
								if (num <= 826051549U)
								{
									if (num != 380752755U)
									{
										if (num == 826051549U)
										{
											if (cmd == "%CLONE")
											{
												WeatherItem.WEATHER weather2;
												WeatherItem.WeatherDescription weatherDescription2;
												if (FEnum<WeatherItem.WEATHER>.TryParse(csvReader._1.ToUpper(), out weather2, true))
												{
													weatherDescription2 = WeatherItem.ADesc[(int)weather2];
												}
												else
												{
													weatherDescription2 = global::XX.X.Get<string, WeatherItem.WeatherDescription>(WeatherItem.ODescSp, csvReader._1);
												}
												if (weatherDescription2 == null)
												{
													csvReader.tError("不明な type : " + csvReader._1);
												}
												else
												{
													weatherDescription.copyFrom(weatherDescription2);
												}
											}
										}
									}
									else if (cmd == "init")
									{
										weatherDescription.init_chance = csvReader.slice_join(1, "|", "");
									}
								}
								else if (num != 1200064310U)
								{
									if (num != 1361572173U)
									{
										if (num == 1414982785U)
										{
											if (cmd == "ptcst")
											{
												weatherDescription.ptcst = csvReader._1;
											}
										}
									}
									else if (cmd == "type")
									{
										WeatherItem.WEATHER weather2;
										if (!flag)
										{
											csvReader.tError("type 指定は ODescSp 用の天候指定にしか使用できない");
										}
										else if (FEnum<WeatherItem.WEATHER>.TryParse(csvReader._1.ToUpper(), out weather2, true))
										{
											weatherDescription.wt = weather2;
										}
										else
										{
											csvReader.tError("不明な type : " + csvReader._1);
										}
									}
								}
								else if (cmd == "quit")
								{
									weatherDescription.quit_chance = csvReader.slice_join(1, "|", "");
								}
							}
							else if (num <= 2671666642U)
							{
								if (num != 2538552783U)
								{
									if (num != 2617892938U)
									{
										if (num == 2671666642U)
										{
											if (cmd == "focus_to_center_pr")
											{
												weatherDescription.focus_to_center_pr = csvReader.Nm(1, 1f) != 0f;
											}
										}
									}
									else if (cmd == "ef_intv")
									{
										weatherDescription.effect_min_intv = csvReader.Int(1, 60);
										weatherDescription.effect_max_intv = csvReader.Int(2, weatherDescription.effect_min_intv);
									}
								}
								else if (cmd == "conflict")
								{
									int clength = csvReader.clength;
									for (int j = 1; j < clength; j++)
									{
										WeatherItem.WEATHER weather3;
										if (FEnum<WeatherItem.WEATHER>.TryParse(csvReader.getIndex(j), out weather3, true))
										{
											weatherDescription.conflict |= 1U << (int)weather3;
										}
									}
								}
							}
							else if (num != 3023287968U)
							{
								if (num != 3167761502U)
								{
									if (num == 4287243090U)
									{
										if (cmd == "ptc_top")
										{
											weatherDescription.ptc_top = csvReader.Nm(1, 1f) != 0f;
										}
									}
								}
								else if (cmd == "follow_spd")
								{
									weatherDescription.follow_spd = csvReader.Nm(1, 0f);
								}
							}
							else if (cmd == "snd")
							{
								weatherDescription.snd_key = csvReader._1;
							}
						}
					}
				}
			}
		}

		public WeatherItem(WeatherItem.WEATHER _weather, int _start_dlevel)
			: this(_weather, _start_dlevel, null)
		{
		}

		private WeatherItem(WeatherItem.WEATHER _weather, int _start_dlevel, WeatherItem.WeatherDescription _Desc)
		{
			this.weather = _weather;
			this.start_dlevel = _start_dlevel;
			if (_Desc == null)
			{
				for (int i = 6; i >= 0; i--)
				{
					if (WeatherItem.ADesc[i].wt == this.weather)
					{
						this.Desc = WeatherItem.ADesc[i];
						break;
					}
				}
			}
			else
			{
				this.Desc = _Desc;
			}
			if (this.weather == WeatherItem.WEATHER.MIST)
			{
				this.DrM = new M2FillingMistDrawer(null, MTR.AEfSmokeL)
				{
					C = C32.d2c(1725298906U),
					len_divide = 0.0020833334f,
					assign_to_final_render = true,
					thresh_len_pixel = 290f,
					draw_scale = 4f
				};
			}
			if (this.weather == WeatherItem.WEATHER.MIST_DENSE)
			{
				this.DrM = new M2FillingMistDrawer(null, MTR.AEfSmokeL)
				{
					C = C32.d2c(4292212954U),
					assign_to_final_render = true,
					thresh_len_pixel = 200f,
					len_divide = 0.0038461538f,
					draw_scale = 2.5f
				};
			}
			if (this.DrM != null)
			{
				this.DrM.cache_gob = true;
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeUInt((uint)this.weather);
			Ba.writeShort((short)this.start_dlevel);
			Ba.writePascalString(this.Desc.sp_key, "utf-8");
		}

		public static WeatherItem readFrom(ByteArray Ba, int vers)
		{
			try
			{
				WeatherItem.WEATHER weather = (WeatherItem.WEATHER)Ba.readUInt();
				int num = (int)Ba.readShort();
				WeatherItem.WeatherDescription weatherDescription = null;
				if (vers >= 6)
				{
					string text = Ba.readPascalString("utf-8", false);
					if (TX.valid(text))
					{
						weatherDescription = global::XX.X.Get<string, WeatherItem.WeatherDescription>(WeatherItem.ODescSp, text);
					}
				}
				return new WeatherItem(weather, num, weatherDescription)
				{
					inits_immediate = 1
				};
			}
			catch
			{
			}
			return null;
		}

		public WeatherItem initS(Map2d Mp, bool immediate = false)
		{
			if (Mp == null)
			{
				Mp = M2DBase.Instance.curMap;
			}
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
			if (this.St != null)
			{
				this.St.kill(false);
				this.St = null;
			}
			this.next_set_floort = global::XX.X.NIXP(30f, 80f);
			if (this.weather == WeatherItem.WEATHER.THUNDER)
			{
				(M2DBase.Instance as NelM2DBase).FlagRain.Add("WEATHER_THUNDER");
			}
			if (this.SndLoop == null && this.Desc.snd_key != null && !global::XX.X.DEBUGNOSND)
			{
				SND.loadSheets(this.Desc.snd_key, this.weather.ToString());
				this.SndLoop = new SndPlayer(this.weather.ToString(), SndPlayer.SNDTYPE.SND);
				this.SndLoop.prepare(this.Desc.snd_key, false);
				this.SndLoop.getPlayerInstance().SetEnvelopeReleaseTime(2400f);
				this.SndLoop.FineVol();
				this.SndLoop.playDefault();
				this.SndLoop.need_update_flag = false;
			}
			if (this.DrM != null)
			{
				this.DrM.assignTo(Mp);
			}
			if (immediate)
			{
				this.inits_immediate = 1;
			}
			return this;
		}

		public void showLog()
		{
			UILog.Instance.AddAlertTX("Weather_init_log_" + this.weather.ToString().ToLower(), UILogRow.TYPE.ALERT_GRAY).setIcon(MTRX.getPF("weather." + ((int)this.weather).ToString()), uint.MaxValue);
		}

		public bool run(float fcnt, Map2d Mp)
		{
			bool flag = false;
			if (this.inits_immediate > 0)
			{
				this.inits_immediate--;
				this.next_set_floort = 0f;
				flag = true;
				if (this.DrM != null)
				{
					this.DrM.fine();
				}
			}
			if (this.next_set_floort >= 0f)
			{
				this.next_set_floort -= fcnt;
				if (this.next_set_floort < 0f || flag)
				{
					this.St = null;
					this.next_set_floort = global::XX.X.NIXP((float)this.Desc.effect_min_intv, (float)this.Desc.effect_max_intv);
					if (this.Desc.ptcst != "")
					{
						this.St = Mp.PtcSTF(this.Desc.ptcst, this, true, this.Desc.ptc_top);
						if (this.St != null)
						{
							this.fine_pos = true;
							if (flag)
							{
								this.St.updateFrameStockEffect((float)this.Desc.effect_min_intv * 0.25f);
								this.next_set_floort = (float)this.Desc.effect_min_intv * 0.75f;
							}
						}
					}
				}
			}
			if (this.SndLoop != null && this.SndLoop.need_update_flag)
			{
				this.SndLoop.FineVol();
				this.SndLoop.need_update_flag = false;
			}
			return true;
		}

		public bool getEffectReposition(PTCThread St, PTCThread.StFollow follow, float fcnt, out Vector3 V)
		{
			Map2d curMap = M2DBase.Instance.curMap;
			float drawx = curMap.M2D.Cam.x;
			float drawy = curMap.M2D.Cam.y;
			if (this.Desc.focus_to_center_pr)
			{
				M2MoverPr pr = curMap.Pr;
				if (pr != null)
				{
					drawx = pr.drawx;
					drawy = pr.drawy;
				}
			}
			if (this.fine_pos)
			{
				this.x = drawx * curMap.rCLEN;
				this.y = drawy * curMap.rCLEN;
				this.fine_pos = false;
			}
			else
			{
				this.x = global::XX.X.VALWALK(this.x, drawx * curMap.rCLEN, this.Desc.follow_spd * fcnt);
				this.y = global::XX.X.VALWALK(this.y, drawy * curMap.rCLEN, this.Desc.follow_spd * fcnt);
			}
			V = new Vector2(this.x, this.y);
			return true;
		}

		public void destruct()
		{
			if (this.weather == WeatherItem.WEATHER.THUNDER)
			{
				(M2DBase.Instance as NelM2DBase).FlagRain.Rem("WEATHER_THUNDER");
			}
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
			if (this.DrM != null)
			{
				this.DrM.deactivate();
				this.DrM.cache_gob = false;
				(M2DBase.Instance as NelM2DBase).NightCon.addRemovingMst(this.DrM);
				this.DrM = null;
			}
			if (this.St != null)
			{
				this.St = null;
			}
			if (this.Desc.snd_key != null)
			{
				SND.unloadSheets(this.Desc.snd_key, this.weather.ToString());
			}
			if (this.SndLoop != null)
			{
				this.SndLoop.Stop();
				this.SndLoop.Dispose();
				this.SndLoop = null;
			}
		}

		public int getThunderOverdriveCount(NightController Con, EnemySummoner Smn)
		{
			if (this.weather == WeatherItem.WEATHER.THUNDER)
			{
				return global::XX.X.Mx((int)(Con.getDangerLevel() + 0.4f), 1);
			}
			return 0;
		}

		public int summoner_max_addition(EnemySummoner Smn)
		{
			WeatherItem.WEATHER weather = this.weather;
			if (weather == WeatherItem.WEATHER.WIND)
			{
				return -2;
			}
			if (weather == WeatherItem.WEATHER.DROUGHT)
			{
				return 1;
			}
			return 0;
		}

		public static bool getEH(EnhancerManager.EH ehbit)
		{
			return (EnhancerManager.enhancer_bits & ehbit) > (EnhancerManager.EH)0U;
		}

		public static void weatherShuffle(List<WeatherItem> AList, int dlevel, int pre_cweather)
		{
			float num = (float)dlevel / 16f;
			WeatherItem.WeatherDescription weatherDescription = (WeatherItem.getEH(EnhancerManager.EH.raincaller) ? global::XX.X.Get<string, WeatherItem.WeatherDescription>(WeatherItem.ODescSp, "_eh_raincaller") : null);
			string text = null;
			if (weatherDescription != null)
			{
				text = "_eh_raincaller";
			}
			int maxCount = WeatherItem.getMaxCount(dlevel, text);
			if (AList.Count >= maxCount)
			{
				return;
			}
			NightController.shuffle<WeatherItem.WeatherDescription>(WeatherItem.ADesc, -1);
			for (int i = 6; i >= 0; i--)
			{
				WeatherItem.WeatherDescription weatherDescription2 = WeatherItem.ADesc[i];
				if ((pre_cweather & (1 << (int)weatherDescription2.wt)) == 0)
				{
					float num2 = 1f;
					if (weatherDescription != null)
					{
						if (weatherDescription2.wt == weatherDescription.wt)
						{
							weatherDescription2 = weatherDescription;
						}
						else
						{
							num2 *= 0.6f;
						}
					}
					num2 *= WeatherItem.Calc(weatherDescription2.init_chance, num, 5f);
					if (NightController.XORSP() < num2)
					{
						WeatherItem weatherItem = new WeatherItem(weatherDescription2.wt, dlevel, weatherDescription2);
						weatherItem.showLog();
						if (weatherDescription2.conflict != 0U)
						{
							for (int j = AList.Count - 1; j >= 0; j--)
							{
								if (((1U << (int)AList[j].weather) & weatherDescription2.conflict) != 0U)
								{
									AList[j].destruct();
									AList.RemoveAt(j);
								}
							}
						}
						AList.Add(weatherItem.initS(null, false));
						if (AList.Count >= maxCount)
						{
							break;
						}
					}
				}
			}
		}

		public bool checkQuit(int dlevel, bool execute_destruct = true)
		{
			float num = WeatherItem.Calc(this.Desc.quit_chance, (float)(dlevel - this.start_dlevel), 80f);
			if (NightController.XORSP() < num)
			{
				if (execute_destruct)
				{
					this.destruct();
				}
				return true;
			}
			return false;
		}

		public bool readPtcScript(PTCThread St)
		{
			return M2DBase.Instance.readPtcScript(St);
		}

		public static int getMaxCount(int dlevel, string fn_key)
		{
			EfParticleFuncCalc efParticleFuncCalc = ((fn_key != null) ? global::XX.X.Get<string, EfParticleFuncCalc>(WeatherItem.OCalcMax, fn_key) : null);
			if (efParticleFuncCalc == null)
			{
				efParticleFuncCalc = WeatherItem.OCalcMax["_"];
			}
			return (int)efParticleFuncCalc.Get(null, (float)dlevel / 16f);
		}

		public bool isSoundActive(SndPlayer S)
		{
			M2SoundPlayerItem m2SoundPlayerItem = S as M2SoundPlayerItem;
			return m2SoundPlayerItem != null && TX.isStart(m2SoundPlayerItem.key, this.getSoundKey(), 0);
		}

		public bool initSetEffect(PTCThread Thread, EffectItem Ef)
		{
			return true;
		}

		public string getSoundKey()
		{
			return "Weather";
		}

		public uint get_conflict()
		{
			return this.Desc.conflict;
		}

		private static float Calc(string s, float v, float max_val)
		{
			WeatherItem.CalcItem.Remake(s, "Z0", max_val);
			return WeatherItem.CalcItem.Get(null, v);
		}

		private static WeatherItem.WeatherDescription[] ADesc;

		private static BDic<string, WeatherItem.WeatherDescription> ODescSp;

		private static EfParticleFuncCalc CalcItem;

		private static BDic<string, EfParticleFuncCalc> OCalcMax;

		private const float amagoi_other_weaken = 0.6f;

		public readonly WeatherItem.WEATHER weather;

		private WeatherItem.WeatherDescription Desc;

		private SndPlayer SndLoop;

		public int start_dlevel;

		private M2DrawBinder Ed;

		private PTCThread St;

		public bool fine_pos = true;

		public float x;

		public float y;

		public float next_set_floort = 60f;

		public int inits_immediate;

		public M2FillingMistDrawer DrM;

		public enum WEATHER : uint
		{
			NORMAL,
			WIND,
			THUNDER,
			MIST,
			DROUGHT,
			MIST_DENSE,
			PLAGUE,
			_MAX
		}

		private sealed class WeatherDescription
		{
			public WeatherDescription(WeatherItem.WEATHER _wt, string _sp_key = null)
			{
				this.wt = _wt;
				this.sp_key = _sp_key;
			}

			public void copyFrom(WeatherItem.WeatherDescription Src)
			{
				this.init_chance = Src.init_chance;
				this.quit_chance = Src.quit_chance;
				this.effect_min_intv = Src.effect_min_intv;
				this.effect_max_intv = Src.effect_max_intv;
				this.conflict = Src.conflict;
				this.ptcst = Src.ptcst;
				this.snd_key = Src.snd_key;
				this.follow_spd = Src.follow_spd;
				this.ptc_top = Src.ptc_top;
				if (this.sp_key != null)
				{
					this.wt = Src.wt;
				}
			}

			public WeatherItem.WEATHER wt;

			public string init_chance = "0";

			public string quit_chance = "0";

			public int effect_min_intv = 120;

			public int effect_max_intv = 120;

			public uint conflict;

			public string ptcst = "";

			public bool ptc_top;

			public bool focus_to_center_pr;

			public string snd_key;

			public float follow_spd = 0.03125f;

			public string sp_key;
		}
	}
}

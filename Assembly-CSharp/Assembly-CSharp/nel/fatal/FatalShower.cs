using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel.fatal
{
	public sealed class FatalShower : IRunAndDestroy, INelMSG, IEventWaitListener
	{
		public FatalShower(string script_key, bool from_memory = false)
		{
			this.script_key = script_key;
			this.from_memory = from_memory;
			this.WhLis = new WheelListener(true);
			IN.setScreenOrtho(true);
			this.fineScreenSize();
			this.CamForMain = IN.getGUICamera();
			this.OLoadSpine = new BDic<string, SpvLoader>(2);
			this.OLoadedTexture = new BDic<string, MTIOneImage>(2);
			this.OVoiceSrc = new BDic<string, VoiceController>(1);
			this.MtrTransitionBite = MTR.newMtr("Hachan/ShaderBiteTransition");
			MTRX.setMaterialST(this.MtrTransitionBite, "_MainTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			if (FatalShower.MtrBiteTransS == null)
			{
				FatalShower.MtrBiteTransS = MTRX.newMtr(this.MtrTransitionBite);
			}
			this.DrRad = new RadiationDrawer();
			this.CR = new CsvReaderA(TX.getResource("Fatal/" + script_key + ".fatal", ".csv", false), new CsvVariableContainer());
			this.CR.tilde_replace = true;
			SND.loadSheets("fatal", script_key);
			NelMSGResource.initResource(false);
			this.FD_WindowSizeChanged = delegate(int w, int h)
			{
				this.fineScreenSize();
				this.fineScreenBuffer(this.cur_buffer);
			};
			IN.FD_WindowSizeChanged = (IN.FnWindowSizeChanged)Delegate.Combine(IN.FD_WindowSizeChanged, this.FD_WindowSizeChanged);
			this.initScript();
			this.M2D = M2DBase.Instance as NelM2DBase;
			if (this.M2D != null)
			{
				this.M2D.FlgHideWholeScreen.Add("FATAL");
				UIBase.FlgLetterBoxFade.Add("FATAL");
			}
		}

		private void fineScreenSize()
		{
			if (this.t_book >= 0f)
			{
				this.t_book = X.Mn(this.t_book, 21f);
			}
			else
			{
				this.t_book = X.Mx(this.t_book, -21f);
			}
			float num = FatalShower.sc_scale;
			float num2 = FatalShower.sc_scale;
			FatalShower.scw = (float)X.Mx(1, Screen.width);
			FatalShower.sch = (float)X.Mx(1, Screen.height);
			FatalShower.ui_scale = 1f;
			if (FatalShower.scw == IN.w && FatalShower.sch == IN.h)
			{
				num2 = (FatalShower.sc_scale = 1f);
				FatalShower.ui_scale = 1f;
				FatalShower.scvw = IN.w;
				FatalShower.scvh = IN.h;
			}
			else
			{
				num2 = (FatalShower.sc_scale = Mathf.Sqrt(FatalShower.scw / IN.w * FatalShower.sch / IN.h));
				if (FatalShower.sc_scale > 1.5f)
				{
					float num3 = 1.5f / FatalShower.sc_scale;
					FatalShower.ui_scale = FatalShower.sc_scale / 1.5f;
					FatalShower.sc_scale = 1.5f;
					FatalShower.scw *= num3;
					FatalShower.sch *= num3;
				}
				FatalShower.scvw = FatalShower.scw / FatalShower.sc_scale;
				FatalShower.scvh = FatalShower.sch / FatalShower.sc_scale;
			}
			FatalShower.scwh = FatalShower.scw * 0.5f;
			FatalShower.schh = FatalShower.sch * 0.5f;
			FatalShower.scvwh = FatalShower.scvw * 0.5f;
			FatalShower.scvhh = FatalShower.scvh * 0.5f;
			if (this.MenuEar != null && this.AALay != null)
			{
				this.WhLis.x_scroll_level = 64f / num2;
				this.WhLis.y_scroll_level = 64f / num2;
				this.GobUi.transform.localScale = new Vector3(FatalShower.ui_scale, FatalShower.ui_scale, 1f);
				this.MenuEar.scw = FatalShower.scw;
				this.MenuEar.sch = FatalShower.sch;
				this.WhLis.resetShowingArea(-1000f, -1000f, -1000f, FatalShower.scvw, FatalShower.scvh);
				this.TxReposit();
				this.BookReposit();
				if (this.EditCfg == null)
				{
					IN.PosP2(this.BxMenu.transform, -FatalShower.scwh + 220f, 0f);
					this.MenuEar.finePositionExecute();
				}
				List<FtLayer> list = this.AALay[this.cur_buffer];
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					list[i].need_refill = true;
				}
				bool flag = this.freecam;
				this.need_redraw_bg = true;
				this.initClipArea(false);
			}
		}

		public FatalShower initScript()
		{
			this.destructResources();
			this.CR.seekReset();
			this.ACutPoint = new List<FatalShower.FCut>(16);
			this.dot_count = 0;
			this.max_spine_count = 0;
			this.cur_cut = -1;
			string text = "";
			FatalShower.BgInfo bgInfo = default(FatalShower.BgInfo);
			string[] array = null;
			int num = -1;
			float num2 = 0f;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			string text2 = null;
			string[][] array2 = null;
			FatalShower.AnmInfo[][] array3 = null;
			this.OAMosaicInfo = new BDic<string, List<MosaicShower.MosaicInfo>>();
			BDic<string, List<VoiceInfo>> bdic = null;
			int[] array4 = null;
			float[] array5 = null;
			Vector3 vector = new Vector3(0f, 0f, 0.33f);
			List<string> list = new List<string>(1);
			List<int> list2 = new List<int>(1);
			Rect rect = new Rect(0f, 0f, 0f, 0f);
			CsvReaderA csvReaderA = null;
			bool flag9 = false;
			FatalShower.FCut fcut = null;
			while (this.CR.read())
			{
				string cmd = this.CR.cmd;
				if (cmd != null)
				{
					uint num3 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
					if (num3 <= 1891474104U)
					{
						if (num3 <= 989430776U)
						{
							if (num3 <= 316823588U)
							{
								if (num3 != 68177146U)
								{
									if (num3 != 135386068U)
									{
										if (num3 != 316823588U)
										{
											goto IL_11E0;
										}
										if (!(cmd == "BG_EVPIC"))
										{
											goto IL_11E0;
										}
									}
									else
									{
										if (!(cmd == "BASE_SPINE"))
										{
											goto IL_11E0;
										}
										text2 = this.CR._1;
										if (fcut != null)
										{
											fcut.base_spine = text2;
											continue;
										}
										continue;
									}
								}
								else
								{
									if (!(cmd == "%BGM"))
									{
										goto IL_11E0;
									}
									this.bgm_key = this.CR._1;
									continue;
								}
							}
							else
							{
								if (num3 != 433934950U)
								{
									if (num3 != 876073928U)
									{
										if (num3 != 989430776U)
										{
											goto IL_11E0;
										}
										if (!(cmd == "%MOSAIC"))
										{
											goto IL_11E0;
										}
									}
									else if (!(cmd == "%MOSAIC_ATTACHMENT"))
									{
										goto IL_11E0;
									}
									List<MosaicShower.MosaicInfo> list3 = X.Get<string, List<MosaicShower.MosaicInfo>>(this.OAMosaicInfo, this.CR._1);
									if (list3 == null)
									{
										list3 = (this.OAMosaicInfo[this.CR._1] = new List<MosaicShower.MosaicInfo>(1));
									}
									list3.Add(new MosaicShower.MosaicInfo
									{
										bone_key = this.CR._2,
										radius = this.CR.Nm(3, 60f),
										only_appear_sensitive = (this.CR.Nm(4, 0f) != 0f),
										for_attachment = (this.CR.cmd == "%MOSAIC_ATTACHMENT")
									});
									continue;
								}
								if (!(cmd == "BG"))
								{
									goto IL_11E0;
								}
							}
							if (this.CR.cmd == "BG")
							{
								MImage bgTexture = this.getBgTexture(this.CR._1, true);
								if (bgTexture == null)
								{
									this.CR.tError("不明な背景テクスチャ: " + this.CR._1);
									continue;
								}
								bgInfo.MI = bgTexture;
								bgInfo.Evi = null;
							}
							else
							{
								if (!EV.ev_prepared)
								{
									this.CR.tError("event Pict が読み込まれていません: " + this.CR._1);
									continue;
								}
								EvImg pic = EV.Pics.getPic(this.CR._1, true, true);
								if (pic == null)
								{
									continue;
								}
								if (EV.Pics.cacheReadFor(pic) != null)
								{
									this.evt_load_wait = true;
								}
								MImage mi = MTRX.getMI(pic.PF.pChar, false);
								if (mi == null)
								{
									this.CR.tError("読み込まれていません" + pic.PF.pChar.title);
									continue;
								}
								bgInfo.MI = mi;
								bgInfo.Evi = pic;
							}
							bgInfo.scale = X.Nm(this.CR._2, 2f, false);
							bgInfo.scroll_ratio = X.Nm(this.CR._3, 0.8f, false);
							if (fcut != null)
							{
								fcut.Bg = bgInfo;
								continue;
							}
							continue;
						}
						else if (num3 <= 1224525797U)
						{
							if (num3 != 1099064151U)
							{
								if (num3 != 1144928650U)
								{
									if (num3 != 1224525797U)
									{
										goto IL_11E0;
									}
									if (!(cmd == "%SPINE_SV"))
									{
										goto IL_11E0;
									}
								}
								else
								{
									if (!(cmd == "ROTATE"))
									{
										goto IL_11E0;
									}
									if (!(this.CR._1 == "#0"))
									{
										continue;
									}
									num2 = this.CR.Nm(2, 0f);
									if (flag3)
									{
										continue;
									}
									flag3 = true;
									if (fcut != null)
									{
										fcut.spine_ag360 = num2;
										continue;
									}
									continue;
								}
							}
							else
							{
								if (!(cmd == "SPINE_STENCIL"))
								{
									goto IL_11E0;
								}
								num = this.CR.Int(1, -1);
								if (flag2)
								{
									continue;
								}
								flag2 = true;
								if (fcut != null)
								{
									fcut.spine_stencil = num;
									continue;
								}
								continue;
							}
						}
						else if (num3 <= 1580586464U)
						{
							if (num3 != 1416396903U)
							{
								if (num3 != 1580586464U)
								{
									goto IL_11E0;
								}
								if (!(cmd == "SPEED"))
								{
									goto IL_11E0;
								}
								if (!flag7)
								{
									flag7 = true;
									array5 = new float[array.Length];
									if (fcut != null)
									{
										fcut.Aspeed = array5;
									}
								}
								if (this.CR.clength <= 2)
								{
									for (int i = 0; i < array5.Length; i++)
									{
										array5[i] = this.CR.Nm(1, 0f);
									}
									continue;
								}
								int num4 = ((this.CR._1 == "_") ? 0 : X.isinStr(array, this.CR._1, -1));
								if (num4 >= 0)
								{
									array5[num4] = this.CR.Nm(2, 0f);
									continue;
								}
								continue;
							}
							else
							{
								if (!(cmd == "LABEL_HEADER"))
								{
									goto IL_11E0;
								}
								text = this.getMsgLabelHeader(ref text, this.CR._1);
								continue;
							}
						}
						else if (num3 != 1643791826U)
						{
							if (num3 != 1891474104U)
							{
								goto IL_11E0;
							}
							if (!(cmd == "SKIN"))
							{
								goto IL_11E0;
							}
							if (!flag)
							{
								flag = true;
								array2 = new string[array.Length][];
								if (fcut != null)
								{
									fcut.AAskin = array2;
								}
							}
							string[] array6 = this.CR.slice(1, -1000);
							for (int j = 0; j < array2.Length; j++)
							{
								array2[j] = array6;
							}
							continue;
						}
						else
						{
							if (!(cmd == "VO"))
							{
								goto IL_11E0;
							}
							goto IL_0C99;
						}
					}
					else if (num3 <= 3412022864U)
					{
						if (num3 <= 2719294369U)
						{
							if (num3 != 2372857733U)
							{
								if (num3 != 2565374185U)
								{
									if (num3 != 2719294369U)
									{
										goto IL_11E0;
									}
									if (!(cmd == "ANIM+"))
									{
										goto IL_11E0;
									}
									int num5 = ((this.CR._1 == "_") ? 0 : X.isinStr(array, this.CR._1, -1));
									if (num5 < 0)
									{
										this.CR.tError("不明なspine: " + this.CR._1);
										continue;
									}
									if (!flag4)
									{
										flag4 = true;
										FatalShower.AnmInfo[][] array7 = array3;
										array3 = new FatalShower.AnmInfo[array.Length][];
										if (array7 != null)
										{
											int num6 = X.Mn(array.Length, array7.Length);
											for (int k = 0; k < num6; k++)
											{
												array3[k] = ((array7[k] == null) ? null : X.concat<FatalShower.AnmInfo>(array7[k], null, -1, -1));
											}
										}
										if (fcut != null)
										{
											fcut.AAanim = array3;
										}
									}
									FatalShower.AnmInfo[] array8 = array3[num5];
									int num7 = this.CR.Int(2, 0);
									if (array8 == null)
									{
										array8 = (array3[num5] = new FatalShower.AnmInfo[num7 + 1]);
									}
									else if (array8.Length <= num7)
									{
										Array.Resize<FatalShower.AnmInfo>(ref array8, num7 + 1);
										array3[num5] = array8;
									}
									int num8 = this.CR.Int(4, -1001);
									if (num8 == -1001)
									{
										num8 = array8[num7].loop_to;
									}
									if (num7 == 0 && num8 >= 0)
									{
										if (!flag5)
										{
											flag5 = true;
											array4 = ((array4 == null) ? new int[array.Length] : X.concat<int>(array4, null, -1, -1));
											if (fcut != null)
											{
												fcut.Aloop_to = array4;
											}
										}
										array4[num5] = num8;
									}
									array8[num7] = new FatalShower.AnmInfo(this.CR._3, num8, this.CR.Int(5, 0));
									continue;
								}
								else
								{
									if (!(cmd == "POS"))
									{
										goto IL_11E0;
									}
									goto IL_0F67;
								}
							}
							else
							{
								if (!(cmd == "%TRM"))
								{
									goto IL_11E0;
								}
								this.bgm_key = ((this.CR._1 == "B") ? "BGM_bukiyou_na_hutari" : "BGM_yatto_deaeta");
								bgInfo = new FatalShower.BgInfo(this.getBgTexture("troom_bg", false))
								{
									scale = -1f,
									scroll_ratio = 0f
								};
								this.msg_header = "___Trm";
								this.CR.VarCon.define("_is_A", (this.CR._1 == "B") ? "0" : "1", true);
								continue;
							}
						}
						else if (num3 != 2815240503U)
						{
							if (num3 != 3096637805U)
							{
								if (num3 != 3412022864U)
								{
									goto IL_11E0;
								}
								if (!(cmd == "ANIM"))
								{
									goto IL_11E0;
								}
								if (!flag4)
								{
									flag4 = true;
									array3 = new FatalShower.AnmInfo[array.Length][];
									if (fcut != null)
									{
										fcut.AAanim = array3;
									}
								}
								FatalShower.AnmInfo[] array9 = null;
								if (this.CR.clength > 1)
								{
									array9 = new FatalShower.AnmInfo[this.CR.clength - 1];
									for (int l = 1; l < this.CR.clength; l++)
									{
										array9[l - 1] = new FatalShower.AnmInfo(this.CR.getIndex(l), -1000, 0);
									}
								}
								for (int m = 0; m < array3.Length; m++)
								{
									array3[m] = array9;
								}
								continue;
							}
							else
							{
								if (!(cmd == "CLIPRECT"))
								{
									goto IL_11E0;
								}
								float num9 = this.CR.Nm(1, vector.x - IN.wh);
								float num10 = this.CR.Nm(2, vector.y - IN.hh);
								rect.Set(num9, num10, this.CR.Nm(3, IN.wh) - num9, this.CR.Nm(4, IN.hh) - num10);
								if (flag8)
								{
									continue;
								}
								flag8 = true;
								if (fcut != null)
								{
									fcut.ClipRect = rect;
									continue;
								}
								continue;
							}
						}
						else if (!(cmd == "%SPINE"))
						{
							goto IL_11E0;
						}
					}
					else if (num3 <= 3687055305U)
					{
						if (num3 != 3474896040U)
						{
							if (num3 != 3521370842U)
							{
								if (num3 != 3687055305U)
								{
									goto IL_11E0;
								}
								if (!(cmd == "%SPINE_SIZE"))
								{
									goto IL_11E0;
								}
								SpvLoader spvLoader = this.OLoadSpine[this.CR._1];
								spvLoader.width = this.CR.Nm(2, spvLoader.width);
								spvLoader.height = this.CR.Nm(3, spvLoader.height);
								continue;
							}
							else
							{
								if (!(cmd == "IMMEDIATE"))
								{
									goto IL_11E0;
								}
								if (fcut != null)
								{
									fcut.immediate_scrolling = true;
									continue;
								}
								continue;
							}
						}
						else
						{
							if (!(cmd == "%SND_LOAD"))
							{
								goto IL_11E0;
							}
							for (int n = 1; n < this.CR.clength; n++)
							{
								SND.loadSheets(this.CR._1, "FATAL");
							}
							continue;
						}
					}
					else if (num3 <= 3688922219U)
					{
						if (num3 != 3687864698U)
						{
							if (num3 != 3688922219U)
							{
								goto IL_11E0;
							}
							if (!(cmd == "LOOP"))
							{
								goto IL_11E0;
							}
							if (!flag5)
							{
								flag5 = true;
								array4 = new int[array.Length];
								if (fcut != null)
								{
									fcut.Aloop_to = array4;
								}
							}
							if (this.CR.clength <= 2)
							{
								for (int num11 = 0; num11 < array4.Length; num11++)
								{
									array4[num11] = this.CR.Int(1, 0);
								}
								continue;
							}
							int num12 = ((this.CR._1 == "_") ? 0 : X.isinStr(array, this.CR._1, -1));
							if (num12 >= 0)
							{
								array4[num12] = this.CR.Int(2, 0);
								continue;
							}
							continue;
						}
						else
						{
							if (!(cmd == "%VOICE"))
							{
								goto IL_11E0;
							}
							for (int num13 = 1; num13 < this.CR.clength; num13++)
							{
								this.OVoiceSrc[this.CR.getIndex(num13)] = NEL.prepareVoiceController(this.CR.getIndex(num13));
							}
							continue;
						}
					}
					else if (num3 != 3764867278U)
					{
						if (num3 != 4144876992U)
						{
							goto IL_11E0;
						}
						if (!(cmd == "SND"))
						{
							goto IL_11E0;
						}
						goto IL_0C99;
					}
					else
					{
						if (!(cmd == "MSG_HEADER"))
						{
							goto IL_11E0;
						}
						this.msg_header = this.CR._1;
						continue;
					}
					array = this.CR.slice(1, -1000);
					for (int num14 = array.Length - 1; num14 >= 0; num14--)
					{
						string text3 = array[num14];
						string text4 = null;
						int num15 = text3.IndexOf('@');
						if (num15 >= 0)
						{
							text4 = TX.slice(text3, num15 + 1);
							text3 = (array[num14] = TX.slice(text3, 0, num15));
						}
						if (!this.OLoadSpine.ContainsKey(text3))
						{
							this.OLoadSpine[text3] = new SpvLoader((this.CR.cmd == "%SPINE_SV") ? "SpineAnim/" : null, text3, text4);
						}
					}
					this.max_spine_count = X.Mx(this.max_spine_count, array.Length);
					array3 = null;
					text2 = array[0];
					flag4 = false;
					array5 = null;
					array4 = null;
					flag7 = (flag5 = (flag4 = false));
					continue;
					IL_0C99:
					if (this.CR.clength == 1)
					{
						bdic = null;
						if (fcut != null)
						{
							fcut.OASndInfo = null;
						}
						flag6 = false;
						continue;
					}
					if (this.CR.cmd == "VO" && !this.OVoiceSrc.ContainsKey(this.CR._1))
					{
						this.CR.tError("不明なボイス発声者: " + this.CR._1 + " (あらかじめ%VOICEで初期化すること) ");
						continue;
					}
					if (!flag6)
					{
						bdic = VoiceInfo.copyDictionary(bdic);
						flag6 = true;
						if (fcut != null)
						{
							fcut.OASndInfo = bdic;
						}
					}
					if (TX.valid(this.CR._2))
					{
						string text5 = ((this.CR.cmd == "VO") ? (this.CR._1 + "_" + this.CR._2) : this.CR._1);
						List<VoiceInfo> list4 = null;
						if (!bdic.TryGetValue(text5, out list4))
						{
							list4 = (bdic[text5] = new List<VoiceInfo>(1));
						}
						VoiceInfo voiceInfo = new VoiceInfo(this.CR._2, this.CR.Nm(3, -1f), this.CR.Nm(4, -1f));
						voiceInfo.t = (voiceInfo.startt = this.CR.Nm(5, 0f));
						list4.Add(voiceInfo);
						if (this.CR.cmd == "VO")
						{
							voiceInfo.VC = this.OVoiceSrc[this.CR._1];
							continue;
						}
						continue;
					}
					else
					{
						bdic.Remove(this.CR._1);
						if (!(this.CR.cmd == "VO"))
						{
							continue;
						}
						VoiceController voiceController = this.OVoiceSrc[this.CR._1];
						using (BList<string> blist = ListBuffer<string>.Pop(bdic.Count))
						{
							foreach (KeyValuePair<string, List<VoiceInfo>> keyValuePair in bdic)
							{
								for (int num16 = keyValuePair.Value.Count - 1; num16 >= 0; num16--)
								{
									if (keyValuePair.Value[num16].VC == voiceController)
									{
										keyValuePair.Value.RemoveAt(num16);
									}
								}
								if (keyValuePair.Value.Count == 0)
								{
									blist.Add(keyValuePair.Key);
								}
							}
							for (int num17 = blist.Count - 1; num17 >= 0; num17--)
							{
								bdic.Remove(blist[num17]);
							}
							continue;
						}
					}
					IL_0F67:
					if (!(this.CR._1 == "#0"))
					{
						continue;
					}
					vector.Set(this.CR.Nm(2, 0f), this.CR.Nm(3, 0f), this.CR.Nm(4, vector.z));
					if (fcut != null)
					{
						fcut.BasePos = vector;
						continue;
					}
					continue;
				}
				IL_11E0:
				if (REG.match(this.CR.cmd, FatalShower.RegImportOtherFile))
				{
					string resource = TX.getResource("Fatal/" + REG.R1 + ".fatal", ".csv", false);
					int cur_line = this.CR.get_cur_line();
					if (!TX.valid(resource))
					{
						continue;
					}
					for (int num18 = 1; num18 < this.CR.clength; num18++)
					{
						this.CR.VarCon.define(num18.ToString(), this.CR.getIndex(num18), true);
					}
					int num19 = this.CR.countCharacters(cur_line + 1, -1);
					using (STB stb = TX.PopBld(null, num19))
					{
						using (STB stb2 = TX.PopBld(null, resource.Length + this.CR.countCharacters(0, cur_line) + num19 + 4))
						{
							this.CR.copyDataTo(stb2, 0, cur_line);
							this.CR.copyDataTo(stb, cur_line + 1, -1);
							this.CR.parseText(stb2.Add("\n", resource, "\n").Add(stb));
							this.CR.seek_set(cur_line);
							continue;
						}
					}
				}
				if (REG.match(this.CR.cmd, FatalShower.RegPreCmd))
				{
					int length = REG.R1.Length;
					flag9 = true;
					if (TX.valid(REG.R2))
					{
						for (int num20 = list.Count - 1; num20 >= 0; num20--)
						{
							if (list2[num20] <= length)
							{
								list2.RemoveAt(num20);
								list.RemoveAt(num20);
							}
						}
					}
					else
					{
						list.Add(this.CR.slice_join(1, " ", ""));
						list2.Add(length);
					}
				}
				else if (REG.match(this.CR.getLastStr(), FatalShower.RegCutPrefix))
				{
					if (array == null)
					{
						this.CR.tError("Spine が初期化されていない");
					}
					else
					{
						if (flag9)
						{
							flag9 = false;
							csvReaderA = new CsvReaderA(null, this.CR.VarCon);
							csvReaderA.parseText(list.ToArray());
						}
						string msgLabelHeader = this.getMsgLabelHeader(ref text, this.CR.cmd);
						text = msgLabelHeader;
						string text6 = this.CR._1;
						bool flag10 = false;
						if (TX.isStart(text6, '!'))
						{
							flag10 = true;
							text6 = TX.slice(text6, 1);
						}
						bool flag11 = this.ACutPoint.Count == 0 || !REG.match(text6, FatalShower.RegNotDot);
						if (flag11)
						{
							this.dot_count++;
						}
						List<FatalShower.FCut> acutPoint = this.ACutPoint;
						FatalShower.FCut fcut2 = new FatalShower.FCut(null);
						fcut2.msg_label = msgLabelHeader + " " + text6;
						fcut2.line = this.CR.get_cur_line();
						fcut2.Ause_spine = array;
						fcut2.show_dot = flag11;
						fcut2.AAskin = array2;
						fcut2.AAanim = array3;
						fcut2.spine_stencil = num;
						fcut2.BasePos = vector;
						fcut2.Bg = bgInfo;
						fcut2.force_swap = flag10;
						fcut2.base_spine = text2;
						fcut2.spine_ag360 = num2;
						fcut2.dot_index = this.dot_count - 1;
						fcut2.CRPre = csvReaderA;
						fcut2.ClipRect = rect;
						fcut2.OASndInfo = bdic;
						fcut2.Aloop_to = array4;
						fcut2.Aspeed = array5;
						fcut = fcut2;
						acutPoint.Add(fcut2);
						flag2 = (flag = (flag3 = (flag8 = false)));
						flag4 = false;
						flag6 = (flag5 = (flag7 = false));
					}
				}
			}
			this.ACutPoint.Add(new FatalShower.FCut(this.ACutPoint[this.ACutPoint.Count - 1]).Simple());
			this.CR.seek_set(0);
			return this;
		}

		private string getMsgLabelHeader(ref string pre_msg_key, string cmd)
		{
			if (cmd == "*")
			{
				return pre_msg_key;
			}
			if (!TX.isStart(cmd, "*/", 0))
			{
				return TX.slice(cmd, 1);
			}
			return this.msg_header + TX.slice(cmd, 1);
		}

		public MImage getBgTexture(string name, bool no_error = false)
		{
			MTIOneImage mtioneImage;
			if (!this.OLoadedTexture.TryGetValue(name, out mtioneImage))
			{
				mtioneImage = new MTIOneImage("Fatal/_bg__" + name, null, name);
				if (!mtioneImage.file_valid)
				{
					if (!no_error)
					{
						X.de("不明なテクスチャ MTI: Fatal/_bg__" + name, null);
					}
					return null;
				}
				this.OLoadedTexture[name] = mtioneImage;
				mtioneImage.addLoadKey("_", false);
			}
			if (mtioneImage == null)
			{
				return null;
			}
			return mtioneImage.MI;
		}

		public bool isMaterialLoaded()
		{
			return !this.evt_load_wait || !EV.isLoading();
		}

		public void initGameObject(GameObject Base, int layer = -1, float shiftz = 0f)
		{
			this.already_seen = MGV.isFatalSceneAlreadyWatched(this.script_key);
			MGV.fatalSceneWatch(this.script_key);
			this.HovCurs = new HoverCursManager("EDT_FOCUS", "select");
			this.ui_lang = (byte)TX.getCurrentFamilyIndex();
			IN.addRunner(this);
			IN.FlgUiUse.Add("Fatal");
			this.GobBase = IN.CreateGob(Base, " fatal " + this.script_key);
			if (layer >= 0)
			{
				this.GobBase.layer = layer;
			}
			IN.setZ(this.GobBase.transform, shiftz + this.GobBase.transform.localPosition.z);
			this.con_base_z = this.GobBase.transform.position.z;
			this.WhLis.resetShowingArea(0f, 0f, 1f, IN.w, IN.h);
			this.WhLis.keyboard_translate = 0.3125f;
			this.WhLis.moveable_margin_x = (this.WhLis.moveable_margin_y = 0f);
			this.WhLis.seeable_margin_x = (this.WhLis.seeable_margin_y = 40f);
			this.WhLis.calc_super_min_scale = true;
			this.WhLis.grab_enabled = true;
			this.WhLis.addChangedFn(new FnWheelBindings(this.fnWheelScale));
			this.WhLis.addChangedFinishedFn(new FnWheelBindings(this.fnGrabFinished));
			this.WhLis.hide();
			this.AFadeCam = new Camera[2];
			this.ACamBindBuffer = new CameraBidingsBehaviour[2];
			this.AMrdFadeScreen = new MeshRenderer[2];
			this.AMdFadeScreen = new MeshDrawer[2];
			this.AMdBg = new MeshDrawer[2];
			this.AMrdBg = new MeshRenderer[2];
			for (int i = 0; i < 2; i++)
			{
				Camera camera = (this.AFadeCam[i] = IN.CreateGob(this.GobBase, "-Cam " + i.ToString()).AddComponent<Camera>());
				this.ACamBindBuffer[i] = IN.GetOrAdd<CameraBidingsBehaviour>(camera.gameObject);
				Camera guicamera = IN.getGUICamera();
				int num = ((i == 0) ? this.buf_layer0 : this.buf_layer1);
				camera.transform.position = guicamera.transform.position;
				camera.transform.localRotation = guicamera.transform.localRotation;
				camera.cullingMask = 1 << num;
				camera.clearFlags = CameraClearFlags.Color;
				camera.backgroundColor = C32.d2c(0U);
				camera.enabled = false;
				camera.gameObject.SetActive(false);
				GameObject gameObject = IN.CreateGob(this.GobObjectBase, "-Bg" + i.ToString());
				IN.setZ(gameObject.transform, 500f);
				gameObject.layer = num;
				MeshDrawer meshDrawer = (this.AMdBg[i] = MeshDrawer.prepareMeshRenderer(gameObject, MTRX.newMtr(MTRX.ShaderGDT), 0f, -1, null, false, false));
				this.AMrdBg[i] = gameObject.GetComponent<MeshRenderer>();
				meshDrawer.getMaterial().name = "fade_bg";
				meshDrawer.setMaterialCloneFlag();
				gameObject.SetActive(false);
				Material material = MTRX.newMtr(MTRX.ShaderGDT);
				GameObject gameObject2 = IN.CreateGob(this.GobBase, "-FadeScreen" + i.ToString());
				MeshDrawer meshDrawer2 = (this.AMdFadeScreen[i] = MeshDrawer.prepareMeshRenderer(gameObject2, material, 0f, -1, null, false, false));
				meshDrawer2.setMaterialCloneFlag();
				material.name = "rendered";
				MeshRenderer meshRenderer = (this.AMrdFadeScreen[i] = gameObject2.GetComponent<MeshRenderer>());
				gameObject2.SetActive(false);
				meshDrawer2.InitSubMeshContainer(0);
				meshDrawer2.connectRendererToTriMulti(meshRenderer);
				meshDrawer2.base_z = -0.05f;
				this.fineScreenBuffer(i);
			}
			this.FlgHideUi = new Flagger(new FlaggerT<string>.FnFlaggerCall(this.FnHideUiExecute), new FlaggerT<string>.FnFlaggerCall(this.FnShowUiExecute));
			this.FlgHideUi.Add("NOMSG");
			this.GobObjectBase = IN.CreateGob(this.GobBase, "-objects");
			this.GobUi = IN.CreateGob(this.GobBase, "-UI");
			NelMSG.prepareBookDrawer("FATAL", this.GobUi, out this.Book, out this.GobBook, false);
			this.BxCon = this.GobUi.AddComponent<UiBoxDesignerFamily>();
			this.BxCon.auto_deactive_gameobject = false;
			this.Book.MAXT_PAGE = 32f;
			this.Tx = IN.CreateGob(this.GobUi, "Tx").AddComponent<NelEvTextRenderer>();
			this.Tx.initNelMsg(this);
			this.Tx.DELAY_FIRST = 0;
			this.Tx.aligny = ALIGNY.BOTTOM;
			this.Tx.BorderCol(2281701376U).Col(uint.MaxValue).Size(18f);
			this.GobTxBack = IN.CreateGob(this.GobUi, "-TxBack");
			this.MdTxBack = MeshDrawer.prepareMeshRenderer(this.GobTxBack, MTRX.MIicon.getMtr(BLEND.NORMAL, -1), 0f, -1, null, false, false);
			IN.setZ(this.GobTxBack.transform, -3.8f);
			this.MaTxBack = new MdArranger(this.MdTxBack);
			this.GobTxScroll = IN.CreateGob(this.GobUi, "-Scroll");
			this.MdScroll = MeshDrawer.prepareMeshRenderer(this.GobTxScroll, MTRX.MIicon.getMtr(BLEND.NORMAL, -1), 0f, -1, null, false, false);
			IN.setZ(this.GobTxScroll.transform, -4.12f);
			this.PFarrow = MTRX.getPF("menu_arrow");
			this.BookReposit();
			this.BxCon.base_z = -4.5f;
			this.BxR = this.BxCon.Create("right", 0f, 0f, 600f, IN.h - 240f, 0, 0f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxR.gameObject.layer = IN.gui_layer;
			this.BxR.anim_time(25);
			this.BxR.Focusable(false, false, null);
			this.BxR.use_button_connection = true;
			this.BxR.box_stencil_ref_mask = 225;
			this.BxR.deactivate(true);
			this.BxDesc = this.BxCon.Create("desc", 0f, 0f, 200f, 200f, 0, 0f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxDesc.gameObject.layer = IN.gui_layer;
			this.BxDesc.anim_time(18);
			this.BxDesc.deactivate(true);
			this.BxCon.base_z = -5f;
			this.BxMenu = this.BxCon.Create("menu", -FatalShower.scwh + 220f, 0f, 440f, 340f, -1, 0f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxMenu.gameObject.layer = IN.gui_layer;
			this.createMenuInner();
			this.MenuEar = aBtnBoxEar.Create(this.BxMenu, 60f, AIM.B, -0.5f);
			this.MenuEar.FnActvChanged = new Func<aBtnBoxEar, bool, bool>(this.fnActvChangedMenu);
			this.MenuEar.setPF("pict_menu");
			if (this.already_seen)
			{
				BtnContainerRunner btnContainerRunner = this.BxMenu.Get("optbtns", false) as BtnContainerRunner;
				this.PreSelected = btnContainerRunner.Get(1);
			}
			this.AAGob = new GameObject[2][];
			this.AASpv = new SpineViewer[2][];
			this.AALay = new List<FtLayer>[2];
			this.OAAMaterial = new BDic<int, Material[][]>();
			this.AAMosShower = new List<FtMosaic>[2];
			this.AApre_spine_loaded = new string[2][];
			this.Apre_spine_stencil = new int[] { -1, -1 };
			this.DefMtr = new Material(MTRX.MtrSpineDefault);
			this.DefMtr.SetFloat("_Cull", 0f);
			for (int j = 0; j < 2; j++)
			{
				this.AALay[j] = new List<FtLayer>(1);
				this.AALay[j].Add(new FtLayer(this, ":" + j.ToString() + "_0", j));
				this.AAGob[j] = new GameObject[this.max_spine_count];
				this.AASpv[j] = new SpineViewer[this.max_spine_count];
				this.AAMosShower[j] = new List<FtMosaic>(3);
				for (int k = this.max_spine_count - 1; k >= 0; k--)
				{
					GameObject gameObject3 = (this.AAGob[j][k] = IN.CreateGob(this.GobObjectBase, " -SpvG" + j.ToString() + " : " + k.ToString()));
					(this.AASpv[j][k] = new SpineViewer(null)).initGameObject(gameObject3);
					ValotileRendererFilter valotileRendererFilter = gameObject3.AddComponent<ValotileRendererFilter>();
					valotileRendererFilter.Init(gameObject3.GetComponent<MeshFilter>(), gameObject3.GetComponent<MeshRenderer>(), true);
					valotileRendererFilter.Init(this.ACamBindBuffer[j]);
					gameObject3.SetActive(false);
				}
			}
			this.prepareMaterialForStencil(-1);
			foreach (KeyValuePair<string, SpvLoader> keyValuePair in this.OLoadSpine)
			{
				keyValuePair.Value.initTexture(keyValuePair.Key);
			}
			if (BGM.load(this.bgm_key, null, false))
			{
				BGM.replace(10f, 100f, false, false);
				this.bgm_replaced = true;
			}
			this.changeCut(0, true, false);
			this.fineScreenSize();
		}

		private Material[][] prepareMaterialForStencil(int stencil)
		{
			Material[][] array;
			if (!this.OAAMaterial.TryGetValue(stencil, out array))
			{
				array = (this.OAAMaterial[stencil] = new Material[2][]);
				for (int i = 0; i < 2; i++)
				{
					array[i] = new Material[this.max_spine_count];
					for (int j = this.max_spine_count - 1; j >= 0; j--)
					{
						Material material = (array[i][j] = MTRX.newMtr(this.DefMtr));
						material.name = string.Concat(new string[]
						{
							this.DefMtr.name,
							" :",
							i.ToString(),
							".",
							j.ToString()
						});
						if (stencil >= 0)
						{
							material.SetFloat("_StencilRef", (float)stencil);
							material.SetFloat("_StencilComp", 3f);
						}
					}
				}
			}
			return array;
		}

		internal void initValotBinding(ValotileRenderer Valot, int buffer)
		{
			Valot.Init(this.ACamBindBuffer[buffer]);
		}

		public void destruct()
		{
			this.CR.destruct();
			CURS.Rem("DRAG", "");
			CURS.Active.Rem("Fatal");
			IN.FD_WindowSizeChanged = (IN.FnWindowSizeChanged)Delegate.Remove(IN.FD_WindowSizeChanged, this.FD_WindowSizeChanged);
			IN.FlgUiUse.Rem("Fatal");
			if (this.AFadeCam != null)
			{
				if (this.cur_cut >= 0)
				{
					FatalShower.FCut fcut = this.ACutPoint[this.cur_cut];
					if (fcut != null)
					{
						VoiceInfo.stopAll(fcut.OASndInfo, false);
					}
				}
				for (int i = 0; i < 2; i++)
				{
					BLIT.nDispose(this.AFadeCam[i].targetTexture);
					this.AFadeCam[i] = null;
					this.AMdFadeScreen[i].destruct();
					this.AMdBg[i].destruct();
					IN.DestroyOne(this.AMrdFadeScreen[i].gameObject);
				}
				this.Book.destruct();
				for (int j = 0; j < 2; j++)
				{
					List<FtMosaic> list = this.AAMosShower[j];
					int num = list.Count;
					for (int k = 0; k < num; k++)
					{
						list[k].destruct();
					}
					num = this.AALay[j].Count;
					for (int l = 0; l < num; l++)
					{
						this.AALay[j][l].destruct();
					}
				}
				foreach (KeyValuePair<int, Material[][]> keyValuePair in this.OAAMaterial)
				{
					Material[][] value = keyValuePair.Value;
					for (int m = 0; m < 2; m++)
					{
						int n = 0;
						int num2 = value[m].Length;
						while (n < num2)
						{
							IN.DestroyOne(value[m][n]);
							n++;
						}
					}
				}
				this.AFadeCam = null;
			}
			if (this.DefMtr != null)
			{
				IN.DestroyOne(this.DefMtr);
			}
			SND.unloadSheets(null, "FATAL");
			if (this.MdTxBack != null)
			{
				this.MdTxBack.destruct();
			}
			if (this.MdScroll != null)
			{
				this.MdScroll.destruct();
			}
			if (this.MtrTransitionBite != null)
			{
				IN.DestroyOne(this.MtrTransitionBite);
			}
			this.WhLis.hide();
			if (this.bgm_replaced && !this.from_memory)
			{
				BGM.replace(0f, 100f, true, false);
			}
			if (this.GobBase != null)
			{
				IN.DestroyOne(this.GobBase);
			}
			foreach (KeyValuePair<string, VoiceController> keyValuePair2 in this.OVoiceSrc)
			{
				keyValuePair2.Value.destruct();
			}
			IN.setScreenOrtho(false);
			SND.unloadSheets("fatal", this.script_key);
			this.destructResources();
			IN.remRunner(this);
			this.M2D = M2DBase.Instance as NelM2DBase;
			if (this.M2D != null)
			{
				UIBase.FlgLetterBoxFade.Rem("FATAL");
				this.M2D.FlgHideWholeScreen.Rem("FATAL");
			}
		}

		private void destructResources()
		{
			if (this.OLoadedTexture != null)
			{
				foreach (KeyValuePair<string, MTIOneImage> keyValuePair in this.OLoadedTexture)
				{
					keyValuePair.Value.Dispose();
				}
			}
			if (this.OLoadSpine != null)
			{
				foreach (KeyValuePair<string, SpvLoader> keyValuePair2 in this.OLoadSpine)
				{
					keyValuePair2.Value.destruct();
				}
			}
		}

		public bool run(float fcnt)
		{
			FatalShower.FCut fcut = this.ACutPoint[this.cur_cut];
			if (this.EditCfg != null)
			{
				if (!this.EditCfg.runBoxDesignerEdit())
				{
					if (this.SubmitCfg == null || aBtn.PreSelected == this.SubmitCfg)
					{
						this.SubmitCfg.ExecuteOnSubmitKey();
					}
					else
					{
						this.SubmitCfg.Select(true);
					}
				}
			}
			else if (this.t_quit < 0f)
			{
				if (IN.isUiRemPD())
				{
					if (this.menu_shown)
					{
						this.BCheckHideUI.ExecuteOnClick();
					}
					else
					{
						this.fnHideUiChanging(null);
						SND.Ui.play("talk_progress", false);
					}
				}
				else if (IN.isUiAddPD())
				{
					if (this.menu_shown)
					{
						this.BCheckAutoMode.ExecuteOnClick();
					}
					else
					{
						this.setAutoMode(!this.auto_mode);
						SND.Ui.play("talk_progress", false);
					}
				}
				else if (IN.isUiSortPD())
				{
					if (this.menu_shown)
					{
						this.BCheckFreeCam.ExecuteOnClick();
					}
					else
					{
						this.fnFreeCamChanging(null);
						SND.Ui.play("talk_progress", false);
					}
				}
				bool flag = false;
				bool flag2 = this.t_click_lock > 0f;
				if (this.WhLis.isGrabing())
				{
					flag2 = true;
					this.t_click_lock = 5f;
				}
				if (!this.menu_shown)
				{
					bool flag3 = this.freecam;
				}
				if (this.allow_auto_progress)
				{
					if (flag || (!IN.isMenuO(0) && (IN.isSubmitPD(1) || (!flag2 && (this.t_click_lock < 0f || IN.isMouseUp(15))) || IN.isCancel())) || (!this.freecam && IN.isR()) || this.t_autonext >= 60f)
					{
						if (!this.all_char_shown)
						{
							this.Tx.showImmediate(false, false);
						}
						else if (this.Tx.progressNextParagraph(false, null))
						{
							this.Tx.forceProgressNextStack();
							this.TxReposit();
							this.Book.progressPage(true);
						}
						else if (this.cur_cut < this.ACutPoint.Count - 1)
						{
							this.changeCut(this.cur_cut + 1, this.ACutPoint[this.cur_cut + 1].immediate_scrolling, true);
						}
					}
					else if (IN.isL() && !this.freecam && this.cur_cut > 0)
					{
						this.changeCut(this.cur_cut - 1, false, false);
					}
				}
				else if (this.MenuEar.box_shown)
				{
					if (IN.isCancel())
					{
						this.MenuEar.deactivate();
						IN.clearCancelPushDown(false);
					}
				}
				else if (this.FlgHideUi.hasKey("B") && IN.isSubmitOrMouseUp(15))
				{
					this.fnHideUiChanging(null);
					SND.Ui.play("talk_progress", false);
				}
			}
			else
			{
				this.t_quit += fcnt;
				if (this.t_quit >= 90f)
				{
					return false;
				}
			}
			if (this.t_click_lock != 0f)
			{
				this.t_click_lock = X.VALWALK(this.t_click_lock, 0f, fcnt);
			}
			fcut = this.ACutPoint[this.cur_cut];
			if (this.t_buffer <= 30f && X.D)
			{
				bool flag4 = this.t_buffer == 0f;
				this.t_buffer += (float)X.AF;
				List<FtLayer> list = this.AALay[1 - this.cur_buffer];
				GameObject gameObject = this.AMrdFadeScreen[1 - this.cur_buffer].gameObject;
				GameObject gameObject2 = this.AMrdBg[1 - this.cur_buffer].gameObject;
				Camera camera = this.AFadeCam[1 - this.cur_buffer];
				if (this.t_buffer >= 30f)
				{
					this.t_buffer = 40f;
					for (int i = list.Count - 1; i >= 0; i--)
					{
						FtLayer ftLayer = list[i];
						if (ftLayer != null)
						{
							ftLayer.enabled = false;
						}
					}
					List<FtMosaic> list2 = this.AAMosShower[1 - this.cur_buffer];
					for (int j = list2.Count - 1; j >= 0; j--)
					{
						list2[j].enabled = false;
					}
					gameObject.SetActive(false);
					gameObject2.SetActive(false);
					camera.gameObject.SetActive(false);
				}
				else
				{
					if (!gameObject.activeSelf)
					{
						gameObject.SetActive(true);
						gameObject2.SetActive(true);
					}
					MeshDrawer meshDrawer = this.AMdFadeScreen[1 - this.cur_buffer];
					meshDrawer.clear(false, false);
					float num = this.fadeTransitionPrepare(meshDrawer, flag4, X.ZLINE(this.t_buffer, 30f));
					List<FtMosaic> list3 = this.AAMosShower[1 - this.cur_buffer];
					for (int k = list3.Count - 1; k >= 0; k--)
					{
						list3[k].drawToMesh();
					}
					camera.Render();
					meshDrawer.Col = C32.MulA(uint.MaxValue, num);
					this.drawBufferScreen(meshDrawer, 1 - this.cur_buffer);
					meshDrawer.updateForMeshRenderer(false);
				}
			}
			for (int l = 0; l < 2; l++)
			{
				if (this.cur_buffer == l || this.t_buffer < 30f)
				{
					List<FtLayer> list = this.AALay[l];
					SpineViewer[] array = this.AASpv[l];
					for (int m = ((this.AApre_spine_loaded[l] != null) ? this.AApre_spine_loaded[l].Length : 0) - 1; m >= 0; m--)
					{
						array[m].updateAnim(X.D, 1f);
					}
					if (X.D)
					{
						for (int n = list.Count - 1; n >= 0; n--)
						{
							FtLayer ftLayer2 = list[n];
							if (ftLayer2 != null)
							{
								ftLayer2.run(X.AF, this.freecam);
							}
						}
					}
					if (l == this.cur_buffer)
					{
						List<FtMosaic> list4 = this.AAMosShower[l];
						for (int num2 = list4.Count - 1; num2 >= 0; num2--)
						{
							list4[num2].drawToMesh();
						}
					}
				}
			}
			if (this.t_text >= 0f)
			{
				this.Tx.run(fcnt, false);
			}
			if (X.D)
			{
				if (this.AALay[this.cur_buffer][0].animating && !this.bg_scroll_fixed)
				{
					this.need_redraw_bg = (this.need_redraw_main_buffer = true);
				}
				if (this.all_char_shown)
				{
					bool flag5 = false;
					if (this.t_arrow <= 0)
					{
						this.t_arrow = 40;
						flag5 = true;
					}
					bool flag6 = this.t_arrow >= 20;
					this.t_arrow -= X.AF;
					if (flag6 && this.t_arrow < 20)
					{
						flag5 = true;
					}
					if (flag5 || (this.auto_mode && (this.allow_auto_progress || this.t_autonext >= 0f)))
					{
						this.MdScroll.RotaPF(0f, (float)(10 - ((this.t_arrow < 20) ? 4 : 0)), 1f, 1f, 0f, this.PFarrow, false, false, false, uint.MaxValue, false, 0);
						if (this.auto_mode)
						{
							this.MdScroll.initForImg(MTRX.IconWhite, 0);
							this.MdScroll.RectBL(-12f, -2f, (float)X.IntC(24f * (60f - X.Abs(this.t_autonext)) / 60f), 2f, false);
							if (this.allow_auto_progress)
							{
								this.t_autonext = X.Abs(this.t_autonext) + (float)X.AF * 2f / X.Mx(1f, (float)CFG.fatal_automode_speed);
							}
							else if (this.t_autonext >= 0f)
							{
								this.t_autonext = -X.Mx(0.001f, this.t_autonext);
							}
						}
						this.MdScroll.updateForMeshRenderer(false);
					}
				}
				this.redrawBg(false);
				if (this.t_text >= 0f)
				{
					this.Tx.run((float)X.AF, false);
					if (this.t_text <= 30f)
					{
						this.t_text += (float)X.AF;
						this.text_alpha = X.ZLINE(this.t_text, 30f);
					}
				}
				if (this.t_book >= 0f)
				{
					this.Book.run((float)X.AF);
					if (this.t_book < 22f)
					{
						this.t_book += (float)X.AF;
						this.BookReposit();
					}
				}
				else if (this.t_book > -22f)
				{
					this.Book.run((float)X.AF);
					this.t_book -= (float)X.AF;
					this.BookReposit();
				}
				if (this.need_redraw_main_buffer)
				{
					this.need_redraw_main_buffer = false;
					MeshDrawer meshDrawer2 = this.AMdFadeScreen[this.cur_buffer];
					meshDrawer2.Col = MTRX.ColWhite;
					this.drawBufferScreen(meshDrawer2, this.cur_buffer);
					meshDrawer2.updateForMeshRenderer(false);
				}
				this.AFadeCam[this.cur_buffer].Render();
			}
			if (fcut.OASndInfo != null)
			{
				fcut.runSnd(fcnt);
			}
			return true;
		}

		public void drawBufferScreen(MeshDrawer Md, int buffer)
		{
			Md.chooseSubMesh(0, false, true);
			Md.base_x = (Md.base_y = 0f);
			Md.initForImg(this.AFadeCam[buffer].targetTexture);
			int num = Screen.width / 2;
			int num2 = Screen.height / 2;
			Md.RectBL((float)(-(float)num), (float)(-(float)num2), (float)Screen.width, (float)Screen.height, false);
		}

		private void fineScreenBuffer(int buffer)
		{
			MeshDrawer meshDrawer = this.AMdFadeScreen[buffer];
			MeshRenderer meshRenderer = this.AMrdFadeScreen[buffer];
			Camera camera = this.AFadeCam[buffer];
			RenderTexture targetTexture = camera.targetTexture;
			global::UnityEngine.Object @object = targetTexture;
			BLIT.Alloc(ref targetTexture, Screen.width, Screen.height, true, RenderTextureFormat.ARGB32, 24);
			if (@object != targetTexture)
			{
				camera.targetTexture = targetTexture;
				camera.projectionMatrix = Matrix4x4.Ortho(-FatalShower.scvwh * 0.015625f, FatalShower.scvwh * 0.015625f, -FatalShower.scvhh * 0.015625f, FatalShower.scvhh * 0.015625f, camera.nearClipPlane, camera.farClipPlane);
				this.ACamBindBuffer[buffer].need_fine_ortho = true;
				meshDrawer.chooseSubMesh(0, false, false);
				meshDrawer.initForImgAndTexture(targetTexture);
				meshDrawer.connectRendererToTriMulti(meshRenderer);
				this.need_redraw_main_buffer = true;
			}
		}

		public void changeCut(int index, bool immediate = false, bool connect_animation = false)
		{
			if (this.EditCfg != null)
			{
				return;
			}
			int num = this.cur_cut;
			FatalShower.FCut fcut = ((this.cur_cut >= 0) ? this.ACutPoint[this.cur_cut] : null);
			FatalShower.BgInfo bgInfo = ((fcut != null) ? fcut.Bg : new FatalShower.BgInfo(null));
			BDic<string, List<VoiceInfo>> bdic = ((fcut != null) ? fcut.OASndInfo : null);
			string[] array = this.AApre_spine_loaded[this.cur_buffer];
			this.PreBg = bgInfo;
			X.MPF(this.cur_cut < index);
			this.cur_cut = index;
			FatalShower.FCut fcut2 = this.ACutPoint[this.cur_cut];
			bool flag = true;
			if (fcut != null && !fcut2.force_swap)
			{
				flag = (!this.freecam && fcut.BasePos != fcut2.BasePos) || !fcut2.isSame(fcut);
			}
			if (flag)
			{
				this.swapBuffer(immediate, true);
			}
			else
			{
				IN.setZ(this.AMrdFadeScreen[this.cur_buffer].transform, 0.1f);
			}
			this.fineScreenBuffer(this.cur_buffer);
			MeshDrawer meshDrawer = this.AMdBg[this.cur_buffer];
			Component component = this.AMrdBg[this.cur_buffer];
			Component component2 = this.AMrdFadeScreen[this.cur_buffer];
			if (fcut2.Bg.Tx != meshDrawer.getMaterial().mainTexture)
			{
				meshDrawer.initForImgAndTexture(fcut2.Bg.Tx);
			}
			component2.gameObject.SetActive(true);
			component.gameObject.SetActive(true);
			this.need_redraw_bg = (this.need_redraw_main_buffer = true);
			List<FtLayer> list = this.AALay[this.cur_buffer];
			FtLayer ftLayer = list[0];
			this.prepareSpineBaseLayer(!flag);
			if (array != null && flag && connect_animation)
			{
				for (int i = array.Length - 1; i >= 0; i--)
				{
					string text = array[i];
					int num2 = X.isinStr(fcut2.Ause_spine, text, -1);
					if (num2 >= 0)
					{
						SpineViewer spineViewer = this.AASpv[1 - this.cur_buffer][i];
						if (spineViewer.gameObject.GetComponent<MeshRenderer>().enabled)
						{
							SpineViewer spineViewer2 = this.AASpv[this.cur_buffer][num2];
							if (spineViewer2.getBaseAnimName() == spineViewer.getBaseAnimName())
							{
								spineViewer2.setTimePositionAll(this.AASpv[1 - this.cur_buffer][i]);
							}
						}
					}
				}
			}
			this.BaseMem.Set(0f, 0f, -1000f);
			this.fineCutText(num);
			if (fcut2.OASndInfo != bdic)
			{
				if (bdic != null)
				{
					VoiceInfo.stopAll(bdic, true);
				}
				fcut2.clearVoice(true);
			}
			bool flag2 = this.freecam && fcut != null && !fcut2.force_swap && fcut2.canInheritBasePos(fcut);
			this.CutReloadReading(!flag2 || flag, !flag);
			this.initClipArea(true);
			this.t_click_lock = 4f;
			if (this.freecam)
			{
				this.effectLayerFade(true);
			}
			if (flag)
			{
				if (flag2)
				{
					ftLayer.RepositBase(this.AALay[1 - this.cur_buffer][0]);
				}
				int count = list.Count;
				for (int j = 1; j < count; j++)
				{
					FtLayer ftLayer2 = list[j];
					FtLayer layer = this.GetLayer(1 - this.cur_buffer, ftLayer2.id, false);
					ftLayer2.sinkTimeIfSame(layer);
				}
			}
			this.MdScroll.clear(false, false);
			this.BConChapter.setValue(fcut2.dot_index, false);
		}

		private void fineCutText(int pre_cut_index)
		{
			if (!this.isQuitChecking())
			{
				this.BxR.deactivate();
				FatalShower.FCut fcut = this.ACutPoint[this.cur_cut];
				using (BList<string> blist = ListBuffer<string>.Pop(0))
				{
					if (NelMSGResource.getContent(fcut.msg_label, blist, false, false, true) && blist.Count > 0 && blist[0].Length > 0)
					{
						this.Tx.reserveText(blist, 0, false);
						if (TX.getCurrentFamily().is_english)
						{
							this.Tx.auto_condense_line = (this.Tx.auto_condense = false);
							this.Tx.auto_wrap = true;
						}
						else
						{
							this.Tx.auto_wrap = false;
							this.Tx.auto_condense_line = true;
						}
						if (this.t_text >= 0f)
						{
							this.t_text = 0f;
							if (pre_cut_index < this.cur_cut)
							{
								this.Book.progressPage(true);
							}
							else if (pre_cut_index > this.cur_cut)
							{
								this.Book.progressPage(false);
							}
						}
						this.FlgHideUi.Rem("NOMSG");
					}
					else
					{
						this.FlgHideUi.Add("NOMSG");
					}
				}
				this.Tx.forceProgressNextStack();
				this.TxReposit();
				this.all_char_shown = false;
				return;
			}
			if (this.freecam)
			{
				this.changeFreeCam(false);
			}
			this.showQuitBox();
		}

		private void showQuitBox()
		{
			this.FlgHideUi.Add("NOMSG");
			this.BxR.activate();
			aBtn aBtn;
			if (this.need_remake_confirm_box)
			{
				this.need_remake_confirm_box = false;
				this.BxR.Clear();
				this.BxR.getBox().frametype = UiBox.FRAMETYPE.DARK;
				this.BxR.positionD(0f, 0f, 3, 30f);
				this.BxR.WHanim(360f, 140f, true, true).wh_animZero(true, true);
				this.BxR.alignx = ALIGN.CENTER;
				this.BxR.use_scroll = false;
				this.BxR.margin_in_tb = 50f;
				aBtn = this.BxR.addButtonT<aBtnNel>(new DsnDataButton
				{
					navi_auto_fill = true,
					title = "&&Fatal_quit",
					name = "Fatal_quit",
					skin = "normal_dark",
					skin_title = (this.from_memory ? "&&Fatal_quit_onmem" : "&&Fatal_quit"),
					w = 280f,
					h = 28f,
					fnClick = new FnBtnBindings(this.fnClickMenuBtn),
					click_snd = "cancel",
					HovCurs = this.HovCurs
				});
			}
			else
			{
				aBtn = this.BxR.getBtn("Fatal_quit");
			}
			aBtn.Select(true);
		}

		public void reselectQuitBtn()
		{
			aBtn btn = this.BxR.getBtn("Fatal_quit");
			if (btn != null)
			{
				btn.Select(true);
			}
		}

		private void prepareSpineBaseLayer(bool no_clear_anim)
		{
			FatalShower.FCut fcut = this.ACutPoint[this.cur_cut];
			GameObject[] array = this.AAGob[this.cur_buffer];
			SpineViewer[] array2 = this.AASpv[this.cur_buffer];
			MeshDrawer meshDrawer = this.AMdFadeScreen[this.cur_buffer];
			if (this.AApre_spine_loaded[this.cur_buffer] != fcut.Ause_spine || this.Apre_spine_stencil[this.cur_buffer] != fcut.spine_stencil)
			{
				this.AApre_spine_loaded[this.cur_buffer] = fcut.Ause_spine;
				Material[][] array3 = this.prepareMaterialForStencil(fcut.spine_stencil);
				for (int i = 0; i < fcut.Ause_spine.Length; i++)
				{
					SpvLoader spvLoader = this.OLoadSpine[fcut.Ause_spine[i]];
					SpineViewer spineViewer = array2[i];
					spineViewer.reloadAnimKey(spvLoader.key);
					spineViewer.attachPreloadAssets(spvLoader.SpAtlasAsset, spvLoader.SpDataAsset, spvLoader.Tx, array3[this.cur_buffer][i]);
				}
				this.Apre_spine_stencil[this.cur_buffer] = fcut.spine_stencil;
			}
			int num = X.isinStr(fcut.Ause_spine, fcut.base_spine, -1);
			FtLayer ftLayer = this.AALay[this.cur_buffer][0];
			ftLayer.enabled = true;
			if (!no_clear_anim)
			{
				ftLayer.setSpine(fcut.base_spine, array2[num], this.OLoadSpine[fcut.base_spine]);
				FatalShower.AnmInfo[] array4 = ((fcut.AAanim != null) ? fcut.AAanim[num] : null);
				if (array4 != null)
				{
					int num2 = ((fcut.Aloop_to != null) ? fcut.Aloop_to[num] : 0);
					FatalShower.AnmInfo anmInfo = array4[0];
					ftLayer.Spv.clearAnim(anmInfo.anim, num2, (fcut.AAskin != null && fcut.AAskin[num] != null) ? fcut.AAskin[num][0] : null);
					ftLayer.Spv.setTimePosition(0, (float)anmInfo.start * 0.033333335f);
					ftLayer.Spv.force_enable_mesh_render = true;
					ftLayer.Spv.update_sharedmaterials_array = false;
					int num3 = array4.Length;
					for (int j = 1; j < num3; j++)
					{
						anmInfo = array4[j];
						if (anmInfo.valid)
						{
							ftLayer.Spv.addAnim(j, anmInfo.anim, (anmInfo.loop_to < 0) ? num2 : anmInfo.loop_to, 0f, 1f);
							ftLayer.Spv.setTimePosition(j, (float)anmInfo.start * 0.033333335f);
						}
					}
					SpvLoader spvLoader2 = this.OLoadSpine[fcut.Ause_spine[num]];
					ftLayer.setTextureForSpine(spvLoader2.Tx);
					if (fcut.AAskin != null)
					{
						ftLayer.Spv.mergeSkins(fcut.AAskin[num], 1);
					}
					ftLayer.Spv.timeScale = ((fcut.Aspeed != null) ? fcut.Aspeed[num] : 1f);
				}
				else
				{
					SpineViewer spv = ftLayer.Spv;
					ftLayer.clear(PIC.SPINE);
					if (spv != null)
					{
						spv.gameObject.SetActive(false);
					}
					ftLayer.enabled = false;
				}
			}
			ftLayer.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, fcut.spine_ag360));
		}

		private void swapBuffer(bool immediate, bool clear_next_buffer = true)
		{
			List<FtLayer> list = this.AALay[this.cur_buffer];
			List<FtMosaic> list2 = this.AAMosShower[this.cur_buffer];
			for (int i = list.Count - 1; i >= 0; i--)
			{
				FtLayer ftLayer = list[i];
				if (ftLayer != null)
				{
					ftLayer.swapInit(true);
				}
			}
			IN.setZ(this.AMrdFadeScreen[this.cur_buffer].transform, 0.1f);
			this.cur_buffer = 1 - this.cur_buffer;
			IN.setZ(this.AMrdFadeScreen[this.cur_buffer].transform, 0.2f);
			this.AFadeCam[this.cur_buffer].gameObject.SetActive(true);
			if (clear_next_buffer)
			{
				list = this.AALay[this.cur_buffer];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					FtLayer ftLayer2 = list[j];
					if (ftLayer2 != null)
					{
						ftLayer2.swapInit(false);
						ftLayer2.enabled = j == 0;
					}
				}
			}
			list2 = this.AAMosShower[this.cur_buffer];
			for (int k = list2.Count - 1; k >= 0; k--)
			{
				FtMosaic ftMosaic = list2[k];
				if (clear_next_buffer)
				{
					ftMosaic.enabled = false;
				}
			}
			this.t_buffer = (immediate ? 30f : 0f);
		}

		private void CutReloadReading(bool set_base, bool not_swap = false)
		{
			FatalShower.FCut fcut = this.ACutPoint[this.cur_cut];
			List<FtLayer> list = this.AALay[this.cur_buffer];
			FtLayer ftLayer;
			if (!this.last_confirm)
			{
				for (int i = list.Count - 1; i >= 0; i--)
				{
					ftLayer = list[i];
					if (ftLayer != null)
					{
						ftLayer.enabled = i == 0;
					}
				}
			}
			ftLayer = (this.CenterLayer = list[0]);
			if (this.BiteSwapLay != null)
			{
				(this.BiteSwapLay.SD as FtSpecialDrawerBiteTransition).abortAnim();
				this.BiteSwapLay = null;
			}
			this.trans_type = TRANSITION.FADE;
			if (set_base)
			{
				ftLayer.RepositBase(fcut.BasePos, 1f);
			}
			SpineViewer spineViewer = this.AASpv[this.cur_buffer][0];
			for (int j = 0; j < 2; j++)
			{
				CsvReaderA csvReaderA = ((j == 0) ? fcut.CRPre : this.CR);
				if (csvReaderA != null)
				{
					csvReaderA.seek_set((j == 0) ? 0 : (fcut.line + 1));
					int num = ((j == 0 || this.cur_cut >= this.ACutPoint.Count - 2) ? csvReaderA.getLength() : this.ACutPoint[this.cur_cut + 1].line);
					while (csvReaderA.read() && csvReaderA.get_cur_line() < num)
					{
						if (!this.last_confirm || csvReaderA.cmd == "MOSAIC" || csvReaderA.cmd == "MOSAIC_")
						{
							string cmd = csvReaderA.cmd;
							if (cmd != null)
							{
								uint num2 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
								FtLayer ftLayer2;
								if (num2 <= 2301316518U)
								{
									if (num2 <= 1144928650U)
									{
										if (num2 <= 601007592U)
										{
											if (num2 <= 369207100U)
											{
												if (num2 != 135386068U)
												{
													if (num2 != 316823588U)
													{
														if (num2 != 369207100U)
														{
															goto IL_1414;
														}
														if (!(cmd == "EF_PTCT"))
														{
															goto IL_1414;
														}
														goto IL_10E2;
													}
													else
													{
														if (!(cmd == "BG_EVPIC"))
														{
															goto IL_1414;
														}
														goto IL_0951;
													}
												}
												else
												{
													if (!(cmd == "BASE_SPINE"))
													{
														goto IL_1414;
													}
													goto IL_0951;
												}
											}
											else if (num2 != 402403584U)
											{
												if (num2 != 433934950U)
												{
													if (num2 != 601007592U)
													{
														goto IL_1414;
													}
													if (!(cmd == "TRANSLATE_COSI"))
													{
														goto IL_1414;
													}
													if ((ftLayer2 = this.GetLayer(csvReaderA._1, false)) != null)
													{
														ftLayer2.translateInit(csvReaderA.Nm(2, 0f), csvReaderA.Nm(3, 0f), csvReaderA.Nm(4, 0f), false);
														ftLayer2.trstype = TRSL.COSI;
														continue;
													}
													continue;
												}
												else
												{
													if (!(cmd == "BG"))
													{
														goto IL_1414;
													}
													goto IL_0951;
												}
											}
											else if (!(cmd == "EF_PTC"))
											{
												goto IL_1414;
											}
										}
										else if (num2 <= 986311122U)
										{
											if (num2 != 822181189U)
											{
												if (num2 != 932065059U)
												{
													if (num2 != 986311122U)
													{
														goto IL_1414;
													}
													if (!(cmd == "EF_RADIATION"))
													{
														goto IL_1414;
													}
													if ((ftLayer2 = this.GetLayer(csvReaderA._1, true)) != null)
													{
														uint num3 = X.NmUI(csvReaderA._2, uint.MaxValue, true, true);
														float num4 = 0f;
														float num5 = 0f;
														uint num6;
														if (TX.noe(csvReaderA._3))
														{
															num6 = num3;
														}
														else
														{
															num6 = X.NmUI(csvReaderA._3, num3, true, true);
															num4 = csvReaderA.Nm(4, 0f);
															num5 = csvReaderA.Nm(5, 0f);
														}
														ftLayer2.initFill(PIC.RADIATION, num3, num6, num4, num5);
														continue;
													}
													continue;
												}
												else
												{
													if (!(cmd == "EF_PICT"))
													{
														goto IL_1414;
													}
													goto IL_116D;
												}
											}
											else
											{
												if (!(cmd == "TRANSITION_BITE"))
												{
													goto IL_1414;
												}
												this.trans_type = TRANSITION.BITE;
												this.trans_agR = csvReaderA.Nm(1, 0f) * 3.1415927f / 180f;
												continue;
											}
										}
										else if (num2 != 989430776U)
										{
											if (num2 != 1099064151U)
											{
												if (num2 != 1144928650U)
												{
													goto IL_1414;
												}
												if (!(cmd == "ROTATE"))
												{
													goto IL_1414;
												}
												if (csvReaderA._1 != "#0" && (ftLayer2 = this.GetLayer(csvReaderA._1, false)) != null)
												{
													ftLayer2.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, csvReaderA.Nm(2, 0f)));
													continue;
												}
												continue;
											}
											else
											{
												if (!(cmd == "SPINE_STENCIL"))
												{
													goto IL_1414;
												}
												goto IL_0951;
											}
										}
										else
										{
											if (!(cmd == "%MOSAIC"))
											{
												goto IL_1414;
											}
											goto IL_0951;
										}
									}
									else if (num2 <= 1614201738U)
									{
										if (num2 <= 1473198116U)
										{
											if (num2 != 1224525797U)
											{
												if (num2 != 1306446278U)
												{
													if (num2 != 1473198116U)
													{
														goto IL_1414;
													}
													if (!(cmd == "EF_FO"))
													{
														goto IL_1414;
													}
												}
												else
												{
													if (!(cmd == "HANDSHAKE"))
													{
														goto IL_1414;
													}
													if ((ftLayer2 = this.GetLayer(csvReaderA._1, false)) != null)
													{
														ftLayer2.handShake(csvReaderA.Nm(2, 0f), csvReaderA.Nm(3, 1f));
														continue;
													}
													continue;
												}
											}
											else
											{
												if (!(cmd == "%SPINE_SV"))
												{
													goto IL_1414;
												}
												goto IL_0951;
											}
										}
										else if (num2 != 1513536024U)
										{
											if (num2 != 1580586464U)
											{
												if (num2 != 1614201738U)
												{
													goto IL_1414;
												}
												if (!(cmd == "EF_GRD_FI"))
												{
													goto IL_1414;
												}
											}
											else
											{
												if (!(cmd == "SPEED"))
												{
													goto IL_1414;
												}
												goto IL_0951;
											}
										}
										else if (!(cmd == "EF_GRD_FO"))
										{
											goto IL_1414;
										}
										if ((ftLayer2 = this.GetLayer(csvReaderA._1, true)) != null)
										{
											uint num7 = X.NmUI(csvReaderA._2, 4278190080U, true, true);
											uint num8;
											if (TX.noe(csvReaderA._3))
											{
												num8 = num7 & 16777215U;
											}
											else
											{
												num8 = X.NmUI(csvReaderA._3, num7 & 16777215U, true, true);
											}
											ftLayer2.initFill((csvReaderA.cmd == "EF_FO") ? PIC.FILL : ((csvReaderA.cmd == "EF_GRD_FI") ? PIC.GRD_FI : PIC.GRD_FO), num7, num8, csvReaderA.Nm(4, 0f), csvReaderA.Nm(5, 0f));
											continue;
										}
										continue;
									}
									else if (num2 <= 1979923609U)
									{
										if (num2 != 1643791826U)
										{
											if (num2 != 1891474104U)
											{
												if (num2 != 1979923609U)
												{
													goto IL_1414;
												}
												if (!(cmd == "MOSAIC"))
												{
													goto IL_1414;
												}
												goto IL_0DBD;
											}
											else
											{
												if (!(cmd == "SKIN"))
												{
													goto IL_1414;
												}
												goto IL_0951;
											}
										}
										else
										{
											if (!(cmd == "VO"))
											{
												goto IL_1414;
											}
											goto IL_0951;
										}
									}
									else if (num2 <= 2218644302U)
									{
										if (num2 != 2177811494U)
										{
											if (num2 != 2218644302U)
											{
												goto IL_1414;
											}
											if (!(cmd == "POPPOLY"))
											{
												goto IL_1414;
											}
											goto IL_1220;
										}
										else
										{
											if (!(cmd == "BASEZ"))
											{
												goto IL_1414;
											}
											if ((ftLayer2 = this.GetLayer(csvReaderA._1, false)) != null)
											{
												ftLayer2.base_z = csvReaderA.Nm(2, 0f);
												continue;
											}
											continue;
										}
									}
									else if (num2 != 2250974389U)
									{
										if (num2 != 2301316518U)
										{
											goto IL_1414;
										}
										if (!(cmd == "PTC"))
										{
											goto IL_1414;
										}
									}
									else
									{
										if (!(cmd == "PTC1"))
										{
											goto IL_1414;
										}
										goto IL_1007;
									}
									if ((ftLayer2 = this.GetLayer(csvReaderA._1, true)) == null)
									{
										continue;
									}
									if (ftLayer2.ptype != PIC.SPINE)
									{
										csvReaderA.tError("spine のレイヤーを渡すこと");
										continue;
									}
									ftLayer2.initEffect().addPtc(csvReaderA._2, csvReaderA._3, csvReaderA.Nm(4, 0f), csvReaderA.Nm(5, 0f), csvReaderA.Int(6, 0), (float)csvReaderA.Int(7, 0), csvReaderA.cmd == "EF_PTC", false, false);
									continue;
								}
								else
								{
									if (num2 <= 3398804388U)
									{
										if (num2 <= 2565374185U)
										{
											if (num2 <= 2397170684U)
											{
												if (num2 != 2367204274U)
												{
													if (num2 != 2372857733U)
													{
														if (num2 != 2397170684U)
														{
															goto IL_1414;
														}
														if (!(cmd == "EF_POPPOLY"))
														{
															goto IL_1414;
														}
														goto IL_1220;
													}
													else
													{
														if (!(cmd == "%TRM"))
														{
															goto IL_1414;
														}
														goto IL_0951;
													}
												}
												else
												{
													if (!(cmd == "MOSAIC_"))
													{
														goto IL_1414;
													}
													goto IL_0DBD;
												}
											}
											else if (num2 != 2414135037U)
											{
												if (num2 != 2423122796U)
												{
													if (num2 != 2565374185U)
													{
														goto IL_1414;
													}
													if (!(cmd == "POS"))
													{
														goto IL_1414;
													}
													if (csvReaderA._1 != "#0" && (ftLayer2 = this.GetLayer(csvReaderA._1, false)) != null)
													{
														ftLayer2.RepositBase(csvReaderA.Nm(2, 0f), csvReaderA.Nm(3, 0f), csvReaderA.Nm(4, 1f));
														continue;
													}
													continue;
												}
												else
												{
													if (!(cmd == "SND_ONCE"))
													{
														goto IL_1414;
													}
													SND.Ui.play(csvReaderA._1, false);
													continue;
												}
											}
											else
											{
												if (!(cmd == "CHILD"))
												{
													goto IL_1414;
												}
												if ((ftLayer2 = this.GetLayer(csvReaderA.getIndex(csvReaderA.clength - 1), false)) != null)
												{
													for (int k = 1; k < csvReaderA.clength - 2; k++)
													{
														FtLayer layer;
														if (!(csvReaderA.getIndex(k) == "->") && (layer = this.GetLayer(csvReaderA.getIndex(k), false)) != null)
														{
															float z = ftLayer2.transform.position.z;
															layer.transform.SetParent(ftLayer2.transform, false);
															IN.setZAbs(ftLayer2.transform, z);
														}
													}
													continue;
												}
												continue;
											}
										}
										else if (num2 <= 2815240503U)
										{
											if (num2 != 2719294369U)
											{
												if (num2 != 2781953465U)
												{
													if (num2 != 2815240503U)
													{
														goto IL_1414;
													}
													if (!(cmd == "%SPINE"))
													{
														goto IL_1414;
													}
													goto IL_0951;
												}
												else
												{
													if (!(cmd == "PTCRZ1"))
													{
														goto IL_1414;
													}
													goto IL_1007;
												}
											}
											else
											{
												if (!(cmd == "ANIM+"))
												{
													goto IL_1414;
												}
												goto IL_0951;
											}
										}
										else if (num2 <= 3096637805U)
										{
											if (num2 != 3076112742U)
											{
												if (num2 != 3096637805U)
												{
													goto IL_1414;
												}
												if (!(cmd == "CLIPRECT"))
												{
													goto IL_1414;
												}
												goto IL_0951;
											}
											else
											{
												if (!(cmd == "TRANSLATE_ZSIN"))
												{
													goto IL_1414;
												}
												if ((ftLayer2 = this.GetLayer(csvReaderA._1, false)) != null)
												{
													ftLayer2.translateInit(csvReaderA.Nm(2, 0f), csvReaderA.Nm(3, 0f), (!this.freecam) ? csvReaderA.Nm(4, 0f) : 1f, false);
													ftLayer2.trstype = TRSL.ZSIN;
													continue;
												}
												continue;
											}
										}
										else if (num2 != 3119780743U)
										{
											if (num2 != 3398804388U)
											{
												goto IL_1414;
											}
											if (!(cmd == "EF_PTCRZ"))
											{
												goto IL_1414;
											}
										}
										else
										{
											if (!(cmd == "BITE_SWAPPING"))
											{
												goto IL_1414;
											}
											if ((ftLayer2 = this.GetLayer(csvReaderA._1, true)) == null)
											{
												continue;
											}
											if (ftLayer2 == ftLayer)
											{
												csvReaderA.tError("#1 以上を指定すること");
												continue;
											}
											this.BiteSwapLay = ftLayer2.initBiteSwapping(ftLayer, csvReaderA, 2);
											continue;
										}
									}
									else if (num2 <= 3688922219U)
									{
										if (num2 <= 3521370842U)
										{
											if (num2 != 3412022864U)
											{
												if (num2 != 3493131381U)
												{
													if (num2 != 3521370842U)
													{
														goto IL_1414;
													}
													if (!(cmd == "IMMEDIATE"))
													{
														goto IL_1414;
													}
													goto IL_0951;
												}
												else
												{
													if (!(cmd == "TRANSLATE"))
													{
														goto IL_1414;
													}
													if ((ftLayer2 = this.GetLayer(csvReaderA._1, false)) != null)
													{
														ftLayer2.translateInit(csvReaderA.Nm(2, 0f), csvReaderA.Nm(3, 0f), (!this.freecam) ? csvReaderA.Nm(4, 0f) : 1f, false);
														continue;
													}
													continue;
												}
											}
											else
											{
												if (!(cmd == "ANIM"))
												{
													goto IL_1414;
												}
												goto IL_0951;
											}
										}
										else if (num2 != 3669304749U)
										{
											if (num2 != 3687055305U)
											{
												if (num2 != 3688922219U)
												{
													goto IL_1414;
												}
												if (!(cmd == "LOOP"))
												{
													goto IL_1414;
												}
												goto IL_0951;
											}
											else
											{
												if (!(cmd == "%SPINE_SIZE"))
												{
													goto IL_1414;
												}
												goto IL_0951;
											}
										}
										else
										{
											if (!(cmd == "BASE_CLIPRECT"))
											{
												goto IL_1414;
											}
											float num9 = csvReaderA.Nm(1, -FatalShower.scvwh);
											float num10 = csvReaderA.Nm(2, -FatalShower.scvhh);
											this.BaseClipRect.Set(num9, num10, csvReaderA.Nm(3, FatalShower.scvwh) - num9, csvReaderA.Nm(4, FatalShower.scvhh) - num10);
											continue;
										}
									}
									else if (num2 <= 3822158642U)
									{
										if (num2 != 3739981390U)
										{
											if (num2 != 3777737718U)
											{
												if (num2 != 3822158642U)
												{
													goto IL_1414;
												}
												if (!(cmd == "PTCRZ"))
												{
													goto IL_1414;
												}
											}
											else
											{
												if (!(cmd == "PTCT"))
												{
													goto IL_1414;
												}
												goto IL_10E2;
											}
										}
										else
										{
											if (!(cmd == "HANDSHAKE_INJECT"))
											{
												goto IL_1414;
											}
											if ((ftLayer2 = this.GetLayer(csvReaderA._1, false)) != null)
											{
												ftLayer2.handShakeInject(csvReaderA.Nm(2, 0f), csvReaderA.Nm(3, 0f), csvReaderA.Nm(4, 1f));
												continue;
											}
											continue;
										}
									}
									else if (num2 <= 4118184341U)
									{
										if (num2 != 4104883935U)
										{
											if (num2 != 4118184341U)
											{
												goto IL_1414;
											}
											if (!(cmd == "PICT"))
											{
												goto IL_1414;
											}
											goto IL_116D;
										}
										else
										{
											if (!(cmd == "TRANSLATE_ZCOS"))
											{
												goto IL_1414;
											}
											if ((ftLayer2 = this.GetLayer(csvReaderA._1, false)) != null)
											{
												ftLayer2.translateInit(csvReaderA.Nm(2, 0f), csvReaderA.Nm(3, 0f), csvReaderA.Nm(4, 0f), false);
												ftLayer2.trstype = TRSL.ZCOS;
												continue;
											}
											continue;
										}
									}
									else if (num2 != 4132132668U)
									{
										if (num2 != 4144876992U)
										{
											goto IL_1414;
										}
										if (!(cmd == "SND"))
										{
											goto IL_1414;
										}
										goto IL_0951;
									}
									else
									{
										if (!(cmd == "TRANSLATE_ZSIN2"))
										{
											goto IL_1414;
										}
										if ((ftLayer2 = this.GetLayer(csvReaderA._1, false)) != null)
										{
											ftLayer2.translateInit(csvReaderA.Nm(2, 0f), csvReaderA.Nm(3, 0f), (!this.freecam) ? csvReaderA.Nm(4, 0f) : 1f, false);
											ftLayer2.trstype = TRSL.ZSIN2;
											continue;
										}
										continue;
									}
									if ((ftLayer2 = this.GetLayer(csvReaderA._1, true)) == null)
									{
										continue;
									}
									if (ftLayer2.ptype != PIC.SPINE)
									{
										csvReaderA.tError("spine のレイヤーを渡すこと");
										continue;
									}
									ftLayer2.initEffect().addPtc(csvReaderA._2, csvReaderA._3, csvReaderA.Nm(4, 0f), 0f, csvReaderA.Int(5, 0), (float)csvReaderA.Int(6, 0), csvReaderA.cmd == "EF_PTCRZ", true, false);
									continue;
								}
								IL_0951:
								if (j == 0)
								{
									csvReaderA.tError("プレコマンドに定義できないコマンド");
									continue;
								}
								continue;
								IL_0DBD:
								if (not_swap || (ftLayer2 = this.GetLayer(csvReaderA._1, false)) == null || (csvReaderA.cmd == "MOSAIC_" && X.sensitive_level == 0))
								{
									continue;
								}
								if (ftLayer2.ptype != PIC.SPINE)
								{
									csvReaderA.tError("spine のレイヤーを渡すこと");
									continue;
								}
								List<MosaicShower.MosaicInfo> list2 = X.Get<string, List<MosaicShower.MosaicInfo>>(this.OAMosaicInfo, ftLayer2.spine_key);
								if (csvReaderA.clength == 2)
								{
									for (int l = 0; l < list2.Count; l++)
									{
										this.GetMosaicLayer().initMosaic(ftLayer2, list2[l]);
									}
									continue;
								}
								int count = list2.Count;
								for (int m = 2; m < csvReaderA.clength; m++)
								{
									string index = csvReaderA.getIndex(m);
									bool flag = false;
									for (int n = 0; n < count; n++)
									{
										if (list2[n].bone_key == index)
										{
											this.GetMosaicLayer().initMosaic(ftLayer2, list2[n]);
											flag = true;
											break;
										}
									}
									if (!flag)
									{
										csvReaderA.tError("不明なモザイク指定: " + index);
									}
								}
								continue;
								IL_1007:
								if ((ftLayer2 = this.GetLayer(csvReaderA._1, true)) == null)
								{
									continue;
								}
								if (ftLayer2.ptype != PIC.SPINE)
								{
									csvReaderA.tError("spine のレイヤーを渡すこと");
									continue;
								}
								FtEffect ftEffect = ftLayer2.initEffect();
								if (csvReaderA.cmd == "PTCRZ1")
								{
									ftEffect.addPtc(csvReaderA._2, csvReaderA._3, csvReaderA.Nm(4, 0f), 0f, csvReaderA.Int(5, 0), (float)csvReaderA.Int(6, 0), true, true, true);
									continue;
								}
								ftEffect.addPtc(csvReaderA._2, csvReaderA._3, csvReaderA.Nm(4, 0f), csvReaderA.Nm(5, 0f), csvReaderA.Int(6, 0), (float)csvReaderA.Int(7, 0), true, false, true);
								continue;
								IL_10E2:
								if ((ftLayer2 = this.GetLayer(csvReaderA._1, true)) == null)
								{
									continue;
								}
								if (ftLayer2.ptype != PIC.SPINE)
								{
									csvReaderA.tError("spine のレイヤーを渡すこと");
									continue;
								}
								ftLayer2.initEffect().addPtc(csvReaderA._2, null, csvReaderA.Nm(3, 0f), csvReaderA.Nm(4, 0f), csvReaderA.Int(5, 0), (float)csvReaderA.Int(6, 0), csvReaderA.cmd == "EF_PTCT", false, false);
								continue;
								IL_116D:
								if ((ftLayer2 = this.GetLayer(csvReaderA._1, true)) != null)
								{
									int num11 = -1;
									bool flag2 = false;
									if (csvReaderA.clength >= 5)
									{
										if (TX.isStart(csvReaderA.getIndex(4), '^'))
										{
											num11 = X.NmI(TX.slice(csvReaderA.getIndex(4), 1), -1, false, false);
											flag2 = num11 >= 0;
										}
										else
										{
											num11 = csvReaderA.Int(4, -1);
										}
									}
									ftLayer2.initPicture(X.Get<string, SpvLoader>(this.OLoadSpine, fcut.base_spine), csvReaderA._2, csvReaderA.Nm(3, 1f), num11, flag2);
									ftLayer2.is_effect = csvReaderA.cmd == "EF_PICT";
									continue;
								}
								continue;
								IL_1220:
								if ((ftLayer2 = this.GetLayer(csvReaderA._1, true)) != null)
								{
									ftLayer2.initPopPoly(X.NmUI(csvReaderA._2, 0U, false, true), X.NmUI(csvReaderA._3, 0U, false, true), csvReaderA.Int(4, 0), csvReaderA, 5);
									ftLayer2.is_effect = csvReaderA.cmd == "EF_POPPOLY";
									continue;
								}
								continue;
							}
							IL_1414:
							if (!TX.isStart(csvReaderA.cmd, '!'))
							{
								csvReaderA.tError("不明なコマンド: " + csvReaderA.cmd);
							}
						}
					}
				}
			}
			if (this.freecam)
			{
				this.AALay[this.cur_buffer][0].finalizeAnimate(true);
			}
			this.CenterFirstPos.Set(this.CenterLayer.x, this.CenterLayer.y, 0f);
			this.need_redraw_bg = true;
		}

		private FtLayer GetLayer(string key, bool set_enable = true)
		{
			return this.GetLayer(this.cur_buffer, key, set_enable);
		}

		private FtLayer GetLayer(int buffer, string key, bool set_enable = true)
		{
			STB stb = TX.PopBld(key, 0);
			int num = 0;
			bool flag = false;
			if (stb.Is('*', num))
			{
				num++;
				flag = true;
			}
			if (stb.Is('#', num))
			{
				num++;
			}
			int num3;
			int num2 = stb.NmI(num, out num3, -1, -1);
			TX.ReleaseBld(stb);
			FtLayer layer = this.GetLayer(buffer, num2, set_enable);
			if (layer != null && flag)
			{
				this.CenterLayer = layer;
			}
			return layer;
		}

		private FtLayer GetLayer(int buffer, int id, bool set_enable = true)
		{
			if (id < 0)
			{
				return null;
			}
			List<FtLayer> list = this.AALay[buffer];
			int count = list.Count;
			FtLayer ftLayer = null;
			int i = 0;
			while (i < count)
			{
				FtLayer ftLayer2 = list[i];
				if (!set_enable && !ftLayer2.enabled)
				{
					return null;
				}
				if (!ftLayer2.enabled || ftLayer2.id == id)
				{
					ftLayer = ftLayer2;
					if (!set_enable)
					{
						return ftLayer;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			if (ftLayer == null)
			{
				if (!set_enable)
				{
					return null;
				}
				ftLayer = new FtLayer(this, buffer.ToString() + " " + id.ToString(), buffer);
				list.Add(ftLayer);
			}
			ftLayer.id = id;
			if (set_enable)
			{
				ftLayer.enabled = true;
			}
			list.Sort((FtLayer A, FtLayer B) => FatalShower.sortFtLayer(A, B));
			return ftLayer;
		}

		private FtMosaic GetMosaicLayer()
		{
			return this.GetMosaicLayer(this.cur_buffer);
		}

		private FtMosaic GetMosaicLayer(int buffer)
		{
			FtMosaic ftMosaic = null;
			List<FtMosaic> list = this.AAMosShower[buffer];
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				FtMosaic ftMosaic2 = list[i];
				if (!ftMosaic2.enabled)
				{
					ftMosaic = ftMosaic2;
					break;
				}
			}
			if (ftMosaic == null)
			{
				list.Add(ftMosaic = new FtMosaic(null));
			}
			ftMosaic.abs_z = this.GobBase.transform.position.z - 3.5f;
			return ftMosaic;
		}

		private static int sortFtLayer(FtLayer A, FtLayer B)
		{
			if (A.enabled == B.enabled)
			{
				return A.id - B.id;
			}
			if (!A.enabled)
			{
				return 1;
			}
			return -1;
		}

		private float fadeTransitionPrepare(MeshDrawer Md, bool init_flag, float tz)
		{
			if (this.trans_type == TRANSITION.BITE)
			{
				Md.chooseSubMesh(1, false, true);
				if (init_flag)
				{
					Md.setMaterial(this.MtrTransitionBite, false);
				}
				FatalShower.drawBiteTransition(Md, this.trans_agR, tz, 1f);
				return (float)((tz < 0.5f) ? 1 : 0);
			}
			return 1f - tz;
		}

		internal static void drawBiteTransition(MeshDrawer Md, float trans_agR, float tz, float alpha)
		{
			float num = ((tz < 0.5f) ? X.ZLINE(tz, 0.5f) : (-1f + X.ZLINE(tz - 0.5f, 0.5f)));
			Md.getMaterial().SetFloat("_Level", num);
			if (trans_agR != -1000f)
			{
				Md.Col = C32.MulA(4283780170U, alpha);
				Md.Uv2(X.Cos(trans_agR), X.Sin(trans_agR), false);
				Md.initForImg(MTRX.MIicon).Rect(0f, 0f, FatalShower.scvw, FatalShower.scvh, false).allocUv2(0, true);
			}
		}

		private void FnHideUiExecute(FlaggerT<string> F)
		{
			if (this.Book == null)
			{
				return;
			}
			if (this.t_book >= 0f)
			{
				this.t_book = -1f;
				this.Book.deactivate();
			}
			this.t_text = -1f;
			this.Tx.showImmediate(false, false);
			this.Tx.enabled = false;
			this.GobTxScroll.SetActive(false);
		}

		private void FnShowUiExecute(FlaggerT<string> F)
		{
			if (this.Book == null)
			{
				return;
			}
			this.t_text = 0f;
			if (this.t_book < 0f)
			{
				this.t_book = 0f;
				this.Book.activate();
			}
			if (this.auto_mode && this.all_char_shown)
			{
				this.t_autonext = X.Mn(this.t_autonext, 30f);
			}
			this.Tx.enabled = true;
			this.GobTxScroll.SetActive(true);
		}

		private void redrawBg(bool force = false)
		{
			if (force || this.need_redraw_bg)
			{
				MeshDrawer meshDrawer = this.AMdBg[this.cur_buffer].clear(false, false);
				meshDrawer.Col = MTRX.ColWhite;
				this.drawBg(meshDrawer, this.ACutPoint[this.cur_cut].Bg, this.AALay[this.cur_buffer][0], true);
				this.need_redraw_bg = false;
			}
		}

		private void drawBg(MeshDrawer Md, FatalShower.BgInfo Info, FtLayer L, bool update = true)
		{
			if (Info.Tx != null)
			{
				float num = L.scale;
				Texture tx = Info.Tx;
				num = ((Info.scale < 0f) ? (-Info.scale) : (((num - 1f) * 0.5f + 1f) * Info.scale));
				Rect uvRect = Info.UvRect;
				float num2 = (float)tx.width * num * uvRect.width;
				float num3 = (float)tx.height * num * uvRect.height;
				if (num2 < FatalShower.scvw || num3 < FatalShower.scvh)
				{
					Md.initForImg(tx, uvRect, false);
					for (;;)
					{
						if (num2 < FatalShower.scvw)
						{
							float num4 = FatalShower.scvw / num2;
							num *= num4;
							num2 = FatalShower.scvw;
							num3 *= num4;
						}
						else
						{
							if (num3 >= FatalShower.scvh)
							{
								break;
							}
							float num5 = FatalShower.scvh / num3;
							num *= num5;
							num2 *= num5;
							num3 = FatalShower.scvh;
						}
					}
					Md.Rect(0f, 0f, num2, num3, false);
				}
				else
				{
					Md.initForImg(tx);
					float x = uvRect.center.x;
					float y = uvRect.center.y;
					float num6 = uvRect.width * 0.5f;
					float num7 = uvRect.height * 0.5f;
					float num8 = L.x * Info.scroll_ratio;
					float num9 = L.y * Info.scroll_ratio;
					float num10 = num2 * 0.5f;
					float num11 = num3 * 0.5f;
					float num12 = x - (num8 + FatalShower.scvwh) / num10 * num6;
					float num13 = y - (num9 + FatalShower.scvhh) / num11 * num7;
					float num14 = x + (FatalShower.scvwh - num8) / num10 * num6;
					float num15 = y + (FatalShower.scvhh - num9) / num11 * num7;
					Md.TriRectBL(0);
					Md.uvRectN(num12, num13).PosD(-FatalShower.scvwh, -FatalShower.scvhh, null).uvRectN(num12, num15)
						.PosD(-FatalShower.scvwh, FatalShower.scvhh, null)
						.uvRectN(num14, num15)
						.PosD(FatalShower.scvwh, FatalShower.scvhh, null)
						.uvRectN(num14, num13)
						.PosD(FatalShower.scvwh, -FatalShower.scvhh, null);
				}
			}
			if (update)
			{
				Md.updateForMeshRenderer(false);
			}
		}

		private void TxReposit()
		{
			float num = this.Tx.get_swidth_px();
			float num2 = this.Tx.get_sheight_px();
			float num3 = 1f;
			if (num >= FatalShower.scw * 0.7f)
			{
				num3 = FatalShower.scw * 0.7f / num;
				num *= num3;
				num2 *= num3;
			}
			this.Tx.transform.localScale = new Vector3(num3, num3, 1f);
			float num4 = X.Mx(-FatalShower.sch * 0.5f + num2 * 0.5f + 30f, -FatalShower.sch * 0.47f + 48f);
			IN.PosP(this.Tx.transform, -num * 0.5f, num4, -4f);
			this.GobTxScroll.transform.localPosition = new Vector3((num * 0.5f + 30f) * 0.015625f, (num4 - 10f) * 0.015625f, -4.75f);
			this.MdTxBack.clearSimple();
			this.MdTxBack.Col = C32.d2c(4278190080U);
			this.MdTxBack.base_x = 0f;
			this.MdTxBack.base_y = (num4 + num2 * 0.5f) * 0.015625f;
			num2 = (num2 + 140f) * 0.5f * 0.015625f;
			num = X.Mx((num + 180f) * 0.5f * 0.015625f, num2 * 2.5f);
			Rect rectIUv = MTRX.EffBlurCircle245.RectIUv;
			float num5 = rectIUv.x + rectIUv.width * 0.5f;
			this.MdTxBack.TriRectBL(0).TriRectBL(4).Tri(3, 2, 5, false)
				.Tri(3, 5, 4, false);
			this.MdTxBack.uvRectN(rectIUv.x, rectIUv.y).Pos(-num, -num2, null).uvRectN(rectIUv.x, rectIUv.yMax)
				.Pos(-num, num2, null)
				.uvRectN(num5, rectIUv.yMax)
				.Pos(-num + num2, num2, null)
				.uvRectN(num5, rectIUv.y)
				.Pos(-num + num2, -num2, null)
				.Pos(num - num2, -num2, null)
				.uvRectN(num5, rectIUv.yMax)
				.Pos(num - num2, num2, null)
				.uvRectN(rectIUv.xMax, rectIUv.yMax)
				.Pos(num, num2, null)
				.uvRectN(rectIUv.xMax, rectIUv.y)
				.Pos(num, -num2, null);
			this.MaTxBack.SetWhole(true);
			if (this.t_text >= 0f)
			{
				this.t_text = 0f;
				this.text_alpha = 0f;
			}
		}

		public float text_alpha
		{
			get
			{
				if (!(this.Tx != null))
				{
					return 0f;
				}
				return this.Tx.alpha;
			}
			set
			{
				if (this.Tx == null)
				{
					return;
				}
				this.Tx.alpha = value;
				this.MaTxBack.setAlpha1(0.8f * value, false);
				this.MdTxBack.updateForMeshRenderer(true);
			}
		}

		private void BookReposit()
		{
			float num = ((this.t_book < 0f) ? X.ZPOW(22f + this.t_book, 22f) : X.ZSIN2(this.t_book, 22f));
			float num2 = X.NI(-280, 0, num);
			IN.PosP2(this.GobTxBack.transform, 0f, num2);
			IN.PosP(this.GobBook.transform, 0f, num2 - 120f - FatalShower.schh, -3.5f);
		}

		private void createMenuInner()
		{
			this.BxMenu.Clear();
			this.BxMenu.stencil_ref = (this.BxMenu.box_stencil_ref_mask = -1);
			this.BxMenu.alignx = ALIGN.LEFT;
			this.BxMenu.getBox().frametype = UiBox.FRAMETYPE.DARK_SIMPLE;
			this.BxMenu.margin_in_lr = 30f;
			this.BxMenu.margin_in_tb = 18f;
			this.BxMenu.item_margin_x_px = 20f;
			this.BxMenu.item_margin_y_px = 10f;
			this.BxMenu.init();
			this.BConChapter = this.BxMenu.addRadioT<aBtnNel>(new DsnDataRadio
			{
				keys = X.makeToStringed<int>(X.makeCountUpArray(this.dot_count, 0, 1)),
				skin = "chapter",
				w = 44f,
				h = 30f,
				clms = X.Mn(this.dot_count, 8),
				margin_w = 1,
				margin_h = 1,
				fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnChangedChapter),
				def = X.Mx(0, this.cur_cut),
				HovCurs = this.HovCurs
			});
			this.BxMenu.Br();
			DsnDataButton dsnDataButton = new DsnDataButton
			{
				skin = "checkbox_dark",
				title = "&&Fatal_check_hide_ui",
				w = this.BxMenu.use_w,
				h = 27f,
				HovCurs = this.HovCurs
			};
			this.BCheckHideUI = this.BxMenu.addButtonT<aBtnNel>(dsnDataButton);
			this.BCheckHideUI.addClickFn(new FnBtnBindings(this.fnHideUiChanging));
			this.BCheckHideUI.SetChecked(this.FlgHideUi.hasKey("B"), true);
			dsnDataButton.title = "&&Fatal_auto_mode";
			this.BCheckAutoMode = this.BxMenu.Br().addButtonT<aBtnNelArrowOvr>(dsnDataButton);
			this.BCheckAutoMode.SetChecked(this.auto_mode, true);
			this.BCheckAutoMode.addClickFn(delegate(aBtn V)
			{
				this.setAutoMode(!this.auto_mode);
				return true;
			});
			this.BCheckAutoMode.addHoverFn(delegate(aBtn V)
			{
				this.BAutoSlider.getCtSetter().activate(true);
				return true;
			});
			this.BCheckAutoMode.addOutFn(delegate(aBtn V)
			{
				this.BAutoSlider.getCtSetter().deactivate();
				return true;
			});
			this.BxMenu.Br();
			Designer designer = this.BxMenu.addTab("Bauto", this.BxMenu.use_w, 28f, 0f, 0f, false);
			designer.Smallest();
			designer.margin_in_lr = 5f;
			designer.init();
			designer.XSh(12f);
			this.BAutoSlider = this.BxMenu.addSliderCT(new DsnDataSlider
			{
				name = "autospd",
				skin = "normal_dark",
				mn = 2f,
				mx = (float)CFG.FATAL_SPEED_MAX,
				def = (float)CFG.fatal_automode_speed,
				w = 100f,
				fnChanged = new aBtnMeter.FnMeterBindings(this.fnChangedAutoModeSpeed),
				fnDescConvert = new FnDescConvert(this.fnDescConvertAutoMode),
				unselectable = 2,
				HovCurs = this.HovCurs
			}, designer.use_w - 100f - 10f, null, false);
			this.BxMenu.endTab(true);
			CtSetterMeter ctSetterMeter = this.BAutoSlider.getCtSetter() as CtSetterMeter;
			ctSetterMeter.auto_deactivate_kettei = false;
			ctSetterMeter.SelectedOverride = this.BCheckAutoMode;
			ctSetterMeter.Color = MTRX.ColWhite;
			this.BCheckAutoMode.addFnDir(new aBtnNelArrowOvr.FnBtnBindingArrowOvr(this.BAutoSlider.simulateLRtoSetter));
			dsnDataButton.title = "&&Fatal_free_cam";
			this.BCheckFreeCam = this.BxMenu.Br().addButtonT<aBtnNel>(dsnDataButton);
			this.BCheckFreeCam.SetChecked(this.freecam, true);
			this.BCheckFreeCam.addClickFn(new FnBtnBindings(this.fnFreeCamChanging));
			this.BCheckFreeCam.setNaviT(this.BCheckAutoMode, true, true);
			this.BxMenu.Br().addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				w = 180f,
				h = 30f,
				margin_w = 10f,
				name = "optbtns",
				skin = "normal_dark",
				titles = new string[] { "&&btn_option", "&&Fatal_skip" },
				fnClick = new FnBtnBindings(this.fnClickMenuBtn),
				HovCurs = this.HovCurs
			}).Get(1)
				.click_snd = "cancel";
		}

		private bool fnChangedAutoModeSpeed(aBtnMeter _B, float pre_value, float cur_value)
		{
			CFG.fatal_automode_speed = (byte)X.IntR(cur_value);
			if (this.all_char_shown)
			{
				this.t_autonext = 0f;
			}
			return true;
		}

		private bool fnChangedChapter(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			int count = this.ACutPoint.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.ACutPoint[i].dot_index == cur_value)
				{
					this.changeCut(i, true, false);
					this.run(1f);
					return true;
				}
			}
			return true;
		}

		public bool fnActvChangedMenu(aBtnBoxEar MenuEar, bool activated)
		{
			if (activated && this.t_quit >= 0f)
			{
				return false;
			}
			if (activated)
			{
				if (this.PreSelected != null && this.PreSelected.skin != "chapter")
				{
					this.PreSelected.Select(true);
				}
				else
				{
					aBtn aBtn = this.BConChapter.Get(this.ACutPoint[this.cur_cut].dot_index);
					if (aBtn != null)
					{
						aBtn.Select(true);
					}
					else
					{
						this.BConChapter.Get(0).Select(true);
					}
				}
				if (this.freecam)
				{
					CURS.Rem("DRAG", "");
				}
			}
			else
			{
				this.PreSelected = aBtn.PreSelected;
				if (this.PreSelected != null && this.PreSelected.title == "&&Fatal_quit")
				{
					this.PreSelected = null;
				}
				if (this.PreSelected != null)
				{
					this.PreSelected.Deselect(true);
				}
				CURS.Active.Rem(this.BxMenu.key);
				if (this.freecam && this.EditCfg == null)
				{
					CURS.Set("DRAG", "tl_move");
					this.WhLis.bind();
				}
				if (this.last_confirm)
				{
					this.reselectQuitBtn();
				}
			}
			return true;
		}

		private bool fnAutoLr(aBtn B, AIM a)
		{
			if (a == AIM.L)
			{
				if (CtSetterMeter.moveToLeftS(this.BAutoSlider, 1f))
				{
					SND.Ui.play("toggle_button_close", false);
				}
				else
				{
					CURS.limitVib(this.BCheckAutoMode, a);
					SND.Ui.play("toggle_button_limit", false);
				}
				return true;
			}
			if (a == AIM.R)
			{
				if (CtSetterMeter.moveToRightS(this.BAutoSlider, 1f))
				{
					SND.Ui.play("toggle_button_open", false);
				}
				else
				{
					CURS.limitVib(this.BCheckAutoMode, a);
					SND.Ui.play("toggle_button_limit", false);
				}
				return true;
			}
			return false;
		}

		private void fnDescConvertAutoMode(STB Stb)
		{
			STB.PARSERES parseres;
			if (Stb.Nm(out parseres))
			{
				float num = (float)STB.NmRes(parseres, -1.0);
				Stb.AddTxA("Fatal_auto_mode_speed", false);
				using (STB stb = TX.PopBld(null, 0))
				{
					Stb.TxRpl(stb.spr_after(num * 0.5f, 1));
				}
			}
		}

		private bool fnHideUiChanging(aBtn B)
		{
			if (!this.BCheckHideUI.isChecked())
			{
				this.BCheckHideUI.SetChecked(true, true);
				if (!this.all_char_shown)
				{
					this.Tx.showImmediate(false, false);
				}
				this.FlgHideUi.Add("B");
			}
			else
			{
				this.BCheckHideUI.SetChecked(false, true);
				this.FlgHideUi.Rem("B");
			}
			return true;
		}

		public bool is_temporary_hiding
		{
			get
			{
				return false;
			}
		}

		private void setAutoMode(bool v)
		{
			this.auto_mode = v;
			if (this.all_char_shown)
			{
				this.t_autonext = 0f;
				if (!v)
				{
					this.t_arrow = 0;
				}
			}
			this.BCheckAutoMode.SetChecked(v, true);
		}

		private void initClipArea(bool fine_xy = true)
		{
			FatalShower.FCut fcut = this.ACutPoint[this.cur_cut];
			float num = fcut.BasePos.z * 1f;
			if (fine_xy)
			{
				this.WhLis.resetShowingArea(-fcut.BasePos.x / fcut.BasePos.z, -fcut.BasePos.y / fcut.BasePos.z, fcut.BasePos.z, -1000f, -1000f);
				if (fcut.ClipRect.width <= 0f || fcut.ClipRect.height <= 0f)
				{
					Rect baseClipRect = this.BaseClipRect;
					if (baseClipRect.width <= 0f)
					{
						new Rect(-FatalShower.scvwh, -FatalShower.scvhh, FatalShower.scvw, FatalShower.scvh);
					}
					this.WhLis.Movable = new Rect(baseClipRect.x * num - fcut.BasePos.x / fcut.BasePos.z, baseClipRect.y * num - fcut.BasePos.y / fcut.BasePos.z, baseClipRect.width * num, baseClipRect.height * num);
				}
				else
				{
					this.WhLis.Movable = new Rect(fcut.ClipRect.x, fcut.ClipRect.y, fcut.ClipRect.width, fcut.ClipRect.height);
				}
			}
			this.WhLis.max_z_scale = num + 2f;
		}

		private bool fnWheelScale(WheelListener WhLis, float x, float y, float z)
		{
			FtLayer ftLayer = this.AALay[this.cur_buffer][0];
			ftLayer.scale = z;
			ftLayer.base_x = -x * z;
			ftLayer.base_y = -y * z;
			ftLayer.Reposit(false);
			if (!this.bg_scale_fixed)
			{
				this.need_redraw_bg = true;
			}
			return true;
		}

		private bool fnGrabFinished(WheelListener WhLis, float x, float y, float z)
		{
			if (IN.isMouseUp(12) && X.chkLEN(0f, 0f, x * 64f, y * 64f, 25f))
			{
				this.t_click_lock = -1f;
			}
			return true;
		}

		internal void fineWhLisPosition(FtLayer Lay)
		{
			if (Lay == this.AALay[this.cur_buffer][0])
			{
				this.WhLis.resetShowingArea(-Lay.x / Lay.scale, -Lay.y / Lay.scale, Lay.scale, -1000f, -1000f);
				if (!this.bg_scale_fixed)
				{
					this.need_redraw_bg = true;
				}
			}
		}

		private bool fnFreeCamChanging(aBtn B)
		{
			if (this.EditCfg != null)
			{
				return false;
			}
			this.changeFreeCam(true);
			return true;
		}

		public void changeFreeCam(bool show_quit_box = true)
		{
			if (!this.all_char_shown)
			{
				this.Tx.showImmediate(false, false);
			}
			FtLayer ftLayer = this.AALay[this.cur_buffer][0];
			if (!this.freecam)
			{
				this.freecam = true;
				if (this.CenterFirstPos.z == 0f)
				{
					float num = this.CenterLayer.x - this.CenterFirstPos.x;
					float num2 = this.CenterLayer.y - this.CenterFirstPos.y;
					if (this.CenterLayer != ftLayer)
					{
						this.CenterFirstPos.z = 1f;
						this.WhLis.shiftPosition(num, num2, false, true);
						if (this.BaseMem.z >= 0f)
						{
							this.BaseMem.x = this.BaseMem.x - num;
							this.BaseMem.y = this.BaseMem.y - num2;
						}
					}
				}
				if (this.BaseMem.z >= 0f)
				{
					ftLayer.RepositBase(this.BaseMem, 1f);
				}
				this.need_redraw_bg = true;
				this.BCheckFreeCam.SetChecked(true, true);
				this.MenuEar.setPF("pict_freecam");
				CURS.Active.Add("Fatal");
				if (!this.menu_shown)
				{
					CURS.Set("DRAG", "tl_move");
					this.WhLis.bind();
				}
				if (show_quit_box && this.isQuitChecking())
				{
					this.BxR.deactivate();
				}
			}
			else
			{
				this.freecam = false;
				this.MenuEar.setPF("pict_menu");
				this.BCheckFreeCam.SetChecked(false, true);
				CURS.Rem("DRAG", "");
				CURS.Active.Rem("Fatal");
				this.BaseMem.Set(ftLayer.base_x, ftLayer.base_y, ftLayer.scale);
				ftLayer.RepositBase(this.ACutPoint[this.cur_cut].BasePos, 1f);
				if (this.CenterLayer != ftLayer && this.CenterFirstPos.z == 1f)
				{
					this.CenterFirstPos.z = 0f;
					Rect movable = this.WhLis.Movable;
					this.initClipArea(true);
					float num3 = movable.x - this.WhLis.Movable.x;
					float num4 = movable.y - this.WhLis.Movable.y;
					this.BaseMem.x = this.BaseMem.x + num3;
					this.BaseMem.y = this.BaseMem.y + num4;
				}
				this.WhLis.hide();
				if (show_quit_box && this.isQuitChecking())
				{
					this.showQuitBox();
				}
			}
			this.effectLayerFade(false);
		}

		public void effectLayerFade(bool immediate = false)
		{
			bool flag = !this.freecam;
			List<FtLayer> list = this.AALay[this.cur_buffer];
			for (int i = list.Count - 1; i >= 0; i--)
			{
				FtLayer ftLayer = list[i];
				if (ftLayer != null)
				{
					ftLayer.effectAlphaFade((float)((immediate || ftLayer.ptype == PIC.BITE_TRANSITION) ? 1 : 70), flag, i == 0, ftLayer == this.CenterLayer);
				}
			}
		}

		public bool isQuitChecking()
		{
			return this.cur_cut >= this.ACutPoint.Count - 1;
		}

		private bool fnClickMenuBtn(aBtn B)
		{
			if (B.title == "&&btn_option")
			{
				this.initCfg();
			}
			if (B.title == "&&Fatal_skip" && this.cur_cut != this.ACutPoint.Count - 1)
			{
				this.changeCut(this.ACutPoint.Count - 1, false, false);
				this.MenuEar.deactivate();
			}
			if (B.title == "&&Fatal_quit")
			{
				if (this.bgm_replaced)
				{
					BGM.fadeout(0f, 90f, true);
				}
				this.t_quit = 0f;
				this.MenuEar.hide();
				this.BxR.deactivate();
				this.BxDesc.deactivate();
				this.GetLayer("#99", true).initFill(PIC.FILL, 5590090U, 4283780170U, 80f, 0f);
				this.BxMenu.position(-FatalShower.scwh + 220f, -FatalShower.schh - 170f - 60f, -1000f, -1000f, false);
				this.BxMenu.getBox().position_max_time(60, 60);
			}
			return true;
		}

		public bool EvtWait(bool is_first = false)
		{
			if (is_first)
			{
				return true;
			}
			if (this.autoinit_z_in_wait_fn != -1000f)
			{
				if (this.isMaterialLoaded())
				{
					this.initGameObject(null, IN.gui_layer, this.autoinit_z_in_wait_fn);
					this.autoinit_z_in_wait_fn = -1000f;
				}
				return true;
			}
			if (!this.isActive())
			{
				this.destruct();
				return false;
			}
			return true;
		}

		public void initCfg()
		{
			this.WhLis.hide();
			this.PreSelected = null;
			this.FlgHideUi.Add("CFG");
			CURS.Rem("DRAG", "tl_move");
			this.MenuEar.hide();
			this.MenuEar.gameObject.SetActive(false);
			this.BxMenu.animate_maxt = 30;
			this.BxMenu.Smallest();
			this.BxMenu.margin_in_tb = 20f;
			this.BxMenu.margin_in_lr = 20f;
			this.BxMenu.WHanim(IN.w * 0.58f, 62f, true, true);
			this.BxMenu.Clear();
			this.BxMenu.bind();
			BtnContainer<aBtn> btnContainer = SceneTitleTemp.remakeSumitCancelButtonS(this.BxMenu, new FnBtnBindings(this.fnSubmitCfg), true, true, 0);
			this.SubmitCfg = btnContainer.Get(0);
			this.BxR.Clear();
			this.BxR.getBox().frametype = UiBox.FRAMETYPE.MAIN;
			this.BxR.WH(680f, IN.h - 240f);
			this.BxR.positionD(-190f, 90f, 0, 50f);
			this.BxR.activate();
			this.BxR.box_stencil_ref_mask = -1;
			this.BxR.margin_in_tb = 11f;
			this.BxR.margin_in_lr = 28f;
			this.BxR.use_scroll = true;
			this.BxR.item_margin_x_px = 14f;
			this.BxR.item_margin_y_px = 18f;
			this.BxR.alignx = ALIGN.CENTER;
			this.EditCfg = new UiCFG(this.BxR, this.BxDesc, this.BxMenu, false, true, null);
			IN.setZ(this.BxMenu.transform, -5f);
		}

		public bool fnSubmitCfg(aBtn B)
		{
			if (B.title == "&&Submit")
			{
				this.EditCfg.submitData();
			}
			else
			{
				this.EditCfg.revertData();
			}
			this.BxMenu.WHanim(440f, 340f, false, false);
			this.createMenuInner();
			BtnContainerRunner btnContainerRunner = this.BxMenu.Get("optbtns", false) as BtnContainerRunner;
			this.BxR.deactivate();
			this.BxDesc.deactivate();
			this.EditCfg.deactivateDesigner();
			this.EditCfg = this.EditCfg.destruct();
			this.BxMenu.posSetA(-FatalShower.scwh + 220f, 0f, -FatalShower.scwh + 220f, 0f, true);
			this.BxMenu.fineMove();
			this.FlgHideUi.Rem("CFG");
			this.MenuEar.gameObject.SetActive(true);
			this.MenuEar.bind();
			this.fineScreenBuffer(this.cur_buffer);
			if (this.freecam)
			{
				CURS.Active.Add("Fatal");
				if (!this.menu_shown)
				{
					this.WhLis.bind();
					CURS.Set("DRAG", "tl_move");
				}
			}
			this.need_remake_confirm_box = true;
			if ((int)this.ui_lang != TX.getCurrentFamilyIndex() || this.cur_cut == this.ACutPoint.Count - 1)
			{
				this.ui_lang = (byte)TX.getCurrentFamilyIndex();
				NelMSGResource.initResource(true);
				this.Tx.TargetFont = TX.getDefaultFont();
				this.fineCutText(this.cur_cut);
			}
			btnContainerRunner.Get(0).Select(true);
			return true;
		}

		public void progressReserved()
		{
		}

		public void executeRestMsgCmd(int count)
		{
		}

		public bool bg_scroll_fixed
		{
			get
			{
				return this.cur_cut >= 0 && this.cur_cut < this.ACutPoint.Count && this.ACutPoint[this.cur_cut].Bg.scroll_ratio <= 0f;
			}
		}

		public bool bg_scale_fixed
		{
			get
			{
				return this.cur_cut >= 0 && this.cur_cut < this.ACutPoint.Count && this.ACutPoint[this.cur_cut].Bg.scale < 0f;
			}
		}

		public bool isActive()
		{
			return this.t_quit < 90f;
		}

		public bool all_char_shown
		{
			get
			{
				return this.t_autonext != -1000f;
			}
			set
			{
				if (this.all_char_shown == value)
				{
					return;
				}
				if (value)
				{
					this.t_autonext = 0f;
					this.t_arrow = 0;
					return;
				}
				this.MdScroll.clear(false, false);
				this.t_autonext = -1000f;
			}
		}

		public bool last_confirm
		{
			get
			{
				return this.cur_cut == this.ACutPoint.Count - 1;
			}
		}

		public CameraBidingsBehaviour getCameraBindingsFor(int buffer)
		{
			return this.ACamBindBuffer[buffer];
		}

		public int getCurrentCut()
		{
			return this.cur_cut;
		}

		public bool menu_shown
		{
			get
			{
				return this.MenuEar.box_shown || this.EditCfg != null;
			}
		}

		public bool allow_auto_progress
		{
			get
			{
				return !this.menu_shown && !this.FlgHideUi.hasKey("B");
			}
		}

		public string talker_snd
		{
			get
			{
				return null;
			}
		}

		public void FixTextContent(STB Stb)
		{
		}

		public void setHkdsTypeToDefault(bool reserve = false)
		{
		}

		public void setHkdsType(NelMSG.HKDSTYPE _type)
		{
		}

		public VoiceController getVoiceController(string name, bool no_error = false)
		{
			VoiceController voiceController = X.Get<string, VoiceController>(this.OVoiceSrc, name);
			if (voiceController == null && !no_error)
			{
				X.de("不明なボイス発話者 " + name, null);
			}
			return voiceController;
		}

		public override string ToString()
		{
			return "FatalShower";
		}

		public EMOT default_emot
		{
			get
			{
				return EMOT.FADEIN;
			}
		}

		public uint default_col
		{
			get
			{
				return uint.MaxValue;
			}
		}

		public void TextRendererUpdated()
		{
		}

		private const string script_dir = "Fatal/";

		private const string script_ext = ".fatal";

		private bool evt_load_wait;

		public float autoinit_z_in_wait_fn = -1000f;

		public string def_material_path;

		public ValotileRenderer.IValotConnetcable ConnectCam;

		public Camera CamForMain;

		public string script_key;

		public readonly bool from_memory;

		private const string msg_header_default = "___Fatal";

		private const string fatal_anim_dir = "Fatal/";

		private const string fatal_bg_dir = "Fatal/_bg__";

		private string msg_header = "___Fatal";

		private const float bg_scale_default = 2f;

		private const float bg_scroll_ratio_default = 0.8f;

		private List<FatalShower.FCut> ACutPoint;

		private BDic<string, SpvLoader> OLoadSpine;

		private BDic<string, MTIOneImage> OLoadedTexture;

		private BDic<string, VoiceController> OVoiceSrc;

		private BDic<string, List<MosaicShower.MosaicInfo>> OAMosaicInfo;

		private byte ui_lang;

		private static float scw;

		private static float sch;

		private static float scwh;

		private static float schh;

		public static float scvw;

		public static float scvh;

		public static float scvwh;

		public static float scvhh;

		private const float object_scale = 1f;

		private const float ui_max_scale = 1.5f;

		public static float sc_scale = 1f;

		public static float ui_scale = 1f;

		private string bgm_key;

		private int dot_count;

		private int cur_cut;

		private int max_spine_count;

		private TRANSITION trans_type;

		private float trans_agR;

		public readonly int buf_layer0 = IN.LAY("UI");

		public readonly int buf_layer1 = IN.LAY("EDITOR_OTHER");

		private CsvReaderA CR;

		private Rect BaseClipRect = new Rect(0f, 0f, 0f, 0f);

		public float con_base_z;

		public RadiationDrawer DrRad;

		public const int MESHID_MAIN = 0;

		public const int MESHID_BITE = 1;

		internal GameObject GobBase;

		private NelEvTextRenderer Tx;

		private GameObject[][] AAGob;

		private SpineViewer[][] AASpv;

		private BDic<int, Material[][]> OAAMaterial;

		private string[][] AApre_spine_loaded;

		private int[] Apre_spine_stencil;

		private List<FtMosaic>[] AAMosShower;

		private int cur_buffer;

		private UiBoxDesignerFamily BxCon;

		private Material MtrTransitionBite;

		private HoverCursManager HovCurs;

		internal static Material MtrBiteTransS;

		private Material DefMtr;

		private const float menu_w = 440f;

		private const float menu_h = 340f;

		private bool need_recreate_quit;

		private UiBoxDesigner BxMenu;

		private UiBoxDesigner BxR;

		private UiBoxDesigner BxDesc;

		private BtnContainerRadio<aBtn> BConChapter;

		private aBtnNel BCheckHideUI;

		private aBtnNelArrowOvr BCheckAutoMode;

		private aBtnNel BCheckFreeCam;

		private aBtnNel BCfg;

		private aBtnNel BSkip;

		private aBtnMeterNel BAutoSlider;

		private WheelListener WhLis;

		private UiCFG EditCfg;

		private aBtnBoxEar MenuEar;

		private Camera[] AFadeCam;

		private CameraBidingsBehaviour[] ACamBindBuffer;

		private MeshDrawer[] AMdFadeScreen;

		private MeshDrawer[] AMdBg;

		private MeshRenderer[] AMrdFadeScreen;

		private MeshRenderer[] AMrdBg;

		private const float Z_FADING_BUF = 0.1f;

		private const float Z_MAIN_BUF = 0.2f;

		private NelBookDrawer Book;

		internal GameObject GobObjectBase;

		private GameObject GobUi;

		private GameObject GobBook;

		private GameObject GobTxBack;

		private GameObject GobTxScroll;

		private PxlFrame PFarrow;

		private int t_arrow;

		private MeshDrawer MdTxBack;

		private MdArranger MaTxBack;

		private MeshDrawer MdScroll;

		private Vector3 BaseMem;

		private aBtn PreSelected;

		private FatalShower.BgInfo PreBg;

		private List<FtLayer>[] AALay;

		private FtLayer CenterLayer;

		private Vector3 CenterFirstPos;

		private Flagger FlgHideUi;

		private float t_buffer;

		private float t_book = -22f;

		private float t_autonext = -1000f;

		private float t_text;

		private float t_quit = -1f;

		private float t_click_lock;

		private FtLayer BiteSwapLay;

		private bool freecam;

		private const float QUIT_MAXT = 90f;

		private const float BUFFER_FADET = 30f;

		private const float BOOK_FADET = 22f;

		private aBtn SubmitCfg;

		private bool auto_mode;

		private float auto_progress_t = -1f;

		private bool need_redraw_bg;

		private bool need_redraw_main_buffer;

		private bool config_active;

		private bool need_remake_confirm_box = true;

		private bool already_seen;

		private bool bgm_replaced;

		private const float Z_TX = -4f;

		public IN.FnWindowSizeChanged FD_WindowSizeChanged;

		private NelM2DBase M2D;

		private const string spine_dir = "SpineAnim/";

		private static Regex RegNotDot = new Regex("\\d+_+\\d+$");

		private static Regex RegPreCmd = new Regex("(\\!+)(CLEAR)?$");

		private static Regex RegCutPrefix = new Regex("^[ \\s\\t]*\\*");

		private static Regex RegImportOtherFile = new Regex("@(\\w+)");

		public const bool do_not_unload_image = false;

		public struct BgInfo
		{
			public BgInfo(MImage _MI)
			{
				this.MI = _MI;
				this.Evi = null;
				this.scale = 2f;
				this.scroll_ratio = 0.8f;
			}

			public Texture Tx
			{
				get
				{
					if (this.Evi != null)
					{
						return this.Evi.PF.getLayer(0).Img.get_I();
					}
					if (this.MI == null)
					{
						return null;
					}
					return this.MI.Tx;
				}
			}

			public Rect UvRect
			{
				get
				{
					if (this.Evi != null)
					{
						return this.Evi.PF.getLayer(0).Img.RectIUv;
					}
					return new Rect(0f, 0f, 1f, 1f);
				}
			}

			public MImage MI;

			public EvImg Evi;

			public float scale;

			public float scroll_ratio;
		}

		public struct AnmInfo
		{
			public AnmInfo(string _anim, int _loop_to = -1000, int _start = 0)
			{
				this.anim = _anim;
				this.loop_to = _loop_to;
				this.start = _start;
			}

			public bool valid
			{
				get
				{
					return TX.valid(this.anim) && this.anim != "''";
				}
			}

			public string anim;

			public int loop_to;

			public int start;
		}

		public sealed class FCut
		{
			public FCut(FatalShower.FCut Src = null)
			{
				if (Src == null)
				{
					return;
				}
				this.msg_label = Src.msg_label;
				this.dot_index = Src.dot_index;
				this.line = Src.line;
				this.loop_to = Src.loop_to;
				this.Ause_spine = Src.Ause_spine;
				this.force_swap = Src.force_swap;
				this.spine_stencil = Src.spine_stencil;
				this.Aloop_to = Src.Aloop_to;
				this.AAskin = Src.AAskin;
				this.AAanim = Src.AAanim;
				this.Aspeed = Src.Aspeed;
				this.base_spine = Src.base_spine;
				this.immediate_scrolling = Src.immediate_scrolling;
				this.spine_ag360 = Src.spine_ag360;
				this.Bg = Src.Bg;
				this.ClipRect = Src.ClipRect;
				this.OASndInfo = Src.OASndInfo;
				this.BasePos = Src.BasePos;
				this.CRPre = Src.CRPre;
			}

			public override string ToString()
			{
				return this.msg_label + " @" + this.line.ToString();
			}

			public void clearVoice(bool time_shuffle = true)
			{
				if (this.OASndInfo == null)
				{
					return;
				}
				foreach (KeyValuePair<string, List<VoiceInfo>> keyValuePair in this.OASndInfo)
				{
					for (int i = keyValuePair.Value.Count - 1; i >= 0; i--)
					{
						keyValuePair.Value[i].clearTime(time_shuffle);
					}
				}
			}

			public FatalShower.FCut Simple()
			{
				this.OASndInfo = null;
				this.show_dot = false;
				return this;
			}

			public void runSnd(float fcnt)
			{
				if (this.OASndInfo == null)
				{
					return;
				}
				foreach (KeyValuePair<string, List<VoiceInfo>> keyValuePair in this.OASndInfo)
				{
					for (int i = keyValuePair.Value.Count - 1; i >= 0; i--)
					{
						keyValuePair.Value[i].run(fcnt);
					}
				}
			}

			public bool isSame(FatalShower.FCut Src)
			{
				return this.canInheritBasePos(Src) && this.Aloop_to == Src.Aloop_to && this.AAskin == Src.AAskin && this.AAanim == Src.AAanim;
			}

			public bool canInheritBasePos(FatalShower.FCut Src)
			{
				return Src.dot_index == this.dot_index && this.Ause_spine == Src.Ause_spine && Src.base_spine == this.base_spine;
			}

			public string msg_label;

			public int dot_index;

			public int line;

			public int loop_to;

			public bool show_dot;

			public string[] Ause_spine;

			public int[] Aloop_to;

			public string[][] AAskin;

			public FatalShower.AnmInfo[][] AAanim;

			public float[] Aspeed;

			public string base_spine;

			public bool immediate_scrolling;

			public int spine_stencil = -1;

			public float spine_ag360;

			public bool force_swap;

			public Rect ClipRect;

			public FatalShower.BgInfo Bg;

			public BDic<string, List<VoiceInfo>> OASndInfo;

			public Vector3 BasePos;

			public CsvReaderA CRPre;
		}
	}
}

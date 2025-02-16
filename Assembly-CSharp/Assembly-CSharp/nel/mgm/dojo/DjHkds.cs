using System;
using System.Collections.Generic;
using System.Reflection;
using Better;
using UnityEngine;
using XX;

namespace nel.mgm.dojo
{
	public class DjHkds
	{
		public DjHkds(DjGM _GM)
		{
			this.Epo_hk_appear = new EfParticleOnce("dojo_hk_appear", EFCON_TYPE.FIXED);
			this.GM = _GM;
			this.OPosData = new BDic<DjHkds.POS, DjHkds.PosData>(12);
			this.TxTitle = IN.CreateGob(this.GM.DJ.Gob, "-hkds_title").AddComponent<TextRenderer>();
			IN.setZAbs(this.TxTitle.transform, -3.7749999f);
			this.TxTitle.size = 26f;
			this.TxTitle.html_mode = true;
			this.TxTitle.Col(MTRX.ColWhite);
			this.TxTitle.MakeValot(null, null);
			this.TxTitle.alignx = ALIGN.CENTER;
			this.TxTitle.aligny = ALIGNY.MIDDLE;
			this.TxTitle.use_valotile = true;
			this.TxTitle.text_content = TX.Get("Mgm_dojo_indicate_next_hand", "");
			this.TxTitle.gameObject.SetActive(false);
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			Type type = Type.GetType("nel.mgm.dojo.DjHkds,Assembly-CSharp");
			for (int i = 0; i < 12; i++)
			{
				Type type2 = type;
				string text = "FnGetPos_";
				DjHkds.POS pos = (DjHkds.POS)i;
				MethodInfo method = type2.GetMethod(text + pos.ToString(), bindingFlags);
				if (method != null)
				{
					this.OPosData[(DjHkds.POS)i] = new DjHkds.PosData((DjHkds.FnGetPos)Delegate.CreateDelegate(typeof(DjHkds.FnGetPos), this, method));
				}
			}
			this.OFadeData = new BDic<DjHkds.FADE, DjHkds.FadeData>(7);
			for (int j = 0; j < 7; j++)
			{
				Type type3 = type;
				string text2 = "FnGetFade_";
				DjHkds.FADE fade = (DjHkds.FADE)j;
				MethodInfo method2 = type3.GetMethod(text2 + fade.ToString(), bindingFlags);
				if (method2 != null)
				{
					this.OFadeData[(DjHkds.FADE)j] = new DjHkds.FadeData((DjHkds.FnGetFade)Delegate.CreateDelegate(typeof(DjHkds.FnGetFade), this, method2));
				}
			}
			this.OStyleData = new BDic<DjHkds.STYLE, DjHkds.StyleData>(8);
			for (int k = 0; k < 8; k++)
			{
				Type type4 = type;
				string text3 = "FnDraw_";
				DjHkds.STYLE style = (DjHkds.STYLE)k;
				MethodInfo method3 = type4.GetMethod(text3 + style.ToString(), bindingFlags);
				if (method3 != null)
				{
					this.OStyleData[(DjHkds.STYLE)k] = new DjHkds.StyleData((DjHkds.FnDrawWithStyle)Delegate.CreateDelegate(typeof(DjHkds.FnDrawWithStyle), this, method3));
				}
			}
			this.OGen = new BDic<string, DjHkdsGenerator>();
			this.CR = new CsvReader(TX.getResource("Data/mg_dojo/dojo_basic", ".csv", false), CsvReader.RegSpace, false);
			while (this.CR.read())
			{
				bool flag = true;
				string cmd = this.CR.cmd;
				if (cmd != null)
				{
					DjHkds.POS pos2;
					if (!(cmd == "%POS"))
					{
						DjHkds.FADE fade2;
						if (!(cmd == "%FADE"))
						{
							DjHkds.STYLE style2;
							if (!(cmd == "%STYLE"))
							{
								if (cmd == "/*" || cmd == "/*___")
								{
									flag = false;
									this.CR.seek_set(this.CR.get_cur_line());
								}
							}
							else if (FEnum<DjHkds.STYLE>.TryParse(this.CR._1, out style2, true))
							{
								DjHkds.StyleData styleData;
								if (this.OStyleData.TryGetValue(style2, out styleData))
								{
									styleData.diff = this.CR.Nm(2, 0f);
									this.OStyleData[style2] = styleData;
								}
								else
								{
									this.CR.tError("Fn 未定義STYLE:" + style2.ToString());
								}
							}
						}
						else if (FEnum<DjHkds.FADE>.TryParse(this.CR._1, out fade2, true))
						{
							DjHkds.FadeData fadeData;
							if (this.OFadeData.TryGetValue(fade2, out fadeData))
							{
								fadeData.diff = this.CR.Nm(2, 0f);
								if (this.CR.clength >= 4)
								{
									fadeData.is_behind = this.CR._3.IndexOf('B') >= 0;
									fadeData.is_stencil = this.CR._3.IndexOf('S') >= 0;
									fadeData.ignore_pos = this.CR._3.IndexOf("Ip") >= 0;
									fadeData.ignore_draw = this.CR._3.IndexOf("Id") >= 0;
								}
								this.OFadeData[fade2] = fadeData;
							}
							else
							{
								this.CR.tError("Fn 未定義FADE:" + fade2.ToString());
							}
						}
					}
					else if (FEnum<DjHkds.POS>.TryParse(this.CR._1, out pos2, true))
					{
						DjHkds.PosData posData;
						if (this.OPosData.TryGetValue(pos2, out posData))
						{
							posData.diff = this.CR.Nm(2, 0f);
							if (this.CR.clength >= 4)
							{
								posData.is_behind = this.CR._3.IndexOf('B') >= 0;
							}
							this.OPosData[pos2] = posData;
						}
						else
						{
							this.CR.tError("Fn 未定義POS:" + pos2.ToString());
						}
					}
				}
				if (!flag)
				{
					break;
				}
			}
		}

		public bool loadProgress()
		{
			if (this.CR == null)
			{
				return true;
			}
			DjHkdsGenerator djHkdsGenerator = null;
			string text = null;
			while (this.CR.read())
			{
				if (this.CR.cmd == "/*" || this.CR.cmd == "/*___")
				{
					if (djHkdsGenerator != null)
					{
						this.CR.seek_set(this.CR.get_cur_line());
						break;
					}
					text = this.CR.getIndex((this.CR.cmd == "/*") ? 2 : 1);
					djHkdsGenerator = new DjHkdsGenerator();
				}
				else if (djHkdsGenerator == null)
				{
					this.CR.tError("HK 読み取りエラー");
				}
				else
				{
					string cmd = this.CR.cmd;
					if (cmd != null)
					{
						uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
						if (num <= 1012374921U)
						{
							if (num != 498989871U)
							{
								if (num != 967958004U)
								{
									if (num == 1012374921U)
									{
										if (cmd == "max_diff")
										{
											djHkdsGenerator.max_diff = this.CR.Nm(1, djHkdsGenerator.max_diff);
											continue;
										}
									}
								}
								else if (cmd == "count")
								{
									djHkdsGenerator.count = this.CR.Int(1, 0);
									if (djHkdsGenerator.count <= 0)
									{
										djHkdsGenerator.count = 8 + djHkdsGenerator.count;
										continue;
									}
									continue;
								}
							}
							else if (cmd == "min_diff")
							{
								djHkdsGenerator.min_diff = this.CR.Nm(1, djHkdsGenerator.min_diff);
								continue;
							}
						}
						else if (num <= 2565374185U)
						{
							if (num != 1450621046U)
							{
								if (num == 2565374185U)
								{
									if (cmd == "POS")
									{
										float num2 = this.CR.Nm(1, 1f);
										for (int i = 2; i < this.CR.clength; i++)
										{
											DjHkds.POS pos;
											if (FEnum<DjHkds.POS>.TryParse(this.CR.getIndex(i), out pos, true))
											{
												DjHkds.PosData posData;
												if (this.OPosData.TryGetValue(pos, out posData))
												{
													djHkdsGenerator.addPos(pos, num2);
												}
												else
												{
													this.CR.tError("Fn 未定義POS:" + pos.ToString());
												}
											}
											else
											{
												this.CR.tError("不明なPOS:" + this.CR.getIndex(i));
											}
										}
										continue;
									}
								}
							}
							else if (cmd == "STYLE")
							{
								float num2 = this.CR.Nm(1, 1f);
								for (int j = 2; j < this.CR.clength; j++)
								{
									DjHkds.STYLE style;
									if (FEnum<DjHkds.STYLE>.TryParse(this.CR.getIndex(j), out style, true))
									{
										DjHkds.StyleData styleData;
										if (this.OStyleData.TryGetValue(style, out styleData))
										{
											djHkdsGenerator.addStyle(style, num2);
										}
										else
										{
											this.CR.tError("Fn 未定義STYLE:" + style.ToString());
										}
									}
									else
									{
										this.CR.tError("不明なSTYLE:" + this.CR.getIndex(j));
									}
								}
								continue;
							}
						}
						else if (num != 2731417303U)
						{
							if (num == 4015962131U)
							{
								if (cmd == "FADE")
								{
									float num2 = this.CR.Nm(1, 1f);
									for (int k = 2; k < this.CR.clength; k++)
									{
										DjHkds.FADE fade;
										if (FEnum<DjHkds.FADE>.TryParse(this.CR.getIndex(k), out fade, true))
										{
											DjHkds.FadeData fadeData;
											if (this.OFadeData.TryGetValue(fade, out fadeData))
											{
												djHkdsGenerator.addFade(fade, num2);
											}
											else
											{
												this.CR.tError("Fn 未定義FADE:" + fade.ToString());
											}
										}
										else
										{
											this.CR.tError("不明なFADE:" + this.CR.getIndex(k));
										}
									}
									continue;
								}
							}
						}
						else if (cmd == "term")
						{
							djHkdsGenerator.skill_term = this.CR.slice_join(1, " ", "");
							continue;
						}
					}
					this.CR.tError("HkdsGen 用の不明なコマンド:" + this.CR.cmd);
				}
			}
			if (djHkdsGenerator != null)
			{
				this.OGen[text] = djHkdsGenerator;
			}
			if (this.CR.isEnd())
			{
				if (!this.OGen.ContainsKey("_random"))
				{
					djHkdsGenerator = (this.OGen["_random"] = new DjHkdsGenerator());
					foreach (KeyValuePair<DjHkds.POS, DjHkds.PosData> keyValuePair in this.OPosData)
					{
						djHkdsGenerator.addPos(keyValuePair.Key, 1f);
					}
					foreach (KeyValuePair<DjHkds.FADE, DjHkds.FadeData> keyValuePair2 in this.OFadeData)
					{
						djHkdsGenerator.addFade(keyValuePair2.Key, 1f);
					}
				}
				this.CR = null;
				return true;
			}
			return false;
		}

		public DjHkdsGenerator GetGen(string s)
		{
			return X.Get<string, DjHkdsGenerator>(this.OGen, s);
		}

		public BDic<string, DjHkdsGenerator> getWholeGeneratorDictionary()
		{
			return this.OGen;
		}

		public void clearGenerated()
		{
			this.cur_pos = DjHkds.POS._UNPREPARED;
		}

		public bool Generate(DjHkdsGenerator Gen, int repeat_count = -1)
		{
			int num = 0;
			DjHkds.POS pos = DjHkds.POS.N;
			DjHkds.FADE fade = DjHkds.FADE.N;
			DjHkds.STYLE style = DjHkds.STYLE.N;
			float num2 = 999f;
			while (repeat_count < 0 || num < repeat_count)
			{
				DjHkds.POS pos2 = Gen.generatePos();
				DjHkds.FADE fade2 = Gen.generateFade();
				DjHkds.STYLE style2 = Gen.generateStyle();
				DjHkds.PosData posData;
				DjHkds.FadeData fadeData;
				DjHkds.StyleData styleData;
				if (this.OPosData.TryGetValue(pos2, out posData) && this.OFadeData.TryGetValue(fade2, out fadeData) && this.OStyleData.TryGetValue(style2, out styleData))
				{
					float num3 = fadeData.diff + (fadeData.ignore_pos ? 0f : posData.diff) + (fadeData.ignore_draw ? 0f : styleData.diff);
					if (X.BTWW(Gen.min_diff, num3, Gen.max_diff))
					{
						this.cur_pos = pos2;
						this.cur_fade = fade2;
						this.cur_style = style2;
						this.cur_hand = X.xors(3);
						return true;
					}
					if (num3 < num2)
					{
						num2 = num3;
						pos = pos2;
						fade = fade2;
						style = style2;
					}
				}
				num++;
				if (repeat_count < 0 && num >= 6)
				{
					this.cur_pos = pos;
					this.cur_fade = fade;
					this.cur_style = style;
					this.cur_hand = X.xors(3);
					return true;
				}
			}
			return false;
		}

		public Vector4 FnGetPos_N(EffectItem E, float t)
		{
			return new Vector4(-90f, 120f, 1f, 0f);
		}

		public Vector4 FnGetPos_L(EffectItem E, float t)
		{
			return new Vector4(-120f, 100f, 1f, 0f);
		}

		public Vector4 FnGetPos_BL(EffectItem E, float t)
		{
			return new Vector4(-100f, -170f, 1f, 0f);
		}

		public Vector4 FnGetPos_TR(EffectItem E, float t)
		{
			return new Vector4(110f, 130f, 1f, 0f);
		}

		public Vector4 FnGetPos_RB(EffectItem E, float t)
		{
			return new Vector4(130f, -120f, 1f, 0f);
		}

		public Vector4 FnGetPos_BH_BIG(EffectItem E, float t)
		{
			return new Vector4(-DjFigure.drawx_en, 50f, 6f, 0f);
		}

		public Vector4 FnGetPos_METEOR(EffectItem E, float t)
		{
			uint ran = X.GETRAN2(E.f0, E.f0 % 33);
			float num = X.ZLINE(t, (float)E.time * 1.05f);
			Vector4 vector = new Vector4(-DjFigure.drawx_en + IN.wh + 20f, -40f + 160f * X.RAN(ran, 513), 1f, (-0.12f + 0.21f * X.ZSIN(num)) * 3.1415927f);
			vector.x -= X.NI(340, 500, X.RAN(ran, 432)) * (X.ZSIN(num, 0.4f) * 0.5f + X.ZSIN(num, 1f) * 0.5f);
			vector.y -= X.NI(80, 150, X.RAN(ran, 968)) * X.ZCOS(num);
			return vector;
		}

		public Vector4 FnGetPos_JUMP(EffectItem E, float t)
		{
			uint ran = X.GETRAN2(E.f0, E.f0 % 33);
			float num = X.ZLINE(t, (float)E.time * 1.25f);
			Vector4 vector = new Vector4(-110f + X.RAN(ran, 1859) * 200f, -380f, 1f, (0.12f - 0.21f * X.ZSIN(num)) * X.NI(0.5f, 1f, X.RAN(ran, 1986)) * 3.1415927f);
			vector.y += X.NI(300, 420, X.RAN(ran, 3000)) * (X.ZPOWV(num, 0.66f) - X.ZPOWV(num - 0.66f, 0.85f) * 0.5f);
			return vector;
		}

		public Vector4 FnGetPos_ASIDE_B(EffectItem E, float t)
		{
			uint ran = X.GETRAN2(E.f0, E.f0 % 33);
			return new Vector4(-DjFigure.drawx_en + 30f + X.RAN(ran, 2384) * 190f, -250f, 1.5f, 0f);
		}

		public Vector4 FnGetPos_ASIDE_R(EffectItem E, float t)
		{
			uint ran = X.GETRAN2(E.f0, E.f0 % 33);
			return new Vector4(-DjFigure.drawx_en - 30f + IN.wh, 120f - X.RAN(ran, 867) * 240f, 1.5f, 0f);
		}

		public Vector4 FnGetPos_ASIDE_T(EffectItem E, float t)
		{
			uint ran = X.GETRAN2(E.f0, E.f0 % 33);
			return new Vector4(-DjFigure.drawx_en + 30f + X.RAN(ran, 2384) * 190f, 180f + 50f * X.RAN(ran, 1034), 1.5f, 0f);
		}

		public Vector4 FnGetPos_FALL(EffectItem E, float t)
		{
			uint ran = X.GETRAN2(E.f0, E.f0 % 33);
			Vector4 vector = new Vector4(-150f + X.RAN(ran, 1859) * 220f, 380f, 1f, X.NI(-0.15f, 0.15f, X.RAN(ran, 1986)) * 3.1415927f);
			float num = X.ZLINE(t, (float)E.time * 1.25f);
			vector.y -= X.NI(300, 420, X.RAN(ran, 3000)) * X.ZSIN(num);
			return vector;
		}

		private void FnGetFade_N(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, ref bool draw_basic, DjHkds.FnDrawWithStyle Draw)
		{
			float falpha = this.getFAlpha(E.af, tz, true);
			Draw(MdB, Md, E, Pos, tz, falpha);
		}

		private void FnGetFade_FADEIN(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, ref bool draw_basic, DjHkds.FnDrawWithStyle Draw)
		{
			float num = this.getFAlpha(E.af, tz, false) * (0.15f + 0.2f * X.ZLINE(E.af, (float)E.time * 0.25f) + 0.66f * X.ZPOW(E.af - 4f, (float)E.time * 0.8f));
			Draw(MdB, Md, E, Pos, tz, num);
		}

		private void FnGetFade_ZOOMIN(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, ref bool draw_basic, DjHkds.FnDrawWithStyle Draw)
		{
			float falpha = this.getFAlpha(E.af, tz, true);
			Pos.z *= X.NIL(0.2f, 1f, E.af, (float)E.time);
			Draw(MdB, Md, E, Pos, tz, falpha);
		}

		private void FnGetFade_WIPEUP(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, ref bool draw_basic, DjHkds.FnDrawWithStyle Draw)
		{
			this.FnGetFade_N(MdB, Md, E, Pos, tz, ref draw_basic, Draw);
			float num = 75f * Pos.z * 1.5f;
			this.getMask(E).RectBL(-num, -num, num * 2f, num * 2f * this.getHAlpha(E, E.af, 0.2f), false);
		}

		private void FnGetFade_SYASEN(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, ref bool draw_basic, DjHkds.FnDrawWithStyle Draw)
		{
			this.FnGetFade_N(MdB, Md, E, Pos, tz, ref draw_basic, Draw);
			float num = 75f * Pos.z * 1.5f * 2f;
			this.getMask(E).StripedRect(0f, 0f, num, num, X.ANMPT(50, 1f), this.getHAlpha(E, E.af, 0.14f), Pos.z * 10f, false);
		}

		private void FnGetFade_POINT_TX_R(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, ref bool draw_basic, DjHkds.FnDrawWithStyle Draw)
		{
			draw_basic = false;
			this.FnGetFade_PointingText(MdB, Md, E, Pos, tz, false);
		}

		private void FnGetFade_POINT_TX_L(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, ref bool draw_basic, DjHkds.FnDrawWithStyle Draw)
		{
			draw_basic = false;
			this.FnGetFade_PointingText(MdB, Md, E, Pos, tz, true);
		}

		private void FnGetFade_PointingText(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, bool is_left)
		{
			Md.base_px_x = DjRPC.drawxS(!is_left);
			Md.base_y = DjRPC.drawy_u;
			Md.base_x += DjRPC.i2posx_u(this.cur_hand);
			Md.base_y += ((this.cur_hand == 2) ? DjRPC.i2posy_u(this.cur_hand) : 0f);
			float num = 1f;
			float num2;
			if (this.cur_hand == 0 || this.cur_hand == 1)
			{
				Md.base_px_y -= 110f;
				num = (float)X.MPF(is_left);
				num2 = 1.5707964f;
			}
			else if (is_left)
			{
				Md.base_px_x += 150f;
				num2 = 3.1415927f;
				num = -1f;
			}
			else
			{
				Md.base_px_x -= 150f;
				num2 = 0f;
			}
			MdB.base_x = Md.base_x;
			MdB.base_y = Md.base_y;
			float num3 = X.ZSIN(E.af, 10f);
			this.TxTitle.gameObject.SetActive(true);
			IN.Pos2(this.TxTitle.transform, MdB.base_x - 0.3125f * num, MdB.base_y);
			this.TxTitle.alpha = num3;
			this.drawWideKadomaruHk(MdB, E.af, tz);
			MdB.Col = MdB.ColGrd.White().mulA(this.getFAlpha(E.af, tz, false) * num3).C;
			MdB.ArrowMush(65f * num, 0f, 22f, num2, false);
		}

		internal bool Draw(EffectItem E, float tz)
		{
			DjHkds.PosData posData;
			DjHkds.FadeData fadeData;
			DjHkds.StyleData styleData;
			if (this.OPosData.TryGetValue(this.cur_pos, out posData) && this.OFadeData.TryGetValue(this.cur_fade, out fadeData) && this.OStyleData.TryGetValue(this.cur_style, out styleData))
			{
				Vector4 vector = posData.Fn(E, E.af);
				DjFigure djFigure = this.GM.DJ.APr[1];
				E.x = (djFigure.drawx + vector.x) * 0.015625f;
				E.y = (djFigure.drawy + vector.y) * 0.015625f;
				this.cur_behind = posData.is_behind || fadeData.is_behind;
				MeshDrawer meshImg = E.GetMeshImg("hkb", MTRX.MIicon, BLEND.NORMAL, this.cur_behind);
				MeshDrawer mesh = E.GetMesh("hk", fadeData.is_stencil ? this.GM.MI.getMtr(251) : this.GM.MI.getMtr(BLEND.NORMAL, -1), this.cur_behind);
				mesh.base_z -= 0.005f;
				bool flag = true;
				fadeData.Fn(meshImg, mesh, E, vector, tz, ref flag, styleData.Fn);
				if (flag)
				{
					this.Epo_hk_appear.drawTo(meshImg, meshImg.base_px_x, meshImg.base_px_y, 0.67f * tz, 0, E.af, 0f);
					float num;
					float num2;
					this.drawBaseHk(meshImg, E.af, tz, vector, out num, out num2);
				}
				return true;
			}
			return false;
		}

		private void drawBaseHk(MeshDrawer MdB, float t, float tz, Vector4 Pos, out float scalex, out float scaley)
		{
			MdB.Col = C32.MulA(MTRX.ColBlack, 0.67f);
			scalex = 1f + (X.ZSIN2(t, 5f) * 0.25f - X.ZSIN(t - 2f, 8f) * 0.25f);
			scaley = (X.ZSIN2(t, 8f) * 1.1f - X.ZCOS(t - 4f, 12f) * 0.1f) * tz;
			MdB.initForImg(MTRX.EffCircle128, 0);
			MdB.Rect(0f, 0f, 150f * scalex * Pos.z, 150f * scaley * Pos.z, false);
			MdB.Col = C32.WMulA(X.ZLINE(t - 5f, 8f) * tz);
			if (MdB.Col.a > 0)
			{
				BMListChars chrL = MTRX.ChrL;
				using (STB stb = TX.PopBld("NEXT", 0))
				{
					chrL.DrawScaleStringTo(MdB, stb, 0f, -75f * Pos.z - 3f, 2f, 2f, ALIGN.CENTER, ALIGNY.BOTTOM, false, 0f, 0f, null);
				}
			}
		}

		private void drawWideKadomaruHk(MeshDrawer MdB, float t, float tz)
		{
			MdB.Col = C32.MulA(MTRX.ColBlack, 0.67f);
			float num = 2.96875f;
			float num2 = 80f * ((X.ZSIN2(t, 8f) * 1.1f - X.ZCOS(t - 4f, 12f) * 0.1f) * tz) * 0.015625f;
			MdB.uvRect(MdB.base_x - num * 0.5f, MdB.base_y - num2 * 0.5f, num, num2, MTRX.IconWhite, true, false);
			MdB.KadomaruRect(0f, 0f, num, num2, 0.390625f, 0f, true, 0f, 0f, false);
		}

		public void quitEffect()
		{
			this.TxTitle.gameObject.SetActive(false);
		}

		private void FnDraw_N(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, float alpha)
		{
			Md.Col = C32.WMulA(alpha);
			this.drawMain(Md, Pos, 0, uint.MaxValue);
		}

		private void FnDraw_G2N(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, float alpha)
		{
			alpha = X.Scr(alpha, 0.5f);
			float halpha = this.getHAlpha(E, E.af, 0.125f);
			Md.Col = C32.WMulA(alpha * (1f - halpha));
			this.drawMain(Md, Pos, 2, uint.MaxValue);
			Md.Col = C32.WMulA(alpha * halpha);
			this.drawMain(Md, Pos, 0, uint.MaxValue);
		}

		private void FnDraw_G(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, float alpha)
		{
			Md.Col = C32.WMulA(alpha);
			this.drawMain(Md, Pos, 1, uint.MaxValue);
		}

		private void FnDraw_ROT180(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, float alpha)
		{
			Md.Col = C32.WMulA(alpha);
			Pos.w += 3.1415927f;
			this.drawMain(Md, Pos, 0, uint.MaxValue);
		}

		private void FnDraw_ROTATING(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, float alpha)
		{
			uint ran = X.GETRAN2(E.f0, E.f0 % 33);
			Md.Col = C32.WMulA(alpha);
			Pos.w += X.NI(0.2f, 1.1f, X.RAN(ran, 2551)) + X.NI(0.7f, 1.8f, X.RAN(ran, 897)) * E.af * 0.016666668f * 3.1415927f;
			this.drawMain(Md, Pos, 0, uint.MaxValue);
		}

		private void FnDraw_SHAPE(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, float alpha)
		{
			Md.Col = C32.WMulA(alpha);
			this.drawMain(Md, Pos, 0, 31U);
		}

		private void FnDraw_GSHAPE(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, float alpha)
		{
			Md.Col = C32.WMulA(alpha);
			this.drawMain(Md, Pos, 3, uint.MaxValue);
		}

		private void FnDraw_COLOR(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, float alpha)
		{
			Md.Col = C32.MulA(DjRPC.i2col(this.cur_hand), alpha);
			float num = 75f * Pos.z * 0.015625f;
			Md.uvRect(Md.base_x - num, Md.base_y - num, num * 2f, num * 2f, this.GM.ImgWhite, true, false);
			Md.Circle(0f, 0f, num * 0.94f, 0f, true, 0f, 0f);
		}

		private float getFAlpha(float t, float tz, bool fade_basic = false)
		{
			return (0.75f + X.COSI(t, 13f) * 0.1f) * tz * (fade_basic ? X.ZLINE(t, 12f) : 1f);
		}

		private float getHAlpha(EffectItem E, float t, float first_val = 0.125f)
		{
			return X.NIL(first_val, 1f, t, (float)E.time);
		}

		private MeshDrawer getMask(EffectItem E)
		{
			MeshDrawer mesh = E.GetMesh("hkb", MTRX.getMtr(BLEND.MASK, 251), this.cur_behind);
			mesh.Col = MTRX.ColTrnsp;
			return mesh;
		}

		private void drawMain(MeshDrawer Md, Vector4 Pos, int id = 0, uint draw_frame_bits = 4294967295U)
		{
			if (Pos.z > 0f && Md.Col.a > 0)
			{
				Md.RotaPF(0f, 0f, Pos.z, Pos.z, Pos.w, this.GM.SqRpc.getFrame(this.cur_hand + id * 3), false, false, false, draw_frame_bits, false, 0);
			}
		}

		public bool isReadFinished()
		{
			return this.CR == null;
		}

		public bool hk_generated
		{
			get
			{
				return this.cur_pos != DjHkds.POS._UNPREPARED;
			}
		}

		internal DjHkds.POS get_cur_pos()
		{
			return this.cur_pos;
		}

		internal DjHkds.FADE get_cur_fade()
		{
			return this.cur_fade;
		}

		private readonly BDic<DjHkds.POS, DjHkds.PosData> OPosData;

		private readonly DjGM GM;

		private readonly BDic<DjHkds.FADE, DjHkds.FadeData> OFadeData;

		private readonly BDic<DjHkds.STYLE, DjHkds.StyleData> OStyleData;

		private readonly BDic<string, DjHkdsGenerator> OGen;

		public const string gen_random_key = "_random";

		private CsvReader CR;

		private const int STENCIL = 251;

		public int cur_hand;

		private DjHkds.FADE cur_fade;

		private DjHkds.STYLE cur_style;

		private DjHkds.POS cur_pos;

		private EfParticleOnce Epo_hk_appear;

		private readonly TextRenderer TxTitle;

		private const float hk_size = 150f;

		private const float hkh = 75f;

		private const float hk_alpha1 = 0.67f;

		private bool cur_behind;

		private const int HID_NORMAL = 0;

		private const int HID_GRAY = 1;

		private const int HID_WHITE = 2;

		private const int HID_GSHAPE = 3;

		private delegate Vector4 FnGetPos(EffectItem E, float t);

		private delegate void FnDrawWithStyle(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, float alpha);

		private delegate void FnGetFade(MeshDrawer MdB, MeshDrawer Md, EffectItem E, Vector4 Pos, float tz, ref bool draw_basic, DjHkds.FnDrawWithStyle FD_Draw);

		internal enum POS
		{
			N,
			L,
			BL,
			TR,
			RB,
			BH_BIG,
			METEOR,
			JUMP,
			ASIDE_B,
			ASIDE_R,
			ASIDE_T,
			FALL,
			_MAX,
			_UNPREPARED
		}

		private struct PosData
		{
			public PosData(DjHkds.FnGetPos _Fn)
			{
				this.diff = 0f;
				this.Fn = _Fn;
				this.is_behind = false;
			}

			public float diff;

			public DjHkds.FnGetPos Fn;

			public bool is_behind;
		}

		internal enum FADE
		{
			N,
			FADEIN,
			ZOOMIN,
			WIPEUP,
			SYASEN,
			POINT_TX_R,
			POINT_TX_L,
			_MAX
		}

		private struct FadeData
		{
			public FadeData(DjHkds.FnGetFade _Fn)
			{
				this.diff = 0f;
				this.Fn = _Fn;
				this.is_stencil = false;
				this.is_behind = false;
				this.ignore_pos = false;
				this.ignore_draw = false;
			}

			public float diff;

			public DjHkds.FnGetFade Fn;

			public bool is_behind;

			public bool is_stencil;

			public bool ignore_pos;

			public bool ignore_draw;
		}

		internal enum STYLE
		{
			N,
			G2N,
			G,
			ROT180,
			SHAPE,
			GSHAPE,
			COLOR,
			ROTATING,
			_MAX
		}

		private struct StyleData
		{
			public StyleData(DjHkds.FnDrawWithStyle _Fn)
			{
				this.diff = 0f;
				this.Fn = _Fn;
			}

			public float diff;

			public DjHkds.FnDrawWithStyle Fn;
		}
	}
}

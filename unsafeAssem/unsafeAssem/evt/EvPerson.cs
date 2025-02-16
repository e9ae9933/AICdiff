using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using PixelLiner;
using UnityEngine;
using XX;

namespace evt
{
	public sealed class EvPerson : IIdvName
	{
		public EvPerson(string _key, string _talker_name)
		{
			this.key = _key;
			this.talker_name_first = _talker_name;
			this.talker_name = _talker_name;
			this.index = (byte)((EvPerson.OPerson != null) ? EvPerson.OPerson.Count : 0);
			this.localized_name_ = null;
			this.APose = new List<EvEmotVisibility>();
		}

		public string localized_name
		{
			get
			{
				if (this.localized_name_ == null)
				{
					this.localized_name_ = TX.Get("Talker_" + this.talker_name, "");
				}
				return this.localized_name_;
			}
		}

		public bool emot_prepared
		{
			get
			{
				return this.APose.Count != 0;
			}
		}

		public void activate(string s)
		{
		}

		public bool setGrp(EvDrawer Drw, string s, out EvEmotVisibility NextPose, out string next_emotion)
		{
			NextPose = null;
			next_emotion = null;
			if (!REG.match(s, EvPerson.RegPicDirAndAM))
			{
				return false;
			}
			string text = REG.rightContext;
			string r = REG.R1;
			string text2 = r + REG.R2;
			string text3 = REG.R2 + "__" + REG.rightContext;
			EvEmotVisibility evEmotVisibility = this.getPoseByName(text2);
			if (evEmotVisibility == null)
			{
				return false;
			}
			bool flag = Drw.checkTempTermReplace(ref text3);
			flag = this.checkTermReplace(evEmotVisibility.SoursePose, ref text3) || flag;
			if (flag && REG.match(text3, EvPerson.RegPicDirAndAMWithoutSlash))
			{
				string text4 = r + REG.R1;
				string rightContext = REG.rightContext;
				evEmotVisibility = this.getPoseByName(text4);
				if (evEmotVisibility == null)
				{
					return false;
				}
				text = rightContext;
			}
			NextPose = evEmotVisibility;
			next_emotion = text;
			return true;
		}

		public bool run()
		{
			return true;
		}

		public EvPerson release()
		{
			if (this.talker_name != this.talker_name_first)
			{
				this.talker_name = this.talker_name_first;
				this.localized_name_ = null;
			}
			return null;
		}

		public void releaseOnEvInit()
		{
			this.clearTermCache();
		}

		public void clearTermCache()
		{
			if (this.ATerm != null)
			{
				for (int i = this.ATerm.Count - 1; i >= 0; i--)
				{
					this.ATerm[i].term_cache = 2;
				}
			}
			this.ATermTemp = null;
		}

		public RenderTexture prepareTextureAndEmotion(RenderTexture Tx, MeshDrawer BufMd, EvEmotVisibility CurPose, string cur_emotion, float scale)
		{
			BufMd.activate(this.key, EvPerson.BufMtr, true, MTRX.ColWhite, null);
			BufMd.initForImgAndTexture(CurPose.texture);
			CurPose.drawTo(BufMd, 0f, 0f, scale, cur_emotion, false);
			Graphics.SetRenderTarget(Tx);
			BLIT.RenderToGLMtr(BufMd, 0f, 0f, 1f, EvPerson.BufMtr, BLIT.getMatrixForImage(Tx, 1f), BufMd.getTriMax(), false, false);
			Graphics.SetRenderTarget(null);
			return Tx;
		}

		public void drawTo(TalkDrawer Drwr, MeshDrawer Md, ref EvEmotVisibility CurPose, ref string cur_emotion, EvEmotVisibility NextPose, string next_emotion)
		{
			RenderTexture renderTexture;
			if (CurPose != NextPose || cur_emotion != next_emotion)
			{
				CurPose = NextPose;
				if (CurPose == null)
				{
					return;
				}
				int num = (int)((CurPose.swidth_px + 0f) * 1f);
				int num2 = (int)((CurPose.sheight_px + 0f) * 1f);
				cur_emotion = next_emotion;
				renderTexture = Drwr.PrepareBufferTx(num, num2, true);
				this.prepareTextureAndEmotion(renderTexture, Drwr.getBufferStaticMesh(), CurPose, cur_emotion, 1f);
			}
			else
			{
				if (CurPose == null)
				{
					return;
				}
				int num = (int)((CurPose.swidth_px + 0f) * 1f);
				int num2 = (int)((CurPose.sheight_px + 0f) * 1f);
				renderTexture = Drwr.PrepareBufferTx(num, num2, false);
			}
			if (Md != null)
			{
				this.DrawerMtr = Drwr.getImageMaterial();
				if (Md.getMaterial() != this.DrawerMtr)
				{
					Drwr.changeMMRDMaterial(Md, this.DrawerMtr);
				}
				Md.initForImgAndTexture(renderTexture);
				Md.RotaGraph(Drwr.img_center_shift_x, Drwr.img_center_shift_y, CurPose.draw_scale, 0f, null, false);
			}
		}

		public void cacheGraphics()
		{
			this.Loader.preparePxlChar(true, true);
		}

		internal void initPxEmot(PxlCharacter Pc)
		{
			for (int i = Pc.countPoses() - 1; i >= 0; i--)
			{
				PxlPose pose = Pc.getPose(i);
				if (pose.title.IndexOf("_") != 0)
				{
					int count = this.APose.Count;
					uint num = 8U;
					int num2 = 0;
					while ((long)num2 < (long)((ulong)num))
					{
						if (pose.isValidAim(num2) && !pose.isFlipped(num2))
						{
							this.initPxEmot(pose.getSequence(num2));
						}
						num2++;
					}
					int count2 = this.APose.Count;
					for (int j = count + 1; j < count2; j++)
					{
						this.APose[j].face_y = this.APose[count].face_y;
					}
					if (TX.valid(pose.comment))
					{
						CsvReader csvReader = new CsvReader(pose.comment, CsvReader.RegSpace, false);
						while (csvReader.read())
						{
							string cmd = csvReader.cmd;
							if (cmd != null)
							{
								if (!(cmd == "scale"))
								{
									if (!(cmd == "shift_y"))
									{
										if (!(cmd == "shift_y_set"))
										{
											if (!(cmd == "shift_x"))
											{
												if (!(cmd == "shift_x_onright"))
												{
													if (cmd == "auto_replace")
													{
														this.addReplaceTerm(pose, csvReader._1, csvReader._2, csvReader.slice_join(3, " ", ""));
													}
												}
												else
												{
													float num3 = csvReader._N1;
													for (int k = count; k < count2; k++)
													{
														this.APose[k].shift_x_onright = num3;
													}
												}
											}
											else
											{
												float num3 = csvReader._N1;
												for (int l = count; l < count2; l++)
												{
													this.APose[l].shift_x = num3;
												}
											}
										}
										else
										{
											float num3 = csvReader._N1;
											for (int m = count; m < count2; m++)
											{
												this.APose[m].shift_y = num3;
											}
										}
									}
									else
									{
										float num3 = csvReader._N1;
										for (int n = count; n < count2; n++)
										{
											this.APose[n].shift_y += num3;
										}
									}
								}
								else
								{
									float num3 = csvReader._N1 / 100f;
									for (int num4 = count; num4 < count2; num4++)
									{
										this.APose[num4].draw_scale = num3;
									}
								}
							}
						}
					}
				}
			}
		}

		private void initPxEmot(PxlSequence P)
		{
			int num = P.countFrames();
			for (int i = 0; i < num; i++)
			{
				this.initPxEmot(i, P.getFrame(i));
			}
		}

		public void initPxEmot(int frm_index, PxlFrame F)
		{
			BDic<string, List<int>> olaBuf = EvPerson.OLaBuf;
			BDic<string, List<int>> oraBuf = EvPerson.ORaBuf;
			olaBuf.Clear();
			oraBuf.Clear();
			if (EvPerson.OMem == null)
			{
				EvPerson.OMem = new BDic<string, EvPerson.EmotMem>(32);
			}
			int num = F.countLayers();
			if (num >= 64)
			{
				X.de(F.ToString() + ": 立ち絵設定PxLFrameが64枚以上のレイヤーがあると正しく動作しない（フレームを分けて対処せよ）", null);
			}
			if (F.name != "" && F.name.IndexOf("__") >= 0)
			{
				X.de(F.ToString() + ": 腕差分フレームの名前にアンダースコアを2個続けてはいけない", null);
			}
			uint num2 = 0U;
			uint num3 = 0U;
			BDic<int, uint> bdic = null;
			List<EvPerson.EmotImportPose> list = null;
			List<int> list2 = null;
			string text = "";
			int num4 = 0;
			uint num5 = 0U;
			bool flag = false;
			int i = 0;
			while (i < num)
			{
				PxlLayer layer = F.getLayer(i);
				string text2 = layer.name;
				bool flag2 = layer.alpha > 0f;
				int num6 = -1;
				if (TX.isEnd(text2, "__DONT_APPEAR_EDITOR__"))
				{
					flag = true;
				}
				if (REG.match(text2, EvPerson.RegFacePat))
				{
					text2 = REG.R1 + REG.rightContext;
					int num7;
					num6 = (num7 = X.NmI(REG.R2, 0, false, false));
					num3 |= 1U << i;
					if (bdic == null)
					{
						bdic = new BDic<int, uint>();
					}
					if (bdic.ContainsKey(num7))
					{
						BDic<int, uint> bdic2 = bdic;
						int num8 = num7;
						bdic2[num8] |= 1U << i;
					}
					else
					{
						bdic[num7] = 1U << i;
					}
					flag2 = false;
				}
				if (text2.IndexOf("m") == 0 && (text2 == "m" || REG.match(text2, EvPerson.RegNameForEmot)) && layer.isImport())
				{
					PxlLayer importSource = layer.getImportSource();
					if (importSource != null && importSource != layer)
					{
						if (list == null)
						{
							list = new List<EvPerson.EmotImportPose>(1);
							list2 = new List<int>(1);
						}
						PxlPose pxlPose = importSource.pFrm.pPose;
						for (int j = list.Count - 1; j >= 0; j--)
						{
							if (list[j].Pose == pxlPose)
							{
								X.de(F.ToString() + ": 同じポーズからインポートしている表情差分レイヤーを複数配置することはできない", null);
								pxlPose = null;
								break;
							}
						}
						if (pxlPose != null)
						{
							list.Add(new EvPerson.EmotImportPose(pxlPose, -importSource.x, importSource.y));
							text = TX.add(text, pxlPose.title, ",");
							list2.Add(num6);
							num4 += pxlPose.countFrames(false);
							num2 |= 1U << i;
							flag2 = num6 == -1;
							goto IL_0313;
						}
					}
				}
				else
				{
					if (REG.match(text2, EvPerson.RegLa))
					{
						string r = REG.R1;
						List<int> list3 = X.Get<string, List<int>>(olaBuf, r);
						if (list3 == null)
						{
							list3 = (olaBuf[r] = new List<int>());
						}
						list3.Add(i);
						flag2 = false;
						goto IL_0313;
					}
					if (REG.match(text2, EvPerson.RegRa))
					{
						string r2 = REG.R1;
						List<int> list4 = X.Get<string, List<int>>(oraBuf, r2);
						if (list4 == null)
						{
							list4 = (oraBuf[r2] = new List<int>());
						}
						list4.Add(i);
						flag2 = false;
						goto IL_0313;
					}
					goto IL_0313;
				}
				IL_0324:
				i++;
				continue;
				IL_0313:
				if (flag2)
				{
					num5 |= 1U << (int)((byte)i);
					goto IL_0324;
				}
				goto IL_0324;
			}
			string text3 = F.pPose.title + "/" + ((F.name.IndexOf("a") == 0) ? F.name : ("a" + frm_index.ToString()));
			EvPerson.EmotLayer[] array = null;
			EvPerson.EmotImportPose[] array2 = null;
			if (text != "")
			{
				text = F.pChar.title + "#" + text;
				EvPerson.EmotMem emotMem = X.Get<string, EvPerson.EmotMem>(EvPerson.OMem, text);
				if (emotMem != null)
				{
					array = emotMem.AEmotions;
					array2 = emotMem.AEmotPose;
				}
				else
				{
					if (list != null)
					{
						List<EvPerson.EmotLayer> list5 = new List<EvPerson.EmotLayer>(num4);
						num = list.Count;
						for (int k = 0; k < num; k++)
						{
							int num9 = list2[k];
							EvPerson.getPxEmotPose(list5, list[k].Pose, (num9 == -1) ? "" : ("F" + num9.ToString() + "__"));
						}
						array = list5.ToArray();
					}
					array2 = ((list != null) ? list.ToArray() : null);
					EvPerson.OMem[text] = new EvPerson.EmotMem
					{
						AEmotions = array,
						AEmotPose = array2
					};
				}
			}
			if (olaBuf.Count > 0 || oraBuf.Count > 0)
			{
				using (BList<string> blist = X.objKeysB<string, List<int>>(olaBuf))
				{
					using (BList<string> blist2 = X.objKeysB<string, List<int>>(oraBuf))
					{
						int count = blist.Count;
						int count2 = blist2.Count;
						for (int l = ((count == 0) ? (-1) : 0); l < count; l++)
						{
							uint num10 = num5;
							if (l >= 0)
							{
								List<int> list6 = olaBuf[blist[l]];
								for (int m = list6.Count - 1; m >= 0; m--)
								{
									num10 |= 1U << (int)((byte)list6[m]);
								}
							}
							for (int n = ((count2 == 0) ? (-1) : 0); n < count2; n++)
							{
								uint num11 = num10;
								if (n >= 0)
								{
									List<int> list7 = oraBuf[blist2[n]];
									for (int num12 = list7.Count - 1; num12 >= 0; num12--)
									{
										num11 |= 1U << (int)((byte)list7[num12]);
									}
								}
								this.APose.Add(new EvEmotVisibility(text3 + ((l >= 0) ? ("L" + blist[l].ToUpper()) : "LL") + ((n >= 0) ? ("R" + blist2[n].ToUpper()) : "RR"), F, num11, num2, array, array2, bdic, flag));
							}
						}
						return;
					}
				}
			}
			this.APose.Add(new EvEmotVisibility(text3, F, num5, num2, array, array2, bdic, flag));
		}

		private static void getPxEmotPose(List<EvPerson.EmotLayer> AEmot, PxlPose P, string prefix)
		{
			uint num = 8U;
			string text = "__";
			if (TX.valid(P.comment) && REG.match(P.comment, EvPerson.RegLayerSplitter))
			{
				text = REG.R1;
			}
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				if (P.isValidAim(num2) && !P.isFlipped(num2))
				{
					EvPerson.getPxEmotPose(AEmot, P.getSequence(num2), prefix, text);
				}
				num2++;
			}
		}

		private static void getPxEmotPose(List<EvPerson.EmotLayer> AEmot, PxlSequence P, string prefix, string splitter)
		{
			int num = P.countFrames();
			for (int i = 0; i < num; i++)
			{
				PxlFrame frame = P.getFrame(i);
				if (!TX.isStart(frame.name, "__", 0))
				{
					EvPerson.EmotLayer emotLayer = new EvPerson.EmotLayer(frame, null, prefix, splitter);
					AEmot.Add(emotLayer);
				}
			}
		}

		public string getIdentifier(EvEmotVisibility Pose, string face_emotion)
		{
			return Pose.name + "__" + face_emotion;
		}

		public List<EvEmotVisibility> getPoseList()
		{
			return this.APose;
		}

		public EvEmotVisibility getPoseByName(string s)
		{
			for (int i = this.APose.Count - 1; i >= 0; i--)
			{
				EvEmotVisibility evEmotVisibility = this.APose[i];
				if (evEmotVisibility.name == s)
				{
					return evEmotVisibility;
				}
			}
			return null;
		}

		public EvPerson addReplaceTerm(PxlPose P, string check_expression, string repl, string term)
		{
			if (this.ATerm == null)
			{
				this.ATerm = new List<EvPerson.EmotReplaceTerm>(1);
			}
			this.ATerm.Add(new EvPerson.EmotReplaceTerm(P, check_expression, repl, term));
			return this;
		}

		public bool checkTermReplace(PxlPose Target, ref string s)
		{
			if (this.ATerm == null)
			{
				return false;
			}
			bool flag = false;
			for (int i = this.ATerm.Count - 1; i >= 0; i--)
			{
				EvPerson.EmotReplaceTerm emotReplaceTerm = this.ATerm[i];
				if (emotReplaceTerm.isUseable(Target) && REG.match(s, emotReplaceTerm.check))
				{
					s = REG.ReplaceExpression(emotReplaceTerm.repl);
					flag = true;
				}
			}
			return flag;
		}

		internal static void loadPerson(string _LT, ref EvPerson.EvPxlsLoader[] APxls_ta)
		{
			if (EvPerson.BufMtr == null)
			{
				EvPerson.BufMtr = MTRX.newMtr(MTRX.ShaderGDT);
				EvPerson.BufMtr.EnableKeyword("NO_PIXELSNAP");
			}
			EvPerson.OPerson = new NDic<EvPerson>("_WHOLE_PERSON", 0, 0);
			CsvReader csvReader = new CsvReader(_LT, CsvReader.RegSpace, true);
			EvPerson evPerson = null;
			List<EvPerson.EvPxlsLoader> list = new List<EvPerson.EvPxlsLoader>();
			while (csvReader.read())
			{
				string cmd = csvReader.cmd;
				if (cmd != null)
				{
					if (!(cmd == "%PXL") && !(cmd == "%PXL_PERSON"))
					{
						if (cmd == "%COLICO")
						{
							if (evPerson != null)
							{
								evPerson.personal_color = X.NmUI(csvReader._1, uint.MaxValue, true, true);
								evPerson.SmallIconPF = MTRX.getPF(csvReader._2);
								continue;
							}
							continue;
						}
					}
					else
					{
						if (csvReader.cmd == "%PXL_PERSON")
						{
							if (evPerson == null)
							{
								csvReader.tError("%PXL_PERSON は Person 定義後に指定する");
								continue;
							}
							if (evPerson.Loader != null)
							{
								csvReader.tError("%PXL_PERSON の Loader 定義は1度だけ実行する");
								continue;
							}
						}
						bool flag = csvReader.Nm(2, 0f) != 0f;
						EvPerson.EvPxlsLoader evPxlsLoader = new EvPerson.EvPxlsLoader((csvReader.cmd == "%PXL") ? "" : evPerson.key, csvReader._1, !flag);
						list.Add(evPxlsLoader);
						evPxlsLoader.preparePxlChar(!flag || !evPxlsLoader.is_person, false);
						if (csvReader.cmd == "%PXL_PERSON")
						{
							evPerson.Loader = evPxlsLoader;
							continue;
						}
						continue;
					}
				}
				if (!TX.isStart(csvReader.cmd, "%", 0))
				{
					if (csvReader.cmd == "_")
					{
						EvPerson.talker_name_default = csvReader._1;
					}
					evPerson = EvPerson.getPerson(csvReader.cmd, csvReader._1);
					if (csvReader._2 == "_")
					{
						evPerson.talk_snd = "";
					}
					else if (csvReader._2 != "")
					{
						evPerson.talk_snd = csvReader._2;
					}
				}
			}
			EvPerson.OPerson.scriptFinalize();
			APxls_ta = list.ToArray();
		}

		public void addTemporaryTerm(string check_expression, string repl, string term)
		{
			if (this.ATermTemp == null)
			{
				this.ATermTemp = new List<EvPerson.EmotReplaceTerm>(1);
			}
			this.ATermTemp.Add(new EvPerson.EmotReplaceTerm(null, check_expression, repl, term));
		}

		public static EvPerson getPerson(string key, string personname_for_make = null)
		{
			EvPerson evPerson = X.Get<string, EvPerson>(EvPerson.OPerson, key);
			if (evPerson == null)
			{
				if (personname_for_make == null)
				{
					return null;
				}
				evPerson = (EvPerson.OPerson[key] = new EvPerson(key, personname_for_make));
			}
			return evPerson;
		}

		public static EvPerson getPersonByNameDefault(string name_default)
		{
			foreach (KeyValuePair<string, EvPerson> keyValuePair in EvPerson.OPerson)
			{
				if (keyValuePair.Value.talker_name_first == name_default)
				{
					return keyValuePair.Value;
				}
			}
			X.de("不明な EvPerson: " + name_default, null);
			return null;
		}

		public static BDic<string, EvPerson> getPersonDictionary()
		{
			return EvPerson.OPerson;
		}

		public string get_individual_key()
		{
			return this.key;
		}

		public static void fineLocalizedName()
		{
			if (EvPerson.OPerson == null)
			{
				return;
			}
			foreach (KeyValuePair<string, EvPerson> keyValuePair in EvPerson.OPerson)
			{
				keyValuePair.Value.localized_name_ = null;
			}
		}

		public static void evInit()
		{
			if (EvPerson.OPerson == null)
			{
				return;
			}
			foreach (KeyValuePair<string, EvPerson> keyValuePair in EvPerson.OPerson)
			{
				keyValuePair.Value.releaseOnEvInit();
			}
		}

		public bool loaderIs(EvPerson.EvPxlsLoader _Loader)
		{
			return this.Loader == _Loader;
		}

		public static byte[] getPngBytesS(RenderTexture Tx)
		{
			Texture2D texture2D = new Texture2D(Tx.width, Tx.height, TextureFormat.ARGB32, false);
			RenderTexture.active = Tx;
			texture2D.ReadPixels(new Rect(0f, 0f, (float)Tx.width, (float)Tx.height), 0, 0);
			texture2D.Apply();
			byte[] array = texture2D.EncodeToPNG();
			global::UnityEngine.Object.Destroy(texture2D);
			return array;
		}

		public readonly string key = "";

		private static string talker_name_default = "＊";

		public string talker_name = EvPerson.talker_name_default;

		public readonly string talker_name_first;

		public string show_type = "";

		public string talk_snd = "talk_progress";

		private static readonly Regex RegFacePat = new Regex("^(m[a-zA-Z0-9]*_)?f(\\d+)", RegexOptions.IgnoreCase);

		private static readonly Regex RegLa = new Regex("^la(\\d+)", RegexOptions.IgnoreCase);

		private static readonly Regex RegRa = new Regex("^ra(\\d+)", RegexOptions.IgnoreCase);

		private static readonly Regex RegNameForEmot = new Regex("^m[_\\d]");

		public static readonly Regex RegFacePrefix = new Regex("^F(\\d+)__");

		public static readonly Regex RegPicDirAndAM = new Regex("^([^\\/]+\\/)(a(?:(?!__)[a-zA-Z0-9_])+)__");

		public static readonly Regex RegPicDirAndAMWithoutSlash = new Regex("^(a(?:(?!__)[a-zA-Z0-9_])+)__");

		public static readonly Regex RegLayerSplitter = new Regex("\\blayer_splitter[ \\s\\t]+([^\\n \\s\\t\\r\\/]+)");

		public const string headerDontAppearEditor = "__DONT_APPEAR_EDITOR__";

		public List<EvPerson.EmotReplaceTerm> ATermTemp;

		private readonly List<EvEmotVisibility> APose;

		private List<EvPerson.EmotReplaceTerm> ATerm;

		private static Material BufMtr;

		private Material DrawerMtr;

		private EvPerson.EvPxlsLoader Loader;

		public readonly byte index;

		private string localized_name_;

		public uint personal_color;

		public PxlFrame SmallIconPF;

		private static readonly BDic<string, List<int>> OLaBuf = new BDic<string, List<int>>();

		private static readonly BDic<string, List<int>> ORaBuf = new BDic<string, List<int>>();

		private static BDic<string, EvPerson.EmotMem> OMem;

		private static NDic<EvPerson> OPerson;

		public sealed class EmotImportPose
		{
			public EmotImportPose(PxlPose _Pose, float _shiftx, float _shifty)
			{
				this.Pose = _Pose;
				this.shiftx = _shiftx;
				this.shifty = _shifty;
			}

			public override string ToString()
			{
				return string.Concat(new string[]
				{
					this.Pose.ToString(),
					"(",
					this.shiftx.ToString(),
					",",
					this.shifty.ToString(),
					")"
				});
			}

			public readonly PxlPose Pose;

			public readonly float shiftx;

			public readonly float shifty;
		}

		public sealed class EmotReplaceTerm
		{
			public EmotReplaceTerm(PxlPose _Target, string _check_express, string _repl, string _term)
			{
				this.Target = _Target;
				this.check = new Regex(_check_express.Replace("¥", "\\"));
				this.repl = _repl;
				this.term = _term;
				if (!TX.valid(this.term))
				{
					X.de("EmotReplaceTerm は条件の設定が必要です", null);
					this.term = "0";
				}
			}

			public bool term_calced
			{
				get
				{
					return this.term_cache >= 2;
				}
			}

			public bool useable
			{
				get
				{
					if (this.term_cache >= 2)
					{
						this.term_cache = ((TX.evalI(this.term) != 0) ? 1 : 0);
					}
					return this.term_cache > 0;
				}
			}

			public bool isUseable(PxlPose _Target)
			{
				return (this.Target == null || _Target == this.Target) && this.useable;
			}

			public static bool checkTermReplace(List<EvPerson.EmotReplaceTerm> ATerm, ref string s)
			{
				if (ATerm == null)
				{
					return false;
				}
				bool flag = false;
				for (int i = ATerm.Count - 1; i >= 0; i--)
				{
					EvPerson.EmotReplaceTerm emotReplaceTerm = ATerm[i];
					if (emotReplaceTerm.useable && REG.match(s, emotReplaceTerm.check))
					{
						s = REG.ReplaceExpression(emotReplaceTerm.repl);
						flag = true;
					}
				}
				return flag;
			}

			public readonly Regex check;

			public readonly string repl;

			public readonly string term;

			public readonly PxlPose Target;

			public byte term_cache = 2;
		}

		public sealed class EmotLayer
		{
			public EmotLayer(PxlFrame _F, string name, string prefix, string layer_splitter)
			{
				this.F = _F;
				if (name == null)
				{
					if (this.F.name != "")
					{
						name = this.F.name;
					}
					else
					{
						int num = _F.countLayers();
						name = "";
						for (int i = 0; i < num; i++)
						{
							name = TX.add(name, _F.getLayer(i).name, layer_splitter);
						}
					}
					if (TX.valid(prefix))
					{
						name = prefix + name;
					}
				}
				this.key = name;
			}

			public readonly PxlFrame F;

			public readonly string key;
		}

		private class EmotMem
		{
			public EvPerson.EmotImportPose[] AEmotPose;

			public EvPerson.EmotLayer[] AEmotions;
		}

		public class EvPxlsLoader
		{
			public EvPxlsLoader(string _person_key, string _pxl_name, bool _is_static)
			{
				this.person_key = _person_key;
				this.pxl_name = _pxl_name;
				this.ticket_loading_value = (_is_static ? EvPerson.EvPxlsLoader.TLD._STATIC : ((EvPerson.EvPxlsLoader.TLD)0));
				this.pxl_ipath = "EvImg/" + this.pxl_name + ".pxls.bytes.texture_0";
			}

			public EvPxlsLoader(string _person_key, PxlCharacter _Pc)
			{
				this.person_key = _person_key;
				this.Pc = _Pc;
				this.pxl_name = this.Pc.title;
				this.ticket_loading_value = EvPerson.EvPxlsLoader.TLD._STATIC;
				this.pxl_ipath = "EvImg/" + this.pxl_name + ".pxls.bytes.texture_0";
			}

			public bool is_static
			{
				get
				{
					return (this.ticket_loading_value & EvPerson.EvPxlsLoader.TLD._STATIC) > (EvPerson.EvPxlsLoader.TLD)0;
				}
			}

			public void setStaticFlagForDebugger()
			{
				this.ticket_loading_value |= EvPerson.EvPxlsLoader.TLD._STATIC;
			}

			public PxlCharacter preparePxlChar(bool pc_load_flag, bool image_load_flag)
			{
				string text = X.basename(this.pxl_name);
				this.Pc = PxlsLoader.getPxlCharacter(text);
				if (this.Pc == null && pc_load_flag)
				{
					using (MTI mti = new MTI("EvImg/" + this.pxl_name + ".pxls", "_"))
					{
						this.Pc = PxlsLoader.loadCharacterASync(text, mti.LoadBytes(this.pxl_name + ".pxls"), null, 64f, false);
						if (this.Pc == null)
						{
							X.de("不明な pxl パス: EvImg/" + this.pxl_name, null);
						}
					}
				}
				if (this.Pc != null && (this.ticket_loading_value & EvPerson.EvPxlsLoader.TLD._LOADING_BITS) < EvPerson.EvPxlsLoader.TLD._LOADING_BITS)
				{
					this.Pc.no_load_external_texture_on_first = true;
					bool flag = !this.Pc.isLoadCompleted();
					bool flag2 = false;
					if (!this.image_loaded && image_load_flag && MTI.LoadContainerOneImage(this.pxl_ipath, null, null).addLoadKey("EV", true))
					{
						flag2 = true;
					}
					if (flag || flag2)
					{
						EvPerson.EvPxlsLoader.TLD tld = (image_load_flag ? EvPerson.EvPxlsLoader.TLD._LOADING_BITS : EvPerson.EvPxlsLoader.TLD.DATA);
						if ((this.ticket_loading_value & EvPerson.EvPxlsLoader.TLD._LOADING_BITS) < tld)
						{
							this.ticket_loading_value |= tld;
							EV.initLoadTicket("PXL", flag2 ? "DI" : "D", EvPerson.EvPxlsLoader.FD_LoadPxl, this, 0);
						}
					}
				}
				return this.Pc;
			}

			public void releasePxlImage()
			{
				if (this.image_loaded)
				{
					this.image_loaded = false;
					MTI.ReleaseContainer(this.pxl_ipath, "EV");
					if (this.Pc != null)
					{
						this.Pc.releaseLoadedExternalTexture(false, null);
					}
					LoadTicketManager.destructTarget(this);
					this.ticket_loading_value &= ~(EvPerson.EvPxlsLoader.TLD.DATA | EvPerson.EvPxlsLoader.TLD.IMG);
				}
			}

			public bool is_person
			{
				get
				{
					return this.person_key != "";
				}
			}

			public readonly string person_key;

			public readonly string pxl_name;

			public readonly string pxl_ipath;

			private EvPerson.EvPxlsLoader.TLD ticket_loading_value;

			public PxlCharacter Pc;

			public bool image_loaded;

			private const string TLOAD_NAME_PXL = "PXL";

			private const string TLOAD_NAME2_PXL_DATAONLY = "D";

			private const string TLOAD_NAME2_PXL_DATA_AND_IMG = "DI";

			private static LoadTicketManager.FnLoadProgress FD_LoadPxl = delegate(LoadTicketManager.LoadTicket Tk)
			{
				EvPerson.EvPxlsLoader evPxlsLoader = Tk.Target as EvPerson.EvPxlsLoader;
				if (!evPxlsLoader.Pc.isLoadCompleted())
				{
					return true;
				}
				if (Tk.name2 == "DI" && !evPxlsLoader.image_loaded)
				{
					MTIOneImage mtioneImage = MTI.LoadContainerOneImage(evPxlsLoader.pxl_ipath, null, null);
					if (mtioneImage.addLoadKey("EV", true) || !mtioneImage.isAsyncLoadFinished())
					{
						return true;
					}
					evPxlsLoader.image_loaded = true;
					evPxlsLoader.Pc.ReplaceExternalPng(new Texture[] { mtioneImage.Image }, true);
					MTRX.assignMI(evPxlsLoader.Pc, mtioneImage.MI);
					if (evPxlsLoader.is_person)
					{
						EvPerson person = EvPerson.getPerson(evPxlsLoader.person_key, null);
						if (person != null && !person.emot_prepared)
						{
							person.initPxEmot(evPxlsLoader.Pc);
						}
					}
				}
				return false;
			};

			[Flags]
			private enum TLD : byte
			{
				DATA = 1,
				IMG = 2,
				_LOADING_BITS = 3,
				_STATIC = 4
			}
		}
	}
}

using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

namespace XX
{
	public static class SND
	{
		public static void initSoundList()
		{
			SND.OOCue2Acb = new BDic<string, BDic<string, SND.CueHandler>>();
			BDic<string, CriAtom> bdic = new BDic<string, CriAtom>();
			List<SND.SndLoader> list = new List<SND.SndLoader>();
			SND.AAtomBehaviour = null;
			SND.OAsameframe_snd = new BDic<string, List<string>>();
			if (X.DEBUGNOSND)
			{
				SND.Ui = new SndPlayerNoUseable("");
				return;
			}
			CsvReaderA csvReaderA = new CsvReaderA(Resources.Load<TextAsset>("Basic/Data/_snd").text, true);
			SND.SndLoader sndLoader = null;
			CriAtom criAtom = null;
			CriWareInitializer component = IN._stage.GetComponent<CriWareInitializer>();
			try
			{
				component.Initialize();
			}
			catch (Exception ex)
			{
				component.enabled = false;
				X.DEBUGNOSND = true;
				string text = ex.ToString().Replace(ex.StackTrace, "").Replace("\n", "");
				X.dl("サウンドドライバがありません: " + text, null, false, true);
				SND.Ui = new SndPlayerNoUseable(text);
				SND.OOCue2Acb = null;
				return;
			}
			Object.DontDestroyOnLoad(CriAtomServer.instance.gameObject);
			List<string> list2 = new List<string>(6);
			while (csvReaderA.read())
			{
				if (csvReaderA.cmd == "%ADX_INIT")
				{
					if (csvReaderA.clength < 3)
					{
						X.de("%ADX_INIT は引数を省略できません", null);
					}
					else
					{
						criAtom = X.Get<string, CriAtom>(bdic, csvReaderA._2);
						if (criAtom == null)
						{
							GameObject gameObject = new GameObject((SND.AAtomBehaviour == null) ? "CRIWARE_Atom" : ("CRIWARE-" + csvReaderA._2));
							gameObject.transform.SetParent(IN._stage.transform);
							gameObject.SetActive(false);
							criAtom = (bdic[csvReaderA._2] = gameObject.AddComponent<CriAtom>());
							criAtom.acfFile = csvReaderA._1;
							criAtom.dspBusSetting = csvReaderA._2;
							gameObject.SetActive(true);
							if (SND.AAtomBehaviour == null)
							{
								SND.AAtomBehaviour = new CriAtom[] { criAtom };
							}
							else
							{
								X.push<CriAtom>(ref SND.AAtomBehaviour, criAtom, -1);
							}
						}
					}
				}
				else if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					if (sndLoader != null)
					{
						sndLoader.initSheetFinalize(list2);
					}
					string cur_timing = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
					sndLoader = list.Find((SND.SndLoader V) => V.key == cur_timing);
					if (sndLoader != null)
					{
						X.de("サウンドタイミング重複: " + cur_timing, null);
					}
					else
					{
						list.Add(sndLoader = new SND.SndLoader(cur_timing));
						sndLoader.Atom = criAtom;
						if (TX.isStart(cur_timing, "BGM_", 0))
						{
							sndLoader.header = "BGM.";
						}
					}
					list2.Clear();
				}
				else if (sndLoader != null)
				{
					if (criAtom == null)
					{
						X.de("%ADX_INIT を最初によぶこと", null);
						break;
					}
					if (csvReaderA.cmd == "%HEADER")
					{
						sndLoader.header = csvReaderA._1;
					}
					else if (csvReaderA.cmd == "%LOAD_AWB")
					{
						sndLoader.use_awb = true;
					}
					else
					{
						list2.Add(csvReaderA.cmd);
					}
				}
			}
			criAtom = null;
			if (sndLoader != null)
			{
				sndLoader.initSheetFinalize(list2);
			}
			SND.ALoader = list.ToArray();
			if (X.DEBUGNOSND)
			{
				SND.Ui = new SndPlayer("Ui", SndPlayer.SNDTYPE.SND);
			}
			SND.AInitializeSoundSheet = new List<SND.InitSndLoad>();
			bool debugannounce = X.DEBUGANNOUNCE;
			if (X.DEBUG && debugannounce)
			{
				IN._stage.gameObject.AddComponent<CriWareErrorHandler>();
				CriWareErrorHandler.OnCallback += delegate(string _s)
				{
					if (_s.IndexOf("Beat Sync") >= 0 || _s.IndexOf("E2010021545:Invalid parameter") >= 0)
					{
						return;
					}
					if (_s.IndexOf("Cue Link Callbacks have been overflowed") >= 0)
					{
						return;
					}
					if (_s.IndexOf("max_pitch") >= 0)
					{
						return;
					}
					X.de("CRIWARE error: " + _s, null);
				};
			}
		}

		public static void sndInitPost()
		{
			BGM.initBgm();
			if (SND.AInitializeSoundSheet == null)
			{
				return;
			}
			List<SND.InitSndLoad> ainitializeSoundSheet = SND.AInitializeSoundSheet;
			SND.AInitializeSoundSheet = null;
			int count = ainitializeSoundSheet.Count;
			for (int i = 0; i < count; i++)
			{
				SND.InitSndLoad initSndLoad = ainitializeSoundSheet[i];
				SND.loadSheets(initSndLoad.timing, initSndLoad.load_key);
			}
		}

		public static string isSndErrorOccured(bool clear_error_state_key = false)
		{
			if (SND.OOCue2Acb == null && SND.Ui is SndPlayerNoUseable)
			{
				string key = SND.Ui.key;
				if (clear_error_state_key && key != "")
				{
					SND.Ui = new SndPlayerNoUseable("");
				}
				return key;
			}
			return null;
		}

		public static string[] loadSheets(string timing, string load_key = "_")
		{
			if (X.DEBUGNOSND)
			{
				return null;
			}
			if (SND.AInitializeSoundSheet != null)
			{
				SND.AInitializeSoundSheet.Add(new SND.InitSndLoad(timing, load_key));
				return null;
			}
			for (int i = SND.ALoader.Length - 1; i >= 0; i--)
			{
				SND.SndLoader sndLoader = SND.ALoader[i];
				if (sndLoader.key == timing)
				{
					sndLoader.Add(load_key);
					return sndLoader.Asheets;
				}
			}
			X.de("サウンドタイミングが存在しません: " + timing, null);
			return null;
		}

		public static void unloadSheets(string timing, string load_key = "_")
		{
			if (X.DEBUGNOSND || SND.Ui == null || SND.OOCue2Acb == null)
			{
				return;
			}
			if (timing == null)
			{
				int num = SND.ALoader.Length;
				for (int i = 0; i < num; i++)
				{
					SND.ALoader[i].Rem(load_key);
				}
				return;
			}
			for (int j = SND.ALoader.Length - 1; j >= 0; j--)
			{
				SND.SndLoader sndLoader = SND.ALoader[j];
				if (sndLoader.key == timing)
				{
					sndLoader.Rem(load_key);
					return;
				}
			}
		}

		private static void unloadAttachedLoader(SND.SndLoader Ld, string _header)
		{
			int i = X.isinC<SND.SndLoader>(SND.ALoader, Ld);
			if (i == -1)
			{
				return;
			}
			BDic<string, SND.CueHandler> bdic;
			if (!SND.OOCue2Acb.TryGetValue(_header, out bdic))
			{
				return;
			}
			List<string> list = new List<string>(64);
			foreach (KeyValuePair<string, SND.CueHandler> keyValuePair in bdic)
			{
				if ((int)keyValuePair.Value.loader_index == i)
				{
					list.Add(keyValuePair.Key);
				}
			}
			for (i = list.Count - 1; i >= 0; i--)
			{
				bdic.Remove(list[i]);
			}
		}

		public static void runSND()
		{
			foreach (KeyValuePair<string, List<string>> keyValuePair in SND.OAsameframe_snd)
			{
				keyValuePair.Value.Clear();
			}
			CriAtom[] aatomBehaviour = SND.AAtomBehaviour;
		}

		public static void flush()
		{
			if (X.DEBUGNOSND || !SND.loaded)
			{
				return;
			}
			int num = SND.ALoader.Length;
			for (int i = 0; i < num; i++)
			{
				if (!SND.ALoader[i].isActive())
				{
					SND.ALoader[i].unload();
				}
			}
		}

		public static bool loaded
		{
			get
			{
				if (!CriAtom.CueSheetsAreLoading)
				{
					if (SND.Ui == null)
					{
						SND.Ui = new SndPlayer("Ui", SndPlayer.SNDTYPE.SND);
					}
					return true;
				}
				return false;
			}
		}

		private static void explodeCueInfo()
		{
			if (!SND.loaded)
			{
				return;
			}
			int num = SND.ALoader.Length;
			for (int i = 0; i < num; i++)
			{
				SND.ALoader[i].explodeCueInfo(i);
			}
		}

		public static void registerCueNameToAcb(string header, string key, int _loader_index, int _sheet_index, CriAtomEx.CueInfo _Info)
		{
			BDic<string, SND.CueHandler> bdic;
			if (!SND.OOCue2Acb.TryGetValue(header, out bdic))
			{
				bdic = (SND.OOCue2Acb[header] = new BDic<string, SND.CueHandler>());
			}
			SND.CueHandler cueHandler;
			if (bdic.TryGetValue(key, out cueHandler))
			{
				if ((int)cueHandler.loader_index != _loader_index || (int)cueHandler.sheet_index != _sheet_index)
				{
					X.de("同じ名前のキューが存在します: " + key, null);
				}
				return;
			}
			bdic[key] = new SND.CueHandler(_loader_index, _sheet_index, _Info);
		}

		public static CriAtomEx.CueInfo[] getSheetInfo(string sheet_name)
		{
			if (X.DEBUGNOSND)
			{
				return null;
			}
			if (SND.need_explode_cue_info > 0)
			{
				SND.explodeCueInfo();
			}
			CriAtomExAcb acb = CriAtom.GetAcb(sheet_name);
			if (acb == null)
			{
				return null;
			}
			return acb.GetCueInfoList();
		}

		public static bool GetCue(string cue_name, SndPlayer MyPlayer)
		{
			return SND.GetCue("", cue_name, MyPlayer);
		}

		public static bool GetCue(string header, string cue_name, SndPlayer MyPlayer)
		{
			if (X.DEBUGNOSND)
			{
				return false;
			}
			if (SND.need_explode_cue_info > 0)
			{
				SND.explodeCueInfo();
			}
			BDic<string, SND.CueHandler> bdic;
			SND.CueHandler cueHandler;
			if (!SND.OOCue2Acb.TryGetValue(header, out bdic) || !bdic.TryGetValue(cue_name, out cueHandler))
			{
				X.de("サウンド " + cue_name + " が読み込まれていないか、存在しません...", null);
				return false;
			}
			CriAtomExAcb acb = SND.ALoader[(int)cueHandler.loader_index].getAcb((int)cueHandler.sheet_index);
			if (acb == null)
			{
				X.de("サウンドAcbの取得に失敗...", null);
				return false;
			}
			MyPlayer.SetCue(acb, cue_name, cueHandler.Info);
			return true;
		}

		public static bool checkSameFrameSound(string header, string cue_name)
		{
			header = header ?? "";
			List<string> list;
			if (!SND.OAsameframe_snd.TryGetValue(header, out list))
			{
				list = (SND.OAsameframe_snd[header] = new List<string>(8));
			}
			return X.pushIdentical<string>(list, cue_name);
		}

		public static float volume01
		{
			get
			{
				return SND.volume01_;
			}
		}

		public static int volume
		{
			get
			{
				return X.IntR(SND.volume01_ * 100f);
			}
			set
			{
				if (SND.volume == value)
				{
					return;
				}
				SND.volume01_ = X.ZLINE((float)value, 100f);
			}
		}

		public static float voice_volume01
		{
			get
			{
				return SND.voice_volume01_;
			}
		}

		public static int voice_volume
		{
			get
			{
				return X.IntR(SND.voice_volume01_ * 100f);
			}
			set
			{
				if (SND.voice_volume == value)
				{
					return;
				}
				SND.voice_volume01_ = X.ZLINE((float)value, 100f);
			}
		}

		public static float bgm_volume01
		{
			get
			{
				return SND.bgm_volume01_;
			}
		}

		public static int bgm_volume
		{
			get
			{
				return X.IntR(SND.bgm_volume01_ * 100f);
			}
			set
			{
				if (SND.bgm_volume == value)
				{
					return;
				}
				SND.bgm_volume01_ = X.ZLINE((float)value, 100f);
				BGM.fineVolume();
			}
		}

		public static int count2pitch(int count)
		{
			int num = count / 7;
			int num2 = count % 7;
			return (12 * num + num2 * 2 - ((num2 >= 3) ? 1 : 0)) * 100;
		}

		public const string snd_cri_acf_file = "nelcri.acf";

		public const string snd_data_file = "Basic/Data/_snd";

		public const bool use_cri_debugger = false;

		private static SND.SndLoader[] ALoader;

		public static int need_explode_cue_info = 0;

		private static BDic<string, BDic<string, SND.CueHandler>> OOCue2Acb;

		public static SndPlayer Ui;

		public static CriAtomEx.CueInfo DepCurInfo;

		private static List<SND.InitSndLoad> AInitializeSoundSheet;

		private static CriAtom[] AAtomBehaviour;

		private static float volume01_ = 0.8f;

		private static float voice_volume01_ = 0.85f;

		private static float bgm_volume01_ = 0.7f;

		private static BDic<string, List<string>> OAsameframe_snd;

		private sealed class SndLoader : Flagger
		{
			public SndLoader(string _key)
				: base(null, null)
			{
				this.key = _key;
				this.FnActivate = new FlaggerT<string>.FnFlaggerCall(this.fnActivatedSheets);
				this.FnDeactivate = new FlaggerT<string>.FnFlaggerCall(this.fnDectivatedSheets);
			}

			public void initSheetFinalize(List<string> Asheets_buf)
			{
				this.Asheets = Asheets_buf.ToArray();
			}

			private void fnActivatedSheets(FlaggerT<string> _This)
			{
				if (this.loaded)
				{
					return;
				}
				this.loaded = true;
				this.snd_exploded_flag++;
				SND.need_explode_cue_info++;
				int num = this.Asheets.Length;
				for (int i = 0; i < num; i++)
				{
					CriAtom.AddCueSheet(this.Asheets[i], this.Asheets[i] + ".acb", this.use_awb ? (this.Asheets[i] + ".awb") : "", null);
				}
			}

			private void fnDectivatedSheets(FlaggerT<string> _This)
			{
				if (TX.isStart(this.key, "BGM_", 0))
				{
					this.unload();
				}
			}

			public void unload()
			{
				if (!this.loaded)
				{
					return;
				}
				if (this.snd_exploded_flag == 1)
				{
					SND.need_explode_cue_info--;
				}
				this.snd_exploded_flag = 0;
				this.loaded = false;
				int num = this.Asheets.Length;
				for (int i = 0; i < num; i++)
				{
					CriAtom.RemoveCueSheet(this.Asheets[i]);
					if (this.AAcb != null)
					{
						this.AAcb[i] = null;
					}
				}
				SND.unloadAttachedLoader(this, this.header);
			}

			public bool explodeCueInfo(int my_index)
			{
				if (!this.loaded || this.snd_exploded_flag != 1)
				{
					return false;
				}
				this.snd_exploded_flag = 2;
				SND.need_explode_cue_info--;
				int num = this.Asheets.Length;
				int num2 = 0;
				this.AAcb = new CriAtomExAcb[num];
				int i = 0;
				while (i < num)
				{
					CriAtomEx.CueInfo[] array = null;
					try
					{
						CriAtomExAcb acb = CriAtom.GetAcb(this.Asheets[i]);
						if (acb != null)
						{
							this.AAcb[num2++] = acb;
							array = acb.GetCueInfoList();
							if (array == null)
							{
								X.de("CRIWARE: " + this.Asheets[i] + " のcueInfoListを取得できませんでした", null);
							}
						}
						else
						{
							X.de("CRIWARE: " + this.Asheets[i] + " is not loaded.", null);
						}
					}
					catch (Exception ex)
					{
						X.de("キューシートの名前に誤りがあります: " + this.Asheets[i] + " " + ex.ToString(), null);
						Debug.LogWarning(ex.ToString());
						goto IL_0174;
					}
					goto IL_00EA;
					IL_0174:
					i++;
					continue;
					IL_00EA:
					if (array != null)
					{
						int num3 = array.Length;
						for (int j = 0; j < num3; j++)
						{
							CriAtomEx.CueInfo cueInfo = array[j];
							try
							{
								SND.registerCueNameToAcb(this.header, cueInfo.name, my_index, i, cueInfo);
							}
							catch
							{
								X.de(string.Concat(new string[]
								{
									"キューシート ",
									this.Asheets[i],
									" でキューが読み込めませんでした: index ",
									j.ToString(),
									" / name:",
									cueInfo.name
								}), null);
							}
						}
						goto IL_0174;
					}
					goto IL_0174;
				}
				return true;
			}

			public CriAtomExAcb getAcb(int index)
			{
				if (this.AAcb == null)
				{
					return null;
				}
				return this.AAcb[index];
			}

			public readonly string key;

			public string[] Asheets;

			public CriAtom Atom;

			private CriAtomExAcb[] AAcb;

			public bool loaded;

			public string header = "";

			public bool use_awb;

			private int snd_exploded_flag;
		}

		private struct InitSndLoad
		{
			public InitSndLoad(string _timing, string _load_key)
			{
				this.timing = _timing;
				this.load_key = _load_key;
			}

			public string timing;

			public string load_key;
		}

		private class CueHandler
		{
			public CueHandler(int _loader_index, int _sheet_index, CriAtomEx.CueInfo _Info)
			{
				this.loader_index = (byte)_loader_index;
				this.sheet_index = (byte)_sheet_index;
				this.Info = _Info;
			}

			public byte loader_index;

			public byte sheet_index;

			public CriAtomEx.CueInfo Info;
		}
	}
}

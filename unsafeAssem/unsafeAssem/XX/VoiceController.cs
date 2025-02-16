using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using m2d;

namespace XX
{
	public class VoiceController
	{
		public VoiceController(string LT, string _unique_key, bool _play_m2d = false)
		{
			this.unique_key = _unique_key;
			this.play_m2d = _play_m2d;
			this.readScriptOnlySheet(LT);
		}

		public VoiceController(VoiceController CloneFrom, string _unique_key, bool _play_m2d = false)
		{
			this.unique_key = _unique_key;
			this.play_m2d = _play_m2d;
			this.s_header = CloneFrom.s_header;
			this.s_load_sheet_key = CloneFrom.s_load_sheet_key;
			this.OData = CloneFrom.OData;
			this.reloadSheet();
		}

		public void reloadSheet()
		{
			this.readScriptOnlySheet(null);
		}

		private void readScriptOnlySheet(string script)
		{
			if (X.DEBUGNOSND || X.DEBUGNOVOICE)
			{
				return;
			}
			if (this.s_load_sheet_key == null && script != null)
			{
				this.CR = new CsvReaderA(script, true);
				while (this.CR.read())
				{
					if (this.CR.cmd == "%LOAD")
					{
						if (this.s_load_sheet_key == null)
						{
							this.s_load_sheet_key = this.CR._1;
							this.s_header = this.CR._2;
							break;
						}
						X.de("%LOAD が2回以上呼ばれた", null);
					}
				}
			}
			if (this.s_load_sheet_key != null && !this.sheet_loaded)
			{
				this.sheet_loaded = true;
				this.ALoadedSheet = SND.loadSheets(this.s_load_sheet_key, this.unique_key);
			}
		}

		public void destruct()
		{
			if (this.sheet_loaded)
			{
				SND.unloadSheets(this.s_load_sheet_key, this.unique_key);
				this.sheet_loaded = false;
			}
			if (this.MyChn != null)
			{
				this.MyChn.Dispose();
				this.MyChn = null;
			}
		}

		public void readScript()
		{
			if (this.ALoadedSheet == null || this.CR == null)
			{
				return;
			}
			this.CR.seek_set(0);
			int num = 0;
			float num2 = 1f;
			this.OData = new BDic<string, VoiceController.VoiceData>();
			int num3 = this.ALoadedSheet.Length;
			CriAtomEx.CueInfo[][] array = new CriAtomEx.CueInfo[num3][];
			bool[][] array2 = new bool[num3][];
			while (this.CR.read())
			{
				if (this.CR.cmd == "%LOAD")
				{
					if (this.s_load_sheet_key != this.CR._1)
					{
						X.de("%LOAD が2回以上呼ばれた", null);
					}
					else
					{
						for (int i = 0; i < num3; i++)
						{
							array[i] = SND.getSheetInfo(this.ALoadedSheet[i]);
							array2[i] = new bool[array[i].Length];
						}
					}
				}
				else if (this.CR.cmd == "%WEIGHT")
				{
					num = this.CR.Int(1, 0);
				}
				else if (this.CR.cmd == "%SAME_ALLOW")
				{
					num2 = this.CR.Nm(1, 0f);
				}
				else
				{
					Regex regex = new Regex("^" + this.CR.cmd + "(\\.[a-zA-Z0-9_]+)?$");
					VoiceController.VoiceData voiceData = null;
					for (int j = 0; j < num3; j++)
					{
						CriAtomEx.CueInfo[] array3 = array[j];
						int num4 = array3.Length;
						for (int k = 0; k < num4; k++)
						{
							CriAtomEx.CueInfo cueInfo = array3[k];
							if (!array2[j][k] && regex.Match(cueInfo.name).Success)
							{
								array2[j][k] = true;
								if (voiceData == null)
								{
									Dictionary<string, VoiceController.VoiceData> odata = this.OData;
									string cmd = this.CR.cmd;
									VoiceController.VoiceData voiceData2 = new VoiceController.VoiceData();
									voiceData2.name = this.CR.cmd;
									voiceData2.weight = this.CR.Int(1, num);
									voiceData2.same_allow = num2;
									VoiceController.VoiceData voiceData3 = voiceData2;
									odata[cmd] = voiceData2;
									voiceData = voiceData3;
								}
								voiceData.Add(cueInfo.name);
							}
						}
					}
				}
			}
			this.ALoadedSheet = null;
		}

		public bool isPlaying()
		{
			return this.Chn != null && this.Chn.isPlaying();
		}

		public bool isPlaying(string t)
		{
			return this.Chn != null && this.Chn.isPlaying() && this.CurVd != null && this.CurVd.name == t;
		}

		public void stop(string family)
		{
			if (this.Chn != null && this.Chn.isPlaying() && this.CurVd != null && this.CurVd.name == family)
			{
				this.Chn.Stop();
				this.Chn = null;
				this.CurVd = null;
			}
		}

		public SndPlayer play(string family, bool no_use_post = false)
		{
			if (this.OData == null)
			{
				return null;
			}
			VoiceController.VoiceData voiceData = X.Get<string, VoiceController.VoiceData>(this.OData, family);
			if (voiceData == null)
			{
				X.de("ボイスファミリーが不明: " + family, null);
				return null;
			}
			if (this.Chn != null && (!this.Chn.isPlaying() || this.Chn.current_cue != this.last_played_cue || this.Chn.rest_duration_milisecond < 150L))
			{
				this.last_played_cue = null;
				this.CurVd = null;
			}
			if (this.CurVd != null)
			{
				if (this.CurVd.name == family)
				{
					if (X.XORSP() * (1f - this.override_allow_level) >= this.CurVd.same_allow)
					{
						return null;
					}
				}
				else
				{
					VoiceController.VoiceData voiceData2 = voiceData;
					if (X.Scr(X.XORSP(), this.override_allow_level) * 100f < (float)X.Mn(this.CurVd.weight - voiceData2.weight, 100))
					{
						return null;
					}
					this.CurVd = voiceData2;
				}
			}
			else
			{
				this.CurVd = voiceData;
			}
			if (this.Chn != null && this.ignore_prechn_stop < 1f && this.ignore_prechn_stop < X.XORSP())
			{
				this.Chn.Stop();
			}
			this.playInner(this.CurVd.getRandom(), no_use_post ? null : VoiceController.BusChn);
			if (this.Chn == null)
			{
				this.CurVd = null;
				this.last_played_cue = null;
			}
			else
			{
				this.last_played_cue = this.Chn.current_cue;
			}
			return this.Chn;
		}

		public virtual void playInner(string key, BusChannelManager BChn)
		{
			if (this.play_m2d)
			{
				this.Chn = M2DBase.playSnd(this.s_header, key);
				if (BChn != null)
				{
					BChn.updateSound(this.Chn);
					return;
				}
			}
			else
			{
				if (this.MyChn == null)
				{
					this.MyChn = new SndPlayer("VoiceCache", SndPlayer.SNDTYPE.VOICE);
				}
				this.MyChn.FineVol();
				this.Chn = (this.MyChn.play(this.s_header, key, false) ? this.MyChn : null);
			}
		}

		public bool destructed
		{
			get
			{
				return this.s_load_sheet_key == null;
			}
		}

		protected string s_header;

		private string s_load_sheet_key;

		public readonly string unique_key = "voice";

		private bool sheet_loaded;

		private string[] ALoadedSheet;

		protected SndPlayer Chn;

		protected SndPlayer MyChn;

		private VoiceController.VoiceData CurVd;

		private string last_played_cue;

		private CsvReaderA CR;

		private BDic<string, VoiceController.VoiceData> OData;

		public float override_allow_level;

		public bool play_m2d;

		public float ignore_prechn_stop;

		public static BusChannelManager BusChn = new BusChannelManager();

		private class VoiceData
		{
			public VoiceData()
			{
				this.ASnd = new List<string>();
			}

			public void Add(string cue_name)
			{
				if (cue_name != null)
				{
					this.ASnd.Add(cue_name);
				}
			}

			public string getRandom()
			{
				return this.ASnd[X.xors(this.ASnd.Count)];
			}

			public int count
			{
				get
				{
					return this.ASnd.Count;
				}
			}

			private List<string> ASnd;

			public string name;

			public int weight;

			public float same_allow = 1f;
		}
	}
}

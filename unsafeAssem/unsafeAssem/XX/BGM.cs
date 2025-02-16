using System;
using System.Collections.Generic;
using evt;

namespace XX
{
	public static class BGM
	{
		public static event BGM.FnBeatAttack EvBeatCallBack;

		public static void initBgm()
		{
			BGM.CurPl = new BGM.BgmPlayer("bgm0");
			BGM.OthPl = new BGM.BgmPlayer("bgm1");
			BgmKind.readBgmScript();
			BGM.HalfCounter = new Flagger(delegate(FlaggerT<string> V)
			{
				BGM.fine_half = true;
			}, delegate(FlaggerT<string> V)
			{
				BGM.fine_half = true;
			});
			BGM.FD_BeatCallBack = new CriAtomExBeatSync.CbFunc(BGM.BeatCallBack);
			BGM.BeatCalcCounter = new Flagger(delegate(FlaggerT<string> V)
			{
				if (!X.DEBUGNOSND)
				{
					CriAtom.OnBeatSyncCallback += BGM.FD_BeatCallBack;
				}
			}, delegate(FlaggerT<string> V)
			{
				if (!X.DEBUGNOSND)
				{
					CriAtom.OnBeatSyncCallback -= BGM.FD_BeatCallBack;
				}
			});
		}

		public static bool load(string timing, string cue_key = null, bool no_overload_error = false)
		{
			BgmKind fromSheet = BgmKind.GetFromSheet(timing);
			if (fromSheet == null)
			{
				X.de("不明なBGMKind " + timing + " 接頭辞BGM_* はありますか？", null);
				return false;
			}
			if (cue_key == null)
			{
				cue_key = fromSheet.default_que;
			}
			if (BGM.CurPl.sheet_key == timing && BGM.CurPl.current_cue == cue_key)
			{
				if (!no_overload_error)
				{
					X.dl("BGM " + cue_key + " はすでに CurPl 側に読み込まれています", null, false, false);
				}
				BGM.CurPl.auto_unload = false;
				BGM.OthPl.unload_fadeouting = true;
				return !BGM.CurPl.isPlaying() || BGM.CurPl.unload_fadeouting;
			}
			if (BGM.OthPl.sheet_key == timing && BGM.OthPl.current_cue == cue_key && !BGM.OthPl.unload_fadeouting)
			{
				if (!no_overload_error)
				{
					X.dl("BGM " + cue_key + " はすでに OthPl 側に読み込まれています", null, false, false);
				}
				return true;
			}
			if (TX.valid(timing))
			{
				BGM.OthPl.loadSheets(timing, fromSheet);
			}
			return TX.valid(cue_key) && BGM.OthPl.prepare("BGM.", cue_key, false);
		}

		public static void stop(bool temporary = false, bool immediate_run = false)
		{
			if (X.DEBUGNOSND)
			{
				return;
			}
			if (temporary)
			{
				if (immediate_run)
				{
					BGM.CurPl.fadeout(0f, 0f, false);
					BGM.CurPl.runFade();
				}
				else
				{
					BGM.CurPl.Pause();
				}
			}
			else
			{
				BGM.CurPl.fadeout(0f, 0f, true);
				if (immediate_run)
				{
					BGM.CurPl.runFade();
				}
			}
			BGM.cur_bpm = (BGM.cur_bpm_r = 0f);
		}

		public static void fadein(float dep_to = 100f, float maxt = 120f)
		{
			if (X.DEBUGNOSND)
			{
				return;
			}
			if (dep_to > 0f)
			{
				BGM.fineBpm();
			}
			BGM.CurPl.fadeout(dep_to, maxt, false);
		}

		public static void fadeout(float dep_to = 0f, float maxt = 120f, bool auto_unload = true)
		{
			if (X.DEBUGNOSND)
			{
				return;
			}
			if (dep_to > 0f)
			{
				BGM.fineBpm();
			}
			BGM.CurPl.fadeout(dep_to, maxt, auto_unload);
		}

		public static void play(float fadein_maxt = 0f)
		{
			if (X.DEBUGNOSND)
			{
				return;
			}
			if (BGM.CurPl.sheet_key == null || BGM.CurPl.current_cue == null)
			{
				X.de("シートがセットされていないのに play が呼ばれた ", null);
				return;
			}
			BGM.fineBpm();
			BGM.CurPl.fadein(fadein_maxt);
			BGM.fine_fade = true;
			BGM.BusChn.fine_flag = true;
		}

		public static void replace(float fadeout_time, float fadein_time = 0f, bool auto_unload = true, bool no_error = false)
		{
			if (!BGM.OthPl.isPrepared() || BGM.OthPl.unload_fadeouting)
			{
				if (!no_error)
				{
					X.dl("反対側プレイヤーが初期化されていません", null, false, false);
				}
				if (!BGM.CurPl.auto_unload)
				{
					BGM.play(fadein_time);
				}
				return;
			}
			BGM.CurPl.fadeout(0f, fadeout_time, auto_unload);
			if (fadein_time >= 0f)
			{
				BGM.OthPl.fadein(fadein_time);
			}
			BGM.BgmPlayer othPl = BGM.OthPl;
			BGM.OthPl = BGM.CurPl;
			BGM.CurPl = othPl;
			BGM.bgm_replace_id = (BGM.bgm_replace_id + 1) & 63;
			BGM.fineBpm();
			BGM.BusChn.fine_flag = false;
			BGM.BusChn.updateSound(BGM.CurPl);
			BGM.CurPl.UpdateAll();
		}

		private static void fineBpm()
		{
			BgmKind kind = BGM.CurPl.getKind();
			if (kind == null)
			{
				return;
			}
			BGM.cur_bpm = kind.bpm;
			BGM.cur_bpm_r = 1f / BGM.cur_bpm;
			if (X.DEBUGNOSND)
			{
				BGM.beatcount = 0;
				BGM.half_level = 3600f * BGM.cur_bpm_r;
				BGM.nextbeattiming = BGM.half_level;
			}
		}

		public static void setAisac(string k, float val)
		{
			if (X.DEBUGNOSND)
			{
				return;
			}
			if (BGM.CurPl.isPlaying())
			{
				BGM.CurPl.SetAisacControl(k, val);
				BGM.update_flag = true;
			}
			if (BGM.OthPl.isPlaying())
			{
				BGM.OthPl.SetAisacControl(k, val);
				BGM.update_flag = true;
			}
		}

		public static void addHalfFlag(string s)
		{
			if (X.DEBUGNOSND)
			{
				return;
			}
			BGM.HalfCounter.Add(s);
		}

		public static void remHalfFlag(string s)
		{
			if (X.DEBUGNOSND)
			{
				return;
			}
			BGM.HalfCounter.Rem(s);
		}

		public static void addUseBeatFlag(string s)
		{
			BGM.BeatCalcCounter.Add(s);
		}

		public static void remUseBeatFlag(string s)
		{
			BGM.BeatCalcCounter.Rem(s);
		}

		public static void clearHalfFlag()
		{
			BGM.BeatCalcCounter.Clear();
			if (X.DEBUGNOSND)
			{
				return;
			}
			BGM.HalfCounter.Clear();
			BGM.half_level = 1f;
		}

		public static void GotoBlock(string c, bool force_to_head = true)
		{
			if (BGM.CurPl.getKind() == null)
			{
				return;
			}
			BGM.CurPl.GotoBlock(c, true);
		}

		public static void setOverrideKey(string _key = "")
		{
			BgmKind kind = BGM.CurPl.getKind();
			if (kind == null)
			{
				return;
			}
			if (kind.ABlkOvr == null)
			{
				if (TX.valid(_key))
				{
					X.de(BGM.CurPl.current_cue + "の オーバーライド情報が定義されていません", null);
				}
				return;
			}
			if (TX.noe(_key))
			{
				_key = "_";
			}
			for (int i = kind.ABlkOvr.Count - 1; i >= 0; i--)
			{
				BgmBlockOverride bgmBlockOverride = kind.ABlkOvr[i];
				if (bgmBlockOverride.key == _key)
				{
					if (BGM.CurPl.OverrideBlock != bgmBlockOverride)
					{
						BGM.fine_fade = true;
						BGM.CurPl.OverrideBlock = bgmBlockOverride;
					}
					return;
				}
			}
			X.de(BGM.CurPl.current_cue + "の オーバーライドキー" + _key + " が定義されていません", null);
		}

		public static void fineVolume()
		{
			if (X.DEBUGNOSND)
			{
				return;
			}
			if (BGM.CurPl.isPlaying())
			{
				BGM.CurPl.FineVol();
			}
			if (BGM.OthPl.isPlaying())
			{
				BGM.OthPl.FineVol();
			}
			BGM.update_flag = true;
		}

		public static void addBattleTransition(string trns_key)
		{
			BGM.CurPl.FlgBattle.Add(trns_key);
		}

		public static void remBattleTransition(string trns_key)
		{
			BGM.CurPl.FlgBattle.Rem(trns_key);
			BGM.OthPl.FlgBattle.Rem(trns_key);
		}

		public static void runBGM(float fcnt)
		{
			if (X.DEBUGNOSND)
			{
				if (BGM.BeatCalcCounter.isActive() && BGM.cur_bpm > 0f && BGM.EvBeatCallBack != null)
				{
					BGM.half_level -= fcnt * 0.95f;
					if (BGM.half_level <= 0f)
					{
						BGM.nextbeattiming = (BGM.half_level = BGM.cur_bpm_r * 3600f);
						BGM.beatcount++;
						BGM.EvBeatCallBack(BGM.beatcount);
					}
				}
				return;
			}
			if (BGM.BusChn.fine_flag)
			{
				BGM.BusChn.fine_flag = false;
				BGM.BusChn.updateSound(BGM.CurPl);
				BGM.update_flag = true;
			}
			if (BGM.fine_fade)
			{
				BGM.fine_fade = false;
				for (int i = 0; i < 2; i++)
				{
					BGM.fine_fade = ((i == 0) ? BGM.CurPl : BGM.OthPl).runFade() || BGM.fine_fade;
				}
			}
			if (BGM.fine_half)
			{
				float num = (BGM.HalfCounter.isActive() ? 0.5f : 1f);
				BGM.half_level = X.VALWALK(BGM.half_level, num, 0.024f);
				if (BGM.half_level == num)
				{
					BGM.fine_half = false;
				}
				BGM.CurPl.FineVol();
				BGM.OthPl.FineVol();
			}
			if (BGM.update_flag)
			{
				BGM.CurPl.UpdateAll();
				BGM.OthPl.UpdateAll();
				BGM.update_flag = false;
			}
		}

		public static float getDummyIndicatorData()
		{
			if (BGM.cur_bpm == 0f || BGM.nextbeattiming <= 0f)
			{
				return 0f;
			}
			return X.ZPOW(BGM.half_level, BGM.nextbeattiming) * 0.5f;
		}

		private static void BeatCallBack(ref CriAtomExBeatSync.Info Info)
		{
			BGM.beatcount = (int)Info.beatCount;
			if (BGM.cur_bpm > 0f && BGM.EvBeatCallBack != null)
			{
				if (BGM.cur_bpm != Info.bpm)
				{
					BGM.cur_bpm = Info.bpm;
					BGM.cur_bpm_r = 1f / BGM.cur_bpm;
				}
				BGM.nextbeattiming = 3600f * BGM.cur_bpm_r;
				BGM.EvBeatCallBack(BGM.beatcount);
			}
		}

		public static bool prepare_flag
		{
			get
			{
				return (BGM.fine_flags & 2) > 0;
			}
			set
			{
				BGM.fine_flags = (value ? (BGM.fine_flags | 4) : (BGM.fine_flags & -3));
			}
		}

		public static bool update_flag
		{
			get
			{
				return (BGM.fine_flags & 4) > 0;
			}
			set
			{
				BGM.fine_flags = (value ? (BGM.fine_flags | 4) : (BGM.fine_flags & -5));
			}
		}

		public static bool fine_fade
		{
			get
			{
				return (BGM.fine_flags & 8) > 0;
			}
			set
			{
				BGM.fine_flags = (value ? (BGM.fine_flags | 8) : (BGM.fine_flags & -9));
			}
		}

		public static bool fine_half
		{
			get
			{
				return (BGM.fine_flags & 16) > 0;
			}
			set
			{
				BGM.fine_flags = (value ? (BGM.fine_flags | 16) : (BGM.fine_flags & -17));
			}
		}

		public static CriAtomExPlayer getSoundPlayerInstanceFor(string cue_key)
		{
			if (BGM.OthPl.current_cue == cue_key)
			{
				return BGM.OthPl.getPlayerInstance();
			}
			if (BGM.CurPl.current_cue == cue_key)
			{
				return BGM.CurPl.getPlayerInstance();
			}
			return null;
		}

		public static CriAtomExPlayer getNextSoundPlayerInstance(bool force_stop_bgm = false)
		{
			if (force_stop_bgm)
			{
				BGM.OthPl.stopBgm(true);
			}
			return BGM.OthPl.getPlayerInstance();
		}

		public static bool frontBGMIs(string timing, string cue)
		{
			return BGM.CurPl.Is(timing, cue);
		}

		public static bool isFrontPlaying()
		{
			return BGM.CurPl.isPlaying() && BGM.CurPl.base_volume > 0f;
		}

		public static void getFrontBgm(out string timing, out string cue)
		{
			if (BGM.CurPl != null)
			{
				timing = BGM.CurPl.sheet_key;
				cue = BGM.CurPl.current_cue;
				return;
			}
			string text;
			cue = (text = null);
			timing = text;
		}

		public static bool EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			if (!X.DEBUGNOSND)
			{
				return false;
			}
			if (cmd != null && cmd == "LOAD_SND_TIMING")
			{
				if (BGM.Aevt_loaded_timing == null)
				{
					BGM.Aevt_loaded_timing = new List<string>(rER.clength - 1);
				}
				for (int i = 1; i < rER.clength; i++)
				{
					string index = rER.getIndex(i);
					if (BGM.Aevt_loaded_timing.IndexOf(index) == -1)
					{
						SND.loadSheets(index, "EV_BGM");
					}
				}
				return true;
			}
			return false;
		}

		public static bool readEvLineBgm(EvReader ER, StringHolder rER, int skipping = 0)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 1718007254U)
				{
					if (num <= 642558078U)
					{
						if (num != 351259876U)
						{
							if (num != 642558078U)
							{
								return false;
							}
							if (!(cmd == "BGM_OVERRIDE_KEY"))
							{
								return false;
							}
							BGM.setOverrideKey(rER._1);
						}
						else if (!(cmd == "LOAD_SND_TIMING"))
						{
							return false;
						}
					}
					else if (num != 912732220U)
					{
						if (num != 1478516090U)
						{
							if (num != 1718007254U)
							{
								return false;
							}
							if (!(cmd == "REPLACE_BGM"))
							{
								return false;
							}
							if (!X.DEBUGNOSND)
							{
								float num2 = rER.Nm(1, 0f);
								float num3 = rER.Nm(2, -1f);
								BGM.replace(num2, (num3 < 0f) ? num2 : num3, rER.NmE(3, 1f) != 0f, false);
							}
						}
						else
						{
							if (!(cmd == "START_BGM"))
							{
								return false;
							}
							if (!X.DEBUGNOSND)
							{
								BGM.fadeout(100f, rER.Nm(1, 0f), false);
							}
						}
					}
					else
					{
						if (!(cmd == "STOP_BGM"))
						{
							return false;
						}
						if (!X.DEBUGNOSND)
						{
							BGM.fadeout(0f, rER.Nm(1, 0f), rER.NmE(2, 0f) != 0f);
						}
					}
				}
				else if (num <= 2594132501U)
				{
					if (num != 2449196888U)
					{
						if (num != 2594132501U)
						{
							return false;
						}
						if (!(cmd == "HALF_BGM"))
						{
							return false;
						}
						if (!X.DEBUGNOSND)
						{
							if (rER.Nm(1, 1f) != 0f)
							{
								BGM.HalfCounter.Add("EVENT_HALF");
							}
							else
							{
								BGM.HalfCounter.Rem("EVENT_HALF");
							}
						}
					}
					else
					{
						if (!(cmd == "LOAD_BGM"))
						{
							return false;
						}
						if (!X.DEBUGNOSND)
						{
							BGM.load(rER._1, TX.valid(rER._2) ? rER._2 : null, false);
						}
					}
				}
				else if (num != 2649859326U)
				{
					if (num != 3896518271U)
					{
						if (num != 4144876992U)
						{
							return false;
						}
						if (!(cmd == "SND"))
						{
							return false;
						}
						SND.Ui.play(rER._1, false);
					}
					else
					{
						if (!(cmd == "BGM_GOTO_BLOCK"))
						{
							return false;
						}
						if (!X.DEBUGNOSND)
						{
							BGM.CurPl.GotoBlock(rER._1, true);
						}
					}
				}
				else
				{
					if (!(cmd == "BATTLE_TRANSITION_BGM"))
					{
						return false;
					}
					if (!X.DEBUGNOSND)
					{
						if (rER.Nm(1, 1f) != 0f)
						{
							BGM.addBattleTransition("EVENT_TRANS");
						}
						else
						{
							BGM.remBattleTransition("EVENT_TRANS");
						}
					}
				}
				return true;
			}
			return false;
		}

		public static bool closeEvBgm(bool is_end)
		{
			if (!X.DEBUGNOSND && is_end)
			{
				BGM.HalfCounter.Rem("EVENT_HALF");
				BGM.CurPl.FlgBattle.Rem("EVENT_TRANS");
				BGM.OthPl.FlgBattle.Rem("EVENT_TRANS");
			}
			return true;
		}

		public static void flushEventLoadedTiming()
		{
			if (BGM.Aevt_loaded_timing != null)
			{
				int count = BGM.Aevt_loaded_timing.Count;
				for (int i = 0; i < count; i++)
				{
					SND.unloadSheets(BGM.Aevt_loaded_timing[i], "EV_BGM");
				}
				BGM.Aevt_loaded_timing = null;
			}
		}

		private static BGM.BgmPlayer CurPl;

		private static BGM.BgmPlayer OthPl;

		private static string bgm_key;

		private static int fine_flags = 0;

		private static float half_level = 1f;

		private static Flagger HalfCounter;

		private static Flagger BeatCalcCounter;

		public static BusChannelManager BusChn = new BusChannelManager();

		public const string bgm_cue_header = "BGM.";

		public static float cur_bpm = 0f;

		public static float cur_bpm_r = 0f;

		public static float nextbeattiming = 0f;

		public static int beatcount = 0;

		public static byte bgm_replace_id = 0;

		private static CriAtomExBeatSync.CbFunc FD_BeatCallBack;

		private static List<string> Aevt_loaded_timing;

		public delegate void FnBeatAttack(int count);

		private class BgmPlayer : SndPlayer
		{
			public BgmPlayer(string key)
				: base(key, SndPlayer.SNDTYPE.BGM)
			{
				if (!X.DEBUGNOSND)
				{
					this.ExpB = new CriAtomExPlayer();
				}
				this.FlgBattle = new Flagger(delegate(FlaggerT<string> V)
				{
					this.initBattleTransition(BgmKind.QUE_KIND.BATTLE);
				}, delegate(FlaggerT<string> V)
				{
					this.initBattleTransition(BgmKind.QUE_KIND.NORMAL);
				});
			}

			public bool isBPlaying()
			{
				if (X.DEBUGNOSND)
				{
					return false;
				}
				CriAtomExPlayer.Status status = this.ExpB.GetStatus();
				return status == CriAtomExPlayer.Status.Prep || status == CriAtomExPlayer.Status.Playing || this.is_loop == 2;
			}

			public override void Dispose()
			{
				if (X.DEBUGNOSND)
				{
					base.Dispose();
					return;
				}
				if (this.isPlaying())
				{
					this.ExpB.StopWithoutReleaseTime();
				}
				base.Dispose();
				this.ExpB.Dispose();
			}

			public bool Is(string _sheet_key, string _cue)
			{
				return this.sheet_key == _sheet_key && this.current_cue == _cue;
			}

			public void loadSheets(string _sheet_key, BgmKind _Kind)
			{
				if (this.sheet_key == _sheet_key)
				{
					return;
				}
				this.releaseSheets();
				this.Kind = _Kind;
				this.auto_unload = false;
				this.sheet_key = _sheet_key;
				SND.loadSheets(_sheet_key, this.key);
				this.resetFading();
			}

			public bool isPrepared()
			{
				return this.sheet_key != null && this.fade_vol >= 0f;
			}

			public override bool prepare(string header, string _cue_key, bool force = false)
			{
				if (X.DEBUGNOSND)
				{
					this.current_cue = _cue_key;
					return false;
				}
				if (this.sheet_key == null)
				{
					X.de("シートがセットされていないのに setCue が呼ばれた ", null);
					return false;
				}
				if (!SND.loaded)
				{
					X.de("キューシート読み込み中に setCue が呼ばれた ", null);
					return false;
				}
				this.auto_unload = false;
				if (this.fade_vol < 0f)
				{
					this.fadedep = (this.fade_vol = 100f);
					this.fadespeed = 0f;
				}
				else
				{
					this.resetFading();
				}
				if (_cue_key == this.current_cue)
				{
					return true;
				}
				BGM.fine_fade = true;
				this.Stop();
				this.OverrideBlock = null;
				if (!base.prepare(header, _cue_key, false))
				{
					return false;
				}
				this.OverrideBlock = ((this.Kind.ABlkOvr != null) ? this.Kind.ABlkOvr[0] : null);
				this.Exp.ResetParameters();
				this.ExpB.ResetParameters();
				this.Exp.SetVoicePriority(base.voice_priority);
				this.ExpB.SetVoicePriority(base.voice_priority);
				this.Exp.SetEnvelopeReleaseTime(0f);
				this.PlbA = this.APlayBack[0];
				this.Exp.Pause();
				this.SetAisacControl("vol", 0f);
				this.SetAisacControl("vol2", 0f);
				this.SetAisacControl("harmonic_pan", 0.5f);
				return true;
			}

			public override void SetCue(CriAtomExAcb acb, string name, CriAtomEx.CueInfo Info)
			{
				if (X.DEBUGNOSND)
				{
					return;
				}
				base.SetCue(acb, name, Info);
				this.ExpB.SetEnvelopeReleaseTime(0f);
				this.Exp.SetEnvelopeReleaseTime(0f);
				this.MyAcb = acb;
				this.expb_stop = 0;
				if (this.current_cue == this.Kind.default_que && this.Kind.battle_que != null)
				{
					this.trs_maxt = -1f;
					this.ExpB.SetCue(acb, this.Kind.battle_que);
					this.ExpB.SetVoicePriority(base.voice_priority);
					this.que_a = BgmKind.QUE_KIND.NORMAL;
					this.que_b = BgmKind.QUE_KIND.BATTLE;
					return;
				}
				this.que_a = (this.que_b = BgmKind.QUE_KIND.NORMAL);
				this.ExpB.StopWithoutReleaseTime();
				this.trs_maxt = -2f;
			}

			public void GotoBlock(string c, bool force_to_head = true)
			{
				if (c == null || c.Length == 0 || this.MyAcb == null)
				{
					return;
				}
				int num = (int)(c[0] - 'A');
				if (force_to_head && num == this.PlbA.GetCurrentBlockIndex())
				{
					return;
				}
				bool flag = false;
				if (this.MyAcb != null)
				{
					this.Exp.SetEnvelopeReleaseTime(0f);
					this.Exp.Stop(true);
					flag = true;
				}
				this.Exp.SetFirstBlockIndex(num);
				if (flag)
				{
					this.Exp.SetVoicePriority(base.voice_priority);
					this.FineVol();
					this.PlbA = this.Exp.Prepare();
					this.resume_flag = true;
				}
				if (this.trs_maxt >= 0f)
				{
					this.trs_t = this.trs_maxt;
				}
				BGM.fine_fade = true;
				BGM.update_flag = true;
			}

			public void resetFading()
			{
				if (this.fadespeed != 0f)
				{
					this.fade_vol = this.fadedep;
				}
				if (this.fade_vol >= 0f)
				{
					this.fadedep = this.fade_vol;
				}
				else
				{
					this.fade_vol = this.fadedep;
				}
				this.fadespeed = 0f;
			}

			public override SndPlayer UpdateAll()
			{
				base.UpdateAll();
				return this;
			}

			public BGM.BgmPlayer stopBgm(bool force_unload = false)
			{
				this.resetFading();
				if (force_unload || (this.fade_vol <= 0f && this.auto_unload))
				{
					this.Stop();
					this.releaseSheets();
				}
				else
				{
					this.Pause();
				}
				return this;
			}

			public override void Stop()
			{
				if (X.DEBUGNOSND)
				{
					return;
				}
				base.Stop();
				this.ExpB.Stop();
				this.FlgBattle.Clear();
			}

			public override void Pause()
			{
				if (X.DEBUGNOSND)
				{
					return;
				}
				base.Pause();
				this.ExpB.Pause();
			}

			public void releaseSheets()
			{
				if (this.sheet_key != null)
				{
					SND.unloadSheets(this.sheet_key, this.key);
				}
				this.expb_stop = 0;
				this.trs_maxt = -2f;
				if (this.Exp != null)
				{
					this.Exp.ResetParameters();
					this.Exp.SetVoicePriority(base.voice_priority);
				}
				this.OverrideBlock = null;
				this.MyAcb = null;
				this.current_cue = null;
				this.fadespeed = 0f;
				this.Kind = null;
				this.FlgBattle.Clear();
				this.fade_vol = -100f;
				this.sheet_key = null;
			}

			public override SndPlayer FineVol()
			{
				if (X.DEBUGNOSND)
				{
					return this;
				}
				float num = this.base_volume * ((this.trs_maxt <= 0f) ? 1f : X.ZLINE(this.trs_t, this.trs_maxt));
				this.need_update_flag = true;
				BGM.update_flag = true;
				this.Exp.SetVolume(num);
				this.Exp.UpdateAll();
				float num2 = this.trs_maxt;
				return this;
			}

			public override void SetAisacControl(string controlName, float value)
			{
				if (X.DEBUGNOSND)
				{
					return;
				}
				base.SetAisacControl(controlName, value);
				if (this.trs_maxt != -2f)
				{
					this.ExpB.SetAisacControl(controlName, value);
				}
			}

			public BGM.BgmPlayer fadein(float maxt = 0f)
			{
				return this.fadeout(100f, maxt, false);
			}

			public BGM.BgmPlayer fadeout(float dep_to = 0f, float maxt = 0f, bool _auto_unload = true)
			{
				if (this.Kind == null || X.DEBUGNOSND)
				{
					return this;
				}
				this.FineVol();
				this.auto_unload = dep_to <= 0f && _auto_unload;
				this.fadedep = dep_to;
				if (dep_to > 0f)
				{
					if (!this.isPlaying())
					{
						BGM.BusChn.fine_flag = true;
						if (this.OverrideBlock != null)
						{
							BGM.fine_fade = true;
						}
						this.playDefault();
					}
					if (this.Exp.IsPaused())
					{
						BGM.BusChn.fine_flag = true;
						this.Exp.Resume(CriAtomEx.ResumeMode.PausedPlayback);
					}
					if (this.isBPlaying() && this.ExpB.IsPaused())
					{
						BGM.BusChn.fine_flag = true;
						this.ExpB.Resume(CriAtomEx.ResumeMode.PausedPlayback);
					}
				}
				if (maxt <= 0f)
				{
					this.fadespeed = 1000f;
					if (dep_to <= 0f)
					{
						this.stopBgm(false);
					}
					else
					{
						this.runFade();
					}
				}
				else
				{
					this.fadespeed = X.Mx(X.Abs((this.fadedep - this.fade_vol) / maxt), 0.01f);
					BGM.fine_fade = true;
				}
				return this;
			}

			public bool runFade()
			{
				if (X.DEBUGNOSND)
				{
					return false;
				}
				if (base.checkPreparing(true))
				{
					return true;
				}
				bool flag = false;
				bool flag2 = false;
				if (this.isPlaying() && this.fadespeed != 0f)
				{
					flag = true;
					if ((this.fade_vol = X.VALWALK(this.fade_vol, this.fadedep, this.fadespeed)) == this.fadedep)
					{
						if (this.fade_vol <= 0f)
						{
							this.stopBgm(false);
						}
						else
						{
							this.auto_unload = false;
							this.resetFading();
							flag2 = true;
						}
						flag = false;
					}
					else
					{
						flag2 = true;
						BGM.update_flag = true;
					}
				}
				if (this.trs_maxt >= 0f)
				{
					this.trs_t += 1f;
					this.resume_flag = true;
					flag2 = true;
					if (this.trs_t > this.trs_maxt)
					{
						this.trs_maxt = -1f;
					}
					else
					{
						flag = true;
					}
				}
				if (flag2)
				{
					this.FineVol();
				}
				if (this.resume_flag)
				{
					this.resume_flag = false;
					this.Exp.Resume(CriAtomEx.ResumeMode.AllPlayback);
				}
				if (this.expb_stop > 0)
				{
					byte b = this.expb_stop - 1;
					this.expb_stop = b;
					if (b == 0)
					{
						this.ExpB.Stop();
					}
				}
				if (this.OverrideBlock != null)
				{
					int currentBlockIndex = this.PlbA.GetCurrentBlockIndex();
					int num;
					if (this.OverrideBlock.Osrc.TryGetValue(currentBlockIndex, out num))
					{
						this.PlbA.SetNextBlockIndex(num);
					}
					flag = true;
				}
				return flag;
			}

			public void initBattleTransition(BgmKind.QUE_KIND kind_type)
			{
				if (this.trs_maxt == -2f || !this.isPlaying() || this.Kind == null || this.Kind.OTrans == null || X.DEBUGNOSND)
				{
					return;
				}
				if (this.que_a == kind_type)
				{
					return;
				}
				int num = this.PlbA.GetCurrentBlockIndex();
				num = (num + 1) % (this.Kind.block_max + 1);
				BgmTransitionPoint bgmTransitionPoint = X.Get<string, BgmTransitionPoint>(this.Kind.OTrans, this.Kind.battle_que);
				BgmTransitionPoint bgmTransitionPoint2 = X.Get<string, BgmTransitionPoint>(this.Kind.OTrans, this.Kind.default_que);
				if (bgmTransitionPoint == null || bgmTransitionPoint2 == null)
				{
					X.de("遷移情報が正しくない", null);
				}
				this.que_b = kind_type;
				BgmTransitionPoint bgmTransitionPoint3;
				if (this.que_a == BgmKind.QUE_KIND.NORMAL)
				{
					bgmTransitionPoint3 = bgmTransitionPoint2;
				}
				else
				{
					bgmTransitionPoint3 = bgmTransitionPoint;
				}
				BgmTransitionPoint.BgmTransData depBlock = bgmTransitionPoint3.GetDepBlock((byte)num);
				int num2 = 0;
				int num3 = 2000;
				int num4 = 2000;
				if (depBlock != null)
				{
					num3 = depBlock.fade_millisec_a;
					num4 = depBlock.fade_millisec_b;
					if (depBlock.block_to != null && depBlock.block_to.Length != 0)
					{
						byte[] block_to = depBlock.block_to;
						if (block_to == null || block_to.Length == 0)
						{
							num2 = 0;
						}
						else if (block_to.Length == 1)
						{
							num2 = (int)block_to[0];
						}
						else
						{
							num2 = (int)block_to[X.xors(block_to.Length)];
						}
					}
				}
				this.Exp.SetEnvelopeReleaseTime(0f);
				this.OverrideBlock = null;
				base.Stop();
				this.expb_stop = 0;
				this.ExpB.SetEnvelopeReleaseTime(0f);
				this.ExpB.Stop();
				this.ExpB.SetVolume(this.base_volume);
				this.ExpB.UpdateAll();
				this.ExpB.SetFirstBlockIndex(num2);
				if (num4 == 0 && num3 == 0)
				{
					this.ExpB.UpdateAll();
					this.PlbB = this.ExpB.Start();
					this.trs_maxt = -1f;
				}
				else
				{
					this.Exp.SetFirstBlockIndex(num);
					this.Exp.SetEnvelopeReleaseTime((float)num3);
					this.Exp.Stop();
					this.ExpB.SetVolume(this.base_volume);
					this.PlbA = this.Exp.Start();
					this.expb_stop = 10;
					this.trs_maxt = (float)(num4 * 60 / 1000);
					this.PlbB = this.ExpB.Prepare();
				}
				CriAtomExPlayer exp = this.Exp;
				this.Exp = this.ExpB;
				this.ExpB = exp;
				BgmKind.QUE_KIND que_KIND = this.que_a;
				this.que_a = this.que_b;
				this.que_b = que_KIND;
				CriAtomExPlayback plbA = this.PlbA;
				this.PlbA = this.PlbB;
				this.PlbB = plbA;
				BGM.BusChn.fine_flag = true;
				this.trs_t = 0f;
				BGM.fine_fade = true;
			}

			public BgmKind getKind()
			{
				return this.Kind;
			}

			public override float base_volume
			{
				get
				{
					return this.base_volume_without_fade * this.fade_vol / 100f;
				}
			}

			public float base_volume_without_fade
			{
				get
				{
					return SND.bgm_volume01 * (X.DEBUGNOSND ? 1f : BGM.half_level);
				}
			}

			public override void SetBusSendLevelAndOffset(string k, float v1, float levelOffset)
			{
				if (X.DEBUGNOSND)
				{
					return;
				}
				base.SetBusSendLevelAndOffset(k, v1, levelOffset);
				this.ExpB.SetBusSendLevel(k, v1);
				this.ExpB.SetBusSendLevelOffset(k, levelOffset);
			}

			public bool unload_fadeouting
			{
				get
				{
					return (this.fadespeed != 0f && this.fadedep <= 0f && this.auto_unload) || this.sheet_key == null;
				}
				set
				{
					if (value == this.unload_fadeouting)
					{
						return;
					}
					if (value)
					{
						this.auto_unload = true;
						if (!this.unload_fadeouting)
						{
							this.fadeout(0f, 120f, this.auto_unload);
							return;
						}
					}
					else
					{
						this.auto_unload = false;
					}
				}
			}

			public char current_block
			{
				get
				{
					try
					{
						return (char)(this.PlbA.GetCurrentBlockIndex() - 65);
					}
					catch
					{
					}
					return '-';
				}
			}

			public string sheet_key;

			public bool auto_unload = true;

			private BgmKind Kind;

			public BgmBlockOverride OverrideBlock;

			private float fadedep = 100f;

			private float fade_vol = 100f;

			private float fadespeed;

			private BgmKind.QUE_KIND que_a;

			private BgmKind.QUE_KIND que_b;

			private CriAtomExPlayback PlbA;

			private CriAtomExPlayback PlbB;

			private CriAtomExPlayer ExpB;

			private CriAtomExAcb MyAcb;

			private float trs_t;

			private float trs_maxt;

			public Flagger FlgBattle;

			public byte expb_stop;
		}
	}
}

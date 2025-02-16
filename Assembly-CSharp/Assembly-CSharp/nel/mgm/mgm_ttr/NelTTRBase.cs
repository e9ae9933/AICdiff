using System;
using System.Collections.Generic;
using evt;
using m2d;
using PixelLiner;
using ttr;
using UnityEngine;
using XX;

namespace nel.mgm.mgm_ttr
{
	public class NelTTRBase : IEventWaitListener, IEventListener, IRunAndDestroy
	{
		public static void LoadInitTTR(CsvReader rER)
		{
			if (rER._1 == "INIT" && NelTTRBase.Instance == null)
			{
				NelTTRBase.Instance = new NelTTRBase();
			}
		}

		private NelTTRBase()
		{
			this.MI = new MTI("MTI_mgm_ttr", null);
			this.M2D = M2DBase.Instance as NelM2DBase;
			this.MI.addLoadKey("_", true);
			EV.addListener(this);
			EV.addWaitListener("TTR", this);
			IN.addRunner(this);
			SND.loadSheets("mgm_ttr", "NelTTR");
			SND.loadSheets("BGM_mgm_ttr", "NelTTR");
		}

		public void destruct()
		{
			if (NelTTRBase.Instance == this)
			{
				NelTTRBase.Instance = null;
				this.MI.remLoadKey("_");
				IN.FlgUiUse.Rem("TTR");
				CURS.Active.Rem("TTR");
				CURS.Rem("NORMAL", "");
				if (this.M2D != null)
				{
					this.M2D.FlgHideWholeScreen.Rem("TTR");
					this.M2D.Ui.setEnabled(true);
				}
				EV.remListener(this);
				EV.remWaitListener(this);
				SND.unloadSheets("mgm_ttr", "NelTTR");
				SND.unloadSheets("BGM_mgm_ttr", "NelTTR");
				MTR.releaseTutoMaterial(true, false);
				PxlsLoader.disposeCharacter(this.PcTtr.title, true);
				if (this.GM != null)
				{
					this.GM.destruct();
					IN.DestroyE(this.GM.gameObject);
					IN.DestroyE(this.MCam.gameObject);
				}
				if (this.GobPreCam != null)
				{
					this.GobPreCam.SetActive(true);
				}
				if (MTR.FontAad != null)
				{
					MTRX.remFontStorageBundled(MTR.FontAad);
					MTR.FontAad = null;
				}
				this.GM = null;
			}
		}

		public override string ToString()
		{
			return "TTRBase";
		}

		public bool run(float fcnt)
		{
			if (this.stt == NelTTRBase.STATE.LOADING)
			{
				if (this.MI.isAsyncLoadFinished())
				{
					this.stt = NelTTRBase.STATE.LOADED;
					this.PcTtr = PxlsLoader.loadCharacterASync("_icons_ttr", this.MI.Load<TextAsset>("_icons_ttr.pxls"), 64f, false);
					MTR.prepareShaderInstance();
				}
			}
			else if (this.stt != NelTTRBase.STATE.LOADED)
			{
				if (this.stt == NelTTRBase.STATE.LOADED_PREPARED)
				{
					if (this.PcTtr.isLoadCompleted())
					{
						IN.FlgUiUse.Add("TTR");
						CURS.Active.Add("TTR");
						CURS.Set("NORMAL", "tl_cross");
						this.stt = NelTTRBase.STATE.PLAYING;
						this.GobPreCam = GameObject.Find("Main Camera");
						if (this.GobPreCam != null)
						{
							this.GobPreCam.SetActive(false);
						}
						if (this.M2D != null)
						{
							this.M2D.FlgHideWholeScreen.Add("TTR");
							this.M2D.Ui.setEnabled(false);
							this.M2D.hideAreaTitle(true);
						}
						this.GM = IN.CreateGob(null, "-TTRGM").AddComponent<TTRGameMain>();
						this.MCam = global::UnityEngine.Object.Instantiate<GameObject>(this.MI.Load<GameObject>("TTR Main Camera")).GetComponent<Camera>();
						GameObject gameObject = this.MI.Load<GameObject>("Dice6/Dice_d6_Plastic Glossy Pure write.prefab");
						this.GM.ReplaceAiScript = this.MI.Load<TextAsset>("DataTTR/ai.csv");
						this.GM.ReplaceStageScript = this.MI.Load<TextAsset>("DataTTR/stage.csv");
						GameObject gameObject2 = this.MI.Load<GameObject>("PrefabTTROnline");
						this.GM.PreparePrefabs(gameObject, gameObject2);
						this.GM.show_copyright = false;
						this.GM.PadVib = NEL.Instance.Vib;
						if (M2DBase.Instance != null)
						{
							this.GM.PHld = COOK.Mgm.PHldTTR;
						}
						this.GM.Aalloc_target = this.Aalloc_target;
						this.GM.selectable = true;
						this.GM.FD_FinishFight = new TTRResultWindow.FnFinishFight(this.fnFinishFight);
						MTR.FontAad = new MFont(this.MI, "Font/Aadhunik.ttf", null);
						MTRX.addFontStorageBundled(MTR.FontAad, new FontStorageAadhunik(MTR.FontAad, this.MI.Load<TextAsset>("Font/letterspace_aadhunik.txt")));
						MTR.initTutoMaterial(this.PcTtr, true);
						MTR.createStoneMesh(this.PcTtr.getPoseByName("pattern_ttr").getSequence(0), 0);
						if (!MTR.prepared)
						{
							EffectItemTTR.initParticleTypeTTR();
							using (BList<string> blist = ListBuffer<string>.Pop(0))
							{
								blist.Add("magic_basic");
								EfParticleManager.addAdditionalFile("ttr", this.MI.Load<TextAsset>("DataTTR/ttr_gamemain.particle.csv"), blist);
							}
						}
						this.GM.StartCamera(this.MCam);
					}
				}
				else
				{
					NelTTRBase.STATE state = this.stt;
				}
			}
			return true;
		}

		public bool EvtWait(bool is_first = false)
		{
			if (this.stt == NelTTRBase.STATE.PLAYING)
			{
				if (!this.GM.enabled)
				{
					EV.stopGMain(false);
					EV.StopGMainDrawFlag(false);
					EV.getVariableContainer().define("_final_result", this.GM.final_result.ToString(), true);
					this.destruct();
					return false;
				}
				if (this.wait_key == "INIT" && this.GM.isLoaded())
				{
					return false;
				}
			}
			else if (this.stt == NelTTRBase.STATE.LOADED)
			{
				this.stt = NelTTRBase.STATE.LOADED_PREPARED;
			}
			return true;
		}

		public void fnFinishFight(TTRRecord.TTRRecordEntry Entry, TTRPlayer PrWinner, TTRPlayer PrLoser)
		{
			if ((PrWinner.is_online_player || PrLoser.is_online_player || PrWinner.is_cpu || PrLoser.is_cpu) && PrWinner.is_manual != PrLoser.is_manual)
			{
				if (!PrWinner.is_online_player && !PrLoser.is_online_player)
				{
					GF.setB("TTR_ALLOC_ONLINE", true);
				}
				if (PrWinner.is_manual)
				{
					COOK.CurAchive.Add(ACHIVE.MENT.ttr_win, 1);
					return;
				}
				COOK.CurAchive.Add(ACHIVE.MENT.ttr_lose, 1);
			}
		}

		public bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			if (rER.cmd != "MGM_4ASCEND")
			{
				return false;
			}
			string _ = rER._1;
			if (_ != null)
			{
				if (_ == "INIT")
				{
					if (this.stt < NelTTRBase.STATE.PLAYING)
					{
						this.wait_key = "INIT";
						EV.initWaitFn(this, 0);
						if (rER.clength >= 2)
						{
							this.Aalloc_target = new List<string>(rER.slice(2, -1000));
						}
					}
					return true;
				}
				if (_ == "PLAY")
				{
					this.wait_key = "PLAY";
					EV.stopGMain(true);
					EV.StopGMainDrawFlag(true);
					EV.initWaitFn(this, 0);
					return true;
				}
				if (_ == "DEFINE_SCORE_EXISTS")
				{
					if (!GF.getB("TTR_ALLOC_ONLINE") && COOK.Mgm.PHldTTR.hasData() && TTRRecordContainer.hasAnyNpcBattleRecord(COOK.Mgm.PHldTTR))
					{
						GF.setB("TTR_ALLOC_ONLINE", true);
					}
					return true;
				}
			}
			return true;
		}

		public bool EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		public bool EvtClose(bool is_first_or_end)
		{
			this.destruct();
			return true;
		}

		public int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		public bool EvtMoveCheck()
		{
			return true;
		}

		private static NelTTRBase Instance;

		public const string gfb_name_alloc_online = "TTR_ALLOC_ONLINE";

		public const string ev_cmd = "MGM_4ASCEND";

		private string wait_key;

		private NelTTRBase.STATE stt;

		private readonly NelM2DBase M2D;

		private List<string> Aalloc_target;

		private TTRGameMain GM;

		private Camera MCam;

		private GameObject GobPreCam;

		private MTI MI;

		private PxlCharacter PcTtr;

		private enum STATE : byte
		{
			LOADING,
			LOADED,
			LOADED_PREPARED,
			PLAY_PREPARED,
			PLAYING,
			QUITED
		}
	}
}

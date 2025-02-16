using System;
using System.Collections.Generic;
using evt;
using nel.smnp;
using XX;

namespace nel.mgm.smncr
{
	public class SummonerForCreator : EnemySummoner
	{
		public SummonerForCreator(string _key)
			: base(_key)
		{
			this.dropable_special_item = false;
		}

		public override CsvReaderA initializeCR(List<string> Aenemy, bool force_reload = false)
		{
			if (!this.smnc_active)
			{
				if (this.preload_script_active)
				{
					return base.initializeCR(Aenemy, true);
				}
				return null;
			}
			else
			{
				if (!force_reload && this.Aload_enemy.Count > 0)
				{
					if (Aenemy != null && Aenemy != this.Aload_enemy)
					{
						for (int i = this.Aload_enemy.Count - 1; i >= 0; i--)
						{
							X.pushIdentical<string>(Aenemy, this.Aload_enemy[i]);
						}
					}
					return null;
				}
				this.need_resource_load = true;
				SmncFile currentFile = this.LpS.getCurrentFile();
				for (int j = currentFile.Aen_list.Count - 1; j >= 0; j--)
				{
					X.pushIdentical<string>(Aenemy, currentFile.Aen_list[j].id_tostr);
				}
				this.fineReelData(this.key, ref this.AReel, ref this.AReelSecretSrc, ref this.replace_secret_to_lower);
				return null;
			}
		}

		protected override CsvReaderA createCR()
		{
			if (this.preload_script_active)
			{
				EnemySummonerManager.SDescription sdescription;
				return new CsvReaderA(EnemySummonerManager.GetManager("_other").getSummonerScript(this.target_script, out sdescription, false), new CsvVariableContainer(EV.getVariableContainer()));
			}
			return null;
		}

		public override void fineReelData(string key, ref ReelManager.ItemReelDrop[] AReel, ref ReelManager.ItemReelContainer[] AReelSecretSrc, ref float replace_secret_to_lower)
		{
			AReel = null;
			AReelSecretSrc = null;
			replace_secret_to_lower = 0f;
		}

		public bool preload_script_active
		{
			get
			{
				return !this.smnc_active && TX.valid(this.target_script);
			}
		}

		public bool smnc_active
		{
			get
			{
				return this.LpS != null && this.LpS.getCurrentFile() != null;
			}
		}

		public override void activate(M2LpSummon _Lp, out bool bgm_replaced)
		{
			if (this.preload_script_active)
			{
				base.activate(_Lp, out bgm_replaced);
				return;
			}
			bgm_replaced = false;
			if (_Lp is M2LpUiSmnCreator)
			{
				this.Aload_enemy.Clear();
				this.QEntry = default(QuestTracker.SummonerEntry);
				this.LpS = _Lp as M2LpUiSmnCreator;
				base.loadMaterial(_Lp.Mp, false);
				base.activateInner(_Lp, out bgm_replaced, new EnemySummoner.FnCreateSummonerPlayer(this.fnCreateSummonerPlayerForC));
			}
		}

		private SummonerPlayer fnCreateSummonerPlayerForC(EnemySummoner _Summoner, EfParticleFuncCalc _FuncBase, CsvReaderA _CR, out bool bgm_replaced)
		{
			return new SummonerPlayerForCreator(this, this.LpS, _FuncBase, _CR, out bgm_replaced, true);
		}

		protected override string defeat_ev_var2(int obtainable)
		{
			return "2";
		}

		protected override string defeat_after_event
		{
			get
			{
				if (this.LpS == null || !this.LpS.summoner_openable)
				{
					return "";
				}
				return this.LpS.ev_returnback;
			}
		}

		public const string smnc_header = "SmnC_";

		private M2LpUiSmnCreator LpS;

		private const string defeat_after_event_default_smnc = "___Other/smnc_after_def";

		public string target_script;
	}
}

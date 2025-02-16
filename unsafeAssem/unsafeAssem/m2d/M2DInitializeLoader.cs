using System;
using PixelLiner;
using XX;

namespace m2d
{
	public class M2DInitializeLoader
	{
		public M2DInitializeLoader(M2DBase _M2D, PAUSER _Pauser)
		{
			this.M2D = _M2D;
			this.Pauser = _Pauser;
		}

		public bool loadBasicMaterialProgress(int cnt = -1)
		{
			while (cnt < 0 || --cnt >= 0)
			{
				if (this.pstate == M2DInitializeLoader.PREPARE.NO_LOAD)
				{
					string text = NKT.readStreamingText("m2d/__m2d_list" + this.ext_list_dat, false);
					this.LoadMapList(text);
					M2DBase.getShaderContainer().addLoadKey("M2D", true);
					cnt = X.Mn(cnt, 0);
					this.pstate = M2DInitializeLoader.PREPARE.PREPARE_CHIPS;
					Bench.mark("loadM2D - LoadChipList", false, false);
				}
				else if (this.pstate == M2DInitializeLoader.PREPARE.PREPARE_CHIPS)
				{
					PxlsLoader.loadSpeed = X.Mn(300f, PxlsLoader.loadSpeed);
					this.M2D.IMGS.Atlas.initCspAtlas(M2DBase.Achip_pxl_key[0]);
					if (this.M2D.fnYSortFunction == null)
					{
						this.M2D.fnYSortFunction = (M2DrawItem Im, M2DrawItem Mv) => M2DBase.fnYSortFunctionDefault(Im, Mv);
					}
					IN.AssignPauseable(this.Pauser);
					this.M2D.IMGS.prepareChipsScript();
					this.pstate = M2DInitializeLoader.PREPARE.PREPARE_CHIPS_READING;
				}
				else if (this.pstate == M2DInitializeLoader.PREPARE.PREPARE_CHIPS_READING)
				{
					if (!this.M2D.IMGS.progressChipsScriptReadOld((cnt < 0) ? (-1) : (cnt + 1)))
					{
						this.pstate = M2DInitializeLoader.PREPARE.PREPARE_CHIPS_COMP;
						Bench.mark("loadM2D - LoadShader", false, false);
					}
					else
					{
						cnt = X.Mn(cnt, 0);
					}
				}
				else if (this.pstate == M2DInitializeLoader.PREPARE.PREPARE_CHIPS_COMP)
				{
					MTI shaderContainer = M2DBase.getShaderContainer();
					if (!shaderContainer.isAsyncLoadFinished())
					{
						return true;
					}
					shaderContainer.LoadAllShader();
					this.pstate = M2DInitializeLoader.PREPARE.LOAD_AFTER;
				}
				else
				{
					if (this.pstate == M2DInitializeLoader.PREPARE.LOAD_AFTER)
					{
						Bench.mark("loadM2D - LoadAfter", false, false);
						this.M2D.LoadAfter();
						Bench.mark(null, false, false);
						this.pstate = M2DInitializeLoader.PREPARE.INIT_FIRST_EVENT;
						this.M2D.transferring_game_stopping = true;
						break;
					}
					break;
				}
			}
			if (this.pstate == M2DInitializeLoader.PREPARE.INIT_FIRST_EVENT && this.M2D.GobBase != null)
			{
				this.pstate = M2DInitializeLoader.PREPARE.COMPLETE;
			}
			return this.pstate < M2DInitializeLoader.PREPARE.INIT_FIRST_EVENT;
		}

		private void LoadMapList(string lt_text)
		{
			NDic<Map2d> mapObject = this.M2D.getMapObject();
			CsvReader csvReader = new CsvReader(lt_text, CsvReader.RegSpace, true);
			while (csvReader.read())
			{
				string text = X.basename_noext(csvReader.cmd);
				if (!(text == ""))
				{
					mapObject[text] = new Map2d(this.M2D, text, false);
				}
			}
			mapObject.scriptFinalize();
		}

		private M2DInitializeLoader.PREPARE pstate;

		private readonly M2DBase M2D;

		public string ext_list_dat = ".dat";

		private PAUSER Pauser;

		protected enum PREPARE
		{
			NO_LOAD,
			PREPARE_CHIPS,
			PREPARE_CHIPS_READING,
			PREPARE_CHIPS_COMP,
			LOAD_AFTER,
			INIT_FIRST_EVENT,
			COMPLETE
		}
	}
}

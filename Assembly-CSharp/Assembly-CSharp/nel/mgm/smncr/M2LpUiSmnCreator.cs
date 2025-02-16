using System;
using System.Collections.Generic;
using evt;
using m2d;
using nel.gm;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel.mgm.smncr
{
	public class M2LpUiSmnCreator : M2LpSummon
	{
		public UiSmnCreator CrtBase { get; private set; }

		public M2LpUiSmnCreator(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay, new SummonerForCreator(_key))
		{
			base.type_no_weedsecure_after_defeat = true;
		}

		protected override void initActionPreSummon(ref NightController.SummonerData NInfo, ref int defeated_count, out M2LpSummon.SUDDEN is_sudden)
		{
			try
			{
				base.nM2D.NightCon.applyDangerousFromEvent(0, true, false, false);
			}
			catch
			{
			}
			base.nM2D.NightCon.clearWeather();
			if (NInfo == null)
			{
				NInfo = new NightController.SummonerData(null, 0);
			}
			defeated_count = 0;
			is_sudden = M2LpSummon.SUDDEN.NORMAL;
			string[] array = this.Meta.Get("type");
			this.auto_save_on_opening_summoner = this.Meta.GetB("auto_save", false);
			SmncStageEditorManager.TYPE type = (SmncStageEditorManager.TYPE)0;
			if (array != null)
			{
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					SmncStageEditorManager.TYPE type2;
					if (FEnum<SmncStageEditorManager.TYPE>.TryParse(array[i], out type2, true))
					{
						type |= type2;
					}
				}
			}
			if (type == (SmncStageEditorManager.TYPE)0)
			{
				X.de("M2LpUiSmnCreator:: type 指定が必要です", null);
				return;
			}
			if (this.SmncMng == null)
			{
				this.SmncMng = new SmncStageEditorManager(base.nM2D, type);
				return;
			}
			this.SmncMng.target_type = type;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			this.no_item_progress = true;
			this.no_gq_progress = true;
			this.need_fine_out_fill = true;
		}

		public override void closeAction(bool when_map_close)
		{
			if (this.CrtBase != null && this.CrtBase.isPreparing())
			{
				return;
			}
			base.closeAction(when_map_close);
			this.quitFile(this.CurFile);
			if (this.CrtBase != null)
			{
				this.CrtBase.destruct();
				this.CrtBase = null;
			}
		}

		public override void considerConfig4(int _l, int _t, int _r, int _b, M2Pt[,] AAPt)
		{
			base.considerConfig4(_l, _t, _r, _b, AAPt);
			if (this.CurFile != null && !this.chip_inserted)
			{
				int count = this.CurFile.Astgo.Count;
				for (int i = 0; i < count; i++)
				{
					SmncStageEditorManager.StgObject stgObject = this.CurFile.Astgo[i];
					for (int j = 0; j < stgObject.Mp.clms; j++)
					{
						int num = (stgObject.flip ? (stgObject.Mp.clms - 1 - j) : j);
						int num2 = stgObject.x + this.mapx + j;
						if (X.BTW((float)_l, (float)num2, (float)_r))
						{
							for (int k = 0; k < stgObject.Mp.rows; k++)
							{
								int num3 = stgObject.y + this.mapy + k;
								if (X.BTW((float)_t, (float)num3, (float)_b))
								{
									CCON.calcConfigManual(ref AAPt[num2, num3], stgObject.Mp.getConfig(num, k));
								}
							}
						}
					}
				}
			}
			if (this.CurFile != null && this.need_fine_out_fill && this.chip_inserted && base.isActiveBorder())
			{
				this.need_fine_out_fill = false;
				if (this.OutFill == null)
				{
					this.OutFill = new SmncOutFillAssist(this);
				}
				this.OutFill.considerConfig4(AAPt);
			}
		}

		public void insertChipsDecide()
		{
			this.removeDecidedChips();
			if (this.CurFile != null && this.SmncMng != null)
			{
				this.need_fine_out_fill = true;
				base.nM2D.no_publish_juice = true;
				if (this.AMaPrevious == null)
				{
					this.AMaPrevious = new MdArranger[5];
					for (int i = 0; i < 5; i++)
					{
						MeshDrawer meshDrawer = this.id2Md(i);
						this.AMaPrevious[i] = new MdArranger(meshDrawer);
					}
				}
				for (int j = 0; j < 5; j++)
				{
					this.id2Md(j).chooseSubMesh(1, false, false);
					this.AMaPrevious[j].Set(true);
				}
				this.chip_inserted_first = this.Lay.count_chips;
				int num = this.CurFile.Astgo.Count;
				for (int k = 0; k < num; k++)
				{
					SmncStageEditorManager.StgObject stgObject = this.CurFile.Astgo[k];
					int count_chips = stgObject.count_chips;
					int num2 = (int)((float)(this.mapx + stgObject.x) * base.CLEN);
					int l = 0;
					while (l < count_chips)
					{
						M2Puts puts = stgObject.getPuts(l);
						int num3 = puts.drawx + num2;
						int num4 = puts.drawy + (int)((float)(this.mapy + stgObject.y) * base.CLEN);
						bool flip = puts.flip;
						int rotation = puts.rotation;
						if (stgObject.flip)
						{
							num3 = puts.Mp.width - puts.drawx - puts.iwidth + num2;
							puts.getFlippedRotation(out rotation, out flip);
						}
						M2Puts m2Puts;
						if (puts is M2Chip)
						{
							m2Puts = this.Lay.MakeChip(puts.Img, num3, num4, puts.opacity, rotation, flip) as M2Chip;
							goto IL_01D7;
						}
						if (puts is M2Picture)
						{
							m2Puts = this.Lay.MakePicture(puts.Img, num3, num4, puts.opacity, rotation, flip, -1);
							goto IL_01D7;
						}
						IL_01E9:
						l++;
						continue;
						IL_01D7:
						this.Lay.assignNewMapChip(m2Puts, -2, true, false);
						goto IL_01E9;
					}
				}
				if ((this.SmncMng.target_type & SmncStageEditorManager.TYPE.GARAGE) != (SmncStageEditorManager.TYPE)0)
				{
					num = this.CurFile.Aplant.Count;
					for (int m = 0; m < num; m++)
					{
						SmncFile.PlantInfo plantInfo = this.CurFile.Aplant[m];
						M2ChipImage m2ChipImage = ((this.CrtBase != null) ? this.CrtBase.getPlantImage(plantInfo) : null) ?? plantInfo.getImage(base.nM2D);
						if (m2ChipImage != null)
						{
							Vector2 mapPos = plantInfo.getMapPos(this);
							M2Chip m2Chip = this.Lay.MakeChip(m2ChipImage, (int)(mapPos.x * base.CLEN - (float)m2ChipImage.iwidth * 0.5f), (int)(mapPos.y * base.CLEN - (float)m2ChipImage.iheight), 255, 0, plantInfo.is_flip) as M2Chip;
							this.Lay.assignNewMapChip(m2Chip, -2, true, false);
						}
					}
				}
				for (int n = this.Lay.count_chips - 1; n >= this.chip_inserted_first; n--)
				{
					this.Lay.getChipByIndex(n).initAction(true);
				}
				this.Mp.considerConfig4(this.mapx, this.mapy, this.mapw + this.mapx, this.maph + this.mapy);
				this.Mp.need_update_collider = true;
			}
		}

		public void removeDecidedChips()
		{
			if (!this.chip_inserted)
			{
				return;
			}
			this.need_fine_out_fill = true;
			int num = 0;
			for (int i = this.Lay.count_chips - 1; i >= this.chip_inserted_first; i--)
			{
				M2Puts chipByIndex = this.Lay.getChipByIndex(i);
				num |= chipByIndex.get_update_flag();
				this.Lay.removeChip(chipByIndex, true, true, true);
			}
			this.Mp.addUpdateMesh(num, false);
			this.Mp.considerConfig4(this.mapx, this.mapy, this.mapw + this.mapx, this.maph + this.mapy);
			this.Mp.need_update_collider = true;
			this.chip_inserted_first = -1;
			for (int j = 0; j < 5; j++)
			{
				this.id2Md(j).chooseSubMesh(1, false, false);
				this.AMaPrevious[j].revertVerAndTriIndexFirstSaved(false);
			}
		}

		private MeshDrawer id2Md(int id)
		{
			MdMap mdMap;
			switch (id)
			{
			case 0:
				mdMap = this.Mp.MyDrawerB;
				break;
			case 1:
				mdMap = this.Mp.MyDrawerG;
				break;
			case 2:
				mdMap = this.Mp.MyDrawerT;
				break;
			case 3:
				mdMap = this.Mp.MyDrawerL;
				break;
			default:
				mdMap = this.Mp.MyDrawerTT;
				break;
			}
			return mdMap;
		}

		public override MANA_HIT deassignActiveWeed(M2ManaWeed _Weed, AttackInfo AtkHitExecute = null)
		{
			if (base.isActive())
			{
				return base.deassignActiveWeed(_Weed, AtkHitExecute);
			}
			return MANA_HIT.NOUSE;
		}

		public bool summoner_openable
		{
			get
			{
				return this.CrtBase != null && this.CurFile != null;
			}
		}

		internal bool openSummoner(int danger, uint weather)
		{
			if (!this.summoner_openable)
			{
				return false;
			}
			if (this.Xors == null)
			{
				this.Xors = new XorsMaker(false);
			}
			NightController nightCon = base.nM2D.NightCon;
			this.pre_danger = nightCon.getDangerMeterVal(false, false);
			nightCon.applyDangerousFromEvent(danger, true, false, true);
			nightCon.initTemporaryWeather("_SMNC", (int)weather);
			this.CrtBase.hideTemporaryMesh();
			this.insertChipsDecide();
			base.nM2D.no_publish_juice = true;
			int num;
			SmncStageEditorManager.StgObject stgObject = this.CurFile.FindStgo("_smnc_generate_pr", out num);
			if (stgObject.valid && this.Mp.Pr != null)
			{
				this.Mp.Pr.setTo((float)(stgObject.x + this.mapx) + 0.5f, (float)(stgObject.y + this.mapy + stgObject.rows) - this.Mp.Pr.sizey);
			}
			uint num2 = this.CurFile.rand_seed;
			if (num2 != 0U)
			{
				num2 ^= 3413251945U;
			}
			else
			{
				num2 = X.xors();
			}
			if (this.BaXorsCache == null)
			{
				this.BaXorsCache = new ByteArray(32U);
			}
			this.BaXorsCache.Clear();
			NightController.Xors.writeBinaryTo(this.BaXorsCache);
			this.Xors.init(false, num2, 0U, 0U, 0U);
			NightController.Xors.init(false, num2 ^ 1731852235U, 0U, 0U, 0U);
			base.openSummoner(null, null, false);
			X.dl("Battle start: W" + weather.ToString() + " / D" + danger.ToString(), null, false, false);
			return true;
		}

		public override void closeSummoner(bool defeated, out bool is_first_defeat)
		{
			if (this.CurFile != null)
			{
				if (this.BaXorsCache != null && this.BaXorsCache.Length > 0UL)
				{
					this.BaXorsCache.position = 0UL;
					NightController.Xors.readBinaryFrom(this.BaXorsCache, false);
					this.BaXorsCache.Clear();
				}
				if (!defeated)
				{
					this.popDangerLevel();
				}
			}
			base.closeSummoner(defeated, out is_first_defeat);
		}

		public override Vector2 RevertPosPr
		{
			get
			{
				if (this.CurFile != null)
				{
					int num;
					SmncStageEditorManager.StgObject stgObject = this.CurFile.FindStgo("_smnc_generate_pr", out num);
					if (stgObject.valid)
					{
						return stgObject.getMapCenterPos(this);
					}
				}
				return base.RevertPosPr;
			}
		}

		public void popDangerLevel()
		{
			if (this.pre_danger >= 0)
			{
				NightController nightCon = base.nM2D.NightCon;
				nightCon.initTemporaryWeather("_SMNC", -1);
				nightCon.applyDangerousFromEvent(this.pre_danger, true, false, true);
				this.pre_danger = -1;
			}
		}

		public void shuffle<T>(List<T> A, int arraymax = -1)
		{
			this.Xors.shuffle<T>(A, arraymax);
		}

		protected override M2LpSummon.ACTV_EFFECT getActvEffectType(bool sudden_effect)
		{
			return M2LpSummon.ACTV_EFFECT.SMNC;
		}

		public override string summoned_prepare_ptc_key
		{
			get
			{
				return "smnc_en_summoned_prepare";
			}
		}

		public override bool EvtRead(EvReader ER, StringHolder rER, int skipping)
		{
			if (rER.cmd == "SMNCREATOR")
			{
				string _ = rER._1;
				if (_ != null)
				{
					if (!(_ == "TARGET_SCRIPT"))
					{
						if (!(_ == "PREPARE"))
						{
							if (!(_ == "DEASSIGN_CHIPS"))
							{
								if (_ == "POP_DANGER_LEVEL")
								{
									this.flushManaAndStain();
									this.popDangerLevel();
								}
							}
							else
							{
								this.quitFile(this.CurFile);
								if (this.CrtBase != null)
								{
									this.CrtBase.initFileSelection(this.CurFile, true);
								}
							}
						}
						else
						{
							if (this.CrtBase == null)
							{
								this.CrtBase = new UiSmnCreator(null, this)
								{
									enabled_file_export = true
								};
							}
							(this.Reader as SummonerForCreator).target_script = null;
							this.flushManaAndStain();
							this.CrtBase.eventActivation(rER);
						}
					}
					else
					{
						if (this.CrtBase != null)
						{
							this.CrtBase.closeFileSelection();
						}
						this.quitFile(this.CurFile);
						(this.Reader as SummonerForCreator).target_script = rER._2;
					}
				}
				return true;
			}
			return base.EvtRead(ER, rER, skipping);
		}

		private void flushManaAndStain()
		{
			base.nM2D.Mana.ClearAll();
			base.nM2D.STAIN.clear();
			base.nM2D.MGC.FindMg(delegate(MagicItem Mg, M2MagicCaster _Caster)
			{
				if (Mg.Other is IMgBombListener)
				{
					Mg.kill(-1f);
				}
				return false;
			}, null);
		}

		public override bool EvtTargetIsMe(string t)
		{
			return t == this.key;
		}

		public override bool EvtClose(bool is_first_or_end)
		{
			if (is_first_or_end && this.CrtBase != null)
			{
				this.CrtBase.deactivate();
			}
			return base.EvtClose(is_first_or_end);
		}

		public override bool wm_icon_enabled
		{
			get
			{
				return false;
			}
		}

		public M2LpUiSmnCreator initEdit(UiSmnCreator _CrtBase, out SmncStageEditorManager _SmncMng)
		{
			this.CrtBase = _CrtBase;
			_SmncMng = this.SmncMng;
			return this;
		}

		public SmncFile getCurrentFile()
		{
			return this.CurFile;
		}

		public void initFile(SmncFile File)
		{
			if (File == this.CurFile)
			{
				return;
			}
			this.quitFile(this.CurFile);
			this.CurFile = File;
			this.need_fine_out_fill = true;
		}

		public void quitFile(SmncFile File)
		{
			if (this.CurFile != null && File == this.CurFile)
			{
				this.CurFile = null;
				this.removeDecidedChips();
			}
		}

		public override bool cannot_open_summoner
		{
			get
			{
				return true;
			}
		}

		public bool chip_inserted
		{
			get
			{
				return this.chip_inserted_first >= 0;
			}
		}

		public override string getTabIconKey(UiGMCStat Gmc, out uint color)
		{
			color = uint.MaxValue;
			if (!base.isActiveBorder() && this.t_ui <= 0f)
			{
				return null;
			}
			return "IconTigrina";
		}

		public override string abortButtonTXKeyForGM(UiGMCStat Gmc)
		{
			if (this.CurFile != null)
			{
				return "&&Status_Tab_smnc_abort";
			}
			return base.abortButtonTXKeyForGM(Gmc);
		}

		public override bool createGMDesc(UiGMCStat Gmc, Designer Ds, out aBtn FirstBtn, out aBtn LastBtn)
		{
			return base.createGMDesc(Gmc, Ds, out FirstBtn, out LastBtn);
		}

		public override string abortEventFromGM()
		{
			if (this.CurFile != null)
			{
				return "___Tigrina/___abort_summoner";
			}
			return base.abortEventFromGM();
		}

		public const string smnc_header = "SmnC_";

		public const string evt_abort_smnc = "___Tigrina/___abort_summoner";

		private SmncStageEditorManager SmncMng;

		private SmncFile CurFile;

		public int chip_inserted_first = -1;

		private MdArranger[] AMaPrevious;

		private const int MA_MAX = 5;

		private XorsMaker Xors;

		private bool need_fine_out_fill = true;

		private SmncOutFillAssist OutFill;

		private ByteArray BaXorsCache;

		private int pre_danger = -1;

		public bool auto_save_on_opening_summoner;

		private const string ev_cmd = "SMNCREATOR";

		public string ev_returnback;
	}
}

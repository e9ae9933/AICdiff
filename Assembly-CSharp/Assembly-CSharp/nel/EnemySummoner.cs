using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class EnemySummoner
	{
		public static void initEnemySummoner()
		{
			EnemySummoner.ActiveScript = null;
			if (EnemySummoner.ORecord != null)
			{
				return;
			}
			EnemySummoner.ORecord = new BDic<string, EnemySummoner>();
			EnemySummoner.FuncBase = new EfParticleFuncCalc(null, "ZLINE", 1f);
		}

		public static EnemySummoner Get(string key, bool no_make = true)
		{
			EnemySummoner enemySummoner;
			if (EnemySummoner.ORecord.TryGetValue(key, out enemySummoner))
			{
				return enemySummoner;
			}
			if (no_make)
			{
				return null;
			}
			string resource = TX.getResource("Data/summon/" + key + ".summon", ".csv", false);
			if (resource == null)
			{
				return null;
			}
			enemySummoner = (EnemySummoner.ORecord[key] = new EnemySummoner(key, resource));
			return enemySummoner;
		}

		public static void FlushAll()
		{
			foreach (KeyValuePair<string, EnemySummoner> keyValuePair in EnemySummoner.ORecord)
			{
				keyValuePair.Value.flush_memory(true, true);
			}
		}

		public static void addSummonerListener(EnemySummoner.ISummonActivateListener Smn)
		{
			if (EnemySummoner.ASummonerLitener.IndexOf(Smn) == -1)
			{
				EnemySummoner.ASummonerLitener.Add(Smn);
			}
		}

		public static void remSummonerListener(EnemySummoner.ISummonActivateListener Smn)
		{
			EnemySummoner.ASummonerLitener.Remove(Smn);
		}

		public static void initS()
		{
			EnemySummoner.ASummonerLitener.Clear();
			EnemySummoner.clearLastSummoner();
		}

		public static void clearLastSummoner()
		{
			EnemySummoner.LastSummoner = null;
		}

		public EnemySummoner(string _key, string text)
		{
			this.key = _key;
			this.base_script = text;
			this.AASummoned = new List<List<NelEnemy>>();
			if (EnemySummoner.OFirstPos == null)
			{
				EnemySummoner.OFirstPos = new BDic<NelEnemy, EnemySummoner.EnemySummonedInfo>();
				EnemySummoner.VPTemp = new VariableP(1);
			}
			this.AIdListAbout = new List<ENEMYID>();
			this.AOverdrived = new List<NelEnemy>(4);
			this.Aload_enemy = new List<string>(2);
		}

		public void releaseLp()
		{
			this.Lp = null;
			this.AABccFootable = null;
		}

		public EnemySummoner flush_memory(bool is_initAction = true, bool clear_cr = false)
		{
			this.about_enemy_count_ = -1;
			this.obtainable_grade_ = -1;
			this.need_resource_load = true;
			if (is_initAction)
			{
				this.WRC_Air = null;
				this.AABccFootable = null;
				this.is_dangerous_ = 2;
			}
			if (clear_cr)
			{
				this.CR = null;
			}
			return this;
		}

		public CsvReaderA initializeCR(List<string> Aenemy, bool force_reload = false)
		{
			if (this.CR != null && !force_reload)
			{
				if (Aenemy != null && Aenemy != this.Aload_enemy)
				{
					for (int i = this.Aload_enemy.Count - 1; i >= 0; i--)
					{
						X.pushIdentical<string>(Aenemy, this.Aload_enemy[i]);
					}
				}
				return this.CR;
			}
			this.need_resource_load = true;
			this.use_if = false;
			this.obtainable_grade_ = -1;
			this.main_data_line = -1;
			this.only_night = false;
			this.CR = new CsvReaderA(this.base_script, new CsvVariableContainer(EV.getVariableContainer()));
			this.AReel = null;
			this.puppetrevenge_replace = null;
			while (this.CR.readCorrectly())
			{
				if (!TX.noe(this.CR.getLastStr()) && this.CR.stringsInput(this.CR.getLastStr()))
				{
					if (this.CR.cmd.IndexOf("#") == 0)
					{
						if (this.CR.cmd == "#GRADE")
						{
							this.grade = this.CR.Int(1, 0);
						}
						if (this.CR.cmd == "#NIGHT")
						{
							this.only_night = this.CR.NmE(1, 1f) != 0f;
						}
						if (this.CR.cmd == "#THUNDER_ODABLE")
						{
							this.thunder_odable_def = X.Mx(this.CR.Int(1, 0), 1);
						}
						if (this.CR.cmd == "#REEL")
						{
						}
					}
					else
					{
						if (this.main_data_line <= -1)
						{
							this.main_data_line = this.CR.get_cur_line();
						}
						if ((this.CR.cmd == "%EN" || this.CR.cmd == "%EN_OD") && Aenemy != null)
						{
							X.pushIdentical<string>(Aenemy, this.CR._1);
						}
						if (this.CR.cmd == "%PUPPETREVENGE")
						{
							this.puppetrevenge_replace = this.CR._1;
							if (this.puppetrevenge_replace == "")
							{
								this.puppetrevenge_replace = null;
							}
						}
						if (this.CR.cmd == "IF")
						{
							this.use_if = true;
						}
						if (this.CR.cmd == "%NEXT_SCRIPT")
						{
							EnemySummoner enemySummoner = EnemySummoner.Get(this.CR._1, true);
							if (enemySummoner != null)
							{
								this.use_if = true;
								enemySummoner.initializeCR(Aenemy, force_reload);
							}
						}
					}
				}
			}
			this.fineReelData(this.key, ref this.AReel, ref this.AReelSecretSrc, ref this.replace_secret_to_lower);
			if (this.main_data_line <= -1)
			{
				this.CR = null;
				this.need_resource_load = true;
				return null;
			}
			return this.CR;
		}

		public void fineReelData(string key, ref ReelManager.ItemReelDrop[] AReel, ref ReelManager.ItemReelContainer[] AReelSecretSrc, ref float replace_secret_to_lower)
		{
			ReelManager.ItemReelContainer[] array;
			float num;
			if (SupplyManager.GetForSummoner(key, out array, out AReelSecretSrc, out num))
			{
				AReel = new ReelManager.ItemReelDrop[array.Length];
				replace_secret_to_lower = num;
				int num2 = -1;
				for (int i = array.Length - 1; i >= 0; i--)
				{
					AReel[i] = new ReelManager.ItemReelDrop(array[i], num2);
				}
			}
		}

		public CsvReaderA loadMaterial(Map2d Mp, bool force_reload = false)
		{
			this.initializeCR(this.Aload_enemy, force_reload);
			if (this.need_resource_load)
			{
				this.need_resource_load = false;
				for (int i = this.Aload_enemy.Count - 1; i >= 0; i--)
				{
					if (!EnemyData.getResources(Mp.M2D as NelM2DBase, this.Aload_enemy[i]))
					{
						this.CR.tError("エネミーID指定に誤りがあります！ :" + this.Aload_enemy[i]);
					}
				}
			}
			return this.CR;
		}

		public void loadMaterialPuppetRevenge(M2LpSummon Lp)
		{
			if (this.puppetrevenge_replace == null)
			{
				return;
			}
			if (this.CR == null)
			{
				this.loadMaterial(Lp.Mp, false);
			}
			Map2d mp = Lp.Mp;
			string text = Lp.nM2D.loadMaterialTxt("Data/summon/" + this.puppetrevenge_replace + ".summon");
			if (text == null)
			{
				return;
			}
			if (this.CR_puppetrevenge == null)
			{
				this.CR_puppetrevenge = new CsvReaderA(text, this.CR.VarCon);
			}
			else
			{
				this.CR_puppetrevenge.parseText(text);
			}
			CsvReaderA cr_puppetrevenge = this.CR_puppetrevenge;
			while (cr_puppetrevenge.read())
			{
				if ((cr_puppetrevenge.cmd == "%EN" || cr_puppetrevenge.cmd == "%EN_OD") && !EnemyData.getResources(Lp.nM2D, cr_puppetrevenge._1))
				{
					cr_puppetrevenge.tError("エネミーID指定に誤りがあります！ :" + cr_puppetrevenge._1);
				}
			}
		}

		public string name_localized
		{
			get
			{
				return TX.Get("Summoner_" + this.key, "");
			}
		}

		public void getDescription(Map2d Mp, STB Stb, bool left_tab = true)
		{
			string text = "";
			string text2 = "";
			List<ENEMYID> appearEnemyAbout = this.getAppearEnemyAbout();
			int count = appearEnemyAbout.Count;
			for (int i = 0; i < count; i++)
			{
				ENEMYID enemyid = appearEnemyAbout[i];
				text2 = ((text2 == "" && left_tab) ? "\u3000\u3000" : "") + TX.add(text2, EnemyData.getEnemyName(enemyid), ", ");
				if ((i + 1) % 3 == 0)
				{
					text = TX.add(text, text2, "\n");
					text2 = "";
				}
			}
			if (text2 != "")
			{
				text = TX.add(text, text2, "\n\u3000\u3000");
			}
			int obtainableGrade = this.getObtainableGrade(Mp);
			Stb.AddTxA("Summoner__descryption", false);
			STB stb = TX.PopBld(null, 0);
			stb += this.grade;
			if (obtainableGrade < this.grade)
			{
				stb.AddTxA("Summoner__obtainable_dangerousness", false);
				stb.TxRpl(obtainableGrade);
			}
			Stb.TxRpl(stb);
			TX.ReleaseBld(stb);
			Stb.TxRpl(text);
			Stb.TxRpl(this.getOverdriveCapacityStr());
			if (this.only_night)
			{
				Stb.AppendTxA("Summoner_can_battle_in_night", "\n");
			}
		}

		public int calcAboutEnemyCount(List<ENEMYID> AIdList, bool clear_list = true, bool add_follower_to_list = false)
		{
			this.prepared = true;
			if (this.main_data_line <= -1)
			{
				return -1;
			}
			int cur_line = this.CR.get_cur_line();
			this.CR.seek_set(this.main_data_line);
			this.overdrive_pre_calced_count = 0;
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			int num = nelM2DBase.NightCon.summoner_enemy_count_addition(this);
			int num2 = 0;
			bool flag = false;
			if (clear_list)
			{
				AIdList.Clear();
			}
			bool flag2 = false;
			while (this.CR.read())
			{
				if (!(this.CR.cmd == "/*") && !(this.CR.cmd == "/*___"))
				{
					if (this.CR.cmd == "%ADD_COUNT")
					{
						string text = this.CR.slice_join(1, " ", "");
						if (TX.isStart(text, "!", 0))
						{
							text = TX.slice(text, 1);
							num = 0;
						}
						num += (int)this.CalcScript(text, "0", "ZLINE");
					}
					else if (this.CR.cmd == "%EN_HR")
					{
						flag2 = TX.isStart(this.CR._1, "_FOLLOW_", 0);
					}
					else
					{
						if ((this.CR.cmd == "%EN" || this.CR.cmd == "%EN_OD") && this.CR.IntE(5, 1) != 0)
						{
							EnemyData.EnemyDescryption typeAndId = EnemyData.getTypeAndId(this.CR._1);
							if (typeAndId == null)
							{
								continue;
							}
							ENEMYID enemyid = (ENEMYID)typeAndId.id;
							int num3;
							this.getEnemyCountFromScript(this.CR, out num3, ref num, nelM2DBase.NightCon);
							if (num3 >= 1)
							{
								num2 += num3;
								if (this.CR.cmd == "%EN_OD")
								{
									enemyid |= (ENEMYID)2147483648U;
									this.overdrive_pre_calced_count += num3;
								}
								if ((!flag2 || add_follower_to_list) && AIdList != null && AIdList.IndexOf(enemyid) == -1)
								{
									AIdList.Add(enemyid);
								}
							}
							if (!flag && this.is_dangerous_ == 2 && typeAndId.FnDangerous != null)
							{
								flag = typeAndId.FnDangerous();
							}
						}
						if (this.CR.cmd == "%NEXT_SCRIPT")
						{
							EnemySummoner enemySummoner = EnemySummoner.Get(this.CR._1, true);
							if (enemySummoner != null && enemySummoner != this && enemySummoner.CR != null)
							{
								enemySummoner.calcAboutEnemyCount(AIdList, false, false);
							}
						}
					}
				}
			}
			if (this.is_dangerous_ == 2)
			{
				this.is_dangerous_ = (flag ? 1 : 0);
			}
			this.CR.seek_set(cur_line + 1);
			return num2;
		}

		public bool getEnemyCountFromScript(CsvReader CR, out int _c, ref int addable, NightController NightCon)
		{
			string text = CR.getIndex(2);
			bool flag = false;
			if (TX.isStart(text, "!", 0))
			{
				text = TX.slice(text, 1);
				flag = true;
			}
			_c = X.IntR(this.CalcScript(text, "1", "ZLINE"));
			if (_c <= 0 && addable > 0 && !flag && (int)X.Mx((float)(NightCon.isNight() ? 1 : 0), NightCon.getDangerLevel() + 0.375f) + _c > 0)
			{
				addable--;
				_c = 1;
			}
			return flag;
		}

		private float getDangerLevel()
		{
			return (M2DBase.Instance as NelM2DBase).NightCon.getDangerLevel();
		}

		private float CalcScript(string t, string default_str = "0", string defaut_fn = "ZLINE")
		{
			EnemySummoner.FuncBase.Remake(TX.noe(t) ? default_str : t, defaut_fn, 5f);
			return EnemySummoner.FuncBase.Get(null, this.getDangerLevel());
		}

		private float Calc(CsvReaderA CR, int index, string default_str = "0", string defaut_fn = "ZLINE")
		{
			return this.CalcScript(CR.getIndex(index), default_str, defaut_fn);
		}

		private float CalcF(CsvReaderA CR, ref bool fix_flag, int index, string default_str = "0", string defaut_fn = "ZLINE")
		{
			string text = CR.getIndex(index);
			if (TX.isStart(text, "!", 0))
			{
				fix_flag = true;
				text = TX.slice(text, 1);
			}
			return this.CalcScript(text, default_str, defaut_fn);
		}

		public void recreateMBoxList(Designer BxDB, bool already_obtained)
		{
			BxDB.Clear();
			BxDB.init();
			BxDB.item_margin_x_px = 1f;
			BxDB.item_margin_y_px = 6f;
			float num = 24f;
			ReelManager.ItemReelDrop[] itemReelVector = this.getItemReelVector();
			if (itemReelVector != null)
			{
				MeshDrawer.FnGeneralDraw fnGeneralDraw = (MeshDrawer Md, float alpha) => this.fnDrawReel(BxDB, already_obtained, Md, alpha);
				Color32 color = (already_obtained ? C32.d2c(4288914339U) : MTRX.ColWhite);
				if (already_obtained)
				{
					BxDB.addP(new DsnDataP("", false)
					{
						size = 20f,
						TxCol = MTRX.ColWhite,
						alignx = ALIGN.CENTER,
						aligny = ALIGNY.MIDDLE,
						swidth = 30f,
						sheight = num,
						html = true,
						text = "<shape check tx_color/>"
					}, false);
				}
				int num2 = itemReelVector.Length;
				if (this.getOverdriveCapacity() != 0)
				{
					num2 += ((this.AReelSecretSrc != null) ? this.AReelSecretSrc.Length : 0);
				}
				for (int i = 0; i < num2; i++)
				{
					bool flag = false;
					ReelManager.ItemReelContainer itemReelContainer;
					if (i < itemReelVector.Length)
					{
						itemReelContainer = itemReelVector[i].IR;
					}
					else
					{
						itemReelContainer = this.AReelSecretSrc[i - itemReelVector.Length];
						flag = true;
					}
					FillImageBlock fillImageBlock = BxDB.addImg(new DsnDataImg
					{
						swidth = 30f,
						sheight = num,
						name = "mbox_" + i.ToString()
					});
					MeshDrawer meshDrawer = fillImageBlock.getMeshDrawer();
					MeshRenderer meshRenderer = fillImageBlock.getMeshRenderer();
					meshDrawer.chooseSubMesh(0, false, false);
					meshDrawer.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, fillImageBlock.stencil_ref), false);
					meshDrawer.chooseSubMesh(1, false, false);
					meshDrawer.setMaterial(MTRX.getMtr(BLEND.NORMAL, -1), false);
					meshDrawer.activation_key = i.ToString();
					if (meshRenderer != null)
					{
						meshDrawer.connectRendererToTriMulti(meshRenderer);
					}
					fillImageBlock.FnDraw = fnGeneralDraw;
					fillImageBlock.redraw_flag = true;
					BxDB.addP(new DsnDataP("", false)
					{
						size = 18f,
						TxCol = color,
						alignx = ALIGN.LEFT,
						aligny = ALIGNY.MIDDLE,
						sheight = num,
						text = (flag ? "???" : TX.Get("_ItemReel_name_" + itemReelContainer.key, "")) + ((i < num2 - 1) ? "／ " : "")
					}, false);
				}
				return;
			}
			BxDB.addP(new DsnDataP("", false)
			{
				size = 18f,
				TxCol = MTRX.ColWhite,
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.MIDDLE,
				sheight = num,
				text = TX.Get("Summoner_no_reward", "")
			}, false);
		}

		private bool fnDrawReel(Designer BxDB, bool already_obtained, MeshDrawer Md, float alpha)
		{
			if (BxDB == null)
			{
				return false;
			}
			alpha *= BxDB.animating_alpha;
			if (alpha <= 0f)
			{
				Md.clearSimple();
				return false;
			}
			int num = X.NmI(Md.activation_key, 0, false, false);
			ReelManager.ItemReelDrop[] itemReelVector = this.getItemReelVector();
			bool flag = false;
			ReelManager.ItemReelContainer itemReelContainer;
			if (num >= itemReelVector.Length)
			{
				itemReelContainer = this.AReelSecretSrc[num - itemReelVector.Length];
				alpha *= 0.5f * ((!already_obtained) ? (0.5f + 0.45f * X.COSIT(120f)) : 1f);
				flag = true;
			}
			else
			{
				itemReelContainer = itemReelVector[num].IR;
			}
			bool flag2 = BxDB.animating_alpha == 1f;
			Md.chooseSubMesh(0, false, false);
			itemReelContainer.drawSmallIcon(Md, 0f, 0f, alpha * (already_obtained ? 0.5f : 1f), 2f, false);
			Md.chooseSubMesh(1, false, false);
			if (!already_obtained)
			{
				if (!flag)
				{
					if (EnemySummoner.EfpKira == null)
					{
						EnemySummoner.EfpKira = new EfParticleOnce("mbox_unikira", EFCON_TYPE.UI);
					}
					EnemySummoner.EfpKira.index = X.GETRAN2(num, 4);
					EnemySummoner.EfpKira.drawTo(Md, 0f, 0f, 14f, (int)C32.c2d(C32.MulA(itemReelContainer.ColSet.top, 0.45f * alpha)), (float)IN.totalframe, EnemySummoner.EfpKira.delay * 14f);
				}
				flag2 = false;
			}
			Md.updateForMeshRenderer(false);
			return flag2;
		}

		public void activate(M2LpSummon _Lp, out bool bgm_replaced)
		{
			bgm_replaced = false;
			this.loadMaterial(_Lp.Mp, this.use_if);
			if (this.CR == null)
			{
				X.dl("CRが読み込まれていないためオープンされませんでした (maindataline=" + this.main_data_line.ToString(), null, false, false);
				return;
			}
			if (EnemySummoner.ActiveScript != null)
			{
				EnemySummoner.ActiveScript = EnemySummoner.ActiveScript.close(true, false);
			}
			if (EnemySummoner.LastSummoner != this)
			{
				EnemySummoner.LastSummoner = this;
				this.WRC_Air = null;
				this.AABccFootable = null;
			}
			COOK.setSF("_BATTLE_STARTED", COOK.getSF("_BATTLE_STARTED") + 1);
			UIPicture.Recheck(30);
			this.clear();
			EnemySummoner.enemy_index = 0;
			(_Lp.Mp.M2D as NelM2DBase).areaTitleHideProgress();
			this.Lp = _Lp;
			EnemySummoner.OFirstPos.Clear();
			this.AASummoned.Clear();
			EnemySummoner.max_enemy_appear_whole = 99;
			EnemySummoner.OMaxEnemyAppear = new BDic<string, EnemySummoner.EnemyAppearMax>();
			EnemySummoner.OFnCheckEnemy = new BDic<NelEnemy.FnCheckEnemy, int>();
			EnemySummoner.delay = 50f;
			EnemySummoner.ActiveScript = this;
			this.AOverdrived.Clear();
			this.prepareEnemyConent(ref bgm_replaced);
			this.callOpenSummonerToListener(EnemySummoner.isActiveBorder());
			this.check(0f);
		}

		public void callOpenSummonerToListener(bool is_active_border)
		{
			int count = EnemySummoner.ASummonerLitener.Count;
			for (int i = 0; i < count; i++)
			{
				EnemySummoner.ASummonerLitener[i].openSummoner(this, is_active_border);
			}
		}

		private void prepareEnemyConent(ref bool bgm_replaced)
		{
			if (this.CR == null)
			{
				return;
			}
			CsvReaderA csvReaderA = this.CR;
			NelM2DBase nelM2DBase = this.Lp.Mp.M2D as NelM2DBase;
			csvReaderA.seek_set(this.main_data_line);
			List<EnemySummoner.SmnEnemyKind> list = new List<EnemySummoner.SmnEnemyKind>(4);
			List<EnemySummoner.SmnPoint> list2 = new List<EnemySummoner.SmnPoint>();
			if (this.Asplitter_title == null)
			{
				this.Asplitter_title = new List<EnemySummoner.SplitterTerm>(1);
			}
			else
			{
				this.Asplitter_title.Clear();
			}
			this.Asplitter_title.Add(default(EnemySummoner.SplitterTerm));
			EnemySummoner.delay_one_second = (EnemySummoner.delay_one = -1f);
			EnemySummoner.weight_add = 0f;
			int num = 0;
			NightController nightCon = this.Lp.nM2D.NightCon;
			int num2;
			nelM2DBase.NightCon.SummonerInited(this, this.Lp.Mp, out num2);
			bool flag = false;
			float num3 = 0f;
			float num4 = 0f;
			int num5 = 1;
			int num6 = nightCon.summoner_enemy_count_addition(this);
			int num7 = 99;
			int num8 = nightCon.summoner_max_addition(this);
			EnemySummoner.max_enemy_appear_whole = X.Mx(2, 3 + num8);
			this.thunder_odable = this.thunder_odable_def + nightCon.summoner_max_thunder_odable_appear(this);
			this.Adecline_enemyid_buf = new List<string>(1);
			this.bgm_block_ovr = null;
			ReelManager.ItemReelDrop[] areel = this.AReel;
			ReelManager.ItemReelContainer[] areelSecretSrc = this.AReelSecretSrc;
			float num9 = this.replace_secret_to_lower;
			BDic<string, int[]> bdic = null;
			EnemySummoner.OMaxEnemyAppear.Clear();
			this.current_splitter_id = 0;
			this.fatal_key = null;
			this.bgm_replace = null;
			this.bgm_block_to = null;
			this.bgm_replace_when_close = true;
			csvReaderA.VarCon.removeTemp();
			csvReaderA.VarCon.define("_here", this.key, true);
			csvReaderA.VarCon.define("_map", this.Lp.Mp.key, true);
			bool flag2 = false;
			bool flag3 = false;
			while (csvReaderA.read())
			{
				bool flag4 = false;
				string cmd = csvReaderA.cmd;
				if (cmd != null)
				{
					uint num10 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
					if (num10 <= 1270314484U)
					{
						if (num10 <= 667981029U)
						{
							if (num10 <= 208699951U)
							{
								if (num10 != 113502404U)
								{
									if (num10 == 208699951U)
									{
										if (cmd == "%BGM_OVERRIDE_KEY")
										{
											this.bgm_block_ovr = csvReaderA._2;
										}
									}
								}
								else if (cmd == "%INITIAL_DELAY")
								{
									EnemySummoner.delay = this.Calc(csvReaderA, 1, "0", "ZLINE");
								}
							}
							else if (num10 != 430789690U)
							{
								if (num10 != 441099668U)
								{
									if (num10 == 667981029U)
									{
										if (cmd == "%BGM_REPLACE_WHEN_CLOSE")
										{
											this.bgm_replace_when_close = TX.eval(csvReaderA._1, "") != 0.0;
										}
									}
								}
								else if (cmd == "%DELAY_ONE")
								{
									EnemySummoner.delay_one = this.Calc(csvReaderA, 1, "0", "ZLINE");
									EnemySummoner.delay_one_second = EnemySummoner.delay_one * nightCon.summoner_delayonesecond_ratio(this);
								}
							}
							else if (cmd == "%MAX_CNT")
							{
								if (csvReaderA.clength == 3)
								{
									if (bdic == null)
									{
										bdic = new BDic<string, int[]>(1);
									}
									float num11 = this.CalcF(csvReaderA, ref flag4, 2, "99", "ZLINE");
									if (!flag4)
									{
										num11 += (float)nightCon.summoner_enemy_countmax_addition(this);
									}
									Dictionary<string, int[]> dictionary = bdic;
									string _ = csvReaderA._1;
									int[] array = new int[2];
									array[0] = X.IntR(num11);
									dictionary[_] = array;
								}
								else
								{
									float num12 = this.CalcF(csvReaderA, ref flag4, 1, "99", "ZLINE");
									if (!flag4)
									{
										num12 += (float)nightCon.summoner_enemy_countmax_addition(this);
									}
									num7 = X.IntR(num12);
								}
							}
						}
						else if (num10 <= 1015927979U)
						{
							if (num10 != 943963272U)
							{
								if (num10 == 1015927979U)
								{
									if (cmd == "%ADD_COUNT")
									{
										string text = csvReaderA.slice_join(1, " ", "");
										if (TX.isStart(text, "!", 0))
										{
											text = TX.slice(text, 1);
											num6 = 0;
										}
										num6 += (int)this.CalcScript(text, "0", "ZLINE");
									}
								}
							}
							else if (cmd == "%DELAY_FILLED")
							{
								EnemySummoner.delay_filled = this.Calc(csvReaderA, 1, "0", "ZLINE");
							}
						}
						else if (num10 != 1066748529U)
						{
							if (num10 != 1205246472U)
							{
								if (num10 == 1270314484U)
								{
									if (cmd == "%MAX_EN")
									{
										if (csvReaderA.clength == 3)
										{
											float num13 = this.CalcF(csvReaderA, ref flag4, 2, "99", "ZLINE");
											if (!flag4)
											{
												num13 = X.Mx(X.Mn(2f, num13), num13 + (float)num8);
											}
											EnemySummoner.OMaxEnemyAppear[csvReaderA._1] = new EnemySummoner.EnemyAppearMax(X.Mx(1, X.IntR(num13)));
										}
										else
										{
											float num14 = this.CalcF(csvReaderA, ref flag4, 1, "99", "ZLINE");
											if (!flag4)
											{
												num14 = X.Mx(1f, num14 + (float)num8);
											}
											EnemySummoner.max_enemy_appear_whole = X.Mx(2, X.IntR(num14));
										}
									}
								}
							}
							else if (cmd == "%PUPPETREVENGE")
							{
								if (this.Lp.is_sudden_puppetrevenge)
								{
									if (this.CR_puppetrevenge == null)
									{
										this.CR_puppetrevenge = new CsvReaderA(TX.getResource("Data/summon/" + this.puppetrevenge_replace + ".summon", ".csv", false), this.CR.VarCon);
									}
									this.fineReelData(csvReaderA._1, ref areel, ref areelSecretSrc, ref num9);
									csvReaderA = this.CR_puppetrevenge;
									csvReaderA.seek_set(0);
									flag2 = (flag3 = true);
								}
							}
						}
						else if (cmd == "%REPLACE_BGM")
						{
							if (csvReaderA.clength < 3 || TX.eval(csvReaderA.slice_join(2, " ", ""), "") != 0.0)
							{
								this.bgm_replace = csvReaderA._1;
							}
						}
					}
					else
					{
						if (num10 <= 2064310418U)
						{
							if (num10 <= 1717652112U)
							{
								if (num10 != 1684797004U)
								{
									if (num10 != 1717652112U)
									{
										continue;
									}
									if (!(cmd == "%BGM_BLOCK"))
									{
										continue;
									}
									this.bgm_block_to = csvReaderA._1;
									continue;
								}
								else
								{
									if (!(cmd == "%GOLEMTOY_DIVIDE"))
									{
										continue;
									}
									EnemySummoner.golemtoy_divide_count = (int)this.Calc(csvReaderA, 1, EnemySummoner.golemtoy_divide_count.ToString(), "ZLINE");
									continue;
								}
							}
							else if (num10 != 1728610943U)
							{
								if (num10 != 1843910277U)
								{
									if (num10 != 2064310418U)
									{
										continue;
									}
									if (!(cmd == "%EN_HR"))
									{
										continue;
									}
									num++;
									this.Asplitter_title.Add(new EnemySummoner.SplitterTerm(csvReaderA, (int)EnemySummoner.delay_one_second));
									continue;
								}
								else if (!(cmd == "%POS_LN"))
								{
									continue;
								}
							}
							else
							{
								if (!(cmd == "%EN_OD"))
								{
									continue;
								}
								goto IL_07F9;
							}
						}
						else if (num10 <= 2619452609U)
						{
							if (num10 != 2459156378U)
							{
								if (num10 != 2619452609U)
								{
									continue;
								}
								if (!(cmd == "%NEXT_SCRIPT"))
								{
									continue;
								}
								EnemySummoner.next_script_key = csvReaderA._1;
								continue;
							}
							else
							{
								if (!(cmd == "%FATAL"))
								{
									continue;
								}
								if (csvReaderA.clength < 3 || TX.eval(csvReaderA.slice_join(2, " ", ""), "") != 0.0)
								{
									this.fatal_key = csvReaderA._1;
									X.dl("フェイタルキー " + this.fatal_key, null, false, false);
									continue;
								}
								continue;
							}
						}
						else if (num10 != 3719324707U)
						{
							if (num10 != 4074695482U)
							{
								if (num10 != 4090079685U)
								{
									continue;
								}
								if (!(cmd == "%DELAY_ONE_FIRST"))
								{
									continue;
								}
								EnemySummoner.delay_one = this.Calc(csvReaderA, 1, "0", "ZLINE");
								continue;
							}
							else if (!(cmd == "%POS"))
							{
								continue;
							}
						}
						else
						{
							if (!(cmd == "%EN"))
							{
								continue;
							}
							goto IL_07F9;
						}
						M2LabelPoint m2LabelPoint = this.Lp;
						int num15 = 0;
						if (csvReaderA.cmd == "%POS_LN")
						{
							m2LabelPoint = this.Mp.getPoint(csvReaderA._1, false) ?? m2LabelPoint;
							num15 = 1;
						}
						EnemySummoner.SmnPoint smnPoint = new EnemySummoner.SmnPoint(m2LabelPoint, csvReaderA.Nm(1 + num15, 0f), csvReaderA.Nm(2 + num15, 0f), this.Calc(csvReaderA, 3 + num15, "1", "ZLINE"), EnemySummoner.XORSP(), csvReaderA.slice(4 + num15, -1000));
						list2.Add(smnPoint);
						continue;
						IL_07F9:
						if (csvReaderA.IntE(5, 1) != 0)
						{
							ENEMYID enemyid;
							if (!FEnum<ENEMYID>.TryParse(csvReaderA._1U, out enemyid, true))
							{
								X.de("不明なID: " + csvReaderA._1, null);
							}
							else
							{
								int num16;
								flag4 = this.getEnemyCountFromScript(csvReaderA, out num16, ref num6, nelM2DBase.NightCon);
								if (num16 > 0)
								{
									EnemySummoner.SmnEnemyKind smnEnemyKind = new EnemySummoner.SmnEnemyKind(csvReaderA._1U, num16, num, this.Calc(csvReaderA, 3, "0.5", "ZLINE"), this.Calc(csvReaderA, 4, "0", "ZLINE"), csvReaderA._5, this.Calc(csvReaderA, 6, "1", "ZLINE"), this.Calc(csvReaderA, 7, "1", "ZLINE"));
									list.Add(smnEnemyKind);
									if (flag4)
									{
										smnEnemyKind.count_add_weight = 0f;
									}
									else
									{
										num5 = X.Mn(num5, num16);
									}
									if (csvReaderA.cmd == "%EN_OD")
									{
										smnEnemyKind.pre_overdrive = true;
									}
									else if (smnEnemyKind.EnemyDesc.overdriveable)
									{
										flag = true;
									}
									num4 += smnEnemyKind.mp_add_weight;
								}
							}
						}
					}
				}
			}
			if (list2.Count == 0)
			{
				list2.Add(new EnemySummoner.SmnPoint(this.Lp, 0f, 0f, 1f, EnemySummoner.XORSP(), null));
			}
			List<EnemySummoner.SmnEnemyKind> list3 = new List<EnemySummoner.SmnEnemyKind>(list);
			int num17 = 0;
			list.Clear();
			List<int[]> list4 = new List<int[]>(2);
			for (int i = list3.Count - 1; i >= 0; i--)
			{
				EnemySummoner.SmnEnemyKind smnEnemyKind2 = list3[i];
				list4.Clear();
				int num18 = 999;
				if (bdic != null)
				{
					foreach (KeyValuePair<string, int[]> keyValuePair in bdic)
					{
						if (smnEnemyKind2.isSame(keyValuePair.Key))
						{
							list4.Add(keyValuePair.Value);
							num18 = X.Mn(keyValuePair.Value[0] - keyValuePair.Value[1], num18);
						}
					}
				}
				int num19 = X.Mn(num18, smnEnemyKind2.def_count);
				if (num18 <= smnEnemyKind2.def_count)
				{
					smnEnemyKind2.count_add_weight = 0f;
				}
				for (int j = list4.Count - 1; j >= 0; j--)
				{
					list4[j][1] += num19;
				}
				if (!smnEnemyKind2.count_fix)
				{
					num3 += smnEnemyKind2.count_add_weight;
				}
				if (smnEnemyKind2.pre_overdrive)
				{
					num17 += num19;
				}
				while (--num19 >= 0)
				{
					list.Add(new EnemySummoner.SmnEnemyKind(smnEnemyKind2));
				}
			}
			if (num3 > 0f && list3.Count > 0)
			{
				EnemySummoner.shuffle<EnemySummoner.SmnEnemyKind>(list3, -1);
				int num20 = 0;
				while (num6 > 0 && num3 > 0f && ++num20 < 300)
				{
					EnemySummoner.SmnEnemyKind smnEnemyKind3 = null;
					if (num5 <= 0)
					{
						int num21 = 1;
						for (int k = list3.Count - 1; k >= 0; k--)
						{
							EnemySummoner.SmnEnemyKind smnEnemyKind4 = list3[k];
							if (!smnEnemyKind4.count_fix)
							{
								num21 = X.Mn(num21, smnEnemyKind4.def_count);
								if (smnEnemyKind4.def_count == num5)
								{
									smnEnemyKind3 = smnEnemyKind4;
									smnEnemyKind3.def_count = 1;
									break;
								}
							}
						}
						num5 = num21;
					}
					if (smnEnemyKind3 == null && num5 > 0)
					{
						float num22 = EnemySummoner.XORSP() * num3;
						for (int k = list3.Count - 1; k >= 0; k--)
						{
							EnemySummoner.SmnEnemyKind smnEnemyKind5 = list3[k];
							if (smnEnemyKind5.count_add_weight > 0f)
							{
								num22 -= smnEnemyKind5.count_add_weight;
								if (num22 <= 0.0002f)
								{
									smnEnemyKind3 = smnEnemyKind5;
									break;
								}
							}
						}
					}
					if (smnEnemyKind3 != null)
					{
						num6--;
						EnemySummoner.SmnEnemyKind smnEnemyKind6 = new EnemySummoner.SmnEnemyKind(smnEnemyKind3);
						smnEnemyKind6.pre_overdrive = false;
						list.Add(smnEnemyKind6);
						if (bdic != null)
						{
							bool flag5 = false;
							foreach (KeyValuePair<string, int[]> keyValuePair2 in bdic)
							{
								if (smnEnemyKind6.isSame(keyValuePair2.Key))
								{
									int[] value = keyValuePair2.Value;
									int num23 = 1;
									int num24 = value[num23] + 1;
									value[num23] = num24;
									if (num24 >= keyValuePair2.Value[0])
									{
										flag5 = true;
									}
								}
							}
							if (flag5)
							{
								num3 -= smnEnemyKind3.count_add_weight;
								smnEnemyKind3.count_add_weight = 0f;
							}
						}
					}
				}
			}
			list3.Clear();
			if (flag && num2 > 0)
			{
				int count = list.Count;
				int[] array2 = X.makeCountUpArray(count, 0, 1);
				EnemySummoner.shuffle<int>(array2, count);
				for (int l = 0; l < count; l++)
				{
					int num25 = array2[l];
					EnemySummoner.SmnEnemyKind smnEnemyKind7 = list[num25];
					EnemyData.EnemyDescryption enemyDesc = smnEnemyKind7.EnemyDesc;
					if (!smnEnemyKind7.pre_overdrive && !smnEnemyKind7.thunder_overdrive && enemyDesc.overdriveable)
					{
						smnEnemyKind7.thunder_overdrive = true;
						list3.Add(smnEnemyKind7);
						num17++;
						if (--num2 <= 0)
						{
							break;
						}
					}
				}
			}
			if (list.Count > num7)
			{
				for (int m = list3.Count - 1; m >= 0; m--)
				{
					list.Remove(list3[m]);
				}
				EnemySummoner.shuffle<EnemySummoner.SmnEnemyKind>(list, -1);
				list.RemoveRange(num7 + list3.Count, list.Count - (num7 + list3.Count));
				for (int n = list3.Count - 1; n >= 0; n--)
				{
					list.Add(list3[n]);
				}
			}
			if (num4 > 0f)
			{
				float num26 = nightCon.summoner_mp_addition(this);
				list3 = new List<EnemySummoner.SmnEnemyKind>(list);
				int num27 = list3.Count;
				int num28 = 0;
				int num29 = X.IntC((float)(nightCon.getDangerMeterVal(false) / 16));
				num4 = 0f;
				for (int num30 = list.Count - 1; num30 >= 0; num30--)
				{
					num4 += list[num30].mp_add_weight;
				}
				while (num26 > 0f && num27 > 0 && num4 > 0f && ++num28 < 300)
				{
					float num31 = EnemySummoner.XORSP() * num4;
					int num30 = 0;
					while (num30 < num27)
					{
						EnemySummoner.SmnEnemyKind smnEnemyKind8 = list3[num30];
						num31 -= smnEnemyKind8.mp_add_weight;
						if (num31 <= 0.0002f)
						{
							float num32 = X.Mx(0f, 1f - smnEnemyKind8.mp_ratio_min);
							if (num32 > 0f)
							{
								float num33 = X.Mn(X.Mn(X.Mx(0.125f, num32 * EnemySummoner.NIXP(0.5f, 1f)), num32), num26);
								smnEnemyKind8.mp_ratio_min += num33;
								smnEnemyKind8.mp_ratio_max += num33;
								num26 -= num33;
								EnemySummoner.SmnEnemyKind smnEnemyKind9 = smnEnemyKind8;
								smnEnemyKind9.mp_added += 1;
							}
							else
							{
								smnEnemyKind8.mp_added = (byte)num29;
							}
							if ((int)smnEnemyKind8.mp_added >= num29)
							{
								list3.RemoveAt(num30);
								num27--;
								num4 -= smnEnemyKind8.mp_add_weight;
								break;
							}
							break;
						}
						else
						{
							num30++;
						}
					}
				}
			}
			this.AReelAfterWholeDefeat = null;
			if (areel != null && (!nightCon.alreadyCleardInThisSession(this.Lp) || flag3))
			{
				int num34 = list.Count;
				List<int> list5 = new List<int>(X.makeCountUpArray(num34, 0, 1));
				if (num17 > 0)
				{
					int num35 = (this.Lp.is_sudden_puppetrevenge ? 3 : nightCon.summoner_drop_od_enemy_box_max(this));
					if (num35 > 0)
					{
						float num36 = nightCon.summoner_drop_od_enemy_box_ratio(this);
						for (int num37 = num34 - 1; num37 >= 0; num37--)
						{
							EnemySummoner.SmnEnemyKind smnEnemyKind10 = list[num37];
							if ((smnEnemyKind10.pre_overdrive || smnEnemyKind10.thunder_overdrive) && EnemySummoner.XORSP() < num36)
							{
								num34--;
								ReelManager.ItemReelContainer itemReelContainer = areelSecretSrc[EnemySummoner.xors(areelSecretSrc.Length)];
								itemReelContainer.GReelItem.touchObtainCount();
								if (EnemySummoner.XORSP() < num9)
								{
									itemReelContainer = ReelManager.ReplaceToLowerGrade(itemReelContainer);
								}
								smnEnemyKind10.DropReel = new ReelManager.ItemReelDrop(itemReelContainer, -1);
								list5.RemoveAt(num37);
								if (--num35 <= 0)
								{
									break;
								}
							}
						}
					}
				}
				EnemySummoner.shuffle<int>(list5, num34);
				ReelManager.ItemReelDrop[] array3 = areel;
				for (int num38 = areel.Length - 1; num38 >= 0; num38--)
				{
					ReelManager.ItemReelDrop itemReelDrop = areel[num38];
					if (itemReelDrop != null)
					{
						itemReelDrop.IR.GReelItem.touchObtainCount();
					}
				}
				int num39 = 0;
				if (!flag2)
				{
					if (this.Asplitter_title.Count > 1)
					{
						for (int num40 = num34 - 1; num40 >= 0; num40--)
						{
							if (!this.is_follower(list[list5[num40]], null))
							{
								num39++;
							}
							else
							{
								num34--;
								list5.RemoveAt(num40);
							}
						}
					}
					else
					{
						num39 = num34;
					}
				}
				if (num39 < areel.Length)
				{
					this.AReelAfterWholeDefeat = new List<ReelManager.ItemReelDrop>(areel);
					EnemySummoner.shuffle<ReelManager.ItemReelDrop>(this.AReelAfterWholeDefeat, -1);
					array3 = this.AReelAfterWholeDefeat.GetRange(0, num39).ToArray();
					this.AReelAfterWholeDefeat = this.AReelAfterWholeDefeat.GetRange(num39, this.AReelAfterWholeDefeat.Count - num39);
				}
				num39 = X.Mn(num39, array3.Length);
				for (int num41 = 0; num41 < num39; num41++)
				{
					list[list5[num41]].DropReel = array3[num41];
				}
			}
			EnemySummoner.ASmnPos = list2.ToArray();
			EnemySummoner.AKindRest = list;
			EnemySummoner.enemy_rest = (EnemySummoner.summoned_count = 0);
			EnemySummoner.ran_session = EnemySummoner.xors(16777215);
			EnemySummoner.battleable_enemy_ = 1;
			int num42 = 0;
			EnemySummoner.enemy_respawn_timescale_when_exists = 1f;
			if (this.Asplitter_title.Count > 1)
			{
				for (int num43 = EnemySummoner.AKindRest.Count - 1; num43 >= 0; num43--)
				{
					if (!this.is_follower(EnemySummoner.AKindRest[num43], null))
					{
						EnemySummoner.enemy_rest++;
					}
					else
					{
						num42++;
					}
				}
			}
			else
			{
				EnemySummoner.enemy_rest = EnemySummoner.AKindRest.Count;
			}
			if (this.bgm_replace != null)
			{
				BGM.load(this.bgm_replace, null, false);
				BGM.fadeout(0f, 120f, false);
				bgm_replaced = true;
			}
			X.dl("enemy total:" + EnemySummoner.enemy_rest.ToString() + ((num42 > 0) ? (" (follower: " + num42.ToString() + ")") : ""), null, false, false);
			EnemySummoner.shuffle<EnemySummoner.SmnEnemyKind>(EnemySummoner.AKindRest, -1);
			if (num >= 1)
			{
				EnemySummoner.AKindRest.Sort(new Comparison<EnemySummoner.SmnEnemyKind>(this.fnSortKind));
			}
			if (EnemySummoner.delay_one < 0f)
			{
				EnemySummoner.delay_one = 0f;
			}
			if (EnemySummoner.delay_filled < 0f)
			{
				EnemySummoner.delay_filled = 90f;
			}
			if (EnemySummoner.delay_one_second < 0f)
			{
				EnemySummoner.delay_one_second = EnemySummoner.delay_one;
			}
			if (this.Lp.type_event_enemy)
			{
				EnemySummoner.delay = 0f;
			}
			for (int num44 = EnemySummoner.AKindRest.Count - 1; num44 >= 0; num44--)
			{
				EnemyData.SummonerOpened(EnemySummoner.AKindRest[num44].enemyid);
			}
		}

		public bool is_follower(NelEnemy En, string check_prefix = null)
		{
			EnemySummoner.EnemySummonedInfo summonedInfo = this.getSummonedInfo(En);
			return summonedInfo != null && this.is_follower(summonedInfo.K, check_prefix);
		}

		public bool is_follower(EnemySummoner.SmnEnemyKind K, string check_prefix = null)
		{
			if (K.splitter_id >= 0)
			{
				EnemySummoner.SplitterTerm splitterTerm = this.Asplitter_title[X.Mn(this.Asplitter_title.Count - 1, K.splitter_id)];
				if (TX.valid(splitterTerm.title) && TX.isStart(splitterTerm.title, "_FOLLOW_", 0))
				{
					return check_prefix == null || TX.isStart(splitterTerm.title, check_prefix, 0);
				}
			}
			return false;
		}

		private int fnSortKind(EnemySummoner.SmnEnemyKind Ka, EnemySummoner.SmnEnemyKind Kb)
		{
			if (Ka.splitter_id == Kb.splitter_id)
			{
				return 0;
			}
			if (Ka.splitter_id >= Kb.splitter_id)
			{
				return -1;
			}
			return 1;
		}

		public bool check(float fcnt = 0f)
		{
			if (this.Mp.M2D.isMaterialLoading(false) || !PxlsLoader.isLoadCompletedAll())
			{
				return true;
			}
			if (this.AASummoned.Count >= this.max_enemy_appear_current_total)
			{
				return true;
			}
			if (EnemySummoner.delay > 0f)
			{
				EnemySummoner.delay = X.Mx((this.count_battleable_enemy <= 0) ? (X.Mn(EnemySummoner.delay, 150f) - fcnt) : (EnemySummoner.delay - EnemySummoner.enemy_respawn_timescale_when_exists * fcnt), 0f);
			}
			else if (EnemySummoner.AKindRest.Count > 0)
			{
				for (int i = EnemySummoner.AKindRest.Count - 1; i >= 0; i--)
				{
					EnemySummoner.SmnEnemyKind smnEnemyKind = EnemySummoner.AKindRest[i];
					if (smnEnemyKind.splitter_id > this.current_splitter_id)
					{
						EnemySummoner.SplitterTerm splitterTerm = this.Asplitter_title[smnEnemyKind.splitter_id];
						bool flag = splitterTerm.can_go_through(smnEnemyKind.splitter_id, this);
						this.Asplitter_title[smnEnemyKind.splitter_id] = splitterTerm;
						if (!flag)
						{
							EnemySummoner.delay = 5f;
							EnemySummoner.delay_one = EnemySummoner.delay_one_second;
							break;
						}
						this.current_splitter_id = smnEnemyKind.splitter_id;
						if (splitterTerm.delay > 0)
						{
							EnemySummoner.delay = (float)splitterTerm.delay;
							break;
						}
					}
					if (this.Adecline_enemyid_buf.IndexOf(smnEnemyKind.check_str) < 0)
					{
						int num = EnemySummoner.max_enemy_appear_whole;
						int num2 = 0;
						if (smnEnemyKind.thunder_overdrive)
						{
							int num3 = 0;
							int num4 = X.Mx(1, this.thunder_odable);
							foreach (KeyValuePair<NelEnemy, EnemySummoner.EnemySummonedInfo> keyValuePair in EnemySummoner.OFirstPos)
							{
								if (keyValuePair.Value.K.thunder_overdrive)
								{
									num3++;
								}
							}
							if (num4 <= num3)
							{
								this.Adecline_enemyid_buf.Add(smnEnemyKind.check_str);
								goto IL_033A;
							}
						}
						if (num > num2)
						{
							foreach (KeyValuePair<string, EnemySummoner.EnemyAppearMax> keyValuePair2 in EnemySummoner.OMaxEnemyAppear)
							{
								if (smnEnemyKind.isSame(keyValuePair2.Key))
								{
									num = X.Mn(num, keyValuePair2.Value.max);
									num2 = X.Mx(num2, keyValuePair2.Value.appeared);
								}
							}
						}
						if (num <= num2)
						{
							this.Adecline_enemyid_buf.Add(smnEnemyKind.check_str);
						}
						else if (!(this.summonEnemy(smnEnemyKind, null, false) == null))
						{
							if (!this.is_follower(smnEnemyKind, null))
							{
								EnemySummoner.enemy_rest--;
							}
							EnemySummoner.AKindRest.RemoveAt(i);
							if (this.AASummoned.Count >= this.max_enemy_appear_current_total)
							{
								EnemySummoner.delay_one = EnemySummoner.delay_one_second;
								EnemySummoner.delay = X.Mx(1f, EnemySummoner.delay_filled);
							}
							else
							{
								EnemySummoner.delay = X.Mx(1f, EnemySummoner.delay_one);
							}
							if (this.bgm_replace == null || !(this.bgm_replace != ""))
							{
								break;
							}
							BGM.replace(30f, 0f, !this.bgm_replace_when_close, false);
							this.bgm_replace = "";
							if (TX.valid(this.bgm_block_ovr))
							{
								BGM.setOverrideKey(this.bgm_block_ovr);
							}
							if (TX.valid(this.bgm_block_to))
							{
								BGM.GotoBlock(this.bgm_block_to, true);
								break;
							}
							break;
						}
					}
					IL_033A:;
				}
				if (EnemySummoner.delay <= 0f)
				{
					if (this.enemy_count == 0f)
					{
						EnemySummoner.enemy_rest = 0;
					}
					else
					{
						EnemySummoner.delay_one = EnemySummoner.delay_one_second;
						EnemySummoner.delay = 60f;
					}
				}
			}
			if (EnemySummoner.enemy_rest <= 0 && this.enemy_count <= 0f)
			{
				if (EnemySummoner.next_script_key != null)
				{
					if (EnemySummoner.delay != 0f)
					{
						return true;
					}
					EnemySummoner enemySummoner = EnemySummoner.Get(EnemySummoner.next_script_key, true);
					EnemySummoner.next_script_key = null;
					if (enemySummoner != null)
					{
						bool flag2 = false;
						enemySummoner.prepareEnemyConent(ref flag2);
						return true;
					}
				}
				this.close(false, true);
				return false;
			}
			return true;
		}

		public NelEnemy summonEnemyFromOther(EnemySummoner.SmnEnemyKind K, EnemySummoner.SmnPoint Target = null)
		{
			return this.summonEnemy(K, Target, true);
		}

		private NelEnemy summonEnemy(EnemySummoner.SmnEnemyKind K, EnemySummoner.SmnPoint Target, bool add_rest_count = false)
		{
			bool flag = false;
			int num = EnemySummoner.ASmnPos.Length;
			Vector2 vector = Vector2.zero;
			int num2 = 0;
			NelEnemy nelEnemy = null;
			List<NelEnemy> list = new List<NelEnemy>(4);
			for (;;)
			{
				nelEnemy = EnemyData.createByKey(this.Lp.Mp, K.enemyid, "-Summonned-" + this.Lp.key + "-" + EnemySummoner.enemy_index.ToString());
				if (nelEnemy == null)
				{
					break;
				}
				float num3 = EnemySummoner.weight_add;
				if (Target == null)
				{
					List<EnemySummoner.SmnPoint> list2 = new List<EnemySummoner.SmnPoint>(2);
					List<EnemySummoner.SmnPoint> list3 = new List<EnemySummoner.SmnPoint>(2);
					if (!flag)
					{
						for (int i = 0; i < 2; i++)
						{
							float num4 = EnemySummoner.weight_add + (float)i;
							list2.Clear();
							for (int j = 0; j < num; j++)
							{
								EnemySummoner.SmnPoint smnPoint = EnemySummoner.ASmnPos[j];
								if (smnPoint.available(num4))
								{
									if (smnPoint.idMatch(K.enemyid))
									{
										smnPoint.AddTo(list2, num4);
									}
									else if (smnPoint.Aalloc_id == null)
									{
										smnPoint.AddTo(list3, num4);
									}
								}
							}
							if (list2.Count > 0)
							{
								Target = list2[(int)(K.NCXORSP(141) * (float)list2.Count)];
								num3 = num4;
								break;
							}
							if (list3.Count > 0)
							{
								Target = list3[(int)(K.NCXORSP(67) * (float)list3.Count)];
								num3 = num4;
								break;
							}
						}
					}
					if (Target == null)
					{
						int i = 0;
						while (Target == null)
						{
							float num5 = EnemySummoner.weight_add + (float)i;
							list2.Clear();
							for (int k = 0; k < num; k++)
							{
								EnemySummoner.SmnPoint smnPoint2 = EnemySummoner.ASmnPos[k];
								if (smnPoint2.available(num5))
								{
									smnPoint2.AddTo(list2, num5);
								}
							}
							if (list2.Count > 0)
							{
								Target = list2[(int)(K.NCXORSP(93) * (float)list2.Count)];
								num3 = num5;
								break;
							}
						}
					}
				}
				nelEnemy.Summoner = this;
				nelEnemy.first_mp_ratio = X.ZLINE(X.NI(K.mp_ratio_min, K.mp_ratio_max, K.NCXORSP(323)));
				if (K.mp_ratio_min < 1f)
				{
					nelEnemy.first_mp_ratio = this.Lp.nM2D.NightCon.fixEnemyFirstMpRatio(nelEnemy.first_mp_ratio);
				}
				float num6 = 0f;
				if (Target.pos_cnt < 0)
				{
					if (Target.pos_cnt == -1)
					{
						vector = Vector2.zero;
						Target.pos_cnt--;
					}
					else
					{
						vector.Set(1.4f * X.Cos(Target.pos_agR), 1.2f * X.Sin(Target.pos_agR));
						Target.pos_agR += 2.2242477f;
						EnemySummoner.SmnPoint smnPoint3 = Target;
						int num7 = smnPoint3.pos_cnt - 1;
						smnPoint3.pos_cnt = num7;
						if (num7 < -6)
						{
							Target.pos_cnt = -1;
						}
					}
					vector.x += Target.x;
					vector.y += Target.y;
				}
				else
				{
					if (Target.pos_cnt == 0)
					{
						vector = Vector2.zero;
						Target.pos_cnt++;
					}
					else
					{
						vector.Set(1.4f * X.Cos(Target.pos_agR), 1.2f * X.Sin(Target.pos_agR));
						num6 = 0.7f;
						Target.pos_agR += 2.5132742f;
						EnemySummoner.SmnPoint smnPoint4 = Target;
						int num7 = smnPoint4.pos_cnt + 1;
						smnPoint4.pos_cnt = num7;
						if (num7 > 5)
						{
							Target.pos_cnt = 0;
						}
					}
					float num8 = Target.Lp.mapfocx;
					float num9 = Target.Lp.mapfocy;
					if (Target.Lp is M2LpPuzzCarrier)
					{
						Vector2 moverTranslatedMp = (Target.Lp as M2LpPuzzCarrier).getMoverTranslatedMp();
						num8 += moverTranslatedMp.x;
						num9 += moverTranslatedMp.y;
						vector.x += Target.x + num8;
						vector.y += Target.y + num9;
					}
					else
					{
						vector.x += Target.x + num8;
						vector.y += Target.y + num9;
						vector = Target.Lp.getWalkable(this.Mp, X.MMX((float)Target.Lp.mapx, vector.x, (float)(Target.Lp.mapx + Target.Lp.mapw)), X.MMX((float)Target.Lp.mapy, vector.y, (float)(Target.Lp.mapy + Target.Lp.maph)));
					}
				}
				Target.use_weight += 1f;
				EnemySummoner.weight_add = num3;
				EnemySummoner.OFirstPos[nelEnemy] = new EnemySummoner.EnemySummonedInfo(K, vector, Target);
				if (num6 != 0f)
				{
					vector.x += X.NI(-num6, num6, K.NCXORSP(47));
					vector.y += X.NI(-num6, num6, K.NCXORSP(39)) * 0.5f;
				}
				Vector3 localPosition = nelEnemy.transform.localPosition;
				localPosition.x = this.Mp.map2ux(localPosition.x);
				localPosition.y = this.Mp.map2uy(localPosition.y);
				nelEnemy.transform.localPosition = localPosition;
				FEnum<ENEMYID>.TryParse(K.enemyid, out nelEnemy.id, true);
				nelEnemy.key = K.enemyid + "_" + EnemySummoner.enemy_index++.ToString();
				nelEnemy.smn_xorsp = K.smn_xorsp;
				this.Mp.assignMover(nelEnemy, false);
				nelEnemy.setTo(vector.x, vector.y + 0.5f - nelEnemy.sizey);
				bool flag2 = Target.sudden_appear || this.Lp.type_event_enemy;
				nelEnemy.initSummoned(K, flag2, num2);
				EnemySummoner.battleable_enemy_ = -1;
				EnemySummoner.enemy_respawn_timescale_when_exists = DIFF.enemy_respawn_timescale_when_exists;
				if (flag2)
				{
					this.fineEventEnemyFlag(nelEnemy);
					nelEnemy.quitSummonAndAppear(false);
				}
				list.Add(nelEnemy);
				EnemySummoner.summoned_count++;
				if (nelEnemy is EnemySummoner.IOtherKillListener)
				{
					if (this.AOtherLsn == null)
					{
						this.AOtherLsn = new List<EnemySummoner.IOtherKillListener>(1);
					}
					this.AOtherLsn.Add(nelEnemy as EnemySummoner.IOtherKillListener);
				}
				foreach (KeyValuePair<string, EnemySummoner.EnemyAppearMax> keyValuePair in EnemySummoner.OMaxEnemyAppear)
				{
					if (K.isSame(keyValuePair.Key))
					{
						keyValuePair.Value.appeared++;
					}
				}
				this.current_splitter_id = X.Mx(K.splitter_id, this.current_splitter_id);
				if (K.temporary_adding_count)
				{
					EnemySummoner.max_enemy_appear_whole_temp++;
				}
				OverDriveManager odManager = nelEnemy.getOdManager();
				if (odManager != null)
				{
					if (K.pre_overdrive)
					{
						odManager.pre_overdrive = true;
					}
					else if (K.thunder_overdrive)
					{
						odManager.thunder_overdrive = true;
					}
				}
				if (K.DupeConnect == null)
				{
					goto IL_074C;
				}
				num2++;
				K = K.DupeConnect;
				if (num2 >= 1 && K.NCXORSP(78) < 0.4f)
				{
					Target = null;
				}
			}
			X.de("エネミー召喚に失敗(" + this.key + ")", null);
			return null;
			IL_074C:
			this.AASummoned.Add(list);
			EnemySummoner.OFnCheckEnemy.Clear();
			return nelEnemy;
		}

		public int callFollowerEnemy(ref int splitter_id, int set_delay = 0, int summonable = -1)
		{
			splitter_id = X.Mx(this.current_splitter_id, splitter_id);
			int count = EnemySummoner.AKindRest.Count;
			if (count == 0)
			{
				return 0;
			}
			bool flag = false;
			int num = 1;
			for (int i = count - 1; i >= 0; i--)
			{
				EnemySummoner.SmnEnemyKind smnEnemyKind = EnemySummoner.AKindRest[i];
				if (smnEnemyKind.splitter_id >= splitter_id)
				{
					if (!flag)
					{
						flag = true;
						this.current_splitter_id = (splitter_id = smnEnemyKind.splitter_id);
						EnemySummoner.delay = X.Mx(EnemySummoner.delay, (float)set_delay);
						this.Asplitter_title.Insert(splitter_id, this.Asplitter_title[X.MMX(0, splitter_id, this.Asplitter_title.Count)]);
					}
					if (summonable == 0 || smnEnemyKind.splitter_id > splitter_id)
					{
						smnEnemyKind.splitter_id++;
					}
					else
					{
						summonable--;
						num = 2;
					}
				}
			}
			return num;
		}

		public int countSplitterEnemy(int splitter_id, bool count_active = true, bool count_rest = true, bool count_lesser_id = false)
		{
			int num = 0;
			if (count_active)
			{
				for (int i = this.AASummoned.Count - 1; i >= 0; i--)
				{
					List<NelEnemy> list = this.AASummoned[i];
					for (int j = list.Count - 1; j >= 0; j--)
					{
						NelEnemy nelEnemy = list[j];
						if (nelEnemy.battleable_enemy)
						{
							EnemySummoner.EnemySummonedInfo summonedInfo = this.getSummonedInfo(nelEnemy);
							if (summonedInfo != null && (count_lesser_id ? (summonedInfo.K.splitter_id <= splitter_id) : (summonedInfo.K.splitter_id == splitter_id)))
							{
								num++;
							}
						}
					}
				}
			}
			if (count_rest)
			{
				for (int k = EnemySummoner.AKindRest.Count - 1; k >= 0; k--)
				{
					if (EnemySummoner.AKindRest[k].splitter_id == splitter_id)
					{
						num++;
					}
				}
			}
			return num;
		}

		public int countActiveEnemy(NelEnemy.FnCheckEnemy Fn, bool memory = true)
		{
			int num;
			if (memory && EnemySummoner.OFnCheckEnemy.TryGetValue(Fn, out num))
			{
				return num;
			}
			int num2 = 0;
			for (int i = this.AASummoned.Count - 1; i >= 0; i--)
			{
				List<NelEnemy> list = this.AASummoned[i];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					if (Fn(list[j]))
					{
						num2++;
					}
				}
			}
			if (memory)
			{
				EnemySummoner.OFnCheckEnemy[Fn] = num2;
			}
			return num2;
		}

		public int countActiveEnemy(string enemy_id, bool count_active = true, bool count_rest = true, bool count_rest_only_current_splitter = true)
		{
			int num = 0;
			if (count_active)
			{
				for (int i = this.AASummoned.Count - 1; i >= 0; i--)
				{
					List<NelEnemy> list = this.AASummoned[i];
					for (int j = list.Count - 1; j >= 0; j--)
					{
						NelEnemy nelEnemy = list[j];
						if (nelEnemy.battleable_enemy)
						{
							EnemySummoner.EnemySummonedInfo summonedInfo = this.getSummonedInfo(nelEnemy);
							if (summonedInfo != null && summonedInfo.K.isSame(enemy_id))
							{
								num++;
							}
						}
					}
				}
			}
			if (count_rest)
			{
				for (int k = EnemySummoner.AKindRest.Count - 1; k >= 0; k--)
				{
					EnemySummoner.SmnEnemyKind smnEnemyKind = EnemySummoner.AKindRest[k];
					if (!count_rest_only_current_splitter && smnEnemyKind.splitter_id <= this.current_splitter_id && smnEnemyKind.isSame(enemy_id))
					{
						num++;
					}
				}
			}
			return num;
		}

		public int count_battleable_enemy
		{
			get
			{
				if (EnemySummoner.battleable_enemy_ < 0)
				{
					EnemySummoner.battleable_enemy_ = 0;
					for (int i = this.AASummoned.Count - 1; i >= 0; i--)
					{
						List<NelEnemy> list = this.AASummoned[i];
						for (int j = list.Count - 1; j >= 0; j--)
						{
							if (list[j].battleable_enemy)
							{
								EnemySummoner.battleable_enemy_++;
							}
						}
					}
				}
				return EnemySummoner.battleable_enemy_;
			}
			set
			{
				if (value < 0)
				{
					EnemySummoner.battleable_enemy_ = -1;
				}
			}
		}

		public NelEnemy searchActiveEnemy(NelEnemy.FnCheckEnemy Fn)
		{
			int count = this.AASummoned.Count;
			for (int i = 0; i < count; i++)
			{
				List<NelEnemy> list = this.AASummoned[i];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					NelEnemy nelEnemy = list[j];
					if (Fn(nelEnemy))
					{
						return nelEnemy;
					}
				}
			}
			return null;
		}

		public void fineEventEnemyFlag(NelEnemy En)
		{
			En.event_enemy_flag = this.Lp.type_event_enemy;
		}

		public void fineEventEnemyFlag()
		{
			for (int i = this.AASummoned.Count - 1; i >= 0; i--)
			{
				List<NelEnemy> list = this.AASummoned[i];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					this.fineEventEnemyFlag(list[j]);
				}
			}
		}

		public void killedEnemy(NelEnemy En)
		{
			int num = -1;
			int num2 = -1;
			for (int i = this.AASummoned.Count - 1; i >= 0; i--)
			{
				num2 = X.isinC<NelEnemy>(this.AASummoned[i], En);
				if (num2 >= 0)
				{
					num = i;
					break;
				}
			}
			EnemySummoner.EnemySummonedInfo enemySummonedInfo = X.Get<NelEnemy, EnemySummoner.EnemySummonedInfo>(EnemySummoner.OFirstPos, En);
			EnemySummoner.battleable_enemy_ = -1;
			if (enemySummonedInfo != null)
			{
				EnemySummoner.OFirstPos.Remove(En);
				if (enemySummonedInfo.K.DropReel != null)
				{
					if (enemySummonedInfo.K.NCXORSP(94) < DIFF.box_drop_ratio(this.Lp.skill_difficulty_restrict))
					{
						this.Lp.nM2D.IMNG.dropMBoxReel(enemySummonedInfo.K.DropReel, En.x, En.y, 0f, 0f);
					}
					enemySummonedInfo.K.DropReel = null;
				}
				if (enemySummonedInfo.K.temporary_adding_count && EnemySummoner.max_enemy_appear_whole_temp > 0)
				{
					EnemySummoner.max_enemy_appear_whole_temp--;
				}
				int num3 = X.Mx(this.current_splitter_id, enemySummonedInfo.K.splitter_id);
				for (int j = this.Asplitter_title.Count - 1; j >= num3 + 1; j--)
				{
					EnemySummoner.SplitterTerm splitterTerm = this.Asplitter_title[j];
					if (splitterTerm.clearBuffer())
					{
						this.Asplitter_title[j] = splitterTerm;
					}
				}
				foreach (KeyValuePair<string, EnemySummoner.EnemyAppearMax> keyValuePair in EnemySummoner.OMaxEnemyAppear)
				{
					if (enemySummonedInfo.K.isSame(keyValuePair.Key))
					{
						keyValuePair.Value.appeared--;
					}
				}
			}
			this.enemyOverDriveQuit(En);
			if (num >= 0)
			{
				EnemySummoner.OFnCheckEnemy.Clear();
				List<NelEnemy> list = this.AASummoned[num];
				list.RemoveAt(num2);
				if (list.Count == 0)
				{
					this.AASummoned.RemoveAt(num);
				}
				if (this.AOtherLsn != null)
				{
					if (En is EnemySummoner.IOtherKillListener)
					{
						this.AOtherLsn.Remove(En as EnemySummoner.IOtherKillListener);
					}
					for (int k = this.AOtherLsn.Count - 1; k >= 0; k--)
					{
						this.AOtherLsn[k].otherEnemyKilled(En);
					}
				}
			}
			this.Adecline_enemyid_buf.Clear();
		}

		public void listUpUIFadeKey(List<string> A, List<MGATTR> Aattr, EpSituation SituCon)
		{
			int count_movers = this.Mp.count_movers;
			for (int i = 0; i < count_movers; i++)
			{
				NelEnemy nelEnemy = this.Mp.getMv(i) as NelEnemy;
				if (!(nelEnemy == null) && nelEnemy.is_alive)
				{
					nelEnemy.addTortureUIFadeKeyFoGO(A, Aattr);
					SituCon.touchTempSituation(nelEnemy.id.ToString().ToUpper());
				}
			}
		}

		public M2BlockColliderContainer.BCCLine[] getFootableBcc(AIM a = AIM.B)
		{
			if (this.AABccFootable == null)
			{
				this.AABccFootable = new M2BlockColliderContainer.BCCLine[4][];
			}
			M2BlockColliderContainer.BCCLine[] array = this.AABccFootable[(int)a];
			if (array != null || this.Mp.BCC == null)
			{
				return array;
			}
			using (BList<M2BlockColliderContainer.BCCLine> blist = ListBuffer<M2BlockColliderContainer.BCCLine>.Pop(0))
			{
				M2BlockColliderContainer.BCCIterator bcciterator;
				this.Mp.BCC.ItrInit(out bcciterator, true);
				while (bcciterator.Next())
				{
					if (bcciterator.Cur.foot_aim == a)
					{
						blist.Add(bcciterator.Cur);
					}
				}
				int count_carryable_bcc = this.Mp.count_carryable_bcc;
				for (int i = 0; i < count_carryable_bcc; i++)
				{
					M2BlockColliderContainer carryableBCCByIndex = this.Mp.getCarryableBCCByIndex(i);
					if (carryableBCCByIndex.active && !(carryableBCCByIndex.BelongTo is M2BoxMover))
					{
						carryableBCCByIndex.ItrInit(out bcciterator, true);
						while (bcciterator.Next())
						{
							if (bcciterator.Cur.foot_aim == a)
							{
								blist.Add(bcciterator.Cur);
							}
						}
					}
				}
				array = (this.AABccFootable[(int)a] = blist.ToArray());
			}
			return array;
		}

		public void enemyOverDriveInit(NelEnemy N)
		{
			if (this.AOverdrived.IndexOf(N) >= 0)
			{
				return;
			}
			this.AOverdrived.Add(N);
		}

		public void enemyOverDriveQuit(NelEnemy N)
		{
			this.AOverdrived.Remove(N);
		}

		public bool hasOverDrivedEnemy(int thresh = 1)
		{
			return this.AOverdrived.Count >= thresh;
		}

		public List<List<NelEnemy>> getAASummoned()
		{
			return this.AASummoned;
		}

		public float mana_desire_multiple
		{
			get
			{
				if (this.AASummoned.Count == 0)
				{
					return 1f;
				}
				float num = 0f;
				foreach (KeyValuePair<NelEnemy, EnemySummoner.EnemySummonedInfo> keyValuePair in EnemySummoner.OFirstPos)
				{
					num += X.Mx(1f - keyValuePair.Key.mp_ratio, 0.125f);
				}
				return 1f / (X.Mx(0f, num - 1f) * 0.28f + 1f);
			}
		}

		public bool checkRingOut(NelEnemy En)
		{
			if (this.Lp == null)
			{
				return false;
			}
			if (!En.ringoutable || En.y < (float)(this.Lp.mapy + this.Lp.maph))
			{
				return true;
			}
			EnemySummoner.EnemySummonedInfo enemySummonedInfo;
			if (!EnemySummoner.OFirstPos.TryGetValue(En, out enemySummonedInfo))
			{
				return false;
			}
			Vector2 vector = enemySummonedInfo.FirstPos;
			vector = this.Lp.getWalkable(this.Mp, X.MMX((float)this.Lp.mapx, vector.x, (float)(this.Lp.mapx + this.Lp.mapw)), X.MMX((float)this.Lp.mapy, vector.y, (float)(this.Lp.mapy + this.Lp.maph)));
			En.setTo(vector.x, vector.y);
			En.initRingOut(false);
			return true;
		}

		public EnemySummoner close(bool clear_enemy = true, bool defeated = false)
		{
			if (EnemySummoner.ActiveScript == this)
			{
				EnemySummoner.ActiveScript = null;
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				BetobetoManager.cleanThreadAll();
				if (defeated && this.AReelAfterWholeDefeat != null)
				{
					for (int i = this.AReelAfterWholeDefeat.Count - 1; i >= 0; i--)
					{
						ReelManager.ItemReelDrop itemReelDrop = this.AReelAfterWholeDefeat[i];
						if (this.Lp != null)
						{
							if (EnemySummoner.XORSP() < DIFF.box_drop_ratio(this.Lp.skill_difficulty_restrict))
							{
								this.Lp.nM2D.IMNG.dropMBoxReel(itemReelDrop, this.Lp.mapfocx, this.Lp.mapfocy, 0f, 0f);
							}
						}
						else
						{
							nelM2DBase.IMNG.dropMBoxReel(itemReelDrop, nelM2DBase.getPrNoel().x, nelM2DBase.getPrNoel().y, 0f, 0f);
						}
					}
				}
				this.AReelAfterWholeDefeat = null;
				int num = (this.Lp.is_sudden_puppetrevenge ? 4 : nelM2DBase.NightCon.getReservedObtainableGrade(this, this.Lp.Mp, true, false));
				if (defeated && !this.Lp.is_sudden_puppetrevenge)
				{
					this.touchAllReelsObtain();
				}
				bool flag = false;
				if (this.Lp != null)
				{
					this.Lp.closeSummoner(defeated, out flag);
				}
				int count = EnemySummoner.ASummonerLitener.Count;
				for (int j = 0; j < count; j++)
				{
					EnemySummoner.ASummonerLitener[j].closeSummoner(this, defeated);
				}
				if (this.bgm_replace == "" && this.bgm_replace_when_close)
				{
					BGM.replace(120f, 160f, true, false);
					this.bgm_replace = null;
				}
				if (defeated)
				{
					EV.stack("__M2D_DEFEAT_SUMMONER", 0, -1, new string[]
					{
						this.key,
						num.ToString(),
						defeated ? (flag ? "2" : "1") : "0"
					}, null);
				}
				NightController.Xors.clearRandA();
			}
			this.obtainable_grade_ = -1;
			UIPicture.Recheck(60);
			if (clear_enemy)
			{
				for (int k = this.AASummoned.Count - 1; k >= 0; k--)
				{
					List<NelEnemy> list = this.AASummoned[k];
					int count2 = list.Count;
					for (int l = 0; l < count2; l++)
					{
						NelEnemy nelEnemy = list[l];
						if (nelEnemy != null)
						{
							try
							{
								nelEnemy.destruct();
							}
							catch
							{
							}
						}
					}
				}
				this.clear();
			}
			return null;
		}

		private void clear()
		{
			EnemySummoner.ASmnPos = null;
			this.AOtherLsn = null;
			EnemySummoner.AKindRest = null;
			EnemySummoner.golemtoy_divide_count = 3;
			this.AASummoned.Clear();
			EnemySummoner.next_script_key = null;
			this.AOverdrived.Clear();
			EnemySummoner.max_enemy_appear_whole_temp = 0;
			EnemySummoner.OFirstPos.Clear();
			this.Lp = null;
		}

		public void touchAllReelsObtain()
		{
			if (this.AReel != null)
			{
				for (int i = this.AReel.Length - 1; i >= 0; i--)
				{
					ReelManager.ItemReelDrop itemReelDrop = this.AReel[i];
					if (itemReelDrop != null && itemReelDrop.IR != null)
					{
						itemReelDrop.IR.GReelItem.touchObtainCount();
					}
				}
			}
		}

		public static void prepareListenerEval(TxEvalListenerContainer EvalT)
		{
			EvalT.Add("enemy_count", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (EnemySummoner.ActiveScript != null && Aargs.Count >= 1)
				{
					TX.InputE((float)EnemySummoner.ActiveScript.countActiveEnemy(Aargs[0], true, true, true));
				}
			}, Array.Empty<string>());
		}

		public static int xors(int i)
		{
			return NightController.xors(i);
		}

		public static float NIXP(float v, float v2)
		{
			return X.NI(v, v2, EnemySummoner.XORSP());
		}

		public static float XORSP()
		{
			return NightController.XORSP();
		}

		public static float XORSPS()
		{
			return (EnemySummoner.XORSP() - 0.5f) * 2f;
		}

		public static T RanSelect<T>(T[] A, int seedA = 1389, int seedB = 51)
		{
			int num = (int)(X.RAN((uint)(EnemySummoner.ran_session + seedA + (EnemySummoner.ran_session & 63) * 11 + EnemySummoner.summoned_count * 43), 5 + EnemySummoner.summoned_count * seedB) * (float)A.Length);
			return A[num];
		}

		public static void shuffle<T>(T[] A, int arraymax = -1)
		{
			NightController.shuffle<T>(A, arraymax);
		}

		public static void shuffle<T>(List<T> A, int arraymax = -1)
		{
			NightController.shuffle<T>(A, arraymax);
		}

		public int max_enemy_appear_current_total
		{
			get
			{
				return EnemySummoner.max_enemy_appear_whole + EnemySummoner.max_enemy_appear_whole_temp;
			}
		}

		public float enemy_count
		{
			get
			{
				return (float)this.AASummoned.Count;
			}
		}

		public bool need_fine
		{
			get
			{
				return this.CR == null;
			}
		}

		public string getClearedSfKey(Map2d Mp)
		{
			return Mp.key + ".." + this.key;
		}

		public int getObtainableGrade(Map2d Mp)
		{
			if (this.obtainable_grade_ == -1)
			{
				this.obtainable_grade_ = (M2DBase.Instance as NelM2DBase).NightCon.getObtainableGrade(this, Mp, true);
			}
			return this.obtainable_grade_;
		}

		public string getOverdriveCapacityStr()
		{
			return this.overdrive_pre_calced_count.ToString() + (M2DBase.Instance as NelM2DBase).NightCon.add_overdriveable_str(this);
		}

		public int getOverdriveCapacity()
		{
			return this.overdrive_pre_calced_count + (M2DBase.Instance as NelM2DBase).NightCon.add_overdriveable_count(this);
		}

		public List<ENEMYID> getAppearEnemyAbout()
		{
			if (this.about_enemy_count_ == -1)
			{
				this.about_enemy_count_ = this.calcAboutEnemyCount(this.AIdListAbout, true, false);
			}
			return this.AIdListAbout;
		}

		public int getAppearEnemyOdAbout()
		{
			if (this.about_enemy_count_ == -1)
			{
				this.about_enemy_count_ = this.calcAboutEnemyCount(this.AIdListAbout, true, false);
			}
			return this.overdrive_pre_calced_count;
		}

		public Map2d Mp
		{
			get
			{
				return this.Lp.Mp;
			}
		}

		public bool prepared
		{
			get
			{
				return this.CR != null;
			}
			set
			{
				if (!this.prepared)
				{
					this.initializeCR(this.Aload_enemy, false);
				}
			}
		}

		public M2LpSummon getSummonedArea()
		{
			return this.Lp;
		}

		public bool AreaContainsMv(M2Mover Mv, float extend_px = 0f)
		{
			return this.Lp == null || this.Lp.isContainingMover(Mv, extend_px);
		}

		public bool AreaCoveringMv(M2Mover Mv, float extend_px = 0f)
		{
			return this.Lp == null || this.Lp.isCoveringMover(Mv, extend_px);
		}

		public float CLEN
		{
			get
			{
				return this.Lp.Mp.CLEN;
			}
		}

		public int getRestCount()
		{
			return EnemySummoner.enemy_rest;
		}

		public bool isActive()
		{
			return EnemySummoner.ActiveScript == this;
		}

		public static bool isActiveBorder()
		{
			return EnemySummoner.ActiveScript != null && EnemySummoner.ActiveScript.Lp != null && !EnemySummoner.ActiveScript.Lp.type_no_border;
		}

		public bool has_puppetrevenge_replace
		{
			get
			{
				return this.puppetrevenge_replace != null;
			}
		}

		public bool puppetrevenge_enable
		{
			get
			{
				return this.Lp != null && this.Lp.is_sudden_puppetrevenge;
			}
		}

		public bool is_dangerous
		{
			get
			{
				return this.is_dangerous_ == 1;
			}
		}

		public int skill_difficulty_restrict
		{
			get
			{
				if (this.Lp == null)
				{
					return DIFF.I;
				}
				return this.Lp.skill_difficulty_restrict;
			}
		}

		public EnemySummoner.EnemySummonedInfo getSummonedInfo(NelEnemy N)
		{
			return X.Get<NelEnemy, EnemySummoner.EnemySummonedInfo>(EnemySummoner.OFirstPos, N);
		}

		public ReelManager.ItemReelDrop[] getItemReelVector()
		{
			return this.AReel;
		}

		public bool isEnemyEventUsing()
		{
			return this.Lp != null && this.Lp.type_event_enemy;
		}

		public static string Lp2smn(string lp_key)
		{
			return TX.slice(lp_key, "Summon".Length + 1);
		}

		public M2AirRegionContainer getRegionCheckerAir()
		{
			if (this.WRC_Air == null)
			{
				this.WRC_Air = new M2AirRegionContainer(this.Mp, null);
				this.WRC_Air.recognizeInit(this.Mp, this.Lp.mapx, this.Lp.mapy, this.Lp.mapw, this.Lp.maph, null);
				this.WRC_Air.recognize(-1);
			}
			return this.WRC_Air;
		}

		public const string header_LpSummon = "Summon";

		private static readonly Regex RegSmnClmAndRow = new Regex("^([A-Za-z0-9_]+)_([0-9]+)");

		private static BDic<string, EnemySummoner> ORecord;

		public static EnemySummoner ActiveScript;

		private static EfParticleOnce EfpKira;

		private const string summon_dir = "Data/summon/";

		private const string extension_full = ".summon.csv";

		private const string extension_resource = ".summon";

		public readonly string key;

		public string base_script;

		private static int enemy_index = 0;

		public static EnemySummoner LastSummoner;

		private static EfParticleFuncCalc FuncBase;

		private static List<EnemySummoner.ISummonActivateListener> ASummonerLitener = new List<EnemySummoner.ISummonActivateListener>();

		private bool use_if;

		private int main_data_line = -2;

		private CsvReaderA CR;

		public int grade = 1;

		public bool only_night;

		public static int golemtoy_divide_count;

		private bool need_resource_load = true;

		private readonly List<string> Aload_enemy;

		private int obtainable_grade_ = -1;

		public int current_splitter_id = -1;

		private static EnemySummoner.SmnPoint[] ASmnPos;

		private static List<EnemySummoner.SmnEnemyKind> AKindRest;

		public static int summoned_count;

		private static int enemy_rest = 0;

		public static int ran_session;

		private static int max_enemy_appear_whole_temp;

		private static int max_enemy_appear_whole;

		private static BDic<string, EnemySummoner.EnemyAppearMax> OMaxEnemyAppear;

		private static float delay_one;

		private static float delay_one_second;

		private static float delay_filled;

		private static float delay = 0f;

		private static float enemy_respawn_timescale_when_exists = 1f;

		private static float weight_add;

		private static string next_script_key;

		private static int battleable_enemy_ = -1;

		private readonly List<List<NelEnemy>> AASummoned;

		private const int INITIAL_DELAY = 50;

		private const int DELAY_FILLED_DEFAULT = 90;

		public int current_overdrive_rest;

		private static BDic<NelEnemy, EnemySummoner.EnemySummonedInfo> OFirstPos;

		private static BDic<NelEnemy.FnCheckEnemy, int> OFnCheckEnemy;

		private M2LpSummon Lp;

		private M2AirRegionContainer WRC_Air;

		private readonly List<NelEnemy> AOverdrived;

		private ReelManager.ItemReelDrop[] AReel;

		private ReelManager.ItemReelContainer[] AReelSecretSrc;

		public float replace_secret_to_lower;

		private List<ReelManager.ItemReelDrop> AReelAfterWholeDefeat;

		private List<EnemySummoner.IOtherKillListener> AOtherLsn;

		private List<string> Adecline_enemyid_buf;

		private List<EnemySummoner.SplitterTerm> Asplitter_title;

		private int overdrive_pre_calced_count;

		private int thunder_odable = 1;

		private int thunder_odable_def = 1;

		public string bgm_replace;

		public string bgm_block_ovr;

		public string bgm_block_to;

		public bool bgm_replace_when_close = true;

		public string fatal_key;

		private string puppetrevenge_replace;

		private CsvReaderA CR_puppetrevenge;

		private static VariableP VPTemp;

		private const string follower_splitter_title = "_FOLLOW_";

		public const string evt_defeat = "__M2D_DEFEAT_SUMMONER";

		private int about_enemy_count_ = -1;

		private readonly List<ENEMYID> AIdListAbout;

		private M2BlockColliderContainer.BCCLine[][] AABccFootable;

		private byte is_dangerous_ = 2;

		public interface ISummonActivateListener
		{
			void openSummoner(EnemySummoner Smn, bool is_active_border);

			void closeSummoner(EnemySummoner Smn, bool defeated);
		}

		public sealed class SmnPoint
		{
			public SmnPoint(M2LabelPoint _Lp, float _x, float _y, float _weight, string[] Aalloc_id_key = null)
			{
				uint num = NightController.Xors.get2((uint)EnemySummoner.enemy_index, (uint)(EnemySummoner.enemy_index % 39));
				this.Set(_Lp, _x, _y, _weight, X.RAN(num, 323), Aalloc_id_key);
			}

			public SmnPoint(M2LabelPoint _Lp, float _x, float _y, float _weight, float _ag01, string[] Aalloc_id_key = null)
			{
				this.Set(_Lp, _x, _y, _weight, _ag01, Aalloc_id_key);
			}

			public EnemySummoner.SmnPoint Set(M2LabelPoint _Lp, float _x, float _y, float _weight, float _ag01, string[] Aalloc_id_key = null)
			{
				this.x = _x;
				this.y = _y;
				this.Lp = _Lp;
				if (this.Lp == null)
				{
					this.pos_cnt = -1;
				}
				this.weight0 = ((_weight <= 0f) ? 1f : _weight);
				this.pos_agR = ((float)EnemySummoner.enemy_index * 0.133f + _ag01) * 6.2831855f;
				if (Aalloc_id_key != null && Aalloc_id_key.Length != 0)
				{
					this.Aalloc_id = Aalloc_id_key;
					for (int i = this.Aalloc_id.Length - 1; i >= 0; i--)
					{
						this.Aalloc_id[i] = this.Aalloc_id[i].ToUpper();
					}
				}
				return this;
			}

			public bool available(float weight_add)
			{
				return this.weight0 + weight_add * this.weight0 - this.use_weight >= 1f;
			}

			public bool idMatch(string e)
			{
				if (this.Aalloc_id != null)
				{
					for (int i = this.Aalloc_id.Length - 1; i >= 0; i--)
					{
						string text = this.Aalloc_id[i];
						if (EnemyData.isSame(e, text, false))
						{
							return true;
						}
					}
				}
				return false;
			}

			public void AddTo(List<EnemySummoner.SmnPoint> ABuf, float weight_add)
			{
				int num = (int)(this.weight0 + weight_add * this.weight0 - this.use_weight);
				while (--num >= 0)
				{
					ABuf.Add(this);
				}
			}

			public float x;

			public float y;

			public float weight0;

			public float use_weight;

			public string[] Aalloc_id;

			public int pos_cnt;

			public float pos_agR;

			public bool sudden_appear;

			public M2LabelPoint Lp;
		}

		public sealed class SmnEnemyKind
		{
			public SmnEnemyKind(string _enemyid, int _def_count, int _splitter_id, float _mp_ratio_min, float _mp_ratio_max, string _zyouken, float _count_add_weight = 0f, float _mp_add_weight = 0f)
			{
				this.enemyid = _enemyid;
				this.EnemyDesc = EnemyData.getTypeAndId(this.enemyid);
				this.def_count = _def_count;
				this.splitter_id = _splitter_id;
				this.mp_ratio_min = _mp_ratio_min;
				this.mp_ratio_max = X.Mx(this.mp_ratio_min, _mp_ratio_max);
				this.zyouken = (TX.valid(_zyouken) ? _zyouken : "");
				this.count_add_weight = _count_add_weight;
				this.mp_add_weight = _mp_add_weight;
				this.smn_xorsp = NightController.XORSP();
			}

			public SmnEnemyKind(EnemySummoner.SmnEnemyKind Src)
			{
				this.enemyid = Src.enemyid;
				this.EnemyDesc = Src.EnemyDesc;
				this.def_count = Src.def_count;
				this.pre_overdrive = Src.pre_overdrive;
				this.splitter_id = Src.splitter_id;
				this.mp_ratio_min = Src.mp_ratio_min;
				this.mp_ratio_max = Src.mp_ratio_max;
				this.zyouken = Src.zyouken;
				this.count_add_weight = Src.count_add_weight;
				this.mp_add_weight = Src.mp_add_weight;
				this.temporary_adding_count = Src.temporary_adding_count;
				this.smn_xorsp = NightController.XORSP();
			}

			public bool count_fix
			{
				get
				{
					return this.count_add_weight == 0f;
				}
			}

			public bool isSame(string s)
			{
				if (TX.isStart(s, "OD_", 0))
				{
					s = TX.slice(s, 3);
					if (!this.pre_overdrive)
					{
						return false;
					}
				}
				return EnemyData.isSame(this.enemyid, s, false);
			}

			public string check_str
			{
				get
				{
					if (this.check_str_ == null)
					{
						this.check_str_ = this.enemyid + (this.thunder_overdrive ? ":T" : "");
					}
					return this.check_str_;
				}
			}

			public float NCXORSP(int val)
			{
				return X.frac((float)val * this.smn_xorsp);
			}

			public string enemyid;

			public EnemyData.EnemyDescryption EnemyDesc;

			public int splitter_id;

			public float mp_ratio_min;

			public float mp_ratio_max;

			public string zyouken = "";

			public float count_add_weight = 1f;

			public float mp_add_weight = 1f;

			public byte mp_added;

			public bool thunder_overdrive;

			public bool pre_overdrive;

			public int def_count;

			public EnemySummoner.SmnEnemyKind DupeConnect;

			public bool temporary_adding_count;

			public float smn_xorsp;

			private string check_str_;

			public ReelManager.ItemReelDrop DropReel;
		}

		public sealed class EnemySummonedInfo
		{
			public EnemySummonedInfo(EnemySummoner.SmnEnemyKind _K, Vector2 _FirstPos, EnemySummoner.SmnPoint _PosInfo)
			{
				this.K = _K;
				this.FirstPos = _FirstPos;
				this.PosInfo = _PosInfo;
			}

			public EnemySummoner.SmnEnemyKind K;

			public Vector2 FirstPos;

			public EnemySummoner.SmnPoint PosInfo;
		}

		private sealed class EnemyAppearMax
		{
			public EnemyAppearMax(int _max)
			{
				this.max = _max;
			}

			public readonly int max;

			public int appeared;
		}

		private struct SplitterTerm
		{
			public SplitterTerm(CsvReader CR = null, int delay_one_second = 0)
			{
				this.can_go_through_ = 2;
				this.delay = CR.Int(3, delay_one_second);
				if (CR == null)
				{
					this.title = "";
					this.Term = null;
					return;
				}
				this.title = CR._1;
				this.Term = null;
				if (TX.valid(CR._2))
				{
					if (CR._2 == "1" || CR._2 == "true")
					{
						this.can_go_through_ = 3;
						return;
					}
					this.Term = new EvalP(null).Parse(CR._2);
				}
			}

			public bool can_go_through(int index, EnemySummoner Con)
			{
				if (this.can_go_through_ == 2)
				{
					if (this.Term == null)
					{
						this.can_go_through_ = ((Con.countSplitterEnemy(index - 1, true, true, true) > 0) ? 0 : 1);
					}
					else
					{
						this.can_go_through_ = ((this.Term.getValue(EnemySummoner.VPTemp) != 0.0) ? 1 : 0);
					}
				}
				return this.can_go_through_ > 0;
			}

			public bool clearBuffer()
			{
				if (this.can_go_through_ <= 1)
				{
					this.can_go_through_ = 2;
					return true;
				}
				return false;
			}

			public readonly string title;

			public readonly EvalP Term;

			public readonly int delay;

			private byte can_go_through_;
		}

		public interface IOtherKillListener
		{
			void otherEnemyKilled(NelEnemy Other);
		}

		private enum PROG_TYPE
		{
			ALWAYS = 1,
			CLMN_ENEMY_COUNT,
			ALL_ENEMY_COUNT = 4
		}
	}
}

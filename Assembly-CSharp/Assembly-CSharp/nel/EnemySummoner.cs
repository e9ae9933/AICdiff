using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using evt;
using m2d;
using nel.smnp;
using UnityEngine;
using XX;

namespace nel
{
	public class EnemySummoner
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
			enemySummoner = new EnemySummoner(key);
			if (enemySummoner.key_header != null)
			{
				EnemySummoner.ORecord[key] = enemySummoner;
				return enemySummoner;
			}
			return null;
		}

		public static void FlushAll()
		{
			foreach (KeyValuePair<string, EnemySummoner> keyValuePair in EnemySummoner.ORecord)
			{
				keyValuePair.Value.flush_memory(true, true, true);
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

		public EnemySummoner(string _key)
		{
			this.key = _key;
			int num = this.key.IndexOf("_");
			if (num < 0)
			{
				return;
			}
			this.key_header = TX.slice(this.key, 0, num);
			if (EnemySummoner.FD_countGolem == null)
			{
				EnemySummoner.FD_countGolem = new NelEnemy.FnCheckEnemy(NelNGolem.countGolem);
				EnemySummoner.FD_countGolemToy = new NelEnemy.FnCheckEnemy(NelNGolem.countGolemToy);
			}
			this.AIdListAbout = new List<int>();
			this.Aload_enemy = new List<string>(2);
		}

		public void releaseLp()
		{
			this.Lp = null;
			this.AABccFootable = null;
		}

		public EnemySummoner flush_memory(bool is_initAction = true, bool clear_cr = false, bool clear_qentry = true)
		{
			this.about_enemy_count_ = -1;
			this.obtainable_grade_ = -1;
			this.need_resource_load = true;
			if (clear_qentry)
			{
				this.QEntry = default(QuestTracker.SummonerEntry);
			}
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

		public virtual EnemySummonerManager GetManager()
		{
			return EnemySummonerManager.GetManager(this.key_header);
		}

		protected virtual CsvReaderA createCR()
		{
			EnemySummonerManager manager = this.GetManager();
			if (manager == null)
			{
				X.de("所属SMMが不明 " + this.key, null);
				return null;
			}
			EnemySummonerManager.SDescription sdescription;
			return new CsvReaderA(manager.getSummonerScript(this.key, out sdescription, false) ?? "", new CsvVariableContainer(EV.getVariableContainer()));
		}

		public virtual CsvReaderA initializeCR(List<string> Aenemy, bool force_reload = false)
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
			this.do_not_open = false;
			this.CR = this.createCR();
			if (this.CR == null)
			{
				return null;
			}
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
						if (this.CR.cmd == "#DO_NOT_OPEN")
						{
							this.do_not_open = true;
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

		public List<int> listupAboutEnemyForFG(List<int> Adest)
		{
			if (this.CR == null)
			{
				this.createCR();
				if (this.CR == null)
				{
					return Adest;
				}
			}
			this.CR.seek_set(0);
			while (this.CR.readCorrectly())
			{
				ENEMYID enemyid;
				if (!TX.noe(this.CR.getLastStr()) && this.CR.stringsInput(this.CR.getLastStr()) && this.CR.cmd.IndexOf("#") != 0 && (this.CR.cmd == "%EN" || this.CR.cmd == "%EN_OD") && FEnum<ENEMYID>.TryParse(this.CR._1, out enemyid, true))
				{
					X.pushIdentical<int>(Adest, (int)(enemyid | ((this.CR.cmd == "%EN_OD") ? ((ENEMYID)2147483648U) : ((ENEMYID)0U))));
				}
			}
			return Adest;
		}

		public virtual void fineReelData(string key, ref ReelManager.ItemReelDrop[] AReel, ref ReelManager.ItemReelContainer[] AReelSecretSrc, ref float replace_secret_to_lower)
		{
			ReelManager.ItemReelContainer[] array;
			float num;
			if (SupplyManager.GetForSummoner(key, out array, out AReelSecretSrc, out num, false))
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
					if (!NDAT.getResources(Mp.M2D as NelM2DBase, this.Aload_enemy[i]))
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
			this.QEntry = default(QuestTracker.SummonerEntry);
			if (this.CR == null)
			{
				this.loadMaterial(Lp.Mp, false);
			}
			Map2d mp = Lp.Mp;
			CsvReaderA csvReaderA = this.createCRPuppetRevenge(true);
			if (csvReaderA == null)
			{
				return;
			}
			while (csvReaderA.read())
			{
				if ((csvReaderA.cmd == "%EN" || csvReaderA.cmd == "%EN_OD") && !NDAT.getResources(Lp.nM2D, csvReaderA._1))
				{
					csvReaderA.tError("エネミーID指定に誤りがあります！ :" + csvReaderA._1);
				}
			}
		}

		public CsvReaderA createCRPuppetRevenge(bool reload = true)
		{
			if (!reload && this.CR_puppetrevenge != null)
			{
				return this.CR_puppetrevenge;
			}
			EnemySummonerManager.SDescription sdescription;
			string summonerScript = EnemySummonerManager.GetManager("_other").getSummonerScript(this.puppetrevenge_replace, out sdescription, false);
			if (summonerScript == null)
			{
				return null;
			}
			if (this.CR_puppetrevenge == null)
			{
				this.CR_puppetrevenge = new CsvReaderA(summonerScript, this.CR.VarCon);
			}
			else
			{
				this.CR_puppetrevenge.parseText(summonerScript);
			}
			return this.CR_puppetrevenge;
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
			List<int> appearEnemyAbout = this.getAppearEnemyAbout();
			int count = appearEnemyAbout.Count;
			for (int i = 0; i < count; i++)
			{
				ENEMYID enemyid = (ENEMYID)appearEnemyAbout[i];
				text2 = ((text2 == "" && left_tab) ? "\u3000\u3000" : "") + TX.add(text2, NDAT.getEnemyName(enemyid, true), ", ");
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
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add(this.grade);
				if (obtainableGrade < this.grade)
				{
					stb.AddTxA("Summoner__obtainable_dangerousness", false);
					stb.TxRpl(obtainableGrade);
				}
				Stb.TxRpl(stb);
			}
			Stb.TxRpl(TX.Get("Summoner_descryption_enemies", ""));
			Stb.TxRpl(text);
			Stb.TxRpl(this.getOverdriveCapacityStr());
			if (this.only_night)
			{
				Stb.AppendTxA("Summoner_can_battle_in_night", "\n");
			}
		}

		public int calcAboutEnemyCount(List<int> AIdList, bool clear_list = true, bool add_follower_to_list = false)
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
							NDAT.EnemyDescryption typeAndId = NDAT.getTypeAndId(this.CR._1);
							if (!typeAndId.valid)
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
								if ((!flag2 || add_follower_to_list) && AIdList != null && AIdList.IndexOf((int)enemyid) == -1)
								{
									AIdList.Add((int)enemyid);
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
			return EnemySummoner.FuncBase.Get(-1f, this.getDangerLevel(), false);
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

		public virtual void activate(M2LpSummon _Lp, out bool bgm_replaced)
		{
			bgm_replaced = false;
			this.loadMaterial(_Lp.Mp, this.use_if);
			if (this.CR == null)
			{
				X.dl("CRが読み込まれていないためオープンされませんでした (maindataline=" + this.main_data_line.ToString(), null, false, false);
				return;
			}
			this.activateInner(_Lp, out bgm_replaced, null);
		}

		protected void activateInner(M2LpSummon _Lp, out bool bgm_replaced, EnemySummoner.FnCreateSummonerPlayer FnCreatePlayer = null)
		{
			bgm_replaced = false;
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
			this.Lp = _Lp;
			EnemySummoner.ActiveScript = this;
			this.Player = ((FnCreatePlayer != null) ? FnCreatePlayer(this, EnemySummoner.FuncBase, this.CR, out bgm_replaced) : new SummonerPlayer(this, EnemySummoner.FuncBase, this.CR, out bgm_replaced, true));
			this.callOpenSummonerToListener(EnemySummoner.isActiveBorder());
			this.Player.run(0f);
		}

		public void callOpenSummonerToListener(bool is_active_border)
		{
			int count = EnemySummoner.ASummonerLitener.Count;
			for (int i = 0; i < count; i++)
			{
				EnemySummoner.ASummonerLitener[i].openSummoner(this, is_active_border);
			}
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

		public EnemySummoner close(bool clear_enemy = true, bool defeated = false)
		{
			SummonerPlayer player = this.Player;
			this.Player = null;
			if (EnemySummoner.ActiveScript == this)
			{
				EnemySummoner.ActiveScript = null;
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				BetobetoManager.cleanThreadAll();
				if (player != null)
				{
					player.close(defeated);
				}
				int reservedObtainableGrade = nelM2DBase.NightCon.getReservedObtainableGrade(this, true, false);
				bool flag = false;
				if (this.Lp != null)
				{
					this.Lp.closeSummoner(defeated, out flag);
				}
				int count = EnemySummoner.ASummonerLitener.Count;
				for (int i = 0; i < count; i++)
				{
					EnemySummoner.ASummonerLitener[i].closeSummoner(this, defeated);
				}
				if (defeated)
				{
					EV.stack("__M2D_DEFEAT_SUMMONER", 0, -1, new string[]
					{
						this.key,
						this.defeat_ev_var2(reservedObtainableGrade),
						defeated ? (flag ? "2" : "1") : "0",
						this.defeat_after_event
					}, null);
					this.QEntry = default(QuestTracker.SummonerEntry);
				}
				NightController.Xors.updateFirst(40);
			}
			this.obtainable_grade_ = -1;
			UIPicture.Recheck(60);
			if (clear_enemy && player != null)
			{
				player.clearEnemy();
			}
			return null;
		}

		protected virtual string defeat_ev_var2(int obtainable)
		{
			return obtainable.ToString();
		}

		protected virtual string defeat_after_event
		{
			get
			{
				return "";
			}
		}

		public static void prepareListenerEval(TxEvalListenerContainer EvalT)
		{
			EvalT.Add("enemy_count", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (EnemySummoner.ActiveScript != null && Aargs.Count >= 1)
				{
					TX.InputE((float)EnemySummoner.ActiveScript.getPlayer().countActiveEnemy(Aargs[0], true, true, true));
				}
			}, Array.Empty<string>());
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
						text = (flag ? "???" : TX.ReplaceTX(itemReelContainer.tx_key, false)) + ((i < num2 - 1) ? "／ " : "")
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
						EnemySummoner.EfpKira = new EfParticleOnce("mbox_unikira", EFCON_TYPE.FIXED);
					}
					EnemySummoner.EfpKira.index = X.GETRAN2(num, 4);
					EnemySummoner.EfpKira.drawTo(Md, 0f, 0f, 14f, (int)C32.c2d(C32.MulA(itemReelContainer.ColSet.top, 0.45f * alpha)), (float)IN.totalframe, EnemySummoner.EfpKira.delay * 14f);
				}
				flag2 = false;
			}
			Md.updateForMeshRenderer(false);
			return flag2;
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

		public List<int> getAppearEnemyAbout()
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

		public CsvReaderA getCsvReader()
		{
			return this.CR;
		}

		public M2LpSummon getSummonedArea()
		{
			return this.Lp;
		}

		public bool isActive()
		{
			return EnemySummoner.ActiveScript == this;
		}

		public SummonerPlayer getPlayer()
		{
			return this.Player;
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

		public static SummonerPlayer ActivePlayer
		{
			get
			{
				if (EnemySummoner.ActiveScript == null)
				{
					return null;
				}
				return EnemySummoner.ActiveScript.getPlayer();
			}
		}

		public ReelManager.ItemReelDrop[] getItemReelVector()
		{
			return this.AReel;
		}

		public ReelManager.ItemReelContainer[] getItemReelVectorForSecret()
		{
			return this.AReelSecretSrc;
		}

		public int get_main_data_line()
		{
			return X.Mx(this.main_data_line, 0);
		}

		public int get_thunder_odable_def()
		{
			return this.thunder_odable_def;
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

		public const string csv_enemy_entry = "%EN";

		public const string csv_od_enemy_entry = "%EN_OD";

		public const string csv_puppetrevenge_entry = "%PUPPETREVENGE";

		public const string follower_splitter_title = "_FOLLOW_";

		public readonly string key;

		public readonly string key_header;

		public bool dropable_special_item = true;

		public QuestTracker.SummonerEntry QEntry;

		public static EnemySummoner LastSummoner;

		private static EfParticleFuncCalc FuncBase;

		private static List<EnemySummoner.ISummonActivateListener> ASummonerLitener = new List<EnemySummoner.ISummonActivateListener>();

		protected bool use_if;

		private int main_data_line = -2;

		private CsvReaderA CR;

		public int grade = 1;

		public bool only_night;

		public bool do_not_open;

		protected bool need_resource_load = true;

		protected readonly List<string> Aload_enemy;

		private int obtainable_grade_ = -1;

		private M2LpSummon Lp;

		private M2AirRegionContainer WRC_Air;

		protected ReelManager.ItemReelDrop[] AReel;

		protected ReelManager.ItemReelContainer[] AReelSecretSrc;

		public float replace_secret_to_lower;

		private int overdrive_pre_calced_count;

		private int thunder_odable_def = 1;

		public string fatal_key;

		private string puppetrevenge_replace;

		private CsvReaderA CR_puppetrevenge;

		private SummonerPlayer Player;

		public const string evt_defeat = "__M2D_DEFEAT_SUMMONER";

		private readonly List<int> AIdListAbout;

		private int about_enemy_count_ = -1;

		private readonly List<int> intbout;

		public static NelEnemy.FnCheckEnemy FD_countGolem;

		public static NelEnemy.FnCheckEnemy FD_countGolemToy;

		private M2BlockColliderContainer.BCCLine[][] AABccFootable;

		private byte is_dangerous_ = 2;

		public delegate SummonerPlayer FnCreateSummonerPlayer(EnemySummoner _Summoner, EfParticleFuncCalc _FuncBase, CsvReaderA _CR, out bool bgm_replaced);

		public interface ISummonActivateListener
		{
			void openSummoner(EnemySummoner Smn, bool is_active_border);

			void closeSummoner(EnemySummoner Smn, bool defeated);
		}

		private enum PROG_TYPE
		{
			ALWAYS = 1,
			CLMN_ENEMY_COUNT,
			ALL_ENEMY_COUNT = 4
		}
	}
}

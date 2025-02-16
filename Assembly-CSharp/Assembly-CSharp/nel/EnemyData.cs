using System;
using Better;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public static class EnemyData
	{
		public static void prepareData()
		{
			EnemyData.Oscript = new BDic<string, string>();
			EnemyData.OSerResist = new BDic<string, FlagCounter<SER>>();
			EnemyData.prepareSerDataScript();
			NOD.reloadNODScript();
		}

		private static void prepareSerDataScript()
		{
			using (MTI mti = new MTI("Enemies/_ser_resist", "_"))
			{
				CsvReaderA csvReaderA = new CsvReaderA(mti.LoadText("_ser_resist", null, true), false);
				FlagCounter<SER> flagCounter = new FlagCounter<SER>(4);
				string[] array = null;
				string[] array2 = null;
				FlagCounter<SER> flagCounter2 = null;
				for (;;)
				{
					bool flag = false;
					if (csvReaderA.readCorrectly())
					{
						if (csvReaderA.stringsInput(csvReaderA.getLastStr()))
						{
							SER ser2;
							if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
							{
								array = TX.split(csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1), "|");
								flag = true;
							}
							else if (csvReaderA.cmd == "%ALL_SER_RESIST")
							{
								int num = csvReaderA.Int(1, 50);
								int num2 = 35;
								for (int i = 0; i < num2; i++)
								{
									SER ser = (SER)((long)i);
									flagCounter.Add(ser, (float)num);
								}
							}
							else if (FEnum<SER>.TryParse(csvReaderA.cmd.ToUpper(), out ser2, true))
							{
								int num3 = csvReaderA.Int(1, 50);
								flagCounter.Add(ser2, (float)num3);
							}
						}
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						if (array2 != null)
						{
							int num4 = array2.Length;
							for (int j = 0; j < num4; j++)
							{
								string text = array2[j];
								FlagCounter<SER> flagCounter3;
								if (!EnemyData.OSerResist.TryGetValue(text, out flagCounter3))
								{
									flagCounter3 = (EnemyData.OSerResist[text] = new FlagCounter<SER>(4));
									if (text == "_DEFAULT_RESIST")
									{
										flagCounter2 = flagCounter3;
									}
									if (flagCounter2 != null && flagCounter2 != flagCounter3)
									{
										flagCounter3.Add(flagCounter2, 1f);
									}
								}
								flagCounter3.Add(flagCounter, 1f);
							}
						}
						if (csvReaderA.isEnd())
						{
							break;
						}
						flagCounter = new FlagCounter<SER>(4);
						array2 = array;
					}
				}
			}
		}

		public static string getData(string key)
		{
			string text = "";
			string text2;
			if (EnemyData.Oscript.TryGetValue(key, out text2))
			{
				text = text2 + "\n" + text;
			}
			if (REG.match(key, REG.RegSuffixNumber))
			{
				key = REG.leftContext;
				if (EnemyData.Oscript.TryGetValue(key, out text2))
				{
					text = text2 + "\n" + text;
				}
			}
			return text;
		}

		public static FlagCounter<SER> getSerRegist(string key, FlagCounter<SER> Default = null, bool check_suffix = true)
		{
			FlagCounter<SER> flagCounter;
			if (EnemyData.OSerResist.TryGetValue(key, out flagCounter))
			{
				return flagCounter;
			}
			if (check_suffix && REG.match(key, REG.RegSuffixNumber))
			{
				key = REG.leftContext;
				if (EnemyData.OSerResist.TryGetValue(key, out flagCounter))
				{
					return flagCounter;
				}
			}
			return Default ?? EnemyData.getSerRegistDefault();
		}

		public static FlagCounter<SER> getSerRegistDefault()
		{
			return X.Get<string, FlagCounter<SER>>(EnemyData.OSerResist, "_DEFAULT_RESIST");
		}

		public static void loadPxl(NelM2DBase M2D, string key)
		{
			M2D.loadMaterialPxl("N_" + key, "Enemies/" + key + ".pxls", true, false);
		}

		public static bool getResources(NelM2DBase M2D, string key)
		{
			if (key.IndexOf("SLIME_") == 0)
			{
				M2D.loadMaterialSnd("enemy_slime");
				EnemyData.loadPxl(M2D, "slime");
				M2D.prepareSvTexture("torture_slime_1", false);
			}
			else if (key.IndexOf("MUSH_") == 0)
			{
				M2D.loadMaterialSnd("enemy_mush");
				EnemyData.loadPxl(M2D, "mush");
				M2D.prepareSvTexture("damage_shrimp", false);
				M2D.prepareSvTexture("stand_weak", false);
			}
			else if (key.IndexOf("PUPPY_") == 0)
			{
				M2D.loadMaterialSnd("enemy_puppy");
				EnemyData.loadPxl(M2D, "puppy");
				M2D.prepareSvTexture("damage_frog", false);
			}
			else if (key.IndexOf("GOLEM_") == 0)
			{
				M2D.loadMaterialSnd("enemy_golem");
				EnemyData.loadPxl(M2D, "golem");
				EnemyData.loadPxl(M2D, "golemtoy");
				M2D.prepareSvTexture("damage_horse", false);
				M2D.prepareSvTexture("damage_mkb", false);
				M2D.prepareSvTexture("damage_behind", false);
			}
			else if (key.IndexOf("MECHGOLEM_") == 0)
			{
				M2D.loadMaterialSnd("enemy_mgolem");
				M2D.loadMaterialSnd("enemy_golem");
				EnemyData.loadPxl(M2D, "mechgolem");
			}
			else if (key.IndexOf("MAGE_") == 0)
			{
				EnemyData.loadPxl(M2D, "mage");
				M2D.prepareSvTexture("damage_horse", false);
				M2D.prepareSvTexture("damage_romero", false);
			}
			else if (key.IndexOf("SNAKE_") == 0)
			{
				M2D.loadMaterialSnd("enemy_snake");
				EnemyData.loadPxl(M2D, "snake");
				M2D.prepareSvTexture("damage_horse", false);
			}
			else if (key.IndexOf("SPONGE_") == 0)
			{
				M2D.loadMaterialSnd("enemy_sponge");
				EnemyData.loadPxl(M2D, "sponge");
			}
			else if (key.IndexOf("UNI_") == 0)
			{
				M2D.loadMaterialSnd("enemy_uni");
				EnemyData.loadPxl(M2D, "uni");
			}
			else if (key.IndexOf("FOX_") == 0)
			{
				M2D.loadMaterialSnd("enemy_fox");
				EnemyData.loadPxl(M2D, "fox");
				M2D.prepareSvTexture("damage_ketsudasi", false);
			}
			else if (key.IndexOf("GECKO_") == 0)
			{
				M2D.loadMaterialSnd("enemy_gecko");
				EnemyData.loadPxl(M2D, "gecko");
			}
			else if (key.IndexOf("FROG_") == 0)
			{
				M2D.loadMaterialSnd("enemy_frog");
				EnemyData.loadPxl(M2D, "frog");
				M2D.prepareSvTexture("damage_swallowed", false);
				M2D.prepareSvTexture("damage_shrimp", false);
			}
			else if (key.IndexOf("BOSS_NUSI_") == 0)
			{
				M2D.loadMaterialSnd("enemy_nusi");
				M2D.loadMaterialSnd("enemy_boss");
				EnemyData.loadPxl(M2D, "boss_nusi");
				M2D.prepareSvTexture("damage_frog", false);
				M2D.prepareSvTexture("damage_worm", false);
				M2D.loadMaterialMTIOneImage("SpineAnimEn/boss_nusi__enemy_boss_nusi", "enemy_boss_nusi");
				M2D.loadMaterialMTISpine("SpineAnimEn/boss_nusi__enemy_boss_nusi.atlas", "enemy_boss_nusi");
			}
			else
			{
				if (!(key == "MGMFARM_COW_NPC"))
				{
					return false;
				}
				M2D.loadMaterialSnd("ev_cow");
			}
			return true;
		}

		public static EnemyData.EnemyDescryption getTypeAndId(string key)
		{
			if (TX.isStart(key, "SLIME_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 256 + X.NmI(TX.slice(key, "SLIME_".Length), 0, false, false),
					EnemyType = typeof(NelNSlime),
					overdriveable = true
				};
			}
			if (TX.isStart(key, "MUSH_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 512 + X.NmI(TX.slice(key, "MUSH_".Length), 0, false, false),
					EnemyType = typeof(NelNMush),
					overdriveable = true
				};
			}
			if (TX.isStart(key, "PUPPY_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 768 + X.NmI(TX.slice(key, "PUPPY_".Length), 0, false, false),
					EnemyType = typeof(NelNPuppy),
					FnDangerous = new Func<bool>(EnemyData.isDangerous_NoFireBall)
				};
			}
			ENEMYID enemyid;
			if (TX.isStart(key, "GOLEM_", 0) && FEnum<ENEMYID>.TryParse(key, out enemyid, true))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 1024 + X.NmI(TX.slice(key, "GOLEM_".Length), 0, false, false),
					EnemyType = typeof(NelNGolem),
					overdriveable = (enemyid < ENEMYID.GOLEM_0_NM)
				};
			}
			ENEMYID enemyid2;
			if (TX.isStart(key, "GOLEMTOY_", 0) && FEnum<ENEMYID>.TryParse(key, out enemyid2, true))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 4608 + X.NmI(TX.slice(key, "GOLEMTOY_".Length), 0, false, false),
					EnemyType = NelNGolemToy.GetType(enemyid2)
				};
			}
			if (TX.isStart(key, "MAGE_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 4096 + X.NmI(TX.slice(key, "MAGE_".Length), 0, false, false),
					EnemyType = typeof(NelNMage),
					FnDangerous = new Func<bool>(EnemyData.isDangerous_NoShield)
				};
			}
			if (TX.isStart(key, "SNAKE_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 1280 + X.NmI(TX.slice(key, "SNAKE_".Length), 0, false, false),
					EnemyType = typeof(NelNSnake),
					FnDangerous = new Func<bool>(EnemyData.isDangerous_NoDropBomb)
				};
			}
			if (TX.isStart(key, "SPONGE_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 1536 + X.NmI(TX.slice(key, "SPONGE_".Length), 0, false, false),
					EnemyType = typeof(NelNSponge)
				};
			}
			if (TX.isStart(key, "UNI_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 1792 + X.NmI(TX.slice(key, "UNI_".Length), 0, false, false),
					EnemyType = typeof(NelNUni),
					overdriveable = true
				};
			}
			if (TX.isStart(key, "FOX_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 4352 + X.NmI(TX.slice(key, "FOX_".Length), 0, false, false),
					EnemyType = typeof(NelNFox)
				};
			}
			if (TX.isStart(key, "GECKO_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 4864 + X.NmI(TX.slice(key, "GECKO_".Length), 0, false, false),
					EnemyType = typeof(NelNGecko)
				};
			}
			if (TX.isStart(key, "FROG_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 5120 + X.NmI(TX.slice(key, "FROG_".Length), 0, false, false),
					EnemyType = typeof(NelNFrog)
				};
			}
			if (TX.isStart(key, "BOSS_NUSI_CAGE", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 524289 + X.NmI(TX.slice(key, "BOSS_NUSI_CAGE".Length), 0, false, false),
					EnemyType = typeof(NelNNusiCage)
				};
			}
			if (TX.isStart(key, "BOSS_NUSI_TENTACLE", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 524290 + X.NmI(TX.slice(key, "BOSS_NUSI_TENTACLE".Length), 0, false, false),
					EnemyType = typeof(NelNNusiTentacle)
				};
			}
			if (TX.isStart(key, "BOSS_NUSI_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 524288 + X.NmI(TX.slice(key, "BOSS_NUSI_".Length), 0, false, false),
					EnemyType = typeof(NelNBoss_Nusi),
					FnDangerous = new Func<bool>(EnemyData.isDangerous_NoBurst)
				};
			}
			if (TX.isStart(key, "MECHGOLEM_", 0))
			{
				return new EnemyData.EnemyDescryption
				{
					id = 393216 + X.NmI(TX.slice(key, "MECHGOLEM_".Length), 0, false, false),
					EnemyType = typeof(NelNMechGolem)
				};
			}
			return null;
		}

		public static void SummonerOpened(string key)
		{
		}

		public static NelEnemy createByKey(Map2d Mp, string key, string gob_key)
		{
			GameObject gameObject = ((Mp == null) ? null : Mp.createMoverGob<NelEnemy>(gob_key, (float)(Mp.clms / 2), (float)(Mp.rows / 2), false));
			NelEnemy nelEnemy = null;
			EnemyData.EnemyDescryption typeAndId = EnemyData.getTypeAndId(key);
			if (typeAndId == null)
			{
				X.de("ENEMYID指定でエラー発生: " + key, null);
				return null;
			}
			try
			{
				ENEMYID id = (ENEMYID)typeAndId.id;
				nelEnemy = gameObject.AddComponent(typeAndId.EnemyType) as NelEnemy;
				nelEnemy.id = id & (ENEMYID)2147483647U;
			}
			catch
			{
				X.de("ENEMYID指定でエラー発生: " + key, null);
				return null;
			}
			return nelEnemy;
		}

		public static string getEnemyName(ENEMYID eid)
		{
			bool flag = false;
			if ((eid & (ENEMYID)2147483648U) != (ENEMYID)0U)
			{
				eid &= (ENEMYID)2147483647U;
				flag = true;
			}
			string text = eid.ToString().ToUpper();
			string text2 = "???";
			TX tx = TX.getTX("Enemy_" + text, true, true, null);
			if (tx == null && REG.match(text, REG.RegSuffixNumber))
			{
				text = REG.leftContext;
				tx = TX.getTX("Enemy_" + text, true, true, null);
			}
			if (tx != null)
			{
				text2 = tx.text;
			}
			return text2 + (flag ? TX.Get("Summoner__enemy_id_overdrived_suffix", "") : "");
		}

		public static bool isSame(ENEMYID eid, string enemy_id_key_uppered)
		{
			string text = FEnum<ENEMYID>.ToStr(eid);
			return text == enemy_id_key_uppered || text.IndexOf(enemy_id_key_uppered + "_") == 0;
		}

		public static bool isSame(string eid, string enemy_id_key, bool uppered = false)
		{
			string text = (uppered ? eid : eid.ToUpper());
			enemy_id_key = (uppered ? enemy_id_key : enemy_id_key.ToUpper());
			return text == enemy_id_key || text.IndexOf(enemy_id_key + "_") == 0;
		}

		public static void killEnemy(NelEnemy N, NelAttackInfo Atk)
		{
			if (Atk != null)
			{
				MagicItem publishMagic = Atk.PublishMagic;
			}
			int num = (int)X.Mn(N.get_mp(), X.NI(N.drop_mp_min, N.drop_mp_max, N.mp_ratio));
			if (N.isOverDrive())
			{
				OverDriveManager odManager = N.getOdManager();
				num += ((odManager != null) ? odManager.od_killed_mana_splash : 0);
			}
			MANA_HIT mana_HIT;
			if (Atk != null && Atk.Caster is NelEnemy)
			{
				mana_HIT = (MANA_HIT)8194;
				num += 2;
			}
			else
			{
				mana_HIT = (MANA_HIT)4169;
			}
			N.nM2D.Mana.AddMulti(N.x, N.y, (float)num, mana_HIT | (N.nM2D.NightCon.hasMistWeather() ? MANA_HIT.EN : MANA_HIT.NOUSE));
		}

		private static bool isDangerous_NoDropBomb()
		{
			return !MagicSelector.isObtained(MGKIND.DROPBOMB);
		}

		private static bool isDangerous_NoFireBall()
		{
			return !MagicSelector.isObtained(MGKIND.FIREBALL);
		}

		private static bool isDangerous_NoBurst()
		{
			return !MagicSelector.isObtained(MGKIND.PR_BURST);
		}

		private static bool isDangerous_NoShield()
		{
			return !SkillManager.isObtained("guard");
		}

		private static BDic<string, string> Oscript;

		private static BDic<string, FlagCounter<SER>> OSerResist;

		private const string def_resist_key = "_DEFAULT_RESIST";

		private const string enemy_pxl_dir = "Enemies";

		public const string enemy_pxl_header = "N_";

		public class EnemyDescryption
		{
			public int id;

			public Type EnemyType;

			public bool overdriveable;

			public Func<bool> FnDangerous;
		}
	}
}

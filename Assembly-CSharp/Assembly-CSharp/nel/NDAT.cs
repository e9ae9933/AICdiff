using System;
using Better;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public static class NDAT
	{
		public static bool isMechGolem(ENEMYID id)
		{
			return X.BTW(393216f, (float)id, 393472f);
		}

		public static void prepareData()
		{
			NDAT.Oscript = new BDic<string, string>();
			NDAT.OASerResist = new BDic<string, M2SerResist[]>();
			NDAT.prepareSerDataScript();
			NOD.reloadNODScript();
		}

		private static void prepareSerDataScript()
		{
			using (MTI mti = new MTI("Enemies/_ser_resist", "_"))
			{
				CsvReaderA csvReaderA = new CsvReaderA(mti.LoadText("_ser_resist", null, true), false);
				M2SerResist m2SerResist = new M2SerResist(4);
				string[] array = null;
				string[] array2 = null;
				M2SerResist m2SerResist2 = null;
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
								int num2 = 38;
								for (int i = 0; i < num2; i++)
								{
									SER ser = (SER)((long)i);
									m2SerResist.Add(ser, (float)num);
								}
							}
							else if (FEnum<SER>.TryParse(csvReaderA.cmd.ToUpper(), out ser2, true))
							{
								int num3 = csvReaderA.Int(1, 50);
								m2SerResist.Rem(ser2);
								m2SerResist.Add(ser2, (float)num3);
							}
							else
							{
								csvReaderA.tError("不明なSER: " + csvReaderA.cmd);
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
								M2SerResist[] array3;
								M2SerResist m2SerResist3;
								if (NDAT.OASerResist.TryGetValue(text, out array3))
								{
									m2SerResist3 = array3[0];
								}
								else
								{
									array3 = (NDAT.OASerResist[text] = new M2SerResist[6]);
									m2SerResist3 = (array3[0] = new M2SerResist(4));
									if (text == "_DEFAULT_RESIST")
									{
										m2SerResist2 = m2SerResist3;
									}
									if (m2SerResist2 != null && m2SerResist2 != m2SerResist3)
									{
										m2SerResist3.Add(m2SerResist2, 1f);
									}
								}
								m2SerResist3.Replace(m2SerResist, 1f);
							}
						}
						if (csvReaderA.isEnd())
						{
							break;
						}
						m2SerResist.Clear();
						array2 = array;
					}
				}
			}
		}

		public static string getData(string key)
		{
			string text = "";
			string text2;
			if (NDAT.Oscript.TryGetValue(key, out text2))
			{
				text = text2 + "\n" + text;
			}
			if (REG.match(key, REG.RegSuffixNumber))
			{
				key = REG.leftContext;
				if (NDAT.Oscript.TryGetValue(key, out text2))
				{
					text = text2 + "\n" + text;
				}
			}
			return text;
		}

		public static M2SerResist getSerRegist(string key, ENATTR nattr, M2SerResist Default = null, bool check_suffix = true)
		{
			M2SerResist[] array = null;
			if (!NDAT.OASerResist.TryGetValue(key, out array) && check_suffix && REG.match(key, REG.RegSuffixNumber))
			{
				key = REG.leftContext;
				if (!NDAT.OASerResist.TryGetValue(key, out array))
				{
					return Default ?? NDAT.getSerRegistDefault(nattr);
				}
			}
			if (array == null)
			{
				return Default ?? NDAT.getSerRegistDefault(nattr);
			}
			int num = EnemyAttr.mattrIndex(nattr) + 1;
			M2SerResist m2SerResist = array[num];
			if (m2SerResist == null)
			{
				m2SerResist = (array[num] = new M2SerResist(array[0]));
				EnemyAttr.createSer(nattr, 255, m2SerResist);
			}
			return m2SerResist;
		}

		public static M2SerResist getSerRegistDefault(ENATTR nattr)
		{
			M2SerResist[] array = X.Get<string, M2SerResist[]>(NDAT.OASerResist, "_DEFAULT_RESIST");
			if (array == null)
			{
				return null;
			}
			int num = EnemyAttr.mattrIndex(nattr) + 1;
			M2SerResist m2SerResist = array[num];
			if (m2SerResist == null)
			{
				m2SerResist = (array[num] = new M2SerResist(array[0]));
				EnemyAttr.createSer(nattr, 255, m2SerResist);
			}
			return m2SerResist;
		}

		public static bool loadPxl(NelM2DBase M2D, string key, string key_with_header = null, bool external_async = false)
		{
			string text = "Enemies/" + key + ".pxls";
			string text2 = key_with_header ?? ("N_" + key);
			PxlCharacter pxlCharacter;
			return M2D.loadMaterialPxl(text2, text, out pxlCharacter, true, true, external_async);
		}

		public static bool getResources(NelM2DBase M2D, string key)
		{
			if (key.IndexOf("SLIME_") == 0)
			{
				M2D.loadMaterialSnd("enemy_slime");
				NDAT.loadPxl(M2D, "slime", null, false);
				M2D.prepareSvTexture("torture_slime_1", false);
			}
			else if (key.IndexOf("MUSH_") == 0)
			{
				M2D.loadMaterialSnd("enemy_mush");
				NDAT.loadPxl(M2D, "mush", null, false);
				M2D.prepareSvTexture("damage_shrimp", false);
				M2D.prepareSvTexture("stand_weak", false);
			}
			else if (key.IndexOf("PUPPY_") == 0)
			{
				M2D.loadMaterialSnd("enemy_puppy");
				NDAT.loadPxl(M2D, "puppy", null, false);
				M2D.prepareSvTexture("damage_frog", false);
			}
			else if (key.IndexOf("GOLEM_") == 0)
			{
				M2D.loadMaterialSnd("enemy_golem");
				NDAT.loadPxl(M2D, "golem", null, false);
				NDAT.getResources(M2D, "GOLEMTOY_0");
			}
			else if (key.IndexOf("GOLEMTOY_") == 0)
			{
				M2D.loadMaterialSnd("enemy_golem");
				NDAT.loadPxl(M2D, "golemtoy", null, false);
				M2D.prepareSvTexture("damage_horse", false);
				M2D.prepareSvTexture("damage_mkb", false);
				M2D.prepareSvTexture("damage_behind", false);
			}
			else if (key.IndexOf("MECHGOLEM_") == 0)
			{
				M2D.loadMaterialSnd("enemy_mgolem");
				M2D.loadMaterialSnd("enemy_golem");
				NDAT.loadPxl(M2D, "mechgolem", null, false);
			}
			else if (key.IndexOf("MAGE_") == 0)
			{
				NDAT.loadPxl(M2D, "mage", null, false);
				M2D.prepareSvTexture("damage_horse", false);
				M2D.prepareSvTexture("damage_romero", false);
			}
			else if (key.IndexOf("SNAKE_") == 0)
			{
				M2D.loadMaterialSnd("enemy_snake");
				NDAT.loadPxl(M2D, "snake", null, false);
				M2D.prepareSvTexture("damage_horse", false);
			}
			else if (key.IndexOf("SPONGE_") == 0)
			{
				M2D.loadMaterialSnd("enemy_sponge");
				NDAT.loadPxl(M2D, "sponge", null, false);
			}
			else if (key.IndexOf("UNI_") == 0)
			{
				M2D.loadMaterialSnd("enemy_uni");
				NDAT.loadPxl(M2D, "uni", null, false);
			}
			else if (key.IndexOf("FOX_") == 0)
			{
				M2D.loadMaterialSnd("enemy_fox");
				NDAT.loadPxl(M2D, "fox", null, false);
				M2D.prepareSvTexture("damage_ketsudasi", false);
			}
			else if (key.IndexOf("GECKO_") == 0)
			{
				M2D.loadMaterialSnd("enemy_gecko");
				NDAT.loadPxl(M2D, "gecko", null, false);
			}
			else if (key.IndexOf("FROG_") == 0)
			{
				M2D.loadMaterialSnd("enemy_frog");
				NDAT.loadPxl(M2D, "frog", null, false);
				M2D.prepareSvTexture("damage_swallowed", false);
				M2D.prepareSvTexture("damage_shrimp", false);
			}
			else if (key.IndexOf("PENTAPOD_") == 0)
			{
				M2D.loadMaterialSnd("enemy_pentapod");
				NDAT.loadPxl(M2D, "pentapod", null, false);
				M2D.prepareSvTexture("damage_backinj", false);
			}
			else if (key.IndexOf("PIG_") == 0)
			{
				M2D.prepareSvTexture("damage_ketsudasi", false);
				M2D.loadMaterialSnd("enemy_pig");
				NDAT.loadPxl(M2D, "pig", null, false);
			}
			else if (key.IndexOf("ROAPER_") == 0)
			{
				M2D.prepareSvTexture("damage_horse", false);
				M2D.prepareSvTexture("damage_smt", false);
				M2D.loadMaterialSnd("enemy_roaper");
				M2D.loadMaterialSnd("enemy_puppy");
				NDAT.loadPxl(M2D, "roaper", null, false);
			}
			else if (key.IndexOf("BOSS_NUSI_") == 0)
			{
				M2D.loadMaterialSnd("enemy_nusi");
				M2D.loadMaterialSnd("enemy_boss");
				NDAT.loadPxl(M2D, "boss_nusi", null, false);
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

		public static NDAT.EnemyDescryption getTypeAndId(string key)
		{
			NDAT.EnemyDescryption typeAndId = NDAT.getTypeAndId0(key);
			if (typeAndId.id > 0)
			{
				typeAndId.overdriveable = NDAT.isOverdriveable((ENEMYID)typeAndId.id, true);
			}
			return typeAndId;
		}

		public static NDAT.EnemyDescryption getTypeAndId(ENEMYID _id, out bool is_od)
		{
			is_od = (_id & (ENEMYID)2147483648U) > (ENEMYID)0U;
			return NDAT.getTypeAndId0(FEnum<ENEMYID>.ToStr(_id & (ENEMYID)2147483647U));
		}

		public static bool typeIs(ENEMYID id, ENEMYID base_id)
		{
			return X.BTW(base_id, id, base_id + 256U);
		}

		public static bool isOverdriveable(ENEMYID id, bool for_thunderbolt = false)
		{
			if (NDAT.typeIs(id, ENEMYID.SLIME_0))
			{
				return true;
			}
			if (NDAT.typeIs(id, ENEMYID.MUSH_0))
			{
				return true;
			}
			if (NDAT.typeIs(id, ENEMYID.GOLEM_0))
			{
				return id < ENEMYID.GOLEM_0_NM;
			}
			return NDAT.typeIs(id, ENEMYID.UNI_0) || (NDAT.typeIs(id, ENEMYID.MECHGOLEM_0) && !for_thunderbolt);
		}

		private static NDAT.EnemyDescryption getTypeAndId0(string key)
		{
			if (TX.isStart(key, "SLIME_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 256 + X.NmI(TX.slice(key, "SLIME_".Length), 0, false, false),
					EnemyType = typeof(NelNSlime)
				};
			}
			if (TX.isStart(key, "MUSH_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 512 + X.NmI(TX.slice(key, "MUSH_".Length), 0, false, false),
					EnemyType = typeof(NelNMush)
				};
			}
			if (TX.isStart(key, "PUPPY_EVENT_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 1008 + X.NmI(TX.slice(key, "PUPPY_EVENT_".Length), 0, false, false),
					EnemyType = typeof(NelNPuppyEvent)
				};
			}
			if (TX.isStart(key, "PUPPY_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 768 + X.NmI(TX.slice(key, "PUPPY_".Length), 0, false, false),
					EnemyType = typeof(NelNPuppy),
					FnDangerous = new Func<bool>(NDAT.isDangerous_NoFireBall)
				};
			}
			ENEMYID enemyid;
			if (TX.isStart(key, "GOLEM_", 0) && FEnum<ENEMYID>.TryParse(key, out enemyid, true))
			{
				return new NDAT.EnemyDescryption
				{
					id = 1024 + X.NmI(TX.slice(key, "GOLEM_".Length), 0, false, false),
					EnemyType = typeof(NelNGolem)
				};
			}
			ENEMYID enemyid2;
			if (TX.isStart(key, "GOLEMTOY_", 0) && FEnum<ENEMYID>.TryParse(key, out enemyid2, true))
			{
				return new NDAT.EnemyDescryption
				{
					id = (int)enemyid2,
					EnemyType = NelNGolemToy.GetType(enemyid2),
					nattr_decline = ENATTR.MP_STABLE
				};
			}
			if (TX.isStart(key, "MAGE_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 4096 + X.NmI(TX.slice(key, "MAGE_".Length), 0, false, false),
					EnemyType = typeof(NelNMage),
					FnDangerous = new Func<bool>(NDAT.isDangerous_NoShield),
					nattr_decline = ENATTR._MATTR,
					nattr_addable_od = 22
				};
			}
			if (TX.isStart(key, "SNAKE_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 1280 + X.NmI(TX.slice(key, "SNAKE_".Length), 0, false, false),
					EnemyType = typeof(NelNSnake),
					FnDangerous = new Func<bool>(NDAT.isDangerous_NoDropBomb)
				};
			}
			if (TX.isStart(key, "SPONGE_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 1536 + X.NmI(TX.slice(key, "SPONGE_".Length), 0, false, false),
					EnemyType = typeof(NelNSponge),
					nattr_addable = 32
				};
			}
			if (TX.isStart(key, "UNI_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 1792 + X.NmI(TX.slice(key, "UNI_".Length), 0, false, false),
					EnemyType = typeof(NelNUni),
					nattr_addable_od = 22
				};
			}
			if (TX.isStart(key, "FOX_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 4352 + X.NmI(TX.slice(key, "FOX_".Length), 0, false, false),
					EnemyType = typeof(NelNFox),
					nattr_addable = 40,
					nattr_decline = (ENATTR.ICE | ENATTR.THUNDER),
					no_replacable_by_quest = true
				};
			}
			if (TX.isStart(key, "GECKO_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 4864 + X.NmI(TX.slice(key, "GECKO_".Length), 0, false, false),
					EnemyType = typeof(NelNGecko)
				};
			}
			if (TX.isStart(key, "FROG_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 5120 + X.NmI(TX.slice(key, "FROG_".Length), 0, false, false),
					EnemyType = typeof(NelNFrog),
					nattr_addable = 40,
					nattr_decline = (ENATTR.FIRE | ENATTR.THUNDER),
					no_replacable_by_quest = true
				};
			}
			if (TX.isStart(key, "BOSS_NUSI_CAGE", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 524289 + X.NmI(TX.slice(key, "BOSS_NUSI_CAGE".Length), 0, false, false),
					EnemyType = typeof(NelNNusiCage)
				};
			}
			if (TX.isStart(key, "BOSS_NUSI_TENTACLE", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 524290 + X.NmI(TX.slice(key, "BOSS_NUSI_TENTACLE".Length), 0, false, false),
					EnemyType = typeof(NelNNusiTentacle)
				};
			}
			if (TX.isStart(key, "BOSS_NUSI_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 524288 + X.NmI(TX.slice(key, "BOSS_NUSI_".Length), 0, false, false),
					EnemyType = typeof(NelNBoss_Nusi),
					FnDangerous = new Func<bool>(NDAT.isDangerous_NoBurst),
					nattr_addable = 40
				};
			}
			if (TX.isStart(key, "MECHGOLEM_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 393216 + X.NmI(TX.slice(key, "MECHGOLEM_".Length), 0, false, false),
					EnemyType = typeof(NelNMechGolem),
					nattr_addable_od = 64,
					no_replacable_by_quest = true
				};
			}
			if (TX.isStart(key, "PENTAPOD_HEAD_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 5632 + X.NmI(TX.slice(key, "PENTAPOD_HEAD_".Length), 0, false, false),
					EnemyType = typeof(NelNPentapodHead)
				};
			}
			if (TX.isStart(key, "PENTAPOD_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 5376 + X.NmI(TX.slice(key, "PENTAPOD_".Length), 0, false, false),
					EnemyType = typeof(NelNPentapod)
				};
			}
			if (TX.isStart(key, "PIG_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 5888 + X.NmI(TX.slice(key, "PIG_".Length), 0, false, false),
					EnemyType = typeof(NelNPig)
				};
			}
			if (TX.isStart(key, "ROAPER_", 0))
			{
				return new NDAT.EnemyDescryption
				{
					id = 6144 + X.NmI(TX.slice(key, "ROAPER_".Length), 0, false, false),
					EnemyType = typeof(NelNRoaper)
				};
			}
			return default(NDAT.EnemyDescryption);
		}

		public static void SummonerOpened(string key)
		{
		}

		public static NelEnemy createByKey(Map2d Mp, string key, string gob_key)
		{
			GameObject gameObject = ((Mp == null) ? null : Mp.createMoverGob<NelEnemy>(gob_key, (float)(Mp.clms / 2), (float)(Mp.rows / 2), false));
			NelEnemy nelEnemy = null;
			NDAT.EnemyDescryption typeAndId = NDAT.getTypeAndId(key);
			if (!typeAndId.valid)
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

		public static string getEnemyName(ENEMYID eid, bool remove_suffix = true)
		{
			bool flag = false;
			if ((eid & (ENEMYID)2147483648U) != (ENEMYID)0U)
			{
				eid &= (ENEMYID)2147483647U;
				flag = true;
			}
			if (eid <= (ENEMYID)0U)
			{
				return "???";
			}
			string text;
			if (eid == ENEMYID.GOLEMTOY_MKB)
			{
				text = "GOLEMTOY_MKB";
			}
			else
			{
				text = eid.ToString().ToUpper();
			}
			string text2 = text;
			string text3 = "???";
			TX tx = TX.getTX("Enemy_" + text2, true, true, null);
			if (tx == null)
			{
				if (remove_suffix && REG.match(text2, REG.RegSuffixAlphabet))
				{
					text2 = REG.leftContext;
					tx = TX.getTX("Enemy_" + text2, true, true, null);
				}
				if (tx == null && REG.match(text2, REG.RegSuffixNumber))
				{
					text2 = REG.leftContext;
					tx = TX.getTX("Enemy_" + text2, true, true, null);
				}
			}
			if (tx != null)
			{
				text3 = tx.text;
			}
			return text3 + (flag ? TX.Get("Summoner__enemy_id_overdrived_suffix", "") : "");
		}

		public static string getEnemyKindName(ENEMYKIND k)
		{
			if (k == ENEMYKIND.MACHINE)
			{
				return TX.Get("EnemyKind_machine", "");
			}
			return TX.Get("EnemyKind_devil", "");
		}

		public static bool isSame(ENEMYID eid, string enemy_id_key_uppered)
		{
			string text = FEnum<ENEMYID>.ToStr(eid);
			return text == enemy_id_key_uppered || TX.headerIs(text, enemy_id_key_uppered, 0, '_', true);
		}

		public static bool isSame(string eid, string enemy_id_key, bool uppered = false)
		{
			string text = (uppered ? eid : eid.ToUpper());
			enemy_id_key = (uppered ? enemy_id_key : enemy_id_key.ToUpper());
			return text == enemy_id_key || TX.headerIs(text, enemy_id_key, 0, '_', true);
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
			if (N.nattr_mp_stable)
			{
				num = X.IntC((float)num * 2f);
			}
			MANA_HIT mana_HIT;
			if (Atk != null && Atk.Caster is NelEnemy)
			{
				mana_HIT = MANA_HIT.EN | MANA_HIT.FROM_SUPPLIER;
				num += 2;
			}
			else
			{
				mana_HIT = MANA_HIT.PR | MANA_HIT.FALL | MANA_HIT.CRYSTAL | MANA_HIT.SPECIAL;
			}
			N.nM2D.Mana.AddMulti(N.x, N.y, (float)num, mana_HIT | (N.nM2D.NightCon.hasMistWeather() ? MANA_HIT.EN : MANA_HIT.NOUSE), 1f);
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

		private static BDic<string, M2SerResist[]> OASerResist;

		private const string def_resist_key = "_DEFAULT_RESIST";

		private const string enemy_pxl_dir = "Enemies";

		public const string enemy_pxl_header = "N_";

		public struct EnemyDescryption
		{
			public bool valid
			{
				get
				{
					return this.EnemyType != null;
				}
			}

			public int id;

			public Type EnemyType;

			public bool overdriveable;

			public Func<bool> FnDangerous;

			public ENATTR nattr_decline;

			public byte nattr_addable;

			public byte nattr_addable_od;

			public bool no_replacable_by_quest;
		}
	}
}

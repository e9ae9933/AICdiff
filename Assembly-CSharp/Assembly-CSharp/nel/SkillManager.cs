using System;
using System.Collections.Generic;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public static class SkillManager
	{
		public static void initScript()
		{
			SkillManager.PPose = PxlsLoader.getPxlCharacter("_icons_l").getPoseByName("nel_skill");
			SkillManager.OSk = new NDic<PrSkill>("PrSkill", 0);
			CsvReaderA csvReaderA = new CsvReaderA(TX.getResource("Data/skill", ".csv", false), true);
			PrSkill prSkill = null;
			NelItem nelItem = null;
			ushort num = 1;
			int num2 = 61200;
			while (csvReaderA.read())
			{
				if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					if (prSkill != null && SkillManager.GetById(prSkill.id, prSkill) != null)
					{
						global::XX.X.de(string.Concat(new string[]
						{
							"重複ID: ",
							prSkill.id.ToString(),
							" (key: ",
							prSkill.key,
							")"
						}), null);
					}
					string index = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
					prSkill = SkillManager.Get(index);
					nelItem = null;
					if (prSkill != null)
					{
						global::XX.X.de("重複キー: " + index, null);
						prSkill = null;
					}
					else
					{
						prSkill = new PrSkill(index, num += 1);
						nelItem = NelItem.CreateItemEntry("skillbook_" + index, new NelItem("skillbook_" + index, 0, 300, 1)
						{
							category = (NelItem.CATEG)2097153U,
							FnGetName = new FnGetItemDetail(NelItem.fnGetNameSkillBook),
							FnGetDesc = new FnGetItemDetail(NelItem.fnGetDescSkillBook),
							FnGetDetail = new FnGetItemDetail(NelItem.fnGetDetailSkillBook),
							specific_icon_id = (TX.isStart(index, "sp_map_", 0) ? 35 : 18),
							SpecificColor = C32.d2c(4294914161U)
						}, num2++, false);
						nelItem.value = (float)prSkill.id;
						SkillManager.OSk[index] = prSkill;
					}
				}
				if (prSkill != null)
				{
					string cmd = csvReaderA.cmd;
					if (cmd != null)
					{
						uint num3 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
						if (num3 <= 2155022221U)
						{
							if (num3 != 515751117U)
							{
								if (num3 != 1786917688U)
								{
									if (num3 == 2155022221U)
									{
										if (cmd == "%ALWAYS")
										{
											prSkill.always_enable = true;
										}
									}
								}
								else if (cmd == "%CATEGORY")
								{
									for (int i = 1; i < csvReaderA.clength; i++)
									{
										SkillManager.SKILL_CTG skill_CTG;
										if (!FEnum<SkillManager.SKILL_CTG>.TryParse(csvReaderA.getIndex(i), out skill_CTG, true))
										{
											csvReaderA.de("不明なカテゴリー: " + csvReaderA.getIndex(i));
										}
										else
										{
											prSkill.category |= skill_CTG;
										}
									}
								}
							}
							else if (cmd == "%PRICE")
							{
								nelItem.price = csvReaderA.Int(1, nelItem.price);
							}
						}
						else if (num3 <= 3240287073U)
						{
							if (num3 != 2277141021U)
							{
								if (num3 == 3240287073U)
								{
									if (cmd == "%DESC_KEY")
									{
										prSkill.desc_key_replace = (TX.valid(csvReaderA._1) ? csvReaderA._1 : null);
									}
								}
							}
							else if (cmd == "%ID")
							{
								ushort num4 = (ushort)csvReaderA.Int(1, (int)num);
								if (num4 == 0)
								{
									csvReaderA.tError("ID を 0 に設定することはできない");
								}
								else
								{
									num = (prSkill.id = num4);
								}
							}
						}
						else if (num3 != 3349628709U)
						{
							if (num3 == 3472574318U)
							{
								if (cmd == "%FIRST")
								{
									prSkill.first_visible = true;
								}
							}
						}
						else if (cmd == "%AT")
						{
							prSkill.map_at = csvReaderA._1;
						}
					}
				}
			}
			SkillManager.OSk.scriptFinalize();
			if (prSkill != null && SkillManager.GetById(prSkill.id, prSkill) != null)
			{
				global::XX.X.de(string.Concat(new string[]
				{
					"重複ID: ",
					prSkill.id.ToString(),
					" (key: ",
					prSkill.key,
					")"
				}), null);
			}
			if (SkillManager.VALIDATION)
			{
				SkillManager.Validate();
			}
		}

		public static void newGame()
		{
			foreach (KeyValuePair<string, PrSkill> keyValuePair in SkillManager.OSk)
			{
				PrSkill value = keyValuePair.Value;
				value.enabled = (value.visible = false);
				value.new_icon = true;
				if (value.first_visible)
				{
					value.visible = (value.enabled = true);
					value.new_icon = false;
				}
			}
			UiSkillManageBox.newGame();
		}

		public static PrSkill Get(string key)
		{
			if (SkillManager.OSk == null)
			{
				return null;
			}
			return SkillManager.OSk.Get(key);
		}

		public static PrSkill Get(NelItem Itm)
		{
			if (Itm != null)
			{
				return SkillManager.Get(TX.slice(Itm.key, "skillbook_".Length));
			}
			return null;
		}

		public static PrSkill GetById(ushort id, PrSkill Except = null)
		{
			if (SkillManager.OSk == null)
			{
				return null;
			}
			foreach (KeyValuePair<string, PrSkill> keyValuePair in SkillManager.OSk)
			{
				PrSkill value = keyValuePair.Value;
				if (value.id == id && Except != value)
				{
					return value;
				}
			}
			return null;
		}

		public static bool isObtained(string key)
		{
			PrSkill prSkill = SkillManager.Get(key);
			return prSkill != null && (prSkill.visible || prSkill.first_visible);
		}

		public static bool isEnabled(string key)
		{
			PrSkill prSkill = SkillManager.Get(key);
			return prSkill != null && (prSkill.visible || prSkill.first_visible || global::XX.X.DEBUGALLSKILL) && prSkill.enabled;
		}

		public static void Validate()
		{
		}

		public static bool use_haniwa
		{
			get
			{
				return false;
			}
		}

		public static NDic<PrSkill> getSkillDictionary()
		{
			return SkillManager.OSk;
		}

		public static void readBinaryFrom(ByteArray Ba, int svd_ver)
		{
			SkillManager.newGame();
			for (;;)
			{
				ushort num = Ba.readUShort();
				if (num == 0)
				{
					break;
				}
				int num2 = (int)Ba.readUByte();
				PrSkill byId = SkillManager.GetById(num, null);
				if (byId != null)
				{
					byId.visible = true;
					byId.enabled = (num2 & 2) != 0;
					byId.new_icon = (num2 & 1) != 0;
					if (byId.key == "sp_difficulty0")
					{
						byId.visible = false;
						if (byId.enabled)
						{
							DIFF.I = 0;
						}
						byId.enabled = false;
					}
				}
			}
			UiSkillManageBox.readBinaryFrom(Ba);
		}

		public static void writeBinaryTo(ByteArray Ba)
		{
			foreach (KeyValuePair<string, PrSkill> keyValuePair in SkillManager.OSk)
			{
				PrSkill value = keyValuePair.Value;
				if (value.first_visible || value.visible)
				{
					Ba.writeUShort(value.id);
					Ba.writeByte((value.new_icon ? 1 : 0) | (value.enabled ? 2 : 0));
				}
			}
			Ba.writeUShort(0);
			UiSkillManageBox.writeBinaryTo(Ba);
		}

		public static bool VALIDATION;

		private static NDic<PrSkill> OSk;

		private const string skill_script = "Data/skill";

		public const string skillheader_map = "sp_map_";

		public const string skillbook_item_header = "skillbook_";

		public static PxlPose PPose;

		public static int gotten;

		public const int map_icon_id = 35;

		public const int TREASURE_BOX_VISIBLE_SKILL_MONEY = 150;

		public enum SKILL_CTG
		{
			PUNCH = 1,
			MAGIC,
			GUARD = 4,
			HP = 8,
			MP = 16,
			HPMP = 24,
			_MAX = 5,
			SPECIAL = 32
		}

		public enum SKILL_TYPE : ulong
		{
			punch,
			shotgun,
			sliding,
			wheel,
			comet,
			guard,
			evade,
			guard_bush,
			guard_lariat,
			evade_dancing,
			burst,
			sp_difficulty0,
			dashpunch,
			airpunch,
			shield_counter
		}
	}
}

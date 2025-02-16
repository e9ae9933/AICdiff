using System;
using System.Collections.Generic;
using m2d;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public sealed class EpSituation
	{
		public EpSituation()
		{
			this.ALastSituEn = new List<EpSituation.SituDesc>(4);
			this.Alast_situation_desc = new List<string>(1);
		}

		public void newGame()
		{
			this.FlgLockWrite.Clear();
			this.clearManual(true);
			this.Alast_situation_desc.Add("&&GM_ep_situation_default");
		}

		public void clearManual(bool force = false)
		{
			if (!force && this.FlgLockWrite.isActive())
			{
				return;
			}
			this.ALastSituEn.Clear();
			this.Alast_situation_desc.Clear();
		}

		public void touchTempSituation(string situation_key_L)
		{
			if (REG.match(situation_key_L, REG.RegSuffixNumber))
			{
				situation_key_L = REG.leftContext;
			}
			if (TX.getTX("Enemy_" + situation_key_L, true, true, null) != null)
			{
				this.addTempSituation(situation_key_L, 0, false);
			}
		}

		public static string situ_write_replace(string situation_key_L)
		{
			if (situation_key_L != null && (situation_key_L == "COW2LICK" || situation_key_L == "COW2" || situation_key_L == "COWLICK"))
			{
				return "COW";
			}
			return situation_key_L;
		}

		public void addTempSituation(string situation_key_L, int multiple_orgasm = 1, bool insert_before = false)
		{
			if (this.FlgLockWrite.isActive())
			{
				return;
			}
			if (TX.valid(situation_key_L))
			{
				int num = this.ALastSituEn.Count;
				int num2 = ((multiple_orgasm >= 1) ? 1 : 0);
				int i = 0;
				while (i < num)
				{
					EpSituation.SituDesc situDesc = this.ALastSituEn[i];
					if (situDesc.key == situation_key_L)
					{
						if (num2 == 0)
						{
							return;
						}
						situDesc.count++;
						situDesc.multiple = global::XX.X.Mx(situDesc.multiple, multiple_orgasm);
						this.ALastSituEn[i] = situDesc;
						num = -1;
						break;
					}
					else
					{
						i++;
					}
				}
				if (num >= 0)
				{
					if (insert_before)
					{
						this.ALastSituEn.Insert(0, new EpSituation.SituDesc(situation_key_L, num2, multiple_orgasm));
						return;
					}
					this.ALastSituEn.Add(new EpSituation.SituDesc(situation_key_L, num2, multiple_orgasm));
				}
			}
		}

		public void addManual(string tx_key)
		{
			if (this.FlgLockWrite.isActive())
			{
				return;
			}
			this.Alast_situation_desc.Remove("&&GM_ep_situation_default");
			this.Alast_situation_desc.Remove(tx_key);
			this.Alast_situation_desc.Add(tx_key);
		}

		public string getRandomKey()
		{
			if (this.ALastSituEn.Count == 0)
			{
				return null;
			}
			int count = this.ALastSituEn.Count;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				EpSituation.SituDesc situDesc = this.ALastSituEn[i];
				if (!(situDesc.key == "masturbate") && situDesc.key.IndexOf("_") != 0)
				{
					num++;
				}
			}
			if (num <= 0)
			{
				return null;
			}
			int num2 = global::XX.X.xors(num);
			for (int j = 0; j < count; j++)
			{
				EpSituation.SituDesc situDesc2 = this.ALastSituEn[j];
				if (!(situDesc2.key == "masturbate") && situDesc2.key.IndexOf("_") != 0 && num2-- <= 0)
				{
					return situDesc2.key;
				}
			}
			return null;
		}

		public bool flushLastExSituationTemp()
		{
			if (this.ALastSituEn.Count != 0)
			{
				this.Alast_situation_desc.Clear();
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				string text = null;
				using (STB stb = TX.PopBld(null, 0))
				{
					int count = this.ALastSituEn.Count;
					for (int i = 0; i < count; i++)
					{
						EpSituation.SituDesc situDesc = this.ALastSituEn[i];
						if (situDesc.count > 0)
						{
							string key = situDesc.key;
							stb.Clear();
							if (key == null)
							{
								goto IL_02AD;
							}
							uint num = <PrivateImplementationDetails>.ComputeStringHash(key);
							if (num <= 628305621U)
							{
								if (num != 75767418U)
								{
									if (num != 587212353U)
									{
										if (num != 628305621U)
										{
											goto IL_02AD;
										}
										if (!(key == "&&GM_ep_situation_gameover"))
										{
											goto IL_02AD;
										}
										if (text == null)
										{
											text = nelM2DBase.getMapTitleTxKey(null, null);
										}
										if (!TX.noe(text))
										{
											stb.Add("<b><font color=\"ff:#000000\">").Add(key, "[&&", text).Add("]</font></b>");
											goto IL_02FC;
										}
										goto IL_0339;
									}
									else
									{
										if (!(key == "&&GM_ep_situation_farm"))
										{
											goto IL_02AD;
										}
										if (text == null)
										{
											text = nelM2DBase.getMapTitleTxKey(null, null);
										}
										if (!TX.noe(text))
										{
											stb.Add(key, "[&&", text).Add("]");
											goto IL_02FC;
										}
										goto IL_0339;
									}
								}
								else if (!(key == "&&GM_ep_situation_gameover_battle_machine"))
								{
									goto IL_02AD;
								}
							}
							else if (num <= 1292934317U)
							{
								if (num != 674732180U)
								{
									if (num != 1292934317U)
									{
										goto IL_02AD;
									}
									if (!(key == "_MILK"))
									{
										goto IL_02AD;
									}
									if (text == null)
									{
										text = nelM2DBase.getMapTitleTxKey(null, null);
									}
									if (!TX.noe(text))
									{
										stb.Add("&&GM_ep_situation_omorashi_milk", "[&&", text).Add("]");
										goto IL_02FC;
									}
									goto IL_0339;
								}
								else if (!(key == "&&GM_ep_situation_gameover_battle_demon"))
								{
									goto IL_02AD;
								}
							}
							else if (num != 1601874105U)
							{
								if (num != 2190547544U)
								{
									goto IL_02AD;
								}
								if (!(key == "_PEE"))
								{
									goto IL_02AD;
								}
								if (text == null)
								{
									text = nelM2DBase.getMapTitleTxKey(null, null);
								}
								if (!TX.noe(text))
								{
									stb.Add("&&GM_ep_situation_omorashi", "[&&", text).Add("]");
									goto IL_02FC;
								}
								goto IL_0339;
							}
							else
							{
								if (!(key == "MASTURBATE"))
								{
									goto IL_02AD;
								}
								stb.Add("&&EP_finish_masturbate");
								goto IL_02FC;
							}
							if (EnemySummoner.isActiveBorder())
							{
								stb.Add("<b><font color=\"ff:#000000\">").Add(key, "[&&Summoner_", EnemySummoner.ActiveScript.key).Add("]</font></b>");
							}
							IL_02FC:
							if (situDesc.multiple > 1)
							{
								stb.Add(" &&GM_ep_situation_total_multi[", situDesc.multiple.ToString() + "]");
							}
							this.Alast_situation_desc.Add(stb.ToString());
							goto IL_0339;
							IL_02AD:
							if (key.IndexOf("&&") >= 0)
							{
								stb.Add(key);
								goto IL_02FC;
							}
							stb.Add("&&GM_ep_situation_total_reached1[");
							stb.Add("&&Enemy_", key, "]").Add("[", situDesc.count, "]");
							goto IL_02FC;
						}
						IL_0339:;
					}
				}
				this.ALastSituEn.Clear();
				return true;
			}
			return false;
		}

		public void readBinaryFrom(ByteArray Ba, bool new_ver = true)
		{
			int num = (int)Ba.readUShort();
			for (int i = 0; i < num; i++)
			{
				string text = EpSituation.situ_write_replace(Ba.readPascalString("utf-8", false));
				EpSituation.SituDesc situDesc = new EpSituation.SituDesc(text, 1, 1);
				if (new_ver)
				{
					situDesc.count = (int)Ba.readUByte();
					situDesc.multiple = (int)Ba.readUByte();
				}
				this.ALastSituEn.Add(situDesc);
			}
			if (new_ver)
			{
				num = (int)Ba.readUShort();
				this.Alast_situation_desc.Clear();
				for (int j = 0; j < num; j++)
				{
					this.Alast_situation_desc.Add(Ba.readString("utf-8", false));
				}
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			int num = this.ALastSituEn.Count;
			Ba.writeUShort((ushort)num);
			for (int i = 0; i < num; i++)
			{
				EpSituation.SituDesc situDesc = this.ALastSituEn[i];
				Ba.writePascalString(situDesc.key, "utf-8");
				Ba.writeByte(situDesc.count);
				Ba.writeByte(situDesc.multiple);
			}
			num = this.Alast_situation_desc.Count;
			Ba.writeUShort((ushort)num);
			for (int j = 0; j < num; j++)
			{
				Ba.writeString(this.Alast_situation_desc[j], "utf-8");
			}
		}

		public int getDescCount()
		{
			return this.Alast_situation_desc.Count;
		}

		public string getDescByIndex(int i)
		{
			return this.Alast_situation_desc[i];
		}

		public const string SITUATION_MASTURBATE = "masturbate";

		public const string SITUATION_MASTURBATE_UPPER = "MASTURBATE";

		public const string SITUATION_GO_DEMON = "&&GM_ep_situation_gameover_battle_demon";

		public const string SITUATION_GO_MECH = "&&GM_ep_situation_gameover_battle_machine";

		public const string SITUATION_GO = "&&GM_ep_situation_gameover";

		public const string SITUATION_FARM = "&&GM_ep_situation_farm";

		public const string SITUATION_IN_CITY = "&&GM_ep_situation_omorashi_in_city";

		public const string SITUATION_IN_DUNGEON = "&&GM_ep_situation_omorashi_in_dungeon";

		public const string SITUATION_MASTURBATE_FAIL = "&&GM_ep_situation_masturbate_fail";

		public const string SITUATION_PEE = "_PEE";

		public const string SITUATION_MILK = "_MILK";

		public const string SITUATION_OTHER = "OTHER";

		private readonly List<EpSituation.SituDesc> ALastSituEn;

		private readonly List<string> Alast_situation_desc;

		private const string default_last_situ_desc = "&&GM_ep_situation_default";

		public readonly Flagger FlgLockWrite = new Flagger(null, null);

		private struct SituDesc
		{
			public SituDesc(string _key, int _count, int _multiple)
			{
				this.key = _key;
				this.count = _count;
				this.multiple = _multiple;
			}

			public string key;

			public int count;

			public int multiple;
		}
	}
}

using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public static class UiEnemyDex
	{
		public static void newGame()
		{
			UiEnemyDex.Odefeated = new BDic<ENEMYID, UiEnemyDex.DefeatData>();
		}

		public static void readBinaryFrom(ByteReader Ba)
		{
			if (UiEnemyDex.Odefeated == null)
			{
				UiEnemyDex.newGame();
			}
			int num = Ba.readByte();
			UiEnemyDex.Odefeated.Clear();
			int num2 = (int)Ba.readUShort();
			for (int i = 0; i < num2; i++)
			{
				ENEMYID enemyid = (ENEMYID)Ba.readUInt();
				if (num == 0)
				{
					enemyid &= (ENEMYID)2147483647U;
				}
				if (num < 2)
				{
					ushort num3 = Ba.readUShort();
					UiEnemyDex.Odefeated[enemyid] = new UiEnemyDex.DefeatData(enemyid, num3);
					if (num == 0 && num3 > 0 && NDAT.isOverdriveable(enemyid, false))
					{
						enemyid |= (ENEMYID)2147483648U;
						UiEnemyDex.Odefeated[enemyid] = new UiEnemyDex.DefeatData(enemyid, num3);
					}
				}
				else
				{
					UiEnemyDex.Odefeated[enemyid] = UiEnemyDex.DefeatData.readBinaryFrom(Ba, enemyid);
				}
			}
			if (num <= 0 && SCN.fine_pvv(false) >= 10 && !UiEnemyDex.Odefeated.ContainsKey(ENEMYID.SLIME_0))
			{
				UiEnemyDex.Odefeated[ENEMYID.SLIME_0] = new UiEnemyDex.DefeatData(ENEMYID.SLIME_0, 1);
			}
		}

		public static void writeBinaryTo(ByteArray Ba)
		{
			if (UiEnemyDex.Odefeated == null)
			{
				UiEnemyDex.newGame();
			}
			Ba.writeByte(2);
			Ba.writeUShort((ushort)UiEnemyDex.Odefeated.Count);
			foreach (KeyValuePair<ENEMYID, UiEnemyDex.DefeatData> keyValuePair in UiEnemyDex.Odefeated)
			{
				Ba.writeUInt((uint)keyValuePair.Key);
				keyValuePair.Value.writeBinaryTo(Ba);
			}
		}

		public static void addDefeatCount(NelEnemy En, ENEMYID id, ushort count = 1, bool check_od = true)
		{
			if (check_od)
			{
				if (En.isOverDrive())
				{
					id |= (ENEMYID)2147483648U;
				}
				else
				{
					id &= (ENEMYID)2147483647U;
				}
			}
			if (UiEnemyDex.Odefeated == null)
			{
				UiEnemyDex.newGame();
			}
			UiEnemyDex.DefeatData defeatData;
			if (UiEnemyDex.Odefeated.TryGetValue(id, out defeatData))
			{
				int num = X.Mn((int)count, (int)(9999 - defeatData.count));
				if (num > 0)
				{
					defeatData.count += (ushort)num;
					UiEnemyDex.Odefeated[id] = defeatData;
				}
			}
			else
			{
				UiEnemyDex.Odefeated[id] = new UiEnemyDex.DefeatData(id, count);
			}
			if ((id & (ENEMYID)2147483648U) != (ENEMYID)0U)
			{
				UiEnemyDex.addDefeatCount(En, id & (ENEMYID)2147483647U, 0, false);
			}
		}

		public static UiEnemyDex.DefeatData Get(ENEMYID id)
		{
			if (UiEnemyDex.Odefeated != null)
			{
				return X.GetS<ENEMYID, UiEnemyDex.DefeatData>(UiEnemyDex.Odefeated, id, default(UiEnemyDex.DefeatData));
			}
			return default(UiEnemyDex.DefeatData);
		}

		public static void Write(UiEnemyDex.DefeatData D)
		{
			if (UiEnemyDex.Odefeated == null)
			{
				UiEnemyDex.newGame();
			}
			UiEnemyDex.Odefeated[D.id] = D;
		}

		public static List<UiEnemyDex.DefeatData> getListupDefeated(List<UiEnemyDex.DefeatData> Aout, bool input_if_not_defeated, bool id_zero_if_not_defeated = false)
		{
			BDic<string, NOD.BasicData> basicDataObject = NOD.getBasicDataObject();
			if (Aout == null)
			{
				Aout = new List<UiEnemyDex.DefeatData>(basicDataObject.Count);
			}
			foreach (KeyValuePair<string, NOD.BasicData> keyValuePair in basicDataObject)
			{
				ENEMYID enemyid;
				if ((keyValuePair.Value.hide_in_FG_list & 3) != 3 && FEnum<ENEMYID>.TryParse(keyValuePair.Key, out enemyid, true))
				{
					UiEnemyDex.DefeatData defeatData = UiEnemyDex.Get(enemyid);
					if ((keyValuePair.Value.hide_in_FG_list & 1) == 0 && (defeatData.enabled_fd || input_if_not_defeated))
					{
						if (defeatData.enabled_fd || !id_zero_if_not_defeated)
						{
							defeatData.id = enemyid;
						}
						Aout.Add(defeatData);
					}
					if ((keyValuePair.Value.hide_in_FG_list & 2) == 0 && NDAT.isOverdriveable(enemyid, false))
					{
						defeatData = UiEnemyDex.Get(enemyid | (ENEMYID)2147483648U);
						if (defeatData.enabled_fd || input_if_not_defeated)
						{
							if (defeatData.enabled_fd || !id_zero_if_not_defeated)
							{
								defeatData.id = enemyid | (ENEMYID)2147483648U;
							}
							Aout.Add(defeatData);
						}
					}
				}
			}
			return Aout;
		}

		public static void set_fd_favorite(ENEMYID id, bool f)
		{
			UiEnemyDex.DefeatData defeatData;
			if (UiEnemyDex.Odefeated.TryGetValue(id, out defeatData))
			{
				defeatData.fd_favorite = f;
				UiEnemyDex.Odefeated[id] = defeatData;
			}
		}

		public static bool get_fd_favorite(ENEMYID id)
		{
			UiEnemyDex.DefeatData defeatData;
			return UiEnemyDex.Odefeated.TryGetValue(id, out defeatData) && defeatData.fd_favorite;
		}

		private static BDic<ENEMYID, UiEnemyDex.DefeatData> Odefeated;

		public struct DefeatData
		{
			public DefeatData(ENEMYID _id, ushort _count)
			{
				this.id = _id;
				this.count = _count;
				this.flags = 0;
			}

			public bool fd_favorite
			{
				get
				{
					return (this.flags & 1) > 0;
				}
				set
				{
					this.flags = (byte)(value ? ((int)(this.flags | 1)) : ((int)this.flags & -2));
				}
			}

			public bool enabled_fd
			{
				get
				{
					return this.count > 0 || this.smnc_unlocked;
				}
			}

			public int get_count(int def = -1)
			{
				if (this.id <= (ENEMYID)0U)
				{
					return def;
				}
				return (int)this.count;
			}

			public bool smnc_unlocked
			{
				get
				{
					return (this.flags & 2) > 0;
				}
				set
				{
					this.flags = (byte)(value ? ((int)(this.flags | 2)) : ((int)this.flags & -3));
				}
			}

			public bool smnc_unlocked_new
			{
				get
				{
					return (this.flags & 4) > 0;
				}
				set
				{
					this.flags = (byte)(value ? ((int)(this.flags | 4)) : ((int)this.flags & -5));
				}
			}

			public bool is_od
			{
				get
				{
					return (this.id & (ENEMYID)2147483648U) > (ENEMYID)0U;
				}
			}

			public static UiEnemyDex.DefeatData readBinaryFrom(ByteReader Ba, ENEMYID id)
			{
				return new UiEnemyDex.DefeatData(id, 0)
				{
					count = Ba.readUShort(),
					flags = Ba.readUByte()
				};
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeUShort(this.count);
				Ba.writeByte((int)this.flags);
			}

			public ENEMYID id;

			public ushort count;

			private byte flags;
		}
	}
}

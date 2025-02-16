using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;

namespace nel
{
	public class UiEnemyDex
	{
		public static void newGame()
		{
			UiEnemyDex.Odefeated = new BDic<ENEMYID, ushort>();
		}

		public static void readBinaryFrom(ByteArray Ba)
		{
			if (UiEnemyDex.Odefeated == null)
			{
				UiEnemyDex.newGame();
			}
			Ba.readByte();
			UiEnemyDex.Odefeated.Clear();
			int num = (int)Ba.readUShort();
			for (int i = 0; i < num; i++)
			{
				uint num2 = Ba.readUInt();
				UiEnemyDex.Odefeated[(ENEMYID)num2] = Ba.readUShort();
			}
		}

		public static void writeBinaryTo(ByteArray Ba)
		{
			if (UiEnemyDex.Odefeated == null)
			{
				UiEnemyDex.newGame();
			}
			Ba.writeByte(0);
			Ba.writeUShort((ushort)UiEnemyDex.Odefeated.Count);
			foreach (KeyValuePair<ENEMYID, ushort> keyValuePair in UiEnemyDex.Odefeated)
			{
				Ba.writeUInt((uint)keyValuePair.Key);
				Ba.writeUShort(keyValuePair.Value);
			}
		}

		public static void addDefeatCount(ENEMYID id)
		{
			if (UiEnemyDex.Odefeated == null)
			{
				UiEnemyDex.newGame();
			}
			ushort num;
			if (UiEnemyDex.Odefeated.TryGetValue(id, out num))
			{
				if (num <= 10000)
				{
					num = (UiEnemyDex.Odefeated[id] = num + 1);
					return;
				}
			}
			else
			{
				UiEnemyDex.Odefeated[id] = 1;
			}
		}

		private static BDic<ENEMYID, ushort> Odefeated;
	}
}

using System;
using System.Collections.Generic;
using Better;
using nel.mgm;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public class ACHIVE
	{
		public ACHIVE()
		{
			this.Ous = new BDic<ACHIVE.MENT, ushort>(16);
		}

		public void newGame()
		{
			this.Ous.Clear();
		}

		public void CopyFrom(ACHIVE Src)
		{
			this.newGame();
			foreach (KeyValuePair<ACHIVE.MENT, ushort> keyValuePair in Src.Ous)
			{
				this.Ous[keyValuePair.Key] = keyValuePair.Value;
			}
		}

		public void readFromBytes(ByteReader Ba)
		{
			this.newGame();
			ushort num = Ba.readUShort();
			for (int i = 0; i < (int)num; i++)
			{
				ushort num2 = Ba.readUShort();
				this.Ous[(ACHIVE.MENT)num2] = Ba.readUShort();
			}
		}

		public void writeToBytes(ByteArray Ba)
		{
			int count = this.Ous.Count;
			Ba.writeUShort((ushort)count);
			foreach (KeyValuePair<ACHIVE.MENT, ushort> keyValuePair in this.Ous)
			{
				Ba.writeUShort((ushort)keyValuePair.Key);
				Ba.writeUShort(keyValuePair.Value);
			}
		}

		public ushort Get(string s)
		{
			ACHIVE.MENT ment;
			if (Enum.TryParse<ACHIVE.MENT>(s, out ment))
			{
				return this.Get(ment);
			}
			X.de("不明なMENT: " + s, null);
			return 0;
		}

		public ushort Get(ACHIVE.MENT type)
		{
			return X.GetS<ACHIVE.MENT, ushort>(this.Ous, type, 0);
		}

		public void Set(string s, int val)
		{
			ACHIVE.MENT ment;
			if (Enum.TryParse<ACHIVE.MENT>(s, out ment))
			{
				this.Set(ment, val);
				return;
			}
			X.de("不明なMENT: " + s, null);
		}

		public void Set(ACHIVE.MENT type, int val)
		{
			this.Ous[type] = (ushort)X.MMX(0, val, 65535);
		}

		public void Add(ACHIVE.MENT type, int val = 1)
		{
			this.Set(type, (int)this.Get(type) + val);
		}

		public BDic<ACHIVE.MENT, ushort> getWholeDictionary()
		{
			return this.Ous;
		}

		public void fineOldData(NelM2DBase M2D)
		{
			this.Set(ACHIVE.MENT.city_milk_total, X.Mx((int)this.Get(ACHIVE.MENT.city_milk_total), COOK.Mgm.getScore(MGMSCORE.FARM, 0)));
			this.Set(ACHIVE.MENT.lunch_total_stomach, X.Mx((int)this.Get(ACHIVE.MENT.lunch_total_stomach), (int)M2D.IMNG.StmNoel.cost));
			this.Set(ACHIVE.MENT.lunch_max_stomach, X.Mx((int)this.Get(ACHIVE.MENT.lunch_max_stomach), (int)M2D.IMNG.StmNoel.cost));
		}

		private readonly BDic<ACHIVE.MENT, ushort> Ous;

		public enum MENT : ushort
		{
			obtain_danger,
			apply_damage_enemy,
			get_damage_enemy,
			gameover,
			cure_hp_bench,
			cure_cloth_bench,
			break_cloth_bench,
			alchemy_crafted,
			treasure_max_obtain = 2000,
			treasure_total_obtain,
			lunch_max_stomach,
			lunch_total_stomach,
			city_gq_success = 50000,
			city_gq_failure,
			ttr_win,
			ttr_lose,
			city_milk_total,
			city_gq_rank,
			city_gq_rank_reduce
		}
	}
}

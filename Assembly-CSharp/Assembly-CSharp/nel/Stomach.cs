using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class Stomach
	{
		public float water_drunk_level { get; private set; }

		public Stomach(Stomach _Src = null, PR Pr = null)
		{
			this.Src = _Src;
			this.Pr = ((_Src != null) ? _Src.Pr : Pr);
		}

		public void clear()
		{
			this.OEffect.Clear();
			this.OReduce.Clear();
			this.AEaten.Clear();
			this.Aeaten_cost.Clear();
			if (this.ADuped != null)
			{
				this.ADuped.Clear();
			}
			this.cost_applied_level = -1f;
			this.water_drunk_level = 0f;
			this.ser_apply_ratio = 1f;
			this.has_water = false;
			this.cost = 0f;
		}

		public Stomach initForTemporary()
		{
			this.clear();
			if (this.ADuped == null)
			{
				this.ADuped = new List<RCP.RPI_EFFECT>(4);
			}
			else
			{
				this.ADuped.Clear();
			}
			if (this.Src != null)
			{
				foreach (KeyValuePair<RCP.RPI_EFFECT, float> keyValuePair in this.Src.OEffect)
				{
					this.OEffect[keyValuePair.Key] = keyValuePair.Value;
				}
				foreach (KeyValuePair<RCP.RPI_EFFECT, float> keyValuePair2 in this.Src.OReduce)
				{
					this.OReduce[keyValuePair2.Key] = keyValuePair2.Value;
				}
				this.AEaten.Clear();
				int count = this.Src.AEaten.Count;
				for (int i = 0; i < count; i++)
				{
					this.AEaten.Add(new Stomach.DishInStomach(this.Src.AEaten[i]));
				}
				this.has_water = this.Src.has_water;
				this.cost = this.Src.cost;
				this.cost_max = this.Src.cost_max;
				this.fineCostCache();
			}
			return this;
		}

		private Stomach addEffect(RCP.RPI_EFFECT ef, float lvl01, float multiply)
		{
			if (ef == RCP.RPI_EFFECT.NONE)
			{
				return this;
			}
			float num = (float)X.IntR(X.MMX(-1f, lvl01, 1f) * 100f) / 100f * multiply;
			if (num >= 0f)
			{
				float num2 = X.Get<RCP.RPI_EFFECT, float>(this.OEffect, ef, 0f);
				if (this.ADuped != null && num2 > 0f && num2 >= num && this.ADuped.IndexOf(ef) == -1)
				{
					this.ADuped.Add(ef);
				}
				this.OEffect[ef] = X.Mn(X.Mx(num2, num), 1f);
			}
			else
			{
				this.OReduce[ef] = X.Get<RCP.RPI_EFFECT, float>(this.OReduce, ef, 0f) - num;
			}
			return this;
		}

		public bool applyableDoubleBonus(RCP.RecipeDish Dh)
		{
			if (Dh.Rcp.is_water)
			{
				return false;
			}
			int count = this.AEaten.Count;
			for (int i = 0; i < count; i++)
			{
				if (!this.AEaten[i].is_water)
				{
					return false;
				}
			}
			return true;
		}

		public float getEatableLevel(float add_cost)
		{
			float num = X.Mn((float)this.cost_max - this.cost, add_cost);
			if (num < 0.0625f)
			{
				return 0f;
			}
			float num2 = 1f;
			if (num < add_cost)
			{
				num2 = num / add_cost;
			}
			return num2;
		}

		public Stomach addEffect(RCP.RecipeDish Dh, bool fine_pr_state = false, bool consider_bonus = true, bool add_to_achive = false)
		{
			float num = 1f;
			bool flag = false;
			if (this.applyableDoubleBonus(Dh) && consider_bonus)
			{
				num *= 1.5f;
				flag = true;
			}
			float num2 = X.Mn((float)this.cost_max - this.cost, (float)Dh.cost);
			this.cost_applied_level = this.getEatableLevel((float)Dh.cost);
			num *= this.cost_applied_level;
			this.cost += num2;
			if (Dh.Rcp.is_water)
			{
				this.has_water = true;
			}
			foreach (KeyValuePair<RCP.RPI_EFFECT, Vector2> keyValuePair in Dh.OEffect)
			{
				this.addEffect(keyValuePair.Key, keyValuePair.Value.x, num);
			}
			this.fineReducedEffect();
			this.calcWaterDrunkLevel();
			this.AEaten.Add(new Stomach.DishInStomach(Dh, num, flag, num2));
			this.Aeaten_cost.Add(new Stomach.CostData(num2, flag, Dh.Rcp.is_water));
			if (this.Src == null)
			{
				Dh.referred++;
			}
			if (fine_pr_state)
			{
				if (this.Pr != null)
				{
					if (Dh.ItemData.has(NelItem.CATEG.SER_APPLY))
					{
						Dh.ItemData.Use(this.Pr, null, Dh.calced_grade, this.Pr);
					}
					this.Pr.JuiceCon.addWaterDrunkCache(num2 * (Dh.Rcp.is_water ? 1f : 0.14f), this.water_drunk_level, -1);
				}
				this.finePrStatus(true);
				if (this.Pr != null && Dh.ItemData.has(NelItem.CATEG.SER_APPLY))
				{
					this.Pr.recheck_emot_in_gm = true;
				}
			}
			if (add_to_achive && this != Stomach.Temporary)
			{
				COOK.CurAchive.Set(ACHIVE.MENT.lunch_max_stomach, (int)X.Mx((float)COOK.getAchive(ACHIVE.MENT.lunch_max_stomach), this.cost));
				COOK.CurAchive.Add(ACHIVE.MENT.lunch_total_stomach, X.IntC(num2));
			}
			return this;
		}

		private float calcWaterDrunkLevel()
		{
			int count = this.AEaten.Count;
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				Stomach.DishInStomach dishInStomach = this.AEaten[i];
				num += (dishInStomach.Dh.Rcp.is_water ? 1f : 0.14f) * dishInStomach.cost;
			}
			this.water_drunk_level = num;
			return num;
		}

		public void fineReducedEffect()
		{
			foreach (KeyValuePair<RCP.RPI_EFFECT, float> keyValuePair in this.OReduce)
			{
				this.OEffect[keyValuePair.Key] = X.Mx(-1f, X.Get<RCP.RPI_EFFECT, float>(this.OEffect, keyValuePair.Key, 0f) - keyValuePair.Value);
			}
			this.OReduce.Clear();
		}

		public Stomach fineEffect(bool fine_pr_state)
		{
			this.OEffect.Clear();
			this.OReduce.Clear();
			int count = this.AEaten.Count;
			this.cost = 0f;
			this.has_water = false;
			for (int i = 0; i < count; i++)
			{
				Stomach.DishInStomach dishInStomach = this.AEaten[i];
				this.cost += dishInStomach.cost;
				if (dishInStomach.Dh.Rcp.is_water)
				{
					this.has_water = true;
				}
				foreach (KeyValuePair<RCP.RPI_EFFECT, Vector2> keyValuePair in dishInStomach.Dh.OEffect)
				{
					this.addEffect(keyValuePair.Key, keyValuePair.Value.x, dishInStomach.lvl01);
				}
				this.fineReducedEffect();
			}
			if (fine_pr_state)
			{
				this.finePrStatus(false);
			}
			return this.fineCostCache();
		}

		private Stomach fineCostCache()
		{
			this.Aeaten_cost.Clear();
			int count = this.AEaten.Count;
			for (int i = 0; i < count; i++)
			{
				Stomach.DishInStomach dishInStomach = this.AEaten[i];
				this.Aeaten_cost.Add(new Stomach.CostData(dishInStomach.cost, dishInStomach.bonused, dishInStomach.is_water));
			}
			this.calcWaterDrunkLevel();
			return this;
		}

		public void finePrStatus(bool extend_hpmp = false)
		{
			this.Pr.getSkillManager().resetSkillConnection(false, extend_hpmp, extend_hpmp);
		}

		public float progress(float lvl_cost, bool fine_pr_state, out float water_progress, bool announce = true, bool only_water = false)
		{
			BList<Stomach.DishInStomach> blist = null;
			if (announce)
			{
				blist = ListBuffer<Stomach.DishInStomach>.Pop(2);
			}
			bool flag = false;
			float num = this.cost;
			water_progress = 0f;
			float num2 = 0f;
			do
			{
				int count = this.AEaten.Count;
				float num3 = 0f;
				for (int i = count - 1; i >= 0; i--)
				{
					Stomach.DishInStomach dishInStomach = this.AEaten[i];
					if (!only_water || dishInStomach.is_water)
					{
						num3 += (float)dishInStomach.getProgressLevel(i, count);
					}
				}
				if (num3 == 0f)
				{
					break;
				}
				this.cost = 0f;
				float num4 = lvl_cost;
				int j = count - 1;
				while (j >= 0)
				{
					Stomach.DishInStomach dishInStomach2 = this.AEaten[j];
					if (only_water && !dishInStomach2.is_water)
					{
						goto IL_0183;
					}
					float num5 = X.Mn(dishInStomach2.cost, num4 * (float)dishInStomach2.getProgressLevel(j, count) / num3);
					float num6 = num5 * dishInStomach2.Dh.Rcp.cost_reduce_ratio;
					if (only_water && dishInStomach2.is_water && dishInStomach2.Dh.Rcp.categ == RCP.RP_CATEG.ACTIHOL)
					{
						num6 = dishInStomach2.cost;
					}
					water_progress += (dishInStomach2.Dh.Rcp.is_water ? 1f : 0.14f) * num5;
					dishInStomach2.reduceCost(num6, only_water);
					lvl_cost -= num5;
					num2 += num6;
					if (only_water)
					{
						flag = true;
					}
					if (dishInStomach2.cost >= 0.0625f)
					{
						goto IL_0183;
					}
					this.AEaten.RemoveAt(j);
					flag = true;
					if (blist != null)
					{
						blist.Add(dishInStomach2);
					}
					IL_0197:
					j--;
					continue;
					IL_0183:
					this.cost += dishInStomach2.cost;
					goto IL_0197;
				}
			}
			while (lvl_cost >= 0.0625f && this.AEaten.Count != 0);
			if (flag)
			{
				this.fineEffect(fine_pr_state);
			}
			else
			{
				this.fineCostCache();
			}
			if (announce && num2 > 0f)
			{
				if (only_water)
				{
					if (blist.Count > 0)
					{
						UILog.Instance.AddAlert(TX.GetA("Alert_water_lost_from_stomach", blist.Count.ToString()), UILogRow.TYPE.ALERT);
					}
					else
					{
						UILog.Instance.AddAlert(TX.Get("Alert_water_reduced_from_stomach", ""), UILogRow.TYPE.ALERT);
					}
				}
				else if (this.cost == 0f)
				{
					if (blist.Count > 0)
					{
						UILog.Instance.AddAlertTX("Alert_no_content_in_stomach", UILogRow.TYPE.ALERT);
					}
				}
				else
				{
					int count2 = blist.Count;
					for (int k = 0; k < count2; k++)
					{
						Stomach.DishInStomach dishInStomach3 = blist[k];
						UILog.Instance.AddAlert(TX.GetA("Alert_stomach_item_lost", dishInStomach3.Dh.title), UILogRow.TYPE.ALERT).setIcon(MTR.AItemIcon[dishInStomach3.Dh.ItemData.getIcon(null, null)], uint.MaxValue);
					}
				}
			}
			if (blist != null)
			{
				blist.Dispose();
			}
			return num2;
		}

		public void progressDrunkSer(float ratio = 1f)
		{
			if (this.Pr.Ser.has(SER.DRUNK) && ratio < 1f)
			{
				X.XORSP();
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			int count = this.AEaten.Count;
			Ba.writeUShort((ushort)count);
			for (int i = 0; i < count; i++)
			{
				this.AEaten[i].writeBinaryTo(Ba);
			}
		}

		public void readBinaryFrom(ByteReader Ba, bool recipe_reffer_add = false, bool fine_pr_state = true)
		{
			this.clear();
			int num = (int)Ba.readUShort();
			for (int i = 0; i < num; i++)
			{
				Stomach.DishInStomach dishInStomach = new Stomach.DishInStomach(Ba);
				if (dishInStomach.valid)
				{
					this.AEaten.Add(dishInStomach);
					if (recipe_reffer_add)
					{
						dishInStomach.Dh.referred++;
					}
				}
			}
			this.fineEffect(fine_pr_state);
		}

		public string cost_str
		{
			get
			{
				if (this.cost == (float)((int)this.cost))
				{
					return this.cost.ToString();
				}
				return X.spr_after(this.cost, 2);
			}
		}

		public bool isEffectDuped(RCP.RPI_EFFECT ef)
		{
			return this.ADuped != null && this.ADuped.IndexOf(ef) >= 0;
		}

		public bool Has(RCP.RP_CATEG categ)
		{
			for (int i = this.AEaten.Count - 1; i >= 0; i--)
			{
				if (this.AEaten[i].Dh.Rcp.categ == categ)
				{
					return true;
				}
			}
			return false;
		}

		public bool hasWater()
		{
			return this.has_water;
		}

		public bool isSame(Stomach Stm, RCP.RPI_EFFECT ef)
		{
			return Stm.getValue(ef) == this.getValue(ef);
		}

		public bool isAnyEffectDuped()
		{
			return this.ADuped != null && this.ADuped.Count > 0;
		}

		public bool eaten_anything
		{
			get
			{
				return this.AEaten.Count > 0;
			}
		}

		public float eaten_cost
		{
			get
			{
				return this.cost;
			}
		}

		public int eaten_item_count
		{
			get
			{
				return this.AEaten.Count;
			}
		}

		public Stomach.DishInStomach[] getEatenDishArray()
		{
			return this.AEaten.ToArray();
		}

		public List<Stomach.CostData> getEatenCostArray()
		{
			return this.Aeaten_cost;
		}

		public BDic<RCP.RPI_EFFECT, float> getLevelDictionary01()
		{
			return this.OEffect;
		}

		public float getValue(RCP.RPI_EFFECT ef)
		{
			float num = X.Get<RCP.RPI_EFFECT, float>(this.OEffect, ef, 0f);
			if (num < 0f)
			{
				return num;
			}
			return num * this.ser_apply_ratio;
		}

		public void newGame()
		{
		}

		public static Stomach Temporary;

		public readonly PR Pr;

		public List<RCP.RPI_EFFECT> ADuped;

		private List<Stomach.DishInStomach> AEaten = new List<Stomach.DishInStomach>();

		private BDic<RCP.RPI_EFFECT, float> OEffect = new BDic<RCP.RPI_EFFECT, float>();

		private BDic<RCP.RPI_EFFECT, float> OReduce = new BDic<RCP.RPI_EFFECT, float>();

		private List<Stomach.CostData> Aeaten_cost = new List<Stomach.CostData>();

		public float cost;

		public int cost_max = 28;

		public float cost_applied_level = -1f;

		public const int COST_MAX_DEFAULT = 28;

		public const float HARAPEKO_BONUS = 1.5f;

		private Stomach Src;

		public bool has_water;

		public float ser_apply_ratio = 1f;

		public const float effect_max = 1f;

		private const float water_level_in_normal_dish = 0.14f;

		public class DishInStomach
		{
			public DishInStomach(RCP.RecipeDish _Dh, float _lvl01, bool _bonused, float _cost)
			{
				this.Dh = _Dh;
				this.lvl01 = _lvl01;
				this.bonused = _bonused;
				this.cost = _cost;
			}

			public DishInStomach(Stomach.DishInStomach _Src)
			{
				this.Dh = _Src.Dh;
				this.lvl01 = _Src.lvl01;
				this.bonused = _Src.bonused;
				this.cost = _Src.cost;
			}

			public DishInStomach(ByteReader Ba)
			{
				this.Dh = RCP.getDishByRecipeId(Ba.readUInt());
				if (this.Dh != null)
				{
					this.Dh.referred++;
				}
				this.lvl01 = Ba.readFloat();
				this.bonused = Ba.readBoolean();
				this.cost = Ba.readFloat();
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeUInt(this.Dh.recipe_id);
				Ba.writeFloat(this.lvl01);
				Ba.writeBool(this.bonused);
				Ba.writeFloat(this.cost);
			}

			public bool is_water
			{
				get
				{
					return this.Dh.Rcp.is_water;
				}
			}

			public int getProgressLevel(int index, int max)
			{
				return (this.is_water ? 6 : (max - index)) * (this.bonused ? 2 : 1);
			}

			public void reduceCost(float lvl, bool reduce_effect)
			{
				if (lvl <= 0f || this.cost <= 0f)
				{
					return;
				}
				float num = this.cost;
				this.cost = X.Mx(0f, this.cost - lvl);
				if (this.cost <= 0f)
				{
					return;
				}
				if (reduce_effect)
				{
					this.lvl01 *= this.cost / num;
				}
			}

			public bool valid
			{
				get
				{
					return this.Dh != null;
				}
			}

			public readonly RCP.RecipeDish Dh;

			public float lvl01;

			public bool bonused;

			public float cost;
		}

		public struct CostData
		{
			public CostData(float _cost, bool _bonused, bool _is_water)
			{
				this.cost = _cost;
				this.bonused = _bonused;
				this.is_water = _is_water;
			}

			public float cost;

			public bool bonused;

			public bool is_water;
		}
	}
}

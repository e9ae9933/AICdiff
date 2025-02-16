using System;
using System.Collections.Generic;
using XX;

namespace nel.mgm.smncr
{
	internal class EnemyEntry
	{
		public bool is_od
		{
			get
			{
				return (this.id & (ENEMYID)2147483648U) > (ENEMYID)0U;
			}
		}

		public EnemyEntry(NelM2DBase NM2D, UiEnemyDex.DefeatData _DData, ENEMYID _id)
		{
			this.id = _id;
			this.DData = _DData;
			this.id_tostr = FEnum<ENEMYID>.ToStr(this.id & (ENEMYID)2147483647U);
			this.Nod = NOD.getBasicData(this.id_tostr);
			bool flag = (this.id & (ENEMYID)2147483648U) > (ENEMYID)0U;
			if (this.Nod == null)
			{
				this.unlock = UiSmncEnemyEditor.UNLK.INVALID;
				return;
			}
			this.unlock = UiSmncEnemyEditor.UNLK.HIDDEN;
			if (this.id != this.DData.id)
			{
				return;
			}
			int num = ((this.Nod.Asmnc_family != null) ? this.Nod.Asmnc_family.Count : 0);
			this.defeat_count = 0;
			for (int i = ((this.Nod.Asmnc_family != null) ? 0 : (-1)); i < num; i++)
			{
				UiEnemyDex.DefeatData defeatData;
				if (i < 0)
				{
					defeatData = this.DData;
				}
				else
				{
					ENEMYID enemyid = this.Nod.Asmnc_family[i] | (flag ? ((ENEMYID)2147483648U) : ((ENEMYID)0U));
					defeatData = ((enemyid == this.id) ? this.DData : UiEnemyDex.Get(enemyid));
				}
				if (defeatData.id != (ENEMYID)0U)
				{
					if (defeatData.smnc_unlocked)
					{
						this.unlock = UiSmncEnemyEditor.UNLK.UNLOCK;
						break;
					}
					this.defeat_count += defeatData.get_count(0);
				}
			}
			if (this.unlock == UiSmncEnemyEditor.UNLK.HIDDEN)
			{
				if (this.defeat_count >= this.need_defeat_count)
				{
					this.unlock = UiSmncEnemyEditor.UNLK.UNLOCK_NEW;
					return;
				}
				this.unlock = ((this.defeat_count > 0) ? UiSmncEnemyEditor.UNLK.LOCKED : UiSmncEnemyEditor.UNLK.HIDDEN);
				if (this.unlock == UiSmncEnemyEditor.UNLK.LOCKED)
				{
					List<NelItemEntry> aneedUnlockItems = this.ANeedUnlockItems;
					if (aneedUnlockItems != null)
					{
						bool flag2 = true;
						num = aneedUnlockItems.Count;
						for (int j = 0; j < num; j++)
						{
							NelItemEntry nelItemEntry = aneedUnlockItems[j];
							if (NM2D.IMNG.countItem(nelItemEntry.Data, (int)nelItemEntry.grade, false, false) < nelItemEntry.count)
							{
								flag2 = false;
								break;
							}
						}
						if (flag2)
						{
							this.unlock = UiSmncEnemyEditor.UNLK.LOCKED_UNLOCKABLE;
						}
					}
				}
			}
		}

		public void Unlock(UiSmncEnemyEditor Con)
		{
			if (this.Nod == null)
			{
				return;
			}
			int num = ((this.Nod.Asmnc_family != null) ? this.Nod.Asmnc_family.Count : 0);
			for (int i = ((this.Nod.Asmnc_family != null) ? 0 : (-1)); i < num; i++)
			{
				UiEnemyDex.DefeatData defeatData;
				if (i < 0)
				{
					defeatData = this.DData;
				}
				else
				{
					ENEMYID enemyid = this.Nod.Asmnc_family[i] | (this.is_od ? ((ENEMYID)2147483648U) : ((ENEMYID)0U));
					defeatData = UiEnemyDex.Get(enemyid);
					defeatData.id = enemyid;
				}
				if (!defeatData.smnc_unlocked)
				{
					defeatData.smnc_unlocked = true;
					defeatData.smnc_unlocked_new = true;
					Con.need_fine_dex_list = true;
					if (defeatData.id == this.id)
					{
						this.DData = defeatData;
					}
					else
					{
						EnemyEntry entry = Con.GetEntry(defeatData.id);
						if (entry != null && entry != this && !entry.available_enemy)
						{
							entry.unlock = UiSmncEnemyEditor.UNLK.UNLOCK_NEW;
						}
					}
					UiEnemyDex.Write(defeatData);
				}
			}
		}

		public bool hidden
		{
			get
			{
				return this.unlock == UiSmncEnemyEditor.UNLK.HIDDEN || this.unlock == UiSmncEnemyEditor.UNLK.INVALID;
			}
		}

		public bool available_enemy
		{
			get
			{
				return this.unlock == UiSmncEnemyEditor.UNLK.UNLOCK;
			}
		}

		public bool locked
		{
			get
			{
				return this.hidden || this.unlock == UiSmncEnemyEditor.UNLK.LOCKED || this.unlock == UiSmncEnemyEditor.UNLK.LOCKED_UNLOCKABLE || this.unlock == UiSmncEnemyEditor.UNLK.UNLOCK_NEW;
			}
		}

		public string localized_name
		{
			get
			{
				if (this.localized_name_ == null)
				{
					this.localized_name_ = NDAT.getEnemyName(this.id, false);
				}
				return this.localized_name_;
			}
		}

		public int need_defeat_count
		{
			get
			{
				if (!this.is_od)
				{
					return (int)this.Nod.unlock_smnc_count;
				}
				return (int)this.Nod.unlock_smnc_count_od;
			}
		}

		public List<NelItemEntry> ANeedUnlockItems
		{
			get
			{
				if (!this.is_od)
				{
					return this.Nod.Aunlock_smnc;
				}
				return this.Nod.Aunlock_smnc_od;
			}
		}

		public ENEMYID id;

		public string id_tostr;

		public UiSmncEnemyEditor.UNLK unlock;

		public NOD.BasicData Nod;

		public int defeat_count;

		public ButtonSkinNelSmncEnemy Sk;

		public UiEnemyDex.DefeatData DData;

		private string localized_name_;
	}
}

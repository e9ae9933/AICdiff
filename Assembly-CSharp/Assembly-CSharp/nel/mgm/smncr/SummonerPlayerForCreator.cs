using System;
using System.Collections.Generic;
using Better;
using nel.smnp;
using XX;

namespace nel.mgm.smncr
{
	public class SummonerPlayerForCreator : SummonerPlayer
	{
		public SummonerPlayerForCreator(SummonerForCreator _Summoner, M2LpUiSmnCreator _LpS, EfParticleFuncCalc _FuncBase, CsvReaderA _CR, out bool bgm_replaced, bool auto_prepare_content = true)
			: base(_Summoner, _FuncBase, _CR, out bgm_replaced, false)
		{
			this.SmnC = _Summoner;
			this.LpS = _LpS;
			if (auto_prepare_content)
			{
				this.prepareEnemyConent(ref bgm_replaced);
			}
		}

		protected override void prepareEnemyConent(ref bool bgm_replaced)
		{
			if (this.SmnC.preload_script_active)
			{
				base.prepareEnemyConent(ref bgm_replaced);
				return;
			}
			base.prepareEnemyConentInner(ref bgm_replaced);
		}

		protected override void prepareEnemyConentFromScript(List<SmnEnemyKind> AKindL, List<SmnPoint> ASmnPosL, out BDic<string, int[]> OEnemyCountMax, ref int appear_add, out int max_enemy_count, ref int count_add, out int splitter_id, out int countadd_priority, out bool drop_all_reels_after, out bool force_can_get_whole_reels, out bool odable_enemy_exist)
		{
			if (this.SmnC.preload_script_active)
			{
				base.prepareEnemyConentFromScript(AKindL, ASmnPosL, out OEnemyCountMax, ref appear_add, out max_enemy_count, ref count_add, out splitter_id, out countadd_priority, out drop_all_reels_after, out force_can_get_whole_reels, out odable_enemy_exist);
				return;
			}
			NightController nightCon = this.LpS.nM2D.NightCon;
			OEnemyCountMax = null;
			max_enemy_count = 99;
			splitter_id = 0;
			countadd_priority = 1;
			drop_all_reels_after = false;
			force_can_get_whole_reels = false;
			odable_enemy_exist = false;
			this.delay_one = 10f;
			this.delay_one_second = (this.delay_filled = 160f);
			SmncFile currentFile = this.LpS.getCurrentFile();
			this.max_enemy_appear_whole = ((currentFile.maxappear == 0) ? 99 : currentFile.maxappear);
			for (int i = currentFile.Aen_list.Count - 1; i >= 0; i--)
			{
				SmncFile.EnemyInfo enemyInfo = currentFile.Aen_list[i];
				NOD.BasicData basicData = NOD.getBasicData(enemyInfo.id_tostr);
				if (basicData != null)
				{
					float num = 1f / (float)X.Mx(30, basicData.maxhp) * (enemyInfo.is_od ? 0.25f : 1f);
					float num2 = 1f / (float)X.Mx(30, basicData.maxmp) * (enemyInfo.is_od ? 0.35f : 1f);
					int count = enemyInfo.count;
					SmnEnemyKind smnEnemyKind = new SmnEnemyKind(enemyInfo.id_tostr, count, 0, (float)enemyInfo.mp_min100 * 0.01f, (float)enemyInfo.mp_max100 * 0.01f, "", num, num2, ENATTR.NORMAL);
					if (enemyInfo.is_od)
					{
						smnEnemyKind.pre_overdrive = true;
					}
					else if (smnEnemyKind.EnemyDesc.overdriveable)
					{
						odable_enemy_exist = true;
					}
					AKindL.Add(smnEnemyKind);
				}
			}
			if (currentFile.Oid2appear.Count > 0)
			{
				foreach (KeyValuePair<ENEMYID, int> keyValuePair in currentFile.Oid2appear)
				{
					ENEMYID key = keyValuePair.Key;
					this.OMaxEnemyAppear[key.ToString()] = new EnAppearMax(((keyValuePair.Value == 0) ? 99 : keyValuePair.Value) + appear_add, key, false);
				}
			}
			float num3 = (float)this.LpS.mapx - this.LpS.mapfocx + 0.5f;
			float num4 = (float)this.LpS.mapy - this.LpS.mapfocy + 1f;
			for (int j = currentFile.Astgo.Count - 1; j >= 0; j--)
			{
				SmncStageEditorManager.StgObject stgObject = currentFile.Astgo[j];
				if (TX.isStart(stgObject.key, "_smnc_generate_en", 0))
				{
					ASmnPosL.Add(new SmnPoint(this, (float)stgObject.x + num3, (float)stgObject.y + num4, 1f, null)
					{
						shuffle_ratio = (float)((stgObject.key == "_smnc_generate_en_fix") ? 0 : 1)
					});
				}
			}
		}

		protected override void prepareEnemyEnAttr(List<SmnEnemyKind> AKindL, List<SmnEnemyKind> AKindBuf, int nattr_addable_count)
		{
			SmncFile currentFile = this.LpS.getCurrentFile();
			base.prepareEnemyEnAttr(AKindBuf, AKindL, currentFile.fix_nattr ? 0 : nattr_addable_count);
			if (currentFile.Oen_attr_count.Count > 0)
			{
				AKindBuf.AddRange(AKindL);
				foreach (KeyValuePair<ENATTR, int> keyValuePair in currentFile.Oen_attr_count)
				{
					int num = keyValuePair.Value;
					ENATTR key = keyValuePair.Key;
					if (num != 0)
					{
						bool flag = (key & ENATTR._MATTR) > ENATTR.NORMAL;
						NightController.shuffle<SmnEnemyKind>(AKindBuf, -1);
						int num2 = 0;
						while (num > 0 && num2 < AKindBuf.Count)
						{
							SmnEnemyKind smnEnemyKind = AKindBuf[num2++];
							if ((smnEnemyKind.EnemyDesc.nattr_decline & key) == ENATTR.NORMAL && (!flag || (smnEnemyKind.nattr & ENATTR._MATTR) == ENATTR.NORMAL))
							{
								smnEnemyKind.nattr |= key;
								num--;
							}
						}
					}
				}
			}
		}

		private readonly SummonerForCreator SmnC;

		private readonly M2LpUiSmnCreator LpS;
	}
}

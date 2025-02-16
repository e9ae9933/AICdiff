using System;
using System.Collections.Generic;
using Better;
using m2d;
using XX;

namespace nel
{
	public static class SupplyManager
	{
		public static void clearDate()
		{
			SupplyManager.ReadDate = default(DateTime);
		}

		private static void initScript(bool force = false)
		{
			if (force)
			{
				SupplyManager.ReadDate = default(DateTime);
			}
			ReelManager.initReelScript();
			string resource = TX.getResource("Data/reel_supply", ref SupplyManager.ReadDate, ".csv", false, "Resources/");
			if (resource == null)
			{
				return;
			}
			SupplyManager.OOSupLink = new BDic<string, NDic<SupplyManager.MapSupLink>>();
			NDic<SupplyManager.MapSupLink> ndic = null;
			SupplyManager.OSmnLink = new BDic<string, SupplyManager.SmnSupLink>();
			CsvReaderA csvReaderA = new CsvReaderA(resource, false);
			bool flag = false;
			List<SupplyManager.LpSup> list = new List<SupplyManager.LpSup>(3);
			List<ReelManager.ItemReelContainer> list2 = new List<ReelManager.ItemReelContainer>(3);
			List<ReelManager.ItemReelContainer> list3 = new List<ReelManager.ItemReelContainer>(3);
			SupplyManager.NewSupLink = null;
			SupplyManager.SmnSupLink smnSupLink = null;
			SupplyManager.MapSupLink mapSupLink = null;
			while (csvReaderA.read())
			{
				if (csvReaderA.cmd == "##ENEMY_SECRET_DEFAULT")
				{
					list3.Clear();
					for (int i = 1; i < csvReaderA.clength; i++)
					{
						ReelManager.ItemReelContainer ir = ReelManager.GetIR(csvReaderA.getIndex(i), false, false);
						if (ir != null)
						{
							list3.Add(ir);
						}
					}
					SupplyManager.ARareReelDefault = list3.ToArray();
					list3.Clear();
				}
				else if (csvReaderA.cmd == "##ENEMY_SUMMONER")
				{
					if (smnSupLink != null)
					{
						smnSupLink.finalize(list2, list3);
					}
					smnSupLink = null;
					flag = true;
					ndic = null;
				}
				else if (csvReaderA.cmd == "##MAP")
				{
					if (mapSupLink != null)
					{
						mapSupLink.finalize(list);
					}
					mapSupLink = null;
					flag = false;
					ndic = null;
					if (TX.valid(csvReaderA._1))
					{
						string _ = csvReaderA._1;
						if (!SupplyManager.OOSupLink.TryGetValue(_, out ndic))
						{
							ndic = (SupplyManager.OOSupLink[_] = new NDic<SupplyManager.MapSupLink>("reel_supply_" + _, 0, 0));
						}
					}
					else
					{
						csvReaderA.tError("##MAP には wholemap のtext_key が必要");
					}
				}
				else if (flag)
				{
					if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
					{
						if (smnSupLink != null)
						{
							smnSupLink.finalize(list2, list3);
						}
						string index = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
						smnSupLink = X.Get<string, SupplyManager.SmnSupLink>(SupplyManager.OSmnLink, index);
						if (smnSupLink != null)
						{
							csvReaderA.tError("SupplyManager: 敵リール設定に重複: " + index);
						}
						smnSupLink = (SupplyManager.OSmnLink[index] = new SupplyManager.SmnSupLink());
						list2.Clear();
						list3.Clear();
					}
					else
					{
						int num = 0;
						List<ReelManager.ItemReelContainer> list4 = list2;
						if (csvReaderA.cmd == "%SECRET_REPLACE_TO_LOWER")
						{
							smnSupLink.replace_secret_to_lower = csvReaderA.Nm(1, smnSupLink.replace_secret_to_lower);
						}
						else if (csvReaderA.cmd == "%NOT_APPEAR_FG")
						{
							smnSupLink.not_appear_fg = csvReaderA.Nm(1, 1f) != 0f;
						}
						else
						{
							if (csvReaderA.cmd == "%SECRET")
							{
								num = 1;
								list4 = list3;
							}
							for (int j = num; j < csvReaderA.clength; j++)
							{
								ReelManager.ItemReelContainer ir2 = ReelManager.GetIR(csvReaderA.getIndex(j), true, false);
								if (ir2 == null)
								{
									csvReaderA.tError("不明なreel: " + csvReaderA.getIndex(j));
								}
								else
								{
									list4.Add(ir2);
								}
							}
						}
					}
				}
				else if (ndic != null)
				{
					if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
					{
						if (mapSupLink != null)
						{
							mapSupLink.finalize(list);
						}
						string index2 = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
						mapSupLink = X.Get<string, SupplyManager.MapSupLink>(ndic, index2);
						if (mapSupLink != null)
						{
							csvReaderA.tError("SupplyManager: マップリール設定に重複: " + index2);
						}
						mapSupLink = (ndic[index2] = new SupplyManager.MapSupLink());
						list.Clear();
					}
					else
					{
						SupplyManager.LpSup lpSup = new SupplyManager.LpSup
						{
							Reel = ReelManager.GetIR(csvReaderA.cmd, true, false)
						};
						if (lpSup.Reel == null)
						{
							csvReaderA.tError("不明なreel: " + csvReaderA.cmd);
						}
						else
						{
							lpSup.lp_key = ((csvReaderA.clength >= 2) ? csvReaderA._1 : null);
							list.Add(lpSup);
						}
					}
				}
			}
			if (smnSupLink != null)
			{
				smnSupLink.finalize(list2, list3);
			}
			if (mapSupLink != null)
			{
				mapSupLink.finalize(list);
			}
			foreach (KeyValuePair<string, NDic<SupplyManager.MapSupLink>> keyValuePair in SupplyManager.OOSupLink)
			{
				keyValuePair.Value.scriptFinalize();
			}
			SupplyManager.M2D = M2DBase.Instance as NelM2DBase;
			if (SupplyManager.M2D != null && SupplyManager.M2D.curMap != null)
			{
				SupplyManager.initS(SupplyManager.M2D.curMap);
			}
		}

		public static void initS(Map2d NextMap)
		{
			SupplyManager.M2D = M2DBase.Instance as NelM2DBase;
			SupplyManager.initScript(false);
			SupplyManager.NewSupLink = null;
			NDic<SupplyManager.MapSupLink> ndic;
			if (SupplyManager.M2D.WM.CurWM != null && SupplyManager.OOSupLink.TryGetValue(SupplyManager.M2D.WM.CurWM.text_key, out ndic))
			{
				SupplyManager.NewSupLink = X.Get<string, SupplyManager.MapSupLink>(ndic, NextMap.key);
				if (SupplyManager.NewSupLink != null)
				{
					SupplyManager.NewSupLink.initS();
				}
			}
		}

		public static bool GetForSummoner(string smn_key, out ReelManager.ItemReelContainer[] AReelMain, out ReelManager.ItemReelContainer[] AReelSecret, out float _replace_secret_to_lower, bool no_error = false)
		{
			SupplyManager.initScript(false);
			SupplyManager.SmnSupLink smnSupLink = X.Get<string, SupplyManager.SmnSupLink>(SupplyManager.OSmnLink, smn_key);
			_replace_secret_to_lower = 0.75f;
			if (smnSupLink == null)
			{
				if (!no_error)
				{
					X.de("SupplyManager:: EnemySummoner " + smn_key + " に対するreel情報がありません", null);
				}
				AReelMain = null;
				AReelSecret = SupplyManager.getSecretDefault(smn_key);
				return false;
			}
			AReelMain = smnSupLink.AReel;
			AReelSecret = smnSupLink.AReelSecret ?? SupplyManager.getSecretDefault(smn_key);
			_replace_secret_to_lower = smnSupLink.replace_secret_to_lower;
			return true;
		}

		public static ReelManager.ItemReelContainer[] getSecretDefault(string smn_key)
		{
			return SupplyManager.ARareReelDefault;
		}

		public static bool GetForLpSupplier(string lp_key, out ReelManager.ItemReelContainer Reel)
		{
			Reel = null;
			if (SupplyManager.NewSupLink == null)
			{
				X.de("SupplyManager:: 現在のマップ " + SupplyManager.M2D.curMap.key + " に対するreel情報がありません", null);
				return false;
			}
			return SupplyManager.NewSupLink.GetForLpSupplier(lp_key, out Reel);
		}

		public static SupplyManager.SupplyDescription listupForIR(NelM2DBase M2D, WholeMapItem WMTarget, ReelManager.ItemReelContainer IR, SupplyManager.SupplyDescription A = default(SupplyManager.SupplyDescription), bool only_known = true)
		{
			A.IR = IR;
			NDic<SupplyManager.MapSupLink> ndic;
			if (SupplyManager.OOSupLink.TryGetValue(WMTarget.text_key, out ndic))
			{
				foreach (KeyValuePair<string, SupplyManager.MapSupLink> keyValuePair in ndic)
				{
					if (keyValuePair.Value.isContains(IR))
					{
						A.AddPos(M2D, keyValuePair.Key, WMTarget);
					}
				}
			}
			if (IR.useableItem)
			{
				using (BList<string> blist = ListBuffer<string>.Pop(0))
				{
					foreach (KeyValuePair<string, SupplyManager.SmnSupLink> keyValuePair2 in SupplyManager.OSmnLink)
					{
						if (keyValuePair2.Value.isContains(IR))
						{
							blist.Add(keyValuePair2.Key);
						}
					}
					A = SupplyManager.AddSummoner(M2D, WMTarget, blist, A, true);
				}
			}
			return A;
		}

		public static SupplyManager.SupplyDescription AddSummoner(NelM2DBase M2D, WholeMapItem WMTarget, string smn_key, SupplyManager.SupplyDescription A = default(SupplyManager.SupplyDescription), bool only_icon = true)
		{
			SupplyManager.SupplyDescription supplyDescription;
			using (BList<string> blist = ListBuffer<string>.Pop(1))
			{
				blist.Add(smn_key);
				supplyDescription = SupplyManager.AddSummoner(M2D, WMTarget, blist, A, only_icon);
			}
			return supplyDescription;
		}

		public static SupplyManager.SupplyDescription AddSummoner(NelM2DBase M2D, WholeMapItem WMTarget, List<string> Asmn_keys, SupplyManager.SupplyDescription A = default(SupplyManager.SupplyDescription), bool only_icon = true)
		{
			if (Asmn_keys.Count > 0)
			{
				EnemySummonerManager manager = EnemySummonerManager.GetManager(WMTarget.text_key);
				if (manager != null)
				{
					for (int i = Asmn_keys.Count - 1; i >= 0; i--)
					{
						string text = Asmn_keys[i];
						A.AddSmn(M2D, text, WMTarget, manager);
					}
				}
			}
			return A;
		}

		public static BDic<string, FDSummonerInfo> getFDSummonerInfoData(NelM2DBase M2D)
		{
			BDic<string, FDSummonerInfo> bdic = new BDic<string, FDSummonerInfo>(SupplyManager.OSmnLink.Count);
			foreach (KeyValuePair<string, SupplyManager.SmnSupLink> keyValuePair in SupplyManager.OSmnLink)
			{
				if (!keyValuePair.Value.not_appear_fg)
				{
					bdic[keyValuePair.Key] = new FDSummonerInfo(keyValuePair.Key, keyValuePair.Value);
				}
			}
			foreach (KeyValuePair<string, WholeMapItem> keyValuePair2 in M2D.WM.getWholeMapDescriptionObject())
			{
				foreach (KeyValuePair<Map2d, List<WMIcon>> keyValuePair3 in keyValuePair2.Value.getNoticedIconObject())
				{
					int count = keyValuePair3.Value.Count;
					for (int i = 0; i < count; i++)
					{
						WMIcon wmicon = keyValuePair3.Value[i];
						if (wmicon.type == WMIcon.TYPE.ENEMY && wmicon.sf_key != null)
						{
							int num = wmicon.sf_key.IndexOf("..");
							if (num >= 0)
							{
								string text = TX.slice(wmicon.sf_key, num + 2);
								FDSummonerInfo fdsummonerInfo;
								if (bdic.TryGetValue(text, out fdsummonerInfo))
								{
									fdsummonerInfo.check(M2D, wmicon.sf_key);
								}
							}
						}
					}
				}
			}
			return bdic;
		}

		public static SupplyManager.ItemDescriptorForWholeMap createWholeMapDescriptor(NelM2DBase M2D, string wm_text_key)
		{
			BDic<string, EnemySummonerManager.SDescription> bdic = EnemySummonerManager.GetManager(wm_text_key).listupAll();
			SupplyManager.ItemDescriptorForWholeMap itemDescriptorForWholeMap = new SupplyManager.ItemDescriptorForWholeMap
			{
				Asummoner = new List<string>(bdic.Count),
				AAReel = new List<ReelManager.ItemReelContainer[]>(bdic.Count)
			};
			int num = 0;
			foreach (KeyValuePair<string, EnemySummonerManager.SDescription> keyValuePair in bdic)
			{
				ReelManager.ItemReelContainer[] array;
				ReelManager.ItemReelContainer[] array2;
				float num2;
				if (SupplyManager.GetForSummoner(keyValuePair.Key, out array, out array2, out num2, keyValuePair.Key.IndexOf("debug") >= 0))
				{
					itemDescriptorForWholeMap.Asummoner.Add(keyValuePair.Key);
					itemDescriptorForWholeMap.AAReel.Add(array);
					for (int i = array.Length - 1; i >= 0; i--)
					{
						num += array[i].Count;
					}
				}
			}
			NDic<SupplyManager.MapSupLink> ndic;
			int num3;
			if (SupplyManager.OOSupLink.TryGetValue(wm_text_key, out ndic))
			{
				foreach (KeyValuePair<string, SupplyManager.MapSupLink> keyValuePair2 in ndic)
				{
					num3 = keyValuePair2.Value.AReel.Length;
					ReelManager.ItemReelContainer[] array3 = new ReelManager.ItemReelContainer[num3];
					for (int j = 0; j < num3; j++)
					{
						ReelManager.ItemReelContainer itemReelContainer = (array3[j] = keyValuePair2.Value.AReel[j].Reel);
						num += itemReelContainer.Count;
					}
					itemDescriptorForWholeMap.AAReel.Add(array3);
				}
			}
			itemDescriptorForWholeMap.AAItem = new List<List<NelItemEntry>>(num);
			num3 = itemDescriptorForWholeMap.AAReel.Count;
			for (int k = 0; k < num3; k++)
			{
				ReelManager.ItemReelContainer[] array4 = itemDescriptorForWholeMap.AAReel[k];
				for (int l = array4.Length - 1; l >= 0; l--)
				{
					ReelManager.ItemReelContainer itemReelContainer2 = array4[l];
					for (int m = itemReelContainer2.Count - 1; m >= 0; m--)
					{
						NelItemEntry nelItemEntry = itemReelContainer2[m];
						for (int n = itemDescriptorForWholeMap.AAItem.Count - 1; n >= 0; n--)
						{
							if (itemDescriptorForWholeMap.AAItem[n][0].Data == nelItemEntry.Data)
							{
								itemDescriptorForWholeMap.AAItem[n].Add(nelItemEntry);
								nelItemEntry = null;
								break;
							}
						}
						if (nelItemEntry != null)
						{
							List<NelItemEntry> list = new List<NelItemEntry>(1);
							list.Add(nelItemEntry);
							itemDescriptorForWholeMap.AAItem.Add(list);
						}
					}
				}
			}
			itemDescriptorForWholeMap.AAItem.Sort((List<NelItemEntry> A, List<NelItemEntry> B) => B.Count - A.Count);
			return itemDescriptorForWholeMap;
		}

		public static NelM2DBase M2D;

		private const string data_file = "Data/reel_supply";

		private static DateTime ReadDate;

		private const float replace_secret_to_lower_default = 0.75f;

		private static BDic<string, NDic<SupplyManager.MapSupLink>> OOSupLink;

		private static BDic<string, SupplyManager.SmnSupLink> OSmnLink;

		public static ReelManager.ItemReelContainer[] ARareReelDefault;

		private static SupplyManager.MapSupLink NewSupLink;

		private struct LpSup
		{
			public string lp_key;

			public ReelManager.ItemReelContainer Reel;
		}

		public struct ItemDescriptorForWholeMap
		{
			public bool valid
			{
				get
				{
					return this.Asummoner != null;
				}
			}

			public List<string> Asummoner;

			public List<ReelManager.ItemReelContainer[]> AAReel;

			public List<List<NelItemEntry>> AAItem;
		}

		public class SmnSupLink
		{
			public void finalize(List<ReelManager.ItemReelContainer> _AReel, List<ReelManager.ItemReelContainer> _AReelSecret)
			{
				this.AReel = _AReel.ToArray();
				if (_AReelSecret.Count > 0)
				{
					this.AReelSecret = _AReelSecret.ToArray();
					_AReelSecret.Clear();
				}
				_AReel.Clear();
			}

			public bool isContains(ReelManager.ItemReelContainer IR)
			{
				return (this.AReel != null && X.isinC<ReelManager.ItemReelContainer>(this.AReel, IR) >= 0) || (this.AReelSecret != null && X.isinC<ReelManager.ItemReelContainer>(this.AReelSecret, IR) >= 0);
			}

			public bool valid
			{
				get
				{
					return (this.AReel != null && this.AReel.Length != 0) || (this.AReelSecret != null && this.AReelSecret.Length != 0);
				}
			}

			public ReelManager.ItemReelContainer[] AReel;

			public ReelManager.ItemReelContainer[] AReelSecret;

			public bool not_appear_fg;

			public float replace_secret_to_lower = 0.75f;
		}

		private class MapSupLink
		{
			public void finalize(List<SupplyManager.LpSup> AReelBuf)
			{
				this.AReel = AReelBuf.ToArray();
				AReelBuf.Clear();
			}

			public void initS()
			{
				this.index = 0;
			}

			public bool GetForLpSupplier(string lp_key, out ReelManager.ItemReelContainer ReelOut)
			{
				int num = this.AReel.Length;
				if (num == 0)
				{
					ReelOut = null;
					return false;
				}
				for (int i = 0; i < num; i++)
				{
					SupplyManager.LpSup lpSup = this.AReel[i];
					if (lpSup.lp_key == lp_key)
					{
						ReelOut = lpSup.Reel;
						this.index = (byte)((int)this.index | (1 << i));
						return true;
					}
				}
				for (int j = 0; j < num; j++)
				{
					if (((int)this.index & (1 << j)) == 0)
					{
						ReelOut = this.AReel[j].Reel;
						this.index = (byte)((int)this.index | (1 << j));
						return true;
					}
				}
				this.index = 0;
				return this.GetForLpSupplier(lp_key, out ReelOut);
			}

			public bool isContains(ReelManager.ItemReelContainer IR)
			{
				for (int i = this.AReel.Length - 1; i >= 0; i--)
				{
					if (this.AReel[i].Reel == IR)
					{
						return true;
					}
				}
				return false;
			}

			public SupplyManager.LpSup[] AReel;

			public byte index;
		}

		public struct SupplyDescription
		{
			public SupplyDescription(ReelManager.ItemReelContainer _IR)
			{
				this.IR = _IR;
				this.APosSpl = null;
				this.APosSmn = null;
			}

			public void Clear()
			{
				this.IR = null;
				if (this.APosSpl != null)
				{
					this.APosSpl.Clear();
				}
				if (this.APosSmn != null)
				{
					this.APosSmn.Clear();
				}
			}

			public SupplyManager.SupplyDescription AddPos(NelM2DBase M2D, string map_key, WholeMapItem WMTarget = null)
			{
				Map2d map2d = M2D.Get(map_key, false);
				if (map2d == null)
				{
					return this;
				}
				WholeMapItem wholeFor = M2D.WM.GetWholeFor(map2d, false);
				if (wholeFor == null || (WMTarget != null && WMTarget != wholeFor))
				{
					return this;
				}
				WmPosition posCache = new WmDeperture(wholeFor.text_key, map_key).getPosCache(wholeFor);
				if (!posCache.Wmi.visitted)
				{
					return this;
				}
				if (this.APosSpl == null)
				{
					this.APosSpl = new List<WmPosition>();
				}
				this.APosSpl.Add(posCache);
				return this;
			}

			public SupplyManager.SupplyDescription AddSmn(NelM2DBase M2D, WMIcon _Ico, WholeMapItem WMTarget)
			{
				if (this.APosSmn == null)
				{
					this.APosSmn = new List<WMIconPosition>();
				}
				int num = _Ico.sf_key.IndexOf("..");
				if (num == -1)
				{
					return this;
				}
				Map2d map2d = M2D.Get(TX.slice(_Ico.sf_key, 0, num), true);
				if (map2d == null)
				{
					return this;
				}
				WholeMapItem.WMItem wmitem = null;
				WMSpecialIcon wmspecialIcon = default(WMSpecialIcon);
				WMTarget.GetWMItem(map2d.key, ref wmitem, ref wmspecialIcon);
				if (wmitem != null)
				{
					this.APosSmn.Add(new WMIconPosition(_Ico, wmitem, default(WMIconHiddenDeperture)));
				}
				return this;
			}

			public SupplyManager.SupplyDescription AddSmn(NelM2DBase M2D, string smn_key, WholeMapItem WMTarget, EnemySummonerManager SMN = null)
			{
				if (SMN == null)
				{
					SMN = EnemySummonerManager.GetManager(WMTarget.text_key);
					if (SMN == null)
					{
						return this;
					}
				}
				WMIconPosition wmiconPosition;
				if (!SMN.getWMPosition(M2D, smn_key, WMTarget, out wmiconPosition, true, false))
				{
					return this;
				}
				if (this.APosSmn == null)
				{
					this.APosSmn = new List<WMIconPosition>();
				}
				this.APosSmn.Add(wmiconPosition);
				return this;
			}

			public SupplyManager.SupplyDescription AddSmn(WMIconPosition Pos)
			{
				if (this.APosSmn == null)
				{
					this.APosSmn = new List<WMIconPosition>();
				}
				this.APosSmn.Add(Pos);
				return this;
			}

			public ReelManager.ItemReelContainer IR;

			public List<WmPosition> APosSpl;

			public List<WMIconPosition> APosSmn;
		}

		public class SupplyDescriptionMulti
		{
			public SupplyDescriptionMulti()
			{
				this.OAPosSpl = new BDic<WmPosition, List<ReelManager.ItemReelContainer>>();
				this.OAPosSmn = new BDic<WMIconPosition, List<ReelManager.ItemReelContainer>>();
			}

			public void Clear()
			{
				if (this.OAPosSpl != null)
				{
					this.OAPosSpl.Clear();
				}
				if (this.OAPosSmn != null)
				{
					this.OAPosSmn.Clear();
				}
				this.is_summoner_target = false;
			}

			public void Merge(SupplyManager.SupplyDescription Src)
			{
				if (Src.APosSpl != null && Src.APosSpl.Count > 0)
				{
					int count = Src.APosSpl.Count;
					for (int i = 0; i < count; i++)
					{
						WmPosition wmPosition = Src.APosSpl[i];
						bool flag = true;
						foreach (KeyValuePair<WmPosition, List<ReelManager.ItemReelContainer>> keyValuePair in this.OAPosSpl)
						{
							if (keyValuePair.Key.isSame(wmPosition))
							{
								X.pushIdentical<ReelManager.ItemReelContainer>(keyValuePair.Value, Src.IR);
								flag = false;
								break;
							}
						}
						if (flag)
						{
							(this.OAPosSpl[wmPosition] = new List<ReelManager.ItemReelContainer>(1)).Add(Src.IR);
						}
					}
				}
				if (Src.APosSmn != null && Src.APosSmn.Count > 0)
				{
					int count2 = Src.APosSmn.Count;
					for (int j = 0; j < count2; j++)
					{
						WMIconPosition wmiconPosition = Src.APosSmn[j];
						bool flag2 = true;
						foreach (KeyValuePair<WMIconPosition, List<ReelManager.ItemReelContainer>> keyValuePair2 in this.OAPosSmn)
						{
							if (keyValuePair2.Key.isSame(wmiconPosition))
							{
								X.pushIdentical<ReelManager.ItemReelContainer>(keyValuePair2.Value, Src.IR);
								flag2 = false;
								break;
							}
						}
						if (flag2)
						{
							(this.OAPosSmn[wmiconPosition] = new List<ReelManager.ItemReelContainer>(1)).Add(Src.IR);
						}
					}
				}
			}

			public void Merge(ReelManager.ReelDecription RDesc, BDic<ReelManager.ItemReelContainer, SupplyManager.SupplyDescription> OIRDescBuffer, WholeMapItem _WM)
			{
				if (RDesc.AIR_useable != null)
				{
					int count = RDesc.AIR_useable.Count;
					for (int i = 0; i < count; i++)
					{
						ReelManager.ItemReelContainer itemReelContainer = RDesc.AIR_useable[i];
						if (itemReelContainer.GReelItem.obtain_count > 0 && !OIRDescBuffer.ContainsKey(itemReelContainer))
						{
							SupplyManager.SupplyDescription supplyDescription = (OIRDescBuffer[itemReelContainer] = SupplyManager.listupForIR(SupplyManager.M2D, _WM, itemReelContainer, default(SupplyManager.SupplyDescription), true));
							this.Merge(supplyDescription);
						}
					}
				}
				if (RDesc.AIR_supplier != null)
				{
					int count2 = RDesc.AIR_supplier.Count;
					for (int j = 0; j < count2; j++)
					{
						ReelManager.ItemReelContainer itemReelContainer2 = RDesc.AIR_supplier[j];
						if (!OIRDescBuffer.ContainsKey(itemReelContainer2))
						{
							SupplyManager.SupplyDescription supplyDescription2 = (OIRDescBuffer[itemReelContainer2] = SupplyManager.listupForIR(SupplyManager.M2D, _WM, itemReelContainer2, default(SupplyManager.SupplyDescription), true));
							this.Merge(supplyDescription2);
						}
					}
				}
			}

			public void pickupFocusPosition(WholeMapItem Wmi, List<WmPosition> APos)
			{
				foreach (KeyValuePair<WmPosition, List<ReelManager.ItemReelContainer>> keyValuePair in this.OAPosSpl)
				{
					APos.Add(keyValuePair.Key);
				}
				foreach (KeyValuePair<WMIconPosition, List<ReelManager.ItemReelContainer>> keyValuePair2 in this.OAPosSmn)
				{
					WmPosition depertWmPos = keyValuePair2.Key.getDepertWmPos(Wmi);
					if (depertWmPos.valid2)
					{
						APos.Add(depertWmPos);
					}
				}
			}

			public bool valid
			{
				get
				{
					return this.OAPosSmn.Count > 0 || this.OAPosSpl.Count > 0;
				}
			}

			public readonly BDic<WmPosition, List<ReelManager.ItemReelContainer>> OAPosSpl;

			public readonly BDic<WMIconPosition, List<ReelManager.ItemReelContainer>> OAPosSmn;

			public bool is_summoner_target;
		}
	}
}

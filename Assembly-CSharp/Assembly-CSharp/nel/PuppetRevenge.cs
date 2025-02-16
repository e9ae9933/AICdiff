using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public static class PuppetRevenge
	{
		public static void defeatShopper(NelEnemy En)
		{
			uint num = GF.getC("PUP_KILL") / 2U;
			GF.setC("PUP_KILL", num * 2U + 1U);
			GF.setC("PUP_BUYR", 15U);
			GF.setC("PUP2", GF.getC("PUP2") & 4294967292U);
			BGM.stop(true, true);
			BGM.fadein(100f, 1160f);
			PostEffect.IT.setPEfadeinout(POSTM.JAMMING, 50f, 5f, 0.7f, -50);
			CoinStorage.Clear(CoinStorage.CTYPE.CRAFTS);
			StoreManager storeManager = StoreManager.Get("Puppet", false);
			using (BList<NelItemEntry> blist = ListBuffer<NelItemEntry>.Pop(0))
			{
				int num2 = 2 + X.Mx(0, -1 + NightController.xors(3));
				int num3 = 2;
				float num4 = 0.15f;
				if (num >= 1U)
				{
					num2 += NightController.xors((int)X.Mn(4U, num + 1U));
					num4 = X.NIL(0.15f, 0.25f, (num - 2U) / 5f, 1f);
				}
				storeManager.stealItems(blist, num2, num3, num4);
				int count = blist.Count;
				for (int i = 0; i < count; i++)
				{
					NelItemEntry nelItemEntry = blist[i];
					float num5 = X.NIXP(0.05f, 0.15f);
					float num6 = X.XORSPS() * 1.5707964f + 3.1415927f;
					En.nM2D.IMNG.dropManual(nelItemEntry.Data, nelItemEntry.count, (int)nelItemEntry.grade, En.x, En.y, num5 * X.Cos(num6), -num5 * X.Sin(num6), null, false, NelItemManager.TYPE.NORMAL).discarded = true;
				}
			}
			En.nM2D.NightCon.clearPuppetRevenge();
		}

		public static bool first_meet
		{
			get
			{
				return GF.getC("PUP0") == 0U;
			}
		}

		public static bool isRevengeEnabled(NelM2DBase M2D, EnemySummoner Smn, M2LpSummon Lp)
		{
			NightController.SummonerData info = Lp.getInfo();
			if (M2D == null || info == null || !SCN.isPuppetWNpcDefeated() || (!info.summoner_is_night && !M2D.NightCon.alreadyCleardAtLeastOnce(Smn, Lp.Mp)))
			{
				return false;
			}
			ItemStorage inventory = M2D.IMNG.getInventory();
			return (inventory.getVisibleRowCount() >= 8 || inventory.getItemCountFn(NelItem.isTreasureBox, null) >= 4) && !M2D.NightCon.isLockPuppetRevenge(Lp.Mp.key);
		}

		public static void progressRevengeBattlePhase()
		{
			GF.setC("PUP_KILL_BP", GF.getC("PUP_KILL_BP") + 1U);
		}

		public static int getRevengeBattlePhase()
		{
			return (int)GF.getC("PUP_KILL_BP");
		}

		public static void losedInRevengeBattle(NelM2DBase M2D)
		{
			uint num = GF.getC("PUP_KILL") / 2U;
			GF.setC("PUP_KILL", X.Mn(14U, num * 2U + 2U));
			GF.setC("PUP_KILL_BP", X.Mn(GF.getC("PUP_KILL_BP"), num));
			M2D.WDR.Get(WanderingManager.TYPE.PUP).appear_ratio = 0.95f;
			M2D.NightCon.clearPuppetRevenge();
		}

		public static bool losedInRevengeBattleAfter(NelM2DBase M2D)
		{
			ItemStorage inventory = M2D.IMNG.getInventory();
			if (inventory.getWholeRowCount() == 0)
			{
				return false;
			}
			uint num = GF.getC("PUP_KILL") / 2U;
			int num2 = X.MMX(1, X.IntC((float)inventory.getWholeRowCount() * 0.13f), (int)(2U + num));
			StoreManager storeManager = StoreManager.Get("Puppet", false);
			using (BList<StoreManager.StoreEntry> blist = ListBuffer<StoreManager.StoreEntry>.Pop(0))
			{
				storeManager.listUpByLineKey(blist, "STOLEN");
				int num3 = X.Mx(0, 8 - num2);
				if (blist.Count > num3)
				{
					NightController.shuffle<StoreManager.StoreEntry>(blist, -1);
					for (int i = blist.Count - 1; i >= num3; i--)
					{
						storeManager.removeSpecificEntry(blist[i]);
					}
				}
			}
			int[] array = X.makeCountUpArray(inventory.getWholeRowCount(), 0, 1);
			NightController.shuffle<int>(array, -1);
			int num4 = 0;
			while (num4 < inventory.getWholeRowCount() && num4 < num2)
			{
				ItemStorage.IRow rowByIndex = inventory.getRowByIndex(array[num4]);
				if (PuppetRevenge.isCannotSteal(rowByIndex))
				{
					num2++;
				}
				else
				{
					ItemStorage.ObtainInfo info = rowByIndex.Info;
					int num5 = X.Mn(rowByIndex.Data.stock, rowByIndex.Info.total);
					for (int j = 4; j >= 0; j--)
					{
						int num6 = X.Mn(num5, rowByIndex.Info.getCount(j));
						if (num6 > 0)
						{
							num5 -= num6;
							inventory.Reduce(rowByIndex.Data, num6, j, false);
							storeManager.Add(rowByIndex.Data, num6, j, "STOLEN", true);
							if (num5 <= 0)
							{
								break;
							}
						}
					}
				}
				num4++;
			}
			inventory.fineRows(false);
			return true;
		}

		public static float WdrOverrideCalcableLength(float def_len)
		{
			if (GF.getC("PUP_BUYR") <= 4U)
			{
				return def_len;
			}
			return 30f;
		}

		public static void FD_StoreGetServiceRatio(StoreManager Store, ItemStorage St, ref float buy_ratio, out string tx_key_buy, ref float sell_ratio, out string tx_key_sell)
		{
			tx_key_sell = null;
			tx_key_buy = null;
			int c = (int)GF.getC("PUP_BUYR");
			if (c > 0)
			{
				tx_key_buy = "Store_service_buy_puppet_penalty";
				buy_ratio = X.NIL(1f, 1.5f, (float)c, 4f);
			}
		}

		public static bool efDraw(EffectItem Ef)
		{
			uint num = X.GETRAN2(Ef.f0, (int)((Ef.index & 7U) + 7U));
			uint num2 = num;
			M2DBase instance = M2DBase.Instance;
			Map2d curMap = instance.curMap;
			if (curMap == null)
			{
				return false;
			}
			if (PuppetRevenge.EpHalo == null)
			{
				PuppetRevenge.EpHalo = new EfParticleOnce("summon_activate_sudden_puppetrevenge_halo", EFCON_TYPE.UI);
			}
			Ef.x = M2DBase.Instance.Cam.x * curMap.rCLEN;
			Ef.y = M2DBase.Instance.Cam.y * curMap.rCLEN;
			bool flag = false;
			int num3 = 3;
			int num4 = 7 + (int)(X.RAN(num, 2745) * 3f) + num3;
			MeshDrawer meshDrawer = null;
			MeshDrawer meshDrawer2 = null;
			MeshDrawer meshDrawer3 = null;
			float num5 = -(IN.hh + 26f) * instance.Cam.getScaleRev() * 0.015625f;
			float num6 = 0f;
			bool flag4;
			using (BList<float> blist = ListBuffer<float>.Pop(num4))
			{
				for (int i = 0; i < num4; i++)
				{
					num = X.GETRAN2((int)(num & 255U), (int)((num & 15U) + 7U));
					bool flag2 = false;
					float num7 = 0f;
					float num8;
					float num9;
					float num10;
					if (i < num3)
					{
						flag2 = true;
						if (i == 0)
						{
							num8 = X.NI(1.05f, 1.18f, X.RAN(num, 2003));
							num9 = X.NI(9, 15, X.RAN(num, 1314));
							num10 = X.NI(-1, 1, X.RAN(num, 1494)) * 50f;
						}
						else
						{
							num7 = 20f + 8f * (float)i + X.RAN(num, 1425) * 18f;
							num8 = X.NI(0.76f, 0.96f, X.RAN(num, 3131));
							num9 = X.NI(25, 35, X.RAN(num, 1314));
							num10 = X.NI(200, 350, X.RAN(num, 1494)) * (float)X.MPF((long)(i % 2) == (long)((ulong)(num2 % 2U)));
						}
						blist.Add(num10);
					}
					else
					{
						num7 = 8f * (float)i * 0.7f + X.RAN(num, 1425) * 12f;
						num8 = X.NI(0.5f, 0.9f, X.RAN(num, 1988));
						num9 = X.NI(20, 40, X.RAN(num, 1314));
						num10 = X.NI(180, 480, X.RAN(num, 1494)) * (float)X.MPF((long)(i % 2) == (long)((ulong)(num2 % 2U)));
					}
					float num11 = Ef.af - num7;
					if (num11 < 0f)
					{
						flag = true;
					}
					else
					{
						float num12 = X.NI(230, 270, X.RAN(num, 1338)) - num7;
						if (num11 < num12)
						{
							if (!flag2)
							{
								float num13 = X.NI(30, 75, X.RAN(num, 2985));
								for (int j = 0; j < 15; j++)
								{
									if (num10 >= 500f)
									{
										num10 -= 1000f;
									}
									bool flag3 = false;
									float num14 = 115f * num8 * 9f / (float)(9 + j);
									for (int k = blist.Count - 1; k >= 0; k--)
									{
										if (X.Abs(blist[k] - num10) < num14 + (float)((k < num3) ? 40 : 0))
										{
											flag3 = true;
											break;
										}
									}
									if (flag3)
									{
										num10 += num13;
									}
								}
								blist.Add(num10);
							}
							flag = true;
							num8 *= 0.09375f * instance.Cam.getScaleRev();
							num10 *= 0.015625f * instance.Cam.getScaleRev();
							float num15 = (float)(flag2 ? 43 : 19) * num8;
							float num16 = (float)(flag2 ? 113 : 50) * num8;
							float num17 = (float)(flag2 ? 58 : 27) * num8;
							float num18 = (flag2 ? 14.4f : 4.5f) * num8;
							float num19 = num16 - num18 - num17;
							float num20 = num15 * 0.5f;
							float num21 = X.ZSIN(num11, num9);
							float num22 = ((num21 >= 1f) ? 0f : (num17 / num16));
							float num23 = ((num21 >= 1f) ? 0f : ((num17 + num18) / num16));
							if (meshDrawer == null)
							{
								meshDrawer = Ef.GetMesh("puprev", MTR.MtrNDSub2, false);
								meshDrawer.base_y += num5;
							}
							meshDrawer.base_x = instance.Cam.PosMainTransform.x + num10;
							int num24 = 4;
							meshDrawer.TriRectBL(0);
							if (num21 >= num22)
							{
								meshDrawer.TriRectBL(1, 4, 5, 2);
								num24 += 2;
								if (num21 >= num23)
								{
									meshDrawer.TriRectBL(4, 6, 7, 5);
									num24 += 2;
								}
							}
							meshDrawer.allocUv23(num24, false);
							meshDrawer.Uv2(X.RAN(num, 995) + X.ANMP((int)curMap.floort, (int)X.NI(100, 140, X.RAN(num, 1143)), 1f), X.RAN(num, 769) + X.ANMP((int)curMap.floort, (int)X.NI(180, 260, X.RAN(num, 2283)), 1f), false);
							meshDrawer.Col = C32.d2c(4284762441U);
							int vertexMax = meshDrawer.getVertexMax();
							if (num21 < num22)
							{
								meshDrawer.RectBL(-num20, 0f, num15, num17 * num21 / num22, true);
							}
							else
							{
								meshDrawer.RectBL(-num20, 0f, num15, num17, true);
								if (num21 < num23)
								{
									float num25 = (num21 - num22) / (num23 - num22);
									meshDrawer.Pos(-num20 + num25 * num18, num17 + num25 * num18, null).Pos(num20 - num25 * num18, num17 + num25 * num18, null);
								}
								else
								{
									meshDrawer.Pos(-num20 + num18, num17 + num18, null).Pos(num20 - num18, num17 + num18, null);
									float num26 = X.ZLINE(num21 - num23, 1f - num23);
									meshDrawer.Pos(-num20 + num18, num17 + num18 + num19 * num26, null).Pos(num20 - num18, num17 + num18 + num19 * num26, null);
								}
							}
							meshDrawer.allocUv2(0, true);
							float num27 = X.NI(-0.5f, -0.25f, X.RAN(num, 1567));
							float num28 = X.NI(60, 90, X.RAN(num, 3534));
							float num29 = X.ZSIN(num11 - 12f, 20f) * X.Mn(1f, (num12 - num11) / num28);
							float num30 = X.ZSIN(num11 + 4f, 11f) * X.Mn(1f, (num12 - num11 - 45f) / num28);
							Vector3[] vertexArray = meshDrawer.getVertexArray();
							for (int l = vertexMax; l < meshDrawer.getVertexMax(); l++)
							{
								float num31 = X.ZLINE(vertexArray[l].y - meshDrawer.base_y, num16);
								num31 = X.NIL(num30, num29, num31, 1f);
								meshDrawer.Uv3(num27, num31, false);
							}
							int num32 = (flag2 ? 3 : 2);
							float num33 = (num15 - num18 * 2f) * X.NI(0.5f, 0.63f, X.RAN(num, 2528));
							float num34 = num19 - num33 * 2f;
							float num35 = (num15 - num18 * 2f) * 0.14f;
							float num36 = (num15 - num18 * 2f) * 0.68f;
							float num37 = (flag2 ? 1.5f : 1f) * num36;
							uint num38 = num;
							float num39 = X.ZLINE(num12 - num11, 50f);
							float num40 = (flag2 ? 1f : 0.45f);
							for (int m = 0; m < num32; m++)
							{
								num = X.GETRAN2((int)((num & 255U) + 31U), (int)((num & 15U) + 7U));
								float num41 = num11 - num9 - X.NI(60, 85, X.RAN(num, 2260));
								if (num41 > 0f)
								{
									if (meshDrawer2 == null)
									{
										meshDrawer2 = Ef.GetMesh("puprev", MTRX.MtrMeshAdd, false);
										meshDrawer3 = Ef.GetMeshImg("puprev", MTRX.MIicon, BLEND.ADD, false);
										meshDrawer2.base_y += num5;
										meshDrawer3.base_y += num5;
									}
									meshDrawer2.base_x = meshDrawer.base_x;
									meshDrawer3.base_x = meshDrawer.base_x;
									if (num6 == 0f)
									{
										num6 = (float)((int)curMap.floort / 5 * 5);
									}
									float num42 = 0.028125f;
									float num43 = X.ZLINE(num41, X.NI(90, 150, X.RAN(num, 2374)));
									float num44 = num17 + num18 + num33 + num34 * (float)m / (float)(num32 - 1);
									float num45 = num42 * X.COSI(num6, X.NI(2.2f, 3.3f, X.RAN(num, 2272))) + num42 * X.COSI(num6, X.NI(5.8f, 11.7f, X.RAN(num, 2465)));
									float num46 = num42 * X.COSI(num6 + 98f, X.NI(2.2f, 3.3f, X.RAN(num, 2384))) + num42 * X.COSI(num6 + 337f, X.NI(5.8f, 11.7f, X.RAN(num, 1098))) + num44;
									meshDrawer2.Col = meshDrawer2.ColGrd.Set(4294923131U).blend(4289331240U, 0.5f + 0.5f * X.COSI(num41 + 3900f + (float)(m * 30), X.NI(9.4f, 13.3f, X.RAN(num, 1605)))).mulA(num39)
										.C;
									meshDrawer3.Col = meshDrawer2.Col;
									meshDrawer2.Daia3(num45, num46, num36, num37, num35 * 1.15f, num35 * 1.15f, true);
									num45 = num42 * X.COSI(num6, X.NI(2.5f, 4.1f, X.RAN(num, 4912))) + num42 * X.COSI(num6, X.NI(6.2f, 13.7f, X.RAN(num, 1765)));
									num46 = num42 * X.COSI(num6 + 98f, X.NI(2.5f, 4.1f, X.RAN(num, 2513))) + num42 * X.COSI(num6 + 337f, X.NI(6.2f, 13.7f, X.RAN(num, 3974))) + num44;
									meshDrawer2.Col = meshDrawer2.ColGrd.Set(4294961133U).blend(4294914918U, X.Mx(0f, -1.25f + 2.25f * X.COSI(num41 + 3900f + (float)(m * 24), X.NI(7.4f, 5.3f, X.RAN(num, 1333))))).mulA(num39)
										.C;
									meshDrawer2.Daia3(num45, num46, num36, num37, num35, num35, true);
									num45 = num42 * X.COSI(num6, X.NI(2.2f, 3.3f, X.RAN(num, 3272))) + num42 * X.COSI(num6, X.NI(5.8f, 11.7f, X.RAN(num, 1465)));
									num46 = num42 * X.COSI(num6 + 98f, X.NI(2.2f, 3.3f, X.RAN(num, 1984))) + num42 * X.COSI(num6 + 337f, X.NI(5.8f, 11.7f, X.RAN(num, 1598))) + num44;
									meshDrawer3.initForImg(MTRX.EffBlurCircle245, 0);
									float num47 = X.NI(70, 80, X.RAN(num, 1314)) * (flag2 ? 1f : 0.7f) * 0.015625f;
									meshDrawer3.Rect(num45, num46, num47, num47, true);
									if (num43 > 0f && num43 < 1f)
									{
										PuppetRevenge.EpHalo.drawTo(meshDrawer2, meshDrawer2.base_px_x, meshDrawer2.base_px_y + num44 * 64f, 0f, (int)(X.NI(90, 120, X.RAN(num, 2797)) * num40), PuppetRevenge.EpHalo.maxt_one * X.Scr(num43, 1f - num39) * 4f, 0f);
										float num48 = X.Scr(X.ZLINE(num43 - 0.13f, 0.87f), 1f - num39);
										if (num48 > 0f)
										{
											PuppetRevenge.EpHalo.drawTo(meshDrawer2, meshDrawer2.base_px_x, meshDrawer2.base_px_y + num44 * 64f, 1.5707964f, (int)(X.NI(180, 200, X.RAN(num, 2797)) * num40), PuppetRevenge.EpHalo.maxt_one * num48, 0f);
										}
									}
								}
							}
							num = num38;
						}
					}
				}
				flag4 = flag;
			}
			return flag4;
		}

		public const int override_summoner_grade = 4;

		public const int buyr_max = 4;

		private const string stolen_line_key = "STOLEN";

		private const int stolen_item_max = 8;

		public const int summoner_drop_od_enemy_box_max = 3;

		public static ItemStorage.FnCheckItemDataAndInfo isCannotSteal = delegate(ItemStorage.IRow IRow)
		{
			NelItem data = IRow.Data;
			return data.is_food || data.key == "mtr_water0" || data.key == "rotten_food" || (IRow.has_wlink && !data.is_water);
		};

		public static EfParticleOnce EpHalo;
	}
}

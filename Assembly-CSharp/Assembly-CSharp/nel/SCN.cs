using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public static class SCN
	{
		public static void newGame()
		{
			SCN.manual_bgm_replace = null;
			SCN.need_fine_pvv = true;
		}

		public static int fine_pvv(bool force = false)
		{
			if (!force && !SCN.need_fine_pvv)
			{
				return (int)COOK.getCurrentFile().phase;
			}
			SCN.need_fine_pvv = false;
			int num = X.NmI(GF.getPVV(), 0, false, false);
			if ((int)COOK.getCurrentFile().phase != num)
			{
				COOK.getCurrentFile().phase = (ushort)num;
				TRMManager.need_fine = true;
			}
			return num;
		}

		public static void initializePreviousWA(WAManager WA)
		{
			WA.NewGameFirstAssign();
			if (SCN.fine_pvv(false) >= 105)
			{
				WA.Touch("forest", "_forest_nusi_defeat", true, true);
			}
			WholeMapItem wholeDescriptionByName = SCN.M2D.WM.GetWholeDescriptionByName("house", true);
			if (wholeDescriptionByName != null && wholeDescriptionByName.isVisitted("house_cave2forest"))
			{
				WA.depertAssign("house", "house_cave2forest", "forest", "forest_athletic_thorn_under");
			}
		}

		public static string getScenarioBGM(Map2d NextMap, WholeMapItem Wm, ref string cue_key)
		{
			if (SCN.manual_bgm_replace != null)
			{
				return SCN.manual_bgm_replace;
			}
			string text = NextMap.key;
			if (text != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num <= 1368021015U)
				{
					if (num <= 981561889U)
					{
						if (num != 321423213U)
						{
							if (num != 981561889U)
							{
								goto IL_01CF;
							}
							if (!(text == "city_in_museum"))
							{
								goto IL_01CF;
							}
							if (GF.getC("MUS0") > 0U)
							{
								return "BGM_houkago_no_hitotoki";
							}
							return "";
						}
						else
						{
							if (!(text == "city_scl_center"))
							{
								goto IL_01CF;
							}
							goto IL_01C3;
						}
					}
					else if (num != 1224726196U)
					{
						if (num != 1287972255U)
						{
							if (num != 1368021015U)
							{
								goto IL_01CF;
							}
							if (!(text == "city_scl_ground"))
							{
								goto IL_01CF;
							}
							goto IL_01C3;
						}
						else
						{
							if (!(text == "school_in_garage"))
							{
								goto IL_01CF;
							}
							return "BGM_tigrina";
						}
					}
					else if (!(text == "school_in_armory"))
					{
						goto IL_01CF;
					}
				}
				else if (num <= 2022766569U)
				{
					if (num != 1599602520U)
					{
						if (num != 1624757242U)
						{
							if (num != 2022766569U)
							{
								goto IL_01CF;
							}
							if (!(text == "city_in_baru"))
							{
								goto IL_01CF;
							}
							return "";
						}
						else if (!(text == "school_in_armory_mtgroom"))
						{
							goto IL_01CF;
						}
					}
					else
					{
						if (!(text == "house_theroom"))
						{
							goto IL_01CF;
						}
						return "BGM_tokimeki";
					}
				}
				else if (num != 3012624432U)
				{
					if (num != 3435686565U)
					{
						if (num != 3499884134U)
						{
							goto IL_01CF;
						}
						if (!(text == "city_in_bar"))
						{
							goto IL_01CF;
						}
						return "BGM_town2";
					}
					else
					{
						if (!(text == "city_in_museum_backyard"))
						{
							goto IL_01CF;
						}
						return "BGM_houkago_no_hitotoki";
					}
				}
				else
				{
					if (!(text == "city_scl_dorm"))
					{
						goto IL_01CF;
					}
					goto IL_01C3;
				}
				return "";
				IL_01C3:
				return "BGM_school";
			}
			IL_01CF:
			NextMap.prepared = true;
			Dungeon dungeon = ((NextMap == NextMap.M2D.curMap) ? NextMap.Dgn : null);
			if (dungeon == null)
			{
				string s = NextMap.Meta.GetS("dgn");
				if (TX.valid(s))
				{
					dungeon = NextMap.M2D.DGN.getDgn(s);
				}
			}
			string text2 = null;
			if (dungeon != null)
			{
				text2 = dungeon.key;
			}
			int num2 = SCN.fine_pvv(false);
			if (Wm.text_key == "forest" && (text2 == null || text2.IndexOf("forest") >= 0))
			{
				if (num2 == 0 && GF.getC("NOE1") < 5U)
				{
					return "BGM_wind";
				}
				if (num2 == 102 && GF.getC("IXA1") == 5U)
				{
					return "BGM_ixia";
				}
			}
			if (Wm.text_key == "house" && (num2 == 10 || num2 == 11))
			{
				return "BGM_suzumusi";
			}
			if (Wm.text_key == "forest" && num2 == 104)
			{
				uint c = GF.getC("IXA1");
				uint c2 = GF.getC("WLR1");
				if (c == 0U && c2 > 0U)
				{
					text = NextMap.key;
					if (text != null && text == "forest_ruin_station_r")
					{
						return "BGM_madhatter";
					}
				}
				else
				{
					text = NextMap.key;
					if (text != null)
					{
						if (!(text == "forest_toylabo") && !(text == "forest_toylabo_pre") && !(text == "forest_nusi_pre") && !(text == "forest_entrance_grazia") && !(text == "forest_burst_treasure"))
						{
							if (text == "forest_nusi_coroseum")
							{
								return "";
							}
						}
						else
						{
							if (c != 0U)
							{
								return "BGM_killing";
							}
							return "";
						}
					}
				}
			}
			return null;
		}

		public static string replaceSafeAreaMemory(string pre_key, WholeMapItem.WMTransferPoint.WMRectItem WmRect)
		{
			if (WmRect == null)
			{
				if (pre_key == "house " || pre_key == "house")
				{
					return "house_noelroom !Ev_transfer_exit@L";
				}
			}
			else if (!TX.isStart(pre_key, "house", 0) && TX.isStart(WmRect.Mp.key, "house", 0))
			{
				return "house_noelroom !Ev_transfer_exit@L";
			}
			return null;
		}

		public static string getScenarioPurposeTx()
		{
			int num = SCN.fine_pvv(false);
			if (num == 0)
			{
				return TX.Get("scenario_desc_pvv_0000", "");
			}
			if (num < 5)
			{
				return TX.Get("scenario_desc_pvv_0001", "");
			}
			if (num < 100)
			{
				return TX.Get("scenario_desc_pvv_0002", "");
			}
			if (num <= 101)
			{
				return TX.Get("scenario_desc_pvv_0100", "");
			}
			if (num == 102)
			{
				return TX.Get("scenario_desc_pvv_0101", "");
			}
			if (num == 103)
			{
				if (GF.getC("WLR0") == 0U)
				{
					return TX.Get("scenario_desc_pvv_0102", "");
				}
				return TX.Get("scenario_desc_pvv_0103", "");
			}
			else if (num == 104)
			{
				if (GF.getC("IXA1") == 0U)
				{
					return TX.Get("scenario_desc_pvv_0102", "");
				}
				return TX.Get("scenario_desc_pvv_0104", "");
			}
			else
			{
				if (num >= 105)
				{
					return TX.Get("scenario_desc_pvv_0102", "") + " " + TX.Get("scenario_wip", "");
				}
				return TX.Get("scenario_wip", "");
			}
		}

		public static string getScenarioPhaseTitle(int _pvv = -1)
		{
			if (_pvv < 0)
			{
				_pvv = SCN.fine_pvv(false);
			}
			if (_pvv < 100)
			{
				return TX.Get("Svd_phase_0", "");
			}
			if (_pvv < 200)
			{
				return TX.Get("Svd_phase_1", "");
			}
			return TX.Get("Svd_phase_5", "");
		}

		public static bool canSave(bool bench_exists)
		{
			int num = SCN.fine_pvv(false);
			if (X.BTW(11f, (float)num, 100f) && GF.getC("PRM3") >= 1U)
			{
				return false;
			}
			if (!COOK.canSave() || SCN.M2D.FlgFastTravelDeclined.isActive())
			{
				return false;
			}
			if (SCN.M2D.PlayerNoel.outfit_type == PRNoel.OUTFIT.BABYDOLL)
			{
				return false;
			}
			PR pr = SCN.M2D.curMap.Pr as PR;
			IrisOutManager iris = SCN.M2D.Iris;
			if (pr == null || iris.isIrisActive() || iris.isWaiting(pr))
			{
				return false;
			}
			bool flag = false;
			int i = SCN.M2D.curMap.Meta.GetI("cannot_save", 0, 0);
			if ((i & 1) != 0)
			{
				flag = true;
			}
			else if ((i & 2) != 0)
			{
				flag = !pr.hasFoot() || (!pr.isNormalState() && !pr.isBenchOrGoRecoveryState());
			}
			return bench_exists || (SCN.M2D.isSafeArea() && SCN.M2D.curMap != null && !flag);
		}

		public static bool isBenchCmdEnable(string key)
		{
			if (key == "cure_ep_sensitivity")
			{
				return false;
			}
			if (key == "cure_egged" && SCN.M2D.IMNG.getInventoryPrecious().getCount(NelItem.GetById("precious_egg_remover", false), -1) == 0)
			{
				return false;
			}
			if (key == "fast_travel" && (M2DBase.Instance as NelM2DBase).cantFastTravel() != null)
			{
				return false;
			}
			if (key == "fast_travel_home" && ((M2DBase.Instance as NelM2DBase).isSafeArea() || TX.noe(COOK.getCurrentFile().safe_area_memory)))
			{
				return false;
			}
			if (key == "shower" || key == "pee" || key == "&&Cancel")
			{
				return true;
			}
			int num = SCN.fine_pvv(false);
			if (num < 8)
			{
				return key == "cure_hp" || key == "save" || key == "cure_mp" || key == "cure_ep";
			}
			if (num < 100)
			{
				return key != "fast_travel" && key != "cure_mp" && key != "fast_travel_home";
			}
			return num != 100 || (key != "fast_travel" && key != "fast_travel_home");
		}

		public static bool occurableOazuke()
		{
			return X.sensitive_level <= 0 && (SCN.fine_pvv(false) >= 100 && SCN.M2D.WM.CurWM != null) && !SCN.M2D.WM.CurWM.safe_area;
		}

		public static bool isSuddenBattleEnable(string reader_key, out int sudden_level)
		{
			int num = SCN.fine_pvv(false);
			sudden_level = -1;
			if (reader_key == "forest_nusi_coroseum")
			{
				return false;
			}
			if (num < 100)
			{
				if (reader_key == "forest_tutorial")
				{
					return false;
				}
				if (reader_key == "forest_clocktower" || reader_key == "forest_wood_hall")
				{
					sudden_level = 255;
				}
			}
			return true;
		}

		public static bool isPuppetWNpcDefeated()
		{
			return GF.getC("PUP_KILL") % 2U == 1U;
		}

		public static bool isWNpcWalkAroundEnable(NelM2DBase M2D, WanderingManager.TYPE type)
		{
			return type != WanderingManager.TYPE.PUP || !SCN.isPuppetWNpcDefeated();
		}

		public static bool isWNpcEnable(NelM2DBase M2D, WanderingManager.TYPE type)
		{
			int num = SCN.fine_pvv(false);
			if (num <= 100 || M2D.WM.CurWM == null || M2D.WM.CurWM.safe_area)
			{
				return false;
			}
			string text_key = M2D.WM.CurWM.text_key;
			if (text_key != null && text_key == "sacred")
			{
				return false;
			}
			switch (type)
			{
			case WanderingManager.TYPE.COF:
				if (!SCN.alchemy_lectured)
				{
					return false;
				}
				break;
			case WanderingManager.TYPE.TLD:
				return SCN.alchemy_lectured && num >= 103;
			case WanderingManager.TYPE.PUP:
				return num >= 104;
			}
			return true;
		}

		public static bool isWNpcEnableInMap(Map2d Mp, WanderingManager.TYPE type)
		{
			if (Mp == null)
			{
				return true;
			}
			int num = SCN.fine_pvv(false);
			string key = Mp.key;
			if (num != 103)
			{
				if (num == 104)
				{
					if (key == "forest_ruin_station")
					{
						return false;
					}
					if (key == "forest_toylabo_pre" && type == WanderingManager.TYPE.COF && (GF.getC("IXA1") & 4U) == 0U)
					{
						return false;
					}
				}
			}
			else
			{
				if (key == "forest_ruin_station_r" && GF.getC("WLR0") >= 3U)
				{
					return false;
				}
				if (key == "forest_ostrea_swampt" && type == WanderingManager.TYPE.COF && GF.getC("WLR0") > 0U)
				{
					return false;
				}
			}
			return true;
		}

		public static bool isNightingaleOnlyBag()
		{
			return GF.getC("NIG0") == 2U && GF.getC("NIG1") == 15U;
		}

		public static bool checkVisibilityForMuseum(string key)
		{
			int num = SCN.fine_pvv(false);
			if (key != null)
			{
				uint num2 = <PrivateImplementationDetails>.ComputeStringHash(key);
				if (num2 <= 3044102616U)
				{
					if (num2 <= 97881920U)
					{
						if (num2 != 91367605U)
						{
							if (num2 == 97881920U)
							{
								if (key == "pic_pvv010_pn")
								{
									return num >= 10;
								}
							}
						}
						else if (key == "2weekattack")
						{
							return GF.getB("CITY_2WEEKATTACK");
						}
					}
					else if (num2 != 635059918U)
					{
						if (num2 != 2114468746U)
						{
							if (num2 == 3044102616U)
							{
								if (key == "pic_eggremove_orgasm_0")
								{
									return SCN.M2D.PlayerNoel.EpCon.getOrgasmedCountForSituation("eggremove") > 0;
								}
							}
						}
						else if (key == "pic_scl_garage")
						{
							return (GF.getC("TIG0") == 3U && GF.getC("TIG1") >= 2U) || GF.getC("TIG0") >= 4U;
						}
					}
					else if (key == "pic_pvv010_vn")
					{
						return num >= 10;
					}
				}
				else if (num2 <= 3329645681U)
				{
					if (num2 != 3172116432U)
					{
						if (num2 != 3183913193U)
						{
							if (num2 == 3329645681U)
							{
								if (key == "pic_omorashi_0")
								{
									return GF.getB("OMORASHI");
								}
							}
						}
						else if (key == "pic_pvv104_nusi")
						{
							return num >= 105;
						}
					}
					else if (key == "pic_house_bath")
					{
						return GF.getB("PVV100_BATH");
					}
				}
				else if (num2 != 3374009128U)
				{
					if (num2 != 3382611654U)
					{
						if (num2 == 3861112191U)
						{
							if (key == "pic_dojo_intro")
							{
								return COOK.getSF("EV____dojo/v_talk_study_first") != 0;
							}
						}
					}
					else if (key == "pic_pvv105_alma")
					{
						return num >= 106;
					}
				}
				else if (key == "pic_pvv103_ixia")
				{
					return num >= 103;
				}
			}
			return false;
		}

		public static bool canUseableMagic(MGKIND kind)
		{
			NelM2DBase m2D = SCN.M2D;
			return m2D == null || kind != MGKIND.THUNDERBOLT || !m2D.isDangerItemDisable();
		}

		public static int getMovableHomeWholeMap(List<string> Astr)
		{
			NelM2DBase m2D = SCN.M2D;
			Dictionary<string, WholeMapItem> wholeMapDescriptionObject = m2D.WM.getWholeMapDescriptionObject();
			int num = 0;
			foreach (KeyValuePair<string, WholeMapItem> keyValuePair in wholeMapDescriptionObject)
			{
				WholeMapItem value = keyValuePair.Value;
				if (value.safe_area)
				{
					WAManager.WARecord wa = WAManager.GetWa(value.text_key, false);
					if (wa != null && wa.isActivated())
					{
						string text_key = value.text_key;
						if (text_key == null || !(text_key == "school"))
						{
							Astr.Add(keyValuePair.Key);
							if (value != m2D.WM.CurWM)
							{
								num++;
							}
						}
					}
				}
			}
			return num;
		}

		public static bool alchemy_lectured
		{
			get
			{
				return GF.getC("TLD0") > 0U;
			}
		}

		public static bool trm_enabled
		{
			get
			{
				return GF.getC("TRM0") >= 1U;
			}
		}

		public static bool sp_config_store_enabled
		{
			get
			{
				return GF.getC("BRTU") >= 8U;
			}
		}

		public static bool alchemy_workbench_enabled
		{
			get
			{
				NelM2DBase m2D = SCN.M2D;
				return m2D == null || m2D.IMNG.has_recipe_collection;
			}
		}

		public static void initTotalBuyBarUnder(NelM2DBase M2D)
		{
			if (!SCN.sp_config_store_enabled)
			{
				return;
			}
			StoreManager storeManager = StoreManager.Get("BarUnder", false);
			if (storeManager == null)
			{
				return;
			}
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in M2D.IMNG.getInventoryPrecious().getWholeInfoDictionary())
			{
				if (TX.isStart(keyValuePair.Key.key, "spconfig_", 0))
				{
					for (int i = 0; i < 5; i++)
					{
						int count = keyValuePair.Value.getCount(i);
						if (count > 0)
						{
							storeManager.total_buy += (uint)(storeManager.buyPrice(keyValuePair.Key, i) * count);
						}
					}
				}
			}
		}

		public static bool addSpecialNoelCondition(Designer Ds, PR Pr)
		{
			if (Pr == null)
			{
				return false;
			}
			NelM2DBase nm2D = Pr.NM2D;
			bool flag = true;
			if (nm2D.WM.CurWM != null && nm2D.WM.CurWM.text_key == "sacred")
			{
				flag = true;
				Ds.addImg(new DsnDataImg
				{
					swidth = Ds.use_w,
					sheight = 80f,
					text_margin_x = 30f,
					size = 14f,
					text = TX.Get("GM_error_sacred", ""),
					text_auto_wrap = true,
					TxCol = C32.d2c(4294901760U),
					alignx = ALIGN.CENTER,
					text_auto_condense = true,
					FnDrawInFIB = new FillImageBlock.FnDrawInFIB(SCN.FnDrawNoelConditionSacred)
				});
			}
			return flag;
		}

		public static bool FnDrawNoelConditionSacred(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			if (!Md.hasMultipleTriangle())
			{
				Md.InitSubMeshContainer(0);
				Material material = MTRX.newMtr(MTR.MtrNoisyMessage);
				material.SetFloat("_StencilRef", (float)FI.stencil_ref);
				material.SetFloat("_StencilComp", 3f);
				Md.setMaterial(material, true);
				Md.connectRendererToTriMulti(FI.getMeshRenderer());
			}
			Md.Col = C32.MulA(4283782485U, alpha);
			Md.uvRectN(0.38f, 0.38f);
			Md.Uv2(0.38f, 1f, true);
			Md.KadomaruRect(0f, 0f, FI.get_swidth_px() - 30f, FI.get_sheight_px() - 12f, 20f, 0f, false, 0f, 0f, false);
			Md.allocUv2(0, true);
			update_meshdrawer = true;
			return true;
		}

		public static NelM2DBase M2D
		{
			get
			{
				return M2DBase.Instance as NelM2DBase;
			}
		}

		public static float draw_shift_x = 0f;

		public static float draw_shift_y = 0f;

		public static string manual_bgm_replace = null;

		public static bool need_fine_pvv;

		public const string gfcname_puppet_kill_count = "PUP_KILL";

		public static Func<bool> isGraziaActivated = () => SCN.fine_pvv(false) >= 104;
	}
}

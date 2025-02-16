using System;
using System.Collections.Generic;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class UiAlchemyTRM : UiCraftBase, ITrmListener
	{
		protected override float cartbtn_x
		{
			get
			{
				return base.cartbtn_x + 85f;
			}
		}

		protected override void Awake()
		{
			this.cartbtn_icon = "pict_aloma";
			base.Awake();
			this.add_achivement = false;
			EV.getVariableContainer().define("_result", "-1", true);
			EV.getVariableContainer().define("_reward", "0", true);
			UiAlchemyTRM.CInfoDef = new UiCraftBase.AutoCreationInfo(0, UiCraftBase.AF_COST.LOW_COST, UiCraftBase.AF_KIND.ENOUGH, UiCraftBase.AF_QUANTITY.MINIMUM)
			{
				previous = true
			};
			this.IMNG = ((this.M2D != null) ? this.M2D.IMNG : null);
			this.Inventory = ((this.IMNG != null) ? this.IMNG.getInventory() : null);
			this.alloc_multiple_creation = false;
			TRMManager.initTRMScript();
			this.ingredient_item_row_skin = "recipe_trm_ingredient";
			this.StRecipeTopic = UiAlchemyTRM.prepareRecipeTopicTRM();
			this.topic_use_topright_counter = false;
			UiAlchemyTRM.Instance = this;
		}

		protected override string topic_title_text_content
		{
			get
			{
				return TX.Get("storage_title_trm", "");
			}
		}

		public override void OnDestroy()
		{
			if (UiAlchemyTRM.Instance == this)
			{
				UiAlchemyTRM.Instance = null;
			}
			base.OnDestroy();
		}

		public static ItemStorage prepareRecipeTopicTRM()
		{
			ItemStorage itemStorage = new ItemStorage("Inventory_trm", 2);
			itemStorage.sort_button_bits = 0;
			itemStorage.infinit_stockable = true;
			UiAlchemyTRM.AddTopicRow(itemStorage);
			itemStorage.fineRows(false);
			return itemStorage;
		}

		public static void AddTopicRow(ItemStorage St)
		{
			TRMManager.fineExecute(false);
			Dictionary<string, TRMManager.TRMItem> wholeTriObject = TRMManager.getWholeTriObject();
			string text = null;
			string text2 = null;
			string text3 = null;
			foreach (KeyValuePair<string, TRMManager.TRMItem> keyValuePair in wholeTriObject)
			{
				TRMManager.TRMItem value = keyValuePair.Value;
				if (!value.watched_a)
				{
					text = keyValuePair.Key;
				}
				if (!value.watched_b)
				{
					text2 = keyValuePair.Key;
				}
				text3 = keyValuePair.Key;
				St.Add(value.RowItm, 1, 0, true, true);
			}
			string text4;
			if ((text4 = text) == null)
			{
				text4 = text2 ?? text3;
			}
			St.select_row_key = text4;
		}

		public override string topic_row_skin
		{
			get
			{
				return "recipe_trm";
			}
		}

		protected override ItemStorage getRecipeTopicDefault()
		{
			return this.StRecipeTopic;
		}

		protected override void prepareRecipeCreatable()
		{
		}

		protected override string[] getRcpTopicTabKeys()
		{
			return null;
		}

		protected override void fnRecipeTopicRowsPrepare(UiItemManageBox IMng, List<ItemStorage.IRow> ASource, List<ItemStorage.IRow> ADest)
		{
			ADest.AddRange(ASource);
		}

		protected override string fnRecipeTopicDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			return UiAlchemyTRM.fnRecipeTopicDescAdditionSForTrm(Itm, row, def_string, grade, Obt, count);
		}

		public static string fnRecipeTopicDescAdditionSForTrm(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			if (row == UiItemManageBox.DESC_ROW.TOPRIGHT_COUNTER)
			{
				return TX.Get("TrmUi_selector_topright", "");
			}
			TRMManager.TRMItem trmitem = ((Itm != null) ? UiAlchemyTRM.GetTrmFromItem(Itm.key) : null);
			if (trmitem == null || !trmitem.is_active)
			{
				return def_string;
			}
			if (row == UiItemManageBox.DESC_ROW.DETAIL)
			{
				RCP.Recipe recipeS = UiAlchemyTRM.getRecipeS(Itm);
				if (recipeS != null)
				{
					def_string = def_string + "\n ----- \n" + recipeS.listupIngredients("\n", true, true);
				}
			}
			return def_string;
		}

		protected override string getCompletedWorkCountString()
		{
			return null;
		}

		protected override void prepareRecipe(RCP.Recipe Rcp, NelItem Itm, List<List<UiCraftBase.IngEntryRow>> AAPre = null)
		{
			TRMManager.TRMItem trmFromItem = UiAlchemyTRM.GetTrmFromItem(Itm.key);
			if (trmFromItem != null)
			{
				if (!trmFromItem.is_active)
				{
					return;
				}
				this.TargetTrm = trmFromItem;
				if (this.TargetTrm.watched_all)
				{
					this.initLunch();
					return;
				}
				if (this.Atarget_hpow == null)
				{
					this.Atarget_hpow = new byte[5];
					this.Adraw_hpow = new float[5];
					this.Acur_hpow = new byte[5];
				}
				this.success_ratio100 = 0;
				if (Rcp.created == 0U)
				{
					Rcp.created = 1U;
				}
				byte[] herbPower = TRMManager.GetHerbPower(this.TargetTrm.RcmHerb.key, false);
				for (int i = 0; i < 5; i++)
				{
					this.Atarget_hpow[i] = (this.Acur_hpow[i] = 0);
					this.Adraw_hpow[i] = 0f;
					if (herbPower != null)
					{
						this.Atarget_hpow[i] = (byte)((int)herbPower[i] * this.TargetTrm.rcm_count);
					}
				}
				base.prepareRecipe(Rcp, Itm, AAPre);
				this.fineTTtext();
			}
		}

		protected override string getIngredientTitle()
		{
			return TX.GetA("aloma_ingredient_title", this.TargetTrm.getNameLocalized());
		}

		protected override UiCraftBase.AutoCreationInfo getFirstAutoCreateInfo()
		{
			this.TargetRcp.CInfo.obtain_flag = true;
			if (this.TargetTrm.watched_a)
			{
				return null;
			}
			List<List<UiCraftBase.IngEntryRow>> list = this.TargetRcp.getPrevList();
			if (this.TargetTrm.RcmHerb.RecipeInfo == null)
			{
				list.Clear();
			}
			else
			{
				int count = this.TargetRcp.AIng.Count;
				if (list == null)
				{
					list = new List<List<UiCraftBase.IngEntryRow>>(count);
				}
				for (int i = 0; i < count; i++)
				{
					while (list.Count <= i)
					{
						list.Add(new List<UiCraftBase.IngEntryRow>());
					}
					List<UiCraftBase.IngEntryRow> list2 = list[i];
					RCP.RecipeIngredient recipeIngredient = this.TargetRcp.AIng[i];
					if (recipeIngredient.target_category != (RCP.RPI_CATEG)0 && (recipeIngredient.target_category & this.TargetTrm.RcmHerb.RecipeInfo.categ) != (RCP.RPI_CATEG)0)
					{
						list2.Clear();
						int num = this.TargetTrm.rcm_count;
						while (--num >= 0)
						{
							list2.Add(new UiCraftBase.IngEntryRow(null, recipeIngredient, this.TargetTrm.RcmHerb, this.TargetTrm.rcm_grade));
						}
					}
				}
			}
			this.TargetRcp.setPrevList(list);
			return UiAlchemyTRM.CInfoDef;
		}

		public void SelectTopicRow(TRMManager.TRMItem Trm)
		{
			if (Trm == null || this.stt != UiCraftBase.STATE.RECIPE_TOPIC)
			{
				return;
			}
			NelItem byId = NelItem.GetById("TrmItem_" + Trm.key, false);
			if (byId == null)
			{
				return;
			}
			using (BList<aBtnItemRow> blist = this.ItemMng.Inventory.PopGetItemRowBtnsFor(byId))
			{
				blist[0].Select(true);
			}
		}

		protected override void changeState(UiCraftBase.STATE st)
		{
			UiCraftBase.STATE stt = this.stt;
			this.quitLunch(true);
			base.changeState(st);
			if (st == UiCraftBase.STATE.RECIPE_CHOOSE_ROW)
			{
				if (this.BxTT == null)
				{
					this.BxTT = base.Create("TT", this.out_w * 0.5f - base.rie_w * 0.5f - 8f, this.out_center_y + this.out_h * 0.5f - 40f + 15f, base.rie_w - 20f, 80f, 3, IN.h * 0.3f, UiBoxDesignerFamily.MASKTYPE.BOX);
					this.BxTT.getBox().frametype = UiBox.FRAMETYPE.DARK_SIMPLE;
					IN.setZ(this.BxTT.transform, -0.1875f);
					this.BxTT.margin_in_lr = 20f;
					this.BxTT.margin_in_tb = 8f;
					this.BxTT.init();
					this.BxTT.addP(new DsnDataP("", false)
					{
						name = "tt_p",
						text = " ",
						TxCol = C32.d2c(uint.MaxValue),
						swidth = this.BxTT.use_w,
						sheight = this.BxTT.use_h,
						alignx = ALIGN.LEFT,
						text_auto_wrap = true,
						size = 14f,
						html = true
					}, false);
					this.fineTTtext();
				}
				this.BxTT.activate();
			}
			if (st == UiCraftBase.STATE._NOUSE && UiAlchemyTRM.Instance == this)
			{
				UiAlchemyTRM.Instance = null;
			}
			if (st == UiCraftBase.STATE.RECIPE_TOPIC && this.BxTT != null)
			{
				this.BxTT.deactivate();
			}
		}

		private void fineTTtext()
		{
			if (this.BxTT != null && this.TargetTrm != null)
			{
				this.BxTT.Get("tt_p", false).setValue(TX.GetA("TrmUi_recommend_aloma_making", "<font color=\"0x" + C32.codeToCodeText(4294410942U) + "\">" + this.TargetTrm.getLocalizedRecommendedItem()) + "</font>");
			}
		}

		protected override string fnRecipeIngredientDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			if (row == UiItemManageBox.DESC_ROW.DETAIL)
			{
				int num = 5;
				using (STB stb = TX.PopBld(null, 0))
				{
					byte[] herbPower = TRMManager.GetHerbPower(Itm.key, true);
					for (int i = 0; i < num; i++)
					{
						STB stb2 = stb;
						string text = "aloma_power_";
						TRMManager.HerbPow herbPow = (TRMManager.HerbPow)i;
						stb2.AppendTxA(text + herbPow.ToString(), "\n");
						stb.Add(": ").Add((int)((herbPower != null) ? herbPower[i] : 0));
					}
					return stb.ToString();
				}
				return def_string;
			}
			return def_string;
		}

		protected override int fnSortAutoCreationItemRow(ItemStorage.IRow Ra, ItemStorage.IRow Rb)
		{
			UiCraftBase.AutoCreationInfo currentSort = UiCraftBase.AutoCreationInfo.CurrentSort;
			if (this.TargetTrm != null && currentSort.kind == UiCraftBase.AF_KIND.NONE)
			{
				if (UiCraftBase.AutoCreationInfo.CurrentIng.target_category != (RCP.RPI_CATEG)0 && (UiCraftBase.AutoCreationInfo.CurrentIng.target_category & this.TargetTrm.RcmHerb.RecipeInfo.categ) != (RCP.RPI_CATEG)0)
				{
					float num = 0f;
					float num2 = 0f;
					byte[] array = null;
					for (int i = 0; i < 2; i++)
					{
						float num3 = 0f;
						ItemStorage.IRow row = ((i == 0) ? Ra : Rb);
						if (row.Data == this.TargetTrm.RcmHerb)
						{
							num3 += 100f;
						}
						else
						{
							if (array == null)
							{
								array = TRMManager.GetHerbPower(this.TargetTrm.RcmHerb.key, false);
							}
							byte[] herbPower = TRMManager.GetHerbPower(row.Data.key, false);
							if (X.isinStr(this.Aingredient_bad, row.Data.key, -1) >= 0)
							{
								num3 -= 8f;
							}
							if (array == null || herbPower == null)
							{
								num3 -= 8f;
							}
							else
							{
								for (int j = 0; j < 5; j++)
								{
									num3 += 2f * X.ZLINE((float)X.Abs((int)(herbPower[i] - array[i])), 6f);
								}
							}
						}
						if (i == 0)
						{
							num = num3;
						}
						else
						{
							num2 = num3;
						}
					}
					if (num != num2)
					{
						if (num - num2 > 0f != !this.TargetTrm.watched_a)
						{
							return 1;
						}
						return -1;
					}
				}
				else if (Ra.total != Rb.total)
				{
					if (Ra.total - Rb.total <= 0)
					{
						return 1;
					}
					return -1;
				}
			}
			return base.fnSortAutoCreationItemRow(Ra, Rb);
		}

		protected override void fineCompletionDetail(STB Stb, bool set_field = false)
		{
			if (this.FiBCompGraph != null)
			{
				this.FiBCompGraph.redraw_flag = true;
			}
			base.fineCompletionDetail(Stb, set_field);
		}

		protected override void getCompletionDetail(STB Stb)
		{
			for (int i = 0; i < 5; i++)
			{
				this.Acur_hpow[i] = 0;
			}
			if (this.CompletionImage == null || this.CompletionImage.OUseIngredient.Count == 0)
			{
				this.success_ratio100 = 0;
			}
			else
			{
				this.success_ratio100 = (byte)(100 + 15 * this.CompletionImage.calced_grade);
				int num = 0;
				bool flag = false;
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.CompletionImage.OUseIngredient)
				{
					NelItem key = keyValuePair.Key;
					if (!flag && X.isinStr(this.Aingredient_bad, key.key, -1) >= 0)
					{
						num++;
					}
					if (key == this.TargetTrm.RcmHerb)
					{
						flag = true;
					}
					byte[] herbPower = TRMManager.GetHerbPower(key.key, true);
					if (herbPower != null)
					{
						for (int j = 0; j < 5; j++)
						{
							byte[] acur_hpow = this.Acur_hpow;
							int num2 = j;
							acur_hpow[num2] += herbPower[j];
						}
					}
				}
				if (!flag)
				{
					this.success_ratio100 = (byte)X.Mx(0, (int)this.success_ratio100 - 35 * num);
				}
				for (int k = 0; k < 5; k++)
				{
					if (this.Acur_hpow[k] == 0 && this.Atarget_hpow[k] > 0)
					{
						this.success_ratio100 = (byte)X.Mx(0, (int)(this.success_ratio100 - 25));
					}
					else
					{
						this.success_ratio100 = (byte)X.Mx(0, (int)this.success_ratio100 - 10 * X.Mn(3, X.Abs((int)(this.Acur_hpow[k] - this.Atarget_hpow[k]))));
					}
				}
			}
			this.getSccessRatioStb(Stb);
			if (this.TargetTrm != null)
			{
				Stb.Add("\n", this.TargetTrm.icon_watched_a, (!this.TargetTrm.watched_a) ? this.TargetTrm.reward_string_a : TX.Get("TrmUi_selector_already_watched", ""));
				Stb.Add("\n", this.TargetTrm.icon_watched_b, (!this.TargetTrm.watched_b) ? this.TargetTrm.reward_string_b : TX.Get("TrmUi_selector_already_watched", ""));
			}
		}

		private STB getSccessRatioStb(STB Stb)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				if (this.success_ratio100 >= 100)
				{
					stb.Add("<font color=\"");
					stb.AddCol(4294913690U, "0x").Add("\">", (int)this.success_ratio100, "%</font>");
				}
				else
				{
					stb.Add((int)this.success_ratio100);
					stb.Add("%");
				}
				Stb.AddTxA("TrmUi_selector_success_change", false).TxRpl(stb);
			}
			return Stb;
		}

		private void getComplCirclePos(FillImageBlock FI, out float posx, out float radius)
		{
			posx = FI.get_swidth_px() * 0.5f + this.BxCmd.item_margin_x_px + (this.BxCmd.w - 90f - FI.get_swidth_px()) * 0.66f;
			radius = FI.get_sheight_px() * 0.44f;
		}

		protected override bool fnDrawCmdImage(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			bool flag = false;
			if (X.D)
			{
				update_meshdrawer = true;
				float num;
				float num2;
				this.getComplCirclePos(FI, out num, out num2);
				Md.base_px_x = num;
				Md.base_px_y = 0f;
				Md.Col = C32.MulA(4283780170U, alpha * 0.5f);
				Md.Circle(0f, 0f, num2, 1f, false, 0f, 0f);
				Md.Col = C32.MulA(4283780170U, alpha * 0.25f);
				int num3 = 5;
				float num4 = num2 * 0.015625f;
				float num5 = 1f / (float)num3 * 6.2831855f;
				flag = true;
				for (int i = 0; i < num3; i++)
				{
					float num6 = 1.5707964f - num5 * (float)i;
					Md.Line(0f, 0f, num2 * X.Cos(num6), num2 * X.Sin(num6), 1f, false, 0f, 0f);
					if (X.Abs(this.Adraw_hpow[i] - (float)this.Acur_hpow[i]) < 0.02f)
					{
						this.Adraw_hpow[i] = (float)this.Acur_hpow[i];
					}
					else
					{
						flag = false;
						this.Adraw_hpow[i] = X.MULWALK(this.Adraw_hpow[i], (float)this.Acur_hpow[i], X.ScrPow(0.28f, X.AF));
					}
				}
				Md.Daia(0f, 0f, 2f, 2f, false);
				int num7 = 0;
				while ((float)num7 <= 6f)
				{
					float num8 = 1f / (float)num3;
					Md.Poly(0f, 0f, num2 * this.graph_radius_ratio((float)((byte)num7)), 1.5707964f, num3, 1f, false, 0f, 0f);
					num7++;
				}
				for (int j = 0; j < 2; j++)
				{
					byte[] array = null;
					float[] array2 = null;
					uint num9;
					if (j == 0)
					{
						array = this.Atarget_hpow;
						num9 = 4287600340U;
					}
					else
					{
						num9 = 4294928810U;
						array2 = this.Adraw_hpow;
					}
					Md.Pos(0f, 0f, null);
					for (int k = 0; k < num3; k++)
					{
						Md.Tri(-1, k, (k + 1) % num3, false);
					}
					for (int l = 0; l < 2; l++)
					{
						Md.Col = C32.MulA(num9, alpha * ((l == 1) ? 1f : 0.5f));
						float num10 = 0f;
						float num11 = 0f;
						float num12 = 0f;
						float num13 = 0f;
						for (int m = 0; m < num3; m++)
						{
							float num14 = 1.5707964f - num5 * (float)m;
							float num15 = this.graph_radius_ratio((array != null) ? ((float)array[m]) : ((array2 != null) ? array2[m] : 0f));
							float num16 = X.Cos(num14) * num4 * num15;
							float num17 = X.Sin(num14) * num4 * num15;
							if (l == 0)
							{
								Md.Pos(num16, num17, null);
							}
							else
							{
								if (m == 0)
								{
									num10 = num16;
									num11 = num17;
								}
								else
								{
									Md.Line(num16, num17, num12, num13, 0.015625f, true, 0f, 0f);
								}
								num12 = num16;
								num13 = num17;
							}
						}
						if (l == 1)
						{
							Md.Line(num10, num11, num12, num13, 0.015625f, true, 0f, 0f);
						}
					}
				}
			}
			return flag;
		}

		private float graph_radius_ratio(float r)
		{
			if (r > 6f)
			{
				return 1f + (r - 6f) * 0.5f / 6f;
			}
			return X.Scr(0.08f, r / 6f);
		}

		protected override BtnContainer<aBtn> initCreationComplete(int created, int add_rest)
		{
			this.creation_complete_fib_height = 90f;
			BtnContainer<aBtn> btnContainer = base.initCreationComplete(created, add_rest);
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					this.getSccessRatioStb(stb2);
					(btnContainer.Get(0).get_Skin() as ButtonSkinRow).setTitleTextS(stb.AddTxA("alchemy_btn_go_to_play", false).TxRpl(stb2));
				}
			}
			return btnContainer;
		}

		protected override string getCompleteDialogPrompt(string item_name, int created, int add_rest, ref float lineSpacing)
		{
			return string.Concat(new string[]
			{
				TX.GetA("alchemy_complete", item_name),
				"\n \n <img mesh=\"nel_item_grade.",
				this.CompletionImage.calced_grade.ToString(),
				"\" width=\"34\" height=\"70\" scale=\"1\" color=\"0x",
				C32.codeToCodeText(4283780170U),
				"\"/> "
			});
		}

		protected override BList<string> getCreationCompleteBtnKeys(BList<string> Akey, bool no_rest, out string confirm_key, out string cancel_key, out int btn_clms)
		{
			string text;
			confirm_key = (text = "&&alchemy_btn_eat_in");
			Akey.Add(text);
			cancel_key = (text = "&&alchemy_btn_cancel_trm");
			Akey.Add(text);
			btn_clms = 1;
			return Akey;
		}

		protected override string completion_state_cancel_btn_key(int i)
		{
			if (i != 0)
			{
				return null;
			}
			return "&&alchemy_btn_cancel_trm";
		}

		protected override FillImageBlock createCmdFIB(float __h)
		{
			FillImageBlock fillImageBlock = (this.FiBCompGraph = this.BxCmd.addImg(new DsnDataImg
			{
				name = "cmd_l",
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawCmdImage),
				swidth = 8f,
				sheight = __h
			}));
			float num;
			float num2;
			this.getComplCirclePos(fillImageBlock, out num, out num2);
			UiBoxDesigner bxCmd = this.BxCmd;
			int num3 = 5;
			float num4 = 1f / (float)num3 * 6.2831855f;
			num2 += 4f;
			for (int i = 0; i < num3; i++)
			{
				TextRenderer textRenderer = IN.CreateGob(fillImageBlock.gameObject, "-Txgraph" + i.ToString()).AddComponent<TextRenderer>();
				float num5 = 1.5707964f - num4 * (float)i;
				bxCmd.addGameObject(textRenderer.gameObject, "tx_power_name_" + i.ToString(), 0f, false);
				IN.PosP(textRenderer.transform, num + num2 * X.Cos(num5), num2 * X.Sin(num5), -0.3f);
				textRenderer.alignx = ((i == 0) ? ALIGN.CENTER : ((i <= 2) ? ALIGN.LEFT : ALIGN.RIGHT));
				textRenderer.aligny = ((i == 0) ? ALIGNY.BOTTOM : ALIGNY.MIDDLE);
				textRenderer.size = 14f;
				textRenderer.BorderColor = C32.d2c(4283780170U);
				textRenderer.TextColor = C32.d2c(uint.MaxValue);
				TextRenderer textRenderer2 = textRenderer;
				string text = "aloma_power_";
				TRMManager.HerbPow herbPow = (TRMManager.HerbPow)i;
				textRenderer2.text_content = TX.Get(text + herbPow.ToString(), "");
			}
			return fillImageBlock;
		}

		protected override bool initLunch()
		{
			if (this.t_ssc >= 0f)
			{
				return false;
			}
			this.t_ssc = 0f;
			base.cancelCopleteEff();
			IN.clearPushDown(true);
			if (this.Fader == null)
			{
				this.Fader = new TransFader(TFKEY.SD, 85f, IN.w + 40f, IN.h, 1f);
				TextRenderer textRenderer = (this.TxKDSsc = IN.CreateGob(this.CartBtn.gameObject, "-TxKDSsc").AddComponent<TextRenderer>());
				IN.setZ(textRenderer.transform, -0.5f);
				textRenderer.use_valotile = true;
				textRenderer.alignx = ALIGN.CENTER;
				textRenderer.size = 18f;
				textRenderer.html_mode = true;
				textRenderer.BorderColor = C32.d2c(4278190080U);
				textRenderer.TextColor = C32.d2c(uint.MaxValue);
				this.EfpSscCharge = new EfParticleOnce("alchemy_trm_ssc_charge", EFCON_TYPE.FIXED);
				this.EfpSscAftCircle = new EfParticleOnce("alchemy_trm_ssc_aft_circle", EFCON_TYPE.FIXED);
				this.EfpSscSuccess = new EfParticleOnce("alchemy_trm_ssc_aft_success", EFCON_TYPE.FIXED);
				this.EfpSscAftCircle.not_mesh_image_replace = true;
				this.EfpSscFailure = new EfParticleOnce("alchemy_trm_ssc_aft_failure", EFCON_TYPE.FIXED);
			}
			this.TxKDSsc.gameObject.SetActive(true);
			this.TxKDSsc.alpha = 1f;
			this.t_shld = 0f;
			this.auto_clear_mdef = false;
			this.icon_center_y = IN.h * 0.0625f;
			if (this.TargetTrm.watched_all)
			{
				this.t_shld = -1000f;
				this.icon_center_y = IN.h * 0.2f;
				if (this.BxManual == null)
				{
					this.BxManual = base.Create("Manual", 0f, -IN.h * 0.14f, IN.w * 0.28f, 105f, 3, IN.h * 0.77f, UiBoxDesignerFamily.MASKTYPE.BOX);
					this.BxManual.getBox().gradation(new Color32[]
					{
						MTRX.ColTrnsp,
						MTRX.ColTrnsp
					}, null);
					IN.setZ(this.BxManual.transform, -0.625f);
					this.BxManual.Smallest().init();
					Designer bxManual = this.BxManual;
					DsnDataRadio dsnDataRadio = new DsnDataRadio();
					dsnDataRadio.name = "manual_bcon";
					dsnDataRadio.skin = "row_dark_center";
					dsnDataRadio.keys = new string[] { "root_a", "root_b", "&&Cancel" };
					dsnDataRadio.navi_loop = 1;
					dsnDataRadio.margin_h = 0;
					dsnDataRadio.clms = 1;
					dsnDataRadio.h = 35f;
					dsnDataRadio.w = this.BxManual.use_w;
					dsnDataRadio.fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnManualRootSelected);
					dsnDataRadio.fnMaking = delegate(BtnContainer<aBtn> BCon, aBtn B)
					{
						B.use_valotile = true;
						return true;
					};
					this.BConManual = bxManual.addRadioT<aBtnNel>(dsnDataRadio);
					this.BConManual.Get(0).setSkinTitle("<img mesh=\"trm_heart_a\" width=\"26\" height=\"22\" />" + TX.Get("TrmUi_root_a", ""));
					this.BConManual.Get(1).setSkinTitle("<img mesh=\"trm_heart_b\" width=\"26\" height=\"22\" />" + TX.Get("TrmUi_root_b", ""));
				}
				this.BxManual.gameObject.SetActive(true);
				this.BxManual.activate();
				this.BConManual.setValue("-1");
				this.BConManual.Get(0).Select(true);
				this.TxKDSsc.text_content = TX.Get("TrmUi_unlocked_all", "");
				this.CartBtn.maxt_pos = 12;
			}
			else
			{
				if (this.BxManual != null)
				{
					this.BxManual.gameObject.SetActive(false);
				}
				this.CartBtn.maxt_pos = 40;
				using (STB stb = TX.PopBld(null, 0))
				{
					using (STB sccessRatioStb = this.getSccessRatioStb(TX.PopBld(null, 0)))
					{
						stb.AddTxA("KD_TrmUi_submit_scene", false).TxRpl(sccessRatioStb);
						this.TxKDSsc.Txt(stb);
					}
				}
			}
			this.Fader.WH(-1000f, IN.h + 40f + X.Abs(this.icon_center_y) * 2f);
			this.Fader.do_not_shape_clear = true;
			this.CartBtn.position(0f, this.icon_center_y, -1000f, -1000f, false);
			this.fineSscCartBtnScale();
			if (this.stt == UiCraftBase.STATE.COMPLETE || this.stt == UiCraftBase.STATE.COMPLETE_ENOUGH)
			{
				this.BxConfirm.deactivate();
				this.BxTT.deactivate();
			}
			return true;
		}

		private void fineSscCartBtnScale()
		{
			float ssc_cartbtn_scale = this.ssc_cartbtn_scale;
			this.CartBtn.transform.localScale = new Vector3(ssc_cartbtn_scale, ssc_cartbtn_scale, 1f);
			float num = 1f / ssc_cartbtn_scale;
			this.TxKDSsc.transform.localScale = new Vector3(num, num, 1f);
			this.need_ssc_redraw = true;
			IN.PosP2(this.TxKDSsc.transform, 0f, -IN.h * 0.18f * num);
		}

		protected override void quitLunch(bool do_not_destruct_element = false)
		{
			if (this.result_success >= 10)
			{
				return;
			}
			bool flag = false;
			IN.setZ(this.CartBtn.transform, -0.125f);
			IN.resort_ui_bind_valotile();
			if (this.t_ssc >= 0f)
			{
				flag = true;
				if (this.Fader != null)
				{
					this.TxKDSsc.gameObject.SetActive(false);
				}
				if (do_not_destruct_element)
				{
					this.t_ssc = -1f;
				}
				else
				{
					this.t_ssc = -30f;
					this.MdEf.clear(false, false);
				}
				this.CartBtn.maxt_pos = 12;
				this.CartBtn.transform.localScale = Vector3.zero;
				IN.clearPushDown(true);
			}
			if (this.BxManual != null)
			{
				this.BxManual.deactivate();
			}
			this.auto_clear_mdef = true;
			if (this.stt == UiCraftBase.STATE.COMPLETE || this.stt == UiCraftBase.STATE.COMPLETE_ENOUGH)
			{
				this.CartBtn.position(this.cartbtn_x, base.cartbtn_y, -1000f, -1000f, false);
				this.BxTT.activate();
				this.BxConfirm.activate();
				BtnContainerRunner btnContainerRunner = this.BxConfirm.Get("ubtn", false) as BtnContainerRunner;
				if (btnContainerRunner != null)
				{
					aBtn aBtn = btnContainerRunner.Get("&&alchemy_btn_eat_in");
					if (aBtn != null)
					{
						aBtn.Select(true);
						aBtn.SetChecked(false, true);
						return;
					}
				}
			}
			else if (this.stt == UiCraftBase.STATE.RECIPE_TOPIC && flag)
			{
				this.CartBtn.posSetA(this.cartbtn_x + 300f, base.cartbtn_y, -1000f, -1000f, false);
				this.CartBtn.hide();
				this.SelectTopicRow(this.TargetTrm);
				this.TargetTrm = null;
			}
		}

		protected override bool drawCompleteEffect(MeshDrawer Md, int af, ref bool update_meshdrawer)
		{
			if (this.t_ssc >= 0f)
			{
				update_meshdrawer = false;
				return false;
			}
			bool flag = base.drawCompleteEffect(Md, af, ref update_meshdrawer);
			if (this.t_ssc > -30f)
			{
				this.MdEf.clear(false, false);
				this.redrawSSC(1);
			}
			return flag;
		}

		protected override bool runLunch(float fcnt)
		{
			bool flag = false;
			bool flag2 = false;
			if (this.t_shld < 130f)
			{
				bool flag3 = false;
				bool flag4 = this.t_shld == -1000f;
				if (this.t_ssc >= 0f)
				{
					if (this.t_ssc < 30f)
					{
						this.t_ssc += fcnt;
						flag = true;
					}
					if (this.CartBtn.move_animating)
					{
						this.need_ssc_redraw = true;
					}
					if (this.t_ssc >= 10f && IN.isSubmitOn(0) && !flag4)
					{
						flag3 = true;
					}
					else if (IN.isCancel())
					{
						this.quitLunch(true);
						return true;
					}
				}
				else if (this.t_ssc > -30f)
				{
					this.t_ssc -= fcnt;
					flag = true;
				}
				if (!flag4)
				{
					if (flag3)
					{
						if (this.t_shld == 0f)
						{
							this.SndHold = this.M2D.Snd.play("trm_alchemy_submit");
						}
						this.t_shld = X.Mn(this.t_shld + fcnt, 130f);
						if (this.t_shld == 130f)
						{
							this.confirmTrmFinal(-1);
						}
						flag = true;
						float num = (0.15f + 0.875f * X.ZPOW(this.t_shld, 80f)) * 6f;
						this.CartBtn.icon_shift_x = num * 0.5f * (X.COSIT(4.93f / num) + X.COSIT(2.33f / num));
						this.CartBtn.icon_shift_y = num * 0.5f * (X.COSIT(6.71f / num) + X.COSIT(1.79f / num));
					}
					else
					{
						if (this.t_shld > 0f)
						{
							this.t_shld = X.Mx(-30f, -1f - this.t_shld);
						}
						if (this.SndHold != null)
						{
							this.SndHold.Stop();
							this.SndHold = null;
							this.CartBtn.icon_shift_x = (this.CartBtn.icon_shift_y = 0f);
						}
						if (this.t_shld < 0f)
						{
							this.t_shld = X.Mn(fcnt, 0f);
							flag = true;
						}
					}
				}
			}
			else
			{
				float num2 = this.t_shld - 130f;
				if (num2 <= 90f)
				{
					this.TxKDSsc.alpha = X.ZLINE(num2 - 10f, 30f);
					flag = true;
				}
				bool flag5 = num2 >= 70f;
				this.t_shld += fcnt;
				num2 += fcnt;
				if (!flag5 && num2 >= 70f)
				{
					this.deactivate(false);
					this.active = true;
					this.CartBtn.posSetA(0f, IN.w * 0.8f, -1000f, -1000f, true);
				}
				if (this.stt != UiCraftBase.STATE._NOUSE)
				{
					if (num2 >= 95f)
					{
						bool flag6 = num2 >= 220f;
						if (IN.isCancelOrReturnPD() || IN.kettei())
						{
							flag6 = true;
							SND.Ui.play("cancel", false);
						}
						if (flag6)
						{
							CsvVariableContainer variableContainer = EV.getVariableContainer();
							variableContainer.define("_result", this.result_success.ToString(), true);
							variableContainer.define("_episode", this.TargetTrm.key, true);
							variableContainer.define("_reward", (TRMManager.CurrentReward != null) ? TRMManager.CurrentReward.total_reward.ToString() : "0", true);
							this.TargetTrm.RowItm.addObtainCount(1);
							this.result_success += 11;
							this.changeState(UiCraftBase.STATE._NOUSE);
							this.CartBtn.posSetA(-1000f, -1000f, -1000f, -1000f, false);
							this.result_success -= 11;
							if (this.result_success == 1 && !this.TargetTrm.watched_a)
							{
								this.TargetTrm.watched_a = true;
								TRMManager.need_fine = (TRMManager.need_recheck_has_newer = true);
							}
							if (this.result_success == 0 && !this.TargetTrm.watched_b)
							{
								this.TargetTrm.watched_b = true;
								TRMManager.need_fine = (TRMManager.need_recheck_has_newer = true);
							}
							this.t_ssc = 30f;
							flag2 = true;
						}
					}
				}
				else
				{
					this.need_ssc_redraw = true;
					this.t_ssc += fcnt;
				}
			}
			if (flag)
			{
				this.fineSscCartBtnScale();
			}
			if ((this.need_ssc_redraw && X.D) || flag2)
			{
				this.need_ssc_redraw = false;
				this.MdEf.clear(false, false);
				this.redrawSSC(X.AF);
				this.MdEf.updateForMeshRenderer(false);
			}
			return this.t_ssc >= 0f;
		}

		protected override bool deactivatable
		{
			get
			{
				return base.deactivatable && (this.result_success < 0 || this.t_ssc - 30f >= 50f);
			}
		}

		private void redrawSSC(int fcnt)
		{
			if (this.Fader == null)
			{
				return;
			}
			if (!this.MdEf.hasMultipleTriangle())
			{
				this.MdEf.chooseSubMesh(2, false, false);
				this.MdEf.setMaterial(MTR.MIiconL.getMtr(BLEND.NORMAL, -1), false);
				this.MdEf.connectRendererToTriMulti(base.GetComponent<MeshRenderer>());
			}
			if (this.MdEf.getSubMeshCount(false) < 4)
			{
				this.MdEf.chooseSubMesh(3, false, false);
				this.MdEf.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
			}
			this.MdEf.chooseSubMesh(3, false, true);
			this.MdEf.chooseSubMesh(2, false, true);
			this.MdEf.chooseSubMesh(1, false, true);
			this.MdEf.chooseSubMesh(0, false, false);
			float num;
			float num2;
			if (this.stt != UiCraftBase.STATE._NOUSE)
			{
				if (this.t_ssc >= 0f)
				{
					num = X.ZLINE(this.t_ssc, 30f);
					this.TxKDSsc.alpha = num;
				}
				else
				{
					num = X.ZLINE(30f + this.t_ssc, 30f);
				}
				this.MdEf.Col = C32.MulA(4278190080U, num * 0.88f);
				this.MdEf.Rect(0f, 0f, IN.w + 200f, IN.h + 200f, false);
				if (this.t_shld != -1000f)
				{
					num2 = num * (X.BTW(0f, this.t_shld, 130f) ? (1f - 0.6f * X.ZLINE(this.t_shld, 91f)) : 1f);
				}
				else
				{
					num2 = num;
				}
			}
			else
			{
				num = 1f;
				num2 = 1f - X.ZLINE(this.t_ssc - 30f, 30.000002f);
			}
			this.MdEf.chooseSubMesh(2, false, false);
			this.MdEf.ColGrd.Set(uint.MaxValue);
			if (this.t_shld >= 130f && this.result_success == 0)
			{
				this.MdEf.ColGrd.blend(4281612866U, X.ZLINE(this.t_shld - 130f, 20f));
			}
			this.MdEf.Col = this.MdEf.ColGrd.mulA(num2).C;
			object obj = ((this.stt != UiCraftBase.STATE._NOUSE || this.t_ssc < 0f) ? this.CartBtn.transform.position : Vector3.zero);
			float x = this.CartBtn.transform.localScale.x;
			float num3 = x * this.CartBtn.icon_scale;
			object obj2 = obj;
			float num4 = obj2.x * 64f;
			float num5 = obj2.y * 64f;
			this.MdEf.RotaPF(num4 + x * this.CartBtn.icon_shift_x, num5 + x * this.CartBtn.icon_shift_y, num3, num3, 0f, this.CartBtn.IconPF, false, false, false, uint.MaxValue, false, 0);
			if (this.t_shld < 130f)
			{
				if (this.t_shld >= 0f)
				{
					float num6 = 250f + 300f * X.ZSINV(this.t_shld, 130f);
					int num7 = X.IntC((190f - this.EfpSscCharge.maxt_one) / (float)this.EfpSscCharge.count);
					this.MdEf.chooseSubMesh(3, false, false);
					this.EfpSscCharge.drawTo(this.MdEf, num4, num5, num6, num7, this.t_shld, 0f);
					return;
				}
			}
			else
			{
				this.MdEf.chooseSubMesh(0, false, false);
				float num8 = this.t_shld - 130f;
				this.MdEf.Col = C32.MulA((this.result_success == 1) ? 4294797757U : 4284128757U, num);
				if (this.stt != UiCraftBase.STATE._NOUSE)
				{
					if (this.Fader.isFinished())
					{
						this.MdEf.Rect(0f, 0f, IN.w + 200f, IN.h + 200f, false);
					}
					else
					{
						this.MdEf.base_px_y = this.icon_center_y;
						if (this.Fader.redraw(this.MdEf, (float)fcnt))
						{
							this.need_ssc_redraw = true;
						}
						this.MdEf.base_y = 0f;
					}
				}
				else
				{
					float num9 = X.ZPOWV(50f - (this.t_ssc - 30f), 50f);
					float num10 = IN.wh * 1.4142135f * 0.85f * num9;
					this.MdEf.Circle(0f, 0f, num10, 0f, false, 0f, 0f);
				}
				if (this.EfpSscAftCircle.drawTo(this.MdEf, num4, num5, 0f, (this.result_success == 1) ? 16777215 : 3422786, num8, 0f))
				{
					this.need_ssc_redraw = true;
				}
				this.MdEf.chooseSubMesh(2, false, false);
				if (((this.result_success == 1) ? this.EfpSscSuccess : this.EfpSscFailure).drawTo(this.MdEf, num4, num5, 0f, 0, num8, 0f))
				{
					this.need_ssc_redraw = true;
				}
			}
		}

		public float ssc_cartbtn_scale
		{
			get
			{
				float num = ((this.t_ssc >= 0f) ? X.ZLINE(this.t_ssc, 30f) : X.ZLINE(30f + this.t_ssc, 30f));
				if (this.t_ssc < 0f)
				{
					return 1f + num;
				}
				float num2;
				if (this.t_shld != -1000f)
				{
					num2 = 1f - 0.25f * ((this.t_shld < 0f) ? X.ZPOW(-this.t_shld, 30f) : X.ZLINE(this.t_shld, 130f));
				}
				else
				{
					num2 = 1f;
					num = X.ZSIN2(num);
				}
				if (this.t_shld >= 130f)
				{
					float num3 = this.t_shld - 130f;
					num2 += 0.75f * X.ZSIN(num3, 20f) - 0.5f * X.ZCOS(num3 - 20f, 60f);
				}
				return (1f + num) * num2;
			}
		}

		private void confirmTrmFinal(int success = -1)
		{
			if (success < 0)
			{
				this.result_success = ((NightController.xors(100) < (int)this.success_ratio100) ? 1 : 0);
			}
			else
			{
				this.result_success = (int)((byte)success);
			}
			this.CartBtn.maxt_pos = 30;
			this.CartBtn.position(0f, 0f, -1000f, -1000f, false);
			SND.Ui.play((this.result_success == 1) ? "trm_alchemy_success" : "trm_alchemy_failure", false);
			this.Fader.resetAnim(TFANIM.EXPAND, false, 0f, 0);
			this.TxKDSsc.text_content = TX.Get((this.result_success == 1) ? "TrmUi_root_success" : "TrmUi_root_failure", "");
			if (this.stt != UiCraftBase.STATE.RECIPE_TOPIC && this.CompletionImage != null && this.CompletionImage.ItemData != null)
			{
				int num = this.AStorage.Length;
				for (int i = 0; i < num; i++)
				{
					this.AStorage[i].Reduce(this.CompletionImage.ItemData, 99, -1, true);
				}
				if ((this.result_success == 1) ? (!this.TargetTrm.watched_a) : (!this.TargetTrm.watched_b))
				{
					TRMManager.CurrentReward = new TRMManager.TRMReward(this.TargetTrm, this.CompletionImage.calced_grade, this.result_success == 1);
				}
				else
				{
					TRMManager.CurrentReward = null;
				}
			}
			else
			{
				TRMManager.CurrentReward = null;
			}
			this.t_shld = 130f;
			if (this.BxManual != null)
			{
				this.BxManual.deactivate();
			}
		}

		protected bool fnManualRootSelected(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (cur_value < 0)
			{
				return true;
			}
			string title = _B.Get(cur_value).title;
			if (title != null && title == "&&Cancel")
			{
				this.quitLunch(true);
			}
			else
			{
				this.confirmTrmFinal((title == "root_a") ? 1 : 0);
			}
			return true;
		}

		protected override IDesignerBlock getTutorialArrowTarget(ref AIM setaim)
		{
			if (this.evwait == UiCraftBase.EVWAIT.TRM_TT)
			{
				setaim = AIM.B;
				return this.BxTT;
			}
			return base.getTutorialArrowTarget(ref setaim);
		}

		public TRMManager.TRMItem CurrentTargetTrmItem
		{
			get
			{
				return this.TargetTrm;
			}
		}

		public override RCP.Recipe getRecipe(string key)
		{
			return UiAlchemyTRM.getRecipeS(key);
		}

		public static RCP.Recipe getRecipeS(string key)
		{
			TRMManager.TRMItem trmFromItem = UiAlchemyTRM.GetTrmFromItem(key);
			if (trmFromItem != null)
			{
				return trmFromItem.TRecipe;
			}
			return null;
		}

		public static TRMManager.TRMItem GetTrmFromItem(string key)
		{
			if (TX.isStart(key, "TrmItem_", 0))
			{
				return TRMManager.Get(TX.slice(key, "TrmItem_".Length), false);
			}
			return null;
		}

		public override RCP.Recipe getRecipe(NelItem Itm)
		{
			return UiAlchemyTRM.getRecipeS(Itm);
		}

		public static RCP.Recipe getRecipeS(NelItem Itm)
		{
			return UiAlchemyTRM.getRecipeS(Itm.key);
		}

		private NelItemManager IMNG;

		private ItemStorage Inventory;

		private TRMManager.TRMItem TargetTrm;

		private static UiCraftBase.AutoCreationInfo CInfoDef;

		private UiBoxDesigner BxTT;

		private const float tt_h = 80f;

		public const uint col_recommend_pink = 4294410942U;

		private byte[] Atarget_hpow;

		private float[] Adraw_hpow;

		private byte[] Acur_hpow;

		private FillImageBlock FiBCompGraph;

		public static UiAlchemyTRM Instance;

		private int result_success = -1;

		private const float power_max = 6f;

		private byte success_ratio100;

		private string[] Aingredient_bad = new string[] { "mtr_weed0" };

		private float t_ssc = -30f;

		private const float MAXT_SSC_FADE = 30f;

		private const float MAXT_SSC_DEACTV_FADE = 50f;

		private float t_shld;

		private const float MAXT_TSHLD_HOLD = 130f;

		private const float MAXT_TSHLD_HOLD_CANCEL = 30f;

		private const float SHLD_MANUAL_SELECT = -1000f;

		private M2SoundPlayerItem SndHold;

		private TransFader Fader;

		private EfParticleOnce EfpSscCharge;

		private EfParticleOnce EfpSscAftCircle;

		private EfParticleOnce EfpSscSuccess;

		private EfParticleOnce EfpSscFailure;

		private bool need_ssc_redraw;

		private TextRenderer TxKDSsc;

		private UiBoxDesigner BxManual;

		private BtnContainer<aBtn> BConManual;

		private float icon_center_y;
	}
}

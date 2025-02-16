using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class UiItemStore : UiBoxDesignerFamily, IEventWaitListener
	{
		private float money_w
		{
			get
			{
				return this.out_w * 0.65f;
			}
		}

		private float money_y
		{
			get
			{
				return -0.5f * this.out_h + this.BxMoney.get_sheight_px() * 0.5f;
			}
		}

		private float cartbtn_x
		{
			get
			{
				return -this.out_w * 0.5f + 60f;
			}
		}

		private float cartbtn_y
		{
			get
			{
				return this.money_y + 20f;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this.auto_deactive_gameobject = false;
			base.gameObject.layer = IN.gui_layer;
			this.M2D = M2DBase.Instance as NelM2DBase;
			this.OFirstData = new BDic<NelItem, ItemStorage.ObtainInfo[]>();
			this.prepareBoxes();
			this.deactivate(true);
		}

		protected virtual void prepareBoxes()
		{
			this.BxC = base.Create("main_cmd", 0f, 0f, 380f, 60f, 0, 0f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxC.DelayT(8);
			this.BxC.anim_time(22);
			this.BxC.Focusable(true, false, null);
			this.BxC.margin_in_tb = 30f;
			this.BxC.margin_in_lr = 20f;
			this.BxC.btn_height = 30f;
			this.BxC.item_margin_y_px = 1f;
			this.BxC.selectable_loop |= 2;
			this.BxC.getBox().frametype = UiBox.FRAMETYPE.MAIN;
			this.BxC.animate_maxt = 0;
			this.BxC.alignx = ALIGN.CENTER;
			this.BxMoney = base.Create("money", 0f, 0f, 16f, 46f, 0, 0f, UiBoxDesignerFamily.MASKTYPE.NO_MASK);
			this.BxMoney.Small();
			this.BxMoney.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			this.BxMoney.margin(new float[] { 60f, 3f });
			this.BxMoney.positionD(0f, this.money_y, 3, 30f);
			this.M2D.FlagValotStabilize.Add("Store");
			UiItemStore.prepareOneSet(this, this.out_w, this.out_h, this.right_h, this.right_h_margin, ref this.BxR, ref this.BxDesc, ref this.BxCmd, 0f, 0f);
			this.CartBtn = IN.CreateGob(base.gameObject, "-cartbtn").AddComponent<aBtnNelPopper>();
			this.CartBtn.initializeSkin("popper", "pict_cart");
			this.CartBtn.position(this.cartbtn_x - 240f, this.cartbtn_y, this.cartbtn_x, this.cartbtn_y, false);
			this.CartBtn.addClickFn(new FnBtnBindings(this.fnClickCartBtn));
			IN.setZ(this.CartBtn.transform, -0.125f);
		}

		public static void prepareOneSet(UiBoxDesignerFamily Con, float out_w, float out_h, float right_h, float right_h_margin, ref UiBoxDesigner BxR, ref UiBoxDesigner BxDesc, ref UiBoxDesigner BxCmd, float shiftx = 0f, float shifty = 0f)
		{
			float num = out_w - 360f - 26f;
			BxR = Con.Create("right", shiftx + out_w * 0.5f - num * 0.5f, shifty + out_h * 0.5f - right_h * 0.5f - right_h_margin, num, right_h, 1, IN.h, UiBoxDesignerFamily.MASKTYPE.BOX);
			BxR.margin_in_lr = 28f;
			BxR.item_margin_x_px = 0f;
			BxR.item_margin_y_px = 0f;
			BxR.animate_maxt = 0;
			BxR.alignx = ALIGN.LEFT;
			BxR.use_scroll = false;
			BxDesc = Con.Create("desc", shiftx - out_w * 0.5f + 180f, shifty + out_h * 0.5f - right_h * 0.5f - right_h_margin, 360f, right_h, 1, IN.h, UiBoxDesignerFamily.MASKTYPE.BOX);
			BxDesc.animate_maxt = 0;
			BxDesc.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			BxCmd = Con.Create("cmd", 0f, shifty, 390f, 120f, 0, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			BxCmd.alignx = ALIGN.CENTER;
		}

		public override T CreateT<T>(string name, float pixel_x, float pixel_y, float pixel_w, float pixel_h, int appear_dir_aim = -1, float appear_len = 30f, UiBoxDesignerFamily.MASKTYPE mask = UiBoxDesignerFamily.MASKTYPE.BOX)
		{
			T t = base.CreateT<T>(name, pixel_x, pixel_y, pixel_w, pixel_h, appear_dir_aim, appear_len, UiBoxDesignerFamily.MASKTYPE.NO_MASK);
			t.Focusable(false, false, null);
			t.WHanim(t.get_swidth_px(), t.get_sheight_px(), false, false);
			return t;
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || this.active;
		}

		public void InitManager(StoreManager Mng, ItemStorage[] _AInventory = null)
		{
			this.ReleaseStorage();
			CsvVariableContainer variableContainer = EV.getVariableContainer();
			variableContainer.define("_buy_money", "0", true);
			variableContainer.define("_sell_money", "0", true);
			IN.setZ(base.transform, -5.05f);
			this.MngProduct = Mng;
			this.FD_fnSortInStore = delegate(ItemStorage.IRow Ra, ItemStorage.IRow Rb, ItemStorage.SORT_TYPE sort_type, out int ret)
			{
				return Mng.fnSortInStore(Ra, Rb, sort_type, out ret, this.buying_mode);
			};
			string text;
			string text2;
			this.StProduct = Mng.CreateItemStorage(out text, out text2);
			this.StProductFirst = new ItemStorage(this.StProduct.key, 99);
			this.StProductFirst.readBinaryFrom(this.StProduct.writeBinaryTo(new ByteArray(0U)).SeekSet(), true, true, false, 9, false);
			ItemStorage[] array;
			if (_AInventory != null && _AInventory.Length != 0)
			{
				array = global::XX.X.concat<ItemStorage>(_AInventory, null, -1, -1);
			}
			else if (this.M2D != null)
			{
				array = global::XX.X.concat<ItemStorage>(this.M2D.IMNG.getInventoryArray(), null, -1, -1);
			}
			else
			{
				array = new ItemStorage[]
				{
					new ItemStorage("_temp", 99)
				};
			}
			BDic<NelItem, ItemStorage.ObtainInfo> wholeInfoDictionary = this.StProduct.getWholeInfoDictionary();
			int num = array.Length;
			int num2 = (1 << num) - 1;
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				ItemStorage itemStorage = array[i];
				if (this.isForceTargetInventory(itemStorage))
				{
					num3 |= 1 << i;
				}
			}
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in wholeInfoDictionary)
			{
				for (int j = 0; j < num; j++)
				{
					if ((num3 & (1 << j)) == 0 && array[j].isAddable(keyValuePair.Key))
					{
						num3 |= 1 << j;
					}
				}
				if (num3 == num2)
				{
					break;
				}
			}
			for (int k = num - 1; k >= 0; k--)
			{
				if ((num3 & (1 << k)) == 0)
				{
					global::XX.X.spliceEmpty<ItemStorage>(array, k, 1);
					num--;
				}
			}
			if (num == 0)
			{
				if (_AInventory != null && _AInventory.Length != 0)
				{
					array = global::XX.X.concat<ItemStorage>(_AInventory, null, -1, -1);
				}
				else if (this.M2D != null)
				{
					array = this.M2D.IMNG.getInventoryIngredientArray(false);
				}
				else
				{
					array = new ItemStorage[]
					{
						new ItemStorage("_temp", 99)
					};
				}
				num = array.Length;
			}
			else
			{
				Array.Resize<ItemStorage>(ref array, num);
			}
			this.icon_max = 1;
			List<UiItemStore.StTab> list = new List<UiItemStore.StTab>(num);
			for (int l = 0; l < num; l++)
			{
				int num4 = 0;
				string text3 = "IconNoel0";
				string key = array[l].key;
				if (this.M2D != null && array[l] == this.M2D.IMNG.getHouseInventory())
				{
					text3 = "house_inventory";
					num4 = 1;
				}
				this.icon_max = global::XX.X.Mx(this.icon_max, num4 + 1);
				list.Add(new UiItemStore.StTab(key, array[l], text3, num4));
			}
			this.AStInventory = list.ToArray();
			this.AStFirstData = new ByteArray[this.AStInventory.Length];
			for (int m = this.AStInventory.Length - 1; m >= 0; m--)
			{
				this.AStFirstData[m] = this.AStInventory[m].St.writeBinaryTo(new ByteArray(0U)).SeekSet();
			}
			this.ItemMng = new UiItemManageBoxSlider((this.M2D != null) ? this.M2D.IMNG : null, null)
			{
				slice_height = 70f,
				fnCommandPrepare = new UiItemManageBox.FnCommandPrepare(this.fnUsingCommandPrepareIncrease),
				cmd_w = 390f,
				hide_list_buttons_when_using = true,
				fnDetailPrepare = delegate(NelItem Itm, ItemStorage.ObtainInfo Obt, ItemStorage.IRow _Row)
				{
					this.fineUnderKD();
				},
				fnDescAddition = new UiItemManageBox.FnDescAddition(this.fnDescAddition),
				fnGradeFocusChange = new UiItemManageBox.FnGradeFocusChange(this.fnGradeFocusChange),
				fnItemRowInitAfter = new UiItemManageBox.FnItemRowInitAfter(this.itemRowInit),
				item_row_skin = "store",
				grade_text_area_width = 110f,
				topright_desc_width = 140f,
				TrsEvacuateTo = base.transform
			};
			this.StBuy = new ItemStorage("item_tab_cart_buy", 99).clearAllItems(99);
			this.StSell = new ItemStorage("item_tab_cart_sell", 99).clearAllItems(99);
			this.StBuy.infinit_stockable = (this.StBuy.water_stockable = (this.StBuy.grade_split = (this.StBuy.check_quest_target = (this.StSell.infinit_stockable = (this.StSell.water_stockable = (this.StSell.grade_split = (this.StSell.check_quest_target = true)))))));
			this.StBuy.auto_splice_zero_row = (this.StSell.auto_splice_zero_row = false);
			this.st_index = 0;
			this.BxMoney.activate();
			this.fine_item_count = true;
			this.active = true;
			base.gameObject.SetActive(true);
			for (int n = 0; n < 2; n++)
			{
				string text4 = ((n == 0) ? text : text2);
				float num5 = ((n == 0) ? Mng.temp_buy_ratio : Mng.temp_sell_ratio);
				if (text4 != null)
				{
					FillImageBlock fillImageBlock = IN.CreateGob(base.gameObject, (n == 0) ? "-ServiceBuy" : "-ServiceSell").AddComponent<FillImageBlock>();
					if (n == 0)
					{
						this.FbServiceBuy = fillImageBlock;
					}
					else
					{
						this.FbServiceSell = fillImageBlock;
					}
					fillImageBlock.TxCol = C32.d2c((num5 > 1f == (n == 0)) ? 4281403657U : 4279831637U);
					fillImageBlock.Col = C32.d2c((num5 > 1f == (n == 0)) ? 4293296550U : 4287998963U);
					fillImageBlock.widthPixel = (float)(global::XX.X.ENG_MODE ? 185 : 200);
					fillImageBlock.heightPixel = (float)(global::XX.X.ENG_MODE ? 60 : 30);
					fillImageBlock.size = 14f;
					fillImageBlock.alignx = ALIGN.CENTER;
					fillImageBlock.aligny = ALIGNY.MIDDLE;
					fillImageBlock.FnDrawFIB = new FillImageBlock.FnDrawInFIB(this.FnDrawServiceFib);
					fillImageBlock.StartFb(TX.GetA(text4, (global::XX.X.ENG_MODE ? "\n" : "") + ((int)global::XX.X.Abs(num5 * 100f - 100f)).ToString()), null, true);
					IN.setZ(fillImageBlock.transform, -0.1f);
					fillImageBlock.gameObject.SetActive(false);
				}
			}
			if (this.stt == UiItemStore.STATE._NOUSE)
			{
				this.changeState(UiItemStore.STATE.TOP);
			}
		}

		protected virtual bool isForceTargetInventory(ItemStorage St)
		{
			return St.key == "Inventory_noel" || St.key == "Inventory_house";
		}

		private void ReleaseStorage()
		{
			this.abortCheckout();
			if (this.MngProduct != null)
			{
				this.MngProduct.releasePremireCache();
			}
			if (this.ItemMng != null)
			{
				this.ItemMng.quitDesigner(false, true);
			}
			if (this.StProduct == null)
			{
				return;
			}
			this.StProduct.fineRows(true);
			this.StProduct = null;
		}

		protected virtual void changeState(UiItemStore.STATE st)
		{
			UiItemStore.STATE state = this.stt;
			this.stt = st;
			this.CartBtn.resetPopPitch();
			IN.clearPushDown(true);
			if (state != UiItemStore.STATE.TOP)
			{
				if (state - UiItemStore.STATE.BUY <= 2)
				{
					if (this.stt == UiItemStore.STATE.TOP)
					{
						this.BxR.Focusable(false, false, null);
						this.BxDesc.deactivate();
						this.BxR.deactivate();
						SND.Ui.play("cancel", false);
					}
					this.ItemMng.quitDesigner(false, true);
					this.saveStorageTouchedGrade();
					this.StBuy.fineRows(true);
					this.StSell.fineRows(true);
					this.FbUnderKD = null;
				}
			}
			else
			{
				if (this.stt == UiItemStore.STATE.CHECKOUT)
				{
					this.CartBtn.hide();
					this.CartBtn.position(this.cartbtn_x - 240f, this.cartbtn_y, -1000f, -1000f, false);
				}
				if (this.stt == UiItemStore.STATE._NOUSE)
				{
					this.CartBtn.hide();
				}
			}
			if (this.stt != UiItemStore.STATE._NOUSE)
			{
				this.initCommandWindow();
			}
			else
			{
				this.BxC.deactivate();
			}
			switch (this.stt)
			{
			case UiItemStore.STATE._NOUSE:
				this.ItemMng.quitDesigner(false, true);
				this.deactivate(false);
				this.ReleaseStorage();
				this.CartBtn.hide();
				break;
			case UiItemStore.STATE.TOP:
			{
				if (state == UiItemStore.STATE.CHECKOUT || state == UiItemStore.STATE._NOUSE)
				{
					this.initMoneyWindow();
				}
				if (state == UiItemStore.STATE._NOUSE || state == UiItemStore.STATE.CHECKOUT)
				{
					this.CartBtn.bind();
				}
				if (state == UiItemStore.STATE.CHECKOUT)
				{
					this.CartBtn.position(this.cartbtn_x, this.cartbtn_y, -1000f, -1000f, false);
				}
				this.BxC.activate();
				BtnContainerRunner btnContainerRunner = this.BxC.Get("cmd_top", false) as BtnContainerRunner;
				if (state == UiItemStore.STATE._NOUSE || state == UiItemStore.STATE.BUY)
				{
					btnContainerRunner.Get("&&cmd_buy").Select(false);
				}
				else if (state == UiItemStore.STATE.SELL)
				{
					btnContainerRunner.Get("&&cmd_sell").Select(false);
				}
				else if (state == UiItemStore.STATE.CHECK_CART)
				{
					btnContainerRunner.Get("&&cmd_cart").Select(false);
				}
				else
				{
					btnContainerRunner.Get("&&cmd_checkout").Select(false);
				}
				btnContainerRunner.Get("&&cmd_cart").SetLocked(this.isCartEmpty(), true, false);
				break;
			}
			case UiItemStore.STATE.CHECKOUT:
				this.initMoneyWindow();
				break;
			case UiItemStore.STATE.BUY:
			case UiItemStore.STATE.SELL:
			case UiItemStore.STATE.CHECK_CART:
				if (state == UiItemStore.STATE.TOP)
				{
					this.initItemWindow();
				}
				if (this.stt == UiItemStore.STATE.CHECK_CART)
				{
				}
				break;
			}
			this.need_recheck_list_count_whole = false;
			this.fineServiceFIB();
			this.fineFocusWindow();
		}

		private void fineFocusWindow()
		{
			if (this.stt == UiItemStore.STATE.TOP)
			{
				this.BxC.Focus();
				return;
			}
			if (this.stt != UiItemStore.STATE.CHECKOUT && this.stt != UiItemStore.STATE._NOUSE)
			{
				this.BxR.Focus();
			}
		}

		private void initCommandWindow()
		{
			this.BxC.Clear();
			this.BxC.alignx = ALIGN.CENTER;
			this.BxC.selectable_loop = 3;
			this.BxC.use_button_connection = true;
			if (this.stt == UiItemStore.STATE.TOP)
			{
				this.BxC.getBox().frametype = UiBox.FRAMETYPE.MAIN;
				this.BxC.WHanim(380f, 180f, true, true);
				this.BxC.margin_in_tb = 30f;
				this.BxC.margin_in_lr = 20f;
				this.BxC.Focusable(true, false, null);
				this.BxC.init();
				this.BxC.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
				{
					clms = 1,
					name = "cmd_top",
					titles = new string[] { "&&cmd_buy", "&&cmd_sell", "&&cmd_cart", "&&cmd_checkout" },
					margin_h = 0f,
					margin_w = 0f,
					hover_to_select = true,
					navi_auto_fill = true,
					h = 30f,
					w = this.BxC.use_w,
					navi_loop = 2,
					fnClick = new FnBtnBindings(this.fnClickCmdTop),
					skin = "row_center"
				}).Get("&&cmd_checkout").click_snd = "";
				this.BxC.position(0f, IN.hh + this.BxC.get_sheight_px() * 0.5f + 40f, 0f, 40f, false);
				return;
			}
			this.BxC.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			this.BxC.WHanim(646f, 46f, true, true);
			this.BxC.margin_in_lr = 48f;
			this.BxC.margin_in_tb = 6f;
			this.BxC.Focusable(false, false, null);
			this.BxC.init();
			this.BxC.addP(new DsnDataP("", false)
			{
				name = "money",
				text = ((this.stt == UiItemStore.STATE.CHECK_CART) ? TX.Get("cmd_cart", "") : TX.Get("Store_title_" + this.stt.ToString().ToLower(), "")),
				alignx = ALIGN.CENTER,
				html = true,
				size = 20f,
				swidth = this.BxC.use_w - 5f,
				sheight = this.BxC.use_h,
				TxCol = C32.d2c(4283780170U)
			}, false);
			if (this.stt == UiItemStore.STATE.CHECKOUT)
			{
				this.BxC.position(0f, 240f, -1000f, -1000f, false);
				return;
			}
			this.BxC.position(0f, this.out_h * 0.5f + 14f, -1000f, -1000f, false);
		}

		private void initMoneyWindow()
		{
			this.BxMoney.Clear();
			this.BxMoney.item_margin_y_px = 2f;
			this.BxMoney.item_margin_x_px = 0f;
			this.BxMoney.selectable_loop = 3;
			this.BxMoney.use_button_connection = true;
			if (this.stt != UiItemStore.STATE.CHECKOUT)
			{
				this.BxMoney.WHanim(this.money_w, 46f, true, true);
				this.BxMoney.margin_in_lr = 44f;
				this.BxMoney.margin_in_tb = 3f;
				this.BxMoney.alignx = ALIGN.LEFT;
				this.BxMoney.init();
				this.BxMoney.position(0f, this.money_y, -1000f, -1000f, false);
				this.FbMoney = this.BxMoney.addP(new DsnDataP("", false)
				{
					name = "money",
					text = " ",
					alignx = ALIGN.LEFT,
					html = true,
					size = 16f,
					swidth = this.money_w - 60f,
					sheight = 40f,
					TxCol = C32.d2c(4283780170U)
				}, false);
				this.fine_item_count = true;
				return;
			}
			this.BxMoney.position(0f, 30f, -1000f, -1000f, false);
			this.BxMoney.WHanim(this.money_w * 1.25f, 260f, true, true);
			this.BxMoney.margin_in_lr = 40f;
			this.BxMoney.margin_in_tb = 34f;
			this.BxMoney.alignx = ALIGN.CENTER;
			this.BxMoney.init();
			int num;
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					UiItemStore.StoreResult storeResult = this.calcStoreResult(null, stb2);
					stb.Add(this.str_money_icon, this.coin_count, "");
					stb.Add("\n   ").AddTxA("buying_price_count", false);
					if (storeResult.buy_money > 0)
					{
						using (STB stb3 = TX.PopBld(null, 0))
						{
							stb.TxRpl(stb3.Add("<font color=\"ff:#DE148D\">").Add("-", storeResult.buy_money, "</font>"));
							goto IL_025F;
						}
					}
					stb.TxRpl(storeResult.buy_money);
					IL_025F:
					if (storeResult.buy_count > 0)
					{
						stb.Add(" ").AddTxA("Store_checkout_product", false).TxRpl(storeResult.buy_count);
					}
					stb.Add("\n   ").AddTxA("selling_price_count", false);
					if (storeResult.sell_money > 0)
					{
						using (STB stb4 = TX.PopBld(null, 0))
						{
							stb.TxRpl(stb4.Add("<font color=\"ff:#1335DF\">").Add("+", storeResult.sell_money, "</font>"));
							goto IL_02FB;
						}
					}
					stb.TxRpl(storeResult.sell_money);
					IL_02FB:
					if (storeResult.sell_count > 0)
					{
						stb.Add(" ").AddTxA("Store_checkout_selling", false).TxRpl(storeResult.sell_count);
					}
					num = this.coin_count + storeResult.money_addition;
					stb.Add("\n    = ").AddTxA("Store_checkout_after", false).TxRpl(stb2);
					if (num < 0)
					{
						stb.Add("<font color=\"ff:#DE148D\">", TX.Get(this.tx_key_not_have_enough_money, ""), "</font>");
					}
					this.FbMoney = this.BxMoney.addP(new DsnDataP("", false)
					{
						name = "money",
						Stb = stb,
						alignx = ALIGN.LEFT,
						aligny = ALIGNY.MIDDLE,
						html = true,
						size = 20f,
						swidth = this.BxMoney.use_w - 60f,
						sheight = 16f,
						TxCol = C32.d2c(4283780170U)
					}, false);
				}
			}
			this.BxMoney.Hr(0.88f, 18f, 26f, 1f).Br();
			BtnContainer<aBtn> btnContainer = this.BxMoney.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				clms = 2,
				name = "cmd_checkout",
				titles = new string[] { "&&cmd_checkout_submit", "&&cmd_checkout_abort", "&&Cancel" },
				navi_loop = 3,
				margin_h = 0f,
				margin_w = 0f,
				hover_to_select = true,
				navi_auto_fill = true,
				h = 30f,
				w = (this.BxMoney.use_w - 6f) * 0.5f,
				fnClick = new FnBtnBindings(this.fnClickCheckout),
				skin = "row_center"
			});
			btnContainer.Get("&&cmd_checkout_abort").click_snd = "cancel";
			aBtn aBtn = btnContainer.Get("&&cmd_checkout_submit");
			if (num < 0)
			{
				aBtn.SetLocked(true, true, false);
				btnContainer.Get("&&Cancel").Select(false);
				return;
			}
			(aBtn.get_Skin() as ButtonSkinNelUi).hilighted = true;
			aBtn.Select(false);
		}

		private void initItemWindow()
		{
			this.BxDesc.activate();
			this.BxR.activate();
			this.BxR.Focusable(true, false, null);
			ItemStorage[] array = null;
			UiItemStore.STATE state = this.stt;
			if (state != UiItemStore.STATE.BUY)
			{
				if (state != UiItemStore.STATE.SELL)
				{
					this.AStCur = new UiItemStore.StTab[]
					{
						new UiItemStore.StTab("item_tab_cart_buy", this.StBuy, "", 0),
						new UiItemStore.StTab("item_tab_cart_sell", this.StSell, "", 0)
					};
				}
				else
				{
					this.AStCur = this.AStInventory;
				}
			}
			else
			{
				this.AStCur = new UiItemStore.StTab[]
				{
					new UiItemStore.StTab("item_tab_product", this.StProduct, "", 0)
				};
				array = new ItemStorage[] { this.StProductFirst };
			}
			int num = 0;
			this.ItemMng.quitDesigner(false, true);
			this.BxR.Clear();
			this.RTabBar = null;
			float use_w = this.BxR.use_w;
			if (this.AStCur.Length >= 2)
			{
				int num2 = 0;
				while (num2 < this.AStCur.Length && this.AStCur[num % this.AStCur.Length].St.getVisibleRowCount() == 0)
				{
					num++;
					num2++;
				}
				this.st_index = num % this.AStCur.Length;
				this.RTabBar = ColumnRow.CreateT<aBtnNel>(this.BxR, "ctg_tab", "row_tab", this.st_index, this.getItemTabKeys(), new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnItemTabChanged), use_w, 0f, false, false).LrInput(false);
			}
			else
			{
				this.st_index = 0;
			}
			this.ItemTab = this.BxR.addTab("status_area", this.BxR.use_w, this.BxR.use_h - 10f, this.BxR.use_w, this.BxR.use_h - 10f, false);
			this.ItemTab.Smallest();
			this.ItemTab.margin_in_tb = 6f;
			this.ItemMng.slice_height = 0f;
			this.BxR.endTab(false);
			this.BxR.Br();
			this.FbUnderKD = this.BxR.addP(new DsnDataP("", false)
			{
				html = true,
				text = " ",
				size = 14f,
				TxCol = C32.d2c(4283780170U),
				swidth = this.BxR.use_w - 15f,
				alignx = ALIGN.CENTER
			}, false);
			this.initItemTab(true);
			this.ItemMng.use_touch_grade_memory = true;
			this.ItemMng.ASupportStorage = array;
			this.fineUnderKD();
		}

		private void initItemTab(bool clear = false)
		{
			Designer itemTab = this.ItemTab;
			if (clear)
			{
				this.ItemMng.quitDesigner(false, true);
				itemTab.Clear();
			}
			ItemStorage st = this.AStCur[this.st_index].St;
			this.saveStorageTouchedGrade();
			this.ItemMng.title_text_content = "";
			this.ItemMng.stencil_ref = 239;
			this.ItemMng.fnSortInjectMng = this.FD_fnSortInStore;
			UiItemStore.STATE state = this.stt;
			bool flag = state == UiItemStore.STATE.BUY || state == UiItemStore.STATE.SELL || this.buying_mode;
			this.ItemMng.use_topright_counter = flag;
			this.ItemMng.APoolEvacuated = this.APoolEvacuated;
			this.ItemMng.initDesigner(st, itemTab, this.BxDesc, this.BxCmd, true, true, true);
			this.APoolEvacuated = this.ItemMng.APoolEvacuated;
			IN.setZ(this.BxCmd.transform, -0.5f);
			this.ItemMng.ParentBoxDesigner = this.BxR;
		}

		private void saveStorageTouchedGrade()
		{
		}

		public override bool runIRD(float fcnt)
		{
			bool flag = base.runIRD(fcnt);
			if (!flag && this.stt == UiItemStore.STATE._NOUSE)
			{
				Object.Destroy(base.gameObject);
				this.destructed = true;
			}
			else if (this.can_progress_main_store)
			{
				if (this.isActive() && this.M2D.isRbkPD())
				{
					if (this.RecipeBook != null)
					{
						IN.DestroyOne(this.RecipeBook.gameObject);
					}
					UiFieldGuide.NextRevealAtAwake = null;
					if (this.isItemChooseState())
					{
						if (this.ItemMng.isUsingState())
						{
							this.ItemMng.changeStateToSelect();
						}
						UiFieldGuide.NextRevealAtAwake = this.ItemMng.SelectingTarget;
					}
					this.Rbk_PreSelected = aBtn.PreSelected;
					this.RecipeBook = new GameObject("RBK" + IN.totalframe.ToString()).AddComponent<UiFieldGuide>();
					SND.Ui.play("tool_hand_init", false);
					this.deactivate(false);
					this.CartBtn.hide();
					this.active = true;
					IN.setZAbs(this.RecipeBook.transform, base.transform.position.z - 0.125f);
					this.opening_recipe_book = true;
				}
				else
				{
					if (this.fine_item_count && this.stt != UiItemStore.STATE.CHECKOUT)
					{
						this.fine_item_count = false;
						if (this.FbPrice != null)
						{
							this.FbPrice.text_content = this.getPriceIncreaseString(this.ItemMng.UsingTarget, this.ItemMng.get_grade_cursor());
						}
						using (STB stb = TX.PopBld(null, 0))
						{
							this.calcStoreResult(stb, null);
							this.FbMoney.Txt(stb);
						}
					}
					if (this.itemwindow_mode && this.FbPrice == null)
					{
						NelItem selectingTarget = this.ItemMng.SelectingTarget;
						if (selectingTarget != null)
						{
							if (this.stt != UiItemStore.STATE.CHECK_CART)
							{
								if (IN.isUiAddPD())
								{
									this.dropCartByAddKey(1, true);
								}
								if ((this.buying_mode ? this.StBuy.getCount(selectingTarget, -1) : this.StSell.getCount(selectingTarget, -1)) > 0 && IN.isUiRemPD())
								{
									this.dropCartByAddKey(-1, false);
								}
							}
							else if (IN.isUiRemPD())
							{
								this.dropCartByAddKey(-1, true);
							}
						}
					}
					this.runServiceFIB(fcnt, this.stt == UiItemStore.STATE.BUY || this.stt == UiItemStore.STATE.SELL);
					UiItemStore.STATE state = this.stt;
					if (state - UiItemStore.STATE.BUY <= 2)
					{
						if (this.need_recheck_list_count_whole)
						{
							if (this.ItemMng.isUsingState())
							{
								NelItem usingTarget = this.ItemMng.UsingTarget;
								if (usingTarget != null)
								{
									using (BList<aBtnItemRow> blist = this.ItemMng.Inventory.PopGetItemRowBtnsFor(usingTarget))
									{
										for (int i = blist.Count - 1; i >= 0; i--)
										{
											this.recheckExistCount(blist[i]);
										}
									}
									this.need_recheck_list_count_whole = false;
								}
							}
							if (this.need_recheck_list_count_whole)
							{
								for (int j = this.ItemMng.Inventory.getItemRowBtnCount() - 1; j >= 0; j--)
								{
									aBtnItemRow itemRowBtnByIndex = this.ItemMng.Inventory.getItemRowBtnByIndex(j);
									this.recheckExistCount(itemRowBtnByIndex);
								}
								this.need_recheck_list_count_whole = false;
							}
						}
						if (this.FbPrice == null)
						{
							this.itemTabLRrun();
						}
						if (!this.ItemMng.runEditItem())
						{
							this.changeState(UiItemStore.STATE.TOP);
						}
					}
					else if (IN.isCancel())
					{
						IN.clearCancelPushDown(false);
						string text = "cancel";
						UiItemStore.STATE state2 = this.stt;
						if (state2 != UiItemStore.STATE.TOP)
						{
							if (state2 == UiItemStore.STATE.CHECKOUT)
							{
								(this.BxMoney.Get("cmd_checkout", false) as BtnContainerRunner).Get("&&Cancel").ExecuteOnClick();
							}
						}
						else if (this.isCartEmpty())
						{
							this.changeState(UiItemStore.STATE._NOUSE);
						}
						else
						{
							text = "tool_hand_init";
							this.changeState(UiItemStore.STATE.CHECKOUT);
						}
						SND.Ui.play(text, false);
					}
					if (this.RecipeBook != null && !this.RecipeBook.gameObject.activeSelf)
					{
						IN.DestroyOne(this.RecipeBook.gameObject);
						this.RecipeBook = null;
					}
				}
			}
			else
			{
				this.runServiceFIB(fcnt, false);
				if (this.cancelable_pause_main_store)
				{
					if (this.RecipeBook != null)
					{
						this.RecipeBook.auto_deactive_gameobject = true;
					}
					this.opening_recipe_book = false;
					if (this.stt == UiItemStore.STATE.TOP)
					{
						this.BxC.activate();
						this.BxC.Focus();
					}
					this.fineServiceFIB();
					this.BxMoney.activate();
					this.CartBtn.bind();
					if (this.isItemChooseState())
					{
						this.BxR.activate();
						this.BxDesc.activate();
					}
					this.fineFocusWindow();
					if (this.Rbk_PreSelected != null)
					{
						this.Rbk_PreSelected.Select(false);
					}
					IN.clearSubmitPushDown(true);
				}
			}
			return flag;
		}

		protected virtual bool can_progress_main_store
		{
			get
			{
				return !this.opening_recipe_book;
			}
		}

		protected virtual bool cancelable_pause_main_store
		{
			get
			{
				return this.RecipeBook == null || !this.RecipeBook.isActive();
			}
		}

		private bool itemTabLRrun()
		{
			if (this.AStCur == null || this.AStCur.Length <= 1 || this.RTabBar == null || IN.isUiSortPD() || IN.isUiAddPD() || IN.isUiRemPD())
			{
				return true;
			}
			if (this.RTabBar.runLRInput(-2) && this.stt == UiItemStore.STATE.CHECK_CART)
			{
				this.fineUnderKD();
			}
			return true;
		}

		private bool fnItemTabChanged(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (this.st_index != cur_value)
			{
				this.st_index = cur_value;
				this.initItemTab(true);
			}
			return true;
		}

		private string fnDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW desc_row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int _default_count)
		{
			if (desc_row == UiItemManageBox.DESC_ROW.NAME)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Add("  <font size=\"12\">");
					this.M2D.IMNG.holdItemString(stb, Itm, -1, true);
					stb.Add("</font>");
					return def_string + stb.ToString();
				}
			}
			if (desc_row == UiItemManageBox.DESC_ROW.GRADE)
			{
				string text = "";
				string text2 = this.str_money_icon;
				if (this.buying_mode)
				{
					text += this.MngProduct.buyPrice(Itm, grade).ToString();
				}
				else
				{
					text += this.MngProduct.sellPrice(Itm, grade).ToString();
				}
				if (def_string == "0")
				{
					text2 = this.str_money_icon_zero;
					text = string.Concat(new string[]
					{
						"<font color=\"0x",
						C32.codeToCodeText(4288057994U),
						"\">",
						text,
						"</font>"
					});
				}
				int num = ((Obt != null) ? Obt.getCount(grade) : 0);
				int num2;
				if (this.buying_mode)
				{
					num2 = this.StProductFirst.getCount(Itm, grade);
				}
				else
				{
					ItemStorage.ObtainInfo[] array = global::XX.X.Get<NelItem, ItemStorage.ObtainInfo[]>(this.OFirstData, Itm);
					num2 = -1000;
					if (array != null)
					{
						int num3 = this.AStInventory.Length;
						for (int i = 0; i < num3; i++)
						{
							if (this.AStInventory[i].St.isAddable(Itm))
							{
								num2 = array[i].getCount(grade);
								break;
							}
						}
					}
				}
				if (num2 != -1000)
				{
					if (num < num2)
					{
						def_string = "<font color=\"ff:#DE148D\">" + def_string + "</font>";
					}
					else if (num > num2)
					{
						def_string = "<font color=\"ff:#1335DF\">" + def_string + "</font>";
					}
				}
				def_string = def_string + " :" + text2 + text;
				return def_string;
			}
			if (desc_row == UiItemManageBox.DESC_ROW.ROW_PRICE)
			{
				if (grade == -1)
				{
					if (Obt != null)
					{
						grade = Obt.min_grade;
					}
					else
					{
						grade = 0;
					}
				}
				if (this.buying_mode)
				{
					return this.str_money_icon + this.MngProduct.buyPrice(Itm, grade).ToString();
				}
				return this.str_money_icon + this.MngProduct.sellPrice(Itm, grade).ToString();
			}
			else if (desc_row == UiItemManageBox.DESC_ROW.ROW_COUNT)
			{
				if (this.buying_mode)
				{
					def_string = def_string + " <font size=\"10\">/" + global::XX.X.spr0(this.StProductFirst.getCount(Itm, grade), 2, ' ') + "</font>";
				}
			}
			else if (desc_row == UiItemManageBox.DESC_ROW.TOPRIGHT_COUNTER && this.buying_mode)
			{
				def_string = TX.Get("Store_topright_buying", "");
			}
			return def_string;
		}

		private string getInventoryCountString(NelItem Itm, int grade)
		{
			int[] array = new int[this.icon_max];
			string[] array2 = new string[this.icon_max];
			int num = this.AStInventory.Length;
			for (int i = 0; i < num; i++)
			{
				UiItemStore.StTab stTab = this.AStInventory[i];
				array[stTab.icon_id] += stTab.St.getCount(Itm, grade);
				if (array2[stTab.icon_id] == null)
				{
					array2[stTab.icon_id] = stTab.icon;
				}
			}
			string[] array3 = new string[this.icon_max];
			ItemStorage.ObtainInfo[] array4 = global::XX.X.Get<NelItem, ItemStorage.ObtainInfo[]>(this.OFirstData, Itm);
			for (int j = 0; j < this.icon_max; j++)
			{
				int num2 = array[j];
				string text = num2.ToString();
				if (array4 != null)
				{
					ItemStorage.ObtainInfo obtainInfo = array4[j];
					int num3 = ((grade < 0) ? obtainInfo.total : obtainInfo.getCount(grade));
					if (num2 > num3)
					{
						text = "<font color=\"ff:#1335DF\">" + text + "</font>";
					}
					else if (num2 < num3)
					{
						text = "<font color=\"ff:#DE148D\">" + text + "</font>";
					}
				}
				array3[j] = "<img mesh=\"" + array2[j] + "\" width=\"22\" height=\"24\" />x" + text;
			}
			return TX.GetA("inventory_count", TX.join<string>("/", array3, 0, -1));
		}

		private string getPriceIncreaseString(NelItem Itm, int grade)
		{
			int num = 5;
			int num2 = 0;
			ItemStorage.ObtainInfo obtainInfo = this.StBuy.getInfo(Itm);
			if (obtainInfo != null)
			{
				for (int i = 0; i < num; i++)
				{
					num2 -= obtainInfo.getCount(i) * this.MngProduct.buyPrice(Itm, i);
				}
			}
			obtainInfo = this.StSell.getInfo(Itm);
			if (obtainInfo != null)
			{
				for (int j = 0; j < num; j++)
				{
					num2 += obtainInfo.getCount(j) * this.MngProduct.sellPrice(Itm, j);
				}
			}
			string text = this.str_money_icon;
			if (num2 > 0)
			{
				text = text + "<font color=\"ff:#1335DF\">" + num2.ToString() + "</font>";
			}
			else if (num2 < 0)
			{
				text = text + "<font color=\"ff:#DE148D\">" + num2.ToString() + "</font>";
			}
			else
			{
				text += "0";
			}
			return text;
		}

		private UiItemStore.StoreResult calcStoreResult(STB Stb = null, STB StbA = null)
		{
			int num = 5;
			UiItemStore.StoreResult storeResult = default(UiItemStore.StoreResult);
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.StBuy.getWholeInfoDictionary())
			{
				for (int i = 0; i < num; i++)
				{
					int count = keyValuePair.Value.getCount(i);
					if (count != 0)
					{
						storeResult.buy_count += count;
						storeResult.buy_money += count * this.MngProduct.buyPrice(keyValuePair.Key, i);
					}
				}
			}
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair2 in this.StSell.getWholeInfoDictionary())
			{
				for (int j = 0; j < num; j++)
				{
					int count2 = keyValuePair2.Value.getCount(j);
					if (count2 != 0)
					{
						storeResult.sell_count += count2;
						storeResult.sell_money += count2 * this.MngProduct.sellPrice(keyValuePair2.Key, j);
					}
				}
			}
			int num2 = -storeResult.buy_money + storeResult.sell_money;
			if (Stb != null)
			{
				Stb.Add(this.str_money_icon, this.coin_count, "");
				if (storeResult.buy_money > 0)
				{
					Stb.Add("<font color=\"ff:#DE148D\">", " ").AddTxA("buying_price_count", false).TxRpl("-", (float)storeResult.buy_money, "")
						.Add("</font> ");
				}
				if (storeResult.sell_money > 0)
				{
					Stb.Add("<font color=\"ff:#1335DF\">", " ").AddTxA("selling_price_count", false).TxRpl("+", (float)storeResult.sell_money, "")
						.Add("</font>");
				}
			}
			if ((Stb != null || StbA != null) && (storeResult.buy_money > 0 || storeResult.sell_money > 0))
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					int num3 = this.coin_count + num2;
					stb.Add("<b><i>");
					if (num3 < 0)
					{
						stb.Add("<font color=\"ff:#DE148D\">", num3, "</font> ");
					}
					else
					{
						stb.Add(num3);
					}
					stb.Add("</i></b>");
					if (Stb != null)
					{
						Stb.Add(" = ").Add(stb);
					}
					if (StbA != null)
					{
						StbA.Add(this.str_money_icon).Add(stb);
					}
				}
			}
			return storeResult;
		}

		private void createFirstData(NelItem Itm)
		{
			if (!this.OFirstData.ContainsKey(Itm))
			{
				ItemStorage.ObtainInfo[] array = (this.OFirstData[Itm] = new ItemStorage.ObtainInfo[this.AStInventory.Length]);
				for (int i = 0; i < this.AStInventory.Length; i++)
				{
					ItemStorage.ObtainInfo obtainInfo = (array[i] = new ItemStorage.ObtainInfo());
					ItemStorage.ObtainInfo info = this.AStInventory[i].St.getInfo(Itm);
					if (info != null)
					{
						info.copyTo(obtainInfo);
					}
				}
			}
		}

		private bool fnUsingCommandPrepareIncrease(UiItemManageBox IMng, UiBoxDesigner BxCmd, aBtnItemRow BRow)
		{
			NelItem itemData = BRow.getItemData();
			ItemStorage.ObtainInfo itemInfo = BRow.getItemInfo();
			string text;
			if (this.stt == UiItemStore.STATE.SELL && this.cannotSell(itemData, itemInfo, out text) && this.StBuy.getCount(itemData, -1) == 0)
			{
				this.ItemMng.errorMessageToDesc(TX.Get(text, ""));
				return false;
			}
			this.createFirstData(itemData);
			this.CartBtn.resetPopPitch();
			BxCmd.WHanim(390f, 150f, true, true);
			BxCmd.margin_in_lr = 26f;
			BxCmd.margin_in_tb = 30f;
			BxCmd.item_margin_x_px = 0f;
			BxCmd.item_margin_y_px = 2f;
			BxCmd.init();
			BxCmd.addP(new DsnDataP("", false)
			{
				TxCol = C32.d2c(4283780170U),
				name = "store_prompt",
				text = TX.Get(this.buying_mode ? "Store_prompt_title_buy" : "Store_prompt_title_sell", ""),
				size = 16f,
				swidth = BxCmd.use_w,
				sheight = 28f,
				html = true,
				aligny = ALIGNY.MIDDLE,
				alignx = ALIGN.CENTER
			}, false);
			BxCmd.Br();
			aBtnMeterNel aBtnMeterNel = this.ItemMng.createSlider(new UiItemManageBoxSlider.DsnDataSliderIMB(new UiItemManageBoxSlider.FnGetMeterVariable(this.fnGetMeterVariable), new UiItemManageBoxSlider.FnSlidedMeter(this.fnMeterSlided))
			{
				name = "buy_count",
				fnDescConvert = new FnDescConvert(this.fnCmdSliderDesc),
				use_wheel = true
			}, 1f);
			BxCmd.Br();
			float num = BxCmd.use_w * 0.5f;
			this.FbPrice = BxCmd.addP(new DsnDataP("", false)
			{
				TxCol = C32.d2c(4283780170U),
				text = "_",
				html = true,
				size = 16f,
				swidth = num - 20f,
				sheight = 18f,
				aligny = ALIGNY.MIDDLE,
				alignx = ALIGN.CENTER
			}, false);
			aBtn aBtn = BxCmd.addButton(new DsnDataButton
			{
				name = "Cancel",
				title = "Cancel",
				skin_title = "&&Submit",
				h = 30f,
				w = num - 10f,
				fnClick = new FnBtnBindings(this.ItemMng.fnClickItemCmd),
				skin = "row_center"
			});
			aBtn.setNaviT(aBtnMeterNel, true, true);
			aBtn.setNaviB(aBtnMeterNel, true, true);
			this.fine_item_count = true;
			this.FbUnderKD.text_content = "";
			this.fnGradeFocusChange(itemData, itemInfo, this.ItemMng.get_grade_cursor());
			aBtnMeterNel.Select(false);
			return true;
		}

		private void ErrorCmdPrompt(NelItem Itm)
		{
			try
			{
				if (this.ItemMng.isUsingState())
				{
					IVariableObject variableObject = this.BxCmd.Get("store_prompt", false);
					if (variableObject != null)
					{
						variableObject.setValue(NEL.error_tag + this.getErrorCmdAddindDesc(Itm) + NEL.error_tag_close);
					}
				}
				else
				{
					this.ItemMng.errorMessageToDesc(this.getErrorCmdAddindDesc(Itm));
				}
			}
			catch
			{
			}
		}

		private string getErrorCmdAddindDesc(NelItem Itm)
		{
			if (Itm.is_water)
			{
				return TX.GetA("cannot_take_need_container_item", NelItem.Bottle.getLocalizedName(0, null));
			}
			return TX.Get("cannot_take_need_enough_room", "");
		}

		private void fineUnderKD()
		{
			if (this.FbUnderKD == null)
			{
				return;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				aBtnItemRow selectedRow = this.ItemMng.Inventory.SelectedRow;
				if (selectedRow != null)
				{
					bool flag = true;
					NelItem itemData = selectedRow.getItemData();
					if (this.stt == UiItemStore.STATE.SELL)
					{
						stb.AddTxA("Store_desc_selling", false);
						if (itemData.stock > 1)
						{
							stb.AppendTxA("Store_desc_add_shifted", " ");
						}
						flag = this.StSell.getCount(itemData, -1) > 0;
					}
					else if (this.stt == UiItemStore.STATE.BUY)
					{
						stb.AddTxA("Store_desc_buying", false);
						if (itemData.stock > 1)
						{
							stb.AddTxA("Store_desc_add_shifted", false);
						}
						flag = this.StBuy.getCount(itemData, -1) > 0;
					}
					if (flag)
					{
						if (stb.Length != 0)
						{
							stb.Add(" / ");
						}
						if (this.buying_mode)
						{
							stb.AddTxA("Store_desc_buying_turnback", false);
						}
						else
						{
							stb.AddTxA("Store_desc_selling_turnback", false);
						}
						if (itemData.stock > 1)
						{
							stb.AppendTxA("Store_desc_remove_shifted", " ");
						}
					}
				}
				if (this.M2D.IMNG.has_recipe_collection)
				{
					stb.AppendTxA("KD_show_catalog", "\n");
				}
				this.FbUnderKD.Txt(stb);
			}
		}

		private bool fnGetMeterVariable(aBtnMeterNel Btn, NelItem Itm, ItemStorage.ObtainInfo Obt, int grade, out int meter_v, out int meter_mn, out int meter_max)
		{
			int count = this.StBuy.getCount(Itm, grade);
			int count2 = this.StSell.getCount(Itm, grade);
			int num = ((Obt != null) ? Obt.getCount(grade) : 0);
			if (this.buying_mode)
			{
				meter_v = count - count2;
				meter_mn = global::XX.X.Mn(meter_v, 0);
				meter_max = meter_v + this.StProduct.getCount(Itm, grade);
				if (!this.getInventoryInfinity(Itm))
				{
					meter_max = global::XX.X.Mn(meter_max, meter_v + this.getInventoryAddable(Itm));
				}
			}
			else
			{
				if (this.stt == UiItemStore.STATE.SELL)
				{
					this.createFirstData(Itm);
					int count3 = this.OFirstData[Itm][this.st_index].getCount(grade);
					num = this.getInventoryItemCount(Itm, grade);
					int num2 = (meter_v = count3 - num);
					meter_max = num2 + global::XX.X.Mn(this.AStInventory[this.st_index].St.getReduceable(Itm, grade), num);
					meter_mn = global::XX.X.Mn(meter_v, 0);
				}
				else
				{
					meter_v = num;
					meter_max = meter_v + this.getInventoryItemCount(Itm, grade);
					meter_mn = 0;
				}
				if (this.cannotSell(Itm, Obt))
				{
					meter_max = global::XX.X.Mn(meter_v + count, meter_max);
				}
			}
			return true;
		}

		protected bool cannotSell(NelItem Itm, ItemStorage.ObtainInfo Obt)
		{
			string text;
			return this.cannotSell(Itm, Obt, out text);
		}

		protected bool cannotSell(NelItem Itm, ItemStorage.ObtainInfo Obt, out string tx_error_key)
		{
			if (Itm.is_precious)
			{
				tx_error_key = "Store_selling_precious_things";
				return true;
			}
			if (Itm.is_food)
			{
				tx_error_key = "Store_selling_food";
				return true;
			}
			if (this.dont_sell_zero_price && this.MngProduct.sellPrice(Itm, Obt.top_grade) <= 0)
			{
				tx_error_key = "Store_selling_worthless";
				return true;
			}
			tx_error_key = null;
			return false;
		}

		private void fnGradeFocusChange(NelItem Itm, ItemStorage.ObtainInfo Obt, int grade)
		{
			if (grade < 0)
			{
				this.closeCountEdit(Itm, Obt);
				this.fineUnderKD();
			}
		}

		private void closeCountEdit(NelItem Itm, ItemStorage.ObtainInfo Obt)
		{
			this.FbPrice = null;
			this.CartBtn.resetPopPitch();
			if (this.stt != UiItemStore.STATE.CHECK_CART)
			{
				using (BList<aBtnItemRow> blist = this.ItemMng.Inventory.PopGetItemRowBtnsFor(Itm))
				{
					int count = blist.Count;
					bool flag = false;
					if (this.stt == UiItemStore.STATE.SELL)
					{
						flag = this.StBuy.getCount(Itm, -1) > 0 || this.StSell.getCount(Itm, -1) > 0;
					}
					for (int i = 0; i < count; i++)
					{
						aBtnItemRow aBtnItemRow = blist[i];
						(aBtnItemRow.get_Skin() as ButtonSkinItemRow).hilighted = flag || (this.stt == UiItemStore.STATE.BUY && this.StBuy.getCount(Itm, (int)aBtnItemRow.getItemRow().splitted_grade) > 0);
					}
					this.StBuy.fineRows(true);
					this.StSell.fineRows(true);
					for (int j = 0; j < count; j++)
					{
						this.recheckExistCount(blist[j]);
					}
				}
			}
		}

		private void itemRowInit(aBtnItemRow B, ItemStorage.IRow Row)
		{
			if (B == null)
			{
				return;
			}
			if (Row == null)
			{
				Row = B.getItemRow();
			}
			if (!this.ItemMng.isUsingState() || B.getItemData() != this.ItemMng.UsingTarget)
			{
				this.recheckExistCount(B);
				return;
			}
			this.need_recheck_list_count_whole = true;
		}

		private void recheckExistCount(aBtnItemRow B)
		{
			NelItem itemData = B.getItemData();
			ItemStorage.ObtainInfo itemInfo = B.getItemInfo();
			if (this.stt == UiItemStore.STATE.BUY)
			{
				ButtonSkinItemRow buttonSkinItemRow = B.get_Skin() as ButtonSkinItemRow;
				int num = this.AStInventory.Length;
				buttonSkinItemRow.use_exist_icon = this.icon_max;
				uint num2 = 0U;
				ItemStorage.ObtainInfo[] array = global::XX.X.Get<NelItem, ItemStorage.ObtainInfo[]>(this.OFirstData, itemData);
				int num3 = -1;
				for (int i = 0; i < num; i++)
				{
					UiItemStore.StTab stTab = this.AStInventory[i];
					if ((num2 & (1U << stTab.icon_id)) == 0U)
					{
						if (array == null)
						{
							num2 |= (stTab.St.ifExists(itemData, num3) ? (1U << stTab.icon_id) : 0U);
						}
						else
						{
							num2 |= ((array[i].getCount(num3) > 0) ? (1U << stTab.icon_id) : 0U);
						}
						buttonSkinItemRow.setRightExistIcon(stTab.icon_id, ((num2 & (1U << stTab.icon_id)) != 0U) ? stTab.icon : null);
					}
				}
			}
			else if (!this.buying_mode && this.cannotSell(itemData, itemInfo) && this.StBuy.getCount(itemData, -1) == 0)
			{
				B.locked_click = true;
				B.SetLocked(true, true, false);
			}
			if (this.stt != UiItemStore.STATE.CHECK_CART)
			{
				ItemStorage.IRow itemRow = B.getItemRow();
				bool flag;
				if (this.stt == UiItemStore.STATE.SELL)
				{
					flag = this.StBuy.getCount(itemData, -1) > 0 || this.StSell.getCount(itemData, -1) > 0;
				}
				else
				{
					flag = this.StBuy.getCount(itemData, (int)itemRow.splitted_grade) > 0;
				}
				(B.get_Skin() as ButtonSkinItemRow).hilighted = flag;
			}
		}

		private int fnMeterSlided(aBtnMeterNel Btn, int count, int pre_val, int cur_val)
		{
			return this.fnMeterSlided(Btn, count, pre_val, cur_val, this.ItemMng.UsingTarget, this.ItemMng.get_grade_cursor());
		}

		private int fnMeterSlided(aBtnMeterNel Btn, int count, int pre_val, int cur_val, NelItem Itm, int grade)
		{
			if (Itm == null)
			{
				return pre_val;
			}
			int num = 0;
			ItemStorage.ObtainInfo[] array;
			if (count > 0)
			{
				if (this.buying_mode)
				{
					int num2 = this.AStInventory.Length;
					int num3 = 0;
					while (num3 < num2 && count > 0)
					{
						ItemStorage st = this.AStInventory[num3].St;
						if (st.isAddable(Itm))
						{
							int num4 = st.Add(Itm, count, grade, true, false);
							if (num4 > 0)
							{
								this.fine_item_count = true;
								count -= num4;
								num++;
								st.Add(Itm, num4, grade, true, true);
								this.StProduct.Reduce(Itm, num4, grade, true);
								int num5 = global::XX.X.Mn(this.StSell.getCount(Itm, grade), num4);
								if (num5 > 0)
								{
									this.StSell.Reduce(Itm, num5, grade, true);
									num4 -= num5;
								}
								this.StBuy.Add(Itm, num4, grade, true, true);
								continue;
							}
						}
						num3++;
					}
					if (count > 0)
					{
						cur_val -= count;
						if (num == 0)
						{
							this.ErrorCmdPrompt(Itm);
						}
					}
				}
				else
				{
					int num6 = this.AStInventory.Length;
					int num7 = 0;
					while (num7 < num6 && count > 0)
					{
						ItemStorage st2 = this.AStInventory[num7].St;
						if (this.stt == UiItemStore.STATE.SELL && this.ItemMng.Inventory != st2)
						{
							num7++;
						}
						else
						{
							int num8 = global::XX.X.Mn(st2.getReduceable(Itm, grade), count);
							if (num8 > 0)
							{
								this.fine_item_count = true;
								count -= num8;
								num--;
								st2.Reduce(Itm, num8, grade, true);
								this.StProduct.Add(Itm, num8, grade, true, true);
								int num9 = global::XX.X.Mn(this.StBuy.getCount(Itm, grade), num8);
								if (num9 > 0)
								{
									this.StBuy.Reduce(Itm, num9, grade, true);
									num8 -= num9;
								}
								this.StSell.Add(Itm, num8, grade, true, true);
							}
							num7++;
						}
					}
					if (count > 0)
					{
						cur_val -= count;
					}
				}
			}
			else if (this.OFirstData.TryGetValue(Itm, out array))
			{
				count = -count;
				if (this.buying_mode)
				{
					int num10 = this.AStInventory.Length - 1;
					while (num10 >= 0 && count > 0)
					{
						ItemStorage st3 = this.AStInventory[num10].St;
						int count2 = array[num10].getCount(grade);
						int count3 = st3.getCount(Itm, grade);
						int num11 = global::XX.X.Mn(st3.getReduceable(Itm, grade), global::XX.X.Mn(global::XX.X.Mn(global::XX.X.Mx(0, count3 - count2), count), this.StBuy.getCount(Itm, grade)));
						if (num11 > 0)
						{
							this.fine_item_count = true;
							num--;
							count -= num11;
							this.AStInventory[num10].St.Reduce(Itm, num11, grade, true);
							this.StBuy.Reduce(Itm, num11, grade, true);
							this.StProduct.Add(Itm, num11, grade, true, true);
						}
						else
						{
							num10--;
						}
					}
					if (count > 0)
					{
						cur_val += count;
					}
				}
				else
				{
					int num12 = this.AStInventory.Length;
					for (int i = ((this.stt == UiItemStore.STATE.SELL) ? 1 : 2); i > 0; i--)
					{
						int num13 = num12 - 1;
						while (num13 >= 0 && count > 0)
						{
							ItemStorage st4 = this.AStInventory[num13].St;
							if (this.stt == UiItemStore.STATE.SELL && this.ItemMng.Inventory != st4)
							{
								num13--;
							}
							else
							{
								int num14 = global::XX.X.Mn(this.StProduct.getCount(Itm, grade), count);
								if (this.stt == UiItemStore.STATE.CHECK_CART)
								{
									int count4 = array[num13].getCount(grade);
									num14 = global::XX.X.Mn(num14, global::XX.X.Mx(0, count4 - st4.getReduceable(Itm, grade)));
								}
								if (num14 > 0)
								{
									int num15 = st4.Add(Itm, num14, grade, true, true);
									if (num15 > 0)
									{
										this.fine_item_count = true;
										num++;
										count -= num15;
										this.StProduct.Reduce(Itm, num15, grade, true);
										int num16 = global::XX.X.Mn(this.StSell.getCount(Itm, grade), num15);
										if (num16 > 0)
										{
											this.StSell.Reduce(Itm, num16, grade, true);
											num15 -= num16;
										}
										this.StBuy.Add(Itm, num15, grade, true, true);
									}
								}
								num13--;
							}
						}
					}
					if (count > 0)
					{
						cur_val += count;
						if (num == 0)
						{
							this.ErrorCmdPrompt(Itm);
						}
					}
				}
			}
			this.CartBtn.initPopping(num);
			return cur_val;
		}

		private int dropTargetGrade(aBtnItemRow Row)
		{
			if (!this.ItemMng.Inventory.grade_split)
			{
				return Row.getItemInfo().min_grade;
			}
			return (int)Row.getItemRow().splitted_grade;
		}

		private void dropCartByAddKey(int dir, bool check_zero = false)
		{
			if (this.FbPrice != null || this.FbUnderKD == null || this.ItemMng.Inventory.SelectedRow == null)
			{
				return;
			}
			aBtnItemRow selectedRow = this.ItemMng.Inventory.SelectedRow;
			if (selectedRow == null)
			{
				return;
			}
			if (selectedRow.is_fake_row)
			{
				SND.Ui.play("locked", false);
				return;
			}
			this.createFirstData(selectedRow.getItemData());
			ItemStorage.ObtainInfo itemInfo = selectedRow.getItemInfo();
			if (check_zero && (this.ItemMng.Inventory.grade_split ? itemInfo.getCount((int)selectedRow.getItemRow().splitted_grade) : itemInfo.total) == 0)
			{
				SND.Ui.play("locked", false);
				return;
			}
			if (IN.isUiShiftO())
			{
				int num = selectedRow.getItemData().stock;
				if (this.stt == UiItemStore.STATE.SELL && !this.ItemMng.Inventory.infinit_stockable)
				{
					num = global::XX.X.Mn(selectedRow.getItemRow().total, num);
				}
				dir *= num;
			}
			this.ItemMng.touchGrade(selectedRow.getItemData(), itemInfo);
			int num2 = this.dropTargetGrade(selectedRow);
			int num3;
			int num4;
			int num5;
			if (!this.fnGetMeterVariable(null, selectedRow.getItemData(), itemInfo, num2, out num3, out num4, out num5))
			{
				return;
			}
			if (selectedRow.getItemData().is_food && this.stt == UiItemStore.STATE.SELL)
			{
				SND.Ui.play("locked", false);
				this.ItemMng.errorMessageToDesc(TX.Get("Store_selling_food", ""));
				return;
			}
			if (!((dir < 0) ? (num3 <= num4) : (num3 >= num5)))
			{
				int num6 = global::XX.X.MMX(num4, num3 + dir, num5);
				this.fnMeterSlided(null, num6 - num3, num3, num6, selectedRow.getItemData(), num2);
				this.ItemMng.fineCount(selectedRow.getItemData(), itemInfo, this.ItemMng.Inventory.auto_splice_zero_row);
				this.ItemMng.fineItemStarsCount(false);
				this.itemRowInit(this.ItemMng.Inventory.SelectedRow, null);
				this.fineUnderKD();
				return;
			}
			SND.Ui.play("locked", false);
			string text;
			if (this.cannotSell(selectedRow.getItemData(), selectedRow.getItemInfo(), out text) && this.stt == UiItemStore.STATE.SELL)
			{
				this.ItemMng.errorMessageToDesc(TX.Get(text, ""));
				return;
			}
			this.ItemMng.errorMessageToDesc(this.getErrorCmdAddindDesc(selectedRow.getItemData()));
		}

		private string fnCmdSliderDesc(string def)
		{
			NelItem usingTarget = this.ItemMng.UsingTarget;
			if (usingTarget == null)
			{
				return def;
			}
			int grade_cursor = this.ItemMng.get_grade_cursor();
			if (this.buying_mode)
			{
				return TX.GetA("product_count", this.StProduct.getCount(usingTarget, grade_cursor).ToString()) + "<img mesh=\"arrow_nel_5\" width=\"32\" height=\"18\" />" + this.getInventoryCountString(usingTarget, grade_cursor);
			}
			return this.getInventoryCountString(usingTarget, grade_cursor) + "<img mesh=\"arrow_nel_5\" width=\"32\" height=\"18\" />" + TX.GetA("selling_count", global::XX.X.Mx(0, this.StSell.getCount(usingTarget, grade_cursor)).ToString());
		}

		public bool buying_mode
		{
			get
			{
				return this.stt == UiItemStore.STATE.BUY || (this.stt == UiItemStore.STATE.CHECK_CART && this.st_index == 0);
			}
		}

		public bool itemwindow_mode
		{
			get
			{
				return this.stt == UiItemStore.STATE.BUY || this.stt == UiItemStore.STATE.SELL || this.stt == UiItemStore.STATE.CHECK_CART;
			}
		}

		public int getInventoryItemCount(NelItem Itm, int grade)
		{
			int num = this.AStInventory.Length;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				num2 += this.AStInventory[i].St.getCount(Itm, grade);
			}
			return num2;
		}

		public bool getInventoryItemExists(NelItem Itm, int grade)
		{
			int num = this.AStInventory.Length;
			for (int i = 0; i < num; i++)
			{
				if (this.AStInventory[i].St.ifExists(Itm, grade))
				{
					return true;
				}
			}
			return false;
		}

		public bool getInventoryInfinity(NelItem Itm)
		{
			int num = this.AStInventory.Length;
			for (int i = 0; i < num; i++)
			{
				if (this.AStInventory[i].St.infinit_stockable)
				{
					return true;
				}
			}
			return false;
		}

		public int getInventoryAddable(NelItem Itm)
		{
			int num = this.AStInventory.Length;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				ItemStorage st = this.AStInventory[i].St;
				if (st.infinit_stockable)
				{
					num2 += 9999;
				}
				else
				{
					num2 += st.getItemCapacity(Itm, false, false);
				}
			}
			return num2;
		}

		private bool fnClickCmdTop(aBtn B)
		{
			if (this.stt != UiItemStore.STATE.TOP)
			{
				return true;
			}
			string title = B.title;
			if (title != null)
			{
				if (!(title == "&&cmd_buy"))
				{
					if (!(title == "&&cmd_sell"))
					{
						if (!(title == "&&cmd_cart"))
						{
							if (title == "&&cmd_checkout")
							{
								if (this.isCartEmpty())
								{
									SND.Ui.play("cancel", false);
									this.changeState(UiItemStore.STATE._NOUSE);
								}
								else
								{
									SND.Ui.play("tool_hand_init", false);
									this.changeState(UiItemStore.STATE.CHECKOUT);
								}
							}
						}
						else if (!this.isCartEmpty())
						{
							this.changeState(UiItemStore.STATE.CHECK_CART);
						}
						else
						{
							SND.Ui.play("locked", false);
						}
					}
					else
					{
						this.changeState(UiItemStore.STATE.SELL);
					}
				}
				else
				{
					this.changeState(UiItemStore.STATE.BUY);
				}
			}
			global::XX.X.dl(B.title, null, false, false);
			return true;
		}

		public bool fnClickCartBtn(aBtn B)
		{
			if (this.stt == UiItemStore.STATE.TOP || this.stt == UiItemStore.STATE.SELL || this.stt == UiItemStore.STATE.BUY || this.stt == UiItemStore.STATE.CHECKOUT)
			{
				if (this.isCartEmpty())
				{
					SND.Ui.play("locked", false);
					return false;
				}
				if (this.stt != UiItemStore.STATE.TOP)
				{
					this.changeState(UiItemStore.STATE.TOP);
				}
				this.changeState(UiItemStore.STATE.CHECK_CART);
			}
			return true;
		}

		private bool FnDrawServiceFib(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			float widthPixel = FI.widthPixel;
			float heightPixel = FI.heightPixel;
			float num = widthPixel * 0.5f;
			float num2 = heightPixel * 0.5f;
			Md.Col = Md.ColGrd.Set(FI.Col).mulA(alpha).C;
			Md.Rect(0f, 0f, widthPixel + 2f, heightPixel + 2f, false);
			Md.Col = Md.ColGrd.Set(FI.TxCol).mulA(alpha).C;
			Md.Box(0f, 0f, widthPixel + 2f, heightPixel + 2f, 3f, false);
			Md.Line(-num, -num2 + 18f, -num + 8f, -num2, 1f, false, 0f, 0f).Line(-num, num2 - 18f, -num + 8f, num2, 1f, false, 0f, 0f).Line(num, num2 - 18f, num - 8f, num2, 1f, false, 0f, 0f)
				.Line(num, -num2 + 18f, num - 8f, -num2, 1f, false, 0f, 0f);
			Md.Line(-num, -num2 + 8f, -num + 18f, -num2, 1f, false, 0f, 0f).Line(-num, num2 - 8f, -num + 18f, num2, 1f, false, 0f, 0f).Line(num, num2 - 8f, num - 18f, num2, 1f, false, 0f, 0f)
				.Line(num, -num2 + 8f, num - 18f, -num2, 1f, false, 0f, 0f);
			update_meshdrawer = true;
			return true;
		}

		private void fineServiceFIB()
		{
			if (this.FbServiceBuy == null && this.FbServiceSell == null)
			{
				return;
			}
			if (this.stt == UiItemStore.STATE.BUY)
			{
				if (this.FbServiceBuy != null)
				{
					this.FbServiceBuy.gameObject.SetActive(true);
				}
				if (this.FbServiceSell != null)
				{
					this.FbServiceSell.gameObject.SetActive(false);
				}
			}
			else
			{
				if (this.stt != UiItemStore.STATE.SELL)
				{
					if (this.service_t >= 30f)
					{
						this.service_t = 30f;
					}
					return;
				}
				if (this.FbServiceSell != null)
				{
					this.FbServiceSell.gameObject.SetActive(true);
				}
				if (this.FbServiceBuy != null)
				{
					this.FbServiceBuy.gameObject.SetActive(false);
				}
			}
			if (this.service_t < 0f)
			{
				this.service_t = 0f;
			}
			this.runServiceFIB(0f, true);
		}

		private void runServiceFIB(float fcnt, bool is_active)
		{
			if (is_active)
			{
				if (this.service_t >= 30f)
				{
					return;
				}
				this.service_t += fcnt;
			}
			else
			{
				if (this.service_t < 0f)
				{
					return;
				}
				this.service_t -= fcnt;
				if (this.service_t < 0f)
				{
					this.service_t = -1f;
					if (this.FbServiceSell != null)
					{
						this.FbServiceSell.gameObject.SetActive(false);
					}
					if (this.FbServiceBuy != null)
					{
						this.FbServiceBuy.gameObject.SetActive(false);
					}
					return;
				}
			}
			float num = global::XX.X.ZSIN(this.service_t, 30f);
			float num2 = global::XX.X.NI(44f, -5.5f, num);
			float num3 = this.out_w * 0.5f - 40f;
			float num4 = this.out_h * 0.5f + global::XX.X.NI(180, global::XX.X.ENG_MODE ? (-10) : (-25), num);
			if (this.FbServiceBuy != null)
			{
				IN.PosP2(this.FbServiceBuy.transform, num3, num4);
				this.FbServiceBuy.transform.localEulerAngles = new Vector3(0f, 0f, num2);
			}
			if (this.FbServiceSell != null)
			{
				IN.PosP2(this.FbServiceSell.transform, num3, num4);
				this.FbServiceSell.transform.localEulerAngles = new Vector3(0f, 0f, num2);
			}
		}

		private bool fnClickCheckout(aBtn B)
		{
			if (this.stt != UiItemStore.STATE.CHECKOUT)
			{
				return true;
			}
			string title = B.title;
			if (title != null)
			{
				if (!(title == "&&cmd_checkout_submit"))
				{
					if (!(title == "&&cmd_checkout_abort"))
					{
						if (title == "&&Cancel")
						{
							this.changeState(UiItemStore.STATE.TOP);
						}
					}
					else
					{
						this.changeState(UiItemStore.STATE._NOUSE);
					}
				}
				else
				{
					UiItemStore.StoreResult storeResult = this.calcStoreResult(null, null);
					if (this.coin_count + storeResult.money_addition < 0)
					{
						SND.Ui.play("locked", false);
					}
					else
					{
						this.confirmCheckout(storeResult);
						this.changeState(UiItemStore.STATE._NOUSE);
					}
				}
			}
			return true;
		}

		private void confirmCheckout(UiItemStore.StoreResult Res)
		{
			SND.Ui.play("store_checkout", false);
			CsvVariableContainer variableContainer = EV.getVariableContainer();
			variableContainer.define("_buy_money", Res.buy_money.ToString(), true);
			variableContainer.define("_sell_money", Res.sell_money.ToString(), true);
			if (Res.money_addition < 0)
			{
				CoinStorage.reduceCount(-Res.money_addition, this.coin_type);
			}
			else
			{
				CoinStorage.addCount(Res.money_addition, this.coin_type, true);
			}
			this.MngProduct.confirmCheckout(this.StBuy, this.StSell, Res);
			if (this.M2D != null && this.M2D.IMNG != null)
			{
				this.M2D.IMNG.confirmStoreCheckout(this.StBuy, this.StSell);
			}
			this.AStFirstData = null;
		}

		private void abortCheckout()
		{
			if (this.AStFirstData != null)
			{
				int num = this.AStInventory.Length;
				for (int i = 0; i < num; i++)
				{
					this.AStInventory[i].St.readBinaryFrom(this.AStFirstData[i], true, false, false, 9, false);
				}
				this.StProduct.copyFrom(this.StProductFirst, false);
				this.AStFirstData = null;
			}
		}

		protected int coin_count
		{
			get
			{
				return (int)CoinStorage.getCount(this.coin_type);
			}
		}

		public bool isItemChooseState()
		{
			return this.stt != UiItemStore.STATE._NOUSE && this.stt != UiItemStore.STATE.TOP && this.stt != UiItemStore.STATE.CHECKOUT;
		}

		public bool isCartEmpty()
		{
			return this.StSell.getVisibleRowCount() == 0 && this.StBuy.getVisibleRowCount() == 0;
		}

		private string[] getItemTabKeys()
		{
			int num = this.AStCur.Length;
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				UiItemStore.StTab stTab = this.AStCur[i];
				string text = TX.Get(stTab.key, "");
				if (TX.valid(stTab.icon))
				{
					text = "<img mesh=\"" + stTab.icon + "\" width=\"20\" height=\"24\" />" + text;
				}
				array[i] = text;
			}
			return array;
		}

		private bool count_edit_mode
		{
			get
			{
				return this.FbPrice != null;
			}
		}

		public ItemStorage[] getUsingInventoryArray()
		{
			int num = this.AStInventory.Length;
			ItemStorage[] array = new ItemStorage[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = this.AStInventory[i].St;
			}
			return array;
		}

		public override void OnDestroy()
		{
			this.ReleaseStorage();
			if (this.RecipeBook != null)
			{
				IN.DestroyOne(this.RecipeBook.gameObject);
				this.RecipeBook = null;
			}
			this.M2D.FlagValotStabilize.Rem("Store");
			base.OnDestroy();
		}

		protected UiItemStore.STATE stt = UiItemStore.STATE._NOUSE;

		private NelM2DBase M2D;

		private StoreManager MngProduct;

		private ItemStorage StProduct;

		private UiItemManageBoxSlider ItemMng;

		private ItemStorage StBuy;

		private ItemStorage StSell;

		private ItemStorage StProductFirst;

		protected aBtnNelPopper CartBtn;

		protected CoinStorage.CTYPE coin_type;

		private ByteArray[] AStFirstData;

		private FillBlock FbMoney;

		private readonly float out_w = IN.w * 0.72f;

		private readonly float out_h = IN.h * 0.85f;

		private readonly float right_h = IN.h * 0.85f - 100f;

		private readonly float right_h_margin = 16f;

		private const float money_h = 46f;

		private const float desc_w = 360f;

		public string str_money_icon = "<img mesh=\"money_icon\" />";

		public string str_money_icon_zero = "<img mesh=\"money_icon\" color=\"66:#ffffff\" />";

		protected string tx_key_not_have_enough_money = "Store_no_enough_money";

		public const string str_arrow5_icon = "<img mesh=\"arrow_nel_5\" width=\"32\" height=\"18\" />";

		private const string tag_inclease = "<font color=\"ff:#1335DF\">";

		private const string tag_declease = "<font color=\"ff:#DE148D\">";

		private const float btn_w = 380f;

		private const float btn_h = 30f;

		private const float cmd_w_count_select = 390f;

		private const float margin_tb = 30f;

		private int icon_max = 1;

		private const string balance_buy_define_var_name = "_buy_money";

		private const string balance_sell_define_var_name = "_sell_money";

		public bool dont_sell_zero_price;

		protected UiBoxDesigner BxC;

		private UiBoxDesigner BxR;

		private UiBoxDesigner BxDesc;

		protected UiBoxDesigner BxMoney;

		protected UiBoxDesigner BxCmd;

		public bool destructed;

		private bool need_recheck_list_count_whole;

		private ColumnRow RTabBar;

		private BDic<NelItem, ItemStorage.ObtainInfo[]> OFirstData;

		private UiItemStore.StTab[] AStInventory;

		private UiItemStore.StTab[] AStCur;

		private Designer ItemTab;

		private int st_index;

		private FillBlock FbPrice;

		private FillBlock FbUnderKD;

		private FillImageBlock FbServiceBuy;

		private FillImageBlock FbServiceSell;

		private float service_t = -1f;

		private const float SERVICE_MAXT = 30f;

		private bool fine_item_count;

		private BDic<ItemStorage, BDic<NelItem, uint>> OOTouchedGrade = new BDic<ItemStorage, BDic<NelItem, uint>>(3);

		private UiItemManageBox.FnSortOverride FD_fnSortInStore;

		private bool opening_recipe_book;

		private UiFieldGuide RecipeBook;

		private aBtn Rbk_PreSelected;

		private List<aBtn> APoolEvacuated;

		protected enum STATE
		{
			_NOUSE = -1,
			TOP,
			CHECKOUT,
			BUY,
			SELL,
			CHECK_CART
		}

		private class StTab
		{
			public StTab(string _key, ItemStorage _St, string _icon = "", int _icon_id = 0)
			{
				this.key = _key;
				this.St = _St;
				this.icon = _icon;
				this.icon_id = _icon_id;
			}

			public ItemStorage St;

			public string key;

			public string icon;

			public int icon_id;
		}

		public struct StoreResult
		{
			public int money_addition
			{
				get
				{
					return this.sell_money - this.buy_money;
				}
			}

			public int buy_count;

			public int buy_money;

			public int sell_count;

			public int sell_money;
		}
	}
}

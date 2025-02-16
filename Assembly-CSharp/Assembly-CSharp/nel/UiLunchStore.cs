using System;
using evt;
using UnityEngine;
using XX;

namespace nel
{
	public class UiLunchStore : UiLunchTimeBase
	{
		protected override ItemStorage AwakeLunch()
		{
			this.alloc_not_food_on_row = true;
			this.Img = EV.Pics.getPic("lunch_cutin/noel_lunchst", true, true);
			this.ImgLoader = EV.Pics.cacheReadFor(this.Img);
			this.noel_scale = 1f;
			this.total_buy_money = 0;
			this.total_buy_count = 0;
			this.load_my_inventory = false;
			this.grade_on_desc_tab = true;
			this.t_noel = -105f;
			this.INIT_DELAY = 20;
			if (UILog.Instance != null)
			{
				UILog.Instance.FlgStopMoneyChangedAnnounce.Add("LUNCH");
			}
			ItemStorage itemStorage = base.AwakeLunch();
			itemStorage.sort_button_bits |= 8;
			return itemStorage;
		}

		public void addExternal(StoreManager _Store)
		{
			this.Store = _Store;
			string text;
			string text2;
			this.StProduct = this.Store.CreateItemStorage(out text, out text2);
			ItemStorage.SORT_TYPE sort_TYPE;
			bool flag;
			this.StProduct.getSortType(out sort_TYPE, out flag);
			base.setSort(sort_TYPE, flag);
			base.addExternal(this.StProduct, true);
		}

		public override void OnDestroy()
		{
			if (UILog.Instance != null)
			{
				UILog.Instance.FlgStopMoneyChangedAnnounce.Rem("LUNCH");
			}
			base.OnDestroy();
		}

		protected override void initBoxes()
		{
			base.initBoxes();
			this.DsMoney = this.CreateT<UiBoxMoney>("Money", 0f, 0f, 30f, 30f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.DsMoney.set_aim = AIM.BL;
			this.DsMoney.activate();
		}

		protected override void initItemTab(ItemStorage St, UiItemManageBox ItemMng, bool clear = false)
		{
			ItemMng.fnSortInjectMng = new UiItemManageBox.FnSortOverride(this.fnSortInjectMng);
			base.initItemTab(St, ItemMng, clear);
		}

		public override UiBoxDesignerFamily deactivate(bool immediate)
		{
			base.deactivate(immediate);
			if (UILog.Instance != null)
			{
				UILog.Instance.FlgStopMoneyChangedAnnounce.Rem("LUNCH");
			}
			if (this.Store != null)
			{
				this.Store.releasePremireCache();
			}
			return this;
		}

		private bool fnSortInjectMng(ItemStorage.IRow Ra, ItemStorage.IRow Rb, ItemStorage.SORT_TYPE sort_type, out int ret)
		{
			if (sort_type != ItemStorage.SORT_TYPE.COST)
			{
				return this.Store.fnSortInStore(Ra, Rb, sort_type, out ret, true);
			}
			RecipeManager.RecipeDish recipeDish = ((Ra.Data.RecipeInfo != null) ? Ra.Data.RecipeInfo.DishInfo : null);
			RecipeManager.RecipeDish recipeDish2 = ((Rb.Data.RecipeInfo != null) ? Rb.Data.RecipeInfo.DishInfo : null);
			if (recipeDish == null && recipeDish2 == null)
			{
				return this.Store.fnSortInStore(Ra, Rb, ItemStorage.SORT_TYPE.PRICE | (sort_type & ItemStorage.SORT_TYPE._DESCEND), out ret, true);
			}
			if (recipeDish == null)
			{
				ret = -1;
				return true;
			}
			if (recipeDish2 == null)
			{
				ret = 1;
				return true;
			}
			ret = 0;
			return false;
		}

		protected override void copyCmdPromptTo(STB Stb, bool is_double, bool is_water, float cost_applied_level, bool is_duped)
		{
			if (is_double)
			{
				Stb.AddTxA("lunch_confirm_double", false).Ret("\n");
				Stb.AddTxA("lunchstore_confirm", false);
				return;
			}
			if (!is_water)
			{
				if (cost_applied_level >= 0f && cost_applied_level < 1f)
				{
					Stb.AddTxA((cost_applied_level == 0f) ? "lunch_confirm_full" : "lunchstore_confirm_burst", false);
					return;
				}
				if (is_duped)
				{
					Stb.AddTxA("lunchstore_confirm_dupe", false);
					return;
				}
				Stb.AddTxA("lunchstore_confirm", false);
				return;
			}
			else
			{
				if (cost_applied_level >= 0f && cost_applied_level < 1f)
				{
					Stb.AddTxA((cost_applied_level == 0f) ? "lunch_drink_confirm_full" : "lunchstore_drink_confirm_burst", false);
					return;
				}
				if (is_duped)
				{
					Stb.AddTxA("lunchstore_drink_confirm_dupe", false);
					return;
				}
				Stb.AddTxA("lunchstore_drink_confirm", false);
				return;
			}
		}

		public override string fnLunchDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			if (row == UiItemManageBox.DESC_ROW.ROW_COUNT)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					CoinStorage.getIconHtml(stb, this.ctype, 30).Add(this.Store.buyPrice(Itm, grade));
					return stb.ToString();
				}
			}
			return base.fnLunchDescAddition(Itm, row, def_string, grade, Obt, count);
		}

		protected override STB getIngredientDescPre(STB Stb, ItemStorage Inventory, ItemStorage.IRow AppliedIR)
		{
			NelItem.getGradeMeshTxTo(Stb, (int)AppliedIR.splitted_grade, 1, 38);
			Stb.Add(" ");
			AppliedIR.Data.getCountString(Stb, Inventory.getCount(AppliedIR.Data, -1), Inventory);
			if (AppliedIR.Data.is_food)
			{
				Stb.Ret("\n");
				base.getIngredientDescPre(Stb, Inventory, AppliedIR);
			}
			return Stb;
		}

		protected override void setConfirmBtnTitle(aBtn B, ItemStorage.IRow AppliedIR)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA(AppliedIR.Data.is_water ? "Eat_Buy_Drink" : "Eat_Buy", false);
				using (STB stb2 = TX.PopBld(null, 0))
				{
					CoinStorage.getIconHtml(stb2, this.ctype, 30);
					stb2.Add(this.Store.buyPrice(AppliedIR.Data, (int)AppliedIR.splitted_grade));
					stb.TxRpl(stb2);
				}
				(B.get_Skin() as ButtonSkinRow).setTitleTextS(stb);
			}
		}

		public override int consumeEaten()
		{
			int num = base.consumeEaten();
			if (num > 0 && this.StBuy != null)
			{
				this.Store.confirmCheckout(this.StBuy, null, new UiItemStore.StoreResult
				{
					buy_money = this.total_buy_money,
					buy_count = this.total_buy_count
				});
				this.StBuy = null;
			}
			return num;
		}

		protected override bool ReduceFromInventorySrc(ItemStorage Str, NelItem Itm, ItemStorage.ObtainInfo StrObt)
		{
			if (Str == this.StProduct)
			{
				if (this.StBuy == null)
				{
					this.StBuy = new ItemStorage("item_tab_cart_buy", 99).clearAllItems(99);
					this.StBuy.infinit_stockable = (this.StBuy.water_stockable = (this.StBuy.grade_split = true));
					this.StBuy.auto_splice_zero_row = false;
				}
				this.StBuy.Add(Itm, 1, StrObt.top_grade, true, true);
				return true;
			}
			return base.ReduceFromInventorySrc(Str, Itm, StrObt);
		}

		protected override bool executeEat(aBtn B, ItemStorage.IRow IR, RecipeManager.RecipeDish AppliedDish)
		{
			if (IR == null)
			{
				return false;
			}
			int num = this.Store.buyPrice(IR.Data, (int)IR.splitted_grade);
			if ((long)num > (long)((ulong)CoinStorage.getCount(this.ctype)))
			{
				B.setSkinTitle(NEL.error_tag + TX.Get("Store_no_enough_money", "") + NEL.error_tag_close);
				return false;
			}
			this.total_buy_money += num;
			this.total_buy_count++;
			CoinStorage.reduceCount(num, this.ctype);
			return base.executeEat(B, IR, AppliedDish);
		}

		protected override bool runEffect(MeshDrawer MdEf, float af_effect)
		{
			return UiLunchTime.runEffectS(this, MdEf, af_effect, this.SMtrNoel, this.PtcUseFood);
		}

		protected override void runNoelImg(MeshDrawer MdNoel, ref bool need_redraw_noel)
		{
			if (this.t_noel >= -100f && this.t_noel < 0f && !UiLunchTime.runNoelImgS(MdNoel, ref this.t_noel, -100f, this.Img, ref this.ImgLoader, ref this.SMtrNoel))
			{
				return;
			}
			if (this.t_noel >= 0f)
			{
				need_redraw_noel = true;
			}
		}

		protected override bool drawNoelImg(MeshDrawer MdNoel, float alpha, ref bool need_redraw_noel)
		{
			MdNoel.clear(false, false);
			need_redraw_noel = false;
			MdNoel.chooseSubMesh(1, false, true);
			MdNoel.base_px_x = -IN.wh * 0.78f;
			MdNoel.base_px_y = 10f + 900f * (base.isActiveL() ? (1f - X.ZSIN(this.t_noel, 50f)) : 0f);
			MdNoel.Col = MdNoel.ColGrd.White().mulA(alpha).C;
			MdNoel.RotaPF((float)X.IntR(6f * X.COSIT(433f)), (float)X.IntR(6f * X.COSIT(511f)), this.noel_scale, this.noel_scale, 0f, this.Img.PF, false, false, false, uint.MaxValue, false, 0);
			return true;
		}

		protected override void executeEatAfter()
		{
			if (this.PtcUseFood == null)
			{
				this.PtcUseFood = new EfParticleOnce("ui_use_food", EFCON_TYPE.UI);
				return;
			}
			this.PtcUseFood.shuffle();
		}

		private EvImg Img;

		private EvPerson.EvPxlsLoader ImgLoader;

		private Material SMtrNoel;

		private EfParticleOnce PtcUseFood;

		private StoreManager Store;

		private ItemStorage StProduct;

		public CoinStorage.CTYPE ctype;

		private ItemStorage StBuy;

		private UiBoxMoney DsMoney;

		private int total_buy_money;

		private int total_buy_count;

		protected const int noel_start_maxt = 50;
	}
}

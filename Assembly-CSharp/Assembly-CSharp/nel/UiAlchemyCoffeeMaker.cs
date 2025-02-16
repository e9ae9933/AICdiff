using System;
using evt;
using PixelLiner;
using XX;

namespace nel
{
	public class UiAlchemyCoffeeMaker : UiAlchemy
	{
		protected override void Awake()
		{
			this.topic_use_topright_counter = false;
			this.cartbtn_icon = "pict_siphon";
			base.Awake();
			this.RecipeTea = NelItem.GetById("Recipe_herbtea", false);
			this.TicketItem = NelItem.GetById("coffeemaker_ticket", false);
			this.SiphonItem = NelItem.GetById("tool_siphon", false);
		}

		protected override ItemStorage getRecipeTopicDefault()
		{
			ItemStorage itemStorage = new ItemStorage("Inventory_COF", 2);
			itemStorage.sort_button_bits = 0;
			itemStorage.infinit_stockable = true;
			itemStorage.Add(this.RecipeTea, 1, 0, true, true);
			return itemStorage;
		}

		public override void InitManager(ItemStorage[] _AInventory, ItemStorage _StRecipeTopic, int _init_tptab_index = -1, PxlFrame _PFCompleteCutin = null)
		{
			ItemStorage inventoryPrecious = this.M2D.IMNG.getInventoryPrecious();
			this.siphon_ticket0 = (this.siphon_ticket = inventoryPrecious.getCount(this.TicketItem, -1));
			this.enable_tool_siphon = inventoryPrecious.getCount(this.SiphonItem, 0) > 0;
			EV.getVariableContainer().define("_siphon_used", "0", true);
			inventoryPrecious.Add(this.SiphonItem, this.siphon_ticket0, 4, true, true);
			base.InitManager(_AInventory, _StRecipeTopic, _init_tptab_index, _PFCompleteCutin);
		}

		public void recountSiphonTicket()
		{
			ItemStorage inventoryPrecious = this.M2D.IMNG.getInventoryPrecious();
			this.siphon_ticket = inventoryPrecious.getCount(this.SiphonItem, 4);
		}

		protected override string topic_title_text_content
		{
			get
			{
				string text;
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Add("<img mesh=\"itemrow_category.", 53, "\" width=\"42\" y=\"6\" tx_color /><font size=\"140%\">x").Add(this.siphon_ticket);
					if (this.enable_tool_siphon)
					{
						using (STB stb2 = TX.PopBld(null, 0))
						{
							stb.Insert(0, stb2.Add("<img mesh=\"itemrow_category.", this.TicketItem.getIcon(this.M2D.IMNG.getInventoryPrecious(), null), "\" width=\"12\" y=\"6\" tx_color />"));
						}
						stb.Add("\u3000+ ").Add("<img mesh=\"itemrow_category.", 55, "\" width=\"12\" y=\"6\" tx_color />");
						stb.Add("<img mesh=\"itemrow_category.", 53, "\" width=\"40\" y=\"6\" tx_color /><bmc c=\"xi\" scale=\"2\" y=\"10\" />");
					}
					else if (this.siphon_ticket == 0)
					{
						stb.Add(NEL.error_tag).AddTxA("Coffeemaker_no_token", false).Add(NEL.error_tag_close);
					}
					text = stb.ToString();
				}
				return text;
			}
		}

		protected override string fnRecipeTopicDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			if (row == UiItemManageBox.DESC_ROW.DESC && Itm == this.RecipeTea)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.AddTxA("Item_desc_coffeemaker_topic", false).TxRpl(this.siphon_ticket);
					stb.Add("\n").AddTxA("Item_desc_suffix_water_needs_empty_bottle", false);
					return stb.ToString();
				}
			}
			return base.fnRecipeTopicDescAddition(Itm, row, def_string, grade, Obt, count);
		}

		public override bool reduceOnCreating(NelItem Itm, int grade)
		{
			if (Itm == this.SiphonItem)
			{
				return grade < 0 || grade == 4;
			}
			return base.reduceOnCreating(Itm, grade);
		}

		protected override void changeState(UiCraftBase.STATE st)
		{
			if (st == UiCraftBase.STATE.RECIPE_TOPIC)
			{
				this.recountSiphonTicket();
			}
			base.changeState(st);
		}

		public override UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			this.recountSiphonTicket();
			ItemStorage inventoryPrecious = this.M2D.IMNG.getInventoryPrecious();
			if (this.siphon_ticket0 > this.siphon_ticket)
			{
				inventoryPrecious.Reduce(this.TicketItem, this.siphon_ticket0 - this.siphon_ticket, -1, true);
				EV.getVariableContainer().define("_siphon_used", (this.siphon_ticket0 - this.siphon_ticket).ToString(), true);
				this.siphon_ticket0 = this.siphon_ticket;
			}
			if (this.siphon_ticket > 0)
			{
				inventoryPrecious.Reduce(this.SiphonItem, this.siphon_ticket, 4, true);
			}
			return base.deactivate(immediate);
		}

		private NelItem RecipeTea;

		private NelItem TicketItem;

		private NelItem SiphonItem;

		private int siphon_ticket0;

		private int siphon_ticket;

		private bool enable_tool_siphon;

		public const int ticket_siphon_grade = 4;

		public const int tool_grade = 0;

		private const string evtdef_siphon_used = "_siphon_used";
	}
}

using System;

namespace nel
{
	public class UiItemStorePuppetCrafts : UiItemStoreWithHelp
	{
		protected override void Awake()
		{
			this.coin_type = CoinStorage.CTYPE.CRAFTS;
			this.help_title_tx_key = "Store_Puppet_Help_Title";
			this.help_detail_tx_key = "Store_Puppet_Help_Contents";
			this.str_money_icon = "<img mesh=\"" + CoinStorage.icon_key(this.coin_type) + "\" />";
			this.str_money_icon_zero = "<img mesh=\"" + CoinStorage.icon_key(this.coin_type) + "\" color=\"66:#ffffff\" />";
			this.tx_key_not_have_enough_money = "Store_no_enough_crafts";
			base.Awake();
		}
	}
}

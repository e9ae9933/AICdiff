using System;
using XX;

namespace nel
{
	public class aBtnItemRow : aBtnNel
	{
		public override ButtonSkin makeButtonSkin(string key)
		{
			this.click_snd = "tool_hand_init";
			if (key != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(key);
				if (num <= 2166136261U)
				{
					if (num <= 999080129U)
					{
						if (num != 291972474U)
						{
							if (num != 881576750U)
							{
								if (num != 999080129U)
								{
									goto IL_0366;
								}
								if (!(key == "enhancer"))
								{
									goto IL_0366;
								}
								return this.Skin = (this.RowSkin = new ButtonSkinEnhancerRow(this, this.w, this.h));
							}
							else
							{
								if (!(key == "lunchtime"))
								{
									goto IL_0366;
								}
								return this.Skin = (this.RowSkin = new ButtonSkinItemLunchTimeRow(this, this.w, this.h));
							}
						}
						else
						{
							if (!(key == "alchemy_ingredient"))
							{
								goto IL_0366;
							}
							return this.Skin = (this.RowSkin = new ButtonSkinAlchemyIngredientRow(this, this.w, this.h));
						}
					}
					else if (num != 1713201584U)
					{
						if (num != 1995765084U)
						{
							if (num != 2166136261U)
							{
								goto IL_0366;
							}
							if (key == null)
							{
								goto IL_0366;
							}
							if (key.Length != 0)
							{
								goto IL_0366;
							}
						}
						else
						{
							if (!(key == "alchemy_ingredient_trm"))
							{
								goto IL_0366;
							}
							return this.Skin = (this.RowSkin = new ButtonSkinAlchemyIngredientTrmRow(this, this.w, this.h));
						}
					}
					else
					{
						if (!(key == "alchemy_use"))
						{
							goto IL_0366;
						}
						return this.Skin = (this.RowSkin = new ButtonSkinAlchemyEntryRow(this, this.w, this.h));
					}
				}
				else if (num <= 3353852974U)
				{
					if (num != 2348494829U)
					{
						if (num != 2626997781U)
						{
							if (num != 3353852974U)
							{
								goto IL_0366;
							}
							if (!(key == "store"))
							{
								goto IL_0366;
							}
							return this.Skin = (this.RowSkin = new ButtonSkinStoreItemRow(this, this.w, this.h));
						}
						else
						{
							if (!(key == "recipe_trm"))
							{
								goto IL_0366;
							}
							this.click_snd = "enter_small";
							return this.Skin = (this.RowSkin = new ButtonSkinRecipeRowTrm(this, this.w, this.h));
						}
					}
					else
					{
						if (!(key == "recipe_trm_ingredient"))
						{
							goto IL_0366;
						}
						return this.Skin = (this.RowSkin = new ButtonSkinTrmIngItemRow(this, this.w, this.h));
					}
				}
				else if (num != 3657089922U)
				{
					if (num != 3867909202U)
					{
						if (num != 3967719375U)
						{
							goto IL_0366;
						}
						if (!(key == "recipe"))
						{
							goto IL_0366;
						}
						this.click_snd = "enter_small";
						return this.Skin = (this.RowSkin = new ButtonSkinRecipeRow(this, this.w, this.h));
					}
					else if (!(key == "normal"))
					{
						goto IL_0366;
					}
				}
				else
				{
					if (!(key == "guildquest"))
					{
						goto IL_0366;
					}
					return this.Skin = (this.RowSkin = new ButtonSkinGuildQuestItemRow(this, this.w, this.h));
				}
				return this.Skin = (this.RowSkin = new ButtonSkinItemRow(this, this.w, this.h));
			}
			IL_0366:
			return base.makeButtonSkin(key);
		}

		public void fineCount()
		{
			if (this.RowSkin != null)
			{
				this.RowSkin.fineCount(true);
			}
		}

		public override void OnPointerEnter()
		{
			if (this.RowSkin != null && this.RowSkin.ItemMng != null && !this.RowSkin.ItemMng.can_handle)
			{
				return;
			}
			base.OnPointerEnter();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		public aBtnItemRow setItem(UiItemManageBox ItemMng, ItemStorage Storage, ItemStorage.IRow ItmRow)
		{
			this.RowSkin.setItem(ItemMng, Storage, ItmRow);
			return this;
		}

		public NelItem getItemData()
		{
			return this.RowSkin.getItemData();
		}

		public ItemStorage.IRow getItemRow()
		{
			return this.RowSkin.getItemRow();
		}

		public bool is_fake_row
		{
			get
			{
				return this.RowSkin.getItemRow() != null && this.RowSkin.getItemRow().is_fake_row;
			}
		}

		public ItemStorage.ObtainInfo getItemInfo()
		{
			return this.RowSkin.getItemRow().Info;
		}

		public override int container_stencil_ref
		{
			get
			{
				if (this.Container != null && this.Container.stencil_ref >= 0)
				{
					return this.Container.stencil_ref;
				}
				return this.default_stencil_ref;
			}
		}

		public int default_stencil_ref = -1;

		public ButtonSkinItemRow RowSkin;
	}
}

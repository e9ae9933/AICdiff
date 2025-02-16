using System;
using XX;

namespace nel
{
	public class aBtnNel : aBtn
	{
		public override ButtonSkin makeButtonSkin(string key)
		{
			if (key != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(key);
				if (num <= 2352143343U)
				{
					if (num <= 1458100919U)
					{
						if (num <= 703916114U)
						{
							if (num <= 429881354U)
							{
								if (num != 104373016U)
								{
									if (num != 429881354U)
									{
										goto IL_0AC1;
									}
									if (!(key == "row_quest_pin"))
									{
										goto IL_0AC1;
									}
									this.click_snd = "enter_small";
									this._SkinRow = new ButtonSkinNelQuestPin(this, this.w, this.h);
									this.Skin = this._SkinRow;
									this._SkinRow.alignx = ALIGN.CENTER;
									return this.Skin;
								}
								else
								{
									if (!(key == "checkbox"))
									{
										goto IL_0AC1;
									}
									goto IL_0583;
								}
							}
							else if (num != 533373674U)
							{
								if (num != 703916114U)
								{
									goto IL_0AC1;
								}
								if (!(key == "chapter"))
								{
									goto IL_0AC1;
								}
								this.click_snd = "talk_progress";
								return this.Skin = new ButtonSkinNelChapter(this, this.w, this.h);
							}
							else
							{
								if (!(key == "blackwin"))
								{
									goto IL_0AC1;
								}
								goto IL_07F4;
							}
						}
						else if (num <= 953853289U)
						{
							if (num != 786246507U)
							{
								if (num != 953853289U)
								{
									goto IL_0AC1;
								}
								if (!(key == "kadomaru_map_marker"))
								{
									goto IL_0AC1;
								}
								this.click_snd = "enter_small";
								return this.Skin = new ButtonSKinKadomaruMapMarker(this, this.w, this.h);
							}
							else
							{
								if (!(key == "keycon_input"))
								{
									goto IL_0AC1;
								}
								return this.Skin = new ButtonSkinKeyConInputNel(this, this.w, this.h);
							}
						}
						else if (num != 1141775739U)
						{
							if (num != 1424148619U)
							{
								if (num != 1458100919U)
								{
									goto IL_0AC1;
								}
								if (!(key == "mini_sort"))
								{
									goto IL_0AC1;
								}
								return this.Skin = new ButtonSkinMiniSortNel(this, this.w, this.h);
							}
							else
							{
								if (!(key == "row_tab"))
								{
									goto IL_0AC1;
								}
								this.click_snd = "tool_hand_init";
								this.hover_snd = "";
								this._SkinRow = new ButtonSkinNelTab(this, this.w, this.h);
								this.Skin = this._SkinRow;
								return this.Skin;
							}
						}
						else if (!(key == "row"))
						{
							goto IL_0AC1;
						}
					}
					else if (num <= 1801086848U)
					{
						if (num <= 1754882087U)
						{
							if (num != 1528006944U)
							{
								if (num != 1754882087U)
								{
									goto IL_0AC1;
								}
								if (!(key == "keycon_iconmenu"))
								{
									goto IL_0AC1;
								}
								return this.Skin = new ButtonSkinKeyConIconMenuNel(this, this.w, this.h);
							}
							else
							{
								if (!(key == "keycon_icon"))
								{
									goto IL_0AC1;
								}
								return this.Skin = new ButtonSkinKeyConIconNel(this, this.w, this.h);
							}
						}
						else if (num != 1787948068U)
						{
							if (num != 1801086848U)
							{
								goto IL_0AC1;
							}
							if (!(key == "row_dark"))
							{
								goto IL_0AC1;
							}
							goto IL_07A5;
						}
						else if (!(key == "ui_category"))
						{
							goto IL_0AC1;
						}
					}
					else if (num <= 2058808823U)
					{
						if (num != 1919382533U)
						{
							if (num != 2058808823U)
							{
								goto IL_0AC1;
							}
							if (!(key == "album_thumb"))
							{
								goto IL_0AC1;
							}
							this.click_snd = "tool_hand_init";
							return this.Skin = new ButtonSkinNelAlbumThumb(this, this.w, this.h);
						}
						else if (!(key == "row_center"))
						{
							goto IL_0AC1;
						}
					}
					else if (num != 2166136261U)
					{
						if (num != 2324299930U)
						{
							if (num != 2352143343U)
							{
								goto IL_0AC1;
							}
							if (!(key == "normal_dark"))
							{
								goto IL_0AC1;
							}
							this.click_snd = "enter";
							return this.Skin = new ButtonSkinNormalNelDark(this, this.w, this.h);
						}
						else
						{
							if (!(key == "mini"))
							{
								goto IL_0AC1;
							}
							this.click_snd = "enter_small";
							return this.Skin = new ButtonSkinMiniNel(this, this.w, this.h);
						}
					}
					else
					{
						if (key == null)
						{
							goto IL_0AC1;
						}
						if (key.Length != 0)
						{
							goto IL_0AC1;
						}
						goto IL_055C;
					}
					this.click_snd = "enter_small";
					this._SkinRow = new ButtonSkinNelUi(this, this.w, this.h);
					this.Skin = this._SkinRow;
					if (key == "row_center")
					{
						this._SkinRow.alignx = ALIGN.CENTER;
					}
					return this.Skin;
				}
				if (num <= 3045830557U)
				{
					if (num <= 2894865639U)
					{
						if (num <= 2725351233U)
						{
							if (num != 2533765901U)
							{
								if (num != 2725351233U)
								{
									goto IL_0AC1;
								}
								if (!(key == "checkbox_small_dark"))
								{
									goto IL_0AC1;
								}
								goto IL_0623;
							}
							else
							{
								if (!(key == "keycon_label"))
								{
									goto IL_0AC1;
								}
								return this.Skin = new ButtonSkinKeyConLabelNel(this, this.w, this.h);
							}
						}
						else if (num != 2865238487U)
						{
							if (num != 2894865639U)
							{
								goto IL_0AC1;
							}
							if (!(key == "mini_dark"))
							{
								goto IL_0AC1;
							}
							this.click_snd = "enter_small";
							return this.Skin = new ButtonSkinMiniNelDark(this, this.w, this.h);
						}
						else
						{
							if (!(key == "popper"))
							{
								goto IL_0AC1;
							}
							this.click_snd = "enter";
							return this.Skin = new ButtonSkinNelPopper(this, this.w, this.h);
						}
					}
					else if (num <= 2923437748U)
					{
						if (num != 2921835916U)
						{
							if (num != 2923437748U)
							{
								goto IL_0AC1;
							}
							if (!(key == "radio"))
							{
								goto IL_0AC1;
							}
						}
						else
						{
							if (!(key == "checkbox_small"))
							{
								goto IL_0AC1;
							}
							goto IL_0583;
						}
					}
					else if (num != 2964010081U)
					{
						if (num != 2979133148U)
						{
							if (num != 3045830557U)
							{
								goto IL_0AC1;
							}
							if (!(key == "row_dark_lang"))
							{
								goto IL_0AC1;
							}
							this.click_snd = "enter_small";
							this._SkinRow = new ButtonSkinRowLangNel(this, this.w, this.h);
							this.Skin = this._SkinRow;
							return this.Skin;
						}
						else
						{
							if (!(key == "row_dark_center"))
							{
								goto IL_0AC1;
							}
							goto IL_07A5;
						}
					}
					else
					{
						if (!(key == "reelinfo"))
						{
							goto IL_0AC1;
						}
						this.click_snd = "";
						return this.Skin = new ButtonSkinNelReelInfo(this, this.w, this.h);
					}
				}
				else if (num <= 3728535457U)
				{
					if (num <= 3428062466U)
					{
						if (num != 3093392453U)
						{
							if (num != 3428062466U)
							{
								goto IL_0AC1;
							}
							if (!(key == "blackwin_center"))
							{
								goto IL_0AC1;
							}
							goto IL_07F4;
						}
						else
						{
							if (!(key == "checkbox_dark"))
							{
								goto IL_0AC1;
							}
							goto IL_0623;
						}
					}
					else if (num != 3541463400U)
					{
						if (num != 3728535457U)
						{
							goto IL_0AC1;
						}
						if (!(key == "kadomaru_icon"))
						{
							goto IL_0AC1;
						}
						this.click_snd = "enter_small";
						return this.Skin = new ButtonSKinKadomaruIconNel(this, this.w, this.h);
					}
					else
					{
						if (!(key == "row_fieldguide"))
						{
							goto IL_0AC1;
						}
						this.click_snd = "enter_small";
						this._SkinRow = new ButtonSkinNelFieldGuide(this, this.w, this.h);
						this.Skin = this._SkinRow;
						return this.Skin;
					}
				}
				else if (num <= 3917559624U)
				{
					if (num != 3867909202U)
					{
						if (num != 3917559624U)
						{
							goto IL_0AC1;
						}
						if (!(key == "radio_small"))
						{
							goto IL_0AC1;
						}
					}
					else
					{
						if (!(key == "normal"))
						{
							goto IL_0AC1;
						}
						goto IL_055C;
					}
				}
				else if (num != 4041525031U)
				{
					if (num != 4208477002U)
					{
						if (num != 4291752307U)
						{
							goto IL_0AC1;
						}
						if (!(key == "whole_map_area"))
						{
							goto IL_0AC1;
						}
						this.click_snd = "tool_eraser_init";
						return this.Skin = new ButtonSkinWholeMapArea(this, this.w, this.h);
					}
					else
					{
						if (!(key == "reel_pict"))
						{
							goto IL_0AC1;
						}
						this.click_snd = "tool_hand_init";
						return this.Skin = new ButtonSkinNelReelPict(this, this.w, this.h);
					}
				}
				else
				{
					if (!(key == "item_grade_star"))
					{
						goto IL_0AC1;
					}
					this.click_snd = "tool_gradation";
					this.hover_snd = "";
					this.Skin = new ButtonSkinItemGrade(this, this.w, this.h);
					return this.Skin;
				}
				this.click_snd = "talk_progress";
				ButtonSkinRadioNel buttonSkinRadioNel = new ButtonSkinRadioNel(this, this.w, this.h);
				buttonSkinRadioNel.fix_h = this.h;
				if (key == "radio_small")
				{
					buttonSkinRadioNel.setScale(1f);
				}
				this.Skin = buttonSkinRadioNel;
				return this.Skin;
				IL_0623:
				this.click_snd = "talk_progress";
				ButtonSkinCheckBoxNelDark buttonSkinCheckBoxNelDark = new ButtonSkinCheckBoxNelDark(this, this.w, this.h);
				buttonSkinCheckBoxNelDark.fix_h = this.h;
				if (key == "checkbox_small_dark")
				{
					buttonSkinCheckBoxNelDark.setScale(1f);
				}
				this.Skin = buttonSkinCheckBoxNelDark;
				return this.Skin;
				IL_055C:
				this.click_snd = "enter";
				return this.Skin = new ButtonSkinNormalNel(this, this.w, this.h);
				IL_0583:
				this.click_snd = "talk_progress";
				ButtonSkinCheckBoxNel buttonSkinCheckBoxNel = new ButtonSkinCheckBoxNel(this, this.w, this.h);
				buttonSkinCheckBoxNel.fix_h = this.h;
				if (key == "checkbox_small")
				{
					buttonSkinCheckBoxNel.setScale(1f);
				}
				this.Skin = buttonSkinCheckBoxNel;
				return this.Skin;
				IL_07A5:
				this.click_snd = "enter_small";
				this._SkinRow = new ButtonSkinRowNelDark(this, this.w, this.h);
				this.Skin = this._SkinRow;
				if (key == "row_dark_center")
				{
					this._SkinRow.alignx = ALIGN.CENTER;
				}
				return this.Skin;
				IL_07F4:
				this.click_snd = "enter_small";
				this._SkinRow = new ButtonSkinRowNelBlackWindow(this, this.w, this.h);
				this.Skin = this._SkinRow;
				if (key == "blackwin_center")
				{
					this._SkinRow.alignx = ALIGN.CENTER;
				}
				return this.Skin;
			}
			IL_0AC1:
			return base.makeButtonSkin(key);
		}

		protected ButtonSkinRow _SkinRow;
	}
}

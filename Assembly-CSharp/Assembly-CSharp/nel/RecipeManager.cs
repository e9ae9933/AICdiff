using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public static class RecipeManager
	{
		public static int ctg2iconId(RecipeManager.RP_CATEG cur_ctg)
		{
			if (cur_ctg == RecipeManager.RP_CATEG.COOK)
			{
				return 20;
			}
			if (cur_ctg != RecipeManager.RP_CATEG.ALOMA)
			{
				return 28;
			}
			return 42;
		}

		public static int getDefaultIcon(NelItem Item)
		{
			if (Item.RecipeInfo != null && Item.RecipeInfo.DishInfo.Rcp.categ == RecipeManager.RP_CATEG.ALOMA)
			{
				return 42;
			}
			return 7;
		}

		public static RecipeManager.RPI_CATEG calcCateg(string _categ_str, out RecipeManager.RPI_CATEG head_categ)
		{
			string[] array = TX.split(_categ_str, "|");
			head_categ = (RecipeManager.RPI_CATEG)0;
			RecipeManager.RPI_CATEG rpi_CATEG = (RecipeManager.RPI_CATEG)0;
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				RecipeManager.RPI_CATEG rpi_CATEG2;
				if (FEnum<RecipeManager.RPI_CATEG>.TryParse(array[i], out rpi_CATEG2, true))
				{
					if (head_categ == (RecipeManager.RPI_CATEG)0)
					{
						head_categ = rpi_CATEG2;
					}
					rpi_CATEG |= rpi_CATEG2;
				}
			}
			return rpi_CATEG;
		}

		public static int getRPICategIcon(RecipeManager.RPI_CATEG categ)
		{
			if (categ <= RecipeManager.RPI_CATEG.FLOWER)
			{
				if (categ <= RecipeManager.RPI_CATEG.FRUIT)
				{
					switch (categ)
					{
					case RecipeManager.RPI_CATEG.WHEAT:
						return 21;
					case RecipeManager.RPI_CATEG.VEGI:
						return 24;
					case (RecipeManager.RPI_CATEG)3:
						break;
					case RecipeManager.RPI_CATEG.MEAT:
						return 25;
					default:
						if (categ == RecipeManager.RPI_CATEG.FISH)
						{
							return 26;
						}
						if (categ == RecipeManager.RPI_CATEG.FRUIT)
						{
							return 2;
						}
						break;
					}
				}
				else if (categ <= RecipeManager.RPI_CATEG.EGG)
				{
					if (categ == RecipeManager.RPI_CATEG.MUSH)
					{
						return 8;
					}
					if (categ == RecipeManager.RPI_CATEG.EGG)
					{
						return 27;
					}
				}
				else
				{
					if (categ == RecipeManager.RPI_CATEG.WATER)
					{
						return 6;
					}
					if (categ == RecipeManager.RPI_CATEG.FLOWER)
					{
						return 31;
					}
				}
			}
			else if (categ <= RecipeManager.RPI_CATEG.CRAFTMATERIAL)
			{
				if (categ == RecipeManager.RPI_CATEG.SEASONING)
				{
					return 32;
				}
				if (categ == RecipeManager.RPI_CATEG.HERB)
				{
					return 31;
				}
				if (categ == RecipeManager.RPI_CATEG.CRAFTMATERIAL)
				{
					return 43;
				}
			}
			else if (categ <= RecipeManager.RPI_CATEG.ENEMY)
			{
				if (categ == RecipeManager.RPI_CATEG.JEWEL)
				{
					return 44;
				}
				if (categ == RecipeManager.RPI_CATEG.ENEMY)
				{
					return 41;
				}
			}
			else
			{
				if (categ == RecipeManager.RPI_CATEG.BEAN)
				{
					return 58;
				}
				if (categ == RecipeManager.RPI_CATEG.MILK)
				{
					return 60;
				}
			}
			return 0;
		}

		public static string getRPIEffectDescription(RecipeManager.RPI_EFFECT e, string t100, int adding_count = 1)
		{
			STB stb = TX.PopBld(null, 0);
			STB stb2 = TX.PopBld(t100, 0);
			RecipeManager.getRPIEffectDescriptionTo(stb, e, stb2, adding_count);
			string text = stb.ToString();
			TX.ReleaseBld(stb2);
			TX.ReleaseBld(stb);
			return text;
		}

		public static STB getRPIEffectDescriptionTo(STB Stb, RecipeManager.RPI_EFFECT e, STB StbT100, int adding_count = 1)
		{
			int num = StbT100.IndexOf('\n', 0, -1);
			Vector3 vector;
			if (num >= 0)
			{
				vector = new Vector3((float)StbT100.NmI(0, num, 0), (float)StbT100.NmI(num + 1, -1, 0), 2f);
			}
			else
			{
				vector = new Vector3((float)StbT100.NmI(0, -1, 0), 0f, 1f);
			}
			return RecipeManager.getRPIEffectDescriptionTo(Stb, e, vector, adding_count);
		}

		public static STB getRPIEffectDescriptionTo(STB Stb, RecipeManager.RPI_EFFECT e, int val100, int adding_count = 1)
		{
			return RecipeManager.getRPIEffectDescriptionTo(Stb, e, new Vector3((float)val100, 0f, 1f), adding_count);
		}

		public static STB getRPIEffectDescriptionTo(STB Stb, RecipeManager.RPI_EFFECT e, Vector3 Vlv100, int adding_count = 1)
		{
			int num = (int)Vlv100.z;
			bool flag = true;
			int i = 0;
			while (i < num)
			{
				float num2 = global::XX.X.MMX(-100f, Vlv100[i], 100f);
				string text = ((num2 > 0f) ? "+" : "");
				string text2 = ((num2 > 0f) ? "" : "+");
				string text3 = ((num2 > 0f) ? "-" : "+");
				if (!flag)
				{
					Stb.Add("〜");
				}
				else
				{
					flag = false;
				}
				if (num2 < 0f)
				{
					Stb.Add(NEL.error_tag);
				}
				switch (e)
				{
				case RecipeManager.RPI_EFFECT.MAXHP:
				case RecipeManager.RPI_EFFECT.MAXMP:
					Stb.Add(text, global::XX.X.IntR(num2), "%");
					break;
				case RecipeManager.RPI_EFFECT.MANA_NEUTRAL:
					Stb.Add(text, global::XX.X.IntR(num2 * 0.875f), "%");
					break;
				case RecipeManager.RPI_EFFECT.SHIELD_ENPOWER:
					Stb.Add(text, global::XX.X.IntR(num2 * 0.1f), "%");
					break;
				case RecipeManager.RPI_EFFECT.ATK:
					Stb.Add(text, global::XX.X.IntR(num2 * 0.75f), "%");
					break;
				case RecipeManager.RPI_EFFECT.ATK_MAGIC:
					Stb.Add(text, global::XX.X.IntR(num2 * 1f), "%");
					break;
				case RecipeManager.RPI_EFFECT.ATK_MAGIC_OVERSPELL:
					Stb.Add(text, global::XX.X.IntR(num2 * 0.66f), "%");
					break;
				case RecipeManager.RPI_EFFECT.PUNCH_DRAIN:
					Stb.Add(text, global::XX.X.IntR(num2 * 1f), "%");
					break;
				case RecipeManager.RPI_EFFECT.EVADE_NODAM_EXTEND:
					Stb.Add(text, global::XX.X.IntR(num2 * 0.875f), "%");
					break;
				case RecipeManager.RPI_EFFECT.ARREST_HPDAMAGE_REDUCE:
					if (num2 >= 0f)
					{
						Stb.Add(text3, global::XX.X.Abs(global::XX.X.IntR(global::XX.X.NI(0f, 0.75f, num2 / 100f) * 100f)), "%");
					}
					else
					{
						Stb.Add(text3, global::XX.X.Abs(global::XX.X.IntR(global::XX.X.NI(0f, 1f, -num2 / 100f) * 100f)), "%");
					}
					break;
				case RecipeManager.RPI_EFFECT.ARREST_MPDAMAGE_REDUCE:
					if (num2 >= 0f)
					{
						Stb.Add(text3, global::XX.X.Abs(global::XX.X.IntR(global::XX.X.NI(0f, 0.75f, num2 / 100f) * 100f)), "%");
					}
					else
					{
						Stb.Add(text3, global::XX.X.Abs(global::XX.X.IntR(global::XX.X.NI(0f, 1f, -num2 / 100f) * 100f)), "%");
					}
					break;
				case RecipeManager.RPI_EFFECT.SMOKE_RESIST:
					Stb.Add(text3, global::XX.X.Abs(global::XX.X.IntR(global::XX.X.NI(0f, 0.9f, num2 / 100f) * 100f)), "%");
					break;
				case RecipeManager.RPI_EFFECT.ARREST_ESCAPE:
					Stb.Add(text, global::XX.X.IntR(global::XX.X.NI(0f, 1f, num2 / 100f) * 100f), "%");
					break;
				case RecipeManager.RPI_EFFECT.FIRE_DAMAGE_REDUCE:
				case RecipeManager.RPI_EFFECT.ELEC_DAMAGE_REDUCE:
				case RecipeManager.RPI_EFFECT.FROZEN_DAMAGE_REDUCE:
					if (num2 >= 0f)
					{
						Stb.Add(text3, global::XX.X.Abs(global::XX.X.IntR(global::XX.X.NI(0f, 0.75f, num2 / 100f) * 100f)), "%");
					}
					else
					{
						Stb.Add(text3, global::XX.X.Abs(global::XX.X.IntR(global::XX.X.NI(0f, 1f, -num2 / 100f) * 100f)), "%");
					}
					break;
				case RecipeManager.RPI_EFFECT.SER_FAST:
				{
					float num3 = 1f / global::XX.X.Mx(0.0625f, global::XX.X.NI(1f, 2f, num2 / 100f));
					Stb.Add(text2, global::XX.X.IntR((num3 - 1f) * 100f), "%");
					break;
				}
				case RecipeManager.RPI_EFFECT.SINK_REDUCE:
					Stb.Add(text3, global::XX.X.Abs(global::XX.X.IntR(num2)), "%");
					break;
				case RecipeManager.RPI_EFFECT.CHANT_SPEED_OVERHOLD:
					Stb.Add(text, global::XX.X.IntR(global::XX.X.NI(0f, 9.64f, num2 / 100f) * 100f), "%");
					break;
				case RecipeManager.RPI_EFFECT.REEL_SPEED:
					Stb.Add(text3, global::XX.X.Abs(global::XX.X.IntR(global::XX.X.NI(0f, 0.65f, num2 / 100f) * 100f)), "%");
					break;
				case RecipeManager.RPI_EFFECT.SER_RESIST:
					Stb.Add(text, global::XX.X.IntR(global::XX.X.NI(0f, 0.75f, num2 / 100f) * 100f), "%");
					break;
				case RecipeManager.RPI_EFFECT.LOST_MP_WHEN_CHANTING:
					Stb.Add(text3, global::XX.X.Abs(global::XX.X.IntR(global::XX.X.NI(0f, 1f, num2 / 100f) * 100f)), "%");
					break;
				case RecipeManager.RPI_EFFECT.CHANT_SPEED:
					Stb.Add(text, global::XX.X.IntR(global::XX.X.NI(0f, 1f, num2 / 100f) * 100f), "%");
					break;
				case RecipeManager.RPI_EFFECT.MP_GAZE_RESIST:
					Stb.Add(text, global::XX.X.Abs(global::XX.X.IntR(global::XX.X.NI(0f, 1f, num2 / 100f) * 100f)), "%");
					break;
				case RecipeManager.RPI_EFFECT.SLEEP_RESIST:
					goto IL_05DA;
				case RecipeManager.RPI_EFFECT.RANDOM:
					if (adding_count >= 2)
					{
						Stb.Add(text, global::XX.X.IntR(num2 / (float)adding_count), "% x").Add(adding_count);
					}
					else
					{
						Stb.Add(text, global::XX.X.IntR(num2), "%");
					}
					break;
				case RecipeManager.RPI_EFFECT.DISH_VARIABLE:
					Stb.Add(text, global::XX.X.IntR(num2), "%");
					break;
				default:
					goto IL_05DA;
				}
				IL_05F7:
				if (num2 < 0f)
				{
					Stb.Add(NEL.error_tag_close);
				}
				i++;
				continue;
				IL_05DA:
				Stb.Add("", global::XX.X.IntR(num2 * 2f), "%");
				goto IL_05F7;
			}
			return Stb;
		}

		public static void initScript()
		{
			RecipeManager.ARp = new List<RecipeManager.Recipe>();
			CsvReaderA csvReaderA = new CsvReaderA(TX.getResource("Data/recipe", ".csv", false), true);
			RecipeManager.ODish = new BDic<uint, RecipeManager.RecipeDish>();
			RecipeManager.Recipe recipe = null;
			NelItem nelItem = null;
			RecipeManager.RP_CATEG rp_CATEG = RecipeManager.RP_CATEG.COOK;
			int num = 61600;
			while (csvReaderA.read())
			{
				if (TX.isStart(csvReaderA.cmd, "#", 0))
				{
					if (csvReaderA.cmd == "#COOK")
					{
						rp_CATEG = RecipeManager.RP_CATEG.COOK;
					}
					if (csvReaderA.cmd == "#ALCHEMY")
					{
						rp_CATEG = RecipeManager.RP_CATEG.ALCHEMY;
					}
					if (csvReaderA.cmd == "#ALOMA")
					{
						rp_CATEG = RecipeManager.RP_CATEG.ALOMA;
					}
					if (csvReaderA.cmd == "#ACTIHOL")
					{
						rp_CATEG = RecipeManager.RP_CATEG.ACTIHOL;
					}
				}
				else if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					if (recipe != null && RecipeManager.Get(csvReaderA.cmd) != null)
					{
						global::XX.X.de("重複key: (key: " + recipe.key + ")", null);
					}
					string index = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
					recipe = RecipeManager.Get(index);
					nelItem = null;
					if (recipe != null)
					{
						global::XX.X.de("重複キー: " + index, null);
						recipe = null;
					}
					else
					{
						recipe = new RecipeManager.Recipe(index, rp_CATEG);
						nelItem = NelItem.CreateItemEntry("Recipe_" + index, new NelItem("Recipe_" + index, 0, 140, 1)
						{
							category = (NelItem.CATEG)2097153U,
							FnGetName = new FnGetItemDetail(NelItem.fnGetNameRecipe),
							FnGetDesc = new FnGetItemDetail(NelItem.fnGetDescRecipe),
							FnGetDetail = new FnGetItemDetail(NelItem.fnGetDetailRecipe),
							specific_icon_id = RecipeManager.ctg2iconId(rp_CATEG),
							SpecificColor = C32.d2c(4294942310U)
						}, num++, false);
						RecipeManager.ARp.Add(recipe);
					}
				}
				else if (recipe != null)
				{
					string cmd = csvReaderA.cmd;
					if (cmd != null)
					{
						uint num2 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
						if (num2 <= 2578216853U)
						{
							if (num2 <= 1731888878U)
							{
								if (num2 != 444604893U)
								{
									if (num2 == 1731888878U)
									{
										if (cmd == "%COMPLETION")
										{
											recipe.Completion = NelItem.GetById(csvReaderA._1, false);
											continue;
										}
									}
								}
								else if (cmd == "%NAMEREPLACE")
								{
									if (recipe.AFinNameRepl == null)
									{
										recipe.AFinNameRepl = new RecipeManager.RecipeFinNameRepl[1];
									}
									else
									{
										global::XX.X.push<RecipeManager.RecipeFinNameRepl>(ref recipe.AFinNameRepl, default(RecipeManager.RecipeFinNameRepl), -1);
									}
									NelItem byId = NelItem.GetById(csvReaderA._1, false);
									if (byId != null)
									{
										recipe.AFinNameRepl[recipe.AFinNameRepl.Length - 1] = new RecipeManager.RecipeFinNameRepl(byId, recipe.AIng.Count - 1, csvReaderA.Int(2, -1), csvReaderA._3, csvReaderA._4);
										continue;
									}
									continue;
								}
							}
							else if (num2 != 2568633698U)
							{
								if (num2 == 2578216853U)
								{
									if (cmd == "%COST")
									{
										recipe.cost = csvReaderA.Int(1, recipe.cost);
										continue;
									}
								}
							}
							else if (cmd == "%RECIPE_OR")
							{
								global::XX.X.push<int>(ref recipe.Aor_splitter, recipe.AIng.Count, -1);
								continue;
							}
						}
						else if (num2 <= 3050540647U)
						{
							if (num2 != 2961679264U)
							{
								if (num2 == 3050540647U)
								{
									if (cmd == "%DEBUG")
									{
										recipe.debug_recipe = true;
										continue;
									}
								}
							}
							else if (cmd == "%IS_WATER")
							{
								recipe.is_water = csvReaderA.Int(1, 1) != 0;
								nelItem.specific_icon_id = 54;
								if (recipe.categ == RecipeManager.RP_CATEG.ACTIHOL)
								{
									nelItem.SpecificColor = C32.d2c(4281388200U);
									continue;
								}
								nelItem.SpecificColor = C32.d2c(4293998648U);
								continue;
							}
						}
						else if (num2 != 3095541196U)
						{
							if (num2 != 3896521336U)
							{
								if (num2 == 4072078424U)
								{
									if (cmd == "%CREATE_COUNT")
									{
										int num3 = csvReaderA.Int(1, 0);
										if (num3 < 0)
										{
											recipe.create_count = ((recipe.Completion != null) ? recipe.Completion.stock : 1) * global::XX.X.Abs(num3);
											continue;
										}
										recipe.create_count = num3;
										continue;
									}
								}
							}
							else if (cmd == "%AFTER_PRICE")
							{
								recipe.after_price = csvReaderA.Int(1, recipe.after_price);
								continue;
							}
						}
						else if (cmd == "%RECIPE_PRICE")
						{
							nelItem.price = csvReaderA.Int(1, 0);
							continue;
						}
					}
					RecipeManager.Recipe recipe2;
					NelItem nelItem2;
					RecipeManager.RPI_CATEG rpi_CATEG;
					NelItem.CATEG categ;
					if (RecipeManager.getIngredientTarget(csvReaderA, out recipe2, out nelItem2, out rpi_CATEG, out categ))
					{
						int num4 = csvReaderA.Int(1, 1);
						string text = csvReaderA.getIndex(2);
						bool flag = false;
						if (TX.isStart(text, '+'))
						{
							text = TX.slice(text, 1);
							flag = true;
						}
						RecipeManager.RecipeIngredient recipeIngredient = new RecipeManager.RecipeIngredient(nelItem2, recipe2, rpi_CATEG, categ, csvReaderA.Nm(3, 1f), num4, num4 + global::XX.X.NmI(text, 0, true, false), csvReaderA.Int(4, 0), recipe.AIng.Count, csvReaderA.Int(5, -2), flag);
						recipe.AIng.Add(recipeIngredient);
					}
				}
			}
			if (SkillManager.VALIDATION)
			{
				RecipeManager.Validate();
			}
		}

		public static bool getIngredientTarget(CsvReaderA CR, out RecipeManager.Recipe TargetRecipe, out NelItem Target, out RecipeManager.RPI_CATEG targ_category, out NelItem.CATEG targ_ni_category)
		{
			TargetRecipe = null;
			Target = null;
			targ_category = (RecipeManager.RPI_CATEG)0;
			targ_ni_category = NelItem.CATEG.OTHER;
			if (TX.isStart(CR.cmd, "&", 0))
			{
				TargetRecipe = RecipeManager.Get(TX.slice(CR.cmd, 1));
				if (TargetRecipe == null)
				{
					CR.tError("レシピ用の不明なレシピキー: " + CR.cmd);
					return false;
				}
			}
			else if (TX.isStart(CR.cmd, "@", 0))
			{
				RecipeManager.RPI_CATEG rpi_CATEG;
				targ_category = RecipeManager.calcCateg(TX.slice(CR.cmd, 1), out rpi_CATEG);
				if (targ_category == (RecipeManager.RPI_CATEG)0)
				{
					CR.tError("レシピ用の不明な RPI_CATEG 指定子: " + CR.cmd);
					return false;
				}
			}
			else if (TX.isStart(CR.cmd, "^", 0))
			{
				targ_ni_category = NelItem.calcCateg(TX.slice(CR.cmd, 1));
				if (targ_ni_category == NelItem.CATEG.OTHER)
				{
					CR.tError("レシピ用の不明な NelItem.CATEG 指定子: " + CR.cmd);
					return false;
				}
			}
			else
			{
				Target = NelItem.GetById(CR.cmd, true);
				if (Target == null)
				{
					CR.tError("レシピ用の不明なアイテム: " + CR.cmd);
					return false;
				}
			}
			return true;
		}

		public static void newGame()
		{
			RecipeManager.ODish.Clear();
			for (int i = RecipeManager.ARp.Count - 1; i >= 0; i--)
			{
				RecipeManager.ARp[i].newGame();
			}
			RecipeManager.dish_id = 0U;
		}

		public static void flush()
		{
			RecipeManager.ARp.ForEach(delegate(RecipeManager.Recipe V)
			{
				V.flush();
			});
		}

		public static RecipeManager.Recipe Get(string key)
		{
			for (int i = RecipeManager.ARp.Count - 1; i >= 0; i--)
			{
				RecipeManager.Recipe recipe = RecipeManager.ARp[i];
				if (recipe.key == key)
				{
					return recipe;
				}
			}
			return null;
		}

		public static RecipeManager.Recipe GetForCompletion(NelItem Itm)
		{
			for (int i = RecipeManager.ARp.Count - 1; i >= 0; i--)
			{
				RecipeManager.Recipe recipe = RecipeManager.ARp[i];
				if (recipe.Completion == Itm)
				{
					return recipe;
				}
			}
			return null;
		}

		public static RecipeManager.Recipe Get(NelItem Itm)
		{
			if (Itm == null)
			{
				return null;
			}
			if (Itm.is_food)
			{
				if (Itm.RecipeInfo == null)
				{
					return null;
				}
				return Itm.RecipeInfo.DishInfo.Rcp;
			}
			else
			{
				if (Itm != null)
				{
					return RecipeManager.Get(TX.slice(Itm.key, "Recipe_".Length));
				}
				return null;
			}
		}

		public static bool isFoodPacked(ItemStorage Storage, ItemStorage.IRow Row)
		{
			ItemStorage.IRow row;
			return Row.Data.is_food && Storage != null && Storage.isLinked(Row, out row);
		}

		public static void Validate()
		{
		}

		public static List<RecipeManager.RecipeDescription>[] listupDefinitionRecipe(NelItem Itm, bool only_useableItem = false)
		{
			List<RecipeManager.RecipeDescription> list = null;
			List<RecipeManager.RecipeDescription> list2 = null;
			int count = RecipeManager.ARp.Count;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < count; j++)
				{
					RecipeManager.Recipe recipe = RecipeManager.ARp[j];
					if (recipe.categ != RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH && !recipe.debug_recipe && recipe.categ == RecipeManager.RP_CATEG.ALOMA == (i == 1))
					{
						if (i == 0 && recipe.Completion == Itm)
						{
							if (list2 == null)
							{
								list2 = new List<RecipeManager.RecipeDescription>(1);
							}
							list2.Add(new RecipeManager.RecipeDescription(recipe, null, null));
						}
						int count2 = recipe.AIng.Count;
						int k = 0;
						while (k < count2)
						{
							if (recipe.AIng[k].forNeeds(Itm, false))
							{
								if (list == null)
								{
									list = new List<RecipeManager.RecipeDescription>(1);
								}
								if (i == 0)
								{
									list.Add(new RecipeManager.RecipeDescription(recipe, recipe.Completion, null));
									break;
								}
								TRMManager.listupDefinitionRecipe(ref list, recipe, only_useableItem);
								break;
							}
							else
							{
								k++;
							}
						}
					}
				}
				if (i == 0)
				{
					UiAlchemyWorkBench.listupDefinitionRecipe(ref list, Itm, only_useableItem);
				}
			}
			if (list == null && list2 == null)
			{
				return null;
			}
			return new List<RecipeManager.RecipeDescription>[] { list, list2 };
		}

		public static List<RecipeManager.Recipe> listupRecipeForCompletion(NelItem Itm, List<RecipeManager.Recipe> A = null)
		{
			int count = RecipeManager.ARp.Count;
			for (int i = 0; i < count; i++)
			{
				RecipeManager.Recipe recipe = RecipeManager.ARp[i];
				if (!recipe.debug_recipe && recipe.Completion == Itm)
				{
					if (A == null)
					{
						A = new List<RecipeManager.Recipe>(1);
					}
					A.Add(recipe);
				}
			}
			return A;
		}

		public static RecipeManager.RecipeDish getDish(NelItem Itm)
		{
			if (Itm != null && Itm.category == NelItem.CATEG.FOOD)
			{
				return global::XX.X.Get<uint, RecipeManager.RecipeDish>(RecipeManager.ODish, global::XX.X.NmUI(TX.slice(Itm.key, "__dish_".Length), 0U, false, false));
			}
			return null;
		}

		public static RecipeManager.RecipeDish getDishByRecipeId(uint id)
		{
			return global::XX.X.Get<uint, RecipeManager.RecipeDish>(RecipeManager.ODish, id);
		}

		public static List<RecipeManager.RecipeDish> getDishItemList(RecipeManager.Recipe R)
		{
			List<RecipeManager.RecipeDish> list = new List<RecipeManager.RecipeDish>();
			foreach (KeyValuePair<uint, RecipeManager.RecipeDish> keyValuePair in RecipeManager.ODish)
			{
				if (keyValuePair.Value.Rcp == R)
				{
					list.Add(keyValuePair.Value);
				}
			}
			return list;
		}

		public static NelItem assignDish(RecipeManager.RecipeDish Dh)
		{
			while (RecipeManager.ODish.ContainsKey(RecipeManager.dish_id += 1U) || NelItem.GetById("__dish_" + RecipeManager.dish_id.ToString(), true) != null)
			{
			}
			return RecipeManager.assignDish(Dh, RecipeManager.dish_id);
		}

		private static NelItem assignDish(RecipeManager.RecipeDish Dh, uint dish_id)
		{
			RecipeManager.ODish[dish_id] = Dh;
			NelItem nelItem = (Dh.ItemData = NelItem.CreateItemEntry("__dish_" + dish_id.ToString(), new NelItem("__dish_" + dish_id.ToString(), 0, Dh.price, 1), 65535, false));
			nelItem.value = dish_id;
			nelItem.max_price_enpower = (nelItem.max_grade_enpower = 1f);
			nelItem.category = NelItem.CATEG.FOOD | (Dh.Rcp.is_water ? NelItem.CATEG.WATER : NelItem.CATEG.OTHER);
			nelItem.RecipeInfo = new RecipeManager.RecipeItemInfo(nelItem, Dh);
			nelItem.SpecificColor = Dh.Rcp.RecipeItem.SpecificColor;
			if (Dh.Rcp.categ == RecipeManager.RP_CATEG.ACTIHOL)
			{
				nelItem.SpecificColor = C32.d2c(4281388200U);
				ulong num = 0UL;
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in Dh.OUseIngredientSource)
				{
					if (keyValuePair.Key.has(NelItem.CATEG.SER_APPLY))
					{
						int valueFor = keyValuePair.Key.getValueFor(NelItem.CATEG.SER_APPLY, keyValuePair.Value.top_grade);
						ulong num2;
						if (valueFor != 0 && NelItem.getSerApplyBits(keyValuePair.Key.key, out num2))
						{
							nelItem.category |= NelItem.CATEG.SER_APPLY;
							num |= num2;
							nelItem.value2 = global::XX.X.Mx(nelItem.value2, (float)(valueFor * keyValuePair.Value.total));
						}
					}
				}
				if (nelItem.has(NelItem.CATEG.SER_APPLY))
				{
					NelItem.setSerApplyBits(nelItem.key, num);
				}
			}
			nelItem.FnGetName = new FnGetItemDetail(NelItem.fnGetNameFood);
			nelItem.FnGetDesc = new FnGetItemDetail(NelItem.fnGetDescFood);
			return nelItem;
		}

		public static void removeDish(RecipeManager.RecipeDish Dh)
		{
			if (Dh != null && Dh.ItemData != null)
			{
				RecipeManager.ODish.Remove(Dh.recipe_id);
				Dh.referred = 0;
				NelItem.flush(Dh.ItemData);
				Dh.ItemData = null;
			}
		}

		public static RecipeManager.RecipeDish findSameDish(RecipeManager.RecipeDish Dh)
		{
			foreach (KeyValuePair<uint, RecipeManager.RecipeDish> keyValuePair in RecipeManager.ODish)
			{
				if (keyValuePair.Value != Dh && keyValuePair.Value.ItemData != null && keyValuePair.Value.isSame(Dh))
				{
					return keyValuePair.Value;
				}
			}
			return null;
		}

		public static void readBinaryFrom(ByteArray Ba, bool use_shift = true)
		{
			RecipeManager.newGame();
			if (use_shift)
			{
				Ba = Ba.readExtractBytesShifted(4);
			}
			try
			{
				int num = Ba.readByte();
				uint num2 = Ba.readUInt();
				bool flag = num >= 3;
				for (uint num3 = 0U; num3 < num2; num3 += 1U)
				{
					uint num4 = Ba.readUInt();
					RecipeManager.RecipeDish recipeDish = new RecipeManager.RecipeDish(null, 0).readBinaryContent(Ba, num);
					if (recipeDish.valid)
					{
						RecipeManager.assignDish(recipeDish, num4);
					}
				}
				int num5 = (int)Ba.readUShort();
				for (int i = 0; i < num5; i++)
				{
					RecipeManager.Recipe.readBinaryFrom(RecipeManager.Get(Ba.readString("utf-8", false)), Ba, flag);
				}
			}
			catch (Exception ex)
			{
				global::XX.X.dl("RecipeManager load error:" + ex.ToString(), null, false, false);
			}
		}

		public static ByteArray writeBinaryTo(ByteArray Ba_first)
		{
			ByteArray byteArray = new ByteArray(0U);
			byteArray.writeByte(3);
			byteArray.writeUInt((uint)RecipeManager.ODish.Count);
			foreach (KeyValuePair<uint, RecipeManager.RecipeDish> keyValuePair in RecipeManager.ODish)
			{
				byteArray.writeUInt(keyValuePair.Key);
				keyValuePair.Value.writeBinaryTo(byteArray);
			}
			using (BList<int> blist = ListBuffer<int>.Pop(0))
			{
				int num = RecipeManager.ARp.Count;
				for (int i = 0; i < num; i++)
				{
					RecipeManager.Recipe recipe = RecipeManager.ARp[i];
					if (recipe.created > 0U || recipe.eaten > 0)
					{
						blist.Add(i);
					}
				}
				num = blist.Count;
				byteArray.writeUShort((ushort)num);
				for (int j = 0; j < num; j++)
				{
					RecipeManager.Recipe recipe2 = RecipeManager.ARp[blist[j]];
					byteArray.writeString(recipe2.key, "utf-8");
					RecipeManager.Recipe.writeBinaryTo(recipe2, byteArray);
				}
			}
			if (Ba_first != null)
			{
				Ba_first.writeExtractBytesShifted(byteArray, 145, 4, -1);
			}
			return byteArray;
		}

		public static void readingFinalize()
		{
			RecipeManager.dish_id = 0U;
			List<RecipeManager.RecipeDish> list = new List<RecipeManager.RecipeDish>(4);
			foreach (KeyValuePair<uint, RecipeManager.RecipeDish> keyValuePair in RecipeManager.ODish)
			{
				if (keyValuePair.Value.referred == 0)
				{
					Debug.Log("レシピ情報" + keyValuePair.Value.title + " が削除されました");
					list.Add(keyValuePair.Value);
				}
				else if (RecipeManager.dish_id < keyValuePair.Key)
				{
					RecipeManager.dish_id = keyValuePair.Key;
				}
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				RecipeManager.removeDish(list[i]);
			}
		}

		public static void fineNameLocalizedWhole()
		{
			foreach (KeyValuePair<uint, RecipeManager.RecipeDish> keyValuePair in RecipeManager.ODish)
			{
				keyValuePair.Value.flushLocalizeTitle();
			}
		}

		public static string getCostString(int cost)
		{
			STB stb = TX.PopBld(null, 0);
			RecipeManager.getCostStringTo(stb, cost);
			string text = stb.ToString();
			TX.ReleaseBld(stb);
			return text;
		}

		public static STB getCostStringTo(STB Stb, int cost)
		{
			if (cost <= 0)
			{
				return Stb;
			}
			int num = global::XX.X.IntC((float)cost / 4f);
			cost--;
			for (int i = 0; i < num; i++)
			{
				Stb.Add("<img mesh=\"recipe_cost_", global::XX.X.Mn(3, cost), "\" width=\"17\" height=\"17\"/>");
				cost -= 4;
			}
			return Stb;
		}

		public static STB getCategoryStringMultiTo(STB Stb, RecipeManager.RPI_CATEG categ_whole)
		{
			int num = global::XX.X.beki_cntC(524288U);
			bool flag = true;
			for (int i = 0; i < num; i++)
			{
				RecipeManager.RPI_CATEG rpi_CATEG = categ_whole & (RecipeManager.RPI_CATEG)(1 << i);
				if (rpi_CATEG != (RecipeManager.RPI_CATEG)0)
				{
					if (!flag)
					{
						Stb.Add(",");
					}
					else
					{
						flag = false;
					}
					RecipeManager.getCategoryStringOneTo(Stb, rpi_CATEG);
				}
			}
			if (flag)
			{
				RecipeManager.getCategoryStringOneTo(Stb, (RecipeManager.RPI_CATEG)0);
			}
			return Stb;
		}

		public static STB getCategoryStringOneTo(STB Stb, RecipeManager.RPI_CATEG c)
		{
			Stb.Add("<img mesh=");
			if (c == RecipeManager.RPI_CATEG.FROMNOEL)
			{
				Stb.Add("\"IconNoel0\"");
			}
			else
			{
				Stb.Add("\"itemrow_category.", RecipeManager.getRPICategIcon(c), "\" tx_color");
			}
			Stb.Add(" />");
			Stb.AddTxA("recipe_item_categ_" + FEnum<RecipeManager.RPI_CATEG>.ToStr(c).ToLower(), false);
			return Stb;
		}

		public static string getDishDetail(RecipeManager.RecipeDish Dh, string delimiter = ", ")
		{
			STB stb = TX.PopBld(null, 0);
			RecipeManager.getDishDetailTo(stb, Dh, delimiter);
			string text = stb.ToString();
			TX.ReleaseBld(stb);
			return text;
		}

		public static STB getDishDetailTo(STB Stb, RecipeManager.RecipeDish Dh, string delimiter = ", ")
		{
			Stb.AddTxA("Item_for_food_cost", false);
			STB stb = TX.PopBld(null, 0);
			RecipeManager.getCostStringTo(stb, Dh.cost);
			stb.Add("\n");
			RecipeManager.getEffectListupTo(stb, Dh, 1f, delimiter, "Item_for_food_no_effect");
			Stb.TxRpl(stb);
			TX.ReleaseBld(stb);
			return Stb;
		}

		public static string getEffectListup(RecipeManager.RecipeDish Dh, string delimiter = ", ", string no_effect_tx = "Item_for_food_no_effect")
		{
			return RecipeManager.getEffectListup(Dh, 1f, delimiter, no_effect_tx);
		}

		public static string getEffectListup(RecipeManager.RecipeDish Dh, float lvl01, string delimiter = ", ", string no_effect_tx = "Item_for_food_no_effect")
		{
			STB stb = TX.PopBld(null, 0);
			RecipeManager.getEffectListupTo(stb, Dh, lvl01, delimiter, no_effect_tx);
			string text = stb.ToString();
			TX.ReleaseBld(stb);
			return text;
		}

		public static STB getEffectListupTo(STB Stb, RecipeManager.RecipeDish Dh, float lvl01, string delimiter = ", ", string no_effect_tx = "Item_for_food_no_effect")
		{
			if (Dh.OEffect.Count == 0)
			{
				return Stb.AddTxA(no_effect_tx, false);
			}
			bool flag = true;
			STB stb = TX.PopBld(null, 0);
			foreach (KeyValuePair<RecipeManager.RPI_EFFECT, Vector2> keyValuePair in Dh.OEffect)
			{
				if (flag)
				{
					Stb.AddTxA("Item_for_food_effect_content", false);
				}
				else
				{
					Stb.AppendTxA("Item_for_food_effect_content", delimiter);
				}
				stb.Set("・");
				stb.AddTxA("recipe_effect_" + keyValuePair.Key.ToString().ToLower(), false);
				Stb.TxRpl(stb);
				RecipeManager.getRPIEffectDescriptionTo(stb.Clear(), keyValuePair.Key, new Vector3(keyValuePair.Value.x * 100f * lvl01, 0f, 1f), (int)keyValuePair.Value.y);
				Stb.TxRpl(stb);
				flag = false;
			}
			TX.ReleaseBld(stb);
			return Stb;
		}

		public static string getIngredientListup(RecipeManager.RecipeDish Dh, string delimiter = ", ")
		{
			STB ingredientListupTo = RecipeManager.getIngredientListupTo(TX.PopBld(null, 0), Dh, delimiter);
			string text = ingredientListupTo.ToString();
			TX.ReleaseBld(ingredientListupTo);
			return text;
		}

		public static STB getIngredientListupTo(STB Stb, RecipeManager.RecipeDish Dh, string delimiter = ", ")
		{
			bool flag = true;
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in Dh.OUseIngredientSource)
			{
				int num = 5;
				for (int i = 0; i < num; i++)
				{
					int count = keyValuePair.Value.getCount(i);
					if (count > 0)
					{
						if (!flag)
						{
							Stb.Add(delimiter);
						}
						else
						{
							flag = false;
						}
						Stb.Add(keyValuePair.Key.getLocalizedName(i, null)).Add("<img mesh=\"nel_item_grade.", 5 + i, "\" width=\"38\" tx_color />").Add("x" + count.ToString());
					}
				}
			}
			return Stb;
		}

		private static List<RecipeManager.Recipe> ARp;

		private static BDic<uint, RecipeManager.RecipeDish> ODish;

		private const string recipe_script = "Data/recipe";

		public const string recipe_item_header = "Recipe_";

		public const string recipe_dish_header = "__dish_";

		public const uint col_actihol = 4281388200U;

		public const uint col_tea = 4293998648U;

		private static uint dish_id;

		private const float effect_view_rate_default = 2f;

		public enum RP_CATEG
		{
			COOK,
			ALCHEMY,
			ALCHEMY_WORKBENCH,
			ALOMA,
			ACTIHOL,
			_MAX
		}

		public enum RPI_CATEG
		{
			WHEAT = 1,
			VEGI,
			MEAT = 4,
			FISH = 8,
			FRUIT = 16,
			MUSH = 32,
			EGG = 64,
			WATER = 128,
			FROMNOEL = 256,
			FLOWER = 512,
			SEASONING = 1024,
			HERB = 2048,
			OTHER = 4096,
			CRAFTMATERIAL = 8192,
			JEWEL = 16384,
			ENEMY = 32768,
			CUSHION = 65536,
			BEAN = 131072,
			MILK = 262144,
			_MAX = 524288,
			_FOR_COOKING = 8191,
			_NOT_FOR_COOKING = 516096
		}

		public enum RPI_EFFECT
		{
			NONE,
			MAXHP,
			MAXMP,
			MANA_NEUTRAL,
			SHIELD_ENPOWER,
			ATK,
			ATK_MAGIC,
			ATK_MAGIC_OVERSPELL,
			PUNCH_DRAIN,
			EVADE_NODAM_EXTEND,
			ARREST_HPDAMAGE_REDUCE,
			ARREST_MPDAMAGE_REDUCE,
			SMOKE_RESIST,
			ARREST_ESCAPE,
			FIRE_DAMAGE_REDUCE,
			ELEC_DAMAGE_REDUCE,
			SER_FAST,
			SINK_REDUCE,
			CHANT_SPEED_OVERHOLD,
			REEL_SPEED,
			SER_RESIST,
			LOST_MP_WHEN_CHANTING,
			CHANT_SPEED,
			MP_GAZE_RESIST,
			FROZEN_DAMAGE_REDUCE,
			SLEEP_RESIST,
			RANDOM,
			DISH_VARIABLE,
			__LISTUP_MAX
		}

		public struct RecipeFinNameRepl
		{
			public RecipeFinNameRepl(NelItem TriggerItem, int _src_index, int _target_index, string _tx_key_target, string _tx_key_src)
			{
				this.trigger_item_key = TriggerItem.key;
				this.src_index = _src_index;
				this.target_index = _target_index;
				this.tx_key_target = _tx_key_target;
				this.tx_key_src = _tx_key_src;
			}

			public bool valid
			{
				get
				{
					return this.trigger_item_key != null;
				}
			}

			public string trigger_item_key;

			public int src_index;

			public int target_index;

			public string tx_key_target;

			public string tx_key_src;
		}

		public class MakeDishDescription : List<RecipeManager.MakeDishIngDescription>
		{
			public MakeDishDescription(int capacity)
				: base(capacity)
			{
			}

			public int cost_add;

			public float power_multiply = 1f;

			public int count = 1;

			public int price = -1;

			public bool tx_no_grade;
		}

		public struct MakeDishIngDescription
		{
			public bool valid
			{
				get
				{
					return this.power_multiply != 0f;
				}
			}

			public MakeDishIngDescription(float _power_multiply)
			{
				this.power_multiply = _power_multiply;
				this.power_weeken_ignore = 0f;
				this.tx_enough_ignore = (this.tx_listup = (this.tx_ignore = false));
			}

			public float power_multiply;

			public bool tx_enough_ignore;

			public bool tx_listup;

			public bool tx_ignore;

			public float power_weeken_ignore;
		}

		public class RecipeItemInfo
		{
			public RecipeItemInfo(NelItem Itm, string _categ, int _cost, string _power, string _effect)
			{
				this.Oeffect100 = new BDic<RecipeManager.RPI_EFFECT, float>();
				this.cost = _cost;
				if (TX.valid(_effect))
				{
					if (TX.noe(_power))
					{
						_power = "50";
					}
					string[] array = TX.split(_power, "|");
					string[] array2 = TX.split(_effect, "|");
					int num = array2.Length;
					int num2 = array.Length;
					for (int i = 0; i < num; i++)
					{
						RecipeManager.RPI_EFFECT rpi_EFFECT;
						if (!FEnum<RecipeManager.RPI_EFFECT>.TryParse(array2[i].ToUpper(), out rpi_EFFECT, true))
						{
							global::XX.X.de("不明なRPI_EFFECT:" + array2[i], null);
						}
						else
						{
							this.Oeffect100[rpi_EFFECT] = global::XX.X.Nm(array[i % num2], 0f, false);
						}
					}
				}
				if (_categ != null)
				{
					RecipeManager.RPI_CATEG rpi_CATEG;
					this.categ = RecipeManager.calcCateg(_categ, out rpi_CATEG);
					if (rpi_CATEG != (RecipeManager.RPI_CATEG)0 && (Itm.category & (NelItem.CATEG)69632U) == NelItem.CATEG.OTHER)
					{
						if (Itm.SpecificColor.a == 0 && (this.categ & RecipeManager.RPI_CATEG._FOR_COOKING) != (RecipeManager.RPI_CATEG)0)
						{
							Itm.SpecificColor = C32.d2c(4293964645U);
						}
						if (Itm.specific_icon_id == -1)
						{
							Itm.specific_icon_id = RecipeManager.getRPICategIcon(rpi_CATEG);
						}
					}
				}
			}

			public RecipeItemInfo(NelItem Itm, RecipeManager.RecipeDish Dh)
			{
				this.Oeffect100 = new BDic<RecipeManager.RPI_EFFECT, float>();
				this.cost = Dh.cost;
				this.DishInfo = Dh;
			}

			public STB getDetailTo(STB Stb, NelItem Itm, NelItem.GrdVariation GrdVar)
			{
				if (this.DishInfo != null)
				{
					return RecipeManager.getDishDetailTo(Stb, this.DishInfo, "\n");
				}
				STB stb = TX.PopBld(null, 0);
				if (this.cost > 0 || !Itm.is_tool)
				{
					if (this.cost >= 0)
					{
						Stb.AddTxA("Item_for_food_cost_and_category", false);
						Stb.TxRpl(RecipeManager.getCostStringTo(stb, this.cost));
						RecipeManager.getCategoryStringMultiTo(stb.Clear(), this.categ);
						Stb.TxRpl(stb);
					}
					else
					{
						RecipeManager.getCategoryStringMultiTo(Stb, this.categ);
					}
				}
				if (this.Oeffect100.Count == 0)
				{
					if (this.cost >= 0)
					{
						Stb.AppendTxA("Item_for_food_no_effect", "\n");
					}
				}
				else
				{
					Stb.AppendTxA("Item_for_food_effect", "\n");
					STB stb2 = TX.PopBld(null, 0);
					bool flag = true;
					foreach (KeyValuePair<RecipeManager.RPI_EFFECT, float> keyValuePair in this.Oeffect100)
					{
						if (!flag)
						{
							Stb.Add("\n");
						}
						else
						{
							flag = false;
						}
						Stb.Add("・");
						Stb.AddTxA("Item_for_food_effect_content", false);
						Stb.TxRpl(TX.Get("recipe_effect_" + keyValuePair.Key.ToString().ToLower(), ""));
						Stb.TxRpl(RecipeManager.getRPIEffectDescriptionTo(stb.Clear(), keyValuePair.Key, GrdVar.getDetailTo(stb2.Clear(), keyValuePair.Value / Itm.max_grade_enpower, "\n", false), 1));
					}
					TX.ReleaseBld(stb2);
				}
				TX.ReleaseBld(stb);
				return Stb;
			}

			public int cost;

			public RecipeManager.RPI_CATEG categ;

			public BDic<RecipeManager.RPI_EFFECT, float> Oeffect100;

			public RecipeManager.RecipeDish DishInfo;
		}

		public class RecipeIngredient
		{
			public RecipeIngredient(NelItem _Target, RecipeManager.Recipe _TargetRecipe, RecipeManager.RPI_CATEG _target_category, NelItem.CATEG _target_ni_category, float _power, int _need, int _max, int _grade, int _index, int _name_id = -2, bool _allloc_over_quantity = false)
			{
				this.Target = _Target;
				this.TargetRecipe = _TargetRecipe;
				this.target_category = _target_category;
				this.target_ni_category = _target_ni_category;
				this.power = _power;
				this.need = _need;
				this.max = global::XX.X.Mx(_max, this.need);
				this.grade = _grade;
				this.index = _index;
				this.name_id = _name_id;
				this.allloc_over_quantity = _allloc_over_quantity;
			}

			public RecipeIngredient(RecipeManager.RecipeIngredient Src)
				: this(Src.Target, Src.TargetRecipe, Src.target_category, Src.target_ni_category, Src.power, Src.need, Src.max, Src.grade, Src.index, Src.name_id, Src.allloc_over_quantity)
			{
			}

			public bool isSame(RecipeManager.RecipeIngredient Src)
			{
				return (this.Target == null || Src.Target == this.Target) && (this.TargetRecipe == null || Src.TargetRecipe == this.TargetRecipe) && (this.target_category == (RecipeManager.RPI_CATEG)0 || Src.target_category == this.target_category) && (this.target_ni_category == NelItem.CATEG.OTHER || Src.target_ni_category == this.target_ni_category) && this.need == Src.need && this.grade == Src.grade;
			}

			public bool checkUseable(List<RecipeManager.RecipeIngredient> AChecker, ItemStorage[] AInventory, int count = 1)
			{
				if (count == 1)
				{
					for (int i = AChecker.Count - 1; i >= 0; i--)
					{
						RecipeManager.RecipeIngredient recipeIngredient = AChecker[i];
						if (recipeIngredient.isSame(this))
						{
							return this.useable = recipeIngredient.useable;
						}
					}
				}
				bool flag = false;
				if (this.need == 0)
				{
					flag = true;
				}
				else
				{
					int num = this.need * count;
					if (this.Target != null)
					{
						int num2 = AInventory.Length;
						int num3 = 0;
						for (int j = 0; j < num2; j++)
						{
							num3 += AInventory[j].getCountMoreGrade(this.Target, this.grade);
							if (num3 >= num)
							{
								break;
							}
						}
						if (num3 >= num)
						{
							flag = true;
						}
					}
					if (this.target_category != (RecipeManager.RPI_CATEG)0)
					{
						int num4 = AInventory.Length;
						int num5 = 0;
						for (int k = 0; k < num4; k++)
						{
							foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in AInventory[k].getWholeInfoDictionary())
							{
								NelItem key = keyValuePair.Key;
								if (key.RecipeInfo != null && (this.target_category & key.RecipeInfo.categ) != (RecipeManager.RPI_CATEG)0)
								{
									num5 += keyValuePair.Value.getCountMoreGrade(this.grade);
									if (num5 >= num)
									{
										break;
									}
								}
							}
							if (num5 >= num)
							{
								break;
							}
						}
						if (num5 >= num)
						{
							flag = true;
						}
					}
					if (this.target_ni_category != NelItem.CATEG.OTHER)
					{
						int num6 = AInventory.Length;
						int num7 = 0;
						for (int l = 0; l < num6; l++)
						{
							foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair2 in AInventory[l].getWholeInfoDictionary())
							{
								if ((keyValuePair2.Key.category & this.target_ni_category) == this.target_ni_category)
								{
									num7 += keyValuePair2.Value.getCountMoreGrade(this.grade);
									if (num7 >= num)
									{
										break;
									}
								}
							}
							if (num7 >= num)
							{
								break;
							}
						}
						if (num7 >= num)
						{
							flag = true;
						}
					}
					if (this.TargetRecipe != null)
					{
						int num8 = AInventory.Length;
						int num9 = 0;
						foreach (KeyValuePair<uint, RecipeManager.RecipeDish> keyValuePair3 in RecipeManager.ODish)
						{
							if (keyValuePair3.Value.ItemData != null && keyValuePair3.Value.Rcp == this.TargetRecipe)
							{
								for (int m = 0; m < num8; m++)
								{
									ItemStorage.ObtainInfo info = AInventory[m].getInfo(keyValuePair3.Value.ItemData);
									if (info != null)
									{
										num9 += info.getCountMoreGrade(this.grade);
										if (num9 >= num)
										{
											break;
										}
									}
								}
								if (num9 >= num)
								{
									break;
								}
							}
						}
						if (num9 >= num)
						{
							flag = true;
						}
						else if (this.TargetRecipe.CInfo.obtain_flag)
						{
							flag = this.TargetRecipe.checkUseable(AChecker, AInventory, num - num9);
						}
					}
				}
				if (count == 1)
				{
					AChecker.Add(new RecipeManager.RecipeIngredient(this)
					{
						useable = (this.useable = flag)
					});
				}
				return flag;
			}

			public bool forNeeds(NelItem Itm, bool check_recipe_inner = true)
			{
				if (Itm == null)
				{
					return false;
				}
				if (this.Target != null && Itm == this.Target)
				{
					return true;
				}
				if (this.TargetRecipe != null)
				{
					RecipeManager.RecipeDish dish = RecipeManager.getDish(Itm);
					if (dish != null && dish.Rcp == this.TargetRecipe)
					{
						return true;
					}
					if (!check_recipe_inner)
					{
						return false;
					}
					if (this.TargetRecipe.forNeeds(Itm))
					{
						return true;
					}
				}
				return (this.target_category != (RecipeManager.RPI_CATEG)0 && Itm.RecipeInfo != null && (Itm.RecipeInfo.categ & this.target_category) != (RecipeManager.RPI_CATEG)0) || (this.target_ni_category != NelItem.CATEG.OTHER && (Itm.category & this.target_ni_category) == this.target_ni_category);
			}

			public void touchObtainCountIngredient()
			{
				if (this.Target != null)
				{
					this.Target.touchObtainCount();
				}
			}

			public string ingredient_desc(bool category_name = false, bool add_need_count = true)
			{
				string text;
				using (STB stb = TX.PopBld(null, 0))
				{
					this.ingredientDescTo(stb, category_name, add_need_count);
					text = stb.ToString();
				}
				return text;
			}

			public static int getToolIcon(NelItem.CATEG target_ni_category)
			{
				if (target_ni_category != (NelItem.CATEG)1179648U)
				{
					return 51;
				}
				return 23;
			}

			public STB ingredientDescTo(STB Stb, bool category_name = false, bool add_need_count = true)
			{
				if (this.Target != null)
				{
					Stb.Add(this.Target.getLocalizedName(this.grade, null));
				}
				else if (this.TargetRecipe != null)
				{
					Stb.Add(this.TargetRecipe.title);
				}
				else
				{
					if (this.target_category != (RecipeManager.RPI_CATEG)0)
					{
						using (STB stb = TX.PopBld(null, 0))
						{
							RecipeManager.RecipeIngredient.ingredientDescForRPCategTo(stb, this.target_category, category_name);
							Stb.Add((stb.Length > 0) ? " " : "").Add(stb);
							goto IL_00BD;
						}
					}
					if (this.target_ni_category != NelItem.CATEG.OTHER)
					{
						NelItem.addCategoryDescTo(Stb, this.target_ni_category);
						Stb.Add(": ");
					}
					else
					{
						Stb.Add("???");
					}
				}
				IL_00BD:
				if (this.need == 0)
				{
					Stb.Insert(0, "(+ ").Add(")");
				}
				if (this.grade > 0)
				{
					Stb.Add("<img mesh=\"nel_item_grade.", 5 + this.grade, "\" width=\"34\" tx_color \"/>");
				}
				if (this.need > 1 && add_need_count)
				{
					Stb.Add(" x", this.need, "");
				}
				return Stb;
			}

			public static STB ingredientDescForRPCategTo(STB Stb, RecipeManager.RPI_CATEG v, bool category_name = false)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					for (int i = 0; i < 64; i++)
					{
						RecipeManager.RPI_CATEG rpi_CATEG = (RecipeManager.RPI_CATEG)(1 << i);
						RecipeManager.RPI_CATEG rpi_CATEG2 = v & ~rpi_CATEG;
						if (rpi_CATEG2 != v)
						{
							v = rpi_CATEG2;
							Stb.Add("<img mesh=\"itemrow_category.", RecipeManager.getRPICategIcon(rpi_CATEG), "\" tx_color />");
							if (category_name)
							{
								stb.AppendTxA("recipe_item_categ_" + rpi_CATEG.ToString().ToLower(), ", ");
							}
							if (v == (RecipeManager.RPI_CATEG)0)
							{
								break;
							}
						}
					}
					Stb.Add(stb);
				}
				return Stb;
			}

			public float getEffectRatio100(float using_count, float power_weeken_ignore = 0f)
			{
				if (this.max == this.need || using_count <= (float)this.need || this.allloc_over_quantity || power_weeken_ignore >= 1f)
				{
					return this.power;
				}
				float num = (float)global::XX.X.IntR(global::XX.X.NIL(1f, global::XX.X.Mx(0.5f, (float)this.need) / using_count, 0.66f, 1f) * this.power);
				if (power_weeken_ignore > 0f)
				{
					num = global::XX.X.NIL(num, this.power, power_weeken_ignore, 1f);
				}
				return num;
			}

			public bool createDish(RecipeManager.RecipeDish Dh, List<UiCraftBase.IngEntryRow> L, float lvl, ref int total_item_for_quality, ref int total_grade, bool abort_if_missing = true, RecipeManager.MakeDishIngDescription RDesc = default(RecipeManager.MakeDishIngDescription))
			{
				if (L == null)
				{
					return !abort_if_missing;
				}
				int count = L.Count;
				if (abort_if_missing && count < this.need)
				{
					return false;
				}
				float num = lvl * this.getEffectRatio100((float)count, RDesc.valid ? RDesc.power_weeken_ignore : 0f) / 100f * (RDesc.valid ? RDesc.power_multiply : 1f);
				for (int i = 0; i < count; i++)
				{
					L[i].addEffectToDish(Dh, num, ref total_item_for_quality, ref total_grade, abort_if_missing);
				}
				return true;
			}

			public bool isCreatable(List<UiCraftBase.IngEntryRow> A)
			{
				return A.Count >= this.need;
			}

			public bool is_tool
			{
				get
				{
					return (this.Target != null && (this.Target.category & NelItem.CATEG.TOOL) != NelItem.CATEG.OTHER) || (this.target_ni_category != NelItem.CATEG.OTHER && (this.target_ni_category & NelItem.CATEG.TOOL) > NelItem.CATEG.OTHER);
				}
			}

			public readonly NelItem Target;

			public readonly RecipeManager.Recipe TargetRecipe;

			public readonly RecipeManager.RPI_CATEG target_category;

			public readonly NelItem.CATEG target_ni_category;

			public readonly float power = 1f;

			public readonly int need;

			public readonly int max;

			public readonly int grade;

			public readonly int index;

			public bool useable;

			public bool allloc_over_quantity;

			public int name_id = -2;
		}

		public class Recipe
		{
			public Recipe(string _key, RecipeManager.RP_CATEG _categ = RecipeManager.RP_CATEG.COOK)
			{
				this.key = _key;
				this.categ = _categ;
				this.AIng = new List<RecipeManager.RecipeIngredient>();
				this.Aor_splitter = new int[0];
			}

			public void newGame()
			{
				this.AAPrevIngredient = null;
				this.created = 0U;
				this.eaten = 0;
			}

			public void flush()
			{
				this.useable = 2;
				this.CInfo.obtain_flag = false;
			}

			public bool checkUseable(List<RecipeManager.RecipeIngredient> AChecker, ItemStorage[] AInventory, int count = 1)
			{
				int num = (int)((count == 1) ? this.useable : 2);
				if (num == 2)
				{
					int count2 = this.AIng.Count;
					int i = 0;
					int num2 = 0;
					num = 1;
					while (i < count2 && num == 1)
					{
						if (num2 < this.Aor_splitter.Length && i >= this.Aor_splitter[num2])
						{
							int num3 = ((++num2 >= this.Aor_splitter.Length) ? count2 : this.Aor_splitter[num2]);
							int num4 = 0;
							while (i < num3)
							{
								if (this.AIng[i].checkUseable(AChecker, AInventory, count))
								{
									i = num3;
									num4 = 1;
									break;
								}
								i++;
							}
							if (num4 == 0)
							{
								num = 0;
							}
						}
						else
						{
							num = ((this.AIng[i].checkUseable(AChecker, AInventory, count) && num == 1) ? 1 : 0);
							i++;
						}
					}
				}
				if (count == 1)
				{
					this.useable = (byte)num;
				}
				return num == 1;
			}

			public bool forNeeds(NelItem Itm)
			{
				int count = this.AIng.Count;
				for (int i = 0; i < count; i++)
				{
					if (this.AIng[i].forNeeds(Itm, true))
					{
						return true;
					}
				}
				return false;
			}

			public bool validate()
			{
				bool flag = true;
				if (this.AIng.Count == 0)
				{
					global::XX.X.de("RecipeManager: Ingredient ==0", null);
					flag = false;
				}
				if (this.Completion == null)
				{
					if (TX.getTX("Recipe_title_" + this.key, true, true, null) == null)
					{
						global::XX.X.de("RecipeManager: テキスト Recipe_title_" + this.key + " がありません", null);
						flag = false;
					}
					if (TX.getTX("Recipe_desc_" + this.key, true, true, null) == null)
					{
						global::XX.X.de("RecipeManager: テキスト Recipe_desc_" + this.key + " がありません", null);
						flag = false;
					}
				}
				return flag;
			}

			public string listupIngredients(string delimiter = "／", bool consider_useable = false, bool show_category_name = false)
			{
				string text3;
				using (STB stb = TX.PopBld(null, 0))
				{
					int count = this.AIng.Count;
					int i = 0;
					int num = 0;
					bool flag = false;
					string text = delimiter;
					int length = stb.Length;
					while (i < count)
					{
						if (num < this.Aor_splitter.Length && i >= this.Aor_splitter[num])
						{
							if (stb.Length > length)
							{
								stb.Add((num > 0) ? " ) " : "", text);
							}
							stb.Add("( ");
							delimiter = " | ";
							flag = true;
							num++;
						}
						RecipeManager.RecipeIngredient recipeIngredient = this.AIng[i];
						string text2 = recipeIngredient.ingredient_desc(show_category_name, true);
						if (consider_useable)
						{
							text2 = "<shape tx_color " + (recipeIngredient.useable ? "check" : "none") + " /> " + text2;
						}
						if (flag)
						{
							stb.Add(text2);
						}
						else
						{
							stb.Append(text2, delimiter);
						}
						flag = false;
						i++;
					}
					if (num > 0)
					{
						stb.Add(" )");
					}
					text3 = stb.ToString();
				}
				return text3;
			}

			public RecipeManager.RecipeDish createDish(List<List<UiCraftBase.IngEntryRow>> AA, bool abort_if_missing = true, RecipeManager.MakeDishDescription ADesc = null)
			{
				RecipeManager.RecipeDish recipeDish = new RecipeManager.RecipeDish(null, 0).Create(this);
				int num = 0;
				int num2 = 0;
				int count = this.AIng.Count;
				int i = 0;
				int num3 = 0;
				int num4 = 0;
				float num5 = 1f;
				if (ADesc != null)
				{
					num5 = ADesc.power_multiply;
					recipeDish.addCostInCreating(ADesc.cost_add);
				}
				while (i < count)
				{
					RecipeManager.MakeDishIngDescription makeDishIngDescription;
					if (ADesc != null && num4 < ADesc.Count)
					{
						makeDishIngDescription = ADesc[num4];
					}
					else
					{
						makeDishIngDescription = default(RecipeManager.MakeDishIngDescription);
					}
					if (num3 < this.Aor_splitter.Length && i >= this.Aor_splitter[num3])
					{
						int num6 = ((++num3 >= this.Aor_splitter.Length) ? count : this.Aor_splitter[num3]);
						bool flag = false;
						while (i < num6)
						{
							if (this.AIng[i].createDish(recipeDish, (AA.Count > i) ? AA[i] : null, num5, ref num, ref num2, abort_if_missing, makeDishIngDescription))
							{
								flag = true;
							}
							i++;
						}
						if (!flag && abort_if_missing)
						{
							return null;
						}
					}
					else
					{
						if (!this.AIng[i].createDish(recipeDish, (AA.Count > i) ? AA[i] : null, num5, ref num, ref num2, abort_if_missing, makeDishIngDescription) && abort_if_missing)
						{
							return null;
						}
						i++;
					}
					num4++;
				}
				recipeDish.finalizeDish();
				recipeDish.calced_grade = ((num == 0) ? 0 : global::XX.X.MMX(0, global::XX.X.IntR((float)num2 / (float)num), 4));
				return recipeDish;
			}

			public bool isCreatable(List<List<UiCraftBase.IngEntryRow>> AA)
			{
				int count = this.AIng.Count;
				int i = 0;
				if (count == 0)
				{
					return false;
				}
				int num = 0;
				while (i < count)
				{
					if (num < this.Aor_splitter.Length && i >= this.Aor_splitter[num])
					{
						int num2 = ((++num >= this.Aor_splitter.Length) ? count : this.Aor_splitter[num]);
						bool flag = false;
						while (i < num2)
						{
							if (this.AIng[i].isCreatable(AA[i]))
							{
								flag = true;
								i = num2;
								break;
							}
							i++;
						}
						if (!flag)
						{
							return false;
						}
					}
					else
					{
						if (!this.AIng[i].isCreatable(AA[i]))
						{
							return false;
						}
						i++;
					}
				}
				return true;
			}

			public void touchObtainCountAllIngredients()
			{
				int count = this.AIng.Count;
				if (count == 0)
				{
					return;
				}
				for (int i = 0; i < count; i++)
				{
					this.AIng[i].touchObtainCountIngredient();
				}
				if (this.Completion != null)
				{
					this.Completion.touchObtainCount();
				}
			}

			public string title
			{
				get
				{
					if (this.Completion != null)
					{
						return this.Completion.getLocalizedName(0, null);
					}
					return TX.Get("Recipe_title_" + this.key, "");
				}
			}

			public string descript
			{
				get
				{
					return TX.Get("Recipe_desc_" + this.key, "");
				}
			}

			public NelItem RecipeItem
			{
				get
				{
					return NelItem.GetById("Recipe_" + this.key, false);
				}
			}

			public int get_icon_id()
			{
				if (this.Completion != null)
				{
					return this.Completion.getIcon(null, null);
				}
				if (this.categ != RecipeManager.RP_CATEG.COOK)
				{
					return 14;
				}
				if (!this.is_water)
				{
					return 7;
				}
				return 6;
			}

			public bool isUseable()
			{
				return this.useable >= 1;
			}

			public List<List<UiCraftBase.IngEntryRow>> getPrevList()
			{
				return this.AAPrevIngredient;
			}

			public void setPrevList(List<List<UiCraftBase.IngEntryRow>> L)
			{
				this.AAPrevIngredient = L;
			}

			public static void writeBinaryTo(RecipeManager.Recipe Target, ByteArray Ba)
			{
				Ba.writeUInt(Target.created);
				Ba.writeByte((int)Target.eaten);
				RecipeManager.Recipe.writeBinaryTo(Target.AAPrevIngredient, Ba);
			}

			public static void writeBinaryTo(List<List<UiCraftBase.IngEntryRow>> AA, ByteArray Ba)
			{
				if (AA == null)
				{
					Ba.writeByte(0);
					return;
				}
				int count = AA.Count;
				Ba.writeByte(count);
				for (int i = 0; i < count; i++)
				{
					List<UiCraftBase.IngEntryRow> list = AA[i];
					int count2 = list.Count;
					Ba.writeByte(count2);
					for (int j = 0; j < count2; j++)
					{
						UiCraftBase.IngEntryRow ingEntryRow = list[j];
						List<List<UiCraftBase.IngEntryRow>> innerIngredient = ingEntryRow.getInnerIngredient();
						if (innerIngredient == null || innerIngredient.Count == 0)
						{
							Ba.writeByte(0);
							Ba.writeByte(ingEntryRow.grade);
							Ba.writePascalString(ingEntryRow.Itm.key, "utf-8");
						}
						else
						{
							RecipeManager.Recipe.writeBinaryTo(innerIngredient, Ba);
						}
					}
				}
			}

			public static void readBinaryFrom(RecipeManager.Recipe Target, ByteArray Ba, bool read_recipe_eaten)
			{
				uint num = Ba.readUInt();
				byte b = 0;
				if (read_recipe_eaten)
				{
					b = Ba.readUByte();
				}
				bool flag = false;
				List<List<UiCraftBase.IngEntryRow>> list = RecipeManager.Recipe.readBinaryIngredientRows(Ba, Target, ref flag, null);
				if (Target != null)
				{
					Target.created = num;
					Target.eaten = b;
					Target.AAPrevIngredient = (flag ? null : list);
				}
			}

			public static List<List<UiCraftBase.IngEntryRow>> readBinaryIngredientRows(ByteArray Ba, RecipeManager.Recipe TargetRcp, ref bool fail_flag, UiCraftBase.IngEntryRow Parent = null)
			{
				int num = Ba.readByte();
				if (num == 0)
				{
					return null;
				}
				List<List<UiCraftBase.IngEntryRow>> list = new List<List<UiCraftBase.IngEntryRow>>(num);
				UiCraftBase.IngEntryRow ingEntryRow = null;
				for (int i = 0; i < num; i++)
				{
					int num2 = Ba.readByte();
					List<UiCraftBase.IngEntryRow> list2 = new List<UiCraftBase.IngEntryRow>(num2);
					RecipeManager.RecipeIngredient recipeIngredient = ((TargetRcp != null && i < TargetRcp.AIng.Count) ? TargetRcp.AIng[i] : null);
					if (fail_flag || (TargetRcp != null && TargetRcp.AIng.Count != num))
					{
						fail_flag = true;
					}
					list.Add(list2);
					for (int j = 0; j < num2; j++)
					{
						if (ingEntryRow == null)
						{
							ingEntryRow = new UiCraftBase.IngEntryRow(Parent);
						}
						List<List<UiCraftBase.IngEntryRow>> list3 = RecipeManager.Recipe.readBinaryIngredientRows(Ba, (recipeIngredient != null) ? recipeIngredient.TargetRecipe : null, ref fail_flag, ingEntryRow);
						if (list3 == null)
						{
							int num3 = Ba.readByte();
							string text = Ba.readPascalString("utf-8", false);
							NelItem nelItem = (TX.isStart(text, "__dish_", 0) ? null : NelItem.GetById(text, false));
							if (nelItem != null && recipeIngredient != null)
							{
								list2.Add(new UiCraftBase.IngEntryRow(Parent, recipeIngredient, nelItem, num3)
								{
									index = list2.Count
								});
							}
						}
						else
						{
							ingEntryRow.setInnerIngredientFromBinary(list3);
							ingEntryRow.index = list2.Count;
							list2.Add(ingEntryRow);
							ingEntryRow = null;
						}
					}
				}
				return list;
			}

			public readonly string key;

			public RecipeManager.RP_CATEG categ;

			public NelItem Completion;

			public int cost = 1;

			public bool is_water;

			public int after_price = 20;

			public int[] Aor_splitter;

			public RecipeManager.RecipeFinNameRepl[] AFinNameRepl;

			public int create_count = 1;

			public bool debug_recipe;

			public UiCraftBase.AutoCreationInfo CInfo = new UiCraftBase.AutoCreationInfo();

			public List<RecipeManager.RecipeIngredient> AIng;

			private byte useable = 2;

			private List<List<UiCraftBase.IngEntryRow>> AAPrevIngredient;

			public uint created;

			public byte eaten;
		}

		public struct RecipeDescription
		{
			public RecipeDescription(RecipeManager.Recipe _Rcp, NelItem _Item, string _title = null)
			{
				this.Rcp = _Rcp;
				this.Item = _Item ?? this.Rcp.RecipeItem;
				this.title = ((_title == null) ? this.Rcp.title : _title);
			}

			public RecipeManager.Recipe Rcp;

			public NelItem Item;

			public string title;
		}

		public class RecipeDish
		{
			public RecipeDish(RecipeManager.RecipeDish Src = null, int _reffered = 0)
			{
				if (Src != null)
				{
					this.Rcp = Src.Rcp;
					this.OEffect = Src.OEffect;
					this.OUseIngredient = Src.OUseIngredient;
					this.OUseIngredientSource = Src.OUseIngredientSource;
					this.cost_ = Src.cost_;
					this.calced_grade = Src.calced_grade;
					this.title_keys = Src.title_keys;
				}
				this.referred = _reffered;
			}

			public bool isSame(RecipeManager.RecipeDish T)
			{
				if (T.Rcp != this.Rcp || T.calced_grade != this.calced_grade || T.price != this.price || T.cost_ != this.cost_ || T.OUseIngredient.Count != this.OUseIngredient.Count || T.OUseIngredientSource.Count != this.OUseIngredientSource.Count || T.OEffect.Count != this.OEffect.Count)
				{
					return false;
				}
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.OUseIngredient)
				{
					ItemStorage.ObtainInfo obtainInfo;
					if (!T.OUseIngredient.TryGetValue(keyValuePair.Key, out obtainInfo) || !keyValuePair.Value.isSame(obtainInfo))
					{
						return false;
					}
				}
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair2 in this.OUseIngredientSource)
				{
					ItemStorage.ObtainInfo obtainInfo2;
					if (!T.OUseIngredientSource.TryGetValue(keyValuePair2.Key, out obtainInfo2) || !keyValuePair2.Value.isSame(obtainInfo2))
					{
						return false;
					}
				}
				foreach (KeyValuePair<RecipeManager.RPI_EFFECT, Vector2> keyValuePair3 in this.OEffect)
				{
					Vector2 vector;
					if (!T.OEffect.TryGetValue(keyValuePair3.Key, out vector) || !keyValuePair3.Value.Equals(vector))
					{
						return false;
					}
				}
				return true;
			}

			public RecipeManager.RecipeDish Create(RecipeManager.Recipe _Rcp)
			{
				this.Rcp = _Rcp;
				this.OEffect.Clear();
				this.title_keys = (this.title_localized = null);
				if (this.OUseIngredient == null)
				{
					this.OUseIngredient = new BDic<NelItem, ItemStorage.ObtainInfo>();
				}
				else
				{
					this.OUseIngredient.Clear();
				}
				this.OUseIngredientSource.Clear();
				this.cost_ = 0;
				this.price = 10;
				return this;
			}

			public RecipeManager.RecipeDish readBinaryContent(ByteArray Ba, int vers)
			{
				bool flag = vers >= 1;
				this.Rcp = RecipeManager.Get(Ba.readPascalString("utf-8", false));
				this.Create(this.Rcp);
				this.cost_ = (int)Ba.readUShort();
				this.calced_grade = Ba.readByte();
				this.title_keys = Ba.readString("utf-8", false);
				ushort num = Ba.readUShort();
				for (int i = 0; i < (int)num; i++)
				{
					NelItem byId = NelItem.GetById(Ba.readPascalString("utf-8", false), false);
					ItemStorage.ObtainInfo obtainInfo = new ItemStorage.ObtainInfo().readBinaryFrom(Ba);
					if (byId != null)
					{
						this.OUseIngredientSource[byId] = obtainInfo;
					}
				}
				num = Ba.readUShort();
				for (int j = 0; j < (int)num; j++)
				{
					int num2 = (int)Ba.readUShort();
					float num3 = Ba.readFloat();
					int num4 = 1;
					if (flag)
					{
						num4 = Ba.readByte();
					}
					this.OEffect[(RecipeManager.RPI_EFFECT)num2] = new Vector2(global::XX.X.MMX(-1f, num3, 1f), (float)num4);
				}
				if (vers >= 2)
				{
					this.price = Ba.readInt();
				}
				else
				{
					this.price = 10;
				}
				return this;
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writePascalString((this.Rcp != null) ? this.Rcp.key : "", "utf-8");
				Ba.writeUShort((ushort)this.cost_);
				Ba.writeByte((int)((byte)this.calced_grade));
				Ba.writeString(this.title_keys, "utf-8");
				Ba.writeUShort((ushort)this.OUseIngredientSource.Count);
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.OUseIngredientSource)
				{
					Ba.writePascalString(keyValuePair.Key.key, "utf-8");
					keyValuePair.Value.writeBinaryTo(Ba);
				}
				Ba.writeUShort((ushort)this.OEffect.Count);
				foreach (KeyValuePair<RecipeManager.RPI_EFFECT, Vector2> keyValuePair2 in this.OEffect)
				{
					Ba.writeUShort((ushort)keyValuePair2.Key);
					Ba.writeFloat(keyValuePair2.Value.x);
					Ba.writeByte((int)((byte)global::XX.X.MMX(0f, keyValuePair2.Value.y, 255f)));
				}
				Ba.writeInt(this.price);
			}

			public RecipeManager.RecipeDish addEffect100(BDic<RecipeManager.RPI_EFFECT, float> Oef, float lvl01)
			{
				foreach (KeyValuePair<RecipeManager.RPI_EFFECT, float> keyValuePair in Oef)
				{
					if (keyValuePair.Key != RecipeManager.RPI_EFFECT.NONE)
					{
						float num = lvl01 * keyValuePair.Value / 100f;
						Vector2 vector;
						if (this.OEffect.TryGetValue(keyValuePair.Key, out vector))
						{
							vector.x += num;
							vector.y += 1f;
							this.OEffect[keyValuePair.Key] = vector;
						}
						else
						{
							this.OEffect[keyValuePair.Key] = new Vector2(num, 1f);
						}
					}
				}
				return this;
			}

			public RecipeManager.RecipeDish addEffect100(BDic<RecipeManager.RPI_EFFECT, Vector2> Oef, float lvl01)
			{
				foreach (KeyValuePair<RecipeManager.RPI_EFFECT, Vector2> keyValuePair in Oef)
				{
					if (keyValuePair.Key != RecipeManager.RPI_EFFECT.NONE)
					{
						float num = lvl01 * keyValuePair.Value.x / 100f;
						Vector2 vector;
						if (this.OEffect.TryGetValue(keyValuePair.Key, out vector))
						{
							vector.x += num;
							vector.y += keyValuePair.Value.y;
							this.OEffect[keyValuePair.Key] = vector;
						}
						else
						{
							this.OEffect[keyValuePair.Key] = new Vector2(num, keyValuePair.Value.y);
						}
					}
				}
				return this;
			}

			public RecipeManager.RecipeDish addEffect(RecipeManager.RecipeDish Dh, float lvl01)
			{
				return this.addEffect100(Dh.OEffect, lvl01 * 100f);
			}

			public int cost
			{
				get
				{
					return this.cost_;
				}
			}

			public void addCostInCreating(int c)
			{
				this.cost_ += c;
			}

			public RecipeManager.RecipeDish finalizeDish()
			{
				this.cost_ = global::XX.X.Mx(1, this.cost_);
				return this;
			}

			public RecipeManager.RecipeDish calcPrice(List<List<UiCraftBase.IngEntryRow>> AACook)
			{
				float num = 0f;
				for (int i = AACook.Count - 1; i >= 0; i--)
				{
					this.calcPrice(AACook[i], ref num);
				}
				this.price = global::XX.X.IntC(num * 3.5f);
				return this;
			}

			private void calcPrice(List<UiCraftBase.IngEntryRow> ACook, ref float price)
			{
				if (ACook == null)
				{
					return;
				}
				for (int i = ACook.Count - 1; i >= 0; i--)
				{
					UiCraftBase.IngEntryRow ingEntryRow = ACook[i];
					if (ingEntryRow == null)
					{
						return;
					}
					List<List<UiCraftBase.IngEntryRow>> innerIngredient = ingEntryRow.getInnerIngredient();
					if (innerIngredient != null)
					{
						for (int j = innerIngredient.Count - 1; j >= 0; j--)
						{
							this.calcPrice(innerIngredient[j], ref price);
						}
					}
					else
					{
						price += ingEntryRow.Itm.getPrice(ingEntryRow.grade);
					}
				}
			}

			public RecipeManager.RecipeDish finalizeDishEffect()
			{
				Vector2 vector;
				if (this.OEffect.TryGetValue(RecipeManager.RPI_EFFECT.RANDOM, out vector) && vector.y >= 1f)
				{
					int num = (int)vector.y;
					int num2 = 26;
					this.OEffect.Remove(RecipeManager.RPI_EFFECT.RANDOM);
					float num3 = vector.x / (float)num;
					while (--num >= 0)
					{
						RecipeManager.RPI_EFFECT rpi_EFFECT = RecipeManager.RPI_EFFECT.MAXHP + global::XX.X.xors(25);
						for (int i = 0; i <= num2; i++)
						{
							if (rpi_EFFECT >= RecipeManager.RPI_EFFECT.RANDOM)
							{
								rpi_EFFECT = RecipeManager.RPI_EFFECT.MAXHP;
							}
							while (rpi_EFFECT <= RecipeManager.RPI_EFFECT.NONE)
							{
								rpi_EFFECT++;
							}
							if (!this.OEffect.ContainsKey(rpi_EFFECT))
							{
								this.OEffect[rpi_EFFECT] = new Vector2(num3, 1f);
								break;
							}
							rpi_EFFECT++;
						}
					}
				}
				Vector2 vector2;
				if (this.OEffect.TryGetValue(RecipeManager.RPI_EFFECT.DISH_VARIABLE, out vector2))
				{
					float num4 = vector2.x + 1f;
					this.OEffect.Remove(RecipeManager.RPI_EFFECT.DISH_VARIABLE);
					foreach (KeyValuePair<RecipeManager.RPI_EFFECT, Vector2> keyValuePair in new BDic<RecipeManager.RPI_EFFECT, Vector2>(this.OEffect))
					{
						Vector2 value = keyValuePair.Value;
						value.x *= num4;
						this.OEffect[keyValuePair.Key] = value;
					}
				}
				for (int j = 0; j < 28; j++)
				{
					Vector2 vector3;
					if (this.OEffect.TryGetValue((RecipeManager.RPI_EFFECT)j, out vector3))
					{
						vector3.x = global::XX.X.MMX(-1f, vector3.x, 1f);
						this.OEffect[(RecipeManager.RPI_EFFECT)j] = vector3;
					}
				}
				return this;
			}

			public void fineTitle(List<List<UiCraftBase.IngEntryRow>> AACook, bool force = true, RecipeManager.MakeDishDescription ADesc = null)
			{
				if (this.Rcp.Completion == null && (this.title_keys == null || force))
				{
					this.title_localized = null;
					int num = this.Rcp.AIng.Count;
					using (BList<string> blist = ListBuffer<string>.Pop(num + 2))
					{
						List<List<RecipeManager.RecipeIngredient>> list = new List<List<RecipeManager.RecipeIngredient>>(num);
						int num2 = 0;
						for (int i = 0; i < num; i++)
						{
							RecipeManager.RecipeIngredient recipeIngredient = this.Rcp.AIng[i];
							if (recipeIngredient.name_id != -1)
							{
								if (recipeIngredient.name_id <= -2)
								{
									recipeIngredient.name_id = num2++;
								}
								while (list.Count <= recipeIngredient.name_id)
								{
									list.Add(new List<RecipeManager.RecipeIngredient>(1));
								}
								list[recipeIngredient.name_id].Add(recipeIngredient);
							}
						}
						num = list.Count;
						int num3 = 0;
						for (int j = 0; j < num; j++)
						{
							List<RecipeManager.RecipeIngredient> list2 = list[j];
							if (list2.Count != 0)
							{
								int index = list2[0].index;
								bool flag = true;
								RecipeManager.RecipeFinNameRepl recipeFinNameRepl;
								if (RecipeManager.RecipeDish.getRecipeNameReplace(this.Rcp, index, out recipeFinNameRepl, AACook[index], true, blist))
								{
									flag = false;
								}
								if (RecipeManager.RecipeDish.getRecipeNameReplace(this.Rcp, index, out recipeFinNameRepl, AACook[index], false, blist))
								{
									flag = false;
								}
								if (flag)
								{
									using (BList<string> blist2 = ListBuffer<string>.Pop(list2.Count))
									{
										RecipeManager.RecipeDish.NameCheck nameCheck = RecipeManager.RecipeDish.checkManyKind(list2, AACook, blist2, this.calced_grade, ref num3, ADesc);
										if (nameCheck.item_count == 0)
										{
											goto IL_026F;
										}
										if (nameCheck.tx_listup)
										{
											int item_count = nameCheck.item_count;
											for (int k = 0; k < item_count; k++)
											{
												blist.Add(nameCheck.Aitem_keys[k]);
											}
										}
										else if (nameCheck.item_count >= 3)
										{
											if (nameCheck.categ_count >= 2)
											{
												blist.Add("recipe_adj_manykind");
											}
											else
											{
												blist.Add("recipe_adj_manykind_specific|recipe_item_categ_" + nameCheck.categ.ToString().ToLower());
											}
										}
										else if (nameCheck.item_count == 2)
										{
											blist.Add("recipe_double_item|" + nameCheck.Aitem_keys[0] + "|" + nameCheck.Aitem_keys[1]);
										}
										else
										{
											blist.Add(nameCheck.Aitem_keys[0]);
										}
										if (nameCheck.enough_bits != 0)
										{
											blist.Add(">recipe_adj_enough_specific");
										}
									}
								}
								blist.Add("+");
							}
							IL_026F:;
						}
						if (num3 <= 2 && (ADesc == null || !ADesc.tx_no_grade))
						{
							blist.Add("+>recipe_adj_grade" + this.calced_grade.ToString());
						}
						this.title_keys = TX.join<string>("\n", blist.ToArray(), 0, -1);
					}
				}
			}

			private static RecipeManager.RecipeDish.NameCheck checkManyKind(List<RecipeManager.RecipeIngredient> AIng, List<List<UiCraftBase.IngEntryRow>> AACook, List<string> Aitem_keys, int calced_grade, ref int grade_another_count, RecipeManager.MakeDishDescription ADesc = null)
			{
				int count = AIng.Count;
				RecipeManager.RecipeDish.NameCheck nameCheck = default(RecipeManager.RecipeDish.NameCheck);
				nameCheck.Aitem_keys = Aitem_keys;
				int i = 0;
				while (i < count)
				{
					RecipeManager.RecipeIngredient recipeIngredient = AIng[i];
					List<UiCraftBase.IngEntryRow> list = AACook[recipeIngredient.index];
					RecipeManager.MakeDishIngDescription makeDishIngDescription = ((ADesc != null && ADesc.Count > recipeIngredient.index) ? ADesc[recipeIngredient.index] : default(RecipeManager.MakeDishIngDescription));
					if (!makeDishIngDescription.valid)
					{
						goto IL_0088;
					}
					if (makeDishIngDescription.tx_listup)
					{
						nameCheck.tx_listup = true;
					}
					if (!makeDishIngDescription.tx_ignore)
					{
						goto IL_0088;
					}
					IL_024D:
					i++;
					continue;
					IL_0088:
					if (list == null)
					{
						goto IL_024D;
					}
					int count2 = list.Count;
					for (int j = 0; j < count2; j++)
					{
						UiCraftBase.IngEntryRow ingEntryRow = list[j];
						if (ingEntryRow.Itm == null)
						{
							if (recipeIngredient.TargetRecipe != null)
							{
								Aitem_keys.Add("%RECIPE_" + recipeIngredient.TargetRecipe.key);
							}
						}
						else if (ingEntryRow.Itm.RecipeInfo != null)
						{
							string text = "%ITEM_" + ingEntryRow.Itm.key;
							if (ingEntryRow.Itm.is_food && ingEntryRow.Itm.RecipeInfo != null && ingEntryRow.Itm.RecipeInfo.DishInfo != null)
							{
								text = string.Concat(new string[]
								{
									"%FOOD:",
									ingEntryRow.Itm.RecipeInfo.DishInfo.Rcp.key,
									"<<",
									ingEntryRow.Itm.RecipeInfo.DishInfo.getTitleKeysEncoded(true),
									">>"
								});
							}
							if (Aitem_keys.IndexOf(text) == -1)
							{
								Aitem_keys.Add(text);
							}
							if (grade_another_count < 4)
							{
								grade_another_count += global::XX.X.Abs(ingEntryRow.grade - calced_grade);
							}
							if ((nameCheck.categ & ingEntryRow.Itm.RecipeInfo.categ) == (RecipeManager.RPI_CATEG)0)
							{
								nameCheck.categ |= ingEntryRow.Itm.RecipeInfo.categ;
								nameCheck.categ_count++;
							}
						}
					}
					if (list.Count > recipeIngredient.need && !recipeIngredient.allloc_over_quantity && (!makeDishIngDescription.valid || !makeDishIngDescription.tx_enough_ignore))
					{
						nameCheck.enough_bits |= 1 << i;
						goto IL_024D;
					}
					goto IL_024D;
				}
				return nameCheck;
			}

			public string getTitleKeysEncoded(bool without_grade = false)
			{
				string leftContext = this.title_keys;
				if (TX.noe(leftContext))
				{
					return "";
				}
				if (without_grade && REG.match(leftContext, RecipeManager.RecipeDish.RegGradeSuffix))
				{
					leftContext = REG.leftContext;
				}
				return TX.escape(leftContext);
			}

			public string title
			{
				get
				{
					if (this.title_keys != null)
					{
						if (this.title_localized == null)
						{
							this.title_localized = RecipeManager.RecipeDish.parseTitleKeys(this.Rcp, "%RECIPE_" + this.Rcp.key, this.title_keys, "\n");
						}
						return this.title_localized;
					}
					return this.Rcp.title;
				}
			}

			private static string parseTitleKeys(RecipeManager.Recipe Rcp, string recipe_title, string _title_keys, string splitter = "\n")
			{
				if (_title_keys == null)
				{
					return "";
				}
				string text2;
				using (STB stb = TX.PopBld(null, 0))
				{
					using (BList<string> blist = ListBuffer<string>.Pop(0))
					{
						Action<STB, STB, STB> action = delegate(STB Buf, STB Pre, STB Temp)
						{
							if (Buf.Length == 0)
							{
								return;
							}
							if (Pre.Length == 0)
							{
								Pre.Set(Buf);
							}
							else
							{
								Temp.Clear().AddTxA("__adding", false).TxRpl(Pre)
									.TxRpl(Buf);
								Pre.Set(Temp);
							}
							Pre.Add("\b");
							Buf.Clear();
						};
						using (STB stb2 = TX.PopBld(null, 0))
						{
							using (STB stb3 = TX.PopBld(null, 0))
							{
								string[] array = TX.split(_title_keys, splitter);
								int num = array.Length;
								for (int i = 0; i < num; i++)
								{
									string text = array[i];
									if (TX.isStart(text, "+>", 0))
									{
										action(stb2, stb, stb3);
										stb3.Clear().AddTxA(TX.slice(text, 2), false).TxRpl(stb);
										stb.Set(stb3);
									}
									else if (TX.isStart(text, ">", 0))
									{
										stb3.Clear().AddTxA(TX.slice(text, 1), false).TxRpl(stb2);
										stb2.Set(stb3);
									}
									else if (TX.isStart(text, "--", 0))
									{
										blist.Add(TX.slice(text, 2));
									}
									else if (text == "+")
									{
										action(stb2, stb, stb3);
									}
									else
									{
										string[] array2 = TX.split(text, "|");
										using (STB stb4 = TX.PopBld(null, 0))
										{
											if (array2.Length >= 2)
											{
												BList<string> blist2 = null;
												if (array2[0] == "recipe_double_item" && array2.Length == 3)
												{
													blist2 = ListBuffer<string>.Pop(2);
													blist2.Add(RecipeManager.RecipeDish.txGet(array2[1], blist));
													blist2.Add(RecipeManager.RecipeDish.txGet(array2[2], blist));
													if (blist2[0] == "" && blist2[1] == "")
													{
														blist2.Dispose();
														goto IL_02BE;
													}
													if (blist2[0] == "")
													{
														action(stb4.Set(blist2[1]), stb2, stb3);
														blist2.Dispose();
														goto IL_02BE;
													}
													if (blist2[1] == "")
													{
														action(stb4.Set(blist2[0]), stb2, stb3);
														blist2.Dispose();
														goto IL_02BE;
													}
												}
												stb4.AddTxA(array2[0], false);
												int num2 = array2.Length;
												for (int j = 1; j < num2; j++)
												{
													stb4.TxRpl((blist2 != null && blist2.Count > j - 1) ? blist2[j - 1] : RecipeManager.RecipeDish.txGet(array2[j], blist));
												}
												if (blist2 != null)
												{
													blist2.Dispose();
												}
											}
											else
											{
												stb4.Set(RecipeManager.RecipeDish.txGet(array2[0], blist));
											}
											action(stb4, stb2, stb3);
										}
									}
									IL_02BE:;
								}
								action(stb2, stb, stb3);
								RecipeManager.RecipeFinNameRepl recipeFinNameRepl;
								if (RecipeManager.RecipeDish.getRecipeNameReplace(Rcp, -1, out recipeFinNameRepl, blist))
								{
									stb2.Set(RecipeManager.RecipeDish.txGet(recipeFinNameRepl.tx_key_target, blist));
								}
								else
								{
									stb2.Set(RecipeManager.RecipeDish.txGet(recipe_title, blist));
								}
								action(stb2, stb, stb3);
							}
						}
					}
					text2 = stb.ToString();
				}
				return text2;
			}

			private static bool getRecipeNameReplace(RecipeManager.Recipe Rcp, int index, out RecipeManager.RecipeFinNameRepl Repl, List<UiCraftBase.IngEntryRow> ACook, bool for_src = false, List<string> Atitle_key = null)
			{
				Repl = default(RecipeManager.RecipeFinNameRepl);
				if (Rcp == null || Rcp.AFinNameRepl == null)
				{
					return false;
				}
				int num = Rcp.AFinNameRepl.Length;
				for (int i = 0; i < num; i++)
				{
					RecipeManager.RecipeFinNameRepl recipeFinNameRepl = Rcp.AFinNameRepl[i];
					if ((for_src ? recipeFinNameRepl.src_index : recipeFinNameRepl.target_index) == index)
					{
						for (int j = ACook.Count - 1; j >= 0; j--)
						{
							if (ACook[j].Itm.key == recipeFinNameRepl.trigger_item_key)
							{
								Repl = recipeFinNameRepl;
								if (Atitle_key != null)
								{
									Atitle_key.Add("+");
									for (int k = ACook.Count - 1; k >= 0; k--)
									{
										string key = ACook[k].Itm.key;
										if (k != j && !(key == recipeFinNameRepl.trigger_item_key))
										{
											Atitle_key.Add("%ITEM_" + key);
										}
									}
									Atitle_key.Add("--" + recipeFinNameRepl.trigger_item_key);
									Atitle_key.Add(Repl.tx_key_src);
								}
								return true;
							}
						}
					}
				}
				return false;
			}

			private static bool getRecipeNameReplace(RecipeManager.Recipe Rcp, int index, out RecipeManager.RecipeFinNameRepl Repl, List<string> Aitm_used)
			{
				Repl = default(RecipeManager.RecipeFinNameRepl);
				if (Rcp == null || Rcp.AFinNameRepl == null)
				{
					return false;
				}
				int num = Rcp.AFinNameRepl.Length;
				for (int i = 0; i < num; i++)
				{
					RecipeManager.RecipeFinNameRepl recipeFinNameRepl = Rcp.AFinNameRepl[i];
					if (recipeFinNameRepl.target_index == index)
					{
						for (int j = Aitm_used.Count - 1; j >= 0; j--)
						{
							if (Aitm_used[j] == recipeFinNameRepl.trigger_item_key)
							{
								Repl = recipeFinNameRepl;
								return true;
							}
						}
					}
				}
				return false;
			}

			private static string txGet(string k, List<string> Aitm_used)
			{
				if (k.IndexOf("%ITEM_") == 0)
				{
					string text = TX.slice(k, "%ITEM_".Length);
					Aitm_used.Add(text);
					TX tx = TX.getCurrentFamily().Get("_NelItem_nameR_" + text);
					if (tx != null)
					{
						return tx.text;
					}
					return TX.Get("_NelItem_name_" + text, "");
				}
				else
				{
					if (REG.match(k, RecipeManager.RecipeDish.RegFoodInserted))
					{
						RecipeManager.Recipe recipe = RecipeManager.Get(REG.R1);
						string text2 = TX.unescape(REG.R2);
						return RecipeManager.RecipeDish.parseTitleKeys(recipe, (recipe != null) ? ("%RECIPE_" + recipe.key) : "Food_unknown", text2, "\n");
					}
					if (k.IndexOf("%RECIPE_") == 0)
					{
						string text3 = TX.slice(k, "%RECIPE_".Length);
						Aitm_used.Add(text3);
						TX tx2 = TX.getCurrentFamily().Get("Recipe_titleR_" + text3);
						if (tx2 != null)
						{
							return tx2.text;
						}
						return TX.Get("Recipe_title_" + text3, "");
					}
					else
					{
						if (!(k == "_"))
						{
							return TX.Get(k, "");
						}
						return "";
					}
				}
			}

			public void flushLocalizeTitle()
			{
				this.title_localized = null;
			}

			public string descript
			{
				get
				{
					return this.Rcp.descript;
				}
			}

			public bool valid
			{
				get
				{
					return this.Rcp != null;
				}
			}

			public uint recipe_id
			{
				get
				{
					if (this.ItemData != null)
					{
						return global::XX.X.NmUI(TX.slice(this.ItemData.key, "__dish_".Length), 0U, false, false);
					}
					return uint.MaxValue;
				}
			}

			public RecipeManager.Recipe Rcp;

			public NelItem ItemData;

			private int cost_;

			public int referred;

			public int calced_grade;

			public int price = 10;

			public const int PRICE_DEFAULT = 10;

			public const float PRODUCT_BUY_RATIO = 3.5f;

			private string title_keys;

			private string title_localized;

			public BDic<NelItem, ItemStorage.ObtainInfo> OUseIngredient;

			public BDic<NelItem, ItemStorage.ObtainInfo> OUseIngredientSource = new BDic<NelItem, ItemStorage.ObtainInfo>();

			public BDic<RecipeManager.RPI_EFFECT, Vector2> OEffect = new BDic<RecipeManager.RPI_EFFECT, Vector2>();

			private static Regex RegFoodInserted = new Regex("^\\%FOOD\\:(\\w+)<<([^>]+)>>$");

			private static Regex RegGradeSuffix = new Regex("\\n?\\+>recipe_adj_grade\\d$");

			private struct NameCheck
			{
				public int item_count
				{
					get
					{
						return this.Aitem_keys.Count;
					}
				}

				public RecipeManager.RPI_CATEG categ;

				public int categ_count;

				public bool tx_listup;

				public bool tx_ignore;

				public int enough_bits;

				public List<string> Aitem_keys;
			}
		}
	}
}

using System;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class ButtonSkinNelFieldGuide : ButtonSkinNelUi
	{
		public ButtonSkinNelFieldGuide(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.alignx_ = ALIGN.LEFT;
			this.auto_fix_max_swidth = false;
			this.MdR = base.makeMesh(null);
			this.nncolor.push_color = uint.MaxValue;
			if (this.TxR == null)
			{
				this.TxR = base.MakeTx("-text_r");
				this.TxR.html_mode = true;
				this.TxR.size = 13f;
				this.TxR.alignx = ALIGN.CENTER;
				this.TxR.aligny = ALIGNY.MIDDLE;
				if (X.ENG_MODE)
				{
					this.TxR.size = 12f;
					this.TxR.max_swidth_px = 70f;
					this.TxR.auto_condense = true;
					this.TxR.letter_spacing = 0.91f;
				}
				IN.setZ(this.TxR.transform, -0.01f);
			}
			if (this.B.Container != null)
			{
				this.TxR.StencilRef(this.B.Container.stencil_ref);
			}
			this.Md.chooseSubMesh(1, false, false);
			this.Md.setMaterial(MTRX.MIicon.getMtr(base.container_stencil_ref), false);
			this.Md.connectRendererToTriMulti(base.getMeshRenderer(this.Md));
			this.Md.chooseSubMesh(0, false, false);
		}

		public void clearVisual(UiFieldGuide _FDCon)
		{
			this.FDCon = _FDCon;
			this.IR = null;
			this.PF = null;
			this.Store = null;
			this.TargetMp = null;
			this.TargetWM = null;
			this.FSmn = null;
			this.fine_flag = true;
			this.text_alpha = 1f;
			this.RecipeIng = null;
			this.row_left_px = 44;
			this.Tx.size = 16f;
			this.Tx.auto_condense = true;
			this.Tx.auto_wrap = true;
			this.Tx.max_swidth_px = this.w * 64f - (float)this.row_left_px - 30f;
			this.Tx.line_spacing = 0.93f;
			this.Md.clear(false, false);
			this.rpi_ef = RecipeManager.RPI_EFFECT.NONE;
			this.TxR.enabled = false;
			this.Itm = null;
			this.MdR.clear(false, false);
		}

		private void initLeftDummyButton(string t)
		{
			if (this.TxR.text_content != t || !this.TxR.enabled)
			{
				this.TxR.text_content = t;
				this.TxR.enabled = true;
				float num = this.w * 64f * 0.5f - 18f - this.TxR.get_swidth_px() * 0.5f;
				IN.PosP2(this.TxR.transform, num - 10f, 0f);
				this.MdR.base_px_x = num - 15f;
				this.Tx.max_swidth_px -= this.TxR.get_swidth_px() + 25f;
				this.Tx.redraw_flag = true;
				this.fine_flag = true;
			}
		}

		protected override void setTitleText(string str)
		{
			if (this.Tx != null)
			{
				this.Tx.text_content = str;
				return;
			}
			base.setTitleText(str);
		}

		public void initTreasure(ReelManager.ItemReelContainer _IR, bool ignore_obtain_count, NelItem Src)
		{
			this.IR = _IR;
			this.Itm = this.IR.GReelItem;
			this.row_left_px += 25;
			base.prepareIconMesh();
			using (STB stb = TX.PopBld(null, 0))
			{
				bool flag = this.FDCon.isRowAssigned(this.Itm);
				if (!ignore_obtain_count && !flag)
				{
					this.Itm = NelItem.Unknown;
					stb.Set(NelItem.Unknown.getLocalizedName(0, null));
				}
				else
				{
					stb.Set(TX.Get(this.IR.tx_key, ""));
					if (flag)
					{
						this.initLeftDummyButton(TX.Get("catalog_jump", ""));
					}
					if (Src != null)
					{
						int num = -1000;
						int num2 = 0;
						int num3 = -1;
						int count = this.IR.Count;
						for (int i = 0; i < count; i++)
						{
							NelItemEntry nelItemEntry = this.IR[i];
							if (nelItemEntry.Data == Src)
							{
								if (num == -1000)
								{
									num = nelItemEntry.count;
								}
								else
								{
									num = X.Mn(num, nelItemEntry.count);
								}
								num2 = X.Mx(num2, nelItemEntry.count);
								if (num3 == -1)
								{
									num3 = (int)nelItemEntry.grade;
								}
								else if (num3 != (int)nelItemEntry.grade)
								{
									num3 = -2;
								}
							}
						}
						if (num != -1000)
						{
							if (num3 >= 0)
							{
								NelItem.getGradeMeshTxTo(stb.Add(" "), num3, 1, 40);
							}
							else
							{
								stb.Add("  ");
							}
							stb.Add("<font size=\"75%\">");
							if (num == num2)
							{
								stb.Add("x", num, "");
							}
							else
							{
								stb.Add("x", num, "...").Add(num2);
							}
							stb.Add("</font>");
						}
					}
					if (!flag)
					{
						this.text_alpha = 0.6f;
					}
				}
				base.setTitleTextS(stb);
			}
			this.fine_flag = true;
		}

		public void initRpiEffect(NelItem SrcItm, RecipeManager.RPI_EFFECT rpi, float f, int grade)
		{
			this.Tx.size = 12f;
			this.rpi_ef = rpi;
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					NelItem.GrdVariation gradeVariation = SrcItm.getGradeVariation(grade, null);
					stb.AddTxA("recipe_effect_" + FEnum<RecipeManager.RPI_EFFECT>.ToStr(rpi).ToLower(), false);
					stb.Add(" : ");
					RecipeManager.getRPIEffectDescriptionTo(stb, rpi, gradeVariation.getDetailTo(stb2.Clear(), f / SrcItm.max_grade_enpower, "\n", false), 1);
					this.Tx.Txt(stb);
				}
			}
			this.initLeftDummyButton(TX.Get("catalog_pickup", ""));
		}

		public void initRecipeIngredient(RecipeManager.RecipeIngredient Ing)
		{
			this.fine_flag = true;
			this.RecipeIng = null;
			if (Ing.Target != null)
			{
				this.initItem(Ing.Target, Ing.need, Ing.grade, true, null);
				this.initLeftDummyButton(TX.Get("catalog_jump", ""));
				return;
			}
			if (Ing.TargetRecipe != null)
			{
				NelItem recipeItem = Ing.TargetRecipe.RecipeItem;
				this.initItem(recipeItem, Ing.need, Ing.grade, true, null);
				this.initLeftDummyButton(TX.Get("catalog_jump", ""));
				return;
			}
			this.RecipeIng = Ing;
			using (STB stb = TX.PopBld(null, 0))
			{
				Ing.ingredientDescTo(stb, true, true);
				this.Tx.Txt(stb);
			}
			if (Ing.target_ni_category > NelItem.CATEG.OTHER)
			{
				this.row_left_px += 25;
				this.PF = MTR.AItemIcon[RecipeManager.RecipeIngredient.getToolIcon(Ing.target_ni_category)];
			}
			else
			{
				this.row_left_px += -27;
			}
			this.initLeftDummyButton(TX.Get("catalog_pickup", ""));
		}

		public void initItem(NelItem Itm, int count, int grade, bool ignore_obtain_count = true, string title_localized = null)
		{
			this.fine_flag = true;
			using (STB stb = TX.PopBld(null, 0))
			{
				bool flag = this.FDCon.isRowAssigned(Itm);
				if (Itm == null || (!ignore_obtain_count && !flag))
				{
					count = 0;
					grade = -1;
					Itm = NelItem.Unknown;
					title_localized = null;
				}
				if (title_localized != null)
				{
					stb.Set(title_localized);
				}
				else
				{
					stb.Add(Itm.getLocalizedName(grade, null));
				}
				if (count > 1)
				{
					stb.Add(" x", count, "");
				}
				if (grade >= 0)
				{
					stb.Add(" ");
					NelItem.getGradeMeshTxTo(stb, grade, 1, 64);
				}
				base.setTitleTextS(stb);
				if (!flag)
				{
					this.text_alpha = 0.6f;
				}
				else
				{
					this.initLeftDummyButton(TX.Get("catalog_jump", ""));
				}
			}
			this.Itm = Itm;
			this.row_left_px += 25;
		}

		public void initStoreManager(NelItem Itm, StoreManager _Store = null, int grade = 0)
		{
			_Store = _Store ?? this.Store;
			if (_Store == null || !_Store.isAlreadyMeet())
			{
				this.initItem(null, 1, 0, true, null);
				return;
			}
			if (this.Store != _Store)
			{
				this.Store = _Store;
				this.row_left_px += 25;
				this.initLeftDummyButton(TX.Get("catalog_pickup", ""));
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				this.Store.AddStoreTitleTo(stb);
				stb.Add("\n  <font size=\"80%\">");
				stb.AddTxA("catalog_price_buy", false).Add(this.Store.buyPrice(Itm, grade));
				if (!Itm.is_precious)
				{
					stb.Add(" / ");
					stb.AddTxA("catalog_price_sell", false).Add(this.Store.sellPrice(Itm, grade));
				}
				stb.Add("</font>");
				base.setTitleTextS(stb);
			}
		}

		public void initMapAddress(NelM2DBase M2D, Map2d Mp)
		{
			Mp = Mp ?? this.TargetMp;
			WholeMapItem wholeFor = M2D.WM.GetWholeFor(Mp, false);
			if (wholeFor == null)
			{
				this.initItem(null, 1, 0, true, null);
				return;
			}
			if (this.TargetMp != Mp)
			{
				this.TargetMp = Mp;
				this.initLeftDummyButton(TX.Get("catalog_show", ""));
			}
			this.TargetWM = wholeFor;
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add(M2D.getMapTitle(Mp, this.TargetWM, true));
				base.setTitleTextS(stb);
			}
		}

		public void initSummonerInfo(NelM2DBase M2D, FDSummonerInfo _FSmn)
		{
			_FSmn = _FSmn ?? this.FSmn;
			if (_FSmn == null || !_FSmn.valid || _FSmn.defeat_count == 0)
			{
				this.initItem(null, 1, 0, true, null);
				return;
			}
			if (this.FSmn != _FSmn)
			{
				this.FSmn = _FSmn;
				this.initLeftDummyButton(TX.Get("catalog_jump", ""));
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add(this.FSmn.Summoner.name_localized);
				base.setTitleTextS(stb);
			}
		}

		protected override void RowFineAfter(float w, float h)
		{
			float num = -w * 0.5f + (float)this.row_left_px - 34f;
			if (this.Itm == NelItem.Unknown)
			{
				this.Md.chooseSubMesh(1, false, false);
				this.Md.Col = this.Md.ColGrd.Set(4283780170U).mulA(this.alpha_).C;
				this.Md.RotaPF(num, 0f, 1f, 1f, 0f, MTR.AItemIcon[57], false, false, false, uint.MaxValue, false, 0);
				this.Md.chooseSubMesh(0, false, false);
			}
			else if (this.Store != null)
			{
				if (this.Store.PFIcon != null)
				{
					this.Md.chooseSubMesh(1, false, false);
					this.Md.Col = this.Md.ColGrd.Set(uint.MaxValue).mulA(this.alpha_).C;
					this.Md.RotaPF(num, 0f, 1f, 1f, 0f, this.Store.PFIcon, false, false, false, uint.MaxValue, false, 0);
					this.Md.chooseSubMesh(0, false, false);
				}
			}
			else if (this.IR != null)
			{
				this.Md.chooseSubMesh(1, false, false);
				this.IR.drawSmallIcon(this.Md, num, 0f, this.alpha_, 1f, false);
				this.Md.chooseSubMesh(0, false, false);
			}
			else if (this.Itm != null)
			{
				this.Itm.drawIconTo(this.Md, null, 0, 1, num, 0f, 1f, this.alpha_, null);
			}
			else if (this.PF != null)
			{
				this.Md.chooseSubMesh(1, false, false);
				this.Md.Col = this.Md.ColGrd.Set(4283780170U).mulA(this.alpha_).C;
				this.Md.RotaPF(num, 0f, 1f, 1f, 0f, this.PF, false, false, false, uint.MaxValue, false, 0);
				this.Md.chooseSubMesh(0, false, false);
			}
			base.RowFineAfter(w, h);
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			this.Md.chooseSubMesh(0, false, false);
			float w = this.w;
			float h = this.h;
			if (this.TxR.enabled)
			{
				this.MdR.clear(false, false);
				ButtonSkinNormalNel.drawS(this, this.MdR, this.TxR, this.nncolor, this.TxR.get_swidth_px() + 19f + 34.5f, 20.4f, 1f, null, 0f);
				this.MdR.updateForMeshRenderer(false);
			}
			base.Fine();
			return this;
		}

		private UiFieldGuide FDCon;

		public ReelManager.ItemReelContainer IR;

		public NelItem Itm;

		public StoreManager Store;

		private TextRenderer TxR;

		private MeshDrawer MdR;

		public FDSummonerInfo FSmn;

		public Map2d TargetMp;

		public WholeMapItem TargetWM;

		public RecipeManager.RPI_EFFECT rpi_ef;

		private PxlFrame PF;

		public const float offline_alpha = 0.6f;

		public const float BTN_DEFAULT_H = 30f;

		public RecipeManager.RecipeIngredient RecipeIng;

		public ButtonSkinNormalNel.NNColor nncolor = ButtonSkinNormalNel.NNColor.NNcolor_default;
	}
}

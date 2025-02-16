using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class UILogRow : IRunAndDestroy
	{
		public UILogRow(UILog _Con)
		{
			this.Con = _Con;
			this.Md = this.Con.MMRD.Make(MTRX.MtrMeshNormal);
			this.Gob = this.Con.MMRD.GetGob(this.Md);
			this.Mrd = this.Con.MMRD.GetMeshRenderer(this.Md);
			this.Md.chooseSubMesh(1, false, true);
			this.Md.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
			this.Md.chooseSubMesh(0, false, false);
			this.Md.connectRendererToTriMulti(this.Con.MMRD.GetMeshRenderer(this.Md));
			this.MaFrame = new MdArranger(this.Md);
			this.MaIco = new MdArranger(this.Md);
			this.Tx = IN.CreateGob(this.Gob, "-tx").AddComponent<TextRenderer>();
			this.MrdTx = this.Tx.gameObject.GetComponent<MeshRenderer>();
			this.Tx.html_mode = true;
			this.Tx.auto_condense = true;
			this.Tx.Align(ALIGN.LEFT).AlignY(ALIGNY.MIDDLE).LetterSpacing(0.9f);
			this.Gob.SetActive(false);
		}

		public bool valotile_enabled
		{
			get
			{
				return this.Md.draw_gl_only;
			}
			set
			{
				this.Md.draw_gl_only = value;
				this.Tx.draw_gl_only = value;
				this.Mrd.enabled = (this.MrdTx.enabled = !value);
				if (!value)
				{
					this.Md.updateForMeshRenderer(true);
					this.Tx.getMeshDrawer().updateForMeshRenderer(true);
				}
			}
		}

		public void fineFonts()
		{
			if (this.Tx != null)
			{
				this.Tx.TargetFont = TX.getDefaultFont();
			}
		}

		public UILogRow activate(UILogRow.TYPE _type, string _key, string _tx)
		{
			this.af = 0f;
			this.hold_t = 0f;
			this.af_evt = 25f;
			this.a_count = 1;
			this.a_count_a = 1;
			this.a_key = _key;
			this.type = _type;
			this.maxt = 250;
			this.swidth_stretched = 340f;
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			if (nelM2DBase != null && this.Con.Base.FlgFrontLog.isActive())
			{
				IN.setZ(this.Gob.transform, -1.2200003f);
			}
			else
			{
				this.activate_when_gmui = nelM2DBase != null && nelM2DBase.GM != null && nelM2DBase.GM.isActive();
				if (this.activate_when_gmui)
				{
					IN.setZ(this.Gob.transform, -2f);
					IN.setZ(this.Tx.transform, -0.01f);
				}
				else
				{
					IN.setZ(this.Gob.transform, 0f);
				}
			}
			IN.setZAbs(this.Tx.transform, -4.86f);
			this.Md.clear(false, false);
			this.valotile_enabled = this.Con.valotile_enabled;
			this.MaFrame.clear(this.Md);
			this.MaIco.clear(this.Md);
			this.pixel_shift_x = 0f;
			this.redrawBg(0f);
			this.Gob.SetActive(false);
			this.Tx.Size(this.Tx.getStorage().defaultRendererSize).Col(4283780170U);
			this.Tx.max_swidth_px = UIBase.gamewh * 0.8f;
			this.Tx.auto_condense_line = true;
			this.Tx.text_content = "";
			this.Tx.effect_confusion = false;
			PR pr;
			if ((pr = UIPicture.getPr()) != null && pr.Ser.getLevel(SER.CONFUSE) >= 2)
			{
				this.Tx.effect_confusion = true;
			}
			this.need_fine_bg = true;
			this.a_tx = ((_tx == null) ? TX.Get(_key, "") : _tx);
			this.a_tx_a = null;
			this.TextShift(0f);
			this.y_level = -1f;
			this.bg_top_color = C32.c2d(C32.MulA(4293321691U, 0.6f));
			this.bg_bottom_color = C32.c2d(C32.MulA(4291611332U, 0.8f));
			this.fade_color = uint.MaxValue;
			this.Con.ev_temporary_sort = true;
			if (this.Con.Length == 1)
			{
				this.Con.yshift = 4f;
				if (this.Con.Base != null)
				{
					this.Con.Base.uiResetLogPos(-1f, -1f);
				}
			}
			return this.fineText(true);
		}

		public UILogRow BgCol(uint tx_col, uint bg_top_col, uint bg_bottom_col, uint fade_col)
		{
			this.bg_top_color = bg_top_col;
			this.bg_bottom_color = bg_bottom_col;
			this.fade_color = fade_col;
			this.Tx.Col(tx_col);
			this.need_fine_bg = true;
			return this;
		}

		public UILogRow fineTextTo(string str, bool fine_af = false)
		{
			this.need_fine_text = false;
			this.a_tx = str;
			this.Tx.Txt(this.a_tx);
			if (fine_af)
			{
				this.af = X.Mn(this.af, 7f);
			}
			return this;
		}

		public UILogRow fineText(bool first = true)
		{
			this.need_fine_text = false;
			STB stb = TX.PopBld(null, 0);
			switch (this.type)
			{
			case UILogRow.TYPE.GETITEM:
				if (this.a_tx_a != null)
				{
					stb.Add("<b>", this.a_tx, "</b>", this.a_tx_a, " x");
					stb.Add(this.a_count);
					stb.Add(" ");
					stb.AddTxA("Item_has", false).TxRpl(this.a_count_a);
				}
				break;
			case UILogRow.TYPE.CONSUMEITEM:
				if (this.a_tx_a != null)
				{
					stb.AddTxA("ItemSel_Consumed", false).TxRpl(this.a_tx).TxRpl(this.a_count_a);
				}
				break;
			case UILogRow.TYPE.MONEY:
			{
				int num = 0;
				if (this.a_count != 0)
				{
					if (first)
					{
						this.a_tx_a = ((this.a_count > 0) ? TX.GetA("Money_adding", this.a_count.ToString()) : ((this.a_count < 0) ? TX.GetA("Money_reducing", (-this.a_count).ToString()) : ""));
					}
					this.need_fine_text = true;
					if (this.af > 7f)
					{
						int num2 = (int)(this.af - 7f) / 2 * 2;
						int num3 = X.Mn(2 * X.Abs(this.a_count_a), 80);
						if (num2 >= num3)
						{
							this.need_fine_text = false;
						}
						else
						{
							num = (int)((float)this.a_count_a * (1f - X.ZLINE((float)(num2 + 2), (float)num3)));
						}
					}
				}
				else if (first)
				{
					this.a_tx_a = "";
				}
				string text = ((long)((ulong)CoinStorage.getCount(CoinStorage.CTYPE.GOLD) - (ulong)((long)num))).ToString();
				if (text != this.a_tx)
				{
					this.a_tx = text;
					SND.Ui.play("adding_coin", false);
				}
				stb.Add("<img mesh=\"money_icon\" /><b>", this.a_tx, " </b> ").Add(this.a_tx_a);
				break;
			}
			default:
				stb.Set(this.a_tx).Replace('\n', ' ', 0, -1);
				break;
			}
			if (this.Tx.Txt(stb))
			{
				this.need_recheck_size = true;
			}
			TX.ReleaseBld(stb);
			return this;
		}

		public UILogRow setIcon(PxlFrame PFIco, uint col = 4294967295U)
		{
			this.TextShift(40f);
			this.Md.chooseSubMesh(1, false, false);
			int vertexMax = this.Md.getVertexMax();
			int triMax = this.Md.getTriMax();
			if (this.MaIco.Length == 0)
			{
				this.MaIco.Set(true);
			}
			else
			{
				this.MaIco.revertVerAndTriIndexFirstSaved(false);
			}
			this.Md.Col = C32.d2c(col);
			this.Md.RotaPF(-108f, 0f, 1f, 1f, 0f, PFIco, false, false, false, uint.MaxValue, false, 0);
			this.need_mesh_update = true;
			if (this.MaIco.Length == 0)
			{
				this.MaIco.Set(false);
			}
			else
			{
				this.Md.revertVerAndTriIndex(vertexMax, triMax, false);
			}
			return this.fineText(true);
		}

		public UILogRow activateGetItem(STB StbActKey, NelItemManager IMNG, NelItem Itm, int count = 1, int grade = 0)
		{
			return this.activateForItem(UILogRow.TYPE.GETITEM, StbActKey, Itm, count, grade, IMNG.countItem(Itm));
		}

		public UILogRow activateForItem(UILogRow.TYPE type, STB StbActKey, NelItem Itm, int count, int grade, int rest_count)
		{
			this.activate(type, StbActKey.ToString(), Itm.getLocalizedName(grade, null));
			this.a_count = count;
			this.a_count_a = rest_count;
			bool flag = TX.isEnglishLang();
			this.a_tx_a = "";
			if (!Itm.individual_grade)
			{
				STB stb = TX.PopBld(null, 0);
				stb += "<img mesh=\"nel_item_grade.";
				bool flag2 = flag || type == UILogRow.TYPE.CONSUMEITEM;
				stb += (flag2 ? (grade + 5) : grade);
				stb += "\" color=\"";
				stb.AddColor(4283780170U);
				stb += "\" width=\"";
				stb += (flag2 ? 38 : 75);
				stb += "\"/> ";
				this.a_tx_a = stb.ToString();
				TX.ReleaseBld(stb);
			}
			this.TextShift(34f);
			this.MaIco.Set(true);
			Itm.drawIconTo(this.Md, (M2DBase.Instance as NelM2DBase).IMNG.getHouseInventory(), 0, 1, -115f, 0f, 1f, 1f, null);
			this.MaIco.Set(false);
			this.Md.chooseSubMesh(0, false, false);
			return this.fineText(true);
		}

		public UILogRow activateConsumeItem(STB StbActKey, NelItem Itm, int grade = 0, int rest_count = 0)
		{
			return this.activateForItem(UILogRow.TYPE.CONSUMEITEM, StbActKey, Itm, 1, grade, rest_count);
		}

		public UILogRow activateMoney(int added)
		{
			this.activate(UILogRow.TYPE.MONEY, "MONEY", "");
			this.a_count = added;
			this.a_count_a = added;
			this.TextShift(30f);
			return this.fineText(true);
		}

		public UILogRow fineTime(bool resort = true)
		{
			this.hold_t = 90f;
			if (this.af_evt < 0f)
			{
				this.af_evt = 25f;
				if (resort)
				{
					this.Con.ev_temporary_sort = true;
				}
				this.repos = true;
			}
			return this;
		}

		public UILogRow addCount(int count = 1, int add_to_a = -1000)
		{
			this.af = X.Mn(this.af, 7f);
			this.a_count += count;
			if (add_to_a != -1000)
			{
				this.a_count_a = add_to_a;
			}
			this.fineTime(true);
			return this.fineText(true);
		}

		public UILogRow hold()
		{
			if (this.af >= (float)this.maxt)
			{
				return null;
			}
			this.hold_t = 20f;
			if (this.af >= 15f)
			{
				this.af = X.Mx(X.Mn(this.af, (float)(this.maxt - 60)), X.Mn(this.af, 15f));
			}
			return this;
		}

		public UILogRow hideProgress()
		{
			if (this.af < 19f)
			{
				this.need_fine_bg = true;
			}
			this.af = X.Mx(this.af, (float)(this.maxt - 30 - 15));
			return this;
		}

		public bool run(float fcnt)
		{
			if (this.Con.act_count >= 4.5f)
			{
				return true;
			}
			if (this.af < 0f)
			{
				this.af = X.Mn(0f, fcnt + this.af);
				return true;
			}
			if (this.af >= (float)this.maxt)
			{
				this.destruct();
				return false;
			}
			if (this.af == 0f)
			{
				this.Gob.SetActive(true);
			}
			float num = X.ZLINE((float)this.maxt - this.af, 30f);
			this.Con.act_count_tdh += num;
			this.Con.act_count += num * ((this.af_evt >= 0f) ? X.ZLINE(this.af_evt, 25f) : X.ZLINE(25f + this.af_evt, 25f));
			if (X.D || this.af == 0f)
			{
				float num2;
				if (this.af_evt >= 0f)
				{
					if (this.af_evt < 25f)
					{
						this.af_evt += fcnt;
						this.repos = true;
					}
					num2 = this.Con.act_count;
				}
				else
				{
					if (this.af_evt > -25f)
					{
						this.af_evt -= fcnt;
						this.repos = true;
					}
					num2 = this.Con.act_count_tdh;
				}
				if (this.af_evt > -25f)
				{
					if (num2 != this.y_level)
					{
						this.y_level = num2;
						this.repos = true;
					}
				}
				else
				{
					this.y_level = -1f;
				}
				if (this.need_fine_text)
				{
					this.fineText(false);
				}
				else if (this.need_recheck_size)
				{
					this.TextShift(this.pixel_shift_x);
				}
				if (this.af < 19f || this.need_fine_bg)
				{
					this.redrawBg(X.ZSIN(this.af, 15f)).reposit(0f);
				}
				else if (this.af >= (float)(this.maxt - 30))
				{
					this.reposit(1f - X.ZLINE((float)this.maxt - this.af, 30f));
				}
				else if (this.repos)
				{
					this.reposit(0f);
				}
				if (this.need_mesh_update)
				{
					this.need_mesh_update = false;
					this.Md.updateForMeshRenderer(true);
				}
			}
			if (this.hold_t > 0f && this.Con.isVisible())
			{
				this.hold_t = X.Mx(this.hold_t - fcnt, 0f);
			}
			if (this.Con.isVisible() || this.af < 100f || this.af >= (float)(this.maxt - 30))
			{
				if (this.hold_t == 0f && this.af < (float)this.maxt - 22.5f && this.Con.top_fasten == 1)
				{
					if (this.af <= 15f)
					{
						this.need_fine_bg = true;
					}
					bool flag = this.type == UILogRow.TYPE.GETITEM || this.type == UILogRow.TYPE.ALERT_BENCH || this.type == UILogRow.TYPE.ALERT_EP;
					this.Con.top_fasten = 2;
					float num3 = (float)this.maxt - 22.5f + fcnt;
					this.af = X.Mn(num3, this.af + fcnt * (float)(flag ? 300 : 3));
				}
				else
				{
					if (this.af >= (float)this.maxt - 22.5f && this.Con.top_fasten == 1)
					{
						this.Con.top_fasten = 2;
						fcnt *= 3f;
					}
					this.af += fcnt;
				}
			}
			return true;
		}

		public UILogRow redrawBg(float tz)
		{
			float num = 1.2f + X.ZSIN(tz, 0.4f) * 0.2f - 0.4f * X.ZCOS(tz - 0.3f, 0.7f);
			this.Md.chooseSubMesh(0, false, false);
			int vertexMax = this.Md.getVertexMax();
			int triMax = this.Md.getTriMax();
			if (this.MaFrame.Length == 0)
			{
				this.MaFrame.Set(true);
			}
			else
			{
				this.MaFrame.revertVerAndTriIndexFirstSaved(false);
			}
			this.Md.KadomaruRect((this.swidth_stretched - 340f) * 0.5f, 0f, this.swidth_stretched * num, 34f * num, 17f, 0f, false, 0f, 0f, false);
			if (this.MaFrame.Length == 0)
			{
				this.MaFrame.Set(false);
			}
			else
			{
				this.Md.revertVerAndTriIndex(vertexMax, triMax, false);
			}
			this.MaFrame.setColAllGrdation(-17f, 17f, this.Md.ColGrd.Set(this.bg_bottom_color).blend(this.fade_color, 1f - tz).mulA((float)((tz == 0f) ? 0 : 1))
				.C, MTRX.cola.Set(this.bg_top_color).blend(this.fade_color, 1f - tz).mulA((float)((tz == 0f) ? 0 : 1))
				.C, GRD.BOTTOM2TOP, false, false);
			this.need_mesh_update = true;
			this.Md.chooseSubMesh(1, false, false);
			this.MaIco.setAlpha1(tz, false);
			this.Tx.setAlpha(tz * this.text_alpha);
			this.need_fine_bg = false;
			return this;
		}

		public float text_alpha
		{
			get
			{
				return 1f - X.ZLINE(1f - ((float)this.maxt - this.af) / 30f, 0.25f);
			}
		}

		public UILogRow reposit(float x_tz)
		{
			Vector3 localPosition = this.Gob.transform.localPosition;
			x_tz = -0.02f * X.ZSIN(x_tz, 0.45f) + 1.02f * X.ZPOW(x_tz - 0.45f, 0.55f);
			float num = ((this.af_evt >= 0f) ? X.ZSIN(this.af_evt, 25f) : (1f - X.ZSIN(-this.af_evt, 25f)));
			float num2 = this.Con.base_active_level * num;
			x_tz = 1f - (1f - x_tz) * num2;
			localPosition.x = -340f * X.NI(2.4f, 1.6f, num) * x_tz * 0.015625f;
			localPosition.y = -40f * (this.y_level + this.Con.yshift) * 0.015625f;
			this.Gob.transform.localPosition = localPosition;
			this.repos = false;
			return this;
		}

		public UILogRow TextShift(float pixel_x)
		{
			this.pixel_shift_x = pixel_x;
			if (this.af < 4f || this.need_fine_text)
			{
				this.need_recheck_size = true;
				return this;
			}
			this.need_recheck_size = false;
			Vector3 localPosition = this.Tx.transform.localPosition;
			localPosition.x = (-130f + pixel_x) * 0.015625f;
			this.Tx.transform.localPosition = localPosition;
			float num = this.Tx.get_swidth_px() + pixel_x + 40f + 27.2f;
			if (num > this.swidth_stretched && this.Tx.size == this.Tx.getStorage().defaultRendererSize)
			{
				this.Tx.Size(this.Tx.getStorage().defaultRendererSize * 0.8f);
				num = this.Tx.get_swidth_px() + pixel_x + 40f + 27.2f;
			}
			if (num > this.swidth_stretched)
			{
				this.swidth_stretched = num;
				this.need_fine_bg = true;
			}
			return this;
		}

		public void RenderOneSide(bool draw_ico, ref bool draw_initted, Matrix4x4 Multiple, Camera Cam = null)
		{
			this.RenderOneSide(draw_ico ? 1 : 0, this.Md, this.Mrd.transform, ref draw_initted, Multiple, Cam);
		}

		public void RenderOneSideText(int id, ref bool draw_initted, Matrix4x4 Multiple, Camera Cam = null)
		{
			MeshDrawer meshDrawer = this.Tx.getMeshDrawer();
			if (!meshDrawer.hasMultipleTriangle() && id != 7)
			{
				return;
			}
			this.RenderOneSide(id, meshDrawer, this.Tx.transform, ref draw_initted, Multiple, Cam);
		}

		public void RenderOneSide(int mesh_id, MeshDrawer Md, Transform Trs, ref bool draw_initted, Matrix4x4 Multiple, Camera Cam = null)
		{
			int num = 0;
			int[] array;
			if (!Md.hasMultipleTriangle())
			{
				array = Md.getTriangleArray();
				num = Md.draw_triangle_count;
			}
			else
			{
				array = Md.getSubMeshData(mesh_id, ref num);
			}
			if (array != null && num > 0)
			{
				if (!draw_initted)
				{
					draw_initted = true;
					Md.getSubMeshMaterial(mesh_id).SetPass(0);
				}
				GL.LoadProjectionMatrix(Multiple * Trs.localToWorldMatrix * ValotileRenderer.Mx_z_zero);
				BLIT.RenderToGLImmediate001(Md, num, mesh_id, false, true, null);
			}
		}

		public bool event_temp_active
		{
			get
			{
				return this.af_evt >= 0f;
			}
			set
			{
				if (this.af_evt >= 0f && !value)
				{
					this.af_evt = X.Mn(-1f, -25f + this.af_evt);
					this.repos = true;
					return;
				}
				if (this.af_evt < 0f && value)
				{
					this.af_evt = X.Mx(0f, 25f + this.af_evt);
					this.repos = true;
				}
			}
		}

		public UILogRow ShowDelay(float delay)
		{
			if (this.maxt > 0)
			{
				if (this.af <= 0f)
				{
					this.af -= delay;
				}
				else
				{
					this.af = X.Mx(this.af - delay, 1f);
				}
			}
			return this;
		}

		public bool isHiding()
		{
			return this.af >= (float)(this.maxt - 30);
		}

		public bool completely_hidden
		{
			get
			{
				return this.maxt == 0 || this.af >= (float)this.maxt;
			}
		}

		public void deactivate(bool immediate = false)
		{
			this.maxt = 45;
			this.af = (immediate ? ((float)(this.maxt - 1)) : X.Mn(this.af, 15f));
		}

		public void destruct()
		{
			this.y_level = 5f;
			this.Gob.SetActive(false);
			this.MaIco.clear(this.Md);
			this.MaFrame.clear(this.Md);
			this.maxt = 0;
		}

		public UILogRow.TYPE type;

		public int id;

		public string a_key;

		public string a_tx;

		private string a_tx_a;

		private int a_count;

		private int a_count_a;

		public float y_level;

		public const float marg_l = 40f;

		private float pixel_shift_x;

		private bool activate_when_gmui;

		public uint bg_top_color;

		public uint bg_bottom_color;

		public uint fade_color;

		public int maxt;

		public const float swidth = 340f;

		public const float sheight = 34f;

		public const float y_intv = 40f;

		public float swidth_stretched = 340f;

		public readonly UILog Con;

		public float af;

		private float hold_t;

		public readonly GameObject Gob;

		public MeshDrawer Md;

		private MeshRenderer Mrd;

		public MdArranger MaFrame;

		public MdArranger MaIco;

		private bool need_mesh_update;

		private bool need_recheck_size;

		private float af_evt;

		public TextRenderer Tx;

		private MeshRenderer MrdTx;

		public bool repos;

		public bool need_fine_text;

		public bool need_fine_bg;

		public const int T_FADE = 15;

		public const int T_FADEOUT = 30;

		public const int T_AEV = 25;

		public const int T_FADE_LINE = 4;

		public const int T_MAXT_DEF = 250;

		public enum TYPE
		{
			NORMAL,
			GETITEM,
			CONSUMEITEM,
			MONEY,
			ALERT,
			ALERT_EGG,
			ALERT_EP,
			ALERT_EP2,
			ALERT_GRAY,
			ALERT_HUNGER,
			ALERT_BENCH,
			ALERT_PUPPET,
			ALERT_FATAL,
			ALERT_BARU
		}

		private enum MESH
		{
			FRAME,
			ICO,
			TEXT
		}
	}
}

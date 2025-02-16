using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class UiTitleDifficultyConfirm : IRunAndDestroy
	{
		public UiTitleDifficultyConfirm(GameObject _Base, float _z, MImage _MI)
		{
			this.MI = _MI;
			this.Gob = IN.CreateGob(_Base, "-difficulty_confirm");
			IN.setZ(this.Gob.transform, _z);
			this.Cld = this.Gob.AddComponent<BoxCollider2D>();
			this.Cld.size = new Vector2(IN.w + 100f, IN.h + 100f) * 0.015625f;
			this.PFArrow = MTRX.getPF("menu_arrow");
			this.Md = MeshDrawer.prepareMeshRenderer(this.Gob, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
			this.Md.chooseSubMesh(1, false, false);
			this.Md.setMaterial(this.MI.getMtr(BLEND.NORMAL, -1), false);
			this.Md.chooseSubMesh(2, false, false);
			this.Md.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
			this.Md.chooseSubMesh(0, false, false);
			this.Md.connectRendererToTriMulti(this.Gob.GetComponent<MeshRenderer>());
			this.Ma = new MdArranger(this.Md);
			this.FbT = IN.CreateGob(this.Gob, "-txT").AddComponent<FillBlock>();
			this.FbB = IN.CreateGob(this.Gob, "-txB").AddComponent<FillBlock>();
			this.FbC = IN.CreateGob(this.Gob, "-txC").AddComponent<FillImageBlock>();
			this.FbC.TxCol = (this.FbT.TxCol = (this.FbB.TxCol = MTRX.ColWhite));
			this.FbT.TxBorderCol = (this.FbB.TxBorderCol = MTRX.ColBlack);
			this.FbC.TxBorderCol = C32.MulA(MTRX.ColBlack, 0.4f);
			this.FbT.size = (this.FbB.size = 20f);
			this.FbT.widthPixel = IN.w * 0.6f;
			this.FbB.widthPixel = IN.w * 0.4f;
			this.FbT.text_auto_wrap = (this.FbB.text_auto_wrap = true);
			this.FbB.alignx = ALIGN.LEFT;
			this.FbC.alignx = (this.FbT.alignx = ALIGN.CENTER);
			this.FbT.aligny = (this.FbB.aligny = (this.FbC.aligny = ALIGNY.MIDDLE));
			this.FbC.TargetFont = TX.getTitleFont();
			this.FbC.size = 38f;
			this.FbC.FnDrawFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawCenterText);
			this.FbC.margin_x = 0f;
			this.FbT.StartFb(TX.Get("Title_difficulty_top", ""), null, true);
			this.FbB.StartFb(" ", null, true);
			this.FbC.StartFb(" ", null, false);
			IN.PosP(this.FbT.transform, 0f, IN.hh * 0.82f, -0.25f);
			IN.PosP(this.FbB.transform, 0f, -IN.hh * 0.74f, -0.25f);
			IN.PosP(this.FbC.transform, -UiTitleDifficultyConfirm.pic_center_x, UiTitleDifficultyConfirm.pic_center_y, -0.25f);
			IN.addRunner(this);
		}

		private void fineText()
		{
			this.FbB.text_content = TX.Get((this.diff_cursor == 0) ? "Title_difficulty_desc_cas" : ((this.diff_cursor == 1) ? "Title_difficulty_desc_nor" : "Title_difficulty_desc_pro"), "");
			this.FbC.text_content = TX.Get((this.diff_cursor == 0) ? "Title_difficulty_cas" : ((this.diff_cursor == 1) ? "Title_difficulty_nor" : "Title_difficulty_pro"), "");
		}

		public bool run(float fcnt)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (this.t >= 0f)
			{
				if (this.t < 40f)
				{
					flag = true;
					this.FbT.alpha = X.ZLINE(this.t, 30f);
				}
				if (this.t < 50f)
				{
					flag2 = true;
				}
				bool flag4 = this.t >= 20f;
				this.t += fcnt;
				bool flag5 = false;
				if (this.t >= 20f && this.result == -1)
				{
					if (!flag4)
					{
						flag5 = true;
					}
					bool flag6 = IN.isL();
					bool flag7 = IN.isR();
					bool flag8 = IN.isSubmit();
					if (IN.isMousePushDown(1))
					{
						Vector2 vector = IN.getMousePos(null) * 64f;
						if (vector.x < -IN.wh * 0.8f)
						{
							flag6 = true;
						}
						else if (vector.x > IN.wh * 0.8f)
						{
							flag7 = true;
						}
						if (X.BTW(-IN.wh * 0.3f, vector.x, IN.wh * 0.3f) && X.BTW(-IN.hh * 0.2f, vector.y - UiTitleDifficultyConfirm.pic_center_y, IN.hh * 0.2f))
						{
							flag8 = true;
						}
					}
					if (flag8)
					{
						this.result = this.diff_cursor;
					}
					else if (IN.isCancel())
					{
						SND.Ui.play("cancel", false);
						this.deactivate();
						flag5 = false;
					}
					else if (flag6)
					{
						if (this.diff_cursor == 0)
						{
							this.t_select = 5.4f;
							SND.Ui.play("toggle_button_limit", false);
						}
						else
						{
							this.diff_cursor--;
							this.t_select = -18f;
							flag5 = true;
						}
					}
					else if (flag7)
					{
						if (this.diff_cursor == 2)
						{
							SND.Ui.play("toggle_button_limit", false);
							this.t_select = -5.4f;
						}
						else
						{
							this.diff_cursor++;
							this.t_select = 18f;
							flag5 = true;
						}
					}
				}
				if (flag5)
				{
					SND.Ui.play("cursor", false);
					this.fineText();
				}
			}
			else
			{
				flag = true;
				this.t -= fcnt;
				if (this.t <= -30f)
				{
					this.destruct();
					return false;
				}
				this.FbT.alpha = (this.FbB.alpha = (this.FbC.alpha = X.ZLINE(30f + this.t, 30f)));
			}
			if (this.t_select != 0f)
			{
				flag2 = true;
				this.t_select = X.VALWALK(this.t_select, 0f, fcnt);
			}
			if (flag)
			{
				flag2 = (this.need_update_mesh = (flag3 = true));
				this.Md.clear(false, false);
				this.Md.chooseSubMesh(0, false, false);
				this.Md.Col = C32.MulA(3388997632U, (this.t >= 0f) ? X.ZLINE(this.t, 40f) : X.ZLINE(30f + this.t, 30f));
				this.Md.Rect(0f, 0f, IN.w + 100f, IN.h + 100f, false);
				this.Ma.Set(true);
			}
			if (flag2)
			{
				flag3 = (this.need_update_mesh = true);
				this.Md.chooseSubMesh(0, false, false);
				this.Ma.revertVerAndTriIndexFirstSaved(false);
				this.Md.chooseSubMesh(2, false, true);
				this.Md.chooseSubMesh(1, false, true);
				this.redrawScrollPicture();
				this.Md.chooseSubMesh(0, false, false);
				this.Ma.Set(false);
			}
			if (this.t >= 0f)
			{
				int num = (int)(this.t_curs_vib / 20f);
				this.t_curs_vib += fcnt;
				int num2 = (int)(this.t_curs_vib / 20f);
				if (num2 != num)
				{
					if (num2 >= 2)
					{
						num2 -= 2;
						this.t_curs_vib -= 40f;
					}
					flag3 = true;
				}
			}
			if (flag3)
			{
				this.need_update_mesh = true;
				this.Md.chooseSubMesh(0, false, false);
				this.Ma.revertVerAndTriIndexSaved(false);
				this.Md.chooseSubMesh(2, false, true);
				float num3 = (float)((int)(this.t_curs_vib / 20f) % 2 * 10 + ((this.t < 0f) ? 400 : 0));
				for (int i = 0; i < 2; i++)
				{
					if (!((i == 1) ? (this.diff_cursor == 0) : (this.diff_cursor >= 2)))
					{
						this.Md.RotaPF((float)X.MPF(i == 0) * (IN.wh * 0.88f + num3), UiTitleDifficultyConfirm.pic_center_y, 2f, 2f, (float)X.MPF(i == 0) * 1.5707964f, this.PFArrow, false, false, false, uint.MaxValue, false, 0);
					}
				}
			}
			if (this.need_update_mesh)
			{
				this.need_update_mesh = false;
				this.Md.updateForMeshRenderer(true);
			}
			return true;
		}

		private void redrawScrollPicture()
		{
			float num = ((this.t >= 0f) ? X.ZLINE(this.t - 15f, 35f) : X.ZLINE(30f + this.t, 30f));
			if (num == 0f)
			{
				return;
			}
			this.Md.Col = C32.MulA(uint.MaxValue, num);
			float num2 = 0.33333334f;
			this.Md.initForImg(this.MI.Tx);
			float num3 = UiTitleDifficultyConfirm.pic_center_y;
			float num4 = IN.w * 0.62f;
			float num5 = UiTitleDifficultyConfirm.pic_center_x * 0.53f - num4 * (float)this.diff_cursor;
			if (this.t_select != 0f)
			{
				if (this.t_select > 0f)
				{
					num5 += X.ZPOW(this.t_select, 18f) * num4;
				}
				else if (this.t_select < 0f)
				{
					num5 += -X.ZPOW(-this.t_select, 18f) * num4;
				}
			}
			float num6 = ((this.t >= 0f) ? X.NI(0.7f, 1f, X.ZLINE(this.t - 15f, 20f)) : 1f);
			for (int i = 0; i < 3; i++)
			{
				this.Md.uvRect(num2 * (float)i, 0f, num2, 1f, false, false).RotaGraph(num5, num3, num6, 0f, null, false);
				num5 += num4;
			}
			this.need_update_mesh = true;
		}

		public bool fnDrawCenterText(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			float swidth_px = FI.get_swidth_px();
			if (swidth_px < 10f || FI.textIs(" "))
			{
				return false;
			}
			float num = FI.text_alpha * ((this.t >= 0f) ? X.ZLINE(this.t - 20f, 20f) : 1f);
			Md.Col = Md.ColGrd.Set(3741319168U).mulA(num).C;
			Md.ColGrd.setA(0f);
			Md.KadomaruRect(0f, 0f, swidth_px + 90f, 90f, 90f, 0f, false, 1f, 0f, false);
			update_meshdrawer = true;
			return num < 1f;
		}

		public bool isDecided()
		{
			if (this.result >= 0)
			{
				DIFF.I = this.result;
				this.deactivate();
				return true;
			}
			return false;
		}

		public bool isActive()
		{
			return this.t >= 0f;
		}

		public void deactivate()
		{
			if (this.t >= 0f)
			{
				this.t = -1f;
				this.Cld.enabled = false;
			}
		}

		public void destruct(bool remove_from_runner)
		{
			if (remove_from_runner)
			{
				IN.remRunner(this);
			}
			this.destruct();
		}

		public void destruct()
		{
			if (this.Md != null)
			{
				this.Md.destruct();
			}
			if (this.Gob != null)
			{
				IN.DestroyOne(this.Gob);
				this.Gob = null;
			}
		}

		private GameObject Gob;

		private BoxCollider2D Cld;

		private MeshDrawer Md;

		private MdArranger Ma;

		private bool need_update_mesh;

		private MImage MI;

		private float t;

		private const float MAXT = 40f;

		private const float MAXT_FADEOUT = 30f;

		private const float t_appear = 20f;

		private static float pic_center_x = -IN.w * 0.16f;

		private static float pic_center_y = IN.h * 0.02f;

		public const float arrow_intv = 20f;

		private const float MAXT_SEL = 18f;

		private float t_select;

		private float t_curs_vib;

		private int diff_cursor = 1;

		private int result = -1;

		private PxlFrame PFArrow;

		private FillBlock FbT;

		private FillBlock FbB;

		private FillImageBlock FbC;
	}
}

using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public abstract class CtSetter : IDesignerBlock, IAlphaSetable
	{
		public float text_size
		{
			get
			{
				return (float)(TX.isEnglishLang() ? 14 : 19);
			}
		}

		public CtSetter(GameObject Parent, string key = "-CtSetter")
		{
			this.Gob = IN.CreateGob(Parent, "-CtSetter");
			this.Col = C32.d2c(MTRX.text_color);
			this.Gob.layer = LayerMask.NameToLayer(IN.gui_layer_name);
			this.Tx = IN.CreateGob(this.Gob, "-Tx").AddComponent<TextRenderer>();
			IN.setZ(this.Tx.transform, -0.0001f);
			this.Tx.Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE).Size(this.text_size)
				.LetterSpacing(0.95f)
				.Col(this.Col);
			this.Tx.auto_condense = true;
			this.Tx.html_mode = true;
		}

		public bool use_valotile
		{
			get
			{
				return this.Tx.use_valotile;
			}
			set
			{
				this.Tx.use_valotile = true;
				if (this.TxFade != null)
				{
					this.TxFade.use_valotile = true;
					if (value)
					{
						if (this.Valot == null)
						{
							this.Valot = IN.GetOrAdd<ValotileRenderer>(this.Gob);
						}
						this.Valot.enabled = true;
						return;
					}
					if (this.Valot != null)
					{
						this.Valot.enabled = false;
					}
				}
			}
		}

		public Transform getTransform()
		{
			return this.Gob.transform;
		}

		public virtual void fineValue(string val, bool set_to_element = false)
		{
			this.Tx.Txt(val);
		}

		public Color32 Color
		{
			get
			{
				return this.Col;
			}
			set
			{
				this.Col = value;
				this.Tx.Col(value);
				if (this.TxFade != null)
				{
					this.TxFade.Col(this.Col);
				}
			}
		}

		public CtSetter activate(bool manual_select = true)
		{
			if (this.Md == null)
			{
				this.TxFade = IN.CreateGob(this.Gob, "-TxFade").AddComponent<TextRenderer>();
				IN.setZ(this.TxFade.transform, -1E-06f);
				this.TxFade.Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE).Size(this.text_size)
					.LetterSpacing(0.95f)
					.Col(this.Col);
				this.TxFade.html_mode = true;
				this.TxFade.auto_condense = true;
				this.TxFade.use_valotile = this.Tx.use_valotile;
				Material mtr = MTRX.MIicon.getMtr(BLEND.NORMALST, this.stencil_ref_);
				if (this.stencil_ref_ >= 0)
				{
					this.TxFade.stencil_ref = this.stencil_ref_;
				}
				this.Md = MeshDrawer.prepareMeshRenderer(this.Gob, mtr, 0f, -1, null, this.Tx.use_valotile, false);
				this.Atl = new int[2];
			}
			if (CtSetter.ActiveSetter != this)
			{
				if (CtSetter.ActiveSetter != null)
				{
					CtSetter.ActiveSetter.deactivate();
				}
				CtSetter.ActiveSetter = this;
			}
			this.Tx.Alpha(this.alpha_);
			this.TxFade.Alpha(0f);
			this.t = (this.text_animate_f = 0f);
			this.Atl[0] = (this.Atl[1] = 0);
			if (manual_select)
			{
				if (CURS.isActive())
				{
					CURS.Loc(this.focus_curs_category, this.Tx.size * 1.6f * 0.015625f, -this.Tx.get_sheight_px() * 0.4f * 0.015625f, this.Gob.transform, false);
				}
				if (this.init_sound != "")
				{
					SND.Ui.play(this.init_sound, false);
				}
				this.auto_selected = false;
			}
			else
			{
				this.auto_selected = true;
			}
			return this;
		}

		public int stencil_ref
		{
			get
			{
				return this.stencil_ref_;
			}
			set
			{
				this.stencil_ref_ = value;
				if (this.Tx != null)
				{
					this.Tx.stencil_ref = value;
				}
				if (this.TxFade != null)
				{
					this.TxFade.stencil_ref = value;
				}
			}
		}

		public bool run(float fcnt)
		{
			if (this.Md == null || this.t < 0f)
			{
				this.activate(true);
			}
			if (this.t >= 5f && this.auto_deactivate_kettei && !this.auto_selected && IN.kettei3())
			{
				this.deactivateAndSelect();
				IN.clearPushDown(false);
				return false;
			}
			bool flag = X.D;
			if (this.isEnableMoving())
			{
				flag = this.executeSlide(0f) || flag;
			}
			if (flag)
			{
				float num = (float)(X.ANM((int)this.t, 2, 15f) * 6);
				this.Md.Col = this.Md.ColGrd.White().mulA(this.alpha_).C;
				for (int i = 0; i < 2; i++)
				{
					float num2 = this.swidth / 2f - (float)this.arrow_margin + num;
					PxlFrame pxlFrame = MTRX.MeshArrowR;
					int num3 = this.Atl[i];
					if (this.Atl[i] != 0)
					{
						this.Atl[i] = X.VALWALK(this.Atl[i], 0, 1);
						int num4 = X.Abs(this.Atl[i]);
						if (20 - num4 <= 4)
						{
							if (num3 > 0)
							{
								pxlFrame = MTRX.MeshArrowRShifted;
								num2 += 18f;
							}
							else
							{
								num2 += 12f;
							}
						}
						else
						{
							num2 += 10f * X.ZPOW((float)num4, 14f);
						}
						float num5 = X.ZLINE((float)num4 - 10f, 10f);
						if (num3 > 0)
						{
							this.Tx.Alpha(this.alpha_ * (1f - num5));
							this.TxFade.Alpha(this.alpha_ * num5);
						}
						else
						{
							this.Tx.Alpha(this.alpha_);
							this.TxFade.Alpha(0f);
						}
						float num6 = (float)(((i == 1) ? (-1) : 1) * ((num3 > 0) ? 28 : (-10))) * 0.015625f;
						Vector3 vector = this.TxFade.transform.localPosition;
						vector.x = num6 * (1f - num5);
						this.TxFade.transform.localPosition = vector;
						vector = this.Tx.transform.localPosition;
						vector.x = -num6 * num5;
						this.Tx.transform.localPosition = vector;
						this.Tx.Update();
						this.TxFade.Update();
					}
					this.Md.Scale((float)X.MPF(i == 1), 1f, false);
					this.Md.RotaPF(num2, 0f, 1f, 1f, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
					this.Md.Identity();
				}
				this.Md.updateForMeshRenderer(false);
				this.text_animate_f += fcnt;
				this.Tx.Alpha(this.alpha_ * (X.COSI(this.text_animate_f, 40f) * 0.2f + 0.8f));
			}
			else
			{
				if (this.Atl[0] != 0)
				{
					this.Atl[0] = X.VALWALK(this.Atl[0], 0, 1);
				}
				if (this.Atl[1] != 0)
				{
					this.Atl[1] = X.VALWALK(this.Atl[1], 0, 1);
				}
			}
			this.t += fcnt;
			return true;
		}

		public bool executeSlide(float move_scale = 0f)
		{
			if (move_scale == 0f)
			{
				if (IN.isL())
				{
					move_scale = (float)(-1 * X.MPF(!this.lr_reverse));
				}
				else if (IN.isR())
				{
					move_scale = (float)X.MPF(!this.lr_reverse);
				}
				else if (IN.isT())
				{
					move_scale = this.updown_move_scale * (float)X.MPF(!this.lr_reverse);
				}
				else if (IN.isB())
				{
					move_scale = -this.updown_move_scale * (float)X.MPF(!this.lr_reverse);
				}
			}
			if (move_scale < 0f)
			{
				this.TxFade.Txt(this.Tx);
				bool flag = this.moveToLeft(-move_scale);
				this.Atl[0] = 20 * X.MPF(flag);
				this.Atl[1] = 0;
				if (flag)
				{
					if (this.slide_sound_left != "")
					{
						SND.Ui.play(this.slide_sound_left, false);
					}
				}
				else if (this.slide_sound_limit != "")
				{
					SND.Ui.play(this.slide_sound_limit, false);
				}
				return true;
			}
			if (move_scale > 0f)
			{
				this.TxFade.Txt(this.Tx);
				bool flag2 = this.moveToRight(move_scale);
				this.Atl[1] = 20 * X.MPF(flag2);
				if (flag2)
				{
					if (this.slide_sound_right != "")
					{
						SND.Ui.play(this.slide_sound_right, false);
					}
				}
				else if (this.slide_sound_limit != "")
				{
					SND.Ui.play(this.slide_sound_limit, false);
				}
				this.Atl[0] = 0;
				return true;
			}
			return false;
		}

		public void deactivate()
		{
			if (this.Md != null)
			{
				this.Md.clear(false, false);
				this.Md.updateForMeshRenderer(false);
				this.TxFade.Alpha(0f);
			}
			this.Tx.Alpha(this.alpha_);
			Vector3 localPosition = this.Tx.transform.localPosition;
			localPosition.x = 0f;
			this.Tx.transform.localPosition = localPosition;
			if (CtSetter.ActiveSetter == this)
			{
				CtSetter.ActiveSetter = null;
				if (!this.auto_selected && this.quit_sound != "")
				{
					SND.Ui.play(this.quit_sound, false);
				}
			}
			this.auto_selected = false;
			this.t = -1f;
		}

		public void setAlpha(float v)
		{
			if (v == this.alpha_)
			{
				return;
			}
			this.alpha_ = v;
			if (!this.isActive())
			{
				this.Tx.Alpha(v);
			}
		}

		public static bool hasFocus()
		{
			return CtSetter.ActiveSetter != null && !CtSetter.ActiveSetter.auto_selected;
		}

		public bool isActive()
		{
			return this.t >= 0f;
		}

		public virtual string focus_curs_category
		{
			get
			{
				return "FOCUS";
			}
		}

		public abstract bool isEnableMoving();

		public float get_swidth_px()
		{
			return this.swidth;
		}

		public float get_sheight_px()
		{
			return this.Tx.get_sheight_px();
		}

		public void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
		}

		public void destruct()
		{
			if (this.Md != null)
			{
				this.Md.destruct();
			}
			if (this.Tx != null)
			{
				IN.DestroyOne(this.Gob);
			}
			this.Gob = null;
			this.Tx = null;
			this.TxFade = null;
		}

		protected abstract bool moveToLeft(float scale = 1f);

		protected abstract bool moveToRight(float scale = 1f);

		protected abstract void deactivateAndSelect();

		public string slide_sound_right = "toggle_button_open";

		public string slide_sound_left = "toggle_button_close";

		public string slide_sound_limit = "toggle_button_limit";

		public string init_sound = "tool_drag_init";

		public string quit_sound = "tool_drag_quit";

		public float updown_move_scale = 4f;

		private GameObject Gob;

		private TextRenderer Tx;

		private TextRenderer TxFade;

		private MeshDrawer Md;

		private int stencil_ref_ = -1;

		public const float DEF_SWIDTH = 114f;

		public const float DEF_SWIDTH_MIDDLE = 154f;

		public const float DEF_SWIDTH_LONG = 214f;

		public float swidth = 114f;

		public float t = -1f;

		public int[] Atl;

		public const int SLIDE_ANIM_T = 20;

		public float alpha_ = 1f;

		public float text_animate_f;

		public int arrow_margin = 30;

		public bool auto_deactivate_kettei = true;

		public bool lr_reverse;

		public bool auto_selected;

		private static CtSetter ActiveSetter;

		private Color32 Col;

		private ValotileRenderer Valot;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class MsgBox : MonoBehaviourAutoRun, IAlphaSetable, IDesignerBlock, IValotileSetable, IDesignerPosSetableBlock
	{
		protected virtual void Awake()
		{
			base.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
			this.MMRD = IN.GetOrAdd<MultiMeshRenderer>(base.gameObject);
			this.MMRD.base_z = 0.0015f;
			this.MMRD.slip_z = 0.0001f;
			this.visible = false;
		}

		public bool visible
		{
			get
			{
				return !this.destructed && base.gameObject.activeSelf;
			}
			set
			{
				base.gameObject.SetActive(value);
			}
		}

		public MsgBox margin(params float[] Aexpand)
		{
			Aexpand = X.expandArrayFine(Aexpand);
			this.lmarg = Aexpand[0];
			this.tmarg = Aexpand[1];
			this.rmarg = Aexpand[2];
			this.bmarg = Aexpand[3];
			return this;
		}

		public virtual MsgBox wh(float _w, float _h = -1f)
		{
			if (_h < 0f)
			{
				_h = _w;
			}
			if (_w >= 0f)
			{
				this.w = _w;
			}
			if (_h >= 0f)
			{
				this.h = _h;
			}
			this.redrawBg(0f, 0f, true);
			this.resetCollider();
			if ((this.Tx != null && this.alignx != ALIGN.CENTER) || this.aligny != ALIGNY.MIDDLE)
			{
				this.Align(this.alignx, this.aligny);
			}
			return this;
		}

		public MsgBox swh(float _w, float _h = -1f)
		{
			if (_h < 0f)
			{
				_h = _w;
			}
			return this.wh(X.Mx(_w - this.lmarg - this.rmarg, 0f), X.Mx(_h - this.tmarg - this.bmarg, 0f));
		}

		public virtual MsgBox wh_anim(float _w, float _h = -1f, bool anim_w = true, bool anim_h = true)
		{
			float swidth = this.swidth;
			float sheight = this.sheight;
			this.wh(_w, _h);
			this.smallen_scale_w = ((anim_w && this.swidth != 0f) ? (swidth / this.swidth) : 1f);
			this.smallen_scale_h = ((anim_h && this.sheight != 0f) ? (sheight / this.sheight) : 1f);
			this._bkg_scale |= 8U;
			this.t = ((this.t >= 0f) ? X.Mn(this.t, (float)this.delayt) : (-1f));
			return this;
		}

		public MsgBox swh_anim(float _w, float _h = -1f, bool anim_w = true, bool anim_h = true)
		{
			if (_h < 0f)
			{
				_h = _w;
			}
			this.wh_anim(X.Mx(_w - this.lmarg - this.rmarg, 0f), X.Mx(_h - this.tmarg - this.bmarg, 0f), anim_w, anim_h);
			return this;
		}

		public MsgBox scaling_alpha_set(bool when_active, bool when_deactivating)
		{
			if (when_active)
			{
				this._bkg_scale &= 4294967287U;
			}
			else
			{
				this._bkg_scale |= 8U;
			}
			if (when_deactivating)
			{
				this._bkg_scale &= 4294967279U;
			}
			else
			{
				this._bkg_scale |= 16U;
			}
			return this;
		}

		public MsgBox wh_animZero(bool anim_w = true, bool anim_h = true, float _value = 0f)
		{
			if (anim_w)
			{
				this.smallen_scale_w = _value;
			}
			if (anim_h)
			{
				this.smallen_scale_h = _value;
			}
			return this;
		}

		public float get_width()
		{
			return this.w;
		}

		public float get_height()
		{
			return this.h;
		}

		public float swidth
		{
			get
			{
				return this.w + this.lmarg + this.rmarg;
			}
		}

		public float sheight
		{
			get
			{
				return this.h + this.tmarg + this.bmarg;
			}
		}

		public virtual MsgBox col(Color32 C)
		{
			this.bg_col.Set(C);
			this.Agrd_col = null;
			return this;
		}

		public MsgBox col(uint _a)
		{
			return this.col(C32.d2c(_a));
		}

		public MsgBox col(C32 C)
		{
			return this.col(C.C);
		}

		public MsgBox TxCol(uint _a)
		{
			this.tx_col = C32.d2c(_a);
			if (this.Tx != null)
			{
				this.Tx.Col(this.tx_col);
				this.fineTextAlpha();
			}
			return this;
		}

		public MsgBox TxCol(Color32 C)
		{
			this.tx_col = C;
			if (this.Tx != null)
			{
				this.Tx.Col(this.tx_col);
				this.fineTextAlpha();
			}
			return this;
		}

		public MsgBox TxCol(C32 C)
		{
			this.tx_col = C.C;
			if (this.Tx != null)
			{
				this.Tx.Col(this.tx_col);
				this.fineTextAlpha();
			}
			return this;
		}

		public MsgBox TxSize(float f)
		{
			this.tx_size = f;
			if (this.Tx != null)
			{
				this.Tx.Size(f);
			}
			return this;
		}

		public MsgBox Align(ALIGN ax, ALIGNY ay)
		{
			this.alignx = ax;
			this.aligny = ay;
			if (this.Tx != null)
			{
				this.Tx.Align(ax).AlignY(ay);
				this.fineTxPos();
			}
			return this;
		}

		protected void fineTxPos()
		{
			this.fineTxPos(this.Tx, this.alignx, this.aligny);
		}

		protected void fineTxPos(TextRenderer Tx, ALIGN alignx, ALIGNY aligny)
		{
			if (Tx != null)
			{
				Vector3 localPosition = Tx.transform.localPosition;
				localPosition.x = (-this.swidth / 2f + this.lmarg + this.w / 2f + this.w * 0.5f * (float)((alignx == ALIGN.LEFT) ? (-1) : ((alignx == ALIGN.RIGHT) ? 1 : 0))) * 0.015625f;
				localPosition.y = (-this.sheight / 2f + this.bmarg + this.h / 2f + this.h * 0.5f * (float)((aligny == ALIGNY.BOTTOM) ? (-1) : ((aligny == ALIGNY.TOP) ? 1 : 0))) * 0.015625f;
				Tx.transform.localPosition = localPosition;
			}
		}

		public MsgBox LineSpacing(float l)
		{
			this.line_spacing = l;
			if (this.Tx != null)
			{
				this.Tx.LineSpacing(l);
			}
			return this;
		}

		public MsgBox AlignX(ALIGN a)
		{
			return this.Align(a, this.aligny);
		}

		public MsgBox AlignY(ALIGNY a)
		{
			return this.Align(this.alignx, a);
		}

		public MsgBox col_alp1(float _a)
		{
			this.bg_col.a = (byte)X.MMX(0f, 255f * _a, 255f);
			return this;
		}

		public virtual void setAlpha(float a)
		{
			if (this.alpha_ == a)
			{
				return;
			}
			this.alpha_ = a;
			this.fineTextAlpha();
			this.redrawBg(0f, 0f, true);
		}

		public MsgBox bkg_scale(bool _x = true, bool _y = false, bool do_not_use_zsin = false)
		{
			this._bkg_scale = (_x ? 1U : 0U) | (_y ? 2U : 0U) | (do_not_use_zsin ? 4U : 0U);
			return this;
		}

		public virtual MsgBox gradation(Color32[] Acol = null, float[] Alvls = null)
		{
			if (Acol != null)
			{
				int num = Acol.Length;
				if (num < 2)
				{
					return this;
				}
				if (Alvls == null)
				{
					Alvls = new float[num];
					for (int i = 0; i < num - 1; i++)
					{
						Alvls[i] = (float)((int)((float)i / (float)(num - 1) * 255f));
					}
					Alvls[num - 1] = 255f;
				}
				else
				{
					Alvls = X.concat<float>(Alvls, null, -1, -1);
				}
				this.Agrd_col = X.concat<Color32>(Acol, null, -1, -1);
				this.Agrd_lvl = Alvls;
			}
			else
			{
				this.Agrd_col = null;
				this.Agrd_lvl = null;
			}
			this.redrawBg(0f, 0f, true);
			return this;
		}

		public MsgBox ColliderMargin(float pixel = -1000f)
		{
			this.collider_margin_pixel = pixel;
			return this.resetCollider();
		}

		protected MsgBox resetCollider()
		{
			if (this.collider_margin_pixel == -1000f)
			{
				if (this.GobCld != null)
				{
					global::UnityEngine.Object.Destroy(this.GobCld);
					this.GobCld = null;
					this.Cld = null;
				}
				return this;
			}
			if (this.Cld == null)
			{
				this.GobCld = IN.CreateGob(this, "-collider");
				IN.setZ(this.GobCld.transform, 0.00015f);
				this.Cld = this.GobCld.AddComponent<BoxCollider2D>();
			}
			this.Cld.size = new Vector2((this.swidth + this.collider_margin_pixel * 2f) * 0.015625f, (this.sheight + this.collider_margin_pixel * 2f) * 0.015625f);
			return this;
		}

		public MsgBox position(float _sx, float _sy, float _dx = -1000f, float _dy = -1000f, bool no_reset_time = false)
		{
			Vector3 vector = base.transform.localPosition * 64f;
			if (this.t >= 0f)
			{
				if (!this.visible || this.alpha < 0.05f)
				{
					if (_dx != -1000f)
					{
						IN.PosP2(base.transform, vector.x = _sx, vector.y = _sy);
					}
				}
				else
				{
					if (_dx == -1000f)
					{
						_dx = (this.dx = _sx);
						_dy = (this.dy = _sy);
					}
					_sx = vector.x;
					_sy = vector.y;
				}
			}
			else if ((this.visible && this.alpha > 0.05f) || _dx == -1000f)
			{
				_dx = vector.x;
				_dy = vector.y;
			}
			if (_dx == -1000f)
			{
				_dx = (this.dx = _sx);
				_dy = (this.dy = _sy);
			}
			else
			{
				this.sx = _sx;
				this.dx = _dx;
				this.sy = _sy;
				this.dy = _dy;
			}
			if (!no_reset_time || this.post == -1)
			{
				this.post = 0;
			}
			if (no_reset_time && this.post >= this.maxt_pos)
			{
				this.post = this.maxt_pos;
			}
			return this;
		}

		public MsgBox posSetA(float _dx, float _dy, bool no_reset_time = false)
		{
			this.posSetA(-1000f, -1000f, _dx, _dy, no_reset_time);
			return this;
		}

		public void posSetA(float _sx, float _sy, float _dx, float _dy, bool no_reset_time = false)
		{
			if (_sx != -1000f)
			{
				this.sx = _sx;
			}
			if (_sy != -1000f)
			{
				this.sy = _sy;
			}
			if (_dx != -1000f)
			{
				this.dx = _dx;
			}
			if (_dy != -1000f)
			{
				this.dy = _dy;
			}
			if (!no_reset_time || this.post == -1)
			{
				this.post = 0;
			}
			if (no_reset_time && this.post >= this.maxt_pos)
			{
				this.post = this.maxt_pos;
			}
		}

		public MsgBox SetParent(Transform Trs, bool world_position_stays = true)
		{
			Vector3 vector = (world_position_stays ? base.transform.position : Vector3.zero);
			float z = base.transform.localPosition.z;
			base.transform.SetParent(Trs, world_position_stays);
			if (world_position_stays)
			{
				Vector3 vector2 = vector - base.transform.position;
				this.sx += vector2.x * 64f;
				this.sy += vector2.y * 64f;
				IN.setZ(base.transform, z);
			}
			return this;
		}

		public virtual MsgBox SetXyPx(float x, float y)
		{
			IN.PosP2(base.transform, x, y);
			return this;
		}

		public float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				this.setAlpha(value);
			}
		}

		public MsgBox set_depart_pos(float _dx, float _dy, bool force_use_anim = false)
		{
			if (!force_use_anim && this.visible && this.alpha >= 0.05f && this.post >= this.maxt_pos)
			{
				this.position(_dx, _dy, -1000f, -1000f, true);
			}
			else
			{
				this.position(_dx, _dy, -1000f, -1000f, false);
			}
			return this;
		}

		public MsgBox appear_time(int t)
		{
			this._appear_time = X.Mx(t, 1);
			return this;
		}

		public MsgBox position_max_time(int t, int t_hide = -1)
		{
			this.maxt_pos = X.Mx(t, 1);
			if (t_hide > 0)
			{
				this.maxt_pos_hide = X.Mx(t_hide, 1);
			}
			return this;
		}

		public MsgBox hideTime(int t)
		{
			this.t_hide = t;
			return this;
		}

		public virtual MsgBox make(string _tex)
		{
			if (_tex == null)
			{
				this.no_text = true;
			}
			bool flag = false;
			if (!this.ginitted && this.MdBkg == null)
			{
				this.MdBkg = this.MMRD.Make(this.bg_col.C, BLEND.NORMAL, null, null);
				this.GobBkg = this.MMRD.GetGob(this.MdBkg);
			}
			if (this.Tx == null || !this.ginitted)
			{
				if (!this.no_text)
				{
					if (this.Tx == null)
					{
						this.Tx = IN.CreateGob(this, "-Tx").AddComponent<TextRenderer>();
					}
					this.Tx.TargetFont = ((this.TargetFont_ == null) ? TX.getDefaultFont() : this.TargetFont_);
					IN.setZ(this.Tx.transform, -0.01f);
					this.Tx.Col(this.tx_col).Size(this.tx_size);
					this.Tx.auto_condense = true;
					this.Tx.html_mode = this.html_mode;
					this.Align(this.alignx, this.aligny).LineSpacing(this.line_spacing);
					this.Tx.Txt(_tex);
					flag = true;
					this.t = 0f;
				}
				else
				{
					if (this.Tx != null)
					{
						this.Tx.enabled = false;
					}
					if (!this.ginitted)
					{
						flag = true;
						this.t = 0f;
					}
				}
			}
			else
			{
				if (this.no_text)
				{
					this.Tx.enabled = false;
				}
				else
				{
					this.Tx.enabled = true;
					this.Tx.TargetFont = ((this.TargetFont_ == null) ? TX.getDefaultFont() : this.TargetFont_);
					this.Tx.html_mode = this.html_mode;
					this.Align(this.alignx, this.aligny).LineSpacing(this.line_spacing);
					this.Tx.Txt(_tex);
				}
				this.showt = 0;
				flag = !this.visible;
			}
			if (this.Tx != null)
			{
				this.Tx.use_valotile = this.use_valotile;
			}
			if (!this.ginitted)
			{
				if (!flag)
				{
					this.visible = false;
				}
				this.ginitted = true;
				this.redrawBg(0f, 0f, true);
			}
			if (flag)
			{
				this.t = 0f;
				this.showt = ((this.delayt > 0) ? 0 : (this.T_SHOW / 2));
				this.alpha = 0f;
				if (this.delayt > 0)
				{
					this.text_alpha = 0f;
				}
				this.visible = true;
				Vector3 localScale = this.GobBkg.transform.localScale;
				if ((this._bkg_scale & 1U) > 0U)
				{
					localScale.x = 0f;
				}
				else
				{
					localScale.x = 1f;
				}
				if ((this._bkg_scale & 2U) > 0U)
				{
					localScale.y = 0f;
				}
				else
				{
					localScale.y = 1f;
				}
				this.GobBkg.transform.localScale = localScale;
				if (this.post >= 0)
				{
					this.SetXyPx(this.sx, this.sy);
				}
				this.fineTextAlpha();
			}
			return this;
		}

		protected virtual MsgBox redrawBg(float w = 0f, float h = 0f, bool update_mrd = true)
		{
			if (!this.ginitted)
			{
				return this;
			}
			if (w <= 0f)
			{
				w = this.swidth;
			}
			if (h <= 0f)
			{
				h = this.sheight;
			}
			this.MdBkg.clear(false, false);
			this.MdBkg.Col = this.bg_col.C;
			if (this.use_grd)
			{
				this.MdBkg.Col.a = byte.MaxValue;
			}
			MdArranger mdArranger = this.MMRD.GetArranger(this.MdBkg).clear(this.MdBkg);
			this.redrawBgMeshInner(this.MdBkg, mdArranger, w, h);
			if (this.Tx != null && this.tx_border_bits > 0U && this.tx_border_thick > 0f)
			{
				float num = this.lmarg - w * 0.5f + w * 0.5f;
				float num2 = -(this.tmarg - h * 0.5f) - h * 0.5f;
				this.MdBkg.Col = C32.MulA(this.tx_border_col, 1f);
				for (byte b = 0; b < 4; b += 1)
				{
					if (((ulong)this.tx_border_bits & (ulong)(1L << (int)(b & 31))) > 0UL)
					{
						float num4;
						float num3;
						float num5;
						float num6;
						if (b == 0 || b == 2)
						{
							num3 = (num4 = (float)(b - 1) * (w * 0.5f + this.tx_border_padding) + num);
							num5 = h * 0.5f + this.tx_border_padding + num2;
							num6 = -num5;
						}
						else
						{
							num5 = (num6 = -((float)(b - 2) * (h * 0.5f + this.tx_border_padding) + 1f) + num2);
							num3 = w * 0.5f + this.tx_border_padding + num;
							num4 = -num3;
						}
						this.MdBkg.Line(num4, num6, num3, num5, this.tx_border_thick, false, 0f, 0f);
					}
				}
			}
			if (this.use_grd)
			{
				mdArranger.setColAllGrdation(-h / 2f, h / 2f, this.Agrd_col, this.Agrd_lvl, GRD.BOTTOM2TOP, false, this.alpha_, true);
			}
			else
			{
				mdArranger.mulAlpha(this.alpha_);
			}
			if (update_mrd)
			{
				this.MdBkg.updateForMeshRenderer(true);
			}
			return this;
		}

		protected virtual void redrawBgMeshInner(MeshDrawer Md, MdArranger Ma, float wpx, float hpx)
		{
			if (Ma != null)
			{
				Ma.Set(true);
			}
			Md.KadomaruRect(0f, 0f, wpx, hpx, (float)this.radius, 0f, false, 0f, 0f, false);
			if (Ma != null)
			{
				Ma.Set(false);
			}
			if (this.bg_border_thick > 0f)
			{
				Md.Col = this.bg_border_col;
				Md.KadomaruRect(0f, 0f, wpx, hpx, (float)this.radius, this.bg_border_thick, false, 0f, 0f, false);
			}
		}

		public MsgBox bg_border(Color32 col, float thick = 1f)
		{
			this.bg_border_thick = thick;
			this.bg_border_col = col;
			this.redrawBg(0f, 0f, true);
			return this;
		}

		public MsgBox tx_border(float padding, string aim, Color32 col, float lineWidth = 1f)
		{
			this.tx_border_bits = ((aim.IndexOf("L") >= 0) ? 1U : 0U) + ((aim.IndexOf("T") >= 0) ? 2U : 0U) + ((aim.IndexOf("R") >= 0) ? 4U : 0U) + ((aim.IndexOf("B") >= 0) ? 8U : 0U);
			this.tx_border_col = col;
			this.tx_border_padding = padding;
			this.tx_border_thick = lineWidth;
			this.redrawBg(0f, 0f, true);
			return this;
		}

		protected override bool runIRD(float fcnt)
		{
			return this.run(fcnt, false) != null;
		}

		public virtual MsgBox run(float fcnt, bool force_draw)
		{
			bool flag = false;
			bool flag2 = force_draw || X.D;
			if (this.visible)
			{
				float num = ((this.t >= 0f) ? (this.t - (float)this.delayt) : this.t);
				if (flag2 && this.Tx != null && (num >= 0f || (this.t < 0f && this.showt > 0)))
				{
					bool flag3 = this.showt <= this.T_SHOW;
					this.showt += X.AF;
					this.text_alpha = ((this.T_SHOW <= 0) ? 1f : (0.4f + 0.6f * X.ZLINE((float)this.showt, (float)this.T_SHOW)));
					flag = true;
				}
				if (this.t >= 0f)
				{
					bool flag3 = this.t == 0f;
					this.t += fcnt;
					if (num > 0f)
					{
						if (this.post >= 0 && flag2)
						{
							flag3 = this.post <= this.maxt_pos;
							this.post += X.AF;
						}
						if ((num <= (float)(this._appear_time + 5) || force_draw) && flag2)
						{
							this.setBkgScale(X.ZLINE(num, (float)this._appear_time));
							flag = false;
						}
					}
					else
					{
						if (this.text_alpha != 0f)
						{
							this.text_alpha = 0f;
							flag = true;
						}
						if (this.t - (float)this.delayt > 0f)
						{
							this.showt = this.T_SHOW / 2;
						}
					}
					if (flag3)
					{
						this.calcPosition(-1f);
					}
				}
				else
				{
					this.t -= fcnt;
					float num2 = X.ZLINE(1f + this.t / (float)this.t_hide);
					if (flag2)
					{
						this.setBkgScale(num2);
						flag = false;
					}
					if (this.post >= 0 && flag2)
					{
						bool flag3 = this.post <= this.maxt_pos_hide;
						this.post += X.AF;
						if (flag3)
						{
							this.calcPosition(-1f);
						}
					}
					if (this.t <= (float)(-(float)this.t_hide))
					{
						this.visible = false;
						this.alpha = 0f;
						return null;
					}
				}
				if (flag)
				{
					this.fineTextAlpha();
				}
				return this;
			}
			if (this.t > (float)(-(float)this.t_hide))
			{
				return this;
			}
			return null;
		}

		protected virtual void calcPosition(float tzs = -1f)
		{
			if (tzs < 0f)
			{
				if (this.t >= 0f)
				{
					tzs = X.ZSIN2((float)this.post, (float)this.maxt_pos);
				}
				else
				{
					tzs = 1f - X.ZSIN((float)this.post, (float)this.maxt_pos_hide);
				}
			}
			this.SetXyPx(X.NAIBUN_I(this.sx, this.dx, tzs), X.NAIBUN_I(this.sy, this.dy, tzs));
		}

		public MsgBox fineMove()
		{
			if (this.GobBkg == null)
			{
				return this;
			}
			Vector3 localScale = this.GobBkg.transform.localScale;
			float num;
			if (this.t >= 0f)
			{
				num = 1f;
				this.t = (float)(this._appear_time + 50 + this.delayt);
				localScale.x = (localScale.y = 1f);
			}
			else
			{
				this.t = (float)(-(float)this.t_hide);
				num = 0f;
				if ((this._bkg_scale & 1U) > 0U)
				{
					localScale.x = 0f;
				}
				if ((this._bkg_scale & 2U) > 0U)
				{
					localScale.y = 0f;
				}
			}
			this.GobBkg.transform.localScale = localScale;
			if (this.post >= 0)
			{
				if (this.t >= 0f)
				{
					this.SetXyPx(this.dx, this.dy);
					this.post = this.maxt_pos;
				}
				else
				{
					this.SetXyPx(this.sx, this.sy);
					this.post = this.maxt_pos_hide;
				}
			}
			if (this.showt >= 1)
			{
				this.showt = this.T_SHOW + 1;
				if (this.text_alpha != 1f)
				{
					this.text_alpha = 1f;
					this.fineTextAlpha();
				}
			}
			this.alpha = num;
			return this;
		}

		protected virtual MsgBox fineTextAlpha()
		{
			if (this.Tx != null)
			{
				this.Tx.Alpha(this.text_alpha * this.alpha_);
			}
			return this;
		}

		protected virtual MsgBox setBkgScale(float tz)
		{
			Vector3 localScale = this.GobBkg.transform.localScale;
			this.setAlpha((this.isActive() ? ((this._bkg_scale & 8U) > 0U) : ((this._bkg_scale & 16U) > 0U)) ? 1f : X.MMX(0f, tz, 1f));
			if ((this._bkg_scale & 4U) > 0U)
			{
				if ((this._bkg_scale & 1U) > 0U)
				{
					localScale.x = X.NI(this.smallen_scale_w, 1f, tz);
				}
				if ((this._bkg_scale & 2U) > 0U)
				{
					localScale.y = X.NI(this.smallen_scale_h, 1f, tz);
				}
			}
			else
			{
				float num = ((this._bkg_scale != 0U) ? X.ZSIN(tz) : tz);
				if ((this._bkg_scale & 1U) > 0U)
				{
					localScale.x = X.NI(this.smallen_scale_w, 1f, X.ZSIN(num));
				}
				if ((this._bkg_scale & 2U) > 0U)
				{
					localScale.y = X.NI(this.smallen_scale_h, 1f, X.ZSIN(num));
				}
			}
			this.GobBkg.transform.localScale = localScale;
			this.redrawBg(0f, 0f, true);
			this.fineTextAlpha();
			return this;
		}

		public string getText()
		{
			if (this.Tx == null)
			{
				return "";
			}
			return this.Tx.text_content;
		}

		public bool isActive()
		{
			return !this.destructed && this.visible && this.t >= 0f;
		}

		public bool isHiding()
		{
			return this.t < 0f;
		}

		public bool isMoving()
		{
			return this.post < ((this.t >= 0f) ? this.maxt_pos : this.maxt_pos_hide);
		}

		public bool use_grd
		{
			get
			{
				return this.Agrd_col != null;
			}
		}

		public bool show_delaying
		{
			get
			{
				return X.BTW(0f, this.t, (float)this.delayt);
			}
		}

		public virtual MsgBox activate()
		{
			if (!this.isActive())
			{
				this.t = 0f;
				this.showt = 1;
				this.post = ((this.post >= 0) ? 0 : (-1));
				this.text_alpha = 0.001f;
				this.alpha = 0f;
				this.visible = true;
				IN.clearCursDown();
				this.use_valotile = this.use_valotile;
				this.run(0f, true);
			}
			return this;
		}

		public virtual MsgBox deactivate()
		{
			if (this.destructed)
			{
				return this;
			}
			if (this.post >= 0 && this.t >= 0f)
			{
				this.post = 0;
			}
			bool flag = this.isActive();
			this.t = X.Mn(this.t, -1f);
			if (this.deactivating_set_sx_shift != -1000f)
			{
				this.sx = this.dx + this.deactivating_set_sx_shift;
			}
			if (this.deactivating_set_sy_shift != -1000f)
			{
				this.sy = this.dy + this.deactivating_set_sy_shift;
			}
			if (flag)
			{
				if (this.alpha_ <= 0.01f)
				{
					this.fineMove();
				}
				else
				{
					this.calcPosition(-1f);
				}
			}
			return this;
		}

		public override void destruct()
		{
			if (this.t >= 0f)
			{
				this.deactivate();
			}
			if (!this.destructed)
			{
				this.destructed = true;
				IN.DestroyOne(base.gameObject);
			}
			this.Tx = null;
			base.destruct();
		}

		public override void OnDestroy()
		{
			this.destruct();
		}

		public bool isShown()
		{
			return this.t >= (float)this.delayt || X.BTW((float)(-(float)this.t_hide + 1), this.t, 0f);
		}

		public bool show_animating
		{
			get
			{
				return this.t >= 0f && this.t - (float)this.delayt < (float)(this._appear_time + 6);
			}
		}

		public float get_deperture_x()
		{
			return this.dx;
		}

		public float get_deperture_y()
		{
			return this.dy;
		}

		public void tradeBoxPosition(MsgBox Another, bool no_reset_time = false)
		{
			float num = this.sx;
			float num2 = this.sy;
			float num3 = this.dx;
			float num4 = this.dy;
			Vector3 localPosition = base.transform.localPosition;
			Vector3 localPosition2 = Another.transform.localPosition;
			base.transform.localPosition = new Vector3(localPosition2.x, localPosition2.y, localPosition.z);
			Another.transform.localPosition = new Vector3(localPosition.x, localPosition.y, localPosition2.z);
			this.posSetA(Another.sx, Another.sy, Another.dx, Another.dy, no_reset_time);
			Another.posSetA(num, num2, num3, num4, no_reset_time);
		}

		public float showing_alpha
		{
			get
			{
				return this.alpha_ * this.text_alpha;
			}
		}

		public MFont TargetFont
		{
			get
			{
				return this.TargetFont_;
			}
			set
			{
				this.TargetFont_ = value;
				if (this.Tx != null)
				{
					this.Tx.TargetFont = ((value == null) ? TX.getDefaultFont() : value);
				}
			}
		}

		public bool textIs(string _txt)
		{
			return this.Tx != null && this.Tx.textIs(_txt);
		}

		public virtual bool use_valotile
		{
			get
			{
				return this.MMRD != null && this.MMRD.use_valotile && this.MMRD.valotile_enabled;
			}
			set
			{
				ValotileRenderer.RecheckUse(this.MMRD, value);
				if (this.Tx != null)
				{
					this.Tx.use_valotile = value;
				}
			}
		}

		public float get_swidth_px()
		{
			return this.swidth;
		}

		public float get_text_swidth_px()
		{
			if (!(this.Tx != null))
			{
				return 0f;
			}
			return this.Tx.get_swidth_px();
		}

		public float get_sheight_px()
		{
			return this.sheight;
		}

		public Transform getTransform()
		{
			return base.transform;
		}

		public void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
		}

		public bool didGraphicInit()
		{
			return this.ginitted;
		}

		public static int DEF_RADIUS = 24;

		public int T_SHOW = 18;

		public const int T_HIDE = 18;

		public const int T_POS = 12;

		public const int T_POS_HIDE = 20;

		public const int T_BASEAPPEAR = 26;

		protected float sx;

		protected float sy;

		protected float dx;

		protected float dy;

		public float deactivating_set_sx_shift = -1000f;

		public float deactivating_set_sy_shift = -1000f;

		protected int post = -1;

		protected int _appear_time = 26;

		protected int maxt_pos = 12;

		protected int maxt_pos_hide = 20;

		protected float w = 300f;

		protected float h = 300f;

		public float lmarg;

		public float tmarg;

		public float rmarg;

		public float bmarg;

		protected float collider_margin_pixel = -1000f;

		protected GameObject GobCld;

		protected BoxCollider2D Cld;

		protected float alpha_;

		protected ALIGN alignx;

		protected ALIGNY aligny;

		protected C32 bg_col = new C32(MTRX.ColMenu);

		protected int radius = MsgBox.DEF_RADIUS;

		public bool destructed;

		private float bg_border_thick;

		private Color32 bg_border_col = MTRX.ColWhite;

		private float tx_border_padding;

		private float tx_border_thick;

		private uint tx_border_bits;

		private Color32 tx_border_col = MTRX.ColWhite;

		protected Color32[] Agrd_col;

		protected float[] Agrd_lvl;

		private Color32 tx_col = MTRX.ColWhite;

		private float tx_size = 18f;

		private float line_spacing = 1.5f;

		private MFont TargetFont_;

		public bool html_mode;

		public bool no_text;

		protected TextRenderer Tx;

		protected bool ginitted;

		protected MultiMeshRenderer MMRD;

		protected MeshDrawer MdBkg;

		protected GameObject GobBkg;

		protected MeshDrawer MdBorder;

		public int delayt;

		protected int showt;

		protected int t_hide = 18;

		protected float t = -18f;

		protected uint _bkg_scale;

		protected const uint BSCALE_X = 1U;

		protected const uint BSCALE_Y = 2U;

		protected const uint BSCALE_DO_NOT_USE_ZSIN = 4U;

		protected const uint BSCALE_DO_NOT_ALP = 8U;

		protected const uint BSCALE_DO_NOT_ALP_DEACTIVATING = 16U;

		protected const uint BSCALE_FLG0 = 32U;

		protected const uint BSCALE_FLG1 = 64U;

		protected const uint BSCALE_FLG2 = 128U;

		protected const uint BSCALE__A_FLGS = 224U;

		public float smallen_scale_w;

		public float smallen_scale_h;

		protected float text_alpha = 1f;

		protected const float mmrd_base_z = 0.0015f;
	}
}

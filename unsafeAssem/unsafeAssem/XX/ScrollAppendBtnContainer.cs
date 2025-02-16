using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class ScrollAppendBtnContainer<T> : ScrollAppend, IAlphaSetable, IDesignerBlock, IPauseable where T : aBtn
	{
		public ScrollAppendBtnContainer(ScrollAppend Base, string _name, GameObject BaseGob, BtnContainer<T> _BCon)
			: base(Base)
		{
			this.name = _name;
			this.Scb = IN.CreateGob(BaseGob, "-Scb-for-" + this.name).AddComponent<ScrollBox>();
			this.Scb.stencil_ref = this.stencil_ref;
			float num = this.out_w_ - this.in_margin_w * 2f;
			float num2 = this.out_h_ - this.in_margin_h * 2f;
			this.Scb.WHAllPx(num, num2, num, num2);
			this.Scb.scrollbar_shift_z = -0.05f;
			this.BCon = _BCon;
			this.BCon.stencil_ref = this.stencil_ref;
			this.BCon.BelongScroll = this.Scb;
			this.BCon.OuterScrollBox = this;
			this.BCon.getGob().transform.SetParent(this.Scb.getViewArea().transform, false);
		}

		public override float out_w
		{
			set
			{
				if (this.out_w == value)
				{
					return;
				}
				this.out_w_ = value;
				this.Scb.WHAllPx(this.out_w_ - this.in_margin_w * 2f, this.out_h_ - this.in_margin_h * 2f, -1f, -1f);
			}
		}

		public override float out_h
		{
			set
			{
				if (this.out_h == value)
				{
					return;
				}
				this.out_h_ = value;
				this.Scb.WHAllPx(this.out_w_ - this.in_margin_w * 2f, this.out_h_ - this.in_margin_h * 2f, -1f, -1f);
			}
		}

		public float get_swidth_px()
		{
			return this.out_w;
		}

		public float get_sheight_px()
		{
			return this.out_h;
		}

		public float useable_max_w
		{
			get
			{
				return this.out_w - this.in_margin_w * 2f - 2f - 10f;
			}
		}

		public void setAlpha(float a)
		{
			this.BCon.setAlpha(a);
		}

		public void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
			this.BCon.AddSelectableItems(ASlc, only_front);
		}

		public BtnContainerRunner getRunner()
		{
			return this.BCon.getRunner();
		}

		public Transform getTransform()
		{
			return this.Scb.getTransform();
		}

		public ScrollBox getScrollBox()
		{
			return this.Scb;
		}

		public void Pause()
		{
			this.BCon.Pause();
		}

		public void Resume()
		{
			this.BCon.Resume();
		}

		public uint bgcol
		{
			get
			{
				if (!(this.Scb != null))
				{
					return this.Scb.background_color;
				}
				return 0U;
			}
			set
			{
				if (this.Scb == null)
				{
					return;
				}
				this.Scb.background_color = value;
				this.Scb.prepareBackground();
			}
		}

		public void reposition(bool fine_navi = true)
		{
			int num = ((this.clms > 0) ? this.clms : X.Mx(1, X.IntR(this.out_w / this.item_w)));
			int length = this.BCon.Length;
			if (length == 0)
			{
				return;
			}
			float num2 = 0f;
			int num3 = 0;
			float num4 = 0f;
			if (this.item_h == -1000f)
			{
				for (int i = 0; i < length; i++)
				{
					T t = this.BCon.Get(i);
					num4 = X.Mx(num4, t.get_sheight_px());
					if (++num3 >= num)
					{
						num3 = 0;
						num2 += num4;
						num4 = 0f;
					}
				}
			}
			float num5 = ((this.item_h == 0f) ? this.BCon.Get(0).get_sheight_px() : this.item_h);
			T t2 = default(T);
			int num6 = 0;
			int num7 = 0;
			for (int j = 0; j < length; j++)
			{
				T t3 = this.BCon.Get(j);
				int num8 = ((num2 == 0f) ? t3.carr_index : j);
				int num9 = num8 % num;
				int num10 = num8 / num;
				num6 = X.Mx(num9 + 1, num6);
				num7 = X.Mx(num10 + 1, num7);
			}
			float num11 = this.out_w - this.in_margin_w * 2f;
			float num12 = this.out_h - this.in_margin_h * 2f;
			float num13 = X.Mx(num11, (float)num6 * this.item_w);
			float num14 = ((num2 > 0f) ? num2 : X.Mx(num12, (float)num7 * num5));
			this.Scb.WHAllPx(num11, num12, num13, num14);
			IN.PosP2(this.BCon.getGob().transform, 0f, 0f);
			aBtn aBtn = null;
			bool flag = (this.BCon.navi_loop & 1) != 0 && this.lr_slide_row > 0;
			float num15 = 0f;
			num4 = 0f;
			num3 = 0;
			for (int k = 0; k < length; k++)
			{
				T t4 = this.BCon.Get(k);
				if (k == 0)
				{
					aBtn = t4;
				}
				object obj = ((num2 == 0f) ? t4.carr_index : k);
				t4.BelongScroll = this.Scb;
				object obj2 = obj;
				int num16 = obj2 % num;
				int num17 = obj2 / num;
				float num18;
				if (num2 == 0f)
				{
					num18 = -((float)num17 + 0.5f) * num5;
				}
				else
				{
					float sheight_px = t4.get_sheight_px();
					num18 = -num15 - sheight_px * 0.5f;
					num4 = X.Mx(num4, sheight_px);
				}
				IN.PosP2(t4.transform, ((float)num16 + 0.5f) * this.item_w, num18);
				if (fine_navi && t2 != null)
				{
					t2.setNaviB(t4, true, true);
				}
				t2 = t4;
				if (fine_navi && flag && k > 0)
				{
					t4.setNaviL(this.BCon.Get(X.Mx(0, k - this.lr_slide_row)), true, false);
				}
				if (num2 > 0f && ++num3 >= num)
				{
					num3 = 0;
					num15 += num4;
					num4 = 0f;
				}
			}
			if (fine_navi)
			{
				if ((this.BCon.navi_loop & 2) != 0)
				{
					aBtn.setNaviT(t2, true, true);
				}
				if (flag)
				{
					aBtn.setNaviL(aBtn, false, true);
					t2.setNaviR(t2, false, true);
				}
			}
		}

		public float item_w;

		public float item_h;

		public int clms;

		public readonly string name;

		private BtnContainer<T> BCon;

		private ScrollBox Scb;

		private bool need_reposit_flag;
	}
}

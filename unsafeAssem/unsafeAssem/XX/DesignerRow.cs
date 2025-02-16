using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class DesignerRow : IDesignerBlock
	{
		public DesignerRow(GameObject _Gob, float _bounds_w_px = 0f)
		{
			this.Gob = IN.CreateGob(_Gob, "-DesignerRow");
			this.Trs = this.Gob.transform;
			this.bounds_w_px = _bounds_w_px;
		}

		public DesignerRow Base(float _base_x, float _base_y)
		{
			IN.Pos(this.Gob, _base_x, _base_y, this.d_base_z);
			return this;
		}

		public DesignerRow Base(Vector2 _base)
		{
			IN.Pos(this.Gob, _base.x, _base.y, this.d_base_z);
			return this;
		}

		public DesignerRow BasePx(float _base_x, float _base_y)
		{
			IN.PosP(this.Gob, _base_x, _base_y, this.d_base_z);
			return this;
		}

		public virtual DesignerRow Clear(bool destruct_block = false)
		{
			this.row_w = (this.row_h = (this.putted_w = (this.putted_h = 0f)));
			this.row_w_marg = 0f;
			return this;
		}

		public DesignerRow Add(IDesignerBlock Blk, float swidth, float sheight)
		{
			float num = this.row_w + this.row_w_marg + ((this.row_w > 0f) ? this.margin_x_px : 0f);
			if (num > 0f && this.bounds_w_px - num < swidth)
			{
				this.Br(false);
				num = 0f;
			}
			Transform transform = Blk.getTransform();
			if (transform == null)
			{
				X.de("Blkが破壊されています: " + Blk.ToString(), null);
				return this;
			}
			transform.SetParent(this.Trs, false);
			IN.PosP2(transform, num + swidth / 2f, -this.putted_h - ((this.putted_h > 0f) ? this.margin_y_px : 0f) - sheight / 2f);
			this.row_w = num + swidth;
			this.row_h = X.Mx(this.row_h, sheight);
			this.row_w_marg = 0f;
			return this;
		}

		public DesignerRow Add(IDesignerBlock Blk)
		{
			return this.Add(Blk, Blk.get_swidth_px(), Blk.get_sheight_px());
		}

		public virtual DesignerRow XSh(float x)
		{
			this.row_w_marg = x;
			return this;
		}

		public virtual DesignerRow Br(bool manual_br = true)
		{
			if (this.row_w <= 0f)
			{
				return this;
			}
			this.putted_w = X.Mx(this.row_w, this.putted_w);
			this.putted_h += ((this.putted_h > 0f) ? this.margin_y_px : 0f) + this.row_h;
			this.row_h = 0f;
			this.row_w = 0f;
			this.row_w_marg = 0f;
			return this;
		}

		public Transform getTransform()
		{
			return this.Gob.transform;
		}

		public void fineBaseZ(int tab_level = 0)
		{
			this.d_base_z = ((tab_level == 0) ? (-0.001f) : (0.001f / (float)(tab_level + 1) - 0.001f / (float)tab_level));
		}

		public void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
		}

		public float use_w
		{
			get
			{
				return this.bounds_w_px - this.row_w - this.row_w_marg - ((this.row_w > 0f) ? this.margin_x_px : 0f);
			}
		}

		public bool row_assigned
		{
			get
			{
				return this.row_w > 0f;
			}
		}

		public bool has_xsh
		{
			get
			{
				return this.row_w_marg > 0f;
			}
		}

		public float get_swidth_px()
		{
			return X.Mx(this.row_w, this.putted_w);
		}

		public float get_sheight_px()
		{
			return this.row_h + ((this.putted_h > 0f) ? (this.margin_y_px + this.putted_h) : 0f);
		}

		public float current_row_height
		{
			get
			{
				return this.row_h;
			}
		}

		private GameObject Gob;

		private Transform Trs;

		public float margin_x_px = 2f;

		public float margin_y_px = 2f;

		public float bounds_w_px;

		protected float row_w;

		protected float row_h;

		protected float row_w_marg;

		private float putted_h;

		private float putted_w;

		public float d_base_z = -0.0001f;

		protected class DesignerRowVariable
		{
			public void Push(DesignerRow V)
			{
				this.row_w = V.row_w;
				this.row_h = V.row_h;
				this.row_w_marg = V.row_w_marg;
				this.putted_w = V.putted_w;
				this.putted_h = V.putted_h;
			}

			public void Pop(DesignerRow V)
			{
				V.row_w = this.row_w;
				V.row_h = this.row_h;
				V.row_w_marg = this.row_w_marg;
				V.putted_w = this.putted_w;
				V.putted_h = this.putted_h;
			}

			public float row_w;

			public float row_h;

			public float row_w_marg;

			public float putted_h;

			public float putted_w;
		}
	}
}

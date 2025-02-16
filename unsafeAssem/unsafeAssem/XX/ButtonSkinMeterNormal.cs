using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class ButtonSkinMeterNormal : ButtonSkin
	{
		public ButtonSkinMeterNormal(aBtnMeter _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.Meter = _B;
			this.Md = base.makeMesh(null);
			this.MdStripeB = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshDashLine.shader, base.container_stencil_ref));
			this.initCircleRenderer();
			this.w = ((_w > 0f) ? _w : 64f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 22f) * 0.015625f;
			this.fine_continue_flags |= 5U;
			this.FD_getBtnMeterLineDefault = new FnBtnMeterLine(this.getBtnMeterLineDefault);
			this.curs_level_x = 0.8f;
			this.curs_level_y = -0.7f;
		}

		protected virtual void initCircleRenderer()
		{
			PxlSequence sqImgSliderArrow = MTRX.SqImgSliderArrow;
			this.MdCircle = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
		}

		public float getBtnMeterLineDefault(aBtnMeter B, int index, float val)
		{
			if (index != 0 && (float)index != B.valcnt)
			{
				return 0.5f;
			}
			return 1f;
		}

		protected virtual void redrawMemori()
		{
			float num = this.h * 64f;
			float num2 = this.moveable_width * 64f;
			float meter_x = this.meter_x;
			for (int i = 0; i < 2; i++)
			{
				if (i == 0)
				{
					this.Md.Col = this.Md.ColGrd.Set(2952790016U).mulA(this.alpha_).C;
				}
				else
				{
					this.Md.Col = this.Md.ColGrd.Set(uint.MaxValue).mulA(this.alpha_).C;
				}
				float num3 = -(num / 2f) + (float)((i == 0) ? 2 : 0);
				this.Md.RectBL(meter_x - (float)((i == 0) ? 1 : 0), -num / 2f - (float)((i == 0) ? 1 : 0), num2 + (float)((i == 0) ? 3 : 0), (float)((i == 0) ? 3 : 1), false);
				int num4 = 0;
				while ((float)num4 <= this.Meter.valcnt)
				{
					if (this.Amemori[num4] != 0f)
					{
						this.Md.RectBL(meter_x + (float)X.IntR(num2 * (float)num4 / this.Meter.valcnt) - (float)((i == 0) ? 1 : 0), num3, (float)((i == 0) ? 3 : 1), (float)(X.IntR(num * this.Amemori[num4]) + ((i == 0) ? (-1) : 0)), false);
					}
					num4++;
				}
			}
			if ((base.isFocused() && !base.isPushed()) || (!base.isFocused() && base.isPushed()))
			{
				this.MdStripeB.Col = this.MdStripeB.ColGrd.Set(4290503423U).mulA(this.alpha_).C;
				this.MdStripeB.RectDashedM(0f, 0f, this.swidth + 8f, num + 6f, X.IntC((this.swidth + num) / 8f), 1f, 0.5f, false, false);
			}
			this.Md.updateForMeshRenderer(false);
			this.MdStripeB.updateForMeshRenderer(false);
		}

		protected override Material getTitleStringChrMaterial(BLEND blnd, BMListChars Chr, MeshDrawer Md)
		{
			Material titleStringChrMaterial = base.getTitleStringChrMaterial(blnd, Chr, Md);
			titleStringChrMaterial.SetColor("_Color", C32.d2c(2569808426U));
			Md.Col = MTRX.ColWhite;
			return titleStringChrMaterial;
		}

		protected virtual void redrawMarker()
		{
			this.MdCircle.Col = this.MdCircle.ColGrd.Set(uint.MaxValue).mulA(this.alpha_).C;
			float moveable_width = this.moveable_width;
			float currentIndex = this.Meter.getCurrentIndex();
			PxlImage image = MTRX.SqImgSliderArrow.getImage(base.isPushed() ? 2 : (base.isFocused() ? 1 : 0), 0);
			this.MdCircle.initForImg(image, 0).DrawScaleGraph(this.meter_x + (moveable_width * 64f * currentIndex / this.Meter.valcnt + 0.4f), this.h * 64f * 0.4f, 1f, 1f, null);
			this.MdCircle.updateForMeshRenderer(false);
		}

		public void clearMemoriCache()
		{
			this.Amemori = null;
		}

		public override ButtonSkin Fine()
		{
			this.Md.clear(false, false);
			if (this.alpha == 0f)
			{
				return this;
			}
			if (this.Amemori == null)
			{
				this.Amemori = new float[X.IntC(this.Meter.valcnt) + 1];
				FnBtnMeterLine fnBtnMeterLine = ((this.Meter.fnBtnMeterLine == null) ? this.FD_getBtnMeterLineDefault : this.Meter.fnBtnMeterLine);
				for (int i = 0; i < this.Amemori.Length; i++)
				{
					this.Amemori[i] = fnBtnMeterLine(this.Meter, i, this.Meter.indexToValue((float)i));
				}
			}
			this.redrawMemori();
			this.redrawMarker();
			if (this.MdSpr != null)
			{
				this.MdSpr.Col = MTRX.ColWhite;
				if (base.isHoveredOrPushOut())
				{
					this.MdSpr.Col = C32.d2c(4290503423U);
				}
				this.MdSpr.Col.a = (byte)((float)this.MdSpr.Col.a * this.alpha_);
				this.DrawTitleSprite(this.MdSpr, (-this.swidth / 2f + this.meter_x) / 2f, 0f, 1f);
				this.MdSpr.updateForMeshRenderer(false);
			}
			return base.Fine();
		}

		public override bool canClickable(Vector2 PosU)
		{
			float num = X.Mx(34f, this.h * 64f);
			if (this.Meter.active_drag)
			{
				if (this.vertical)
				{
					return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), num * 0.015625f, this.swidth * 0.015625f);
				}
				return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), this.swidth * 0.015625f, num * 0.015625f);
			}
			else
			{
				float num2 = this.swidth + this.Meter.get_setter_swidth();
				Vector2 vector = (this.vertical ? new Vector2(0f, 1f) : new Vector2(1f, 0f)) * (-this.swidth * 0.5f + num2 * 0.5f);
				PosU += vector;
				if (this.vertical)
				{
					return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), num * 0.015625f, num2 * 0.015625f);
				}
				return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), num2 * 0.015625f, num * 0.015625f);
			}
		}

		public override ButtonSkin setTitle(string str)
		{
			base.setTitle(str);
			this.fine_flag = true;
			if (!this.do_not_write_default_title)
			{
				this.makeDefaultTitleString(str, ref this.MdSpr, BLEND._MAX);
			}
			return this;
		}

		public bool pointDownFirstJump(ref float curind, Vector2 PosLeft, Vector2 FirstVec)
		{
			Vector2 vector = this.getGlobalPtrLeftPos() - PosLeft;
			float indexDragCarried = this.Meter.getIndexDragCarried(vector, false);
			float indexDragCarried2 = this.Meter.getIndexDragCarried(this.getGlobalPtrRightPos() - PosLeft, false);
			float num = (indexDragCarried2 - indexDragCarried) / 2f;
			float num2 = this.Meter.getIndexDragCarried(FirstVec, false);
			if (num2 >= indexDragCarried2)
			{
				num2 -= indexDragCarried2 - indexDragCarried;
			}
			else if (num2 >= indexDragCarried)
			{
				return false;
			}
			num2 = X.MMX(0f, num2, this.Meter.get_valrange());
			float num3 = this.Meter.PosLRmagnitudeDivCount * 64f;
			float num4 = 12f / num3;
			return X.Abs(curind - num2) > num4;
		}

		public void setCursLevelToMeterPos()
		{
			float num = this.Meter.indexToLevel(this.Meter.getCurrentIndex());
			if (this.vertical)
			{
				this.curs_level_x = 0.15f;
				this.curs_level_y = (1f - num * 2f) * ((this.w - this.Meter.px_bar_width) / this.w);
			}
			else
			{
				this.curs_level_y = 0.15f;
				this.curs_level_x = (-1f + num * 2f) * ((this.w - this.Meter.px_bar_width) / this.w);
			}
			CURS.focusOnBtn(this.B, this.B.isHovered());
		}

		public Quaternion getRotationQ(bool include_transform_rot = false)
		{
			if (include_transform_rot)
			{
				if (!this.vertical)
				{
					return this.Gob.transform.localRotation;
				}
				return Quaternion.Euler(0f, 0f, -90f) * this.Gob.transform.localRotation;
			}
			else
			{
				if (!this.vertical)
				{
					return Quaternion.identity;
				}
				return Quaternion.Euler(0f, 0f, -90f);
			}
		}

		public Vector2 getGlobalLeftPos()
		{
			return this.Gob.transform.TransformPoint(this.getRotationQ(false) * new Vector3(this.meter_x * 0.015625f, 0f, 0f));
		}

		public Vector2 getGlobalPtrLeftPos()
		{
			return this.Gob.transform.TransformPoint(this.getRotationQ(false) * new Vector3(this.meter_x * 0.015625f + this.moveable_width * this.Meter.getCurrentIndex() / this.Meter.valcnt, 0f, 0f));
		}

		public Vector2 getGlobalPtrRightPos()
		{
			return this.Gob.transform.TransformPoint(this.getRotationQ(false) * new Vector3(this.meter_x * 0.015625f + this.moveable_width * this.Meter.getCurrentIndex() / this.Meter.valcnt + this.Meter.px_bar_width * 0.015625f, 0f, 0f));
		}

		public Vector2 getGlobalPtrCenterPos()
		{
			return this.Gob.transform.TransformPoint(this.getRotationQ(false) * new Vector3(this.meter_x * 0.015625f + this.moveable_width * this.Meter.getCurrentIndex() / this.Meter.valcnt + this.Meter.px_bar_width / 2f * 0.015625f, 0f, 0f));
		}

		public Vector2 getGlobalRightPos()
		{
			return this.Gob.transform.TransformPoint(this.getRotationQ(false) * new Vector3(this.swidth * 0.015625f / 2f - this.Meter.px_bar_width * 0.015625f, 0f, 0f));
		}

		public Vector2 getHousenVec()
		{
			return this.getRotationQ(true) * new Vector3(0f, X.Mx(0.5f, this.h), 0f);
		}

		public Vector2 getGlobalPos(Vector3 Base)
		{
			return this.Gob.transform.TransformPoint(Base);
		}

		public float moveable_width
		{
			get
			{
				return this.w - this.Meter.px_bar_width * 0.015625f;
			}
		}

		public override float swidth
		{
			get
			{
				return this.w * 64f + ((this.default_title_width > 0f) ? (this.default_title_width + 8f) : this.default_title_width);
			}
		}

		public float meter_x
		{
			get
			{
				return -this.swidth / 2f + ((this.default_title_width > 0f) ? (this.default_title_width + 8f) : this.default_title_width);
			}
		}

		public bool vertical
		{
			get
			{
				return this.Meter.vertical;
			}
		}

		protected MeshDrawer Md;

		protected MeshDrawer MdCircle;

		protected MeshDrawer MdSpr;

		protected aBtnMeter Meter;

		protected MeshDrawer MdStripeB;

		public bool do_not_write_default_title;

		protected float[] Amemori;

		private const float DEFAULT_W = 64f;

		private const float DEFAULT_H = 22f;

		private FnBtnMeterLine FD_getBtnMeterLineDefault;
	}
}

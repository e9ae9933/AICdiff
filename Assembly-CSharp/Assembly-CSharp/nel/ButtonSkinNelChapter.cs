using System;
using XX;

namespace nel
{
	public class ButtonSkinNelChapter : ButtonSkin
	{
		public ButtonSkinNelChapter(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.w = ((_w > 0f) ? _w : 40f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 20f) * 0.015625f;
			this.Md = base.makeMesh(null);
			this.MdStripe = base.makeMesh(MTRX.MtrMeshDashLine);
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			if (this.position == ButtonSkinNelChapter.POS.AUTO)
			{
				if (this.B.Container != null)
				{
					this.position = ((this.B.carr_index == 0) ? ButtonSkinNelChapter.POS.LEFT : ((this.B.carr_index == this.B.Container.Length - 1) ? ButtonSkinNelChapter.POS.RIGHT : ButtonSkinNelChapter.POS.CENTER));
				}
				else
				{
					this.position = ButtonSkinNelChapter.POS.CENTER;
				}
			}
			float num = this.w * 0.5f;
			float num2 = this.h * 0.35f;
			float num3 = num - num2;
			float num4 = X.Mx(0.03125f, num2 * 0.15f);
			float num5 = num2 - num4 * 2f;
			float num6 = num5 * 0.7f * 2f;
			if (this.isPushDown())
			{
				this.Md.Col = this.Md.ColGrd.Set(4282515319U).mulA(this.alpha).C;
				this.Md.Rect(0f, 0f, this.w, this.h, true);
				this.Md.Col = this.Md.ColGrd.Set(4278190080U).mulA(this.alpha).C;
				if (base.isChecked())
				{
					this.Md.Daia(0f, 0f, num6, num6, true);
				}
			}
			else
			{
				this.Md.Col = this.Md.ColGrd.Set(4282515319U).mulA(this.alpha).C;
				if (base.isHoveredOrPushOut())
				{
					if (base.isChecked())
					{
						this.Md.Daia(0f, 0f, num6, num6, true);
					}
					this.Md.Col = this.Md.ColGrd.Set(4282515319U).blend(uint.MaxValue, 0.7f + 0.3f * X.COSIT(40f)).mulA(this.alpha)
						.C;
				}
				else
				{
					if (base.isChecked())
					{
						this.Md.Daia(0f, 0f, num6 * 0.77f, num6 * 0.77f, true);
					}
					this.Md.Col = this.Md.ColGrd.Set(uint.MaxValue).mulA(this.alpha * 0.77f).C;
				}
			}
			if (this.position == ButtonSkinNelChapter.POS.LEFT)
			{
				this.Md.Tri(0, 2, 1, false).Tri(1, 3, 4, false).Tri(2, 5, 3, false)
					.Tri(2, 3, 1, false)
					.Tri(6, 7, 10, false)
					.Tri(10, 7, 11, false)
					.Tri(7, 3, 11, false)
					.Tri(11, 3, 12, false)
					.Tri(10, 9, 6, false)
					.Tri(6, 9, 8, false)
					.Tri(8, 9, 12, false)
					.Tri(8, 12, 3, false);
				this.Md.Pos(num, 0f, null).Pos(num - num4, num4, null).Pos(num - num4, -num4, null)
					.Pos(num - num3, 0f, null)
					.Pos(num - num3 - num4, num4, null)
					.Pos(num - num3 - num4, -num4, null);
				this.Md.Pos(-num2, 0f, null);
			}
			else if (this.position == ButtonSkinNelChapter.POS.RIGHT)
			{
				this.Md.Tri(0, 1, 2, false).Tri(1, 3, 2, false).Tri(1, 4, 3, false)
					.Tri(2, 3, 5, false)
					.Tri(3, 7, 10, false)
					.Tri(10, 7, 11, false)
					.Tri(7, 6, 11, false)
					.Tri(11, 6, 12, false)
					.Tri(10, 9, 3, false)
					.Tri(3, 9, 8, false)
					.Tri(8, 9, 12, false)
					.Tri(8, 12, 6, false);
				this.Md.Pos(-num, 0f, null).Pos(-num + num4, num4, null).Pos(-num + num4, -num4, null)
					.Pos(-num + num3, 0f, null)
					.Pos(-num + num4 + num3, num4, null)
					.Pos(-num + num4 + num3, -num4, null);
				this.Md.Pos(num2, 0f, null);
			}
			else
			{
				this.Md.Tri(0, 1, 2, false).Tri(1, 3, 2, false).Tri(1, 4, 3, false)
					.Tri(2, 3, 5, false)
					.Tri(6, 8, 7, false)
					.Tri(7, 9, 10, false)
					.Tri(8, 11, 9, false)
					.Tri(8, 9, 7, false)
					.Tri(3, 12, 15, false)
					.Tri(15, 12, 16, false)
					.Tri(12, 9, 16, false)
					.Tri(16, 9, 17, false)
					.Tri(3, 15, 13, false)
					.Tri(15, 14, 13, false)
					.Tri(14, 17, 13, false)
					.Tri(13, 17, 9, false);
				this.Md.Pos(-num, 0f, null).Pos(-num + num4, num4, null).Pos(-num + num4, -num4, null)
					.Pos(-num + num3, 0f, null)
					.Pos(-num + num4 + num3, num4, null)
					.Pos(-num + num4 + num3, -num4, null);
				this.Md.Pos(num, 0f, null).Pos(num - num4, num4, null).Pos(num - num4, -num4, null)
					.Pos(num - num3, 0f, null)
					.Pos(num - num3 - num4, num4, null)
					.Pos(num - num3 - num4, -num4, null);
			}
			this.Md.Pos(0f, num2, null).Pos(0f, -num2, null).Pos(0f, -num5, null)
				.Pos(-num5, 0f, null)
				.Pos(0f, num5, null)
				.Pos(num5, 0f, null);
			if (base.isHoveredOrPushOut())
			{
				this.MdStripe.RectDashedM(0f, 0f, this.w, this.h, 16, 0.03125f, 0.5f, true, false);
			}
			this.Md.updateForMeshRenderer(false);
			this.MdStripe.updateForMeshRenderer(false);
			return this;
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			return this;
		}

		private MeshDrawer Md;

		private MeshDrawer MdStripe;

		public ButtonSkinNelChapter.POS position;

		public enum POS : byte
		{
			AUTO,
			LEFT,
			CENTER,
			RIGHT
		}
	}
}

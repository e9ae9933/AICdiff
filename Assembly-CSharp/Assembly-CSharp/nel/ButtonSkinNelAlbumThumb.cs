using System;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinNelAlbumThumb : ButtonSkinNelUi
	{
		public ButtonSkinNelAlbumThumb(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			base.aligny = ALIGNY.BOTTOM;
			base.alignx = ALIGN.CENTER;
			base.fix_text_size = 10f;
		}

		protected override void setTitleText(string str)
		{
			base.setTitleText(str);
			this.Tx.AlignY(ALIGNY.MIDDLE);
			this.Tx.auto_wrap = true;
		}

		public void initAlbumThumb(UiAlbum.AlbumEntry _Be, Material MtrThumb)
		{
			this.Be = _Be;
			this.setTitleText(this.Be.visible_unlock ? this.Be.title_localized : TX.Get("Talker_Unknown", ""));
			this.fine_flag = true;
			if (this.MdThumb == null)
			{
				this.MdThumb = base.makeMesh(MtrThumb);
			}
			else
			{
				this.MdThumb.setMaterial(MtrThumb, false);
				base.getMeshRenderer(this.MdThumb).sharedMaterial = MtrThumb;
			}
			this.B.SetLocked(!_Be.visible_unlock, true, false);
		}

		protected override void fineDrawAdditional(ref float shift_px_x, ref float shift_px_y, ref bool shift_y_abs)
		{
			shift_px_y = -this.h * 64f * 0.5f + 20f;
			shift_y_abs = true;
			if (this.MdThumb != null)
			{
				this.MdThumb.clear(false, false);
				ButtonSkinNelAlbumThumb.drawAlbumThumbS(this.Md, this.MdThumb, this.Be, this.alpha_, 18f, 1f);
				this.MdThumb.updateForMeshRenderer(false);
			}
		}

		public static void drawAlbumThumbS(MeshDrawer Md, MeshDrawer MdT, UiAlbum.AlbumEntry Be, float alpha_, float thumb_y, float scale = 1f)
		{
			if (Be.key != null)
			{
				int num;
				if (Be.visible)
				{
					num = 2;
				}
				else if (Be.visible_unlock)
				{
					num = 3;
				}
				else
				{
					num = 0;
				}
				if ((num & 1) != 0)
				{
					if (MdT == null)
					{
						Md.chooseSubMesh(0, false, false);
					}
					Md.Col = Md.ColGrd.Set(4290951095U).mulA(alpha_).C;
					Md.Rect(0f, thumb_y, 100f * scale, 56f * scale, false);
				}
				if ((num & 2) != 0 && Be.PF != null)
				{
					MeshDrawer meshDrawer;
					if (MdT == null)
					{
						meshDrawer = Md;
						Md.chooseSubMesh(1, false, false);
					}
					else
					{
						meshDrawer = MdT;
					}
					meshDrawer.Col = meshDrawer.ColGrd.Set(MTRX.ColWhite).mulA(alpha_ * ((num == 3) ? 0.25f : 1f)).C;
					meshDrawer.RotaPF(0f, thumb_y, scale * 0.5f, scale * 0.5f, 0f, Be.PF, false, false, false, uint.MaxValue, false, 0);
				}
			}
			if (MdT == null)
			{
				Md.chooseSubMesh(0, false, false);
			}
			Md.Col = Md.ColGrd.Set(4283780170U).mulA(alpha_).C;
			Md.Box(0f, thumb_y, 100f * scale, 56f * scale, 1f, false);
		}

		private UiAlbum.AlbumEntry Be;

		private MeshDrawer MdThumb;

		public const float thumb_w = 100f;

		public const float thumb_h = 56f;

		public const float thumb_y = 18f;

		public const float image_scale = 0.5f;
	}
}

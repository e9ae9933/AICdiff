using System;
using System.Collections.Generic;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2SmartChipImage : M2ChipImageMulti
	{
		public M2SmartChipImage(M2ImageContainer _IMGS, string _name)
			: base(_IMGS, '#', _name)
		{
			this.ACon = new M2SmartImagePiece[50];
			for (int i = 49; i >= 0; i--)
			{
				this.ACon[i] = new M2SmartImagePiece(2);
			}
		}

		private M2SmartChipImage(M2ImageContainer _IMGS, string dirname, string basename, M2SmartImagePiece[] _ACon)
			: base(_IMGS, '#', dirname, basename)
		{
			this.ACon = _ACon;
		}

		public int kind
		{
			get
			{
				return (int)this.kind_;
			}
			set
			{
				this.kind_ = (M2SmartChipImage.SMI_KIND)value;
				switch (this.kind_)
				{
				case M2SmartChipImage.SMI_KIND.A8:
					this.btn_clms = 10;
					this.btn_rows = 5;
					return;
				case M2SmartChipImage.SMI_KIND.A4:
					this.btn_clms = 6;
					this.btn_rows = 3;
					return;
				case M2SmartChipImage.SMI_KIND.LR:
				case M2SmartChipImage.SMI_KIND.TB:
					this.btn_clms = 5;
					this.btn_rows = 2;
					return;
				default:
					return;
				}
			}
		}

		public static M2SmartChipImage readFromBytes(ByteReader Ba, byte load_ver, M2ImageContainer IMGS, string dirname, bool create = true)
		{
			Ba.readPascalString("utf-8", false);
			Ba.readByte();
			Ba.readUInt();
			Ba.readByte();
			Ba.readByte();
			int num = Ba.readByte();
			M2SmartImagePiece[] array = null;
			int num2 = 50;
			if (create)
			{
				array = new M2SmartImagePiece[num2];
			}
			for (int i = 0; i < num; i++)
			{
				M2SmartImagePiece m2SmartImagePiece = M2SmartImagePiece.readFromBytes(Ba, load_ver, create);
				if (array != null && m2SmartImagePiece != null && i < num2)
				{
					array[i] = m2SmartImagePiece;
				}
			}
			Ba.readByte();
			Ba.readUShort();
			if (load_ver < 3)
			{
				Ba.readPascalString("utf-8", false);
			}
			else
			{
				Ba.readUInt();
			}
			return null;
		}

		public override M2ChipImage getFirstImage()
		{
			if (this.Thumbnail != null)
			{
				return this.Thumbnail;
			}
			int num = 50;
			int num2 = 0;
			while (this.FirstImage == null && num2 < num)
			{
				this.FirstImage = this.ACon[num2].getFirstImage(this.IMGS);
				num2++;
			}
			return this.FirstImage;
		}

		public override List<M2Picture> MakePicture(M2MapLayer Lay, float x, float y, int opacity, int rotation, bool flip)
		{
			return this.getFirstImage().MakePicture(Lay, x, y, opacity, rotation, flip);
		}

		public override bool isSame(M2Puts Cp, bool in_editor = false, int editor_curs_x = -1, int editor_curs_y = -1)
		{
			return Cp.pattern != 0U && Cp.pattern == this.chip_id;
		}

		public override Vector2Int getClmsAndRows(INPUT_CR cr)
		{
			if (cr == INPUT_CR.WHOLE_BOUNDS || cr == INPUT_CR.PIXELS_CCFG)
			{
				return new Vector2Int(this.btn_clms, this.btn_rows);
			}
			if (cr == INPUT_CR.PIXELS || cr == INPUT_CR.PIXELS_EDIT)
			{
				return new Vector2Int(10, 5);
			}
			return new Vector2Int(this.iclms, this.irows);
		}

		public int alloc_dir_bits
		{
			get
			{
				int num;
				switch (this.kind_)
				{
				case M2SmartChipImage.SMI_KIND.A4:
					num = 15;
					break;
				case M2SmartChipImage.SMI_KIND.LR:
					num = 5;
					break;
				case M2SmartChipImage.SMI_KIND.TB:
					num = 10;
					break;
				default:
					num = 255;
					break;
				}
				return num;
			}
		}

		private M2SmartImagePiece getCon(M2SmartChipImage.SC id)
		{
			return this.getCon((int)id);
		}

		private bool con_setted(params M2SmartChipImage.SC[] id)
		{
			int num = id.Length;
			for (int i = 0; i < num; i++)
			{
				if (this.ACon[(int)id[i]].count > 0)
				{
					return true;
				}
			}
			return false;
		}

		private M2SmartImagePiece getCon(int id)
		{
			M2SmartImagePiece m2SmartImagePiece = this.ACon[id];
			if (m2SmartImagePiece.count > 0)
			{
				return m2SmartImagePiece;
			}
			switch (id)
			{
			case 0:
				return m2SmartImagePiece;
			case 1:
			case 5:
			case 6:
			case 10:
			case 12:
			case 16:
			case 20:
			case 21:
			case 22:
			case 30:
			case 40:
			case 45:
			case 46:
				return this.getCon(M2SmartChipImage.SC.FILL).Rotate(0);
			case 2:
			case 3:
			case 4:
				return this.getCon(M2SmartChipImage.SC.S1).Rotate(id - 1);
			case 7:
				return this.getCon(M2SmartChipImage.SC.C1).Rotate(1);
			case 8:
				return this.getCon(M2SmartChipImage.SC.C1).Rotate(2);
			case 9:
				return this.getCon(M2SmartChipImage.SC.C1).Rotate(3);
			case 11:
				if (this.kind_ != M2SmartChipImage.SMI_KIND.LR && this.kind_ != M2SmartChipImage.SMI_KIND.TB)
				{
					return this.getCon(M2SmartChipImage.SC.S5).Rotate(1);
				}
				return this.getCon(M2SmartChipImage.SC.FILL).Rotate(0);
			case 13:
			case 14:
			case 15:
				return this.getCon(M2SmartChipImage.SC.S3).Rotate(id - 12);
			case 17:
			case 18:
			case 19:
				return this.getCon(M2SmartChipImage.SC.C3).Rotate(id - 16);
			case 23:
			case 24:
			case 25:
				return this.getCon(M2SmartChipImage.SC.S7).Rotate(id - 22);
			case 26:
				if (!this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8 }))
				{
					return this.getCon(M2SmartChipImage.SC.FILL).Rotate(0);
				}
				return this.getCon(M2SmartChipImage.SC.S8).Rotate(0);
			case 27:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8C1 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S1 }))
				{
					return this.getCon(M2SmartChipImage.SC.S8C1).Rotate(id - 26);
				}
				return this.getCon(M2SmartChipImage.SC.S1).Rotate(0);
			case 28:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8C1 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S2 }))
				{
					return this.getCon(M2SmartChipImage.SC.S8C1).Rotate(id - 26);
				}
				return this.getCon(M2SmartChipImage.SC.S2).Rotate(0);
			case 29:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8C1 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S4 }))
				{
					return this.getCon(M2SmartChipImage.SC.S8C1).Rotate(id - 26);
				}
				return this.getCon(M2SmartChipImage.SC.S4).Rotate(0);
			case 31:
				return this.getCon(M2SmartChipImage.SC.C5).Rotate(1);
			case 32:
				if (!this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8 }))
				{
					return this.getCon(M2SmartChipImage.SC.FILL).Rotate(0);
				}
				return this.getCon(M2SmartChipImage.SC.S8).Rotate(0);
			case 33:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8C3 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S1 }))
				{
					return this.getCon(M2SmartChipImage.SC.S8C3).Rotate(id - 32);
				}
				return this.getCon(M2SmartChipImage.SC.S1).Rotate(0);
			case 34:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8C3 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S2 }))
				{
					return this.getCon(M2SmartChipImage.SC.S8C3).Rotate(id - 32);
				}
				return this.getCon(M2SmartChipImage.SC.S2).Rotate(0);
			case 35:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8C3 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S4 }))
				{
					return this.getCon(M2SmartChipImage.SC.S8C3).Rotate(id - 32);
				}
				return this.getCon(M2SmartChipImage.SC.S4).Rotate(0);
			case 36:
				if (!this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8 }))
				{
					return this.getCon(M2SmartChipImage.SC.FILL).Rotate(0);
				}
				return this.getCon(M2SmartChipImage.SC.S8).Rotate(0);
			case 37:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8C2 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S1 }))
				{
					return this.getCon(M2SmartChipImage.SC.S8C2).Rotate(id - 36);
				}
				return this.getCon(M2SmartChipImage.SC.S1).Rotate(0);
			case 38:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8C2 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S2 }))
				{
					return this.getCon(M2SmartChipImage.SC.S8C2).Rotate(id - 36);
				}
				return this.getCon(M2SmartChipImage.SC.S2).Rotate(0);
			case 39:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S8C2 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S4 }))
				{
					return this.getCon(M2SmartChipImage.SC.S8C2).Rotate(id - 36);
				}
				return this.getCon(M2SmartChipImage.SC.S4).Rotate(0);
			case 41:
				if (!this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S12 }))
				{
					return this.getCon(M2SmartChipImage.SC.FILL).Rotate(0);
				}
				return this.getCon(M2SmartChipImage.SC.S12).Rotate(0);
			case 42:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S12C1 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S9 }))
				{
					return this.getCon(M2SmartChipImage.SC.S12C1).Rotate(id - 41);
				}
				return this.getCon(M2SmartChipImage.SC.S9).Rotate(0);
			case 43:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S12C1 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S3 }))
				{
					return this.getCon(M2SmartChipImage.SC.S12C1).Rotate(id - 41);
				}
				return this.getCon(M2SmartChipImage.SC.S3).Rotate(0);
			case 44:
				if (this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S12C1 }) || !this.con_setted(new M2SmartChipImage.SC[] { M2SmartChipImage.SC.S6 }))
				{
					return this.getCon(M2SmartChipImage.SC.S12C1).Rotate(id - 41);
				}
				return this.getCon(M2SmartChipImage.SC.S6).Rotate(0);
			case 47:
			case 48:
			case 49:
				return this.getCon(M2SmartChipImage.SC.C7).Rotate(id - 46);
			default:
				return m2SmartImagePiece;
			}
		}

		public bool isAvailableSc(int id)
		{
			int num = id % 10;
			int num2 = id / 10;
			if (this.btn_clms <= num || this.btn_rows <= num2)
			{
				return false;
			}
			if (id - 20 <= 1 || id == 40)
			{
				return false;
			}
			if (this.kind == 2 && (id == 2 || id == 4 || id - 11 <= 4))
			{
				return false;
			}
			if (this.kind == 3)
			{
				if (id <= 3)
				{
					if (id != 1 && id != 3)
					{
						return true;
					}
				}
				else if (id != 10 && id - 12 > 3)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public override List<M2Chip> MakeChip(M2MapLayer Lay, int x, int y, int opacity, int rotation, bool flip)
		{
			if (opacity >= 0)
			{
				int num = ((this.kind_ == M2SmartChipImage.SMI_KIND.LR) ? 5 : ((this.kind_ == M2SmartChipImage.SMI_KIND.TB) ? 10 : 15));
				int num2 = 0;
				int num3 = ((this.kind_ == M2SmartChipImage.SMI_KIND.A8) ? 15 : 0);
				for (int i = 0; i < 8; i++)
				{
					if (!((i < 4) ? ((num & (1 << i)) == 0) : (this.kind_ > M2SmartChipImage.SMI_KIND.A8)))
					{
						M2Pt pointPuts = Lay.Mp.getPointPuts(x + CAim._XD(i, 1) * this.iclms, y - CAim._YD(i, 1) * this.irows, false, false);
						if (pointPuts != null && pointPuts.getSmartChipFamily(this, Lay) != null)
						{
							if (i < 4)
							{
								num &= ~(1 << i);
							}
							else
							{
								num3 &= ~((i == 6) ? 8 : ((i == 7) ? 4 : (1 << i - 4)));
							}
						}
					}
				}
				if ((num & 1) == 0 && (num & 2) == 0)
				{
					num2 |= num3 & 1;
				}
				if ((num & 2) == 0 && (num & 4) == 0)
				{
					num2 |= num3 & 2;
				}
				if ((num & 4) == 0 && (num & 8) == 0)
				{
					num2 |= num3 & 4;
				}
				if ((num & 8) == 0 && (num & 1) == 0)
				{
					num2 |= num3 & 8;
				}
				int num4 = 0;
				M2SmartChipImage.SC sc;
				if (num == 0 && num2 == 0)
				{
					num4 = 0;
				}
				else if (!FEnum<M2SmartChipImage.SC>.TryParse(((num > 0) ? ("S" + num.ToString()) : "") + ((num2 > 0) ? ("C" + num2.ToString()) : ""), out sc, true))
				{
					X.dl("不明な SC タイプ:+key", null, false, false);
				}
				else
				{
					num4 = (int)sc;
				}
				M2SmartImagePiece con = this.getCon(num4);
				List<M2Chip> list = new List<M2Chip>(4);
				int count = con.count;
				for (int j = 0; j < count; j++)
				{
					int num5 = x;
					int num6 = y;
					for (;;)
					{
						int num7 = 1;
						int num8 = 1;
						M2Chip m2Chip = con.MakeChip(this.IMGS, Lay, num5, num6, opacity, j, ref num7, ref num8);
						if (m2Chip != null)
						{
							m2Chip.pattern = this.chip_id;
							list.Add(m2Chip);
						}
						num5 += X.Mx(1, num7);
						if (num5 >= x + this.iclms)
						{
							num5 = x;
							num6 += X.Mx(1, num8);
							if (num6 >= y + this.irows)
							{
								break;
							}
						}
					}
				}
				return list;
			}
			M2ChipImage firstImage = this.getFirstImage();
			if (firstImage == null)
			{
				return null;
			}
			return firstImage.MakeChip(Lay, x, y, opacity, rotation, flip);
		}

		public void copyFrom(int j, M2SmartChipImage Smi)
		{
			M2SmartImagePiece con = Smi.getCon(j);
			this.ACon[j].Rem().AddFrom(con, this.IMGS, false);
		}

		public override bool isBg()
		{
			if (this.is_bg == -1)
			{
				this.is_bg = 1;
				for (int i = 49; i >= 0; i--)
				{
					if (!this.ACon[i].isBg(this.IMGS))
					{
						this.is_bg = 0;
						break;
					}
				}
			}
			return this.is_bg == 1;
		}

		public override IM2CLItem spoitImageAt(int j, ref int rotation, ref bool flip)
		{
			if (!this.isAvailableSc(j))
			{
				return null;
			}
			return this.getCon(j).spoitLast(this.IMGS, ref rotation, ref flip);
		}

		public override void writeAt(int j, IM2CLItem Img, int rotation, bool flip)
		{
			if (!this.isAvailableSc(j) || Img == null)
			{
				return;
			}
			if (Img is M2SmartChipImage || Img is M2StampImage)
			{
				Img = Img.getFirstImage();
			}
			this.FirstImage = null;
			this.is_bg = -1;
			this.ACon[j].Add(Img.getChipId(), (rotation & 3) | (flip ? 4 : 0), true);
		}

		public override void eraseAt(int j)
		{
			M2ChipImage firstImage = this.getFirstImage();
			this.FirstImage = null;
			this.is_bg = -1;
			this.ACon[j].Rem();
			if (this.getFirstImage() == null)
			{
				this.ACon[j].Add(firstImage.chip_id, 0, false);
				X.dl("イメージが消滅するので消去不可", null, false, false);
			}
		}

		public override void LinkSources(bool need_fine_editor_buttons = true)
		{
			base.getTitle();
			for (int i = 49; i >= 0; i--)
			{
				this.ACon[i].LinkSources(base.src, this.IMGS, need_fine_editor_buttons);
			}
		}

		public override void UnlinkSources(bool need_fine_editor_buttons = true)
		{
			string src = base.src;
			for (int i = 49; i >= 0; i--)
			{
				this.ACon[i].UnlinkSources(base.src, this.IMGS, need_fine_editor_buttons);
			}
		}

		public int iclms = 1;

		public int irows = 1;

		private M2ChipImage FirstImage;

		private readonly M2SmartImagePiece[] ACon;

		public const int BTN_CLMS_MAX = 10;

		public const int BTN_ROWS_MAX = 5;

		public int btn_clms = 10;

		public int btn_rows = 5;

		public M2SmartChipImage.SMI_KIND kind_;

		public enum SC
		{
			FILL,
			S1,
			S2,
			S4,
			S8,
			S15,
			C1,
			C2,
			C4,
			C8,
			S5,
			S10,
			S3,
			S6,
			S12,
			S9,
			C3,
			C6,
			C12,
			C9,
			N0,
			N1,
			S7,
			S14,
			S13,
			S11,
			S8C1,
			S1C2,
			S2C4,
			S4C8,
			C5,
			C10,
			S8C3,
			S1C6,
			S2C12,
			S4C9,
			S8C2,
			S1C4,
			S2C8,
			S4C1,
			N2,
			S12C1,
			S9C2,
			S3C4,
			S6C8,
			C15,
			C7,
			C14,
			C13,
			C11,
			_MAX
		}

		public enum SMI_KIND
		{
			A8,
			A4,
			LR,
			TB
		}
	}
}

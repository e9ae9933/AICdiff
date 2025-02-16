using System;
using System.Collections.Generic;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2StampImage : M2ChipImageMulti
	{
		public M2StampImage(M2ImageContainer _IMGS, string _name)
			: base(_IMGS, '$', _name)
		{
			this.APuts = new List<M2StampImage.M2StampImageItem>();
		}

		private M2StampImage(M2ImageContainer _IMGS, string dirname, string basename, List<M2StampImage.M2StampImageItem> _APuts)
			: base(_IMGS, '$', dirname, basename)
		{
			this.APuts = _APuts;
		}

		public static M2StampImage readFromBytes(ByteArray Ba, byte load_ver, M2ImageContainer IMGS, string dirname, bool create = true, bool is_pxls_builtin = false)
		{
			string text = Ba.readPascalString("utf-8", false);
			int num = Ba.readByte();
			bool flag = (num & 2) != 0;
			bool flag2 = (num & 4) != 0;
			uint num2 = Ba.readUInt();
			int num3 = Ba.readByte();
			int num4 = Ba.readByte();
			int num5 = Ba.readByte();
			List<M2StampImage.M2StampImageItem> list = null;
			if (create)
			{
				list = new List<M2StampImage.M2StampImageItem>(num5);
			}
			for (int i = 0; i < num5; i++)
			{
				M2StampImage.M2StampImageItem m2StampImageItem = M2StampImage.M2StampImageItem.readFromBytes(Ba, load_ver, create);
				if (list != null && m2StampImageItem != null)
				{
					list.Add(m2StampImageItem);
				}
			}
			ushort num6 = Ba.readUShort();
			int num7 = Ba.readByte();
			int num8 = Ba.readByte();
			Vector3 zero = Vector3.zero;
			if (is_pxls_builtin)
			{
				zero.Set(Ba.readFloat(), Ba.readFloat(), 2f);
			}
			if (create)
			{
				return new M2StampImage(IMGS, dirname, text, list)
				{
					chip_id = num2,
					pclms = num3,
					prows = num4,
					favorited = flag,
					no_strict_image = flag2,
					family_index = num6,
					input_clms = num7,
					input_rows = num8,
					PosPxlsBuiltIn = zero
				};
			}
			return null;
		}

		public bool prepared
		{
			get
			{
				return this.prepared_;
			}
			set
			{
				if (!this.prepared_ && value)
				{
					this.prepareImages();
				}
			}
		}

		private bool prepareImages()
		{
			if (!this.prepared_)
			{
				for (int i = this.APuts.Count - 1; i >= 0; i--)
				{
					if (this.APuts[i].prepareImage(this.IMGS))
					{
						this.prepared_ = true;
					}
					else
					{
						this.APuts.RemoveAt(i);
					}
				}
			}
			return this.prepared_;
		}

		public override M2ChipImage getFirstImage()
		{
			if (this.APuts == null || this.APuts.Count <= 0 || this.APuts[0] == null)
			{
				return null;
			}
			return this.APuts[0].Img;
		}

		public override bool isSame(M2Puts Cp, bool in_editor = false, int editor_curs_x = -1, int editor_curs_y = -1)
		{
			return false;
		}

		public override Vector2Int getClmsAndRows(INPUT_CR cr)
		{
			if (cr == INPUT_CR.SNAP_WRITING || cr == INPUT_CR.SNAP_BASEPOS)
			{
				return new Vector2Int(this.input_clms, this.input_rows);
			}
			return new Vector2Int(this.pclms, this.prows);
		}

		public override List<M2Chip> MakeChip(M2MapLayer Lay, int x, int y, int opacity, int rotation, bool flip)
		{
			if (opacity >= 0)
			{
				int count = this.APuts.Count;
				List<M2Chip> list = new List<M2Chip>(count);
				float num = (float)x;
				float num2 = (float)y;
				if (rotation % 2 == 1)
				{
					num += (float)this.prows * 0.5f;
					num2 += (float)this.pclms * 0.5f;
				}
				else
				{
					num += (float)this.pclms * 0.5f;
					num2 += (float)this.prows * 0.5f;
				}
				for (int i = 0; i < count; i++)
				{
					M2StampImage.M2StampImageItem m2StampImageItem = this.APuts[i];
					IM2Inputtable inputtable = m2StampImageItem.getInputtable(true, !this.no_strict_image);
					Vector2 vector = global::XX.X.ROTV2e(new Vector2((float)global::XX.X.MPF(!flip) * m2StampImageItem.x, m2StampImageItem.y), (float)rotation * 1.5707964f);
					int num3 = global::XX.X.IntR(m2StampImageItem.rot01 * 4f);
					float num4 = num * (float)base.CLEN + vector.x;
					float num5 = num2 * (float)base.CLEN + vector.y;
					int num6 = num3 + rotation;
					List<M2Chip> list2 = inputtable.MakeChip(Lay, global::XX.X.IntR(num4 / (float)base.CLEN), global::XX.X.IntR(num5 / (float)base.CLEN), opacity, num6, flip != m2StampImageItem.flip);
					if (list2 != null && list2.Count > 0)
					{
						M2Chip m2Chip = list2[0];
						for (int j = list2.Count - 1; j >= 0; j--)
						{
							list2[j].pattern = (this.pxls_built_in ? base.getChipId() : m2StampImageItem.pattern);
						}
						m2Chip.drawx = (int)(num4 - (float)m2Chip.iwidth * 0.5f);
						m2Chip.drawy = (int)(num5 - (float)m2Chip.iheight * 0.5f);
						m2Chip.inputXy();
						list.Add(m2Chip);
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

		public override List<M2Picture> MakePicture(M2MapLayer Lay, float x, float y, int opacity, int rotation, bool flip)
		{
			if (opacity >= 0)
			{
				int count = this.APuts.Count;
				List<M2Picture> list = new List<M2Picture>(count);
				for (int i = 0; i < count; i++)
				{
					M2StampImage.M2StampImageItem m2StampImageItem = this.APuts[i];
					IM2Inputtable inputtable = m2StampImageItem.getInputtable(true, false);
					Vector2 vector = global::XX.X.ROTV2e(new Vector2((float)global::XX.X.MPF(!flip) * m2StampImageItem.x / (float)base.CLEN, m2StampImageItem.y / (float)base.CLEN), (float)rotation / 180f * 3.1415927f);
					int num = global::XX.X.IntR(m2StampImageItem.rot01 * 360f);
					List<M2Picture> list2 = inputtable.MakePicture(Lay, x + vector.x, y + vector.y, opacity, num + rotation, flip != m2StampImageItem.flip);
					if (list2 != null && list2.Count > 0)
					{
						list2[0].pattern = (this.pxls_built_in ? base.getChipId() : m2StampImageItem.pattern);
						list.Add(list2[0]);
					}
				}
				return list;
			}
			M2ChipImage firstImage = this.getFirstImage();
			if (firstImage == null)
			{
				return null;
			}
			return firstImage.MakePicture(Lay, x, y, opacity, rotation, flip);
		}

		public static void applyEditorShift(M2StampImage Target)
		{
			if (Target != null)
			{
				int count = Target.APuts.Count;
				for (int i = 0; i < count; i++)
				{
					Target.APuts[i].x += M2StampImage.EditorShift.x;
					Target.APuts[i].y += M2StampImage.EditorShift.y;
				}
			}
			M2StampImage.EditorShift = Vector2.zero;
		}

		public override bool isBg()
		{
			if (this.is_bg == -1)
			{
				this.is_bg = 1;
				for (int i = this.APuts.Count - 1; i >= 0; i--)
				{
					if (this.APuts[i].Img != null && !this.APuts[i].Img.isBg())
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
			M2StampImage.M2StampImageItem m2StampImageItem = this.spoitStampItemAt(j);
			if (m2StampImageItem != null)
			{
				rotation = global::XX.X.IntR(m2StampImageItem.rot01 * 4f);
				flip = m2StampImageItem.flip;
				return m2StampImageItem.Img;
			}
			return null;
		}

		private M2StampImage.M2StampImageItem spoitStampItemAt(int j)
		{
			int num = j % this.pclms;
			int num2 = j / this.pclms;
			for (int i = this.APuts.Count - 1; i >= 0; i--)
			{
				Rect boundsGrid = this.APuts[i].getBoundsGrid(this);
				if (global::XX.X.BTW(boundsGrid.x, (float)num, boundsGrid.xMax) && global::XX.X.BTW(boundsGrid.y, (float)num2, boundsGrid.yMax))
				{
					return this.APuts[i];
				}
			}
			return null;
		}

		public override void writeAt(int j, IM2CLItem Img, int rotation, bool flip)
		{
		}

		public override void eraseAt(int j)
		{
			if (this.APuts.Count == 1)
			{
				global::XX.X.dl("イメージが消滅するので消去不可", null, false, false);
				return;
			}
			this.reindex();
		}

		private void reindex()
		{
			int count = this.APuts.Count;
			for (int i = 0; i < count; i++)
			{
				this.APuts[i].index = i;
			}
		}

		public override void LinkSources(bool need_fine_editor_buttons = true)
		{
			this.prepared = true;
		}

		public override void UnlinkSources(bool need_fine_editor_buttons = true)
		{
		}

		public bool pxls_built_in
		{
			get
			{
				return this.PosPxlsBuiltIn.z > 0f;
			}
		}

		public int getStampLength()
		{
			return this.APuts.Count;
		}

		public M2StampImage.M2StampImageItem Get(int i)
		{
			return this.APuts[i];
		}

		public int pclms = 1;

		public int prows = 1;

		public int input_clms = 1;

		public int input_rows = 1;

		public bool no_strict_image;

		private Vector3 PosPxlsBuiltIn;

		private bool prepared_;

		public RenderTexture TxRendered;

		public static Vector2 EditorShift;

		private readonly List<M2StampImage.M2StampImageItem> APuts;

		public const char stamp_headkey_char = '$';

		public class M2StampImageItem
		{
			public M2ChipImage Img
			{
				get
				{
					return this.Img_;
				}
				set
				{
					this.Img_ = value;
					this.chip_id = ((this.Img_ != null) ? this.Img_.chip_id : 0U);
				}
			}

			public M2StampImageItem(M2Puts Cp, int _index)
			{
				if (Cp is M2Picture)
				{
					M2Picture m2Picture = Cp as M2Picture;
					this.rot01 = m2Picture.rotR / 6.2831855f;
				}
				else
				{
					this.rot01 = (float)Cp.rotation / 4f;
				}
				this.Img = Cp.Img;
				this.flip = Cp.flip;
				this.index = _index;
				this.pattern = Cp.pattern;
			}

			public M2StampImageItem(uint _chip_id, float _x, float _y, float _rot01, bool _flip, uint _pattern)
			{
				this.chip_id = _chip_id;
				this.x = _x;
				this.y = _y;
				this.rot01 = _rot01;
				this.flip = _flip;
				this.pattern = _pattern;
			}

			public M2StampImageItem(IM2CLItem Img_, float _x, float _y, float _rot01, bool _flip)
				: this(Img_.getChipId(), _x, _y, _rot01, _flip, (Img_ is M2ChipImage) ? 0U : Img_.getChipId())
			{
			}

			public bool prepareImage(M2ImageContainer IMGS)
			{
				if (this.Img == null && this.chip_id > 0U)
				{
					this.Img = IMGS.GetById(this.chip_id) as M2ChipImage;
				}
				return this.Img != null;
			}

			public static M2StampImage.M2StampImageItem readFromBytes(ByteArray Ba, byte load_ver, bool create = true)
			{
				uint num = Ba.readUInt();
				if (num == 0U)
				{
					return null;
				}
				float num2 = Ba.readFloat();
				float num3 = Ba.readFloat();
				float num4 = Ba.readFloat();
				bool flag = Ba.readBoolean();
				uint num5 = Ba.readUInt();
				if (create)
				{
					return new M2StampImage.M2StampImageItem(num, num2, num3, num4, flag, num5);
				}
				return null;
			}

			public void writeToBytes(ByteArray Ba)
			{
				if (this.chip_id == 0U)
				{
					Ba.writeUInt(0U);
					return;
				}
				Ba.writeUInt(this.chip_id);
				Ba.writeFloat(this.x);
				Ba.writeFloat(this.y);
				Ba.writeFloat(this.rot01);
				Ba.writeBool(this.flip);
				Ba.writeUInt(this.pattern);
			}

			public Rect getBoundsGrid(M2StampImage Con)
			{
				float clen = Con.IMGS.CLEN;
				Rect rect = new Rect(this.x / clen + (float)Con.pclms * 0.5f, this.y / clen + (float)Con.prows * 0.5f, 0f, 0f);
				rect.width = (float)(global::XX.X.IntC(rect.x + (float)this.Img.iwidth / clen) - (int)rect.x);
				rect.height = (float)(global::XX.X.IntC(rect.y + (float)this.Img.iheight / clen) - (int)rect.y);
				rect.x = (float)((int)rect.x);
				rect.y = (float)((int)rect.y);
				return rect;
			}

			public IM2Inputtable getInputtable(bool no_use_smart_image = false, bool strict_image = false)
			{
				if (this.pattern != 0U && !strict_image)
				{
					IM2CLItem im2CLItem = this.Img.IMGS.GetById(this.pattern);
					if (no_use_smart_image && im2CLItem is M2SmartChipImage)
					{
						im2CLItem = null;
					}
					if (im2CLItem != null && !(im2CLItem is M2StampImage))
					{
						return im2CLItem;
					}
				}
				return this.Img;
			}

			public float getChipCenterX(M2StampImage Con)
			{
				return (float)((int)(this.x / (float)this.Img.CLEN + (float)Con.pclms * 0.5f + 0.0001f)) - (float)Con.pclms * 0.5f + 0.5f;
			}

			public float getChipCenterY(M2StampImage Con)
			{
				return (float)((int)(this.y / (float)this.Img.CLEN + (float)Con.prows * 0.5f + 0.0001f)) - (float)Con.prows * 0.5f + 0.5f;
			}

			public static bool isSame(List<M2StampImage.M2StampImageItem> Aa, List<M2StampImage.M2StampImageItem> Ab)
			{
				if (Aa.Count != Ab.Count)
				{
					return false;
				}
				int count = Aa.Count;
				for (int i = 0; i < count; i++)
				{
					if (!Aa[i].isSame(Ab[i]))
					{
						return false;
					}
				}
				return true;
			}

			public bool isSame(M2StampImage.M2StampImageItem Ab)
			{
				return this.chip_id == Ab.chip_id && this.x == Ab.x && this.y == Ab.y && this.rot01 == Ab.rot01 && this.flip == Ab.flip;
			}

			public float x;

			public float y;

			public float rot01;

			public bool flip;

			public uint pattern;

			private uint chip_id;

			public int index;

			private M2ChipImage Img_;
		}
	}
}

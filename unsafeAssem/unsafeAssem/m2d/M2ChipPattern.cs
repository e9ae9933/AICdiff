using System;
using System.Collections.Generic;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2ChipPattern : M2ChipImageMulti
	{
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

		public M2ChipPattern(M2ImageContainer _IMGS, string _dirname, string _basename, int _pclms, int _prows, M2ChipPattern.PatData[] _APd)
			: base(_IMGS, '&', _dirname, _basename)
		{
			this.pclms = _pclms;
			this.prows = _prows;
			this.len = this.pclms * this.prows;
			this.APd = ((_APd != null && _APd.Length == this.len) ? _APd : new M2ChipPattern.PatData[this.len]);
			this.resizingByClms();
		}

		public M2ChipPattern(M2ImageContainer _IMGS, string _name)
			: base(_IMGS, '&', _name)
		{
			this.iclms = 1;
			this.irows = 1;
			this.pclms = 4;
			this.prows = 4;
			this.len = this.pclms * this.prows;
			this.APd = new M2ChipPattern.PatData[this.len];
			this.resizingByClms();
		}

		public M2ChipPattern resizingByClms()
		{
			if (this.len != this.pclms * this.prows || this.pclms != this.pre_pclms || this.prows != this.pre_prows)
			{
				this.len = this.pclms * this.prows;
				M2ChipPattern.PatData[] apd = this.APd;
				if (this.pre_pclms != 0)
				{
					this.APd = X.copyTwoDimensionArray<M2ChipPattern.PatData>(new M2ChipPattern.PatData[this.len], this.pclms, this.prows, apd, this.pre_pclms, this.pre_prows, false);
				}
				else
				{
					Array.Resize<M2ChipPattern.PatData>(ref this.APd, this.len);
				}
			}
			this.pre_pclms = this.pclms;
			this.pre_prows = this.prows;
			this.prepared_ = false;
			this.is_bg = -1;
			return this;
		}

		public void replaceArray(int clms, int rows, M2ChipPattern.PatData[] APat)
		{
			this.UnlinkSources(true);
			this.pclms = clms;
			this.prows = rows;
			this.prepared_ = false;
			this.is_bg = -1;
			this.APd = APat;
			this.resizingByClms();
		}

		public bool prepareImages()
		{
			if (this.prepared_)
			{
				return true;
			}
			bool flag = this.isNestCreatedName() && this.NestedParent == null;
			bool flag2 = false;
			bool flag3 = false;
			for (int i = 0; i < this.len; i++)
			{
				M2ChipPattern.PatData patData = this.APd[i];
				if (patData.valid)
				{
					IM2CLItem byId = this.IMGS.GetById(patData.chip_id);
					if (byId == null || patData.Img == this)
					{
						if (flag)
						{
							flag3 = true;
							goto IL_008A;
						}
						patData.chip_id = 0U;
					}
					else
					{
						patData.Img = byId;
					}
					this.APd[i] = patData;
					flag2 = true;
				}
				IL_008A:;
			}
			this.width = this.iclms * base.CLEN;
			this.height = this.irows * base.CLEN;
			this.pwidth = this.iclms * this.pclms * base.CLEN;
			this.pheight = this.irows * this.prows * base.CLEN;
			if (flag3)
			{
				return false;
			}
			if (flag2)
			{
				this.prepared_ = true;
			}
			this.is_bg = -1;
			return flag2;
		}

		public override void eraseAt(int j)
		{
			M2ChipPattern.PatData patData = this.APd[j];
			if (!patData.valid)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < this.len; i++)
			{
				if (patData.valid)
				{
					num++;
				}
			}
			if (num <= 1)
			{
				X.de("パターン内で使用するイメージがなくなるような操作はできません", null);
				return;
			}
			this.is_bg = -1;
			M2ChipImageNestedParent nestedParent = this.NestedParent;
			this.APd[j] = default(M2ChipPattern.PatData);
		}

		public override void writeAt(int j, IM2CLItem Img, int rotation, bool flip)
		{
			if (Img == this)
			{
				X.dl("このパターン自身を writeAt しようとしました", null, false, false);
				return;
			}
			if (!X.BTW(0f, (float)j, (float)this.len))
			{
				return;
			}
			if (Img is M2StampImage)
			{
				Img = Img.getFirstImage();
			}
			if (Img is M2SmartChipImage)
			{
				rotation = 0;
				flip = false;
			}
			M2ChipImageNestedParent nestedParent = this.NestedParent;
			this.APd[j] = new M2ChipPattern.PatData(Img, rotation + (flip ? 4 : 0));
		}

		public override IM2CLItem spoitImageAt(int j, ref int rotation, ref bool flip)
		{
			if (j < 0 || j >= this.APd.Length)
			{
				return null;
			}
			M2ChipPattern.PatData patData = this.APd[j];
			if (!patData.valid || patData.Img == null)
			{
				return null;
			}
			rotation = patData.direcs & 3;
			flip = (patData.direcs & 4) > 0;
			return patData.Img;
		}

		public override List<M2Chip> MakeChip(M2MapLayer Lay, int x, int y, int opacity, int rotation, bool flip)
		{
			return null;
		}

		private BList<M2ChipPattern.PatDataInput> CalcAt(BList<M2ChipPattern.PatDataInput> A, int mapx, int mapy, int rot, bool flip)
		{
			this.prepared_ = true;
			if (!this.prepared_)
			{
				return A;
			}
			bool flag = rot == 1 || rot == 3;
			int num = (flag ? this.irows : this.iclms);
			int num2 = (flag ? this.iclms : this.irows);
			int num3 = (flag ? this.prows : this.pclms);
			int num4 = (flag ? this.pclms : this.prows);
			int num5 = num3 * num;
			int num6 = num4 * num2;
			int num7 = 0;
			int num8 = 0;
			float num9 = (float)num7 + ((float)((mapx - num7) / num5) + 0.5f) * (float)num5;
			float num10 = (float)num8 + ((float)((mapy - num8) / num6) + 0.5f) * (float)num6;
			float num11 = (float)mapx + 0.5f - num9;
			float num12 = (float)mapy + 0.5f - num10;
			Matrix4x4 matrix4x = Matrix4x4.Scale(new Vector3((float)(flip ? (-1) : 1), 1f, 0f));
			Vector3 vector = (matrix4x * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, (float)(-(float)rot * 90)))).MultiplyPoint3x4(new Vector3(num11, num12, 0f));
			int num13 = (int)(vector.x / (float)this.iclms + (float)this.pclms * 0.5f + 0.001f);
			int num14 = (int)(vector.y / (float)this.irows + (float)this.prows * 0.5f + 0.001f);
			int num15 = this.APd.Length;
			for (int i = 0; i < num15; i++)
			{
				M2ChipPattern.PatData patData = this.APd[i];
				if (patData.valid && patData.Img != null)
				{
					int num16 = i % this.pclms;
					int num17 = i / this.pclms;
					Vector2Int clmsAndRows = patData.Img.getClmsAndRows(INPUT_CR.SNAP);
					clmsAndRows.x = X.IntC((float)clmsAndRows.x / (float)this.iclms);
					clmsAndRows.y = X.IntC((float)clmsAndRows.y / (float)this.irows);
					if (X.BTW((float)num16, (float)num13, (float)(num16 + clmsAndRows.x)) && X.BTW((float)num17, (float)num14, (float)(num17 + clmsAndRows.y)))
					{
						M2ChipPattern.PatDataInput patDataInput = new M2ChipPattern.PatDataInput
						{
							Img = patData.Img,
							rotation = (rot + 4 + (flip ? (-1) : 1) * patData.direcs) % 4,
							flip = (((flip ? 1 : 0) + ((patData.direcs >= 4) ? 1 : 0)) % 2 != 0)
						};
						Vector3 vector2 = new Vector3((float)num16 + (float)clmsAndRows.x * 0.5f - (float)this.pclms * 0.5f, (float)num17 + (float)clmsAndRows.y * 0.5f - (float)this.prows * 0.5f);
						Vector3 vector3 = (Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, (float)(rot * 90))) * matrix4x).MultiplyPoint3x4(vector2);
						patDataInput.mapx = X.IntR(vector3.x * (float)num + num9 - (float)((flag ? clmsAndRows.y : clmsAndRows.x) + (num - 1)) * 0.5f);
						patDataInput.mapy = X.IntR(vector3.y * (float)num2 + num10 - (float)((flag ? clmsAndRows.x : clmsAndRows.y) + (num2 - 1)) * 0.5f);
						A.Add(patDataInput);
					}
				}
			}
			return A;
		}

		public override void LinkSources(bool need_fine_editor_buttons = true)
		{
			if (!this.prepared_ && !this.prepareImages())
			{
				return;
			}
			if (this.APd == null)
			{
				return;
			}
			for (int i = 0; i < this.len; i++)
			{
				M2ChipPattern.PatData patData = this.APd[i];
				if (patData.Img != null)
				{
					this.IMGS.LinkSourcesForInputtable(patData.Img, base.src, need_fine_editor_buttons);
				}
			}
		}

		public override void UnlinkSources(bool need_fine_editor_buttons = true)
		{
			if (this.APd == null)
			{
				return;
			}
			for (int i = 0; i < this.len; i++)
			{
				M2ChipPattern.PatData patData = this.APd[i];
				if (patData.Img != null)
				{
					this.IMGS.UnlinkSourcesForInputtable(patData.Img, base.src, need_fine_editor_buttons);
				}
			}
		}

		public override M2ChipImage getFirstImage()
		{
			if (this.Thumbnail != null)
			{
				return this.Thumbnail;
			}
			for (int i = 0; i < this.len; i++)
			{
				M2ChipPattern.PatData patData = this.APd[i];
				if (patData.valid && patData.Img != null)
				{
					M2ChipImage firstImage = patData.Img.getFirstImage();
					if (firstImage != null)
					{
						return firstImage;
					}
				}
			}
			return null;
		}

		public IM2CLItem GetByIndex(int i)
		{
			M2ChipPattern.PatData patData = this.APd[i];
			if (!patData.valid)
			{
				return null;
			}
			return patData.Img;
		}

		public override Vector2Int getClmsAndRows(INPUT_CR cr)
		{
			if (cr == INPUT_CR.WHOLE_BOUNDS || cr == INPUT_CR.SNAP_BASEPOS)
			{
				return new Vector2Int(this.pclms * this.iclms, this.prows * this.irows);
			}
			if (cr == INPUT_CR.PIXELS || cr == INPUT_CR.PIXELS_CCFG || cr == INPUT_CR.PIXELS_EDIT)
			{
				return new Vector2Int(this.pclms, this.prows);
			}
			return new Vector2Int(this.iclms, this.irows);
		}

		public override bool isSame(M2Puts Cp, bool in_editor = false, int editor_curs_x = -1, int editor_curs_y = -1)
		{
			return Cp.pattern != 0U && Cp.pattern == this.chip_id;
		}

		public override bool isBg()
		{
			if (!this.prepared_)
			{
				return this.is_bg == 0;
			}
			if (this.is_bg == -1)
			{
				this.is_bg = 1;
				for (int i = this.len - 1; i >= 0; i--)
				{
					M2ChipPattern.PatData patData = this.APd[i];
					if (patData.valid && patData.Img != null)
					{
						if (patData.Img is M2ChipImageMulti)
						{
							if (!(patData.Img as M2ChipImageMulti).isBg())
							{
								this.is_bg = 0;
								break;
							}
						}
						else
						{
							M2ChipImage firstImage = patData.Img.getFirstImage();
							if (firstImage != null && !firstImage.isBg())
							{
								this.is_bg = 0;
								break;
							}
						}
					}
				}
			}
			return this.is_bg == 1;
		}

		public void fine_bg_flag()
		{
			this.is_bg = -1;
		}

		public M2ChipImage getRandomImage(ref int rot, ref bool flip)
		{
			if (!this.prepared_ && !this.prepareImages())
			{
				return null;
			}
			int num = X.xors(this.len);
			for (int i = this.len - 1; i >= 0; i--)
			{
				M2ChipPattern.PatData patData = this.APd[(i + num) % this.len];
				if (patData.valid && patData.Img != null)
				{
					rot = patData.direcs & 3;
					flip = (patData.direcs & 4) > 0;
					return patData.Img.getFirstImage();
				}
			}
			return null;
		}

		public override List<M2Picture> MakePicture(M2MapLayer Lay, float x, float y, int opacity, int rotation, bool flip)
		{
			int num = 0;
			bool flag = false;
			if (this.NestedParent != null)
			{
				List<M2Picture> list = this.NestedParent.MakePicture(Lay, x, y, opacity, rotation + num * 90, flip != flag);
				if (list != null)
				{
					for (int i = list.Count - 1; i >= 0; i--)
					{
						list[i].pattern = this.chip_id;
					}
				}
				return list;
			}
			M2ChipImage randomImage = this.getRandomImage(ref num, ref flag);
			if (randomImage == null)
			{
				return null;
			}
			return randomImage.MakePicture(Lay, x, y, opacity, rotation + num * 90, flip != flag);
		}

		public static M2ChipPattern readFromBytes(ByteReader Ba, byte load_ver, M2ImageContainer IMGS, string dirname, bool create = true)
		{
			Ba.readPascalString("utf-8", false);
			Ba.readByte();
			Ba.readUInt();
			int num = Ba.readByte();
			int num2 = Ba.readByte();
			if (load_ver >= 5)
			{
				int num3 = Ba.readByte();
				int num4 = (num3 >> 4) & 15;
				int num5 = num3 & 15;
				if (num4 == 0 || num5 == 0)
				{
				}
			}
			Ba.readUShort();
			int num6 = (int)Ba.readUShort();
			M2ChipPattern.PatData[] array = null;
			int num7 = num * num2;
			if (create)
			{
				array = new M2ChipPattern.PatData[num7];
			}
			for (int i = 0; i < num6; i++)
			{
				uint num8 = Ba.readUInt();
				int num9 = Ba.readByte();
				if (array != null && i < num7)
				{
					array[i] = new M2ChipPattern.PatData(null, num8, num9);
				}
			}
			byte b = 32;
			if (load_ver < 3)
			{
				Ba.readPascalString("utf-8", false);
			}
			else
			{
				Ba.readUInt();
				if (load_ver >= 8)
				{
					b = Ba.readUByte();
				}
			}
			if (b == 0)
			{
			}
			return null;
		}

		public bool isNestCreatedName()
		{
			return TX.isEnd(base.basename, ".pat.png");
		}

		public int width;

		public int height;

		public int pwidth;

		public int pheight;

		public int pclms = 1;

		public int prows = 1;

		public int iclms = 1;

		public int irows = 1;

		public int len;

		private M2ChipPattern.PatData[] APd;

		private bool prepared_;

		public bool thumbnail_fullsize;

		public M2ChipImageNestedParent NestedParent;

		public const char pat_head_char = '&';

		private int pre_prows;

		private int pre_pclms;

		public struct PatData
		{
			public PatData(M2ImageContainer IMGS, uint _chip_id, int _direcs)
			{
				this.chip_id = _chip_id;
				this.direcs = _direcs;
				this.Img = ((IMGS != null && this.chip_id > 0U) ? IMGS.GetById(this.chip_id) : null);
			}

			public PatData(IM2CLItem _Img, int _direcs)
			{
				this.direcs = _direcs;
				this.Img = null;
				this.chip_id = 0U;
				this.Set(_Img);
			}

			public void Set(IM2CLItem _Img)
			{
				this.Img = _Img;
				this.chip_id = ((this.Img != null) ? this.Img.getChipId() : 0U);
			}

			public bool valid
			{
				get
				{
					return this.chip_id > 0U;
				}
			}

			public uint chip_id;

			public IM2CLItem Img;

			public int direcs;
		}

		public struct PatDataInput
		{
			public IM2CLItem Img;

			public int mapx;

			public int mapy;

			public int rotation;

			public bool flip;
		}
	}
}

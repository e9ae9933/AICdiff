using System;
using System.Collections.Generic;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace m2d
{
	public sealed class M2SmartImagePiece
	{
		public M2SmartImagePiece(int reserve = 4)
		{
			this.Aputs_id = new uint[reserve];
			this.Adirecs = new int[reserve];
		}

		public M2SmartImagePiece(uint[] _Aputs_id, int[] _Adirecs, int _MAX)
		{
			this.Aputs_id = _Aputs_id;
			this.Adirecs = _Adirecs;
			this.MAX = _MAX;
		}

		public M2SmartImagePiece Rotate(int d)
		{
			M2SmartImagePiece m2SmartImagePiece = new M2SmartImagePiece(this.MAX)
			{
				copied = true
			};
			for (int i = 0; i < this.MAX; i++)
			{
				int num = this.Adirecs[i];
				int num2 = num & 4;
				num = ((num & 3) + d + 16) % 4;
				m2SmartImagePiece.Add(this.Aputs_id[i], num | num2, false);
			}
			return m2SmartImagePiece;
		}

		public M2Chip MakeChip(M2ImageContainer IMGS, M2MapLayer Lay, int x, int y, int opacity, int i, ref int cclms, ref int crows)
		{
			int num = this.Adirecs[i];
			IM2CLItem byId = IMGS.GetById(this.Aputs_id[i]);
			if (byId == null)
			{
				return null;
			}
			List<M2Chip> list = byId.MakeChip(Lay, x, y, opacity, num & 3, (num & 4) > 0);
			if (list == null)
			{
				Vector2 vector = byId.getClmsAndRows(INPUT_CR.SNAP);
				cclms = (int)vector.x;
				crows = (int)vector.y;
				return null;
			}
			cclms = list[0].clms;
			crows = list[0].rows;
			return list[0];
		}

		public void Add(uint chip_id, int direc, bool checking_dupe = false)
		{
			if (chip_id == 0U)
			{
				return;
			}
			if (checking_dupe)
			{
				for (int i = 0; i < this.MAX; i++)
				{
					if (this.Adirecs[i] == direc && this.Aputs_id[i] == chip_id)
					{
						return;
					}
				}
			}
			if (this.MAX >= this.Aputs_id.Length)
			{
				Array.Resize<uint>(ref this.Aputs_id, this.MAX + 4);
				Array.Resize<int>(ref this.Adirecs, this.MAX + 4);
			}
			this.Aputs_id[this.MAX] = chip_id;
			int[] adirecs = this.Adirecs;
			int max = this.MAX;
			this.MAX = max + 1;
			adirecs[max] = direc;
		}

		public M2SmartImagePiece Rem()
		{
			this.MAX = 0;
			for (int i = this.Aputs_id.Length - 1; i >= 0; i--)
			{
				this.Aputs_id[i] = 0U;
			}
			return this;
		}

		public M2SmartImagePiece AddFrom(M2SmartImagePiece Smi, M2ImageContainer IMGS, bool checking_dupe = true)
		{
			int count = Smi.count;
			for (int i = 0; i < count; i++)
			{
				IM2CLItem im2CLItem = Smi.Get(IMGS, i);
				this.Add((im2CLItem != null) ? im2CLItem.getChipId() : 0U, Smi.getDir(i), checking_dupe);
			}
			return this;
		}

		public int count
		{
			get
			{
				return this.MAX;
			}
		}

		public M2ChipImage getFirstImage(M2ImageContainer IMGS)
		{
			if (this.MAX <= 0)
			{
				return null;
			}
			IM2CLItem byId = IMGS.GetById(this.Aputs_id[0]);
			if (byId == null)
			{
				return null;
			}
			return byId.getFirstImage();
		}

		public IM2CLItem getLast(M2ImageContainer IMGS)
		{
			if (this.MAX <= 0)
			{
				return null;
			}
			IM2CLItem byId = IMGS.GetById(this.Aputs_id[this.MAX - 1]);
			if (byId == null)
			{
				return null;
			}
			return byId.getFirstImage();
		}

		public IM2CLItem spoitLast(M2ImageContainer IMGS, ref int rotation, ref bool flip)
		{
			if (this.MAX == 0)
			{
				return null;
			}
			rotation = this.Adirecs[this.MAX - 1] & 3;
			flip = (this.Adirecs[this.MAX - 1] & 4) >= 1;
			return IMGS.GetById(this.Aputs_id[this.MAX - 1]);
		}

		public IM2CLItem Get(M2ImageContainer IMGS, int i)
		{
			if (this.MAX > 0 && i < this.MAX)
			{
				return IMGS.GetById(this.Aputs_id[i]);
			}
			return null;
		}

		public int getDir(int i)
		{
			return this.Adirecs[i];
		}

		public static M2SmartImagePiece readFromBytes(ByteArray Ba, byte load_ver, bool create = true)
		{
			int num = Ba.readByte();
			uint[] array = null;
			int[] array2 = null;
			if (create)
			{
				array = new uint[num];
				array2 = new int[num];
			}
			for (int i = 0; i < num; i++)
			{
				uint num2 = Ba.readUInt();
				int num3 = Ba.readByte();
				if (create)
				{
					array[i] = num2;
					array2[i] = num3;
				}
			}
			if (create)
			{
				return new M2SmartImagePiece(array, array2, num);
			}
			return null;
		}

		public void writeToBytes(ByteArray Ba)
		{
			Ba.writeByte(this.MAX);
			for (int i = 0; i < this.MAX; i++)
			{
				Ba.writeUInt(this.Aputs_id[i]);
				Ba.writeByte(this.Adirecs[i]);
			}
		}

		public void LinkSources(string key, M2ImageContainer IMGS, bool need_fine_editor_buttons)
		{
			for (int i = this.MAX - 1; i >= 0; i--)
			{
				IMGS.LinkSourcesForInputtable(IMGS.GetById(this.Aputs_id[i]), key, need_fine_editor_buttons);
			}
		}

		public void UnlinkSources(string key, M2ImageContainer IMGS, bool need_fine_editor_buttons)
		{
			for (int i = this.MAX - 1; i >= 0; i--)
			{
				IMGS.UnlinkSourcesForInputtable(IMGS.GetById(this.Aputs_id[i]), key, need_fine_editor_buttons);
			}
		}

		public bool isBg(M2ImageContainer IMGS)
		{
			for (int i = this.MAX - 1; i >= 0; i--)
			{
				IM2CLItem byId = IMGS.GetById(this.Aputs_id[i]);
				if (byId != null && byId.isBg())
				{
					return false;
				}
			}
			return true;
		}

		private uint[] Aputs_id;

		private int[] Adirecs;

		private int MAX;

		public bool copied;
	}
}

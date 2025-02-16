using System;
using System.Collections.Generic;
using Better;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace XX.mobpxl
{
	public class MobSklt
	{
		private MobSklt(int parts_max)
		{
			this.Msp = new MobSkltPosition("-Base", null);
			this.AParts = new List<SkltParts>(parts_max);
			this.AImgSorted = new List<SkltImage>(10);
			this.OACCBase = new BDic<string, MobPCCContainer.ACC>();
			this.ASq = new List<SkltSequence>(2);
		}

		internal MobSklt(string _name, PxlFrame _DefSkltFrame = null)
			: this(10)
		{
			this.name = _name;
			this.DefSkltFrame = _DefSkltFrame;
			if (this.DefSkltFrame != null)
			{
				this.baseline = (float)(-(float)this.DefSkltFrame.pSq.height) * 0.5f;
			}
			for (int i = 0; i < 10; i++)
			{
				this.AParts.Add(new SkltParts(this, (PARTS_TYPE)i, (i == 6) ? 5 : 1, 1));
			}
		}

		public void destruct()
		{
			for (int i = this.ASq.Count - 1; i >= 0; i--)
			{
				this.ASq[i].referred--;
			}
		}

		internal bool readFromBytes(MobGenerator Gen, ByteArray Ba, int vers = 10)
		{
			SkltImageSrc skltImageSrc = null;
			this.need_sort_image = true;
			int num = Ba.readByte();
			while (this.AParts.Count < num)
			{
				this.AParts.Add(null);
			}
			for (int i = 0; i < num; i++)
			{
				SkltParts skltParts = this.AParts[i];
				int num2 = (int)Ba.readUShort();
				short num3 = 0;
				string text = "";
				if (vers >= 2)
				{
					num3 = Ba.readShort();
				}
				if (vers >= 5)
				{
					text = Ba.readPascalString("utf-8", false);
				}
				if (num2 != 0)
				{
					if (skltParts == null)
					{
						skltParts = (this.AParts[i] = new SkltParts(this, (PARTS_TYPE)i, 1, num2));
					}
					skltParts.sort_order = num3;
					skltParts.ptype = text;
					for (int j = 0; j < num2; j++)
					{
						SkltImage skltImage = SkltImage.readFromBytes(Gen, skltParts, Ba, null, true, vers);
						if (skltImage != null)
						{
							skltParts.AImage.Add(skltImage);
							if (skltImage.Source.is_base_body)
							{
								skltImageSrc = skltImage.Source;
							}
						}
					}
				}
			}
			MobPCCContainer.readFromBytes(Ba, ref this.OACCBase, Gen.getBaseCharacter());
			bool flag = false;
			if (skltImageSrc != null)
			{
				if (Gen.getBasicSkltForBodySrc(skltImageSrc, this) != null)
				{
					flag = true;
				}
				else
				{
					X.de("Joint 対象が不明:" + this.name, null);
				}
			}
			if (vers >= 4)
			{
				if (vers >= 10)
				{
					MobSkltPosition.readFromBytes(Ba, this.Msp);
				}
				for (int k = 0; k < num; k++)
				{
					SkltParts.readFromBytesJoint(this, Ba, (PARTS_TYPE)k, true);
				}
			}
			else
			{
				for (int l = 0; l < num; l++)
				{
					for (int m = 0; m < 2; m++)
					{
						SkltParts.readFromBytesJoint(this, Ba, (PARTS_TYPE)l, m == 0);
					}
				}
			}
			int num4 = Ba.readByte();
			for (int n = 0; n < num4; n++)
			{
				SkltSequence sequence = Gen.getSequence(Ba.readPascalString("utf-8", false));
				if (sequence != null)
				{
					this.addAnimSequence(sequence, -1);
				}
			}
			if (vers >= 3)
			{
				this.Size.x = Ba.readFloat();
				this.Size.y = Ba.readFloat();
				this.Size.width = Ba.readFloat();
				this.Size.height = Ba.readFloat();
			}
			else
			{
				this.Size.Set(0f, 0f, 80f, 200f);
			}
			return flag;
		}

		internal SkltParts this[PARTS_TYPE j]
		{
			get
			{
				int count = this.AParts.Count;
				for (int i = 0; i < count; i++)
				{
					SkltParts skltParts = this.AParts[i];
					if (skltParts != null && skltParts.type == j)
					{
						return skltParts;
					}
				}
				return null;
			}
		}

		internal SkltParts MakeParts(PARTS_TYPE j)
		{
			while (this.AParts.Count <= (int)j)
			{
				this.AParts.Add(null);
			}
			if (this.AParts[(int)j] == null)
			{
				this.AParts[(int)j] = new SkltParts(this, j, 1, 1);
			}
			return this.AParts[(int)j];
		}

		public void ClearJoint()
		{
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					skltParts.ClearJoint();
				}
			}
		}

		public SkltImage getImage(int i, int j, bool slip_to_next_image = false)
		{
			if (slip_to_next_image)
			{
				for (int k = 0; k < this.parts_max; k++)
				{
					SkltParts skltParts = this.AParts[(i + k) % this.parts_max];
					if (skltParts != null && skltParts.AImage.Count != 0)
					{
						return skltParts.AImage[X.MMX(0, j, skltParts.AImage.Count - 1)];
					}
					j = 0;
				}
				return null;
			}
			if (!X.BTW(0f, (float)i, (float)this.parts_max))
			{
				return null;
			}
			SkltParts skltParts2 = this.AParts[i];
			if (skltParts2 == null || !X.BTW(0f, (float)j, (float)skltParts2.AImage.Count))
			{
				return null;
			}
			return skltParts2.AImage[j];
		}

		internal void fineJointPositionOnLoad()
		{
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					skltParts.fineJointPositionOnLoad();
				}
			}
		}

		public int parts_max
		{
			get
			{
				return this.AParts.Count;
			}
		}

		internal bool fineSort(bool force = false, bool execute_fine_position = true)
		{
			if (!force && !this.need_sort_image_)
			{
				return false;
			}
			this.AImgSorted.Clear();
			this.need_sort_image_ = false;
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					int count2 = skltParts.AImage.Count;
					int num = i * 256;
					for (int j = 0; j < count2; j++)
					{
						SkltImage skltImage = skltParts.AImage[j];
						skltImage.sort_index = num++;
						this.AImgSorted.Add(execute_fine_position ? skltImage.finePosition() : skltImage);
					}
				}
			}
			this.AImgSorted.Sort((SkltImage SiA, SkltImage SiB) => SiA.calced_sort_order - SiB.calced_sort_order);
			return true;
		}

		internal void fineImagePositionAll()
		{
			this.fineSort(false, false);
			this.Msp.finePositionBase();
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					skltParts.sortPartsJointAll();
				}
			}
			for (int j = this.AImgSorted.Count - 1; j >= 0; j--)
			{
				this.AImgSorted[j].finePosition();
			}
		}

		internal void clearTextureAtlas()
		{
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					int count2 = skltParts.AImage.Count;
					for (int j = 0; j < count2; j++)
					{
						skltParts.AImage[j].clearTextureAtlas();
					}
				}
			}
			this.atlas_created = false;
		}

		internal void createAtlas(RectAtlasTexture CalcAtlas)
		{
			if (this.atlas_created)
			{
				return;
			}
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					int count2 = skltParts.AImage.Count;
					for (int j = 0; j < count2; j++)
					{
						SkltImage skltImage = skltParts.AImage[j];
						if (!skltImage.atlas_created)
						{
							skltImage.createAtlas(CalcAtlas);
						}
					}
				}
			}
			this.atlas_created = true;
		}

		internal void createPccAppliedMesh(MobGenerator Gen, MeshDrawer Md, SkltImage Ignore_Image = null)
		{
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					int count2 = skltParts.AImage.Count;
					for (int j = 0; j < count2; j++)
					{
						SkltImage skltImage = skltParts.AImage[j];
						if (skltImage != Ignore_Image)
						{
							skltImage.createPccAppliedMesh(Gen, Md, true);
						}
					}
				}
			}
		}

		internal void addAnimSequence(SkltSequence Sq, int index = -1)
		{
			this.ASq.Insert((index < 0) ? this.ASq.Count : index, Sq);
			Sq.referred++;
		}

		public SkltSequence getAnim(int i)
		{
			if (this.ASq.Count <= i)
			{
				return null;
			}
			return this.ASq[i];
		}

		public SkltSequence getAnimByType(string type)
		{
			int count = this.ASq.Count;
			for (int i = 0; i < count; i++)
			{
				SkltSequence skltSequence = this.ASq[i];
				if (skltSequence.type == type)
				{
					return skltSequence;
				}
			}
			return null;
		}

		public Vector2 getBasePosFor(MobSkltPosition.IPosSyncable POI)
		{
			if (POI is SkltParts)
			{
				return (POI as SkltParts).getJointBase().PosAbs;
			}
			if (POI is SkltImage)
			{
				return (POI as SkltImage).getJointBase().PosAbs;
			}
			return this.Msp.PosAbs;
		}

		public MobSkltPosition getBaseMspFor(MobSkltPosition.IPosSyncable POI)
		{
			return POI.getMsp();
		}

		public override string ToString()
		{
			return "<MobSklt>" + this.name;
		}

		public bool is_default_skeleton
		{
			get
			{
				return this.DefSkltFrame != null;
			}
		}

		public bool need_sort_image
		{
			get
			{
				return this.need_sort_image_;
			}
			set
			{
				if (value)
				{
					this.need_sort_image_ = true;
					this.finePartsSize();
				}
			}
		}

		public void finePartsSize()
		{
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					skltParts.fine_parts_size = true;
				}
			}
		}

		public const int old_skin_max = 2;

		internal string name;

		internal readonly List<SkltParts> AParts;

		internal readonly MobSkltPosition Msp;

		public Rect Size;

		public float baseline = -1000f;

		internal readonly List<SkltImage> AImgSorted;

		internal readonly List<SkltSequence> ASq;

		internal BDic<string, MobPCCContainer.ACC> OACCBase;

		private bool need_sort_image_ = true;

		public PxlFrame DefSkltFrame;

		public bool atlas_created;
	}
}

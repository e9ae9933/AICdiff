using System;
using System.Collections.Generic;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2ChipImage : IIdvName, IM2CLItem, IM2Inputtable
	{
		public int alloc_rots
		{
			get
			{
				return (int)(this.basic_flags & 15U);
			}
			set
			{
				this.basic_flags = (uint)((ulong)(this.basic_flags & 4294967280U) | (ulong)((long)value & 15L));
			}
		}

		public int alloc_flips
		{
			get
			{
				return (int)((this.basic_flags >> 4) & 3U);
			}
			set
			{
				this.basic_flags = (uint)((ulong)(this.basic_flags & 4294967247U) | (ulong)((ulong)((long)value & 3L) << 4));
			}
		}

		public int odd_flip
		{
			get
			{
				return (int)((this.basic_flags >> 6) & 1U);
			}
			set
			{
				this.basic_flags = (this.basic_flags & 4294967231U) | (((value != 0) ? 1U : 0U) << 6);
			}
		}

		public bool favorited
		{
			get
			{
				return ((this.basic_flags >> 7) & 1U) > 0U;
			}
			set
			{
				this.basic_flags = (this.basic_flags & 4294967167U) | ((value ? 1U : 0U) << 7);
			}
		}

		public bool is_background
		{
			get
			{
				return this.mesh_type == 0;
			}
		}

		public int mesh_type
		{
			get
			{
				return (int)((this.basic_flags >> 8) & 3U);
			}
			set
			{
				this.basic_flags = (uint)((ulong)(this.basic_flags & 4294966527U) | (ulong)((ulong)((long)value & 3L) << 8));
			}
		}

		public int mesh_layer
		{
			get
			{
				return global::XX.X.Mx(this.mesh_type - 1, 0);
			}
		}

		public bool need_fine_atlas
		{
			get
			{
				return ((this.basic_flags >> 13) & 1U) > 0U;
			}
			set
			{
				this.basic_flags = (this.basic_flags & 4294959103U) | ((value ? 1U : 0U) << 13);
			}
		}

		public bool need_fine_create_chip_fn
		{
			get
			{
				return ((this.basic_flags >> 14) & 1U) > 0U;
			}
			set
			{
				this.basic_flags = (this.basic_flags & 4294950911U) | ((value ? 1U : 0U) << 14);
			}
		}

		public bool need_fine_create_drawer_fn
		{
			get
			{
				return ((this.basic_flags >> 15) & 1U) > 0U;
			}
			set
			{
				this.basic_flags = (this.basic_flags & 4294934527U) | ((value ? 1U : 0U) << 15);
			}
		}

		public bool loaded_additional_material
		{
			get
			{
				return ((this.basic_flags >> 16) & 1U) > 0U;
			}
			set
			{
				this.basic_flags = (this.basic_flags & 4294901759U) | ((value ? 1U : 0U) << 16);
			}
		}

		public bool can_foot_on
		{
			get
			{
				return ((this.basic_flags >> 17) & 1U) > 0U;
			}
			set
			{
				this.basic_flags = (this.basic_flags & 4294836223U) | ((value ? 1U : 0U) << 17);
			}
		}

		public bool has_remover_mesh
		{
			get
			{
				return ((this.basic_flags >> 18) & 1U) > 0U;
			}
			set
			{
				this.basic_flags = (this.basic_flags & 4294705151U) | ((value ? 1U : 0U) << 18);
			}
		}

		public int clms
		{
			get
			{
				return this.clms_;
			}
		}

		public int rows
		{
			get
			{
				return this.rows_;
			}
		}

		public bool isFavorited()
		{
			return this.favorited;
		}

		public string family
		{
			get
			{
				return this.IMGS.getFamilyName(this.family_index);
			}
			set
			{
				this.family_index = this.IMGS.getFamilyIndex(value);
			}
		}

		public int CLEN
		{
			get
			{
				return (int)this.IMGS.CLEN;
			}
		}

		public M2DBase M2D
		{
			get
			{
				return this.IMGS.M2D;
			}
		}

		private M2ChipImage(M2ImageContainer _IMGS)
		{
			this.IMGS = _IMGS;
			this.need_fine_create_chip_fn = (this.need_fine_create_drawer_fn = (this.need_fine_atlas = true));
			this.mesh_type = 1;
		}

		public M2ChipImage(M2ImageContainer _IMGS, string _src)
			: this(_IMGS)
		{
			this.src_ = _src;
			this.dirname = global::XX.X.dirname(_src);
			this.basename = global::XX.X.basename_noext(_src);
		}

		public M2ChipImage(M2ImageContainer _IMGS, string _dirname, string _basename_noext)
			: this(_IMGS)
		{
			this.dirname = _dirname;
			this.basename = _basename_noext;
		}

		public string src
		{
			get
			{
				if (this.src_ == null)
				{
					this.src_ = this.dirname + this.basename + ".png";
				}
				return this.src_;
			}
		}

		public static M2ChipImage readFromBytes(ByteArray Ba, byte load_ver, M2ImageContainer IMGS, string dirname, bool create = true)
		{
			string text = Ba.readPascalString("utf-8", false);
			if (load_ver <= 6)
			{
				text = global::XX.X.basename_noext(text);
			}
			int num = (int)Ba.readUShort();
			int num2 = (int)Ba.readUShort();
			int num3 = Ba.readByte();
			int num4 = Ba.readByte();
			int num5 = (int)Ba.readShort();
			uint num6 = Ba.readUInt();
			int num7 = (int)Ba.readUShort();
			byte[] array = null;
			if (create)
			{
				array = new byte[num7];
				for (int i = 0; i < num7; i++)
				{
					int num8 = Ba.readByte();
					array[i] = (byte)num8;
				}
			}
			else
			{
				Ba.position += (ulong)((long)num7);
			}
			ushort num9 = Ba.readUShort();
			METACImg metacimg = null;
			uint num10 = ((load_ver <= 8) ? ((uint)Ba.readUShort()) : Ba.readUInt());
			int num11 = 0;
			if (load_ver == 1)
			{
				metacimg = new METACImg(null);
				metacimg.readFromBytes(Ba, load_ver);
				if (create)
				{
					IMGS.getCImgMetaIndex(ref metacimg);
				}
				num10 ^= 256U;
			}
			else
			{
				metacimg = IMGS.getCImgMeta(Ba.readUShort());
				num11 = Ba.readByte();
			}
			if (create)
			{
				M2ChipImage m2ChipImage = new M2ChipImage(IMGS, dirname, text)
				{
					iwidth = num,
					iheight = num2,
					shiftx = num3,
					shifty = num4,
					horizony = num5,
					chip_id = num6,
					family_index = num9
				};
				m2ChipImage.setMeta(metacimg);
				m2ChipImage.basic_flags = (m2ChipImage.basic_flags & 4294965248U) | (num10 & 2047U);
				m2ChipImage.fineWH();
				m2ChipImage.Aconfig = array;
				if (num11 > 0)
				{
					m2ChipImage.ACIMesh = new M2ChipImage.CIMesh[num11];
				}
				return m2ChipImage.refineConfig();
			}
			return null;
		}

		public void releaseSourceLayer()
		{
			this.SourceLayer_ = null;
			if (this.ACIMesh != null)
			{
				for (int i = this.ACIMesh.Length - 1; i >= 0; i--)
				{
					this.ACIMesh[i] = default(M2ChipImage.CIMesh);
				}
			}
		}

		public virtual void assignLayerAndAtlas(PxlLayer Lay, M2ImageAtlas.AtlasRect _Atlas, bool apply_basic = false)
		{
			this.SourceLayer_ = Lay;
			this.SourceAtlas_ = _Atlas;
			if (apply_basic)
			{
				this.Aconfig = new byte[] { 4 };
				this.clms_ = 1;
				this.rows_ = 1;
				if (Lay == null || Lay.Img == null)
				{
					global::XX.X.de("イメージ不明: " + this.src, null);
				}
				this.iwidth = Lay.Img.width;
				this.iheight = Lay.Img.height;
				this.getChipFixedShiftPos(false, 3, 1);
				this.getChipFixedShiftPos(true, 3, 1);
				this.horizony = global::XX.X.IntR((float)this.iheight * 0.5f);
				this.fineWH();
				this.ImgMain = null;
			}
			else
			{
				PxlImage img = Lay.Img;
				if (this.iwidth != img.width || this.iheight != img.height)
				{
					this.iwidth = img.width;
					this.iheight = img.height;
					this.fineWH();
					this.ImgMain = null;
				}
			}
			if (this.SourceAtlas_.valid)
			{
				this.recreateImageMesh(false);
			}
		}

		public void copyBasicAttributesFrom(M2ChipImage Src)
		{
			this.family_index = Src.family_index;
			this.setMeta(Src.Meta);
			this.basic_flags = (this.basic_flags & 4294965248U) | (Src.basic_flags & 2047U);
			this.can_foot_on = Src.can_foot_on;
		}

		public virtual M2ChipImage copyAttributesFrom(M2ChipImage Src)
		{
			Src.prepareImageMesh();
			if (this.iheight == Src.iheight && this.iwidth == Src.iwidth)
			{
				this.shiftx = Src.shiftx;
				this.shifty = Src.shifty;
				this.horizony = Src.horizony;
				this.fineWH();
			}
			this.copyBasicAttributesFrom(Src);
			if (this.clms_ == Src.clms_ && this.rows_ == Src.rows_)
			{
				this.Aconfig = global::XX.X.concat<byte>(Src.Aconfig, null, -1, -1);
			}
			else if (this.is_background)
			{
				int num = this.Aconfig.Length;
				for (int i = 0; i < num; i++)
				{
					this.Aconfig[i] = 10;
				}
			}
			this.ImgMain = null;
			return this;
		}

		public int fineRotFlip(int rotf, ref DebugLogBlockBase DBlk)
		{
			int num = rotf & 3;
			int alloc_rots = this.alloc_rots;
			int odd_flip = this.odd_flip;
			bool flag = (rotf & 4) > 0;
			int num2 = num;
			bool flag2 = flag;
			for (int i = 0; i < 4; i++)
			{
				if ((alloc_rots & (1 << num2)) == 0)
				{
					num2 = (num2 + 1) % 4;
				}
			}
			int num3 = ((odd_flip != 0) ? ((num2 % 2 == 1) ? 2 : 1) : this.alloc_flips);
			for (int j = 0; j < 2; j++)
			{
				if ((num3 & ((!flag2 || 2 != 0) ? 1 : 0)) == 0)
				{
					flag2 = !flag2;
				}
			}
			if (num2 != num || flag2 != flag)
			{
				DBlk = global::XX.X.dl("回転フィックス => rot:" + num2.ToString() + " flip: " + flag2.ToString(), DBlk, false, false);
			}
			return num2 + (flag2 ? 4 : 0);
		}

		public virtual M2ChipImage fineWH()
		{
			int num = this.clms_;
			int num2 = this.rows_;
			this.clms_ = global::XX.X.IntC((float)this.width / (float)this.CLEN);
			this.rows_ = global::XX.X.IntC((float)this.height / (float)this.CLEN);
			this.horizony = global::XX.X.MMX(0, this.horizony, this.iheight - 1);
			if (this.Aconfig == null)
			{
				return this;
			}
			int num3 = this.clms_ * this.rows_;
			if (this.Aconfig.Length != num3 || num != this.clms_ || num2 != this.rows_)
			{
				this.Aconfig = global::XX.X.copyTwoDimensionArray<byte>(new byte[num3], this.clms_, this.rows_, this.Aconfig, num, num2, true);
			}
			return this;
		}

		public Vector2Int getShift(int rot, bool flip)
		{
			int num = (flip ? (-1) : 1);
			if (rot == 1)
			{
				M2ChipImage.Ash[0] = -1;
				M2ChipImage.Ash[1] = num;
			}
			else if (rot == 2)
			{
				M2ChipImage.Ash[0] = -1 * num;
				M2ChipImage.Ash[1] = -1;
			}
			else if (rot == 3)
			{
				M2ChipImage.Ash[0] = 1;
				M2ChipImage.Ash[1] = -1 * num;
			}
			else
			{
				M2ChipImage.Ash[0] = num;
				M2ChipImage.Ash[1] = 1;
			}
			int num2;
			int num3;
			int num4;
			int num5;
			int num6;
			int num7;
			if (rot == 1 || rot == 3)
			{
				num2 = this.rows_ * this.CLEN;
				num3 = this.clms_ * this.CLEN;
				num4 = this.iheight;
				num5 = this.iwidth;
				num6 = this.shifty;
				num7 = this.shiftx;
			}
			else
			{
				num2 = this.clms_ * this.CLEN;
				num3 = this.rows_ * this.CLEN;
				num4 = this.iwidth;
				num5 = this.iheight;
				num6 = this.shiftx;
				num7 = this.shifty;
			}
			if (M2ChipImage.Ash[0] < 0)
			{
				M2ChipImage.Ash[0] = num2 - num4 - num6;
			}
			else
			{
				M2ChipImage.Ash[0] = num6;
			}
			if (M2ChipImage.Ash[1] < 0)
			{
				M2ChipImage.Ash[1] = num3 - num5 - num7;
			}
			else
			{
				M2ChipImage.Ash[1] = num7;
			}
			return M2ChipImage.Ash;
		}

		public int width
		{
			get
			{
				return this.iwidth + this.shiftx;
			}
		}

		public int height
		{
			get
			{
				return this.iheight + this.shifty;
			}
		}

		public Vector2Int getRWH(int rot)
		{
			int num = ((rot == 1 || rot == 3) ? 0 : 1);
			M2ChipImage.Arw[num] = this.rows_ * this.CLEN;
			M2ChipImage.Arw[1 - num] = this.clms_ * this.CLEN;
			return M2ChipImage.Arw;
		}

		public void prepareImageMesh()
		{
			if (this.ImgMain == null && this.SourceAtlas_.valid)
			{
				this.recreateImageMesh(false);
			}
		}

		public M2ChipImage refineConfig()
		{
			this.can_foot_on = false;
			int num = this.Aconfig.Length;
			for (int i = 0; i < num; i++)
			{
				if (CCON.canFootOn((int)this.Aconfig[i], null))
				{
					this.can_foot_on = true;
					break;
				}
			}
			return this;
		}

		public void recreateImageMesh(M2ImageAtlas.AtlasRect _SourceAtlas)
		{
			this.SourceAtlas_ = _SourceAtlas;
			this.recreateImageMesh(false);
		}

		public virtual void recreateImageMesh(bool no_error = false)
		{
			if (this.SourceLayer_ == null || this.width == 0)
			{
				if (!no_error)
				{
					global::XX.X.de("チップリストに登録されていない画像: " + this.src, null);
				}
				return;
			}
			if (this.IMGS.loading_version <= 1)
			{
				int num = this.Aconfig.Length;
				M2ChipImage.Pt = default(Vector2);
				M2ChipImage.Rc = new Rect(0f, 0f, (float)this.CLEN, (float)this.CLEN);
				M2ChipImage.ARcUse = new Rect[3];
				M2ChipImage.ARcCopyng = new Rect[3, num];
				M2ChipImage.Acopied = new int[3];
				M2ChipImage._ignored = 0;
				for (int i = 0; i < num; i++)
				{
					int num2 = (int)this.Aconfig[i];
					if (CCON.canFootOn(num2, null))
					{
						this.can_foot_on = true;
					}
					int j = (CCON.isCeil(num2) ? 2 : (CCON.isFloor(num2) ? 0 : 1));
					if (num2 == 0)
					{
						M2ChipImage._ignored++;
					}
					else
					{
						this.copyBlock(i % this.clms_, i / this.clms_, this.shiftx, this.shifty, j);
					}
				}
				int num3 = 0;
				for (int j = 0; j < 3; j++)
				{
					if (M2ChipImage.Acopied[j] != 0)
					{
						PxlMeshDrawer pxlMeshDrawer = (this.SourceAtlas_.valid ? this.extractblock(j) : null);
						if (num3 == 0)
						{
							num3 |= 1 << j;
							this.ImgMain = pxlMeshDrawer;
							if (j >= 1)
							{
								if (this.is_background)
								{
									global::XX.X.dl(string.Concat(new string[]
									{
										"! ",
										this.src,
										" は背景指定されていますが、レイヤー",
										j.ToString(),
										"が使用されます。"
									}), null, false, false);
								}
								this.mesh_type = j + 1;
							}
							else
							{
								this.mesh_type = (this.is_background ? 0 : 1);
							}
						}
						else
						{
							global::XX.X.de("! " + this.src + " に複数レイヤーが指定されています。", null);
						}
					}
				}
				M2ChipImage.ARcCopyng = null;
				M2ChipImage.ARcUse = null;
				M2ChipImage.Acopied = null;
			}
			else if (this.SourceAtlas_.valid)
			{
				this.ImgMain = this.SourceAtlas_.makeMesh(this.IMGS.MIchip, 0f, 0f, this.ImgMain);
			}
			if (this.ImgMain != null)
			{
				this.need_fine_atlas = false;
			}
		}

		public void initAdditionalLayer(PxlLayer Lay, int layer, M2ImageAtlas.AtlasRect nAtlas, bool is_remover = false)
		{
			if (layer == 0)
			{
				if (!is_remover && this.mesh_type <= 1)
				{
					global::XX.X.de("メッシュレイヤー0コンフリクト " + this.src, null);
					return;
				}
			}
			else if (layer == 2 && !is_remover && this.mesh_type == 3)
			{
				global::XX.X.de("メッシュレイヤー2コンフリクト " + this.src, null);
				return;
			}
			this.MeshPushFromAdditionalLayer(Lay, nAtlas, layer | (is_remover ? 32 : 0));
		}

		public PxlMeshDrawer getMainMesh()
		{
			return this.ImgMain;
		}

		protected virtual void MeshPushFromAdditionalLayer(PxlLayer Lay, M2ImageAtlas.AtlasRect nAtlas, int layer)
		{
			this.MeshPush(Lay, nAtlas, layer);
		}

		protected void MeshPush(PxlLayer Lay, M2ImageAtlas.AtlasRect nAtlas, int layer)
		{
			if (!nAtlas.valid)
			{
				return;
			}
			PxlMeshDrawer pxlMeshDrawer = nAtlas.makeMesh(this.IMGS.MIchip, Lay.rot_center_x - this.SourceLayer_.rot_center_x, -(Lay.rot_center_y - this.SourceLayer_.rot_center_y), Lay.zmx, Lay.zmy, Lay.rotR, this.getSrcMesh(layer));
			this.MeshPush(pxlMeshDrawer, layer);
		}

		public void MeshPush(PxlMeshDrawer Img, int layer)
		{
			int i = 0;
			if (this.ACIMesh == null)
			{
				this.ACIMesh = new M2ChipImage.CIMesh[1];
			}
			else
			{
				int num = this.ACIMesh.Length;
				for (i = 0; i < num; i++)
				{
					M2ChipImage.CIMesh cimesh = this.ACIMesh[i];
					if (cimesh.Img == null || (int)cimesh.layer == layer)
					{
						break;
					}
				}
				if (i >= num)
				{
					Array.Resize<M2ChipImage.CIMesh>(ref this.ACIMesh, i + 1);
				}
			}
			this.ACIMesh[i] = new M2ChipImage.CIMesh(Img, layer);
			if ((layer & 32) != 0)
			{
				this.has_remover_mesh = true;
			}
		}

		private void copyBlock(int x, int y, int shiftx, int shifty, int _layer)
		{
			M2ChipImage.Rc.x = (M2ChipImage.Pt.x = (float)(x = x * this.CLEN - shiftx));
			M2ChipImage.Rc.y = (M2ChipImage.Pt.y = (float)(y = y * this.CLEN - shifty));
			Rect rect = M2ChipImage.ARcUse[_layer];
			Rect rect2 = new Rect((float)x, (float)y, (float)this.CLEN, (float)this.CLEN);
			if (M2ChipImage.Acopied[_layer] == 0)
			{
				rect = rect2;
			}
			else
			{
				float num = rect.x + rect.width;
				float num2 = rect.y + rect.height;
				rect.x = global::XX.X.Mn(rect.x, (float)x);
				rect.y = global::XX.X.Mn(rect.y, (float)y);
				num = global::XX.X.Mx(num, (float)(x + this.CLEN));
				num2 = global::XX.X.Mx(num2, (float)(y + this.CLEN));
				rect.width = num - rect.x;
				rect.height = num2 - rect.y;
			}
			M2ChipImage.ARcUse[_layer] = rect;
			M2ChipImage.ARcCopyng[_layer, M2ChipImage.Acopied[_layer]] = rect2;
			M2ChipImage.Acopied[_layer]++;
		}

		private PxlMeshDrawer extractblock(int _layer)
		{
			return null;
		}

		private float calc_sprite_vertice_x(float x)
		{
			if (this.shiftx > 0)
			{
				if (x == 0f)
				{
					return 0f;
				}
				x -= 1f;
			}
			return global::XX.X.Mn((float)this.shiftx + x * (float)this.CLEN, (float)this.SourceAtlas_.w);
		}

		private float calc_sprite_vertice_y(float y)
		{
			if (this.shifty > 0)
			{
				if (y == 0f)
				{
					return (float)this.SourceAtlas_.h;
				}
				y -= 1f;
			}
			return global::XX.X.Mx(0f, (float)this.SourceAtlas_.h - ((float)this.shifty + y * (float)this.CLEN));
		}

		public virtual void AtlasRescale(float x, float y)
		{
			if (x <= 0f || (x == 1f && y == 1f))
			{
				return;
			}
			int num = ((this.ACIMesh != null) ? this.ACIMesh.Length : 0);
			for (int i = -1; i < num; i++)
			{
				this.AtlasRescaleExecute((i == -1) ? this.ImgMain : this.ACIMesh[i].Img, x, y);
			}
		}

		protected void AtlasRescaleExecute(PxlMeshDrawer Ms, float x, float y)
		{
			if (Ms == null)
			{
				return;
			}
			int num;
			Vector2[] rawUvArray = Ms.getRawUvArray(out num);
			for (int i = 0; i < num; i++)
			{
				Vector2 vector = rawUvArray[i];
				vector.x *= x;
				vector.y *= y;
				rawUvArray[i] = vector;
			}
		}

		public void flushAdditinal()
		{
			this.loaded_additional_material = false;
		}

		public virtual void finalizeCreating()
		{
			if (this.Meta == null)
			{
				this.setMeta(this.IMGS.getCImgMeta(ushort.MaxValue));
			}
		}

		public virtual bool isWithinOnPicture(float px_x, float px_y)
		{
			return global::XX.X.BTW(0f, px_x, (float)this.iwidth) && global::XX.X.BTW(0f, px_y, (float)this.iheight);
		}

		public PxlMeshDrawer getSrcMesh(int layer)
		{
			int num = this.mesh_type - 1;
			if ((layer == 0) ? (num <= 0) : (layer == num))
			{
				this.prepareImageMesh();
				return this.ImgMain;
			}
			if (this.ACIMesh == null)
			{
				return null;
			}
			int num2 = this.ACIMesh.Length;
			byte b = (byte)layer;
			for (int i = 0; i < num2; i++)
			{
				M2ChipImage.CIMesh cimesh = this.ACIMesh[i];
				if (cimesh.Img != null && cimesh.layer == b)
				{
					return cimesh.Img;
				}
			}
			return null;
		}

		public virtual PxlMeshDrawer getSrcMeshForPicture(int layer, uint pattern_for_picture)
		{
			return this.getSrcMesh(layer);
		}

		public PxlMeshDrawer MeshT
		{
			get
			{
				return this.getSrcMesh(2);
			}
		}

		public PxlMeshDrawer MeshG
		{
			get
			{
				return this.getSrcMesh(1);
			}
		}

		public PxlMeshDrawer MeshB
		{
			get
			{
				return this.getSrcMesh(0);
			}
		}

		public bool allowMultipleInput()
		{
			return this.Meta != null && this.Meta.alloc_multiple_input;
		}

		public virtual bool initAtlasMd(MeshDrawer Md, uint pattern_for_picture = 0U)
		{
			if (!this.SourceAtlas_.valid)
			{
				return false;
			}
			this.SourceAtlas_.initAtlasMd(Md, this.IMGS.MIchip);
			return true;
		}

		public override string ToString()
		{
			return this.src;
		}

		public virtual int getChipFixedShiftPos(bool is_y, int id, int set_variable = 2)
		{
			int num = (is_y ? this.iheight : this.iwidth);
			int i = 0;
			switch (id)
			{
			case 1:
				i = global::XX.X.Int(((float)(global::XX.X.IntC((float)(num / this.CLEN)) * this.CLEN) - (float)num) * 0.5f);
				break;
			case 2:
				i = global::XX.X.Int((float)(global::XX.X.IntC((float)(num / this.CLEN)) * this.CLEN) - (float)num);
				break;
			case 3:
			{
				PxlLayer sourceLayer_ = this.SourceLayer_;
				i = (is_y ? sourceLayer_.cvs_top : sourceLayer_.cvs_left) % this.CLEN;
				break;
			}
			}
			while (i < 0)
			{
				i += (int)this.M2D.CLEN;
			}
			i %= (int)this.M2D.CLEN;
			if (set_variable > 0)
			{
				if (is_y)
				{
					this.shifty = i;
				}
				else
				{
					this.shiftx = i;
				}
				if (set_variable >= 2)
				{
					this.fineWH();
				}
			}
			return i;
		}

		public List<M2Chip> MakeChip(M2MapLayer Lay, int x, int y, int opacity, int rotation, bool flip)
		{
			M2Chip m2Chip = (M2Chip)Lay.MakeChip(this, this.shiftx, this.shifty, global::XX.X.Abs(opacity), 0, false);
			if (m2Chip == null)
			{
				return null;
			}
			m2Chip.rotation = rotation;
			m2Chip.flip = flip;
			m2Chip.mapx = x;
			m2Chip.mapy = y;
			m2Chip.inputRots(true);
			return new List<M2Chip>(1) { m2Chip };
		}

		public List<M2Picture> MakePicture(M2MapLayer Lay, float x, float y, int opacity, int rotation, bool flip)
		{
			int num = (int)(x * (float)this.CLEN);
			int num2 = (int)(y * (float)this.CLEN);
			M2Picture m2Picture = Lay.MakePicture(this, num, num2, opacity, rotation, flip, -1);
			if (m2Picture == null)
			{
				return null;
			}
			return new List<M2Picture>(1) { m2Picture.finePos((float)num, (float)num2, m2Picture.rotR, true) };
		}

		public M2Chip CreateOneChip(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip)
		{
			if (this.need_fine_create_chip_fn)
			{
				this.need_fine_create_chip_fn = false;
				this.fnCreateOneChip = this.IMGS.getCreateOneChipFunc(this);
			}
			if (this.fnCreateOneChip != null)
			{
				return this.fnCreateOneChip(_Lay, drawx, drawy, opacity, rotation, flip, this);
			}
			return new M2Chip(_Lay, drawx, drawy, opacity, rotation, flip, this);
		}

		public M2CImgDrawer CreateOneDrawer(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer, bool force_default = false)
		{
			if (!force_default)
			{
				if (this.need_fine_create_drawer_fn)
				{
					this.need_fine_create_drawer_fn = false;
					this.fnCreateOneDrawer = this.IMGS.getCreateOneDrawerFunc(this);
				}
				if (this.fnCreateOneDrawer != null)
				{
					return this.fnCreateOneDrawer(ref Md, lay, Cp, Meta, Pre_Drawer);
				}
			}
			return new M2CImgDrawer(Md, lay, Cp, false);
		}

		public M2ChipImage getFirstImage()
		{
			return this;
		}

		public virtual Vector2Int getClmsAndRows(INPUT_CR cr)
		{
			if (cr == INPUT_CR.SNAP_CHIPBTN && this.Meta.is_small_image)
			{
				return new Vector2Int(global::XX.X.IntC((float)this.iwidth / (float)this.CLEN), global::XX.X.IntC((float)this.iheight / (float)this.CLEN));
			}
			if (cr != INPUT_CR.PIXELS)
			{
				return new Vector2Int(this.clms_, this.rows_);
			}
			return new Vector2Int(1, 1);
		}

		public void getPutsBounds(M2Puts Mcp, float CLEN, out int l, out int t, out int r, out int b)
		{
			if (Mcp is M2Picture && this.SourceLayer != null)
			{
				float mapcx = Mcp.mapcx;
				float mapcy = Mcp.mapcy;
				M2ChipImage.RcBuf.Set(0f, 0f, 0f, 0f);
				float num = global::XX.X.Cos(Mcp.draw_rotR);
				float num2 = -global::XX.X.Sin(Mcp.draw_rotR);
				float num3 = 1f / CLEN;
				Vector2 vector = global::XX.X.ROTV2e(new Vector2((float)(-(float)this.SourceLayer.Img.width) * num3 * 0.5f, (float)this.SourceLayer.Img.height * num3 * 0.5f), num, num2);
				Vector2 vector2 = global::XX.X.ROTV2e(new Vector2((float)this.SourceLayer.Img.width * num3 * 0.5f, (float)this.SourceLayer.Img.height * num3 * 0.5f), num, num2);
				Vector2 vector3 = global::XX.X.ROTV2e(new Vector2((float)this.SourceLayer.Img.width * num3 * 0.5f, (float)(-(float)this.SourceLayer.Img.height) * num3 * 0.5f), num, num2);
				Vector2 vector4 = global::XX.X.ROTV2e(new Vector2((float)(-(float)this.SourceLayer.Img.width) * num3 * 0.5f, (float)(-(float)this.SourceLayer.Img.height) * num3 * 0.5f), num, num2);
				M2ChipImage.RcBuf.Expand(vector.x, vector.y, 0f, 0f, true);
				M2ChipImage.RcBuf.Expand(vector2.x, vector2.y, 0f, 0f, true);
				M2ChipImage.RcBuf.Expand(vector3.x, vector3.y, 0f, 0f, true);
				M2ChipImage.RcBuf.Expand(vector4.x, vector4.y, 0f, 0f, true);
				M2ChipImage.RcBuf.x += mapcx;
				M2ChipImage.RcBuf.y += mapcy;
				l = (int)M2ChipImage.RcBuf.x;
				t = (int)M2ChipImage.RcBuf.y;
				r = global::XX.X.IntC(M2ChipImage.RcBuf.right);
				b = global::XX.X.IntC(M2ChipImage.RcBuf.bottom);
				return;
			}
			l = Mcp.mapx;
			t = Mcp.mapy;
			r = global::XX.X.IntC((float)(Mcp.drawx + Mcp.iwidth) / CLEN);
			b = global::XX.X.IntC((float)(Mcp.drawy + Mcp.iheight) / CLEN);
		}

		public string getTitle()
		{
			return this.src;
		}

		public string getBaseName()
		{
			return this.basename;
		}

		public uint getChipId()
		{
			return this.chip_id;
		}

		public bool isSame(M2Puts Cp, bool in_editor = false, int editor_curs_x = -1, int editor_curs_y = -1)
		{
			return Cp.pattern == 0U && Cp.Img == this;
		}

		public bool isLinked()
		{
			return false;
		}

		public virtual Rect getClipArea(uint pattern_for_picture = 0U)
		{
			return new Rect(0f, 0f, (float)this.iwidth, (float)this.iheight);
		}

		public void MdInitImgBySourceLayer(MeshDrawer Md, uint pattern_for_picture = 0U)
		{
			if (this.SourceLayer != null)
			{
				PxlImage img = this.SourceLayer.Img;
				Texture i = img.get_I();
				if (i != null)
				{
					Rect rect = global::XX.X.rectDivide(this.getClipArea(pattern_for_picture), (float)i.width, (float)i.height);
					Rect rectIUv = img.RectIUv;
					Md.initForImg(i, new Rect(rectIUv.x + rect.x, rectIUv.y + rect.y, rect.width, rect.height), false);
				}
			}
		}

		public bool isBg()
		{
			return this.is_background;
		}

		public bool is_atlas_empty
		{
			get
			{
				return !this.SourceAtlas_.valid || this.SourceLayer_ == null;
			}
		}

		public PxlFrame SourceFrm
		{
			get
			{
				return this.SourceLayer_.pFrm;
			}
		}

		public string getDirName()
		{
			return this.dirname;
		}

		public string get_individual_key()
		{
			return this.src;
		}

		public METACImg Meta
		{
			get
			{
				return this.Meta_;
			}
		}

		public void setMeta(METACImg value)
		{
			if (this.Meta_ == value)
			{
				return;
			}
			this.Meta_ = value;
		}

		public M2ImageAtlas.AtlasRect SourceAtlas
		{
			get
			{
				return this.SourceAtlas_;
			}
		}

		public PxlLayer SourceLayer
		{
			get
			{
				return this.SourceLayer_;
			}
		}

		public M2DropObject.FnDropObjectDraw fnDrawForDropObject
		{
			get
			{
				if (this.SourceLayer == null)
				{
					return null;
				}
				return this.IMGS.Atlas.getFnDrawForDropObject(this.SourceLayer.Img);
			}
		}

		public readonly M2ImageContainer IMGS;

		private string src_;

		public readonly string basename;

		public readonly string dirname;

		public uint chip_id;

		public byte[] Aconfig;

		public int shiftx;

		public int shifty;

		public int horizony;

		public int iwidth;

		public int iheight;

		protected int clms_ = 1;

		protected int rows_ = 1;

		private static Vector2Int Ash;

		private static Vector2Int Arw;

		public ushort family_index = ushort.MaxValue;

		private METACImg Meta_;

		private M2ImageContainer.FnCreateOneChip fnCreateOneChip;

		private M2CImgDrawer.FnCreateDrawer fnCreateOneDrawer;

		private M2ChipImage.CIMesh[] ACIMesh;

		protected PxlMeshDrawer ImgMain;

		protected M2ImageAtlas.AtlasRect SourceAtlas_;

		protected PxlLayer SourceLayer_;

		private static DRect RcBuf = new DRect("RcBuf");

		public const int CIMESH_REMOVER_LAYER = 32;

		protected uint basic_flags;

		protected const uint metadata_bits = 2047U;

		private static Vector2 Pt;

		private static Rect Rc;

		private static Rect[] ARcUse;

		private static Rect[,] ARcCopyng;

		private static int[] Acopied;

		private static int _ignored;

		protected struct CIMesh
		{
			public CIMesh(PxlMeshDrawer _Img, int _layer)
			{
				this.Img = _Img;
				this.layer = (byte)_layer;
			}

			public PxlMeshDrawer Img;

			public byte layer;
		}
	}
}

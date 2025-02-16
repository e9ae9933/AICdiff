using System;
using System.Collections.Generic;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace XX.mobpxl
{
	public class SkltImage : MobSkltPosition.IPosSyncable
	{
		public SkltImage(SkltParts _Con, SkltImageSrc _Source)
		{
			this.Con = _Con;
			this.Source = _Source;
			if (this.Source == null)
			{
				this.Msp = new MobSkltPosition("nouse", null);
				return;
			}
			this.ptype = this.Source.first_ptype;
			this.Msp = new MobSkltPosition(FEnum<PARTS_TYPE>.ToStr(this.Con.type), this.Source.short_name);
		}

		public string palette_key
		{
			get
			{
				if (TX.noe(this.palette_key_))
				{
					this.palette_key_ = this.Source.palette_key_default;
				}
				return this.palette_key_;
			}
			set
			{
				if (this.palette_key == value)
				{
					return;
				}
				this.palette_key_ = value;
				if (this.use_base_oacc)
				{
					this.OACC = this.Con.pSklt.getPalette("_", false);
					return;
				}
				this.OACC = null;
			}
		}

		public SkltImageSrc Source
		{
			get
			{
				return this.Source_;
			}
			set
			{
				if (this.Source_ == value)
				{
					return;
				}
				this.Source_ = value;
				X.ALLN<string>(this.Aui_name_);
			}
		}

		public bool pcc_sync_base
		{
			get
			{
				return this.pcc_sync_base_;
			}
			set
			{
				this.pcc_sync_base_ = value;
			}
		}

		public bool merge_base_oacc
		{
			get
			{
				return this.pcc_sync_base && this.palette_key != "_";
			}
		}

		public bool use_base_oacc
		{
			get
			{
				return this.palette_key == "_";
			}
		}

		public void setPosOnLoad(PxlLayer Lay)
		{
			MobSkltPosition msp = this.Msp;
			msp.PosAbs = new Vector2(Lay.x, Lay.y);
			msp.Scale = new Vector2(Lay.zmx, Lay.zmy);
			msp.rotateR_abs = Lay.rotR;
			msp.fineAbs2Pos(this);
		}

		public SkltImage finePosition()
		{
			this.Msp.finePosition(this, true, true);
			return this;
		}

		internal void finePositionFlagP2A()
		{
			this.Msp.need_fine_pos2abs = true;
		}

		internal ATC_TYPE atc
		{
			get
			{
				return this.Source.atc;
			}
		}

		public int calced_sort_order
		{
			get
			{
				return this.sort_index + (this.sort_order + (int)this.Con.sort_order) * 256;
			}
		}

		public string get_ui_name(bool use_html = true)
		{
			int num = (use_html ? 1 : 0);
			string text = this.Aui_name_[num];
			if (text == null)
			{
				string text2 = this.Source.name;
				if (this.Source.is_skin)
				{
					text2 = string.Concat(new string[]
					{
						this.Con.type.ToString(),
						use_html ? "＜" : "<< ",
						text2,
						"(",
						this.Source.follow_to.ToString(),
						")"
					});
				}
				else
				{
					if (TX.noe(text2))
					{
						text2 = "-";
					}
					text2 = this.atc.ToString() + ":: " + text2;
				}
				if (use_html)
				{
					text2 += " <font alpha=\"0.5\" size=\"0.6\"> -";
				}
				text2 = text2 + "( " + this.Source.Source.pFrm.name + ")";
				if (use_html)
				{
					text2 += "</font>";
				}
				text = (this.Aui_name_[num] = text2);
			}
			return text;
		}

		internal void createPccAppliedMesh(MobGenerator Gen, SkltRenderTicket Tkt, MeshDrawer Md, bool apply_effect_whole = true)
		{
			this.Source.fineUseBits(Gen);
			RectInt[] array = Tkt.createAtlasImage(this, Gen.getAtlasCalculator());
			if (Md != null)
			{
				Md.clearSimple();
				int vertexMax = Md.getVertexMax();
				int num = array.Length;
				Md.allocUv2(num * 4, false);
				for (int i = 0; i < num; i++)
				{
					RectInt rectInt = array[i];
					Md.initForImg(this.Source.getPatByIndex(i).Img, 0);
					Md.RectBL((float)rectInt.x, (float)rectInt.y, (float)rectInt.width, (float)rectInt.height, true);
				}
				int vertexMax2 = Md.getVertexMax();
				Vector2[] uvArray = Md.getUvArray();
				Md.getVertexArray();
				num = vertexMax2 - vertexMax;
				for (int j = 0; j < num; j++)
				{
					Vector2 vector = uvArray[vertexMax++];
					Md.Uv2(vector.x, vector.y, false);
				}
			}
			MobPCCContainer pcc = Gen.get_PCC();
			pcc.Clear();
			if (this.pcc_sync_base && this.palette_key != "_" && (this.Source.parts_use_bits & Gen.pcr_base_parts_bits) != 0U)
			{
				foreach (KeyValuePair<string, MobPCCContainer.ACC> keyValuePair in this.Con.pSklt.OACCBase)
				{
					if (MobGenerator.is_base_oacc_parts(keyValuePair.Key))
					{
						int partsInfoIndex = this.Source_.Source.pChar.getPartsInfoIndex(keyValuePair.Key, false);
						if (partsInfoIndex >= 0 && (this.Source.parts_use_bits & (1U << partsInfoIndex)) != 0U)
						{
							pcc.MergeOACC(keyValuePair.Key, keyValuePair.Value);
						}
					}
				}
			}
			if (this.OACC == null)
			{
				this.OACC = this.Con.pSklt.getPalette(this.palette_key, true);
			}
			if (this.OACC != null)
			{
				pcc.MergeOACC(this.OACC);
			}
			pcc.initColVari(this, this.palette_key_);
			if (apply_effect_whole)
			{
				pcc.applyEffectWhole(null);
			}
		}

		internal bool initForImgMd(MeshDrawer Md, string ptype, Texture Tx, SkltRenderTicket Tkt, float texture_w_r, float texture_h_r, out SkltImageSrc.ISrcPat Pat)
		{
			int ptypeIndex = this.Source.getPTypeIndex(ptype, out Pat);
			RectInt[] array = Tkt.createAtlasImage(this, null);
			if (array == null)
			{
				return false;
			}
			RectInt rectInt = array[ptypeIndex];
			Md.initForImg(Tx, new Rect((float)rectInt.x * texture_w_r, (float)rectInt.y * texture_h_r, (float)rectInt.width * texture_w_r, (float)rectInt.height * texture_h_r), false);
			return true;
		}

		public MobSkltPosition getJointBase()
		{
			return this.Con.getJointBase();
		}

		internal static SkltImage readFromBytes(MobGenerator Gen, SkltParts Con, ByteArray Ba, SkltImage Target = null, bool write_pcc_data = true, int vers = 13)
		{
			uint num = Ba.readUInt();
			double num2 = Ba.readDouble();
			string idString = PxlImage.getIdString(num, num2);
			SkltImageSrc imageSrc = Gen.getImageSrc(idString, true);
			if (imageSrc == null)
			{
				X.dl("不明なイメージ for" + Con.ToString(), null, false, false);
			}
			bool flag = false;
			if (Target == null)
			{
				if (imageSrc == null)
				{
					flag = true;
				}
				Target = new SkltImage(Con, imageSrc);
			}
			else if (imageSrc != null)
			{
				Target.Source = imageSrc;
			}
			Target.sort_order = (int)Ba.readShort();
			MobSkltPosition.readFromBytes(Ba, Target.Msp);
			if (vers < 4)
			{
				MobSkltPosition.readFromBytes(Ba, null);
			}
			int num3 = 0;
			if (vers >= 5)
			{
				Target.ptype = Ba.readPascalString("utf-8", false);
				if (vers >= 6)
				{
					num3 = Ba.readByte();
					Target.visible = (num3 & 1) != 0;
				}
			}
			bool flag2 = false;
			if (vers >= 11)
			{
				Target.palette_key = Ba.readPascalString("utf-8", false);
				Target.pcc_sync_base = (num3 & 2) != 0;
			}
			else if (vers >= 1)
			{
				SkltImage.PCC_SYNC pcc_SYNC = (SkltImage.PCC_SYNC)Ba.readByte();
				if (pcc_SYNC == SkltImage.PCC_SYNC.USE_BASE)
				{
					Target.palette_key = "_";
				}
				else
				{
					Target.pcc_sync_base = pcc_SYNC == SkltImage.PCC_SYNC.MERGE;
					Target.palette_key = imageSrc.palette_key_default;
					if (pcc_SYNC == SkltImage.PCC_SYNC.INDIVIDUAL)
					{
						flag2 = true;
					}
				}
			}
			if (write_pcc_data)
			{
				MobPCCContainer.readFromBytes(Ba, Target.Con.pSklt, Target.palette_key, Gen.getBaseCharacter(), flag2);
			}
			if (flag)
			{
				return null;
			}
			return Target;
		}

		public SkltSequence.SkltDesc getSqDescKey()
		{
			return new SkltSequence.SkltDesc(this);
		}

		public MobSkltPosition getMsp()
		{
			return this.Msp.finePosition(this, true, true);
		}

		public string current_ptype
		{
			get
			{
				if (this.ptype != this.Source.first_ptype && TX.valid(this.ptype))
				{
					return this.ptype;
				}
				if (!TX.valid(this.Con.ptype))
				{
					return this.Source.first_ptype;
				}
				return this.Con.ptype;
			}
		}

		public readonly SkltParts Con;

		private SkltImageSrc Source_;

		public readonly MobSkltPosition Msp;

		public int sort_order;

		public const int SORT_ORDER_MUL = 256;

		public bool visible = true;

		private string palette_key_ = "";

		private bool pcc_sync_base_;

		public string ptype = "";

		internal SkltPalette OACC;

		public int sort_index;

		private string[] Aui_name_ = new string[2];

		public enum PCC_SYNC
		{
			USE_BASE,
			MERGE,
			INDIVIDUAL,
			_MAX
		}
	}
}

using System;
using System.Collections.Generic;
using Better;
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

		public SkltImage.PCC_SYNC pcc_sync
		{
			get
			{
				return this.pcc_sync_;
			}
			set
			{
				this.pcc_sync_ = value;
				if (!this.use_base_oacc)
				{
					if (this.OACC == this.Con.pSklt.OACCBase)
					{
						this.OACC = null;
						return;
					}
				}
				else
				{
					this.OACC = this.Con.pSklt.OACCBase;
				}
			}
		}

		public bool use_base_oacc
		{
			get
			{
				return this.pcc_sync == SkltImage.PCC_SYNC.USE_BASE;
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

		public bool atlas_created
		{
			get
			{
				return this.ACalcedAtlas != null && this.ACalcedAtlas[0].width > 0;
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

		internal void clearTextureAtlas()
		{
			if (this.ACalcedAtlas != null)
			{
				this.ACalcedAtlas[0].width = (this.ACalcedAtlas[0].height = 0);
			}
		}

		internal void createAtlas(RectAtlasTexture CalcAtlas)
		{
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				this.Source.CopyAllPType(blist);
				if (this.ACalcedAtlas == null)
				{
					this.ACalcedAtlas = new RectInt[blist.Count];
				}
				else if (this.ACalcedAtlas.Length != blist.Count)
				{
					Array.Resize<RectInt>(ref this.ACalcedAtlas, blist.Count);
				}
				for (int i = 0; i < blist.Count; i++)
				{
					SkltImageSrc.ISrcPat forAnm = this.Source.GetForAnm(blist[i]);
					int num;
					RenderTexture renderTexture;
					RectInt rectInt = CalcAtlas.createRect(2 + forAnm.Img.width, 2 + forAnm.Img.height, out num, out renderTexture, true);
					this.ACalcedAtlas[i] = new RectInt(rectInt.x + 1, rectInt.y + 1, rectInt.width - 2, rectInt.height - 2);
				}
			}
		}

		internal void createPccAppliedMesh(MobGenerator Gen, MeshDrawer Md, bool apply_effect_whole = true)
		{
			this.Source.fineUseBits(Gen);
			if (!this.atlas_created)
			{
				this.createAtlas(Gen.getAtlasCalculator());
			}
			if (Md != null)
			{
				Md.clearSimple();
				int vertexMax = Md.getVertexMax();
				int num = this.ACalcedAtlas.Length;
				Md.allocUv2(num * 4, false);
				for (int i = 0; i < num; i++)
				{
					RectInt rectInt = this.ACalcedAtlas[i];
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
			if (this.OACC != this.Con.pSklt.OACCBase && this.pcc_sync_ != SkltImage.PCC_SYNC.INDIVIDUAL && (this.Source.parts_use_bits & Gen.pcr_base_parts_bits) != 0U)
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
			if (this.OACC != null)
			{
				pcc.MergeOACC(this.OACC);
			}
			if (apply_effect_whole)
			{
				pcc.applyEffectWhole(null);
			}
		}

		internal void initForImgMd(MeshDrawer Md, string ptype, Texture Tx, float texture_w_r, float texture_h_r, out SkltImageSrc.ISrcPat Pat)
		{
			int ptypeIndex = this.Source.getPTypeIndex(ptype, out Pat);
			RectInt rectInt = this.ACalcedAtlas[ptypeIndex];
			Md.initForImg(Tx, new Rect((float)rectInt.x * texture_w_r, (float)rectInt.y * texture_h_r, (float)rectInt.width * texture_w_r, (float)rectInt.height * texture_h_r), false);
		}

		public MobSkltPosition getJointBase()
		{
			return this.Con.getJointBase();
		}

		internal static SkltImage readFromBytes(MobGenerator Gen, SkltParts Con, ByteArray Ba, SkltImage Target = null, bool write_pcc_data = true, int vers = 10)
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
			if (vers >= 5)
			{
				Target.ptype = Ba.readPascalString("utf-8", false);
				if (vers >= 6)
				{
					Target.visible = Ba.readBoolean();
				}
			}
			if (vers >= 1)
			{
				Target.pcc_sync = (SkltImage.PCC_SYNC)Ba.readByte();
			}
			if (write_pcc_data)
			{
				if (!Target.use_base_oacc)
				{
					MobPCCContainer.readFromBytes(Ba, ref Target.OACC, Gen.getBaseCharacter());
				}
				else
				{
					BDic<string, MobPCCContainer.ACC> bdic = null;
					MobPCCContainer.readFromBytes(Ba, ref bdic, Gen.getBaseCharacter());
				}
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

		private SkltImage.PCC_SYNC pcc_sync_ = SkltImage.PCC_SYNC.MERGE;

		public string ptype = "";

		internal BDic<string, MobPCCContainer.ACC> OACC;

		public int sort_index;

		public RectInt[] ACalcedAtlas;

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

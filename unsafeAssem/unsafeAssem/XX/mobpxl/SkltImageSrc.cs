using System;
using System.Collections.Generic;
using Better;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace XX.mobpxl
{
	public class SkltImageSrc
	{
		internal SkltImageSrc(PxlLayer _Source, string _name, string _header, PARTS_TYPE _follow_to, ATC_TYPE _atc)
		{
			this.Source = _Source;
			this.name = _name;
			this.header = _header;
			this.follow_to = _follow_to;
			this.atc = _atc;
			if (this.Source.isImport())
			{
				PxlLayer importSource = this.Source.getImportSource();
				if (importSource != null && importSource != this.Source && TX.isStart(importSource.pFrm.pPose.title, "_pat", 0))
				{
					this.CreatePattern(importSource, importSource.pFrm.pSq);
				}
			}
		}

		public bool isFamily(SkltImageSrc Sis)
		{
			if (this.Source.pFrm.name == Sis.Source.pFrm.name)
			{
				string text = this.name;
				string text2 = Sis.name;
				if (text == text2 && SkltImageSrc.isFamilyAtc(Sis.atc, this.atc))
				{
					return true;
				}
			}
			return false;
		}

		internal static bool isFamilyAtc(ATC_TYPE atc0, ATC_TYPE atc1)
		{
			if (atc0 == atc1)
			{
				return true;
			}
			if (atc0 > atc1)
			{
				return SkltImageSrc.isFamilyAtc(atc1, atc0);
			}
			return (atc0 == ATC_TYPE.h && atc1 == ATC_TYPE.hb) || (atc0 == ATC_TYPE.clt && atc1 == ATC_TYPE.cltu) || (atc0 == ATC_TYPE.clt && atc1 == ATC_TYPE.cltb) || (atc0 == ATC_TYPE.cltu && atc1 == ATC_TYPE.cltb);
		}

		public bool isFamilySameFollow(SkltImageSrc Sis)
		{
			return this.isFamily(Sis) && Sis.follow_to == this.follow_to;
		}

		public bool is_base_body
		{
			get
			{
				return this.follow_to == PARTS_TYPE.BODY && this.atc == ATC_TYPE._OTHER;
			}
		}

		public bool use_base_oacc_default
		{
			get
			{
				return this.atc == ATC_TYPE._OTHER || this.atc == ATC_TYPE.h || this.atc == ATC_TYPE.hb || this.atc == ATC_TYPE.mouse || this.atc == ATC_TYPE.eyeb;
			}
		}

		public bool is_skin
		{
			get
			{
				return this.atc == ATC_TYPE._OTHER;
			}
		}

		public bool is_face
		{
			get
			{
				return this.follow_to == PARTS_TYPE.FACE && this.atc == ATC_TYPE._OTHER;
			}
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.Source.name,
				" / ",
				this.atc.ToString(),
				" => ",
				this.follow_to.ToString()
			});
		}

		public void addUv2RectPartsUv(MeshDrawer Md)
		{
			Rect rectPUv = this.Source.Img.RectPUv;
			Md.Uv2(rectPUv.x, rectPUv.y, false);
			Md.Uv2(rectPUv.x, rectPUv.yMax, false);
			Md.Uv2(rectPUv.xMax, rectPUv.yMax, false);
			Md.Uv2(rectPUv.xMax, rectPUv.y, false);
		}

		public void fineUseBits(MobGenerator Gen)
		{
		}

		public bool saveable
		{
			get
			{
				return this.parts_use_bits > 0U;
			}
		}

		internal static void readFromBytes(SkltImageSrc Src, ByteArray Ba)
		{
			uint num = Ba.readUInt();
			if (Src != null)
			{
				Src.parts_use_bits = num;
			}
		}

		public string short_name
		{
			get
			{
				if (TX.valid(this.name))
				{
					return this.name;
				}
				return this.Source.name;
			}
		}

		public SkltImageSrc.ISrcPat GetForAnm(string ptype = null)
		{
			SkltImageSrc.ISrcPat srcPat;
			if (ptype != null && this.OPat != null && this.OPat.TryGetValue(ptype, out srcPat))
			{
				return srcPat;
			}
			return new SkltImageSrc.ISrcPat(this.Source, Vector2.zero);
		}

		private void CreatePattern(PxlLayer PatL, PxlSequence PatSq)
		{
			PxlFrame pFrm = PatL.pFrm;
			this.first_ptype = pFrm.name ?? "";
			int index = PatSq.getIndex(pFrm);
			int num = PatSq.countFrames();
			for (int i = index + 1; i < num; i++)
			{
				PxlFrame frame = PatSq.getFrame(i);
				PxlLayer layerByName = frame.getLayerByName(PatL.name);
				if (layerByName != null && frame.name != this.first_ptype)
				{
					this.addPat((frame.name == "") ? "_" : frame.name, PatL, layerByName);
				}
			}
		}

		public int atlas_count
		{
			get
			{
				return 1 + ((this.OPat != null) ? this.OPat.Count : 0);
			}
		}

		private void addPat(string name, PxlLayer BaseL, PxlLayer L)
		{
			if (this.OPat == null)
			{
				this.OPat = new BDic<string, SkltImageSrc.ISrcPat>(1);
			}
			this.OPat[name] = new SkltImageSrc.ISrcPat(L, new Vector2(L.x - BaseL.x, L.y - BaseL.y));
		}

		public void CopyAllPType(List<string> A)
		{
			if (this.OPat == null)
			{
				X.pushIdentical<string>(A, "_");
				return;
			}
			X.pushIdentical<string>(A, (this.first_ptype == "") ? "_" : this.first_ptype);
			foreach (KeyValuePair<string, SkltImageSrc.ISrcPat> keyValuePair in this.OPat)
			{
				X.pushIdentical<string>(A, keyValuePair.Key);
			}
		}

		public int getPTypeIndex(string ptype, out SkltImageSrc.ISrcPat Pat)
		{
			int num = 0;
			if (this.OPat != null)
			{
				foreach (KeyValuePair<string, SkltImageSrc.ISrcPat> keyValuePair in this.OPat)
				{
					num++;
					if (keyValuePair.Key == ptype)
					{
						Pat = keyValuePair.Value;
						return num;
					}
				}
			}
			Pat = new SkltImageSrc.ISrcPat(this.Source, Vector2.zero);
			return 0;
		}

		public SkltImageSrc.ISrcPat getPatByIndex(int i = 0)
		{
			if (i <= 0 || this.OPat == null)
			{
				return new SkltImageSrc.ISrcPat(this.Source, Vector2.zero);
			}
			foreach (KeyValuePair<string, SkltImageSrc.ISrcPat> keyValuePair in this.OPat)
			{
				if (--i <= 0)
				{
					return keyValuePair.Value;
				}
			}
			return new SkltImageSrc.ISrcPat(this.Source, Vector2.zero);
		}

		public readonly PxlLayer Source;

		public readonly string name;

		public readonly string header;

		internal readonly PARTS_TYPE follow_to;

		internal readonly ATC_TYPE atc;

		private BDic<string, SkltImageSrc.ISrcPat> OPat;

		public string first_ptype = "";

		public uint parts_use_bits;

		public struct ISrcPat
		{
			public ISrcPat(PxlLayer _Lay, Vector2 _Shift)
			{
				this.Lay = _Lay;
				this.Shift = _Shift;
			}

			public PxlImage Img
			{
				get
				{
					return this.Lay.Img;
				}
			}

			public float x
			{
				get
				{
					return this.Shift.x;
				}
			}

			public float y
			{
				get
				{
					return this.Shift.y;
				}
			}

			public PxlLayer Lay;

			public Vector2 Shift;
		}
	}
}

using System;
using System.Collections.Generic;
using PixelLiner;
using PixelLiner.PixelLinerLib;

namespace XX.mobpxl
{
	public class SkltParts : MobSkltPosition.IPosSyncable
	{
		internal SkltParts(MobSklt _pSklt, PARTS_TYPE _type, int joint_capacity = 1, int img_capacity = 1)
		{
			this.pSklt = _pSklt;
			this.type = _type;
			if (this.AJoint != null)
			{
				this.AJoint = new List<SkltJoint>(joint_capacity);
			}
			if (img_capacity > 0)
			{
				this.AImage = new List<SkltImage>(img_capacity);
			}
			this.sort_order = ((this.type == PARTS_TYPE.RARM || this.type == PARTS_TYPE.RARM2 || this.type == PARTS_TYPE.FACE) ? 24 : 0);
		}

		public void AddJoint(SkltJoint Joint)
		{
			if (this.AJoint == null)
			{
				this.AJoint = new List<SkltJoint>(1);
			}
			if (Joint.BelongD == this)
			{
				int count = this.AJoint.Count;
				for (int i = 0; i < count; i++)
				{
					if (this.AJoint[i].BelongS == this)
					{
						this.AJoint.Insert(i, Joint);
						return;
					}
				}
			}
			this.AJoint.Add(Joint);
		}

		internal void ClearJoint()
		{
			if (this.AJoint != null)
			{
				this.AJoint.Clear();
			}
		}

		internal void ReplaceJoint(SkltParts J)
		{
			List<SkltJoint> ajoint = J.AJoint;
			if (ajoint == null)
			{
				return;
			}
			int count = ajoint.Count;
			if (this.AJoint == null)
			{
				this.AJoint = new List<SkltJoint>(count);
			}
			for (int i = 0; i < count; i++)
			{
				SkltJoint skltJoint = ajoint[i];
				if (skltJoint.BelongS == J)
				{
					new SkltJoint(skltJoint, this.pSklt);
				}
			}
		}

		public void addImageOnLoad(PxlLayer Lay, SkltImageSrc ImgSrc)
		{
			this.PartsSize_ = default(MobSkltPosition.SizeM);
			if (ImgSrc.atc == ATC_TYPE.shoe && (this.type != PARTS_TYPE.LFOOT && this.type != PARTS_TYPE.RFOOT && this.type != PARTS_TYPE.LFOOT2) && this.type != PARTS_TYPE.RFOOT2)
			{
				X.de("モーションのデフォルト follow が不正: " + Lay.ToString(), null);
				return;
			}
			if (ImgSrc.Source.alpha == 0f)
			{
				return;
			}
			int count = this.AImage.Count;
			for (int i = 0; i < count; i++)
			{
				SkltImage skltImage = this.AImage[i];
				if (skltImage.Source == ImgSrc)
				{
					skltImage.setPosOnLoad(Lay);
					return;
				}
			}
			if (this.type != PARTS_TYPE.BODY || ImgSrc.atc != ATC_TYPE.clt)
			{
				for (int j = 0; j < count; j++)
				{
					if (this.AImage[j].atc == ImgSrc.atc)
					{
						return;
					}
				}
			}
			SkltImage skltImage2 = new SkltImage(this, ImgSrc);
			skltImage2.setPosOnLoad(Lay);
			int count2 = this.AImage.Count;
			this.AImage.Add(skltImage2);
			if (!ImgSrc.is_skin && skltImage2.sort_order == 0)
			{
				bool flag = false;
				for (int k = 0; k < count2; k++)
				{
					if (this.AImage[k].Source.is_skin)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					skltImage2.sort_order = -48;
				}
			}
		}

		internal SkltImage findAtcImage(ATC_TYPE atc)
		{
			int count = this.AImage.Count;
			for (int i = 0; i < count; i++)
			{
				SkltImage skltImage = this.AImage[i];
				if (skltImage.atc == atc)
				{
					return skltImage;
				}
			}
			return null;
		}

		internal void fineJointPositionOnLoad()
		{
			int count = this.AJoint.Count;
			for (int i = 0; i < count; i++)
			{
				SkltJoint skltJoint = this.AJoint[i];
				if (skltJoint.BelongD != this)
				{
					return;
				}
				skltJoint.Pos.fineAbs2Pos(skltJoint);
			}
		}

		internal bool getJointParent(out SkltJoint Jt)
		{
			if (this.AJoint != null && this.AJoint.Count > 0)
			{
				Jt = this.AJoint[0];
				if (Jt.BelongD == this)
				{
					return true;
				}
			}
			Jt = null;
			return false;
		}

		internal void sortPartsJointAll()
		{
			if (this.AJoint != null)
			{
				int count = this.AJoint.Count;
				for (int i = 0; i < count; i++)
				{
					SkltJoint skltJoint = this.AJoint[i];
					if (skltJoint.BelongD != this)
					{
						break;
					}
					skltJoint.Pos.finePosition(skltJoint, true, true);
				}
			}
		}

		public MobSkltPosition getJointBase()
		{
			PARTS_TYPE parts_TYPE;
			return this.getJointBase(out parts_TYPE);
		}

		internal MobSkltPosition getJointBase(out PARTS_TYPE base_parts_type)
		{
			SkltJoint skltJoint;
			if (this.getJointParent(out skltJoint))
			{
				skltJoint.Pos.finePosition(skltJoint, true, true);
				base_parts_type = skltJoint.BelongS.type;
				return skltJoint.Pos;
			}
			MobSkltPosition msp = this.pSklt.Msp;
			base_parts_type = this.type;
			msp.finePositionBase();
			return msp;
		}

		public override string ToString()
		{
			return "<SkltParts>" + FEnum<PARTS_TYPE>.ToStr(this.type) + " - @" + this.pSklt.name;
		}

		public bool fine_parts_size
		{
			get
			{
				return !this.PartsSize_.valid;
			}
			set
			{
				if (value)
				{
					this.PartsSize_ = default(MobSkltPosition.SizeM);
					this.fineDPartsPosition();
				}
			}
		}

		public void fineDPartsPosition()
		{
			this.allImageFinePositionFlagP2A();
			if (this.AJoint == null)
			{
				return;
			}
			int count = this.AJoint.Count;
			for (int i = 0; i < count; i++)
			{
				SkltJoint skltJoint = this.AJoint[i];
				skltJoint.Pos.finePosition(skltJoint, true, true);
				if (skltJoint.BelongS == this)
				{
					skltJoint.Pos.need_fine_pos2abs = true;
					skltJoint.Pos.finePosition(skltJoint, true, true);
					skltJoint.BelongD.fineDPartsPosition();
				}
				else
				{
					skltJoint.Pos.need_fine_pos2abs = true;
				}
			}
		}

		public List<SkltJoint> getJointList()
		{
			return this.AJoint;
		}

		internal void allImageFinePositionFlagP2A()
		{
			int count = this.AImage.Count;
			for (int i = 0; i < count; i++)
			{
				this.AImage[i].finePositionFlagP2A();
			}
		}

		public MobSkltPosition.SizeM PartsSize
		{
			get
			{
				if (this.fine_parts_size)
				{
					this.finePartsSize();
				}
				return this.PartsSize_;
			}
		}

		private void finePartsSize()
		{
			int count = this.AImage.Count;
			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					SkltImage skltImage = this.AImage[i];
					if (skltImage.Source.is_skin)
					{
						PxlImage img = skltImage.Source.Source.Img;
						this.PartsSize_ = new MobSkltPosition.SizeM((float)img.width, (float)img.height);
						return;
					}
				}
				int num = 0;
				if (num < 1)
				{
					PxlImage img2 = this.AImage[num].Source.Source.Img;
					this.PartsSize_ = new MobSkltPosition.SizeM((float)img2.width, (float)img2.height);
					return;
				}
			}
			this.PartsSize_ = new MobSkltPosition.SizeM(32f, 32f);
		}

		public SkltSequence.SkltDesc getSqDescKey()
		{
			return new SkltSequence.SkltDesc(this);
		}

		public MobSkltPosition getMsp()
		{
			return this.getJointBase();
		}

		public bool has_children()
		{
			List<SkltJoint> ajoint = this.AJoint;
			if (ajoint == null)
			{
				return false;
			}
			int num = ajoint.Count - 1;
			return num >= 0 && ajoint[num].BelongS == this;
		}

		internal static void readFromBytesJoint(MobSklt Sklt, ByteArray Ba, PARTS_TYPE parts_type, bool use_flag = true)
		{
			int num = Ba.readByte();
			if (num > 0)
			{
				List<SkltJoint> ajoint = Sklt.MakeParts(parts_type).AJoint;
				for (int i = 0; i < num; i++)
				{
					SkltJoint skltJoint = X.Get<SkltJoint>(ajoint, i);
					MobSkltPosition.readFromBytes(Ba, (use_flag && skltJoint != null) ? skltJoint.Pos : null);
				}
			}
		}

		public readonly MobSklt pSklt;

		internal PARTS_TYPE type;

		private List<SkltJoint> AJoint;

		internal readonly List<SkltImage> AImage;

		private MobSkltPosition.SizeM PartsSize_;

		internal short sort_order;

		public string ptype = "";
	}
}

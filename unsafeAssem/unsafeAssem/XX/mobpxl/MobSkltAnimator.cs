using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;

namespace XX.mobpxl
{
	public class MobSkltAnimator
	{
		internal MobSklt Sklt
		{
			get
			{
				return this.Sklt_;
			}
			set
			{
				if (this.Sklt_ == value)
				{
					return;
				}
				this.Sklt_ = value;
				this.need_fine_position = true;
			}
		}

		public int cframe
		{
			get
			{
				return this.cframe_;
			}
		}

		public SkltSequence MSq
		{
			get
			{
				return this.MSq_;
			}
			set
			{
				this.setSequence(value);
			}
		}

		public void setSequence(SkltSequence _MSq)
		{
			if (this.MSq_ != _MSq)
			{
				this.changed = true;
				this.need_fine_position = (this.need_fine_order = true);
			}
			this.MSq_ = _MSq;
			this.animReset(0);
		}

		public void animReset(int _cframe)
		{
			if (this.cframe_ != _cframe)
			{
				this.need_fine_position = (this.changed = true);
			}
			this.cframe_ = _cframe;
			this.af = 0f;
			if (this.MSq_ == null)
			{
				this.MFrm = null;
				return;
			}
			SkltSequence.SkltFrame mfrm = this.MFrm;
			int num = this.MSq_.countFrames();
			while (this.cframe_ >= num)
			{
				this.cframe_ = (int)this.MSq_.loop_to + (this.cframe_ - num);
			}
			this.MFrm = this.MSq_.getFrame(this.cframe_);
			if (mfrm == null || mfrm.isChangedOrder(this.MFrm))
			{
				this.need_fine_order = true;
			}
		}

		public virtual bool updateAnimator(float fcnt)
		{
			if (this.MSq_ == null || this.MSq_.countFrames() <= 1)
			{
				return false;
			}
			this.af += fcnt * this.timeScale;
			if (this.af >= (float)this.MFrm.crf60)
			{
				this.af -= (float)this.MFrm.crf60;
				this.animReset(this.cframe + 1);
				this.changed = true;
				return true;
			}
			return false;
		}

		public void draw(MeshDrawer Md, Texture Tx, float x, float y)
		{
			if (this.Sklt == null)
			{
				return;
			}
			if (this.Sklt.fineSort(false, true))
			{
				this.need_fine_position = true;
			}
			if (this.MSq_ != null)
			{
				this.executeAnimFinePosition(false);
			}
			MobSkltAnimator.draw(Md, Tx, this.Sklt, x, y, this);
		}

		internal void executeAnimFinePosition(bool force = false)
		{
			if (this.AOvrImg == null)
			{
				this.AOvrImg = new MobSkltAnimator.OvrPos[this.Sklt.AImgSorted.Count];
				this.need_fine_position = true;
			}
			else if (this.AOvrImg.Length < this.Sklt.AImgSorted.Count)
			{
				this.need_fine_position = true;
				Array.Resize<MobSkltAnimator.OvrPos>(ref this.AOvrImg, this.Sklt.AImgSorted.Count);
			}
			if (this.AOvrParts == null)
			{
				this.AOvrParts = new MobSkltAnimator.OvrPos[this.Sklt.parts_max];
				this.need_fine_position = true;
			}
			else if (this.AOvrParts.Length < this.Sklt.parts_max)
			{
				this.need_fine_position = true;
				Array.Resize<MobSkltAnimator.OvrPos>(ref this.AOvrParts, this.Sklt.parts_max);
			}
			if (this.need_fine_position || force)
			{
				this.need_fine_position = false;
				this.checkAllParts(this.MFrm);
				this.checkAllImages(this.MFrm);
			}
			if (this.need_fine_order || force)
			{
				this.need_fine_order = false;
				if (this.MFrm != null && this.MFrm.order_writed)
				{
					if (this.Arewrited_order == null)
					{
						this.Arewrited_order = new List<MobSkltAnimator.OvrOrder>(this.MFrm.countInfo());
					}
					this.Arewrited_order.Clear();
					int count = this.Sklt.AImgSorted.Count;
					for (int i = 0; i < count; i++)
					{
						this.Arewrited_order.Add(new MobSkltAnimator.OvrOrder(this.MFrm, this.Sklt.AImgSorted[i], i));
					}
					this.Arewrited_order.Sort((MobSkltAnimator.OvrOrder A, MobSkltAnimator.OvrOrder B) => A.order - B.order);
					return;
				}
				if (this.Arewrited_order != null)
				{
					this.Arewrited_order.Clear();
				}
			}
		}

		private void checkAllParts(SkltSequence.SkltFrame MFrm)
		{
			int parts_max = this.Sklt.parts_max;
			for (int i = 0; i < parts_max; i++)
			{
				this.AOvrParts[i].checked_ = false;
			}
			for (int j = 0; j < parts_max; j++)
			{
				this.checkAllParts(ref this.AOvrParts[j], j, MFrm);
			}
		}

		private MobSkltPosition checkAllParts(ref MobSkltAnimator.OvrPos Ovr, int i, SkltSequence.SkltFrame MFrm)
		{
			if (Ovr.checked_)
			{
				return Ovr.Msp;
			}
			Ovr.checked_ = true;
			SkltParts skltParts = this.Sklt.AParts[i];
			SkltSequence.PosInfo posInfo = MFrm.Get(skltParts);
			MobSkltPosition msp = posInfo.Msp;
			PARTS_TYPE parts_TYPE;
			MobSkltPosition jointBase = skltParts.getJointBase(out parts_TYPE);
			Ovr.active = false;
			if (parts_TYPE != skltParts.type)
			{
				MobSkltPosition mobSkltPosition = this.checkAllParts(ref this.AOvrParts[(int)parts_TYPE], (int)parts_TYPE, MFrm);
				if (mobSkltPosition != null)
				{
					SkltParts skltParts2 = this.Sklt[parts_TYPE];
					Ovr.setActive(posInfo, true);
					MobSkltPosition mobSkltPosition2 = Ovr.Msp.CopyFromCorrectly(jointBase);
					if (msp != null)
					{
						mobSkltPosition2.Multiply(msp);
					}
					mobSkltPosition2.finePos2Abs(mobSkltPosition, skltParts2.PartsSize);
				}
			}
			if (!Ovr.active && posInfo.valid)
			{
				Ovr.setActive(posInfo, true);
				Ovr.Msp.Multiply(jointBase, msp);
			}
			if (Ovr.active && Ovr.Msp.need_fine_pos2abs)
			{
				if (parts_TYPE != skltParts.type)
				{
					SkltParts skltParts3 = this.Sklt[parts_TYPE];
					Ovr.Msp.finePos2Abs(skltParts3.getJointBase(), skltParts3.PartsSize);
				}
				else
				{
					Ovr.Msp.finePos2Abs(null, default(MobSkltPosition.SizeM));
				}
			}
			return Ovr.Msp;
		}

		private void checkAllImages(SkltSequence.SkltFrame MFrm)
		{
			int count = this.Sklt.AImgSorted.Count;
			for (int i = 0; i < count; i++)
			{
				this.checkAllImages(ref this.AOvrImg[i], i, MFrm);
			}
		}

		private void checkAllImages(ref MobSkltAnimator.OvrPos Ovr, int index, SkltSequence.SkltFrame MFrm)
		{
			SkltImage skltImage = this.Sklt.AImgSorted[index];
			SkltSequence.PosInfo posInfo = MFrm.Get(skltImage);
			MobSkltPosition mobSkltPosition = this.fineOvrMsp(skltImage.Con, null);
			Ovr.setActive(posInfo, false);
			if (!posInfo.valid)
			{
				if (!this.editor_mode)
				{
					return;
				}
				MobSkltPosition msp = this.AOvrParts[(int)skltImage.Con.type].Msp;
				if (msp == null || mobSkltPosition != msp)
				{
					return;
				}
			}
			Ovr.active = true;
			MobSkltPosition msp2 = Ovr.Msp;
			msp2.CopyFromCorrectly(skltImage.Msp.finePosition(skltImage, true, true));
			if (posInfo.Msp != null)
			{
				msp2.Multiply(posInfo.Msp);
			}
			msp2.need_fine_pos2abs = true;
			msp2.finePosition(mobSkltPosition, skltImage.Con.PartsSize, true, true);
		}

		internal static void draw(MeshDrawer Md, Texture Tx, MobSklt Sklt, float x, float y, MobSkltAnimator Caller = null)
		{
			Sklt.fineSort(false, true);
			int count = Sklt.AImgSorted.Count;
			Matrix4x4 currentMatrix = Md.getCurrentMatrix();
			Md.TranslateP(x, y, true);
			float num = 1f / (float)Tx.width;
			float num2 = 1f / (float)Tx.height;
			List<MobSkltAnimator.DrawnCache> list = null;
			string text = "";
			MobSkltAnimator.OvrPos[] array = null;
			MobSkltAnimator.OvrPos[] array2 = null;
			List<MobSkltAnimator.OvrOrder> list2 = null;
			if (Caller != null)
			{
				list = Caller.ADrawnCache;
				if (list != null)
				{
					list.Clear();
					Md.allocUv2(count * 4, false);
				}
				if (Caller.MSq_ != null)
				{
					array = Caller.AOvrImg;
					array2 = Caller.AOvrParts;
					if (Caller.Arewrited_order != null && Caller.Arewrited_order.Count == count)
					{
						list2 = Caller.Arewrited_order;
					}
				}
			}
			MobSkltPosition mobSkltPosition = null;
			SkltParts skltParts = null;
			byte b = 0;
			for (int i = 0; i < count; i++)
			{
				int num3 = ((list2 != null) ? list2[i].index : i);
				SkltImage skltImage = Sklt.AImgSorted[num3];
				if (skltImage.atlas_created)
				{
					MobSkltPosition mobSkltPosition2 = null;
					string text2 = "";
					byte b2 = 8;
					if (array != null)
					{
						MobSkltAnimator.OvrPos ovrPos = array[num3];
						mobSkltPosition2 = ovrPos.Msp;
						text2 = ovrPos.ovr_ptype;
						b2 = ovrPos.visible;
					}
					if ((b2 & 8) != 0)
					{
						if (!skltImage.visible)
						{
							goto IL_0423;
						}
					}
					else if ((b2 & 1) == 0)
					{
						goto IL_0423;
					}
					if (mobSkltPosition2 == null)
					{
						mobSkltPosition2 = skltImage.Msp.finePosition(skltImage, true, false);
					}
					if (skltParts != skltImage.Con)
					{
						skltParts = skltImage.Con;
						mobSkltPosition = null;
						if (array2 != null)
						{
							MobSkltAnimator.OvrPos ovrPos2 = array2[(int)skltImage.Con.type];
							mobSkltPosition = ovrPos2.Msp;
							text = ovrPos2.ovr_ptype;
						}
						if (mobSkltPosition == null)
						{
							mobSkltPosition = skltParts.getJointBase();
						}
						b = mobSkltPosition.flipping_flag;
					}
					SkltImageSrc source = skltImage.Source;
					string text3 = (TX.valid(text2) ? text2 : (TX.valid(text) ? text : skltImage.current_ptype));
					SkltImageSrc.ISrcPat srcPat;
					skltImage.initForImgMd(Md, text3, Tx, num, num2, out srcPat);
					PxlImage img = srcPat.Img;
					Vector2 vector = new Vector2(srcPat.x, srcPat.y);
					float num4 = (float)img.width * 0.5f;
					float num5 = (float)img.height * 0.5f;
					float num6 = X.Cos(mobSkltPosition2.rotateR);
					float num7 = X.Sin(mobSkltPosition2.rotateR);
					for (int j = 0; j < 4; j++)
					{
						Vector2 vector2;
						switch (j)
						{
						case 0:
							vector2 = (new Vector2(-num4, -num5 + (float)img.height) + vector) * mobSkltPosition2.ScaleBL;
							break;
						case 1:
							vector2 = (new Vector2(-num4, -num5) + vector) * mobSkltPosition2.ScaleLT;
							break;
						case 2:
							vector2 = (new Vector2(-num4 + (float)img.width, -num5) + vector) * mobSkltPosition2.ScaleTR;
							break;
						default:
							vector2 = (new Vector2(-num4 + (float)img.width, -num5 + (float)img.height) + vector) * mobSkltPosition2.ScaleRB;
							break;
						}
						Vector2 vector3 = vector2;
						vector3 = X.ROTV2e(vector3, num6, num7) + mobSkltPosition2.Pos;
						vector3 = MobSkltPosition.calcPositionP2A(vector3, skltParts.PartsSize, mobSkltPosition);
						MobSkltAnimator.ABuf[j] = new Vector2(vector3.x * 0.015625f, -vector3.y * 0.015625f);
					}
					byte b3 = mobSkltPosition2.calcFlippingFlag(b);
					if ((b3 & 1) != 0)
					{
						Md.TriRectBL(0);
					}
					if ((b3 & 2) != 0)
					{
						Md.Tri(0, 2, 1, false).Tri(0, 3, 2, false);
					}
					int vertexMax = Md.getVertexMax();
					Md.Pos(MobSkltAnimator.ABuf[0], null).Pos(MobSkltAnimator.ABuf[1], null).Pos(MobSkltAnimator.ABuf[2], null)
						.Pos(MobSkltAnimator.ABuf[3], null);
					Md.InputImageUv();
					if (list != null)
					{
						skltImage.Source.addUv2RectPartsUv(Md);
						list.Add(new MobSkltAnimator.DrawnCache(skltImage, vertexMax));
					}
				}
				IL_0423:;
			}
			Md.setCurrentMatrix(currentMatrix, false);
		}

		internal MobSkltPosition fineOvrMsp(SkltParts Pi, SkltParts Ignore_Child_Parts = null)
		{
			this.Sklt.fineSort(false, true);
			MobSkltPosition msp = this.AOvrParts[(int)Pi.type].Msp;
			if (msp == null)
			{
				return Pi.getMsp();
			}
			if (!msp.need_fine_pos2abs && !msp.need_fine_abs2pos)
			{
				return msp;
			}
			PARTS_TYPE parts_TYPE;
			Pi.getJointBase(out parts_TYPE);
			if (parts_TYPE != Pi.type)
			{
				SkltParts skltParts = this.Sklt.AParts[(int)parts_TYPE];
				MobSkltPosition mobSkltPosition = this.fineOvrMsp(skltParts, Pi);
				msp.finePosition(mobSkltPosition, skltParts.PartsSize, true, true);
			}
			else
			{
				msp.finePositionBase();
			}
			List<SkltJoint> jointList = Pi.getJointList();
			if (jointList != null)
			{
				for (int i = jointList.Count - 1; i >= 0; i--)
				{
					SkltJoint skltJoint = jointList[i];
					if (skltJoint.BelongS != Pi)
					{
						break;
					}
					if (Ignore_Child_Parts != skltJoint.BelongD)
					{
						MobSkltPosition msp2 = this.AOvrParts[(int)skltJoint.BelongD.type].Msp;
						if (msp2 != null)
						{
							msp2.need_fine_pos2abs = true;
							this.fineOvrMsp(skltJoint.BelongD, null);
						}
					}
				}
			}
			return msp;
		}

		private static Vector2[] ABuf = new Vector2[4];

		private MobSklt Sklt_;

		internal List<MobSkltAnimator.DrawnCache> ADrawnCache;

		private MobSkltAnimator.OvrPos[] AOvrImg;

		private MobSkltAnimator.OvrPos[] AOvrParts;

		internal List<MobSkltAnimator.OvrOrder> Arewrited_order;

		private SkltSequence MSq_;

		private SkltSequence.SkltFrame MFrm;

		private float af;

		private int cframe_;

		public float timeScale = 1f;

		public bool need_fine_position = true;

		public bool need_fine_order = true;

		public bool changed;

		public bool editor_mode;

		internal struct DrawnCache
		{
			public DrawnCache(SkltImage _Img, int _ver_i)
			{
				this.Img = _Img;
				this.ver_i = _ver_i;
			}

			public bool valid
			{
				get
				{
					return this.Img != null;
				}
			}

			public readonly SkltImage Img;

			public readonly int ver_i;
		}

		private struct OvrPos
		{
			public bool active
			{
				get
				{
					return this.active_;
				}
				set
				{
					this.active_ = value;
					if (!value)
					{
						this.ovr_ptype = "";
					}
				}
			}

			public MobSkltPosition Msp
			{
				get
				{
					if (!this.active)
					{
						return null;
					}
					if (this.Msp_ == null)
					{
						this.Msp_ = new MobSkltPosition(null, null);
					}
					return this.Msp_;
				}
			}

			public void setActive(SkltSequence.PosInfo Psi, bool flag = true)
			{
				this.active = flag;
				this.ovr_ptype = Psi.ovr_ptype ?? "";
				this.visible = ((!Psi.valid) ? 8 : Psi.visible);
				this.order = Psi.order;
			}

			private MobSkltPosition Msp_;

			public bool active_;

			public byte visible;

			public bool checked_;

			public string ovr_ptype;

			public int order;
		}

		internal struct OvrOrder
		{
			public OvrOrder(SkltSequence.SkltFrame Frm, SkltImage Img, int _index)
			{
				SkltSequence.PosInfo posInfo = Frm.Get(Img);
				SkltSequence.PosInfo posInfo2 = Frm.Get(Img.Con);
				this.index = _index;
				this.order = (posInfo2.order + posInfo.order) * 256 + Img.calced_sort_order;
			}

			internal int index;

			internal int order;
		}
	}
}

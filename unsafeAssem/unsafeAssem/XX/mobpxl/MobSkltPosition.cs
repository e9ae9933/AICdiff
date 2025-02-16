using System;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace XX.mobpxl
{
	public class MobSkltPosition
	{
		public MobSkltPosition(string _name = null, string _name2 = null)
		{
		}

		public MobSkltPosition CopyFrom(MobSkltPosition Src, bool need_fine_pos = false)
		{
			this.Pos_ = Src.Pos_;
			this.rotateR_ = Src.rotateR_;
			this.ScaleLT = Src.ScaleLT;
			this.ScaleTR = Src.ScaleTR;
			this.ScaleRB = Src.ScaleRB;
			this.ScaleBL = Src.ScaleBL;
			this.rotateR_abs_ = Src.rotateR_abs_;
			this.PosAbs_ = Src.PosAbs_;
			this.need_fine = (need_fine_pos ? 2 : Src.need_fine);
			return this;
		}

		public MobSkltPosition CopyFromCorrectly(MobSkltPosition Src)
		{
			this.CopyFrom(Src, false);
			this.need_fine = Src.need_fine;
			return this;
		}

		public MobSkltPosition Multiply(MobSkltPosition Src, MobSkltPosition Mul)
		{
			this.CopyFromCorrectly(Src);
			if (Mul == null)
			{
				return this;
			}
			return this.Multiply(Mul);
		}

		public MobSkltPosition Multiply(MobSkltPosition Mul)
		{
			if (!Mul.Pos_.Equals(Vector2.zero))
			{
				this.Pos_ += Mul.Pos_;
				this.need_fine_pos2abs = true;
			}
			if (Mul.rotateR_ != 0f)
			{
				this.rotateR_ += Mul.rotateR_;
				this.need_fine_pos2abs = true;
			}
			this.ScaleLT *= Mul.ScaleLT;
			this.ScaleTR *= Mul.ScaleTR;
			this.ScaleRB *= Mul.ScaleRB;
			this.ScaleBL *= Mul.ScaleBL;
			return this;
		}

		public MobSkltPosition Divide(MobSkltPosition Rsl, MobSkltPosition Src)
		{
			this.Pos_ = Rsl.Pos_;
			if (!Src.Pos_.Equals(Vector2.zero))
			{
				this.Pos_ -= Src.Pos_;
			}
			this.rotateR_ = Rsl.rotateR_;
			if (Src.rotateR_ != 0f)
			{
				this.rotateR_ -= Src.rotateR_;
			}
			this.need_fine_pos2abs = true;
			MobSkltPosition.ScaleDivide(ref this.ScaleLT, Rsl.ScaleLT, Src.ScaleLT);
			MobSkltPosition.ScaleDivide(ref this.ScaleTR, Rsl.ScaleTR, Src.ScaleTR);
			MobSkltPosition.ScaleDivide(ref this.ScaleRB, Rsl.ScaleRB, Src.ScaleRB);
			MobSkltPosition.ScaleDivide(ref this.ScaleBL, Rsl.ScaleBL, Src.ScaleBL);
			return this;
		}

		private static void ScaleDivide(ref Vector2 Ans, Vector2 Rsl, Vector2 Src)
		{
			if (Src.x != 0f)
			{
				Ans.x = Rsl.x / Src.x;
			}
			if (Src.y != 0f)
			{
				Ans.y = Rsl.y / Src.y;
			}
		}

		public Vector2 PosAbs
		{
			get
			{
				return this.PosAbs_;
			}
			set
			{
				this.PosAbs_ = value;
				this.need_fine_abs2pos = true;
			}
		}

		public Vector2 Pos
		{
			get
			{
				return this.Pos_;
			}
			set
			{
				this.Pos_ = value;
				this.need_fine_pos2abs = true;
			}
		}

		public Vector3 AbsPosAndRot
		{
			get
			{
				return new Vector3(this.PosAbs_.x, this.PosAbs_.y, this.rotateR_abs_);
			}
			set
			{
				this.PosAbs = value;
				this.rotateR_abs = value.z;
			}
		}

		public float rotateR_abs
		{
			get
			{
				return this.rotateR_abs_;
			}
			set
			{
				this.rotateR_abs_ = value;
				this.need_fine_abs2pos = true;
			}
		}

		public float rotateR
		{
			get
			{
				return this.rotateR_;
			}
			set
			{
				this.rotateR_ = value;
				this.need_fine_pos2abs = true;
			}
		}

		public bool finePositionBase()
		{
			if (this.need_fine_abs2pos)
			{
				this.need_fine = 0;
				this.Pos_ = this.PosAbs_;
				this.rotateR_ = this.rotateR_abs_;
				return true;
			}
			if (this.need_fine_pos2abs)
			{
				this.need_fine = 0;
				this.rotateR_abs_ = this.rotateR_;
				this.PosAbs_ = this.Pos_;
				return true;
			}
			return false;
		}

		public MobSkltPosition finePosition(SkltJoint Jt, bool execute_a2p = true, bool execute_p2a = true)
		{
			if (this.need_fine_abs2pos && execute_a2p)
			{
				this.fineAbs2Pos(Jt);
			}
			if (this.need_fine_pos2abs && execute_p2a)
			{
				this.finePos2Abs(Jt);
			}
			return this;
		}

		public MobSkltPosition finePosition(SkltImage Img, bool execute_a2p = true, bool execute_p2a = true)
		{
			if (this.need_fine_abs2pos && execute_a2p)
			{
				this.fineAbs2Pos(Img);
			}
			if (this.need_fine_pos2abs && execute_p2a)
			{
				this.finePos2Abs(Img);
			}
			return this;
		}

		public MobSkltPosition finePosition(MobSkltPosition ParentMSP, MobSkltPosition.SizeM ParentPartsSize, bool execute_a2p = true, bool execute_p2a = true)
		{
			if (this.need_fine_abs2pos && execute_a2p)
			{
				this.fineAbs2Pos(ParentMSP, ParentPartsSize);
			}
			if (this.need_fine_pos2abs && execute_p2a)
			{
				this.finePos2Abs(ParentMSP, ParentPartsSize);
			}
			return this;
		}

		public void finePos2Abs(SkltJoint Jt)
		{
			this.need_fine = 0;
			SkltParts belongS = Jt.BelongS;
			MobSkltPosition jointBase = belongS.getJointBase();
			this.rotateR_abs_ = this.rotateR_ + jointBase.rotateR_abs;
			this.PosAbs_ = MobSkltPosition.calcPositionP2A(this.Pos_, belongS.PartsSize, jointBase);
		}

		public void finePos2Abs(MobSkltPosition ParentMSP, MobSkltPosition.SizeM ParentPartsSize)
		{
			this.need_fine = 0;
			if (ParentMSP == null || ParentMSP == this)
			{
				this.rotateR_abs_ = this.rotateR_;
				this.PosAbs_ = this.Pos_;
				return;
			}
			this.rotateR_abs_ = this.rotateR_ + ParentMSP.rotateR_abs;
			this.PosAbs_ = MobSkltPosition.calcPositionP2A(this.Pos_, ParentPartsSize, ParentMSP);
		}

		public static Vector2 calcPositionP2A(Vector2 P, MobSkltPosition.SizeM Size, MobSkltPosition ParentMSP)
		{
			return MobSkltPosition.calcPositionP2AS(P, ParentMSP.PosAbs, ParentMSP.rotateR_abs, Size, ParentMSP.ScaleLT, ParentMSP.ScaleTR, ParentMSP.ScaleRB, ParentMSP.ScaleBL);
		}

		private static Vector2 calcPositionP2AS(Vector2 P0, Vector2 Center, float rotateR, MobSkltPosition.SizeM Size, Vector2 ScaleLT, Vector2 ScaleTR, Vector2 ScaleRB, Vector2 ScaleBL)
		{
			Vector2 vector = new Vector2(Size.x * 0.5f, Size.y * 0.5f);
			Vector2 vector2 = new Vector2(-vector.x, vector.y) * ScaleBL;
			Vector2 vector3 = new Vector2(vector.x, -vector.y) * ScaleTR;
			Vector2 vector4 = new Vector2(vector.x, vector.y) * ScaleRB;
			Vector2 vector5 = new Vector2(-vector.x, -vector.y) * ScaleLT;
			float ydivx = Size.ydivx;
			Vector2 vector6;
			if (P0.y > -ydivx * P0.x == -vector.y > -ydivx * -vector.x)
			{
				float num = (P0.x - -vector.x) * Size.xdiv;
				float num2 = (P0.y - -vector.y) * Size.ydiv;
				vector6 = (vector2 - vector5) * num2 + (vector3 - vector5) * num + vector5;
			}
			else
			{
				float num = -(P0.x - vector.x) * Size.xdiv;
				float num2 = -(P0.y - vector.y) * Size.ydiv;
				vector6 = (vector2 - vector4) * num + (vector3 - vector4) * num2 + vector4;
			}
			return X.ROTV2e(vector6, rotateR) + Center;
		}

		public void fineAbs2Pos(SkltJoint Jt)
		{
			this.need_fine = 0;
			MobSkltPosition jointBase = Jt.BelongS.getJointBase();
			this.rotateR_ = this.rotateR_abs_ - jointBase.rotateR_abs;
			this.Pos_ = MobSkltPosition.calcPositionA2P(this.PosAbs_, Jt.BelongS.PartsSize, jointBase);
		}

		public void fineAbs2Pos(MobSkltPosition ParentMSP, MobSkltPosition.SizeM ParentPartsSize)
		{
			this.need_fine = 0;
			if (ParentMSP == null || ParentMSP == this)
			{
				this.rotateR_ = this.rotateR_abs_;
				this.Pos_ = this.PosAbs_;
				return;
			}
			this.rotateR_ = this.rotateR_abs_ - ParentMSP.rotateR_abs;
			this.Pos_ = MobSkltPosition.calcPositionA2P(this.PosAbs_, ParentPartsSize, ParentMSP);
		}

		public static Vector2 calcPositionA2P(Vector2 PA, MobSkltPosition.SizeM Size, MobSkltPosition ParentMSP)
		{
			return MobSkltPosition.calcPositionA2PS(PA, ParentMSP.PosAbs, ParentMSP.rotateR_abs, Size, ParentMSP.ScaleLT, ParentMSP.ScaleTR, ParentMSP.ScaleRB, ParentMSP.ScaleBL);
		}

		private static Vector2 calcPositionA2PS(Vector2 PA, Vector2 Center, float rotateR, MobSkltPosition.SizeM Size, Vector2 ScaleLT, Vector2 ScaleTR, Vector2 ScaleRB, Vector2 ScaleBL)
		{
			Vector2 vector = X.ROTV2e(PA - Center, -rotateR);
			Vector2 vector2 = new Vector2(Size.x * 0.5f, Size.y * 0.5f);
			Vector2 vector3 = new Vector2(-vector2.x, vector2.y) * ScaleBL;
			Vector2 vector4 = new Vector2(vector2.x, -vector2.y) * ScaleTR;
			if (vector3.Equals(vector4))
			{
				return Vector2.zero;
			}
			Vector2 vector5 = new Vector2(vector2.x, vector2.y) * ScaleRB;
			Vector2 vector6 = new Vector2(-vector2.x, -vector2.y) * ScaleLT;
			float num;
			float num2;
			float num3;
			float num4;
			X.calcLineGraphVariable(vector3.x, vector3.y, vector4.x, vector4.y, out num, out num2, out num3, out num4);
			if (num * vector.x + num2 * vector.y + num3 > 0f == num * vector6.x + num2 * vector6.y + num3 > 0f)
			{
				Vector3 vector7 = X.getUvFrom3Point(vector, vector6, vector4, vector3, Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, 1f));
				if (vector7.z < 0f)
				{
					return Vector2.zero;
				}
				return new Vector2(X.NI(-vector2.x, vector2.x, vector7.x), X.NI(-vector2.y, vector2.y, vector7.y));
			}
			else
			{
				Vector3 vector7 = X.getUvFrom3Point(vector, vector5, vector3, vector4, Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, 1f));
				if (vector7.z < 0f)
				{
					return Vector2.zero;
				}
				return new Vector2(X.NI(vector2.x, -vector2.x, vector7.x), X.NI(vector2.y, -vector2.y, vector7.y));
			}
		}

		public void finePos2Abs(SkltImage Img)
		{
			this.finePos2Abs(Img.Con.getJointBase(), Img.Con.PartsSize);
		}

		public void fineAbs2Pos(SkltImage Img)
		{
			this.need_fine = 0;
			MobSkltPosition jointBase = Img.Con.getJointBase();
			this.rotateR_ = this.rotateR_abs_ - jointBase.rotateR_abs_;
			this.Pos_ = MobSkltPosition.calcPositionA2P(this.PosAbs_, Img.Con.PartsSize, jointBase);
		}

		public Rect calcBounds(SkltImageSrc.ISrcPat Pat, Vector3 RotateCenter = default(Vector3))
		{
			return this.calcBounds(new Vector2((float)Pat.Img.width, (float)Pat.Img.height), RotateCenter, Pat.Shift);
		}

		public Vector2 getCorner(int i, SkltImage Img)
		{
			PxlImage img = Img.Source.Source.Img;
			return this.getCornerH(i, new Vector2((float)img.width * 0.5f, (float)img.height * 0.5f));
		}

		public Vector2 getCornerH(int i, Vector2 SizeH)
		{
			Vector2 vector;
			switch (i)
			{
			case 0:
				vector = new Vector2(-SizeH.x, SizeH.y) * this.ScaleBL;
				break;
			case 1:
				vector = new Vector2(-SizeH.x, -SizeH.y) * this.ScaleLT;
				break;
			case 2:
				vector = new Vector2(SizeH.x, -SizeH.y) * this.ScaleTR;
				break;
			default:
				vector = new Vector2(SizeH.x, SizeH.y) * this.ScaleRB;
				break;
			}
			return vector;
		}

		public Rect calcBounds(Vector2 Size, Vector3 RotateCenter = default(Vector3), Vector2 Shift = default(Vector2))
		{
			MobSkltPosition.RcBuf.Set(0f, 0f, 0f, 0f);
			Vector2 vector = Size * 0.5f;
			float num = X.Cos(this.rotateR_abs);
			float num2 = X.Sin(this.rotateR_abs);
			float num3 = 1f;
			float num4 = 0f;
			if (RotateCenter.z != 0f)
			{
				num3 = X.Cos(-RotateCenter.z);
				num4 = X.Sin(-RotateCenter.z);
			}
			for (int i = 0; i < 4; i++)
			{
				Vector2 vector2 = this.getCornerH(i, vector);
				vector2 = X.ROTV2e(vector2, num, num2) + this.PosAbs + Shift;
				if (!object.Equals(RotateCenter, Vector3.zero))
				{
					vector2 = X.ROTV2e(vector2 - RotateCenter, num3, num4);
				}
				MobSkltPosition.RcBuf.Expand(vector2.x, vector2.y, 0f, 0f, i >= 1);
			}
			return MobSkltPosition.RcBuf.getRect();
		}

		public bool need_fine_abs2pos
		{
			get
			{
				return this.need_fine == 1;
			}
			set
			{
				if (value)
				{
					this.need_fine = 1;
				}
			}
		}

		public bool need_fine_pos2abs
		{
			get
			{
				return this.need_fine == 2;
			}
			set
			{
				if (value)
				{
					this.need_fine = 2;
				}
			}
		}

		public Vector2 Scale
		{
			get
			{
				return (this.ScaleLT + this.ScaleTR + this.ScaleRB + this.ScaleBL) * 0.25f;
			}
			set
			{
				this.ScaleBL = value;
				this.ScaleRB = value;
				this.ScaleTR = value;
				this.ScaleLT = value;
			}
		}

		public byte flipping_flag
		{
			get
			{
				int num = 0;
				Vector2 vector = this.ScaleLT;
				if (vector.x * vector.y < 0f)
				{
					num |= 2;
				}
				else
				{
					num |= 1;
				}
				vector = this.ScaleRB;
				if (vector.x * vector.y < 0f)
				{
					num |= 2;
				}
				else
				{
					num |= 1;
				}
				vector = this.ScaleRB;
				if (vector.x * vector.y < 0f)
				{
					num |= 2;
				}
				else
				{
					num |= 1;
				}
				vector = this.ScaleLT;
				if (vector.x * vector.y < 0f)
				{
					num |= 2;
				}
				else
				{
					num |= 1;
				}
				return (byte)num;
			}
		}

		public byte calcFlippingFlag(byte p)
		{
			byte flipping_flag = this.flipping_flag;
			if (flipping_flag == 2)
			{
				return (p == 1) ? 2 : ((p == 2) ? 1 : p);
			}
			if (flipping_flag == 1)
			{
				return p;
			}
			return flipping_flag | p;
		}

		internal static MobSkltPosition readFromBytes(ByteArray Ba, MobSkltPosition Msp)
		{
			Vector2 vector = Ba.readVector2();
			Vector2 vector2 = Ba.readVector2();
			Vector2 vector3 = Ba.readVector2();
			Vector2 vector4 = Ba.readVector2();
			float num = Ba.readFloat();
			Vector2 vector5 = Ba.readVector2();
			if (Msp != null)
			{
				Msp.ScaleLT = vector;
				Msp.ScaleTR = vector2;
				Msp.ScaleRB = vector3;
				Msp.ScaleBL = vector4;
				Msp.rotateR_abs_ = num;
				Msp.PosAbs_ = vector5;
				Msp.need_fine_abs2pos = true;
			}
			return Msp;
		}

		private Vector2 Pos_;

		private float rotateR_;

		public Vector2 ScaleLT = Vector2.one;

		public Vector2 ScaleTR = Vector2.one;

		public Vector2 ScaleRB = Vector2.one;

		public Vector2 ScaleBL = Vector2.one;

		private float rotateR_abs_;

		private Vector2 PosAbs_;

		private byte need_fine;

		private static DRect RcBuf = new DRect("RcBuf");

		public interface IPosSyncable
		{
			SkltSequence.SkltDesc getSqDescKey();

			MobSkltPosition getMsp();
		}

		public struct SizeM
		{
			public SizeM(float _x, float _y)
			{
				this.x = _x;
				this.y = _y;
				this.xdiv = 1f / this.x;
				this.ydiv = 1f / this.y;
			}

			public bool valid
			{
				get
				{
					return this.x > 0f && this.y > 0f;
				}
			}

			public float ydivx
			{
				get
				{
					return this.y * this.xdiv;
				}
			}

			public readonly float x;

			public readonly float y;

			public readonly float xdiv;

			public readonly float ydiv;
		}
	}
}

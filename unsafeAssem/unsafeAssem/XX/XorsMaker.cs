using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PixelLiner.PixelLinerLib;

namespace XX
{
	public class XorsMaker
	{
		public XorsMaker(bool use_save)
		{
			this.Randseed = new uint[4];
			this.randA = new uint[128];
			if (use_save)
			{
				this.RandseedFirst = new uint[4];
			}
		}

		public XorsMaker(uint randseedset = 0U, bool use_save = false)
			: this(use_save)
		{
			this.init(false, randseedset, 0U, 0U, 0U);
		}

		public XorsMaker(uint randseedset0, uint randseedset1, uint randseedset2, uint randseedset3, bool use_save = false)
			: this(use_save)
		{
			this.init(false, randseedset0, randseedset1, randseedset2, randseedset3);
		}

		public XorsMaker(ByteArray Ba)
			: this(true)
		{
			this.readBinaryFrom(Ba, false);
		}

		public void reset(bool no_clear_randA = true)
		{
			if (this.RandseedFirst != null)
			{
				this.init(this.RandseedFirst, no_clear_randA);
			}
		}

		public void updateFirst(int skip_first = 0)
		{
			if (this.RandseedFirst != null)
			{
				while (--skip_first >= 0)
				{
					this.get0();
				}
				this.init(false, this.Randseed[0], this.Randseed[1], this.Randseed[2], this.Randseed[3]);
			}
		}

		public uint init(bool noClearRandA = false, uint randseedset = 0U, uint randseedset1 = 0U, uint randseedset2 = 0U, uint randseedset3 = 0U)
		{
			DateTime now = DateTime.Now;
			uint num;
			if (randseedset != 0U)
			{
				num = randseedset;
			}
			else
			{
				uint num2 = (uint)(now.Ticks / 10000L);
				uint num3 = this.times;
				this.times = num3 + 1U;
				num = num2 + num3;
			}
			randseedset = num;
			this.Randseed[0] = randseedset;
			this.Randseed[1] = ((randseedset1 != 0U) ? randseedset1 : (this.Randseed[0] + 123456789U));
			this.Randseed[2] = ((randseedset2 != 0U) ? randseedset2 : (this.Randseed[0] + 362436069U));
			this.Randseed[3] = ((randseedset3 != 0U) ? randseedset3 : (this.Randseed[0] + 521288629U));
			if (this.RandseedFirst != null)
			{
				Array.Copy(this.Randseed, this.RandseedFirst, 4);
			}
			if (!noClearRandA)
			{
				this.clearRandA();
			}
			return randseedset;
		}

		public uint init(uint[] Arandseed, bool noClearRandA = true)
		{
			return this.init(noClearRandA, Arandseed[0], Arandseed[1], Arandseed[2], Arandseed[3]);
		}

		public void clearRandA()
		{
			this.get0();
			this.get0();
			this.get0();
			this.get0();
			for (uint num = 0U; num < 128U; num += 1U)
			{
				this.randA[(int)num] = this.get0();
			}
		}

		public float XORSP()
		{
			this.Bytes.In = (this.get0() >> 9) | 1065353216U;
			return this.Bytes.Out - 1f;
		}

		public uint get0()
		{
			uint num = this.Randseed[0] ^ (this.Randseed[0] << 11);
			this.Randseed[0] = this.Randseed[1];
			this.Randseed[1] = this.Randseed[2];
			this.Randseed[2] = this.Randseed[3];
			return this.Randseed[3] = this.Randseed[3] ^ (this.Randseed[3] >> 19) ^ (num ^ (num >> 8));
		}

		public uint get2(uint seed1, uint seed2)
		{
			return (this.randA[(int)(seed1 & 63U)] ^ this.randA[(int)(64U + ((seed2 * 137U + 69U) & 63U))]) & 134217727U;
		}

		public uint get2O(uint seed1, uint seed2)
		{
			return (this.randA[(int)(seed1 & 127U)] / ((seed2 & 65535U) + 1U)) & 134217727U;
		}

		public int xors(int i)
		{
			if (i > 0)
			{
				return (int)(this.get0() % (uint)i);
			}
			return 0;
		}

		public int map(int l0, int l1 = 0, int l2 = 0, int l3 = 0, int l4 = 0)
		{
			int num = this.xors(l0 + l1 + l2 + l3 + l4);
			if (num < l0)
			{
				return 0;
			}
			if (num < l0 + l1)
			{
				return 1;
			}
			if (num < l0 + l1 + l2)
			{
				return 2;
			}
			if (num < l0 + l1 + l2 + l3)
			{
				return 3;
			}
			return 4;
		}

		public uint comb_bits(int va, int vb)
		{
			if (vb <= 0 || va <= 0)
			{
				return 0U;
			}
			if (vb >= va)
			{
				return (1U << va) - 1U;
			}
			uint num = 0U;
			while (--vb >= 0)
			{
				int num2 = (int)((this.get0() & 2147483647U) % (uint)va);
				int num3 = 0;
				while (num3 < va && (num & (1U << num2)) != 0U)
				{
					num2 = (num2 + 1) % va;
					num3++;
				}
				num |= 1U << num2;
			}
			return num;
		}

		public void shuffle<T>(T[] A, int arraymax = -1)
		{
			arraymax = ((arraymax < 0) ? A.Length : arraymax);
			int num = (arraymax - 1) * 23;
			for (int i = 0; i < num; i++)
			{
				int num2 = (int)((ulong)this.get0() % (ulong)((long)arraymax));
				int num3 = (int)((ulong)this.get0() % (ulong)((long)arraymax));
				int num4 = num2;
				int num5 = num3;
				T t = A[num3];
				T t2 = A[num2];
				A[num4] = t;
				A[num5] = t2;
			}
		}

		public void shuffle<T>(List<T> A, int arraymax = -1)
		{
			arraymax = ((arraymax < 0) ? A.Count : arraymax);
			int num = (arraymax - 1) * 23;
			for (int i = 0; i < num; i++)
			{
				int num2 = (int)((ulong)this.get0() % (ulong)((long)arraymax));
				int num3 = (int)((ulong)this.get0() % (ulong)((long)arraymax));
				int num4 = num2;
				int num5 = num3;
				T t = A[num3];
				T t2 = A[num2];
				A[num4] = t;
				A[num5] = t2;
			}
		}

		public XorsMaker saveRandSeeds(uint[] Av)
		{
			if (this.RandseedFirst == null)
			{
				X.de("正しい保存結果を得るには use_save をオンにして作成する必要があります", null);
				Av[0] = this.Randseed[0];
				Av[1] = this.Randseed[1];
				Av[2] = this.Randseed[2];
				Av[3] = this.Randseed[3];
			}
			else
			{
				Av[0] = this.RandseedFirst[0];
				Av[1] = this.RandseedFirst[1];
				Av[2] = this.RandseedFirst[2];
				Av[3] = this.RandseedFirst[3];
			}
			return this;
		}

		public virtual void readBinaryFrom(ByteReader Ba, bool noClearRandA = false)
		{
			if (this.RandseedFirst == null)
			{
				this.RandseedFirst = new uint[4];
			}
			this.RandseedFirst[0] = (this.Randseed[0] = Ba.readUInt());
			this.RandseedFirst[1] = (this.Randseed[1] = Ba.readUInt());
			this.RandseedFirst[2] = (this.Randseed[2] = Ba.readUInt());
			this.RandseedFirst[3] = (this.Randseed[3] = Ba.readUInt());
			if (!noClearRandA)
			{
				this.clearRandA();
			}
		}

		public virtual void writeBinaryTo(ByteArray Ba)
		{
			if (this.RandseedFirst == null)
			{
				X.de("正しい保存結果を得るには use_save をオンにして作成する必要があります", null);
				Ba.writeUInt(this.Randseed[0]);
				Ba.writeUInt(this.Randseed[1]);
				Ba.writeUInt(this.Randseed[2]);
				Ba.writeUInt(this.Randseed[3]);
				return;
			}
			Ba.writeUInt(this.RandseedFirst[0]);
			Ba.writeUInt(this.RandseedFirst[1]);
			Ba.writeUInt(this.RandseedFirst[2]);
			Ba.writeUInt(this.RandseedFirst[3]);
		}

		private uint times;

		private uint[] Randseed;

		public uint[] randA;

		private uint[] RandseedFirst;

		private XorsMaker.UnionBytes Bytes;

		public const int binary_count = 16;

		[StructLayout(LayoutKind.Explicit)]
		private struct UnionBytes
		{
			[FieldOffset(0)]
			public float Out;

			[FieldOffset(0)]
			public uint In;
		}
	}
}

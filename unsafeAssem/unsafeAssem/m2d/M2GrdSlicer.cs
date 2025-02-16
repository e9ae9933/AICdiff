using System;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2GrdSlicer
	{
		public M2GrdSlicer(M2GrdSlicer Src)
		{
			if (Src != null)
			{
				this.copyFrom(Src);
			}
		}

		public M2GrdSlicer copyFrom(M2GrdSlicer Src)
		{
			if (Src != null)
			{
				this.clms = Src.clms;
				this.rows = Src.rows;
				this.Alevel = X.concat<float>(Src.Alevel, null, -1, -1);
				this.Ax = X.concat<float>(Src.Ax, null, -1, -1);
				this.Ay = X.concat<float>(Src.Ay, null, -1, -1);
				this.alloc(false);
			}
			return this;
		}

		public M2GrdSlicer(int _clms, int _rows)
		{
			this.clms = _clms;
			this.rows = _rows;
			this.alloc(false);
		}

		public M2GrdSlicer(M2GradationRect.GRD_DIR slc)
		{
			this.Clear(slc);
		}

		public M2GrdSlicer Clear(M2GradationRect.GRD_DIR slc)
		{
			this.clms = (this.rows = 2);
			switch (slc)
			{
			case M2GradationRect.GRD_DIR.T:
				this.prepareDefault(0f, 1f, 1f, 0f);
				return this;
			case M2GradationRect.GRD_DIR.R:
				this.prepareDefault(0f, 0f, 1f, 1f);
				return this;
			case M2GradationRect.GRD_DIR.B:
				this.prepareDefault(1f, 0f, 0f, 1f);
				return this;
			case M2GradationRect.GRD_DIR.LT:
				this.prepareDefault(0.5f, 1f, 0.5f, 0f);
				return this;
			case M2GradationRect.GRD_DIR.TR:
				this.prepareDefault(0f, 0.5f, 1f, 0.5f);
				return this;
			case M2GradationRect.GRD_DIR.BL:
				this.prepareDefault(1f, 0.5f, 0f, 0.5f);
				return this;
			case M2GradationRect.GRD_DIR.RB:
				this.prepareDefault(0.5f, 0f, 0.5f, 1f);
				return this;
			case M2GradationRect.GRD_DIR.SLT:
				this.prepareDefault(1f, 1f, 1f, 0f);
				return this;
			case M2GradationRect.GRD_DIR.STR:
				this.prepareDefault(0f, 1f, 1f, 1f);
				return this;
			case M2GradationRect.GRD_DIR.SBL:
				this.prepareDefault(1f, 1f, 0f, 1f);
				return this;
			case M2GradationRect.GRD_DIR.SRB:
				this.prepareDefault(1f, 0f, 1f, 1f);
				return this;
			}
			this.prepareDefault(1f, 1f, 0f, 0f);
			return this;
		}

		public void alloc(bool save_vars = false)
		{
			this.clms = X.Mx(this.clms, 2);
			this.rows = X.Mx(this.rows, 2);
			int num = this.clms - 2;
			int num2 = this.rows - 2;
			int num3 = this.clms * this.rows;
			int num4 = this.clms;
			int num5 = this.rows;
			float[] array = null;
			float[] array2 = null;
			if (this.Ax == null)
			{
				this.Ax = new float[num];
			}
			else if (this.Ax.Length != num)
			{
				float[] array3 = (save_vars ? X.concat<float>(this.Ax, null, -1, -1) : null);
				Array.Resize<float>(ref this.Ax, num);
				if (array3 != null)
				{
					int num6 = array3.Length;
					num4 = num6 + 2;
					array = array3;
					for (int i = 0; i < num; i++)
					{
						if (i == num - 1)
						{
							this.Ax[i] = array3[num6 - 1];
						}
						else if (i < num6 - 1)
						{
							this.Ax[i] = array3[i];
						}
						else
						{
							this.Ax[i] = X.NIL(array3[num6 - 2], array3[num6 - 1], (float)(i + 1 - (num6 - 1)), (float)(1 + num - num6));
						}
					}
				}
			}
			if (this.Ay == null)
			{
				this.Ay = new float[num2];
			}
			else if (this.Ay.Length != num2)
			{
				float[] array3 = (save_vars ? X.concat<float>(this.Ay, null, -1, -1) : null);
				Array.Resize<float>(ref this.Ay, num2);
				if (array3 != null)
				{
					int num7 = array3.Length;
					num5 = num7 + 2;
					array2 = array3;
					for (int j = 0; j < num2; j++)
					{
						if (j == num2 - 1)
						{
							this.Ay[j] = array3[num7 - 1];
						}
						else if (j < num7 - 1)
						{
							this.Ay[j] = array3[j];
						}
						else
						{
							this.Ay[j] = X.NIL(array3[num7 - 2], array3[num7 - 1], (float)(j + 1 - (num7 - 1)), (float)(1 + num2 - num7));
						}
					}
				}
			}
			if (this.Alevel == null)
			{
				this.Alevel = new float[num3];
				return;
			}
			if (this.Alevel.Length != num3)
			{
				float[] array3 = ((save_vars && (num4 != this.clms || num5 != this.rows) && (array != null || array2 != null)) ? X.concat<float>(this.Alevel, null, -1, -1) : null);
				Array.Resize<float>(ref this.Alevel, num3);
				if (array3 != null)
				{
					float[] array4 = array ?? this.Ax;
					float[] array5 = array2 ?? this.Ay;
					for (int k = 0; k < num3; k++)
					{
						int num8;
						int num9;
						this.getPos(k, out num8, out num9);
						this.Alevel[k] = array3[num4 * X.Mn(num9, num5 - 1) + X.Mn(num8, num4 - 1)];
					}
				}
			}
		}

		private void prepareDefault(float lv_BL, float lv_LT, float lv_TR, float lv_RB)
		{
			this.alloc(false);
			int num = this.Alevel.Length;
			for (int i = 0; i < num; i++)
			{
				float num2;
				float num3;
				this.getPosLevel(i, out num2, out num3);
				float num4 = X.NI(lv_BL, lv_RB, num2);
				float num5 = X.NI(lv_LT, lv_TR, num2);
				this.Alevel[i] = X.NI(num4, num5, num3);
			}
		}

		public void getPos(int i, out int x, out int y)
		{
			M2GrdSlicer.getPosS(i, this.clms, out x, out y);
		}

		public static void getPosS(int i, int clms, out int x, out int y)
		{
			x = i % clms;
			y = i / clms;
		}

		public void getPosLevel(int i, out float xlv, out float ylv)
		{
			M2GrdSlicer.getPosLevelS(i, this.Ax, this.Ay, out xlv, out ylv);
		}

		public static void getPosLevelS(int i, float[] Ax, float[] Ay, out float xlv, out float ylv)
		{
			int num = Ax.Length + 2;
			int num2 = Ay.Length + 2;
			int num3;
			int num4;
			M2GrdSlicer.getPosS(i, num, out num3, out num4);
			if (num3 == 0)
			{
				xlv = 0f;
			}
			else if (num3 == num - 1)
			{
				xlv = 1f;
			}
			else
			{
				xlv = Ax[num3 - 1];
			}
			if (num4 == 0)
			{
				ylv = 0f;
				return;
			}
			if (num4 == num2 - 1)
			{
				ylv = 1f;
				return;
			}
			ylv = Ay[num4 - 1];
		}

		public static float getRawLevel(float level, out int head_val)
		{
			head_val = 0;
			if (level >= 2f)
			{
				level -= 2f;
				head_val += 2;
			}
			return level;
		}

		public void drawTo(MeshDrawer Md, float w, float h, Color32 C0, Color32 C1, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				w *= 0.015625f;
				h *= 0.015625f;
			}
			float num = w * 0.5f;
			float num2 = h * 0.5f;
			Md.allocTri(Md.getTriMax() + 6 * (this.clms - 1) * (this.rows - 1), 60).allocVer(Md.getVertexMax() + this.clms * this.rows, 64);
			for (int i = 0; i < this.rows - 1; i++)
			{
				for (int j = 0; j < this.clms - 1; j++)
				{
					int num3 = i * this.clms + j;
					float num4 = this.Alevel[num3];
					if (num4 >= 2f)
					{
						num4 -= 2f;
						Md.Tri(num3, num3 + this.clms, num3 + 1, false).Tri(num3 + this.clms, num3 + this.clms + 1, num3 + 1, false);
					}
					else
					{
						Md.TriRectBL(num3, num3 + this.clms, num3 + this.clms + 1, num3 + 1);
					}
				}
			}
			Color32 c = Md.ColGrd.C;
			int num5 = this.Alevel.Length;
			int num6 = 0;
			int num7 = 0;
			for (int k = 0; k < num5; k++)
			{
				int num8;
				float rawLevel = M2GrdSlicer.getRawLevel(this.Alevel[k], out num8);
				if (rawLevel == 0f)
				{
					Md.ColGrd.Set(C0);
				}
				else if (rawLevel == 1f)
				{
					Md.ColGrd.Set(C1);
				}
				else
				{
					Md.ColGrd.Set(C0).blend(C1, rawLevel);
				}
				Md.Pos(-num + w * ((num6 == 0) ? 0f : ((num6 == this.clms - 1) ? 1f : this.Ax[num6 - 1])), -num2 + h * ((num7 == 0) ? 0f : ((num7 == this.rows - 1) ? 1f : this.Ay[num7 - 1])), Md.ColGrd);
				if (++num6 >= this.clms)
				{
					num6 = 0;
					num7++;
				}
			}
			Md.ColGrd.Set(c);
		}

		public void Update()
		{
		}

		public static M2GrdSlicer readBytesContentGrdSlicer(ByteArray Ba, M2GrdSlicer Gsl = null)
		{
			int num = (int)Ba.readUByte();
			if (num <= 0)
			{
				return null;
			}
			int num2 = (int)Ba.readUByte();
			if (Gsl == null)
			{
				Gsl = new M2GrdSlicer(num, num2);
			}
			else
			{
				Gsl.clms = num;
				Gsl.rows = num2;
				Gsl.alloc(false);
			}
			int num3 = Gsl.clms;
			num2 = Gsl.rows;
			int num4 = Gsl.Ax.Length;
			for (int i = 0; i < num4; i++)
			{
				Gsl.Ax[i] = Ba.readFloat();
			}
			num4 = Gsl.Ay.Length;
			for (int j = 0; j < num4; j++)
			{
				Gsl.Ay[j] = Ba.readFloat();
			}
			num4 = Gsl.Alevel.Length;
			for (int k = 0; k < num4; k++)
			{
				Gsl.Alevel[k] = Ba.readFloat();
			}
			return Gsl;
		}

		public void writeBytesTo(ByteArray Ba)
		{
			this.alloc(false);
			Ba.writeByte(this.clms);
			Ba.writeByte(this.rows);
			int num = this.Ax.Length;
			for (int i = 0; i < num; i++)
			{
				Ba.writeFloat(this.Ax[i]);
			}
			num = this.Ay.Length;
			for (int j = 0; j < num; j++)
			{
				Ba.writeFloat(this.Ay[j]);
			}
			num = this.Alevel.Length;
			for (int k = 0; k < num; k++)
			{
				Ba.writeFloat(this.Alevel[k]);
			}
		}

		public void getRawPosAndLevel(out float[] Ax, out float[] Ay, out float[] Alevel)
		{
			Ax = this.Ax;
			Ay = this.Ay;
			Alevel = this.Alevel;
		}

		public int clms = 2;

		public int rows = 2;

		public const int LEVEL_FLIP_TRI = 2;

		private float[] Alevel;

		private float[] Ax;

		private float[] Ay;
	}
}

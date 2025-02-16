using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class C32
	{
		public byte r
		{
			get
			{
				return this.C.r;
			}
			set
			{
				this.C.r = value;
			}
		}

		public byte g
		{
			get
			{
				return this.C.g;
			}
			set
			{
				this.C.g = value;
			}
		}

		public byte b
		{
			get
			{
				return this.C.b;
			}
			set
			{
				this.C.b = value;
			}
		}

		public byte a
		{
			get
			{
				return this.C.a;
			}
			set
			{
				this.C.a = value;
			}
		}

		public float v
		{
			get
			{
				return (float)(this.C.r + this.C.g + this.C.b) / 3f * 100f / 255f;
			}
		}

		public uint rgba
		{
			get
			{
				return (uint)(((int)this.C.a << 24) | ((int)this.C.r << 16) | ((int)this.C.g << 8) | (int)this.C.b);
			}
			set
			{
				this.C.r = (byte)((value >> 16) & 255U);
				this.C.g = (byte)((value >> 8) & 255U);
				this.C.b = (byte)(value & 255U);
				this.C.a = (byte)((value >> 24) & 255U);
			}
		}

		public uint rgb
		{
			get
			{
				return (uint)(((int)this.C.r << 16) | ((int)this.C.g << 8) | (int)this.C.b);
			}
			set
			{
				this.C.r = (byte)((value >> 16) & 255U);
				this.C.g = (byte)((value >> 8) & 255U);
				this.C.b = (byte)(value & 255U);
			}
		}

		public string rgbax
		{
			get
			{
				string text = this.rgba.ToString("x");
				while (text.Length < 6)
				{
					text = "0" + text;
				}
				return text;
			}
			set
			{
				this.Set(X.NmUI(value, this.rgba, true, true));
			}
		}

		public string rgbx
		{
			get
			{
				string text = this.rgb.ToString("x");
				while (text.Length < 6)
				{
					text = "0" + text;
				}
				return text;
			}
			set
			{
				this.Set(X.NmUI(value, this.rgb, true, true) | (uint)((uint)this.C.a << 24));
			}
		}

		public C32()
		{
			this.C.r = byte.MaxValue;
			this.C.g = byte.MaxValue;
			this.C.b = byte.MaxValue;
			this.C.a = byte.MaxValue;
		}

		public C32(float _r, float _g, float _b, float _a = 255f)
		{
			this.C.r = (byte)_r;
			this.C.g = (byte)_g;
			this.C.b = (byte)_b;
			this.C.a = (byte)_a;
		}

		public C32(C32 _C)
		{
			this.C.r = _C.r;
			this.C.g = _C.g;
			this.C.b = _C.b;
			this.C.a = _C.a;
		}

		public C32(string s)
		{
			this.Set(X.NmUI(s, 0U, true, true));
		}

		public C32(uint _rgba)
		{
			this.C.r = (byte)((_rgba >> 16) & 255U);
			this.C.g = (byte)((_rgba >> 8) & 255U);
			this.C.b = (byte)(_rgba & 255U);
			this.C.a = (byte)((_rgba >> 24) & 255U);
		}

		public C32(int _rgba)
		{
			this.C.r = (byte)((_rgba >> 16) & 255);
			this.C.g = (byte)((_rgba >> 8) & 255);
			this.C.b = (byte)(_rgba & 255);
			this.C.a = (byte)((_rgba >> 24) & 255);
		}

		public C32(Color32 _C)
		{
			this.C.r = _C.r;
			this.C.g = _C.g;
			this.C.b = _C.b;
			this.C.a = _C.a;
		}

		public C32 White()
		{
			this.C.r = (this.C.g = (this.C.b = (this.C.a = byte.MaxValue)));
			return this;
		}

		public C32 Gray()
		{
			this.C.r = (this.C.g = (this.C.b = (this.C.a = 127)));
			return this;
		}

		public C32 Black()
		{
			this.C.r = (this.C.g = (this.C.b = 0));
			this.C.a = byte.MaxValue;
			return this;
		}

		public C32 Transparent()
		{
			this.C.r = (this.C.g = (this.C.b = (this.C.a = 0)));
			return this;
		}

		public C32 TransparentWhite()
		{
			this.C.r = (this.C.g = (this.C.b = byte.MaxValue));
			this.C.a = 0;
			return this;
		}

		public C32 Set(float _r, float _g, float _b, float _a = 255f)
		{
			this.r = (byte)_r;
			this.g = (byte)_g;
			this.b = (byte)_b;
			this.a = (byte)_a;
			return this;
		}

		public C32 Set1(float _r, float _g, float _b, float _a = 1f)
		{
			this.r = (byte)(_r * 255f);
			this.g = (byte)(_g * 255f);
			this.b = (byte)(_b * 255f);
			this.a = (byte)(_a * 255f);
			return this;
		}

		public C32 Set(uint _rgba)
		{
			this.rgba = _rgba;
			return this;
		}

		public C32 Set(int _rgba)
		{
			this.rgba = (uint)_rgba;
			return this;
		}

		public C32 Set(C32 _C)
		{
			this.r = _C.r;
			this.g = _C.g;
			this.b = _C.b;
			this.a = _C.a;
			return this;
		}

		public C32 Set(Color32 _C)
		{
			this.r = _C.r;
			this.g = _C.g;
			this.b = _C.b;
			this.a = _C.a;
			return this;
		}

		public C32 Set2(float _r, float _g, float _b, float _a = 255f)
		{
			this.r = (byte)X.MMX(0f, _r, 255f);
			this.g = (byte)X.MMX(0f, _g, 255f);
			this.b = (byte)X.MMX(0f, _b, 255f);
			this.a = (byte)X.MMX(0f, _a, 255f);
			return this;
		}

		public C32 randomize(float r255, float g255, float b255, float a255 = 0f)
		{
			this.r = (byte)X.MMX(0f, (float)this.r + (X.XORSP() * 2f - 1f) * r255, 255f);
			this.g = (byte)X.MMX(0f, (float)this.g + (X.XORSP() * 2f - 1f) * g255, 255f);
			this.b = (byte)X.MMX(0f, (float)this.b + (X.XORSP() * 2f - 1f) * b255, 255f);
			this.a = (byte)X.MMX(0f, (float)this.a + (X.XORSP() * 2f - 1f) * a255, 255f);
			return this;
		}

		public C32 setR(float _a)
		{
			this.r = (byte)X.MMX(0f, _a, 255f);
			return this;
		}

		public C32 setG(float _a)
		{
			this.g = (byte)X.MMX(0f, _a, 255f);
			return this;
		}

		public C32 setB(float _a)
		{
			this.b = (byte)X.MMX(0f, _a, 255f);
			return this;
		}

		public C32 setA(float _a)
		{
			this.a = (byte)X.MMX(0f, _a, 255f);
			return this;
		}

		public C32 setA1(float _a)
		{
			this.a = (byte)X.MMX(0f, _a * 255f, 255f);
			return this;
		}

		public bool Equals(C32 C)
		{
			return C.r == this.r && C.g == this.g && C.b == this.b && C.a == this.a;
		}

		public static bool isEqual(Color32 Ca, Color32 Cb)
		{
			return Ca.r == Cb.r && Ca.g == Cb.g && Ca.b == Cb.b && Ca.a == Cb.a;
		}

		public C32 mulA(float tz)
		{
			this.a = (byte)X.MMX(0f, (float)this.a * tz, 255f);
			return this;
		}

		public C32 blend(Color32 C, float level = 0.5f)
		{
			if (level <= 0f)
			{
				return this;
			}
			level = X.MMX(0f, level, 1f);
			float num = 1f - level;
			this.r = (byte)X.IntR((float)this.r * num + (float)C.r * level);
			this.g = (byte)X.IntR((float)this.g * num + (float)C.g * level);
			this.b = (byte)X.IntR((float)this.b * num + (float)C.b * level);
			this.a = (byte)X.IntR((float)this.a * num + (float)C.a * level);
			return this;
		}

		public C32 blendRgb(Color32 C, float level = 0.5f, bool multiply_alpha = true)
		{
			if (level <= 0f)
			{
				return this;
			}
			level = X.MMX(0f, level * (multiply_alpha ? ((float)C.a / 255f) : 1f), 1f);
			float num = 1f - level;
			this.r = (byte)X.IntR((float)this.r * num + (float)C.r * level);
			this.g = (byte)X.IntR((float)this.g * num + (float)C.g * level);
			this.b = (byte)X.IntR((float)this.b * num + (float)C.b * level);
			return this;
		}

		public C32 blend(uint rgba, float level = 0.5f)
		{
			if (level <= 0f)
			{
				return this;
			}
			level = X.MMX(0f, level, 1f);
			float num = 1f - level;
			byte b = (byte)((rgba >> 16) & 255U);
			byte b2 = (byte)((rgba >> 8) & 255U);
			byte b3 = (byte)(rgba & 255U);
			byte b4 = (byte)((rgba >> 24) & 255U);
			this.r = (byte)X.IntR((float)this.r * num + (float)b * level);
			this.g = (byte)X.IntR((float)this.g * num + (float)b2 * level);
			this.b = (byte)X.IntR((float)this.b * num + (float)b3 * level);
			this.a = (byte)X.IntR((float)this.a * num + (float)b4 * level);
			return this;
		}

		public C32 blend(float _r, float _g, float _b, float _a, float level = 0.5f)
		{
			if (level <= 0f)
			{
				return this;
			}
			level = X.MMX(0f, level, 1f);
			float num = 1f - level;
			this.r = (byte)X.IntR((float)this.r * num + X.MMX(0f, _r, 255f) * level);
			this.g = (byte)X.IntR((float)this.g * num + X.MMX(0f, _g, 255f) * level);
			this.b = (byte)X.IntR((float)this.b * num + X.MMX(0f, _b, 255f) * level);
			this.a = (byte)X.IntR((float)this.a * num + X.MMX(0f, _a, 255f) * level);
			return this;
		}

		public C32 blend(C32 C, float level = 0.5f)
		{
			this.blend(C.C, level);
			return this;
		}

		public C32 blend3(uint c0, uint c1, uint c2, float level)
		{
			if (level > 1f)
			{
				this.Set(c2).blend(c0, X.ZLINE(level - 1f));
			}
			else if (level < 0.5f)
			{
				this.Set(c0).blend(c1, X.ZLINE(level * 2f));
			}
			else
			{
				this.Set(c1).blend(c2, X.ZLINE((level - 0.5f) * 2f));
			}
			return this;
		}

		public C32 blend3(Color32 c0, Color32 c1, Color32 c2, float level)
		{
			if (level > 1f)
			{
				this.Set(c2).blend(c0, X.ZLINE(level - 1f));
			}
			else if (level < 0.5f)
			{
				this.Set(c0).blend(c1, X.ZLINE(level * 2f));
			}
			else
			{
				this.Set(c1).blend(c2, X.ZLINE((level - 0.5f) * 2f));
			}
			return this;
		}

		public C32 blend3(uint c0, uint c1, uint c2, float level0, float level1, float level2)
		{
			float num = level0 + level1 + level2;
			if (num == 0f)
			{
				return this;
			}
			num = 1f / num;
			this.C.r = (byte)X.MMX(0f, (((c0 >> 16) & 255U) * level0 + ((c1 >> 16) & 255U) * level1 + ((c2 >> 16) & 255U) * level2) * num, 255f);
			this.C.g = (byte)X.MMX(0f, (((c0 >> 8) & 255U) * level0 + ((c1 >> 8) & 255U) * level1 + ((c2 >> 8) & 255U) * level2) * num, 255f);
			this.C.b = (byte)X.MMX(0f, ((c0 & 255U) * level0 + (c1 & 255U) * level1 + (c2 & 255U) * level2) * num, 255f);
			this.C.a = (byte)X.MMX(0f, (((c0 >> 24) & 255U) * level0 + ((c1 >> 24) & 255U) * level1 + ((c2 >> 24) & 255U) * level2) * num, 255f);
			return this;
		}

		public Color32 calcAddGray(Color32 C, bool alpha_multiply = true)
		{
			C.r = (byte)X.MMX(0, (int)(C.r + this.r - 127), 255);
			C.g = (byte)X.MMX(0, (int)(C.g + this.g - 127), 255);
			C.b = (byte)X.MMX(0, (int)(C.b + this.b - 127), 255);
			if (alpha_multiply)
			{
				C.a = (byte)X.MMX(0f, (float)(C.a * this.a) / 255f, 255f);
			}
			else
			{
				C.a = (byte)X.MMX(0, (int)(C.a + this.a - 127), 255);
			}
			return C;
		}

		public C32 HSV(float h, float s, float v)
		{
			while (h < 0f)
			{
				h += 360f;
			}
			while (h >= 360f)
			{
				h -= 360f;
			}
			s = X.MMX(0f, s, 100f) / 100f;
			v = X.MMX(0f, v, 100f) / 100f;
			float num = v;
			float num2 = v;
			float num3 = v;
			if (s > 0f)
			{
				h /= 60f;
				int num4 = (int)h;
				float num5 = h - (float)num4;
				switch (num4)
				{
				case 0:
					num2 *= 1f - s * (1f - num5);
					num3 *= 1f - s;
					break;
				case 1:
					num *= 1f - s * num5;
					num3 *= 1f - s;
					break;
				case 2:
					num *= 1f - s;
					num3 *= 1f - s * (1f - num5);
					break;
				case 3:
					num *= 1f - s;
					num2 *= 1f - s * num5;
					break;
				case 4:
					num *= 1f - s * (1f - num5);
					num2 *= 1f - s;
					break;
				case 5:
					num2 *= 1f - s;
					num3 *= 1f - s * num5;
					break;
				}
			}
			return this.Set(num * 255f, num2 * 255f, num3 * 255f, (float)this.a);
		}

		public Vector3 ToHSV()
		{
			float num = (float)this.r * 0.003921569f;
			float num2 = (float)this.g * 0.003921569f;
			float num3 = (float)this.b * 0.003921569f;
			float num4 = ((num > num2) ? num : num2);
			num4 = ((num4 > num3) ? num4 : num3);
			float num5 = ((num < num2) ? num : num2);
			num5 = ((num5 < num3) ? num5 : num3);
			float num6 = num4 - num5;
			if (num6 > 0f)
			{
				if (num4 == num)
				{
					num6 = (num2 - num3) / num6;
					num6 = ((num6 < 0f) ? (num6 + 6f) : num6);
				}
				else if (num4 == num2)
				{
					num6 = 2f + (num3 - num) / num6;
				}
				else
				{
					num6 = 4f + (num - num2) / num6;
				}
			}
			float num7 = (num4 - num5) / ((num4 > 0f) ? num4 : 1f);
			return new Vector3(num6 * 60f, num7, num4);
		}

		public C32 colorShift255(int imr, int img, int imb, int ima, float pr, float pg, float pb, float pa)
		{
			return this.Set2((float)((int)this.C.r * imr) / 255f + pr, (float)((int)this.C.g * img) / 255f + pg, (float)((int)this.C.b * imb) / 255f + pb, (float)((int)this.C.a * ima) / 255f + pa);
		}

		public C32 colorShift(float mr, float mg, float mb, float ma, float pr, float pg, float pb, float pa)
		{
			return this.Set2((float)this.C.r * mr + pr, (float)this.C.g * mg + pg, (float)this.C.b * mb + pb, (float)this.C.a * ma + pa);
		}

		public C32 multiply(float s, bool apply_to_alpha = true)
		{
			this.C.r = (byte)X.MMX(0f, (float)this.C.r * s, 255f);
			this.C.g = (byte)X.MMX(0f, (float)this.C.g * s, 255f);
			this.C.b = (byte)X.MMX(0f, (float)this.C.b * s, 255f);
			if (apply_to_alpha)
			{
				this.C.a = (byte)X.MMX(0f, (float)this.C.a * s, 255f);
			}
			return this;
		}

		public C32 multiply(float r, float g, float b, float a = 1f)
		{
			this.C.r = (byte)X.MMX(0f, (float)this.C.r * r, 255f);
			this.C.g = (byte)X.MMX(0f, (float)this.C.g * g, 255f);
			this.C.b = (byte)X.MMX(0f, (float)this.C.b * b, 255f);
			this.C.a = (byte)X.MMX(0f, (float)this.C.a * a, 255f);
			return this;
		}

		public C32 multiply255(float r, float g, float b, float a = 255f)
		{
			this.C.r = (byte)X.MMX(0f, (float)this.C.r * r * 0.003921569f, 255f);
			this.C.g = (byte)X.MMX(0f, (float)this.C.g * g * 0.003921569f, 255f);
			this.C.b = (byte)X.MMX(0f, (float)this.C.b * b * 0.003921569f, 255f);
			this.C.a = (byte)X.MMX(0f, (float)this.C.a * a * 0.003921569f, 255f);
			return this;
		}

		public C32 multiply(Color32 s, bool apply_to_alpha = true)
		{
			this.C.r = (byte)X.MMX(0f, (float)(this.C.r * s.r) * 0.003921569f, 255f);
			this.C.g = (byte)X.MMX(0f, (float)(this.C.g * s.g) * 0.003921569f, 255f);
			this.C.b = (byte)X.MMX(0f, (float)(this.C.b * s.b) * 0.003921569f, 255f);
			if (apply_to_alpha)
			{
				this.C.a = (byte)X.MMX(0f, (float)(this.C.a * s.a) * 0.003921569f, 255f);
			}
			return this;
		}

		public C32 multiply(Color32 s, float apply_level, bool apply_to_alpha = true)
		{
			apply_level = 1f - apply_level;
			this.C.r = (byte)X.MMX(0f, (float)this.C.r * X.Scr((float)s.r / 255f, apply_level), 255f);
			this.C.g = (byte)X.MMX(0f, (float)this.C.g * X.Scr((float)s.g / 255f, apply_level), 255f);
			this.C.b = (byte)X.MMX(0f, (float)this.C.b * X.Scr((float)s.b / 255f, apply_level), 255f);
			if (apply_to_alpha)
			{
				this.C.a = (byte)X.MMX(0f, (float)(this.C.a * s.a) / 255f, 255f);
			}
			return this;
		}

		public C32 add(Color32 s, bool apply_to_alpha = true, float apply_level = 1f)
		{
			this.C.r = (byte)X.MMX(0f, (float)this.C.r + (float)s.r * apply_level, 255f);
			this.C.g = (byte)X.MMX(0f, (float)this.C.g + (float)s.g * apply_level, 255f);
			this.C.b = (byte)X.MMX(0f, (float)this.C.b + (float)s.b * apply_level, 255f);
			if (apply_to_alpha)
			{
				this.C.a = (byte)X.MMX(0f, (float)this.C.a + (float)s.a * apply_level, 255f);
			}
			return this;
		}

		public C32 addG(Color32 s, bool apply_to_alpha = true)
		{
			this.C.r = (byte)X.MMX(0, (int)(this.C.r + s.r - 127), 255);
			this.C.g = (byte)X.MMX(0, (int)(this.C.g + s.g - 127), 255);
			this.C.b = (byte)X.MMX(0, (int)(this.C.b + s.b - 127), 255);
			if (apply_to_alpha)
			{
				this.C.a = (byte)X.MMX(0, (int)(this.C.a + s.a - 127), 255);
			}
			return this;
		}

		public C32 Scr(Color32 s, float alpha = 1f)
		{
			this.C.r = (byte)(255f * X.Scr((float)this.C.r / 255f, alpha * (float)s.r / 255f));
			this.C.g = (byte)(255f * X.Scr((float)this.C.g / 255f, alpha * (float)s.g / 255f));
			this.C.b = (byte)(255f * X.Scr((float)this.C.b / 255f, alpha * (float)s.b / 255f));
			return this;
		}

		public C32 Scr(C32 s, float alpha = 1f)
		{
			this.C.r = (byte)(255f * X.Scr((float)this.C.r / 255f, alpha * (float)s.r / 255f));
			this.C.g = (byte)(255f * X.Scr((float)this.C.g / 255f, alpha * (float)s.g / 255f));
			this.C.b = (byte)(255f * X.Scr((float)this.C.b / 255f, alpha * (float)s.b / 255f));
			return this;
		}

		public C32 Scr(float alpha)
		{
			this.C.r = (byte)(255f * X.Scr((float)this.C.r / 255f, alpha));
			this.C.g = (byte)(255f * X.Scr((float)this.C.g / 255f, alpha));
			this.C.b = (byte)(255f * X.Scr((float)this.C.b / 255f, alpha));
			return this;
		}

		public C32 ScrA(float alpha)
		{
			this.C.a = (byte)(255f * X.Scr((float)this.C.a * 0.003921569f, alpha));
			return this;
		}

		public override string ToString()
		{
			return "0x" + this.rgbax;
		}

		public static byte getRed(uint r)
		{
			return (byte)((r >> 16) & 255U);
		}

		public static byte getGreen(uint r)
		{
			return (byte)((r >> 8) & 255U);
		}

		public static byte getBlue(uint r)
		{
			return (byte)(r & 255U);
		}

		public static byte getAlpha(uint r)
		{
			return (byte)((r >> 24) & 255U);
		}

		public static Color32 codeToColor32(uint code)
		{
			return new Color32
			{
				r = C32.getRed(code),
				g = C32.getGreen(code),
				b = C32.getBlue(code),
				a = C32.getAlpha(code)
			};
		}

		public static Color32 d2c(uint code)
		{
			return C32.codeToColor32(code);
		}

		public static Color32 MulA(Color32 C, float alpha01)
		{
			C.a = (byte)((float)C.a * alpha01);
			return C;
		}

		public static Color32 RplA(Color32 C, byte b)
		{
			C.a = b;
			return C;
		}

		public static Color32 RplA(uint code, byte b)
		{
			Color32 color = C32.codeToColor32(code);
			color.a = b;
			return color;
		}

		public static Color32 MulA(uint code, float alpha01)
		{
			Color32 color = C32.codeToColor32(code);
			color.a = (byte)((float)color.a * alpha01);
			return color;
		}

		public static Color32 WMulA(float alpha01)
		{
			return C32.MulA(MTRX.ColWhite, alpha01);
		}

		public static uint c2d(Color32 C)
		{
			return C32.Color32ToCode(C);
		}

		public static uint Color32ToCode(Color32 C)
		{
			return (uint)(((int)C.a << 24) | ((int)C.r << 16) | ((int)C.g << 8) | (int)C.b);
		}

		public static string Color32ToCodeText(Color32 C)
		{
			return C32.codeToCodeText(C32.Color32ToCode(C));
		}

		public static string codeToCodeText(uint i)
		{
			return i.ToString("x");
		}

		public static uint colToCode(int cr, int cg, int cb)
		{
			return (X.MMX(0U, (uint)cr, 255U) << 16) | (X.MMX(0U, (uint)cg, 255U) << 8) | X.MMX(0U, (uint)cb, 255U);
		}

		public static Color32 GC(float cr, float cg, float cb, float ca)
		{
			return C32.d2c(C32.colToCode(cr, cg, cb, ca));
		}

		public static uint colToCode(int cr, int cg, int cb, int ca)
		{
			return (X.MMX(0U, (uint)ca, 255U) << 24) | (X.MMX(0U, (uint)cr, 255U) << 16) | (X.MMX(0U, (uint)cg, 255U) << 8) | X.MMX(0U, (uint)cb, 255U);
		}

		public static uint colToCode(float cr, float cg, float cb)
		{
			return (X.MMX(0U, (uint)cr, 255U) << 16) | (X.MMX(0U, (uint)cg, 255U) << 8) | X.MMX(0U, (uint)cb, 255U);
		}

		public static uint colToCode(float cr, float cg, float cb, float ca)
		{
			return (X.MMX(0U, (uint)ca, 255U) << 24) | (X.MMX(0U, (uint)cr, 255U) << 16) | (X.MMX(0U, (uint)cg, 255U) << 8) | X.MMX(0U, (uint)cb, 255U);
		}

		public static bool isDark(Color32 C)
		{
			return (float)(C.r + C.g + C.b) < 382.5f;
		}

		public static List<Color32> pushIdentical(List<Color32> A, Color32 C)
		{
			if (A != null)
			{
				for (int i = A.Count - 1; i >= 0; i--)
				{
					if (C32.isEqual(A[i], C))
					{
						return A;
					}
				}
				A.Add(C);
			}
			return A;
		}

		public static List<Color32> pushIdentical(List<Color32> A, List<Color32> ASrc)
		{
			if (A != null && ASrc != null)
			{
				int count = ASrc.Count;
				for (int i = 0; i < count; i++)
				{
					C32.pushIdentical(A, ASrc[i]);
				}
			}
			return A;
		}

		public Color32 C;
	}
}

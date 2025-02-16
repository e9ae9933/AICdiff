using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public sealed class BMListChars
	{
		public BMListChars(int _cw = 0, int _ch = 0, float ppu = 0f)
		{
			this.OImg = new BDic<char, BMListChars.ChrImage>();
			this.cw = (float)_cw;
			this.ch = (float)_ch;
			ppu = this.pixelsPerUnit;
		}

		public void add(PxlLayer Lay, char c)
		{
			this.OImg[c] = new BMListChars.ChrImageL(Lay);
			if (this.MI == null)
			{
				this.MI = MTRX.getMI(Lay.pChar);
			}
			if (this.cw == 0f)
			{
				this.cw = (float)Lay.Img.width;
			}
			if (this.ch == 0f)
			{
				this.ch = (float)Lay.Img.height;
			}
			if (this.pixelsPerUnit == 0f)
			{
				this.pixelsPerUnit = Lay.pChar.pixelsPerUnit;
			}
		}

		public void add(PxlFrame F, char c, float sx, float sy, int w, int h)
		{
			this.OImg[c] = new BMListChars.ChrImageF(F, sx, sy, w, h);
		}

		public void add(BMListChars.ChrImage s, char c)
		{
			this.OImg[c] = s;
		}

		public void add(BMListChars _List, string c = null)
		{
			if (this.MI == null)
			{
				this.MI = _List.MI;
			}
			if (this.cw == 0f)
			{
				this.cw = _List.cw;
			}
			if (this.ch == 0f)
			{
				this.ch = _List.ch;
			}
			if (this.pixelsPerUnit == 0f)
			{
				this.pixelsPerUnit = _List.pixelsPerUnit;
			}
			foreach (KeyValuePair<char, BMListChars.ChrImage> keyValuePair in _List.getImageDict())
			{
				if (c == null || c.IndexOf(keyValuePair.Key) >= 0)
				{
					this.add(keyValuePair.Value, keyValuePair.Key);
				}
			}
		}

		public void add(PxlSequence Sq, string chars, int margin = 0)
		{
			if (this.MI == null)
			{
				this.MI = MTRX.getMI(Sq.pChar);
			}
			if (this.cw == 0f)
			{
				this.cw = (float)((Sq.width == 0) ? Sq.pPose.width : Sq.width);
			}
			if (this.ch == 0f)
			{
				this.ch = (float)((Sq.height == 0) ? Sq.pPose.height : Sq.height);
			}
			if (this.pixelsPerUnit == 0f)
			{
				this.pixelsPerUnit = Sq.pChar.pixelsPerUnit;
			}
			float num = (float)(((Sq.width == 0) ? Sq.pPose.width : Sq.width) / 2);
			float num2 = (float)(((Sq.height == 0) ? Sq.pPose.height : Sq.height) / 2);
			char[] array = chars.ToCharArray();
			int num3 = array.Length;
			int num4 = 0;
			int num5 = Sq.countFrames();
			int num6 = 0;
			while (num6 < num5 && num4 < num3)
			{
				PxlFrame frame = Sq.getFrame(num6);
				float num7 = this.cw;
				float num8 = this.ch;
				PxlLayer layer = frame.getLayer(0);
				if (REG.match(layer.name, this.RegSizeLayer))
				{
					int num9 = X.NmI(REG.R1, -1, false, false);
					int num10 = X.NmI(REG.R2, -1, false, false);
					if (num9 > 0)
					{
						num7 = (float)num9;
					}
					else if (num9 == -1)
					{
						num7 = (float)layer.Img.width;
					}
					if (num10 > 0)
					{
						num8 = (float)num10;
					}
					else if (num10 == -1)
					{
						num8 = (float)layer.Img.height;
					}
				}
				this.add(frame, array[num4], num, num2, (int)num7, (int)num8);
				if (++num4 >= num3)
				{
					break;
				}
				num6++;
			}
			if (num4 < num3)
			{
				X.dl(Sq.ToString() + "の文字割当に必要なイメージが指定文字列よりも少ない (要求:" + num3.ToString() + ")", null, false, false);
			}
		}

		public void addResourceA(PxlLayer[] AImg, string c, int start_i = 0)
		{
			if (AImg != null)
			{
				char[] array = c.ToCharArray();
				int num = X.Mn(AImg.Length - start_i, array.Length);
				for (int i = 0; i < num; i++)
				{
					this.add(AImg[i + start_i], array[i]);
				}
			}
		}

		public BMListChars.ChrImage getCharMesh(char c)
		{
			return X.Get<char, BMListChars.ChrImage>(this.OImg, c);
		}

		public float DrawStringTo(MeshDrawer Md, STB Stb, float x = 0f, float y = 0f, ALIGN ali = ALIGN.LEFT, ALIGNY aliv = ALIGNY.TOP, bool use_color = false, float bounds_w = 0f, float bounds_h = 0f, fnDrawOneCharacterMd fnDraw = null)
		{
			return this.DrawScaleStringTo(Md, Stb, x, y, 1f, 1f, ali, aliv, use_color, bounds_w, bounds_h, fnDraw);
		}

		public float DrawBorderedScaleStringTo(MeshDrawer Md, STB Stb, float x = 0f, float y = 0f, float scalex = 1f, float scaley = 1f, ALIGN ali = ALIGN.LEFT, ALIGNY aliv = ALIGNY.TOP, bool use_color = false, float bounds_w = 0f, float bounds_h = 0f, fnDrawOneCharacterMd fnDraw = null)
		{
			Color32 col = Md.Col;
			Md.Col = Md.ColGrd.C;
			for (int i = 0; i < 8; i++)
			{
				this.DrawScaleStringTo(Md, Stb, x + (float)CAim._XD(i, 1) * scalex, y + (float)CAim._YD(i, 1) * scaley, scalex, scaley, ali, aliv, use_color, bounds_w, bounds_h, fnDraw);
			}
			Md.Col = col;
			return this.DrawScaleStringTo(Md, Stb, x, y, scalex, scaley, ali, aliv, use_color, bounds_w, bounds_h, fnDraw);
		}

		public float DrawScaleStringTo(MeshDrawer Md, STB Stb, float x = 0f, float y = 0f, float scalex = 1f, float scaley = 1f, ALIGN ali = ALIGN.LEFT, ALIGNY aliv = ALIGNY.TOP, bool use_color = false, float bounds_w = 0f, float bounds_h = 0f, fnDrawOneCharacterMd fnDraw = null)
		{
			if (aliv != ALIGNY.TOP)
			{
				float num2;
				if (bounds_h <= 0f)
				{
					int num = TX.countLine(Stb);
					num2 = ((float)num * this.ch + (float)((num - 1) * this.marginh)) * scaley;
				}
				else
				{
					num2 = bounds_h;
				}
				y += (float)((int)(num2 * ((aliv == ALIGNY.BOTTOM) ? 1f : 0.5f)));
			}
			int num3 = -1;
			int num4 = -1;
			int length = Stb.Length;
			float num5 = 0f;
			float num6 = x;
			float num7 = 0f;
			float num8 = 0f;
			bool flag = fnDraw != null;
			for (int i = 0; i <= length; i++)
			{
				if (num3 == -1)
				{
					num6 = (float)((int)x);
					num4++;
					if (ali != ALIGN.LEFT)
					{
						STB stb = TX.PopBld(null, 0);
						int num9 = Stb.IndexOf('\n', i, -1);
						stb.Add(Stb, i, (num9 < 0) ? (length - i) : (num9 - i));
						float num10 = ((bounds_w > 0f) ? bounds_w : this.DrawScaleStringTo(null, stb, 0f, 0f, scalex, scaley, ALIGN.LEFT, ALIGNY.TOP, use_color, bounds_w, bounds_h, null));
						TX.ReleaseBld(stb);
						num6 -= (float)((int)(num10 * ((ali == ALIGN.RIGHT) ? 1f : 0.5f)));
					}
				}
				num3++;
				char c = ((i == length) ? '\n' : Stb[i]);
				if (TX.isReturn(c))
				{
					num3 = -1;
					num5 = X.Mx(num5, num7 - (float)this.marginw);
					num7 = 0f;
					num8 -= (this.ch + (float)this.marginh) * scaley;
				}
				else if (c == ' ')
				{
					num7 += (float)((int)((this.cw + (float)this.marginw) * scalex));
				}
				else
				{
					if (use_color && c == 'C')
					{
						char c2 = Stb[i + 1];
						if (c2 == '{')
						{
							i++;
							int num11 = i;
							while (++i != length)
							{
								c2 = Stb[i];
								if (TX.isReturn(c2) || c2 == '}')
								{
									int num12;
									if (Md == null || Stb.Nm(num11, out num12, i, true) != STB.PARSERES.INT)
									{
										goto IL_038A;
									}
									Md.Col = C32.d2c((uint)STB.parse_result_int);
									if (i - (int)c2 == 6)
									{
										Md.Col.a = byte.MaxValue;
										goto IL_038A;
									}
									goto IL_038A;
								}
							}
							X.de("カラーコード取得 C{...} 時にEOFに到達", null);
							return num5;
						}
						int num13 = TX.charToHex(c2, -1);
						if (num13 >= 0)
						{
							if (Md != null)
							{
								Md.Col = C32.codeToColor32(MTRX.Acolors[num13]);
							}
							i++;
							goto IL_038A;
						}
					}
					BMListChars.ChrImage charMesh = this.getCharMesh(c);
					if (charMesh != null)
					{
						if (Md != null)
						{
							float num14 = y + num8;
							float num15 = 0.5f;
							if (aliv == ALIGNY.TOP)
							{
								num15 = 1f;
							}
							else if (aliv == ALIGNY.MIDDLE)
							{
								num14 -= this.ch / 2f * scaley;
							}
							else
							{
								num14 += -this.ch;
								num15 = 0f;
							}
							this.curx = num6 + num7;
							this.cury = num14;
							if (flag)
							{
								this.curscalex = scalex;
								this.curscaley = scaley;
								if (fnDraw(Md, this, c, i, num3, num4) != null)
								{
									charMesh.DrawTo(Md, this.curx, this.cury - num15 * this.curscaley * this.ch, this.curscalex, this.curscaley);
								}
							}
							else
							{
								charMesh.DrawTo(Md, this.curx, this.cury - num15 * scaley * this.ch, scalex, scaley);
							}
						}
						num7 += (float)((int)((X.NAIBUN_I(this.cw, charMesh.w, this.frexible) + (float)this.marginw) * scalex));
					}
				}
				IL_038A:;
			}
			if (use_color && Md != null)
			{
				Md.White();
			}
			return num5;
		}

		public BDic<char, BMListChars.ChrImage> getImageDict()
		{
			return this.OImg;
		}

		public float getLineDrawHeight(int line_count)
		{
			return (float)line_count * (this.ch + (float)this.marginh) - (float)this.marginh;
		}

		public float getFixedDrawWidth(int char_count)
		{
			return (float)char_count * (this.cw + (float)this.marginw) - (float)this.marginw;
		}

		public int marginw;

		public int marginh = 1;

		public float cw;

		public float ch;

		public float curx;

		public float cury;

		public float curscalex;

		public float curscaley;

		public float frexible = 1f;

		public float pixelsPerUnit;

		public MImage MI;

		private static MeshDrawer MdTemp;

		private Regex RegSizeLayer = new Regex("size_([s\\d]+)_([s\\d]+)");

		private BDic<char, BMListChars.ChrImage> OImg;

		public abstract class ChrImage
		{
			public abstract void DrawTo(MeshDrawer Md, float x, float y, float sclx, float scly);

			public float w;

			public float h;
		}

		public class ChrImageMesh : BMListChars.ChrImage
		{
			public ChrImageMesh(Sprite Spr)
			{
				this.w = Spr.textureRect.width;
				this.h = Spr.textureRect.height;
				if (BMListChars.MdTemp == null)
				{
					BMListChars.MdTemp = new MeshDrawer(null, 4, 6);
					BMListChars.MdTemp.draw_gl_only = true;
					BMListChars.MdTemp.activate("temp", MTRX.MtrMeshNormal, false, MTRX.ColWhite, null);
				}
				BMListChars.MdTemp.clearSimple();
				BMListChars.MdTemp.initForImg(Spr);
				BMListChars.MdTemp.DrawGraph(0f, 0f, null);
				this.AVertice = BMListChars.MdTemp.getVertexArray();
				this.AMeshUv = BMListChars.MdTemp.getUvArray();
				this.Acolor = BMListChars.MdTemp.getColorArray();
				this.Atriangle = BMListChars.MdTemp.getTriangleArray();
				this.ver_i = BMListChars.MdTemp.getVertexMax();
				this.tri_i = BMListChars.MdTemp.getTriMax();
			}

			public override void DrawTo(MeshDrawer Md, float x, float y, float sclx, float scly)
			{
				Md.RotaMesh(x, y, sclx, scly, 0f, this.AVertice, this.Acolor, this.AMeshUv, null, null, this.Atriangle, false, 0, this.tri_i, 0, this.ver_i);
			}

			public Vector3[] AVertice;

			public Vector2[] AMeshUv;

			public Color32[] Acolor;

			public int[] Atriangle;

			public int ver_i;

			public int tri_i;
		}

		public class ChrImageL : BMListChars.ChrImage
		{
			public ChrImageL(PxlLayer Lay)
			{
				this.w = (float)Lay.Img.width;
				this.h = (float)Lay.Img.height;
				this.Lay = Lay;
			}

			public override void DrawTo(MeshDrawer Md, float x, float y, float sclx, float scly)
			{
				Md.initForImg(this.Lay.Img, 0);
				Md.RotaGraph3(x, y, 0.5f, 0.5f, sclx, scly, 0f, null, false);
			}

			private PxlLayer Lay;
		}

		public class ChrImageF : BMListChars.ChrImage
		{
			public ChrImageF(PxlFrame PF, float x, float y, int _w, int _h)
			{
				this.sx = x;
				this.sy = y;
				this.w = (float)_w;
				this.h = (float)_h;
				this.PF = PF;
			}

			public override void DrawTo(MeshDrawer Md, float x, float y, float sclx, float scly)
			{
				Md.RotaPF(x + this.sx * sclx, y + this.sy * scly, sclx, scly, 0f, this.PF, false, false, false, uint.MaxValue, false, 0);
			}

			private PxlFrame PF;

			private float sx;

			private float sy;
		}
	}
}

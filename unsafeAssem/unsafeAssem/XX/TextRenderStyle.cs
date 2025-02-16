using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class TextRenderStyle
	{
		public bool bold
		{
			get
			{
				return (this.flags & 1) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 1) : (this.flags & -2));
			}
		}

		public bool italic
		{
			get
			{
				return (this.flags & 2) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 2) : (this.flags & -3));
			}
		}

		public bool monospace
		{
			get
			{
				return (this.flags & 4) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 4) : (this.flags & -5));
			}
		}

		public bool fix_line_spacing
		{
			get
			{
				return (this.flags & 8) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 8) : (this.flags & -9));
			}
		}

		public bool consider_leftshift
		{
			get
			{
				return (this.flags & 16) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 16) : (this.flags & -17));
			}
		}

		public bool confusion
		{
			get
			{
				return (this.flags & 32) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 32) : (this.flags & -33));
			}
		}

		public bool isApplying()
		{
			return this.TargetRenderer != null;
		}

		public TextRenderer getTargetRenderer()
		{
			return this.TargetRenderer;
		}

		public FontStorage Storage
		{
			get
			{
				return this.TargetStorage;
			}
		}

		public TextRenderStyle CopyFrom(TextRenderStyle Src, bool secure_changed_flag = false)
		{
			this.size = Src.size;
			this.alpha = Src.alpha;
			this.alignx = Src.alignx;
			this.aligny = Src.aligny;
			this.TargetStorage = Src.TargetStorage;
			this.image_aligny = Src.image_aligny;
			this.lineSpacing = Src.lineSpacing;
			this.letterSpacing = Src.letterSpacing;
			this.tab_spacing_chars = Src.tab_spacing_chars;
			this.Aword_splitter = Src.Aword_splitter;
			this.BorderCol = Src.BorderCol;
			this.MyCol = Src.MyCol;
			this.flags = Src.flags;
			this.head_x_shift = Src.head_x_shift;
			this.consider_leftshift = TX.getCurrentFamily().consider_leftshift;
			return this;
		}

		public TextRenderStyle ApplyTo(TextRenderer _TxDefault = null, TextRenderStyle _FirstStyle = null)
		{
			if (_TxDefault != null)
			{
				this.TargetRenderer = _TxDefault;
				this.TargetStorage = (_FirstStyle.TargetStorage = this.TargetRenderer.getStorage());
				this.TargetMd = this.TargetRenderer.getDrawer();
				this.FirstStyle = _FirstStyle;
				this.keyassign_used = -1;
				this.consider_leftshift = TX.getCurrentFamily().consider_leftshift;
			}
			if (this.TargetMd != null)
			{
				this.TargetMd.Col = this.MyCol;
				this.TargetMd.Col.a = (byte)((float)this.MyCol.a * this.alpha);
			}
			return this;
		}

		public bool isWordDelimiter(char chr)
		{
			if (this.Aword_splitter != null)
			{
				return X.isinS<char>(this.Aword_splitter, chr) >= 0;
			}
			return chr == '\b' || chr == ' ' || chr == '\u3000' || chr == '\t' || chr == '／' || chr == '/' || chr == '¥' || chr == '。' || chr == '、';
		}

		public TextRenderStyle quitRender()
		{
			if (this.FirstStyle != this)
			{
				this.FirstStyle.keyassign_used = this.keyassign_used;
			}
			this.TargetMd = null;
			this.TargetRenderer = null;
			this.FirstStyle = null;
			return this;
		}

		public TextRenderStyle reconsider(STB StbSrc, TextRendererTagMemory TMem, ref bool lock_auto_wrap)
		{
			if (this.FirstStyle == null)
			{
				return this;
			}
			this.CopyFrom(this.FirstStyle, true);
			lock_auto_wrap = false;
			if (this.TargetRenderer != null)
			{
				this.TargetRenderer.ReconsiderSpecialTag(TMem, null, null);
			}
			int tagCount = TMem.TagCount;
			for (int i = 0; i < tagCount; i++)
			{
				TextRendererHtmlTag tag = TMem.getTag(i);
				if (tag.TagNameIs("b"))
				{
					this.bold = true;
				}
				else if (tag.TagNameIs("i"))
				{
					this.italic = true;
				}
				else if (tag.TagNameIs("rb"))
				{
					lock_auto_wrap = true;
				}
				else
				{
					if (tag.TagNameIs("font"))
					{
						using (STB stb = TX.PopBld(null, 0))
						{
							if (tag.getVarContent("size", stb.Clear()))
							{
								int num;
								int num3;
								if ((num = stb.IndexOf("%", 0, -1)) >= 0)
								{
									int num2;
									if (stb.Nm(0, out num2, num, false) != STB.PARSERES.ERROR)
									{
										this.size = (float)(STB.parse_result_double / 100.0 * (double)this.size);
									}
								}
								else if (stb.Nm(0, out num3, -1, false) != STB.PARSERES.ERROR)
								{
									this.size = (float)STB.parse_result_double;
								}
							}
							int num4;
							if (tag.getVarContent("color", stb.Clear()) && stb.Nm(0, out num4, -1, false) == STB.PARSERES.INT)
							{
								this.MyCol = C32.d2c((uint)STB.parse_result_int);
								if (this.TargetRenderer != null)
								{
									this.TargetRenderer.color_arranged = true;
								}
							}
							if (tag.getVarContent("alpha", stb.Clear()))
							{
								int num5;
								if (stb.Nm(0, out num5, -1, false) != STB.PARSERES.ERROR)
								{
									this.alpha = this.FirstStyle.alpha * (float)STB.parse_result_double;
									if (this.TargetRenderer != null)
									{
										this.TargetRenderer.color_arranged = (this.TargetRenderer.alpha_arranged = true);
									}
								}
								else
								{
									this.alpha = this.FirstStyle.alpha;
								}
							}
							if (tag.getVarContent("letterspacing", stb.Clear()))
							{
								int num6;
								if (stb.Nm(0, out num6, -1, false) != STB.PARSERES.ERROR)
								{
									this.letterSpacing = this.FirstStyle.letterSpacing * (float)STB.parse_result_double;
								}
								else
								{
									this.letterSpacing = this.FirstStyle.letterSpacing;
								}
							}
							if (tag.getVarContent("linespacing", stb.Clear()))
							{
								int num7;
								if (stb.Nm(0, out num7, -1, false) != STB.PARSERES.ERROR)
								{
									this.lineSpacing = this.FirstStyle.lineSpacing * (float)STB.parse_result_double;
								}
								else
								{
									this.lineSpacing = this.FirstStyle.lineSpacing;
								}
							}
							goto IL_0304;
						}
					}
					if (tag.TagNameIs("align"))
					{
						if (tag.getVarContent("left", null))
						{
							this.alignx = ALIGN.LEFT;
						}
						else if (tag.getVarContent("center", null))
						{
							this.alignx = ALIGN.CENTER;
						}
						else if (tag.getVarContent("right", null))
						{
							this.alignx = ALIGN.RIGHT;
						}
						else
						{
							this.alignx = this.FirstStyle.alignx;
						}
					}
					else if (this.TargetRenderer != null)
					{
						this.TargetRenderer.ReconsiderSpecialTag(TMem, this, tag);
					}
				}
				IL_0304:;
			}
			this.ApplyTo(null, null);
			if (this.TargetRenderer != null)
			{
				this.TargetRenderer.ReconsiderSpecialTag(TMem, this, null);
			}
			return this;
		}

		public float createImage(TextRendererTagMemory TMem, TextRendererHtmlTag Tg, ref float drawx, ref float margin)
		{
			float num = 0f;
			float num2 = 1f;
			float num3 = 1f;
			int num4 = -1;
			MeshDrawer meshDrawer = null;
			float num42;
			using (STB stb = TX.PopBld(null, 0))
			{
				if (Tg.TagNameIs("img") || Tg.TagNameIs("fiximg"))
				{
					bool flag = Tg.TagNameIs("fiximg");
					PxlFrame pxlFrame = null;
					PxlImage pxlImage = null;
					if (Tg.getVarContent("mesh", stb.Clear()))
					{
						pxlFrame = MTRX.getPF(stb.ToString());
					}
					else if (Tg.getVarContent("keyconfig", stb.Clear()))
					{
						pxlImage = MTRX.SqFImgKCIcon.getLayer(stb.NmI(0, -1, 0)).Img;
					}
					if (pxlFrame != null || pxlImage != null)
					{
						if (pxlFrame != null)
						{
							num = pxlFrame.width;
						}
						else
						{
							num = (float)pxlImage.width;
						}
						float num5 = ((pxlFrame != null) ? pxlFrame.height : ((float)pxlImage.height));
						if (Tg.getVarContent("width", stb.Clear()) && stb.Nm())
						{
							num5 = (num = (float)STB.parse_result_double);
						}
						if (Tg.getVarContent("height", stb.Clear()) && stb.Nm())
						{
							num5 = (float)STB.parse_result_double;
						}
						float num6 = (this.letterSpacing - 1f) * this.size * this.Storage.xratio;
						if (Tg.getVarContent("margin", stb.Clear()) && stb.Nm())
						{
							num6 = (float)STB.parse_result_double;
						}
						if (Tg.getVarContent("scale", stb.Clear()) && stb.Nm())
						{
							num2 = (float)STB.parse_result_double;
						}
						if (Tg.getVarContent("alpha", stb.Clear()) && stb.Nm())
						{
							num3 = (float)STB.parse_result_double;
						}
						float num7 = -this.size * this.Storage.yshift_to_baseline * 0.5f + (float)this.image_aligny * ((num5 * num2 - this.Storage.base_height * this.size / this.Storage.def_size) * 0.5f);
						float num8;
						if (flag)
						{
							if (Tg.getVarContent("x", stb.Clear()) && stb.Nm())
							{
								num8 = ((this.alignx == ALIGN.LEFT) ? 0f : ((this.alignx == ALIGN.CENTER) ? 0.5f : 1f)) * this.Storage.getLineSWidth(drawx, margin, this.FirstStyle.size) + (float)STB.parse_result_double;
							}
							else
							{
								num8 = drawx;
							}
						}
						else
						{
							num8 = drawx + num * 0.5f * num2;
							if (Tg.getVarContent("x", stb.Clear()) && stb.Nm())
							{
								num8 += (float)STB.parse_result_double;
							}
						}
						if (Tg.getVarContent("y", stb.Clear()) && stb.Nm())
						{
							num7 += (float)STB.parse_result_double;
						}
						meshDrawer = this.TargetRenderer.initMeshSub(Tg.HasVar("behind") ? TextRenderer.MESH_TYPE.ICO_B0 : TextRenderer.MESH_TYPE.ICO_B1);
						bool flag2 = false;
						num4 = meshDrawer.getVertexMax();
						if (Tg.getVarContent("color", stb.Clear()))
						{
							int num9;
							if (stb.Nm(0, out num9, -1, false) == STB.PARSERES.INT)
							{
								meshDrawer.Col = C32.d2c((uint)STB.parse_result_int);
								if (meshDrawer.Col.a < 255 || this.TargetRenderer.color_apply_to_image)
								{
									this.TargetRenderer.color_arranged = true;
								}
							}
						}
						else if (Tg.HasVar("tx_color"))
						{
							meshDrawer.Col = this.MyCol;
							flag2 = true;
						}
						else
						{
							meshDrawer.Col = (this.TargetRenderer.color_apply_to_image ? this.MyCol : MTRX.ColWhite);
						}
						if (num3 < 1f)
						{
							this.TargetRenderer.color_arranged = (this.TargetRenderer.alpha_arranged = true);
						}
						meshDrawer.Col.a = (byte)((float)meshDrawer.Col.a * num3 * this.alpha);
						bool flag3 = X.Nm("flip", 0f, false) != 0f;
						if (pxlFrame != null)
						{
							meshDrawer.RotaPF(num8, num7, num2, num2, 0f, pxlFrame, flag3, false, false, uint.MaxValue, false, 0);
						}
						else if (pxlImage != null)
						{
							meshDrawer.initForImg(pxlImage, 0).RotaGraph(num8, num7, num2, 0f, null, flag3);
						}
						if (flag2)
						{
							this.TargetRenderer.AddTxColorPosition(meshDrawer, num4);
						}
						if (!flag)
						{
							drawx += num * num2 + num6;
							margin = num6;
						}
					}
				}
				else if (Tg.TagNameIs("bmc") || Tg.TagNameIs("bmcs"))
				{
					BMListChars bmlistChars = (Tg.TagNameIs("bmc") ? MTRX.ChrL : MTRX.ChrM);
					num2 = 1f;
					bool flag2 = false;
					if (Tg.getVarContent("scale", stb.Clear()) && stb.Nm())
					{
						num2 = (float)STB.parse_result_double;
					}
					float num7 = this.size * this.Storage.yshift_to_baseline;
					if (Tg.getVarContent("y", stb.Clear()) && stb.Nm())
					{
						num7 += (float)STB.parse_result_double;
					}
					if (Tg.HasVar("tx_color"))
					{
						meshDrawer.Col = this.MyCol;
						flag2 = true;
					}
					if (Tg.getVarContent("c", stb.Clear()))
					{
						meshDrawer = this.TargetRenderer.initMeshSub(Tg.HasVar("behind") ? TextRenderer.MESH_TYPE.ICO_B0 : TextRenderer.MESH_TYPE.ICO_B1);
						num4 = meshDrawer.getVertexMax();
						bmlistChars.DrawScaleStringTo(meshDrawer, stb, drawx, num7, num2, num2, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
						if (flag2)
						{
							this.TargetRenderer.AddTxColorPosition(meshDrawer, num4);
						}
					}
				}
				else if (Tg.TagNameIs("shape"))
				{
					num = this.size * 0.5f;
					float num8 = drawx;
					float num7 = -this.size * this.Storage.yshift_to_baseline * 0.5f;
					float num6 = (this.letterSpacing - 1f) * this.size * this.Storage.xratio;
					if (Tg.getVarContent("margin", stb.Clear()) && stb.Nm())
					{
						num6 = (float)STB.parse_result_double;
					}
					bool flag2 = Tg.HasVar("tx_color");
					if (Tg.HasVar("right_arrow"))
					{
						meshDrawer = this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.MESH_B1);
						num4 = meshDrawer.getVertexMax();
						if (flag2)
						{
							meshDrawer.Col = this.MyCol;
						}
						meshDrawer.Poly(num8 + num * num2, num7, num * 0.8f, 0f, 3, 0f, false, 0f, 0f);
					}
					else if (Tg.HasVar("dashline"))
					{
						num6 = num * 0.4f;
						num *= 2.25f;
						meshDrawer = this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.MESH_B1);
						num4 = meshDrawer.getVertexMax();
						if (flag2)
						{
							meshDrawer.Col = this.MyCol;
						}
						meshDrawer.Rect(num8 + num, num7, num * 2f * 0.96f, 1.5f, false);
					}
					else if (Tg.HasVar("check"))
					{
						meshDrawer = this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.MESH_B1);
						num4 = meshDrawer.getVertexMax();
						if (flag2)
						{
							meshDrawer.Col = this.MyCol;
						}
						meshDrawer.CheckMark(num8 + num, num7 + this.size * 0.2f, num * 1.8f, num * 0.4f, false);
					}
					else if (Tg.HasVar("lock"))
					{
						meshDrawer = this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.MESH_B1);
						num4 = meshDrawer.getVertexMax();
						if (flag2)
						{
							meshDrawer.Col = this.MyCol;
						}
						MTRX.DrawMeshIcon(meshDrawer, num8 + num * 1.8f, num7, num * 1.8f, "lock", 0f);
						drawx += num * 0.7f;
					}
					else if (Tg.HasVar("borderrect"))
					{
						meshDrawer = this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.MESH_B1);
						num4 = meshDrawer.getVertexMax();
						Color32 color = (flag2 ? this.MyCol : meshDrawer.Col);
						int num10;
						if (!flag2 && Tg.getVarContent("color", stb.Clear()) && stb.Nm(0, out num10, -1, false) == STB.PARSERES.INT)
						{
							color = C32.d2c((uint)STB.parse_result_int);
						}
						float num11 = num * 1.66f;
						float num12 = num8 + num11 * 0.5f;
						meshDrawer.Col = (C32.isDark(color) ? MTRX.ColWhite : MTRX.ColBlack);
						meshDrawer.Box(num12, num7, num11 + 2f, num11 + 2f, 1f, false);
						meshDrawer.Col = color;
						meshDrawer.Rect(num12, num7, num11, num11, false);
						if (color.a < 255)
						{
							meshDrawer.Col.a = byte.MaxValue;
							meshDrawer.Rect(num12 + num11 * 0.5f, num7, num11 * 0.5f, num11, false);
						}
					}
					else if (!Tg.HasVar("none"))
					{
						num = 0f;
					}
					if (num4 >= 0 && flag2)
					{
						this.TargetRenderer.AddTxColorPosition(meshDrawer, num4);
					}
					if (num != 0f)
					{
						drawx += 2f * num * num2 + num6;
						margin = num6;
					}
				}
				else
				{
					if (Tg.TagNameIs("key") || Tg.TagNameIs("key_s"))
					{
						this.keyassign_used = IN.getKeyAssignResetId();
						int inputCount = IN.getInputCount();
						PxlImage pxlImage2 = null;
						int num13 = 0;
						int num14 = 0;
						int num15 = 0;
						PxlImage pxlImage3 = null;
						num2 = this.size / 50f;
						using (STB stb2 = TX.PopBld(null, 0))
						{
							float num16 = drawx;
							float num17 = 0f;
							bool flag4 = true;
							bool flag5 = false;
							float num18 = 19.4f * num2;
							int num19 = -1;
							if (KEY.keydesc_appearance == 2 || Tg.HasVar("pad"))
							{
								num19 = 1;
							}
							else if (KEY.keydesc_appearance == 1 || Tg.HasVar("kb"))
							{
								num19 = 0;
							}
							int i = 0;
							while (i <= inputCount)
							{
								float num20 = 0f;
								bool flag6;
								if (i >= inputCount)
								{
									flag6 = true;
									goto IL_0A4D;
								}
								string inputName = IN.getInputName(i);
								if (Tg.HasVar(inputName))
								{
									pxlImage3 = IN.getKeyAssignIcon(inputName, ref num20, ref num17, num19);
									flag6 = pxlImage3 != null && pxlImage3 != pxlImage2;
									goto IL_0A4D;
								}
								IL_13C7:
								i++;
								continue;
								IL_0A4D:
								if (flag6 && stb2.Length > 0)
								{
									float num21 = 0f;
									float num22 = X.ZSIN(this.alpha);
									bool flag7 = stb2.Equals("←");
									bool flag8 = stb2.Equals("↑");
									bool flag9 = stb2.Equals("→");
									bool flag10 = stb2.Equals("↓");
									float num23 = ((!flag5) ? (drawx + num18) : ((flag4 ? (drawx + this.size * 0.32f) : (drawx + 68f * num2 * 0.5f)) + num18));
									if (flag7 || flag8 || flag9 || flag10)
									{
										num23 += this.size * 0.32f;
										MeshDrawer meshDrawer2 = this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.MESH_T0);
										float num24 = -this.size * this.Storage.yshift_to_baseline * 0.5f;
										float num25 = CAim.get_agR(flag7 ? AIM.L : (flag8 ? AIM.T : (flag9 ? AIM.R : AIM.B)), 0f);
										float num26 = X.Mx(2.5f, this.size * 0.125f);
										float num27 = this.size * 0.38f;
										meshDrawer2.Col = C32.MulA(MTRX.ColBlack, this.alpha);
										meshDrawer2.Arrow(num23, num24, num27 + num26 * 1.5f, num25, num26 * 3.5f, false);
										meshDrawer2.Col = C32.MulA(MTRX.ColWhite, this.alpha);
										meshDrawer2.Arrow(num23, num24, num27, num25, num26, false);
										num21 = this.size * 1.125f;
									}
									else if (stb2.Equals("◯") || stb2.Equals("☓") || stb2.Equals("□") || stb2.Equals("△") || stb2.Equals("\uff3f"))
									{
										num23 += this.size * 0.32f;
										MeshDrawer meshDrawer3 = this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.MESH_T0);
										float num28 = -this.size * this.Storage.yshift_to_baseline * 0.5f;
										float num29 = X.Mx(2.5f, this.size * 0.125f);
										float num30 = this.size * 0.42f;
										meshDrawer3.Col = C32.MulA(MTRX.ColBlack, this.alpha);
										if (stb2.Equals("\uff3f"))
										{
											num30 *= 0.8f;
											float num31 = num30 * 0.25f;
											meshDrawer3.Boko(num23, num28 - num30 * 0.66f, num30 * 2.11f + num31, num30 * 0.7f + num31 * 0.5f, num29 * 0.8f + num31, false);
											meshDrawer3.Col = C32.MulA(MTRX.ColWhite, this.alpha);
											meshDrawer3.Boko(num23, num28 - num30 * 0.66f, num30 * 2.11f, num30 * 0.7f, num29 * 0.8f, false);
										}
										else if (stb2.Equals("◯") || stb2.Equals("□") || stb2.Equals("△"))
										{
											if (stb2.Equals("◯"))
											{
												num30 *= 0.88f;
											}
											int num32 = (stb2.Equals("◯") ? 18 : (stb2.Equals("□") ? 4 : 3));
											float num33 = (stb2.Equals("△") ? 1.5707964f : (-0.7853982f));
											meshDrawer3.Poly(num23, num28, num30 + num29 * 1f, num33, num32, 0f, false, 0f, 0f);
											meshDrawer3.Col = C32.MulA(MTRX.ColWhite, this.alpha);
											meshDrawer3.Poly(num23, num28, num30, num33, num32, num29 * 1.5f, false, 0f, 0f);
										}
										else
										{
											float num34 = num30 + num29 * 0.5f;
											meshDrawer3.Line(num23 - num34, num28 - num34, num23 + num34, num28 + num34, num29 * 2.1f, false, 0f, 0f).Line(num23 + num34, num28 - num34, num23 - num34, num28 + num34, num29 * 2.1f, false, 0f, 0f);
											meshDrawer3.Col = C32.MulA(MTRX.ColWhite, this.alpha);
											meshDrawer3.Line(num23 - num30, num28 - num30, num23 + num30, num28 + num30, num29, false, 0f, 0f).Line(num23 + num30, num28 - num30, num23 - num30, num28 + num30, num29, false, 0f, 0f);
										}
									}
									else
									{
										FontStorage fontStorage = MTRX.OFontStorage[MTRX.getCabinFont()];
										if (TextRenderStyle.StyleTxCabin == null)
										{
											TextRenderStyle.StyleTxCabin = new TextRenderStyle();
											TextRenderStyle.StyleTxCabin.alpha = 1f;
											TextRenderStyle.StyleTxCabin.TargetStorage = fontStorage;
											TextRenderStyle.StyleTxCabin.aligny = ALIGNY.MIDDLE;
											TextRenderStyle.StyleTxCabin.letterSpacing = 0.96f;
										}
										TextRenderStyle.StyleTxCabin.size = this.size * 0.92f;
										MeshDrawer meshDrawer4 = this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.TX_ANOTHER);
										TextRenderStyle.StyleTxCabin.MyCol = C32.MulA(TextRenderer.keyassign_text_col, num22);
										meshDrawer4.Col = TextRenderStyle.StyleTxCabin.MyCol;
										TextRenderStyle.StyleTxCabin.BorderCol = C32.MulA(TextRenderer.keyassign_text_border_col, 1f);
										if (this.TargetRenderer == null)
										{
											this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.TX);
											X.dl("タグパース中にテクスチャ再描画が発生", null, false, false);
											return -1000f;
										}
										num21 = TextRenderer.DrawStringTo(meshDrawer4, num23, -this.size * fontStorage.yshift_to_baseline * 0.45f, stb2, TextRenderStyle.StyleTxCabin, TextRenderStyle.StyleTxCabin, fontStorage, null, null, false, (float)(70 + 256 * num15) * num2, COMPRESS_STYPE.WHOLE_SIZE);
										if (num21 == -1000f)
										{
											this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.TX);
											X.dl("タグパース中にテクスチャ再描画が発生", null, false, false);
											return -1000f;
										}
									}
									if (Tg.TagNameIs("key_s"))
									{
										MeshDrawer meshDrawer5 = this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.MESH_T0);
										meshDrawer5.Col = C32.MulA(MTRX.ColWhite, this.alpha);
										float num35 = -this.size * this.Storage.yshift_to_baseline * 0.5f;
										float num36 = num21 / 2f + 6f;
										float num37 = this.size * 0.5f + 6f;
										meshDrawer5.Line(drawx + num18 + num36 + 6f, num35 + num37, drawx + num18 - num36, num35 - num37, 4f, false, 0f, 0f);
									}
									num21 += (flag5 ? (this.size * 0.66f) : 0f);
									num16 = X.Mx(num16, drawx + num21);
									flag5 = false;
									drawx += num21;
									margin = X.Mx(0f, this.letterSpacing * this.size * 0.3f) + num17 * num2;
									if (drawx < num16 + margin)
									{
										margin = num17 * num2;
										drawx = num16 + margin;
									}
									num16 = drawx;
									stb2.Clear();
									num15 = 0;
									if (this.TargetRenderer.color_apply_to_image)
									{
										this.TargetRenderer.color_arranged = true;
									}
									if (i == inputCount)
									{
										break;
									}
								}
								if (pxlImage3 != null && pxlImage3 != pxlImage2)
								{
									if (num14 > 0)
									{
										drawx -= num17 * num2 * 0.25f;
									}
									num = 68f;
									float num38 = drawx + num * 0.5f * num2 + num18;
									float num39 = -this.size * this.Storage.yshift_to_baseline * 0.5f;
									Color32 color2 = C32.MulA(TextRenderer.keyassign_text_border_col, this.alpha * TextRenderer.keyassign_image_alpha);
									if (IN.canDrawIconByShape(IN.getInputName(i), num19))
									{
										meshDrawer = this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.MESH_B2);
										float num40 = num2 * num * 0.95f;
										meshDrawer.Col = color2;
										meshDrawer.ColGrd.White().mulA(this.alpha * TextRenderer.keyassign_image_alpha);
										KEY.DrawKeyBackIconTo(meshDrawer, num38, num39, num40, IN.getInputName(i), num19);
										num16 = num38 + num * num2 / 2f;
										flag4 = true;
									}
									else
									{
										MeshDrawer meshDrawer6 = this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.ICO_BORDER);
										meshDrawer6.Uv23(MTRX.ColWhite, true);
										meshDrawer6.Col = color2;
										float num41 = num2;
										if (num41 < 0.55f)
										{
											num41 /= 0.33f;
											num *= 0.33f;
											pxlImage3 = IN.getKeyAssignIconSmall(IN.getInputName(i), num19);
										}
										meshDrawer6.initForImg(pxlImage3, 1).RotaGraph(num38, num39, num41, 0f, null, false);
										num16 = num38 + num * num41 / 2f;
										flag4 = false;
										meshDrawer6.allocUv23(0, true);
									}
									flag5 = KEY.x_shifting_icon(IN.getKeyAssignIconId(IN.getInputName(i), num19));
									drawx += num20 * num2;
									num13++;
								}
								else if (num15 == 0)
								{
									num17 = this.letterSpacing * 0.4f;
									num16 += 40f * num2;
									drawx += 12f * num2;
								}
								num14++;
								if (i < inputCount)
								{
									string keyAssignLabel = IN.getKeyAssignLabel(IN.getInputName(i), true, num19);
									if (keyAssignLabel != "")
									{
										stb2.Append(keyAssignLabel, " ");
										num15++;
									}
								}
								pxlImage2 = pxlImage3;
								goto IL_13C7;
							}
							goto IL_1429;
						}
					}
					if (this.TargetRenderer == null || (!this.TargetRenderer.ParseSpecialTag(TMem, this, Tg, ref drawx, ref margin) && !this.TargetRenderer.do_not_error_unknown_tag))
					{
						X.de("Unknown tag:" + Tg.getTagName(), null);
					}
				}
				IL_1429:
				if (this.TargetRenderer != null)
				{
					this.TargetRenderer.initMeshSub(TextRenderer.MESH_TYPE.TX);
				}
				num42 = num + margin;
			}
			return num42;
		}

		private TextRenderStyle FirstStyle;

		public float size;

		public float alpha = 1f;

		public ALIGN alignx = ALIGN.LEFT;

		public ALIGNY aligny = ALIGNY.TOP;

		public ALIGNY image_aligny;

		public float lineSpacing = 1.5f;

		public float letterSpacing = 1.05f;

		public float tab_spacing_chars = 10f;

		public float head_x_shift;

		public Color32 MyCol = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public Color32 BorderCol = new Color32(0, 0, 0, 0);

		public char[] Aword_splitter;

		private int flags;

		private TextRenderer TargetRenderer;

		private FontStorage TargetStorage;

		private MeshDrawer TargetMd;

		public int keyassign_used = -1;

		private static TextRenderStyle StyleTxCabin;
	}
}

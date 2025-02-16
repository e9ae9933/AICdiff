using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class TextRenderer : MonoBehaviour, IFontStorageListener, IDesignerBlock, IValotileSetable
	{
		protected virtual void Awake()
		{
			if (MTRX.OFontStorage == null || this.Mtr != null)
			{
				return;
			}
			if (TextRenderer.BufStyle == null)
			{
				TextRenderer.BufStyle = new TextRenderStyle();
				TextRenderer.MaWord = new MdArranger(null);
			}
			if (this.TargetFont_ == null)
			{
				this.TargetFont_ = TX.getDefaultFont();
			}
			this.Stb = new STB(this.text_content_cache_);
			this.Storage = MTRX.OFontStorage[this.TargetFont_];
			this.Storage.Add(this);
			if (this.Style.size <= 0f)
			{
				this.Style.size = this.Storage.defaultRendererSize;
			}
			this.Mtr = this.Storage.MtrNoBorder;
			this.Md = MeshDrawer.prepareMeshRenderer(base.gameObject, this.Mtr, 0f, 3000, null, false, false);
			this.Mrd = base.GetComponent<MeshRenderer>();
			this.Mf = base.GetComponent<MeshFilter>();
			this.fineMaterial(false);
			if (this.text_content_cache_ != "")
			{
				this.Storage.fine_text = X.Mx(1, TextRenderer.STORAGE_REFINE_DELAY - 1);
			}
		}

		public TextRenderer CopyFrom(TextRenderer Tx, bool text_copying = false)
		{
			if (this.TargetFont_ != Tx.TargetFont_)
			{
				if (this.Storage != null)
				{
					this.Storage.Rem(this);
				}
				this.TargetFont_ = Tx.TargetFont_;
				this.Storage.Add(this);
			}
			this.Style.lineSpacing = Tx.Style.lineSpacing;
			this.Style.fix_line_spacing = Tx.Style.fix_line_spacing;
			this.Style.alpha = Tx.Style.alpha;
			this.Style.monospace = Tx.Style.monospace;
			this.Style.bold = Tx.Style.bold;
			this.Style.italic = Tx.Style.italic;
			this.BorderCol(Tx.BorderColor).Col(Tx.TextColor).LetterSpacing(Tx.letter_spacing)
				.Align(Tx.alignx)
				.AlignY(Tx.aligny)
				.ImgAlignY(Tx.Style.image_aligny)
				.Size(Tx.size);
			this.StencilRef(Tx.stencil_ref);
			if (text_copying)
			{
				this.Txt(Tx.Stb);
			}
			return this;
		}

		public TextRenderer Start()
		{
			if (MTRX.OFontStorage != null && this.Mtr == null)
			{
				this.Awake();
			}
			return this;
		}

		public void clearMesh()
		{
			if (this.Md != null)
			{
				this.Md.clear(false, false);
				if (this.ATxColPos != null)
				{
					this.ATxColPos.Clear();
				}
				if (this.Aline_ver_pos != null)
				{
					this.Aline_ver_pos.Clear();
				}
				this.redraw_flag = true;
			}
		}

		public virtual void OnDestroy()
		{
			try
			{
				this.Storage.Rem(this);
			}
			catch
			{
			}
			if (this.Md != null)
			{
				this.Md.destruct();
			}
			this.ATxColPos = null;
			this.Mtr = null;
		}

		protected TextRenderer fineMaterial(bool force = false)
		{
			if (force && this.Mtr == null)
			{
				this.Awake();
			}
			if (this.Mtr == null)
			{
				return this;
			}
			this.Mtr = this.Storage.getMaterial(C32.c2d(this.Style.BorderCol), this.stencil_ref_, false);
			this.initMeshSub(TextRenderer.MESH_TYPE.TX);
			this.Md.setMaterial(this.Mtr, false);
			if (this.mesh_using != 0)
			{
				int subMeshCount = this.Md.getSubMeshCount(true);
				for (int i = 0; i < subMeshCount; i++)
				{
					if (i != 7 && this.Md.getSubMeshMaterialActive(i))
					{
						this.Md.chooseSubMesh(i, false, false);
						this.Md.setMaterial(this.SubMeshMtr((TextRenderer.MESH_TYPE)i), false);
					}
				}
				this.Md.chooseSubMesh(7, false, false);
			}
			this.Mrd.sharedMaterials = this.Md.getMaterialArray(false);
			return this;
		}

		public virtual void Update()
		{
			if (this.Style.keyassign_used >= 0 && IN.getKeyAssignResetId() != this.Style.keyassign_used)
			{
				this.redraw_flag = true;
			}
			if (this.redraw_flag)
			{
				bool flag = true;
				if (this.BelongScroll != null)
				{
					flag = this.BelongScroll.isShowing(this, this.swidth + 50f, this.sheight + 50f, -(float)this.alignx * this.swidth * 0.5f, (float)this.aligny * this.swidth * 0.5f);
				}
				if (flag)
				{
					this.Redraw(true);
				}
			}
		}

		public TextRenderer Redraw(bool execute = false)
		{
			if (this.TargetFont_ == null)
			{
				this.Awake();
			}
			if (!execute)
			{
				this.redraw_flag = true;
				return this;
			}
			this.entryMesh();
			return this;
		}

		public TextRenderer MustRedraw()
		{
			while (this.redraw_flag)
			{
				this.Redraw(true);
			}
			return this;
		}

		public virtual void entryMesh()
		{
			this.swidth = 0f;
			this.sheight = 0f;
			if (this.Mtr == null)
			{
				this.fineMaterial(true);
			}
			if (this.Mtr == null)
			{
				return;
			}
			bool flag = this.mesh_using != 0;
			this.Md.getSubMeshCount(true);
			this.mesh_using = 0;
			this.redraw_flag = false;
			if (this.ATxColPos != null)
			{
				this.ATxColPos.Clear();
			}
			this.Md.clear(false, false);
			if (flag)
			{
				this.Md.chooseSubMesh(7, false, true);
				this.mesh_using = 128;
			}
			if (this.Aline_ver_pos != null)
			{
				this.Aline_ver_pos.Clear();
			}
			if (this.Amain_mesh_ver_pos != null)
			{
				this.Amain_mesh_ver_pos.Clear();
			}
			this.Md.setMaterial(this.Mtr, false);
			if (this.textIs(""))
			{
				return;
			}
			if (this.Ma == null)
			{
				this.Ma = new MdArranger(this.Md);
			}
			TextRenderer.BufStyle.CopyFrom(this.Style, false).ApplyTo(this, this.Style);
			this.color_arranged = false;
			if (TextRenderer.DrawStringTo(this.Md, 0f, 0f, this.Stb, this.Style, TextRenderer.BufStyle, this.Storage, this, this.Ma, this.html_mode, this.max_swidth_px, this.auto_condense_line ? COMPRESS_STYPE.LINE_COMPRESS : (this.auto_wrap ? COMPRESS_STYPE.WRAP : COMPRESS_STYPE.NONE)) == -1000f)
			{
				return;
			}
			if (this.auto_condense && this.swidth > this.max_swidth_px && this.max_swidth_px > 0f)
			{
				float num = this.max_swidth_px / this.swidth;
				this.Ma.scaleAll(num, num, 0f, 0f, true);
				this.swidth = this.max_swidth_px;
				this.sheight *= num;
			}
			this.Md.updateForMeshRenderer(true);
			TextRenderer.BufStyle.quitRender();
		}

		public static float DrawStringTo(MeshDrawer Md, float x0, float y0, STB Stb, TextRenderStyle Style, TextRenderStyle UseStyle, FontStorage Storage, TextRenderer Tx = null, MdArranger Ma = null, bool html_mode = false, float condense_swidth = -1f, COMPRESS_STYPE compress_type = COMPRESS_STYPE.WHOLE_SIZE)
		{
			bool flag = false;
			int num = 0;
			float num2 = 1f;
			if (Ma == null)
			{
				Ma = new MdArranger(Md);
			}
			Ma.Set(true);
			int vertexMax = Md.getVertexMax();
			int triMax = Md.getTriMax();
			int num3 = Md.getVertexMax();
			int num4 = Md.getTriMax();
			if (Tx != null)
			{
				if (Tx.can_create_big_size_texture)
				{
					num = Tx.storage_request_size;
					num2 = (float)num / Storage.def_size;
				}
				if (Tx.Aline_ver_pos != null)
				{
					Tx.Aline_ver_pos.Add(num3);
				}
			}
			if (num == 0)
			{
				num = (int)Storage.def_size;
			}
			float num5 = Style.size / (float)num;
			float num6 = Style.size * Style.letterSpacing;
			float num7 = -(Style.size * Storage.xratio) * Storage.xmargin;
			float num8 = num7 + Style.head_x_shift;
			float num9 = 0f;
			float num10 = 0f;
			float num11 = 0f;
			int num12 = 0;
			float num13 = -1000f;
			char c = '0';
			bool flag2 = false;
			float num14 = Style.size / (float)num;
			float num15 = num14;
			int length = Stb.Length;
			bool flag3 = UseStyle.BorderCol.a >= 1;
			bool flag4 = condense_swidth > 0f && compress_type == COMPRESS_STYPE.WRAP;
			if (flag4)
			{
				TextRenderer.MaWord.clear(Md).Set(false);
			}
			bool flag5 = Tx != null;
			byte b = 0;
			BList<int> blist = null;
			float num28;
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					bool flag6 = false;
					TextRendererTagMemory textRendererTagMemory = null;
					CharacterInfo characterInfo = default(CharacterInfo);
					int i = 0;
					while (i <= length)
					{
						bool flag7 = false;
						if (i < length)
						{
							c = Stb[i];
							flag7 = c == '\n' || c == '\r';
						}
						bool flag8 = html_mode && c == '<';
						if (!flag7 && flag4 && (!html_mode || textRendererTagMemory == null || textRendererTagMemory.CurTag == null) && (b == 0 || b == 3))
						{
							bool flag9 = true;
							if (i == length || (!flag6 && (UseStyle.isWordDelimiter(c) || flag8 || b == 3)))
							{
								flag9 = false;
								float lineSWidth;
								if (num13 > -1000f && !stb.Equals(stb2) && !stb2.Equals("") && (lineSWidth = Storage.getLineSWidth(num8, num11, Style.size)) > condense_swidth)
								{
									TextRenderer.MaWord.Set(false);
									Ma.SetLastVerAndTriIndex(TextRenderer.MaWord.getStartVerIndex(), TextRenderer.MaWord.getStartTriIndex());
									float num16 = lineSWidth - num13;
									num8 = num13;
									TextRenderer.MaWord.translateAll(-num13 + num7, 0f, false);
									num13 = num16;
									b = 1;
									flag7 = true;
								}
								else
								{
									num13 = -1000f;
									stb2.Set("");
									if (!UseStyle.isWordDelimiter(c))
									{
										flag9 = true;
									}
								}
							}
							if (flag9)
							{
								if (num13 == -1000f)
								{
									TextRenderer.MaWord.Set(true);
									num13 = num8;
								}
								stb2.Add(c);
								stb.Add(c);
							}
						}
						if (b == 3)
						{
							b = 0;
						}
						if (i == length || b == 2)
						{
							flag7 = true;
						}
						if (!flag7)
						{
							goto IL_0577;
						}
						if (b != 1)
						{
							Ma.Set(false);
						}
						if (Ma.vertexContains(blist, true))
						{
							float num17 = num14 * Storage.base_height * ((Tx == null) ? 0.3f : Tx.ruby_height_ratio(num9));
							num9 -= num17;
						}
						float num18 = Storage.getLineSWidth(num8, num11, Style.size);
						if (condense_swidth > 0f && num18 > condense_swidth && (compress_type == COMPRESS_STYPE.LINE_COMPRESS || compress_type == COMPRESS_STYPE.WRAP))
						{
							Ma.scaleAll(condense_swidth / num18, 1f, 0f, 0f, false);
							num18 = condense_swidth;
						}
						float num19 = ((UseStyle.alignx != ALIGN.LEFT) ? (((UseStyle.alignx == ALIGN.CENTER) ? (-0.5f) : (-1f)) * num18) : 0f);
						float num20 = num15 * (float)num * Storage.yshift_to_baseline;
						num10 = X.Mx(num10, num18);
						if (flag5)
						{
							if (Tx.Aline_ver_pos != null)
							{
								if (flag4 && b == 1)
								{
									Tx.Aline_ver_pos.Add(TextRenderer.MaWord.getStartVerIndex());
								}
								else
								{
									Tx.Aline_ver_pos.Add(Md.getVertexMax());
								}
							}
							if (UseStyle.alignx != Tx.alignx)
							{
								num19 += X.Mx(num10, Tx.max_swidth_px) * (float)(UseStyle.alignx - Tx.alignx) * 0.5f;
							}
						}
						num19 *= 0.015625f;
						num20 = (num20 + num9) * 0.015625f;
						Ma.translateAll(num19, num20, true);
						num8 = num7;
						num12++;
						num9 -= (UseStyle.fix_line_spacing ? UseStyle.lineSpacing : (num15 * num2 * Storage.base_height * UseStyle.lineSpacing));
						num15 = num14;
						bool flag10 = true;
						stb.Set("");
						if (flag4)
						{
							if (b == 1)
							{
								Ma.Set(TextRenderer.MaWord.getStartVerIndex(), TextRenderer.MaWord.getStartTriIndex());
								num8 = num13;
								stb.Set(stb2);
								flag10 = false;
								if (i >= length)
								{
									b = 2;
									i--;
									goto IL_09DB;
								}
							}
							else
							{
								Ma.Set(true);
							}
							stb2.Set("");
							b = 0;
							num13 = num8;
							TextRenderer.MaWord.Set(true);
							if (i < length)
							{
								stb2.Add(c);
							}
						}
						else
						{
							Ma.Set(true);
						}
						if (i >= length)
						{
							break;
						}
						if (!flag10)
						{
							goto IL_0577;
						}
						IL_09DB:
						i++;
						continue;
						IL_0577:
						float num21;
						float num22;
						if (html_mode)
						{
							bool flag11 = flag2;
							bool flag12;
							if (!TextRendererHtmlTag.Parse(Stb, i, c, ref num8, ref num11, ref flag2, ref textRendererTagMemory, ref blist, ref flag6, out flag12, UseStyle))
							{
								if (!UseStyle.isApplying())
								{
									flag = true;
									break;
								}
								if (flag5)
								{
									int vertexMax2 = Md.getVertexMax();
									int triMax2 = Md.getTriMax();
									if (vertexMax2 > num3)
									{
										Tx.InputCharacterOnMesh(Md, c, num3, num4, vertexMax2, triMax2, ref num15);
										num4 = triMax2;
										num3 = vertexMax2;
									}
								}
								if (flag12 && flag4)
								{
									b = 3;
									goto IL_09DB;
								}
								goto IL_09DB;
							}
							else if (flag11 && !flag2 && c == 'b')
							{
								if (flag4)
								{
									b = 3;
									goto IL_09DB;
								}
								goto IL_09DB;
							}
							else
							{
								num21 = UseStyle.size / (float)num;
								num22 = UseStyle.size * UseStyle.letterSpacing;
								num15 = X.Mx(num21, num15);
							}
						}
						else
						{
							num21 = num5;
							num22 = num6;
						}
						char c2 = c;
						if (Style.confusion)
						{
							characterInfo = Storage.getConfusionRandom(ref c2);
						}
						float num23;
						float letterSpaceRatio = Storage.getLetterSpaceRatio(c, UseStyle.monospace && !UseStyle.consider_leftshift, out num23);
						num22 *= letterSpaceRatio;
						if (c == '\t')
						{
							num8 = (float)(1 + (int)((num8 + num22) / (num22 * UseStyle.tab_spacing_chars))) * (num22 * UseStyle.tab_spacing_chars);
							goto IL_09DB;
						}
						if (c == ' ' || c == '\u3000')
						{
							num8 += num22;
							goto IL_09DB;
						}
						if (c == '\b')
						{
							goto IL_09DB;
						}
						bool flag13 = false;
						TextRenderer.CHAR_INFO_RES char_INFO_RES = TextRenderer.CHAR_INFO_RES.SUCCESS;
						PxlFrame pxlFrame = null;
						float num24 = 1f;
						float num25 = num21;
						if (!Style.confusion)
						{
							if (flag5)
							{
								char_INFO_RES = Tx.GetCharacterInfo(c, i, out characterInfo, ref pxlFrame, ref num24, num, Style.size);
							}
							else
							{
								char_INFO_RES = (Storage.TargetFont.Target.GetCharacterInfo(c, out characterInfo, num) ? TextRenderer.CHAR_INFO_RES.SUCCESS : TextRenderer.CHAR_INFO_RES.FAIL);
							}
							num25 *= num24;
							if (char_INFO_RES == TextRenderer.CHAR_INFO_RES.CONTINUE)
							{
								goto IL_09DB;
							}
							if (char_INFO_RES == TextRenderer.CHAR_INFO_RES.FAIL)
							{
								Storage.TargetFont.Target.RequestCharactersInTexture((flag5 && Tx.Stb == Stb) ? Tx.text_content : Stb.ToString(), num);
								if (!UseStyle.isApplying())
								{
									flag = true;
									break;
								}
								if (!Storage.TargetFont.Target.GetCharacterInfo(c, out characterInfo, num))
								{
									goto IL_09DB;
								}
								flag = false;
							}
						}
						if (flag3)
						{
							Md.allocUv23(4, false);
						}
						int maxY = characterInfo.maxY;
						int minY = characterInfo.minY;
						if (char_INFO_RES == TextRenderer.CHAR_INFO_RES.MESH_REPLACED)
						{
							if (pxlFrame != null)
							{
								num25 = num24 * Style.size * 0.0625f;
								Md.RotaPF(num8, 0f, num24, num24, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
								num8 += (float)pxlFrame.pSq.width * num25 * UseStyle.letterSpacing;
							}
						}
						else if (!flag13)
						{
							float num26 = (float)(characterInfo.maxX - characterInfo.minX + characterInfo.bearing * 2) * num25;
							Md.BoxCharacterInfo(num8 + ((UseStyle.monospace && !UseStyle.consider_leftshift) ? (UseStyle.size / 2f - num26 / 2f) : 0f), 0f, num25, num25, c, Storage, characterInfo, false, UseStyle);
							if (UseStyle.monospace)
							{
								num11 = (UseStyle.letterSpacing - 1f) * UseStyle.size * Storage.xratio;
								num8 += num22;
							}
							else
							{
								num11 = (UseStyle.letterSpacing + letterSpaceRatio - 2f) * UseStyle.size * Storage.xratio;
								num8 += num26 + num11;
							}
						}
						else
						{
							Md.BoxCharacterInfo(num8, 0f, num25, num25, c, Storage, characterInfo, false, UseStyle);
							float num27 = (float)characterInfo.maxX * num25;
							if ((float)characterInfo.maxX >= (float)(characterInfo.maxX - characterInfo.minX) * 0.5f)
							{
								num11 = X.Mn(num22 - num27, (UseStyle.letterSpacing - 1f) * UseStyle.size * Storage.xratio);
								num8 += num27 + num11;
							}
							else
							{
								num8 += 0f;
							}
						}
						if (flag3)
						{
							Md.Uv23(UseStyle.BorderCol, false).allocUv23(0, true);
						}
						if (!flag5)
						{
							goto IL_09DB;
						}
						int vertexMax3 = Md.getVertexMax();
						int triMax3 = Md.getTriMax();
						if (vertexMax3 > num3)
						{
							Tx.InputCharacterOnMesh(Md, c, num3, num4, vertexMax3, triMax3, ref num15);
							num4 = triMax3;
							num3 = vertexMax3;
						}
						if (char_INFO_RES == TextRenderer.CHAR_INFO_RES.MESH_REPLACED)
						{
							Tx.initMeshSub(TextRenderer.MESH_TYPE.TX);
							goto IL_09DB;
						}
						goto IL_09DB;
					}
					if (blist != null)
					{
						blist.Dispose();
						blist = null;
					}
					if (textRendererTagMemory != null)
					{
						TextRendererTagMemory.Release(textRendererTagMemory);
					}
					if (flag)
					{
						num28 = -1000f;
					}
					else
					{
						float num29 = -num9 - (UseStyle.fix_line_spacing ? UseStyle.lineSpacing : ((UseStyle.lineSpacing - 1f) * num15 * Storage.base_height));
						Ma.Set(vertexMax, triMax);
						float num30 = ((UseStyle.aligny == ALIGNY.TOP) ? 0f : ((UseStyle.aligny == ALIGNY.MIDDLE) ? 0.5f : 1f)) * num29 * 0.015625f;
						Ma.translateAll(0f, num30, true);
						if (compress_type == COMPRESS_STYPE.WHOLE_SIZE && condense_swidth > 0f && num10 != 0f && condense_swidth < num10)
						{
							float num31 = condense_swidth / num10;
							Ma.scaleAll(num31, num31, 0f, 0f, true);
							num10 = condense_swidth;
							num29 *= num31;
						}
						float num32 = x0 * 0.015625f;
						num30 = y0 * 0.015625f;
						Ma.translateAll(num32, num30, true);
						if (flag5)
						{
							Tx.swidth = num10;
							Tx.sheight = num29;
							Ma.translateAll(num32, num30, true);
						}
						num28 = num10;
					}
				}
			}
			return num28;
		}

		public bool RenderRuby(TextRenderStyle NowStyle, int ini_ver_i, STB StbContent, out int last_ver_i)
		{
			last_ver_i = ini_ver_i;
			if (ini_ver_i < 0 || this.Md.getVertexMax() == 0)
			{
				return false;
			}
			ALIGN alignx = NowStyle.alignx;
			ALIGNY aligny = NowStyle.aligny;
			float size = NowStyle.size;
			bool fix_line_spacing = NowStyle.fix_line_spacing;
			NowStyle.alignx = ALIGN.CENTER;
			NowStyle.aligny = ALIGNY.BOTTOM;
			NowStyle.size *= 0.5f;
			NowStyle.fix_line_spacing = false;
			float num = -this.Storage.base_height * NowStyle.size / this.Storage.def_size * 0.05f;
			Vector3[] vertexArray = this.Md.getVertexArray();
			int vertexMax = this.Md.getVertexMax();
			Vector2 vector = vertexArray[ini_ver_i + 1];
			Vector2 vector2 = vertexArray[vertexMax - 2];
			TextRenderer.DrawStringTo(this.Md, (vector.x + vector2.x) * 0.5f * 64f, X.Mx(vector.y, vector2.y) * 64f + num, StbContent, NowStyle, NowStyle, this.Storage, null, new MdArranger(this.Md).Set(true), true, (vector.x + vector2.x) * 64f, COMPRESS_STYPE.LINE_COMPRESS);
			NowStyle.alignx = alignx;
			NowStyle.aligny = aligny;
			NowStyle.size = size;
			NowStyle.fix_line_spacing = fix_line_spacing;
			last_ver_i = this.Md.getVertexMax();
			return last_ver_i > vertexMax;
		}

		public bool RenderSLine(TextRenderStyle NowStyle, int ini_ver_i, out int last_ver_i)
		{
			last_ver_i = this.Md.getVertexMax();
			if (ini_ver_i < 0 || last_ver_i == 0 || last_ver_i <= ini_ver_i || this.Aline_ver_pos == null)
			{
				return false;
			}
			this.initMeshSub(TextRenderer.MESH_TYPE.MESH_T0);
			int num = this.Aline_ver_pos.Count - 1;
			Vector3[] vertexArray = this.Md.getVertexArray();
			while (last_ver_i > ini_ver_i && num >= 0)
			{
				int num2 = this.Aline_ver_pos[num--];
				int num3 = X.Mx(ini_ver_i, num2);
				this.Md.Col = C32.MulA(NowStyle.MyCol, NowStyle.alpha);
				if (num3 <= last_ver_i - 4)
				{
					Vector3 zero = Vector3.zero;
					zero.y = X.NI(vertexArray[last_ver_i - 1].y, vertexArray[last_ver_i - 2].y, 0.5f);
					int num4 = 1;
					zero.z = (zero.x = X.NI(vertexArray[last_ver_i - 1].x, vertexArray[last_ver_i - 2].x, 0.5f));
					for (int i = num3; i < last_ver_i; i += 4)
					{
						float num5 = X.NI(vertexArray[i].x, vertexArray[i + 1].x, 0.5f);
						if (num5 < zero.x)
						{
							zero.x = num5;
						}
						zero.y += X.NI(vertexArray[i].y, vertexArray[i + 1].y, 0.5f);
						num4++;
					}
					if (num4 > 1)
					{
						zero.y /= (float)num4;
					}
					this.Md.Line(zero.x, zero.y, zero.z, zero.y, NowStyle.size * 0.0769f * 1.5f * 0.015625f, true, 0f, 0f);
				}
				last_ver_i = num3;
			}
			last_ver_i = this.Md.getVertexMax();
			this.initMeshSub(TextRenderer.MESH_TYPE.TX);
			return true;
		}

		protected virtual TextRenderer.CHAR_INFO_RES GetCharacterInfo(char chr, int index, out CharacterInfo Ch, ref PxlFrame PFReplace, ref float scale, int ext_size_request, float size)
		{
			if (!this.Storage.TargetFont.Target.GetCharacterInfo(chr, out Ch, ext_size_request))
			{
				return TextRenderer.CHAR_INFO_RES.FAIL;
			}
			return TextRenderer.CHAR_INFO_RES.SUCCESS;
		}

		protected virtual void InputCharacterOnMesh(MeshDrawer Md, char chr, int pre_ver_i, int pre_tri_i, int n_ver_i, int n_tri_i, ref float line_max_scale)
		{
		}

		public virtual bool ParseSpecialTag(TextRendererTagMemory TMem, TextRenderStyle NowStyle, TextRendererHtmlTag Tg, ref float drawx, ref float margin)
		{
			return false;
		}

		public virtual bool ReconsiderSpecialTag(TextRendererTagMemory TMem, TextRenderStyle NowStyle, TextRendererHtmlTag Tg)
		{
			return true;
		}

		public virtual void checkAnotherMeshIndex()
		{
		}

		public virtual Material SubMeshMtr(TextRenderer.MESH_TYPE mtype)
		{
			switch (mtype)
			{
			case TextRenderer.MESH_TYPE.ICO_B0:
			case TextRenderer.MESH_TYPE.ICO_B1:
			case TextRenderer.MESH_TYPE.ICO_T0:
			case TextRenderer.MESH_TYPE.ICO_T1:
				return MTRX.MIicon.getMtr(BLEND.NORMAL, this.stencil_ref_);
			case TextRenderer.MESH_TYPE.ICO_BORDER:
				return MTRX.MIicon.getMtr(BLEND.NORMALBORDER8, this.stencil_ref_);
			case TextRenderer.MESH_TYPE.TX:
				return this.Mtr;
			case TextRenderer.MESH_TYPE.TX_ANOTHER:
				return MTRX.StorageCabin.getMaterial(4278190080U, this.stencil_ref, true);
			}
			return MTRX.getMtr(BLEND.NORMAL, this.stencil_ref_);
		}

		public bool isEntryingDefaultMesh()
		{
			return this.mesh_using == 0 || this.Md.getCurrentSubMeshIndex() == 7;
		}

		public MeshDrawer initMeshSub(TextRenderer.MESH_TYPE mtype = TextRenderer.MESH_TYPE.TX)
		{
			TextRenderer.MESH_TYPE mesh_TYPE = (TextRenderer.MESH_TYPE)this.Md.getCurrentSubMeshIndex();
			if (this.mesh_using == 0)
			{
				if (mtype == TextRenderer.MESH_TYPE.TX)
				{
					this.Md.base_z = (float)(-(float)mtype) * 0.008f;
					return this.Md;
				}
				this.Md.InitSubMeshContainer(7);
				this.Md.connectRendererToTriMulti(this.Mrd);
				mesh_TYPE = TextRenderer.MESH_TYPE.TX;
				this.mesh_using |= 128;
				if (this.Amain_mesh_ver_pos == null)
				{
					this.Amain_mesh_ver_pos = new List<int>(2);
				}
			}
			if (mtype == mesh_TYPE)
			{
				return this.Md;
			}
			if ((this.mesh_using & (1 << (int)mtype)) == 0)
			{
				this.mesh_using |= 1 << (int)mtype;
				this.Md.chooseSubMesh((int)mtype, false, true);
				this.Md.setMaterial(this.SubMeshMtr(mtype), false);
			}
			else
			{
				this.Md.chooseSubMesh((int)mtype, false, false);
			}
			if (mtype == TextRenderer.MESH_TYPE.ICO_BORDER)
			{
				this.Md.allocUv23(64, false);
			}
			else if (mtype == TextRenderer.MESH_TYPE.TX)
			{
				this.Md.Col = C32.MulA(TextRenderer.BufStyle.MyCol, TextRenderer.BufStyle.alpha);
			}
			else
			{
				if (this.color_apply_to_image)
				{
					this.Md.Col = C32.MulA(TextRenderer.BufStyle.MyCol, TextRenderer.BufStyle.alpha);
				}
				else
				{
					this.Md.Col = C32.MulA(MTRX.ColWhite, TextRenderer.BufStyle.alpha);
				}
				this.Md.base_z = (float)(-(float)mtype) * 0.008f;
			}
			bool flag = mesh_TYPE == TextRenderer.MESH_TYPE.MESH_B_TEXTCOLOR || mesh_TYPE == TextRenderer.MESH_TYPE.TX;
			bool flag2 = mtype == TextRenderer.MESH_TYPE.MESH_B_TEXTCOLOR || mtype == TextRenderer.MESH_TYPE.TX;
			if (flag != flag2)
			{
				this.Amain_mesh_ver_pos.Add(this.Md.getVertexMax());
			}
			this.Md.Identity();
			return this.Md;
		}

		public void AddTxColorPosition(MeshDrawer Md, int _pre_ver_i)
		{
			if (Md == this.Md)
			{
				if (this.ATxColPos == null)
				{
					this.ATxColPos = new List<TextRenderer.TxColorPos>(1);
				}
				this.ATxColPos.Add(new TextRenderer.TxColorPos
				{
					ver_start_i = _pre_ver_i,
					ver_end_i = Md.getVertexMax(),
					mesh_index = Md.getCurrentSubMeshIndex()
				});
			}
		}

		public void drawToEffect(EffectItem Ef, string mesh_key, bool bottom_flag = false, float base_z = -1000f, float pixel_x = 0f, float pixel_y = 0f, float zmx = 1f, float zmy = 1f, float agR = 0f, MeshDrawer BaseMesh = null)
		{
			this.MustRedraw();
			int subMeshCount = this.Md.getSubMeshCount(true);
			int i = 0;
			while (i < subMeshCount)
			{
				int num = 0;
				int[] array = null;
				if (subMeshCount == 1)
				{
					goto IL_004D;
				}
				array = this.Md.getSubMeshData(i, ref num);
				if (array != null && num != 0)
				{
					this.Md.chooseSubMesh(i, false, false);
					goto IL_004D;
				}
				IL_013E:
				i++;
				continue;
				IL_004D:
				Material material = this.Md.getMaterial();
				MeshDrawer meshDrawer;
				if (BaseMesh != null && (material.shader == MTRX.ShaderMesh || material.shader == MTRX.ShaderMeshST || material.shader == BaseMesh.getMaterial().shader))
				{
					meshDrawer = BaseMesh;
				}
				else
				{
					meshDrawer = Ef.GetMesh(mesh_key + "_" + i.ToString(), material, bottom_flag);
					if (base_z != -1000f)
					{
						meshDrawer.base_z = base_z + 0.0001f * (float)(subMeshCount - i) / (float)subMeshCount;
					}
				}
				meshDrawer.Col = MTRX.ColWhite;
				meshDrawer.RotaMesh(pixel_x, pixel_y, zmx, zmy, agR, this.Md.getVertexArray(), this.Md.getColorArray(), this.Md.getUvArray(), this.Md.getUv2Array(true), this.Md.getUv3Array(true), array, false, 0, num, 0, -1);
				goto IL_013E;
			}
			if (subMeshCount != 1)
			{
				this.Md.chooseSubMesh(7, false, false);
			}
		}

		public TextRenderer StencilRef(int i)
		{
			if (i == this.stencil_ref_)
			{
				return this;
			}
			this.stencil_ref_ = i;
			return this.fineMaterial(false);
		}

		public TextRenderer Txt(string i)
		{
			if (this.Stb.Equals(i))
			{
				return this;
			}
			this.Stb.Set(i);
			this.Stb.Replace('─', '-', 0, -1);
			this.text_content_cache_ = i;
			return this.Redraw(false);
		}

		public TextRenderer Txt(STB Stb)
		{
			this.setText(Stb);
			return this;
		}

		public TextRenderer Txt(TextRenderer TxSrc)
		{
			this.Txt(TxSrc.Stb);
			return this;
		}

		public bool setText(STB _Stb)
		{
			if (_Stb.Equals(this.Stb))
			{
				return false;
			}
			this.Stb.Set(_Stb);
			this.Stb.Replace('─', '-', 0, -1);
			this.text_content_cache_ = null;
			return this.Redraw(false);
		}

		public TextRenderer Col(uint code)
		{
			return this.Col(C32.d2c(code));
		}

		public virtual TextRenderer Col(Color32 C)
		{
			if (C32.isEqual(this.Style.MyCol, C))
			{
				return this;
			}
			this.Style.MyCol = C;
			C.a = (byte)((float)C.a * this.Style.alpha);
			if (this.color_arranged)
			{
				return this.Redraw(false);
			}
			if (this.Ma != null && !this.redraw_flag)
			{
				if (this.Amain_mesh_ver_pos == null || this.color_apply_to_image)
				{
					this.Ma.setColAll(C, false);
				}
				else
				{
					int count = this.Amain_mesh_ver_pos.Count;
					bool flag = true;
					int i = 0;
					Color32[] colorArray = this.Md.getColorArray();
					for (int j = 0; j <= count; j++)
					{
						int num = ((j == count) ? this.Md.getVertexMax() : this.Amain_mesh_ver_pos[j]);
						if (flag)
						{
							while (i < num)
							{
								colorArray[i] = C;
								i++;
							}
							flag = false;
						}
						else
						{
							i = num;
							flag = true;
						}
					}
					if (this.ATxColPos != null)
					{
						for (int k = this.ATxColPos.Count - 1; k >= 0; k--)
						{
							TextRenderer.TxColorPos txColorPos = this.ATxColPos[k];
							if (txColorPos.ver_end_i <= this.Md.getVertexMax())
							{
								try
								{
									for (int l = txColorPos.ver_start_i; l < txColorPos.ver_end_i; l++)
									{
										colorArray[l] = C;
									}
								}
								catch
								{
								}
							}
						}
					}
				}
				this.Md.updateForMeshRenderer(true);
			}
			return this;
		}

		public virtual TextRenderer Alpha(float tz)
		{
			tz = X.ZLINE(tz);
			if (this.Style.alpha == tz)
			{
				return this;
			}
			this.Style.alpha = tz;
			if (this.alpha_arranged)
			{
				return this.Redraw(false);
			}
			if (this.Ma != null && !this.redraw_flag)
			{
				if (this.Style.MyCol.a == 255 || this.Amain_mesh_ver_pos == null || this.color_apply_to_image)
				{
					this.Ma.setAlpha1(tz * (float)this.Style.MyCol.a / 255f, false);
				}
				else
				{
					byte b = (byte)(tz * (float)this.Style.MyCol.a);
					int count = this.Amain_mesh_ver_pos.Count;
					bool flag = true;
					int i = 0;
					Color32[] colorArray = this.Md.getColorArray();
					for (int j = 0; j <= count; j++)
					{
						int num = ((j == count) ? this.Md.getVertexMax() : this.Amain_mesh_ver_pos[j]);
						if (flag)
						{
							while (i < num)
							{
								colorArray[i].a = b;
								i++;
							}
							flag = false;
						}
						else
						{
							i = num;
							flag = true;
						}
					}
					if (this.ATxColPos != null)
					{
						for (int k = this.ATxColPos.Count - 1; k >= 0; k--)
						{
							TextRenderer.TxColorPos txColorPos = this.ATxColPos[k];
							try
							{
								if (txColorPos.ver_end_i <= this.Md.getVertexMax())
								{
									for (int l = txColorPos.ver_start_i; l < txColorPos.ver_end_i; l++)
									{
										colorArray[l].a = b;
									}
								}
							}
							catch
							{
							}
						}
					}
				}
				this.Md.updateForMeshRenderer(true);
			}
			return this;
		}

		public Color32 BorderColor
		{
			get
			{
				return this.Style.BorderCol;
			}
			set
			{
				this.BorderCol(value);
			}
		}

		public TextRenderer BorderCol(Color32 C)
		{
			if (C32.isEqual(C, this.Style.BorderCol))
			{
				return this;
			}
			bool has_border = this.has_border;
			this.Style.BorderCol = C;
			this.redraw_flag = true;
			if (has_border != this.has_border)
			{
				return this.fineMaterial(false);
			}
			return this;
		}

		public TextRenderer BorderCol(uint u)
		{
			return this.BorderCol(C32.d2c(u));
		}

		public TextRenderer LineSpacing(float f)
		{
			if (this.Style.lineSpacing == f && !this.Style.fix_line_spacing)
			{
				return this;
			}
			this.Style.lineSpacing = f;
			this.Style.fix_line_spacing = false;
			return this.Redraw(false);
		}

		public TextRenderer LetterSpacing(float f)
		{
			if (this.Style.letterSpacing == f)
			{
				return this;
			}
			this.Style.letterSpacing = f;
			return this.Redraw(false);
		}

		public TextRenderer LineSpacePixel(float f)
		{
			if (this.Style.lineSpacing == f && this.Style.fix_line_spacing)
			{
				return this;
			}
			this.Style.lineSpacing = f;
			this.Style.fix_line_spacing = true;
			return this.Redraw(false);
		}

		public TextRenderer Align(ALIGN i)
		{
			if (this.Style.alignx == i)
			{
				return this;
			}
			this.Style.alignx = i;
			return this.Redraw(false);
		}

		public TextRenderer AlignY(ALIGNY i)
		{
			if (this.Style.aligny == i)
			{
				return this;
			}
			this.Style.aligny = i;
			return this.Redraw(false);
		}

		public TextRenderer ImgAlignY(ALIGNY i)
		{
			if (this.Style.image_aligny == i)
			{
				return this;
			}
			this.Style.image_aligny = i;
			return this.Redraw(false);
		}

		public TextRenderer SizeFromHeight(float i, float margin = 0.125f)
		{
			int num = this.Stb.countLines();
			return this.Size(i * (1f + (float)(num - 1) * margin) / (this.Style.lineSpacing * (float)(num - 1) + 1f));
		}

		public TextRenderer Size(float i)
		{
			if (this.Style.size == i)
			{
				return this;
			}
			this.Style.size = i;
			return this.Redraw(false);
		}

		public TextRenderer Monospace(bool _f)
		{
			this.Style.monospace = _f;
			return this.Redraw(false);
		}

		public TextRenderer Bold(bool _f)
		{
			if (this.Style.bold == _f)
			{
				return this;
			}
			this.Style.bold = _f;
			return this.Redraw(false);
		}

		public TextRenderer Italic(bool _f)
		{
			if (this.Style.italic == _f)
			{
				return this;
			}
			this.Style.italic = _f;
			return this.Redraw(false);
		}

		public void setAlpha(float tz)
		{
			this.Alpha(tz);
		}

		public float get_swidth_px()
		{
			if (this.redraw_flag)
			{
				this.Redraw(true);
			}
			return this.swidth;
		}

		public float get_sheight_px()
		{
			if (this.redraw_flag)
			{
				this.Redraw(true);
			}
			return this.sheight;
		}

		public float get_line_sheight_px(bool with_margin = false)
		{
			return this.Style.size;
		}

		public TextRenderer HeadXShift(float shift)
		{
			if (this.Style.head_x_shift == shift)
			{
				return this;
			}
			this.Style.head_x_shift = shift;
			return this.Redraw(false);
		}

		public virtual void OnEnable()
		{
			if (this.use_valotile_ && this.Valot != null)
			{
				this.Valot.enabled = true;
				return;
			}
			if (this.Mrd != null)
			{
				this.Mrd.enabled = true;
			}
		}

		public virtual void OnDisable()
		{
			if (this.Valot != null)
			{
				this.Valot.enabled = false;
				this.Valot.ReleaseBinding(false, true, false);
			}
			if (this.Mrd != null)
			{
				this.Mrd.enabled = false;
			}
		}

		protected virtual float ruby_height_ratio(float drawy)
		{
			return 0.5f;
		}

		public bool has_border
		{
			get
			{
				return this.Style.BorderCol.a >= 1;
			}
		}

		public bool monospace
		{
			get
			{
				return this.Style.monospace;
			}
			set
			{
				if (value != this.Style.monospace)
				{
					this.Style.monospace = value;
					this.Redraw(false);
				}
			}
		}

		public bool html_mode
		{
			get
			{
				return (this.flags & 1) > 0;
			}
			set
			{
				if (value != this.html_mode)
				{
					this.flags = (value ? (this.flags | 1) : (this.flags & -2));
					this.Redraw(false);
				}
			}
		}

		public bool auto_condense
		{
			get
			{
				return (this.flags & 2) > 0;
			}
			set
			{
				if (value != this.auto_condense)
				{
					this.flags = (value ? (this.flags | 2) : (this.flags & -15));
					this.Redraw(false);
				}
			}
		}

		public bool auto_wrap
		{
			get
			{
				return (this.flags & 4) > 0;
			}
			set
			{
				if (value != this.auto_wrap)
				{
					this.flags = (value ? (this.flags | 4) : (this.flags & -15));
					this.Redraw(false);
				}
			}
		}

		public bool auto_condense_line
		{
			get
			{
				return (this.flags & 8) > 0;
			}
			set
			{
				if (value != this.auto_condense_line)
				{
					this.flags = (value ? (this.flags | 8) : (this.flags & -15));
					this.Redraw(false);
				}
			}
		}

		public bool color_arranged
		{
			get
			{
				return (this.flags & 256) > 0;
			}
			set
			{
				if (value != this.color_arranged)
				{
					this.flags = (value ? (this.flags | 256) : (this.flags & -257));
				}
			}
		}

		public bool alpha_arranged
		{
			get
			{
				return (this.flags & 512) > 0;
			}
			set
			{
				if (value != this.alpha_arranged)
				{
					this.flags = (value ? (this.flags | 512) : (this.flags & -513));
				}
			}
		}

		public bool color_apply_to_image
		{
			get
			{
				return (this.flags & 1024) > 0;
			}
			set
			{
				if (value != this.color_apply_to_image)
				{
					this.flags = (value ? (this.flags | 1024) : (this.flags & -1025));
				}
			}
		}

		public virtual void getStringForListener(STB _Stb)
		{
			_Stb += this.Stb;
			if (this.can_create_big_size_texture && this.Storage != null)
			{
				this.Storage.TargetFont.Target.RequestCharactersInTexture(this.text_content, this.storage_request_size);
			}
		}

		public string text_content
		{
			get
			{
				if (this.text_content_cache_ == null)
				{
					this.text_content_cache_ = this.Stb.ToString();
				}
				return this.text_content_cache_;
			}
			set
			{
				this.Txt(value);
			}
		}

		public string text_
		{
			get
			{
				return "";
			}
		}

		public bool textIs(string t)
		{
			return this.Stb.Equals(t);
		}

		public bool textIs(STB _Stb)
		{
			return this.Stb.Equals(_Stb);
		}

		public int countLines()
		{
			return this.Stb.countLines();
		}

		public int stencil_ref
		{
			get
			{
				return this.stencil_ref_;
			}
			set
			{
				this.StencilRef(value);
			}
		}

		public Color32 TextColor
		{
			get
			{
				return this.Style.MyCol;
			}
			set
			{
				this.Col(value);
			}
		}

		public float line_spacing
		{
			get
			{
				return this.Style.lineSpacing;
			}
			set
			{
				this.LineSpacing(value);
			}
		}

		public float line_spacing_pixel
		{
			get
			{
				if (!this.Style.fix_line_spacing)
				{
					return this.Style.lineSpacing * this.Storage.base_height * this.Style.size / this.Storage.def_size;
				}
				return this.Style.lineSpacing;
			}
		}

		public float letter_spacing
		{
			get
			{
				return this.Style.letterSpacing;
			}
			set
			{
				this.LetterSpacing(value);
			}
		}

		public bool effect_confusion
		{
			get
			{
				return this.Style.confusion;
			}
			set
			{
				if (this.Style.confusion != value)
				{
					this.Style.confusion = value;
					this.redraw_flag = true;
				}
			}
		}

		public float size
		{
			get
			{
				return this.Style.size;
			}
			set
			{
				this.Size(value);
			}
		}

		public virtual int storage_request_size
		{
			get
			{
				return X.IntC(this.size);
			}
		}

		public char[] Aword_splitter
		{
			get
			{
				return this.Style.Aword_splitter;
			}
			set
			{
				if (this.Style.Aword_splitter == value)
				{
					return;
				}
				this.Style.Aword_splitter = value;
				if (this.auto_wrap)
				{
					this.Redraw(false);
				}
			}
		}

		public MFont TargetFont
		{
			get
			{
				return this.TargetFont_;
			}
			set
			{
				if (MTRX.OFontStorage == null)
				{
					this.TargetFont_ = value;
					return;
				}
				if (value == null)
				{
					value = TX.getDefaultFont();
				}
				if (this.TargetFont_ != value)
				{
					this.TargetFont_ = value;
					if (this.Storage != null)
					{
						this.Storage.Rem(this);
					}
					this.Storage = MTRX.OFontStorage[value];
					this.Storage.Add(this);
					this.fineMaterial(true);
					if (!this.textIs(""))
					{
						this.Storage.fine_text = X.Mx(1, TextRenderer.STORAGE_REFINE_DELAY - 1);
					}
					this.redraw_flag = true;
				}
			}
		}

		public ALIGN alignx
		{
			get
			{
				return this.Style.alignx;
			}
			set
			{
				this.Align(value);
			}
		}

		public ALIGNY aligny
		{
			get
			{
				return this.Style.aligny;
			}
			set
			{
				this.AlignY(value);
			}
		}

		public ALIGNY image_aligny
		{
			get
			{
				return this.Style.image_aligny;
			}
			set
			{
				this.ImgAlignY(value);
			}
		}

		public bool bold
		{
			get
			{
				return this.Style.bold;
			}
			set
			{
				this.Bold(value);
			}
		}

		public bool italic
		{
			get
			{
				return this.Style.italic;
			}
			set
			{
				this.Italic(value);
			}
		}

		public float alpha
		{
			get
			{
				return this.Style.alpha;
			}
			set
			{
				this.setAlpha(value);
			}
		}

		public Transform getTransform()
		{
			return base.transform;
		}

		public MeshDrawer getDrawer()
		{
			return this.Md;
		}

		public FontStorage getStorage()
		{
			return this.Storage;
		}

		public void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
		}

		public MeshDrawer getMeshDrawer()
		{
			return this.Md;
		}

		public MeshRenderer getMeshRenderer()
		{
			return this.Mrd;
		}

		public ValotileRenderer MakeValot(ValotileRenderer.IValotConnetcable Connect = null, CameraBidingsBehaviour BindTo = null)
		{
			if (this.Valot == null)
			{
				this.Valot = IN.GetOrAdd<ValotileRenderer>(base.gameObject);
				if (BindTo != null)
				{
					this.Valot.Init(this.Md, this.Mrd, BindTo);
				}
				else if (Connect != null)
				{
					this.Valot.Init(this.Md, this.Mrd, true);
					Connect.connectToBinder(this.Valot);
				}
				else
				{
					this.Valot.InitUI(this.Md, this.Mrd);
				}
				this.redraw_flag = true;
			}
			return this.Valot;
		}

		public void ResortZValot()
		{
			if (this.Valot != null)
			{
				this.Valot.ResortZ();
			}
		}

		public bool draw_gl_only
		{
			get
			{
				return this.Md != null && this.Md.draw_gl_only;
			}
			set
			{
				if (this.Md == null)
				{
					return;
				}
				this.Md.draw_gl_only = value;
				if (!value)
				{
					this.Md.updateForMeshRenderer(true);
				}
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				if (!value && this.Valot == null)
				{
					return;
				}
				this.MakeValot(null, null);
				Behaviour valot = this.Valot;
				this.use_valotile_ = value;
				valot.enabled = value;
			}
		}

		public bool initted
		{
			get
			{
				return this.Stb != null;
			}
		}

		private MFont TargetFont_;

		protected FontStorage Storage;

		protected MeshDrawer Md;

		public ScrollBox BelongScroll;

		private int stencil_ref_ = -1;

		private List<int> Amain_mesh_ver_pos;

		public List<int> Aline_ver_pos;

		protected TextRenderStyle Style = new TextRenderStyle();

		protected STB Stb;

		private string text_content_cache_;

		private Material Mtr;

		private MeshRenderer Mrd;

		private MeshFilter Mf;

		private int flags;

		protected int mesh_using;

		private float swidth;

		private float sheight;

		public bool do_not_error_unknown_tag;

		public float max_swidth_px;

		protected static TextRenderStyle BufStyle;

		private static MdArranger MaWord;

		protected MdArranger Ma;

		protected ValotileRenderer Valot;

		protected bool use_valotile_;

		protected static int STORAGE_REFINE_DELAY = 1;

		public bool can_create_big_size_texture;

		private List<TextRenderer.TxColorPos> ATxColPos;

		public static float keyassign_image_alpha = 1f;

		public static uint keyassign_text_col = uint.MaxValue;

		public static uint keyassign_text_border_col = 4278190080U;

		public bool redraw_flag;

		public enum MESH_TYPE
		{
			MESH_B0,
			ICO_B0,
			MESH_B1,
			ICO_B1,
			MESH_B2,
			MESH_B_TEXTCOLOR,
			ICO_BORDER,
			TX,
			TX_ANOTHER,
			MESH_T0,
			ICO_T0,
			MESH_T1,
			ICO_T1,
			SPECIAL0,
			SPECIAL1,
			_MAX
		}

		protected enum CHAR_INFO_RES
		{
			FAIL,
			SUCCESS,
			MESH_REPLACED,
			CONTINUE
		}

		private struct TxColorPos
		{
			public int ver_start_i;

			public int ver_end_i;

			public int mesh_index;
		}
	}
}

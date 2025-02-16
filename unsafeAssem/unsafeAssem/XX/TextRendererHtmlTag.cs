using System;
using System.Collections.Generic;

namespace XX
{
	public class TextRendererHtmlTag
	{
		public bool hasTagName()
		{
			return this.tagname_si < this.tagname_ei;
		}

		public void Dispose()
		{
			if (this.AVarContentMem != null)
			{
				TextRendererTagMemory.ReleaseVarMem(this.AVarContentMem);
				this.AVarContentMem = null;
			}
			this.AVarContentMem = null;
			this.tagname_si = (this.tagname_ei = 0);
			this.Stb = null;
		}

		public TextRendererHtmlTag InitTag(STB _Stb)
		{
			this.Stb = _Stb;
			return this;
		}

		public void inputVarNameS(int stb_i)
		{
			if (this.AVarContentMem == null)
			{
				this.AVarContentMem = TextRendererTagMemory.PopVarMem();
			}
			this.AVarContentMem.Add(new TextRendererHtmlTag.TagVarMem
			{
				name_si = stb_i,
				name_ei = stb_i + 1
			});
		}

		public void inputVarName(int stb_i)
		{
			if (this.AVarContentMem == null || this.AVarContentMem.Count == 0)
			{
				return;
			}
			TextRendererHtmlTag.TagVarMem tagVarMem = this.AVarContentMem[this.AVarContentMem.Count - 1];
			tagVarMem.name_ei = stb_i + 1;
			this.AVarContentMem[this.AVarContentMem.Count - 1] = tagVarMem;
		}

		public void inputVarContentS(int stb_i)
		{
			if (this.AVarContentMem == null || this.AVarContentMem.Count == 0)
			{
				return;
			}
			TextRendererHtmlTag.TagVarMem tagVarMem = this.AVarContentMem[this.AVarContentMem.Count - 1];
			tagVarMem.inputVarContentS(stb_i);
			this.AVarContentMem[this.AVarContentMem.Count - 1] = tagVarMem;
		}

		public void inputVarContent(int stb_i)
		{
			if (this.AVarContentMem == null || this.AVarContentMem.Count == 0)
			{
				return;
			}
			TextRendererHtmlTag.TagVarMem tagVarMem = this.AVarContentMem[this.AVarContentMem.Count - 1];
			tagVarMem.inputVarContent(stb_i);
			this.AVarContentMem[this.AVarContentMem.Count - 1] = tagVarMem;
		}

		public void inputVarContent(string s)
		{
			if (this.AVarContentMem == null || this.AVarContentMem.Count == 0)
			{
				return;
			}
			TextRendererHtmlTag.TagVarMem tagVarMem = this.AVarContentMem[this.AVarContentMem.Count - 1];
			tagVarMem.assignCache(s);
			this.AVarContentMem[this.AVarContentMem.Count - 1] = tagVarMem;
		}

		public bool getVarContent(string s, STB StbOut = null)
		{
			if (this.AVarContentMem == null || this.AVarContentMem.Count == 0)
			{
				return false;
			}
			for (int i = this.AVarContentMem.Count - 1; i >= 0; i--)
			{
				TextRendererHtmlTag.TagVarMem tagVarMem = this.AVarContentMem[i];
				if ((tagVarMem.name_cache != null) ? (tagVarMem.name_cache == s) : tagVarMem.KeyIs(this.Stb, s))
				{
					if (StbOut != null)
					{
						tagVarMem.getContentData(this.Stb, StbOut);
					}
					return true;
				}
			}
			return false;
		}

		public bool SetVar(string n, string v)
		{
			if (this.AVarContentMem == null)
			{
				this.AVarContentMem = TextRendererTagMemory.PopVarMem();
			}
			TextRendererHtmlTag.TagVarMem tagVarMem = default(TextRendererHtmlTag.TagVarMem);
			tagVarMem.assignCache(n, v);
			this.AVarContentMem.Add(tagVarMem);
			return false;
		}

		public bool TagNameIs(string s)
		{
			return this.Stb.Equals(this.tagname_si, this.tagname_ei, s, false);
		}

		public bool TagNameIs(TextRendererHtmlTag Tag)
		{
			return this.Stb.Equals(this.tagname_si, this.tagname_ei, Tag.Stb, Tag.tagname_si, Tag.tagname_ei);
		}

		public bool TagNameIsHeadAndNum(string header, out float suffix_num, bool ignore_case = false)
		{
			suffix_num = -1f;
			int num = this.tagname_si + header.Length;
			int num2;
			if (this.Stb.Equals(this.tagname_si, num, header, ignore_case) && this.Stb.Nm(num, out num2, -1, false) != STB.PARSERES.ERROR)
			{
				suffix_num = (float)STB.parse_result_double;
				if (suffix_num >= 0f)
				{
					return true;
				}
			}
			return false;
		}

		public string getTagName()
		{
			return this.Stb.ToString(this.tagname_si, this.tagname_ei - this.tagname_si);
		}

		public bool GetB(string name)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				if (this.getVarContent(name, stb))
				{
					if (stb.Equals(0, -1, "true", true))
					{
						return true;
					}
					if (stb.Nm())
					{
						return STB.parse_result_double != 0.0;
					}
				}
			}
			return false;
		}

		public bool HasVar(string name)
		{
			return this.getVarContent(name, null);
		}

		public static bool isSeparator(char chr)
		{
			return chr == ' ' || chr == '\t' || chr == '\r' || chr == '\n';
		}

		public static bool Parse(STB Stb, int stb_char_i, ref bool escape_flag, ref TextRendererTagMemory TMem, out TextRendererHtmlTag LastFinishedTag)
		{
			TextRendererHtmlTag textRendererHtmlTag = ((TMem != null) ? TMem.CurTag : null);
			float num = 0f;
			float num2 = 0f;
			BList<int> blist = null;
			bool flag = false;
			bool flag3;
			bool flag2 = TextRendererHtmlTag.Parse(Stb, stb_char_i, Stb[stb_char_i], ref num, ref num2, ref escape_flag, ref TMem, ref blist, ref flag, out flag3, null);
			TextRendererHtmlTag textRendererHtmlTag2 = ((TMem != null) ? TMem.CurTag : null);
			if (textRendererHtmlTag2 == null && textRendererHtmlTag != null && textRendererHtmlTag.Stb != null && textRendererHtmlTag2 != textRendererHtmlTag)
			{
				LastFinishedTag = textRendererHtmlTag;
				return flag2;
			}
			LastFinishedTag = null;
			return flag2;
		}

		public static bool Parse(STB Stb, int stb_char_i, char chr, ref float drawx, ref float pre_margin, ref bool escape_flag, ref TextRendererTagMemory TMem, ref BList<int> Aruby_start_ver_i, ref bool lock_auto_wrap, out bool recheck_word_wrap_on_tag_end, TextRenderStyle ApplyingStyle)
		{
			recheck_word_wrap_on_tag_end = false;
			if (chr == '\\')
			{
				if (!escape_flag)
				{
					escape_flag = true;
					return false;
				}
				escape_flag = false;
			}
			bool flag = escape_flag;
			escape_flag = false;
			TextRendererHtmlTag textRendererHtmlTag = ((TMem != null) ? TMem.CurTag : null);
			if (!flag && chr == '<')
			{
				if (TMem == null)
				{
					TMem = TextRendererTagMemory.Pop();
				}
				textRendererHtmlTag = TMem.MakeNewTag(Stb);
				return false;
			}
			if (textRendererHtmlTag != null)
			{
				if (!flag)
				{
					if (chr == '/')
					{
						if (TMem.st == TextRendererHtmlTag.STATE.START)
						{
							TMem.st = TextRendererHtmlTag.STATE.CLOSE_TAGNAME_WAIT;
						}
						else if (TMem.st == TextRendererHtmlTag.STATE.VAR_CONTENT_QUOTE)
						{
							textRendererHtmlTag.inputVarContent(stb_char_i);
						}
						else
						{
							if (TMem.st == TextRendererHtmlTag.STATE.VAR_NAME || TMem.st == TextRendererHtmlTag.STATE.VAR_WAIT_EQUAL)
							{
								TMem.st = TextRendererHtmlTag.STATE.VAR_NAME_WAIT;
							}
							if (!TMem.is_closer)
							{
								textRendererHtmlTag.can_close_once = true;
							}
						}
						return false;
					}
					if (chr == '>')
					{
						if (TMem.st == TextRendererHtmlTag.STATE.VAR_NAME || TMem.st == TextRendererHtmlTag.STATE.VAR_WAIT_EQUAL)
						{
							TMem.st = TextRendererHtmlTag.STATE.VAR_NAME_WAIT;
						}
						if ((textRendererHtmlTag.TagNameIs("rb") || textRendererHtmlTag.TagNameIs("s")) && !TMem.is_closer && ApplyingStyle != null && ApplyingStyle.isApplying())
						{
							TextRenderer targetRenderer = ApplyingStyle.getTargetRenderer();
							int vertexMax = targetRenderer.getMeshDrawer().getVertexMax();
							if (textRendererHtmlTag.TagNameIs("s") && targetRenderer.Aline_ver_pos == null)
							{
								targetRenderer.Aline_ver_pos = new List<int>(vertexMax);
							}
							textRendererHtmlTag.SetVar("ver_i", vertexMax.ToString());
						}
						if (textRendererHtmlTag.can_close_once)
						{
							if (ApplyingStyle != null && ApplyingStyle.createImage(TMem, textRendererHtmlTag, ref drawx, ref pre_margin) == -1000f)
							{
								return false;
							}
							TMem.RemoveTag(textRendererHtmlTag);
						}
						else
						{
							if (TMem.is_closer)
							{
								int i;
								for (i = TMem.TagCount - 1; i >= 0; i--)
								{
									TextRendererHtmlTag tag = TMem.getTag(i);
									if (tag != textRendererHtmlTag && tag.TagNameIs(textRendererHtmlTag))
									{
										if (textRendererHtmlTag.TagNameIs("rb") && ApplyingStyle != null && ApplyingStyle.isApplying())
										{
											using (STB stb = TX.PopBld(null, 0))
											{
												using (STB stb2 = TX.PopBld(null, 0))
												{
													if (tag.getVarContent("c", stb) && tag.getVarContent("ver_i", stb2))
													{
														int num = stb2.NmI(0, -1, -1);
														int num2;
														if (num >= 0 && ApplyingStyle.getTargetRenderer().RenderRuby(ApplyingStyle, num, stb, out num2))
														{
															if (!ApplyingStyle.isApplying())
															{
																return false;
															}
															if (Aruby_start_ver_i == null)
															{
																Aruby_start_ver_i = ListBuffer<int>.Pop(2);
															}
															Aruby_start_ver_i.Add(num);
															Aruby_start_ver_i.Add(num2);
														}
													}
												}
											}
										}
										if (textRendererHtmlTag.TagNameIs("s") && ApplyingStyle != null && ApplyingStyle.isApplying())
										{
											using (STB stb3 = TX.PopBld(null, 0))
											{
												if (tag.getVarContent("ver_i", stb3))
												{
													int num3 = stb3.NmI(0, -1, -1);
													if (num3 >= 0)
													{
														int num4;
														ApplyingStyle.getTargetRenderer().RenderSLine(ApplyingStyle, num3, out num4);
													}
												}
											}
										}
										TMem.RemoveTagAt(i);
										break;
									}
								}
								if (i < 0)
								{
									X.de("タグクロージャー (" + textRendererHtmlTag.getTagName() + ") に合致する開始タグがありません", null);
								}
								TMem.RemoveTag(textRendererHtmlTag);
							}
							if (ApplyingStyle != null)
							{
								ApplyingStyle.reconsider(Stb, TMem, ref lock_auto_wrap);
							}
						}
						if (!lock_auto_wrap)
						{
							recheck_word_wrap_on_tag_end = true;
						}
						TMem.CurTag = null;
						return false;
					}
					if (TextRendererHtmlTag.isSeparator(chr))
					{
						if (TMem.st == TextRendererHtmlTag.STATE.CLOSE_TAGNAME)
						{
							TMem.st = TextRendererHtmlTag.STATE.CLOSE;
						}
						else if (TMem.st == TextRendererHtmlTag.STATE.TAGNAME)
						{
							TMem.st = TextRendererHtmlTag.STATE.VAR_NAME_WAIT;
						}
						else if (TMem.st == TextRendererHtmlTag.STATE.VAR_NAME)
						{
							TMem.st = TextRendererHtmlTag.STATE.VAR_WAIT_EQUAL;
						}
						else if (TMem.st == TextRendererHtmlTag.STATE.VAR_CONTENT)
						{
							TMem.st = TextRendererHtmlTag.STATE.VAR_NAME_WAIT;
						}
						else if (TMem.st == TextRendererHtmlTag.STATE.VAR_CONTENT_QUOTE)
						{
							textRendererHtmlTag.inputVarContent(stb_char_i);
						}
						return false;
					}
					textRendererHtmlTag.can_close_once = false;
					if (chr == '=')
					{
						if (TMem.st == TextRendererHtmlTag.STATE.VAR_WAIT_EQUAL || TMem.st == TextRendererHtmlTag.STATE.VAR_NAME)
						{
							TMem.st = TextRendererHtmlTag.STATE.VAR_CONTENT_WAIT;
						}
						else if (TMem.st == TextRendererHtmlTag.STATE.VAR_CONTENT_QUOTE || TMem.st == TextRendererHtmlTag.STATE.VAR_CONTENT)
						{
							textRendererHtmlTag.inputVarContent(stb_char_i);
						}
						return false;
					}
					if (chr == '"')
					{
						if (TMem.st == TextRendererHtmlTag.STATE.VAR_CONTENT_WAIT)
						{
							TMem.st = TextRendererHtmlTag.STATE.VAR_CONTENT_QUOTE;
							textRendererHtmlTag.inputVarContentS(stb_char_i + 1);
						}
						else if (TMem.st == TextRendererHtmlTag.STATE.VAR_CONTENT_QUOTE)
						{
							TMem.st = TextRendererHtmlTag.STATE.VAR_NAME_WAIT;
						}
						return false;
					}
				}
				if (TMem.st == TextRendererHtmlTag.STATE.START)
				{
					TMem.st = TextRendererHtmlTag.STATE.TAGNAME;
					textRendererHtmlTag.tagname_si = stb_char_i;
					textRendererHtmlTag.tagname_ei = stb_char_i + 1;
				}
				if (TMem.st == TextRendererHtmlTag.STATE.CLOSE_TAGNAME_WAIT)
				{
					TMem.st = TextRendererHtmlTag.STATE.CLOSE_TAGNAME;
					textRendererHtmlTag.tagname_si = stb_char_i;
					textRendererHtmlTag.tagname_ei = stb_char_i + 1;
				}
				if (TMem.st == TextRendererHtmlTag.STATE.VAR_WAIT_EQUAL)
				{
					TMem.st = TextRendererHtmlTag.STATE.VAR_NAME_WAIT;
					textRendererHtmlTag.inputVarContent("1");
				}
				if (TMem.st == TextRendererHtmlTag.STATE.VAR_NAME_WAIT)
				{
					textRendererHtmlTag.inputVarNameS(stb_char_i);
					TMem.st = TextRendererHtmlTag.STATE.VAR_NAME;
				}
				if (TMem.st == TextRendererHtmlTag.STATE.VAR_CONTENT_WAIT)
				{
					textRendererHtmlTag.inputVarContentS(stb_char_i);
					TMem.st = TextRendererHtmlTag.STATE.VAR_CONTENT;
				}
				if (TMem.st == TextRendererHtmlTag.STATE.VAR_CONTENT || TMem.st == TextRendererHtmlTag.STATE.VAR_CONTENT_QUOTE)
				{
					textRendererHtmlTag.inputVarContent(stb_char_i);
				}
				else if (TMem.st == TextRendererHtmlTag.STATE.VAR_NAME)
				{
					textRendererHtmlTag.inputVarName(stb_char_i);
				}
				else if (TMem.st == TextRendererHtmlTag.STATE.TAGNAME || TMem.st == TextRendererHtmlTag.STATE.CLOSE_TAGNAME)
				{
					textRendererHtmlTag.tagname_ei = stb_char_i + 1;
				}
				return false;
			}
			return textRendererHtmlTag == null;
		}

		public int tagname_si;

		public int tagname_ei;

		public bool can_close_once;

		private STB Stb;

		private List<TextRendererHtmlTag.TagVarMem> AVarContentMem;

		public struct TagVarMem
		{
			public bool valid()
			{
				return this.name_si < this.name_ei;
			}

			public void inputVarContentS(int stb_i)
			{
				this.value_si = stb_i;
				this.value_ei = stb_i + 1;
			}

			public void inputVarContent(int stb_i)
			{
				this.value_ei = stb_i + 1;
			}

			public void assignCache(string s)
			{
				this.value_cache = s;
			}

			public void assignCache(string n, string v)
			{
				this.name_cache = n;
				this.value_cache = v;
			}

			public bool hasVarContent()
			{
				return this.value_cache != null || this.value_si < this.value_ei;
			}

			public bool KeyIs(STB Stb, string s)
			{
				return Stb.Equals(this.name_si, this.name_ei, s, true);
			}

			public void getContentData(STB Stb, STB StbOut)
			{
				if (this.value_cache != null)
				{
					StbOut.Add(this.value_cache);
					return;
				}
				StbOut.Add(Stb, this.value_si, this.value_ei - this.value_si);
			}

			public void getNm(STB Stb, STB StbOut)
			{
				if (this.value_cache != null)
				{
					StbOut.Add(this.value_cache);
					return;
				}
				StbOut.Add(Stb, this.value_si, this.value_ei - this.value_si);
			}

			public int name_si;

			public int name_ei;

			public string name_cache;

			private int value_si;

			private int value_ei;

			private string value_cache;
		}

		public enum STATE
		{
			START,
			TAGNAME,
			VAR_NAME_WAIT,
			VAR_NAME,
			VAR_CONTENT_WAIT,
			VAR_CONTENT,
			VAR_WAIT_EQUAL,
			VAR_CONTENT_QUOTE,
			CLOSE_TAGNAME_WAIT,
			CLOSE_TAGNAME,
			CLOSE
		}
	}
}

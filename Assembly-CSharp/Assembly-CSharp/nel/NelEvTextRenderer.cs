using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using evt;
using UnityEngine;
using XX;

namespace nel
{
	public class NelEvTextRenderer : TextRenderer
	{
		public void initNelMsg(INelMSG _Container)
		{
			this.Container = _Container;
			this.AVer0 = null;
			this.AChr = new NelEvTextRenderer.EvChar[128];
			this.AChangePos = new List<NelEvTextRenderer.ColorChangePos>(4);
			base.html_mode = true;
			this.Areserved_text = new List<string>(1);
		}

		public void forceProgressNextStack()
		{
			if (this.Areserved_text.Count == 0 || this.Areserved_text[0] == null)
			{
				return;
			}
			string text = this.Areserved_text[0];
			this.Areserved_text.RemoveAt(0);
			this.Areserved_text.Add(null);
			this.forceProgressNextStack(text);
			if (!this.hasReservedContent())
			{
				this.Areserved_text.Clear();
			}
		}

		public void forceProgressNextStack(string rsv_text)
		{
			using (STB stb = TX.PopBld(rsv_text, 0))
			{
				this.forceProgressNextStack(stb);
			}
		}

		public void forceProgressNextStack(STB TargetStb)
		{
			int num = TargetStb.Length;
			int i = 0;
			while (i < num)
			{
				if (TargetStb[i] == '─')
				{
					int num2;
					TargetStb.Scroll(i, out num2, (char _c) => _c == '─', num);
					TargetStb.Splice(i, num2 - i);
					TargetStb.Insert(i, "<shape dashline tx_color/>", 1);
					num = TargetStb.Length;
					i += "<shape dashline tx_color/>".Length;
				}
				else
				{
					i++;
				}
			}
			this.Container.FixTextContent(TargetStb);
			this.Container.progressReserved();
			base.Txt(TargetStb);
			this.us_cp_i = (this.us_char_i = 0);
			this.t = 1f;
			this.wait_char_time = 0;
			this.need_character_pos_check = true;
			this.Container.all_char_shown = false;
			this.redraw_flag = true;
			this.entry_char_index = 0;
			this.use_noise = 0;
			if (this.next_progress_t < 0f)
			{
				this.next_progress_t = 0f;
			}
			base.MustRedraw();
			this.wait_char_time = 0;
			this.byte_talk = 0;
			this.need_color_update = true;
			this.next_progress_t = 1f;
		}

		public void run(float fcnt, bool skipping = false)
		{
			if (this.t < 0f)
			{
				this.t += fcnt;
				if (this.t >= 0f || skipping || this.use_noise > 0)
				{
					this.t = 0f;
					this.need_color_update = true;
				}
				else if (X.D)
				{
					this.need_color_update = true;
					this.need_pos_update = true;
				}
			}
			if (this.t == 0f && this.hasReservedContent())
			{
				this.forceProgressNextStack();
			}
			if (this.t >= 1f)
			{
				if (this.us_char_i < this.chr_use_i)
				{
					bool flag = false;
					NelEvTextRenderer.EvChar[] achr = this.AChr;
					int num = this.us_char_i;
					float num2 = -1f;
					bool flag2 = false;
					while (this.next_progress_t >= 0f && this.t >= this.next_progress_t && !flag2)
					{
						for (;;)
						{
							if (num2 == -1f)
							{
								num2 = this.text_hiding_alpha;
							}
							if (!this.showOneChar(this.us_char_i, ref this.AChr[this.us_char_i], num2, -1))
							{
								goto Block_9;
							}
							this.next_progress_t += this.AChangePos[this.us_cp_i].char_timescale * 1.5f;
							if (this.byte_talk <= 1)
							{
								SND.Ui.play(this.Container.talker_snd, false);
								this.byte_talk = 4;
							}
							flag = true;
							int num3 = this.us_char_i + 1;
							this.us_char_i = num3;
							if (num3 >= this.chr_use_i)
							{
								goto Block_11;
							}
						}
						IL_016B:
						if (this.next_progress_t >= 0f)
						{
							continue;
						}
						break;
						Block_9:
						flag2 = true;
						goto IL_016B;
						Block_11:
						this.allCharShown(false, false, false);
						flag2 = true;
						goto IL_016B;
					}
					if (flag)
					{
					}
				}
				else if (!this.Container.all_char_shown)
				{
					this.allCharShown(false, false, false);
				}
				this.t += fcnt;
			}
			if (this.byte_talk > 1)
			{
				int num3 = this.byte_talk - 1;
				this.byte_talk = num3;
			}
			if (this.need_color_update)
			{
				this.recolorWhole();
			}
			if (X.D && this.t != 0f && this.chr_use_i > 0 && (this.need_pos_update || this.need_character_pos_check))
			{
				NelEvTextRenderer.ColorChangePos colorChangePos = this.AChangePos[0];
				if (this.us_char_i >= this.chr_use_i)
				{
					this.need_character_pos_check = false;
				}
				float num4 = X.ZLINE(-this.t, (float)this.DELAY_CONTINUE);
				float num5 = ((this.t > 0f) ? 0f : ((1f - num4) * 30f * 0.015625f));
				this.Md.getColorArray();
				MeshDrawer md = this.Md;
				int num6 = X.Mn(this.AChangePos.Count - 1, this.us_cp_i + 1);
				for (int i = 1; i <= num6; i++)
				{
					NelEvTextRenderer.ColorChangePos colorChangePos2 = this.AChangePos[i];
					int num7 = X.Mn(colorChangePos2.index, this.us_char_i);
					for (int j = colorChangePos.index; j < num7; j++)
					{
						NelEvTextRenderer.EvChar evChar = this.AChr[j];
						evChar.reposit(this, (this.next_progress_t <= -2f) ? 65535f : this.t, this.Md, j, this.need_pos_update, colorChangePos, num5, EMOT.NORMAL);
					}
					colorChangePos = colorChangePos2;
				}
				this.need_mesh_update = true;
				this.need_pos_update = false;
			}
			if (this.need_mesh_update)
			{
				this.Container.TextRendererUpdated();
				this.need_mesh_update = false;
				this.Md.updateForMeshRenderer(true);
				if (this.FnMeshUpdated != null)
				{
					this.FnMeshUpdated(this);
				}
			}
		}

		public void showImmediate(bool show_only_first_char = false, bool play_snd = false)
		{
			if (this.hasReservedContent() && this.t < 0f)
			{
				this.t = -1f;
			}
			if (show_only_first_char)
			{
				return;
			}
			if (this.t < 0f)
			{
				if (this.hasReservedContent() && !TX.isStart(this.Areserved_text[0], "<I><S0>", 0))
				{
					this.Areserved_text[0] = "<I><S0>" + this.Areserved_text[0];
					return;
				}
			}
			else
			{
				this.allCharShown(true, true, true);
				if (play_snd)
				{
					SND.Ui.play(this.Container.talker_snd, false);
				}
			}
		}

		private void allCharShown(bool immediate_finalize = false, bool changepos_finalize = true, bool write_changepos_one = false)
		{
			this.us_char_i = this.chr_use_i;
			this.us_cp_i = this.AChangePos.Count - 1;
			this.next_progress_t = (float)((immediate_finalize || this.next_progress_t <= -2f) ? (-2) : (-1));
			if (this.byte_talk > 0)
			{
				this.byte_talk = 0;
				SND.Ui.play(this.Container.talker_snd, false);
			}
			if (changepos_finalize)
			{
				int count = this.AChangePos.Count;
				for (int i = 0; i < count; i++)
				{
					NelEvTextRenderer.ColorChangePos colorChangePos = this.AChangePos[i];
					colorChangePos.startt = 0;
					colorChangePos.char_timescale = 0f;
				}
			}
			if (write_changepos_one)
			{
				this.byte_talk = 0;
				for (int j = 0; j <= this.us_cp_i; j++)
				{
					this.AChangePos[j].startt = 1;
				}
			}
			this.need_color_update = true;
			this.Container.all_char_shown = true;
			if (!this.hasReservedContent())
			{
				this.Container.executeRestMsgCmd();
			}
		}

		private bool showOneChar(int i, ref NelEvTextRenderer.EvChar C, float _alpha, int cp_limit = -1)
		{
			NelEvTextRenderer.ColorChangePos colorChangePos = this.AChangePos[this.us_cp_i];
			if (this.us_cp_i < this.AChangePos.Count - 1)
			{
				NelEvTextRenderer.ColorChangePos colorChangePos2 = this.AChangePos[this.us_cp_i + 1];
				while (colorChangePos2.index <= i)
				{
					if (colorChangePos2.startt <= 0)
					{
						if (cp_limit >= 0 && this.us_cp_i >= cp_limit)
						{
							return false;
						}
						if (colorChangePos.index < colorChangePos2.index && colorChangePos.time_from_shown(i, (int)this.t + colorChangePos2.startt) <= 0f)
						{
							return false;
						}
						colorChangePos2.startt = (int)this.t;
					}
					int num = this.us_cp_i + 1;
					this.us_cp_i = num;
					if (num >= this.AChangePos.Count - 1)
					{
						break;
					}
					colorChangePos = colorChangePos2;
					colorChangePos2 = this.AChangePos[this.us_cp_i + 1];
				}
			}
			Color32 color = MTRX.ColWhite;
			Color32[] colorArray = this.Md.getColorArray();
			if (!C.use_default_emot)
			{
				color = colorChangePos.Col;
			}
			bool flag2;
			bool flag = colorChangePos.MulA(ref color, i, (this.next_progress_t <= -2f) ? 65535 : ((int)this.t - C.wait), base.alpha * _alpha, out flag2);
			if (flag2)
			{
				this.need_color_update = true;
			}
			if (!flag)
			{
				return false;
			}
			if (cp_limit < 0 && this.need_color_update)
			{
				return true;
			}
			for (int j = C.pre_ver_i; j < C.last_ver_i; j++)
			{
				if (C.use_default_emot)
				{
					colorArray[j].a = color.a;
				}
				else
				{
					colorArray[j] = color;
				}
			}
			if (this.use_noise > 0 && !C.noise_attached)
			{
				C.noise_attached = true;
				uint num2 = NelEvTextRenderer.XorsNoise.get2((uint)(i * 177), (uint)(i % 31));
				if (X.RAN(num2, 2079) < 0.24f * (float)this.use_noise)
				{
					Vector3[] vertexArray = this.Md.getVertexArray();
					Vector2 vector = vertexArray[C.pre_ver_i];
					Vector2 vector2 = vertexArray[C.pre_ver_i + 2];
					float num3 = (vector2.x - vector.x) * X.NI(-0.1f, 0.11f, X.RAN(num2, 770));
					float num4 = (vector2.x - vector.x) * X.NI(-0.1f, 0.11f, X.RAN(num2, 449));
					float num5 = (vector2.y - vector.y) * X.NI(-0.1f, 0.11f, X.RAN(num2, 1123));
					float num6 = (vector2.y - vector.y) * X.NI(-0.1f, 0.11f, X.RAN(num2, 2495));
					vector.x -= num3;
					vector.y -= num6;
					vector2.x += num4;
					vector2.y += num5;
					int currentSubMeshIndex = this.Md.getCurrentSubMeshIndex();
					int num7 = 0;
					bool flag3 = this.Md.getSubMeshData(13, ref num7) == null || num7 == 0;
					MeshDrawer meshDrawer = base.initMeshSub(TextRenderer.MESH_TYPE.SPECIAL0);
					float base_x = meshDrawer.base_x;
					float base_y = meshDrawer.base_y;
					meshDrawer.base_x = vector.x;
					meshDrawer.base_y = vector.y;
					meshDrawer.uvRect(0f, 0f, 1f, 1f, false, false);
					meshDrawer.uv_settype = UV_SETTYPE.IMG;
					int num8 = (int)(X.RAN(num2, 507) * 3f);
					meshDrawer.Col = meshDrawer.ColGrd.Set(4287168256U).mulA((num8 == 0) ? 0.25f : 1f).C;
					float num9 = X.NI(0.15f, 0.6f, X.RAN(num2, 1837));
					float num10 = X.NI(0.15f, 0.6f, X.RAN(num2, 3145));
					float num11 = X.NI(0.15f, 0.6f, X.RAN(num2, 4343));
					float num12 = X.NI(0.15f, 0.6f, X.RAN(num2, 1988));
					meshDrawer.RectBL(0f, 0f, vector2.x - vector.x, vector2.y - vector.y, true).InputImageUv(-num9, -num10, num9 + num11 + 1f, num10 + num12 + 1f);
					meshDrawer.Col = meshDrawer.ColGrd.Set(4294901896U).mulA((num8 == 1) ? 0.25f : 1f).C;
					num9 = X.NI(0.15f, 0.6f, X.RAN(num2, 1141));
					num10 = X.NI(0.15f, 0.6f, X.RAN(num2, 2191));
					num11 = X.NI(0.15f, 0.6f, X.RAN(num2, 2569));
					num12 = X.NI(0.15f, 0.6f, X.RAN(num2, 472));
					meshDrawer.RectBL(0f, 0f, vector2.x - vector.x, vector2.y - vector.y, true).InputImageUv(-num9, -num10, num9 + num11 + 1f, num10 + num12 + 1f);
					meshDrawer.Col = meshDrawer.ColGrd.Set(4278225151U).mulA((num8 >= 2) ? 0f : 0.85f).C;
					num9 = X.NI(0.15f, 0.6f, X.RAN(num2, 1764));
					num10 = X.NI(0.15f, 0.6f, X.RAN(num2, 2093));
					num11 = X.NI(0.15f, 0.6f, X.RAN(num2, 1384));
					num12 = X.NI(0.15f, 0.6f, X.RAN(num2, 1037));
					meshDrawer.RectBL(0f, 0f, vector2.x - vector.x, vector2.y - vector.y, true).InputImageUv(-num9, -num10, num9 + num11 + 1f, num10 + num12 + 1f);
					SND.Ui.play("msg_noise", false);
					meshDrawer.base_x = base_x;
					meshDrawer.base_y = base_y;
					this.Md.chooseSubMesh(currentSubMeshIndex, false, false);
					if (flag3)
					{
						base.fineMaterial(true);
					}
					if (this.byte_talk >= 1)
					{
						this.byte_talk = 5;
					}
				}
			}
			this.need_mesh_update = true;
			return true;
		}

		public override Material SubMeshMtr(TextRenderer.MESH_TYPE mtype)
		{
			if (mtype == TextRenderer.MESH_TYPE.SPECIAL0)
			{
				if (NelEvTextRenderer.MtrNoise == null)
				{
					NelEvTextRenderer.MtrNoise = MTRX.newMtr(MTR.getShd("Hachan/MessageNoise"));
				}
				return NelEvTextRenderer.MtrNoise;
			}
			return base.SubMeshMtr(mtype);
		}

		private float text_hiding_alpha
		{
			get
			{
				if (this.t <= 0f)
				{
					return X.ZLINE(-this.t, (float)this.DELAY_CONTINUE);
				}
				return 1f;
			}
		}

		private void recolorWhole()
		{
			this.need_color_update = false;
			int num = this.us_cp_i;
			this.us_cp_i = 0;
			float text_hiding_alpha = this.text_hiding_alpha;
			int num2 = 0;
			while (num2 < this.us_char_i && this.showOneChar(num2, ref this.AChr[num2], text_hiding_alpha, num))
			{
				num2++;
			}
			this.us_cp_i = num;
			this.need_mesh_update = true;
		}

		public override void getStringForListener(STB Stb)
		{
			base.getStringForListener(Stb);
			int count = this.Areserved_text.Count;
			for (int i = 0; i < count; i++)
			{
				string text = this.Areserved_text[i];
				if (text == null)
				{
					break;
				}
				Stb += text;
			}
		}

		public override void entryMesh()
		{
			int num = this.entry_char_index;
			int num2 = this.us_char_i;
			NelEvTextRenderer.entry_emotion = this.default_emot;
			NelEvTextRenderer.entry_timescale = 1f;
			this.need_character_pos_check = false;
			this.Style.MyCol = this.DefaultCol;
			this.can_create_big_size_texture = false;
			if (this.Stb.IndexOf("<big>", 0, -1) >= 0)
			{
				this.can_create_big_size_texture = true;
			}
			this.chr_use_i = 0;
			this.entry_char_index = 0;
			this.AChangePos.Clear();
			int num3 = this.wait_char_time;
			this.wait_char_time = 0;
			bool flag = false;
			if (this.next_progress_t <= -1f && this.Stb.Length > 0 && this.t >= 0f)
			{
				this.next_progress_t = -3f;
				flag = true;
			}
			this.insertChangePos(null, null);
			this.AChangePos[0].startt = 1;
			if (this.AChr.Length < this.Stb.Length)
			{
				Array.Resize<NelEvTextRenderer.EvChar>(ref this.AChr, this.Stb.Length);
			}
			base.entryMesh();
			this.entry_char_index = num;
			this.insertChangePos(null, null);
			this.wait_char_time = num3;
			this.need_pos_update = false;
			int vertexMax = this.Md.getVertexMax();
			if (this.AVer0 != null && this.AVer0.Length >= vertexMax)
			{
				Array.Copy(this.Md.getVertexArray(), this.AVer0, vertexMax);
			}
			else
			{
				this.AVer0 = X.concat<Vector3>(this.Md.getVertexArray(), null, -1, -1);
			}
			if (this.us_char_i > 0)
			{
				this.need_color_update = true;
			}
			if (flag)
			{
				if (this.next_progress_t == -3f)
				{
					this.next_progress_t = -2f;
				}
				this.allCharShown(true, true, true);
			}
		}

		public bool reserveText(List<string> Atext, int container_time, bool merge_flag = false)
		{
			if (Atext == null || Atext.Count == 0 || TX.noe(Atext[0]))
			{
				return false;
			}
			if (!merge_flag || !this.hasReservedContent())
			{
				this.Areserved_text.Clear();
				this.Areserved_text.AddRange(Atext);
				this.progressNextParagraph(container_time <= 5, null);
			}
			else
			{
				int num = this.Areserved_text.Count;
				for (int i = 0; i < num; i++)
				{
					if (this.Areserved_text[i] == null)
					{
						this.Areserved_text.InsertRange(i, Atext);
						num = -1;
						break;
					}
				}
				if (num >= 0)
				{
					this.Areserved_text.AddRange(Atext);
				}
			}
			if (this.Storage != null)
			{
				this.Storage.fine_text = TextRenderer.STORAGE_REFINE_DELAY;
			}
			return true;
		}

		protected override void InputCharacterOnMesh(MeshDrawer Md, char chr, int pre_ver_i, int pre_tri_i, int n_ver_i, int n_tri_i, ref float line_max_scale)
		{
			NelEvTextRenderer.EvChar evChar = new NelEvTextRenderer.EvChar(this, pre_ver_i, n_ver_i, this.entry_char_index, this.wait_char_time, Md);
			if (pre_tri_i == n_tri_i)
			{
				evChar.use_default_emot = true;
			}
			if (this.chr_use_i >= this.AChr.Length)
			{
				Array.Resize<NelEvTextRenderer.EvChar>(ref this.AChr, this.chr_use_i + 16);
			}
			this.AChr[this.chr_use_i] = evChar;
			this.chr_use_i++;
			this.entry_char_index++;
		}

		public override bool ParseSpecialTag(TextRendererTagMemory TMem, TextRenderStyle NowStyle, TextRendererHtmlTag Tg, ref float drawx, ref float margin)
		{
			bool flag = this.ReconsiderSpecialTag(TMem, NowStyle, Tg);
			if (flag)
			{
				this.ReconsiderSpecialTag(TMem, NowStyle, null);
			}
			return flag;
		}

		public override bool ReconsiderSpecialTag(TextRendererTagMemory TMem, TextRenderStyle NowStyle, TextRendererHtmlTag Tg)
		{
			if (NowStyle == null)
			{
				this.wait_char_time = 0;
				NelEvTextRenderer.entry_emotion = this.default_emot;
				NelEvTextRenderer.entry_timescale = 1f;
			}
			else if (Tg != null)
			{
				if (Tg.TagNameIs("normal"))
				{
					this.Container.setHkdsType(NelMSG.HKDSTYPE.NORMAL);
					NowStyle.MyCol = this.DefaultCol;
				}
				else if (Tg.TagNameIs("angry"))
				{
					this.Container.setHkdsType(NelMSG.HKDSTYPE.ANGRY);
					NowStyle.MyCol = this.DefaultCol;
				}
				else if (Tg.TagNameIs("think"))
				{
					this.Container.setHkdsType(NelMSG.HKDSTYPE.THINK);
					NowStyle.MyCol = this.DefaultCol;
				}
				else if (Tg.TagNameIs("circ"))
				{
					this.Container.setHkdsType(NelMSG.HKDSTYPE.CIRC);
					NowStyle.MyCol = this.DefaultCol;
				}
				else if (Tg.TagNameIs("device"))
				{
					this.Container.setHkdsType(NelMSG.HKDSTYPE.DEVICE);
					NowStyle.MyCol = this.DefaultCol;
				}
				else if (Tg.TagNameIs("evil"))
				{
					this.Container.setHkdsType(NelMSG.HKDSTYPE.EVIL);
					NowStyle.MyCol = MTRX.ColWhite;
					NelEvTextRenderer.entry_timescale = 6f;
				}
				else if (Tg.TagNameIs("noise"))
				{
					if (NelEvTextRenderer.XorsNoise == null)
					{
						NelEvTextRenderer.XorsNoise = new XorsMaker(0U, false);
					}
					using (STB stb = TX.PopBld(null, 0))
					{
						if (Tg.getVarContent("level", stb) && stb.Nm())
						{
							this.use_noise = (byte)STB.parse_result_double;
						}
						else
						{
							this.use_noise = 1;
						}
					}
					uint num = (uint)TX.text2id(this.Stb, 16777215);
					NelEvTextRenderer.XorsNoise.init(false, num, 0U, 0U, 0U);
				}
				else if (!Tg.TagNameIs("I"))
				{
					float num2;
					if (Tg.TagNameIs("m"))
					{
						NelEvTextRenderer.entry_emotion = this.default_emot;
						if (Tg.HasVar("pleasure") || Tg.HasVar("ple"))
						{
							NelEvTextRenderer.entry_emotion = EMOT.PLEASURE;
						}
						else if (Tg.HasVar("joy"))
						{
							NelEvTextRenderer.entry_emotion = EMOT.JOY;
						}
						else if (Tg.HasVar("scary"))
						{
							NelEvTextRenderer.entry_emotion = EMOT.SCARY;
						}
						else if (Tg.HasVar("fadein"))
						{
							NelEvTextRenderer.entry_emotion = EMOT.FADEIN;
						}
						else if (Tg.HasVar("cry"))
						{
							NelEvTextRenderer.entry_emotion = EMOT.CRY;
						}
						else if (Tg.HasVar("strong"))
						{
							NelEvTextRenderer.entry_emotion = EMOT.STRONG;
						}
						else if (Tg.HasVar("vib"))
						{
							NelEvTextRenderer.entry_emotion = EMOT.VIB;
						}
						else if (Tg.HasVar("normal"))
						{
							NelEvTextRenderer.entry_emotion = EMOT.NORMAL;
						}
					}
					else if (Tg.TagNameIs("c"))
					{
						NowStyle.MyCol = this.DefaultCol;
						if (Tg.HasVar("red"))
						{
							NowStyle.MyCol = MSG.AmsgColors[1];
						}
						else if (Tg.HasVar("orange") || Tg.HasVar("ora"))
						{
							NowStyle.MyCol = MSG.AmsgColors[2];
						}
						else if (Tg.HasVar("yellow") || Tg.HasVar("yel"))
						{
							NowStyle.MyCol = MSG.AmsgColors[3];
						}
						else if (Tg.HasVar("green") || Tg.HasVar("gre"))
						{
							NowStyle.MyCol = MSG.AmsgColors[4];
						}
						else if (Tg.HasVar("blue") || Tg.HasVar("blu"))
						{
							NowStyle.MyCol = MSG.AmsgColors[5];
						}
						else if (Tg.HasVar("pink") || Tg.HasVar("pin"))
						{
							NowStyle.MyCol = MSG.AmsgColors[6];
						}
						else if (Tg.HasVar("gray") || Tg.HasVar("gra"))
						{
							NowStyle.MyCol = MSG.AmsgColors[7];
						}
						else if (Tg.HasVar("white") || Tg.HasVar("whi"))
						{
							NowStyle.MyCol = MSG.AmsgColors[8];
						}
						else if (Tg.HasVar("black") || Tg.HasVar("blk"))
						{
							NowStyle.MyCol = C32.d2c(4278190080U);
						}
						NowStyle.MyCol.a = byte.MaxValue;
					}
					else if (Tg.TagNameIs("big"))
					{
						NowStyle.size = 38f;
					}
					else if (Tg.TagNameIs("small"))
					{
						NowStyle.size = 12f;
					}
					else if (Tg.TagNameIsHeadAndNum("W", out num2, false))
					{
						this.wait_char_time += 10 * (int)num2;
					}
					else if (Tg.TagNameIsHeadAndNum("w", out num2, false))
					{
						this.wait_char_time += (int)num2;
					}
					else if (Tg.TagNameIsHeadAndNum("s", out num2, false))
					{
						NelEvTextRenderer.entry_timescale = num2;
					}
					else if (Tg.TagNameIsHeadAndNum("S", out num2, false))
					{
						NelEvTextRenderer.entry_timescale = ((num2 == 0f) ? 0f : (1f / num2));
					}
					else if (Tg.TagNameIsHeadAndNum("m", out num2, true))
					{
						NelEvTextRenderer.entry_emotion = (EMOT)num2;
					}
					else
					{
						if (!Tg.TagNameIsHeadAndNum("c", out num2, true))
						{
							return false;
						}
						NowStyle.MyCol = (X.BTW(0f, num2, (float)MSG.AmsgColors.Length) ? MSG.AmsgColors[(int)num2] : MSG.AmsgColors[0]);
						NowStyle.MyCol.a = byte.MaxValue;
					}
				}
			}
			else
			{
				bool flag = false;
				NelEvTextRenderer.ColorChangePos colorChangePos = this.AChangePos[this.AChangePos.Count - 1];
				if (colorChangePos.emot != NelEvTextRenderer.entry_emotion || colorChangePos.char_timescale != NelEvTextRenderer.entry_timescale)
				{
					flag = true;
				}
				if (!C32.isEqual(colorChangePos.Col, NowStyle.MyCol) || NowStyle.MyCol.a == 0)
				{
					NowStyle.MyCol.a = byte.MaxValue;
					flag = true;
				}
				if (flag)
				{
					this.insertChangePos(TMem, NowStyle);
				}
			}
			return true;
		}

		private void insertChangePos(TextRendererTagMemory TMem = null, TextRenderStyle Style = null)
		{
			if (Style == null)
			{
				Style = this.Style;
			}
			this.AChangePos.Add(new NelEvTextRenderer.ColorChangePos
			{
				Col = Style.MyCol,
				emot = NelEvTextRenderer.entry_emotion,
				char_timescale = NelEvTextRenderer.entry_timescale,
				index = this.chr_use_i,
				startt = -this.wait_char_time
			});
			EMOT emot = NelEvTextRenderer.entry_emotion;
			if (emot - EMOT.PLEASURE <= 2 || emot - EMOT.CRY <= 2)
			{
				this.need_character_pos_check = true;
			}
			if (TMem != null && this.wait_char_time > 0)
			{
				for (int i = TMem.TagCount - 1; i >= 0; i--)
				{
					TextRendererHtmlTag tag = TMem.getTag(i);
					float num;
					if (tag.TagNameIsHeadAndNum("W", out num, false) || tag.TagNameIsHeadAndNum("w", out num, false))
					{
						TMem.RemoveTagAt(i);
					}
				}
			}
			this.wait_char_time = 0;
		}

		public EMOT default_emot
		{
			get
			{
				return this.Container.default_emot;
			}
		}

		public Color32 DefaultCol
		{
			get
			{
				return C32.d2c(this.Container.default_col);
			}
		}

		public override TextRenderer Alpha(float tz)
		{
			if (this.Style.alpha == tz)
			{
				return this;
			}
			this.Style.alpha = tz;
			this.need_color_update = true;
			return this;
		}

		public bool hasReservedContent()
		{
			return this.Areserved_text.Count > 0 && this.Areserved_text[0] != null;
		}

		public int getReservedProgress()
		{
			if (!this.hasReservedContent())
			{
				return -1;
			}
			int num = 0;
			int num2 = this.Areserved_text.Count - 1;
			while (num2 >= 0 && this.Areserved_text[num2] == null)
			{
				num++;
				num2--;
			}
			return num;
		}

		public bool progressNextParagraph(bool long_delay = false, STB StbShow = null)
		{
			if (!this.hasReservedContent())
			{
				return false;
			}
			this.t = ((this.use_noise > 0) ? (-1f) : X.Mn(this.t, (float)(long_delay ? (-(float)this.DELAY_FIRST) : (-(float)this.DELAY_CONTINUE))));
			this.use_noise = 0;
			int count = this.AChangePos.Count;
			for (int i = 0; i < count; i++)
			{
				this.AChangePos[i].startt = 0;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				if (StbShow != null)
				{
					stb.Add(StbShow);
				}
				else
				{
					stb.Set(this.Areserved_text[0]);
				}
				if (stb.IndexOf("<I>", 0, -1) >= 0)
				{
					this.t = -1f;
					if (this.Container is NelMSG)
					{
						(this.Container as NelMSG).showImmediate(true, false);
					}
				}
				if (stb.isStart('<'))
				{
					stb.ToLower();
					for (int j = 0; j < 8; j++)
					{
						NelMSG.HKDSTYPE hkdstype = (NelMSG.HKDSTYPE)j;
						string text = FEnum<NelMSG.HKDSTYPE>.ToStr(hkdstype);
						if (stb.isStart(text, 1) && stb.isStart(">", 1 + text.Length))
						{
							this.Container.setHkdsType(hkdstype);
							break;
						}
					}
				}
			}
			this.next_progress_t = -2f;
			return true;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			if (this.Container.is_temporary_hiding)
			{
				return;
			}
			if (this.AChr != null)
			{
				this.chr_use_i = 0;
			}
			this.us_char_i = 0;
			this.AChangePos.Clear();
		}

		protected override float ruby_height_ratio(float drawy)
		{
			if (drawy != 0f)
			{
				return 0.5f;
			}
			return 0.1f;
		}

		public override int storage_request_size
		{
			get
			{
				return X.Mx(38, base.storage_request_size);
			}
		}

		public INelMSG Container;

		private NelEvTextRenderer.EvChar[] AChr;

		private List<NelEvTextRenderer.ColorChangePos> AChangePos;

		private int chr_use_i;

		private int entry_char_index;

		private static EMOT entry_emotion;

		private static float entry_timescale;

		private List<string> Areserved_text;

		private float t;

		private Vector3[] AVer0;

		public int DELAY_FIRST = 20;

		public int DELAY_CONTINUE = 14;

		public const int DELAY_FIRST_DEFAULT = 20;

		public const int DELAY_CONTINUE_DEFAULT = 14;

		private bool need_color_update;

		private bool need_pos_update;

		private bool need_mesh_update;

		private bool need_character_pos_check;

		private int us_char_i;

		private int us_cp_i;

		private const float CHAR_TIME_INTV_DEFAULT = 1.5f;

		private const float SHIFTY_PX = 30f;

		private float next_progress_t;

		private static readonly Regex RegEmotOrColorTag = new Regex("^([mMeE])(\\d+)$");

		private static readonly Regex RegWait = new Regex("^([wW])(\\d+)$");

		public Func<NelEvTextRenderer, bool> FnMeshUpdated;

		private int wait_char_time;

		private int byte_talk;

		private static XorsMaker XorsNoise;

		private byte use_noise;

		private static Material MtrNoise;

		private const int SIZE_BIG = 38;

		private struct EvChar
		{
			public EvChar(NelEvTextRenderer Tx, int pv, int v, int _char_index, int _wait, MeshDrawer _Md)
			{
				this.pre_ver_i = pv;
				this.last_ver_i = v;
				this.char_index = _char_index;
				this.wait = _wait;
				this.noise_attached = false;
				this.use_default_emot = false;
				Color32[] colorArray = _Md.getColorArray();
				for (int i = this.pre_ver_i; i < this.last_ver_i; i++)
				{
					colorArray[i].a = 0;
				}
			}

			public void reposit(NelEvTextRenderer Tx, float t, MeshDrawer MdDep, int char_index, bool force, NelEvTextRenderer.ColorChangePos Cp, float shifty, EMOT default_emot)
			{
				Vector3[] aver = Tx.AVer0;
				Vector3[] vertexArray = MdDep.getVertexArray();
				EMOT emot = (this.use_default_emot ? default_emot : Cp.emot);
				if (emot == EMOT.NORMAL && !force)
				{
					return;
				}
				int num = this.last_ver_i - this.pre_ver_i;
				float num2 = 0f;
				switch (emot)
				{
				case EMOT.PLEASURE:
				{
					float num3 = Cp.time_from_shown(char_index, (int)t - this.wait);
					float num4 = (1f - X.ZLINE(num3, 10f)) * 1.5f;
					if (num4 > 0f)
					{
						Tx.need_character_pos_check = true;
						float num5 = (aver[this.pre_ver_i + 3].x - aver[this.pre_ver_i].x) * 0.5f * num4;
						float num6 = (aver[this.pre_ver_i + 1].y - aver[this.pre_ver_i].y) * 0.5f * num4;
						for (int i = 0; i < num; i++)
						{
							AIM aim = CAim.get_clockwiseN(AIM.BL, 2 * i);
							Vector3 vector = aver[i + this.pre_ver_i];
							vector.x += (float)CAim._XD(aim, 1) * num5;
							vector.y += (float)CAim._YD(aim, 1) * num6 + shifty;
							vertexArray[i + this.pre_ver_i] = vector;
						}
						Tx.need_mesh_update = true;
						return;
					}
					break;
				}
				case EMOT.JOY:
				{
					Tx.need_character_pos_check = true;
					float num3 = (float)(IN.totalframe + char_index * 2);
					num2 += 0.046875f * X.Cos(num3 / 36f * 3.1415927f * 2f);
					shifty += 0.046875f * X.Sin(num3 / 36f * 3.1415927f * 2f);
					break;
				}
				case EMOT.SCARY:
				{
					Tx.need_character_pos_check = true;
					float num3 = (float)(IN.totalframe + char_index * 2);
					num2 += 0.015625f * X.Cos(num3 / 0.27f * 3.1415927f * 2f);
					shifty += 0.015625f * X.Sin(num3 / 0.18f * 3.1415927f * 2f);
					break;
				}
				case EMOT.CRY:
				{
					Tx.need_character_pos_check = true;
					float num3 = (float)(IN.totalframe + char_index * 2);
					shifty += 0.015625f * (1.5f + 1.9f * X.Sin((num3 + 9.8f * (float)char_index) / 17f * 3.1415927f * 2f));
					break;
				}
				case EMOT.VIB:
				{
					Tx.need_character_pos_check = true;
					float num3 = (float)IN.totalframe;
					shifty += 0.046875f * X.Sin(num3 / 36f * 3.1415927f * 2f);
					break;
				}
				}
				for (int j = this.pre_ver_i; j < this.last_ver_i; j++)
				{
					Vector3 vector = aver[j];
					vector.x += num2;
					vector.y += shifty;
					vertexArray[j] = vector;
				}
			}

			public readonly int pre_ver_i;

			public readonly int last_ver_i;

			public readonly int char_index;

			public readonly int wait;

			public bool noise_attached;

			public bool use_default_emot;
		}

		private class ColorChangePos
		{
			public float time_from_shown(int char_index, int base_t)
			{
				if (this.startt > 0)
				{
					return (float)base_t - ((float)(char_index - this.index) * 1.5f * this.char_timescale + (float)this.startt);
				}
				return 128f;
			}

			public bool MulA(ref Color32 C, int char_index, int base_t, float mul_alpha, out bool fine_col_whole)
			{
				fine_col_whole = false;
				float num = this.time_from_shown(char_index, base_t);
				if (base_t > 0 && this.startt > 0)
				{
					EMOT emot = this.emot;
					if (emot != EMOT.PLEASURE)
					{
						if (emot == EMOT.FADEIN)
						{
							mul_alpha *= X.ZLINE(num, 20f);
							if (num <= 25f)
							{
								fine_col_whole = true;
							}
						}
					}
					else
					{
						mul_alpha *= X.ZLINE(num, 10f);
						if (num <= 15f)
						{
							fine_col_whole = true;
						}
					}
				}
				C.a = (byte)((float)(C.a * ((num >= 0f) ? 1 : 0)) * mul_alpha);
				return C.a > 0;
			}

			public EMOT emot;

			public Color32 Col;

			public float char_timescale = 1f;

			public int startt;

			public int index;
		}
	}
}

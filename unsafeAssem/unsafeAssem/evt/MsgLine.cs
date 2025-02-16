using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace evt
{
	public class MsgLine : IFontStorageListener
	{
		public MsgLine(MSG Msg, MsgLine _pMsgLine = null)
		{
			if (MsgLine.Storage == null)
			{
				MsgLine.Storage = MTRX.OFontStorage[TX.getDefaultFont()];
			}
			this.ParMsg = Msg;
			this.MMRD = ((Msg != null) ? Msg.get_MMRD() : null);
			this.MdChar = this.getMeshDrawer(MsgLine.Storage.getMaterial(0U, -1, false));
			if (this.MdChar != null)
			{
				this.Gob = this.MMRD.GetGob(this.MdChar);
			}
			if (_pMsgLine != null)
			{
				this.wait_by_char = _pMsgLine.wait_by_char;
				this.emotion = _pMsgLine.emotion;
				this.charindex = _pMsgLine.charindex;
				this.showt = _pMsgLine.showt;
				this.color = _pMsgLine.color;
				this.bold = _pMsgLine.bold;
				this.zoom = _pMsgLine.zoom;
				this.default_color = _pMsgLine.default_color;
			}
			else
			{
				this.resetValues();
			}
			this.mChars = new MsgChar[64];
			this.charcnt = 0;
			this.logtext = "";
			this.msx = 0f;
		}

		public static void fineFontStorage(MsgLine[] ALine, int line_max, string req_str)
		{
			MFont defaultFont = TX.getDefaultFont();
			if (defaultFont != MsgLine.Storage.TargetFont)
			{
				for (int i = 0; i < line_max; i++)
				{
					MsgLine.Storage.Rem(ALine[i]);
				}
				MsgLine.Storage = MTRX.OFontStorage[defaultFont];
				if (TX.valid(req_str))
				{
					MsgLine.Storage.TargetFont.Target.RequestCharactersInTexture(req_str);
				}
				for (int j = 0; j < line_max; j++)
				{
					MsgLine msgLine = ALine[j];
					if (msgLine.MMRD != null && msgLine.MdChar != null)
					{
						msgLine.MMRD.setMaterial(msgLine.MdChar, MsgLine.Storage.getMaterial(0U, -1, false), false);
					}
					msgLine.activate();
				}
				return;
			}
			if (TX.valid(req_str))
			{
				MsgLine.Storage.TargetFont.Target.RequestCharactersInTexture(req_str);
			}
		}

		public MsgLine activate()
		{
			MsgLine.Storage.Add(this);
			return this;
		}

		public MeshDrawer getMeshDrawer(Material Mtr)
		{
			if (this.MMRD == null)
			{
				return null;
			}
			int length = this.MMRD.getLength();
			for (int i = 0; i < length; i++)
			{
				MeshRenderer meshRenderer = this.MMRD.GetMeshRenderer(i);
				if (!meshRenderer.enabled)
				{
					MeshDrawer meshDrawer = this.MMRD.Get(i);
					if (meshDrawer.getMaterial() == Mtr)
					{
						meshRenderer.enabled = true;
						return meshDrawer;
					}
				}
			}
			return this.MMRD.Make(Mtr);
		}

		public MsgLine noMakeMode()
		{
			this.mChars = null;
			return this;
		}

		public MsgLine resetValues()
		{
			this.wait_by_char = 2f;
			this.emotion = EMOT.NORMAL;
			this.charindex = 0;
			this.showt = this.ParMsg.firstShowTime;
			this.color = MSG.AmsgColors[0];
			this.zoom = 0;
			this.bold = false;
			this.msx = 0f;
			this.logtext = "";
			return this;
		}

		public static string logColorTag(Color32 _color, bool bold = false, bool closing = false)
		{
			return "";
		}

		public void finalize()
		{
			if (this.msx == 0f)
			{
				this.all_shown = true;
			}
		}

		public string setText(string str)
		{
			this.logtext = MsgLine._setText(ref str, this);
			MsgLine.Storage.fine_text = 2;
			return str;
		}

		internal static string _setText(ref string str, MsgLine Li)
		{
			bool flag = false;
			Color32 color = MSG.AmsgColors[0];
			bool flag2 = false;
			if (Li != null)
			{
				Li.all_shown = false;
				color = Li.color;
				flag2 = Li.bold;
			}
			string text = MsgLine.logColorTag(color, flag2, false);
			int i = 0;
			int num = str.Length - 1;
			char[] array = str.ToCharArray();
			while (i <= num)
			{
				char c = array[i];
				if (c == '\\')
				{
					if (++i > num)
					{
						str = "";
						goto IL_049E;
					}
					c = array[i];
					if (c != '\\')
					{
						flag = !flag;
					}
				}
				int num2 = i;
				if (!flag)
				{
					if (i < num)
					{
						if (Li != null)
						{
							if (c == 'w')
							{
								Li.showt += (float)X.NmI(array[i + 1].ToString(), 0, false, false);
								i += 2;
							}
							else if (c == 'W')
							{
								Li.showt += (float)(X.NmI(array[i + 1].ToString(), 0, false, false) * 10);
								i += 2;
							}
							else if (c == 'E')
							{
								Li.emotion = (EMOT)X.NmI(array[i + 1].ToString(), 0, false, false);
								i += 2;
							}
							else if (c == 'S')
							{
								Li.wait_by_char = (float)X.NmI(array[i + 1].ToString(), 0, false, false);
								Li.wait_by_char = ((Li.wait_by_char == 0f) ? 0f : (2f / Li.wait_by_char));
								i += 2;
							}
							else if (c == 'M' && Li.commandListener != null)
							{
								Li.commandListener.set_execute_time(X.NmI(array[i + 1].ToString(), 0, false, false), (int)Li.showt);
								i += 2;
							}
							else if (c == 'Z')
							{
								Li.zoom = ((array[i + 1] == 'L') ? 1 : ((array[i + 1] == 'S') ? (-1) : 0));
								i += 2;
							}
							else if ((c == 'I' || c == '&' || c == '%') && array[i + 1] == '{')
							{
								char c2 = c;
								string text2 = "";
								int j;
								for (j = i + 2; j <= num; j++)
								{
									char c3 = array[j];
									if (c3 == '}')
									{
										break;
									}
									text2 += c3.ToString();
								}
								if (c2 == 'I' || c2 != '%')
								{
								}
								i = j + 1;
							}
							else if (c == 's')
							{
								Li.wait_by_char = X.Nm(array[i + 1].ToString(), 2f, true);
								i += 2;
							}
							if (i > num2)
							{
								continue;
							}
						}
						if (c == 'C' || c == 'c')
						{
							int num3 = X.NmI(array[i + 1].ToString(), 0, false, false);
							Color32 color2 = ((Li != null) ? Li.default_color : MSG.AmsgColors[0]);
							color = ((num3 == 0) ? color2 : (X.BTW(0f, (float)num3, (float)MSG.AmsgColors.Length) ? MSG.AmsgColors[num3] : color2));
							if (Li != null)
							{
								Li.color = color;
							}
							text += MsgLine.logColorTag(color, flag2, true);
							i += 2;
						}
						else if (c == 'B' && "01".IndexOf(array[i + 1]) >= 0)
						{
							bool flag3 = flag2;
							flag2 = array[i + 1] == '1';
							if (Li != null)
							{
								Li.bold = flag2;
							}
							if (flag3 != flag2)
							{
								text += (flag2 ? "<b>" : "</b>");
							}
							i += 2;
						}
					}
					if (i > num2)
					{
						continue;
					}
				}
				if (c == '\n' || c == '\r')
				{
					if (Li == null)
					{
						text += "<br/>\n";
						i++;
						continue;
					}
					str = TX.slice(str, i + 1, str.Length);
				}
				else
				{
					if (c == '\u3000' || c == '\t')
					{
						if (Li != null)
						{
							Li.msx += 18f;
							text += "\u3000";
						}
						else
						{
							text += c.ToString();
						}
						i++;
						continue;
					}
					if (c == ' ')
					{
						if (Li != null)
						{
							Li.msx += 14.400001f;
							text += " ";
						}
						else
						{
							text += c.ToString();
						}
						i++;
						continue;
					}
					if (Li != null)
					{
						Li.characterPush(c, null);
					}
					text += c.ToString();
					i++;
					continue;
				}
				IL_049E:
				if (flag2)
				{
					text += "</b>";
				}
				if (Li != null)
				{
					Li.color = color;
					Li.bold = flag2;
					Li.logtext += text;
					Li.finalize();
				}
				return text;
			}
			str = "";
			goto IL_049E;
		}

		public void getStringForListener(STB Stb)
		{
			Stb += this.logtext;
		}

		public void characterPush(char _char, PxlMeshDrawer _B = null)
		{
			if (this.mChars != null)
			{
				MeshDrawer meshDrawer;
				if (_B != null)
				{
					if (this.MdImage == null)
					{
						meshDrawer = (this.MdImage = this.getMeshDrawer(MTRX.getMI(_B.SourceFrame).getMtr(BLEND.NORMAL, -1)));
						if (this.MdImage == null)
						{
							return;
						}
						this.GobImage = this.MMRD.GetGob(this.MdImage);
						this.GobImage.transform.localPosition = this.Gob.transform.localPosition;
					}
					else
					{
						meshDrawer = this.MdImage;
					}
				}
				else
				{
					meshDrawer = this.MdChar;
				}
				MsgChar msgChar = new MsgChar(this, meshDrawer, _char, this.msx0 + this.msx, 0f, this.color, this.charindex, (int)this.showt, this.emotion, (float)this.zoom, _B, 0);
				X.pushToEmptyS<MsgChar>(ref this.mChars, msgChar, ref this.charcnt, 16);
				this.pre_is_small = msgChar.is_small;
				this.msx += msgChar.sizew;
			}
			this.showt += this.wait_by_char;
			if (this.zoom > 0)
			{
				this.large_line = true;
			}
			this.charindex++;
		}

		public MsgLine destruct()
		{
			if (this.MdChar != null)
			{
				this.MdChar.clear(false, false);
				this.MMRD.GetMeshRenderer(this.MdChar).enabled = false;
				this.MdChar = null;
			}
			if (this.MdImage != null)
			{
				this.MdImage.clear(false, false);
				this.MMRD.GetMeshRenderer(this.MdImage).enabled = false;
				this.MdImage = null;
			}
			this.mChars = null;
			this.logtext = "";
			MsgLine.Storage.Rem(this);
			return null;
		}

		public bool runDraw(int fcnt = 1, bool can_draw = true)
		{
			if (this.mChars == null)
			{
				return true;
			}
			for (int i = 0; i < this.charcnt; i++)
			{
				this.mChars[i].runDraw(fcnt, can_draw);
			}
			if (this.showt >= 0f)
			{
				this.showt -= (float)fcnt;
				if (this.showt <= 0f)
				{
					this.all_shown = true;
				}
			}
			if (this.remake_mesh_flag > 0U)
			{
				if ((this.remake_mesh_flag & 1U) > 0U)
				{
					this.MdChar.updateForMeshRenderer(true);
				}
				if ((this.remake_mesh_flag & 2U) > 0U)
				{
					this.MdImage.updateForMeshRenderer(true);
				}
				this.remake_mesh_flag = 0U;
			}
			return this.all_shown;
		}

		public void charsAllShow()
		{
			for (int i = 0; i < this.charcnt; i++)
			{
				this.mChars[i].showImmediate();
			}
			this.all_shown = true;
			this.showt = 0f;
		}

		public int showFirstChar(int first_wait = 0)
		{
			for (int i = 0; i < this.charcnt; i++)
			{
				if (this.mChars[i].showt >= 0)
				{
					return -1;
				}
				if (first_wait == 0)
				{
					first_wait = -this.mChars[i].showt;
				}
				this.mChars[i].showt += first_wait;
			}
			return first_wait;
		}

		public void entryMesh()
		{
			this.remakeCharMesh(true);
		}

		public void remakeCharMesh(bool rundraw = true)
		{
			bool flag = false;
			for (int i = 0; i < this.charcnt; i++)
			{
				MsgChar msgChar = this.mChars[i];
				if (!msgChar.visible)
				{
					break;
				}
				flag = (msgChar.mesh_remake_flag = true);
			}
			if (rundraw && flag)
			{
				this.runDraw(0, false);
			}
		}

		public void talkProgressSound()
		{
			if (this.ParMsg != null)
			{
				SND.Ui.play(this.ParMsg.talker_snd, false);
			}
		}

		public bool visible
		{
			get
			{
				return this.alpha_ >= 0f;
			}
			set
			{
				if (this.visible != value)
				{
					this.alpha_ = -1f - this.alpha_;
					this.remakeCharMesh(!this.visible);
				}
			}
		}

		public float alpha
		{
			get
			{
				if (this.alpha_ >= 0f)
				{
					return this.alpha_;
				}
				return -1f - this.alpha_;
			}
			set
			{
				this.alpha_ = ((this.alpha_ < 0f) ? (-1f - value) : value);
				this.remakeCharMesh(false);
			}
		}

		public float char_alpha
		{
			get
			{
				return X.Mx(0f, this.alpha_) * ((this.ParMsg != null) ? this.ParMsg.alpha : 1f);
			}
		}

		public float y
		{
			set
			{
				if (this.Gob == null)
				{
					return;
				}
				Vector3 localPosition = this.Gob.transform.localPosition;
				localPosition.y = value * 0.015625f;
				this.Gob.transform.localPosition = localPosition;
				if (this.GobImage != null)
				{
					this.GobImage.transform.localPosition = localPosition;
				}
			}
		}

		public static FontStorage Storage;

		private int t;

		public bool all_shown;

		private MsgChar[] mChars;

		private int charcnt;

		public int nest_count = 1;

		public bool large_line;

		private MSG ParMsg;

		private MultiMeshRenderer MMRD;

		private GameObject Gob;

		private MeshDrawer MdChar;

		private MeshDrawer MdImage;

		private GameObject GobImage;

		private float alpha_ = 1f;

		private float msx0 = -280f;

		public float wait_by_char;

		public EMOT emotion;

		public int charindex;

		public float showt;

		public Color32 color;

		public bool bold;

		public Color32 default_color = MSG.AmsgColors[0];

		public float msx;

		public EvMsgCommand commandListener;

		public string logtext = "";

		public int zoom;

		public uint remake_mesh_flag;

		public const uint REMAKE_CHAR = 1U;

		public const uint REMAKE_IMAGE = 2U;

		public bool pre_is_small;

		public const float WAIT_CHAR_DEF = 2f;
	}
}

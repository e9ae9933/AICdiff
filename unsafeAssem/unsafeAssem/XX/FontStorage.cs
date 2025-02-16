using System;
using System.Collections.Generic;
using System.Text;
using Better;
using UnityEngine;

namespace XX
{
	public class FontStorage
	{
		public FontStorage(MFont _TargetFont, string _letterspacing_script_name, int renderQueue = 3000)
		{
			if (FontStorage.EncodeUTF == null)
			{
				FontStorage.EncodeUTF = Encoding.UTF8;
			}
			this.letterspacing_script_name = _letterspacing_script_name;
			this.TargetFont = _TargetFont;
			this.AListener = new List<IFontStorageListener>();
			this.MtrNoBorder = MTRX.newMtr(MTRX.ShaderGDTFont);
			this.MtrNoBorder.renderQueue = renderQueue;
			this.MtrBorder = MTRX.newMtr(MTRX.ShaderGDTFontBorder8);
			this.MtrBorder.renderQueue = renderQueue;
			this.OMtr = new BDic<int, Material>();
			this.OMtrBorder = new BDic<int, Material>();
			if (this.TargetFont.isLoaded())
			{
				this.MFontInit();
			}
		}

		public MFont LoadMFont()
		{
			if (!this.TargetFont.isLoaded())
			{
				this.TargetFont.Load();
				this.MFontInit();
			}
			return this.TargetFont;
		}

		private void MFontInit()
		{
			this.MtrNoBorder.SetTexture("_MainTex", this.FontTexture);
			this.MtrBorder.SetTexture("_MainTex", this.FontTexture);
			Font target = this.TargetFont.Target;
			Font.textureRebuilt += this.OnFontTextureRebuilt;
			target.RequestCharactersInTexture("0123456789/=abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+*-.");
		}

		public void Add(IFontStorageListener Listener)
		{
			if (this.AListener.IndexOf(Listener) == -1)
			{
				this.AListener.Add(Listener);
			}
		}

		public int fine_text
		{
			get
			{
				return this.fine_text_;
			}
			set
			{
				if (this.fine_text == value)
				{
					return;
				}
				this.fine_text_ = ((this.fine_text_ == 0) ? value : X.Mn(this.fine_text_, value));
			}
		}

		public void clearListener()
		{
			this.AListener.Clear();
		}

		public void Fine()
		{
			this.fine_text_ = 2;
		}

		public bool FineExecute(out bool canceled, bool force = false)
		{
			this.LoadMFont();
			canceled = false;
			if (!force && this.fined_totalframe >= IN.totalframe)
			{
				return false;
			}
			this.fine_text_--;
			if (!force && this.fine_text >= 3)
			{
				return false;
			}
			this.fined_totalframe = 0;
			int count = this.AListener.Count;
			STB stb = TX.PopBld("0123456789/=abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+*-.", 0);
			for (int i = count - 1; i >= 0; i--)
			{
				IFontStorageListener fontStorageListener = this.AListener[i];
				if (fontStorageListener == null)
				{
					this.AListener.RemoveAt(i);
				}
				else
				{
					fontStorageListener.getStringForListener(stb);
					if (this.fined_totalframe > 0)
					{
						canceled = true;
						return true;
					}
				}
			}
			this.fined_totalframe = IN.totalframe;
			this.TargetFont.Target.RequestCharactersInTexture(stb.ToString());
			TX.ReleaseBld(stb);
			return true;
		}

		public CharacterInfo getConfusionRandom(ref char chr)
		{
			CharacterInfo[] characterInfo = this.TargetFont.Target.characterInfo;
			float num = X.XORSP();
			if (num <= 0.03125f)
			{
				chr = ' ';
				CharacterInfo characterInfo2;
				this.TargetFont.Target.GetCharacterInfo(' ', out characterInfo2);
				return characterInfo2;
			}
			if (FontStorage.EncodeUTF.GetByteCount(chr.ToString() ?? "") == 1)
			{
				chr = "0123456789/=abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+*-."[X.xors("0123456789/=abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+*-.".Length)];
				CharacterInfo characterInfo3;
				this.TargetFont.Target.GetCharacterInfo(' ', out characterInfo3);
				return characterInfo3;
			}
			int num2 = characterInfo.Length;
			CharacterInfo characterInfo4 = characterInfo[X.Mn(num2 - 1, (int)((float)num2 * ((num - 0.03125f) / 0.96875f)))];
			chr = Convert.ToChar(characterInfo4.index);
			return characterInfo4;
		}

		public void Rem(IFontStorageListener Listener)
		{
			int num = this.AListener.IndexOf(Listener);
			if (num >= 0)
			{
				this.AListener.RemoveAt(num);
			}
		}

		public Texture FontTexture
		{
			get
			{
				return this.TargetFont.material.GetTexture("_MainTex");
			}
		}

		public float def_size
		{
			get
			{
				return (float)this.TargetFont.fontSize;
			}
		}

		public float defaultRendererSize
		{
			get
			{
				if (this.defaultRendererSize_ <= 0f)
				{
					return (float)this.TargetFont.fontSize;
				}
				return this.defaultRendererSize_;
			}
		}

		public void OnFontTextureRebuilt(Font changedFont)
		{
			if (changedFont != this.TargetFont.Target)
			{
				return;
			}
			int count = this.AListener.Count;
			this.fine_text_ = 1;
			bool flag;
			this.FineExecute(out flag, true);
			this.fine_text_ = 0;
			if (flag)
			{
				return;
			}
			for (int i = 0; i < count; i++)
			{
				try
				{
					this.AListener[i].entryMesh();
				}
				catch
				{
				}
			}
		}

		public Material getMaterial(uint border_color, int stencil_ref = -1, bool set_color = false)
		{
			bool flag = border_color >= 16777216U;
			Material material = ((!flag) ? this.MtrNoBorder : this.MtrBorder);
			if (stencil_ref >= 0)
			{
				BDic<int, Material> bdic = ((!flag) ? this.OMtr : this.OMtrBorder);
				if (!bdic.TryGetValue(stencil_ref, out material))
				{
					material = (bdic[stencil_ref] = MTRX.newMtr(flag ? MTRX.ShaderGDTSTFontBorder8 : MTRX.ShaderGDTSTFont));
					material.renderQueue = this.MtrBorder.renderQueue;
					material.SetFloat("_StencilRef", (float)stencil_ref);
					material.SetFloat("_StencilComp", 3f);
				}
			}
			if (set_color && flag)
			{
				material.SetColor("_BorderColor", C32.d2c(border_color));
			}
			material.SetTexture("_MainTex", this.TargetFont.material.GetTexture("_MainTex"));
			return material;
		}

		public float getLineSWidth(float drawx, float pre_margin, float size)
		{
			return drawx - -size * this.xratio * this.xmargin - X.Mx(pre_margin, 0f);
		}

		public virtual float getLetterSpaceRatio(char chr, bool monospace, out float xratio_char)
		{
			FontStorage.Achr_buf[0] = chr;
			xratio_char = ((FontStorage.EncodeUTF.GetByteCount(FontStorage.Achr_buf, 0, 1) > 1) ? this.xratio : this.xratio_1byte);
			return xratio_char * ((!monospace && this.Oletter_spacing != null) ? X.GetS<char, float>(this.Oletter_spacing, chr, 1f) : 1f);
		}

		protected virtual string getLetterSpaceScript()
		{
			if (TX.noe(this.letterspacing_script_name))
			{
				return null;
			}
			return NKT.readStreamingText(this.letterspacing_script_name + ".txt", false);
		}

		public void reloadLetterSpacingScript()
		{
			string letterSpaceScript = this.getLetterSpaceScript();
			if (TX.noe(letterSpaceScript))
			{
				return;
			}
			CsvReader csvReader = new CsvReader(letterSpaceScript, CsvReader.RegSpace, true);
			this.Oletter_spacing = new BDic<char, float>();
			while (csvReader.read())
			{
				string cmd = csvReader.cmd;
				if (cmd != null)
				{
					if (cmd == "%%%SPACE")
					{
						this.Oletter_spacing[' '] = csvReader.Nm(1, 0f);
						continue;
					}
					if (cmd == "%%%TAB")
					{
						this.Oletter_spacing['\t'] = csvReader.Nm(1, 0f);
						continue;
					}
					if (cmd == "%%%QUOTE")
					{
						this.Oletter_spacing['\''] = csvReader.Nm(1, 0f);
						continue;
					}
				}
				char[] array = csvReader.cmd.ToCharArray();
				int num = array.Length;
				float num2 = csvReader.Nm(1, 0f);
				for (int i = 0; i < num; i++)
				{
					this.Oletter_spacing[array[i]] = num2;
				}
			}
		}

		public readonly MFont TargetFont;

		private List<IFontStorageListener> AListener;

		private BDic<char, float> Oletter_spacing;

		private BDic<int, Material> OMtrBorder;

		private BDic<int, Material> OMtr;

		public Material MtrNoBorder;

		public Material MtrBorder;

		private int fine_text_ = 2;

		private int fined_totalframe = -1;

		public int margin = 2;

		public float xmargin;

		public float ymargin = 0.55f;

		public float base_height = 15f;

		public float yshift_to_baseline = -0.88f;

		public float xratio = 1f;

		public float xratio_1byte = 0.7f;

		protected float defaultRendererSize_;

		protected string letterspacing_script_name;

		private static char[] Achr_buf = new char[1];

		public static Encoding EncodeUTF;

		private const string basic_string = "0123456789/=abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+*-.";
	}
}

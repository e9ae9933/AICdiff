using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class FillBlock : MonoBehaviourAutoRun, IDesignerBlock, IPauseable, IAlphaSetable, IVariableObject, IFontStorageListener, IValotileSetable
	{
		public void Start()
		{
			this.StartFb(null, null, false);
		}

		public virtual void StartFb(string text, STB Stb, bool text_html_mode)
		{
			if (this.initted)
			{
				return;
			}
			this.initted = true;
			if (this.Mtr == null)
			{
				this.replaceMaterial(null);
			}
			if (this.widthPixel > 0f || this.text_auto_condense || this.text_auto_wrap)
			{
				this.fix_width = true;
			}
			if (this.heightPixel > 0f)
			{
				this.fix_height = true;
			}
			if (this.use_mesh_renderer)
			{
				this.Md = MeshDrawer.prepareMeshRenderer(base.gameObject, this.Mtr, 0f, -1, null, false, false);
				this.Mrd = base.GetComponent<MeshRenderer>();
			}
			if (text != "" || Stb != null)
			{
				GameObject gameObject = IN.CreateGob(this, "-TextRenderer");
				this.Tm = gameObject.AddComponent<TextRenderer>();
				if (this.TargetFont != null)
				{
					this.Tm.TargetFont = this.TargetFont;
				}
				this.Tm.Start().Align(this.alignx).AlignY(this.aligny)
					.ImgAlignY(this.image_aligny)
					.Col(this.TxCol)
					.Size(this.size)
					.Alpha(this.alpha_)
					.StencilRef(this.stencil_ref)
					.BorderCol(this.TxBorderCol);
				if (this.lineSpacing_ > 0f)
				{
					this.Tm.LineSpacing(this.lineSpacing_);
				}
				if (this.letterSpacing > 0f)
				{
					this.Tm.LetterSpacing(this.letterSpacing);
				}
				this.Tm.Aword_splitter = this.Aword_splitter;
				this.Tm.getStorage().Add(this);
				this.Tm.html_mode = text_html_mode;
				this.Tm.do_not_error_unknown_tag = this.do_not_error_unknown_tag;
				if (this.text_auto_condense)
				{
					this.Tm.auto_condense = this.text_auto_condense;
				}
				this.Tm.effect_confusion = this.effect_confusion_;
				if (Stb != null)
				{
					this.Tm.Txt(Stb);
				}
				else
				{
					this.Tm.Txt(text);
				}
				if (this.fix_width)
				{
					this.Tm.auto_condense = true;
					this.Tm.max_swidth_px = this.get_swidth_px() - this.margin_x * 2f;
					if (this.text_auto_wrap)
					{
						this.Tm.auto_wrap = true;
					}
					this.Tm.Redraw(true);
				}
				else
				{
					this.Tm.MustRedraw();
				}
				this.fineWH(true);
				this.fineTxTransform();
			}
			if (this.use_valotile_)
			{
				this.use_valotile_ = false;
				this.use_valotile = true;
			}
			this.redraw(false);
		}

		public void getStringForListener(STB Stb)
		{
		}

		public void entryMesh()
		{
			if (this.Tm != null && !this.fix_width)
			{
				this.fineWH(true);
				this.redraw(false);
			}
		}

		public virtual bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				this.use_valotile_ = value;
				if (this.Tm != null)
				{
					this.Tm.use_valotile = value;
				}
				if (this.Md != null)
				{
					if (this.Valot == null)
					{
						if (!value)
						{
							return;
						}
						this.Valot = ValotileRenderer.Create(this.Md, this.Mrd, true);
					}
					this.Valot.enabled = value;
				}
			}
		}

		public string text_content
		{
			get
			{
				if (!(this.Tm != null))
				{
					return null;
				}
				return this.Tm.text_content;
			}
			set
			{
				this.Txt(value);
			}
		}

		public bool textIs(string str)
		{
			return this.Tm != null && this.Tm.textIs(str);
		}

		public bool textIs(STB Stb)
		{
			return this.Tm != null && this.Tm.textIs(Stb);
		}

		public FillBlock Txt(string text)
		{
			if (this.Tm == null || this.Tm.textIs(text))
			{
				return this;
			}
			this.Tm.Txt(text);
			this.textModifiedAfter();
			return this;
		}

		public FillBlock Txt(STB Stb)
		{
			if (this.Tm == null || this.Tm.textIs(Stb))
			{
				return this;
			}
			this.Tm.Txt(Stb);
			this.textModifiedAfter();
			return this;
		}

		private void textModifiedAfter()
		{
			if (!this.fix_width || !this.fix_height)
			{
				this.fineWH(false);
				this.redraw(true);
				this.fineTxTransform();
			}
			else if (this.alloc_extending)
			{
				this.fineTxTransform();
			}
			if (this.af >= 10f)
			{
				this.af_retex = 0;
				this.Tm.Alpha(0f);
			}
		}

		public void extendHeight()
		{
			if (this.Tm != null && this.fix_height)
			{
				this.heightPixel = X.Mx(this.Tm.get_sheight_px(), this.heightPixel);
			}
		}

		public bool text_italic
		{
			get
			{
				return this.Tm != null && this.Tm.italic;
			}
			set
			{
				if (this.Tm != null)
				{
					this.Tm.italic = value;
				}
			}
		}

		public bool text_bold
		{
			get
			{
				return this.Tm != null && this.Tm.bold;
			}
			set
			{
				if (this.Tm != null)
				{
					this.Tm.bold = value;
				}
			}
		}

		public Color32 TextColor
		{
			get
			{
				if (!(this.Tm != null))
				{
					return this.TxCol;
				}
				return this.Tm.TextColor;
			}
			set
			{
				this.TxCol = value;
				if (this.Tm != null)
				{
					this.Tm.TextColor = this.TxCol;
				}
			}
		}

		protected override bool runIRD(float fcnt)
		{
			bool flag = X.D || this.af <= 1f;
			this.af += fcnt;
			if (this.redraw_flag && flag)
			{
				this.redraw(false);
			}
			if (this.Tm != null && this.af_retex < 14 && X.D)
			{
				this.af_retex += X.AF;
				this.Tm.Alpha(this.text_alpha * this.text_alpha_multiple_);
			}
			return true;
		}

		public float text_alpha_raw
		{
			get
			{
				return X.ZLINE((float)this.af_retex, 14f);
			}
		}

		public float text_alpha
		{
			get
			{
				return this.alpha_ * X.NI(0.5f, 1f, this.text_alpha_raw);
			}
			set
			{
				if (value <= 0f)
				{
					this.af_retex = 0;
				}
				if (value >= 1f && this.af_retex < 14)
				{
					this.af_retex = 13;
				}
			}
		}

		public float text_alpha_multiple
		{
			get
			{
				return this.text_alpha_multiple_;
			}
			set
			{
				if (this.text_alpha_multiple == value)
				{
					return;
				}
				this.text_alpha_multiple_ = value;
				if (this.Tm != null)
				{
					this.Tm.Alpha(this.text_alpha * this.text_alpha_multiple_);
				}
			}
		}

		protected virtual bool use_mesh_renderer
		{
			get
			{
				return this.Col.a > 0;
			}
		}

		public void redraw(bool force = false)
		{
			this.redraw_flag = false;
			if (!this.initted || (!base.enabled && !force))
			{
				return;
			}
			this.redrawMesh();
			if (this.Tm != null)
			{
				this.Tm.Alpha(this.text_alpha * this.text_alpha_multiple_);
			}
		}

		protected virtual Material replaceMaterial(Material _Mtr = null)
		{
			if (_Mtr == null)
			{
				_Mtr = MTRX.getMtr(BLEND.NORMAL, this.stencil_ref);
			}
			this.Mtr = _Mtr;
			if (this.Mrd != null && this.Md != null)
			{
				this.Md.setMaterial(this.Mtr, false);
				if (this.Md.hasMultipleTriangle())
				{
					this.Md.connectRendererToTriMulti(this.Mrd);
				}
				else
				{
					this.Mrd.sharedMaterials = this.Md.getMaterialArray(false);
				}
			}
			return this.Mtr;
		}

		public MeshDrawer getMeshDrawer()
		{
			return this.Md;
		}

		protected virtual void redrawMesh()
		{
			if (this.Md == null)
			{
				return;
			}
			this.Md.Col = this.Col;
			this.Md.Col.a = (byte)((float)this.Md.Col.a * this.alpha_);
			if (this.radius > 0f)
			{
				this.Md.KadomaruRect(0f, 0f, this.widthPixel, this.heightPixel, this.radius, 0f, false, 0f, 0f, false);
			}
			else
			{
				this.Md.Box(0f, 0f, this.widthPixel, this.heightPixel, 0f, false);
			}
			this.Md.updateForMeshRenderer(false);
		}

		public void fineWH(bool redrawing = true)
		{
			if (this.Tm != null)
			{
				if (this.widthPixel <= 0f || !this.fix_width)
				{
					this.widthPixel = this.Tm.get_swidth_px() + this.margin_x * 2f;
				}
				if (this.heightPixel <= 0f || !this.fix_height)
				{
					this.heightPixel = this.Tm.get_sheight_px() + this.margin_y * 2f;
				}
			}
			if (redrawing && this.Col.a > 0)
			{
				this.redraw(true);
			}
		}

		public void fineTxTransform()
		{
			Transform transform = this.Tm.transform;
			Vector3 localPosition = transform.localPosition;
			if (this.alignx != ALIGN.CENTER)
			{
				localPosition.x = (((!this.alloc_extending && this.fix_width) ? this.widthPixel : this.get_swidth_px()) / 2f - this.margin_x) * 0.015625f * (float)X.MPF(this.alignx == ALIGN.RIGHT);
			}
			else
			{
				localPosition.x = 0f;
			}
			if (this.aligny != ALIGNY.MIDDLE)
			{
				localPosition.y = (((!this.alloc_extending && this.fix_height) ? this.heightPixel : this.get_sheight_px()) / 2f - this.margin_y) * 0.015625f * (float)X.MPF(this.aligny == ALIGNY.TOP);
			}
			else
			{
				localPosition.y = 0f;
			}
			transform.localPosition = localPosition;
		}

		public float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (this.alpha_ == value)
				{
					return;
				}
				this.alpha_ = value;
				this.redraw(false);
			}
		}

		public Color32 TxCol
		{
			get
			{
				return this.TxCol_;
			}
			set
			{
				this.TxCol_ = value;
				if (this.Tm != null)
				{
					this.Tm.Col(value);
				}
			}
		}

		public bool effect_confusion
		{
			get
			{
				return this.effect_confusion_;
			}
			set
			{
				if (this.effect_confusion_ == value)
				{
					return;
				}
				this.effect_confusion_ = value;
				if (this.Tm != null)
				{
					this.Tm.effect_confusion = value;
				}
			}
		}

		public float lineSpacing
		{
			get
			{
				return this.lineSpacing_;
			}
			set
			{
				if (this.lineSpacing_ == value)
				{
					return;
				}
				this.lineSpacing_ = value;
				if (this.Tm != null)
				{
					this.Tm.LineSpacing(this.lineSpacing_);
				}
			}
		}

		public float size
		{
			get
			{
				return this.size_;
			}
			set
			{
				if (this.size_ == value)
				{
					return;
				}
				this.size_ = value;
				if (this.Tm != null)
				{
					this.Tm.Size(this.size_);
				}
			}
		}

		public ALIGN alignx
		{
			get
			{
				return this.alignx_;
			}
			set
			{
				if (this.alignx == value)
				{
					return;
				}
				this.alignx_ = value;
				if (this.Tm != null)
				{
					this.Tm.alignx = this.alignx_;
					this.fineTxTransform();
				}
			}
		}

		public ALIGNY aligny
		{
			get
			{
				return this.aligny_;
			}
			set
			{
				if (this.aligny == value)
				{
					return;
				}
				this.aligny_ = value;
				if (this.Tm != null)
				{
					this.Tm.aligny = this.aligny_;
					this.fineTxTransform();
				}
			}
		}

		public void setAlpha(float value)
		{
			this.alpha = value;
		}

		public float get_swidth_px()
		{
			if (!this.fix_width)
			{
				return X.Mx((this.Tm != null) ? (this.Tm.get_swidth_px() + this.margin_x * 2f) : 0f, this.widthPixel);
			}
			return this.widthPixel;
		}

		public float get_text_swidth_px(bool raw = false)
		{
			if (!(this.Tm != null))
			{
				return 0f;
			}
			return this.Tm.get_swidth_px() + (raw ? 0f : (this.margin_x * 2f));
		}

		public float get_sheight_px()
		{
			return X.Mx((this.Tm != null) ? (this.Tm.get_sheight_px() + this.margin_y * 2f) : 0f, this.heightPixel);
		}

		public Transform getTransform()
		{
			return base.transform;
		}

		public void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
		}

		public string getValueString()
		{
			if (!(this.Tm != null))
			{
				return null;
			}
			return this.Tm.text_content;
		}

		public void setValue(string s)
		{
			this.Txt(s);
		}

		public GameObject getGob()
		{
			return base.gameObject;
		}

		public override void OnEnable()
		{
			base.OnEnable();
			if (this.Tm != null)
			{
				this.Tm.enabled = true;
			}
			if (this.Mrd != null)
			{
				this.Mrd.enabled = true;
			}
		}

		public override void OnDestroy()
		{
			if (this.Md != null)
			{
				this.Md.destruct();
			}
			this.Md = null;
			base.OnDestroy();
		}

		public override void OnDisable()
		{
			base.OnDisable();
			if (this.Tm != null)
			{
				this.Tm.enabled = false;
			}
			if (this.Mrd != null)
			{
				this.Mrd.enabled = false;
			}
		}

		public MeshRenderer getMeshRenderer()
		{
			return this.Mrd;
		}

		public void Pause()
		{
		}

		public void Resume()
		{
		}

		protected bool initted;

		public Color32 Col;

		protected Color32 TxCol_;

		public Color32 TxBorderCol;

		public float widthPixel;

		public float heightPixel;

		private float size_ = 16f;

		public MFont TargetFont;

		public bool fix_width;

		public bool fix_height;

		public bool alloc_extending;

		public bool text_auto_condense;

		public bool text_auto_wrap;

		public char[] Aword_splitter;

		private float lineSpacing_ = -1f;

		public float letterSpacing = 0.98f;

		public float radius;

		public bool do_not_error_unknown_tag;

		public int stencil_ref = -1;

		private ALIGN alignx_;

		private ALIGNY aligny_;

		public ALIGNY image_aligny;

		public Material Mtr;

		private TextRenderer Tm;

		protected MeshDrawer Md;

		protected MeshRenderer Mrd;

		private float alpha_ = 1f;

		private float text_alpha_multiple_ = 1f;

		public float margin_x = 5f;

		public float margin_y = 3f;

		private bool use_collider_ = true;

		private bool use_valotile_;

		protected ValotileRenderer Valot;

		private bool effect_confusion_;

		public bool redraw_flag;

		private float af;

		private int af_retex = 14;

		private const int RETEX_MAXT = 14;
	}
}

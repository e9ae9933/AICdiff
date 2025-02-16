using System;
using UnityEngine;

namespace XX
{
	public abstract class ButtonSkin : IAlphaSetable, IValotileSetable
	{
		public ButtonSkin(aBtn _B, float _w = 0f, float _h = 0f)
		{
			this.B = _B;
			this.Gob = _B.gameObject;
			this.RectTrs = this.Gob.AddComponent<RectTransform>();
			this.fine_flag = true;
			this.w = _w;
			this.h = _h;
		}

		public virtual bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				if (this.use_valotile == value)
				{
					return;
				}
				this.use_valotile_ = value;
				ValotileRenderer.RecheckUse(this.MyMesh, this.MyMeshRenderer, ref this.MyValot, value);
				ValotileRenderer.RecheckUse(this.MMRD, value);
			}
		}

		public virtual void destruct()
		{
			if (this.MyMesh != null)
			{
				this.MyMesh.destruct();
			}
		}

		public virtual ButtonSkin WHPx(float _wpx, float _hpx)
		{
			this.w = ((_wpx != 0f) ? (_wpx * 0.015625f) : _wpx);
			this.h = ((_hpx != 0f) ? (_hpx * 0.015625f) : _hpx);
			this.fine_flag = true;
			return this;
		}

		protected MeshDrawer makeMesh(Material Mtr = null)
		{
			int container_stencil_ref = this.container_stencil_ref;
			if (this.MyMesh == null)
			{
				if (container_stencil_ref >= 0 && (Mtr == MTRX.MtrMeshNormal || Mtr == null))
				{
					Mtr = MTRX.getMtr(BLEND.NORMALST, container_stencil_ref);
				}
				this.MyMesh = MeshDrawer.prepareMeshRenderer(this.Gob, Mtr, 0f, -1, null, false, false);
				this.MyMeshRenderer = this.Gob.GetComponent<MeshRenderer>();
				return this.MyMesh;
			}
			if (this.MMRD == null)
			{
				this.MMRD = this.Gob.AddComponent<MultiMeshRenderer>();
				this.MMRD.base_z = -1E-05f;
				this.MMRD.z_apply_to_gameobject = true;
				this.MMRD.use_cache = false;
				this.MMRD.stencil_ref = this.container_stencil_ref;
			}
			return this.MMRD.Make(Mtr);
		}

		protected MeshDrawer makeMesh(BLEND blend, MImage Img)
		{
			int container_stencil_ref = this.container_stencil_ref;
			if (container_stencil_ref >= 0 && blend == BLEND.NORMAL)
			{
				blend = BLEND.NORMALST;
			}
			if (Img == null)
			{
				return this.makeMesh(MTRX.getMtr(blend, container_stencil_ref));
			}
			if (this.MMRD == null)
			{
				this.MMRD = this.Gob.AddComponent<MultiMeshRenderer>();
				this.MMRD.base_z = -1E-05f;
			}
			this.MMRD.stencil_ref = container_stencil_ref;
			return this.MMRD.Make(blend, Img);
		}

		public MeshRenderer getMeshRenderer(MeshDrawer _Md)
		{
			if (_Md == this.MyMesh)
			{
				return this.Gob.GetComponent<MeshRenderer>();
			}
			return this.MMRD.GetMeshRenderer(_Md);
		}

		public virtual ButtonSkin Fine()
		{
			if (((this.fine_continue_flags & 1U) == 0U || !this.isFocused()) && ((this.fine_continue_flags & 2U) == 0U || !this.isActive()) && ((this.fine_continue_flags & 4U) == 0U || !this.isPushed()) && ((this.fine_continue_flags & 8U) == 0U || !this.isChecked()) && ((this.fine_continue_flags & 16U) == 0U || !this.isNormal()))
			{
				this.fine_flag = false;
			}
			return this;
		}

		public virtual bool canClickable(Vector2 PosU)
		{
			return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), this.swidth * 0.015625f, this.sheight * 0.015625f);
		}

		public virtual void RenderToCamManual()
		{
			if (this.MyMesh != null)
			{
				ValotileRenderer.renderOneMd(this.MyMesh, null, null, null, false);
			}
			if (this.MMRD != null)
			{
				this.MMRD.RenderToCamManual();
			}
		}

		public virtual ButtonSkin setTitle(string str)
		{
			this.title = str;
			return this;
		}

		protected virtual bool makeDefaultTitleString(string str, ref MeshDrawer MdSpr, BLEND blnd = BLEND._MAX)
		{
			this.default_title_width = 0f;
			this.DefaultTitleChr = null;
			MeshRenderer meshRenderer = null;
			if (TX.noe(str))
			{
				if (MdSpr != null)
				{
					if (meshRenderer == null)
					{
						meshRenderer = this.MMRD.GetMeshRenderer(MdSpr);
					}
					if (meshRenderer != null)
					{
						meshRenderer.GetComponent<MeshFilter>().sharedMesh = null;
					}
				}
				return false;
			}
			STB stb = TX.PopBld(null, 0);
			stb.Add(str).ToUpper();
			BMListChars bmlistChars = (this.DefaultTitleChr = MTRX.ChrM);
			Material material = bmlistChars.MI.getMtr(blnd, -1);
			this.default_title_width = bmlistChars.DrawStringTo(null, stb, 0f, 0f, ALIGN.LEFT, ALIGNY.TOP, false, 0f, 0f, null);
			TX.ReleaseBld(stb);
			this.Vdefault_title_shift.Set(0f, 0f);
			if (MdSpr == null)
			{
				MdSpr = this.makeMesh(material);
			}
			meshRenderer = this.MMRD.GetMeshRenderer(MdSpr);
			meshRenderer.GetComponent<MeshFilter>().sharedMesh = MdSpr.getMesh();
			material = this.getTitleStringChrMaterial(blnd, bmlistChars, MdSpr);
			if (material != null)
			{
				meshRenderer.sharedMaterials = new Material[] { material };
				MdSpr.initForImg(bmlistChars.MI);
			}
			return true;
		}

		public TextRenderer MakeTx(string name)
		{
			TextRenderer textRenderer = IN.CreateGob(this.Gob, name).AddComponent<TextRenderer>();
			IN.setZ(textRenderer.gameObject.transform, -0.01f);
			textRenderer.BelongScroll = this.B.BelongScroll;
			return textRenderer;
		}

		public virtual float default_title_width
		{
			get
			{
				return this.default_title_width_;
			}
			set
			{
				if (this.default_title_width == value)
				{
					return;
				}
				this.default_title_width_ = value;
			}
		}

		protected virtual void DrawTitleSprite(MeshDrawer MdSpr, float x, float y, float scale = 1f)
		{
			if (this.DefaultTitleChr == null)
			{
				MdSpr.DrawScaleGraph(x, y, scale, scale, null);
				return;
			}
			STB stb = TX.PopBld(null, 0);
			aBtn.convert_eng_title(stb, this.title);
			this.DefaultTitleChr.DrawScaleStringTo(MdSpr, stb, x, y - (this.DefaultTitleChr.ch - 3f) * scale, scale, scale, ALIGN.CENTER, ALIGNY.BOTTOM, false, 0f, 0f, null);
			TX.ReleaseBld(stb);
		}

		protected virtual Material getTitleStringChrMaterial(BLEND blnd, BMListChars Chr, MeshDrawer Md)
		{
			if (blnd == BLEND._MAX)
			{
				blnd = BLEND.NORMALBORDER8;
			}
			return Chr.MI.getMtr(blnd, -1);
		}

		public virtual void setEnable(bool f)
		{
			if (this.MyMeshRenderer != null)
			{
				this.MyMeshRenderer.enabled = f;
			}
			if (this.MMRD != null)
			{
				if (f)
				{
					this.MMRD.activate();
					return;
				}
				this.MMRD.deactivate(false);
			}
		}

		public virtual void bindChanged(bool f)
		{
		}

		public virtual bool isEnable()
		{
			return this.MyMesh != null && this.MyMeshRenderer.enabled;
		}

		public ButtonSkin setLayer(int lay)
		{
			if (this.MMRD != null)
			{
				this.MMRD.setLayer(lay);
			}
			return this;
		}

		public aBtn getBtn()
		{
			return this.B;
		}

		public bool isActive()
		{
			return this.B.isActive();
		}

		public bool isPushed()
		{
			return this.B.isPushed();
		}

		public virtual bool isPushDown()
		{
			return this.B.isPushDown();
		}

		public bool isFocused()
		{
			return this.B.isFocused();
		}

		public bool isHoveredOrPushOut()
		{
			return this.B.isHoveredOrPushOut();
		}

		public bool isChecked()
		{
			return this.B.isChecked();
		}

		public bool isLocked()
		{
			return this.locked_only_skin || this.B.isLocked();
		}

		public bool isNormal()
		{
			return !this.B.isPushed() && !this.B.isHovered() && !this.B.isChecked();
		}

		public Vector3 getLocalPosFromContainer()
		{
			ScrollBox belongScroll = this.B.BelongScroll;
			if (belongScroll != null)
			{
				Vector3 vector = belongScroll.getShift();
				return this.B.transform.localPosition - vector;
			}
			return this.B.transform.localPosition;
		}

		public Vector3 local2global(Vector3 Loc, bool consider_scroll = true)
		{
			Vector3 vector = this.B.transform.TransformPoint(Loc);
			if (consider_scroll)
			{
				ScrollBox belongScroll = this.B.BelongScroll;
				if (belongScroll != null)
				{
					vector += belongScroll.getAnimScrollingShift();
				}
			}
			return vector;
		}

		public Vector3 global2local(Vector3 Loc)
		{
			return this.B.transform.InverseTransformPoint(Loc);
		}

		public int af
		{
			get
			{
				return this.B.get_af();
			}
		}

		public virtual float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (this.alpha_ != value)
				{
					float num = this.alpha_;
					this.alpha_ = value;
					this.fine_flag = true;
				}
			}
		}

		public int container_stencil_ref
		{
			get
			{
				return this.B.container_stencil_ref;
			}
		}

		public void setAlpha(float value)
		{
			this.alpha = value;
		}

		public string meshicon_name
		{
			get
			{
				return this.meshicon_name_;
			}
			set
			{
				this.meshicon_name_ = (TX.noe(value) ? "" : value);
				this.fine_flag = true;
			}
		}

		public virtual float swidth
		{
			get
			{
				return this.w * 64f;
			}
		}

		public virtual float sheight
		{
			get
			{
				return this.h * 64f;
			}
		}

		protected readonly aBtn B;

		protected readonly GameObject Gob;

		protected readonly RectTransform RectTrs;

		protected MeshDrawer MyMesh;

		protected MeshRenderer MyMeshRenderer;

		protected ValotileRenderer MyValot;

		protected MultiMeshRenderer MMRD;

		protected float w;

		protected float h;

		public string title;

		protected float default_title_width_;

		protected BMListChars DefaultTitleChr;

		protected Vector2 Vdefault_title_shift;

		protected string meshicon_name_ = "";

		protected float alpha_ = 1f;

		public bool html_mode;

		public uint base_color = 4293519849U;

		public bool fine_flag;

		public bool fine_on_binding_changing = true;

		public uint fine_continue_flags;

		public const uint FINE_HOVER = 1U;

		public const uint FINE_ACTIVE = 2U;

		public const uint FINE_PUSHED = 4U;

		public const uint FINE_CHECKED = 8U;

		public const uint FINE_NORMAL = 16U;

		public const uint FINE_ALL = 31U;

		public float curs_level_x = 0.6f;

		public float curs_level_y = -0.4f;

		public bool locked_only_skin;

		protected bool use_valotile_;
	}
}

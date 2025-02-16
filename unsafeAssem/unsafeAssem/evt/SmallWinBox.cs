using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace evt
{
	public class SmallWinBox : MsgBox
	{
		public void Init(PxlFrame _PF, int _width = 440)
		{
			this.PF = _PF;
			this.bm_scale(this._bm_scale, true);
			this.wh((float)_width, 0f).AlignX(ALIGN.CENTER);
		}

		public override MsgBox make(string _tex)
		{
			bool flag = false;
			bool mdBm = this.MdBm != null;
			if (this.toph > 0f)
			{
				if (_tex != "")
				{
					this.wh(X.Mx(this.topw, this.w), 0f).margin(new float[]
					{
						(float)this.pic_margin,
						(float)this.pic_margin + this.toph + 5f,
						(float)this.pic_margin,
						(float)this.pic_margin * 1.25f
					});
					flag = !mdBm;
				}
				else
				{
					this.wh(this.topw, this.toph).margin(new float[] { (float)this.pic_margin });
					this.img_cy = 0f;
				}
			}
			else
			{
				this.wh(this.w, 0f).margin(new float[]
				{
					(float)this.pic_margin,
					(float)this.pic_margin * 1.1f
				});
			}
			base.make(_tex);
			if (_tex != "")
			{
				this.img_cy = base.sheight * 0.5f - ((float)(this.pic_margin - 5) + this.tophh);
			}
			else
			{
				this.img_cy = 0f;
			}
			if (this.PF != null)
			{
				this.img_cx = 0f;
				if (this.MdBm == null)
				{
					this.MdBm = this.MMRD.Make(BLEND.NORMAL, MTRX.getMI(this.PF));
					this.redrawBm();
				}
				else
				{
					this.replaceGrp(this.PF);
				}
			}
			if (flag)
			{
				base.tx_border(6f, "T", MTRX.ColWhite, 1f);
			}
			this.bm_scale(this._bm_scale, true);
			return this;
		}

		public SmallWinBox spMove(float _px, float _py, float _prot, bool force = false)
		{
			if (_px != this.sp_x || _py != this.sp_y || _prot != this.sp_rot || force)
			{
				this.sp_x = _px;
				this.sp_rot = _prot;
				this.sp_y = _py;
				this.bm_scale(this._bm_scale, true);
			}
			return this;
		}

		public SmallWinBox bm_scale(float _a, bool force = false)
		{
			if (this._bm_scale != _a || force)
			{
				this._bm_scale = _a;
				this.need_redraw = true;
			}
			if (!this.bmwh_fix)
			{
				this.bmw = ((this.PF != null) ? ((float)this.PF.pPose.width * this._bm_scale) : 0f);
				this.bmh = ((this.PF != null) ? ((float)this.PF.pPose.height * this._bm_scale) : 0f);
			}
			return this;
		}

		public SmallWinBox bm_alpha(float _a)
		{
			this.bm_base_alpha = _a;
			if (this.MaSdw != null)
			{
				this.MaSdw.setAlpha1(this.shadow_alpha * this.bm_base_alpha * base.alpha, false);
				this.MdSdw.updateForMeshRenderer(true);
			}
			if (this.MaBm != null)
			{
				this.MaBm.setAlpha1(this.bm_base_alpha * base.alpha, false);
				this.MdBm.updateForMeshRenderer(true);
			}
			return this;
		}

		public override void setAlpha(float a)
		{
			if (base.alpha == a)
			{
				return;
			}
			base.setAlpha(a);
			this.bm_alpha(this.bm_base_alpha);
		}

		public SmallWinBox bm_size(float _w = 0f, float _h = 0f)
		{
			if (_w < 0f)
			{
				this.bmwh_fix = false;
				this.bmw = ((this.PF != null) ? ((float)this.PF.pPose.width * this._bm_scale) : 0f);
				this.bmh = ((this.PF != null) ? ((float)this.PF.pPose.height * this._bm_scale) : 0f);
			}
			else
			{
				this.bmw = _w;
				this.bmh = _h;
				this.bmwh_fix = true;
			}
			return this;
		}

		public SmallWinBox replaceGrp(PxlFrame _PF)
		{
			this.PF = _PF;
			Material material = ((this.PF != null) ? MTRX.getMI(this.PF).getMtr(BLEND.NORMAL, -1) : null);
			this.MaBm = null;
			if (this.MdBm != null && this.MdBm.getMaterial() != material)
			{
				this.MMRD.DestroyOne(this.MdBm);
				if (material != null)
				{
					this.MdBm = this.MMRD.Make(material);
				}
				else
				{
					this.MdBm = null;
				}
			}
			this.redrawBm();
			return this;
		}

		public SmallWinBox redrawBm()
		{
			if (this.MdBm == null)
			{
				return this;
			}
			this.MdBm.clear(false, false);
			if (this.PF == null)
			{
				this.MaBm = null;
				return this;
			}
			this.MaBm = new MdArranger(this.MdBm);
			this.MaBm.Set(true);
			this.MdBm.Col = MTRX.cola.White().setA1(base.alpha * this.bm_base_alpha).C;
			this.MdBm.RotaPF(0f, 0f, this._bm_scale, this._bm_scale, 0f, this.PF, false, false, false, uint.MaxValue, false, 0);
			this.MaBm.Set(false);
			this.MdBm.updateForMeshRenderer(true);
			this.need_redraw = false;
			return this;
		}

		public SmallWinBox prepareShadow(PxlFrame PFShadow, float _scale = -1f)
		{
			if (PFShadow == null)
			{
				return this;
			}
			if (_scale < 0f)
			{
				_scale = (float)SmallWinBox.DEF_SHADOW_SCALE;
			}
			if (_scale == 0f)
			{
				if (this.MdSdw != null)
				{
					this.MMRD.GetGob(this.MdSdw).SetActive(false);
					this.MaSdw = null;
				}
			}
			else
			{
				if (this.MdSdw != null)
				{
					this.MMRD.GetGob(this.MdSdw).SetActive(true);
				}
				else
				{
					this.MdSdw = this.MMRD.Make(BLEND.NORMAL, MTRX.getMI(PFShadow));
				}
				this.MaSdw = new MdArranger(this.MdSdw);
				this.MaSdw.Set(true);
				this.MdSdw.clear(false, false);
				this.MdSdw.Col = MTRX.cola.Black().setA1(0f).C;
				this.MdSdw.RotaPF(0f, 0f, _scale, _scale, 0f, PFShadow, false, false, false, uint.MaxValue, false, 0);
				this.MaSdw.Set(false);
				this.MdSdw.updateForMeshRenderer(true);
				Transform transform = this.MMRD.GetGob(this.MdSdw).transform;
				transform.SetParent(this.MMRD.GetGob(this.MdBm).transform, false);
				transform.localPosition = new Vector3(0f, 0f, 0.001f);
				this.shadow_alpha = 0f;
			}
			return this;
		}

		public override MsgBox run(float fcnt, bool force_draw = false)
		{
			if (base.run(fcnt, force_draw) == null)
			{
				return null;
			}
			if ((X.D || force_draw) && this.need_redraw)
			{
				this.redrawBm();
			}
			if ((((this.t >= 14f || this.t < 0f) && this.shadow_alpha < 1f) || force_draw) && this.MaSdw != null)
			{
				this.shadow_alpha = X.VALWALK(this.shadow_alpha, 1f, 0.018f * fcnt);
				this.MaSdw.setAlpha1(this.shadow_alpha * base.alpha * this.bm_base_alpha, false);
				this.MdSdw.updateForMeshRenderer(true);
			}
			return this;
		}

		public override void destruct()
		{
			base.destruct();
		}

		public override MsgBox deactivate()
		{
			base.deactivate();
			this.bm_size(-1f, 0f);
			return this;
		}

		public float get_bm_scale()
		{
			return this._bm_scale;
		}

		public float topw
		{
			get
			{
				if (this.PF == null)
				{
					return 0f;
				}
				return this.bmw;
			}
		}

		public float toph
		{
			get
			{
				if (this.PF == null)
				{
					return 0f;
				}
				return this.bmh;
			}
		}

		public float topwh
		{
			get
			{
				return this.topw * 0.5f;
			}
		}

		public float tophh
		{
			get
			{
				return this.toph * 0.5f;
			}
		}

		public int pic_margin = 30;

		public int cap_marginleft = 20;

		private bool bmwh_fix;

		private float _bm_scale = 2f;

		private PxlFrame PF;

		private float bmw;

		private float bmh;

		private MeshDrawer MdBm;

		private MdArranger MaBm;

		private Sprite BmSdw;

		private MeshDrawer MdSdw;

		private MdArranger MaSdw;

		private float img_cx;

		private float img_cy;

		private static readonly int DEF_SHADOW_SCALE = 6;

		private static readonly int CAP_SIZE = 18;

		private float shadow_alpha;

		private float bm_base_alpha = 1f;

		private bool need_redraw;

		private float sp_x;

		private float sp_y;

		private float sp_rot;
	}
}

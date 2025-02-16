using System;
using UnityEngine;
using XX;
using XX.mobpxl;

namespace m2d
{
	public class M2MobGAnimator : MobSkltAnimator, ITeScaler, ITeShift, ITeColor
	{
		private static M2MobGenerator MOBG
		{
			get
			{
				return M2MobGenerator.Instance;
			}
		}

		public M2MobGAnimator(M2EventItem _Mv, string _sklt_name)
		{
			this.Mv = _Mv;
			bool instance_prepared = M2MobGenerator.instance_prepared;
			this.MdFin = new MeshDrawer(null, 4, 6);
			this.MdFin.draw_gl_only = true;
			this.MdFin.activate(this.Mv.ToString(), MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false, MTRX.ColWhite, null);
			this.sklt_name_ = _sklt_name;
			this.timeScale = X.NIXP(0.8f, 1.2f);
			M2MobGAnimator.MOBG.assignAnimator(this);
		}

		public string sklt_name
		{
			get
			{
				return this.sklt_name_;
			}
			set
			{
				if (this.sklt_name == value)
				{
					return;
				}
				this.sklt_name_ = value;
				if (M2MobGAnimator.MOBG.sklt_loaded)
				{
					this.fineSklt();
				}
			}
		}

		private bool fineSklt()
		{
			if (this.sklt_name_ == null)
			{
				return false;
			}
			base.Sklt = M2MobGAnimator.MOBG.getSklt(this.sklt_name_);
			if (base.Sklt == null)
			{
				X.dl("不明なSKlt : " + this.sklt_name, null, false, false);
				this.sklt_name_ = null;
				if (base.MSq != null)
				{
					base.MSq = null;
				}
				return false;
			}
			if (this.finatlas_created && ((float)this.FinAtlas.width != base.Sklt.Size.width || (float)this.FinAtlas.height != base.Sklt.Size.height))
			{
				M2MobGAnimator.MOBG.need_refine_fin_atlas = true;
			}
			this.recreateSkltAtlas();
			this.SpSetPose((base.MSq != null) ? base.MSq.type : this.pose_title_, -1);
			this.ClipArea(this.LpClip);
			return true;
		}

		public void recreateSkltAtlas()
		{
			if (base.Sklt != null && !base.Sklt.atlas_created)
			{
				M2MobGAnimator.MOBG.createSkltAtlas(this.Mp, base.Sklt);
				this.changed = true;
			}
		}

		public void ClipArea(M2LabelPoint Lp)
		{
			this.LpClip = Lp;
			if (base.Sklt == null)
			{
				return;
			}
			this.need_fine_finalize_mesh = true;
			if (this.LpClip == null)
			{
				this.clip_b = 0;
				this.clip_t = 0;
				this.clip_r = 0;
				this.clip_l = 0;
				return;
			}
			float num = Lp.bottom * this.Mp.rCLEN;
			float num2 = Lp.y * this.Mp.rCLEN;
			float num3 = this.Mv.mbottom - base.Sklt.Size.height * this.Mp.rCLENB;
			float num4 = this.Mv.x + base.Sklt.Size.width * 0.5f * this.Mp.rCLENB;
			float num5 = Lp.x * this.Mp.rCLEN;
			float num6 = this.Mv.x - base.Sklt.Size.width * 0.5f * this.Mp.rCLENB;
			float num7 = Lp.right * this.Mp.rCLEN;
			if (X.isCovering(num5, num7, num6, num4, 0f))
			{
				if (this.Mv.mbottom > num)
				{
					if (num <= num3)
					{
						this.clip_b = 1000;
					}
					else
					{
						float num8 = this.Mv.mbottom - num;
						this.clip_b = X.IntR((num8 * this.Mp.CLENB + (base.Sklt.Size.y + base.Sklt.Size.height * 0.5f + base.Sklt.baseline)) * (float)this.prepare_scale);
					}
				}
				else
				{
					this.clip_b = 0;
				}
				if (this.clip_b < 1000)
				{
					if (num3 < num2)
					{
						if (this.Mv.mbottom <= num2)
						{
							this.clip_t = 1000;
						}
						else
						{
							float num9 = num2 - num3;
							this.clip_t = X.IntR(num9 * this.Mp.CLENB * (float)this.prepare_scale);
						}
					}
					else
					{
						this.clip_t = 0;
					}
				}
			}
			else
			{
				this.clip_b = 0;
				this.clip_t = 0;
			}
			if (this.clip_b < 1000 && this.clip_t < 1000 && X.isCovering(num2, num, num3, this.Mv.mbottom, 0f))
			{
				if (num4 > num7)
				{
					if (num7 <= num6)
					{
						this.clip_r = 1000;
					}
					else
					{
						float num10 = this.Mv.mright - num7;
						this.clip_r = X.IntR(num10 * this.Mp.CLENB * (float)this.prepare_scale);
					}
				}
				else
				{
					this.clip_r = 0;
				}
				if (this.clip_r < 1000 && num6 < num5)
				{
					if (this.Mv.mright <= num5)
					{
						this.clip_l = 1000;
						return;
					}
					float num11 = num5 - num6;
					this.clip_l = X.IntR(num11 * this.Mp.CLENB * (float)this.prepare_scale);
					return;
				}
			}
			else
			{
				this.clip_r = 0;
				this.clip_l = 0;
			}
		}

		public override bool updateAnimator(float fcnt)
		{
			if (base.Sklt == null)
			{
				if (this.sklt_name_ == null)
				{
					return false;
				}
				if (!M2MobGenerator.instance_prepared)
				{
					return false;
				}
				if (!this.fineSklt())
				{
					return false;
				}
			}
			return base.updateAnimator(fcnt);
		}

		public void destruct()
		{
			if (this.RTkt != null)
			{
				this.RTkt = M2DBase.Instance.Cam.MovRender.deassignDrawable(this.RTkt, -1);
			}
			if (this.MdFin != null)
			{
				this.MdFin.destruct();
			}
			if (M2MobGAnimator.MOBG != null)
			{
				M2MobGAnimator.MOBG.removeAnimator(this);
			}
		}

		public void SpSetPose(string s, int reset_anmf = -1)
		{
			this.pose_title_ = s;
			if (base.Sklt == null)
			{
				return;
			}
			SkltSequence animByType = base.Sklt.getAnimByType(s);
			if (animByType == null)
			{
				X.dl("不明なシークエンス : " + s + " @" + this.sklt_name, null, false, false);
				return;
			}
			if (CAim._XD(this.aim, 1) == 0)
			{
				this.aim = ((X.xors() % 2U == 0U) ? AIM.L : AIM.R);
			}
			this.initRenderTicket();
			int cframe = base.cframe;
			if (base.MSq == animByType && reset_anmf <= 0)
			{
				return;
			}
			base.setSequence(animByType);
			if (reset_anmf == 0)
			{
				base.animReset(cframe);
			}
		}

		public void setAim(AIM a, int restart_anim = -1)
		{
			if (this.aim != a)
			{
				float num = (float)this.mpf_is_right;
				this.aim = a;
				if ((float)this.mpf_is_right != num)
				{
					this.need_fine_finalize_mesh = true;
				}
				if (restart_anim < 0)
				{
					restart_anim = 1;
				}
			}
			if (restart_anim > 0)
			{
				base.animReset(0);
			}
		}

		private void initRenderTicket()
		{
			if (this.RTkt == null)
			{
				this.fineMaterial();
				this.RTkt = this.Mp.MovRenderer.assignDrawable(this.order, null, new M2RenderTicket.FnPrepareMd(this.RenderPrepareMesh), null, this.Mv, null);
			}
		}

		public void fineMaterial()
		{
			Material mtr = M2MobGAnimator.MOBG.MIFin.getMtr(BLEND.NORMALP3, -1);
			this.MdFin.setMaterial(mtr, false);
		}

		public void fineMaterial(Shader Shd)
		{
			this.MdFin.setMaterial(M2MobGAnimator.MOBG.MIFin.getMtr(Shd, -1), false);
		}

		public void fineMaterial(Material Mtr)
		{
			if (Mtr == null)
			{
				this.fineMaterial();
				return;
			}
			this.MdFin.setMaterial(M2MobGAnimator.MOBG.MIFin.getMtr(Mtr.shader, -1), false);
		}

		public M2Mover.DRAW_ORDER order
		{
			get
			{
				return this.order_;
			}
			set
			{
				this.order_ = value;
				if (this.RTkt != null)
				{
					this.RTkt.order = this.order_;
				}
			}
		}

		protected bool RenderPrepareMesh(Camera Cam, M2RenderTicket Tk, bool need_redraw, int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			MdOut = null;
			if (this.Mv.destructed || base.Sklt == null)
			{
				return false;
			}
			if (draw_id == 0 && need_redraw)
			{
				Vector3 localPosition = this.Mv.transform.localPosition;
				localPosition.x = this.Mv.Mp.pixel2ux_rounded(this.Mv.drawx + this.Mv.getSpShiftX()) + this.TeShift.x * 0.015625f;
				localPosition.y = this.Mv.Mp.pixel2uy_rounded(this.Mv.drawy - this.Mv.getSpShiftY()) + this.TeShift.y * 0.015625f;
				float x = this.TeScale.x;
				float y = this.TeScale.y;
				float num = localPosition.x * this.Mp.base_scale + this.offsetPixelX * 0.015625f;
				float num2 = localPosition.y * this.Mp.base_scale + this.offsetPixelY * 0.015625f;
				Tk.Matrix = Matrix4x4.Translate(new Vector3(num, num2, 1f)) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, this.rotationR / 3.1415927f * 180f)) * Matrix4x4.Scale(new Vector3(x, y, 1f));
			}
			M2MobGenerator mobg = M2MobGAnimator.MOBG;
			if (this.need_fine_finalize_mesh)
			{
				this.need_fine_finalize_mesh = false;
				Rect rect = new Rect((float)this.FinAtlas.x, (float)this.FinAtlas.y, (float)this.FinAtlas.width, (float)this.FinAtlas.height);
				int mpf_is_right = this.mpf_is_right;
				float num3 = (float)((mpf_is_right < 0) ? this.clip_l : this.clip_r);
				float num4 = (float)((mpf_is_right < 0) ? this.clip_r : this.clip_l);
				float num5 = ((this.clip_b > 0) ? ((float)this.clip_b + this.Mv.getSpShiftY() * this.Mp.base_scale * (float)this.prepare_scale) : 0f);
				if (num3 > 0f)
				{
					rect.x += num3;
					rect.width -= num3;
				}
				if (num4 > 0f)
				{
					rect.width -= num4;
				}
				if (num5 > 0f)
				{
					rect.y += num5;
					rect.height -= num5;
				}
				if (this.clip_t > 0)
				{
					rect.height -= (float)this.clip_t;
				}
				float num6 = 1f / (float)this.prepare_scale;
				float num7 = (num5 - (float)this.clip_t) * 0.5f * num6;
				float num8 = (num3 - num4) * 0.5f * num6;
				this.need_fine_finalize_mesh = false;
				this.MdFin.clearSimple();
				this.MdFin.Identity();
				this.MdFin.initForImg(mobg.MIFin.Tx, rect.x, rect.y, rect.width, rect.height);
				if (mpf_is_right > 0)
				{
					this.MdFin.Scale(-1f, 1f, false);
					num8 = -num8;
				}
				this.MdFin.Col = this.MulColor;
				this.MdFin.Uv23(this.AddColor, false);
				this.MdFin.Rect(num8, num7, rect.width * num6, rect.height * num6, false);
				this.MdFin.allocUv23(0, true);
			}
			if (this.FnReplaceRender != null)
			{
				return this.FnReplaceRender(Cam, Tk, need_redraw, draw_id, out MdOut, ref paste_mesh);
			}
			if (draw_id == 0)
			{
				MdOut = this.MdFin;
				return true;
			}
			return false;
		}

		public bool need_draw
		{
			get
			{
				return this.RTkt != null && this.RTkt.camera_in;
			}
		}

		public bool finatlas_created
		{
			get
			{
				return this.FinAtlas.width > 0;
			}
			set
			{
				if (!value)
				{
					this.FinAtlas.width = (this.FinAtlas.height = 0);
					this.changed = (this.need_fine_finalize_mesh = true);
				}
			}
		}

		public Vector2 getScaleTe()
		{
			return this.TeScale;
		}

		public void setScaleTe(Vector2 V)
		{
			this.TeScale = V;
		}

		public Vector2 getShiftTe()
		{
			return this.TeShift;
		}

		public void setShiftTe(Vector2 V)
		{
			if (V.x != this.TeShift.x || V.y != this.TeShift.y)
			{
				this.TeShift.Set(V.x, V.y);
			}
		}

		public Color32 getColorTe()
		{
			return MTRX.ColWhite;
		}

		public void setColorTe(C32 Buf, C32 CMul, C32 CAdd)
		{
			this.MulColor = CMul.C;
			this.AddColor = CAdd.C;
			this.need_fine_finalize_mesh = true;
		}

		public Map2d Mp
		{
			get
			{
				return this.Mv.Mp;
			}
		}

		public int mpf_is_right
		{
			get
			{
				return CAim._XD(this.aim, 1);
			}
		}

		public MImage MI
		{
			get
			{
				return M2MobGAnimator.MOBG.MIFin;
			}
		}

		public Material getRendererMaterial()
		{
			if (this.MdFin == null)
			{
				return null;
			}
			return this.MdFin.getMaterial();
		}

		public void setRendererMaterial(Material Mtr)
		{
			if (this.MdFin != null)
			{
				this.MdFin.setMaterial(Mtr, false);
				this.changed = true;
			}
		}

		private readonly M2EventItem Mv;

		private string sklt_name_;

		private string pose_title_ = "stand";

		private Vector2 TeScale = Vector2.one;

		private Vector2 TeShift = Vector2.zero;

		private Color32 MulColor = MTRX.ColWhite;

		private Color32 AddColor = MTRX.ColTrnsp;

		private M2LabelPoint LpClip;

		private int clip_l;

		private int clip_r;

		private int clip_t;

		private int clip_b;

		public M2RenderTicket.FnPrepareMd FnReplaceRender;

		public M2RenderTicket RTkt;

		public int prepare_scale = 2;

		private MeshDrawer MdFin;

		public bool need_fine_finalize_mesh = true;

		private M2Mover.DRAW_ORDER order_;

		private AIM aim = AIM.T;

		public float offsetPixelX;

		public float offsetPixelY;

		public float rotationR;

		internal RectInt FinAtlas = new RectInt(0, 0, 0, 0);
	}
}

using System;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2PxlAnimator : PxlCharaAnimator, ITeShift, ITeColor
	{
		public Vector2 getShiftTe()
		{
			return this.TeShift;
		}

		public void setShiftTe(Vector2 V)
		{
			if (V.x != this.TeShift.x || V.y != this.TeShift.y)
			{
				Vector3 localPosition = this.Trs.localPosition;
				localPosition.x += (V.x - this.TeShift.x) * 0.015625f;
				localPosition.y += (V.y - this.TeShift.y) * 0.015625f;
				this.Trs.localPosition = localPosition;
				this.TeShift.Set(V.x, V.y);
			}
		}

		public Color32 getColorTe()
		{
			return MTRX.ColWhite;
		}

		public virtual void setColorTe(C32 Buf, C32 CMul, C32 CAdd)
		{
			this.color = CMul.C;
			if (this.Mtr != null)
			{
				this.Mtr.SetColor("_AddColor", CAdd.C);
			}
		}

		public virtual void OnDestroy()
		{
			this.check_torture = false;
			if (this.MtrCreated != null)
			{
				IN.DestroyOne(this.MtrCreated);
				this.MtrCreated = null;
			}
		}

		protected virtual void Awake()
		{
			this.auto_update_animation = false;
			this.Trs = base.transform;
		}

		public Map2d Mp
		{
			get
			{
				return this.Mv.Mp;
			}
		}

		public float base_scale
		{
			get
			{
				return this.Mp.mover_scale;
			}
		}

		public virtual void assignMover(M2Mover _Mv, bool auto_start = false)
		{
			this.Mv = _Mv;
			this.MvTrs = this.Mv.transform;
			this.multiply_global_timescale = false;
			this.Trs.localScale = this.MvTrs.localScale;
			this.base_scaleX = this.MvTrs.localScale.x;
			this.base_scaleY = this.MvTrs.localScale.y;
			this.timescale = 1f;
			this.Trs.localEulerAngles = this.MvTrs.localEulerAngles;
			this.base_rotation = this.MvTrs.localEulerAngles.z / 180f * 3.1415927f;
			Vector3 localPosition = this.Trs.localPosition;
			localPosition.x = this.Mv.Mp.pixel2ux_rounded(this.Mv.drawx + this.Mv.getSpShiftX());
			localPosition.y = this.Mv.Mp.pixel2uy_rounded(this.Mv.drawy - this.Mv.getSpShiftY());
			localPosition.z = this.z;
			this.Trs.localPosition = localPosition;
			base.setAim((global::PixelLiner.PixelLinerLib.AIM)this.Mv.aim, -1);
			if (auto_start)
			{
				this.Start();
				return;
			}
			base.Start();
			base.enabled = true;
		}

		public override Material prepareMaterial(ref Material Mtr, Texture TargetTexture)
		{
			if (this.MtrCreated == null)
			{
				this.MtrCreated = MTRX.newMtr(MTRX.MIicon.getMtr(BLEND.NORMALP3, -1));
			}
			this.MtrCreated.mainTexture = TargetTexture;
			Mtr = this.MtrCreated;
			return Mtr;
		}

		public override void Start()
		{
			if (this.Mv is M2Attackable && this.auto_assign_tecon)
			{
				this.TeCon = (this.Mv as M2Attackable).TeCon;
				this.TeCon.RegisterCol(this, false);
				this.TeCon.RegisterPos(this);
			}
			base.enabled = false;
			base.Start();
		}

		public override void setPose(string title, int restart_anim = -1)
		{
			PxlPose currentPose = base.getCurrentPose();
			if (this.fnChangePoseListener != null)
			{
				title = this.fnChangePoseListener(this, title, ref restart_anim);
			}
			base.setPose(title, restart_anim);
			PxlPose currentPose2 = base.getCurrentPose();
			if (currentPose != currentPose2)
			{
				this.poseCanged(title, currentPose2, currentPose);
				this.PointData = (this.read_point ? M2PxlPointContainer.GetPoints(currentPose2) : null);
			}
		}

		public virtual void poseCanged(string title, PxlPose New, PxlPose Pre)
		{
		}

		public void runPre(float ts)
		{
			if (!base.isCharacterLoadFinished())
			{
				this.Update();
				if (this.pSq == null)
				{
					this.alpha = 0f;
					return;
				}
				if (this.alpha == 0f)
				{
					if (this.TeCon != null)
					{
						this.TeCon.setFadeIn(20f, 0f);
						this.alpha = 0.0078125f;
					}
					else
					{
						this.alpha = 1f;
					}
				}
			}
			ts *= this.Mv.TS * this.Mv.animator_TS;
			base.updateAnimator(ts * this.timescale);
			this.Update();
			Vector3 localPosition = this.Trs.localPosition;
			localPosition.x = this.Mv.Mp.pixel2ux_rounded(this.Mv.drawx_tstack + this.Mv.getSpShiftX()) + this.TeShift.x * 0.015625f;
			localPosition.y = this.Mv.Mp.pixel2uy_rounded(this.Mv.drawy_tstack - this.Mv.getSpShiftY()) + this.TeShift.y * 0.015625f;
			this.Trs.localPosition = localPosition;
			if (this.need_fine_rotscale)
			{
				this.Trs.localScale = new Vector3(this.scaleX_ * this.base_scaleX, this.scaleY_ * this.base_scaleY, 1f);
				this.Trs.localEulerAngles = new Vector3(0f, 0f, this.rotation_ + this.base_rotation);
				this.need_fine_rotscale = false;
			}
			if (base.isChanged())
			{
				this.need_fine = true;
				PxlFrame frame = this.pSq.getFrame(base.cframe);
				string name = frame.name;
				M2Phys physic = this.Mv.getPhysic();
				if (physic != null)
				{
					physic.no_play_footsnd_animator = false;
				}
				if (name != null && name.Length > 0)
				{
					STB stb = TX.PopBld(name, 0);
					int num = 0;
					if (physic != null && stb.isStart("NOFOOTSND", num))
					{
						stb.Scroll(num + "NOFOOTSND".Length, out num, (char c) => c == '_' || TX.isNumMatch(c), -1);
						physic.no_play_footsnd_animator = true;
					}
					else if ((physic == null || this.Mv.hasFoot()) && stb.isStart("FOOTSND", num))
					{
						stb.Scroll(num + "FOOTSND".Length, out num, (char c) => c == '_' || TX.isNumMatch(c), -1);
						M2FootManager footManager = this.Mv.getFootManager();
						if (footManager != null)
						{
							footManager.playFootStampEffect(false);
						}
					}
					if (stb.isStart("PTC", num))
					{
						stb.Scroll(num + "PTC".Length, out num, (char c) => c == '_', -1);
						int num2;
						stb.ScrollBeforeDoubleUnderScore(num, out num2, -1);
						if (this.Mv is M2Attackable)
						{
							M2Attackable m2Attackable = this.Mv as M2Attackable;
							if (m2Attackable.PtcHld != null)
							{
								m2Attackable.PtcVar("by", (double)this.Mv.mbottom);
								m2Attackable.PtcHld.PtcST(stb, num, num2 - num, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							}
						}
						else
						{
							this.Mp.PtcSTsetVar("cx", (double)this.Mv.x).PtcSTsetVar("cy", (double)this.Mv.y).PtcSTsetVar("by", (double)this.Mv.mbottom)
								.PtcST(this.Mp.M2D.Hash.Get(stb, num, num2 - num), null, PTCThread.StFollow.NO_FOLLOW);
						}
						num = num2 + 2;
					}
					if (stb.isStart("SND", num))
					{
						stb.Scroll(num + "SND".Length, out num, (char c) => c == '_', -1);
						int num3;
						stb.ScrollBeforeDoubleUnderScore(num, out num3, -1);
						M2Attackable m2Attackable2 = this.Mv as M2Attackable;
						if (m2Attackable2 != null && m2Attackable2.PtcHld != null)
						{
							m2Attackable2.PtcHld.playSndPos(stb, num, num3 - num, m2Attackable2.x, m2Attackable2.y, PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow.NO_FOLLOW, 1);
						}
						else
						{
							this.Mv.playSndPos(this.Mp.M2D.Hash.Get(stb, num, num3 - num), 1);
						}
						num = num3 + 2;
					}
					TX.ReleaseBld(stb);
				}
				if (this.fnChangeFrameListener != null)
				{
					this.fnChangeFrameListener(this, frame);
				}
			}
			if (this.need_fine || this.loop_count != this.pre_loop_count)
			{
				this.fineCurrentFrameMeshManual();
				return;
			}
			this.cur_changed = false;
		}

		public override bool fineCurrentFrameMesh(bool force_rewrite = false)
		{
			if (!this.run_cf)
			{
				this.need_fine = true;
				return false;
			}
			if (this.pSq != null && this.PointData != null)
			{
				PxlFrame frame = this.pSq.getFrame(this.cframe_);
				this.PointData.GetPoints(frame, false);
			}
			if (!base.fineCurrentFrameMesh(force_rewrite))
			{
				return false;
			}
			this.need_fine = false;
			return true;
		}

		public virtual void fineCurrentFrameMeshManual()
		{
			if (this.auto_replace_mesh)
			{
				this.run_cf = true;
				base.Update();
				if (this.need_fine)
				{
					base.fineCurrentFrameMesh(true);
				}
				this.run_cf = false;
			}
			this.cur_changed = true;
			this.need_fine = false;
		}

		public M2Mover get_Mv()
		{
			return this.Mv;
		}

		public static PxlLayer getSpecificLayer(PxlFrame F, string lay_name_prefix, string source_pose_prefix, ref PxlLayer LinkSourceLayer, string ng_lay_name_prefix = null)
		{
			int num = F.countLayers();
			PxlLayer pxlLayer = null;
			for (int i = 0; i < num; i++)
			{
				PxlLayer layer = F.getLayer(i);
				if (!TX.valid(ng_lay_name_prefix) || layer.name.IndexOf(ng_lay_name_prefix) != 0)
				{
					if (lay_name_prefix != "" && layer.name.IndexOf(lay_name_prefix) == 0)
					{
						PxlLayer pxlLayer2;
						LinkSourceLayer = (pxlLayer2 = layer);
						pxlLayer = pxlLayer2;
						break;
					}
					if (layer.isImport() && !(source_pose_prefix == ""))
					{
						PxlLayer importSource = layer.Img.getImportSource(layer);
						if (importSource != null && importSource.pFrm.pPose.title.IndexOf(source_pose_prefix) == 0)
						{
							LinkSourceLayer = importSource;
							pxlLayer = layer;
							break;
						}
					}
				}
			}
			return pxlLayer;
		}

		public PxlLayer getRodPos(ref float depx, ref float depy, string lay_name_prefix, string source_pose_prefix, float dx, float dy, ALIGN alignx = ALIGN.CENTER, ALIGNY aligny = ALIGNY.MIDDLE, int multiply_bits = 0)
		{
			return M2PxlAnimator.getRodPosS(this.Mp.rCLENB, this.getCurrentDrawnFrame(), ref depx, ref depy, lay_name_prefix, source_pose_prefix, dx, dy, alignx, aligny, multiply_bits);
		}

		public static PxlLayer getRodPosS(float rCLENB, PxlFrame F, ref float depx, ref float depy, string lay_name_prefix, string source_pose_prefix, float dx, float dy, ALIGN alignx = ALIGN.CENTER, ALIGNY aligny = ALIGNY.MIDDLE, int multiply_bits = 0)
		{
			if (F == null)
			{
				return null;
			}
			PxlLayer pxlLayer = null;
			PxlLayer specificLayer = M2PxlAnimator.getSpecificLayer(F, lay_name_prefix, source_pose_prefix, ref pxlLayer, "point");
			if (pxlLayer == null)
			{
				return null;
			}
			float num = (float)(pxlLayer.Img.height / 2);
			int num2 = -pxlLayer.Img.width / 2;
			float zmx = specificLayer.zmx;
			if ((multiply_bits & 1) > 0)
			{
				dx *= (float)pxlLayer.Img.width;
			}
			if ((multiply_bits & 2) > 0)
			{
				dx *= (float)pxlLayer.Img.height;
			}
			if ((multiply_bits & 4) > 0)
			{
				dy *= (float)pxlLayer.Img.width;
			}
			if ((multiply_bits & 8) > 0)
			{
				dy *= (float)pxlLayer.Img.height;
			}
			M2PxlAnimator.BufTg.x = ((float)alignx * 0.5f * (float)pxlLayer.Img.width + dx) * specificLayer.zmx;
			M2PxlAnimator.BufTg.y = ((float)aligny * 0.5f * (float)pxlLayer.Img.height + dy) * specificLayer.zmy;
			M2PxlAnimator.BufTg = global::XX.X.ROTV2e(M2PxlAnimator.BufTg, specificLayer.rotR);
			depx = (M2PxlAnimator.BufTg.x + specificLayer.x) * rCLENB;
			depy = (M2PxlAnimator.BufTg.y + specificLayer.y) * rCLENB;
			return specificLayer;
		}

		public bool isCurrentFrameChanged()
		{
			return this.cur_changed;
		}

		public float scaleX
		{
			get
			{
				return this.scaleX_;
			}
			set
			{
				float pivot_pixel_x = this.pivot_pixel_x;
				this.scaleX_ = value;
				this.pivot_pixel_x = pivot_pixel_x;
				this.need_fine_rotscale = true;
			}
		}

		public float scaleY
		{
			get
			{
				return this.scaleY_;
			}
			set
			{
				float pivot_pixel_y = this.pivot_pixel_y;
				this.scaleY_ = value;
				this.pivot_pixel_y = pivot_pixel_y;
				this.need_fine_rotscale = true;
			}
		}

		public M2PxlAnimator setScale(float sx, float sy = -1000f, bool fine_offset = true)
		{
			if (fine_offset)
			{
				this.scaleX = ((sx == -1000f) ? this.scaleX_ : sx);
				this.scaleY = ((sy == -1000f) ? this.scaleY_ : sy);
			}
			else
			{
				this.scaleX_ = ((sx == -1000f) ? this.scaleX_ : sx);
				this.scaleY_ = ((sy == -1000f) ? this.scaleY_ : sy);
				this.need_fine_rotscale = true;
			}
			return this;
		}

		public float rotation
		{
			get
			{
				return this.rotation_;
			}
			set
			{
				this.rotation_ = value;
				this.need_fine_rotscale = true;
			}
		}

		public float pivot_pixel_x
		{
			get
			{
				return -base.offsetPixelX / this.scaleX_;
			}
			set
			{
				base.offsetPixelX = -value * this.scaleX_;
				this.need_fine = true;
			}
		}

		public float pivot_pixel_y
		{
			get
			{
				return -base.offsetPixelY / this.scaleY_;
			}
			set
			{
				base.offsetPixelY = -value * this.scaleY_;
				this.need_fine = true;
			}
		}

		public Vector2 ViewScale
		{
			get
			{
				return new Vector2(this.scaleX_, this.scaleY_);
			}
		}

		public bool check_torture
		{
			get
			{
				return this.check_torture_;
			}
			set
			{
				if (this.check_torture_ == value)
				{
					return;
				}
				this.check_torture_ = value;
				if (this.Mv is ITortureListener)
				{
					if (value)
					{
						this.Mp.addTortureListener(this.Mv as ITortureListener);
						return;
					}
					this.Mp.remTortureListener(this.Mv as ITortureListener);
				}
			}
		}

		protected M2Mover Mv;

		public Transform Trs;

		private Transform MvTrs;

		public float z = 300f;

		private bool cur_changed;

		public bool auto_assign_tecon = true;

		public bool auto_replace_mesh = true;

		public bool read_point;

		public const BLEND mtr_blend = BLEND.NORMALP3;

		private float rotation_;

		private float scaleX_ = 1f;

		private float scaleY_ = 1f;

		private float base_scaleX = 1f;

		private float base_scaleY = 1f;

		private float base_rotation;

		private bool need_fine_rotscale = true;

		public float timescale = 1f;

		public M2PxlPointContainer.PosePointsData PointData;

		public M2PxlAnimator.FnChangePoseListener fnChangePoseListener;

		public M2PxlAnimator.FnChangeFrameListener fnChangeFrameListener;

		private static Vector2 BufTg;

		private Vector2 TeShift = Vector2.zero;

		private Material MtrCreated;

		private bool check_torture_;

		public bool need_fine;

		private bool run_cf;

		public TransEffecter TeCon;

		public delegate string FnChangePoseListener(M2PxlAnimator This, string pose0, ref int restart_anim);

		public delegate string FnChangeFrameListener(M2PxlAnimator This, PxlFrame F);
	}
}

using System;
using Spine;
using UnityEngine;
using XX;

namespace nel.fatal
{
	internal sealed class FtLayer
	{
		public Transform transform
		{
			get
			{
				return this.Gob.transform;
			}
		}

		public FtLayer(FatalShower _FtCon, string name, int _buffer)
		{
			this.FtCon = _FtCon;
			this.buffer = _buffer;
			this.Gob = IN.CreateGob(this.FtCon.GobObjectBase, name);
			this.Gob.layer = ((this.buffer == 0) ? this.FtCon.buf_layer0 : this.FtCon.buf_layer1);
			this.Md = MeshDrawer.prepareMeshRenderer(this.Gob, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
			this.Md.InitSubMeshContainer(0);
			this.Mrd = (this.TMrd = this.Gob.GetComponent<MeshRenderer>());
			this.Valot = this.Gob.AddComponent<ValotileRenderer>();
			this.Valot.enabled = false;
			this.Md.connectRendererToTriMulti(this.Mrd);
			this.Mf = this.Gob.GetComponent<MeshFilter>();
			this.FD_eventTrigger = new AnimationState.TrackEntryEventDelegate(this.FnEventTrigger);
			this.FD_animLooped = new AnimationState.TrackEntryDelegate(this.FnAnimLooped);
		}

		public bool enabled
		{
			get
			{
				return this.Gob.activeSelf;
			}
			set
			{
				this.Gob.SetActive(value);
				if (!value && this.EF != null)
				{
					this.EF.clear();
				}
			}
		}

		public bool mrd_enabed
		{
			get
			{
				return this.TMrd.enabled;
			}
			set
			{
				this.TMrd.enabled = value;
			}
		}

		public int layer
		{
			get
			{
				return this.Gob.layer;
			}
		}

		public void swapInit(bool buf_flag)
		{
			if (this.Spv != null)
			{
				this.Spv.remListenerEvent(this.FD_eventTrigger);
				this.Spv.remListenerComplete(this.FD_animLooped);
			}
		}

		public float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				this.alpha_ = value;
				if (this.AMtr != null && this.ptype == PIC.SPINE)
				{
					int num = this.AMtr.Length;
					Color32 color = C32.MulA(uint.MaxValue, this.alpha_);
					for (int i = 0; i < num; i++)
					{
						this.AMtr[i].SetColor("_FillColor", color);
					}
				}
			}
		}

		public MeshDrawer getMeshDrawer()
		{
			return this.Md;
		}

		public void clear(PIC _ptype)
		{
			this.enabled = true;
			this.is_effect = false;
			this.transform.SetParent(this.FtCon.GobObjectBase.transform, false);
			this.transform.localRotation = Quaternion.identity;
			this.scale = 1f;
			this.Gob.transform.localScale = new Vector3(this.scale, this.scale, 1f);
			this.resetBaseZ();
			if (this.Spv != null)
			{
				this.Spv.remListenerComplete(this.FD_animLooped);
				this.Spv.remListenerEvent(this.FD_eventTrigger);
				if (this.Loader != null && this.Spv.gameObject.transform.parent == this.transform.parent)
				{
					this.Spv.gameObject.transform.SetParent(this.FtCon.GobObjectBase.transform, false);
					this.TMrd.enabled = false;
					this.Spv.gameObject.SetActive(false);
				}
			}
			this.Spv = null;
			this.Loader = null;
			this.Valot.ReleaseBinding(true, true, true);
			this.Valot.enabled = false;
			this.ptype = _ptype;
			this.trs_maxt = -1f;
			this.alpha_ = 1f;
			this.trs_t = 0f;
			this.event_snd_time = -1f;
			if (this.SD != null)
			{
				this.SD.destruct(this.Md);
				this.SD = null;
			}
			this.fadeout_maxt = 0f;
			this.TMrd = this.Mrd;
			this.shiftx = (this.shifty = (this.sp_shiftx = (this.sp_shifty = 0f)));
			this.trstype = TRSL.LINE;
			this.spmove = SPMOVE.NONE;
			this.spmove_TS = 1f;
			this.spmv_t = 0f;
			this.spmv_level = 1f;
			this.Mosaic = null;
			if (this.EF != null)
			{
				this.EF.clear();
				this.fineEffectTargetGob();
			}
			this.Md.clear(false, true);
			this.Mrd.enabled = true;
			this.Md.connectRendererToTriMulti(this.Mrd);
		}

		public void destruct()
		{
			this.clear(PIC.SPINE);
			if (this.SD != null)
			{
				this.SD.destruct(this.Md);
				this.SD = null;
			}
			if (this.EF != null)
			{
				this.EF.destruct();
			}
			if (this.Md != null)
			{
				this.Md.destruct();
			}
		}

		public MeshRenderer getMeshRenderer()
		{
			return this.TMrd;
		}

		public void initValot(MeshDrawer Md)
		{
			this.Valot.Init(Md, this.TMrd, true);
			Md.connectRendererToTriMulti(this.TMrd);
			this.Valot.enabled = true;
			this.FtCon.initValotBinding(this.Valot, this.buffer);
		}

		public void setSpine(string _spine_key, SpineViewer _Spv, SpvLoader _Loader)
		{
			this.clear(PIC.SPINE);
			this.spine_key = _spine_key;
			this.Loader = _Loader;
			this.Spv = _Spv;
			this.Col0 = MTRX.ColWhite;
			GameObject gameObject = this.Spv.gameObject;
			gameObject.transform.SetParent(this.transform, false);
			IN.PosP(gameObject.transform, -this.Loader.width * 0.5f, this.Loader.height * 0.5f, 0f);
			gameObject.gameObject.SetActive(true);
			this.TMrd = gameObject.GetComponent<MeshRenderer>();
			this.TMrd.enabled = true;
			this.TMrd.gameObject.layer = this.Gob.layer;
			this.Spv.addListenerEvent(this.FD_eventTrigger);
			this.Spv.addListenerComplete(this.FD_animLooped);
			this.Spv.call_complete_evnet_when_track0 = true;
			this.alpha = this.alpha_;
			this.fineEffectTargetGob();
		}

		public void fineSpineMaterial()
		{
		}

		public void setTextureForSpine(Texture Tx)
		{
			if (this.AMtr != null)
			{
				for (int i = this.AMtr.Length - 1; i >= 0; i--)
				{
					this.AMtr[i].mainTexture = Tx;
				}
			}
		}

		public FtLayer RepositBase(float x, float y, float z = 1f)
		{
			this.shiftx = 0f;
			this.base_x = x;
			this.shifty = 0f;
			this.base_y = y;
			this.scale = z;
			return this.Reposit(true);
		}

		public FtLayer RepositBase(Vector3 V, float scale = 1f)
		{
			return this.RepositBase(V.x * scale, V.y * scale, V.z * scale);
		}

		public FtLayer RepositBase(FtLayer Src)
		{
			this.shiftx = Src.shiftx;
			this.base_x = Src.base_x;
			this.shifty = Src.shifty;
			this.base_y = Src.base_y;
			this.scale = Src.scale;
			return this.Reposit(true);
		}

		public float x
		{
			get
			{
				return this.base_x + this.shiftx + this.sp_shiftx;
			}
			set
			{
				this.base_x = value - (this.shiftx + this.sp_shiftx);
			}
		}

		public float y
		{
			get
			{
				return this.base_y + this.shifty + this.sp_shifty;
			}
			set
			{
				this.base_y = value - (this.shifty + this.sp_shifty);
			}
		}

		public void resetBaseZ()
		{
			this.base_z = (float)(-(float)(this.id + 1)) * 0.001f;
		}

		public float base_z
		{
			get
			{
				return this.base_z_;
			}
			set
			{
				if (this.base_z == value)
				{
					return;
				}
				this.base_z_ = value;
				IN.setZAbs(this.Gob.transform, this.FtCon.con_base_z + this.base_z_);
			}
		}

		public FtLayer Reposit(bool fine_container_whlis = true)
		{
			this.Gob.transform.localScale = new Vector3(this.scale, this.scale, 1f);
			Vector3 vector = new Vector3(this.x, this.y, 0f) * 0.015625f;
			if (this.Mosaic != null)
			{
				this.Mosaic.fineScale();
			}
			IN.Pos2(this.Gob.transform, vector);
			if (fine_container_whlis)
			{
				this.FtCon.fineWhLisPosition(this);
			}
			return this;
		}

		public FtLayer alphaFade(float maxt, bool fadein)
		{
			this.fadeout_maxt = (float)X.MPF(!fadein) * maxt;
			this.fadeout_t = 0f;
			return this;
		}

		public FtLayer effectAlphaFade(float maxt, bool fadein, bool is_first, bool is_center)
		{
			if (this.is_effect)
			{
				this.alphaFade(maxt, fadein);
			}
			if (this.SD != null)
			{
				this.SD.initAlphaFade(fadein);
			}
			this.effect_enabled = fadein;
			if (!is_first && !is_center)
			{
				this.shiftx = 0f;
				this.shifty = 0f;
				this.sp_shiftx = 0f;
				this.sp_shifty = 0f;
				if (this.trstype == TRSL.COSI)
				{
					this.base_x = X.NI(this.trans_sx, this.trans_dx, 0.5f);
					this.base_y = X.NI(this.trans_sy, this.trans_dy, 0.5f);
				}
				this.Reposit(true);
			}
			return this;
		}

		public float fade_alpha
		{
			get
			{
				if (this.fadeout_maxt == 0f)
				{
					return 1f;
				}
				if (this.fadeout_maxt <= 0f)
				{
					return X.ZLINE(-this.fadeout_t, -this.fadeout_maxt);
				}
				return 1f - X.ZLINE(this.fadeout_t, this.fadeout_maxt);
			}
		}

		public bool alpha_is_zero
		{
			get
			{
				return this.fadeout_maxt > 0f && this.fadeout_t >= this.fadeout_maxt;
			}
		}

		public void run(int fcnt, bool no_progress_moveanim)
		{
			if (!this.enabled)
			{
				return;
			}
			if (this.ptype == PIC.SPINE)
			{
				if (this.fadeout_maxt != 0f && this.fadeout_t != this.fadeout_maxt)
				{
					this.fadeout_t = X.VALWALK(this.fadeout_t, this.fadeout_maxt, (float)fcnt);
				}
			}
			else
			{
				if (this.SD != null && this.SD.checkRedraw((float)fcnt))
				{
					this.need_refill = true;
				}
				if (this.fadeout_maxt != 0f && this.fadeout_t != this.fadeout_maxt)
				{
					this.fadeout_t = X.VALWALK(this.fadeout_t, this.fadeout_maxt, (float)fcnt);
					this.need_refill = true;
				}
				if (this.ptype == PIC.RADIATION && this.fade_alpha > 0f && this.FtCon.DrRad.needDraw((float)IN.totalframe))
				{
					this.need_refill = true;
				}
				if (this.need_refill)
				{
					this.need_refill = false;
					this.refill();
				}
			}
			bool flag = false;
			if (this.trs_maxt >= 0f && (!no_progress_moveanim || this.trs_maxt <= 1f))
			{
				if (this.trstype == TRSL.HANDSHAKE_INJECT)
				{
					this.trs_t += (float)fcnt * this.trs_maxt;
					this.shiftx = this.trans_sx * X.COSI(this.trs_t, 214f);
					this.shifty = this.trans_sx * X.COSI(this.trs_t, 332f) + this.trans_sy * X.COSI(this.trs_t, 180f);
					flag = true;
				}
				else if (this.trs_t <= this.trs_maxt)
				{
					this.trs_t += (float)fcnt;
					float num;
					switch (this.trstype)
					{
					case TRSL.COSI:
					case TRSL.ZCOS:
						num = X.ZCOS(this.trs_t, this.trs_maxt);
						break;
					case TRSL.ZSIN:
						num = X.ZSIN(this.trs_t, this.trs_maxt);
						break;
					case TRSL.ZSIN2:
						num = X.ZSIN2(this.trs_t, this.trs_maxt);
						break;
					default:
						num = X.ZLINE(this.trs_t, this.trs_maxt);
						break;
					}
					this.base_x = X.NI(this.trans_sx, this.trans_dx, num);
					this.base_y = X.NI(this.trans_sy, this.trans_dy, num);
					flag = true;
				}
				else if (this.trstype == TRSL.COSI)
				{
					this.translateInit(this.trans_sx, this.trans_sy, this.trs_maxt, true);
				}
				else
				{
					this.trs_maxt = 0f;
				}
			}
			if (this.spmv_level > 0f && !no_progress_moveanim)
			{
				SPMOVE spmove = this.spmove;
				if (spmove != SPMOVE.HANDSHAKE)
				{
					if (spmove == SPMOVE.SCARY)
					{
						this.spmv_t += (float)fcnt * this.spmove_TS;
						float num2 = this.spmv_level * X.randA((uint)((int)this.spmv_t / 3 + 40)) % 256f / 256f * 5f;
						float num3 = X.randA((uint)(this.spmv_t + 50f)) % 170U / 170f * 6.2831855f;
						this.sp_shiftx = num2 * X.Cos(num3 * 1.2f);
						this.sp_shifty = num2 * X.Sin(num3);
						flag = true;
					}
				}
				else
				{
					this.spmv_t += (float)fcnt;
					this.sp_shiftx = this.spmv_level * X.COSI(this.spmv_t * this.spmove_TS + (float)(this.id * 33), 244f) * X.ZSIN(this.spmv_t, 40f);
					this.sp_shifty = this.spmv_level * X.COSI(this.spmv_t * this.spmove_TS + (float)(this.id * 63), 312f) * X.ZSIN(this.spmv_t, 40f);
					flag = true;
				}
			}
			if (flag)
			{
				this.Reposit(true);
			}
			if (this.EF != null)
			{
				this.EF.runDrawOrRedrawMesh(X.D_EF, (float)X.AF_EF, 1f);
			}
		}

		public void translateInit(float dx, float dy, float maxt, bool abs_flag = false)
		{
			this.shiftx = (this.shifty = 0f);
			if (maxt <= 0f)
			{
				this.trs_maxt = -1f;
				if (abs_flag)
				{
					this.base_x += dx;
					this.base_y += dy;
				}
				else
				{
					this.base_x = dx;
					this.base_y = dy;
				}
				this.Reposit(true);
				return;
			}
			this.trs_maxt = maxt;
			this.trans_sx = this.base_x;
			this.trans_sy = this.base_y;
			this.trans_dx = (abs_flag ? dx : (this.base_x + dx));
			this.trans_dy = (abs_flag ? dy : (this.base_y + dy));
			this.trs_t = 0f;
		}

		public void handShake(float level, float maxt)
		{
			this.spmv_level = level;
			this.spmove_TS = maxt;
			if (this.spmove == SPMOVE.HANDSHAKE)
			{
				return;
			}
			this.spmove = SPMOVE.HANDSHAKE;
		}

		public void handShakeInject(float level, float levely, float maxt)
		{
			this.trans_sx = level;
			this.trans_sy = levely;
			if (this.trstype == TRSL.HANDSHAKE_INJECT && this.trs_maxt == maxt)
			{
				return;
			}
			this.trans_dx = this.base_x;
			this.trans_dy = this.base_y;
			this.trs_maxt = maxt;
			this.trs_t = 0f;
			this.trstype = TRSL.HANDSHAKE_INJECT;
		}

		public void initFill(PIC ptype, uint cols, uint cole, float maxt, float saf = 0f)
		{
			if (this.enabled && this.SD != null && ptype == this.ptype && this.SD is FtSpecialDrawerFill && (this.SD as FtSpecialDrawerFill).isSame(cols, cole, maxt))
			{
				return;
			}
			this.clear(ptype);
			this.SD = new FtSpecialDrawerFill(this, cols, cole, maxt, saf).initDrawFunc(ptype);
			this.initValot(this.Md);
			this.RepositBase(0f, 0f, 1f);
			this.refill();
		}

		public void initPopPoly(uint col_i, uint col_t, int stencil_ref, StringHolder CR, int cr_start_index)
		{
			FtSpecialDrawerPopPoly ftSpecialDrawerPopPoly = new FtSpecialDrawerPopPoly(this, col_i, col_t, stencil_ref, CR, cr_start_index);
			if (this.enabled && this.SD != null && PIC.POPPOLY == this.ptype && this.SD is FtSpecialDrawerPopPoly && (this.SD as FtSpecialDrawerPopPoly).isSame(ftSpecialDrawerPopPoly))
			{
				this.SD.resetTime(true);
				return;
			}
			this.clear(PIC.POPPOLY);
			this.SD = ftSpecialDrawerPopPoly;
			this.initValot(this.Md);
			this.RepositBase(0f, 0f, 1f);
			this.refill();
		}

		public void initPicture(SpvLoader SpvL, string mesh_key, float scale, int stencil_ref = -1, bool free_handleable = false)
		{
			if (this.enabled && SpvL == null)
			{
				return;
			}
			AtlasRegion image = SpvL.GetImage(mesh_key);
			if (image == null || (PIC.PICTURE == this.ptype && this.SD is FtSpecialDrawerPicture && (this.SD as FtSpecialDrawerPicture).isSame(image, stencil_ref)))
			{
				return;
			}
			this.clear(PIC.PICTURE);
			this.SD = new FtSpecialDrawerPicture(this, SpvL, image, stencil_ref, free_handleable);
			this.initValot(this.Md);
			this.RepositBase(0f, 0f, scale);
			this.refill();
		}

		public FtLayer initBiteSwapping(FtLayer Target, StringHolder SH, int maxt_index)
		{
			this.clear(PIC.BITE_TRANSITION);
			this.Col0 = C32.d2c(4283780170U);
			this.SD = new FtSpecialDrawerBiteTransition(this, Target, SH, maxt_index);
			this.initValot(this.Md);
			this.trans_sx = Target.base_x;
			this.trans_sy = Target.base_y;
			this.trs_maxt = 0f;
			this.is_effect = true;
			this.RepositBase(0f, 0f, 1f);
			return this;
		}

		public void refill()
		{
			if (this.SD != null)
			{
				this.SD.drawTo(this.Md);
			}
		}

		public FtLayer finalizeAnimate(bool abort_transition)
		{
			if (this.trs_maxt >= 0f)
			{
				this.trs_maxt = -1f;
				if (!abort_transition && this.trstype != TRSL.HANDSHAKE_INJECT)
				{
					this.base_x = this.trans_dx;
					this.base_y = this.trans_dy;
					this.Reposit(true);
				}
			}
			if (this.SD != null)
			{
				this.SD.finalizeAnimate(abort_transition);
			}
			return this;
		}

		public void sinkTimeIfSame(FtLayer Lo)
		{
			if (Lo == null || this.SD == null || Lo.SD == null || this.trs_maxt != Lo.trs_maxt || this.trstype != Lo.trstype || this.trstype != Lo.trstype || this.spmove != Lo.spmove || this.spmove_TS != Lo.spmove_TS || this.spmv_level != Lo.spmv_level)
			{
				return;
			}
			if (this.SD.sinkTimeIfSame(Lo.SD))
			{
				this.trs_t = X.Mn(this.trs_maxt, Lo.trs_t);
				this.spmv_t = Lo.spmv_t;
			}
		}

		internal void fineMrdMaterial()
		{
			this.Md.connectRendererToTriMulti(this.Mrd);
		}

		public FtEffect initEffect()
		{
			if (this.EF == null)
			{
				this.EF = new FtEffect(this, this.Gob.name);
			}
			this.fineEffectTargetGob();
			return this.EF;
		}

		public void fineEffectTargetGob()
		{
			if (this.EF != null)
			{
				this.EF.TargetTransform = ((this.Spv != null) ? this.Spv.gameObject.transform : this.transform);
			}
		}

		public bool effect_enabled
		{
			get
			{
				return this.EF != null && this.EF.enabled;
			}
			set
			{
				if (this.EF == null)
				{
					return;
				}
				this.EF.effect_ptc_enabled = value;
			}
		}

		public void FnEventTrigger(TrackEntry trackEntry, Event e)
		{
			if (this.enabled)
			{
				if (this.event_snd_time >= 0f && e.Time < this.event_snd_time)
				{
					return;
				}
				if (e.Data.Name == "loop" && e.Int >= 1)
				{
					this.Spv.setAnmLoopFrameSecond(trackEntry.Animation.Name, trackEntry.AnimationTime);
				}
				if (e.Data.Name == "sound" && FtLayer.fnSpinePlaySound(e))
				{
					SND.Ui.play(e.String, false);
				}
				if (e.Data.Name == "voice_noel" && FtLayer.fnSpinePlaySound(e))
				{
					VoiceController voiceController = this.FtCon.getVoiceController("noel", false);
					if (voiceController != null)
					{
						voiceController.play(e.String, false);
					}
				}
			}
		}

		private static bool fnSpinePlaySound(Event e)
		{
			return e.Int >= 1 || e.Float >= 10f || X.XORSP() * 10f < e.Float;
		}

		public void FnAnimLooped(TrackEntry trackEntry)
		{
			if (trackEntry.TrackIndex == 0)
			{
				this.event_snd_time = trackEntry.TrackTime;
			}
		}

		public bool animating
		{
			get
			{
				return this.trs_maxt >= 0f;
			}
		}

		public Bone FindBone(string name)
		{
			if (this.Spv == null)
			{
				return null;
			}
			return this.Spv.FindBone(name);
		}

		public ValotileRenderer.IValotConnetcable ConnectCam
		{
			get
			{
				return this.FtCon.ConnectCam;
			}
		}

		public int id;

		public readonly FatalShower FtCon;

		public readonly int buffer;

		public GameObject Gob;

		internal SpineViewer Spv;

		public string spine_key;

		public SpvLoader Loader;

		internal PIC ptype;

		internal TRSL trstype;

		private MeshRenderer Mrd;

		private MeshDrawer Md;

		private Material[] AMtr;

		private Material[] AMtrSrc;

		private MeshFilter Mf;

		private MeshRenderer TMrd;

		public bool need_refill;

		private float base_z_;

		private float alpha_;

		public float shiftx;

		public float shifty;

		public float sp_shiftx;

		public float sp_shifty;

		public float base_x;

		public float base_y;

		public float scale;

		public bool is_effect;

		internal SPMOVE spmove;

		private float spmove_TS = 1f;

		public float spmv_level = 1f;

		public float spmv_t;

		private MaterialPropertyBlock Mpb;

		internal Color32 Col0;

		public FtSpecialDrawer SD;

		private ValotileRenderer Valot;

		private float trans_sx;

		private float trans_sy;

		private float trans_dx;

		private float trans_dy;

		private float trs_t;

		private float trs_maxt = -1f;

		private float event_snd_time = -1f;

		private float fadeout_t;

		private float fadeout_maxt;

		private FtEffect EF;

		private AnimationState.TrackEntryEventDelegate FD_eventTrigger;

		private AnimationState.TrackEntryDelegate FD_animLooped;

		public FtMosaic Mosaic;
	}
}

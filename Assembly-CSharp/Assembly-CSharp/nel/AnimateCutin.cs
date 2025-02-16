using System;
using evt;
using m2d;
using Spine;
using Spine.Unity;
using UnityEngine;
using XX;

namespace nel
{
	public class AnimateCutin : MonoBehaviour, IRunAndDestroy, IMosaicDescriptor
	{
		public void releaseMaterial()
		{
			if (this.Mtr != null)
			{
				IN.DestroyOne(this.Mtr);
			}
			this.Mtr = null;
		}

		public void Init(Map2d _Mp, string _cutin_name, AnimateCutin.FnDrawAC _fnDrawAC)
		{
			if (this.Spw == null)
			{
				this.Spw = new SpineViewer("");
				this.Spw.initGameObject(IN.CreateGob(base.gameObject, "-Spw"));
				this.Spw.update_sharedmaterials_array = false;
				IN.setZ(this.Spw.gameObject.transform, -0.03f);
			}
			if (this.Mosaic == null)
			{
				Transform transform = IN.CreateGob(this, "-Mosaic").transform;
				IN.setZ(transform, this.Spw.gameObject.transform.position.z - 0.03f);
				this.Mosaic = new MosaicShower(transform);
			}
			this.deactivate(false);
			this.Mp = _Mp;
			this.cutin_name = _cutin_name;
			this.fader_key = null;
			this.stencil_ref_ = -1;
			this.fineMaterial();
			this.setMulColor(uint.MaxValue, 0f);
			this.TS_spv = 1f;
			this.restarted = false;
			this.position_consider_basepos = true;
			this.t = 0f;
			base.gameObject.SetActive(true);
			this.prepareValot(true, null);
			if (this.MdB == null)
			{
				this.MdB = MeshDrawer.prepareMeshRenderer(base.gameObject, MTRX.MtrMeshNormal, 0f, -1, null, true, true);
				this.MrdB = base.GetComponent<MeshRenderer>();
				GameObject gameObject = IN.CreateGob(this, "-child");
				this.MdT = MeshDrawer.prepareMeshRenderer(gameObject, MTRX.MtrMeshNormal, 0f, -1, null, true, true);
				IN.setZ(gameObject.transform, -0.06f);
				this.MrdT = gameObject.GetComponent<MeshRenderer>();
			}
			this.MdB.setMaterial(MTRX.MtrMeshNormal, false);
			this.setBase(0f, 0f, 1f);
			this.MdB.clear(false, true);
			this.MdT.clear(false, false);
			IN.addRunner(this);
			if (_fnDrawAC != null)
			{
				this.fnDrawAC = _fnDrawAC;
			}
		}

		private void fineMaterial()
		{
			Material material = this.Mtr;
			if (material == null)
			{
				material = (this.Mtr = MTRX.newMtr(MTRX.MtrSpineDefault));
			}
			if (this.stencil_ref_ >= 0)
			{
				material.SetFloat("_StencilRef", (float)this.stencil_ref_);
				material.SetFloat("_StencilComp", 3f);
			}
			else
			{
				material.SetFloat("_StencilComp", 8f);
			}
			this.Spw.prepareMaterial(material);
		}

		public void destruct()
		{
		}

		public void OnDestroy()
		{
			if (this.Spw != null)
			{
				this.Spw.destruct();
			}
			this.releaseMaterial();
		}

		public int stencil_ref
		{
			get
			{
				return this.stencil_ref_;
			}
			set
			{
				this.stencil_ref_ = value;
				this.fineMaterial();
			}
		}

		public virtual void deactivate(bool not_remrunner = false)
		{
			if (this.Mp != null)
			{
				if (!not_remrunner)
				{
					IN.remRunner(this);
				}
				this.Mp = null;
				if (this.FD_Deactivate != null)
				{
					this.FD_Deactivate(this);
				}
				this.Mosaic.setTarget(null, false);
			}
			this.PictBody = null;
			this.pictbody_async_load = false;
			this.fnDrawAC = null;
			base.gameObject.SetActive(false);
		}

		public bool isActive()
		{
			return base.gameObject.activeSelf;
		}

		public MosaicShower getMosaic()
		{
			return this.Mosaic;
		}

		public bool isASyncLoading()
		{
			if (this.PictBody != null && this.pictbody_async_load)
			{
				if (!this.PictBody.isPreparedResource())
				{
					return true;
				}
				this.PictBodyFinalize(this.PictBody.getViewer().getSvTexture());
			}
			return false;
		}

		public bool run(float fcnt)
		{
			if (this.Mp == null)
			{
				return false;
			}
			if (this.isASyncLoading() || !this.isGameActive())
			{
				return true;
			}
			if (this.PictBody != null)
			{
				this.PictBody.getViewer().getSvTexture().runBetobeto(this.BetoMng.get_current_dirt(), this.BetoMng);
			}
			if (X.D)
			{
				bool flag = true;
				this.Spw.updateAnim(true, (float)X.AF * this.TS_spv);
				if (!this.fnDrawAC(this, this.Mp, this.MdB, this.MdT, this.t, ref flag))
				{
					this.deactivate(true);
					return false;
				}
				if (this.t == 0f && this.MdB.hasMultipleTriangle())
				{
					this.MdB.connectRendererToTriMulti(this.MrdB);
				}
				if (this.t == 0f && this.MdT.hasMultipleTriangle())
				{
					this.MdT.connectRendererToTriMulti(this.MrdT);
				}
				if (flag)
				{
					this.MdB.updateForMeshRenderer(false);
					this.MdT.updateForMeshRenderer(false);
				}
				this.t += (float)X.AF;
			}
			return true;
		}

		public void initUIPictBody(string _fader_key, UIPictureBodySpine _PictBody, PR Pr, UIPictureBase.EMSTATE st)
		{
			this.PictBody = _PictBody;
			this.pictstate = st;
			this.fader_key = _fader_key;
			this.BetoMng = Pr.BetoMng;
			this.setBase(0f, 0f, 1f);
			BetobetoManager.SvTexture svTexture = Pr.BetoMng.prepareTexture(this.PictBody.getViewer().key, false);
			if (svTexture.isAsyncLoadFinished())
			{
				this.PictBodyFinalize(svTexture);
				return;
			}
			this.pictbody_async_load = true;
		}

		protected virtual void PictBodyFinalize(BetobetoManager.SvTexture CurSvt)
		{
			if (this.AMtrPict == null)
			{
				this.AMtrPict = new Material[1];
			}
			this.AMtrPict[0] = this.Mtr;
			SpineAtlasAsset spineAtlasAsset;
			SkeletonDataAsset skeletonDataAsset;
			CurSvt.prepareAtlasAssets(out spineAtlasAsset, out skeletonDataAsset, this.AMtrPict, this.PictBody.getViewer().replace_json_key);
			this.Spw.attachPreloadAssets(spineAtlasAsset, skeletonDataAsset, CurSvt.prepareTexture(true), this.Mtr);
			this.animRandomize(this.Spw, this.pictstate, out this.pictstate);
			this.Mosaic.setTarget(this, false);
			this.pictbody_async_load = false;
			if (this.valotile_enabled)
			{
				this.Spw.force_enable_mesh_render = true;
			}
		}

		protected virtual void animRandomize(SpineViewer Spv, UIPictureBase.EMSTATE st, out UIPictureBase.EMSTATE current_state)
		{
			string text;
			this.PictBody.animRandomize(this.Spw, this.pictstate, out text, out current_state);
		}

		public int countMosaic(bool only_sensitive)
		{
			return this.PictBody.countMosaic(only_sensitive);
		}

		public bool getSensitiveOrMosaicRect(ref Matrix4x4 Out, int id, ref MeshAttachment OutMesh, ref Slot BelongSlot)
		{
			return this.PictBody.getSensitiveOrMosaicRect(ref Out, id, ref OutMesh, ref BelongSlot, this.Spw);
		}

		public void restart(float start_t)
		{
			this.t = start_t;
			this.restarted = true;
			if (this.Spw.GetSkeletonAnimation() != null)
			{
				this.Spw.setTimePositionAll(start_t * 0.016666668f);
			}
			this.animRandomize(this.Spw, this.pictstate, out this.pictstate);
			this.Mosaic.setTarget(this, false);
		}

		public void setMulColor(uint c, float alpha)
		{
			this.Mtr.SetColor("_FillColor", C32.d2c(c | 4278190080U));
			this.Mtr.SetFloat("_FillPhase", alpha);
		}

		public bool valotile_enabled
		{
			get
			{
				return this.Valot != null && this.Valot.enabled;
			}
			set
			{
				if (value)
				{
					this.prepareValot(true, null);
					return;
				}
				if (this.Valot == null)
				{
					return;
				}
				this.Valot.enabled = false;
			}
		}

		public void prepareValot(bool enabled, CameraBidingsBehaviour _BelongCB = null)
		{
			if (this.Valot == null)
			{
				this.Valot = IN.GetOrAdd<ValotileRendererFilter>(this.Spw.gameObject);
				this.Valot.Init(this.Spw.gameObject.GetComponent<MeshFilter>(), this.Spw.GetRenderer(), true);
				if (_BelongCB != null)
				{
					this.Valot.Init(_BelongCB);
				}
			}
			this.Valot.enabled = enabled;
			if (enabled)
			{
				this.Spw.force_enable_mesh_render = true;
			}
		}

		public void setBase(float pixel_x, float pixel_y, float scale = 1f)
		{
			Transform transform = this.Spw.gameObject.transform;
			if (this.PictBody != null)
			{
				Vector3 posSyncSlide = this.PictBody.PCon.PosSyncSlide;
				this.PictBody.PCon.PosSyncSlide = new Vector3(0f, 0f, 1f);
				float scale2 = this.PictBody.scale;
				if (this.position_consider_basepos)
				{
					pixel_x += this.PictBody.shift_ux * 64f;
					pixel_y += this.PictBody.shift_uy * 64f;
					scale *= scale2;
				}
				else
				{
					pixel_x += -this.PictBody.base_swidth / scale2 * scale * 0.5f;
					pixel_y += this.PictBody.base_sheight / scale2 * scale * 0.5f;
				}
				this.PictBody.PCon.PosSyncSlide = posSyncSlide;
			}
			IN.PosP2(transform, pixel_x, pixel_y);
			transform.localScale = new Vector3(scale, scale, 1f);
		}

		public MeshRenderer getMeshRenderer(MeshDrawer Md)
		{
			if (Md == this.MdB)
			{
				return this.MrdB;
			}
			if (Md != this.MdT)
			{
				return null;
			}
			return this.MrdT;
		}

		public bool isGameActive()
		{
			if (this.Mp == null)
			{
				return false;
			}
			NelM2DBase nelM2DBase = this.Mp.M2D as NelM2DBase;
			return nelM2DBase.pre_map_active || (nelM2DBase.GameOver != null && nelM2DBase.GameOver.isGivingUp()) || EV.isActive(false);
		}

		public SpineViewer getViewer()
		{
			return this.Spw;
		}

		public Bone FindBone(string name)
		{
			if (name == null)
			{
				return null;
			}
			return this.Spw.FindBone(name);
		}

		public override string ToString()
		{
			return this.cutin_name;
		}

		private Map2d Mp;

		private int stencil_ref_ = -1;

		public string cutin_name;

		protected float t;

		public bool position_consider_basepos = true;

		protected SpineViewer Spw;

		private Material Mtr;

		private MeshDrawer MdB;

		private MeshDrawer MdT;

		private MeshRenderer MrdB;

		private MeshRenderer MrdT;

		protected AnimateCutin.FnDrawAC fnDrawAC;

		private BetobetoManager BetoMng;

		private MosaicShower Mosaic;

		public UIPictureBodySpine PictBody;

		public UIPictureBase.EMSTATE pictstate;

		private bool pictbody_async_load;

		public float TS_spv;

		public string fader_key;

		public bool restarted;

		public Action<AnimateCutin> FD_Deactivate;

		private Material[] AMtrPict;

		private ValotileRendererFilter Valot;

		public delegate bool FnDrawAC(AnimateCutin Cti, Map2d Mp, MeshDrawer Md, MeshDrawer MdT, float t, ref bool update_meshdrawer);
	}
}

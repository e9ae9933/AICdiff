using System;
using Spine;
using UnityEngine;

namespace XX
{
	public class SpineViewerBehaviour : MonoBehaviour, ITeColor
	{
		public virtual void Awake()
		{
			if (this.Spw == null)
			{
				this.Spw = new SpineViewer("");
				this.Spw.initGameObject(base.gameObject);
			}
		}

		public void releaseMaterial()
		{
			if (this.Mtr != null && this.material_cloned)
			{
				IN.DestroyOne(this.Mtr);
			}
			this.Mtr = null;
		}

		public Material prepareMaterial(Material _Mtr = null, bool _material_cloned = false)
		{
			if (_Mtr != null)
			{
				if (this.Mtr == _Mtr)
				{
					return this.Mtr;
				}
				this.releaseMaterial();
				this.Mtr = _Mtr;
				this.material_cloned = _material_cloned;
			}
			else if (this.Mtr == null)
			{
				this.Mtr = MTRX.newMtr(MTRX.MtrSpineDefault);
				this.material_cloned = true;
			}
			this.Spw.prepareMaterial(this.Mtr);
			return this.Mtr;
		}

		public void OnDestroy()
		{
			if (this.Spw != null)
			{
				this.Spw.destruct();
			}
			this.releaseMaterial();
		}

		public void attachTextMTI(MTISpine _Mti)
		{
			this.Spw.attachTextMTI(_Mti);
		}

		public void releaseTextMTI()
		{
			this.Spw.releaseTextMTI();
		}

		public void attachImageMTI(MTI _Mti)
		{
			this.Spw.attachImageMTI(_Mti);
		}

		public void releaseImageMTI()
		{
			this.Spw.releaseImageMTI();
		}

		public SpineViewerBehaviour reloadAnimKey(string _key)
		{
			if (this.Spw == null)
			{
				this.Awake();
			}
			this.Spw.reloadAnimKey(_key);
			return this;
		}

		public void fineAtlasMaterial()
		{
			this.Spw.fineAtlasMaterial();
		}

		public Texture prepareTexturePreLoaded(Texture PreLoadedTexture)
		{
			this.Spw.prepareTexturePreLoaded(PreLoadedTexture);
			return PreLoadedTexture;
		}

		public string atlas_key
		{
			get
			{
				return this.Spw.atlas_key;
			}
			set
			{
				this.Spw.atlas_key = value;
			}
		}

		public bool call_complete_evnet_when_track0
		{
			get
			{
				return this.Spw.call_complete_evnet_when_track0;
			}
			set
			{
				this.Spw.call_complete_evnet_when_track0 = value;
			}
		}

		public void OnEnable()
		{
			if (this.Spw != null)
			{
				this.Spw.enabled = true;
			}
		}

		public void OnDisable()
		{
			if (this.Spw != null)
			{
				this.Spw.enabled = false;
			}
		}

		public void clearAnim(string anim_name, int loopTo_frame = -1000, string skin_name = null)
		{
			if (this.Mtr == null)
			{
				this.Spw.prepareMaterial(this.prepareMaterial(null, false));
			}
			this.Spw.clearAnim(anim_name, loopTo_frame, skin_name);
			if (this.valotile_enabled)
			{
				this.Spw.force_enable_mesh_render = true;
			}
		}

		public void updateAnim(bool draw_flag = true, float fcnt = 1f)
		{
			this.Spw.updateAnim(draw_flag, fcnt);
		}

		public void addListenerComplete(AnimationState.TrackEntryDelegate Fn)
		{
			this.Spw.addListenerComplete(Fn);
		}

		public void addListenerEvent(AnimationState.TrackEntryEventDelegate Fn)
		{
			this.Spw.addListenerEvent(Fn);
		}

		public void remListenerComplete(AnimationState.TrackEntryDelegate Fn)
		{
			this.Spw.remListenerComplete(Fn);
		}

		public void remListenerEvent(AnimationState.TrackEntryEventDelegate Fn)
		{
			this.Spw.remListenerEvent(Fn);
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

		public ValotileRendererFilter prepareValot(bool enabled, CameraBidingsBehaviour _BelongCB = null)
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
			return this.Valot;
		}

		public SpineViewer getViewer()
		{
			return this.Spw;
		}

		public Color32 getColorTe()
		{
			return this.Spw.getColorTe();
		}

		public void setColorTe(C32 Buf, C32 CMul, C32 CAdd)
		{
			this.Spw.setColorTe(Buf, CMul, CAdd);
		}

		protected SpineViewer Spw;

		public Material Mtr;

		public bool material_cloned;

		protected ValotileRendererFilter Valot;
	}
}

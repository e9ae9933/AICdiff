using System;
using System.Collections.Generic;
using Better;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2UnstabilizeMapItem : IPauseable
	{
		public M2UnstabilizeMapItem(Map2d _Mp, M2SubMap _Sm = null)
		{
			this.Mp = _Mp;
			this.M2D = this.Mp.M2D;
			this.Sm = _Sm;
			this.Base = ((this.Sm != null) ? this.Sm.getBaseMap() : this.Mp);
			this.OBd = new BDic<string, M2UnstabilizeMapItem.RenderBind>();
			this.M2D.AssignPauseableP(this);
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				stb.Add("<UMI>");
				if (this.Sm != null)
				{
					stb.Add((this.Sm != null) ? this.Sm.ToString() : "", " << ");
				}
				stb += this.Mp.ToString();
				this._tostring = stb.ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		public void openMap()
		{
			this.M2D.AssignPauseableP(this);
		}

		public void closeMap(bool release_render_bind = false)
		{
			this.Pause(release_render_bind);
			this.M2D.DeassignPauseable(this);
		}

		public M2UnstabilizeMapItem destruct(bool no_deactivate = false)
		{
			if (!no_deactivate)
			{
				this.deactivateAll(true);
			}
			else
			{
				this.OBd.Clear();
			}
			this.M2D.DeassignPauseable(this);
			if (this.MtrDark != null)
			{
				IN.DestroyOne(this.MtrDark);
				this.MtrDark = null;
			}
			return null;
		}

		public M2UnstabilizeMapItem FineAuto()
		{
			if (Map2d.editor_decline_lighting || X.DEBUGSTABILIZE_DRAW)
			{
				return this;
			}
			if (this.Mp.MyDrawerWater != null)
			{
				M2UnstabilizeMapItem.RenderBind renderBind = this.Get("WATER");
				if (renderBind == null)
				{
					CameraRenderBinderWater cameraRenderBinderWater = new CameraRenderBinderWater(this.Mp, this.Sm, false);
					renderBind = this.Add("WATER", new M2UnstabilizeMapItem.RenderBind(cameraRenderBinderWater, cameraRenderBinderWater.layer, this.GetGobForMesh(this.Mp.MyDrawerWater)));
				}
				else
				{
					(renderBind.IRd as CameraRenderBinderWater).fineMtrColor();
				}
				if (this.Sm == null)
				{
					renderBind = this.Get("WATER_MV");
					if (renderBind == null)
					{
						CameraRenderBinderWater cameraRenderBinderWater2 = new CameraRenderBinderWater(this.Mp, this.Sm, true);
						renderBind = this.Add("WATER_MV", new M2UnstabilizeMapItem.RenderBind(cameraRenderBinderWater2, M2MovRenderContainer.drawer_mask_layer, null));
					}
					else
					{
						(renderBind.IRd as CameraRenderBinderWater).fineMtrColor();
					}
				}
				else
				{
					this.Remove("WATER_MV");
				}
			}
			else
			{
				this.Remove("WATER");
				this.Remove("WATER_MV");
			}
			if (this.Mp.mode == MAPMODE.NORMAL && (this.Mp.Meta.GetB("dark", false) || this.use_dark_))
			{
				if (this.Get("DARK") == null)
				{
					this.Add("DARK", new M2DarkRenderer(this.M2D, this.Mp, ref this.MtrDark), this.M2D.Cam.getFinalSourceRenderedLayer(), null);
				}
			}
			else
			{
				M2UnstabilizeMapItem.RenderBind renderBind = this.Get("DARK");
				if (renderBind != null)
				{
					M2DarkRenderer m2DarkRenderer = renderBind.IRd as M2DarkRenderer;
					if (m2DarkRenderer != null)
					{
						m2DarkRenderer.deactivate();
						this.OBd.Remove("DARK");
					}
					else
					{
						this.Remove("DARK");
					}
				}
			}
			if (this.Mp.hasLight())
			{
				if (this.Get("LIGHT") == null)
				{
					CameraRenderBinderLight cameraRenderBinderLight = new CameraRenderBinderLight(this.M2D.Cam, this.Mp, this.Sm);
					this.Add("LIGHT", cameraRenderBinderLight, cameraRenderBinderLight.layer, null);
				}
			}
			else
			{
				this.Remove("LIGHT");
			}
			return this;
		}

		public GameObject GetGobForMesh(MeshDrawer Md)
		{
			if (this.Sm == null)
			{
				return this.Mp.get_MMRD().GetGob(Md);
			}
			return this.Sm.getGobForMd(Md);
		}

		public M2UnstabilizeMapItem Remove(string key)
		{
			M2UnstabilizeMapItem.RenderBind renderBind = X.Get<string, M2UnstabilizeMapItem.RenderBind>(this.OBd, key);
			if (renderBind != null)
			{
				renderBind.Pause(this.M2D.Cam);
				this.OBd.Remove(key);
			}
			return this;
		}

		private M2UnstabilizeMapItem.RenderBind Get(string key)
		{
			return X.Get<string, M2UnstabilizeMapItem.RenderBind>(this.OBd, key);
		}

		public M2UnstabilizeMapItem Add(string key, ICameraRenderBinder IRd, int layer, GameObject Gob = null)
		{
			this.Remove(key);
			M2UnstabilizeMapItem.RenderBind renderBind = (this.OBd[key] = new M2UnstabilizeMapItem.RenderBind(IRd, layer, Gob));
			if (!this.paused_base)
			{
				renderBind.Resume(this.M2D.Cam);
			}
			return this;
		}

		private M2UnstabilizeMapItem.RenderBind Add(string key, M2UnstabilizeMapItem.RenderBind Ri)
		{
			this.Remove(key);
			this.OBd[key] = Ri;
			if (!this.paused_base)
			{
				Ri.Resume(this.M2D.Cam);
			}
			return Ri;
		}

		public M2UnstabilizeMapItem AddSmMeshDrawer(string key, MeshDrawer Md, MultiMeshRenderer MMRD, int layer, Material[] AMtr = null)
		{
			AMtr = ((AMtr == null) ? Md.getMaterialArray(false) : AMtr);
			M2UnstabilizeMapItem.RenderBind renderBind = this.Get(key);
			GameObject gob = MMRD.GetGob(Md);
			if (gob == null)
			{
				return this;
			}
			if (renderBind != null && renderBind.layer == layer)
			{
				M2SubMap.SmCameraRenderBinderMesh smCameraRenderBinderMesh = renderBind.IRd as M2SubMap.SmCameraRenderBinderMesh;
				smCameraRenderBinderMesh.FineAttr(gob);
				smCameraRenderBinderMesh.AMtr = AMtr;
				renderBind.GobStabilize = gob;
			}
			else
			{
				this.Add(key, new M2SubMap.SmCameraRenderBinderMesh(this.Sm, key, Md, gob)
				{
					AMtr = AMtr
				}, layer, gob);
			}
			return this;
		}

		public virtual Matrix4x4 getTransformForRender()
		{
			return Matrix4x4.identity;
		}

		public void deactivateAll(bool release = false)
		{
			foreach (KeyValuePair<string, M2UnstabilizeMapItem.RenderBind> keyValuePair in this.OBd)
			{
				keyValuePair.Value.Pause(this.M2D.Cam);
			}
			if (release)
			{
				this.OBd.Clear();
			}
		}

		public void Pause()
		{
			this.Pause(false);
		}

		public void Pause(bool release)
		{
			this.deactivateAll(release);
			this.paused_base = true;
		}

		public void Resume()
		{
			this.paused_base = false;
			foreach (KeyValuePair<string, M2UnstabilizeMapItem.RenderBind> keyValuePair in this.OBd)
			{
				keyValuePair.Value.Resume(this.M2D.Cam);
			}
		}

		public bool use_dark
		{
			get
			{
				return this.use_dark_;
			}
			set
			{
				if (this.use_dark == value)
				{
					return;
				}
				this.use_dark_ = value;
				this.FineAuto();
			}
		}

		public readonly Map2d Mp;

		public readonly Map2d Base;

		public readonly M2DBase M2D;

		public readonly M2SubMap Sm;

		private bool paused_base = true;

		private bool use_dark_;

		public const string key_light = "LIGHT";

		public const string key_water = "WATER";

		public const string key_water_mv = "WATER_MV";

		public const string key_dark = "DARK";

		public Material MtrDark;

		private BDic<string, M2UnstabilizeMapItem.RenderBind> OBd;

		private string _tostring;

		private class RenderBind
		{
			public RenderBind(ICameraRenderBinder _IRd, int _layer, GameObject _GobStabilize = null)
			{
				this.IRd = _IRd;
				this.layer = _layer;
				this.GobStabilize_ = _GobStabilize;
			}

			public void Pause(M2Camera Cam)
			{
				if (!this.paused)
				{
					Cam.deassignRenderFunc(this.IRd, this.layer);
					this.paused = true;
					try
					{
						if (this.GobStabilize_ != null)
						{
							this.GobStabilize_.SetActive(true);
						}
					}
					catch
					{
					}
				}
			}

			public void Resume(M2Camera Cam)
			{
				if (this.paused)
				{
					Cam.assignRenderFunc(this.IRd, this.layer, false, null);
					this.paused = false;
					try
					{
						if (this.GobStabilize_ != null)
						{
							this.GobStabilize_.SetActive(false);
						}
					}
					catch
					{
					}
				}
			}

			public GameObject GobStabilize
			{
				get
				{
					return this.GobStabilize_;
				}
				set
				{
					if (this.GobStabilize_ == value)
					{
						return;
					}
					this.GobStabilize_ = value;
					if (this.GobStabilize_ != null)
					{
						this.GobStabilize_.SetActive(this.paused);
					}
				}
			}

			public ICameraRenderBinder IRd;

			public int layer;

			private GameObject GobStabilize_;

			private bool paused = true;
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class M2BlurMeshDrawer
	{
		public M2BlurMeshDrawer(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.ABlurMd = new List<M2BlurMeshDrawer.BlurMd>();
		}

		public M2BlurMeshDrawer destruct()
		{
			this.deactivate(true);
			this.ABlurMd.Clear();
			return null;
		}

		public M2BlurMeshDrawer activate(bool on_entrying = false)
		{
			if (this.MtrSrc == null)
			{
				this.base_z = ((this.Mp.SubMapData != null) ? this.Mp.gameObject.transform.position.z : 309.999f);
				this.Mp.Dgn.initMap(this.Mp, this.Mp.SubMapData);
				this.layer = this.Mp.Dgn.getLayerForChip(2, Dungeon.MESHTYPE.CHIP);
				Material material = (this.MtrSrc = this.Mp.Dgn.getChipMaterial(2, null));
				int count = this.ABlurMd.Count;
				bool flag = this.need_reentry;
				for (int i = 0; i < count; i++)
				{
					if (!this.ABlurMd[i].fineMaterial(this, this.MtrSrc, on_entrying) && !on_entrying)
					{
						this.need_reentry = true;
					}
				}
				if (!flag && this.need_reentry)
				{
					this.deactivate(true);
					this.MtrSrc = material;
				}
			}
			return this;
		}

		public void deactivate(bool clear_mesh)
		{
			this.MtrSrc = null;
			int count = this.ABlurMd.Count;
			for (int i = 0; i < count; i++)
			{
				this.ABlurMd[i].deactivate(clear_mesh);
			}
			if (clear_mesh)
			{
				this.need_reentry = true;
			}
		}

		public MeshDrawer getMeshDrawerForBluredChip(M2BlurImage Bri, M2Puts Cp)
		{
			this.need_reentry = false;
			this.activate(true);
			this.MtrSrc == null;
			M2ChipImage img = Cp.Img;
			M2BlurMeshDrawer.BlurMd blurMd = null;
			for (int i = this.ABlurMd.Count - 1; i >= 0; i--)
			{
				M2BlurMeshDrawer.BlurMd blurMd2 = this.ABlurMd[i];
				if (!blurMd2.Gob.activeSelf || blurMd2.isSame(Bri))
				{
					blurMd = blurMd2;
				}
			}
			if (blurMd == null)
			{
				this.ABlurMd.Add(blurMd = new M2BlurMeshDrawer.BlurMd(this, this.MtrSrc));
			}
			blurMd.activate(this, img, Bri);
			blurMd.base_z = 0f;
			blurMd.initForImg(Bri.getTexture());
			blurMd.Col = Cp.Lay.LayerColor.C;
			return blurMd;
		}

		public void updateAll(Map2d Mp)
		{
			int count = this.ABlurMd.Count;
			for (int i = 0; i < count; i++)
			{
				this.ABlurMd[i].updateForMeshRenderer(true);
			}
		}

		public readonly Map2d Mp;

		public bool activated;

		private Material MtrSrc;

		public int layer;

		public float base_z;

		public bool need_reentry = true;

		private readonly List<M2BlurMeshDrawer.BlurMd> ABlurMd;

		private class BlurMd : MeshDrawer
		{
			public BlurMd(M2BlurMeshDrawer Con, Material Mtr)
				: base(null, 4, 6)
			{
				this.Gob = IN.CreateGob(Con.Mp.gameObject, "-Blur");
				ValotileRenderer orAdd = IN.GetOrAdd<ValotileRenderer>(this.Gob);
				this.Mrd = this.Gob.AddComponent<MeshRenderer>();
				orAdd.Init(this, this.Mrd, true);
				orAdd.Mpb = new MProperty(this.Mrd, -1);
				this.fineMaterial(Con, Mtr, false);
				this.Gob.SetActive(false);
			}

			public bool isSame(M2BlurImage _Bri)
			{
				return this.Bri == _Bri;
			}

			public override void destruct()
			{
				this.deactivate(true);
				base.destruct();
			}

			public bool fineMaterial(M2BlurMeshDrawer Con, Material Mtr, bool on_entrying = false)
			{
				if (this.Bri != null && this.Bri.destructed)
				{
					this.deactivate(true);
					return false;
				}
				this.Gob.layer = Con.layer;
				base.setMaterial(Mtr, false);
				this.Mrd.sharedMaterial = Mtr;
				IN.setZAbs(this.Gob.transform, Con.base_z);
				if (!on_entrying && this.Bri != null)
				{
					this.activate(Con, this.Img, this.Bri);
				}
				return true;
			}

			public void activate(M2BlurMeshDrawer Con, M2ChipImage _Img, M2BlurImage _Bri)
			{
				if (!this.Gob.activeSelf)
				{
					this.Gob.SetActive(true);
					Con.Mp.M2D.Cam.connectToBinder(this.Valot);
				}
				if (this.Bri != _Bri)
				{
					this.Img = _Img;
					this.Bri = _Bri;
					this.Valot.Mpb.SetTexture("_MainTex", this.Bri.getTexture(), true);
					this.blur_level = this.Bri.level;
				}
			}

			public void deactivate(bool clear_mesh)
			{
				if (clear_mesh && this.Img != null)
				{
					this.clear(false, false);
					if (this.Img != null)
					{
						this.Img = null;
						this.Bri = null;
						this.blur_level = -1;
					}
				}
				if (this.Gob.activeSelf)
				{
					this.Valot.ReleaseBinding(true, true, false);
					this.Gob.SetActive(false);
				}
			}

			public void refine(Dungeon Dgn)
			{
				if (this.Img == null)
				{
					return;
				}
				this.Img.IMGS.Atlas.getBluredImage(this.Img, this.blur_level, Dgn);
			}

			public readonly GameObject Gob;

			private MeshRenderer Mrd;

			private M2ChipImage Img;

			private M2BlurImage Bri;

			public int blur_level = -1;
		}
	}
}

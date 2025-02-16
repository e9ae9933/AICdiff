using System;
using UnityEngine;

namespace XX
{
	public class ValotileRendererFilter : ValotileRendererSMesh
	{
		public void Init(MeshFilter _Mf, Renderer _Mrd, bool _enabled = true)
		{
			this.Filter = _Mf;
			this.render_smesh_activated = this.Filter != null;
			this.source_attached = this.Filter != null || this.Md != null;
			base.Init(_Mrd, _enabled);
		}

		public void changerFilterMode(MeshFilter _Mf)
		{
			this.Filter = _Mf;
			this.render_smesh_activated = this.Filter != null;
			this.source_attached = this.Filter != null || this.Md != null;
		}

		public override void ReleaseBinding(bool release_link = false, bool no_mesh_resample = false, bool release_mf = false)
		{
			base.ReleaseBinding(release_link, no_mesh_resample, release_mf);
			if (release_mf)
			{
				this.Filter = null;
			}
		}

		protected override bool prepareMeshAndMaterials(out Mesh Mf)
		{
			Mf = this.Filter.sharedMesh;
			return !(Mf == null);
		}

		private MeshFilter Filter;
	}
}

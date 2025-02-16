using System;
using UnityEngine;

namespace XX
{
	public class ValotileRendererSkinnedMesh : ValotileRendererSMesh
	{
		public void InitSk(SkinnedMeshRenderer Mrd, bool _enabled = true)
		{
			this.SkMrd = Mrd;
			base.Init(Mrd, true);
			this.render_smesh_activated = this.SkMrd != null;
			this.source_attached = this.render_smesh_activated || this.Md != null;
			base.Init(Mrd, _enabled);
			if (this.render_smesh_activated)
			{
				this.Trs = ((this.SkMrd.rootBone != null) ? this.SkMrd.rootBone.transform : this.SkMrd.transform);
			}
		}

		protected override bool prepareMeshAndMaterials(out Mesh Mf)
		{
			Mf = this.SkMrd.sharedMesh;
			return !(Mf == null);
		}

		public override float getFarLength()
		{
			return base.transform.position.z + ((this.Md != null) ? this.Md.base_z : 0f);
		}

		private SkinnedMeshRenderer SkMrd;
	}
}

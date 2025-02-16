using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public abstract class ValotileRendererSMesh : ValotileRenderer
	{
		protected void prepareMeshAndMaterials()
		{
			if (ValotileRendererSMesh.AMtrForFilter == null)
			{
				ValotileRendererSMesh.AMtrForFilter = new List<Material>(1);
			}
		}

		protected override bool Render(ref bool pre_drawn, ref byte bounds_clip_, ref DRect RcBounds, ProjectionContainer JCon, Matrix4x4 Mx, Material[] AMtrForAlias = null)
		{
			if (!this.render_smesh_activated)
			{
				return base.Render(ref pre_drawn, ref bounds_clip_, ref RcBounds, JCon, Mx, AMtrForAlias);
			}
			if (this.Mrd != null)
			{
				Mesh mesh;
				if (!this.prepareMeshAndMaterials(out mesh))
				{
					return true;
				}
				pre_drawn = false;
				this.prepareMeshAndMaterials();
				ValotileRendererSMesh.AMtrForFilter.Clear();
				this.Mrd.GetSharedMaterials(ValotileRendererSMesh.AMtrForFilter);
				int count = ValotileRendererSMesh.AMtrForFilter.Count;
				for (int i = 0; i < count; i++)
				{
					Material material = ValotileRendererSMesh.AMtrForFilter[i];
					if (material != null)
					{
						material.SetPass(i);
						pre_drawn = true;
					}
				}
				if (!pre_drawn)
				{
					return false;
				}
				GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed * Mx);
				Graphics.DrawMeshNow(mesh, Matrix4x4.identity, 0);
			}
			return true;
		}

		protected abstract bool prepareMeshAndMaterials(out Mesh Mf);

		protected static List<Material> AMtrForFilter;

		protected bool render_smesh_activated;
	}
}

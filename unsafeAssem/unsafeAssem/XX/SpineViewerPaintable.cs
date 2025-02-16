using System;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace XX
{
	public class SpineViewerPaintable : SpineViewer
	{
		public SpineViewerPaintable(string _key)
			: base(_key)
		{
			this.TexP = new RenderTexture(this.Tex.width, this.Tex.height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			if (SpineViewerPaintable.MtrClearing == null)
			{
				SpineViewerPaintable.MtrClearing = MTRX.newMtr("Buffer/NormalClearing");
			}
			this.TexP.name = this.Tex.name;
			this.TexP.Create();
			this.Tex0 = this.Tex;
			this.Tex = this.TexP;
			this.resetTexture();
		}

		public void resetTexture()
		{
			Graphics.Blit(this.Tex0, this.TexP, SpineViewerPaintable.MtrClearing);
		}

		public void addSeparatorSlotName(params string[] args)
		{
			if (this.charaAnim.separatorSlotNames == null)
			{
				this.charaAnim.separatorSlotNames = args;
				return;
			}
			X.pushA<string>(ref this.charaAnim.separatorSlotNames, args);
		}

		public override void fineAtlasMaterial()
		{
			this.Mtr.mainTexture = this.Tex;
			if (this.MtrDefDraw == null)
			{
				this.MtrDefDraw = MTRX.newMtr(this.Mtr);
				this.MtrDefDraw.SetTexture("_MainTex", this.Tex0);
				this.MtrDefDraw.name = this.Mtr.name + " -DefDraw";
			}
			if (this.charaAnim.separatorSlotNames != null && this.charaAnim.separatorSlotNames.Length != 0)
			{
				Debug.Log("separated:" + TX.join<string>(",", this.charaAnim.separatorSlotNames, 0, -1));
				Skeleton skeleton = this.charaAnim.Skeleton;
				int num = this.charaAnim.separatorSlotNames.Length + 1;
				if (this.AMtrReplace == null || this.AMtrReplace.Length != num)
				{
					this.AMtrReplace = new Material[num];
				}
				for (int i = 0; i < num; i++)
				{
					this.AMtrReplace[i] = ((i % 2 == 0) ? this.Mtr : this.MtrDefDraw);
				}
				this.Mrd.sharedMaterials = this.AMtrReplace;
				this.SpAtlasAsset.materials = this.AMtrReplace;
				this.Mrd.sharedMaterials = this.AMtrReplace;
				return;
			}
			base.fineAtlasMaterial();
		}

		public override void clearAnim(string anim_name, int loopf = -1000, string skin_name = null)
		{
			base.clearAnim(anim_name, loopf, skin_name);
		}

		protected override void initializeAnimState(string skin_name)
		{
			base.initializeAnimState(skin_name);
			if (this.AMtrReplace != null)
			{
				if (this.Separator == null)
				{
					this.Separator = SkeletonRenderSeparator.AddToSkeletonRenderer(this.charaAnim, 0, 0, 5, 0, true);
					this.Separator.copyMeshRendererFlags = true;
					this.Separator.copyPropertyBlock = true;
				}
				this.Mrd.sharedMaterials = this.AMtrReplace;
				int num = X.Mn(this.Separator.partsRenderers.Count, this.AMtrReplace.Length);
				for (int i = 0; i < num; i++)
				{
					SkeletonPartsRenderer skeletonPartsRenderer = this.Separator.partsRenderers[i];
					skeletonPartsRenderer.auto_rewrite_material = false;
					skeletonPartsRenderer.MeshRenderer.sharedMaterial = this.AMtrReplace[i];
				}
			}
		}

		public Material DrMaterialInit(Material _Mtr, Texture NTex, bool reset_texture = true, string source_name = "_MainTex", string _ntex_name = "_NTex")
		{
			Texture texture = (reset_texture ? this.Tex0 : this.TexP);
			_Mtr.SetTexture(source_name, texture);
			_Mtr.SetTexture(_ntex_name, NTex);
			return _Mtr;
		}

		public void Blit(Material _Mtr)
		{
			Graphics.Blit(_Mtr.mainTexture, this.TexP, _Mtr);
		}

		protected Texture Tex0;

		protected RenderTexture TexP;

		protected Material MtrDefDraw;

		private Material[] AMtrReplace;

		private SkeletonRenderSeparator Separator;

		private static Material MtrClearing;
	}
}

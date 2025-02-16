using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2CImgAnimate : M2CImgDrawer
	{
		public M2CImgAnimate(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, true)
		{
			PxlCharacter pChar = this.Cp.Img.SourceLayer.pFrm.pChar;
			PxlPose poseByName = pChar.getPoseByName(base.Meta.GetSi(1, "animate"));
			if (poseByName == null)
			{
				X.de("キャラクター" + pChar.title + "にPoseがありません: " + base.Meta.GetS("animate"), null);
				return;
			}
			this.Sq = poseByName.getSequence(0);
		}

		protected override void entryMainPicToMeshInner(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			base.entryMainPicToMeshInner(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			this.ver_i_anim = Md.getVertexMax();
			this.tri_i_anim = Md.getTriMax();
			if (this.Sq != null)
			{
				this.sqx = base.Meta.GetNm("animate", 0f, 2) + meshx;
				this.sqy = base.Meta.GetNm("animate", 0f, 3) + meshy;
				this.current_floort = base.Mp.floort;
				this.redrawAnim();
			}
		}

		protected override bool use_mdarray
		{
			get
			{
				return false;
			}
		}

		public override int redraw(float fcnt)
		{
			if (this.index_last <= this.index_first || this.Sq == null)
			{
				return 0;
			}
			PxlFrame frame = this.Sq.getFrame(this.cframe);
			if (base.Mp.floort - this.current_floort >= (float)frame.crf60)
			{
				int num = this.cframe + 1;
				this.cframe = num;
				if (num >= this.Sq.countFrames())
				{
					this.cframe = this.Sq.loop_to;
				}
				int vertexMax = this.Md.getVertexMax();
				int triMax = this.Md.getTriMax();
				this.Md.revertVerAndTriIndex(this.ver_i_anim, this.tri_i_anim, false);
				this.Md.Col = base.Lay.LayerColor.C;
				this.redrawAnim();
				this.Md.revertVerAndTriIndex(vertexMax, triMax, false);
				return base.layer2update_flag;
			}
			return 0;
		}

		private void redrawAnim()
		{
			PxlFrame frame = this.Sq.getFrame(this.cframe);
			Matrix4x4 currentMatrix = this.Md.getCurrentMatrix();
			int num = frame.countLayers();
			Texture tx = base.M2D.IMGS.MIchip.Tx;
			float num2 = 1f / (float)tx.width;
			float num3 = 1f / (float)tx.height;
			PxlImage pxlImage = null;
			M2ImageAtlas.AtlasRect atlasRect = default(M2ImageAtlas.AtlasRect);
			Matrix4x4 matrix4x = currentMatrix * Matrix4x4.Translate(new Vector3(this.sqx * 0.015625f, this.sqy * 0.015625f, 0f));
			matrix4x = matrix4x * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, this.Cp.draw_rotR * 0.31830987f * 180f)) * Matrix4x4.Scale(new Vector3((float)(this.Cp.flip ? (-1) : 1), 1f, 1f));
			this.Md.setCurrentMatrix(matrix4x, false);
			int num4 = this.Md.getVertexMax();
			for (int i = 0; i < num; i++)
			{
				PxlLayer layer = frame.getLayer(i);
				if (pxlImage != layer.Img)
				{
					pxlImage = layer.Img;
					atlasRect = base.M2D.IMGS.Atlas.getAtlasData(layer.Img);
				}
				this.Md.RotaL(0f, 0f, layer, true, false, 0);
				if (atlasRect.valid)
				{
					this.Md.InputImageUv((float)atlasRect.x * num2, (float)atlasRect.y * num3, (float)atlasRect.w * num2, (float)atlasRect.h * num3, num4, true);
				}
				num4 = this.Md.getVertexMax();
			}
			this.Md.setCurrentMatrix(currentMatrix, false);
		}

		private PxlSequence Sq;

		private float sqx;

		private float sqy;

		private int cframe;

		private float current_floort;

		private int ver_i_anim;

		private int tri_i_anim;
	}
}

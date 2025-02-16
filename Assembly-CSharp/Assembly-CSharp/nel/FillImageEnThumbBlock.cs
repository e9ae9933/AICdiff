using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class FillImageEnThumbBlock : FillImageBlock
	{
		public static FillImageEnThumbBlock makeEnemyThumbnail(Designer Ds, float w, float h, ENEMYID _enemy_id = (ENEMYID)0U)
		{
			FillImageEnThumbBlock fillImageEnThumbBlock = Ds.addImgT<FillImageEnThumbBlock>(new DsnDataImg
			{
				swidth = w,
				sheight = h
			});
			fillImageEnThumbBlock.enemy_id = _enemy_id;
			return fillImageEnThumbBlock;
		}

		public void InitEnemy(NelM2DBase _M2D, ENEMYID _enemy_id)
		{
			if (this.enemy_id == _enemy_id)
			{
				return;
			}
			this.enemy_id = _enemy_id;
			this.Md.clear(false, false);
			this.M2D = _M2D;
			this.Pc = null;
			this.Sq = null;
			this.PF = null;
			this.EnBasic = null;
			this.pre_cframe = 0;
			this.use_valotile = true;
			if (this.Anormal_rend != null)
			{
				this.Anormal_rend.Clear();
			}
			if (this.enemy_id == (ENEMYID)0U)
			{
				this.t_pc_load = 0f;
				return;
			}
			this.t_pc_load = -20f;
		}

		protected override bool runIRD(float fcnt)
		{
			if (this.t_pc_load < -1f)
			{
				this.t_pc_load += fcnt;
				if (this.t_pc_load >= -1f)
				{
					this.t_pc_load = -1f;
					ENEMYID enemyid = this.enemy_id & (ENEMYID)2147483647U;
					string text = FEnum<ENEMYID>.ToStr(enemyid);
					this.EnBasic = NOD.getBasicData(text);
					this.Pc = null;
					if (this.EnBasic != null && TX.valid(this.EnBasic.anim_chara_name))
					{
						if (NDAT.typeIs(enemyid, ENEMYID.MECHGOLEM_0))
						{
							if (this.Anormal_rend == null)
							{
								this.Anormal_rend = new List<string>(2);
							}
							this.Anormal_rend.Add("gun");
							this.Anormal_rend.Add("mech");
							this.Anormal_rend.Add("pod");
							this.Anormal_rend.Add("missile");
						}
						if (this.Aloaded == null)
						{
							this.Aloaded = new List<ENEMYID>(16);
						}
						string text2 = this.EnBasic.anim_chara_name;
						string text3 = text2;
						if (this.Aloaded.IndexOf(enemyid) == -1)
						{
							this.Aloaded.Add(enemyid);
							if (TX.isStart(text2, "N_", 0))
							{
								text2 = TX.slice(text2, "N_".Length);
							}
							if (NDAT.loadPxl(this.M2D, text2, text3, true))
							{
								COOK.map_walk_count = 99;
							}
						}
						this.Pc = PxlsLoader.getPxlCharacter(text3);
					}
					else
					{
						this.enemy_id = (ENEMYID)0U;
					}
				}
			}
			if (this.t_pc_load == -1f && this.Pc != null && this.Pc.isLoadCompleted())
			{
				MImage mi = MTRX.getMI(this.Pc, true);
				if (mi == null || mi.Tx == null)
				{
					return true;
				}
				this.t_pc_load = 0f;
				if (!this.Md.hasMultipleTriangle())
				{
					this.Md.InitSubMeshContainer(0);
					Material material = new Material(MTRX.ShaderGDT);
					this.Md.setMaterial(material, true);
					material.EnableKeyword("_USEADDCOLOR_ON");
					material.SetColor("_AddColor", C32.MulA(4294905358U, 0f));
				}
				this.Md.chooseSubMesh(0, false, false);
				this.Md.getMaterial().mainTexture = mi.Tx;
				this.Mtr = mi.getMtr(MTR.ShaderEnemyDark, this.stencil_ref);
				this.M2D.ENMTR.assign(this.Mtr);
				this.MtrMech = mi.getMtr(BLEND.NORMAL, this.stencil_ref);
				this.MtrEye = mi.getMtr(BLEND.ADD, this.stencil_ref);
				this.Md.connectRendererToTriMulti(this.Mrd);
				if (this.Pc != null)
				{
					PxlPose pxlPose;
					if (this.EnBasic.FG_pose != null)
					{
						pxlPose = this.Pc.getPoseByName(this.EnBasic.FG_pose);
					}
					else
					{
						pxlPose = this.Pc.getPoseByName(((this.enemy_id & (ENEMYID)2147483648U) != (ENEMYID)0U) ? "od_stand" : "stand") ?? this.Pc.getPose(this.Pc.countPoses() - 1);
					}
					if (pxlPose != null)
					{
						this.Sq = pxlPose.getSequence(0);
						this.initFrame();
					}
				}
				this.redraw_flag = true;
			}
			if (this.Sq != null)
			{
				this.t_pc_load += fcnt;
				if ((float)this.Sq.getFrame(this.pre_cframe).crf60 <= this.t_pc_load)
				{
					this.t_pc_load = 0f;
					if (this.pre_cframe >= this.Sq.countFrames() - 1)
					{
						this.pre_cframe = this.Sq.loop_to;
					}
					else
					{
						this.pre_cframe++;
					}
					this.redraw_flag = true;
					this.initFrame();
				}
			}
			return base.runIRD(fcnt);
		}

		private void initFrame()
		{
			if (this.Sq == null)
			{
				return;
			}
			if (this.Aframe_bits == null)
			{
				this.Aframe_bits = new List<uint>(4);
			}
			this.Aframe_bits.Clear();
			this.PF = this.Sq.getFrame(this.pre_cframe);
			int num = this.PF.countLayers();
			int num2 = 0;
			uint num3 = 0U;
			for (int i = 0; i < num; i++)
			{
				PxlLayer layer = this.PF.getLayer(i);
				if (!TX.isStart(layer.name, "point", 0) && layer.alpha != 0f)
				{
					int num4;
					if (TX.isStart(layer.name, "eye", 0))
					{
						num4 = 3;
					}
					else if (this.EnBasic.is_machine || TX.isStart(layer.name, this.Anormal_rend))
					{
						num4 = 2;
					}
					else
					{
						num4 = 1;
					}
					if (num2 != num4)
					{
						if (num3 != 0U && num2 > 0)
						{
							this.Aframe_bits.Add(num3);
							this.Md.chooseSubMesh(this.Aframe_bits.Count, false, false);
							this.Md.setMaterial((num2 == 3) ? this.MtrEye : ((num2 == 2) ? this.MtrMech : this.Mtr), false);
						}
						num2 = num4;
						num3 = 0U;
					}
					num3 |= 1U << i;
				}
			}
			if (num3 != 0U && num2 > 0)
			{
				this.Aframe_bits.Add(num3);
				this.Md.chooseSubMesh(this.Aframe_bits.Count, false, false);
				this.Md.setMaterial((num2 == 3) ? this.MtrEye : ((num2 == 2) ? this.MtrMech : this.Mtr), false);
			}
		}

		protected override void redrawMesh()
		{
			if (this.PF == null || this.EnBasic == null)
			{
				this.Md.clear(false, false);
				return;
			}
			float num;
			if ((this.enemy_id & (ENEMYID)2147483648U) != (ENEMYID)0U)
			{
				num = (float)this.Sq.height * 0.5f - (float)this.EnBasic.sizew_od_y * 0.5f;
			}
			else
			{
				num = ((this.EnBasic.sizew_y <= 0f) ? 10f : ((float)this.Sq.height * 0.5f - this.EnBasic.sizew_y * 0.5f + 16f)) * this.draw_scale;
			}
			int num2 = this.PF.countLayers();
			this.Md.allocUv23(num2 * 4 * 2, false);
			this.Md.Uv2(1f / this.scale, 1f / this.scale, false);
			this.Md.Uv3(0f, 0f, false);
			if (!this.EnBasic.is_machine)
			{
				uint ran = X.GETRAN2((IN.totalframe >> 2) + 19, 16);
				float num3 = 1f + 0.028f * X.RAN(ran, 2963);
				float num4 = 1f + 0.028f * X.RAN(ran, 2504);
				float num5 = (-1f + X.RAN(ran, 1070) * 2f) * 1.3f * 0.017453292f;
				this.Md.Col = C32.MulA(4294905358U, base.alpha);
				this.Md.chooseSubMesh(0, false, true);
				this.Md.RotaPF(0f, num, this.draw_scale * num3, this.draw_scale * num4, num5, this.PF, false, false, false, uint.MaxValue, false, 0);
			}
			int count = this.Aframe_bits.Count;
			for (int i = 0; i < count; i++)
			{
				this.Md.chooseSubMesh(i + 1, false, true);
				this.Md.Col = C32.MulA((this.Md.getMaterial() == this.MtrEye) ? 4294905358U : uint.MaxValue, base.alpha);
				this.Md.RotaPF(0f, num, this.draw_scale, this.draw_scale, 0f, this.PF, false, false, false, this.Aframe_bits[i], false, 0);
			}
			this.Md.allocUv23(0, true);
			this.Md.updateForMeshRenderer(false);
		}

		private ENEMYID enemy_id;

		private List<ENEMYID> Aloaded;

		private PxlCharacter Pc;

		private PxlSequence Sq;

		private NOD.BasicData EnBasic;

		private float t_pc_load;

		private int pre_cframe;

		public float draw_scale = 1f;

		private List<string> Anormal_rend;

		private List<uint> Aframe_bits;

		private Material MtrMech;

		private Material MtrEye;

		private NelM2DBase M2D;
	}
}

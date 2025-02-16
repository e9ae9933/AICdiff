using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class EnemyAnimatorGolemToy : EnemyAnimator
	{
		public EnemyAnimatorGolemToy(NelNGolemToy _Mv, EnemyAnimator.FnCreate _fnConvertPxlFrame, EnemyAnimator.FnFineFrame _fnFineFrame = null)
			: base(_Mv, _fnConvertPxlFrame, _fnFineFrame, true)
		{
			this.Toy = _Mv;
			if (this.ABone == null)
			{
				this.ABone = new List<Vector3>(6);
			}
		}

		public override void initS(M2PxlAnimatorRT _Anm)
		{
			base.order_back = M2Mover.DRAW_ORDER.N_BACK0;
			base.order_front = M2Mover.DRAW_ORDER.N_TOP0;
			base.initS(_Anm);
			this.SqBone = _Anm.getCurrentCharacter().getPoseByName("making_bone").getSequence(0);
		}

		protected override void redrawBodyMeshInner()
		{
			if (this.Mv.disappearing)
			{
				return;
			}
			this.need_fine_mesh = false;
			this.Md.Identity();
			if (!this.Toy.create_finished)
			{
				this.redrawBone();
				return;
			}
			if (!this.Toy.drawSpecial(this.Md))
			{
				base.redrawBodyMeshInner();
			}
			this.Md.Identity();
			this.Toy.drawAfter(this.Md);
		}

		public override bool checkFrame()
		{
			return this.Toy.create_finished && this.allow_check_main && base.checkFrame(this.Mv.TS * base.D_AF, false);
		}

		private void redrawBone()
		{
			if (this.SqBone == null)
			{
				return;
			}
			if (this.ABone.Count == 0)
			{
				this.Toy.makeBone(this.ABone);
			}
			int count = this.ABone.Count;
			int num = 0;
			int num2 = 0;
			float hp_ratio = this.Mv.hp_ratio;
			float num3 = -this.Mv.sizey * base.CLEN / this.Anm.scaleY - 24f;
			for (int i = 0; i < count; i++)
			{
				Vector3 vector = this.ABone[i];
				float num4 = this.Mv.state_time - vector.z;
				int num5 = (int)vector.y;
				if (num4 <= 0f)
				{
					num = (num + num5 * 5) % 4;
					num2 = (num2 + num5) % 4;
					this.need_fine_mesh = true;
				}
				else
				{
					float num6 = num3 - (vector.y - (float)num5) * 18.6f;
					float num7 = vector.x - 13.5f;
					float num8 = X.ZSIN(num4, (float)(11 * num5));
					float num9 = 1f / (float)num5;
					for (int j = 0; j < num5; j++)
					{
						float num10 = (num8 - num9 * (float)j) * (float)num5;
						if (num10 <= 0f)
						{
							num = (num + (num5 - j) * 5) % 4;
							num2 = (num2 + (num5 - j)) % 4;
							this.need_fine_mesh = true;
							break;
						}
						num10 = X.Mn(num10, 1f);
						if (num10 < 1f)
						{
							this.need_fine_mesh = true;
						}
						float num11 = num6 + num10 * 18.6f;
						if (++num >= 4)
						{
							num = 0;
						}
						this.Md.initForImg(this.SqBone.getImage(0, num), 0);
						this.Md.Line(num7, num6, num7, num11, 2f, false, 0f, 0f);
						if (++num >= 4)
						{
							num = 0;
						}
						this.Md.initForImg(this.SqBone.getImage(0, num), 0);
						this.Md.Line(num7 + 27f, num6, num7 + 27f, num11, 2f, false, 0f, 0f);
						if (++num >= 4)
						{
							num = 0;
						}
						this.Md.initForImg(this.SqBone.getImage(0, num), 0);
						this.Md.Line(num7, num11, num7 + 27f, num11, 2f, false, 0f, 0f);
						if (++num >= 4)
						{
							num = 0;
						}
						this.Md.initForImg(this.SqBone.getImage(0, num), 0);
						this.Md.Line(num7, num6, num7 + 27f, num11, 2f, false, 0f, 0f);
						if (++num >= 4)
						{
							num = 0;
						}
						if (num10 >= 1f && hp_ratio * (float)num5 >= (float)j)
						{
							this.Md.initForImg(this.SqBone.getImage(1, num2), 0);
							this.Md.RotaGraph(num7 + 13.5f, num6 + 9.3f, 0.61f, 0f, null, false);
						}
						if (++num2 >= 4)
						{
							num2 = 0;
						}
						num6 = num11 + 2.25f;
					}
				}
			}
		}

		public void destructEffect()
		{
			if (!this.Toy.create_finished)
			{
				if (this.Mv.disappearing || this.ABone.Count == 0 || this.SqBone == null)
				{
					this.Mv.snd_die = "";
					return;
				}
				if (this.FD_BoneDrawDro == null)
				{
					this.FD_BoneDrawDro = new M2DropObject.FnDropObjectDraw(this.BoneDrawDro);
				}
				M2DropObjectReader m2DropObjectReader = M2DropObjectReader.Get("golemtoy_bone_break", false);
				int count = this.ABone.Count;
				float num = this.Mv.y + this.Mv.sizey + 24f * this.Anm.scaleY * this.Mp.rCLEN;
				float num2 = 27f * this.Mp.rCLEN * this.Anm.scaleX;
				float num3 = 18.6f * this.Mp.rCLEN * this.Anm.scaleY;
				float num4 = 2.25f * this.Mp.rCLEN * this.Anm.scaleY;
				for (int i = 0; i < count; i++)
				{
					Vector3 vector = this.ABone[i];
					int num5 = (int)vector.y;
					float num6 = num + (vector.y - (float)num5) * num3;
					float num7 = this.Mv.x + vector.x * this.Mp.rCLEN * this.Anm.scaleX - num2 * 0.5f;
					for (int j = 0; j < num5; j++)
					{
						m2DropObjectReader.createObjects(this.Mp, this.FD_BoneDrawDro, num7, num6, 1f, 1f, 1.5707964f, null);
						m2DropObjectReader.createObjects(this.Mp, this.FD_BoneDrawDro, num7 + num2, num6, 1f, 1f, 1.5707964f, null);
						m2DropObjectReader.createObjects(this.Mp, this.FD_BoneDrawDro, num7 + num2 * 0.5f, num6, 1f, 1f, 0.5497787f, null);
						m2DropObjectReader.createObjects(this.Mp, this.FD_BoneDrawDro, num7 + num2 * 0.5f, num6 + num3, 1f, 1f, 0f, null);
						num6 -= num3 + num4;
					}
				}
			}
		}

		private bool BoneDrawDro(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			if (Dro.on_ground && X.Abs(Dro.vy) < 0.004f)
			{
				return false;
			}
			MeshDrawer mesh = Ef.GetMesh("", this.MtrBase, false);
			float num = Dro.z - 1.5707964f + Dro.af / Dro.time * 6.2831855f * (float)X.MPF(Dro.index % 2 == 1);
			mesh.initForImg(this.SqBone.getImage(0, Dro.index % 4), 0);
			mesh.RotaGraph(0f, 0f, 0.7f, num, null, false);
			return true;
		}

		public float vib_y
		{
			get
			{
				return this.vib_y_;
			}
			set
			{
				if (this.vib_y == value)
				{
					return;
				}
				this.vib_y_ = value;
				this.need_fine_mesh = true;
			}
		}

		public ENEMYID toy_kind
		{
			get
			{
				return this.Toy.toy_kind;
			}
		}

		private NelNGolemToy Toy;

		private PxlSequence SqBone;

		private float vib_y_;

		private const float box_h = 18.6f;

		private const float box_w = 27f;

		private const float boneshift_y = 24f;

		private const float boneslide_y = 2.25f;

		private List<Vector3> ABone;

		public bool allow_check_main = true;

		private M2DropObject.FnDropObjectDraw FD_BoneDrawDro;
	}
}

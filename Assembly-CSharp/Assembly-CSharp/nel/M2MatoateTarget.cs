using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2MatoateTarget : MonoBehaviour, IM2RayHitAble
	{
		public void appear(Map2d _Mp, MatoateReader _Reader, MatoateReader.MatoItem _Mato, Vector2 MapPos, int _index)
		{
			this.Mp = _Mp;
			this.Mato = _Mato;
			this.index = _index;
			this.x = (this.x0 = MapPos.x);
			this.y = (this.y0 = MapPos.y);
			this.agR0 = this.Mato.agR;
			this.f0 = this.Mp.floort;
			this.Reader = _Reader;
			this.level_addition = 1f / this.Mato.rot_duration;
			this.level = this.Mato.start_level;
			this.finished = 0;
			if (this.Cld == null)
			{
				this.prepareCollider();
			}
			base.gameObject.layer = IN.LAY("Ignore Raycast");
			base.transform.localScale = new Vector3(1f, 1f, 1f);
			this.finePosision();
			this.defineParticlePreVariable();
			this.Mp.PtcST("matoate_appeared", null, PTCThread.StFollow.NO_FOLLOW);
		}

		public void prepareCollider()
		{
			this.Cld = base.gameObject.AddComponent<CircleCollider2D>();
			this.Cld.radius = this.Mp.CLEN * 0.8f * 0.015625f;
		}

		public int run()
		{
			if (this.finished > 0 || this.Mato.mvtype == MatoateReader.MVTYPE.STOP)
			{
				return (int)this.finished;
			}
			if (this.Mato.hold >= 0 && this.Mp.floort - this.f0 >= (float)this.Mato.hold)
			{
				this.finished = 1;
				return (int)this.finished;
			}
			this.level += this.level_addition * Map2d.TS;
			this.finePosision();
			return (int)this.finished;
		}

		public void OnDisable()
		{
			if (this.Mp != null && this.finished <= 1)
			{
				this.defineParticlePreVariable();
				this.Mp.PtcST("matoate_disappeared", null, PTCThread.StFollow.NO_FOLLOW);
			}
		}

		private void finePosision()
		{
			MatoateReader.MVTYPE mvtype = this.Mato.mvtype;
			switch (mvtype)
			{
			case MatoateReader.MVTYPE.SIN_H:
				this.x = this.x0 + 4.8f * X.Sin(this.level * 6.2831855f);
				break;
			case MatoateReader.MVTYPE.SIN_V:
				this.y = this.y0 + 4.8f * X.Sin(this.level * 6.2831855f);
				break;
			case MatoateReader.MVTYPE.CIRCLE:
			case MatoateReader.MVTYPE.CIRCLE_H:
			case MatoateReader.MVTYPE.CIRCLE_V:
				this.x = this.x0 + 6f * X.Cos(this.level * 6.2831855f) * ((mvtype == MatoateReader.MVTYPE.CIRCLE_V) ? 0.25f : 1f);
				this.y = this.y0 + 6f * X.Sin(this.level * 6.2831855f) * ((mvtype == MatoateReader.MVTYPE.CIRCLE_H) ? 0.25f : 1f);
				break;
			}
			base.transform.localPosition = new Vector3(this.Mp.map2ux(this.x), this.Mp.map2uy(this.y), 0f);
		}

		public bool drawMatoateTarget(EffectItem Ef, M2DrawBinder Ed, ref MeshDrawer Md, ref MeshDrawer MdA)
		{
			if (!Ed.isinCamera(this.x, this.y, 2f, 4f))
			{
				return true;
			}
			if (Md == null)
			{
				Md = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, true);
				MdA = Ef.GetMesh("", 4294381327U, BLEND.ADD, true);
			}
			float num = this.Mp.floort - this.f0;
			uint ran = X.GETRAN2(this.index + 133, (this.index & 7) + 1);
			float num2 = 0.3f * X.COSI(num + X.RAN(ran, 2641) * 130f, X.NI(180, 200, X.RAN(ran, 1163)));
			float num3 = 0.3f * X.SINI(num + X.RAN(ran, 1482) * 130f, X.NI(240, 330, X.RAN(ran, 2778)));
			float num4 = X.Abs(X.COSI(num + X.RAN(ran, 1234) * 130f, X.NI(100, 130, X.RAN(ran, 1454))));
			Md.base_x = (MdA.base_x = this.Mp.ux2effectScreenx(this.Mp.map2ux(this.x + num2)));
			Md.base_y = (MdA.base_y = this.Mp.uy2effectScreeny(this.Mp.map2uy(this.y + num3)));
			PxlSequence sqMatoateTarget = MTR.SqMatoateTarget;
			Md.RotaPF(0f, 0f, 2f, 2f, 0f, sqMatoateTarget.getFrame(X.ANM((int)num, 3, 3f)), false, false, false, uint.MaxValue, false, 0);
			Md.RotaPF(0f, 0f, 2f * num4, 2f, 0f, sqMatoateTarget.getFrame(4), false, false, false, uint.MaxValue, false, 0);
			PxlFrame frame = sqMatoateTarget.getFrame(3);
			Md.RotaPF(num2 * 4.5f, -num3 * 4.5f, 2f, 2f, 0f, frame, false, false, false, uint.MaxValue, false, 0);
			float num5 = X.ANMP((int)num, 45, 1f);
			float num6 = (32f + 50f * num5) * 2f;
			float num7 = 18f * (1f - num5 * 0.99f);
			MdA.Daia3(num2 * 4.5f, -num3 * 4.5f - frame.getLayer(0).y * 2f, num6, num6, num7, num7, false);
			return true;
		}

		public RAYHIT can_hit(M2Ray Ray)
		{
			return (RAYHIT)3;
		}

		public int applyHpDamage(int val, bool force = false, AttackInfo _Atk = null)
		{
			if (this.finished > 0)
			{
				return 0;
			}
			NelAttackInfo nelAttackInfo = _Atk as NelAttackInfo;
			if (nelAttackInfo == null || nelAttackInfo.AttackFrom == null || nelAttackInfo.PublishMagic == null)
			{
				return 0;
			}
			if (this.Reader.killable_not_pr || nelAttackInfo.Caster is M2MoverPr)
			{
				this.finished = 2;
				this.defineParticlePreVariable();
				this.Mp.PtcST("matoate_defeated", null, PTCThread.StFollow.NO_FOLLOW);
				return 1;
			}
			return 0;
		}

		public HITTYPE getHitType(M2Ray Ray)
		{
			return HITTYPE.EN;
		}

		public bool isActive()
		{
			return this.finished == 0;
		}

		public virtual void defineParticlePreVariable()
		{
			this.Mp.PtcSTsetVar("cx", (double)this.x).PtcSTsetVar("cy", (double)(this.y - 0.8f));
		}

		private Map2d Mp;

		private MatoateReader.MatoItem Mato;

		private MatoateReader Reader;

		public float x;

		public float y;

		public float x0;

		public float y0;

		public float agR0;

		private float f0;

		private float level_addition;

		private float level;

		private byte finished;

		private int index;

		private CircleCollider2D Cld;
	}
}

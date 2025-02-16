using System;
using m2d;
using XX;

namespace nel
{
	public sealed class EnemyHpMpBar
	{
		public EnemyHpMpBar(NelEnemy _En)
		{
			this.En = _En;
			this.Mp = this.En.Mp;
			this.MdIn = new MeshDrawer(null, 4, 6);
			this.MdIn.draw_gl_only = true;
			this.MdIn.activate("hpmpbar", MTRX.MtrMeshNormal, false, MTRX.ColWhite, null);
			this.En.Mp.setED("HpMpBar-" + this.En.index.ToString(), new M2DrawBinder.FnEffectBind(this.drawED), 0f);
			this.active_flag = 0;
		}

		public void fine()
		{
			this.MdIn.clear(false, false);
			this.MdIn.Col = C32.d2c(2281701376U);
			this.MdIn.BoxBL(-43f, -2f, 87f, 4f, 0f, false);
			if (this.En.get_hp() > 0f)
			{
				this.MdIn.Col = C32.d2c(3739510374U);
				int num = X.IntC(42f * this.En.hp_ratio);
				this.MdIn.Line((float)(-(float)num), 0f, 0f, 0f, 2f, false, 0f, 0f);
			}
			if (this.En.get_mp() > 0f)
			{
				this.MdIn.Col = C32.d2c(3728523232U);
				int num2 = X.IntC(42f * this.En.mp_ratio);
				this.MdIn.Line(1f, 0f, (float)(1 + num2), 0f, 2f, false, 0f, 0f);
			}
		}

		public void destruct()
		{
			this.Ed = this.Mp.remED(this.Ed);
			this.MdIn.destruct();
		}

		private bool drawED(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.En.Mp == null || this.En.getAnimator() == null)
			{
				this.Ed = null;
				return false;
			}
			if (Ed.t >= 15f)
			{
				Ed.t = 0f;
				if (this.Mp.Pr == null || !this.Mp.Pr.is_alive)
				{
					this.active_flag = 2;
				}
				else
				{
					this.active_flag = (this.En.getAI().isPrAbsorbed() ? 1 : 0);
				}
			}
			if (this.active_flag >= 2 || this.En.disappearing || this.En.getAnimator().alpha == 0f)
			{
				return true;
			}
			Ef.x = this.En.x;
			float num = X.NIL(1f, 2f, this.En.sizex - 0.4f, 3.5f);
			Ef.y = this.En.y + this.En.sizey + 0.18f * num;
			if (!Ed.isinCamera(Ef, 5f * num, 2f * num))
			{
				return true;
			}
			MeshDrawer mesh = Ef.GetMesh("Bar", MTRX.MtrMeshNormal, false);
			if (this.active_flag == 1)
			{
				mesh.Col = C32.WMulA(0.33f);
			}
			else
			{
				mesh.Col = MTRX.ColWhite;
			}
			uint ran = X.GETRAN2((int)(this.Mp.floort * 0.25f) + this.En.index, this.En.index % 8);
			mesh.RotaTempMeshDrawer(0f, 0f, num, num, (-1f + X.RAN(ran, 1070) * 2f) * 3.1415927f * 0.011f, this.MdIn, false, true, 0, -1, 0, -1);
			return true;
		}

		public override string ToString()
		{
			return "EnemyHpMpBar";
		}

		public readonly NelEnemy En;

		public readonly Map2d Mp;

		private MeshDrawer MdIn;

		private M2DrawBinder Ed;

		private const int HPBAR_W = 42;

		private byte active_flag;
	}
}

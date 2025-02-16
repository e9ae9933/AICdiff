using System;
using XX;

namespace m2d
{
	public class M2Manpu
	{
		public M2Manpu(string _key, M2Mover _Mv, int _maxt)
		{
			this.key = _key;
			this.Mv = _Mv;
			this.maxt = ((_maxt <= 0) ? M2Manpu.DEF_TIME : _maxt);
			this.dy = (int)(-_Mv.getSpHeight() - 6f - _Mv.getSpShiftY());
			SND.Ui.play("mpm2_" + this.key, false);
		}

		public void gotolast()
		{
		}

		public bool run()
		{
			int num = this.t;
			this.t = num + 1;
			return num < this.maxt || this.maxt < 0;
		}

		public void drawTo(MeshDrawer BmTo, int sx, int sy)
		{
		}

		public bool isVariable()
		{
			return this.t < this.maxt;
		}

		public bool MoverIs(M2Mover _Mv)
		{
			return this.Mv == _Mv;
		}

		public int get_maxt()
		{
			return this.maxt;
		}

		private static XorsMaker Xors;

		private string key;

		private M2Mover Mv;

		private int t;

		private int maxt;

		private int dx;

		private int dy;

		private static int DEF_TIME = 70;
	}
}

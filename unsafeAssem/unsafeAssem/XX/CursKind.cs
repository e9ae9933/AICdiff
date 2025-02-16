using System;
using PixelLiner;

namespace XX
{
	public class CursKind
	{
		public CursKind(string _key, string image_key, string _family = "", int _cx = -1000, int _cy = -1000)
		{
			this.family = _family;
			this.cx = _cx;
			this.cy = _cy;
			this.key = _key;
			this.LoadCursImg(MTRX.getPF(image_key));
		}

		private bool LoadCursImg(PxlFrame _PF)
		{
			if (_PF == null)
			{
				return this.PF != null;
			}
			this.PF = _PF;
			if (this.PF.MeshGenerator == null)
			{
				this.PF.getDrawnMeshGenerator(false, 0f, 0f, true).FinalizeForTriCache();
			}
			PxlPose pPose = this.PF.pPose;
			if (this.cx <= -1000)
			{
				this.cx = 0;
				this.cy = 0;
			}
			else
			{
				this.cx -= pPose.width / 2;
				this.cy -= pPose.height / 2;
			}
			CURS.mxw = X.Mx(CURS.mxw, pPose.width);
			CURS.mxh = X.Mx(CURS.mxh, pPose.height);
			return true;
		}

		public bool valid
		{
			get
			{
				return this.PF != null;
			}
		}

		public string key;

		public string family = "";

		public PxlFrame PF;

		public int cx;

		public int cy;
	}
}

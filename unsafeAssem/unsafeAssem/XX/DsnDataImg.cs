using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class DsnDataImg : DsnDataP
	{
		public DsnDataImg()
			: base("", false)
		{
			this.Col = MTRX.ColWhite;
		}

		public MImage MI;

		public PxlFrame PF;

		public Rect UvRect = new Rect(0f, 0f, 1f, 1f);

		public float scale = 1f;

		public bool stencil_lessequal = true;

		public MeshDrawer.FnGeneralDraw FnDraw;

		public FillImageBlock.FnDrawInFIB FnDrawInFIB;
	}
}

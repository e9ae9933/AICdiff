using System;
using UnityEngine;

namespace XX
{
	public class DsnDataHr : DsnData
	{
		public DsnDataHr H(float height)
		{
			this.margin_t = height;
			this.margin_b = 0f;
			this.line_height = 0f;
			return this;
		}

		public DsnDataHr W(float _swidth)
		{
			this.swidth = _swidth;
			return this;
		}

		public float swidth;

		public bool vertical;

		public float line_height = 1f;

		public float margin_t = 18f;

		public float margin_b = 26f;

		public float dashed_oneline_lgt;

		public float draw_width_rate = 0.75f;

		public Color32 Col = new Color32(0, 0, 0, 190);
	}
}

using System;
using UnityEngine;

namespace XX
{
	public sealed class FontStorageUtsukusi : FontStorage
	{
		public FontStorageUtsukusi(MFont _TargetFont, string _letterspace_script_key)
			: base(_TargetFont, _letterspace_script_key, 3000)
		{
			this.TargetFont.filterMode = FilterMode.Bilinear;
			this.margin = 2;
			this.xmargin = 0f;
			this.ymargin = 0f;
			this.base_height = 52f;
			this.yshift_to_baseline = -0.78f;
			this.xratio = 0.95f;
			this.xratio_1byte = 0.96f;
		}
	}
}

using System;

namespace XX
{
	public sealed class FontStorageCabin : FontStorage
	{
		public FontStorageCabin(MFont _TargetFont, string _letterspace_script_key)
			: base(_TargetFont, _letterspace_script_key, 3000)
		{
			this.margin = 2;
			this.xmargin = 0f;
			this.ymargin = 0f;
			this.base_height = 18f;
			this.yshift_to_baseline = -0.78f;
			this.xratio = 1.25f;
			this.xratio_1byte = 1f;
		}
	}
}

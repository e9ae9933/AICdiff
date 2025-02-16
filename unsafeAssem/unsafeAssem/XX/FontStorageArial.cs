using System;

namespace XX
{
	public sealed class FontStorageArial : FontStorage
	{
		public FontStorageArial(MFont _TargetFont, string _letterspace_script_key)
			: base(_TargetFont, _letterspace_script_key, 3000)
		{
			this.margin = 2;
			this.xmargin = 0.2f;
			this.ymargin = 0f;
			this.base_height = (float)this.TargetFont.fontSize * 0.78f;
			this.yshift_to_baseline = -0.88f;
			this.xratio = 1.02f;
			this.xratio_1byte = 1f;
		}
	}
}

using System;

namespace XX
{
	public sealed class FontStorageBundled : FontStorage
	{
		public FontStorageBundled(MFont _TargetFont, float _base_height, float _yshift, float _xratio, float _xratio_1byte, float _default_renderer_size, string letterspace_script_name)
			: base(_TargetFont, letterspace_script_name, 3000)
		{
			this.margin = 2;
			this.xmargin = 0f;
			this.ymargin = 0f;
			this.base_height = (float)this.TargetFont.fontSize * _base_height;
			this.yshift_to_baseline = -_yshift;
			this.defaultRendererSize_ = _default_renderer_size;
			this.xratio = _xratio;
			this.xratio_1byte = _xratio_1byte;
			base.reloadLetterSpacingScript();
		}

		protected override string getLetterSpaceScript()
		{
			if (TX.noe(this.letterspacing_script_name))
			{
				return null;
			}
			return NKT.readSpecificStreamingText(this.letterspacing_script_name, true);
		}
	}
}

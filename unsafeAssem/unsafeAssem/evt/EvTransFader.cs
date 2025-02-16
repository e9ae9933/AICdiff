using System;
using XX;

namespace evt
{
	public class EvTransFader : TransFader
	{
		public EvTransFader(TFKEY _key, float _maxt, float _pw, float _ph, float _scale = 1f)
			: base(_key, _maxt, _pw, _ph, _scale)
		{
		}

		public bool prepare(EvDrawer P, float vt_max, uint view_type)
		{
			this.release();
			if (P is TalkDrawer)
			{
				TalkDrawer talkDrawer = P as TalkDrawer;
				this.Person = talkDrawer.get_Person();
				this.Pose = this.Person.get_CurPose();
				if (this.Pose == null)
				{
					return false;
				}
				this.pw = this.Pose.swidth_px;
				this.ph = this.Pose.sheight_px;
			}
			else
			{
				EvImg drawImage = P.getDrawImage();
				if (drawImage != null)
				{
					this.pw = drawImage.swidth_px;
					this.ph = drawImage.sheight_px;
				}
				else
				{
					this.pw = (float)EV.pw;
					this.ph = (float)EV.ph;
				}
			}
			this.Dr = P;
			this.vt = X.Abs(vt_max);
			this.do_not_shape_clear = true;
			return true;
		}

		public bool fadeDrawPrepare(MeshDrawer _MdImg, MeshDrawer _MdFill, float fcnt)
		{
			if (this.vt <= 0f)
			{
				return !this.anim_reverse;
			}
			this.MdImg = _MdImg;
			this.MdFill = _MdFill;
			this.MdFill.Col = C32.d2c(this.Dr.get_gcol() | 4278190080U);
			if (!base.redraw(_MdFill, fcnt))
			{
				this.vt = 0f;
				return !this.anim_reverse;
			}
			return false;
		}

		public TransFader release()
		{
			this.vt = 0f;
			this.Dr = null;
			this.Pose = null;
			this.Person = null;
			this.MdImg = null;
			this.MdFill = null;
			return null;
		}

		private EvDrawer Dr;

		private EvPerson Person;

		private EvEmotVisibility Pose;

		private MeshDrawer MdImg;

		private MeshDrawer MdFill;

		private float vt;
	}
}

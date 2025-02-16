using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public interface ITutorialBox
	{
		void setPositionDefault();

		void setPosition(string is_left, string is_bottom);

		void destructTutoBox();

		void addActiveFlag(string f);

		void remActiveFlag(string f);

		void AddText(string tex, int time, string image_key);

		void RemText(bool whole_clear, bool immediate = false);

		void AddImage(PxlFrame Img, string tex);

		float local_z { get; set; }

		Transform getTransform();
	}
}

using System;
using UnityEngine;

namespace XX
{
	public interface IClickable
	{
		bool getClickable(Vector2 PosU, out IClickable Res);

		float getFarLength();

		void OnPointerEnter();

		void OnPointerExit();

		bool OnPointerDown();

		void OnPointerUp(bool clicking);
	}
}

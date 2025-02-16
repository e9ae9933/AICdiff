using System;
using System.Collections.Generic;
using UnityEngine;

namespace m2d
{
	public interface IM2Inputtable
	{
		List<M2Chip> MakeChip(M2MapLayer Lay, int x, int y, int opacity, int rotation, bool flip);

		List<M2Picture> MakePicture(M2MapLayer Lay, float x, float y, int opacity, int rotation, bool flip);

		bool isSame(M2Puts Cp, bool in_editor = false, int editor_curs_x = -1, int editor_curs_y = -1);

		Vector2Int getClmsAndRows(INPUT_CR cr);

		M2ChipImage getFirstImage();

		string getTitle();

		string getDirName();

		string getBaseName();

		uint getChipId();

		bool isLinked();

		bool isFavorited();

		bool isBg();
	}
}

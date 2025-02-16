using System;
using Spine;
using UnityEngine;

namespace nel
{
	public interface IMosaicDescriptor
	{
		int countMosaic(bool only_on_sensitive);

		bool getSensitiveOrMosaicRect(ref Matrix4x4 Out, int id, ref MeshAttachment OutMesh, ref Slot BelongSlot);
	}
}

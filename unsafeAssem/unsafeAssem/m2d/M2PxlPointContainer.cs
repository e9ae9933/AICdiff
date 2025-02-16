using System;
using Better;
using PixelLiner;
using XX;

namespace m2d
{
	public static class M2PxlPointContainer
	{
		public static void clear()
		{
			if (M2PxlPointContainer.OData == null)
			{
				M2PxlPointContainer.OData = new BDic<PxlPose, M2PxlPointContainer.PosePointsData>();
			}
			M2PxlPointContainer.OData.Clear();
		}

		public static void clear(PxlCharacter C)
		{
			if (M2PxlPointContainer.OData != null && C != null)
			{
				for (int i = C.countPoses() - 1; i >= 0; i--)
				{
					PxlPose pose = C.getPose(i);
					if (pose != null)
					{
						M2PxlPointContainer.OData.Remove(pose);
					}
				}
			}
		}

		public static bool isPointCreated(PxlPose P)
		{
			return M2PxlPointContainer.OData.ContainsKey(P);
		}

		public static M2PxlPointContainer.PosePointsData GetPoints(PxlPose P)
		{
			if (P == null)
			{
				return null;
			}
			if (X.Get<PxlPose, M2PxlPointContainer.PosePointsData>(M2PxlPointContainer.OData, P) == null)
			{
				return M2PxlPointContainer.OData[P] = new M2PxlPointContainer.PosePointsData();
			}
			return M2PxlPointContainer.OData[P];
		}

		public static BDic<PxlPose, M2PxlPointContainer.PosePointsData> OData;

		public sealed class PosePointsData
		{
			public static string Frame2Key(PxlFrame F)
			{
				return F.index.ToString() + "@" + F.pSq.aim.ToString();
			}

			public PosePointsData()
			{
				this.OFrmData = new BDic<string, PxlLayer[]>();
			}

			public PxlLayer[] GetPoints(PxlFrame F, bool no_make = false)
			{
				string text = M2PxlPointContainer.PosePointsData.Frame2Key(F);
				PxlLayer[] array = X.Get<string, PxlLayer[]>(this.OFrmData, text);
				if (array != null)
				{
					return array;
				}
				if (no_make)
				{
					return null;
				}
				PxlLayer[] array2;
				using (BList<PxlLayer> blist = ListBuffer<PxlLayer>.Pop(0))
				{
					int num = F.countLayers();
					for (int i = 0; i < num; i++)
					{
						PxlLayer layer = F.getLayer(i);
						if (TX.isStart(layer.name, "point_", 0))
						{
							layer.alpha = 0f;
							F.releaseDrawnMesh();
							blist.Add(layer);
						}
					}
					array2 = (this.OFrmData[text] = ((blist.Count == 0) ? null : blist.ToArray()));
					array2 = array2;
				}
				return array2;
			}

			private BDic<string, PxlLayer[]> OFrmData;
		}
	}
}

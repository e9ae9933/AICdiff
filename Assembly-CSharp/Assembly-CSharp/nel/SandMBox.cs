using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class SandMBox : MonoBehaviour
	{
		public void Start()
		{
		}

		public void Update()
		{
			if (this.t == 0)
			{
				if (MTR.preparedT)
				{
					this.t++;
					this.MMRD = base.gameObject.AddComponent<MultiMeshRenderer>();
					base.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
					this.MdS = this.MMRD.Make(MTRX.ColWhite, BLEND.SUB, null, null);
					this.Md = this.MMRD.Make(MTRX.ColWhite, BLEND.NORMAL, null, null);
					this.MdSprite = this.MMRD.Make(BLEND.NORMAL, MTRX.MIicon);
					this.TrBox = new TreasureBoxDrawer(190f);
					this.TrBox.Set(100f, 24f);
					this.TrBox.CenterIcon = MTR.ANelChars[9];
					this.Uni = new UniBrightDrawer();
					this.Uni.rot_anti_ratio = 0f;
					this.Uni.RotTime(300f, 700f);
					this.Uni2 = new UniBrightDrawer();
					this.Uni.rot_anti_ratio = 1f;
					this.Uni2.CenterCicle(0f, -1f, 160f).Count(8).Radius(30f, 90f)
						.RotTime(500f, 900f)
						.Thick(30f, 50f);
					this.Md.Col = MTRX.ColWhite;
					return;
				}
			}
			else if (this.t >= 1)
			{
				this.t++;
				this.TrBox.Level(X.ZLINE(-1f + X.ANMP(this.t, 180, 1f) * 3f) * 2f).drawTo(this.Md, this.MdS, this.MdSprite, 0f, 0f, 1f);
				this.Md.updateForMeshRenderer(false);
			}
		}

		private bool fnChanged(aBtnMeter B, float pre, float cur)
		{
			M2DBase.Instance.curMap.drawCheck(0f);
			return true;
		}

		private int t;

		private float tinterval = 9f;

		private float sx;

		private float sy;

		private MultiMeshRenderer MMRD;

		private RenderTexture Tx;

		private Sprite Src;

		private Material Mtr;

		private aBtnMeterNel Btn;

		private MeshDrawer Md;

		private MeshDrawer MdS;

		private MeshDrawer MdSprite;

		private TreasureBoxDrawer TrBox;

		private UniBrightDrawer Uni;

		private UniBrightDrawer Uni2;
	}
}

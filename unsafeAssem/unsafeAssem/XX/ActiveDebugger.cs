using System;
using m2d;
using UnityEngine;
using UnityEngine.InputSystem;

namespace XX
{
	public class ActiveDebugger : MonoBehaviour, IFontStorageListener
	{
		public void Start()
		{
			if (!X.DEBUGRELOADMTR)
			{
				Object.Destroy(this);
				return;
			}
			ActiveDebugger.Instance = this;
			Debug.Log("active debugger on");
		}

		public void runActiveDebugger()
		{
			if (MTRX.prepared && IN.getKD(Key.F9, -1))
			{
				this.ReloadF9(KEY.getModifier(null));
			}
		}

		protected virtual bool ReloadF9(MODIF md)
		{
			if (md == MODIF.NONE)
			{
				md = MODIF.OPT;
			}
			if (md == MODIF.OPT)
			{
				TX.reloadFontLetterSpace();
				TX.reloadTx(true);
				SND.Ui.play("saved", false);
			}
			else if (md == MODIF.CTRL)
			{
				M2DBase instance = M2DBase.Instance;
				if (instance != null)
				{
					instance.DGN.ColCon.reload();
					instance.curMap.closeSubMaps(false);
					instance.curMap.openSubMaps();
					instance.curMap.fineSubMap();
					instance.curMap.drawUCol();
					instance.curMap.drawCheck(0f);
				}
			}
			else if (md == MODIF.SH_OP_CT)
			{
				this.storage_recreating = true;
				FontStorage storageLogoTypeGothic = MTRX.StorageLogoTypeGothic;
				storageLogoTypeGothic.Add(this);
				int num = 100;
				while (this.storage_recreating)
				{
					storageLogoTypeGothic.TargetFont.Target.RequestCharactersInTexture(((char)num++).ToString());
				}
				X.dl("Storage の再生性を促しました。", null, false, false);
				storageLogoTypeGothic.Rem(this);
			}
			else
			{
				if (md != MODIF.SH_OP_CT_CM)
				{
					return false;
				}
				X.loadDebug();
				Debug.Log("debug.txt reloaded.");
			}
			return true;
		}

		public void getStringForListener(STB Stb)
		{
			this.storage_recreating = false;
		}

		public void entryMesh()
		{
		}

		private bool storage_recreating;

		public static ActiveDebugger Instance;
	}
}

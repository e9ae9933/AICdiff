using System;
using evt;
using UnityEngine;
using XX;

namespace nel
{
	public class SandEvent : MonoBehaviour, IEventListener
	{
		public void Awake()
		{
			EfParticleManager.do_not_access_other_file = true;
			EfParticleManager.accessableOtherFile(new string[] { "enemyod", "mg_dojo", "item" });
		}

		public void Start()
		{
		}

		public void Update()
		{
			if (this.t == 0 && MTRX.prepared && MTR.prepare1)
			{
				MTR.initT();
				EV.loadEV();
				GameObject gameObject = IN.CreateGob(base.gameObject, "-Log");
				IN.PosP(gameObject.transform, -IN.wh + 170f - 40f + 10f, -IN.hh * 0.35f, -1.4f);
				this.LogBox = new UILog(null, gameObject);
				if (this.coin_first > 0)
				{
					CoinStorage.addCount(this.coin_first, false);
				}
				this.LogBox.AddAlert("DebugEvent Start", UILogRow.TYPE.NORMAL);
				this.t++;
				CoinStorage.addCount(10000, false);
			}
			if (this.t == 1 && EV.material_prepared && MTR.preparedT && NEL.Instance != null && !NEL.Instance.enabled)
			{
				this.t = 2;
				if (TX.getCurrentFamilyName() != "_")
				{
					TX.changeFamily("_");
				}
				GameObject gameObject2 = IN.CreateGob(base.gameObject, "-baseline");
				this.Md = MeshDrawer.prepareMeshRenderer(gameObject2, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
				this.Md.Col = MTRX.ColWhite;
				this.Md.base_z = 0.1f;
				this.Md.Line(-IN.wh, 0f, IN.wh, 0f, 1f, false, 0f, 0f);
				this.Md.Line(0f, -IN.hh, 0f, IN.hh, 1f, false, 0f, 0f);
				this.Md.updateForMeshRenderer(false);
				EV.initEvent(this.MsgCon = new NelMSGContainer(), null, new GameObject("Ev-Tutorial").AddComponent<TutorialBox>());
				EV.initDebugger(true);
				EV.addListener(NEL.Instance);
				EV.addListener(this);
				NelItem.fineNameLocalizedWhole();
			}
			int num = this.t;
		}

		public bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			return false;
		}

		public bool EvtOpen(bool is_first_or_end)
		{
			if (is_first_or_end)
			{
				this.LogBox.deactivateEventTemporary();
			}
			return true;
		}

		public bool EvtClose(bool is_first_or_end)
		{
			if (is_first_or_end)
			{
				this.LogBox.activate();
			}
			return true;
		}

		public int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		public bool EvtMoveCheck()
		{
			return true;
		}

		[SerializeField]
		private int coin_first;

		private int t;

		private MeshDrawer Md;

		private int ev_delay = 100;

		private NelMSGContainer MsgCon;

		private UILog LogBox;
	}
}

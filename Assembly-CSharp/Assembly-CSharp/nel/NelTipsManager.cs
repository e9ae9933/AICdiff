using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class NelTipsManager : IRunAndDestroy
	{
		public NelTipsManager(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
		}

		public void reloadScripts()
		{
			string resource = TX.getResource("Data/tips", ref this.ResourceDate, ".csv", false, "Resources/");
			if (resource == null)
			{
				return;
			}
			this.OEntry = new NDic<NelTipsManager.TipsEntry>("Tips_Entry", 16, 0);
			this.ACurShow = new List<NelTipsManager.TipsEntry>(5);
			CsvReader csvReader = new CsvReader(resource, CsvReader.RegSpace, false);
			string text = null;
			EvalP evalP = null;
			string text2 = null;
			NelTipsManager.TipsEntry tipsEntry = default(NelTipsManager.TipsEntry);
			while (csvReader.read())
			{
				if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
				{
					if (text != null)
					{
						tipsEntry.finalize(text, this.OEntry);
					}
					text = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
					tipsEntry = default(NelTipsManager.TipsEntry);
					tipsEntry.Term = evalP;
				}
				else if (text != null)
				{
					if (csvReader.cmd == "%PRI")
					{
						tipsEntry.priority = (byte)X.MMX(0, csvReader.Int(1, 0), 255);
					}
					else if (csvReader.cmd == "%TX")
					{
						tipsEntry.tx_key = csvReader.slice_join(1, "", "");
					}
					else if (csvReader.cmd == "%TERM")
					{
						string text3 = csvReader.slice_join(1, "", "");
						if (text2 == text3)
						{
							tipsEntry.Term = evalP;
						}
						else
						{
							text2 = text3;
							evalP = (tipsEntry.Term = (TX.noe(text3) ? null : new EvalP(null).Parse(text3)));
						}
					}
				}
			}
			if (text != null)
			{
				tipsEntry.finalize(text, this.OEntry);
			}
		}

		public bool fineEnabled()
		{
			this.reloadScripts();
			NelTipsManager.TipsEntry tipsEntry = default(NelTipsManager.TipsEntry);
			this.ACurShow.Clear();
			foreach (KeyValuePair<string, NelTipsManager.TipsEntry> keyValuePair in this.OEntry)
			{
				NelTipsManager.TipsEntry value = keyValuePair.Value;
				if (value.fineEnabled(tipsEntry))
				{
					this.ACurShow.Add(value);
				}
				tipsEntry = value;
			}
			if (this.ACurShow.Count <= 0)
			{
				return false;
			}
			this.entry_index = 0;
			X.shuffle<NelTipsManager.TipsEntry>(this.ACurShow, -1, null);
			float num = 0f;
			int count = this.ACurShow.Count;
			for (int i = 0; i < count; i++)
			{
				NelTipsManager.TipsEntry tipsEntry2 = this.ACurShow[i];
				int num2 = (int)((1 + tipsEntry2.priority) * (tipsEntry2.watched ? 1 : 2));
				num += (float)num2;
			}
			if (num == 0f)
			{
				return false;
			}
			num = X.XORSP() * num;
			for (int j = 0; j < count; j++)
			{
				NelTipsManager.TipsEntry tipsEntry3 = this.ACurShow[j];
				int num3 = (int)((1 + tipsEntry3.priority) * (tipsEntry3.watched ? 1 : 2));
				num -= (float)num3;
				if (num < 0f)
				{
					this.ACurShow.RemoveAt(j);
					this.ACurShow.Insert(0, tipsEntry3);
					break;
				}
			}
			return true;
		}

		private static int fnSortEntry(NelTipsManager.TipsEntry A, NelTipsManager.TipsEntry B)
		{
			int num = (int)(A.priority + (A.watched ? 0 : 1));
			int num2 = (int)(B.priority + (B.watched ? 0 : 1));
			if (num == num2)
			{
				return 0;
			}
			if (num <= num2)
			{
				return 1;
			}
			return -1;
		}

		public void destruct()
		{
			this.deactivate(false);
		}

		private bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (this.runner_assigned == value)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (value)
				{
					IN.addRunner(this);
					this.activate();
					return;
				}
				IN.remRunner(this);
				this.deactivate(true);
			}
		}

		public void activate()
		{
			if (this.t_tx < 0f && this.ACurShow.Count > 0)
			{
				this.t_tx = 0f;
				this.t_dact = -1f;
				if (this.Tx == null)
				{
					for (int i = 0; i < 2; i++)
					{
						TextRenderer textRenderer = IN.CreateGobGUI(null, (i == 0) ? "-Tips_TX" : "-Tips_TXKD").AddComponent<TextRenderer>();
						if (i == 0)
						{
							this.Tx = textRenderer;
						}
						else
						{
							this.TxKD = textRenderer;
							this.TxKD.transform.SetParent(this.Tx.transform, false);
							textRenderer.text_content = TX.Get("KD_tips_next", "");
						}
						textRenderer.Col(MTRX.ColWhite);
						textRenderer.BorderCol(MTRX.ColBlack);
						textRenderer.alignx = ALIGN.RIGHT;
						textRenderer.aligny = ALIGNY.BOTTOM;
						textRenderer.html_mode = true;
						textRenderer.size = (float)((i == 0) ? 16 : 12);
						textRenderer.alpha = 0f;
						textRenderer.use_valotile = true;
					}
				}
				this.Tx.TargetFont = TX.getDefaultFont();
				this.TxKD.TargetFont = TX.getDefaultFont();
				this.reposit();
				this.fineTextContent();
				this.Tx.gameObject.SetActive(true);
				this.TxKD.gameObject.SetActive(true);
				this.runner_assigned = true;
			}
		}

		public void autoDeact()
		{
			if (this.t_dact < 0f && this.t_tx >= 0f)
			{
				this.t_dact = 0f;
				if (this.Tx != null)
				{
					this.reposit();
					this.TxKD.gameObject.SetActive(false);
				}
			}
		}

		private void destructObject()
		{
			if (this.Tx != null)
			{
				IN.DestroyE(this.TxKD.gameObject);
				IN.DestroyE(this.Tx.gameObject);
			}
			this.Tx = null;
			this.TxKD = null;
		}

		public void deactivate(bool immediate = false)
		{
			if (immediate)
			{
				if (this.t_tx > -50f)
				{
					this.t_tx = -50f;
					if (this.Tx != null)
					{
						this.destructObject();
					}
				}
				this.runner_assigned = false;
				return;
			}
			if (this.t_tx >= 0f)
			{
				this.t_tx = -1f;
			}
			if (this.Tx != null)
			{
				this.TxKD.gameObject.SetActive(false);
				this.reposit();
			}
		}

		public void fineTextContent()
		{
			NelTipsManager.TipsEntry tipsEntry = this.ACurShow[this.entry_index];
			this.Tx.text_content = TX.ReplaceTX(tipsEntry.tx_key, false);
			if (!tipsEntry.watched)
			{
				tipsEntry.watched = true;
				this.ACurShow[this.entry_index] = tipsEntry;
			}
		}

		public bool progress_enabled
		{
			get
			{
				return this.t_dact >= 0f || this.t_dact <= -90f;
			}
		}

		public override string ToString()
		{
			return "TIPS MANAGER";
		}

		public bool run(float fcnt)
		{
			if (this.t_tx >= 0f)
			{
				if (this.t_tx >= 1f)
				{
					this.Tx.alpha = (this.TxKD.alpha = X.ZLINE(this.t_tx - 1f, 15f));
				}
				this.t_tx += fcnt;
				if (this.t_dact >= 0f)
				{
					this.t_dact += fcnt;
					if (this.t_dact >= 120f)
					{
						this.deactivate(false);
					}
				}
				else
				{
					if (this.t_tx >= 13f && IN.isSubmitPD(1))
					{
						this.t_tx = 1f;
						this.entry_index = (this.entry_index + 1) % this.ACurShow.Count;
						this.fineTextContent();
						this.Tx.alpha = 0f;
						IN.clearSubmitPushDown(true);
					}
					if (this.t_dact > -90f)
					{
						this.t_dact -= fcnt;
					}
				}
			}
			else
			{
				this.t_tx -= fcnt;
				if (this.t_tx <= -50f)
				{
					this.runner_assigned_ = false;
					this.destructObject();
					return false;
				}
				this.Tx.alpha = X.ZLINE(50f + this.t_tx, 50f) * 0.8f;
			}
			return true;
		}

		public void reposit()
		{
			if (this.t_tx > -50f)
			{
				float num = ((M2DBase.Instance.ui_shift_x < 0f) ? (IN.wh + this.M2D.ui_shift_x * 2f - 20f) : (this.M2D.transferring_game_stopping ? (IN.wh - 152f) : (IN.wh - 26f)));
				IN.PosP(this.Tx.transform, num, -IN.hh + 55f, -9.45f);
				IN.PosP(this.TxKD.transform, 0f, -25f, -0.03f);
			}
		}

		private readonly NelM2DBase M2D;

		private NDic<NelTipsManager.TipsEntry> OEntry;

		private const string script_path = "Data/tips";

		private DateTime ResourceDate;

		private List<NelTipsManager.TipsEntry> ACurShow;

		private const int TIPS_MAX = 5;

		private const float T_FADE = 15f;

		private const float T_FADEOUT = 50f;

		private bool runner_assigned_;

		public float t_tx = -50f;

		private const float T_PROGRESS_ENABLE = 90f;

		private TextRenderer Tx;

		private TextRenderer TxKD;

		private int entry_index;

		public float t_dact = -1f;

		private const int T_DELAY = 1;

		private struct TipsEntry
		{
			public void finalize(string key, NDic<NelTipsManager.TipsEntry> OEntry)
			{
				if (this.tx_key == null)
				{
					this.tx_key = "&&tips_" + key;
				}
				OEntry[key] = this;
			}

			public bool fineEnabled(NelTipsManager.TipsEntry Pre)
			{
				if (this.Term == null)
				{
					this.is_enabled = true;
				}
				else if (this.Term == Pre.Term)
				{
					this.is_enabled = Pre.is_enabled;
				}
				else
				{
					this.is_enabled = this.Term.getValue(null) != 0.0;
				}
				return this.is_enabled;
			}

			public string tx_key;

			public byte priority;

			public bool watched;

			public EvalP Term;

			public bool is_enabled;
		}
	}
}

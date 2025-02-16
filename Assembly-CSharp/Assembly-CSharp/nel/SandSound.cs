using System;
using UnityEngine;
using XX;

namespace nel
{
	public class SandSound : MonoBehaviour
	{
		public void Start()
		{
		}

		public void Update()
		{
			if (this.t == 0)
			{
				if (MTRX.prepared)
				{
					this.t++;
					X.dl("prepared", null, false, false);
					return;
				}
			}
			else if (this.t >= 1 && this.play_queue)
			{
				this.play_queue = false;
				SND.Ui.play(this.play_sound, false);
			}
		}

		public string play_sound = "magic_casting";

		private int t;

		public bool play_queue;
	}
}

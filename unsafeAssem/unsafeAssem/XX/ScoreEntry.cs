using System;
using PixelLiner.PixelLinerLib;

namespace XX
{
	public class ScoreEntry
	{
		public ScoreEntry(int _default_score, string _suffix)
		{
			this.score = _default_score;
			this.default_score = _default_score;
			this.suffix = _suffix;
		}

		public void Clear()
		{
			this.score = this.default_score;
		}

		public bool UpdateScore(int val)
		{
			if (this.default_score > 0)
			{
				if (this.score > val)
				{
					this.score = val;
					return true;
				}
			}
			else if (this.score < val)
			{
				this.score = val;
				return true;
			}
			return false;
		}

		public static void readFromBytes(ScoreEntry Target, ByteArray Ba)
		{
			int num = Ba.readInt();
			if (Target != null)
			{
				Target.Clear();
				Target.score = num;
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeInt(this.score);
		}

		public int score;

		public readonly int default_score;

		public string suffix;
	}
}

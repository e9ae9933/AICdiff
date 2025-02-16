using System;

namespace XX
{
	public class Flagger : FlaggerT<string>
	{
		public Flagger(FlaggerT<string>.FnFlaggerCall _FnActivate = null, FlaggerT<string>.FnFlaggerCall _FnDeactivate = null)
			: base(_FnActivate, _FnDeactivate)
		{
		}

		public static Flagger operator +(Flagger Fl, string w)
		{
			Fl.Add(w);
			return Fl;
		}

		public static Flagger operator +(Flagger Fl, Flagger w)
		{
			Fl.Add(w);
			return Fl;
		}

		public static Flagger operator -(Flagger Fl, string w)
		{
			Fl.Rem(w);
			return Fl;
		}
	}
}

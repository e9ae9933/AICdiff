using System;

namespace XX
{
	public interface IStringHolder
	{
		int clength { get; }

		string cmd { get; }

		string _1 { get; }

		string _2 { get; }

		string _3 { get; }

		string _4 { get; }

		string _5 { get; }

		string _6 { get; }

		string _7 { get; }

		float _N1 { get; }

		float _N2 { get; }

		float _N3 { get; }

		float _N4 { get; }

		float _N5 { get; }

		float _N6 { get; }

		float Nm(int i, float defv = 0f);

		float NmE(int i, float defv = 0f);

		int Int(int i, int defv = 0);

		int IntE(int i, int defv = 0);

		bool tError(string t);
	}
}

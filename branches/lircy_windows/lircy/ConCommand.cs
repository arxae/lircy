using System;

using Meebey.SmartIrc4net;

namespace lircy
{
	public class ConCommand
	{
		public static void Part(IrcClient irc, string[] channels)
		{
			irc.RfcPart(channels);
		}
	}
}

/*
 ********************************************************
 * Lircy, IRC Chat Bot									* 
 * ircBot.cs, IRC Communication and event catching		*
 * lircy.exe											*
 ********************************************************
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;

using Meebey.SmartIrc4net;

namespace lircy
{
	class ircBot
	{
		public static XmlDocument xDoc = new XmlDocument();
		public static IrcClient irc = new IrcClient();
		public static Dictionary<string, string> Settings = new Dictionary<string, string>();
		public static ircCmd icmd;
		
		// Main Entrypoint
		public static void Main(string[] args)
		{			
			// Get the settings from lircy.xml
			xDoc.Load("lircy.xml");
			
			// Attribute collection for connection settings
			XmlAttributeCollection atr = xDoc.SelectSingleNode("/lircy/connection").Attributes;
			Settings.Add("irc_server", atr["server"].InnerText);
			Settings.Add("irc_port", atr["port"].InnerText);
			Settings.Add("irc_channels", atr["channels"].InnerText);
			Console.WriteLine("{0} -> Connection Settings Loaded", DateTime.Now.ToShortTimeString());
			
			// Attribute collection for bot settings
			atr = xDoc.SelectSingleNode("/lircy/bot").Attributes;
			Settings.Add("bot_nick", atr["nick"].InnerText);
			Settings.Add("bot_rname", atr["realname"].InnerText);
			Console.WriteLine("{0} -> Bot Settings Loaded", DateTime.Now.ToShortTimeString());
			
			// Attribute collection for general preferences
			atr = xDoc.SelectSingleNode("/lircy/prefs").Attributes;
			Settings.Add("pref_senddelay", atr["senddelay"].InnerText);
			Settings.Add("pref_channelsync", atr["channelsync"].InnerText);
			Settings.Add("pref_logfile", atr["logfile"].InnerText); // Warninglog
			Settings.Add("pref_logpath", atr["channellog"].InnerText); // Channel logging
			
			// Switches
			Settings.Add("sw_logfile", xDoc.SelectSingleNode("lircy/switches").Attributes["uselogfile"].InnerText);
			Settings.Add("sw_channellog", xDoc.SelectSingleNode("lircy/switches").Attributes["usechannellog"].InnerText);
			
			// Params and returns
			Settings.Add("login_msg", xDoc.SelectSingleNode("/lircy/commands/returns/OnBotJoin").Attributes["p"].InnerText);
			Settings.Add("server_list", xDoc.SelectSingleNode("/lircy/commands/returns/servers").Attributes["p"].InnerText);
			Settings.Add("die_pass", xDoc.SelectSingleNode("/lircy/commands/params/die").Attributes["p"].InnerText);
			
			// Set the irc options
			irc.Encoding = System.Text.Encoding.UTF8;
			irc.SendDelay = int.Parse(Settings["pref_senddelay"]);
			irc.ActiveChannelSyncing = bool.Parse(Settings["pref_channelsync"]);
			Console.WriteLine("{0} -> Prefs Loaded", DateTime.Now.ToShortTimeString());
			
			// Hook the events
			HookEvents();
			
			Console.WriteLine("{0} -> Settings loaded, trying to connect", DateTime.Now.ToShortTimeString());
			
			// Try to make a connection to the server
			try
			{
				irc.Connect(Settings["irc_server"], int.Parse(Settings["irc_port"]));
				Console.WriteLine("{0} -> Connected, joining channels", DateTime.Now.ToShortTimeString());
			}
			catch(ConnectionException ce)
			{
				Console.WriteLine("Could not establish connection, exception: {0}", ce.Message);
			}
			
			// Join the channel and give a hello message
			try
			{
				irc.Login(Settings["bot_nick"].Split(','), Settings["bot_rname"]);
				// Join all the channels in the list
				string[] channels = Settings["irc_channels"].Split(',');
				for (int i = 0; i < channels.Length; i++)
				{
					irc.RfcJoin(channels[i]);
					Console.WriteLine("{0} -> Joining {1}", DateTime.Now.ToShortTimeString(), channels[i]);
				}
				Console.WriteLine("{0} -> Joined channels, going to listen mode", DateTime.Now.ToShortTimeString());
				
				// Get bot command input (at console), new thread so you can enter commands
				//new Thread(new ThreadStart(ReadConsoleCommands)).Start();
				
				// Go into listen mode
				irc.Listen();
				
				// When listen returns, our IRC session is over, disconnect
				irc.Disconnect();
			}
			catch(ConnectionException ce)
			{
				// sometimes, irc.Disconnect throws a not connected exception, catch it
				Console.WriteLine("This exception is probably safe to ignore: {0}", ce.Message);
			}
			catch(Exception e)
			{
				// Should not happen, but just in case
				Console.WriteLine("Error occured! Message: {0}", e.Message);
				Console.WriteLine("Exception: {0}", e.StackTrace);
			}
		}
		
		public static void ReadConsoleCommands()
		{
			while(true)
			{
				Console.WriteLine(Console.ReadLine());
			}
		}
		
		// --------------------------------------------------------------------
		// IRC Event hooks
		//	All events are hooked here and executed in the ircCmd class
		// --------------------------------------------------------------------
		public static void HookEvents()
		{
			icmd = new ircCmd(irc, Settings);
			
			irc.OnJoin += OnJoin; // Anyone joins the channel
			irc.OnChannelMessage += OnChannelMessage;
			irc.OnQueryMessage += OnQueryMessage;
			
			Console.WriteLine("{0} -> Events hooked: OnQueryMessage", DateTime.Now.ToShortTimeString());
		}

		static void OnJoin(object sender, IrcEventArgs e) { icmd.OnJoin(e.Data.MessageArray, e.Data.Channel); }
		static void OnChannelMessage(object sender, IrcEventArgs e) { icmd.OnChannelMessage(e.Data.Nick, e.Data.Channel, e.Data.MessageArray); }
		static void OnQueryMessage (object sender, IrcEventArgs e) 
		{
			IrcUser user = irc.GetIrcUser(e.Data.Nick);
			icmd.OnBotQuery(user, e.Data.MessageArray); 
		}
	}
}
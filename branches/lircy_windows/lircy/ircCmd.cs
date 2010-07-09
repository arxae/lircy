using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Meebey.SmartIrc4net;

namespace lircy
{
	public class ircCmd
	{
		private static IrcClient irc;
		private static Dictionary<string, string> Settings = new Dictionary<string, string>();
		private static StreamWriter logWriter;
		private static StreamWriter channelLog;
		
		public ircCmd(IrcClient client, Dictionary<string, string>settings)
		{
			irc = client;
			Settings = settings;
			
			Console.WriteLine("{0} -> Commands loaded", DateTime.Now.ToShortTimeString());
		}
				
		// ---------------------------------------------------
		// Irc Events
		// ---------------------------------------------------
		public void OnJoin(string[] msg, string channel)
		{
			/*if(Settings["irc_channels"].Split(',').Length > 1) // Connected to more then one channel
			{
				string[] channels = Settings["irc_channels"].Split(',');
				for (int i = 0; i < channels.Length; i++)
				{
					irc.SendMessage(SendType.Message, channels[i], Settings["login_msg"]);
				}
			}
			else
			{
				irc.SendMessage(SendType.Message, Settings["irc_channels"], Settings["login_msg"]);
			}*/
		}
		
		public void OnBotQuery(IrcUser user, string[] msg)
		{
			string fullmessage = "";
			for (int i = 0; i < msg.Length; i++)
			{
				fullmessage += msg[i] + " ";
			}
			Console.WriteLine("QUERY :: {0} -> {1}: {2}", DateTime.Now.ToShortTimeString(), user.Nick, fullmessage);
			
			// Check for command
			switch (msg[0])
			{
			case "die": 
				if (msg.Length > 1) { command_die(msg[1], user); }
				else { irc.SendMessage(SendType.Message, user.Nick, "Not a valid command"); Console.WriteLine("WARNING :: User {0} tried to shut down the bot", user.Nick); }
				break;
				
			default : irc.SendMessage(SendType.Message, user.Nick, "Not a valid commandddddd"); break;
			}
		}
		
		public void OnChannelMessage(string nick, string channel, string[] msg)
		{
			string fullmsg = "";
			for (int i = 0; i < msg.Length; i++)
			{
				fullmsg += msg[i] + " ";
			}
			// relay it to the console
			Console.WriteLine("{0} -> {1}: {2}", DateTime.Now.ToShortTimeString(),nick , fullmsg);
			
			// Check if it is a ! command
			if (msg[0].Substring(0, 1) == "!") 
			{
				switch (msg[0].Substring(1, msg[0].Length-1))
				{
				case "srv": // WHEEEE
				case "server":// EEEEE :D
				case "servers": bang_servers(channel); break;
				}
			}
			
			// Log it when switch is true	
			if(bool.Parse(Settings["sw_channellog"]))
			{
				channelLog = new StreamWriter(Settings["pref_logpath"] + "/" + DateTime.Now.ToString("dd-MM-yyyy"));
				channelLog.WriteLine("{0} :: {1}", DateTime.Now.ToShortTimeString(), fullmsg);
				channelLog.Close();
				channelLog.Dispose();
			}
		}
		
		
		//------------------------------------------------------------------
		// Commands
		//------------------------------------------------------------------
		
		// Query commands
		private static void command_die(string pass, IrcUser user)
		{
			
		// TODO: IsIRCOp geeft altijd false terug
			if(!user.IsIrcOp)
			{
				Console.WriteLine("user is ircop");
			}
			else
			{
				Console.WriteLine("is not ircop");
			}
			
			
			
			
			/*
			if(user.IsIrcOp)
			{
				if(pass == Settings["die_pass"])
				{
					Console.WriteLine("{0} :: {0} made me exit", DateTime.Now.ToShortTimeString(), user.Nick);
					Environment.Exit(0);
				}
				else
				{
					irc.SendMessage(SendType.Message, user.Nick, "Wrong password");
				}
			}
			else
			{
				if(bool.Parse(Settings["sw_logfile"]))
				{
					logWriter = new StreamWriter(Settings["pref_logfile"]);
					logWriter.WriteLine("{0} :: User {1} has tried to use the die command, but he/she is not a op!", DateTime.Now, user.Nick);
					logWriter.Close();
					logWriter.Dispose();
				}
			}*/
		}
		
		// Bang (!) commands
		private static void bang_servers(string channel)
		{
			Console.WriteLine("{0} CMD :: Executed server listing", DateTime.Now.ToShortTimeString());
			string[] servers = Settings["server_list"].Split(',');
			for (int i = 0; i < servers.Length; i++) {
				irc.SendMessage(SendType.Message, channel, servers[i]);
			}
		}
	}
}

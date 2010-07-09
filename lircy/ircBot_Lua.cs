
using System;

using LuaSharp;
using Meebey.SmartIrc4net;

namespace lircy
{
	public class ircBotLua
	{
		public void testscript()
		{
			try
			{
				using( Lua state = new Lua(  ) )
				{
					state.DoFile( "script.lua" );
					
					LuaFunction f1 = state["AFunction"] as LuaFunction;
					
					state.DoString( "AFunction = nil" );
					
					f1.Call(  );
					f1.Dispose(  );
				
					LuaFunction f2 = state["BFunction"] as LuaFunction;
					f2.Call(  );
					f2.Dispose(  );
					
					LuaFunction f3 = state["CFunction"] as LuaFunction;
					f3.Call();
					f3.Dispose();
					
					LuaFunction print = state["print"] as LuaFunction;
					
					LuaTable sillytable = state["SillyTable"] as LuaTable;
					
					string str = sillytable["aaa"] as string;

					print.Call( str );

					sillytable["aaa"] = 9001;
					
					print.Call( state["SillyTable", "aaa"] );
					
					sillytable.Dispose(  );

					state.CreateTable( "table" );
					print.Call( state["table"] as LuaTable );

					print.Dispose(  );
				}
			}
			catch( LuaException e )
			{
				Console.WriteLine( "Fail: " + e.Message );
			}
		}
	}
}

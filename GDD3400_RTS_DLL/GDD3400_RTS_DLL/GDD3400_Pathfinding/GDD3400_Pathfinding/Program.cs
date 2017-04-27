using System;
using System.Collections.Generic;
using System.Reflection;
using GDD3400_RTS_Lib;

namespace GDD3400_Pathfinding
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// Load DLLS - loads the DLLs that represent other agents
        /// </summary>
        /// <param name="agents">collection of agents created by
        /// this method to be sent to the GameManager</param>
        static void LoadDLLs(List<Agent> agents)
        {
            // TODO: set this string to indicate the names of the DLLs
            // You MUST use the absolute path (from C:), you cannot use
            // relative paths for this.  Put the other DLLs wherever you
            // want, open their folder in Windows Explorer, copy the
            // file location from the address bar by clicking inside
            // the bar and then add the name of the individual dll.
            // If you are just developing, put 4 copies of your own
            // DLL here - found in the Debug folder of the
            // GDD3400_PlanningAgent_Lib project folder within this solution.
            string[] dlls =
            {
                @"C:\Users\Gray\Documents\Version Controlled\GDD-3400\GDD3400_RTS_DLL\GDD3400_RTS_DLL\GDD3400_PlanningAgent_Lib\bin\x86\Debug\GDD3400_PlanningAgent_Lib.dll",
                @"C:\Users\Gray\Documents\Version Controlled\GDD-3400\GDD3400_RTS_DLL\GDD3400_RTS_DLL\GDD3400_PlanningAgent_Lib\bin\x86\Debug\GDD3400_PlanningAgent_Lib.dll"
            };

            // For each of the DLL files, load it and then launch
            // an object of the type in the DLL
            for (int i = 0; i < Constants.NUMBER_PLAYERS && i < dlls.Length; ++i)
            {
                var DLL = Assembly.LoadFile(dlls[i]);

                foreach (Type type in DLL.GetExportedTypes())
                {
                    agents.Add((Agent)Activator.CreateInstance(type));
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
			// Debugging short-cuts
			// 1 - Hide/Show line to target gridcell for each troop
			// 2 - Hide/Show cells with obstacles in purple
			// 3 - Hide/Show troop & building timers, mine gold remaining
			// 4 - Hide/Show player data
			// 5 - Hide/Show influence map values

            // Game constants you might want to tweak for your testing
            Constants.NUMBER_PLAYERS = 2;					// Number of players - minimum 1, maximum 4
            Constants.GAME_SPEED = 3.0f;					// Make the game play-out faster or slower
            Constants.START_GOLD = 1000;					// Player starting gold
            Constants.VALUE[(int)ResourceType.MINE] = 1000;	// Total gold per mine
            Constants.USE_TERRAIN_SEED = true;				// Use the following seed for terrain, false if you want random terrain
            Constants.TERRAIN_SEED = 444;					// Set this to whatever you want, but seed determines terrain variation
            Constants.SHOW_BUILDABLE = false;				// Show buildable cells (also available through debug short-cuts)
            Constants.SHOW_PATHFINDING = false;				// Show pathfinding target per troop (also available through debug short-cuts)

            List<Agent> agents = new List<Agent>();
            LoadDLLs(agents);

            using (GameManager game = new GameManager(agents))
            {
                game.IsMouseVisible = true;
                game.Run();
            }
        }
    }
#endif
}


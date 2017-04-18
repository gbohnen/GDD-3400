using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using GDD3400_RTS_Lib;

namespace GDD3400_PlanningAgent_Lib
{
    // All of your code needs to belong to the PlanningAgent class or
    // needs to be a privately defined class (as in AnotherClass below)
    // you cannot define any other public classes in this library or
    // else the DLL launching will fail.
    
    // Define any other classes with the default protection so that
    // the DLL does not recognize them but you may use them within
    // the PlanningAgent class

    /// <summary>
    /// AnotherClass - just an example class, delete it if you don't
    /// want to use it
    /// </summary>
    class AnotherClass
    {
        // Just an example class...delete this if you don't use it...
    }

    /// <summary>
    /// Main class for the PlanningAgent, this is the only one
    /// that can be public.  Inherits from the Agent class
    /// </summary>
    public class PlanningAgent : Agent
    {
        private enum Mood { Passive, Defensive, Aggressive }

        //Dictionary<int, UnitGoals> goals = new Dictionary<int, UnitGoals>();
        int resourceCount = int.MaxValue;
        ResourceSprite closestMine = null;
        Random rand = new Random();
        GameState gameState;
        UnitSprite mainBase;

        List<UnitSprite> myUnits;
        List<UnitSprite> enemyUnits;

        List<UnitSprite> myPeons;
        List<UnitSprite> mySoldiers;
        List<UnitSprite> myBases;
        List<UnitSprite> myBarracks;
        List<UnitSprite> myRefineries;

        List<UnitSprite> enemyPeons;
        List<UnitSprite> enemySoldiers;
        List<UnitSprite> enemyBases;
        List<UnitSprite> enemyBarracks;
        List<UnitSprite> enemyRefineries;

        // more fields
        Mood currentMood = Mood.Passive;

        // data for heuristic



        // other planning thresholds/constants
        const int MINE_EXHAUSTED_THRESHOLD = 0;
        int currentGold = 0;
        int totalGold;

        public PlanningAgent()
        {
            AnotherClass anotherClass = new AnotherClass();
        }

        public override void AnalyzeTerrain(GameState gameState)
        {
            foreach (UnitSprite unit in gameState.Units)
            {
                Point gridPos = Tools.WorldToGrid(unit.Position);
                for (int i = -2; i < 3; ++i)
                {
                    for (int j = -2; j < 3; j++)
                    {
                        Point neighbor = new Point(gridPos.X + i, gridPos.Y + j);
                        if (Tools.IsValidGridLocation(neighbor) && gridPos != neighbor)
                        {
                            gameState.Grid[neighbor.X, neighbor.Y].Influence += (1 / Tools.DistanceBetweenPoints(gridPos, neighbor)) / Constants.NUMBER_PLAYERS;
                            MathHelper.Clamp(gameState.Grid[neighbor.X, neighbor.Y].Influence, 0.0f, 1.0f);
                        }
                    }
                }
            }
            foreach (ResourceSprite resource in gameState.Resources)
            {
                Point gridPos = Tools.WorldToGrid(resource.Position);
                for (int i = -2; i < 3; ++i)
                {
                    for (int j = -2; j < 3; j++)
                    {
                        Point neighbor = new Point(gridPos.X + i, gridPos.Y + j);
                        if (Tools.IsValidGridLocation(neighbor) && gridPos != neighbor)
                        {
                            gameState.Grid[neighbor.X, neighbor.Y].Influence += (1 / Tools.DistanceBetweenPoints(gridPos, neighbor)) / Constants.NUMBER_PLAYERS;
                            MathHelper.Clamp(gameState.Grid[neighbor.X, neighbor.Y].Influence, 0.0f, 1.0f);
                        }
                    }
                }
            }
        }

        #region Private Methods
        private void FindClosestMine()
        {
            // If this is the first time or a mine is destroyed (Resource count has decreased)
            // Find the closest mine
            if (closestMine == null || gameState.Resources.Count < resourceCount)
            {
                float closestMineDist = float.MaxValue;
                foreach (ResourceSprite mineResource in gameState.Resources.Where(x => x.ResourceType == ResourceType.MINE).ToList())
                {
                    foreach (UnitSprite baseUnit in myBases)
                    {
                        float mineDist = Vector2.Distance(mineResource.Position, baseUnit.Position);
                        if (mineDist < closestMineDist)
                        {
                            closestMineDist = mineDist;
                            closestMine = mineResource;
                        }
                    }
                }
            }
        }

        private Point FindRandomOpenCellToBuildWithinRange(int minXRange, int maxXRange, int minYRange, int maxYRange)
        {
            Point p;
            int i = 0;
            int j = 0;
            Point gridPosition = Tools.WorldToGrid(myBases[0].Position);

            do
            {
                i = gridPosition.X + (rand.Next(maxXRange - minXRange) + minXRange) * (rand.Next(2) > 0 ? -1 : 1);
                j = gridPosition.Y + (rand.Next(maxYRange - minYRange) + minYRange) * (rand.Next(2) > 0 ? -1 : 1);
                p = new Point(i, j);
            } while (!Tools.IsValidGridLocation(p) || !gameState.Grid[i, j].IsBuildable);// || !gameState.Grid[i, j].IsWalkable );

            return p;
        }

        private void ProcessPeons()
        {
            // For each peon
            foreach (UnitSprite unit in myPeons)
            {
                if (unit.CurrentAction == UnitAction.IDLE)
                {
                    // Test Move
                    //Move(unit, new Point(rand.Next(Constants.GRID_HEIGHT), rand.Next(Constants.GRID_WIDTH)));

                    // If we have enough gold and need a barracks, build a barracks
                    if (Gold >= Constants.COST[(int)UnitType.BARRACKS]
                        && myBarracks.Count < 3)
                    {
                        Point toBuild = FindRandomOpenCellToBuildWithinRange(
                            2, Constants.GRID_HEIGHT / 2 - 4,
                            2, Constants.GRID_WIDTH / 2 - 6);
                        Build(unit, toBuild, UnitType.BARRACKS);
                    }
                    // If we have enough gold and need a refinery, build a refinery
                    else if (Gold > Constants.COST[(int)UnitType.REFINERY]
                        && myRefineries.Count < 1 && myBarracks.Count >= 3)
                    {
                        Point toBuild = FindRandomOpenCellToBuildWithinRange(
                            2, Constants.GRID_HEIGHT / 2 - 4,
                            2, Constants.GRID_WIDTH / 2 - 6);
                        Build(unit, toBuild, UnitType.REFINERY);
                    }
                    // Ohterwise, just mine
                    else if (mainBase != null && closestMine.Value > 0)
                    {
                        Gather(unit, closestMine, mainBase);
                    }
                }
            }
        }
        private void ProcessBases()
        {
            // Process the Base
            foreach (UnitSprite unit in myBases)
            {
                if (unit.CurrentAction == UnitAction.IDLE && myPeons.Count < 10
                    && Gold > Constants.COST[(int)UnitType.PEON])
                {
                    Train(unit, UnitType.PEON);
                }
            }
        }

        private void ProcessBarracks()
        {
            // Process the Barracks
            foreach (UnitSprite unit in myBarracks)
            {
                if (unit.CurrentAction == UnitAction.IDLE
                    && (mySoldiers.Count < 10
                    || mySoldiers.Count <= enemySoldiers.Count * 1.5)
                    && Gold > Constants.COST[(int)UnitType.SOLDIER])
                {
                    Train(unit, UnitType.SOLDIER);
                }
            }
        }

        private void ProcessSoldiers()
        {
            // For each soldier, determine what they should attack
            foreach (UnitSprite unit in mySoldiers)
            {
                if (unit.CurrentAction == UnitAction.IDLE)
                {
                    if (enemySoldiers.Count > 0)
                    {
                        Attack(unit, enemySoldiers[0]);
                    }
                    else if (enemyPeons.Count > 0)
                    {
                        Attack(unit, enemyPeons[0]);
                    }
                    else if (enemyBases.Count > 0)
                    {
                        Attack(unit, enemyBases[0]);
                    }
                    else if (enemyBarracks.Count > 0)
                    {
                        Attack(unit, enemyBarracks[0]);
                    }
                    else if (enemyRefineries.Count > 0)
                    {
                        Attack(unit, enemyRefineries[0]);
                    }
                }
            }
        }

        public void RunGame()
        {
            // Identify all my units
            myUnits = gameState.Units.Where(y => y.AgentNbr == AgentNbr).ToList();
            enemyUnits = gameState.Units.Where(y => y.AgentNbr != AgentNbr).ToList();

            // Identify all of my peons
            myPeons = myUnits.Where(y => y.UnitType == UnitType.PEON).ToList();
            enemyPeons = enemyUnits.Where(y => y.UnitType == UnitType.PEON).ToList();

            // Identify all of my soldiers
            mySoldiers = myUnits.Where(y => y.UnitType == UnitType.SOLDIER).ToList();
            enemySoldiers = enemyUnits.Where(y => y.UnitType == UnitType.SOLDIER).ToList();

            // Identify all of my barracks
            myBarracks = myUnits.Where(y => y.UnitType == UnitType.BARRACKS).ToList();
            enemyBarracks = enemyUnits.Where(y => y.UnitType == UnitType.BARRACKS).ToList();

            // Identify all of my bases
            myBases = myUnits.Where(y => y.UnitType == UnitType.BASE).ToList();
            enemyBases = enemyUnits.Where(y => y.UnitType == UnitType.BASE).ToList();

            //// Identify all of my refineries
            myRefineries = myUnits.Where(y => y.UnitType == UnitType.REFINERY).ToList();
            enemyRefineries = enemyUnits.Where(y => y.UnitType == UnitType.REFINERY).ToList();

            if (myBases.Count > 0)
            {
                mainBase = myBases[0];
            }

            FindClosestMine();

            // update data, apply heuristic method
            UpdateData();
            UpdateMood();

            // passive mode
            if (currentMood == Mood.Passive)
            {
                ProcessPeonsPassive();
                ProcessSoldiersPassive();
                ProcessBarracksPassive();
                ProcessBasesPassive();
            }
            // defensive mode
            else if (currentMood == Mood.Defensive)
            {
                ProcessPeonsDefensive();
                ProcessSoldiersDefensive();
                ProcessBarracksDefensive();
                ProcessBasesDefensive();
            }
            // aggressive mode
            else if (currentMood == Mood.Aggressive)
            {
                ProcessPeonsAggressive();
                ProcessSoldiersAggressive();
                ProcessBarracksAggressive();
                ProcessBasesAggressive();
            }
            
            if (Constants.SHOW_MESSAGES)
            {
                lock (debugger.messages)
                {
                    debugger.messages = new List<string>();
                    debugger.messages.Add("Gold       " + Gold);
                    debugger.messages.Add("Peons      " + myPeons.Count + " " + enemyPeons.Count);
                    debugger.messages.Add("Soldiers   " + mySoldiers.Count + " " + enemySoldiers.Count);
                    debugger.messages.Add("Barracks   " + myBarracks.Count + " " + enemyBarracks.Count);
                    debugger.messages.Add("Bases      " + myBases.Count + " " + enemyBases.Count);
                    debugger.messages.Add("Refineries " + myRefineries.Count + " " + enemyRefineries.Count);
                }
            }
        }

        #endregion

        #region Public Methods

        public override void Update(GameTime gameTime, GameState gameState)
        {
            this.gameState = gameState;

            // If the gameState is null, do nothing, this is bad...
            if (gameState == null)
                return;

            RunGame();
        }

        #endregion

        #region General Agent Methods

        private void UpdateData()
        {

        }

        private void UpdateMood()
        {

        }

        #endregion


        #region Passive Methods

        private void ProcessPeonsPassive()
        {

        }

        private void ProcessSoldiersPassive()
        {

        }

        private void ProcessBarracksPassive()
        {

        }

        private void ProcessBasesPassive()
        {

        }

        #endregion

        #region Defensive Methods

        private void ProcessPeonsDefensive()
        {

        }

        private void ProcessSoldiersDefensive()
        {

        }

        private void ProcessBarracksDefensive()
        {

        }

        private void ProcessBasesDefensive()
        {

        }

        #endregion

        #region Aggressive Methods

        private void ProcessPeonsAggressive()
        {

        }

        private void ProcessSoldiersAggressive()
        {

        }

        private void ProcessBarracksAggressive()
        {

        }

        private void ProcessBasesAggressive()
        {

        }

        #endregion
    }
}

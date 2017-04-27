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

    delegate void AgentAction();

    class Actions
    {
        public Actions(AgentAction del)
        {
            Action = del;
            Weight = 0;
        }

        public float Weight
        { get; set; }
        public AgentAction Action
        { get; set; }
    }

    class GRAYS_CONSTANTS
    {
        // maximum peon gold held
        public static Dictionary<Mood, int> PEON_GOLD_MAX = new Dictionary<Mood, int>()
            {
                { Mood.Passive, 50 },
                { Mood.Defensive, 50 },
                { Mood.Aggressive, 50 }
            };

        // barracks count
        public static Dictionary<Mood, int> BARRACKS_COUNT = new Dictionary<Mood, int>()
            {
                { Mood.Passive, 3 },
                { Mood.Defensive, 3 },
                { Mood.Aggressive, 5 }
            };

        // mine exhaustion threshold
        public static Dictionary<Mood, int> MINE_EXHAUSTION_THRESHOLD = new Dictionary<Mood, int>()
            {
                { Mood.Passive, 0 },
                { Mood.Defensive, 100},
                { Mood.Aggressive, 50 }
            };

        // thresholds determine the times at which to take each action. as values approach threshold, the action becomes more likely
        public static Dictionary<ActionThresholdKey, float> actionThresholds = new Dictionary<ActionThresholdKey, float>()
            {
                { new ActionThresholdKey(Mood.Passive, Action.AttackPeon),          .5f },
                { new ActionThresholdKey(Mood.Passive, Action.AttackSoldier),       .5f },
                { new ActionThresholdKey(Mood.Passive, Action.AttackBuilding),      .5f },
                { new ActionThresholdKey(Mood.Passive, Action.TrainPeon),           .5f },
                { new ActionThresholdKey(Mood.Passive, Action.TrainSoldier),        .5f },
                { new ActionThresholdKey(Mood.Passive, Action.GatherGold),          .5f },
                { new ActionThresholdKey(Mood.Passive, Action.BuildBarracks),        1f },
                { new ActionThresholdKey(Mood.Passive, Action.Move),                .5f },


                { new ActionThresholdKey(Mood.Defensive, Action.AttackPeon),        .5f },
                { new ActionThresholdKey(Mood.Defensive, Action.AttackSoldier),     .5f },
                { new ActionThresholdKey(Mood.Defensive, Action.AttackBuilding),    .5f },
                { new ActionThresholdKey(Mood.Defensive, Action.TrainPeon),         .5f },
                { new ActionThresholdKey(Mood.Defensive, Action.TrainSoldier),      .5f },
                { new ActionThresholdKey(Mood.Defensive, Action.GatherGold),        .5f },
                { new ActionThresholdKey(Mood.Defensive, Action.BuildBarracks),     .5f },
                { new ActionThresholdKey(Mood.Defensive, Action.Move),              .5f },


                { new ActionThresholdKey(Mood.Aggressive, Action.AttackPeon),       .5f },
                { new ActionThresholdKey(Mood.Aggressive, Action.AttackSoldier),    .5f },
                { new ActionThresholdKey(Mood.Aggressive, Action.AttackBuilding),   .5f },
                { new ActionThresholdKey(Mood.Aggressive, Action.TrainPeon),        .5f },
                { new ActionThresholdKey(Mood.Aggressive, Action.TrainSoldier),     .5f },
                { new ActionThresholdKey(Mood.Aggressive, Action.GatherGold),       .5f },
                { new ActionThresholdKey(Mood.Aggressive, Action.BuildBarracks),    .5f },
                { new ActionThresholdKey(Mood.Aggressive, Action.Move),             .5f },
            };
    }

    struct ActionThresholdKey
    {
        public readonly Mood Mood;
        public readonly Action Action;
        public ActionThresholdKey(Mood m, Action e)
        {
            Mood = m;
            Action = e;
        }

        public override int GetHashCode()
        {
            return (int)Mood ^ (int)Action;
        }

    }

    enum Mood { Defensive = 0, Passive = 3, Aggressive = 7}

    enum Action { AttackPeon = 1, AttackSoldier, AttackBuilding, TrainPeon, TrainSoldier, GatherGold, BuildBarracks, Move }

    /// <summary>
    /// Main class for the PlanningAgent, this is the only one
    /// that can be public.  Inherits from the Agent class
    /// </summary>
    public class PlanningAgent : Agent
    {

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

        // influence maps
        float[,] terrainAnalysis;
        float[,] buildingAnalysis;

        // more fields
        Mood currentMood = Mood.Passive;

        // data for heuristic
        // action pool 
        Dictionary<Action, Actions> ActionPool;


        // other planning thresholds/constants
        int currentGold = 0;
        int totalGold;

        public PlanningAgent()
        {
            ActionPool = new Dictionary<Action, Actions>()
            {
                { Action.AttackBuilding,    new Actions(new AgentAction(AttackBuilding)) },
                { Action.AttackPeon,        new Actions(new AgentAction(AttackPeon)) },
                { Action.AttackSoldier,     new Actions(new AgentAction(AttackSoldier)) },
                { Action.BuildBarracks,     new Actions(new AgentAction(BuildBarracks)) },
                { Action.GatherGold,        new Actions(new AgentAction(GatherGold)) },
                { Action.TrainPeon,         new Actions(new AgentAction(TrainPeon)) },
                { Action.TrainSoldier,      new Actions(new AgentAction(TrainSoldier)) },
                { Action.Move,              new Actions(new AgentAction(MoveUnit)) }
            };
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


        public float[,] CreateBuildingMap(GameState gameState)
        {
            float[,] gridInf = new float[gameState.Grid.GetLength(0), gameState.Grid.GetLength(1)];

            // account for barrackses
            foreach (UnitSprite unit in gameState.Units.Where(x => x.UnitType == UnitType.BARRACKS))
            {
                Point gridPos = Tools.WorldToGrid(unit.Position);
                // get 3x3 grid of neighbors
                for (int i = -1; i < 2; ++i)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Point neighbor = new Point(gridPos.X + i, gridPos.Y + j);
                        if (Tools.IsValidGridLocation(neighbor) && gridPos != neighbor)
                        {
                            // add influence value, use the square distance from the barracks
                            gridInf[neighbor.X, neighbor.Y] += (1 / (Tools.DistanceBetweenPoints(gridPos, neighbor) * Tools.DistanceBetweenPoints(gridPos, neighbor)));
                            
                            MathHelper.Clamp(gridInf[neighbor.X, neighbor.Y], 0.0f, 1.0f);
                        }
                    }
                }
            }

            // account for bases
            foreach (UnitSprite unit in gameState.Units.Where(x => x.UnitType == UnitType.BARRACKS))
            {
                Point gridPos = Tools.WorldToGrid(unit.Position);
                for (int i = -2; i < 3; ++i)
                {
                    for (int j = -2; j < 3; j++)
                    {
                        Point neighbor = new Point(gridPos.X + i, gridPos.Y + j);
                        if (Tools.IsValidGridLocation(neighbor) && gridPos != neighbor)
                        {
                            // add influence value, use the linear distance from the barracks
                            gridInf[neighbor.X, neighbor.Y] += (1 / Tools.DistanceBetweenPoints(gridPos, neighbor));

                            MathHelper.Clamp(gridInf[neighbor.X, neighbor.Y], 0.0f, 1.0f);
                        }
                    }
                }
            }

            return gridInf;
        }


        #region Public Methods

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
            UpdateMood();
            UpdateActionWeights();
            CheckForActions();

            ProcessPeons();
            ProcessSoldiers();
            ProcessBarracks();
            ProcessBases();
            
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

        private void UpdateActionWeights()
        {
            // account for total gold earned, current gold, and total gold possible

            // account for current peons and enemy peons

            // account for current soldiers and enemy soldiers

            // account for current barracks and enemy barracks
            

        }

        private void UpdateMood()
        {
            // 0 <= x < .3 == defensive
            // .3 <= x < .7 == passive
            // .7 <= x <= 1 == aggressive

            float range = 0;

            //// here's the heuristic
            //range += ClampNormal(   1.0f   * (AvgDistanceFromBase(enemySoldiers)));                // check avg distance from base
            //range += ClampNormal(   1.0f   * (1 - (myPeons.Count / enemyPeons.Count)));            // check ratio of peon counts
            //range += ClampNormal(   1.0f   * (1 - (mySoldiers.Count / enemySoldiers.Count)));      // check ratio of soldier counts
            //range += ClampNormal(   1.0f   * (1 - (myBarracks.Count / enemyBarracks.Count)));      // check ratio of barrack counts
            //range += ClampNormal(   1.0f   * (1 - (Gold / GoldLeft())));                           // check ratio of our gold compared to remaining gold

            range = ClampNormal(range / 5);                                                        // average by total factors

            if (range < .3)
                currentMood = Mood.Defensive;
            else if (.7f <= range)
                currentMood = Mood.Aggressive;
            else
                currentMood = Mood.Passive;
        }

        // executes any actions that are at or above their thresholds
        private void CheckForActions()
        {
            // for each possible action
            foreach (Action action in Enum.GetValues(typeof(Action)))
            {
                // if the value for taking this action is greater than the heuristic threshold to take that action
                if (ActionPool[action].Weight >= GRAYS_CONSTANTS.actionThresholds[new ActionThresholdKey(currentMood, action)])
                {
                    // fire the delegate
                    ActionPool[action].Action();
                }
            }
        }

        #endregion

        #region Private Methods

        // find closest unit
        private UnitSprite FindClosestUnit(UnitSprite actor, List<UnitSprite> targets)
        {
            float dist = float.MaxValue;
            UnitSprite target = null;

            foreach (UnitSprite sprite in targets)
            {
                if (Tools.DistanceBetweenPoints(Tools.WorldToGrid(actor.Position), Tools.WorldToGrid(sprite.Position)) < dist)
                {
                    dist = Tools.DistanceBetweenPoints(Tools.WorldToGrid(actor.Position), Tools.WorldToGrid(sprite.Position));
                    target = sprite;
                } 
            }

            return target;
        }

        // clamps a value between 0 and 1
        private float ClampNormal(float num)
        {
            if (num > 1)
                return 1;
            else if (num < 0)
                return 0;
            else
                return num;
        }

        // gets the total gold available in all mines
        private int GoldLeft()
        {
            int i = 0;

            foreach (ResourceSprite mineResource in gameState.Resources.Where(x => x.ResourceType == ResourceType.MINE).ToList())
            {
                i += mineResource.Value;
            }

            return i;
        }

        // checks the average distance from base to the given collection
        private float AvgDistanceFromBase(List<UnitSprite> set)
        {
            float dist = 0;

            foreach (UnitSprite sprite in set)
            {
                dist += Tools.DistanceBetweenPoints(Tools.WorldToGrid(sprite.Position), Tools.WorldToGrid(mainBase.Position));
            }

            return dist / set.Count;
        }

        // checks how many of a given points neighbors are buildable
        private int CheckOccupiedNeighbors(Point p)
        {
            if (!Tools.IsValidGridLocation(p))
                return -1;
            else
            {
                int count = 0;

                for (int i = p.X - 1; i <= p.X + 1; i++)
                {
                    for (int j = p.Y - 1; j <= p.Y + 1; j++)
                    {
                        if (!Tools.IsValidGridLocation(Tools.WorldToGrid(new Vector2(i, j))))
                            if (!gameState.Grid[i, j].IsBuildable)
                                count++;
                    }
                }

                return count;
            }
        }

        // finds the closest mine to the base
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

        // finds a random cell that is buildable
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

        // finds the neighbor with the minimum given occupied neighbors within range, otherwise returns the highest possible cover
        private Point FindCellWithOccupiedNeighbors(int minXRange, int maxXRange, int minYRange, int maxYRange, int desiredCoveredNeighbors)
        {
            Point p;
            int i = 0;
            int j = 0;
            Point gridPosition = Tools.WorldToGrid(myBases[0].Position);

            int cover = 0;

            for (int x = -maxXRange; x <= maxXRange; x++)
                for (int y = -maxYRange; y <= maxYRange; y++)
                {
                    if (!(Tools.DistanceBetweenPoints(new Point(x, y), Tools.WorldToGrid(myBases[0].Position)) < minXRange) || !((Tools.DistanceBetweenPoints(new Point(x, y), Tools.WorldToGrid(myBases[0].Position)) < minYRange)))
                    {
                        if (CheckOccupiedNeighbors(new Point(x, y)) > cover)
                        {
                            cover = CheckOccupiedNeighbors(new Point(x, y));
                            i = x;
                            j = y;

                            if (cover > desiredCoveredNeighbors)
                            {
                                p = new Point(i, j);
                                return p;
                            }
                        }
                    }
                }

            p = new Point(i, j);
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

        #endregion        

        #region Passive Methods

        private void ProcessPeonsPassive()
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
                        // look for point with at least 3 occupied neighbors
                        Point toBuild = FindCellWithOccupiedNeighbors(
                            2, Constants.GRID_HEIGHT / 2 - 4,
                            2, Constants.GRID_WIDTH / 2 - 6,
                            3);
                        Build(unit, toBuild, UnitType.BARRACKS);
                    }
                    // we dont need a refinery

                    // Ohterwise, just mine
                    else if (mainBase != null && closestMine.Value > 0)
                    {
                        Gather(unit, closestMine, mainBase);
                    }
                }
            }
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

        #region DelegateMethods

        // Actions.Attackpeon
        void AttackPeon()
        {

            Console.WriteLine(">AttackPeon");
        }

        // Actions.AttackSoldier
        void AttackSoldier()
        {

            Console.WriteLine(">AttackSoldier");
        }

        // Actions.AttackBuilding
        protected void AttackBuilding()
        {

            Console.WriteLine(">AttackBuilding");
        }

        // Actions.TrainPeon
        void TrainPeon()
        {

            Console.WriteLine(">TrainPeon");
        }

        // Actions.TrainSoldier
        void TrainSoldier()
        {

            Console.WriteLine(">TrainSoldier");
        }

        // Actions.GatherGold
        void GatherGold()
        {

            Console.WriteLine(">GatherGold");
        }

        // Actions.BuildBarracks
        void BuildBarracks()
        {
            Console.WriteLine(">BuildBarracks");
        }

        // Actions.MovePeon
        void MoveUnit()
        {

            Console.WriteLine(">MoveUnit");
        }

        #endregion

    }
}

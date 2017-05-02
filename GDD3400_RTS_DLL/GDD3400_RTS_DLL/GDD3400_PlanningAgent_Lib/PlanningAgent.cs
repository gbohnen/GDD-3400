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
        float[,] enemyUnitAnalysis;
        float[,] allyUnitAnalysis;

        // data for heuristic
        // action pool 
        Dictionary<Action, Actions> ActionPool;

        bool hasWiped = false;
        bool buildingBarracks = false;

        // other planning thresholds/constants
        int currentGold = 0;
        int totalGold;

        public PlanningAgent()
        {
            //ActionPool = new Dictionary<Action, Actions>()
            //{
            //    { Action.AttackBuilding,    new Actions(new AgentAction(AttackBuilding)) },
            //    { Action.AttackPeon,        new Actions(new AgentAction(AttackPeon)) },
            //    { Action.AttackSoldier,     new Actions(new AgentAction(AttackSoldier)) },
            //    { Action.BuildBarracks,     new Actions(new AgentAction(BuildBarracks)) },
            //    { Action.GatherGold,        new Actions(new AgentAction(GatherGold)) },
            //    { Action.TrainPeon,         new Actions(new AgentAction(TrainPeon)) },
            //    { Action.TrainSoldier,      new Actions(new AgentAction(TrainSoldier)) },
            //    { Action.Move,              new Actions(new AgentAction(MoveUnit)) }
            //};
        }

        // dana's example
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

        // creates influence map of terrain and resources
        public float[,] CreateTerrainMap(GameState gameState)
        {
            float[,] gridInf = new float[gameState.Grid.GetLength(0), gameState.Grid.GetLength(1)];

            foreach (GridCell cell in gameState.Grid)
            {
                if (!cell.IsBuildable)
                {
                    Point gridPos = Tools.WorldToGrid(cell.Position);

                    for (int i = -2; i < 3; ++i)
                    {
                        for (int j = -2; j < 3; j++)
                        {
                            Point neighbor = new Point(gridPos.X + i, gridPos.Y + j);
                            if (Tools.IsValidGridLocation(neighbor) && gridPos != neighbor)
                            {
                                gridInf[neighbor.X, neighbor.Y] += (1 / Tools.DistanceBetweenPoints(gridPos, neighbor)) / Constants.NUMBER_PLAYERS;
                                MathHelper.Clamp(gridInf[neighbor.X, neighbor.Y], 0.0f, 1.0f);
                            }
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
                            gridInf[neighbor.X, neighbor.Y] += (1 / Tools.DistanceBetweenPoints(gridPos, neighbor)) / Constants.NUMBER_PLAYERS;
                            MathHelper.Clamp(gridInf[neighbor.X, neighbor.Y], 0.0f, 1.0f);
                        }
                    }
                }
            }

            return gridInf;
        }

        // enemy unit map. stronger falloff for peons than for soldiers
        public float[,] CreateEnemyMap(GameState gameState)
        {
            float[,] gridInf = new float[gameState.Grid.GetLength(0), gameState.Grid.GetLength(1)];

            // account for enemy peons
            foreach (UnitSprite unit in enemyPeons)
            {
                Point gridPos = Tools.WorldToGrid(unit.Position);

                for (int i = -1; i < 2; ++i)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Point neighbor = new Point(gridPos.X + i, gridPos.Y + j);
                        if (Tools.IsValidGridLocation(neighbor) && gridPos != neighbor)
                        {
                            // add influence value, use the square distance from each unit
                            gridInf[neighbor.X, neighbor.Y] += (1 / (Tools.DistanceBetweenPoints(gridPos, neighbor) * Tools.DistanceBetweenPoints(gridPos, neighbor))) / Constants.NUMBER_PLAYERS;
                            
                            MathHelper.Clamp(gridInf[neighbor.X, neighbor.Y], 0.0f, 1.0f);
                        }
                    }
                }
            }

            // account for enemy soldiers
            foreach (UnitSprite unit in enemySoldiers)
            {
                Point gridPos = Tools.WorldToGrid(unit.Position);

                for (int i = -1; i < 2; ++i)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Point neighbor = new Point(gridPos.X + i, gridPos.Y + j);
                        if (Tools.IsValidGridLocation(neighbor) && gridPos != neighbor)
                        {
                            // add influence value, use the linear distance from each unit
                            gridInf[neighbor.X, neighbor.Y] += (1 / Tools.DistanceBetweenPoints(gridPos, neighbor)) / Constants.NUMBER_PLAYERS;

                            MathHelper.Clamp(gridInf[neighbor.X, neighbor.Y], 0.0f, 1.0f);
                        }
                    }
                }
            }

            return gridInf;
        }

        public float[,] CreateAllyMap(GameState gameState)
        {
            float[,] gridInf = new float[gameState.Grid.GetLength(0), gameState.Grid.GetLength(1)];

            // account for my peons
            foreach (UnitSprite unit in myPeons)
            {
                Point gridPos = Tools.WorldToGrid(unit.Position);

                for (int i = -1; i < 2; ++i)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Point neighbor = new Point(gridPos.X + i, gridPos.Y + j);
                        if (Tools.IsValidGridLocation(neighbor) && gridPos != neighbor)
                        {
                            // add influence value, use the square distance from each unit
                            gridInf[neighbor.X, neighbor.Y] += (1 / (Tools.DistanceBetweenPoints(gridPos, neighbor) * Tools.DistanceBetweenPoints(gridPos, neighbor))) / Constants.NUMBER_PLAYERS;

                            MathHelper.Clamp(gridInf[neighbor.X, neighbor.Y], 0.0f, 1.0f);
                        }
                    }
                }
            }

            // account for my soldiers
            foreach (UnitSprite unit in mySoldiers)
            {
                Point gridPos = Tools.WorldToGrid(unit.Position);

                for (int i = -1; i < 2; ++i)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Point neighbor = new Point(gridPos.X + i, gridPos.Y + j);
                        if (Tools.IsValidGridLocation(neighbor) && gridPos != neighbor)
                        {
                            // add influence value, use the linear distance from each unit
                            gridInf[neighbor.X, neighbor.Y] += (1 / Tools.DistanceBetweenPoints(gridPos, neighbor)) / Constants.NUMBER_PLAYERS;

                            MathHelper.Clamp(gridInf[neighbor.X, neighbor.Y], 0.0f, 1.0f);
                        }
                    }
                }
            }

            return gridInf;
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
                            gridInf[neighbor.X, neighbor.Y] += (1 / (Tools.DistanceBetweenPoints(gridPos, neighbor) * Tools.DistanceBetweenPoints(gridPos, neighbor))) / Constants.NUMBER_PLAYERS;

                            MathHelper.Clamp(gridInf[neighbor.X, neighbor.Y], 0.0f, 1.0f);
                        }
                    }
                }
            }

            // account for bases
            foreach (UnitSprite unit in gameState.Units.Where(x => x.UnitType == UnitType.BASE))
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
                            gridInf[neighbor.X, neighbor.Y] += (1 / Tools.DistanceBetweenPoints(gridPos, neighbor)) / Constants.NUMBER_PLAYERS;

                            MathHelper.Clamp(gridInf[neighbor.X, neighbor.Y], 0.0f, 1.0f);
                        }
                    }
                }
            }

            return gridInf;
        }
        
        #region Public Methods

        public void RunGame(GameState gameState)
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

            // Identify all of my refineries
            myRefineries = myUnits.Where(y => y.UnitType == UnitType.REFINERY).ToList();
            enemyRefineries = enemyUnits.Where(y => y.UnitType == UnitType.REFINERY).ToList();

            // update relevant influence maps
            if (terrainAnalysis == null)    
                terrainAnalysis = CreateTerrainMap(gameState);          // terrain only needs to be analyzed once

            enemyUnitAnalysis = CreateEnemyMap(gameState);
            allyUnitAnalysis = CreateAllyMap(gameState);
            buildingAnalysis = CreateBuildingMap(gameState);

            if (myBases.Count > 0)
            {
                mainBase = myBases[0];
            }

            // scramble the public influence map. this is intentional
            WipeInfluence(gameState);

            FindClosestMine();

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

            RunGame(gameState);
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

        public void WipeInfluence(GameState gameState)
        {
            foreach(GridCell cell in gameState.Grid)
            {
                cell.Influence = (float)rand.NextDouble();
            }
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

        // uses terrain map to find a location that is buildable, close to base, and has the highest local influence given
        private Point FindLocalMaxima(Point start, int minXRange, int maxXRange, int minYRange, int maxYRange, float[,] grid)
        {
            Point p = start;
            float influence = 0;

            for (int i = start.X - maxXRange; i < start.X + maxXRange; i++)
            {
                if (i <= start.X - minXRange || i >= start.X + minXRange)
                {
                    for (int j = start.Y - maxYRange; j < start.Y + maxYRange; j++)
                    {
                        if (i <= start.Y - minYRange || i >= start.Y + minYRange)
                        {
                            Point temp = new Point(i, j);

                            if (Tools.IsValidGridLocation(temp) && gameState.Grid[i, j].IsBuildable)
                            {
                                if (grid[i, j] > influence)
                                {
                                    p = temp;
                                    influence = grid[i, j];
                                }
                            }
                        }
                    }
                }
            }

            return p;
        }

        private UnitSprite FindHighestInfluenceUnit(UnitType type)
        {
            UnitSprite unit = null;
            float inf = 0;

            foreach (UnitSprite sprite in gameState.Units.Where(x => x.UnitType == type))
            {
                if (enemyUnitAnalysis[Tools.WorldToGrid(sprite.Position).X, Tools.WorldToGrid(sprite.Position).Y] > inf)
                {
                    unit = sprite;
                    inf = enemyUnitAnalysis[Tools.WorldToGrid(sprite.Position).X, Tools.WorldToGrid(sprite.Position).Y];
                }
            }

            return unit;
        }

        private UnitSprite FindLowestInfluenceUnit(UnitType type)
        {
            UnitSprite unit = null;
            float inf = float.MaxValue;

            foreach (UnitSprite sprite in gameState.Units.Where(x => x.UnitType == type))
            {
                if (enemyUnitAnalysis[Tools.WorldToGrid(sprite.Position).X, Tools.WorldToGrid(sprite.Position).Y] < inf)
                {
                    unit = sprite;
                    inf = enemyUnitAnalysis[Tools.WorldToGrid(sprite.Position).X, Tools.WorldToGrid(sprite.Position).Y];
                }
            }

            return unit;
        }

        private UnitSprite FindClosestEnemyToBase(UnitSprite targetBase)
        {
            UnitSprite unit = null;
            float dist = float.MaxValue;

            foreach (UnitSprite enemy in enemyUnits)
            {
                if (Tools.DistanceBetweenPoints(Tools.WorldToGrid(enemy.Position), Tools.WorldToGrid(targetBase.Position)) < dist)
                {
                    unit = enemy;
                    dist = Tools.DistanceBetweenPoints(Tools.WorldToGrid(enemy.Position), Tools.WorldToGrid(targetBase.Position));
                }
            }

            return unit;
        }

        private ResourceSprite LeastOccupiedOrClosestMine()
        {
            ResourceSprite target = gameState.Resources.First(x => x.ResourceType == ResourceType.MINE);

            // sort a list by closest distance to mainbase
            List<ResourceSprite> mines = gameState.Resources.Where(x => x.ResourceType == ResourceType.MINE).ToList();
            mines.OrderBy(x => Vector2.Distance(x.Position, mainBase.Position));

            // run the list, compare weight of occupancy vs distance, return the first one with unoccupied spots
            foreach (ResourceSprite mine in mines)
            {
                if (enemyUnitAnalysis[Tools.WorldToGrid(mine.Position).X, Tools.WorldToGrid(mine.Position).Y] + allyUnitAnalysis[Tools.WorldToGrid(mine.Position).X, Tools.WorldToGrid(mine.Position).Y] < .5)
                    return mine;
            }

            return target;
        }

        private void ProcessPeons()
        {
            UnitSprite peon = null;

            // build two barracks asap
            if (Gold >= Constants.COST[(int)UnitType.BARRACKS] && myBarracks.Count < 3 && !buildingBarracks)
            {
                // try and grab an idle peon
                peon = myPeons.FirstOrDefault(x => x.CurrentAction == UnitAction.IDLE && x.CanBuildUnit(UnitType.BARRACKS));

                // if its null, just grab the first one that can build barracks
                if (peon == null)
                {
                    myPeons.FirstOrDefault(x => x.CanBuildUnit(UnitType.BARRACKS));
                }

                if (peon.CurrentAction != UnitAction.BUILD)
                {
                    Point toBuild = FindLocalMaxima(Tools.WorldToGrid(mainBase.Position), 5, 15, 5, 15, terrainAnalysis);
                    Build(peon, toBuild, UnitType.BARRACKS);
                }

                buildingBarracks = true; 
            }

            // For each peon
            foreach (UnitSprite unit in myPeons)
            {
                if (unit.CurrentAction == UnitAction.IDLE && unit != peon)
                {
                    // Test Move
                    //Move(unit, new Point(rand.Next(Constants.GRID_HEIGHT), rand.Next(Constants.GRID_WIDTH)));

                    // If we have enough gold and need a barracks, build a barracks
                    if (Gold >= Constants.COST[(int)UnitType.BARRACKS]
                        && myBarracks.Count <= 3)
                    {
                        Point toBuild = FindLocalMaxima(Tools.WorldToGrid(mainBase.Position), 5, 15, 5, 15, terrainAnalysis);
                        Build(unit, toBuild, UnitType.BARRACKS);
                    }
                    // Otherwise, just mine
                    else if (mainBase != null && LeastOccupiedOrClosestMine().Value > 0)
                    {
                        Gather(unit, LeastOccupiedOrClosestMine(), mainBase);
                    }
                }
            }
        }

        private void ProcessBases()
        {
            // Process the Base
            foreach (UnitSprite unit in myBases)
            {
                if (myBarracks.Count < 2)
                {
                    if (unit.CurrentAction == UnitAction.IDLE && myPeons.Count < 8
                        && Gold > Constants.COST[(int)UnitType.PEON])
                    {
                        Train(unit, UnitType.PEON);
                    }
                }
                else
                {
                    if (unit.CurrentAction == UnitAction.IDLE && myPeons.Count < 12
                        && Gold > Constants.COST[(int)UnitType.PEON])
                    {
                        Train(unit, UnitType.PEON);
                    }
                }
            }
        }

        private void ProcessBarracks()
        {

            // only train soldiers if we have at least two barracks
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
                // check for zerg rush
                if (enemyUnitAnalysis[Tools.WorldToGrid(mainBase.Position).X, Tools.WorldToGrid(mainBase.Position).Y] > .5 && unit.CurrentAction != UnitAction.ATTACK)
                {
                    // attack with all units not currently attacking
                    Attack(unit, FindClosestEnemyToBase(mainBase));
                }
                
                if (unit.CurrentAction == UnitAction.IDLE)
                {
                    if (enemySoldiers.Count > 0)
                    {
                        Attack(unit, FindLowestInfluenceUnit(UnitType.SOLDIER));
                    }
                    else if (enemyPeons.Count > 0)
                    {
                        Attack(unit, FindLowestInfluenceUnit(UnitType.PEON));
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
        

    }
}

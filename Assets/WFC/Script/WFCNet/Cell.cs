using System;
using System.Collections;
using System.Collections.Generic;

namespace WFCNet
{
    
    public class Cell
    {
        private int observedTile;

        public int ObservedTile
        {
            get
            {
                if (isPossiblesParamsDirty) UpdatePossiblesParams();
                return observedTile; 
            }
        }

        //[tileIndex]
        private bool[] possibleTiles;
        private Session session;

        // [direction][index]
        private int[][] compatibles;

        private int[] unpropagatedBannedTiles;

        private int unpropagatedBannedTilesPointer = 0;
        public bool HasUnpropagatedBannedTiles => unpropagatedBannedTilesPointer > 0;

        public void PushUnpropagatedTile(int tileIndex)
        {
            unpropagatedBannedTiles[unpropagatedBannedTilesPointer] = tileIndex;
            unpropagatedBannedTilesPointer++;
        }

        public int PopUnpropagatedTile()
        {
            unpropagatedBannedTilesPointer--;
            return unpropagatedBannedTiles[unpropagatedBannedTilesPointer];
        }

        private bool isPossiblesParamsDirty = true;

        private int possiblesCount;

        public int PossiblesCount
        {
            get
            {
                if (isPossiblesParamsDirty) UpdatePossiblesParams();
                return possiblesCount; 
            }
        }

        private double sumOfWeight;

        private double SumOfWeight
        {
            get
            {
                if (isPossiblesParamsDirty) UpdatePossiblesParams();
                return sumOfWeight; 
            }
        }

        private double sumOfLogWeight;

        private double SumOfLogWeight
        {
            get
            {
                if (isPossiblesParamsDirty) UpdatePossiblesParams();
                return sumOfLogWeight;
            }
        }

        private double entropy;

        public double Entropy
        {
            get
            {
                if (isPossiblesParamsDirty) UpdatePossiblesParams();
                return entropy;
            }
        }

        private void UpdatePossiblesParams()
        {
            isPossiblesParamsDirty = false;
            possiblesCount = 0;
            sumOfWeight = 0;
            sumOfLogWeight = 0;
            observedTile = -1;

            for (int i = 0; i < possibleTiles.Length; i++)
            {
                if (!possibleTiles[i]) continue;
                observedTile = i;
                possiblesCount++;
                sumOfWeight += session.Tiles[i].Weight;
                sumOfLogWeight += session.Tiles[i].LogWeight;
            }

            if (possiblesCount != 1)
            {
                observedTile = -1;
            }

            entropy = Math.Log(sumOfWeight) - sumOfLogWeight / sumOfWeight;
        }


        private Cell[] adjacents;

        public Cell(Session session, int tileCount)
        {
            possibleTiles = new bool[tileCount];
            this.session = session;

            unpropagatedBannedTiles = new int[tileCount];

            compatibles = new int[4][];
            for (int d = 0; d < 4; d++)
            {
                compatibles[d] = new int[tileCount];
            }

            for (int i = 0; i < possibleTiles.Length; i++)
            {
                possibleTiles[i] = true;
            }
        }

        public void Init(Cell right, Cell up, Cell left, Cell down)
        {
            adjacents = new[]
            {
                right,
                up,
                left,
                down
            };


            for (int d = 0; d < 4; d++)
            {
                Cell target = adjacents[d];
                if (target == null) continue;
                for (int i = 0; i < possibleTiles.Length; i++)
                {
                    var possible = session.Tiles[i];
                    foreach (var compatible in possible.ConnectableTiles[d])
                    {
                        target.NotifyCompatible(compatible.TileIndex, GetOppositeDirection(d));
                    }
                }
            }
        }

        public void Ban(int tileIndex)
        {
            if (possibleTiles[tileIndex])
            {
                possibleTiles[tileIndex] = false;
                isPossiblesParamsDirty = true;
                PushUnpropagatedTile(tileIndex);
            }
        }

        private int GetOppositeDirection(int source)
        {
            return (source + 2) % 4;
        }

        public void PropagateBan()
        {
            while (unpropagatedBannedTilesPointer > 0)
            {
                int bannedTile = PopUnpropagatedTile();
                for (int d = 0; d < 4; d++)
                {
                    Cell target = adjacents[d];
                    if (target == null) continue;
                    
                    var banned = session.Tiles[bannedTile];
                    foreach (var compatible in banned.ConnectableTiles[d])
                    {
                        target.NotifyBan(compatible.TileIndex, GetOppositeDirection(d));
                    }
                }
            }
        }

        public void NotifyBan(int tileIndex, int direction)
        {
            var dic = compatibles[direction];
            if (dic[tileIndex] >= 1)
            {
                dic[tileIndex]--;
                if (dic[tileIndex] == 0)
                {
                    Ban(tileIndex);
                }
            }
            else
            {
                //You notified unregistered compatibility.
            }
        }

        public void NotifyCompatible(int tileIndex, int direction)
        {
            compatibles[direction][tileIndex]++;
        }

        public void ObserveRandomFromPossibles(Random random)
        {
            double r = random.NextDouble();
            double sumOfWeight = SumOfWeight;
            double seqSumOfWeight = 0;
            int selected = -1;

            for (int i = 0; i < possibleTiles.Length; i++)
            {
                if (!possibleTiles[i]) continue;
                seqSumOfWeight += session.Tiles[i].Weight / SumOfWeight;
                if (seqSumOfWeight > r)
                {
                    selected = i;
                    break;
                }
            }

            if (selected == -1) return;

            for (int i = 0; i < possibleTiles.Length; i++)
            {
                if (!possibleTiles[i]) continue;

                if (i != selected)
                {
                    Ban(i);
                }
            }
        }
    }
}
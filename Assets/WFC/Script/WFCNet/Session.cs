using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Random = System.Random;

namespace WFCNet
{
    public abstract class Session
    {
        public int N { get; private set; }
        public int OutputWidth { get; private set; }
        public int OutputHeight { get; private set; }

        public Tile[] Tiles { get; set; }

        protected int TileCount => Tiles.Length;

        private Cell[] cells;

        private System.Random random;

        public Tile[] Observed { get; private set; }

        public Session(int n, int outputWidth, int outputHeight)
        {
            N = n;
            OutputWidth = outputWidth;
            OutputHeight = outputHeight;
        }

        public void Init(int seed)
        {
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i].TileIndex = i;
            }


            cells = new Cell[OutputWidth * OutputHeight];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new Cell(this, Tiles.Length);
            }

            for (int i = 0; i < cells.Length; i++)
            {
                (int x, int y) = SequentialToCoordinate(i);

                Cell right = null;
                Cell up = null;
                Cell left = null;
                Cell down = null;

                int? rightIndex = CoordinateToSequential(x + 1, y);
                int? upIndex = CoordinateToSequential(x, y - 1);
                int? leftIndex = CoordinateToSequential(x - 1, y);
                int? downIndex = CoordinateToSequential(x, y + 1);

                if (rightIndex != null) right = cells[rightIndex.Value];
                if (upIndex != null) up = cells[upIndex.Value];
                if (leftIndex != null) left = cells[leftIndex.Value];
                if (downIndex != null) down = cells[downIndex.Value];

                cells[i].Init(right, up, left, down);
            }

            random = new System.Random(seed);
        }

        private byte[] resultBuffer;

        public byte[] GetResult()
        {
            if (resultBuffer == null)
            {
                resultBuffer = new byte[OutputWidth * OutputHeight];
            }

            for (int i = 0; i < resultBuffer.Length; i++)
            {
                int observed = cells[i].ObservedTile;
                if (observed != -1)
                    resultBuffer[i] = Tiles[observed].Pixels[0];
                else
                {
                    resultBuffer[i] = 0;
                }
            }

            return resultBuffer;
        }

        public bool Run(int limit)
        {
            for (int l = 0; l < limit || limit == 0; l++)
            {
                bool? result = Observe();
                if (result != null) return (bool) result;
                Propagate();
            }

            return true;
        }

        public bool? RunStep()
        {
            bool? result = Observe();
            if (result != null) return (bool) result;
            Propagate();
            return null;
        }

        private bool? Observe()
        {
            double minEntropy = double.MaxValue;
            Cell argminEntropy = null;

            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                if (cell.PossiblesCount == 0) return false;

                var entropy = cell.Entropy;

                if (cell.PossiblesCount >= 2 && entropy <= minEntropy)
                {
                    double noise = 1E-6 * random.NextDouble();

                    if (entropy + noise < minEntropy)
                    {
                        minEntropy = entropy + noise;
                        argminEntropy = cell;
                    }
                }
            }

            // all cells are observed
            if (argminEntropy == null)
            {
                Observed = new Tile[OutputWidth * OutputHeight];
                for (int i = 0; i < Observed.Length; i++)
                {
                    Observed[i] = Tiles[cells[i].ObservedTile];
                }

                return true;
            }

            argminEntropy.ObserveRandomFromPossibles(random);

            return null;
        }

        public void Propagate()
        {
            while (true)
            {
                // Loops propagation until there is no unpropagated ban
                bool needToPrepagate = false;
                foreach (var cell in cells)
                {
                    if (cell.HasUnpropagatedBannedTiles)
                    {
                        needToPrepagate = true;
                        cell.PropagateBan();
                    }
                }

                if (!needToPrepagate) break;
            }
        }

        private (int, int) SequentialToCoordinate(int sequential)
        {
            int x = sequential % OutputWidth;
            int y = sequential / OutputHeight;
            return (x, y);
        }

        /// <summary>
        /// Converts an orthogonal coordinate system to an array index.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Array Index. If it is out of the area, return null.</returns>
        private int? CoordinateToSequential(int x, int y)
        {
            if (x < 0 || y < 0 || x >= OutputWidth || y >= OutputHeight) return null;
            return x + y * OutputWidth;
        }
    }
}
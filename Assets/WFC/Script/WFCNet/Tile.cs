using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WFCNet
{
    public class Tile
    {
        public byte[] Pixels { get; private set; }
        public int N { get; }

        private List<Tile>[] tempConnectableTiles;

        public Tile[][] ConnectableTiles { get; private set; }

        public double Weight { get; set; }
        public double LogWeight => Weight * Math.Log(Weight);

        public int TileIndex { get; set; }

        public Tile(byte[] pixels, int? n = null)
        {
            int estimatedN = (int) Math.Sqrt(pixels.Length);
            if (pixels.Length % estimatedN == 0 && pixels.Length / estimatedN == estimatedN)
            {
                if (n != null && n != estimatedN)
                {
                    throw new ArgumentException("Specified N or length of pixels is invalid.");
                }

                N = estimatedN;
                Pixels = pixels;
            }
            else
            {
                throw new ArgumentException("Tile size must be square number.");
            }
        }

        public bool Agress(Tile other, int direction)
        {
            int dx = 0;

            int dy = 0;
            switch (direction)
            {
                case 0:
                    dx = 1;
                    break;
                case 1:
                    dy = 1;
                    break;
                case 2:
                    dx = -1;
                    break;
                case 3:
                    dy = -1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            int xmin = dx < 0 ? 0 : dx, xmax = dx < 0 ? dx + N : N, ymin = dy < 0 ? 0 : dy, ymax = dy < 0 ? dy + N : N;
            for (int y = ymin;
                y < ymax;
                y++)
            for (int x = xmin;
                x < xmax;
                x++)
                if (Pixels[x + N * y] != other.Pixels[x - dx + N * (y - dy)])
                    return false;
            return true;
        }

        public void RegisterConnectableTile(Tile t, int direction)
        {
            if (tempConnectableTiles == null) tempConnectableTiles = new List<Tile>[4];
            if (tempConnectableTiles[direction] == null) tempConnectableTiles[direction] = new List<Tile>();
            tempConnectableTiles[direction].Add(t);
        }

        public void ApplyConnectableTileRegistration()
        {
            ConnectableTiles = new Tile[4][];
            for (int d = 0; d < 4; d++)
            {
                ConnectableTiles[d] = tempConnectableTiles[d].ToArray();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Tile)
            {
                return (obj as Tile).Pixels.SequenceEqual(Pixels);
            }
            else return false;
        }

        public Tile GetRotateLeft()
        {
            byte[] o = new byte[Pixels.Length];
            for (int iy = 0;
                iy < N;
                iy++)
            {
                for (int ix = 0; ix < N; ix++)
                {
                    byte i = Pixels[ix + iy * N];
                    int ox = N - iy - 1;
                    int oy = ix;
                    o[ox + oy * N] = i;
                }
            }

            return new Tile(o, N);
        }

        public Tile GetFilpHorizontal()
        {
            byte[] o = new byte[Pixels.Length];
            for (int iy = 0;
                iy < N;
                iy++)
            {
                for (int ix = 0; ix < N; ix++)
                {
                    byte i = Pixels[ix + iy * N];
                    int ox = N - ix - 1;
                    int oy = iy;
                    o[ox + oy * N] = i;
                }
            }

            return new Tile(o, N);
        }
    }
}
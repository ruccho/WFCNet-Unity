using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WFCNet
{

    public class OverlapSession : Session
    {
        private bool Periodic { get; set; } = false;

        public OverlapSession(int n, int outputGridWidth, int outputGridHeight, byte[] inputPixels, int inputWidth,
            bool periodic, int symmetry) : base(n, outputGridWidth,
            outputGridHeight)
        {
            Periodic = periodic;

            int inputHeight = inputPixels.Length / inputWidth;

            List<Tile> tileList = new List<Tile>();

            //create tiles
            for (int y = 0; y < inputHeight - (periodic ? 0 : n - 1); y++)
            {
                for (int x = 0; x < inputWidth - (periodic ? 0 : n - 1); x++)
                {
                    var seg = CropSegment(n, inputPixels, inputWidth, x, y);

                    Tile[] variants = new Tile[8];

                    variants[0] = new Tile(seg, n);
                    variants[1] = variants[0].GetFilpHorizontal();
                    variants[2] = variants[0].GetRotateLeft();
                    variants[3] = variants[2].GetFilpHorizontal();
                    variants[4] = variants[2].GetRotateLeft();
                    variants[5] = variants[4].GetFilpHorizontal();
                    variants[6] = variants[4].GetRotateLeft();
                    variants[7] = variants[6].GetFilpHorizontal();

                    symmetry = Math.Min(symmetry, variants.Length);

                    for (int i = 0; i < symmetry; i++)
                    {
                        var pre = tileList.FirstOrDefault(t => t.Equals(variants[i]));
                        if (pre != default)
                        {
                            //already exist
                            pre.Weight += 1;
                        }
                        else
                        {
                            variants[i].Weight += 1;
                            tileList.Add(variants[i]);
                        }
                    }
                }
            }

            Tiles = tileList.ToArray();

            //self
            for (int i = 0; i < Tiles.Length; i++)
            {
                //other
                for (int j = i; j < Tiles.Length; j++)
                {
                    //direction
                    for (int d = 0; d < 4; d++)
                    {
                        if (Tiles[i].Agress(Tiles[j], d))
                        {
                            Tiles[i].RegisterConnectableTile(Tiles[j], d);
                            int opposite = (d + 2) % 4;
                            Tiles[j].RegisterConnectableTile(Tiles[i], opposite);
                        }
                    }
                }
            }

            foreach (var t in Tiles)
            {
                t.ApplyConnectableTileRegistration();
            }
            
        }


        private static byte[] CropSegment(int n, byte[] pixels, int inputWidth, int x, int y)
        {
            byte[] o = new byte[n * n];
            int inputHeight = pixels.Length / inputWidth;

            for (int py = 0; py < n; py++)
            {
                for (int px = 0; px < n; px++)
                {
                    int ix = x + px;
                    int iy = y + py;
                    ix = ix % inputWidth;
                    iy = iy % inputHeight;

                    byte i = pixels[ix + iy * inputWidth];
                    o[px + py * n] = i;
                }
            }

            return o;
        }
    }
}
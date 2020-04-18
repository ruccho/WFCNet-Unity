using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WFCNet;
using Random = UnityEngine.Random;

public class WFCOverlap : MonoBehaviour
{
    [SerializeField] private Texture2D tex;

    [SerializeField] private float targetDeltaTime = 1f / 60f;

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int n;
    [SerializeField] private int symmetry;

    private Session session;
    private Texture2D output;
    List<Color> colorTable = new List<Color>();

    void Start()
    {
        Color[] cols = tex.GetPixels();
        byte[] pixels = new byte[cols.Length];

        colorTable.Add(Color.black);

        for (int i = 0; i < cols.Length; i++)
        {
            var col = cols[i];
            if (colorTable.FirstOrDefault(c => c.Equals(col)) == default)
            {
                colorTable.Add(col);
            }

            byte b = (byte) colorTable.IndexOf(col);

            pixels[i] = b;
        }

        Debug.Log("Color count: " + colorTable.Count.ToString());

        byte[] pixelsSwapped = new byte[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % tex.width;
            int y = Mathf.FloorToInt(i / tex.width);

            pixelsSwapped[x + (tex.height - y - 1) * tex.width] = pixels[i];
        }

        session = new OverlapSession(n, width, height, pixelsSwapped, tex.width, true, symmetry);

        Debug.Log($"Tile count: {session.Tiles.Length}");

        session.Init(Random.Range(0, 10000));

        output = new Texture2D(width, height, TextureFormat.ARGB32, false);
        output.filterMode = FilterMode.Point;

        GetComponent<Renderer>().material.mainTexture = output;

        StartCoroutine(StepLoop());
    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator StepLoop()
    {
        float timer;
        timer = Time.realtimeSinceStartup;
        while (true)
        {
            if (session.RunStep() == false)
            {
                yield break;
            }
            
            if (Time.realtimeSinceStartup - timer >= targetDeltaTime)
            {
                Color[] rCols = session.GetResult().Select(b => colorTable[b]).ToArray();
                output.SetPixels(rCols);
                output.Apply();
                
                timer = Time.realtimeSinceStartup;
                yield return null;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class PerformanceSpriteOutline
    {
        public static void Apply(string path, bool keepDetached = false)
        {
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!source.LoadImage(File.ReadAllBytes(path))) throw new InvalidOperationException("Cannot read source outline " + path);
                Color32[] pixels = source.GetPixels32();
                var factories = new SpriteDataProviderFactories(); factories.Init();
                var provider = factories.GetSpriteEditorDataProviderFromObject(AssetImporter.GetAtPath(path)); provider.InitSpriteEditorDataProvider();
                var outlines = provider.GetDataProvider<ISpriteOutlineDataProvider>();
                foreach (SpriteRect sprite in provider.GetSpriteRects())
                {
                    Rect rectangle = sprite.rect;
                    int width = Mathf.RoundToInt(rectangle.width), height = Mathf.RoundToInt(rectangle.height);
                    int left = Mathf.RoundToInt(rectangle.x), bottom = Mathf.RoundToInt(rectangle.y);
                    var visited = new bool[width * height]; var queue = new Queue<int>();
                    List<int> main = null;
                    var components = new List<List<int>>();
                    for (int start = 0; start < visited.Length; start++)
                    {
                        if (visited[start]) continue;
                        visited[start] = true;
                        if (pixels[(bottom + start / width) * source.width + left + start % width].a < 32) continue;
                        var component = new List<int>(); queue.Enqueue(start);
                        while (queue.Count > 0)
                        {
                            int point = queue.Dequeue(); component.Add(point);
                            void Visit(int next)
                            {
                                if (next < 0 || next >= visited.Length || visited[next] || Mathf.Abs(next % width - point % width) > 1) return;
                                visited[next] = true;
                                if (pixels[(bottom + next / width) * source.width + left + next % width].a >= 32) queue.Enqueue(next);
                            }
                            Visit(point - 1); Visit(point + 1); Visit(point - width); Visit(point + width);
                        }
                        if (main == null || component.Count > main.Count) main = component;
                        components.Add(component);
                    }
                    if (main == null || main.Count < 100) throw new InvalidOperationException("Empty connected performance pose " + sprite.name);
                    if (keepDetached) main = components.Where(component => component.Count >= Mathf.Max(100, main.Count * .015f)).SelectMany(component => component).ToList();
                    var occupied = new HashSet<int>(main);
                    var edges = new Dictionary<Vector2Int, List<Vector2Int>>();
                    void Edge(int startX, int startY, int endX, int endY)
                    {
                        var key = new Vector2Int(startX, startY);
                        if (!edges.TryGetValue(key, out List<Vector2Int> ends)) { ends = new List<Vector2Int>(); edges.Add(key, ends); }
                        ends.Add(new Vector2Int(endX, endY));
                    }
                    foreach (int point in main)
                    {
                        int horizontal = point % width, vertical = point / width;
                        if (vertical == 0 || !occupied.Contains(point - width)) Edge(horizontal, vertical, horizontal + 1, vertical);
                        if (horizontal == width - 1 || !occupied.Contains(point + 1)) Edge(horizontal + 1, vertical, horizontal + 1, vertical + 1);
                        if (vertical == height - 1 || !occupied.Contains(point + width)) Edge(horizontal + 1, vertical + 1, horizontal, vertical + 1);
                        if (horizontal == 0 || !occupied.Contains(point - 1)) Edge(horizontal, vertical + 1, horizontal, vertical);
                    }
                    List<Vector2Int> largest = null; double largestArea = 0;
                    var loops = new List<List<Vector2Int>>();
                    while (edges.Count > 0)
                    {
                        Vector2Int first = edges.Keys.First(), current = first;
                        var loop = new List<Vector2Int>();
                        do
                        {
                            loop.Add(current);
                            if (!edges.TryGetValue(current, out List<Vector2Int> ends)) throw new InvalidOperationException("Open outline " + sprite.name);
                            Vector2Int next = ends[ends.Count - 1]; ends.RemoveAt(ends.Count - 1);
                            if (ends.Count == 0) edges.Remove(current);
                            current = next;
                        } while (current != first);
                        double area = 0;
                        for (int index = 0; index < loop.Count; index++)
                        {
                            Vector2Int point = loop[index], next = loop[(index + 1) % loop.Count];
                            area += (double)point.x * next.y - (double)point.y * next.x;
                        }
                        if (Math.Abs(area) > largestArea) { largestArea = Math.Abs(area); largest = loop; }
                        if (area > 200) loops.Add(loop);
                    }
                    var contours = new List<Vector2[]>();
                    foreach (List<Vector2Int> contour in keepDetached ? loops : new List<List<Vector2Int>> { largest })
                    {
                        var corners = new List<Vector2>();
                        for (int index = 0; index < contour.Count; index++)
                        {
                            Vector2Int previous = contour[(index + contour.Count - 1) % contour.Count], point = contour[index], next = contour[(index + 1) % contour.Count];
                            if ((point - previous).x * (next - point).y == (point - previous).y * (next - point).x) continue;
                            corners.Add((Vector2)point - new Vector2(width, height) * .5f);
                        }
                        contours.Add(corners.ToArray());
                    }
                    outlines.SetOutlines(sprite.spriteID, contours);
                    outlines.SetTessellationDetail(sprite.spriteID, .25f);
                }
                provider.Apply(); ((TextureImporter)AssetImporter.GetAtPath(path)).SaveAndReimport();
            }
            finally { UnityEngine.Object.DestroyImmediate(source); }
        }
    }
}

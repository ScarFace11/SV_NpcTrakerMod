using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace NpcTrackerMod
{
    public class Pathfinding
    {
        private readonly int tileSize = 16; // Размер тайла, измените при необходимости
        private readonly List<Point> directions = new List<Point>
        {
            new Point(0, -1), // вверх
            new Point(1, 0),  // вправо
            new Point(0, 1),  // вниз
            new Point(-1, 0)  // влево
        };

        public List<Point> FindPath(Point start, Point end)
        {
            var openList = new List<Node> { new Node(start, null) };
            var closedList = new HashSet<Point>();

            while (openList.Count > 0)
            {
                var currentNode = openList.OrderBy(node => node.F).First();
                openList.Remove(currentNode);

                if (currentNode.Position == end)
                {
                    return ReconstructPath(currentNode);
                }

                closedList.Add(currentNode.Position);

                foreach (var direction in directions)
                {
                    var neighbor = new Point(currentNode.Position.X + direction.X, currentNode.Position.Y + direction.Y);
                    if (IsValid(neighbor) && !closedList.Contains(neighbor))
                    {
                        var g = currentNode.G + 1;
                        var h = Heuristic(neighbor, end);
                        var neighborNode = new Node(neighbor, currentNode, g, h);

                        if (openList.All(node => node.Position != neighbor) || g < neighborNode.G)
                        {
                            openList.Add(neighborNode);
                        }
                    }
                }
            }

            return null;
        }

        private List<Point> ReconstructPath(Node node)
        {
            var path = new List<Point>();
            while (node != null)
            {
                path.Add(node.Position);
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

        private bool IsValid(Point position)
        {
            // Проверьте, что точка находится в пределах карты и не является непроходимой
            return true; // Реализуйте проверку валидности в зависимости от вашей карты
        }

        private int Heuristic(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y); // Манхэттенское расстояние
        }

        private class Node
        {
            public Point Position { get; }
            public Node Parent { get; }
            public int G { get; }
            public int H { get; }
            public int F => G + H;

            public Node(Point position, Node parent, int g = 0, int h = 0)
            {
                Position = position;
                Parent = parent;
                G = g;
                H = h;
            }
        }
    }
}

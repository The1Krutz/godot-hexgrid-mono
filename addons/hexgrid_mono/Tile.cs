using System.Collections.Generic;
using Godot;

namespace MonoHexGrid
{
    public abstract class Tile : Node2D
    {
        public Vector2 Coordinates { get; set; }
        public Tile Parent { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsOnMap { get; set; }
        public bool HasRoadMarchBonus { get; set; }
        public int SearchCount { get; set; }
        public int Acc { get; set; }
        public float F { get; set; }

        public void Configure(Vector2 position, Vector2 coordinates, List<string> o)
        {
            Position = position;
            Coordinates = coordinates;
            IsOnMap = true;

            foreach (string t in o)
            {
                Sprite s = new Sprite
                {
                    Texture = GD.Load<Texture>(t),
                    Visible = false
                };
                AddChild(s);
            }
            Visible = false;
        }

        public void EnableOverlay(int index, bool visibility)
        {
            GetChild<Node2D>(index).Visible = visibility;
            if (visibility)
            {
                Visible = true;
            }
            else
            {
                Visible = false;
                foreach (Node2D o in GetChildren())
                {
                    if (o.Visible)
                    {
                        Visible = true;
                        break;
                    }
                }
            }
        }

        public bool IsOverlayOn(int index)
        {
            return GetChild<Node2D>(index).Visible;
        }

        /// <summary>
        /// is there a road with given orientation that drives out of that Tile
        /// </summary>
        /// <param name="orientation"></param>
        public abstract bool HasRoad(int orientation);

        /// <summary>
        /// is the line of sight blocked from a Tile to another
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="distance">Distance between from and to</param>
        /// <param name="distanceThis">Distance between from and this Tile</param>
        public abstract bool IsLosBlocked(Tile from, Tile to, float distance, float distanceThis);
    }
}

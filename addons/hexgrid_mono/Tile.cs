using System.Collections.Generic;
using Godot;

namespace MonoHexGrid
{
    /// <summary>
    ///
    /// </summary>
    public abstract class Tile : Node2D
    {
        public Vector2 coords;
        public bool blocked;
        public bool on_map;

        public int acc;
        public float f;
        public Tile parent;
        public bool road_march;
        public int search_count;

        /// <summary>
        ///
        /// </summary>
        /// <param name="p"></param>
        /// <param name="c"></param>
        /// <param name="o"></param>
        public void Configure(Vector2 p, Vector2 c, List<string> o)
        {
            Position = p;
            coords = c;
            on_map = true;

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

        /// <summary>
        /// is there a road with given orientation that drives out of that Tile
        /// </summary>
        /// <param name="orientation"></param>
        public abstract bool HasRoad(int orientation);

        /// <summary>
        /// is the line of sight blocked from a Tile to another, d beeing the distance between from and
        /// to, dt beeing the distance between from and this Tile
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="d"></param>
        /// <param name="dt"></param>
        public abstract bool BlockLos(Tile from, Tile to, float d, float dt);

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <param name="v"></param>
        public void EnableOverlay(int i, bool v)
        {
            GetChild<Node2D>(i).Visible = v;
            if (v)
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        public bool IsOverlayOn(int i)
        {
            return GetChild<Node2D>(i).Visible;
        }
    }
}

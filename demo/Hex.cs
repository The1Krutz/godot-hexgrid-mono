using System;
using Godot;
using MonoHexGrid;

namespace Demo
{
    public class Hex : Tile
    {
        public int type = -1;
        public int roads;

        public override void _Ready()
        {
            type = -1;
        }

        public string Inspect()
        {
            string s = "plain";
            if (type == 0)
            {
                s = "city";
            }
            else if (type == 1)
            {
                s = "wood";
            }
            else if (type == 2)
            {
                s = "mountain";
            }
            else if (type == 3)
            {
                s = "blocked";
            }
            return $"[{coords.x:F0};{coords.y:F0}]\n -> ({Position.x:F0};{Position.y:F0})\n -> {s}\ne:{Elevation()} h:{Height()} c:{Cost()} r:{roads}";
        }

        public override bool HasRoad(int orientation)
        {
            return (orientation & roads) > 0;
        }

        public override bool BlockLos(Tile from, Tile to, float d, float dt)
        {
            if (from is Hex fromhex && to is Hex tohex)
            {
                return BlockLos(fromhex, tohex, d, dt);
            }

            throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
        }

        private bool BlockLos(Hex from, Hex to, float d, float dt)
        {
            int h = Height() + Elevation();
            if (h == 0)
            {
                return false;
            }
            int e = from.Elevation();
            if (e > h)
            {
                if (to.Elevation() > h)
                {
                    return false;
                }
                return (h * dt / (e - h)) >= (d - dt);
            }
            h -= e;
            return (h * d / dt) >= to.Elevation() - e;
        }

        public void Change()
        {
            type = ((type + 2) % 5) - 1;
            for (int i = 0; i < 4; i++)
            {
                EnableOverlay(i + 3, i == type);
            }
        }

        public int Cost()
        {
            if (type == -1)
            {
                return 1;
            }
            else if (type == 3)
            {
                return -1;
            }
            return type + 1;
        }

        public int Height()
        {
            if (type == 0)
            {
                return 2;
            }
            else if (type == 1)
            {
                return 1;
            }
            else if (type == 2)
            {
                return 0;
            }
            return 0;
        }

        public int Elevation()
        {
            if (type == 2)
            {
                return 3;
            }
            return 0;
        }

        public int RangeModifier(int category)
        {
            return type == 2 ? 1 : 0;
        }

        public int AttackModifier(int category, int orientation)
        {
            return type == 1 ? 2 : 0;
        }

        public int DefenseValue(int category, int orientation)
        {
            if (type == 0)
            {
                return 2;
            }
            else if (type == 1)
            {
                return 1;
            }
            else if (type == 2)
            {
                return 1;
            }
            return 0;
        }

        public void ShowLos(bool b)
        {
            if (b)
            {
                EnableOverlay(blocked ? 2 : 1, true);
            }
            else
            {
                EnableOverlay(1, false);
                EnableOverlay(2, false);
            }
        }

        public void ShowMove(bool b)
        {
            EnableOverlay(7, b);
        }

        public void ShowShort(bool b)
        {
            EnableOverlay(8, b);
        }

        public void ShowInfluence(bool b)
        {
            Sprite s = GetChild<Sprite>(0);
            s.Modulate = new Color(f / 10.0f, 0.0f, 0.0f);
            EnableOverlay(0, b);
        }
    }
}
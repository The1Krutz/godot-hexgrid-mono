using System;
using Godot;
using MonoHexGrid;

namespace Demo
{
    /// <summary>
    ///
    /// </summary>
    public enum HexType
    {
        Plain = -1,
        City = 0,
        Wood = 1,
        Mountain = 2,
        Blocked = 3
    }

    /// <summary>
    ///
    /// </summary>
    public class Hex : Tile
    {
        public HexType type = HexType.Plain;
        public int roads;

        public override void _Ready()
        {
            type = HexType.Plain;
        }

        /// <summary>
        ///
        /// </summary>
        public string Inspect()
        {
            string s = Enum.GetName(typeof(HexType), type);
            return $"[{coords.x:F0};{coords.y:F0}]\n -> ({Position.x:F0};{Position.y:F0})\n -> {s}\ne:{Elevation()} h:{Height()} c:{Cost()} r:{roads}";
        }

        /// <summary>
        /// is there a road with given orientation that drives out of that Tile
        /// </summary>
        /// <param name="orientation"></param>
        public override bool HasRoad(int orientation)
        {
            return (orientation & roads) > 0;
        }

        /// <summary>
        /// is the line of sight blocked from a Tile to another, d beeing the distance between from and
        /// to, dt beeing the distance between from and this Tile
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="d"></param>
        /// <param name="dt"></param>
        /// <exception cref="ArgumentException"><paramref name="from"/> is not <c>Hex</c>.</exception>
        public override bool BlockLos(Tile from, Tile to, float d, float dt)
        {
            if (from is Hex fromhex && to is Hex tohex)
            {
                return BlockLos(fromhex, tohex, d, dt);
            }

            throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="d"></param>
        /// <param name="dt"></param>
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
                return to.Elevation() <= h && (h * dt / (e - h)) >= (d - dt);
            }
            h -= e;
            return (h * d / dt) >= to.Elevation() - e;
        }

        /// <summary>
        ///
        /// </summary>
        public void Change()
        {
            type = (HexType)((((int)type + 2) % 5) - 1);
            for (int i = 0; i < 4; i++)
            {
                EnableOverlay(i + 3, i == (int)type);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int Cost()
        {
            return type switch
            {
                HexType.Plain => 1,
                HexType.Blocked => -1,
                _ => (int)type + 1,
            };
        }

        /// <summary>
        ///
        /// </summary>
        public int Height()
        {
            return type switch
            {
                HexType.City => 2,
                HexType.Wood => 1,
                HexType.Mountain => 0,
                _ => 0,
            };
        }

        /// <summary>
        ///
        /// </summary>
        public int Elevation()
        {
            return type == HexType.Mountain ? 3 : 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category"></param>
        public int RangeModifier(int category)
        {
            return type == HexType.Mountain ? 1 : 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category"></param>
        /// <param name="orientation"></param>
        public int AttackModifier(int category, int orientation)
        {
            return type == HexType.Wood ? 2 : 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category"></param>
        /// <param name="orientation"></param>
        public int DefenseValue(int category, int orientation)
        {
            return type switch
            {
                HexType.City => 2,
                HexType.Wood => 1,
                HexType.Mountain => 1,
                _ => 0,
            };
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="b"></param>
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="b"></param>
        public void ShowMove(bool b)
        {
            EnableOverlay(7, b);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="b"></param>
        public void ShowShort(bool b)
        {
            EnableOverlay(8, b);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="b"></param>
        public void ShowInfluence(bool b)
        {
            Sprite s = GetChild<Sprite>(0);
            s.Modulate = new Color(f / 10.0f, 0.0f, 0.0f);
            EnableOverlay(0, b);
        }
    }
}
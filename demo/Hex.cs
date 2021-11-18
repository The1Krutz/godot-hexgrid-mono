using System;
using Godot;
using MonoHexGrid;

namespace Demo
{
    public enum HexType
    {
        Plain = -1,
        City = 0,
        Wood = 1,
        Mountain = 2,
        Blocked = 3
    }

    public class Hex : Tile
    {
        public HexType Type { get; set; } = HexType.Plain;
        public int Roads { get; set; }

        public override void _Ready()
        {
            Type = HexType.Plain;
        }

        public override string ToString()
        {
            string s = Enum.GetName(typeof(HexType), Type);
            return $"[{Coordinates.x:F0};{Coordinates.y:F0}]\n -> ({Position.x:F0};{Position.y:F0})"
                + $"\n -> {s}\ne:{Elevation()} h:{Height()} c:{Cost()} r:{Roads}";
        }

        /// <summary>
        /// is there a road with given orientation that drives out of that Tile
        /// </summary>
        /// <param name="orientation"></param>
        public override bool HasRoad(int orientation)
        {
            return (orientation & Roads) > 0;
        }

        /// <summary>
        /// is the line of sight blocked from a Tile to another, d beeing the distance between from and
        /// to, dt beeing the distance between from and this Tile
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="distance">Distance between from and to</param>
        /// <param name="distanceThis">Distance between from and this Tile</param>
        /// <exception cref="ArgumentException"><paramref name="from"/> is not <c>Hex</c>.</exception>
        public override bool IsLosBlocked(Tile from, Tile to, float distance, float distanceThis)
        {
            if (from is Hex fromhex && to is Hex tohex)
            {
                return IsLosBlocked(fromhex, tohex, distance, distanceThis);
            }

            throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
        }

        /// <summary>
        /// is the line of sight blocked from a Tile to another, d beeing the distance between from and
        /// to, dt beeing the distance between from and this Tile
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="distance">Distance between from and to</param>
        /// <param name="distanceThis">Distance between from and this Tile</param>
        private bool IsLosBlocked(Hex from, Hex to, float distance, float distanceThis)
        {
            int h = Height() + Elevation();
            if (h == 0)
            {
                return false;
            }
            int e = from.Elevation();
            if (e > h)
            {
                return to.Elevation() <= h && (h * distanceThis / (e - h)) >= (distance - distanceThis);
            }
            h -= e;
            return (h * distance / distanceThis) >= to.Elevation() - e;
        }

        public void Change()
        {
            Type = (HexType)((((int)Type + 2) % 5) - 1);
            for (int i = 0; i < 4; i++)
            {
                EnableOverlay(i + 3, i == (int)Type);
            }
        }

        public int Cost()
        {
            switch (Type)
            {
                case HexType.Plain:
                    return 1;
                case HexType.Blocked:
                    return -1;
                default:
                    return (int)Type + 1;
            }
        }

        public int Height()
        {
            switch (Type)
            {
                case HexType.City:
                    return 2;
                case HexType.Wood:
                    return 1;
                case HexType.Mountain:
                default:
                    return 0;
            }
        }

        public int Elevation()
        {
            return Type == HexType.Mountain ? 3 : 0;
        }

        public int RangeModifier(int category)
        {
            return Type == HexType.Mountain ? 1 : 0;
        }

        public int AttackModifier(int category, int orientation)
        {
            return Type == HexType.Wood ? 2 : 0;
        }

        public int DefenseValue(int category, int orientation)
        {
            switch (Type)
            {
                case HexType.City:
                    return 2;
                case HexType.Wood:
                case HexType.Mountain:
                    return 1;
                default:
                    return 0;
            }
        }

        public void ShowLos(bool turnLosOn)
        {
            if (turnLosOn)
            {
                EnableOverlay(IsBlocked ? 2 : 1, true);
            }
            else
            {
                EnableOverlay(1, false);
                EnableOverlay(2, false);
            }
        }

        public void ShowMove(bool turnMoveOn)
        {
            EnableOverlay(7, turnMoveOn);
        }

        public void ShowMovementPath(bool turnMovePathOn)
        {
            EnableOverlay(8, turnMovePathOn);
        }

        public void ShowInfluence(bool turnInfluenceOn)
        {
            Sprite s = GetChild<Sprite>(0);
            s.Modulate = new Color(F / 10.0f, 0.0f, 0.0f);
            EnableOverlay(0, turnInfluenceOn);
        }
    }
}
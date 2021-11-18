using System;
using MonoHexGrid;

namespace Demo
{
    public class Unit : Piece
    {
        /// <summary>
        /// movement points
        /// </summary>
        public override int GetMp()
        {
            return 2;
        }

        /// <summary>
        /// movement point bonus if you start your movement on a road and follow it
        /// </summary>
        public override int RoadMarchBonus()
        {
            return 2;
        }

        /// <summary>
        /// movement cost from a Tile to another adjacent Tile
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="orientation"></param>
        /// <exception cref="ArgumentException"></exception>
        public override int MoveCost(Tile source, Tile destination, int orientation)
        {
            if (source is Hex srchex && destination is Hex dsthex)
            {
                return MoveCost(srchex, dsthex, orientation);
            }

            throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
        }

        private int MoveCost(Hex source, Hex destination, int orientation)
        {
            return (source.HasRoad(orientation) && destination.Type != HexType.Blocked)
                ? 1
                : destination.Cost();
        }

        /// <summary>
        /// are you allowed to move into that Tile as only move even if you don't have enough movement points
        /// </summary>
        /// <param name="destination"></param>
        public override bool AtLeastOneTile(Tile destination)
        {
            return false;
        }

        /// <summary>
        /// the maximum range of fire with a given category of weapon
        /// </summary>
        /// <param name="category"></param>
        /// <param name="from"></param>
        /// <exception cref="ArgumentException"></exception>
        public override int MaxRangeOfFire(int category, Tile from)
        {
            if (from is Hex fromhex)
            {
                return MaxRangeOfFire(category, fromhex);
            }

            throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
        }

        private int MaxRangeOfFire(int category, Hex from)
        {
            return 6 + from.RangeModifier(category);
        }

        /// <summary>
        /// the projected volume of fire with a given category of weapon at a given distance,
        /// out of a given Tile with a given orientation, into a given Tile with a given orientation
        /// </summary>
        /// <param name="category"></param>
        /// <param name="distance"></param>
        /// <param name="source"></param>
        /// <param name="sourceOrientation"></param>
        /// <param name="destination"></param>
        /// <param name="destinationOrientation"></param>
        /// <exception cref="ArgumentException"></exception>
        public override int VolumeOfFire(
            int category,
            int distance,
            Tile source,
            int sourceOrientation,
            Tile destination,
            int destinationOrientation)
        {
            if (source is Hex srchex && destination is Hex dsthex)
            {
                return VolumeOfFire(
                    category,
                    distance,
                    srchex,
                    sourceOrientation,
                    dsthex,
                    destinationOrientation);
            }

            throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
        }

        private int VolumeOfFire(
            int category,
            int distance,
            Hex source,
            int sourceOrientation,
            Hex destination,
            int destinationOrientation)
        {
            int fp = 10;
            if (distance > 6)
            {
                return -1;
            }
            else if (distance > 4)
            {
                fp = 4;
            }
            else if (distance > 2)
            {
                fp = 7;
            }
            fp -= source.AttackModifier(category, sourceOrientation);
            fp -= destination.DefenseValue(category, destinationOrientation);
            return fp;
        }
    }
}
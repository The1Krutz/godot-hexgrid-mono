using System;
using MonoHexGrid;

namespace Demo
{
    /// <summary>
    ///
    /// </summary>
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
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="orientation"></param>
        /// <exception cref="ArgumentException"></exception>
        public override int MoveCost(Tile src, Tile dst, int orientation)
        {
            if (src is Hex srchex && dst is Hex dsthex)
            {
                return MoveCost(srchex, dsthex, orientation);
            }

            throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="orientation"></param>
        private int MoveCost(Hex src, Hex dst, int orientation)
        {
            return (src.HasRoad(orientation) && dst.type != HexType.Blocked)
                ? 1
                : dst.Cost();
        }

        /// <summary>
        /// are you allowed to move into that Tile as only move even if you don't have enough movement points
        /// </summary>
        /// <param name="dst"></param>
        public override bool AtLeastOneTile(Tile dst)
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="category"></param>
        /// <param name="from"></param>
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
        /// <param name="src"></param>
        /// <param name="src_o"></param>
        /// <param name="dst"></param>
        /// <param name="dst_o"></param>
        /// <exception cref="ArgumentException"></exception>
        public override int VolumeOfFire(int category, int distance, Tile src, int src_o, Tile dst, int dst_o)
        {
            if (src is Hex srchex && dst is Hex dsthex)
            {
                return VolumeOfFire(category, distance, srchex, src_o, dsthex, dst_o);
            }

            throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category"></param>
        /// <param name="distance"></param>
        /// <param name="src"></param>
        /// <param name="srcO"></param>
        /// <param name="dst"></param>
        /// <param name="dstO"></param>
        private int VolumeOfFire(int category, int distance, Hex src, int srcO, Hex dst, int dstO)
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
            fp -= src.AttackModifier(category, srcO);
            fp -= dst.DefenseValue(category, dstO);
            return fp;
        }
    }
}
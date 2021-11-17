using Godot;

namespace MonoHexGrid
{
    /// <summary>
    ///
    /// </summary>
    public abstract class Piece : Node2D
    {
        /// <summary>
        /// movement points
        /// </summary>
        public abstract int GetMp();

        /// <summary>
        /// movement point bonus if you start your movement on a road and follow it
        /// </summary>
        public abstract int RoadMarchBonus();

        /// <summary>
        /// movement cost from a Tile to another adjacent Tile
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="orientation"></param>
        public abstract int MoveCost(Tile src, Tile dst, int orientation);

        /// <summary>
        /// are you allowed to move into that Tile as only move even if you don't have enough movement points
        /// </summary>
        /// <param name="dst"></param>
        public abstract bool AtLeastOneTile(Tile dst);

        /// <summary>
        /// the maximum range of fire with a given category of weapon
        /// </summary>
        /// <param name="category"></param>
        /// <param name="from"></param>
        public abstract int MaxRangeOfFire(int category, Tile from);

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
        public abstract int VolumeOfFire(int category, int distance, Tile src, int src_o, Tile dst, int dst_o);
    }
}

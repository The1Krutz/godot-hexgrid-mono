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
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="orientation"></param>
        public abstract int MoveCost(Tile source, Tile destination, int orientation);

        /// <summary>
        /// are you allowed to move into that Tile as only move even if you don't have enough movement points
        /// </summary>
        /// <param name="destination"></param>
        public abstract bool AtLeastOneTile(Tile destination);

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
        /// <param name="source"></param>
        /// <param name="sourceOrientation"></param>
        /// <param name="destination"></param>
        /// <param name="destinationOrientation"></param>
        public abstract int VolumeOfFire(
            int category,
            int distance,
            Tile source,
            int sourceOrientation,
            Tile destination,
            int destinationOrientation);
    }
}

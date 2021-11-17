using Godot;

namespace MonoHexGrid {
  public abstract class Piece : Node2D {
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
    public abstract int MoveCost(Tile src, Tile dst, int orientation);

    /// <summary>
    /// are you allowed to move into that Tile as only move even if you don't have enough movement points
    /// </summary>
    public abstract bool AtLeastOneTile(Tile dst);

    /// <summary>
    /// the maximum range of fire with a given category of weapon
    /// </summary>
    public abstract int MaxRangeOfFire(int category, Tile from);

    /// <summary>
    /// the projected volume of fire with a given category of weapon at a given distance,
    /// out of a given Tile with a given orientation, into a given Tile with a given orientation
    /// </summary>
    public abstract int VolumeOfFire(int category, int distance, Tile src, int src_o, Tile dst, int dst_o);
  }
}

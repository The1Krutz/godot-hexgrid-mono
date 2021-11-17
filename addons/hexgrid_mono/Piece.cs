using Godot;

namespace MonoHexGrid {
  public abstract class Piece : Node2D {
    /// <summary>
    /// movement points
    /// </summary>
    public abstract int get_mp();

    /// <summary>
    /// movement point bonus if you start your movement on a road and follow it
    /// </summary>
    public abstract int road_march_bonus();

    /// <summary>
    /// movement cost from a Tile to another adjacent Tile
    /// </summary>
    public abstract int move_cost(Tile src, Tile dst, int orientation);

    /// <summary>
    /// are you allowed to move into that Tile as only move even if you don't have enough movement points
    /// </summary>
    public abstract bool at_least_one_tile(Tile dst);

    /// <summary>
    /// the maximum range of fire with a given category of weapon
    /// </summary>
    public abstract int max_range_of_fire(int category, Tile from);

    /// <summary>
    /// the projected volume of fire with a given category of weapon at a given distance,
    /// out of a given Tile with a given orientation, into a given Tile with a given orientation
    /// </summary>
    public abstract int volume_of_fire(int category, int distance, Tile src, int src_o, Tile dst, int dst_o);
  }
}

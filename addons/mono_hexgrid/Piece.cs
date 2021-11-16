using Godot;

namespace MonoHexGrid {
  /// <summary>
  ///
  /// </summary>
  public abstract class MonoPiece : Node2D {
    /// <summary>
    /// movement points
    /// </summary>
    public virtual int get_mp() {
      GD.Print("Piece#get_mp() must be overriden in a subclass");
      return 0;
    }

    /// <summary>
    /// movement point bonus if you start your movement on a road and follow it
    /// </summary>
    public virtual int road_march_bonus() {
      GD.Print("Piece#road_march_bonus() must be overriden in a subclass");
      return 0;
    }

    /// <summary>
    /// movement cost from a Tile to another adjacent Tile
    /// </summary>
    public virtual int move_cost(MonoTile src, MonoTile dst, int orientation) {
      GD.Print("Piece#move_cost() must be overriden in a subclass");
      return -1; // impracticable
    }

    /// <summary>
    /// are you allowed to move into that Tile as only move even if you don't have enough movement points
    /// </summary>
    public virtual bool at_least_one_tile(MonoTile dst) {
      GD.Print("Piece#at_least_one_tile() must be overriden in a subclass");
      return true;
    }

    /// <summary>
    /// the maximum range of fire with a given category of weapon
    /// </summary>
    public virtual int max_range_of_fire(int category, MonoTile from) {
      GD.Print("Piece#max_range_of_fire() must be overriden in a subclass");
      return 0;
    }

    /// <summary>
    /// the projected volume of fire with a given category of weapon at a given distance,
    /// out of a given Tile with a given orientation, into a given Tile with a given orientation
    /// </summary>
    public virtual int volume_of_fire(int category, int distance, MonoTile src, int src_o, MonoTile dst, int dst_o) {
      GD.Print("Piece#volume_of_fire() must be overriden in a subclass");
      return -1; // out of range
    }
  }
}

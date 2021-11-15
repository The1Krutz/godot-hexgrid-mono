using MonoHexGrid;

namespace Demo {
  public class Unit : MonoPiece {
    public override bool at_least_one_tile(MonoTile dst) {
      return false;
    }

    public override int get_mp() {
      return 2;
    }

    public override int max_range_of_fire(int category, MonoTile from) {
      return 6 + from.range_modifier(category);
    }

    public override int move_cost(MonoTile src, MonoTile dst, int orientation) {
      return (src.has_road(orientation) && dst.type != 3) ? 1 : dst.cost();
    }

    public override int road_march_bonus() {
      return 2;
    }

    public override int volume_of_fire(int category, int distance, MonoTile src, int src_o, MonoTile dst, int dst_o) {
      int fp = 10;
      if (distance > 6) {
        return -1;
      } else if (distance > 4) {
        fp = 4;
      } else if (distance > 2) {
        fp = 7;
      }
      fp -= src.attack_modifier(category, src_o);
      fp -= dst.defense_value(category, dst_o);
      return fp;
    }
  }
}

using MonoHexGrid;

namespace Demo {
  public class Unit : MonoPiece {
    public override int get_mp() {
      return 2;
    }

    public override int road_march_bonus() {
      return 2;
    }

    public int move_cost(Hex src, Hex dst, int orientation) {
      return ((src.has_road(orientation) && dst.type != 3) ? 1 : dst.cost());
    }

    public int max_range_of_fire(int category, Hex from) {
      return 6 + from.range_modifier(category);
    }

    public int volume_of_fire(int category, int distance, Hex src, int srcO, Hex dst, int dstO) {
      int fp = 10;
      if (distance > 6) {
        return -1;
      } else if (distance > 4) {
        fp = 4;
      } else if (distance > 2) {
        fp = 7;
      }
      fp -= src.attack_modifier(category, srcO);
      fp -= dst.defense_value(category, dstO);
      return fp;
    }
  }
}
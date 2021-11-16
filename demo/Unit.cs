using System;
using MonoHexGrid;

namespace Demo {
  public class Unit : Piece {
    public override int get_mp() {
      return 2;
    }

    public override int road_march_bonus() {
      return 2;
    }

    public override int move_cost(Tile src, Tile dst, int orientation) {
      if (src is Hex srchex && dst is Hex dsthex) {
        return move_cost(srchex, dsthex, orientation);
      }

      throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
    }

    private int move_cost(Hex src, Hex dst, int orientation) {
      return (src.has_road(orientation) && dst.type != 3) ? 1 : dst.cost();
    }

    public override bool at_least_one_tile(Tile dst) {
      return false;
    }

    public override int max_range_of_fire(int category, Tile from) {
      if (from is Hex fromhex) {
        return max_range_of_fire(category, fromhex);
      }

      throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
    }

    private int max_range_of_fire(int category, Hex from) {
      return 6 + from.range_modifier(category);
    }

    public override int volume_of_fire(int category, int distance, Tile src, int src_o, Tile dst, int dst_o) {
      if (src is Hex srchex && dst is Hex dsthex) {
        return volume_of_fire(category, distance, srchex, src_o, dsthex, dst_o);
      }

      throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
    }

    private int volume_of_fire(int category, int distance, Hex src, int srcO, Hex dst, int dstO) {
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
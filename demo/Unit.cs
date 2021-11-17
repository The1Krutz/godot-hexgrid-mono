using System;
using MonoHexGrid;

namespace Demo {
  public class Unit : Piece {
    public override int GetMp() {
      return 2;
    }

    public override int RoadMarchBonus() {
      return 2;
    }

    public override int MoveCost(Tile src, Tile dst, int orientation) {
      if (src is Hex srchex && dst is Hex dsthex) {
        return MoveCost(srchex, dsthex, orientation);
      }

      throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
    }

    private int MoveCost(Hex src, Hex dst, int orientation) {
      return (src.HasRoad(orientation) && dst.type != 3) ? 1 : dst.Cost();
    }

    public override bool AtLeastOneTile(Tile dst) {
      return false;
    }

    public override int MaxRangeOfFire(int category, Tile from) {
      if (from is Hex fromhex) {
        return MaxRangeOfFire(category, fromhex);
      }

      throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
    }

    private int MaxRangeOfFire(int category, Hex from) {
      return 6 + from.RangeModifier(category);
    }

    public override int VolumeOfFire(int category, int distance, Tile src, int src_o, Tile dst, int dst_o) {
      if (src is Hex srchex && dst is Hex dsthex) {
        return VolumeOfFire(category, distance, srchex, src_o, dsthex, dst_o);
      }

      throw new ArgumentException("Somehow ended up with the wrong type of Tiles!");
    }

    private int VolumeOfFire(int category, int distance, Hex src, int srcO, Hex dst, int dstO) {
      int fp = 10;
      if (distance > 6) {
        return -1;
      } else if (distance > 4) {
        fp = 4;
      } else if (distance > 2) {
        fp = 7;
      }
      fp -= src.AttackModifier(category, srcO);
      fp -= dst.DefenseValue(category, dstO);
      return fp;
    }
  }
}
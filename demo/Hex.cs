using Godot;
using MonoHexGrid;

namespace Demo {
  public class Hex : MonoTile {
    public int type = -1;
    public int roads = 0;

    public override void _Ready() {
      type = -1;
    }

    public string inspect() {
      string s = "plain";
      if (type == 0) {
        s = "city";
      } else if (type == 1) {
        s = "wood";
      } else if (type == 2) {
        s = "mountain";
      } else if (type == 3) {
        s = "blocked";
      }
      return $"[{coords.x};{coords.y}]\n -> ({Position.x};{Position.y})\n -> {s}\ne:{elevation()} h:{height()} c:{cost()} r:{roads}";
    }

    public override bool has_road(int orientation) {
      return (orientation & roads) > 0;
    }

    public bool block_los(Hex from, Hex to, float d, float dt) {
      int h = height() + elevation();
      if (h == 0) {
        return false;
      }
      int e = from.elevation();
      if (e > h) {
        if (to.elevation() > h) {
          return false;
        }
        return (h * dt / (e - h)) >= (d - dt);
      }
      h -= e;
      return ((h * d / dt) >= to.elevation() - e);
    }

    public void change() {
      type = (type + 2) % 5 - 1;
      for (int i = 0; i < 4; i++) // i in range(4), double check that gives the same range as this
      { enable_overlay(i + 3, i == type); }
    }

    public int cost() {
      if (type == -1) {
        return 1;
      } else if (type == 3) {
        return -1;
      }
      return type + 1;
    }

    public int height() {
      if (type == 0) {
        return 2;
      } else if (type == 1) {
        return 1;
      } else if (type == 2) {
        return 0;
      }
      return 0;
    }

    public int elevation() {
      if (type == 2) {
        return 3;
      }
      return 0;
    }

    public int range_modifier(int category) {
      return type == 2 ? 1 : 0;
    }

    public int attack_modifier(int category, int orientation) {
      return type == 1 ? 2 : 0;
    }

    public int defense_value(int category, int orientation) {
      if (type == 0) {
        return 2;
      } else if (type == 1) {
        return 1;
      } else if (type == 2) {
        return 1;
      }
      return 0;
    }

    public void show_los(bool b) {
      if (b) {
        enable_overlay((blocked ? 2 : 1), true);
      } else {
        enable_overlay(1, false);
        enable_overlay(2, false);
      }
    }

    public void show_move(bool b) {
      enable_overlay(7, b);
    }

    public void show_short(bool b) {
      enable_overlay(8, b);
    }

    public void show_influence(bool b) {
      Sprite s = GetChild<Sprite>(0);
      s.Modulate = Color.Color8((byte)(f / 10.0), 0, 0);
      enable_overlay(0, b);
    }
  }
}
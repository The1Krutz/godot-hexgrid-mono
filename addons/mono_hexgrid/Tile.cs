using System.Collections.Generic;
using Godot;

namespace MonoHexGrid {
  public abstract class Tile : Node2D {
    public Vector2 coords;
    public bool blocked;
    public bool on_map;

    public int acc;
    public float f;
    public Tile parent;
    public bool road_march;
    public int search_count;

    public void configure(Vector2 position, Vector2 coordinates, List<string> o) {
      Position = position;
      coords = coordinates;
      on_map = true;

      foreach (var t in o) {
        Sprite s = new Sprite {
          Texture = GD.Load<Texture>(t),
          Visible = false
        };
        AddChild(s);
      }
      Visible = false;
    }

    /// <summary>
    /// is there a road with given orientation that drives out of that Tile
    /// </summary>
    public abstract bool has_road(int orientation);

    /// <summary>
    /// is the line of sight blocked from a Tile to another, d beeing the distance between from and
    /// to, dt beeing the distance between from and this Tile
    /// </summary>
    public abstract bool block_los(Tile from, Tile to, float d, float dt);

    public void enable_overlay(int i, bool v) {
      GetChild<Node2D>(i).Visible = v;
      if (v) {
        Visible = true;
      } else {
        Visible = false;
        foreach (Node2D o in GetChildren()) {
          if (o.Visible) {
            Visible = true;
            break;
          }
        }
      }
    }

    public bool is_overlay_on(int i) {
      return GetChild<Node2D>(i).Visible;
    }
  }
}

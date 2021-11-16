using System.Collections.Generic;
using System.Linq;
using Godot;
using MonoHexGrid;

namespace Demo {
  public class Map : Sprite {
    [Signal]
    public delegate void hex_touched(Vector2 pos, Vector2 hex, int key);

    public const string MAPH = "res://demo/assets/map-h.png";
    public const string MAPV = "res://demo/assets/map-v.png";
    public const string BLOCK = "res://demo/assets/block.png";
    public const string BLACK = "res://demo/assets/black.png";
    public const string MOVE = "res://demo/assets/move.png";
    public const string SHORT = "res://demo/assets/short.png";
    public const string RED = "res://demo/assets/red.png";
    public const string GREEN = "res://demo/assets/green.png";
    public const string TREE = "res://demo/assets/tree.png";
    public const string CITY = "res://demo/assets/city.png";
    public const string MOUNT = "res://demo/assets/mountain.png";

    public Sprite drag;
    public HexBoard board;
    public Vector2 prev;
    public Dictionary<int, Tile> hexes = new Dictionary<int, Tile>();
    public int hex_rotation;
    private Vector2 p0;
    private Vector2 p1;
    public List<Hex> los = new List<Hex>();
    public List<Hex> move = new List<Hex>();
    public List<Hex> shoort = new List<Hex>();
    public List<Hex> influence = new List<Hex>();
    public Unit unit;
    public bool show_los;
    public bool show_move;
    public bool show_influence;

    // $etc
    private Sprite Tank;
    private Sprite Target;
    private Node Hexes;
    private Los Los;

    public override void _Ready() {
      // $etc
      Tank = GetNode<Sprite>("Tank");
      Target = GetNode<Sprite>("Target");
      Hexes = GetNode<Node>("Hexes");
      Los = GetNode<Los>("Los");

      drag = null;
      unit = new Unit();
      rotate_map();
    }

    public void reset() {
      los.Clear();
      move.Clear();
      shoort.Clear();
      influence.Clear();
      hexes.Clear();
      hexes[-1] = new Hex();  // off map
      p0 = new Vector2(0, 0);
      p1 = new Vector2(3, 3);

      Tank.Position = board.center_of(p0);
      Target.Position = board.center_of(p1);
      foreach (Node hex in Hexes.GetChildren()) {
        Hexes.RemoveChild(hex);
        hex.QueueFree();
      }
      compute();
    }

    public void rotate_map() {
      Texture = GD.Load<Texture>(IsInstanceValid(board) && board.v ? MAPH : MAPV);
      configure();
      reset();
    }

    public void set_mode(bool l, bool m, bool i) {
      show_los = l;
      show_move = m;
      show_influence = i;
      compute();
    }

    public void configure() {
      bool v = (IsInstanceValid(board) && board.v);
      Vector2 v0 = new Vector2(50, 100);
      if (Centered) {
        Vector2 ts = Texture.GetSize();
        if (v) {
          v0.x -= ts.y / 2;
          v0.y -= ts.x / 2;
        } else {
          v0 -= ts / 2;
        }
      }
      if (v) {
        hex_rotation = 30;
        board = new HexBoard(10, 4, 100, v0, false, GD.FuncRef(this, "get_tile"));
      } else {
        hex_rotation = 0;
        board = new HexBoard(10, 7, 100, v0, true, GD.FuncRef(this, "get_tile"));
      }
    }

    public Vector2 texture_size() {
      return Texture.GetSize();
    }

    public Vector2 center() {
      return Centered ? new Vector2(0, 0) : Texture.GetSize() / 2;
    }

    public void on_mouse_move() {
      if (drag != null) {
        drag.Position = GetLocalMousePosition();
      }
    }

    public bool on_click(bool pressed) {
      Vector2 pos = GetLocalMousePosition();
      Vector2 coords = board.to_map(pos);
      if (pressed) {
        notify(pos, coords);
        prev = coords;
        if (board.to_map(Tank.Position) == coords) {
          drag = Tank;
        } else if (board.to_map(Target.Position) == coords) {
          drag = Target;
        } else {
          return true;
        }
      } else {
        if (drag != null) {
          if (board.is_on_map(coords)) {
            drag.Position = board.center_of(coords);
            if (drag == Tank) {
              p0 = coords;
            } else {
              p1 = coords;
            }
            notify(pos, coords);
            compute();
          } else {
            drag.Position = board.center_of(prev);
          }
          drag = null;
        } else {
          if (coords == prev && board.is_on_map(coords)) {
            change_tile(coords, pos);
          }
        }
      }
      return false;
    }

    public void change_tile(Vector2 coords, Vector2 pos) {
      Hex hex = (Hex)board.get_tile(coords);
      hex.change();
      notify(pos, coords);
      compute();
    }

    public Tile get_tile(Vector2 coords, int k) {
      if (hexes.ContainsKey(k)) {
        return hexes[k];
      }
      Hex hex = new Hex();
      hex.roads = get_road(k);
      hex.RotationDegrees = hex_rotation;
      hex.configure(board.center_of(coords), coords, new List<string>() { RED, GREEN, BLACK, CITY, TREE, MOUNT, BLOCK, MOVE, SHORT });
      hexes[k] = hex;
      Hexes.AddChild(hex);
      return hex;
    }

    public int get_road(int k) {
      if (!board.v) {
        return 0;
      }
      int v = 0;

      Dictionary<MonoHexGrid.Orientation, List<int>> RoadsLookup = new Dictionary<MonoHexGrid.Orientation, List<int>> {
        [MonoHexGrid.Orientation.E] = new List<int> { 19, 20, 21, 23, 24, 42, 43, 44, 45, 46, 47 },
        [MonoHexGrid.Orientation.NE] = new List<int> { 7, 16, 25, 32 },
        [MonoHexGrid.Orientation.N] = new List<int>(),
        [MonoHexGrid.Orientation.NW] = new List<int> { 32, 42, 52, 62 },
        [MonoHexGrid.Orientation.W] = new List<int> { 19, 20, 21, 22, 24, 25, 43, 44, 45, 46, 47 },
        [MonoHexGrid.Orientation.SW] = new List<int> { 7, 16, 23 },
        [MonoHexGrid.Orientation.S] = new List<int>(),
        [MonoHexGrid.Orientation.SE] = new List<int> { 22, 32, 42, 52, 62 }
      };

      foreach (MonoHexGrid.Orientation orientation in RoadsLookup.Keys) {
        v += RoadsLookup[orientation].Contains(k) ? (int)orientation : 0;
      }

      return v;
    }

    public void notify(Vector2 pos, Vector2 coords) {
      EmitSignal("hex_touched", pos, board.get_tile(coords), (board.is_on_map(coords) ? board.key(coords) : -1));
    }

    public void compute() {
      Los.Visible = false;
      foreach (var hex in los) {
        hex.show_los(false);
      }
      if (show_los) {
        Los.Visible = true;
        Vector2 ct = board.line_of_sight(p0, p1, los.Cast<Tile>().ToList());
        Los.setup(Tank.Position, Target.Position, ct);
        foreach (var hex in los) {
          hex.show_los(true);
        }
      }
      foreach (var hex in move) {
        hex.show_move(false);
      }
      foreach (var hex in shoort) {
        hex.show_short(false);
      }
      if (show_move) {
        board.possible_moves(unit, board.get_tile(p0), move.Cast<Tile>().ToList());
        board.shortest_path(unit, board.get_tile(p0), board.get_tile(p1), shoort.Cast<Tile>().ToList());
        foreach (var hex in move) {
          hex.show_move(true);
        }
        foreach (var i in GD.Range(1, shoort.Count - 1)) {
          shoort[i].show_short(true);
        }
      }
      foreach (var hex in influence) {
        hex.show_influence(false);
      }
      if (show_influence) {
        board.range_of_influence(unit, board.get_tile(p0), 0, influence.Cast<Tile>().ToList());
        foreach (var hex in influence) {
          hex.show_influence(true);
        }
      }
    }
  }
}
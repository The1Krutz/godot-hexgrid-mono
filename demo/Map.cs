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
    public List<Tile> los = new List<Tile>();
    public List<Tile> move = new List<Tile>();
    public List<Tile> shoort = new List<Tile>();
    public List<Tile> influence = new List<Tile>();
    public Unit unit;
    public bool show_los;
    public bool show_move;
    public bool show_influence;

    // $etc
    private Sprite _tank;
    private Sprite _target;
    private Node _hexes;
    private Los _los;

    public override void _Ready() {
      // $etc
      _tank = GetNode<Sprite>("Tank");
      _target = GetNode<Sprite>("Target");
      _hexes = GetNode<Node>("Hexes");
      _los = GetNode<Los>("Los");

      drag = null;
      unit = new Unit();
      rotate_map();
    }

    public void Reset() {
      los.Clear();
      move.Clear();
      shoort.Clear();
      influence.Clear();
      hexes.Clear();
      hexes[-1] = new Hex(); // off map
      p0 = new Vector2(0, 0);
      p1 = new Vector2(3, 3);

      _tank.Position = board.CenterOf(p0);
      _target.Position = board.CenterOf(p1);
      foreach (Node hex in _hexes.GetChildren()) {
        _hexes.RemoveChild(hex);
        hex.QueueFree();
      }
      compute();
    }

    public void rotate_map() {
      Texture = GD.Load<Texture>(IsInstanceValid(board) && board.v ? MAPH : MAPV);
      configure();
      Reset();
    }

    public void set_mode(bool l, bool m, bool i) {
      show_los = l;
      show_move = m;
      show_influence = i;
      compute();
    }

    public void configure() {
      bool v = IsInstanceValid(board) && board.v;
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
        board = new HexBoard(10, 4, 100, v0, false, get_tile);
      } else {
        hex_rotation = 0;
        board = new HexBoard(10, 7, 100, v0, true, get_tile);
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
      Vector2 coords = board.ToMap(pos);
      if (pressed) {
        notify(pos, coords);
        prev = coords;
        if (board.ToMap(_tank.Position) == coords) {
          drag = _tank;
        } else if (board.ToMap(_target.Position) == coords) {
          drag = _target;
        } else {
          return true;
        }
      } else {
        if (drag != null) {
          if (board.IsOnMap(coords)) {
            drag.Position = board.CenterOf(coords);
            if (drag == _tank) {
              p0 = coords;
            } else {
              p1 = coords;
            }
            notify(pos, coords);
            compute();
          } else {
            drag.Position = board.CenterOf(prev);
          }
          drag = null;
        } else {
          if (coords == prev && board.IsOnMap(coords)) {
            change_tile(coords, pos);
          }
        }
      }
      return false;
    }

    public void change_tile(Vector2 coords, Vector2 pos) {
      Hex hex = (Hex)board.GetTile(coords);
      hex.change();
      notify(pos, coords);
      compute();
    }

    public Tile get_tile(Vector2 coords, int k) {
      if (hexes.ContainsKey(k)) {
        return hexes[k];
      }
      Hex hex = new Hex {
        roads = get_road(k),
        RotationDegrees = hex_rotation
      };
      hex.configure(board.CenterOf(coords), coords, new List<string>() { RED, GREEN, BLACK, CITY, TREE, MOUNT, BLOCK, MOVE, SHORT });
      hexes[k] = hex;
      _hexes.AddChild(hex);
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
      EmitSignal(nameof(hex_touched), pos, board.GetTile(coords), board.IsOnMap(coords) ? board.Key(coords) : -1);
    }

    public void compute() {
      _los.Visible = false;
      foreach (Hex hex in los) {
        hex.show_los(false);
      }
      if (show_los) {
        _los.Visible = true;
        Vector2 ct = board.LineOfSight(p0, p1, los);
        _los.setup(_tank.Position, _target.Position, ct);
        foreach (Hex hex in los) {
          hex.show_los(true);
        }
      }
      foreach (Hex hex in move) {
        hex.show_move(false);
      }
      foreach (Hex hex in shoort) {
        hex.show_short(false);
      }
      if (show_move) {
        board.PossibleMoves(unit, board.GetTile(p0), move);
        board.ShortestPath(unit, board.GetTile(p0), board.GetTile(p1), shoort);
        foreach (Hex hex in move) {
          hex.show_move(true);
        }
        foreach (Hex hex in shoort) {
          hex.show_short(true);
        }
      }
      foreach (Hex hex in influence) {
        hex.show_influence(false);
      }
      if (show_influence) {
        board.RangeOfInfluence(unit, board.GetTile(p0), 0, influence);
        foreach (Hex hex in influence) {
          hex.show_influence(true);
        }
      }
    }
  }
}

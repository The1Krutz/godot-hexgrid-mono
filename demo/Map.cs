using System.Collections.Generic;
using Godot;
using MonoHexGrid;

namespace Demo
{
    /// <summary>
    ///
    /// </summary>
    public class Map : Sprite
    {
        [Signal]
        public delegate void HexTouched(Vector2 position, Vector2 hex, int key);

        public Vector2 TextureSize => Texture.GetSize();
        public Vector2 Center => Centered ? new Vector2(0, 0) : Texture.GetSize() / 2;

        private const string _mapHorizontal = "res://demo/assets/map-h.png";
        private const string _mapVertical = "res://demo/assets/map-v.png";
        private const string _hexBlocked = "res://demo/assets/block.png";
        private const string _hexBlack = "res://demo/assets/black.png";
        private const string _hexMove = "res://demo/assets/move.png";
        private const string _hexMovePath = "res://demo/assets/short.png";
        private const string _hexRed = "res://demo/assets/red.png";
        private const string _hexGreen = "res://demo/assets/green.png";
        private const string _hexTree = "res://demo/assets/tree.png";
        private const string _hexCity = "res://demo/assets/city.png";
        private const string _hexMountain = "res://demo/assets/mountain.png";

        private Sprite _drag;
        private HexBoard _board;
        private Dictionary<int, Tile> _mapHexes = new Dictionary<int, Tile>();
        private List<Tile> _losTiles = new List<Tile>();
        private List<Tile> _moveTiles = new List<Tile>();
        private List<Tile> _movePathTiles = new List<Tile>();
        private List<Tile> _influenceTiles = new List<Tile>();
        private Unit _unit;
        private Vector2 _p0;
        private Vector2 _p1;
        private Vector2 _previous;
        private int _hexRotation;
        private bool _showLos;
        private bool _showMove;
        private bool _showInfluence;

        // $etc
        private Sprite _tank;
        private Sprite _target;
        private Node _hexes;
        private Los _los;

        public override void _Ready()
        {
            // $etc
            _tank = GetNode<Sprite>("Tank");
            _target = GetNode<Sprite>("Target");
            _hexes = GetNode<Node>("Hexes");
            _los = GetNode<Los>("Los");

            _drag = null;
            _unit = new Unit();
            RotateMap();
        }

        /// <summary>
        ///
        /// </summary>
        public void Reset()
        {
            _losTiles.Clear();
            _moveTiles.Clear();
            _movePathTiles.Clear();
            _influenceTiles.Clear();
            _mapHexes.Clear();
            _mapHexes[-1] = new Hex(); // off map
            _p0 = new Vector2(0, 0);
            _p1 = new Vector2(3, 3);

            _tank.Position = _board.CenterOf(_p0);
            _target.Position = _board.CenterOf(_p1);
            foreach (Node hex in _hexes.GetChildren())
            {
                _hexes.RemoveChild(hex);
                hex.QueueFree();
            }
            Compute();
        }

        /// <summary>
        ///
        /// </summary>
        public void RotateMap()
        {
            Texture = GD.Load<Texture>(IsInstanceValid(_board) && _board.HasVerticalEdge
                ? _mapHorizontal
                : _mapVertical);
            Configure();
            Reset();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="showLos"></param>
        /// <param name="showMove"></param>
        /// <param name="showInfluence"></param>
        public void SetMode(bool showLos, bool showMove, bool showInfluence)
        {
            _showLos = showLos;
            _showMove = showMove;
            _showInfluence = showInfluence;
            Compute();
        }

        /// <summary>
        ///
        /// </summary>
        public void Configure()
        {
            bool v = IsInstanceValid(_board) && _board.HasVerticalEdge;
            Vector2 v0 = new Vector2(50, 100);
            if (Centered)
            {
                Vector2 ts = Texture.GetSize();
                if (v)
                {
                    v0.x -= ts.y / 2;
                    v0.y -= ts.x / 2;
                }
                else
                {
                    v0 -= ts / 2;
                }
            }
            if (v)
            {
                _hexRotation = 30;
                _board = new HexBoard(10, 4, 100, v0, false, GetTile);
            }
            else
            {
                _hexRotation = 0;
                _board = new HexBoard(10, 7, 100, v0, true, GetTile);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void OnMouseMove()
        {
            if (_drag != null)
            {
                _drag.Position = GetLocalMousePosition();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pressed"></param>
        public bool OnClick(bool pressed)
        {
            Vector2 pos = GetLocalMousePosition();
            Vector2 coords = _board.ToMap(pos);
            if (pressed)
            {
                Notify(pos, coords);
                _previous = coords;
                if (_board.ToMap(_tank.Position) == coords)
                {
                    _drag = _tank;
                }
                else if (_board.ToMap(_target.Position) == coords)
                {
                    _drag = _target;
                }
                else
                {
                    return true;
                }
            }
            else if (_drag != null)
            {
                if (_board.IsOnMap(coords))
                {
                    _drag.Position = _board.CenterOf(coords);
                    if (_drag == _tank)
                    {
                        _p0 = coords;
                    }
                    else
                    {
                        _p1 = coords;
                    }
                    Notify(pos, coords);
                    Compute();
                }
                else
                {
                    _drag.Position = _board.CenterOf(_previous);
                }
                _drag = null;
            }
            else if (coords == _previous && _board.IsOnMap(coords))
            {
                ChangeTile(coords, pos);
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="position"></param>
        public void ChangeTile(Vector2 coordinates, Vector2 position)
        {
            Hex hex = (Hex)_board.GetTile(coordinates);
            hex.Change();
            Notify(position, coordinates);
            Compute();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="key"></param>
        public Tile GetTile(Vector2 coordinates, int key)
        {
            if (_mapHexes.ContainsKey(key))
            {
                return _mapHexes[key];
            }
            Hex hex = new Hex
            {
                Roads = GetRoad(key),
                RotationDegrees = _hexRotation
            };
            hex.Configure(
                _board.CenterOf(coordinates),
                coordinates,
                new List<string>()
                {
                    _hexRed,
                    _hexGreen,
                    _hexBlack,
                    _hexCity,
                    _hexTree,
                    _hexMountain,
                    _hexBlocked,
                    _hexMove,
                    _hexMovePath
                });
            _mapHexes[key] = hex;
            _hexes.AddChild(hex);
            return hex;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        public int GetRoad(int key)
        {
            if (!_board.HasVerticalEdge)
            {
                return 0;
            }
            int v = 0;

            Dictionary<MonoHexGrid.Orientation, List<int>> RoadsLookup = new Dictionary<MonoHexGrid.Orientation, List<int>>
            {
                [MonoHexGrid.Orientation.E] = new List<int> { 19, 20, 21, 23, 24, 42, 43, 44, 45, 46, 47 },
                [MonoHexGrid.Orientation.SE] = new List<int> { 22, 32, 42, 52, 62 },
                [MonoHexGrid.Orientation.S] = new List<int>(),
                [MonoHexGrid.Orientation.SW] = new List<int> { 7, 16, 23 },
                [MonoHexGrid.Orientation.W] = new List<int> { 19, 20, 21, 22, 24, 25, 43, 44, 45, 46, 47 },
                [MonoHexGrid.Orientation.NW] = new List<int> { 32, 42, 52, 62 },
                [MonoHexGrid.Orientation.N] = new List<int>(),
                [MonoHexGrid.Orientation.NE] = new List<int> { 7, 16, 25, 32 },
            };

            foreach (MonoHexGrid.Orientation orientation in RoadsLookup.Keys)
            {
                v += RoadsLookup[orientation].Contains(key) ? (int)orientation : 0;
            }

            return v;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="coords"></param>
        public void Notify(Vector2 pos, Vector2 coords)
        {
            EmitSignal(
                nameof(HexTouched),
                pos,
                _board.GetTile(coords),
                _board.IsOnMap(coords) ? _board.Key(coords) : -1);
        }

        /// <summary>
        ///
        /// </summary>
        public void Compute()
        {
            _los.Visible = false;
            foreach (Hex hex in _losTiles)
            {
                hex.ShowLos(false);
            }
            if (_showLos)
            {
                _los.Visible = true;
                Vector2 ct = _board.LineOfSight(_p0, _p1, _losTiles);
                _los.Setup(_tank.Position, _target.Position, ct);
                foreach (Hex hex in _losTiles)
                {
                    hex.ShowLos(true);
                }
            }
            foreach (Hex hex in _moveTiles)
            {
                hex.ShowMove(false);
            }
            foreach (Hex hex in _movePathTiles)
            {
                hex.ShowShort(false);
            }
            if (_showMove)
            {
                _board.PossibleMoves(_unit, _board.GetTile(_p0), _moveTiles);
                _board.ShortestPath(_unit, _board.GetTile(_p0), _board.GetTile(_p1), _movePathTiles);
                foreach (Hex hex in _moveTiles)
                {
                    hex.ShowMove(true);
                }
                foreach (Hex hex in _movePathTiles)
                {
                    hex.ShowShort(true);
                }
            }
            foreach (Hex hex in _influenceTiles)
            {
                hex.ShowInfluence(false);
            }
            if (_showInfluence)
            {
                _board.RangeOfInfluence(_unit, _board.GetTile(_p0), 0, _influenceTiles);
                foreach (Hex hex in _influenceTiles)
                {
                    hex.ShowInfluence(true);
                }
            }
        }
    }
}

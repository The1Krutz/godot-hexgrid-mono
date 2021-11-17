using System.Collections.Generic;
using System.Linq;
using Godot;

namespace MonoHexGrid
{
    /// <summary>
    ///
    /// </summary>
    public enum Orientation
    {
        E = 1,
        NE = 2,
        N = 4,
        NW = 8,
        W = 16,
        SW = 32,
        S = 64,
        SE = 128
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="key"></param>
    public delegate Tile GetTile(Vector2 coords, int key);

    /// <summary>
    ///
    /// </summary>
    public class HexBoard : Node
    {
        public int Size => ((int)_columnRow.y / 2 * _tl) + ((int)_columnRow.y % 2 * (int)_columnRow.x);

        public bool HasVerticalEdge { get; set; }

        private const int _DegreeAdj = 2;

        private GetTile _tileFactory;
        private Dictionary<int, int> _angles = new Dictionary<int, int>();
        private List<Tile> _adjacents = new List<Tile>();
        private List<Tile> stack = new List<Tile>();
        private Vector2 _bottomCorner;
        private Vector2 _columnRow;
        private float _side; // hex side length
        private float _width; // hex width between 2 parallel sides
        private float _height; // hex height from the bottom of the middle rectangle to the top of the upper edge
        private float _halfWidth; // half width
        private float _halfHeight; // half height (from the top ef tho middle rectangle to the top of the upper edge)
        private float _m; // dh / dw
        private float _im; // dw / dh
        private int _tl; // num of hexes in 2 consecutives rows
        private int _searchCount;

        /// <summary>
        ///
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="rows"></param>
        /// <param name="side"></param>
        /// <param name="v0"></param>
        /// <param name="hasVerticalEdge"></param>
        /// <param name="tileFactory"></param>
        public HexBoard(
            int cols,
            int rows,
            float side,
            Vector2 v0,
            bool hasVerticalEdge,
            GetTile tileFactory)
        {
            _tileFactory = tileFactory;
            HasVerticalEdge = hasVerticalEdge;
            _side = side;
            _width = _side * 1.73205f;
            _halfWidth = _width / 2.0f;
            _halfHeight = _side / 2.0f;
            _height = _side + _halfHeight;
            _m = _halfHeight / _halfWidth;
            _im = _halfWidth / _halfHeight;
            if (HasVerticalEdge)
            {
                _bottomCorner = v0;
                _columnRow = new Vector2(cols, rows);
            }
            else
            {
                _bottomCorner = v0;
                _columnRow = new Vector2(rows, cols);
            }
            _tl = (2 * (int)_columnRow.x) - 1;
            _searchCount = 0;
            _angles.Clear();
            if (HasVerticalEdge)
            {
                // origin [top-left] East is at 0°, degrees grows clockwise
                _angles[(int)Orientation.E] = 0;
                _angles[(int)Orientation.SE] = 60;
                _angles[(int)Orientation.SW] = 120;
                _angles[(int)Orientation.W] = 180;
                _angles[(int)Orientation.NW] = 240;
                _angles[(int)Orientation.NE] = 300;
            }
            else
            {
                _angles[(int)Orientation.SE] = 30;
                _angles[(int)Orientation.S] = 90;
                _angles[(int)Orientation.SW] = 150;
                _angles[(int)Orientation.NW] = 210;
                _angles[(int)Orientation.N] = 270;
                _angles[(int)Orientation.NE] = 330;
            }
        }

        /// <summary>
        /// fetch a Tile given it's col;row coordinates
        /// </summary>
        /// <param name="coordinates">Grid coordinates for the hex</param>
        public Tile GetTile(Vector2 coordinates)
        {
            return _tileFactory(coordinates, Key(coordinates));
        }

        /// <summary>
        /// Orientation to degrees
        /// </summary>
        /// <param name="orientation"></param>
        public int ToDegrees(int orientation)
        {
            return _angles.TryGetValue(orientation, out int temp) ? temp : -1;
        }

        /// <summary>
        /// convert the given angle between 2 adjacent Tiles into an Orientation
        /// </summary>
        /// <param name="angle"></param>
        public int ToOrientation(float angle)
        {
            foreach (int k in _angles.Keys)
            {
                if (_angles[k] == angle)
                {
                    return k;
                }
            }
            return -1;
        }

        /// <summary>
        /// compute the angle between 2 adjacent Tiles
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public int Angle(Tile from, Tile to)
        {
            float a = Mathf.Rad2Deg((to.Position - from.Position).Angle()) + _DegreeAdj;
            if (a < 0)
            {
                a += 360;
            }
            return (int)(a / 10) * 10;
        }

        /// <summary>
        /// return the opposite of a given Orientation
        /// </summary>
        /// <param name="orientation"></param>
        public int Opposite(int orientation)
        {
            return orientation <= (int)Orientation.NW
                ? orientation << 4
                : orientation >> 4;
        }

        /// <summary>
        /// return the Orientation given to distant Tiles
        /// Orientation is combined in case of diagonals
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public int DistantOrientation(Tile from, Tile to)
        {
            float a = Mathf.Rad2Deg((to.Position - from.Position).Angle());
            if (a < 0)
            {
                a += 360;
            }
            a = (int)(a * 10) / 10.0f;

            foreach (int k in _angles.Keys)
            {
                int z = _angles[k];
                if (a >= (z + 30 - _DegreeAdj) && a <= (z + 30 + _DegreeAdj))
                {
                    // diagonal
                    int p = k >> 1;
                    if (p == 0)
                    {
                        p = (int)Orientation.SE;
                    }
                    if (!_angles.ContainsKey(p))
                    {
                        return k | (p >> 1); // v : N S and not v : W E;
                    }

                    return k | p;
                }
                else if (z == 30 && (a < _DegreeAdj || a > 360 - _DegreeAdj))
                {
                    return (int)Orientation.NE | (int)Orientation.SE;
                }
                else if (a >= (z - 30) && a <= (z + 30))
                {
                    return k;
                }
            }
            return _angles.ContainsKey((int)Orientation.E) && a > 330 && a <= 360
                ? (int)Orientation.E
                : -1;
        }

        /// <summary>
        /// return the opposite of a possibly combined given Orientation
        /// </summary>
        /// <param name="orientation"></param>
        public int DistantOpposite(int orientation)
        {
            int r = 0;
            foreach (int k in _angles.Keys)
            {
                if ((k & orientation) == k)
                {
                    r |= Opposite(k);
                }
            }
            return r;
        }

        /// <summary>
        /// return the key of a given col;row coordinate
        /// </summary>
        /// <param name="coordinates">Grid coordinates for the hex</param>
        public int Key(Vector2 coordinates)
        {
            if (!IsOnMap(coordinates))
            {
                return -1;
            }
            return HasVerticalEdge
                ? Key((int)coordinates.x, (int)coordinates.y)
                : Key((int)coordinates.y, (int)coordinates.x);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private int Key(int x, int y)
        {
            int n = y / 2;
            int i = x - n + (n * _tl);
            if ((y % 2) != 0)
            {
                i += (int)_columnRow.x - 1;
            }
            return i;
        }

        /// <summary>
        /// build the 6 adjacent Tiles of a Tile given by it's col;row coordinates
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="tiles"></param>
        public void AdjacentsOf(Tile tile, List<Tile> tiles)
        {
            tiles.Clear();
            tiles.AddRange(BuildAdjacents(tile.Coordinates));
        }

        /// <summary>
        /// return true if the Tile is on the map
        /// </summary>
        /// <param name="coordinates">Grid coordinates for the hex</param>
        public bool IsOnMap(Vector2 coordinates)
        {
            return HasVerticalEdge
                ? IsOnMap((int)coordinates.x, (int)coordinates.y)
                : IsOnMap((int)coordinates.y, (int)coordinates.x);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private bool IsOnMap(int x, int y)
        {
            if ((y < 0) || (y >= (int)_columnRow.y))
            {
                return false;
            }
            return x >= ((y + 1) / 2) && x < ((int)_columnRow.x + (y / 2));
        }

        /// <summary>
        /// compute the center of a Tile given by it's col;row coordinates
        /// </summary>
        /// <param name="coordinates">Grid coordinates for the hex</param>
        public Vector2 CenterOf(Vector2 coordinates)
        {
            return HasVerticalEdge
                ? new Vector2(
                    _bottomCorner.x + _halfWidth + (coordinates.x * _width) - (coordinates.y * _halfWidth),
                    _bottomCorner.y + _halfHeight + (coordinates.y * _height))
                : new Vector2(
                    _bottomCorner.y + _halfHeight + (coordinates.x * _height),
                    _bottomCorner.x + _halfWidth + (coordinates.y * _width) - (coordinates.x * _halfWidth));
        }

        /// <summary>
        /// compute the col;row coordinates of a Tile given it's real coordinates
        /// </summary>
        /// <param name="r"></param>
        public Vector2 ToMap(Vector2 r) // TODO - figure out what this is and rename it
        {
            return HasVerticalEdge
                ? ToMap(r.x, r.y, false)
                : ToMap(r.y, r.x, true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="swap"></param>
        private Vector2 ToMap(float x, float y, bool swap)
        {
            // compute row
            float dy = y - _bottomCorner.y;
            int row = (int)(dy / _height);
            if (dy < 0)
            {
                row--;
            }
            // compute col
            float dx = x - _bottomCorner.x + (row * _halfWidth);
            int col = (int)(dx / _width);
            if (dx < 0)
            {
                col--;
            }
            // upper rectangle or hex body
            if (dy > ((row * _height) + _side))
            {
                dy -= (row * _height) + _side;
                dx -= col * _width;
                // upper left or right rectangle
                if (dx < _halfWidth)
                {
                    if (dy > (dx * _m))
                    {
                        // upper left hex
                        row++;
                    }
                }
                else if (dy > ((_width - dx) * _m))
                {
                    // upper right hex
                    row++;
                    col++;
                }
            }
            return swap
                ? new Vector2(row, col)
                : new Vector2(col, row);
        }

        /// <summary>
        /// compute the distance between 2 Tiles given by their col;row coordinates
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="euclidean"></param>
        public float Distance(Vector2 p0, Vector2 p1, bool euclidean = true)
        {
            int dx = (int)(p1.x - p0.x);
            int dy = (int)(p1.y - p0.y);
            if (euclidean)
            {
                if (dx == 0)
                {
                    return Mathf.Abs(dy);
                }
                else if (dy == 0 || dx == dy)
                {
                    return Mathf.Abs(dx);
                }
                float fdx = dx - (dy / 2);
                float fdy = dy * 0.86602f;
                return Mathf.Sqrt((fdx * fdx) + (fdy * fdy));
            }

            dx = Mathf.Abs(dx);
            dy = Mathf.Abs(dy);
            float dz = Mathf.Abs(p1.x - p0.x - p1.y + p0.y);
            if (dx > dy)
            {
                if (dx > dz)
                {
                    return dx;
                }
            }
            else if (dy > dz)
            {
                return dy;
            }

            return dz;
        }

        /// <summary>
        /// http://zvold.blogspot.com/2010/01/bresenhams-line-drawing-algorithm-on_26.html
        /// http://zvold.blogspot.com/2010/02/line-of-sight-on-hexagonal-grid.html
        /// compute as an List, the line of sight between 2 Tiles given by their col;row coordinates
        /// return the point after which the line of sight is blocked
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="tiles"></param>
        public Vector2 LineOfSight(Vector2 p0, Vector2 p1, List<Tile> tiles)
        {
            tiles.Clear();
            // orthogonal projection
            float ox0 = p0.x - ((p0.y + 1) / 2);
            float ox1 = p1.x - ((p1.y + 1) / 2);
            int dy = (int)p1.y - (int)p0.y;
            float dx = ox1 - ox0;
            // quadrant I && III
            bool q13 = (dx >= 0 && dy >= 0) || (dx < 0 && dy < 0);
            // is positive
            int xs = 1;
            int ys = 1;
            if (dx < 0)
            {
                xs = -1;
            }
            if (dy < 0)
            {
                ys = -1;
            }
            // dx counts half width
            dy = Mathf.Abs(dy);
            dx = Mathf.Abs(2 * dx);
            int dx3 = (int)(3 * dx);
            int dy3 = 3 * dy;
            // check for diagonals
            if (dx == 0 || dx == dy3)
            {
                return DiagonalLineOfSight(p0, p1, dx == 0, q13, tiles);
            }
            // angle is less than 45°
            bool flat = dx > dy3;
            int x = (int)p0.x;
            int y = (int)p0.y;
            int e = (int)(-2 * dx);
            Tile from = GetTile(p0);
            Tile to = GetTile(p1);
            float d = Distance(p0, p1);
            tiles.Add(from);
            from.IsBlocked = false;
            Vector2 ret = new Vector2(-1, -1);
            bool contact = false;
            bool los_blocked = false;
            while ((x != p1.x) || (y != p1.y))
            {
                if (e > 0)
                {
                    // quadrant I : up left
                    e -= dy3 + dx3;
                    y += ys;
                    if (!q13)
                    {
                        x -= xs;
                    }
                }
                else
                {
                    e += dy3;
                    if ((e > -dx) || (!flat && (e == -dx)))
                    {
                        // quadrant I : up right
                        e -= dx3;
                        y += ys;
                        if (q13)
                        {
                            x += xs;
                        }
                    }
                    else if (e < -dx3)
                    {
                        // quadrant I : down right
                        e += dx3;
                        y -= ys;
                        if (!q13)
                        {
                            x += xs;
                        }
                    }
                    else
                    {
                        // quadrant I : right
                        e += dy3;
                        x += xs;
                    }
                }
                Vector2 q = new Vector2(x, y);
                Tile t = GetTile(q);
                if (los_blocked && !contact)
                {
                    Tile prev = tiles[tiles.Count - 1];
                    int o = ToOrientation(Angle(prev, t));
                    ret = ComputeContact(from.Position, to.Position, prev.Position, o);
                    contact = true;
                }
                tiles.Add(t);
                t.IsBlocked = los_blocked;
                los_blocked = los_blocked || t.IsLosBlocked(from, to, d, Distance(p0, q));
            }
            return ret;
        }

        /// <summary>
        /// compute as an List, the Tiles that can be reached by a given Piece from a Tile given by it's col;row coordinates
        /// return the size of the built List
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="from"></param>
        /// <param name="tiles"></param>
        public int PossibleMoves(Piece piece, Tile from, List<Tile> tiles)
        {
            tiles.Clear();
            if (piece.GetMp() <= 0 || !IsOnMap(from.Coordinates))
            {
                return 0;
            }
            int road_march_bonus = piece.RoadMarchBonus();
            _searchCount++;
            from.Parent = null;
            from.Acc = piece.GetMp();
            from.SearchCount = _searchCount;
            from.HasRoadMarchBonus = road_march_bonus > 0;
            stack.Add(from);
            while (stack.Count > 0)
            {
                Tile src = stack.Last();
                stack.RemoveAt(stack.Count - 1);
                if ((src.Acc + (src.HasRoadMarchBonus ? road_march_bonus : 0)) <= 0)
                {
                    continue;
                }
                BuildAdjacents(src.Coordinates);
                foreach (Tile dst in _adjacents)
                {
                    if (!dst.IsOnMap)
                    {
                        continue;
                    }
                    int o = ToOrientation(Angle(src, dst));
                    int cost = piece.MoveCost(src, dst, o);
                    if (cost == -1)
                    {
                        continue;
                    } // impracticable
                    int r = src.Acc - cost;
                    bool rm = src.HasRoadMarchBonus && src.HasRoad(o);
                    // not enough MP even with RM, maybe first move allowed
                    if ((r + (rm ? road_march_bonus : 0)) < 0 && !(src == from && piece.AtLeastOneTile(dst)))
                    {
                        continue;
                    }
                    if (dst.SearchCount != _searchCount)
                    {
                        dst.SearchCount = _searchCount;
                        dst.Acc = r;
                        dst.Parent = src;
                        dst.HasRoadMarchBonus = rm;
                        stack.Add(dst);
                        tiles.Add(dst);
                    }
                    else if (r > dst.Acc || (rm && (r + road_march_bonus > dst.Acc + (dst.HasRoadMarchBonus
                        ? road_march_bonus
                        : 0))))
                    {
                        dst.Acc = r;
                        dst.Parent = src;
                        dst.HasRoadMarchBonus = rm;
                        stack.Add(dst);
                    }
                }
            }
            return tiles.Count;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="tiles"></param>
        public int ShortestPath(Piece piece, Tile from, Tile to, List<Tile> tiles)
        {
            tiles.Clear();
            if (from == to || !IsOnMap(from.Coordinates) || !IsOnMap(to.Coordinates))
            {
                return tiles.Count;
            }
            int road_march_bonus = piece.RoadMarchBonus();
            _searchCount++;
            from.Acc = 0;
            from.Parent = null;
            from.SearchCount = _searchCount;
            from.HasRoadMarchBonus = road_march_bonus > 0;
            stack.Add(from);
            while (stack.Count > 0)
            {
                Tile src = stack.Last();
                stack.RemoveAt(stack.Count - 1);
                if (src == to)
                {
                    break;
                }
                BuildAdjacents(src.Coordinates);
                foreach (Tile dst in _adjacents)
                {
                    if (!dst.IsOnMap)
                    {
                        continue;
                    }
                    int o = ToOrientation(Angle(src, dst));
                    int cost = piece.MoveCost(src, dst, o);
                    if (cost == -1)
                    {
                        continue; // impracticable
                    }
                    cost += src.Acc;
                    float total = cost + Distance(dst.Coordinates, to.Coordinates);
                    bool rm = src.HasRoadMarchBonus && src.HasRoad(o);
                    if (rm)
                    {
                        total -= road_march_bonus;
                    }
                    bool add = false;
                    if (dst.SearchCount != _searchCount)
                    {
                        dst.SearchCount = _searchCount;
                        add = true;
                    }
                    else if (dst.F > total || (rm && !dst.HasRoadMarchBonus && Mathf.Abs(dst.F - total) < 0.001))
                    {
                        stack.Remove(dst);
                        add = true;
                    }
                    if (add)
                    {
                        dst.Acc = cost;
                        dst.F = total;
                        dst.HasRoadMarchBonus = rm;
                        dst.Parent = src;
                        int idx = int.MaxValue;
                        for (int k = 0; k < stack.Count; k++)
                        {
                            if (stack[k].F <= dst.F)
                            {
                                idx = k;
                                break;
                            }
                        }
                        if (idx == int.MaxValue)
                        {
                            stack.Add(dst);
                        }
                        else
                        {
                            stack.Insert(idx, dst);
                        }
                    }
                }
            }
            stack.Clear();
            if (to.SearchCount == _searchCount)
            {
                Tile t = to;
                while (t != from)
                {
                    tiles.Add(t);
                    t = t.Parent;
                }
                tiles.Add(from);
            }
            return tiles.Count;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="from"></param>
        /// <param name="category"></param>
        /// <param name="tiles"></param>
        public int RangeOfInfluence(Piece piece, Tile from, int category, List<Tile> tiles)
        {
            tiles.Clear();
            int max_range = piece.MaxRangeOfFire(category, from);
            if (!IsOnMap(from.Coordinates))
            {
                return 0;
            }
            List<Tile> tmp = new List<Tile>();
            _searchCount++;
            from.SearchCount = _searchCount;
            stack.Add(from);
            while (stack.Count > 0)
            {
                Tile src = stack.Last();
                stack.RemoveAt(stack.Count - 1);
                BuildAdjacents(src.Coordinates);
                foreach (Tile dst in _adjacents)
                {
                    if (!dst.IsOnMap)
                    {
                        continue;
                    }
                    if (dst.SearchCount == _searchCount)
                    {
                        continue;
                    }
                    dst.SearchCount = _searchCount;
                    int d = (int)Distance(from.Coordinates, dst.Coordinates, false);
                    if (d > max_range)
                    {
                        continue;
                    }
                    if (LineOfSight(from.Coordinates, dst.Coordinates, tmp).x != -1)
                    {
                        continue;
                    }
                    int o = DistantOrientation(from, dst);
                    dst.F = piece.VolumeOfFire(category, d, from, o, dst, DistantOpposite(o));
                    stack.Add(dst);
                    tiles.Add(dst);
                }
            }
            return tiles.Count;
        }

        /// <summary>
        /// build the 6 adjacent Tiles of a Tile given by it's col;row coordinates
        /// </summary>
        /// <param name="coordinates">Grid coordinates for the hex</param>
        private List<Tile> BuildAdjacents(Vector2 coordinates)
        {
            _adjacents.Clear();
            coordinates.x++;
            _adjacents.Add(GetTile(coordinates));
            coordinates.y++;
            _adjacents.Add(GetTile(coordinates));
            coordinates.x--;
            _adjacents.Add(GetTile(coordinates));
            coordinates.x--;
            coordinates.y--;
            _adjacents.Add(GetTile(coordinates));
            coordinates.y--;
            _adjacents.Add(GetTile(coordinates));
            coordinates.x++;
            _adjacents.Add(GetTile(coordinates));
            return _adjacents;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="flat"></param>
        /// <param name="q13"></param>
        /// <param name="tiles"></param>
        private Vector2 DiagonalLineOfSight(Vector2 p0, Vector2 p1, bool flat, bool q13, List<Tile> tiles)
        {
            int dy = (p1.y > p0.y) ? 1 : -1;
            int dx = (p1.x > p0.x) ? 1 : -1;
            int x = (int)p0.x;
            int y = (int)p0.y;
            Tile from = GetTile(p0);
            Tile to = GetTile(p1);
            float d = Distance(p0, p1);
            tiles.Add(from);
            from.IsBlocked = false;
            Vector2 ret = new Vector2(-1, -1);
            int blocked = 0;
            bool contact = false;
            bool los_blocked = false;
            while ((x != p1.x) || (y != p1.y))
            {
                int idx = 4;
                if (flat)
                {
                    y += dy; // up left
                }
                else
                {
                    x += dx; // right
                }
                Vector2 q = new Vector2(x, y);
                Tile t = GetTile(q);
                if (t.IsOnMap)
                {
                    tiles.Add(t);
                    t.IsBlocked = los_blocked;
                    if (t.IsLosBlocked(from, to, d, Distance(p0, q)))
                    {
                        blocked |= 0x01;
                    }
                }
                else
                {
                    blocked |= 0x01;
                    idx = 3;
                }

                if (flat)
                {
                    x += dx; // up right
                }
                else
                {
                    y += dy;  // up right
                    if (!q13) { x -= dx; }
                }
                q = new Vector2(x, y);
                t = GetTile(q);
                if (t.IsOnMap)
                {
                    tiles.Add(t);
                    t.IsBlocked = los_blocked;
                    if (t.IsLosBlocked(from, to, d, Distance(p0, q))) { blocked |= 0x02; }
                }
                else
                {
                    blocked |= 0x02;
                    idx = 3;
                }

                if (flat)
                {
                    y += dy;  // up 
                }
                else
                {
                    x += dx;  // diagonal
                }
                q = new Vector2(x, y);
                t = GetTile(q);
                tiles.Add(t);
                t.IsBlocked = los_blocked || blocked == 0x03;
                if (t.IsBlocked && !contact)
                {
                    int o = ComputeOrientation(dx, dy, flat);
                    ret = !los_blocked && blocked == 0x03
                        ? ComputeContact(from.Position, to.Position, t.Position, Opposite(o))
                        : ComputeContact(from.Position, to.Position, tiles[tiles.Count - idx].Position, o);
                    contact = true;
                }
                los_blocked = t.IsBlocked || t.IsLosBlocked(from, to, d, Distance(p0, q));
            }
            return ret;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="flat"></param>
        private int ComputeOrientation(int dx, int dy, bool flat)
        {
            if (flat)
            {
                if (HasVerticalEdge)
                {
                    return dy == 1 ? (int)Orientation.S : (int)Orientation.N;
                }

                return dx == 1 ? (int)Orientation.S : (int)Orientation.N;
            }
            if (dx == 1)
            {
                if (dy == 1)
                {
                    return (int)Orientation.E;
                }

                return HasVerticalEdge ? (int)Orientation.E : (int)Orientation.N;
            }
            if (dy == 1)
            {
                return HasVerticalEdge ? (int)Orientation.W : (int)Orientation.S;
            }

            return (int)Orientation.W;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="t"></param>
        /// <param name="orientation"></param>
        private Vector2 ComputeContact(Vector2 from, Vector2 to, Vector2 t, int orientation)
        {
            float dx = to.x - from.x;
            float dy = to.y - from.y;
            float n = (dx == 0) ? int.MaxValue : (dy / dx);
            float c = from.y - (n * from.x);
            if (HasVerticalEdge)
            {
                switch (orientation)
                {
                    case (int)Orientation.N:
                        return new Vector2(t.x, t.y - _side);
                    case (int)Orientation.S:
                        return new Vector2(t.x, t.y + _side);
                    case (int)Orientation.E:
                        return new Vector2(t.x + _halfWidth, from.y + (n * (t.x + _halfWidth - from.x)));
                    case (int)Orientation.W:
                        return new Vector2(t.x - _halfWidth, from.y + (n * (t.x - _halfWidth - from.x)));
                    default:
                        float p = (orientation == (int)Orientation.SE || orientation == (int)Orientation.NW) ? -_m : _m;
                        float k = t.y - (p * t.x);
                        if (orientation == (int)Orientation.SE || orientation == (int)Orientation.SW)
                        {
                            k += _side;
                        }
                        else
                        {
                            k -= _side;
                        }
                        float x = (k - c) / (n - p);
                        return new Vector2(x, (n * x) + c);
                }
            }
            else
            {
                switch (orientation)
                {
                    case (int)Orientation.E:
                        return new Vector2(t.x + _side, t.y);
                    case (int)Orientation.W:
                        return new Vector2(t.x - _side, t.y);
                    case (int)Orientation.N:
                        return new Vector2(from.x + ((t.y - _halfWidth - from.y) / n), t.y - _halfWidth);
                    case (int)Orientation.S:
                        return new Vector2(from.x + ((t.y + _halfWidth - from.y) / n), t.y + _halfWidth);
                    default:
                        float p = (orientation == (int)Orientation.SE || orientation == (int)Orientation.NW) ? -_im : +_im;
                        float k = orientation == (int)Orientation.SW || orientation == (int)Orientation.NW
                            ? t.y - (p * (t.x - _side))
                            : t.y - (p * (t.x + _side));
                        float x = (k - c) / (n - p);
                        return new Vector2(x, (n * x) + c);
                }
            }
        }
    }
}

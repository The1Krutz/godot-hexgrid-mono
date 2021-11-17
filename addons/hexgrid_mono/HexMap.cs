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
    /// <param name="k"></param>
    public delegate Tile GetTile(Vector2 coords, int k);

    /// <summary>
    ///
    /// </summary>
    public class HexBoard : Node
    {
        public const int IMAX = int.MaxValue;
        public const int DEGREE_ADJ = 2;

        public Vector2 bt; // bottom corner
        public Vector2 cr; // column, row

        public bool v; // hex have a vertical edje

        public float s; // hex side length
        public float w; // hex width between 2 parallel sides
        public float h; // hex height from the bottom of the middle rectangle to the top of the upper edje
        public float dw; // half width
        public float dh; // half height (from the top ef tho middle rectangle to the top of the upper edje)
        public float m; // dh / dw
        public float im; // dw / dh
        public int tl; // num of hexes in 2 consecutives rows

        public GetTile tile_factory_fct;
        public Dictionary<int, int> angles = new Dictionary<int, int>();
        public List<Tile> adjacents = new List<Tile>();
        public int search_count;
        public List<Tile> stack = new List<Tile>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="rows"></param>
        /// <param name="side"></param>
        /// <param name="v0"></param>
        /// <param name="vertical"></param>
        /// <param name="fct"></param>
        public HexBoard(int cols, int rows, float side, Vector2 v0, bool vertical, GetTile fct)
        {
            tile_factory_fct = fct;
            v = vertical;
            s = side;
            w = s * 1.73205f;
            dw = w / 2.0f;
            dh = s / 2.0f;
            h = s + dh;
            m = dh / dw;
            im = dw / dh;
            if (v)
            {
                bt = v0;
                cr = new Vector2(cols, rows);
            }
            else
            {
                bt = v0;
                cr = new Vector2(rows, cols);
            }
            tl = (2 * (int)cr.x) - 1;
            search_count = 0;
            angles.Clear();
            if (v)
            {
                // origin [top-left] East is at 0°, degrees grows clockwise
                angles[(int)Orientation.E] = 0;
                angles[(int)Orientation.SE] = 60;
                angles[(int)Orientation.SW] = 120;
                angles[(int)Orientation.W] = 180;
                angles[(int)Orientation.NW] = 240;
                angles[(int)Orientation.NE] = 300;
            }
            else
            {
                angles[(int)Orientation.SE] = 30;
                angles[(int)Orientation.S] = 90;
                angles[(int)Orientation.SW] = 150;
                angles[(int)Orientation.NW] = 210;
                angles[(int)Orientation.N] = 270;
                angles[(int)Orientation.NE] = 330;
            }
        }

        /// <summary>
        /// the number of Tile
        /// </summary>
        public int Size()
        {
            return ((int)cr.y / 2 * tl) + ((int)cr.y % 2 * (int)cr.x);
        }

        /// <summary>
        /// fetch a Tile given it's col;row coordinates
        /// </summary>
        /// <param name="coords"></param>
        public Tile GetTile(Vector2 coords)
        {
            return tile_factory_fct(coords, Key(coords));
        }

        /// <summary>
        /// Orientation to degrees
        /// </summary>
        /// <param name="o"></param>
        public int ToDegrees(int o)
        {
            return angles.TryGetValue(o, out int temp) ? temp : -1;
        }

        /// <summary>
        /// convert the given angle between 2 adjacent Tiles into an Orientation
        /// </summary>
        /// <param name="a"></param>
        public int ToOrientation(float a)
        {
            foreach (int k in angles.Keys)
            {
                if (angles[k] == a)
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
            float a = Mathf.Rad2Deg((to.Position - from.Position).Angle()) + DEGREE_ADJ;
            if (a < 0)
            {
                a += 360;
            }
            return (int)(a / 10) * 10;
        }

        /// <summary>
        /// return the opposite of a given Orientation
        /// </summary>
        /// <param name="o"></param>
        public int Opposite(int o)
        {
            return o <= (int)Orientation.NW
                ? o << 4
                : o >> 4;
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

            foreach (int k in angles.Keys)
            {
                int z = angles[k];
                if (a >= (z + 30 - DEGREE_ADJ) && a <= (z + 30 + DEGREE_ADJ))
                {
                    // diagonal
                    int p = k >> 1;
                    if (p == 0)
                    {
                        p = (int)Orientation.SE;
                    }
                    if (!angles.ContainsKey(p))
                    {
                        return k | (p >> 1); // v : N S and not v : W E;
                    }

                    return k | p;
                }
                else if (z == 30 && (a < DEGREE_ADJ || a > 360 - DEGREE_ADJ))
                {
                    return (int)Orientation.NE | (int)Orientation.SE;
                }
                else if (a >= (z - 30) && a <= (z + 30))
                {
                    return k;
                }
            }
            return angles.ContainsKey((int)Orientation.E) && a > 330 && a <= 360
                ? (int)Orientation.E
                : -1;
        }

        /// <summary>
        /// return the opposite of a possibly combined given Orientation
        /// </summary>
        /// <param name="o"></param>
        public int DistantOpposite(int o)
        {
            int r = 0;
            foreach (int k in angles.Keys)
            {
                if ((k & o) == k)
                {
                    r |= Opposite(k);
                }
            }
            return r;
        }

        /// <summary>
        /// return the key of a given col;row coordinate
        /// </summary>
        /// <param name="coords"></param>
        public int Key(Vector2 coords)
        {
            if (!IsOnMap(coords))
            {
                return -1;
            }
            return v
                ? Key((int)coords.x, (int)coords.y)
                : Key((int)coords.y, (int)coords.x);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private int Key(int x, int y)
        {
            int n = y / 2;
            int i = x - n + (n * tl);
            if ((y % 2) != 0)
            {
                i += (int)cr.x - 1;
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
            tiles.AddRange(BuildAdjacents(tile.coords));
        }

        /// <summary>
        /// build the 6 adjacent Tiles of a Tile given by it's col;row coordinates
        /// </summary>
        /// <param name="coords"></param>
        private List<Tile> BuildAdjacents(Vector2 coords)
        {
            adjacents.Clear();
            coords.x++;
            adjacents.Add(GetTile(coords));
            coords.y++;
            adjacents.Add(GetTile(coords));
            coords.x--;
            adjacents.Add(GetTile(coords));
            coords.x--;
            coords.y--;
            adjacents.Add(GetTile(coords));
            coords.y--;
            adjacents.Add(GetTile(coords));
            coords.x++;
            adjacents.Add(GetTile(coords));
            return adjacents;
        }

        /// <summary>
        /// return true if the Tile is on the map
        /// </summary>
        /// <param name="coords"></param>
        public bool IsOnMap(Vector2 coords)
        {
            return v
                ? IsOnMap((int)coords.x, (int)coords.y)
                : IsOnMap((int)coords.y, (int)coords.x);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private bool IsOnMap(int x, int y)
        {
            if ((y < 0) || (y >= (int)cr.y))
            {
                return false;
            }
            return x >= ((y + 1) / 2) && x < ((int)cr.x + (y / 2));
        }

        /// <summary>
        /// compute the center of a Tile given by it's col;row coordinates
        /// </summary>
        /// <param name="coords"></param>
        public Vector2 CenterOf(Vector2 coords)
        {
            return v
                ? new Vector2(bt.x + dw + (coords.x * w) - (coords.y * dw), bt.y + dh + (coords.y * h))
                : new Vector2(bt.y + dh + (coords.x * h), bt.x + dw + (coords.y * w) - (coords.x * dw));
        }

        /// <summary>
        /// compute the col;row coordinates of a Tile given it's real coordinates
        /// </summary>
        /// <param name="r"></param>
        public Vector2 ToMap(Vector2 r)
        {
            return v
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
            float dy = y - bt.y;
            int row = (int)(dy / h);
            if (dy < 0)
            {
                row--;
            }
            // compute col
            float dx = x - bt.x + (row * dw);
            int col = (int)(dx / w);
            if (dx < 0)
            {
                col--;
            }
            // upper rectangle or hex body
            if (dy > ((row * h) + s))
            {
                dy -= (row * h) + s;
                dx -= col * w;
                // upper left or right rectangle
                if (dx < dw)
                {
                    if (dy > (dx * m))
                    {
                        // upper left hex
                        row++;
                    }
                }
                else if (dy > ((w - dx) * m))
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
            else
              if (dy > dz)
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
                return DiagonalLoS(p0, p1, dx == 0, q13, tiles);
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
            from.blocked = false;
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
                    Tile prev = tiles[^1];
                    int o = ToOrientation(Angle(prev, t));
                    ret = ComputeContact(from.Position, to.Position, prev.Position, o);
                    contact = true;
                }
                tiles.Add(t);
                t.blocked = los_blocked;
                los_blocked = los_blocked || t.BlockLos(from, to, d, Distance(p0, q));
            }
            return ret;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="flat"></param>
        /// <param name="q13"></param>
        /// <param name="tiles"></param>
        private Vector2 DiagonalLoS(Vector2 p0, Vector2 p1, bool flat, bool q13, List<Tile> tiles)
        {
            int dy = (p1.y > p0.y) ? 1 : -1;
            int dx = (p1.x > p0.x) ? 1 : -1;
            int x = (int)p0.x;
            int y = (int)p0.y;
            Tile from = GetTile(p0);
            Tile to = GetTile(p1);
            float d = Distance(p0, p1);
            tiles.Add(from);
            from.blocked = false;
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
                if (t.on_map)
                {
                    tiles.Add(t);
                    t.blocked = los_blocked;
                    if (t.BlockLos(from, to, d, Distance(p0, q)))
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
                if (t.on_map)
                {
                    tiles.Add(t);
                    t.blocked = los_blocked;
                    if (t.BlockLos(from, to, d, Distance(p0, q))) { blocked |= 0x02; }
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
                t.blocked = los_blocked || blocked == 0x03;
                if (t.blocked && !contact)
                {
                    int o = ComputeOrientation(dx, dy, flat);
                    ret = !los_blocked && blocked == 0x03
                        ? ComputeContact(from.Position, to.Position, t.Position, Opposite(o))
                        : ComputeContact(from.Position, to.Position, tiles[^idx].Position, o);
                    contact = true;
                }
                los_blocked = t.blocked || t.BlockLos(from, to, d, Distance(p0, q));
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
                if (v)
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

                return v ? (int)Orientation.E : (int)Orientation.N;
            }
            if (dy == 1)
            {
                return v ? (int)Orientation.W : (int)Orientation.S;
            }

            return (int)Orientation.W;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="t"></param>
        /// <param name="o"></param>
        private Vector2 ComputeContact(Vector2 from, Vector2 to, Vector2 t, int o)
        {
            float dx = to.x - from.x;
            float dy = to.y - from.y;
            float n = (dx == 0) ? IMAX : (dy / dx);
            float c = from.y - (n * from.x);
            if (v)
            {
                switch (o)
                {
                    case (int)Orientation.N:
                        return new Vector2(t.x, t.y - s);
                    case (int)Orientation.S:
                        return new Vector2(t.x, t.y + s);
                    case (int)Orientation.E:
                        return new Vector2(t.x + dw, from.y + (n * (t.x + dw - from.x)));
                    case (int)Orientation.W:
                        return new Vector2(t.x - dw, from.y + (n * (t.x - dw - from.x)));
                    default:
                        float p = (o == (int)Orientation.SE || o == (int)Orientation.NW) ? -m : m;
                        float k = t.y - (p * t.x);
                        if (o == (int)Orientation.SE || o == (int)Orientation.SW)
                        {
                            k += s;
                        }
                        else
                        {
                            k -= s;
                        }
                        float x = (k - c) / (n - p);
                        return new Vector2(x, (n * x) + c);
                }
            }
            else
            {
                switch (o)
                {
                    case (int)Orientation.E:
                        return new Vector2(t.x + s, t.y);
                    case (int)Orientation.W:
                        return new Vector2(t.x - s, t.y);
                    case (int)Orientation.N:
                        return new Vector2(from.x + ((t.y - dw - from.y) / n), t.y - dw);
                    case (int)Orientation.S:
                        return new Vector2(from.x + ((t.y + dw - from.y) / n), t.y + dw);
                    default:
                        float p = (o == (int)Orientation.SE || o == (int)Orientation.NW) ? -im : +im;
                        float k = o == (int)Orientation.SW || o == (int)Orientation.NW
                            ? t.y - (p * (t.x - s))
                            : t.y - (p * (t.x + s));
                        float x = (k - c) / (n - p);
                        return new Vector2(x, (n * x) + c);
                }
            }
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
            if (piece.GetMp() <= 0 || !IsOnMap(from.coords))
            {
                return 0;
            }
            int road_march_bonus = piece.RoadMarchBonus();
            search_count++;
            from.parent = null;
            from.acc = piece.GetMp();
            from.search_count = search_count;
            from.road_march = road_march_bonus > 0;
            stack.Add(from);
            while (stack.Count > 0)
            {
                Tile src = stack.Last();
                stack.RemoveAt(stack.Count - 1);
                if ((src.acc + (src.road_march ? road_march_bonus : 0)) <= 0)
                {
                    continue;
                }
                BuildAdjacents(src.coords);
                foreach (Tile dst in adjacents)
                {
                    if (!dst.on_map)
                    {
                        continue;
                    }
                    int o = ToOrientation(Angle(src, dst));
                    int cost = piece.MoveCost(src, dst, o);
                    if (cost == -1)
                    {
                        continue;
                    } // impracticable
                    int r = src.acc - cost;
                    bool rm = src.road_march && src.HasRoad(o);
                    // not enough MP even with RM, maybe first move allowed
                    if ((r + (rm ? road_march_bonus : 0)) < 0 && !(src == from && piece.AtLeastOneTile(dst)))
                    {
                        continue;
                    }
                    if (dst.search_count != search_count)
                    {
                        dst.search_count = search_count;
                        dst.acc = r;
                        dst.parent = src;
                        dst.road_march = rm;
                        stack.Add(dst);
                        tiles.Add(dst);
                    }
                    else if (r > dst.acc || (rm && (r + road_march_bonus > dst.acc + (dst.road_march ? road_march_bonus : 0))))
                    {
                        dst.acc = r;
                        dst.parent = src;
                        dst.road_march = rm;
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
            if (from == to || !IsOnMap(from.coords) || !IsOnMap(to.coords))
            {
                return tiles.Count;
            }
            int road_march_bonus = piece.RoadMarchBonus();
            search_count++;
            from.acc = 0;
            from.parent = null;
            from.search_count = search_count;
            from.road_march = road_march_bonus > 0;
            stack.Add(from);
            while (stack.Count > 0)
            {
                Tile src = stack.Last();
                stack.RemoveAt(stack.Count - 1);
                if (src == to)
                {
                    break;
                }
                BuildAdjacents(src.coords);
                foreach (Tile dst in adjacents)
                {
                    if (!dst.on_map)
                    {
                        continue;
                    }
                    int o = ToOrientation(Angle(src, dst));
                    int cost = piece.MoveCost(src, dst, o);
                    if (cost == -1)
                    {
                        continue; // impracticable
                    }
                    cost += src.acc;
                    float total = cost + Distance(dst.coords, to.coords);
                    bool rm = src.road_march && src.HasRoad(o);
                    if (rm)
                    {
                        total -= road_march_bonus;
                    }
                    bool add = false;
                    if (dst.search_count != search_count)
                    {
                        dst.search_count = search_count;
                        add = true;
                    }
                    else if (dst.f > total || (rm && !dst.road_march && Mathf.Abs(dst.f - total) < 0.001))
                    {
                        stack.Remove(dst);
                        add = true;
                    }
                    if (add)
                    {
                        dst.acc = cost;
                        dst.f = total;
                        dst.road_march = rm;
                        dst.parent = src;
                        int idx = IMAX;
                        for (int k = 0; k < stack.Count; k++)
                        {
                            if (stack[k].f <= dst.f)
                            {
                                idx = k;
                                break;
                            }
                        }
                        if (idx == IMAX)
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
            if (to.search_count == search_count)
            {
                Tile t = to;
                while (t != from)
                {
                    tiles.Add(t);
                    t = t.parent;
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
            if (!IsOnMap(from.coords))
            {
                return 0;
            }
            List<Tile> tmp = new List<Tile>();
            search_count++;
            from.search_count = search_count;
            stack.Add(from);
            while (stack.Count > 0)
            {
                Tile src = stack.Last();
                stack.RemoveAt(stack.Count - 1);
                BuildAdjacents(src.coords);
                foreach (Tile dst in adjacents)
                {
                    if (!dst.on_map)
                    {
                        continue;
                    }
                    if (dst.search_count == search_count)
                    {
                        continue;
                    }
                    dst.search_count = search_count;
                    int d = (int)Distance(from.coords, dst.coords, false);
                    if (d > max_range)
                    {
                        continue;
                    }
                    if (LineOfSight(from.coords, dst.coords, tmp).x != -1)
                    {
                        continue;
                    }
                    int o = DistantOrientation(from, dst);
                    dst.f = piece.VolumeOfFire(category, d, from, o, dst, DistantOpposite(o));
                    stack.Add(dst);
                    tiles.Add(dst);
                }
            }
            return tiles.Count;
        }
    }
}

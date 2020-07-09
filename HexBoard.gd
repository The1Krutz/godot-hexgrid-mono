#warning-ignore-all:integer_division
extends Node

class_name HexBoard, "res://godot/HexBoard.png"

enum Orientation { E=1, NE=2, N=4, NW=8, W=16, SW=32, S=64, SE=128 }

var bt : Vector2	# bottom corner
var cr : Vector2	# column, row

var v : bool		# hex have a vertical edje

var s : float		# hex side length
var w : float		# hex width between 2 parallel sides
var h : float		# hex height from the bottom of the middle rectangle to the top of the upper edje
var dw : float		# half width
var dh : float		# half height (from the top ef tho middle rectangle to the top of the upper edje)
var m : float		# dh / dw
var im : float		# dw / dh
var tl : int		# num of hexes in 2 consecutives rows

var tile_factory_fct : FuncRef
var angles : Dictionary

func configure(cols : int, rows : int, side : float, v0 : Vector2, vertical : bool) -> void:
	v = vertical
	s = side
	w  = s * 1.73205
	dw = w / 2.0
	dh = s / 2.0
	h  = s + dh
	m = dh / dw
	im = dw / dh
	if v:
		bt = v0
		cr = Vector2(cols, rows)
	else:
		bt = v0
		cr = Vector2(rows, cols)
	tl = (2 * int(cr.x) - 1)
	angles = {}
	if v:
		angles[Orientation.E] = 0
		angles[Orientation.NE] = 60
		angles[Orientation.NW] = 120
		angles[Orientation.W] = 180
		angles[Orientation.SW] = 240
		angles[Orientation.SE] = 300
	else:
		angles[Orientation.NE] = 30
		angles[Orientation.N] = 90
		angles[Orientation.NW] = 150
		angles[Orientation.SW] = 210
		angles[Orientation.S] = 270
		angles[Orientation.SE] = 330

func size() -> int:
	return int(cr.y) / 2 * tl + int(cr.y) % 2 * int(cr.x)

func get_tile(coords : Vector2) -> Tile:
	return tile_factory_fct.call_func(coords, key(coords))

func to_angle(o : int) -> int:
	return angles.get(o, -1)

func to_orientation(a : float) -> int:
	for k in angles.keys():
		if angles[k] == a:
			return k
	return -1

func angle(from : Tile, to : Tile) -> int:
	var a : float = rad2deg((to.position - from.position).angle()) + 2
	if a < 0: a += 360
	return int(a / 10) * 10

func opposite(o : int) -> int:
	if o <= Orientation.NW: return o << 4
	return o >> 4

func key(coords : Vector2) -> int:
	if not is_on_map(coords): return -1
	if v: return _key(int(coords.x), int(coords.y))
	else: return _key(int(coords.y), int(coords.x))

func _key(x : int, y : int) -> int:
	var n : int = y / 2
	var i : int =  x - n + n * tl
	if (y % 2) != 0:
		i += (int(cr.x) - 1)
	return i

func is_on_map(coords : Vector2) -> bool:
	if v: return _is_on_map(int(coords.x), int(coords.y))
	else: return _is_on_map(int(coords.y), int(coords.x))

func _is_on_map(x : int, y : int) -> bool:
	if (y < 0) || (y >= int(cr.y)): return false
	if (x < ((y + 1) / 2)) || (x >= (int(cr.x) + (y / 2))): return false
	return true

func center_of(coords : Vector2) -> Vector2:
	if v: return Vector2(bt.x + dw + (coords.x * w) - (coords.y * dw), bt.y + dh + (coords.y * h))
	else: return Vector2(bt.y + dh + (coords.x * h), bt.x + dw + (coords.y * w) - (coords.x * dw))

func to_map(r : Vector2) -> Vector2:
	if v: return _to_map(r.x, r.y, false)
	else: return _to_map(r.y, r.x, true)

func _to_map(x : float, y : float, swap : bool) -> Vector2:
	var col : int = -1
	var row : int = -1
	# compute row
	var dy : float = y - bt.y
	row = int(dy / h)
	if dy < 0:
		row -= 1
	# compute col
	var dx : float = x - bt.x + (row * dw);
	col = int(dx / w)
	if dx < 0:
		col -= 1
	# upper rectangle or hex body
	if dy > ((row * h) + s):
		dy -= ((row * h) + s)
		dx -= (col * w)
		# upper left or right rectangle
		if dx < dw:
			if dy > (dx * m):
				# upper left hex
				row += 1
		else:
			if dy > ((w - dx) * m):
				# upper right hex
				row += 1
				col += 1
	if swap: return Vector2(row, col)
	else: return Vector2(col, row)

func distance(p0 : Vector2, p1 : Vector2, euclidean : bool = true) -> float:
	var dx : int = int(p1.x - p0.x)
	var dy : int = int(p1.y - p0.y)
	if euclidean:
		if dx == 0: return abs(dy)
		elif dy == 0 || dx == dy: return abs(dx)
		var fdx : float = dx - dy / 2;
		var fdy : float = dy * 0.86602
		return sqrt((fdx * fdx) + (fdy * fdy))
	else:
		var dz : float = abs((p0.x - p0.y) - (p1.x - p1.y))
		if dx > dy:
			if dx > dz : return abs(dx)
		else:
			if dy > dz: return abs(dy)
		return dz

# http://zvold.blogspot.com/2010/01/bresenhams-line-drawing-algorithm-on_26.html
# http://zvold.blogspot.com/2010/02/line-of-sight-on-hexagonal-grid.html
func line_of_sight(p0 : Vector2, p1 : Vector2, tiles : Array) -> Vector2:
	tiles.clear()
	# orthogonal projection
	var ox0 : float = p0.x - (p0.y + 1) / 2
	var ox1 : float = p1.x - (p1.y + 1) / 2
	var dy : int = int(p1.y) - int(p0.y)
	var dx : float = ox1 - ox0
	# quadrant I && III
	var q13 : bool = (dx >= 0 && dy >= 0) || (dx < 0 && dy < 0)
	# is positive
	var xs : int = 1
	var ys : int = 1
	if dx < 0: xs = -1
	if dy < 0: ys = -1
	# dx counts half width
	dy = int(abs(dy))
	dx = abs(2 * dx)
	var dx3 : int = int(3 * dx)
	var dy3 : int = 3 * dy
	# check for diagonals
	if dx == 0 || dx == dy3:
		return diagonal_los(p0, p1, (dx == 0), q13, tiles)
	# angle is less than 45°
	var flat : bool = dx > dy3
	var x : int = int(p0.x)
	var y : int = int(p0.y);
	var e : int = int(-2 * dx)
	var from : Tile = get_tile(p0)
	var to : Tile = get_tile(p1)
	var d : float = distance(p0, p1)
	tiles.append(from)
	from.blocked = false
	var ret : Vector2 = Vector2(-1, -1)
	var contact : bool = false
	var los_blocked : bool = false
	while (x != p1.x) or (y != p1.y):
		if e > 0:
			# quadrant I : up left
			e -= (dy3 + dx3)
			y += ys
			if not q13: x -= xs
		else:
			e += dy3
			if (e > -dx) or (not flat && (e == -dx)):
				# quadrant I : up right
				e -= dx3
				y += ys
				if q13: x += xs
			elif e < -dx3:
				# quadrant I : down right
				e += dx3
				y -= ys
				if not q13: x += xs
			else:
				# quadrant I : right
				e += dy3
				x += xs
		var q : Vector2 = Vector2(x, y)
		var t : Tile = get_tile(q)
		if los_blocked and not contact:
			var o : int = to_orientation(angle(tiles[tiles.size() - 1], t))
			ret = compute_contact(from.position, to.position, o, t.position, true)
			contact = true
		tiles.append(t)
		t.blocked = los_blocked
		los_blocked = los_blocked or t.block_los(from, to, d, distance(p0, q))
	return ret

func diagonal_los(p0 : Vector2, p1 : Vector2, flat : bool, q13 : bool, tiles : Array) -> Vector2:
	var dy : int = 1 if p1.y > p0.y else -1
	var dx : int = 1 if p1.x > p0.x else -1
	var x : int = int(p0.x)
	var y : int = int(p0.y)
	var from : Tile = get_tile(p0);
	var to : Tile = get_tile(p1);
	var d : float = distance(p0, p1)
	tiles.append(from);
	from.blocked = false;
	var ret : Vector2 = Vector2(-1, -1)
	var blocked : int = 0
	var contact : bool = false
	var los_blocked : bool = false
	while (x != p1.x) or (y != p1.y):
		var idx : int = 4
		if flat: y += dy	# up left
		else: x += dx		# right
		var q : Vector2 = Vector2(x, y)
		var t : Tile = get_tile(q)
		if t.on_board:
			tiles.append(t)
			t.blocked = los_blocked
			if t.block_los(from, to, d, distance(p0, q)):
				blocked |= 0x01
		else:
			blocked |= 0x01
			idx = 3

		if flat: x += dx	# up right
		else:
			y += dy		# up right
			if not q13: x -= dx
		q = Vector2(x, y)
		t = get_tile(q)
		if t.on_board:
			tiles.append(t)
			t.blocked = los_blocked
			if t.block_los(from, to, d, distance(p0, q)):
				blocked |= 0x02
		else:
			blocked |= 0x02
			idx = 3

		if flat: y += dy	# up
		else: x += dx 		# diagonal
		q = Vector2(x, y)
		t = get_tile(q)
		tiles.append(t)
		t.blocked = los_blocked || blocked == 0x03
		if t.blocked and not contact:
			var o : int = compute_orientation(dx, dy, flat)
			if not los_blocked and blocked == 0x03:
				ret = compute_contact(from.position, to.position, o, t.position, false)
			else:
				ret = compute_contact(from.position, to.position, opposite(o), tiles[tiles.size() - idx].position, false)
			contact = true;
		los_blocked = t.blocked || t.block_los(from, to, d, distance(p0, q))
	return ret

func compute_orientation(dx :int, dy :int, flat : bool) -> int:
	if flat:
		if v: return Orientation.N if dy == 1 else Orientation.S
		else: return Orientation.NE if dy == 1 else Orientation.SW
	if dx == 1:
		if dy == 1: return Orientation.NE if v else Orientation.E
		else: return Orientation.SE
	else:
		if dy == 1: return Orientation.NW
		else: return Orientation.SW if v else Orientation.W

func compute_contact(from : Vector2, to : Vector2, o : int, t : Vector2, line : bool) -> Vector2:
	var dx : float = to.x - from.x
	var dy : float = to.y - from.y
	var n : float = 9999999999.0 if dx == 0 else (dy / dx)
	var c : float = from.y - (n * from.x)
	if v:
		if o == Orientation.N: return Vector2(t.x, t.y - s)
		elif o == Orientation.S: return Vector2(t.x, t.y + s)
		elif o == Orientation.E:
			var x : float = t.x - dw
			return Vector2(x, from.y + n * (x - from.x))
		elif o == Orientation.W:
			var x : float = t.x + dw
			return Vector2(x, from.y + n * (x - from.x))
		else:
			if line:
				var p : float = m if (o == Orientation.SE or o == Orientation.NW) else -m
				var k : float = t.y - p * t.x
				if o == Orientation.SE || o == Orientation.SW: k += s
				else: k -= s
				var x : float = (k - c) / (n - p)
				return Vector2(x, n * x + c)
			else:
				var x : float = t.x + (-dw if (o == Orientation.NE or o == Orientation.SE) else dw)
				var y : float = t.y + (dh if (o == Orientation.SE or o == Orientation.SW) else -dh)
				return Vector2(x, y)
	else:
		if o == Orientation.E: return Vector2(t.x - s, t.y)
		elif o == Orientation.W: return Vector2(t.x + s, t.y)
		elif o == Orientation.N:
			var y : float = t.y - dw
			return Vector2(from.x + (y - from.y) / n, y)
		elif o == Orientation.S:
			var y : float = t.y + dw
			return Vector2(from.x + (y - from.y) / n, y)
		else:
			if line:
#				o = 1
				var p : float = im if (o == Orientation.SE or o == Orientation.NW) else -im
				var k : float = 0
				if o == Orientation.SW or o == Orientation.NW: k = t.y - (p * (t.x + s))
				else: k = t.y - (p * (t.x - s))
				var x : float = (k - c) / (n - p)
				return Vector2(x, n * x + c);
			else:
				var x : float = t.x + (dh if (o == Orientation.NW || o == Orientation.SW) else -dh)
				var y : float = t.y + (dw if (o == Orientation.SE || o == Orientation.SW) else -dw)
				return Vector2(x, y)

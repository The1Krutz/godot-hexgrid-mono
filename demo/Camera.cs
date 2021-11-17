using Godot;

namespace Demo
{
    /// <summary>
    ///
    /// </summary>
    public class Camera : Camera2D
    {
        public Vector2 margin;
        public Vector2 window;
        public Vector2 map_center;
        public Vector2 texture_size;
        public Vector2 zoom_boundaries;

        public override void _Ready()
        {
            margin = new Vector2(0, 0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="w"></param>
        /// <param name="c"></param>
        /// <param name="ts"></param>
        public void Configure(Vector2 w, Vector2 c, Vector2 ts)
        {
            window = w;
            map_center = c;
            texture_size = ts;
            float zout = Mathf.Max((texture_size.x + margin.x) / window.x, (texture_size.y + margin.y) / window.y);
            zoom_boundaries = new Vector2(zout - 0.5f, zout);
            UpdateCamera(0, 0, zoom_boundaries.y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void UpdateCamera(float x, float y, float z)
        {
            if (z != 0)
            {
                Zoom = new Vector2(Mathf.Clamp(Zoom.x + z, zoom_boundaries.x, zoom_boundaries.y), Zoom.x);
            }
            Vector2 posUpdate = Position + new Vector2(x, y);
            Vector2 delta = texture_size + margin - (window * Zoom.x);
            if (delta.x <= 0)
            {
                posUpdate.x = map_center.x;
            }
            else
            {
                int dx = (int)(delta.x / 2);
                posUpdate.x = Mathf.Clamp(Position.x, map_center.x - dx, map_center.x + dx);
            }
            if (delta.y <= 0)
            {
                posUpdate.y = map_center.y;
            }
            else
            {
                int dy = (int)(delta.y / 2);
                posUpdate.y = Mathf.Clamp(Position.y, map_center.y - dy, map_center.y + dy);
            }

            Position = posUpdate;
        }
    }
}

using Godot;

namespace Demo
{
    /// <summary>
    ///
    /// </summary>
    public class Camera : Camera2D
    {
        private Vector2 _margin;
        private Vector2 _window;
        private Vector2 _mapCenter;
        private Vector2 _textureSize;
        private Vector2 _zoomBoundaries;

        public override void _Ready()
        {
            _margin = new Vector2(0, 0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="window"></param>
        /// <param name="mapCenter"></param>
        /// <param name="textureSize"></param>
        public void Configure(Vector2 window, Vector2 mapCenter, Vector2 textureSize)
        {
            _window = window;
            _mapCenter = mapCenter;
            _textureSize = textureSize;
            float zout = Mathf.Max((_textureSize.x + _margin.x) / _window.x, (_textureSize.y + _margin.y) / _window.y);
            _zoomBoundaries = new Vector2(zout - 0.5f, zout);
            UpdateCamera(0, 0, _zoomBoundaries.y);
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
                Zoom = new Vector2(Mathf.Clamp(Zoom.x + z, _zoomBoundaries.x, _zoomBoundaries.y), Zoom.x);
            }
            Vector2 posUpdate = Position + new Vector2(x, y);
            Vector2 delta = _textureSize + _margin - (_window * Zoom.x);
            if (delta.x <= 0)
            {
                posUpdate.x = _mapCenter.x;
            }
            else
            {
                int dx = (int)(delta.x / 2);
                posUpdate.x = Mathf.Clamp(Position.x, _mapCenter.x - dx, _mapCenter.x + dx);
            }
            if (delta.y <= 0)
            {
                posUpdate.y = _mapCenter.y;
            }
            else
            {
                int dy = (int)(delta.y / 2);
                posUpdate.y = Mathf.Clamp(Position.y, _mapCenter.y - dy, _mapCenter.y + dy);
            }

            Position = posUpdate;
        }
    }
}

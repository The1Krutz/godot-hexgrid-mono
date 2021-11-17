using Godot;

namespace Demo
{
    public class Los : Node2D
    {
        private Vector2 _p0;
        private Vector2 _p1;
        private Vector2 _p2;

        public override void _Draw()
        {
            if (_p2.x == -1)
            {
                DrawLine(_p0, _p1, Color.Color8(0, 255, 0));
            }
            else
            {
                DrawLine(_p0, _p2, Color.Color8(0, 255, 0));
                DrawLine(_p2, _p1, Color.Color8(255, 0, 0));
            }
        }

        public void Setup(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            _p0 = v0;
            _p1 = v1;
            _p2 = v2;
            Update();
        }
    }
}

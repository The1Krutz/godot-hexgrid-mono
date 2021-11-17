using Godot;

namespace Demo {
  public class Los : Node2D {
    public Vector2 p0;
    public Vector2 p1;
    public Vector2 p2;

    public override void _Draw() {
      if (p2.x == -1) {
        DrawLine(p0, p1, Color.Color8(0, 255, 0));
      } else {
        DrawLine(p0, p2, Color.Color8(0, 255, 0));
        DrawLine(p2, p1, Color.Color8(255, 0, 0));
      }
    }

    public void Setup(Vector2 v0, Vector2 v1, Vector2 v2) {
      p0 = v0;
      p1 = v1;
      p2 = v2;
      Update();
    }
  }
}

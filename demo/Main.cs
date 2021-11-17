using Godot;
using Array = Godot.Collections.Array;

namespace Demo {
  public class Main : Node2D {
    public int moved;
    public bool drag_map;

    private Control _ui;
    private Map _map;
    private Camera _camera;
    private Viewport _viewport;
    private ViewportContainer _viewportcontainer;

    public override async void _Ready() {
      _ui = GetNode<Control>("CanvasLayer/HBOX/UI");
      _map = GetNode<Map>("CanvasLayer/HBOX/ViewportContainer/Viewport/Map");
      _camera = GetNode<Camera>("CanvasLayer/HBOX/ViewportContainer/Viewport/Camera");
      _viewport = GetNode<Viewport>("CanvasLayer/HBOX/ViewportContainer/Viewport");
      _viewportcontainer = GetNode<ViewportContainer>("CanvasLayer/HBOX/ViewportContainer");

      _ui.GetNode<Button>("rotate").Connect("pressed", this, nameof(OnRotate));
      _ui.GetNode<Button>("zin").Connect("pressed", this, nameof(OnZoom), new Array() { true });
      _ui.GetNode<Button>("zout").Connect("pressed", this, nameof(OnZoom), new Array() { false });
      _ui.GetNode<CheckBox>("LOS").Connect("pressed", this, nameof(OnToggle));
      _ui.GetNode<CheckBox>("Move").Connect("pressed", this, nameof(OnToggle));
      _ui.GetNode<CheckBox>("Influence").Connect("pressed", this, nameof(OnToggle));
      _map.Connect(nameof(Map.hex_touched), this, nameof(OnHexTouched));
      _viewportcontainer.Connect("resized", this, nameof(OnViewportResized));
      OnToggle();
      await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
      OnViewportResized();
      _ui.GetNode<Label>("OSInfo").Text = $"screen\n{OS.GetScreenSize()}\ndpi {OS.GetScreenDpi():F0}";
    }

    public void OnViewportResized() {
      _camera.Configure(_viewport.Size, _map.Center(), _map.TextureSize());
    }

    public void OnRotate() {
      _map.RotateMap();
      OnViewportResized();
    }

    public void OnZoom(bool b) {
      _camera.UpdateCamera(0, 0, b ? -0.05f : 0.05f);
    }

    public void OnToggle() {
      _map.SetMode(_ui.GetNode<CheckBox>("LOS").Pressed, _ui.GetNode<CheckBox>("Move").Pressed, _ui.GetNode<CheckBox>("Influence").Pressed);
    }

    public void OnHexTouched(Vector2 pos, Hex hex, int key) {
      string s = key == -1 ? "offmap" : hex.Inspect();
      _ui.GetNode<Label>("Info").Text = $"\n({pos.x:F1};{pos.y:F1})\n -> {s}\n -> {key}";
    }

    public override void _UnhandledInput(InputEvent @event) {
      if (@event is InputEventMouseMotion _mme) {
        if (drag_map) {
          Vector2 dv = _mme.Relative * _camera.Zoom;
          _camera.UpdateCamera(-dv.x, -dv.y, 0);
          moved++;
        } else {
          _map.OnMouseMove();
        }
      } else if (@event is InputEventMouseButton _mbe) {
        if (_mbe.ButtonIndex == 1) {
          if (moved < 5) {
            drag_map = _map.OnClick(_mbe.Pressed);
          } else {
            drag_map = false;
          }
          moved = 0;
        } else if (_mbe.ButtonIndex == 3) {
          drag_map = _mbe.Pressed;
        } else if (_mbe.ButtonIndex == 4) {
          OnZoom(true);
        } else if (_mbe.ButtonIndex == 5) {
          OnZoom(false);
        }
      } else if (@event is InputEventKey _ke) {
        if (_ke.Scancode == (int)KeyList.Escape) {
          GetTree().Quit();
        }
      }
    }
  }
}
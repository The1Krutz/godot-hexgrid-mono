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

      _ui.GetNode<Button>("rotate").Connect("pressed", this, nameof(on_rotate));
      _ui.GetNode<Button>("zin").Connect("pressed", this, nameof(on_zoom), new Array() { true });
      _ui.GetNode<Button>("zout").Connect("pressed", this, nameof(on_zoom), new Array() { false });
      _ui.GetNode<CheckBox>("LOS").Connect("pressed", this, nameof(on_toggle));
      _ui.GetNode<CheckBox>("Move").Connect("pressed", this, nameof(on_toggle));
      _ui.GetNode<CheckBox>("Influence").Connect("pressed", this, nameof(on_toggle));
      _map.Connect(nameof(Map.hex_touched), this, nameof(on_hex_touched));
      _viewportcontainer.Connect("resized", this, nameof(on_viewport_resized));
      on_toggle();
      await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
      on_viewport_resized();
      _ui.GetNode<Label>("OSInfo").Text = $"screen\n{OS.GetScreenSize()}\ndpi {OS.GetScreenDpi()}";
    }

    public void on_viewport_resized() {
      _camera.configure(_viewport.Size, _map.center(), _map.texture_size());
    }

    public void on_rotate() {
      _map.rotate_map();
      on_viewport_resized();
    }

    public void on_zoom(bool b) {
      _camera.update_camera(0, 0, b ? -0.05f : 0.05f);
    }

    public void on_toggle() {
      _map.set_mode(_ui.GetNode<CheckBox>("LOS").Pressed, _ui.GetNode<CheckBox>("Move").Pressed, _ui.GetNode<CheckBox>("Influence").Pressed);
    }

    public void on_hex_touched(Vector2 pos, Hex hex, int key) {
      string s = key == -1 ? "offmap" : hex.inspect();
      _ui.GetNode<Label>("Info").Text = $"\n({pos.x};{pos.y})\n -> {s}\n -> {key}";
    }

    public override void _UnhandledInput(InputEvent @event) {
      if (@event is InputEventMouseMotion _mme) {
        if (drag_map) {
          Vector2 dv = _mme.Relative * _camera.Zoom;
          _camera.update_camera(-dv.x, -dv.y, 0);
          moved++;
        } else {
          _map.on_mouse_move();
        }
      } else if (@event is InputEventMouseButton _mbe) {
        if (_mbe.ButtonIndex == 1) {
          if (moved < 5) {
            drag_map = _map.on_click(_mbe.Pressed);
          } else {
            drag_map = false;
          }
          moved = 0;
        } else if (_mbe.ButtonIndex == 3) {
          drag_map = _mbe.Pressed;
        } else if (_mbe.ButtonIndex == 4) {
          on_zoom(true);
        } else if (_mbe.ButtonIndex == 5) {
          on_zoom(false);
        }
      } else if (@event is InputEventKey _ke) {
        if (_ke.Scancode == (int)KeyList.Escape) {
          GetTree().Quit();
        }
      }
    }
  }
}
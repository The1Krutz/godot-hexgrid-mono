using Godot;
using Array = Godot.Collections.Array;

namespace Demo
{
    /// <summary>
    ///
    /// </summary>
    public class Main : Node2D
    {
        private Control _ui;
        private Map _map;
        private Camera _camera;
        private Viewport _viewport;
        private ViewportContainer _viewportcontainer;
        private int _moved;
        private bool _dragMap;

        public override async void _Ready()
        {
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

            _map.Connect(nameof(Map.HexTouched), this, nameof(OnHexTouched));
            _viewportcontainer.Connect("resized", this, nameof(OnViewportResized));

            OnToggle();
            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
            OnViewportResized();
            _ui.GetNode<Label>("OSInfo").Text = $"screen\n{OS.GetScreenSize()}\ndpi {OS.GetScreenDpi():F0}";
        }

        /// <summary>
        ///
        /// </summary>
        public void OnViewportResized()
        {
            _camera.Configure(_viewport.Size, _map.Center, _map.TextureSize);
        }

        /// <summary>
        ///
        /// </summary>
        public void OnRotate()
        {
            _map.RotateMap();
            OnViewportResized();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="b"></param>
        public void OnZoom(bool b) // TODO - figure out what this is and rename it
        {
            _camera.UpdateCamera(0, 0, b ? -0.05f : 0.05f);
        }

        /// <summary>
        ///
        /// </summary>
        public void OnToggle()
        {
            _map.SetMode(_ui.GetNode<CheckBox>("LOS").Pressed, _ui.GetNode<CheckBox>("Move").Pressed, _ui.GetNode<CheckBox>("Influence").Pressed);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="position"></param>
        /// <param name="hex"></param>
        /// <param name="key"></param>
        public void OnHexTouched(Vector2 position, Hex hex, int key)
        {
            string s = key == -1 ? "offmap" : hex.ToString();
            _ui.GetNode<Label>("Info").Text = $"\n({position.x:F1};{position.y:F1})\n -> {s}\n -> {key}";
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion _mme)
            {
                if (_dragMap)
                {
                    Vector2 dv = _mme.Relative * _camera.Zoom;
                    _camera.UpdateCamera(-dv.x, -dv.y, 0);
                    _moved++;
                }
                else
                {
                    _map.OnMouseMove();
                }
            }
            else if (@event is InputEventMouseButton _mbe)
            {
                switch (_mbe.ButtonIndex)
                {
                    case 1:
                        _dragMap = _moved < 5 && _map.OnClick(_mbe.Pressed);
                        _moved = 0;
                        break;
                    case 3:
                        _dragMap = _mbe.Pressed;
                        break;
                    case 4:
                        OnZoom(true);
                        break;
                    case 5:
                        OnZoom(false);
                        break;
                }
            }
            else if (@event is InputEventKey _ke)
            {
                if (_ke.Scancode == (int)KeyList.Escape)
                {
                    GetTree().Quit();
                }
            }
        }
    }
}
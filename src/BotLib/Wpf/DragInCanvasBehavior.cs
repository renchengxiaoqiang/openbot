using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace BotLib.Wpf
{
    public class DragInCanvasBehavior : Behavior<UIElement>
    {
        private MouseButtonEventHandler _downHandler;
        private MouseButtonEventHandler _upHandler;
        private MouseEventHandler _moveHandler;
        private Canvas _canvas;
        private bool _isDraging = false;
        private Point _offset;
        private Point _originLocation;
        public event EventHandler EvLocationChanged;

        protected override void OnAttached()
        {
            base.OnAttached();
            this._downHandler = new MouseButtonEventHandler(this.AssociatedObject_MouseLeftButtonDown);
            this._upHandler = new MouseButtonEventHandler(this.AssociatedObject_MouseLeftButtonUp);
            this._moveHandler = new MouseEventHandler(this.AssociatedObject_MouseMove);
            this.AssociatedObject.AddHandler(UIElement.MouseLeftButtonDownEvent, this._downHandler, true);
            this.AssociatedObject.AddHandler(UIElement.MouseLeftButtonUpEvent, this._upHandler, true);
            this.AssociatedObject.AddHandler(UIElement.MouseMoveEvent, this._moveHandler, true);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.RemoveHandler(UIElement.MouseLeftButtonDownEvent, this._downHandler);
            this.AssociatedObject.RemoveHandler(UIElement.MouseLeftButtonUpEvent, this._upHandler);
            this.AssociatedObject.RemoveHandler(UIElement.MouseMoveEvent, this._moveHandler);
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isDraging)
            {
                _isDraging = false;
                AssociatedObject.ReleaseMouseCapture();
                Point location = this.GetLocation();
                if (location.X != this._originLocation.X || location.Y != this._originLocation.Y)
                {
                    if (EvLocationChanged != null)
                    {
                        EvLocationChanged(AssociatedObject, null);
                    }
                }
            }


        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            bool isDraging = this._isDraging;
            if (isDraging)
            {
                Point position = e.GetPosition(this._canvas);
                AssociatedObject.SetValue(Canvas.TopProperty, position.Y - this._offset.Y);
                AssociatedObject.SetValue(Canvas.LeftProperty, position.X - this._offset.X);
            }

        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_canvas == null)
            {
                this._canvas = (VisualTreeHelper.GetParent(AssociatedObject) as Canvas);
            }
            if (_canvas == null)
            {
                Log.Error("Canvas==null");
            }
            else
            {
                this._isDraging = true;
                this._offset = e.GetPosition(AssociatedObject);
                this._originLocation = this.GetLocation();
                base.AssociatedObject.CaptureMouse();
            }
        }

        private Point GetLocation()
        {
            return AssociatedObject.PointToScreen(new Point(0.0, 0.0));
        }
    }
}

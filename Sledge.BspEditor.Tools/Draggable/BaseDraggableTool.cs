using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using Sledge.BspEditor.Rendering.Resources;
using Sledge.BspEditor.Rendering.Viewport;
using Sledge.Rendering.Cameras;
using Sledge.Rendering.Resources;
using Sledge.Rendering.Viewports;

namespace Sledge.BspEditor.Tools.Draggable
{
    public abstract class BaseDraggableTool : BaseTool
    {
        public List<IDraggableState> States { get; set; }

        public IDraggable CurrentDraggable { get; private set; }
        private ViewportEvent _lastDragMoveEvent;
        private Vector3? _lastDragPoint;

        protected BaseDraggableTool()
        {
            States = new List<IDraggableState>();
        }

        #region Virtual events
        protected virtual void OnDraggableMouseDown(MapViewport viewport, OrthographicCamera camera, ViewportEvent e, Vector3 position, IDraggable draggable)
        {

        }
        protected virtual void OnDraggableMouseUp(MapViewport viewport, OrthographicCamera camera, ViewportEvent e, Vector3 position, IDraggable draggable)
        {

        }
        protected virtual void OnDraggableClicked(MapViewport viewport, OrthographicCamera camera, ViewportEvent e, Vector3 position, IDraggable draggable)
        {

        }

        protected virtual void OnDraggableDragStarted(MapViewport viewport, OrthographicCamera camera, ViewportEvent e, Vector3 position, IDraggable draggable)
        {

        }

        protected virtual void OnDraggableDragMoving(MapViewport viewport, OrthographicCamera camera, ViewportEvent e, Vector3 previousPosition, Vector3 position, IDraggable draggable)
        {

        }

        protected virtual void OnDraggableDragMoved(MapViewport viewport, OrthographicCamera camera, ViewportEvent e, Vector3 previousPosition, Vector3 position, IDraggable draggable)
        {

        }

        protected virtual void OnDraggableDragEnded(MapViewport viewport, OrthographicCamera camera, ViewportEvent e, Vector3 position, IDraggable draggable)
        {

        }
        #endregion

        protected override void MouseClick(MapViewport viewport, OrthographicCamera camera, ViewportEvent e)
        {
            if (e.Dragging || e.Button != MouseButtons.Left) return;
            if (CurrentDraggable == null) return;
            var point = camera.ScreenToWorld(e.X, e.Y);
            point = camera.Flatten(point);
            OnDraggableClicked(viewport, camera, e, point, CurrentDraggable);
            if (!e.Handled) CurrentDraggable.Click(viewport, camera, e, point);
            Invalidate();
        }

        protected override void MouseDown(MapViewport viewport, OrthographicCamera camera, ViewportEvent e)
        {
            if (CurrentDraggable == null) return;
            var point = camera.ScreenToWorld(e.X, e.Y);
            point = camera.Flatten(point);
            OnDraggableMouseDown(viewport, camera, e, point, CurrentDraggable);
            if (!e.Handled) CurrentDraggable.MouseDown(viewport, camera, e, point);
            Invalidate();
        }

        protected override void MouseUp(MapViewport viewport, OrthographicCamera camera, ViewportEvent e)
        {
            if (CurrentDraggable == null) return;
            var point = camera.ScreenToWorld(e.X, e.Y);
            point = camera.Flatten(point);
            OnDraggableMouseUp(viewport, camera, e, point, CurrentDraggable);
            if (!e.Handled) CurrentDraggable.MouseUp(viewport, camera, e, point);
            Invalidate();
        }

        protected override void MouseMove(MapViewport viewport, OrthographicCamera camera, ViewportEvent e)
        {
            if (e.Dragging || e.Button == MouseButtons.Left) return;
            var point = camera.ScreenToWorld(e.X, e.Y);
            point = camera.Flatten(point);
            IDraggable drag = null;
            foreach (var state in States)
            {
                var drags = state.GetDraggables().ToList();
                drags.Add(state);
                foreach (var draggable in drags)
                {
                    if (draggable.CanDrag(viewport, camera, e, point))
                    {
                        drag = draggable;
                        break;
                    }
                }
                if (drag != null) break;
            }
            if (drag != CurrentDraggable)
            {
                CurrentDraggable?.Unhighlight(viewport);
                CurrentDraggable = drag;
                CurrentDraggable?.Highlight(viewport);
                Invalidate();
            }
        }

        protected override void DragStart(MapViewport viewport, OrthographicCamera camera, ViewportEvent e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (CurrentDraggable == null) return;
            var point = camera.Flatten(camera.ScreenToWorld(e.X, e.Y));
            OnDraggableDragStarted(viewport, camera, e, point, CurrentDraggable);
            if (!e.Handled) CurrentDraggable.StartDrag(viewport, camera, e, point);
            _lastDragPoint = point;
            _lastDragMoveEvent = e;
            Invalidate();
        }

        protected override void DragMove(MapViewport viewport, OrthographicCamera camera, ViewportEvent e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (CurrentDraggable == null || !_lastDragPoint.HasValue) return;
            var point = camera.Flatten(camera.ScreenToWorld(e.X, e.Y));
            var last = _lastDragPoint.Value;
            OnDraggableDragMoving(viewport, camera, e, last, point, CurrentDraggable);
            if (!e.Handled) CurrentDraggable.Drag(viewport, camera, e, last, point);
            if (!e.Handled) OnDraggableDragMoved(viewport, camera, e, last, point, CurrentDraggable);
            _lastDragPoint = point;
            _lastDragMoveEvent = e;
            Invalidate();
        }

        protected override void DragEnd(MapViewport viewport, OrthographicCamera camera, ViewportEvent e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (CurrentDraggable == null) return;
            var point = camera.ScreenToWorld(e.X, e.Y);
            point = camera.Flatten(point);
            OnDraggableDragEnded(viewport, camera, e, point, CurrentDraggable);
            if (!e.Handled) CurrentDraggable.EndDrag(viewport, camera, e, point);
            _lastDragMoveEvent = null;
            _lastDragPoint = null;
            Invalidate();
        }

        private IEnumerable<T> CollectObjects<T>(Func<IDraggable, IEnumerable<T>> collector)
        {
            var list = new List<T>();

            var foundActive = false;
            foreach (var state in States)
            {
                foreach (var draggable in state.GetDraggables())
                {
                    if (draggable == CurrentDraggable) foundActive = true;
                    else list.AddRange(collector(draggable));
                }
                if (state == CurrentDraggable) foundActive = true;
                else list.AddRange(collector(state));
            }
            if (CurrentDraggable != null && foundActive) list.AddRange(collector(CurrentDraggable));

            return list;
        }

        public override void Render(BufferBuilder builder, ResourceCollector resourceCollector)
        {
            foreach (var obj in CollectObjects(x => new[] {x}))
            {
                obj.Render(builder);
            }
            base.Render(builder, resourceCollector);
        }

        public override void Render(IViewport viewport, OrthographicCamera camera, Vector3 worldMin, Vector3 worldMax, Graphics graphics)
        {
            foreach (var obj in CollectObjects(x => new[] { x }).OrderBy(x => camera.GetUnusedValue(x.ZIndex)))
            {
                obj.Render(viewport, camera, worldMin, worldMax, graphics);
            }
            base.Render(viewport, camera, worldMin, worldMax, graphics);
        }

        public override void Render(IViewport viewport, PerspectiveCamera camera, Graphics graphics)
        {
            foreach (var obj in CollectObjects(x => new[] { x }).OrderByDescending(x => (x.Origin - camera.Position).LengthSquared()))
            {
                obj.Render(viewport, camera, graphics);
            }
            base.Render(viewport, camera, graphics);
        }
    }
}
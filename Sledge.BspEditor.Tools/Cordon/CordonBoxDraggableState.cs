﻿using System.Numerics;
using Sledge.BspEditor.Primitives.MapData;
using Sledge.BspEditor.Rendering.Viewport;
using Sledge.BspEditor.Tools.Draggable;
using Sledge.Rendering.Cameras;

namespace Sledge.BspEditor.Tools.Cordon
{
    public class CordonBoxDraggableState : BoxDraggableState
    {
        public CordonBoxDraggableState(BaseDraggableTool tool) : base(tool)
        {
        }

        public void Update()
        {
            if (Tool.Document == null)
            {
                State.Action = BoxAction.Idle;
            }
            else
            {
                var cordon = Tool.Document.Map.Data.GetOne<CordonBounds>() ?? new CordonBounds {Enabled = false};
                State.Start = cordon.Box.Start;
                State.End = cordon.Box.End;
                State.Action = BoxAction.Drawn;
            }
        }

        public override void Click(MapViewport viewport, OrthographicCamera camera, ViewportEvent e, Vector3 position)
        {
            //
        }

        public override bool CanDrag(MapViewport viewport, OrthographicCamera camera, ViewportEvent e, Vector3 position)
        {
            return false;
        }
    }
}

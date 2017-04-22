using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sledge.BspEditor.Primitives;
using Sledge.BspEditor.Primitives.MapObjectData;
using Sledge.BspEditor.Primitives.MapObjects;
using Sledge.BspEditor.Tools.Brush.Brushes.Controls;
using Sledge.Common;
using Sledge.DataStructures.Geometric;

namespace Sledge.BspEditor.Tools.Brush.Brushes
{
    [Export(typeof(IBrush))]
    public class PyramidBrush : IBrush
    {
        public string Name => "Pyramid";
        public bool CanRound => true;

        public IEnumerable<BrushControl> GetControls()
        {
            return new List<BrushControl>();
        }

        public IEnumerable<IMapObject> Create(UniqueNumberGenerator generator, Box box, string texture, int roundDecimals)
        {
            var solid = new Solid(generator.Next("MapObject"));
            solid.Data.Add(new ObjectColor(Colour.GetRandomBrushColour()));

            // The lower Z plane will be base
            var c1 = new Coordinate(box.Start.X, box.Start.Y, box.Start.Z).Round(roundDecimals);
            var c2 = new Coordinate(box.End.X, box.Start.Y, box.Start.Z).Round(roundDecimals);
            var c3 = new Coordinate(box.End.X, box.End.Y, box.Start.Z).Round(roundDecimals);
            var c4 = new Coordinate(box.Start.X, box.End.Y, box.Start.Z).Round(roundDecimals);
            var c5 = new Coordinate(box.Center.X, box.Center.Y, box.End.Z).Round(roundDecimals);
            var faces = new[]
                            {
                                new[] { c1, c2, c3, c4 },
                                new[] { c2, c1, c5 },
                                new[] { c3, c2, c5 },
                                new[] { c4, c3, c5 },
                                new[] { c1, c4, c5 }
                            };
            foreach (var arr in faces)
            {
                var face = new Face(generator.Next("Face"))
                {
                    Plane = new Plane(arr[0], arr[1], arr[2]),
                    Texture = {Name = texture }
                };
                face.Vertices.AddRange(arr);
                solid.Data.Add(face);
            }
            solid.DescendantsChanged();
            yield return solid;
        }
    }
}
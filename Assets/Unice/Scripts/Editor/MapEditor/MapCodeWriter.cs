using System;
using System.Globalization;
using System.Text;

namespace Unice
{
    /// <summary>
    /// Turns a <see cref="MapDefinition"/> into the C# source of its <c>Map&lt;Name&gt;.cs</c> file, in the same shape the maps
    /// have always had (<c>Id</c> + <c>Build()</c> with the <see cref="Room"/>, <see cref="MapDoor"/> and <see cref="MapProp"/>
    /// constructors). Output is deterministic (same map, same bytes): LF line endings, invariant numbers, no BOM.
    /// </summary>
    public static class MapCodeWriter
    {
        /// <summary>Doors written per line, so a map with many doors stays readable.</summary>
        const int DoorsPerLine = 4;

        const string Indent = "    ";
        const string FieldIndent = Indent + Indent + Indent;

        /// <summary>
        /// The full source of the map's file. <paramref name="extraComment"/> (optional, one line per paragraph, already XML-safe)
        /// follows the generated header in the class summary; <paramref name="extraMembers"/> (optional, already indented source)
        /// is written between <c>Id</c> and <c>Build()</c>, for static data the definition does not hold (the yard maps' off-limits rectangles).
        /// </summary>
        public static string Write(MapDefinition map, string className, string extraComment = null, string extraMembers = null)
        {
            var source = new StringBuilder();
            source.Append("namespace Unice\n{\n");
            source.Append(Indent).Append("/// <summary>\n");
            source.Append(Indent).Append("/// Written by the map editor (Unice &gt; Editor de mapas) and still fine to edit by hand: the editor reads the compiled map (Maps.ById), not this file, so a hand edit shows up next time it opens.\n");
            if (!string.IsNullOrEmpty(extraComment))
                foreach (var line in extraComment.Split('\n'))
                    source.Append(Indent).Append("/// ").Append(line.TrimEnd('\r')).Append('\n');
            source.Append(Indent).Append("/// </summary>\n");
            source.Append(Indent).Append("public static class ").Append(className).Append('\n');
            source.Append(Indent).Append("{\n");
            source.Append(Indent).Append(Indent).Append("public const string Id = ").Append(Quote(map.id)).Append(";\n\n");
            if (!string.IsNullOrEmpty(extraMembers))
                source.Append(extraMembers.Replace("\r\n", "\n").TrimEnd('\n')).Append("\n\n");

            source.Append(Indent).Append(Indent).Append("public static MapDefinition Build() => new MapDefinition\n");
            source.Append(Indent).Append(Indent).Append("{\n");
            source.Append(FieldIndent).Append("id = Id,\n");
            source.Append(FieldIndent).Append("worldW = ").Append(Num(map.worldW)).Append(",\n");
            source.Append(FieldIndent).Append("worldH = ").Append(Num(map.worldH)).Append(",\n");
            if (map.groundTint != null) source.Append(FieldIndent).Append("groundTint = ").Append(Quote(map.groundTint)).Append(",\n");
            source.Append(FieldIndent).Append("entryX = ").Append(Num(map.entryX)).Append(", entryY = ").Append(Num(map.entryY)).Append(",\n");
            source.Append(FieldIndent).Append("spawnX = ").Append(Num(map.spawnX)).Append(", spawnY = ").Append(Num(map.spawnY)).Append(",\n");

            WriteArray(source, "rooms", "Room", map.rooms == null ? 0 : map.rooms.Length, 1, index =>
            {
                var room = map.rooms[index];
                var text = "new Room(" + Quote(room.id) + ", " + Num(room.x) + ", " + Num(room.y) + ", " + Num(room.w) + ", " + Num(room.h);
                return room.tint != null ? text + ", tint: " + Quote(room.tint) + ")" : text + ")";
            });
            WriteDoors(source, "verticalDoors", map.verticalDoors);
            WriteDoors(source, "horizontalDoors", map.horizontalDoors);
            WriteArray(source, "furniture", "MapProp", map.furniture == null ? 0 : map.furniture.Length, 1, index =>
            {
                var prop = map.furniture[index];
                var text = "new MapProp(" + Quote(prop.type) + ", " + Num(prop.x) + ", " + Num(prop.y) + ", " + Num(prop.w) + ", " + Num(prop.h);
                return prop.hideSide != '\0' ? text + ", " + CharLiteral(prop.hideSide) + ")" : text + ")";
            });

            source.Append(Indent).Append(Indent).Append("};\n");
            source.Append(Indent).Append("}\n}\n");
            return source.ToString();
        }

        static void WriteDoors(StringBuilder source, string field, MapDoor[] doors)
        {
            WriteArray(source, field, "MapDoor", doors == null ? 0 : doors.Length, DoorsPerLine,
                index => "new MapDoor(" + Num(doors[index].x) + ", " + Num(doors[index].y) + ")");
        }

        /// <summary>One array field: <c>new T[0]</c> when empty, otherwise <c>new[] { … }</c> with <paramref name="perLine"/> entries per line.</summary>
        static void WriteArray(StringBuilder source, string field, string elementType, int count, int perLine, Func<int, string> entry)
        {
            if (count == 0)
            {
                source.Append(FieldIndent).Append(field).Append(" = new ").Append(elementType).Append("[0],\n");
                return;
            }
            source.Append(FieldIndent).Append(field).Append(" = new[]\n").Append(FieldIndent).Append("{\n");
            for (var start = 0; start < count; start += perLine)
            {
                source.Append(FieldIndent).Append(Indent);
                for (var index = start; index < Math.Min(start + perLine, count); index++)
                {
                    if (index > start) source.Append(' ');
                    source.Append(entry(index)).Append(',');
                }
                source.Append('\n');
            }
            source.Append(FieldIndent).Append("},\n");
        }

        /// <summary>Whole numbers without decimals ("824"), fractions with the <c>f</c> suffix ("12.5f"), always with the invariant culture.</summary>
        static string Num(float value)
        {
            if (value == MathF.Floor(value) && Math.Abs(value) < 1e9f) return ((long)value).ToString(CultureInfo.InvariantCulture);
            return value.ToString("R", CultureInfo.InvariantCulture) + "f";
        }

        static string Quote(string text) => "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

        static string CharLiteral(char value) => value == '\'' ? "'\\''" : value == '\\' ? "'\\\\'" : "'" + value + "'";
    }
}

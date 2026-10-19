using System;
using System.Collections.Generic;
using System.Globalization;

namespace CrystalRuins.Core
{
    public sealed class Level
    {
        public string Name { get; }
        public float TimeLimit { get; }
        public string[] Rows { get; }

        public Level(string name, float timeLimit, string[] rows)
        {
            Name = name;
            TimeLimit = timeLimit;
            Rows = rows;
        }

        // Text format: "key=value" header lines, then the map rows ('#' wall, 'P' start, ...).
        public static Level Parse(string text)
        {
            string name = "Fase";
            float time = 90f;
            var rows = new List<string>();

            foreach (var raw in text.Replace("\r", "").Split('\n'))
            {
                string line = raw.TrimEnd();
                if (line.Length == 0) continue;

                int eq = line.IndexOf('=');
                if (rows.Count == 0 && eq > 0)
                {
                    string key = line.Substring(0, eq).Trim();
                    string value = line.Substring(eq + 1).Trim();
                    if (key == "name") name = value;
                    else if (key == "time") time = float.Parse(value, CultureInfo.InvariantCulture);
                    continue;
                }

                rows.Add(line);
            }

            if (rows.Count == 0)
                throw new FormatException("Level has no map rows.");

            int width = rows[0].Length;
            foreach (var row in rows)
                if (row.Length != width)
                    throw new FormatException("Level rows must all have the same width.");

            return new Level(name, time, rows.ToArray());
        }
    }
}


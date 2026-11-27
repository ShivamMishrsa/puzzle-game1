using System;
using System.Collections.Generic;
using System.Reflection;

namespace ColorGateBlast.EditorTools
{
    /// <summary>
    /// Finds LevelNN classes by name (reflection), so each level lives in - and ships with - its own file.
    /// </summary>
    public static class LevelLibrary
    {
        public static LevelDefinition Build(int id)
        {
            Type t = Type.GetType("ColorGateBlast.EditorTools.Level" + id.ToString("00"));
            if (t == null) return null;
            MethodInfo m = t.GetMethod("Build", BindingFlags.Public | BindingFlags.Static);
            return m == null ? null : (LevelDefinition)m.Invoke(null, null);
        }

        public static List<LevelDefinition> Range(int from, int to)
        {
            List<LevelDefinition> l = new List<LevelDefinition>();
            for (int i = from; i <= to; i++)
            {
                LevelDefinition d = Build(i);
                if (d != null) l.Add(d);
            }
            return l;
        }

        public static List<LevelDefinition> All() { return Range(1, 150); }
    }
}

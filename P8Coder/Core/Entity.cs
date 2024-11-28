using System.Collections.Generic;
using System.Xml.Linq;

namespace P8Coder.Core
{
    public class Entity
    {
        public bool Enabled = true;
        public string Name = "Entity";
        public List<Function> Functions = [];

        public Entity()
        {

        }

        public Function Add(string name, string code, bool enabled = true)
        {
            Function f = new(name, code)
            {
                Enabled = enabled
            };
            Functions.Add(f);
            return f;
        }

        public bool FromXElement(XElement xentity)
        {
            Functions.Clear();
            Name = (string)xentity.Attribute("name");
            Enabled = (int)xentity.Attribute("enabled") == 1;

            foreach (XElement xfunc in xentity.Elements())
            {
                Function f = new("", "");
                f.FromXElement(xfunc);
                Functions.Add(f);
            }

            return true;
        }

        public XElement ToXElement()
        {
            XElement xentity = new("entity",
                new XAttribute("name", Name),
                new XAttribute("enabled", Enabled ? 1 : 0)
                );

            foreach (Function func in Functions)
            {
                xentity.Add(func.ToXElement());
            }

            return xentity;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

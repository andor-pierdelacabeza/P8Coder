﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace P8Coder.Core
{
    public class Project
    {
        public bool Changed = false;

        public string Filename;
        public string CartFilename
        {
            get
            {
                if (Cart == null) return "";
                else return Cart.Filename;
            }
            set
            {
                if (Cart != null)
                {
                    Cart.Dispose();
                    Cart = null;
                }

                if (value != string.Empty)
                {
                    Cart = new Cartridge(value);
                    try
                    {
                        Cart.Load();
                    }
                    catch (Exception e)
                    {
                        //Debug.WriteLine(e.Message);
                        Cart = null;
                    }
                }
            }
        }
        public Cartridge Cart { get; private set; }

        public int Version { get; private set; }
        public string Name;
        public string Author;

        public string Code { get { return AssembleCode(); } }

        public List<Entity> Entities = [];

        public Project()
        {
            Version = 1;
            Filename = "";
            CartFilename = "";
            Name = "New game";
            Author = "";

            Entity notes = new()
            {
                Enabled = false,
                Name = "Notes"
            };
            notes.Add("ideas", "ideas\n\n  1. \n  2. ", false);
            notes.Add("todo", "todo\n\n  1. \n  2. ", false);
            Entities.Add(notes);

            Entity main = new()
            {
                Name = "Game"
            };
            main.Add("globals", "f=0");
            main.Add("_init", "function _init()\nend");
            main.Add("_update", "function _update()\nend");
            main.Add("_draw", "function _draw()\nend");
            Entities.Add(main);
        }

        public bool Save()
        {
            if (String.IsNullOrEmpty(Filename)) return false;

            XElement xml = ToXElement();
            xml.Save(Filename);

            return true;
        }

        public bool Load(string filename)
        {
            if (!File.Exists(filename)) return false;
            Filename = filename;

            XElement xproject = XElement.Load(filename, LoadOptions.None);

            try
            {
                FromXElement(xproject);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private string AssembleCode()
        {
            string nl = "\n";
            string code = "--" + Name.ToLower() + nl;
            code += "--by " + Author.ToLower() + nl + nl;

            foreach (Entity entity in Entities)
            {
                if (!entity.Enabled) continue;

                code += "--" + entity.Name + nl;
                foreach (Function func in entity.Functions)
                {
                    if (func.Enabled) code += func.Code.ToLower() + nl + nl;
                }
            }

            return code;
        }

        public bool WriteCart()
        {
            //NOTE: this could be done inside the Cartridge class

            if (String.IsNullOrWhiteSpace(CartFilename) ||
                !File.Exists(CartFilename)) return false;

            string rawText = File.ReadAllText(CartFilename);

            string[] sections = Regex.Split(rawText, "(__(?:lua|gfx|gff|map|sfx|music)__)");

            for (int i = 1; i < sections.Length; i += 2)
            {
                if (sections[i] == "__lua__")
                {
                    sections[i + 1] = "\n" + AssembleCode();
                }
            }

            rawText = string.Join("", sections);
            File.WriteAllText(CartFilename, rawText);

            return true;
        }


        // XML LOADING / SAVING
        private XElement ToXElement()
        {
            XElement xproject = new("project",
                new XAttribute("version", Version)
            );

            XElement xgame = new("game",
                new XElement("name", Name),
                new XElement("author", Author),
                new XElement("cart", CartFilename));
            xproject.Add(xgame);

            XElement xobjects = new("entities");
            foreach (Entity entity in Entities)
            {
                xobjects.Add(entity.ToXElement());
            }
            xproject.Add(xobjects);

            return xproject;
        }

        private void FromXElement(XElement xproject)
        {
            Entities.Clear();
            Version = (int)xproject.Attribute("version");

            switch (Version)
            {
                case 1:
                    XElement xgame = xproject.Element("game");
                    if (xgame != null)
                    {
                        Name = (string)xgame.Element("name");
                        Author = (string)xgame.Element("author");
                        CartFilename = (string)xgame.Element("cart");
                    }

                    // load game entities
                    foreach (XElement xentity in xproject.Element("entities").Elements())
                    {
                        Entity entity = new();
                        entity.FromXElement(xentity);
                        Entities.Add(entity);
                    }
                    break;

                default:
                    throw new Exception("Unknown project file format version!");
            }
        }
    }
}

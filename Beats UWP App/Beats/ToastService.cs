﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace Beats
{
    public static class ToastService
    {
        public static XmlDocument CreateToast(string msg)
        {
            var xDoc = new XDocument(new XElement("toast",
                new XElement("visual",
                new XElement("binding",
                new XAttribute("template", "ToastGeneric"),
                new XElement("text", "BEATS: Alert"),
                new XElement("text", msg)) // binding
                ), // visual
                new XElement("actions",
                new XElement("action",
                new XAttribute("activationType", "foreground"),
                new XAttribute("content", "I'm Okay"),
                new XAttribute("arguments", "yes")),
                new XElement("action",
                new XAttribute("activationType", "foreground"),
                new XAttribute("content", "Help me!"),
                new XAttribute("arguments", "no")))
                // actions
                ));

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            return xmlDoc;
        }
    }
}

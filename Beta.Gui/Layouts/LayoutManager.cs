using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Beta.Common;
using Beta.Gui.Elements;
using Beta.Gui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Gui.Layouts;

public class LayoutManager
{
    private readonly StyleManager _styleManager;
    private readonly ContentCache _contentCache;

    public LayoutManager(ContentCache contentCache)
    {
        if (Gui.Instance.StyleManager is null)
        {
            var message = "Style Manager is not loaded.";
            Gui.Instance.Logger?.Error(message);
            throw new InvalidOperationException(message);
        }
        _styleManager = Gui.Instance.StyleManager;
        _contentCache = contentCache;
    }

    public GuiElement LoadTag(XElement tag, float layerDepth)
    {
        var children = new List<GuiElement>();

        Gui.Instance.Logger?.Debug($"Process tag <{tag.Name}>");
        foreach (var child in tag.Elements())
        {
            Gui.Instance.Logger?.Debug($"Found elements in tag, recursing...");
            children.Add(
                LoadTag(child, layerDepth + Constants.LayerDepthStep)
            );
        }

        // Create the element based on tag.
        // If it didn't have elements, just use the text.
        // If it had elements, add them as children
        GuiElement? newElem = null;

        var elemClass = tag.Attribute("class")?.Value;
        var elemId = tag.Attribute("id")?.Value;
        var extraInputContexts = tag.Attribute("extra-input-contexts")?.Value;

        var style = _styleManager.GetStyle(tag.Name.LocalName, elemClass, elemId);
        if (style.LayerDepth == 0f)
        {
            style.LayerDepth = layerDepth;
        }
        if (string.Equals(tag.Name.LocalName, "root", StringComparison.OrdinalIgnoreCase))
        {
            newElem = new GuiRoot(style with { RelativePosition = new Vector2(0, 0), Size = new SizeF(float.MaxValue, float.MaxValue) }, extraInputContexts);
        }
        if (string.Equals(tag.Name.LocalName, "empty", StringComparison.OrdinalIgnoreCase))
        {
            newElem = new Empty(style with { }, extraInputContexts);
        }
        if (string.Equals(tag.Name.LocalName, "box", StringComparison.OrdinalIgnoreCase))
        {
            newElem = new Box(style with { }, extraInputContexts);
        }
        if (string.Equals(tag.Name.LocalName, "label", StringComparison.OrdinalIgnoreCase))
        {
            newElem = new Label(tag.Value.Trim(), style with { }, extraInputContexts);
        }
        if (string.Equals(tag.Name.LocalName, "text-button", StringComparison.OrdinalIgnoreCase))
        {
            newElem = new TextButton(tag.Value.Trim(), style with { }, extraInputContexts);
        }
        if (string.Equals(tag.Name.LocalName, "image", StringComparison.OrdinalIgnoreCase))
        {
            var srcStr = (tag.Attribute("src")?.Value)
                ?? throw new ArgumentException(
                    $"`src` attribute is required for {tag.Name.LocalName}");

            var texture = _contentCache.Get<Texture2D>(srcStr);
            newElem = new Image(texture, style with { }, extraInputContexts);
        }
        if (string.Equals(tag.Name.LocalName, "image-button", StringComparison.OrdinalIgnoreCase))
        {
            var srcStr = (tag.Attribute("src")?.Value)
                ?? throw new ArgumentException(
                    $"`src` attribute is required for {tag.Name.LocalName}");

            var texture = _contentCache.Get<Texture2D>(srcStr);

            var tooltipStr = tag.Attribute("tooltip")?.Value;

            newElem = new ImageButton(texture, style with { }, tooltipStr, extraInputContexts);
        }
        if (string.Equals(tag.Name.LocalName, "text-box", StringComparison.OrdinalIgnoreCase))
        {
            newElem = new TextBox(tag.Value.Trim(), style with { }, extraInputContexts);
        }
        if (string.Equals(tag.Name.LocalName, "checkbox", StringComparison.OrdinalIgnoreCase))
        {
            var text = tag.Attribute("text")?.Value;
            var initValueStr = tag.Attribute("value")?.Value;
            var initValue = initValueStr is null ? false : bool.Parse(initValueStr);
            newElem = new Checkbox(text ?? "", initValue, style with { }, extraInputContexts);
        }
        if (string.Equals(tag.Name.LocalName, "slider", StringComparison.OrdinalIgnoreCase))
        {
            var text = tag.Attribute("text")?.Value;
            var initValueStr = tag.Attribute("value")?.Value;
            var minStr = tag.Attribute("min")?.Value;
            var maxStr = tag.Attribute("max")?.Value;
            var initValue = initValueStr is null ? 0 : int.Parse(initValueStr, CultureInfo.InvariantCulture);
            var min = minStr is null ? 0 : int.Parse(minStr, CultureInfo.InvariantCulture);
            var max = maxStr is null ? 0 : int.Parse(maxStr, CultureInfo.InvariantCulture);
            newElem = new Slider(text ?? "", min, max, initValue, style with { }, extraInputContexts);
        }
        if (string.Equals(tag.Name.LocalName, "text-input", StringComparison.OrdinalIgnoreCase))
        {
            var value = tag.Attribute("value")?.Value;
            var hint = tag.Attribute("hint")?.Value;
            // TODO: Parse allowed input
            newElem = new TextInput(
                value ?? "",
                hint ?? "",
                AllowedTextInput.Letters | AllowedTextInput.Numbers,
                style with { },
                extraInputContexts
            );
        }
        if (string.Equals(tag.Name.LocalName, "listview", StringComparison.OrdinalIgnoreCase))
        {
            var listView = new ListView<GuiElement>(style, extraInputContexts);
            newElem = listView;
        }
        if (string.Equals(tag.Name.LocalName, "dropdown", StringComparison.OrdinalIgnoreCase))
        {
            var name = tag.Attribute("name")?.Value;
            var listView = new DropdownMenu(name ?? "", style with { }, extraInputContexts);
            newElem = listView;
        }

        if (newElem is null)
        {
            throw new ArgumentException($"Unsupported tag: <{tag.Name}>");
        }
        else
        {
            newElem.ElemClass = elemClass ?? "";
            newElem.ElemId = elemId ?? "";
            foreach (var child in children)
            {
                newElem.AddElement(child);
            }
            return newElem;
        }
    }

    public GuiElement LoadFromFile(string file, float baseLayerDepth)
    {
        var xdoc = XDocument.Parse(FileLoader.ReadAllFromFile(file));
        var rootTag = xdoc.Element("root");

        if (rootTag is null)
        {
            var message = "Root tag is missing.";
            Gui.Instance.Logger?.Error(message);
            throw new InvalidOperationException(message);
        }

        var root = LoadTag(rootTag, baseLayerDepth);

        Walk(root, 0);

        return root;
    }

    public GuiElement ReadAppendixFromFile(string file, float baseLayerDepth)
    {
        var xdoc = XDocument.Parse(FileLoader.ReadAllFromFile(file));
        var firstTag = xdoc.Elements().FirstOrDefault();
        if (firstTag is null)
        {
            var message = "Tag is missing.";
            Gui.Instance.Logger?.Error(message);
            throw new InvalidOperationException(message);
        }

        var elem = LoadTag(firstTag, baseLayerDepth);

        Walk(elem, 0);

        return elem;
    }

    private void Walk(GuiElement root, int indent)
    {
        Gui.Instance.Logger?.Debug($"{new string(' ', indent)}{root.GetType()}, class {root.ElemClass} | id {root.ElemId}");
        foreach (var child in root.Children)
        {
            Walk(child, indent + 2);
        }
    }
}
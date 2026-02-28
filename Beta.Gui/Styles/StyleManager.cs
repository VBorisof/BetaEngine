using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Beta.Common;
using Beta.Common.Extensions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System.Globalization;

namespace Beta.Gui.Styles;

public partial class StyleManager
{
    [GeneratedRegex(@"\s*(?<comment>\/\*[\s\S]*?\*\/\s*)*(?<selector>[^{]+)\s*\{\s*(?<declarations>[^}]+)\}", RegexOptions.Compiled)]
    private static partial Regex RuleSetRegex();

    [GeneratedRegex(@"(?<property>[^:]+)\s*:\s*(?<value>[^;]+);", RegexOptions.Compiled)]
    private static partial Regex DeclarationRegex();

    private const string AllSelector = "*";
    private StyleSheet _styleSheet;

    public StyleManager()
    {
        _styleSheet = new StyleSheet();
    }

    public void Clear()
    {
        _styleSheet = new StyleSheet();
    }

    public void AppendFromFile(string file)
    {
        var css = FileLoader.ReadAllFromFile(file);

        // Split CSS content into rule sets
        var ruleSetRegex = RuleSetRegex();
        var matches = ruleSetRegex.Matches(css);

        foreach (var match in matches.Cast<Match>())
        {
            // Extract selectors
            var selectors = match.Groups["selector"].Value.Trim().Split(' ');
            foreach (var selector in selectors)
            {
                var ruleSet = _styleSheet.RuleSets.SingleOrDefault(rs =>
                    string.Equals(
                        rs.Selector, selector,
                        StringComparison.OrdinalIgnoreCase));
                if (ruleSet is null)
                {
                    ruleSet = new StyleRuleSet
                    {
                        Selector = selector
                    };
                    _styleSheet.RuleSets.Add(ruleSet);
                }

                // Split declarations
                var declarationRegex = DeclarationRegex();
                var declarationMatches = declarationRegex.Matches(match.Groups["declarations"].Value);

                foreach (var declarationMatch in declarationMatches.Cast<Match>())
                {
                    var property = declarationMatch.Groups["property"].Value.Trim();
                    var value = declarationMatch.Groups["value"].Value.Trim();
                    ruleSet.Declarations[property] = value;
                }
            }
        }

        Gui.Instance.Logger?.Debug($"Stylesheet {file} loaded OK.");
        Dump();
    }

    public bool TryFindStyles(string selector, out GuiElementStyleQueryObject? style)
    {
        if (_styleSheet is null)
        {
            Gui.Instance.Logger?.Error("Stylesheet is not loaded.");
            style = null;
            return false;
        }
        if (selector is null)
        {
            style = null;
            return false;
        }

        Gui.Instance.Logger?.Debug($"Try match selector: {selector}");
        foreach (var ruleSet in _styleSheet.RuleSets)
        {
            var selectors = ruleSet.Selector.Split(' ');
            foreach (var s in selectors)
            {
                Gui.Instance.Logger?.Debug($"Ruleset selector: {s.Trim('#').Trim('.')}");
            }
            if (selectors.Any(s => string.Equals(s.Trim('#').Trim('.'), selector, StringComparison.OrdinalIgnoreCase)))
            {
                Gui.Instance.Logger?.Debug($"MATCH selector: {selector}");
                float? layerDepth = null;
                SizeF? size = null;
                Color? color = null;
                Color? hoverColor = null;
                Vector2? position = null;
                Color? textColor = null;
                Color? hoverTextColor = null;
                Color? borderColor = null;
                int? childItemHeight = null;
                bool? hidden = null;
                bool? disabled = null;
                Color? disabledColor = null;

                var hasLayerDepth = ruleSet.Declarations.TryGetValue("layer-depth", out var layerDepthStr);
                var hasSize = ruleSet.Declarations.TryGetValue("size", out var sizeStr);
                var hasColor = ruleSet.Declarations.TryGetValue("color", out var colorStr);
                var hasHoverColor = ruleSet.Declarations.TryGetValue("hover-color", out var hoverColorStr);
                var hasPosition = ruleSet.Declarations.TryGetValue("position", out var positionStr);
                var hasTextColor = ruleSet.Declarations.TryGetValue("text-color", out var textColorStr);
                var hasHoverTextColor = ruleSet.Declarations.TryGetValue("hover-text-color", out var hoverTextColorStr);
                var hasBorderColor = ruleSet.Declarations.TryGetValue("border-color", out var borderColorStr);
                var hasChildItemHeight = ruleSet.Declarations.TryGetValue("child-item-height", out var childItemHeightStr);
                var hasHidden = ruleSet.Declarations.TryGetValue("hidden", out var hiddenStr);
                var hasDisabled = ruleSet.Declarations.TryGetValue("disabled", out var disabledStr);
                var hasDisabledColor = ruleSet.Declarations.TryGetValue("disabled-color", out var disabledColorStr);

                if (hasLayerDepth)
                {
                    layerDepth = float.Parse(layerDepthStr!, CultureInfo.InvariantCulture);
                }
                if (hasSize)
                {
                    var dimensions = sizeStr!.Split(',');
                    size = new SizeF(float.Parse(dimensions[0], CultureInfo.InvariantCulture), float.Parse(dimensions[1], CultureInfo.InvariantCulture));
                }
                if (hasColor)
                {
                    color = ColorEx.FromHexString(colorStr!.Trim('\"'));
                }
                if (hasHoverColor)
                {
                    hoverColor = ColorEx.FromHexString(hoverColorStr!.Trim('\"'));
                }
                if (hasPosition)
                {
                    var dimensions = positionStr!.Split(',');
                    position = new Vector2(float.Parse(dimensions[0], CultureInfo.InvariantCulture), float.Parse(dimensions[1], CultureInfo.InvariantCulture));
                }
                if (hasTextColor)
                {
                    textColor = ColorEx.FromHexString(textColorStr!.Trim('\"'));
                }
                if (hasHoverTextColor)
                {
                    hoverTextColor = ColorEx.FromHexString(hoverTextColorStr!.Trim('\"'));
                }
                if (hasBorderColor)
                {
                    borderColor = ColorEx.FromHexString(borderColorStr!.Trim('\"'));
                }
                if (hasChildItemHeight)
                {
                    childItemHeight = int.Parse(childItemHeightStr!, CultureInfo.InvariantCulture);
                }
                if (hasHidden)
                {
                    hidden = bool.Parse(hiddenStr!);
                }
                if (hasDisabled)
                {
                    disabled = bool.Parse(disabledStr!);
                }
                if (hasDisabledColor)
                {
                    disabledColor = ColorEx.FromHexString(disabledColorStr!.Trim('\"'));
                }

                style = new GuiElementStyleQueryObject
                {
                    LayerDepth = layerDepth,
                    Size = size,
                    Color = color,
                    HoverColor = hoverColor,
                    TextColor = textColor,
                    HoverTextColor = hoverTextColor,
                    Position = position,
                    BorderColor = borderColor,
                    ChildItemHeight = childItemHeight,
                    Hidden = hidden,
                    Disabled = disabled,
                    DisabledColor = disabledColor,
                };
                return true;
            }
        }
        style = null;
        return false;
    }

    private void OverrideStyleProperties(GuiElementStyle style, GuiElementStyleQueryObject styleQueryObject)
    {
        if (styleQueryObject.LayerDepth is not null)
        {
            style.LayerDepth = styleQueryObject.LayerDepth.Value;
        }
        if (styleQueryObject.Size is not null)
        {
            style.Size = styleQueryObject.Size.Value;
        }
        if (styleQueryObject.Color is not null)
        {
            style.Color = styleQueryObject.Color.Value;
        }
        if (styleQueryObject.HoverColor is not null)
        {
            style.HoverColor = styleQueryObject.HoverColor.Value;
        }
        if (styleQueryObject.Position is not null)
        {
            style.RelativePosition = styleQueryObject.Position.Value;
        }
        if (styleQueryObject.TextColor is not null)
        {
            style.TextColor = styleQueryObject.TextColor.Value;
        }
        if (styleQueryObject.HoverTextColor is not null)
        {
            style.HoverTextColor = styleQueryObject.HoverTextColor.Value;
        }
        if (styleQueryObject.BorderColor is not null)
        {
            style.BorderColor = styleQueryObject.BorderColor.Value;
        }
        if (styleQueryObject.ChildItemHeight is not null)
        {
            style.ChildItemHeight = styleQueryObject.ChildItemHeight.Value;
        }
        if (styleQueryObject.Hidden is not null)
        {
            style.Hidden = styleQueryObject.Hidden.Value;
        }
        if (styleQueryObject.Disabled is not null)
        {
            style.Disabled = styleQueryObject.Disabled.Value;
        }
        if (styleQueryObject.DisabledColor is not null)
        {
            style.DisabledColor = styleQueryObject.DisabledColor.Value;
        }
    }

    public GuiElementStyle GetStyle(string tagName, string? className, string? idName)
    {
        var style = new GuiElementStyle
        {
            Size = new SizeF(10, 10),
            Color = Color.Transparent,
            RelativePosition = new Vector2(0, 0),
        };

        var hasAllStyle = TryFindStyles(AllSelector, out var allStyle);
        var hasTagStyle = TryFindStyles(tagName, out var tagStyle);

        GuiElementStyleQueryObject? classStyle = null, idStyle = null;
        var hasClassStyle = className is not null && TryFindStyles(className, out classStyle);
        var hasIdStyle = idName is not null && TryFindStyles(idName, out idStyle);

        // Wildcard < Tag < Class < Id 
        if (hasAllStyle)
        {
            OverrideStyleProperties(style, allStyle!);
        }
        if (hasTagStyle)
        {
            OverrideStyleProperties(style, tagStyle!);
        }
        if (hasClassStyle)
        {
            OverrideStyleProperties(style, classStyle!);
        }
        if (hasIdStyle)
        {
            OverrideStyleProperties(style, idStyle!);
        }

        return style;
    }

    private void Dump()
    {
        if (_styleSheet is null || Gui.Instance.Logger is null)
        {
            return;
        }

        foreach (var ruleset in _styleSheet.RuleSets)
        {
            Gui.Instance.Logger.Debug($"{ruleset.Selector}: {{");
            foreach (var declarations in ruleset.Declarations)
            {
                Gui.Instance.Logger.Debug($"  {declarations.Key}: {declarations.Value}");
            }
            Gui.Instance.Logger.Debug($"}}");
        }
    }
}
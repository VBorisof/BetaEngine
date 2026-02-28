using Beta.Verbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Beta.Tutorials;

public record TutorialStepAction
{
    [JsonPropertyName("actionType")]
    [JsonConverter(typeof(JsonStringEnumConverter<TutorialStepActionType>))]
    public TutorialStepActionType ActionType { get; set; }
    
    [JsonPropertyName("entityName")]
    public string? EntityName { get; set; }
    
    [JsonPropertyName("propName")]
    public string? PropName { get; set; }

    [JsonPropertyName("itemName")]
    public string? ItemName { get; init; }
    
    [JsonPropertyName("exitName")]
    public string? ExitName { get; set; }

    public static TutorialStepActionType GetActionTypeFromVerb(Verb verb)
    {
        return verb switch
        {
            Verb.Walk => TutorialStepActionType.WalkableAreaLeftClick,
            Verb.Look => TutorialStepActionType.Look,
            Verb.Pickup => TutorialStepActionType.Pickup,
            Verb.Talk => TutorialStepActionType.Talk,
            Verb.Interact => TutorialStepActionType.Interact,
            Verb.Use => TutorialStepActionType.Use,
            _ => throw new ArgumentException($"Invalid verb {verb}.")
        };
    }

    public bool IsMatch(TutorialStepAction other)
    {
        if (ActionType == TutorialStepActionType.All)
        {
            return true;
        }

        if (ActionType != other.ActionType)
        {
            return false;
        }

        bool isEntityNameMatch =
            string.Equals(
                EntityName,
                other.EntityName,
                StringComparison.OrdinalIgnoreCase
            );

        bool isPropNameMatch =
            string.Equals(
                PropName,
                other.PropName,
                StringComparison.OrdinalIgnoreCase
            );

        bool isItemNameMatch =
            string.Equals(
                ItemName,
                other.ItemName,
                StringComparison.OrdinalIgnoreCase
            );

        switch (ActionType)
        {
            case TutorialStepActionType.WalkableAreaLeftClick:
            case TutorialStepActionType.InventoryOpen:
            case TutorialStepActionType.InventoryDismiss:
            case TutorialStepActionType.VerbMenuDismiss:
                return true;

            case TutorialStepActionType.EntityLeftClick:
                return isEntityNameMatch;

            case TutorialStepActionType.PropLeftClick:
                return isPropNameMatch;

            case TutorialStepActionType.Look:
            case TutorialStepActionType.Pickup:
            case TutorialStepActionType.Interact:
            case TutorialStepActionType.Talk:
                return isEntityNameMatch || isPropNameMatch;
                    
            case TutorialStepActionType.InventoryPickItem:
                return isItemNameMatch;

            case TutorialStepActionType.InventoryCombineItem:
                // Items can be combined both-ways ":/
                var isOtherWayMatch = string.Equals(
                        ItemName,
                        other.EntityName,
                        StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        EntityName,
                        other.ItemName,
                        StringComparison.OrdinalIgnoreCase);

                return isOtherWayMatch || (isEntityNameMatch && isItemNameMatch);


            case TutorialStepActionType.EntityUse:
                return isEntityNameMatch && isItemNameMatch;

            case TutorialStepActionType.PropUse:
                return isPropNameMatch && isItemNameMatch;

            case TutorialStepActionType.Exit:
                return true;

            default:
                return false;
        }
    }

    public bool IsAllowed(IEnumerable<TutorialStepAction> allowedActions)
    {
        var actions = allowedActions
            .Where(a => a.ActionType == ActionType)
            .ToList();

        if (actions.Count == 0)
        {
            return false;
        }

        switch (ActionType)
        {
            case TutorialStepActionType.All:
            case TutorialStepActionType.WalkableAreaLeftClick:
            case TutorialStepActionType.VerbMenuClick:
            case TutorialStepActionType.VerbMenuDismiss:
            case TutorialStepActionType.InventoryOpen:
            case TutorialStepActionType.InventoryDismiss:
            case TutorialStepActionType.Exit:
                return true;

            case TutorialStepActionType.EntityLeftClick:
                if (string.Equals("*", EntityName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return actions.Any(a => string.Equals(
                    EntityName,
                    a.EntityName,
                    StringComparison.OrdinalIgnoreCase
                ));

            case TutorialStepActionType.EntityUse:
                if (string.Equals("*", EntityName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return actions.Any(a => string.Equals(
                        EntityName,
                        a.EntityName,
                        StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        ItemName,
                        a.ItemName,
                        StringComparison.OrdinalIgnoreCase));

            case TutorialStepActionType.PropLeftClick:
                if (string.Equals("*", PropName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return actions.Any(a => string.Equals(
                    PropName,
                    a.PropName,
                    StringComparison.OrdinalIgnoreCase
                ));

            case TutorialStepActionType.PropUse:
                if (string.Equals("*", PropName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return actions.Any(a => string.Equals(
                        PropName,
                        a.PropName,
                        StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        ItemName,
                        a.ItemName,
                        StringComparison.OrdinalIgnoreCase));

            case TutorialStepActionType.InventoryPickItem:
                if (string.Equals("*", ItemName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return actions.Any(a => string.Equals(
                        ItemName,
                        a.ItemName,
                        StringComparison.OrdinalIgnoreCase));

            case TutorialStepActionType.InventoryCombineItem:
                if (string.Equals("*", EntityName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return actions.Any(a => string.Equals(
                        EntityName,
                        a.EntityName,
                        StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        ItemName,
                        a.ItemName,
                        StringComparison.OrdinalIgnoreCase));

            case TutorialStepActionType.Look:
            case TutorialStepActionType.Pickup:
            case TutorialStepActionType.Interact:
            case TutorialStepActionType.Talk:
                return actions.Any(a => string.Equals(
                        PropName,
                        a.PropName,
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        ItemName,
                        a.ItemName,
                        StringComparison.OrdinalIgnoreCase));
            default:
                throw new ArgumentException($"Invalid TutorialStepActionType: {ActionType}");
        }
    }
}
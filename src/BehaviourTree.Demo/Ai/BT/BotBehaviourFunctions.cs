using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.Nodes;
using BehaviourTree.Demo.UI;
using System.Numerics;

namespace BehaviourTree.Demo.Ai.BT
{
    internal static class BotBehaviourFunctions
    {
        public static BehaviourStatus EatFoodFromInventory(BtContext context)
        {
            var inventoryComponent = context.Agent.GetComponent<InventoryComponent>()!;

            if (!inventoryComponent.Has(ItemTypes.Food))
            {
                return BehaviourStatus.Failed;
            }

            var healthComponent = context.Agent.GetComponent<HealthComponent>()!;

            healthComponent.IncreaseBy(30);

            inventoryComponent.Remove(ItemTypes.Food);

            return BehaviourStatus.Succeeded;

        }

        public static bool IsHealthLow(BtContext context)
        {
            return context.Agent.GetComponent<HealthComponent>()!.Health < 50;
        }

        public static BehaviourStatus BuildHouse(BtContext context, int requiredStones, int requiredWood)
        {
            var inventoryComponent = context.Agent.GetComponent<InventoryComponent>()!;

            if (!inventoryComponent.Has(ItemTypes.Stone, requiredStones) ||
                !inventoryComponent.Has(ItemTypes.Wood, requiredWood))
            {
                return BehaviourStatus.Failed;
            }

            var position = context.Agent.GetComponent<PositionComponent>()!.Position;

            context.Engine.NewEntity()
                .AddComponent(new RenderComponent(new StaticImage(Assets.House)))
                .AddComponent(new PositionComponent(position))
                .AddComponent(new ItemComponent(ItemTypes.House));

            inventoryComponent.Remove(ItemTypes.Stone, requiredStones);
            inventoryComponent.Remove(ItemTypes.Wood, requiredWood);

            return BehaviourStatus.Succeeded;
        }

        public static bool HasItem(BtContext context, ItemTypes itemType, int quantity)
        {
            return context.Agent.GetComponent<InventoryComponent>()!.Has(itemType, quantity);
        }

        public static BehaviourStatus SetItemAsTarget(BtContext context, ItemTypes itemType)
        {
            var agentPosition = context.Agent.GetComponent<PositionComponent>()!.Position;

            ItemNode? closest = null;
            var closestDistSq = float.MaxValue;

            foreach (var node in context.Engine.GetNodes<ItemNode>())
            {
                if (node.ItemComponent.ItemType != itemType)
                    continue;

                var distSq = Vector2.DistanceSquared(node.PositionComponent.Position, agentPosition);
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closest = node;
                }
            }

            if (closest == null)
            {
                return BehaviourStatus.Failed;
            }

            context.Agent.AddComponent(new TargetEntityComponent { TargetId = closest.Entity.Id });

            return BehaviourStatus.Succeeded;
        }

        public static BehaviourStatus MoveToTargetEntity(BtContext context)
        {
            var movementComponent = context.Agent.GetComponent<MovementComponent>()!;

            if (!context.Agent.HasComponent<TargetEntityComponent>() || !context.Agent.HasComponent<PositionComponent>())
            {
                movementComponent.Velocity = Vector2.Zero;
                return BehaviourStatus.Failed;
            }

            var positionComponent = context.Agent.GetComponent<PositionComponent>()!;
            var targetEntityComponent = context.Agent.GetComponent<TargetEntityComponent>()!;

            var position = positionComponent.Position;
            var targetId = targetEntityComponent.TargetId;

            var target = context.Engine.GetEntityById(targetId);

            if (target == null)
            {
                movementComponent.Velocity = Vector2.Zero;
                return BehaviourStatus.Failed;
            }

            if (!target.HasComponent<PositionComponent>())
            {
                movementComponent.Velocity = Vector2.Zero;
                return BehaviourStatus.Failed;
            }

            var targetPosition = target.GetComponent<PositionComponent>()!;
            var distance = Vector2.Distance(position, targetPosition.Position);

            if (distance < 2)
            {
                movementComponent.Velocity = Vector2.Zero;
                return BehaviourStatus.Succeeded;
            }

            var direction = new Vector2(targetPosition.Position.X - position.X, targetPosition.Position.Y - position.Y);
            var velocity = Vector2.Normalize(direction) * 4;

            movementComponent.Velocity = velocity;
            return BehaviourStatus.Running;
        }

        public static BehaviourStatus PickupTarget(BtContext context)
        {
            if (!context.Agent.HasComponent<TargetEntityComponent>())
            {
                return BehaviourStatus.Failed;
            }

            var targetEntityComp = context.Agent.GetComponent<TargetEntityComponent>()!;
            var targetEntity = context.Engine.GetEntityById(targetEntityComp.TargetId);
            if (targetEntity == null)
            {
                return BehaviourStatus.Failed;
            }

            if (!targetEntity.HasComponent<LootableComponent>() || !targetEntity.HasComponent<ItemComponent>())
            {
                return BehaviourStatus.Failed;
            }

            var lootableComponent = targetEntity.GetComponent<LootableComponent>()!;
            var itemComponent = targetEntity.GetComponent<ItemComponent>()!;

            var quantity = lootableComponent.LootAll();

            var inventoryComponent = context.Agent.GetComponent<InventoryComponent>()!;
            inventoryComponent.Add(itemComponent.ItemType, quantity);

            var staminaCost = GetStaminaCost(itemComponent.ItemType);

            context.Agent.GetComponent<StaminaComponent>()!.ReduceBy(staminaCost);

            return BehaviourStatus.Succeeded;

        }

        private static int GetStaminaCost(ItemTypes itemType)
        {
            switch (itemType)
            {
                case ItemTypes.Axe: return 10;
                case ItemTypes.Pickaxe: return 10;
                case ItemTypes.Food: return 1;
                case ItemTypes.Wood: return 30;
                case ItemTypes.Stone: return 40;
                default: return 0;
            }
        }

        public static bool IsStaminaLow(BtContext context)
        {
            var staminaComponent = context.Agent.GetComponent<StaminaComponent>()!;
            return staminaComponent.Stamina < staminaComponent.MaxStamina / 3;
        }

        public static BehaviourStatus Rest(BtContext context)
        {
            // Stop moving
            var movementComponent = new MovementComponent { Velocity = Vector2.Zero };
            context.Agent.AddComponent(movementComponent);

            // Stay in Running until stamina recovers above threshold
            var staminaComponent = context.Agent.GetComponent<StaminaComponent>()!;
            if (staminaComponent.Stamina < staminaComponent.MaxStamina / 2)
            {
                return BehaviourStatus.Running;
            }

            return BehaviourStatus.Succeeded;
        }
    }
}

using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.Nodes;
using BehaviourTree.Demo.UI;
using System.Linq;
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
            var position = context.Agent.GetComponent<PositionComponent>()!;

            var lootableNode = context.Engine
                .GetNodes<ItemNode>()
                .Where(x => x.ItemComponent.ItemType == itemType)
                .OrderBy(x => Vector2.Distance(x.PositionComponent.Position, position.Position))
                .FirstOrDefault();

            if (lootableNode == null)
            {
                return BehaviourStatus.Failed;
            }

            var targetComponent = new TargetEntityComponent { TargetId = lootableNode.Entity.Id };

            context.Agent.AddComponent(targetComponent);

            return BehaviourStatus.Succeeded;
        }

        public static BehaviourStatus MoveToTargetEntity(BtContext context)
        {
            var movementComponent = context.Agent.GetComponent<MovementComponent>()!;

            if (!context.Agent.HasComponent<TargetEntityComponent>())
            {
                movementComponent.Velocity = Vector2.Zero;
                return BehaviourStatus.Failed;
            }

            if (!context.Agent.HasComponent<PositionComponent>() || !context.Agent.HasComponent<TargetEntityComponent>())
            {
                movementComponent.Velocity = Vector2.Zero;
                return BehaviourStatus.Failed;
            }

            var positionComponent = context.Agent.GetComponent<PositionComponent>()!.Value;
            var targetEntityComponent = context.Agent.GetComponent<TargetEntityComponent>()!.Value;

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

            var targetPosition = target.GetComponent<PositionComponent>()!.Value;
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

            var targetEntityComp = context.Agent.GetComponent<TargetEntityComponent>();
            if (!targetEntityComp.HasValue)
            {
                return BehaviourStatus.Failed;
            }

            var targetEntity = context.Engine.GetEntityById(targetEntityComp.Value.TargetId);
            if (targetEntity == null)
            {
                return BehaviourStatus.Failed;
            }

            var lootableComponent = targetEntity.GetComponent<LootableComponent>();
            var itemComponent = targetEntity.GetComponent<ItemComponent>();

            if (lootableComponent == null || !itemComponent.HasValue)
            {
                return BehaviourStatus.Failed;
            }

            var quantity = lootableComponent.LootAll();

            var inventoryComponent = context.Agent.GetComponent<InventoryComponent>()!;
            inventoryComponent.Add(itemComponent.Value.ItemType, quantity);

            var staminaCost = GetStaminaCost(itemComponent.Value.ItemType);

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
    }
}

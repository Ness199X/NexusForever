﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Command;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Network.Message.Model;

namespace NexusForever.WorldServer.Game.Entity
{
    public abstract class WorldEntity : GridEntity
    {
        public EntityType Type { get; }
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Dictionary<Stat, StatValue> Stats { get; } = new Dictionary<Stat, StatValue>();
        public Dictionary<Property, PropertyValue> Properties { get; } = new Dictionary<Property, PropertyValue>();
        public Dictionary<ItemSlot, VisibleItem> VisibleItems { get; } = new Dictionary<ItemSlot, VisibleItem>();

        public uint DisplayInfo { get; protected set; }
        public ushort OutfitInfo { get; protected set; }
        public ushort Faction1 { get; protected set; }
        public ushort Faction2 { get; protected set; }

        protected WorldEntity(EntityType type)
        {
            Type = type;
        }

        public override void Update(double lastTick)
        {
        }

        protected abstract IEntityModel BuildEntityModel();

        public virtual ServerEntityCreate BuildCreatePacket()
        {
            return new ServerEntityCreate
            {
                Guid      = Guid,
                Type      = Type,
                EntityModel    = BuildEntityModel(),
                Unknown60 = 1,
                Stats     = Stats.Values.ToList(),
                Commands =
                {
                    {
                        EntityCommand.SetPosition,
                        new SetPositionCommand
                        {
                            Position = new Position(Position)
                        }
                    },
                    {
                        EntityCommand.SetRotation,
                        new SetRotationCommand
                        {
                            Position = new Position(Rotation)
                        }
                    }
                },
                VisibleItems = VisibleItems.Values.ToList(),
                Properties   = Properties.Values.ToList(),
                Faction1    = 166,
                Faction2    = 166,
                DisplayInfo = DisplayInfo,
                OutfitInfo = OutfitInfo
            };
        }

        protected void SetProperty(Property property, float value, float baseValue = 0.0f)
        {
            if (Properties.ContainsKey(property))
                Properties[property].Value = value;
            else
                Properties.Add(property, new PropertyValue(property, baseValue, value));
        }

        protected float? GetPropertyValue(Property property)
        {
            return Properties.ContainsKey(property) ? Properties[property].Value : default;
        }

        /// <summary>
        /// Enqueue broadcast of <see cref="IWritable"/> to all visible <see cref="Player"/>'s in range.
        /// </summary>
        public void EnqueueToVisible(IWritable message)
        {
            foreach (WorldEntity entity in visibleEntities)
            {
                var player = entity as Player;
                if (player == null)
                    continue;

                if (player == this)
                    continue;

                player.Session.EnqueueMessageEncrypted(message);
            }       
        }
    }
}
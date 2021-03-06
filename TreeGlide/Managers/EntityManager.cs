﻿using System;
using System.Collections.Generic;
using System.Linq;


namespace TreeGlide
{
    public class EntityManager
    {
        private MemoryManager memoryManager;
        private Movement movement;
        private List<int> attackList;
        private Entity target;
        private IntPtr baseAddress;

        #region Offsets
        private struct Offsets
        {
            internal const int ENTITY_BASE_1 = 0x4;
            internal const int ENTITY_BASE_2 = 0x90;
            internal const int ENTITY_ID = 0x3DC;
            internal const int ENTITY_HEALTH = 0x428;
            internal const int ENTITY_X = 0x4E4;
            internal const int ENTITY_Y = 0x4EC;
            internal const int ENTITY_Z = 0x4E8;
        }
        #endregion

        public EntityManager(MemoryManager memoryManager, Movement movement)
        {
            this.memoryManager = memoryManager;
            this.movement = movement;
            this.attackList = new List<int>();
            this.baseAddress = memoryManager.GetAddressSigScan(@"\xa3\x00\x00\x00\x00\xc7\x45\x00\x00\x00\x00\x00\x68", "x????xx?????x", -0x7) + 0x14;
            //Console.WriteLine(this.baseAddress.ToString("X"));
            //\xa3\x00\x00\x00\x00\xc7\x45\x00\x00\x00\x00\x00\x68 x????xx?????x
        }

        public bool AttackListEmpty() => attackList.Count == 0;

        public Entity GetTarget(float distance)
        {
            this.target = NearestEntity(distance);
            return target ?? null;
        }

        public List<Entity> GetEntities()
        {
            List<Entity> entities = new List<Entity>();
            int[] offsetArr = { Offsets.ENTITY_BASE_1, Offsets.ENTITY_BASE_2, 0x0, 0x0 };
            for (int i = 0x0; i < 0x200; i += 0x4)
            {
                if (this.memoryManager.ReadValue<int>(this.baseAddress, new int[] { Offsets.ENTITY_BASE_1, Offsets.ENTITY_BASE_2, i }) == 0)
                    break;

                int entityType = this.memoryManager.ReadValue<int>(this.baseAddress, offsetArr);
                if (entityType == 19247100)
                    entities.Add(new Entity(this.baseAddress, i, memoryManager, movement));
                offsetArr[2] = i + 0x4;
            }
            return entities;
        }

        public List<Entity> GetEntitiesDistinct(List<int> idList)
        {
            List<Entity> entityList = GetEntities();
            foreach(Entity entity in entityList)
            {
                foreach(int id in idList)
                {
                    if (!idList.Contains(entity.id) || !entity.IsAlive())
                        entityList.Remove(entity);
                }
            }
            return entityList;
        }

        public void AddAttackList(int entityId) => this.attackList.Add(entityId);

        public void RemoveAttackList(int entityId) => this.attackList.Remove(entityId);

        public Entity NearestEntity(float distance)
        {
            Entity closest = null;
            double closestDistance = 99999;
            List<Entity> entityList = GetEntitiesDistinct(this.attackList);
            foreach(Entity entity in entityList)
            {
                entity.UpdateValues();
                double entityDistance = entity.GetDistance();
                if (entityDistance > distance)
                    continue;
                if (entityDistance < closestDistance)
                {
                    closest = entity;
                    closestDistance = entityDistance;
                }
            }
            return closest;
        }        
    }
}

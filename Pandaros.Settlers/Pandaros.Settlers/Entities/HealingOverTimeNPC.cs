﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Entities
{
    [ModLoader.ModManager]
    public class HealingOverTimeNPC
    {
        public static event EventHandler NewInstance;
        private static long _nextUpdate = 0;
        private static List<HealingOverTimeNPC> _instances = new List<HealingOverTimeNPC>();
        private static List<HealingOverTimeNPC> _toRemove = new List<HealingOverTimeNPC>();

        public NPC.NPCBase Target { get; private set; }

        public float TotalHoTTime { get; private set; }

        public float InitialHeal { get; private set; }

        public int DurationSeconds { get; private set; }

        public int TicksLeft { get; private set; }

        public float HealPerTic { get; private set; }

        public event EventHandler Complete;

        public event EventHandler Tick;

        public HealingOverTimeNPC(NPC.NPCBase nPC, float initialHeal, float totalHoT, int durationSeconds)
        {
            HealPerTic = totalHoT / durationSeconds;
            DurationSeconds = durationSeconds;
            InitialHeal = initialHeal;
            Target = nPC;
            TotalHoTTime = totalHoT;
            TicksLeft = durationSeconds;

            if (NewInstance != null)
                NewInstance(this, null);

            _instances.Add(this);
            Target.Heal(InitialHeal);
            Tick += HealingOverTimeNPC_Tick;
        }

        private void HealingOverTimeNPC_Tick(object sender, EventArgs e)
        {
            TicksLeft--;
            Target.Heal(HealPerTic);

            if (TicksLeft <= 0 && Complete != null)
                Complete(this, null);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Entities.HealingOverTimeNPC.Update")]
        public static void Update()
        {
            if (Pipliz.Time.MillisecondsSinceStart > _nextUpdate && _instances.Count > 0)
            {
                _toRemove.Clear();

                foreach (var healing in _instances)
                {
                    if (healing.Tick != null)
                        healing.Tick(healing, null);

                    if (healing.TicksLeft <= 0)
                        _toRemove.Add(healing);
                }

                foreach (var remove in _toRemove)
                    _instances.Remove(remove);

                _toRemove.Clear();

                _nextUpdate = Pipliz.Time.MillisecondsSinceStart + 1000;
            }
        }
    }
}

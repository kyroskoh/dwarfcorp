﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gum;

namespace DwarfCorp.Goals.Goals
{
    public class Ordu_Necro_Invasion : Goal
    {
        public Ordu_Necro_Invasion()
        {
            Name = "Ordu: Siding with the Elves";
            Description = "Uzzikal is enraged by your betrayal. Now you will feel his wrath.";
            GoalType = GoalTypes.UnavailableAtStartup;
        }

        public override ActivationResult Activate(WorldManager World)
        {
            // Spawn multiple war parties from Ordu

            return new ActivationResult { Succeeded = true };
        }

        public override void OnGameEvent(WorldManager World, GameEvent Event)
        {
            // If all war parties are killed..
            World.GoalManager.UnlockGoal(typeof(Ordu_Elf_Betrayal));
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EpicLoot.BaseEL.Adventure.Feature
{
    [Serializable]
    public class BountyKillLog
    {
        public string BountyID = "";
        public string MonsterID = "";
        public bool IsAdd = false;
    }

    [Serializable]
    public class BountyLedger
    {
        public long WorldID;
        public bool ConvertedGlobalKeys;
        public Dictionary<long, List<BountyKillLog>> KillLogsPerPlayer = new Dictionary<long, List<BountyKillLog>>();

        public List<BountyKillLog> GetAllKillLogs(long playerID)
        {
            if (!KillLogsPerPlayer.TryGetValue(playerID, out var logs))
            {
                logs = new List<BountyKillLog>();
                KillLogsPerPlayer.Add(playerID, logs);
            }

            return logs;
        }

        public List<BountyKillLog> GetKillLogForBounty(long playerID, string bountyID)
        {
            KillLogsPerPlayer.TryGetValue(playerID, out var killLog);
            return killLog?.Where(x => x.BountyID == bountyID).ToList() ?? new List<BountyKillLog>();
        }

        public void RemoveKillLogForBounty(long playerID, string bountyID)
        {
            KillLogsPerPlayer.TryGetValue(playerID, out var killLog);
            killLog?.RemoveAll(x => x.BountyID == bountyID);
        }

        public void RemoveKillLogsForPlayer(long playerID)
        {
            KillLogsPerPlayer.Remove(playerID);
        }

        public void AddKillLog(long playerID, string bountyID, string monsterID, bool isAdd)
        {
            if (!KillLogsPerPlayer.TryGetValue(playerID, out var list))
            {
                list = new List<BountyKillLog>();
                KillLogsPerPlayer.Add(playerID, list);
            }
            list.Add(new BountyKillLog { BountyID = bountyID, MonsterID = monsterID, IsAdd = isAdd });
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScheduleDetailsListSO", menuName = "Npc Schedule/ScheduleDetailsList")]
public class ScheduleDetailsListSO : ScriptableObject
{
    public List<ScheduleDetails> scheduleDetails;
}

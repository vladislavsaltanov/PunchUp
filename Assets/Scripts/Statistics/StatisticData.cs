[System.Serializable]
public class StatisticData
{
    public uint deaths;
    public uint kills;
    public uint items_picked;
    public uint flasks_picked;
    public uint floors_cleared;
    public uint total_playtime;

    public string cause_of_death;
    public uint floor_of_death;
    public uint run_number;

    public StatisticData() { }
    public StatisticData(StatisticData other)
    {
        deaths = other.deaths;
        kills = other.kills;
        items_picked = other.items_picked;
        flasks_picked = other.flasks_picked;
        floors_cleared = other.floors_cleared;
        total_playtime = other.total_playtime;
        cause_of_death = other.cause_of_death;
        floor_of_death = other.floor_of_death;
        run_number = other.run_number;
    }
}

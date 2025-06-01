public interface IStatisticsReporter
{
    void ReportAgentBorn();
    void ReportAgentDied(float age, string cause, int generation = 1);
    void ReportFoodCount(int count);
}
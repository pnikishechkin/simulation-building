using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationBuliding
{
  /// <summary>
  /// Состояния мастера
  /// </summary>
  public enum WorkerState
  {
    Free,
    ExcavatorRepair,
    BulldozerRepair
  }

  public class Worker
  {
    // Зарплата (руб в минуту)
    public decimal salaryPerMinute = 0;

    // Математическое ожидание времени ремонта экскаватора
    public int expectationDurationRepairExcavator = 0;

    // Математическое ожидание времени ремонта бульдозера
    public int expectationDurationRepairBulldozer = 0;


    // Текущее состояние мастера (обновляется в процессе моделирования)
    public WorkerState stateWorker = WorkerState.Free;

    // Продолжительности времени нахождения в разных состояниях за последний день моделирования (в минутах)
    public int modelTimeRepairExcavatorPerDay = 0;
    public int modelTimeRepairBulldozerPerDay = 0;
    public int modelTimeFreePerDay = 0;

    // Продолжительности времени нахождения в разных состояниях за весь период моделирования (в минутах)
    public int modelTimeRepairExcavatorPerAllPeriod = 0;
    public int modelTimeRepairBulldozerPerAllPeriod = 0;
    public int modelTimeFreePerAllPeriod = 0;

    public Worker()
    {

    }

    /// <summary>
    /// Определить зарплату за последний день моделирования
    /// </summary>
    /// <returns></returns>
    public decimal getSalaryPerDay()
    {
      return Math.Round(salaryPerMinute * (modelTimeRepairExcavatorPerDay + modelTimeRepairBulldozerPerDay), 2);
    }

    /// <summary>
    /// Определить зарплату за весь период моделирования
    /// </summary>
    /// <returns></returns>
    public decimal getSalaryPerAllPeriod()
    {
      return Math.Round(salaryPerMinute * (modelTimeRepairExcavatorPerAllPeriod + modelTimeRepairBulldozerPerAllPeriod), 2);
    }

    /// <summary>
    /// Сбросить все модельные данные за день
    /// </summary>
    public void ResetDataPerDay()
    {
      this.modelTimeRepairExcavatorPerDay = 0;
      this.modelTimeRepairBulldozerPerDay = 0;
      this.modelTimeFreePerDay = 0;
      this.stateWorker = WorkerState.Free;
    }

    /// <summary>
    /// Сбросить все данные моделирования
    /// </summary>
    public void ResetAllData()
    {
      this.ResetDataPerDay();

      this.modelTimeFreePerAllPeriod = 0;
      this.modelTimeRepairBulldozerPerAllPeriod = 0;
      this.modelTimeRepairExcavatorPerAllPeriod = 0;
    }

  }
}

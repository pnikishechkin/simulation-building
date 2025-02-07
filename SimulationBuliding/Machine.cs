using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationBuliding
{
  /// <summary>
  /// Состояния машин
  /// </summary>
  public enum MachineState
  {
    ReadyToWork,
    Working,
    WaitingRepair,
    Repair
  }

  /// <summary>
  /// Тип машин
  /// </summary>
  public enum MachineType
  {
    None,
    Excavator,
    Bulldozer
  }


  public class Machine
  {
    // Математическое ожидание времени работы машины
    public int expectationDurationWork = 0;

    // Тип машины
    public MachineType type = MachineType.None;

    // Текущее состояние машины (обновляется в процессе моделирования)
    public MachineState state = MachineState.ReadyToWork;

    // Время, до которого машина находится в работе (обновляется в процессе моделирования)
    public int workTime = 0;

    // Время, до которого машина находится в ремонте (обновляется в процессе моделирования)
    public int repairTime = 0;

    // Прибыль/убытки за минуту от работы/простоя
    public decimal profitPerMinute = 0;
    public decimal lossPerMinute = 0;

    // Продолжительности времени нахождения в разных состояниях за последний день моделирования (минуты)
    public int modelDurationWorkPerDay = 0;
    public int modelDurationWaitingPerDay = 0;
    public int modelDurationRepairPerDay = 0;

    // Продолжительности времени нахождения в разных состояниях за весь период моделирования (минуты)
    public int modelDurationWorkPerAllPeriod = 0;
    public int modelDurationWaitingPerAllPeriod = 0;
    public int modelDurationRepairPerAllPeriod = 0;

    /// <summary>
    /// Рассчитать доход от работы машины за день
    /// </summary>
    /// <returns></returns>
    public decimal getProfitPerDay()
    {
      return Math.Round(modelDurationWorkPerDay * profitPerMinute, 2);
    }

    /// <summary>
    /// Рассчитать убыток от работы машины за день
    /// </summary>
    /// <returns></returns>
    public decimal getLossPerDay()
    {
      return Math.Round((modelDurationWaitingPerDay + modelDurationRepairPerDay) * lossPerMinute, 2);
    }

    /// <summary>
    /// Рассчитать доход от работы машины за весь период моделирования
    /// </summary>
    /// <returns></returns>
    public decimal getProfitPerAllPeriod()
    {
      return Math.Round(modelDurationWorkPerAllPeriod * profitPerMinute, 2);
    }

    /// <summary>
    /// Рассчитать убыток от работы машины за весь период моделирования
    /// </summary>
    /// <returns></returns>
    public decimal getLossPerAllPeriod()
    {
      return Math.Round((modelDurationWaitingPerAllPeriod + modelDurationRepairPerAllPeriod) * lossPerMinute, 2);
    }

    /// <summary>
    /// Сбросить данные моделирования за день
    /// </summary>
    public void ResetDataPerDay()
    {
      this.state = MachineState.ReadyToWork;

      this.modelDurationWorkPerDay = 0;
      this.modelDurationWaitingPerDay = 0;
      this.modelDurationRepairPerDay = 0;
    }

    /// <summary>
    /// Сбросить данные моделирования за весь период
    /// </summary>
    public void ResetAllData()
    {
      this.ResetDataPerDay();

      this.modelDurationRepairPerAllPeriod = 0;
      this.modelDurationWaitingPerAllPeriod = 0;
      this.modelDurationWorkPerAllPeriod = 0;
    }
  }

}

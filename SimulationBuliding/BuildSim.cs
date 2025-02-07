using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimulationBuliding
{
  public partial class BuildSim : Form
  {
    // Параметры моделирования: длительность смены, длительность моделирования
    public int durationWorkChange = 16;
    public int durationModelingDay = 100;

    // Объекты с описанием параметров мастеров
    Worker worker3 = new Worker();
    Worker worker6 = new Worker();

    // Объекты с описанием параметров машин
    Machine excavator = new Machine();
    Machine bulldozer = new Machine();

    // Текущий день и минута моделирования
    public int modelDay = 0;
    public int modelMinute = 0;

    // Параметры для моделирования работы бригады мастеров (мастер 3 р + мастер 6 р)
    public bool bothMasterWorking = true;
    public int timeBothMasterWorkingPerDay = 0;
    public int timeBothMasterWorkingPerAllPeriod = 0;
    // Накладные расходы работы бригады
    public decimal costOverheadsWorkersPerMinute = 0;
    // Математические ожидания времени ремонта машин бригадой мастеров (мастер 3 р + мастер 6 р)
    int expValueRepairExcavatorWorkers, expValueRepairBulldozerWorkers;

    // Прибыль за весь период моделирования
    public decimal profitPerAllPeriod = 0;

    public BuildSim()
    {
      InitializeComponent();

      excavator.type = MachineType.Excavator;
      bulldozer.type = MachineType.Bulldozer;

      btnStartModeling.Enabled = true;
      btnStopModeling.Enabled = false;
      btnNextDay.Enabled = false;
      btnLastDay.Enabled = false;
    }

    #region Обработка событий формы

    /// <summary>
    /// Кнопка "Старт моделирования"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnStartModeling_Click(object sender, EventArgs e)
    {   
      // Элементы настроки параметров моделирования сделать недоступными
      groupBoxMachinesParams.Enabled = false;
      groupBoxWorkersParams.Enabled = false;
      groupBoxModelingParams.Enabled = false;

      // Заполнение переменных моделирования на основе значений, заданных пользователем на форме:

      // Если выбрано, что работают оба мастера, то устанавливаем переменную bothMasterWorking как true
      bothMasterWorking = radioButtonBothWorkers.Checked;

      // Математические ожидания работы экскаватора и бульдозера (в минутах)
      excavator.expectationDurationWork = (int)(numericUpDownExpWorkExcavator.Value * 60);
      bulldozer.expectationDurationWork = (int)(numericUpDownExpWorkBulldozer.Value * 60);

      // Математические ожидания ремонта экскаватора и бульдозера бригадой мастеров (в минутах)
      expValueRepairExcavatorWorkers = (int)(numericUpDownExpRepairExcavatorWorkers.Value * 60);
      expValueRepairBulldozerWorkers = (int)(numericUpDownExpRepairBulldozerWorkers.Value * 60);

      // Математические ожидания ремонта экскаватора и бульдозера по одному мастеру (в минутах)
      worker3.expectationDurationRepairExcavator = (int)(numericUpDownExpRepairExcavatorWorker3.Value * 60);
      worker6.expectationDurationRepairExcavator = (int)(numericUpDownExpRepairExcavatorWorker6.Value * 60);
      worker6.expectationDurationRepairBulldozer = (int)(numericUpDownExpRepairBulldozerWorker6.Value * 60);

      // Финансовые параметры (в мин)
      costOverheadsWorkersPerMinute = numericUpDownOverheadsWorkers.Value / 60;
      worker3.salaryPerMinute = numericUpDownSalaryWorker3.Value / 60;
      worker6.salaryPerMinute = numericUpDownSalaryWorker6.Value / 60;

      // Доходы от работы машин (в мин)
      excavator.profitPerMinute = numericUpDownExcProfit.Value / 60;
      bulldozer.profitPerMinute = numericUpDownBullProfit.Value / 60;

      // Убыток от простоя машин (в мин)
      excavator.lossPerMinute = numericUpDownExcLoss.Value / 60;
      bulldozer.lossPerMinute = numericUpDownBullLoss.Value / 60;

      // Время работы машин в сутки (по умолчанию 16 часов)
      durationWorkChange = (int)numericUpDownWorkingHours.Value;

      // Длительность моделирования
      durationModelingDay = (int)numericUpDownModelPeriod.Value;

      // Текущий день моделирования
      modelDay = 0;

      // Прибыль за весь период моделирования
      profitPerAllPeriod = 0;

      // Старт моделирования первого дня
      ModelingOneDay();

      btnStartModeling.Enabled = false;
      btnStopModeling.Enabled = true;
      btnNextDay.Enabled = true;
      btnLastDay.Enabled = true;
    }

    /// <summary>
    /// Кнопка "Остановить моделирование"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnStopModeling_Click(object sender, EventArgs e)
    {
      groupBoxMachinesParams.Enabled = true;
      groupBoxWorkersParams.Enabled = true;
      groupBoxModelingParams.Enabled = true;

      ResetAllModelingData();

      btnStartModeling.Enabled = true;
      btnStopModeling.Enabled = false;
      btnNextDay.Enabled = false;
      btnLastDay.Enabled = false;
    }

    /// <summary>
    /// Нажатие на кнопку "Следующий день"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnNextDay_Click(object sender, EventArgs e)
    {
      ModelingOneDay();
    }

    /// <summary>
    /// Нажатие на кнопку "Конец периода"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnFinishModeling_Click(object sender, EventArgs e)
    {
      for (int i = modelDay; i < durationModelingDay; i++)
      {
        ModelingOneDay();
      }
    }

    #endregion

    #region Основные функции для моделирования

    /// <summary>
    /// Моделирование одного дня работы системы
    /// </summary>
    private void ModelingOneDay()
    {
      ResetModelingDataPerDay();

      modelDay++;
      modelMinute = 0;

      // Очистка полей с результатами моделирования
      richTextBoxModelingLog.Clear();
      richTextBoxDayStatistic.Clear();
      richTextBoxResults.Clear();

      // Заполнение данных о текущем дне моделирования
      textBoxModelDay.Text = modelDay.ToString();

      // Моделирование всех минут одной смены (например, 16 часов * 60 = 960 мин)
      for (int i = 0; i < (durationWorkChange * 60); i++)
      {
        modelMinute = i;
        // Моделирование следующей минуты работы системы
        ModelingOneMinute();
      }

      // Обновление данных о моделировании (статистики)
      UpdateDayStatistics();
      UpdateResultStatistics();
    }


    /// <summary>
    /// Моделирование одной минуты работы всей системы
    /// </summary>
    private void ModelingOneMinute()
    {
      ModelingOneMinuteExcavator();
      ModelingOneMinuteBulldozer();

      // Накопительный учет состояния мастера 6 разряда
      switch (worker6.stateWorker)
      {
        case WorkerState.ExcavatorRepair:
          worker6.modelTimeRepairExcavatorPerDay++;
          worker6.modelTimeRepairExcavatorPerAllPeriod++;
          break;
        case WorkerState.BulldozerRepair:
          worker6.modelTimeRepairBulldozerPerDay++;
          worker6.modelTimeRepairBulldozerPerAllPeriod++;
          break;
        case WorkerState.Free:
          worker6.modelTimeFreePerDay++;
          worker6.modelTimeFreePerAllPeriod++;
          break;
      }

      // Если работают оба мастера
      if (bothMasterWorking)
      {
        // Накопительный учет состояния мастера 3 разряда
        switch (worker3.stateWorker)
        {
          case WorkerState.ExcavatorRepair:
            worker3.modelTimeRepairExcavatorPerDay++;
            worker3.modelTimeRepairExcavatorPerAllPeriod++;
            break;
          case WorkerState.BulldozerRepair:
            worker3.modelTimeRepairBulldozerPerDay++;
            worker3.modelTimeRepairBulldozerPerAllPeriod++;
            break;
          case WorkerState.Free:
            worker3.modelTimeFreePerDay++;
            worker3.modelTimeFreePerAllPeriod++;
            break;
        }

        // Если сразу оба мастера были заняты, считается дополнительно общее время их работы
        if (worker3.stateWorker != WorkerState.Free && worker6.stateWorker != WorkerState.Free)
        {
          timeBothMasterWorkingPerDay++;
          timeBothMasterWorkingPerAllPeriod++;
        }
      }
    }

    private void ModelingOneMinuteExcavator()
    {
      // Если состояние экскаватора "готов к работе", то рассчитывается время, когда машина выйдет из строя и устанавливается статус "в работе"
      if (excavator.state == MachineState.ReadyToWork)
      {
        excavator.workTime = modelMinute + GetRandomValueByExponential(excavator.expectationDurationWork);
        excavator.state = MachineState.Working;
        PrintStateMachine(excavator);
      }
      // Если наступило время ремонта, меняется состояние на ожидание ремонта (простой)
      else if (excavator.state == MachineState.Working && modelMinute == excavator.workTime)
      {
        excavator.state = MachineState.WaitingRepair;
        PrintStateMachine(excavator);
      }
      // Если экскаватор находится в ожидании ремонта
      if (excavator.state == MachineState.WaitingRepair)
      {
        // Условия для отправки экскаватора в ремонт:
        // 1. Работают оба мастера и свободны оба
        // 2. Работают оба мастера, но свободен только 3 разряда
        // 3. Работает один мастер 6 разряда и он свободен

        // Если работают оба мастера (бригада)
        if (bothMasterWorking)
        {
          // Если свободны оба мастера
          if (worker3.stateWorker == WorkerState.Free && worker6.stateWorker == WorkerState.Free)
          {
            // Устанавливаем состояние экскаватора "В ремонте"
            excavator.state = MachineState.Repair;
            // Состояние мастеров - "Ремонт экскаватора"
            worker3.stateWorker = WorkerState.ExcavatorRepair;
            worker6.stateWorker = WorkerState.ExcavatorRepair;

            // Расчет времени завершение ремонта экскаватора
            excavator.repairTime = modelMinute + GetRandomValueByExponential(expValueRepairExcavatorWorkers);
            PrintStateMachine(excavator);
          }
          // Если свободен только мастер 3 разряда
          else if (worker3.stateWorker == WorkerState.Free && worker6.stateWorker != WorkerState.Free)
          {
            // Устанавливаем состояние экскаватора "В ремонте"
            excavator.state = MachineState.Repair;
            // Состояние мастера 3 разр. - "Ремонт экскаватора"
            worker3.stateWorker = WorkerState.ExcavatorRepair;
            // Расчет времени завершение ремонта экскаватора
            excavator.repairTime = modelMinute + GetRandomValueByExponential(worker3.expectationDurationRepairExcavator);
            PrintStateMachine(excavator);
          }
        }
        // Работает только мастер 6 разряда
        else
        {
          if (worker6.stateWorker == WorkerState.Free)
          {
            // Устанавливаем состояние экскаватора "В ремонте"
            excavator.state = MachineState.Repair;
            // Состояние мастера 6 разр. - "Ремонт экскаватора"
            worker6.stateWorker = WorkerState.ExcavatorRepair;
            // Расчет времени завершение ремонта экскаватора
            excavator.repairTime = modelMinute + GetRandomValueByExponential(worker6.expectationDurationRepairExcavator);
            PrintStateMachine(excavator);
          }
        }
      }

      // Если подошло время завершения ремонта экскаватора
      if (excavator.state == MachineState.Repair && excavator.repairTime == modelMinute)
      {
        // Переводим экскаватор в рабочее состояние
        excavator.state = MachineState.Working;
        // Рассчитываем время до следующей поломки
        excavator.workTime = modelMinute + GetRandomValueByExponential(excavator.expectationDurationWork);

        PrintStateMachine(excavator);

        // Освобождаем мастера 3 разряда, если он занимался ремонтом экскаватора
        if (worker3.stateWorker == WorkerState.ExcavatorRepair)
          worker3.stateWorker = WorkerState.Free;

        // Освобождаем мастера 6 разряда, если он занимался ремонтом экскаватора
        if (worker6.stateWorker == WorkerState.ExcavatorRepair)
          worker6.stateWorker = WorkerState.Free;
      }

      // Накопительный учет состояния работы экскаватора
      switch (excavator.state)
      {
        case MachineState.Working:
          excavator.modelDurationWorkPerDay++;
          excavator.modelDurationWorkPerAllPeriod++;
          break;
        case MachineState.Repair:
          excavator.modelDurationRepairPerDay++;
          excavator.modelDurationRepairPerAllPeriod++;
          break;
        case MachineState.WaitingRepair:
          excavator.modelDurationWaitingPerDay++;
          excavator.modelDurationWaitingPerAllPeriod++;
          break;
      }
    }

    /// <summary>
    /// Моделирование одной минуты работы бульдозера
    /// </summary>
    private void ModelingOneMinuteBulldozer()
    {
      // Если состояние экскаватора "готов к работе", то рассчитывается время, когда машина выйдет из строя и устанавливается статус "в работе"
      if (bulldozer.state == MachineState.ReadyToWork)
      {
        bulldozer.workTime = modelMinute + GetRandomValueByExponential(bulldozer.expectationDurationWork); 
        bulldozer.state = MachineState.Working;
        PrintStateMachine(bulldozer);
      }
      // Если наступило время ремонта, меняется состояние на ожидание ремонта (простой)
      if (bulldozer.state == MachineState.Working && modelMinute == bulldozer.workTime)
      {
        bulldozer.state = MachineState.WaitingRepair;
        PrintStateMachine(bulldozer);
      }
      // Если бульдозер находится в ожидании ремонта
      if (bulldozer.state == MachineState.WaitingRepair)
      {
        // Условия для отправки бульдозера в ремонт:
        // 1. Работают оба мастера и свободны оба
        // 2. Работает один мастер 6 разряда и он свободен

        // Если работают оба мастера (бригада)
        if (bothMasterWorking)
        {
          // Если свободны оба мастера
          if (worker3.stateWorker == WorkerState.Free && worker6.stateWorker == WorkerState.Free)
          {
            bulldozer.state = MachineState.Repair;
            worker3.stateWorker = WorkerState.BulldozerRepair;
            worker6.stateWorker = WorkerState.BulldozerRepair;

            // Расчет времени завершения ремонта бульдозера
            bulldozer.repairTime = modelMinute + GetRandomValueByExponential(expValueRepairBulldozerWorkers);
            PrintStateMachine(bulldozer);
          }
        }
        // Если работает только мастер 6 разряда
        else
        {
          if (worker6.stateWorker == WorkerState.Free)
          {
            bulldozer.state = MachineState.Repair;
            worker6.stateWorker = WorkerState.BulldozerRepair;
            // Расчет времени завершения ремонта бульдозера
            bulldozer.repairTime = modelMinute + GetRandomValueByExponential(worker6.expectationDurationRepairBulldozer);
            PrintStateMachine(bulldozer);
          }
        }
      }

      // Если подошло время завершения ремонта экскаватора
      if (bulldozer.state == MachineState.Repair && bulldozer.repairTime == modelMinute)
      {
        // Переводим экскаватор в рабочее состояние
        bulldozer.state = MachineState.Working;
        // Рассчитываем время до следующей поломки
        bulldozer.workTime = modelMinute + GetRandomValueByExponential(bulldozer.expectationDurationWork);

        PrintStateMachine(bulldozer);

        // Освобождаем мастера 3 разряда, если он занимался ремонтом бульдозера
        if (worker3.stateWorker == WorkerState.BulldozerRepair)
          worker3.stateWorker = WorkerState.Free;

        // Освобождаем мастера 6 разряда, если он занимался ремонтом бульдозера
        if (worker6.stateWorker == WorkerState.BulldozerRepair)
          worker6.stateWorker = WorkerState.Free;
      }

      // Накопительный учет состояния работы бульдозера
      switch (bulldozer.state)
      {
        case MachineState.Working:
          bulldozer.modelDurationWorkPerDay++;
          bulldozer.modelDurationWorkPerAllPeriod++;
          break;
        case MachineState.Repair:
          bulldozer.modelDurationRepairPerDay++;
          bulldozer.modelDurationRepairPerAllPeriod++;
          break;
        case MachineState.WaitingRepair:
          bulldozer.modelDurationWaitingPerDay++;
          bulldozer.modelDurationWaitingPerAllPeriod++;
          break;
      }
    }


    /// <summary>
    /// Сбросить необходимые данные моделирования за день
    /// </summary>
    private void ResetModelingDataPerDay()
    {
      excavator.ResetDataPerDay();
      bulldozer.ResetDataPerDay();

      worker3.ResetDataPerDay();
      worker6.ResetDataPerDay();

      timeBothMasterWorkingPerDay = 0;
    }

    /// <summary>
    /// Сбросить все данные моделирования
    /// </summary>
    private void ResetAllModelingData()
    {
      ResetModelingDataPerDay();

      excavator.ResetAllData();
      bulldozer.ResetAllData();

      worker3.ResetAllData();
      worker6.ResetAllData();

      timeBothMasterWorkingPerAllPeriod = 0;
      profitPerAllPeriod = 0;
    }

    #endregion

    #region Функции для отображения результатов

    /// <summary>
    /// Добавить описание нового состояния машины в лог
    /// </summary>
    private void PrintStateMachine(Machine machine)
    {
      richTextBoxModelingLog.AppendText("Модельное время: " + TimeSpan.FromMinutes(modelMinute).ToString() + ".  ");

      richTextBoxModelingLog.AppendText("Состояние ");

      if (machine.type == MachineType.Bulldozer)
      {
        richTextBoxModelingLog.AppendText("бульдозера: ");
      }
      else if (machine.type == MachineType.Excavator)
      {
        richTextBoxModelingLog.AppendText("экскаватора: ");
      }

      switch (machine.state)
      {
        case MachineState.Working:
          richTextBoxModelingLog.AppendText("работа. Ожидаемое время поломки: ");
          richTextBoxModelingLog.AppendText(TimeSpan.FromMinutes(machine.workTime).ToString() + "\r\n");
          break;
        case MachineState.Repair:
          richTextBoxModelingLog.AppendText("поломка, начало ремонта. Время выхода с ремонта: ");
          richTextBoxModelingLog.AppendText(TimeSpan.FromMinutes(machine.repairTime).ToString() + "\r\n");
          break;
        case MachineState.WaitingRepair:
          richTextBoxModelingLog.AppendText("поломка, ожидание ремонта." + "\r\n");
          break;
      }
    }


    /// <summary>
    /// Вывести результаты за день моделирования
    /// </summary>
    private void UpdateDayStatistics()
    {
      richTextBoxDayStatistic.AppendText("-----------РАБОТА МАШИН-----------" + "\r\n");
      richTextBoxDayStatistic.AppendText("Продолжительность работы экскаватора за день: " + TimeSpan.FromMinutes(excavator.modelDurationWorkPerDay).ToString() + "\r\n");
      richTextBoxDayStatistic.AppendText("Продолжительность ожидания ремонта экскаватора за день: " + TimeSpan.FromMinutes(excavator.modelDurationWaitingPerDay).ToString() + "\r\n");
      richTextBoxDayStatistic.AppendText("Продолжительность ремонта экскаватора за день: " + TimeSpan.FromMinutes(excavator.modelDurationRepairPerDay).ToString() + "\r\n");
      richTextBoxDayStatistic.AppendText("\r\n");
      richTextBoxDayStatistic.AppendText("Продолжительность работы бульдозера за день: " + TimeSpan.FromMinutes(bulldozer.modelDurationWorkPerDay).ToString() + "\r\n");
      richTextBoxDayStatistic.AppendText("Продолжительность ожидания ремонта бульдозера за день: " + TimeSpan.FromMinutes(bulldozer.modelDurationWaitingPerDay).ToString() + "\r\n");
      richTextBoxDayStatistic.AppendText("Продолжительность ремонта бульдозера за день: " + TimeSpan.FromMinutes(bulldozer.modelDurationRepairPerDay).ToString() + "\r\n");

      richTextBoxDayStatistic.AppendText("\r\n-----------РАБОТА МАСТЕРОВ-----------" + "\r\n");

      if (bothMasterWorking)
      {
        richTextBoxDayStatistic.AppendText("Продолжительность работы мастера 3 разряда - ремонт экскаватора: " + TimeSpan.FromMinutes(worker3.modelTimeRepairExcavatorPerDay).ToString() + "\r\n");
        richTextBoxDayStatistic.AppendText("Продолжительность работы мастера 3 разряда - ремонт бульдозера: " + TimeSpan.FromMinutes(worker3.modelTimeRepairBulldozerPerDay).ToString() + "\r\n");
        richTextBoxDayStatistic.AppendText("Продолжительность простоя мастера 3 разряда: " + TimeSpan.FromMinutes(worker3.modelTimeFreePerDay).ToString() + "\r\n");
        richTextBoxDayStatistic.AppendText("\r\n");
      }

      richTextBoxDayStatistic.AppendText("Продолжительность работы мастера 6 разряда - ремонт экскаватора: " + TimeSpan.FromMinutes(worker6.modelTimeRepairExcavatorPerDay).ToString() + "\r\n");
      richTextBoxDayStatistic.AppendText("Продолжительность работы мастера 6 разряда - ремонт бульдозера: " + TimeSpan.FromMinutes(worker6.modelTimeRepairBulldozerPerDay).ToString() + "\r\n");
      richTextBoxDayStatistic.AppendText("Продолжительность простоя мастера 6 разряда: " + TimeSpan.FromMinutes(worker6.modelTimeFreePerDay).ToString() + "\r\n");

      richTextBoxDayStatistic.AppendText("\r\n");

      richTextBoxDayStatistic.AppendText("Продолжительность одновременной работы мастеров: " + TimeSpan.FromMinutes(timeBothMasterWorkingPerDay).ToString() + "\r\n");

      richTextBoxDayStatistic.AppendText("\r\n-----------ФИНАНСОВЫЕ ПОКАЗАТЕЛИ-----------" + "\r\n");

      richTextBoxDayStatistic.AppendText("Прибыль от работы экскаватора за день: " + excavator.getProfitPerDay().ToString() + " руб. \r\n");
      richTextBoxDayStatistic.AppendText("Убыток от простоя экскаватора за день: " + excavator.getLossPerDay().ToString() + " руб. \r\n");
      richTextBoxDayStatistic.AppendText("\r\n");
      richTextBoxDayStatistic.AppendText("Прибыль от работы бульдозера за день: " + bulldozer.getProfitPerDay().ToString() + " руб. \r\n");
      richTextBoxDayStatistic.AppendText("Убыток от простоя экскаватора за день: " + bulldozer.getLossPerDay().ToString() + " руб. \r\n");
      richTextBoxDayStatistic.AppendText("\r\n");
      if (bothMasterWorking)
      {
        richTextBoxDayStatistic.AppendText("Зарплата мастера 3 разряда: " + worker3.getSalaryPerDay().ToString() + " руб. \r\n");
      }
      richTextBoxDayStatistic.AppendText("Зарплата мастера 6 разряда: " + worker6.getSalaryPerDay().ToString() + " руб. \r\n");
      
      // Накладные расходы на бригаду в день (Время работы хотя бы одного из мастеров за день)
      decimal overheadsPerDay = Math.Round((worker6.modelTimeRepairBulldozerPerDay + worker6.modelTimeRepairExcavatorPerDay + worker3.modelTimeRepairExcavatorPerDay + worker3.modelTimeRepairBulldozerPerDay - timeBothMasterWorkingPerDay) * costOverheadsWorkersPerMinute, 2);
      richTextBoxDayStatistic.AppendText("Накладные расходы на работу мастеров: " + overheadsPerDay.ToString() + " руб. \r\n");
      richTextBoxDayStatistic.AppendText("\r\n");

      // М1 0-2-0
      // М2 0-2-3
      // Сумма - 7 ч
      // 2+2+3-2 = 5 ч

      // Доходы
      decimal proceeds = Math.Round(excavator.getProfitPerDay() + bulldozer.getProfitPerDay(), 2);
      // Расходы
      decimal expenses = Math.Round(excavator.getLossPerDay() + bulldozer.getLossPerDay() + worker3.getSalaryPerDay() + worker6.getSalaryPerDay() + overheadsPerDay, 2);

      richTextBoxDayStatistic.AppendText("Суммарные доходы за день: " + proceeds.ToString() + " руб. \r\n");
      richTextBoxDayStatistic.AppendText("Суммарные расходы за день: " + expenses.ToString() + " руб. \r\n");

      // Общая прибыль за день
      decimal profit = proceeds - expenses;
      profitPerAllPeriod += profit;
      richTextBoxDayStatistic.AppendText("Общая прибыль за день: " + profit.ToString() + " руб. \r\n");
    }

    /// <summary>
    /// Обновить общую статистику за весь период моделирования
    /// </summary>
    private void UpdateResultStatistics()
    {
      double avgDurationWorkExcavator = Math.Round((double)excavator.modelDurationWorkPerAllPeriod / modelDay, 1);
      double avgDurationWaitingExcavator = Math.Round((double)excavator.modelDurationWaitingPerAllPeriod / modelDay, 1);
      double avgDurationRepairExcavator = Math.Round((double)excavator.modelDurationRepairPerAllPeriod / modelDay, 1);

      double avgDurationWorkBulldozer = Math.Round((double)bulldozer.modelDurationWorkPerAllPeriod / modelDay, 1);
      double avgDurationWaitingBulldozer = Math.Round((double)bulldozer.modelDurationWaitingPerAllPeriod / modelDay, 1);
      double avgDurationRepairBulldozer = Math.Round((double)bulldozer.modelDurationRepairPerAllPeriod / modelDay, 1);

      richTextBoxResults.AppendText("-----------РАБОТА МАШИН-----------" + "\r\n");
      richTextBoxResults.AppendText("Средняя продолжительность работы экскаватора за день: " + TimeSpan.FromMinutes(avgDurationWorkExcavator).ToString() + "\r\n");
      richTextBoxResults.AppendText("Средняя продолжительность ожидания ремонта экскаватора за день: " + TimeSpan.FromMinutes(avgDurationWaitingExcavator).ToString() + "\r\n");
      richTextBoxResults.AppendText("Средняя продолжительность ремонта экскаватора за день: " + TimeSpan.FromMinutes(avgDurationRepairExcavator).ToString() + "\r\n");
      richTextBoxResults.AppendText("\r\n");
      richTextBoxResults.AppendText("Средняя продолжительность работы бульдозера за день: " + TimeSpan.FromMinutes(avgDurationWorkBulldozer).ToString() + "\r\n");
      richTextBoxResults.AppendText("Средняя продолжительность ожидания ремонта бульдозера за день: " + TimeSpan.FromMinutes(avgDurationWaitingBulldozer).ToString() + "\r\n");
      richTextBoxResults.AppendText("Средняя продолжительность ремонта бульдозера за день: " + TimeSpan.FromMinutes(avgDurationRepairBulldozer).ToString() + "\r\n");

      richTextBoxResults.AppendText("\r\n-----------ФИНАНСОВЫЕ ПОКАЗАТЕЛИ-----------" + "\r\n");

      richTextBoxResults.AppendText("Общая прибыль от работы экскаватора: " + excavator.getProfitPerAllPeriod().ToString() + " руб. \r\n");
      richTextBoxResults.AppendText("Общие убытки от простоя экскаватора: " + excavator.getLossPerAllPeriod().ToString() + " руб. \r\n");
      richTextBoxResults.AppendText("\r\n");
      richTextBoxResults.AppendText("Общая прибыль от работы бульдозера: " + bulldozer.getProfitPerAllPeriod().ToString() + " руб. \r\n");
      richTextBoxResults.AppendText("Общие убытки от простоя экскаватора: " + bulldozer.getLossPerAllPeriod().ToString() + " руб. \r\n");
      richTextBoxResults.AppendText("\r\n");

      richTextBoxResults.AppendText("Общая прибыль: " + profitPerAllPeriod.ToString() + " руб. \r\n");

    }

    #endregion

    #region Вспомогательные функции

    /// <summary>
    /// Кнопка "TestExponential"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void button1_Click(object sender, EventArgs e)
    {
      int value, sum = 0;
      richTextBoxTest.Clear();
      for (int i = 0; i < 100; i++)
      {
        value = this.GetRandomValueByExponential(5000);
        richTextBoxTest.AppendText(value.ToString() + "\r\n");
        sum += value;
      }
      richTextBoxTest.AppendText("---------------\r\n");
      richTextBoxTest.AppendText((sum / 100).ToString());
    }

    Random r = new Random();

    /// <summary>
    /// Сгенерировать случайную величину по экспоненциальному закону
    /// </summary>
    /// <param name="expectedValue">математическое ожидание</param>
    /// <returns></returns>
    private int GetRandomValueByExponential(int expectedValue)
    {
      double val = -expectedValue * Math.Log(r.NextDouble());
      return (int)val;
    }

    #endregion
  }
}


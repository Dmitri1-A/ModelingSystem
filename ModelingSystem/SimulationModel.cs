using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ModelingSystem
{
    public class SimulationModel : ICloneable
    {
        public enum StateChannel
        {
            Enabled,
            Disabled,
            Transfer,
            Broken,
        }

        private Dispatcher WindowDispatcher;
        private Action<SimulationModel> WindowsStateFunc;

        /// <summary>
        /// Время задержки запуска запасного канала
        /// </summary>
        public int T { get; set; }

        /// <summary>
        /// Время запуска запасного канала в имитационной модели
        /// </summary>
        private int tEnd;

        /// <summary>
        /// Время передачи сообщения по основному каналу
        /// </summary>
        public int T1 { get; set; }

        /// <summary>
        /// Время окончания передачи сообщения по основному каналу в имитационной модели
        /// </summary>
        private int t1End;

        /// <summary>
        /// Время задержки восстановления основного канала
        /// </summary>
        public int T2 { get; set; }

        /// <summary>
        /// Время окончания восстановления основного канала в имитационной модели
        /// </summary>
        private int t2End;

        /// <summary>
        /// Интервал времени, через который происходят сбои в основном канале
        /// </summary>
        public int T3 { get; set; }

        /// <summary>
        /// Время сбоя основного канала в имитационной модели
        /// </summary>
        private int t3End;

        /// <summary>
        /// Время поступления сообщения
        /// </summary>
        public int T4 { get; set; }

        /// <summary>
        /// Сообщение приходит в это время в имитационной модели
        /// </summary>
        private int t4End;

        /// <summary>
        /// Время передачи сообщения по запасному каналу
        /// </summary>
        public int T5 { get; set; }

        /// <summary>
        /// Время окончания передачи сообщения по запасному каналу в имитационной модели
        /// </summary>
        private int t5End;

        /// <summary>
        /// Емкость общего накопителя
        /// </summary>
        public int BufferCapacity { get; set; }

        public int BufferSize { get; set; }

        /// <summary>
        /// Количество прерванных сообщений
        /// </summary>
        public int CountMesIntercept { get; set; }

        /// <summary>
        /// Количество отброшенных сообщений при поступлении в буфер
        /// </summary>
        public int CountMesDiscarded { get; set; }

        /// <summary>
        /// Количество переданных сообщений
        /// </summary>
        public int CountMesTransferred { get; set; }

        /// <summary>
        /// Количество включений запасного канала
        /// </summary>
        public int CountInclusionReserveChannel { get; set; }

        /// <summary>
        /// Отображает приход сообщения в текущем состоянии
        /// </summary>
        public bool MessageWasIn { get; set; }

        public int TimeModel { get; set; }
        public int TimeEnd { get; set; }
        public int TimeStep { get; set; }
        public int TimeSpeed { get; set; }

        public StateChannel StateChannelMain { get; set; }
        public StateChannel StateChannelReserve { get; set; }

        public SimulationModel(int t = 0, int t1 = 0, int t2 = 0, int t3 = 0,
            int t4 = 0, int t5 = 0, int timeEnd = 3600, int timeStep = 1,
            int capacity = 4, Dispatcher dispatcher = null,
            int timeSpeed = 1, Action<SimulationModel> action = null)
        {
            WindowDispatcher = dispatcher;
            WindowsStateFunc = action;
            T = t;
            T1 = t1;
            T2 = t2;
            T3 = t3;
            T4 = t4;
            T5 = t5;
            BufferCapacity = capacity;
            BufferSize = 0;
            CountMesIntercept = 0;
            CountMesDiscarded = 0;
            CountInclusionReserveChannel = 0;
            TimeEnd = timeEnd;
            TimeStep = timeStep;
            TimeModel = 0;
            TimeSpeed = timeSpeed;
        }

        /// <summary>
        /// Код имитационной модели
        /// </summary>
        public void RunSimulation(CancellationToken token, ManualResetEvent resetEvent)
        {
            // Передает сообщение по запасному каналу, если буфер не пустой
            void actionReserveChannel()
            {
                if (BufferCapacity > 0)
                {
                    if (BufferSize > 0)
                    {
                        StateChannelReserve = StateChannel.Transfer;
                        BufferSize--;
                        t5End = TimeModel + T5;
                    }
                    else
                        StateChannelReserve = StateChannel.Enabled;
                }
                else
                {
                    if (MessageWasIn)
                    {
                        StateChannelReserve = StateChannel.Transfer;
                        t5End = TimeModel + T5;
                    }
                    else
                        StateChannelReserve = StateChannel.Enabled;
                }
            }

            void actionMainChannel()
            {
                if (BufferCapacity > 0)
                {
                    if (BufferSize > 0)
                    {
                        StateChannelMain = StateChannel.Transfer;
                        BufferSize--;
                        t1End = TimeModel + T1;
                    }
                    else
                        StateChannelMain = StateChannel.Enabled;
                }
                else
                {
                    if (MessageWasIn)
                    {
                        StateChannelMain = StateChannel.Transfer;
                        t1End = TimeModel + T1;
                    }
                    else
                        StateChannelMain = StateChannel.Enabled;
                }
            }

            t1End = T1;
            t2End = T2;
            t3End = T3;
            t4End = T4;
            t5End = T5;
            tEnd = T;

            StateChannelMain = StateChannel.Enabled;
            StateChannelReserve = StateChannel.Disabled;

            while (TimeModel < TimeEnd)
            {
                if (token.IsCancellationRequested)
                    return;

                resetEvent?.WaitOne();

                MessageWasIn = false;
                // Проверка прихода сообщения
                if (t4End <= TimeModel)
                {
                    MessageWasIn = true;

                    t4End = TimeModel + T4;
                    
                    if (StateChannel.Enabled == StateChannelMain)
                    {
                        StateChannelMain = StateChannel.Transfer;
                        t1End = TimeModel + T1;
                    }
                    else if (StateChannel.Enabled == StateChannelReserve)
                    {
                        StateChannelReserve = StateChannel.Transfer;
                        t5End = TimeModel + T5;
                    }
                    else if (BufferCapacity > 0)
                    {
                        if (BufferSize < BufferCapacity)
                        {
                            BufferSize++;
                        }
                        else
                            CountMesDiscarded++;
                    }
                    else
                        CountMesDiscarded++;
                }

                // Время выхода из строя основного канала?
                if ((StateChannel.Enabled == StateChannelMain ||
                    StateChannel.Transfer == StateChannelMain) &&
                    t3End <= TimeModel)
                {
                    if (StateChannel.Transfer == StateChannelMain)
                    {
                        if (BufferCapacity > 0)
                            BufferSize++;

                        CountMesIntercept++;
                    }

                    StateChannelMain = StateChannel.Broken;
                    t2End = TimeModel + T2;
                    tEnd = TimeModel + T;
                }
                // Время восстановления основного канала?
                else if (StateChannel.Broken == StateChannelMain && t2End <= TimeModel)
                {
                    t3End = TimeModel + T3;

                    actionMainChannel();
                }
                // Начать передачу по основному каналу, если он простаивает
                else if (StateChannel.Enabled == StateChannelMain)
                {
                    actionMainChannel();
                }
                // Основной канал передал сообщение?
                else if (StateChannel.Transfer == StateChannelMain && t1End <= TimeModel)
                {
                    CountMesTransferred++;

                    actionMainChannel();
                }

                // Запустить запасной канал, если он отключен и основной канал вышел из строя
                if (StateChannel.Disabled == StateChannelReserve && tEnd <= TimeModel)
                {
                    if (StateChannelMain == StateChannel.Broken)
                    {
                        CountInclusionReserveChannel++;

                        actionReserveChannel();
                    }
                    else
                        StateChannelReserve = StateChannel.Disabled;
                }
                // Начать передачу по запасному каналу, если он простаивает
                else if (StateChannel.Enabled == StateChannelReserve)
                {
                    if (StateChannelMain == StateChannel.Broken)
                    {
                        actionReserveChannel();
                    }
                    else
                        StateChannelReserve = StateChannel.Disabled;
                }
                // Запасной канал передал сообщение?
                else if (StateChannel.Transfer == StateChannelReserve && t5End <= TimeModel)
                {
                    CountMesTransferred++;
                    if (StateChannelMain == StateChannel.Broken)
                    {
                        actionReserveChannel();
                    }
                    else
                        StateChannelReserve = StateChannel.Disabled;
                }

                if (WindowDispatcher != null)
                {
                    if (!WindowDispatcher.CheckAccess())
                    {
                        WindowDispatcher.BeginInvoke(WindowsStateFunc, (SimulationModel)Clone());
                    }
                    else
                        WindowsStateFunc(this);
                }

                TimeModel += TimeStep;

                Thread.Sleep(TimeSpeed);

                if (token.IsCancellationRequested)
                    return;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}

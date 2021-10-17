﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ModelingSystem
{
    class SimulationModel
    {
        public enum StateChannel
        {
            Enabled,
            Disabled,
            Transfer,
            Broken,
        }

        public Dispatcher WindowDispatcher;
        public Action<SimulationModel> WindowsStateFunc;

        /// <summary>
        /// Время задержки запуска запасного канала
        /// </summary>
        public int T { get; set; }

        /// <summary>
        /// Время запуска запасного канала
        /// </summary>
        private int tEnd;

        /// <summary>
        /// Время передачи сообщения по основному каналу
        /// </summary>
        public int T1 { get; set; }

        /// <summary>
        /// Время окончания передачи сообщения по основному каналу
        /// </summary>
        private int t1End;

        /// <summary>
        /// Время задержки восстановления основного канала
        /// </summary>
        public int T2 { get; set; }

        /// <summary>
        /// Время окончания восстановления основного канала
        /// </summary>
        private int t2End;

        /// <summary>
        /// Интервал времени, через который происходят сбои в основном канале
        /// </summary>
        public int T3 { get; set; }

        /// <summary>
        /// Время сбоя основного канала
        /// </summary>
        private int t3End;

        /// <summary>
        /// Время поступления сообщения
        /// </summary>
        public int T4 { get; set; }

        /// <summary>
        /// Сообщение приходит в это время
        /// </summary>
        private int t4End;

        /// <summary>
        /// Время передачи сообщения по запасному каналу
        /// </summary>
        public int T5 { get; set; }

        /// <summary>
        /// Время окончания передачи сообщения по запасному каналу
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
        public int CountIntercept { get; set; }

        public int TimeModel { get; set; }
        public int TimeEnd { get; set; }
        public int TimeStep { get; set; }

        public int TimeSpeed { get; set; }

        public StateChannel StateChannelMain { get; set; }
        public StateChannel StateChannelReserve { get; set; }

        public SimulationModel(int t = 0, int t1 = 0, int t2 = 0, int t3 = 0,
            int t4 = 0, int t5 = 0, int timeEnd = 3600, int timeStep = 1,
            int capacity = 4, Dispatcher dispatcher = null,
            Action<SimulationModel> action = null)
        {
            T = t;
            T1 = t1;
            T2 = t2;
            T3 = t3;
            T4 = t4;
            T5 = t5;
            TimeEnd = timeEnd;
            TimeStep = timeStep;
            TimeModel = 0;
            BufferCapacity = capacity;
            WindowDispatcher = dispatcher;
            WindowsStateFunc = action;
        }

        public void ChangeStateChannelMain(StateChannel state)
        {
            StateChannelMain = state;

            // Увеличить количество прерванных сообщений, если основной канал
            // переходит из transfer в broken
            if ((StateChannel.Transfer == StateChannelMain) &&
                (StateChannel.Broken == state))
                CountIntercept++;

        }

        /// <summary>
        /// Код имитационной модели
        /// </summary>
        public void RunSimulation(CancellationToken token)
        {
            t1End = T1;
            t2End = T2;
            t3End = T3;
            t4End = T4;
            t5End = T5;
            tEnd = T;

            while (TimeModel < TimeEnd)
            {
                if (token.IsCancellationRequested)
                    return;

                TimeModel += TimeStep;

                // Проверка прихода сообщения
                if (t4End <= TimeModel)
                {
                    if (BufferCapacity > BufferSize)
                    {
                        BufferSize += 1;
                        t4End = TimeModel + T4;
                    }
                }

                // Время выхода из строя основного канала?
                if ((StateChannel.Enabled == StateChannelMain ||
                    StateChannel.Transfer == StateChannelMain) &&
                    t3End <= TimeModel)
                {
                    if (StateChannel.Transfer == StateChannelMain)
                        BufferSize++;

                    StateChannelMain = StateChannel.Broken;
                    t2End = TimeModel + T2;
                    tEnd = TimeModel + T;
                }

                // Время восстановления канала?
                if (StateChannel.Broken == StateChannelMain && t2End <= TimeModel)
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

                // Начать передачу по основному каналу, если он простаивает
                if (StateChannel.Enabled == StateChannelMain && BufferSize > 0)
                {
                    StateChannelMain = StateChannel.Transfer;
                    BufferSize--;
                    t1End = TimeModel + T1;
                }

                // Запустить запасной канал, если он отключен
                if (StateChannel.Disabled == StateChannelReserve && tEnd <= TimeModel)
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

                // Запустить запасной канал, если он простаивает
                if (StateChannel.Enabled == StateChannelReserve && BufferSize > 0)
                {
                    StateChannelReserve = StateChannel.Transfer;
                    BufferSize--;
                    t5End = TimeModel + T5;
                }

                // Основной канал передал сообщение?
                if (StateChannel.Transfer == StateChannelMain && t1End <= TimeModel)
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

                // Запасной канал передал сообщение?
                if (StateChannel.Transfer == StateChannelReserve && t1End <= TimeModel)
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

                if (!WindowDispatcher.CheckAccess())
                {
                    WindowDispatcher.BeginInvoke(WindowsStateFunc, this);
                }
                else
                    WindowsStateFunc(this);

                Thread.Sleep(11 - TimeSpeed);

                if (token.IsCancellationRequested)
                    return;
            }
        }
    }
}
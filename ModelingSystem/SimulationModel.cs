using System;
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

        public int T { get; set; }
        public int T1 { get; set; }
        public int T2 { get; set; }
        public int T3 { get; set; }
        public int T4 { get; set; }
        public int T5 { get; set; }

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
            //CancellationToken token = tokenSource.Token;
            token.ThrowIfCancellationRequested();

            if (token.IsCancellationRequested)
                return;

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(200);
                if (token.IsCancellationRequested)
                    return;
            }

            if (!WindowDispatcher.CheckAccess())
            {
                WindowDispatcher.BeginInvoke(WindowsStateFunc, this);
            }
            else
                WindowsStateFunc(this);
        }
    }
}

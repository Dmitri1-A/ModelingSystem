using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingSystem
{
    public class StateSimulationModel
    {
        public int TimeModel { get; set; }
        public SimulationModel.StateChannel StateChannelMain { get; set; }
        public SimulationModel.StateChannel StateChannelReserve { get; set; }
        public int CountMesIntercept { get; set; }
        public int CountInclusionReserveChannel { get; set; }

        public StateSimulationModel(int time, int countMes, int countInclusion,
            SimulationModel.StateChannel stateChannelMain,
            SimulationModel.StateChannel stateChannelReserve)
        {
            TimeModel = time;
            StateChannelMain = stateChannelMain;
            StateChannelReserve = stateChannelReserve;
            CountMesIntercept = countMes;
            CountInclusionReserveChannel = countInclusion;
        }
    }
}

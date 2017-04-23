using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using SourceMeterUnit;
using Keithley26xx;
using System.IO;

namespace FET_Characterization
{
	public class FET_IVModel : INotifyPropertyChanged
	{
		public FET_IVModel()
		{
			
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion

        private string keithleyRscName = "GPIB0::26::INSTR";
        public string KeithleyRscName
        {
            get { return keithleyRscName; }
            set
            {
                keithleyRscName = value;
                NotifyPropertyChanged("KeithleyRscName");
            }
        }
        
        private Keithley26xxB_Channels v_dsChannel = Keithley26xxB_Channels.Channel_A;
        public Keithley26xxB_Channels VdsChannel
        {
            get { return v_dsChannel; }
            set 
            {
                v_dsChannel = value;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        v_gChannel = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        v_gChannel = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                NotifyPropertyChanged("VdsChannel");
                NotifyPropertyChanged("VgChannel");
            }
        }

        private Keithley26xxB_Channels v_gChannel = Keithley26xxB_Channels.Channel_B;
        public Keithley26xxB_Channels VgChannel
        {
            get { return v_gChannel; }
            set
            {
                v_gChannel = value;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        v_dsChannel = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        v_dsChannel = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                NotifyPropertyChanged("VgChannel");
                NotifyPropertyChanged("VdsChannel");
            }
        }

        private SMUSourceMode smu_SourceMode = SMUSourceMode.Voltage;
        public SMUSourceMode SMU_SourceMode
        {
            get { return smu_SourceMode; }
            set 
            {
                smu_SourceMode = value;
                NotifyPropertyChanged("SMU_SourceMode");
            }
        }

        private double v_dsStart = 0.0;
        public double VdsStart
        {
            get { return v_dsStart; }
            set 
            {
                v_dsStart = value;
                NotifyPropertyChanged("VdsStart");
            }
        }

        private double v_dsStop = -1.0;
        public double VdsStop
        {
            get { return v_dsStop; }
            set
            {
                v_dsStop = value;
                NotifyPropertyChanged("VdsStop");
            }
        }

        private int n_v_dsSweep = 101;
        public int N_VdsSweep
        {
            get { return n_v_dsSweep; }
            set
            {
                n_v_dsSweep = value;
                NotifyPropertyChanged("N_VdsSweep");
            }
        }

        private double dsCompliance = 0.001;
        public double DS_Complaince
        {
            get { return dsCompliance; }
            set 
            {
                dsCompliance = value;
                NotifyPropertyChanged("DS_Complaince");
            }
        }

        private double v_gStart = 0.0;
        public double VgStart
        {
            get { return v_gStart; }
            set
            {
                v_gStart = value;
                NotifyPropertyChanged("VgStart");
            }
        }

        private double v_gStop = -5.0;
        public double VgStop
        {
            get { return v_gStop; }
            set
            {
                v_gStop = value;
                NotifyPropertyChanged("VgStop");
            }
        }

        private int n_v_gStep = 6;
        public int N_VgStep
        {
            get { return n_v_gStep; }
            set
            {
                n_v_gStep = value;
                NotifyPropertyChanged("N_VgStep");
            }
        }

        private double gateCompliance = 0.001;
        public double Gate_Complaince
        {
            get { return gateCompliance; }
            set
            {
                gateCompliance = value;
                NotifyPropertyChanged("Gate_Complaince");
            }
        }

        private double pulseWidth = 0.001;
        public double PulseWidth
        {
            get { return pulseWidth; }
            set
            {
                pulseWidth = value;
                NotifyPropertyChanged("PulseWidth");
            }
        }

        private double delayTime = 0.001;
        public double DelayTime
        {
            get { return delayTime; }
            set
            {
                delayTime = value;
                NotifyPropertyChanged("DelayTime");
            }
        }

        private string ivFET_FilePath = Directory.GetCurrentDirectory();
        public string IV_FET_DataFilePath
        {
            get { return ivFET_FilePath; }
            set 
            {
                ivFET_FilePath = value;
                NotifyPropertyChanged("IV_FET_DataFilePath");
            }
        }

        private string ivFileName = "Ch (1, 1) T #01 IV FG.dat";
        public string IV_FileName
        {
            get { return ivFileName; }
            set 
            {
                if (!value.EndsWith(".dat"))
                    value += ".dat";

                ivFileName = value;
                NotifyPropertyChanged("IV_FileName");
            }
        }

        private int ke_IV_FET_Averaging = 1;
        public int Ke_IV_FET_Averaging
        {
            get { return ke_IV_FET_Averaging; }
            set
            { 
                ke_IV_FET_Averaging = value;
                NotifyPropertyChanged("Ke_IV_FET_Averaging");
            }
        }

        private double ke_IV_FET_NPLC = 1.0;
        public double Ke_IV_FET_NPLC
        {
            get { return ke_IV_FET_NPLC; }
            set
            {
                if (value < 0.01)
                    value = 0.01;
                else if (value > 25)
                    value = 25;

                ke_IV_FET_NPLC = value;
                NotifyPropertyChanged("Ke_IV_FET_NPLC");
            }
        }

        private double iv_FET_GateDelay;
        public double IV_FET_GateDelay
        {
            get { return iv_FET_GateDelay; }
            set 
            {
                iv_FET_GateDelay = value;
                NotifyPropertyChanged("IV_FET_GateDelay");
            }
        }

        // Transfer implementation

        private string transferKeithleyRscName = "GPIB0::26::INSTR";
        public string TransferKeithleyRscName
        {
            get { return transferKeithleyRscName; }
            set
            {
                transferKeithleyRscName = value;
                NotifyPropertyChanged("TransferKeithleyRscName");
            }
        }

        private Keithley26xxB_Channels transfer_v_dsChannel = Keithley26xxB_Channels.Channel_A;
        public Keithley26xxB_Channels TransferVdsChannel
        {
            get { return transfer_v_dsChannel; }
            set
            {
                transfer_v_dsChannel = value;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        transfer_v_gChannel = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        transfer_v_gChannel = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                NotifyPropertyChanged("TransferVdsChannel");
                NotifyPropertyChanged("TransferVgChannel");
            }
        }

        private Keithley26xxB_Channels transfer_v_gChannel = Keithley26xxB_Channels.Channel_B;
        public Keithley26xxB_Channels TransferVgChannel
        {
            get { return transfer_v_gChannel; }
            set
            {
                transfer_v_gChannel = value;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        transfer_v_dsChannel = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        transfer_v_dsChannel = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                NotifyPropertyChanged("TransferVgChannel");
                NotifyPropertyChanged("TransferVdsChannel");
            }
        }

        private SMUSourceMode transfer_smu_SourceMode = SMUSourceMode.Voltage;
        public SMUSourceMode TransferSMU_SourceMode
        {
            get { return transfer_smu_SourceMode; }
            set
            {
                transfer_smu_SourceMode = value;
                NotifyPropertyChanged("TransferSMU_SourceMode");
            }
        }

        private double transfer_v_dsStart = 0.0;
        public double TransferVdsStart
        {
            get { return transfer_v_dsStart; }
            set
            {
                transfer_v_dsStart = value;
                NotifyPropertyChanged("TransferVdsStart");
            }
        }

        private double transfer_v_dsStop = -1.0;
        public double TransferVdsStop
        {
            get { return transfer_v_dsStop; }
            set
            {
                transfer_v_dsStop = value;
                NotifyPropertyChanged("TransferVdsStop");
            }
        }

        private int transfer_n_v_dsStep = 6;
        public int TransferN_VdsStep
        {
            get { return transfer_n_v_dsStep; }
            set
            {
                transfer_n_v_dsStep = value;
                NotifyPropertyChanged("TransferN_VdsStep");
            }
        }

        private double transfer_dsCompliance = 0.001;
        public double TransferDS_Complaince
        {
            get { return transfer_dsCompliance; }
            set
            {
                transfer_dsCompliance = value;
                NotifyPropertyChanged("TransferDS_Complaince");
            }
        }

        private double transfer_v_gStart = 0.0;
        public double TransferVgStart
        {
            get { return transfer_v_gStart; }
            set
            {
                transfer_v_gStart = value;
                NotifyPropertyChanged("TransferVgStart");
            }
        }

        private double transfer_v_gStop = -5.0;
        public double TransferVgStop
        {
            get { return transfer_v_gStop; }
            set
            {
                transfer_v_gStop = value;
                NotifyPropertyChanged("TransferVgStop");
            }
        }

        private int transfer_n_v_gSweep = 101;
        public int TransferN_VgSweep
        {
            get { return transfer_n_v_gSweep; }
            set
            {
                transfer_n_v_gSweep = value;
                NotifyPropertyChanged("TransferN_VgSweep");
            }
        }

        private double transfer_gateCompliance = 0.001;
        public double TransferGate_Complaince
        {
            get { return transfer_gateCompliance; }
            set
            {
                transfer_gateCompliance = value;
                NotifyPropertyChanged("TransferGate_Complaince");
            }
        }

        private double transfer_pulseWidth = 0.001;
        public double TransferPulseWidth
        {
            get { return transfer_pulseWidth; }
            set
            {
                transfer_pulseWidth = value;
                NotifyPropertyChanged("TransferPulseWidth");
            }
        }

        private double transfer_delayTime = 0.001;
        public double TransferDelayTime
        {
            get { return transfer_delayTime; }
            set
            {
                transfer_delayTime = value;
                NotifyPropertyChanged("TransferDelayTime");
            }
        }

        private string transferFilePath = Directory.GetCurrentDirectory();
        public string TransferDataFilePath
        {
            get { return transferFilePath; }
            set
            {
                transferFilePath = value;
                NotifyPropertyChanged("TransferDataFilePath");
            }
        }

        private string transferFileName = "Ch (1, 1) T #01 Transfer.dat";
        public string Transfer_FileName
        {
            get { return transferFileName; }
            set
            {
                if (!value.EndsWith(".dat"))
                    value += ".dat";

                transferFileName = value;
                NotifyPropertyChanged("Transfer_FileName");
            }
        }

        private int ke_Transfer_Averaging = 1;
        public int Ke_Transfer_Averaging
        {
            get { return ke_Transfer_Averaging; }
            set
            {
                ke_Transfer_Averaging = value;
                NotifyPropertyChanged("Ke_Transfer_Averaging");
            }
        }

        private double ke_Transfer_NPLC = 1.0;
        public double Ke_Transfer_NPLC
        {
            get { return ke_Transfer_NPLC; }
            set
            {
                if (value < 0.01)
                    value = 0.01;
                else if (value > 25)
                    value = 25;

                ke_Transfer_NPLC = value;
                NotifyPropertyChanged("Ke_Transfer_NPLC");
            }
        }

        private double transfer_VdsDelay;
        public double Transfer_VdsDelay
        {
            get { return transfer_VdsDelay; }
            set
            {
                transfer_VdsDelay = value;
                NotifyPropertyChanged("Transfer_VdsDelay");
            }
        }

        private bool measureLeakage = true;
        public bool MeasureLeakage
        {
            get { return measureLeakage; }
            set 
            {
                measureLeakage = value;
                NotifyPropertyChanged("MeasureLeakage");
            }
        }
	}
}
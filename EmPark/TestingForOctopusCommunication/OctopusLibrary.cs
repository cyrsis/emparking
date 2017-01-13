using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestingForOctopusCommunication
{
  

    public class OctopusLibrary
    {
        

        public struct DevVerRec
        {
            public int DevID;
            public int OperID;
            public int DevTime;
            public int CompID;
            public int KeyVer;
            public int EODVer;
            public int BLVer;
            public int FIRMVer;
            public int CCHSVer;
            public int LocID;
            public int IntBLVer;
            public int Reserved1;

            public override string ToString()
            {
                return
                    string.Format(
                        "DevId: {0}, OperId: {1}, DevTime: {2}, CompId: {3}, KeyVer: {4}, EodVer: {5}, BlVer: {6}, FirmVer: {7}, CchsVer: {8}, LocId: {9}, IntBlVer: {10}, Reserved1: {11}",
                        DevID, OperID, DevTime, CompID, KeyVer, EODVer, BLVer, FIRMVer, CCHSVer, LocID, IntBLVer,
                        Reserved1);
            }
        }

        [DllImport("rwl.dll", EntryPoint = "_InitComm@8")] //,CharSet = CharSet.None

        public static extern int InitComm(byte cPort, int cBaud);
        //int ,byte int


        [DllImport("rwl.dll", EntryPoint = "_TxnAmt@16")] //, CharSet = CharSet.Ansi
        public static extern int TxnAmt(int V, int RV, byte led, byte Sounds);




        [DllImport("rwl.dll", EntryPoint = "_TimeVer@4")]//,CharSet = CharSet.None
        public static extern int TimeVer(ref DevVerRec rec);



        [DllImport("rwl.dll", EntryPoint = "_Poll@12")]//,CharSet = CharSet.Ansi
        public static extern int Poll(byte subCom, byte TimeOut, StringBuilder PollData);
        //public static extern int Poll(byte subCom, byte TimeOut,[MarshalAs(UnmanagedType.VBByRefStr)] ref string PollData);
        //int ,byte ,byte ,not a ref stringbuilder
        //i=

        [DllImport("rwl.dll", EntryPoint = "_AddValue@12")]//,CharSet = CharSet.None
        public static extern int AddValue(int AddValueInTenCents, int AddValueType,char[] AdditionalTransActionInfo);
        // int,int,int,char[] 
        

        [DllImport("rwl.dll", EntryPoint = "_Reset@0")]//,CharSet = CharSet.None)
        public static extern int Rest();// This function performs software reset on the R/W 


        [DllImport("rwl.dll", EntryPoint = "_Deduct@8")] 
        public static extern int Deduct(int deductbyCents,byte[] AdditionalInformationForTransaction);
        //int, int, char[]

        


        //This function instructs the R/W to perform a deduction transaction on

        [DllImport("rwl.dll", EntryPoint = "_XFile@4")]//,CharSet = CharSet.None
        public static extern int XFile(StringBuilder XFileName);
            //int ,no string

        //This function instructs the R/W to perform a deduction transaction on

        [DllImport("rwl.dll", EntryPoint = "_PortClose@0")]//,CharSet = CharSet.None
        public static extern int PortClose();

        //This function closes the communication port and opened files 

        [DllImport("rwl.dll", EntryPoint = "_HouseKeeping@0")]//,CharSet = CharSet.None
        public static extern int HouseKeeping();

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);


    }

  
}

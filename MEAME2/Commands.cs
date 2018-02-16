using System;
using System.Linq;
using System.Collections.Generic;

namespace MEAME2
{

  [Serializable]
  public class DebugMessage
  {
    public string message { get; set; }
    public override string ToString()
    {
      return $"(DEBUG) {message}";
    }
	}


  [Serializable]
  public class DAQconfig {
    public int samplerate { get; set; }
    public int segmentLength { get; set; }
  }


  /**
     Should contain 4 lists, each sublist containing two sublists (uint to be precise,
     but view the uint as a List<bool> and it makes sense)

     Example (for the two first elements of top level list)
     SG: Stimulus group (see DSP code, basically a logical group of electrodes)

     +-------------------------------------+----------------------------------------+
     |stimSites[0]                         |  stimSites[1]                          |
     +-----------------+-------------------+--------------------+-------------------+
     |stimSites[0][0]  | stimSites[0][1]   |  stimSites[0][0]   | stimSites[0][1]   |
     +-----------------+-------------------+--------------------+-------------------+
     |SG0 electrode 0  | SG0 electrode 30  |  SG1  electrode 0  | SG1 electrode 30  |
     |SG0 electrode 1  | SG0 electrode 31  |  SG1  electrode 1  | SG1 electrode 31  |
     |SG0 electrode 2  | SG0 electrode 32  |  SG1  electrode 2  | SG1 electrode 32  |
     |...              | ...               |  ...               | ...               |
     |SG0 electrode 29 | SG0 electrode 59  |  SG1  electrode 29 | SG1 electrode 59  |
     +-----------------+-------------------+--------------------+-------------------+
   */
  [Serializable]
  public class DSPconfig {
    public List<List<uint>> stimSites { get; set; }
  }


  [Serializable]
  public class DspFuncCall {
    public uint func { get; set; }
    public uint[] argAddrs { get; set; }
    public uint[] argVals { get; set; }
  }


  [Serializable]
  public class RegReadRequest {
    public uint[] addresses { get; set; }

    public override string ToString(){
      String r = "Register read request with values:\n";
      for(int ii = 0; ii < addresses.Length; ii++){
        r = r + $"{addresses[ii]:X}\n";
      }
      return r;
    }
  }


  [Serializable]
  public class RegWriteRequest {
    public uint[] addresses { get; set; }
    public uint[] values { get; set; }

    public override string ToString(){
      String r = "Register write request with values:\n";
      for(int ii = 0; ii < addresses.Length; ii++){
        r = r + $"0x{addresses[ii]:X} <- 0x{values[ii]:X}\n";
      }
      return r;
    }
  }


  [Serializable]
  public class RegReadResponse {
    public uint[] addresses { get; set; }
    public uint[] values { get; set; }

    public override string ToString(){
      String r = "Register read response with values:\n";
      for(int ii = 0; ii < values.Length; ii++){
        r = r + $"{values[ii]:X}\n";
      }
      return r;
    }
  }
}

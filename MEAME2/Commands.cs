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


  [Serializable]
  public class MEAMEstatus {
    public bool isAlive { get; set; }
    public bool dspAlive { get; set; }
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

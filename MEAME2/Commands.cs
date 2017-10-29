using System;
using System.Linq;

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
  public class StimReq{
    public int[] electrodes { get; set; }
    public double[] stimFreqs { get; set; }
  }


  [Serializable]
  public class DAQconfig {
    public int samplerate { get; set; }
    public int segmentLength { get; set; }
  }


  [Serializable]
  public class RegSetRequest {
    public uint[] addresses { get; set; }
    public uint[] values { get; set; }
    public String desc { get; set; }
    public override string ToString(){
      String r = "Register set request with values:\n";
      for(int ii = 0; ii < values.Length; ii++){
        r = r + $"{addresses[ii]:X}\t<-{values[ii]:X}\n";
      }
      return r;
    }
  }


  [Serializable]
  public class RegReadRequest {
    public uint[] addresses { get; set; }
    public String desc { get; set; }
    public override string ToString(){
      String r = "Register read request with values:\n";
      for(int ii = 0; ii < addresses.Length; ii++){
        r = r + $"{addresses[ii]:X}\n";
      }
      return r;
    }
  }


  [Serializable]
  public class RegReadResponse {
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

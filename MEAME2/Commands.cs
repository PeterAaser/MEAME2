using System;
using System.Linq;

namespace MEAME2
{
  public class DebugMessage
  {
    public string message { get; set; }
    public override string ToString()
    {
      return $"(DEBUG) {message}";
    }
	}


  public class StimReq{
    public int[] electrodes { get; set; }
    public double[] stimFreqs { get; set; }
  }


  [Serializable]
  public class DAQconfig {
    public int samplerate { get; set; }
    public int segmentLength { get; set; }
  }
}

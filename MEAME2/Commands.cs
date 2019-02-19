using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MEAME2
{
  public class CommandSerializer
  {
    private static string commandsDir = "DspCommands/";


    abstract class JsonCreationConverter<T> : JsonConverter
    {
      protected abstract Type GetType(Type objectType, JObject jObject);


      public override bool CanConvert(Type objectType)
      {
        return typeof(T).IsAssignableFrom(objectType);
      }


      public override object ReadJson(JsonReader reader, Type objectType,
                                      object existingValue, JsonSerializer serializer)
      {
        JObject jObject = JObject.Load(reader);
        Type targetType = GetType(objectType, jObject);
        object target = Activator.CreateInstance(targetType);
        serializer.Populate(jObject.CreateReader(), target);
        return target;
      }


      public override void WriteJson(JsonWriter writer, Object value,
                                     JsonSerializer serializer)
      {
        throw new NotImplementedException();
      }
    }


    class DspInteractionConverter : JsonCreationConverter<DspInteraction>
    {
      protected override Type GetType(Type objectType, JObject jObject)
      {
        if (jObject["addresses"] != null)
        {
          return typeof(RegWriteRequest);
        }
        else if (jObject["func"] != null)
        {
          return typeof(DspFuncCall);
        }

        throw new ApplicationException(String.Format(
          "The given DspInteraction type {0} is not supported!", objectType));
      }
    }


    public static T fromJSONFile<T>(string fp)
    {
      TextReader r = null;
      Directory.CreateDirectory(commandsDir);
      fp = commandsDir + fp;

      try
      {
        r = new StreamReader(fp);
        var contents = r.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(contents, new DspInteractionConverter());
      }
      finally
      {
        if (r != null)
          r.Close();
      }
    }
  }


  public class DspInteraction
  {
  }


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
  public class DspFuncCall : DspInteraction {
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
  public class RegWriteRequest : DspInteraction {
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

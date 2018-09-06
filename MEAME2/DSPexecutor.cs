using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;
using static LanguageExt.List;
using Mcs.Usb;

namespace MEAME2
{

  public interface Executor {
    void execute_(DspOp[] ops);
    T execute<T>(DspOp<T> op);
  }

  public class LiveExecutor : Executor {

    public static CMcsUsbListNet usblist = new CMcsUsbListNet();
    public static CMcsUsbListEntryNet dspPort;
    public static CMcsUsbFactoryNet dspDevice;
    private static uint requestID = 0;
    public static bool connected = false;
    private static uint lockMask = 64;


    public static uint MAIL_BASE = 0x1000;

    public static uint SLAVE_INSTRUCTION_ID  = (MAIL_BASE + 0x0);
    public static uint MASTER_INSTRUCTION_ID = (MAIL_BASE + 0x4);
    public static uint INSTRUCTION_TYPE      = (MAIL_BASE + 0x8);

    public static uint STIM_BASE               = 0x9000;
    public static uint TRIGGER_CTRL_BASE       = 0x0200;


    public LiveExecutor()
    {
      log.info("LIVE DSP EXECUTOR CREATED");
      log.info("ALL DSP CALLS WILL BE INVOKED ON THE DSP");
      log.info("THE FORECAST IS LIGHT SYSTEM FAILURE WITH HIGH CHANCE OF PLEASE POWERCYCLE THE DEVICE");
      dspDevice = new CMcsUsbFactoryNet();
      dspDevice.EnableExceptions(true);
      usblist.Initialize(DeviceEnumNet.MCS_MEAUSB_DEVICE); // Get list of MEA devices connect by USB

      bool dspPortFound = false;
      uint lockMask = 64;

      for (uint ii = 0; ii < usblist.Count; ii++){
        if (usblist.GetUsbListEntry(ii).SerialNumber.EndsWith("B")){
          dspPort = usblist.GetUsbListEntry(ii);
          dspPortFound = true;
          break;
        }
      }

      if(dspPortFound && (dspDevice.Connect(dspPort, lockMask) == 0)){
        connected = true;
        dspDevice.Disconnect();
      }
      else {
        Console.WriteLine("Fug!");
      }
    }

    public class DspConnection : DspExecutor {

      public DspConnection() {
        log.msg("Connecting to dsp");
        if(dspDevice.Connect(dspPort, lockMask) != 0){
          log.err("DSP connection error");
        }
      }
      public void close() {
        log.err("Disconnecting from dsp");
        dspDevice.Disconnect();
      }

      public void write(uint address, uint word) {
        log.info($"live executor writing:\0x{address:X} <- {word:X}");
        dspDevice.WriteRegister(address, word);
      }

      public uint read(uint address) {
        var regWord = dspDevice.ReadRegister(address);
        log.info($"live executor reading: 0x{address:X} which contained 0x{regWord}");
        return regWord;
      }
    }

    public void execute_(DspOp[] ops){
      DspConnection conn = new DspConnection();
      for(int ii = 0; ii < ops.Length; ii++){
        ops[ii].run(conn);
      }
      conn.close();
    }

    public T execute<T>(DspOp<T> op){
      DspConnection conn = new DspConnection();
      var r = op.run(conn);
      conn.close();
      return r;
    }
  }


  public class MockExecutor : Executor {

    public MockExecutor(){
      log.info("MOCK EXECUTOR SELECTED");
      log.info("DSP CALLS WILL NOT BE INVOKED ON DSP");
    }

    public class DspConnection : DspExecutor {

      uint top = 0;

      public DspConnection(){
        log.msg("Mock connection created");
      }
      public void close(){
        log.err("Mock connection destroyed");
      }

      public void write(uint address, uint word) {
        log.info($"mock write 0x{word:X} to 0x{address:X}");
      }

      public uint read(uint address) {
        log.info($"mock read from 0x{address:X}");
        if(address == 0x1000){
          return top++;
        }
        return 0;
      }
    }

    public void execute_(DspOp[] ops){
      DspConnection conn = new DspConnection();
      for(int ii = 0; ii < ops.Length; ii++){
        ops[ii].run(conn);
      }
      conn.close();
    }

    public T execute<T>(DspOp<T> op){
      DspConnection conn = new DspConnection();
      var r = op.run(conn);
      conn.close();
      return r;
    }
  }


  public interface DspExecutor{
    void write(uint address, uint word);
    uint read(uint address);
  }

  public interface DspOp<T> {
    T run(DspExecutor ex);
  }

  public interface DspOp {
    void run(DspExecutor ex);
  }


  public class ReadOp : DspOp<uint[]> {
    public uint[] addresses { get; set; }

    public ReadOp(uint[] addresses){
      this.addresses = addresses;
    }

    public uint[] run(DspExecutor ex){
      uint[] ret = new uint[addresses.Length];
      for (int ii = 0; ii < addresses.Length; ii++){
        ret[ii] = ex.read(addresses[ii]);
      }
      return ret;
    }
  }

  public class WriteArgsOp : DspOp<uint> {
    public uint[] address { get; set; }
    public uint[] word { get; set; }

    public WriteArgsOp(uint[] addresses, uint[] words){
      address = addresses;
      word = words;
    }

    public uint run(DspExecutor ex){
      for (int ii = 0; ii < address.Length; ii++){
        ex.write(address[ii], word[ii]);
      }
      return 0;
    }
  }

  public class ReadNewestOp : DspOp<uint> {
    public uint run(DspExecutor ex){
      return ex.read(LiveExecutor.SLAVE_INSTRUCTION_ID);
    }
  }

  public class WriteFuncCall : DspOp<uint> {
    private uint call;
    public WriteFuncCall(uint call){
      this.call = call;
    }
    public uint run(DspExecutor ex){
      ex.write(LiveExecutor.INSTRUCTION_TYPE, call);
      return 0;
    }
  }


  public class CallDspFunc : DspOp<bool> {
    public WriteArgsOp args;
    public WriteFuncCall call;
    ReadNewestOp readNewest;

    public CallDspFunc(uint call, uint[] argAddresses, uint[] vals){
      readNewest = new ReadNewestOp();
      this.call = new WriteFuncCall(call);
      args = new WriteArgsOp(argAddresses, vals);

    }

    public bool run(DspExecutor ex){
      uint oldTop = readNewest.run(ex);
      args.run(ex);
      call.run(ex);
      ex.write(LiveExecutor.MASTER_INSTRUCTION_ID, oldTop + 1);
      uint newTop = readNewest.run(ex);
      if((oldTop + 1) == newTop){
        return true;
      }
      else {
        Thread.Sleep(100);
        newTop = readNewest.run(ex);
        if((oldTop + 1) == newTop){
          return true;
        }
        log.err($"dsp call failure. oldTop was {oldTop}, newTop was {newTop}");
        return false;
      }
    }
  }
}

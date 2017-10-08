using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MEAME2
{
  using Mcs.Usb;

  public static class ForEachExtensions
  {
    public static void ForEachWithIndex<T>(this IEnumerable<T> enumerable, Action<T, int> handler)
    {
      int idx = 0;
      foreach (T item in enumerable)
        handler(item, idx++);
    }
  }


  public partial class DSPComms {

    private bool DSPready = false;
    private uint readReqCounter = 0;
    private uint writeReqCounter = 0;
    Dictionary<uint, String> registers;
    Dictionary<uint, uint> registerValues;


    static uint MAIL_BASE = 0x1000;

    static uint WRITE_REQ_ID    = (MAIL_BASE + 0xc);
    static uint WRITE_ACK_ID    = (MAIL_BASE + 0x10);
    static uint WRITE_ADDRESS   = (MAIL_BASE + 0x14);
    static uint WRITE_VALUE     = (MAIL_BASE + 0x18);

    static uint READ_REQ_ID     = (MAIL_BASE + 0x1c);
    static uint READ_ACK_ID     = (MAIL_BASE + 0x20);
    static uint READ_ADDRESS    = (MAIL_BASE + 0x24);
    static uint READ_VALUE      = (MAIL_BASE + 0x28);

    static uint DEBUG1          = (MAIL_BASE + 0x2c);
    static uint DEBUG2          = (MAIL_BASE + 0x30);
    static uint DEBUG3          = (MAIL_BASE + 0x34);
    static uint DEBUG4          = (MAIL_BASE + 0x38);
    static uint DEBUG5          = (MAIL_BASE + 0x3c);
    static uint DEBUG6          = (MAIL_BASE + 0x40);
    static uint DEBUG7          = (MAIL_BASE + 0x44);
    static uint DEBUG8          = (MAIL_BASE + 0x48);
    static uint DEBUG9          = (MAIL_BASE + 0x4c);
    static uint WRITTEN_ADDRESS = (MAIL_BASE + 0x50);
    static uint COUNTER         = (MAIL_BASE + 0x54);
    static uint PING_SEND       = (MAIL_BASE + 0x58);
    static uint PING_READ       = (MAIL_BASE + 0x5c);

    static uint STIM_BASE = 0x9000;
    static uint TRIGGER_CTRL_BASE  = 0x0200;

    public void init(){
      Console.WriteLine("running init");
      this.registers = new Dictionary<uint, String>();


      registers.Add(STIM_BASE + 0x158, "Electrode enbale 1");
      registers.Add(STIM_BASE + 0x15c, "Electrode enbale 2");

      registers.Add(STIM_BASE + 0x120, "Electrode mode 1");
      registers.Add(STIM_BASE + 0x124, "Electrode mode 2");
      registers.Add(STIM_BASE + 0x128, "Electrode mode 3");
      registers.Add(STIM_BASE + 0x12c, "Electrode mode 4");

      registers.Add(STIM_BASE + 0x160, "Electrode dac select 1");
      registers.Add(STIM_BASE + 0x164, "Electrode dac select 2");
      registers.Add(STIM_BASE + 0x168, "Electrode dac select 3");
      registers.Add(STIM_BASE + 0x16c, "Electrode dac select 4");

      registers.Add(STIM_BASE + 0x190, "Trigger 1 repeat");
      registers.Add(STIM_BASE + 0x194, "Trigger 2 repeat");
      registers.Add(STIM_BASE + 0x198, "Trigger 3 repeat");

      for(int ii = 0; ii < 3; ii++){
        uint trigger_ctrl = (uint)(TRIGGER_CTRL_BASE + (ii*0x20));
        uint start_stim   = (uint)(TRIGGER_CTRL_BASE + (ii*0x20) + 0x4);
        uint end_stim     = (uint)(TRIGGER_CTRL_BASE + (ii*0x20) + 0x8);
        uint write_start  = (uint)(TRIGGER_CTRL_BASE + (ii*0x20) + 0xc);
        uint read_start   = (uint)(TRIGGER_CTRL_BASE + (ii*0x20) + 0x10);

        registers.Add(trigger_ctrl, $"trigger ctrl {ii+1}");
        registers.Add(start_stim,   $"start stim {ii+1}");
        registers.Add(end_stim,     $"end stim {ii+1}");
        registers.Add(write_start,  $"write start {ii+1}");
        registers.Add(read_start,   $"read start {ii+1}");
      }


      this.registerValues = new Dictionary<uint,uint>();
      foreach(var entry in registers){
        registerValues.Add(entry.Key, 0xDEAD);
      }

      DSPready = true;

      uploadAndTest();
    }

    public void writeReg(uint addr, uint val){
      Thread.Sleep(300);
      if(dspDevice.Connect(dspPort, lockMask) == 0)
        {
          dspDevice.WriteRegister(addr, val);
          dspDevice.Disconnect();
        }
      else{
        Console.WriteLine("Write failure");
      }
    }

    public uint readReg(uint addr){
      Thread.Sleep(300);
      if(dspDevice.Connect(dspPort, lockMask) == 0)
        {
          uint rval = dspDevice.ReadRegister(addr);
          dspDevice.Disconnect();
          return rval;
        }
      else{
        Console.WriteLine("read is Unable to connect to device");
        return 0xDEAD;
      }
    }

    private bool uploadAndTest(){

      Console.WriteLine("STARTING TESTS & UPLOAD --------\n\n");


      bool success = true;
      Console.WriteLine("Trying to upload firmware");
      success = (success && uploadMeameBinary());
      Console.WriteLine("----------------------\n\n");


      Console.WriteLine("Running ping test");
      success = (success && pingTest());
      Console.WriteLine("----------------------\n\n");

      Console.WriteLine("Running read test");
      success = (success && readAddressTest());
      Console.WriteLine("----------------------\n\n");

      return success;
    }


    public void readAllRegisters(){
      foreach(var entry in registers){
        // readRegister(entry.Key);
      }
    }


    // public bool readRegister(uint address){
    //   if(DSPready){
    //     DSPready = false;
    //     readReqCounter++;

    //     writeReg(READ_ADDRESS, address);
    //     writeReg(READ_REQ_ID, readReqCounter);

    //     bool done = false;
    //     for(int attempts = 0; attempts < 5; attempts++){
    //       uint ack = readReg(READ_ACK_ID);
    //       if(ack == readReqCounter){
    //         done = true;
    //         Console.WriteLine("Read Success");
    //         break;
    //       }
    //       else{
    //         Console.WriteLine("\nFailed to read, trying again");
    //         barfDebug();
    //       }
    //     }
    //     if(done){
    //       registerValues[address] = readReg(READ_VALUE);
    //     }
    //     else{
    //       Console.WriteLine("Failed to read register :(");
    //       return false;
    //     }
    //   }

    //   DSPready = true;
    //   return true;
    // }
    // else{
    //   Console.WriteLine("DSP not ready!!!!");
    //   return false;
    // }


    // public void writeRegister(uint address, uint value){
    //   if(DSPready){
    //     DSPready = false;
    //     writeReqCounter++;

    //     writeReg(WRITE_ADDRESS, address);
    //     writeReg(WRITE_VALUE, value);
    //     writeReg(WRITE_REQ_ID, writeReqCounter);
    //     bool done = false;
    //     for(int attempts = 0; attempts < 5; attempts++){
    //       if(readReg(READ_ACK_ID) == writeReqCounter){
    //         done = true;
    //         Console.WriteLine("Write Success");
    //         break;
    //       }
    //       else{
    //         Console.WriteLine("Failed to write, trying again");
    //       }
    //     }
    //     if(done){
    //       if(registers.ContainsKey(address)){
    //         registerValues[address] = value;
    //       }
    //     }
    //     else{
    //       Console.WriteLine("Failed to write register :(");
    //     }
    //   }
    //   else{
    //     Console.WriteLine("dsp not ready");
    //   }

    //   DSPready = true;
    // }


    public String prettyRegister(uint address){
      String desc = registers[address];
      uint value = registerValues[address];
      String valString = String.Format("[{0}]", value);
      String addString = String.Format("[{0:X}]", address);
      String hexString = String.Format("[{0:X}]", value);

      return String.Format("{0, -10}{1, -10}0x{2, -10}{3}", addString, valString, hexString, desc);
    }


    public void generateReport(){
      String header = String.Format("\n{0, -10}{1, -10}{2, -12}DESCRIPTION\n", "ADDRESS", "VALUE", "HEX VALUE");
      Console.WriteLine(header);
      if(registers == null){
        Console.WriteLine("oh boy it's another null...");
      }
      foreach(var entry in registers){
        Console.WriteLine(prettyRegister(entry.Key));
      }
    }


    public bool pingTest(){
      Console.WriteLine("TESTING BASIC READ AND WRITE CONNECTIVITY");
      Random rnd = new Random();
      uint rval1 = 0x123 + (uint)(rnd.Next(1,10));
      uint rval2 = 0x123 + (uint)(rnd.Next(1,10));
      uint rval3 = 0x123 + (uint)(rnd.Next(1,10));
      uint rval4 = 0x123 + (uint)(rnd.Next(1,10));

      uint cnt1;
      uint cnt2;
      uint cnt3;
      uint cnt4;

      writeReg(DEBUG1, rval1);
      cnt1 = readReg(COUNTER);
      writeReg(DEBUG2, rval2);
      cnt2 = readReg(COUNTER);
      writeReg(DEBUG3, rval3);
      cnt3 = readReg(COUNTER);
      writeReg(DEBUG4, rval4);
      cnt4 = readReg(COUNTER);

      uint test1 = readReg(DEBUG1);
      uint test2 = readReg(DEBUG2);
      uint test3 = readReg(DEBUG3);
      uint test4 = readReg(DEBUG4);


      if((test1 == rval1) && (test2 == rval2) && (test3 == rval3) && (test4 == rval4)){
        Console.WriteLine("Ping test success");
        return true;
      }
      else{
        Console.WriteLine("!!!! Ping test failed, device is broken again !!!!");
        Console.WriteLine("This is what they call german \"\"\"engineering\"\"\"");
        Console.WriteLine("Here's some data");
        Console.WriteLine(test1);
        Console.WriteLine(test2);
        Console.WriteLine(test3);
        Console.WriteLine(test4);

        Console.WriteLine(cnt1);
        Console.WriteLine(cnt2);
        Console.WriteLine(cnt3);
        Console.WriteLine(cnt4);
        dspDevice.Disconnect();
        return false;
      }


      Console.WriteLine("\nTESTING BASIC INTERRUPT HANDLING");
      uint pingTest1 = 0x1234 + (uint)(rnd.Next(1, 100));
      uint pingTest2 = 0x1234 + (uint)(rnd.Next(1, 100));
      uint pingTest3 = 0x1234 + (uint)(rnd.Next(1, 100));
      uint pingTest4 = 0x1234 + (uint)(rnd.Next(1, 100));

      writeReg(PING_SEND, pingTest1);
      uint pingRecv1 = readReg(PING_READ);

      writeReg(PING_SEND, pingTest2);
      uint pingRecv2 = readReg(PING_READ);

      writeReg(PING_SEND, pingTest3);
      uint pingRecv3 = readReg(PING_READ);

      writeReg(PING_SEND, pingTest4);
      uint pingRecv4 = readReg(PING_READ);

      if(
         (pingTest1 == pingRecv1) &&
         (pingTest2 == pingRecv2) &&
         (pingTest3 == pingRecv3) &&
         (pingTest4 == pingRecv4))

        {
          Console.WriteLine("Basic interrupting working");
        } else
        {
          Console.WriteLine("!!!! Interrupt handler test error !!!!");
          Console.WriteLine("This is what they call german \"\"\"engineering\"\"\"");
          Console.WriteLine("Here's some data");

          Console.WriteLine($"send: 0x{pingTest1:X}, read: 0x{pingRecv1:X}");
          Console.WriteLine($"send: 0x{pingTest2:X}, read: 0x{pingRecv2:X}");
          Console.WriteLine($"send: 0x{pingTest3:X}, read: 0x{pingRecv3:X}");
          Console.WriteLine($"send: 0x{pingTest4:X}, read: 0x{pingRecv4:X}");
          return false;
        }
    }


    public bool readAddressTest(){
      uint fail = readReg(READ_VALUE);
      writeReg(READ_ADDRESS, DEBUG1);
      writeReg(READ_REQ_ID, 0x12);
      uint fug = readReg(DEBUG1);
      uint ads = readReg(DEBUG2);
      uint mem = readReg(READ_ACK_ID);
      uint mem2 = readReg(READ_VALUE);

      Console.WriteLine($"DEBUG1: 0x{fug:X}, DEBUG2: 0x{ads:X}, READ_ACK_ID: 0x{mem:X}, READ_VALUE: 0x{mem2:X}");
      Console.WriteLine($"old READ_VALUE: 0x{fail:X}");
      Console.WriteLine("Proceeding...");

      barfDebug();
      return true;
    }


    public void barfDebug(){
      for(int ii = 0; ii < 10; ii++){
        uint addr = (uint)(DEBUG1 + ii*4);
        uint val = readReg(addr);
        Console.WriteLine($"DEBUG{ii+1} = {val}, - 0x{val:X}");
      }
      uint val_ = readReg(WRITTEN_ADDRESS);
      Console.WriteLine($"WRITTEN = {val_}, - 0x{val_:X}");
      val_ = readReg(COUNTER);
      Console.WriteLine($"COUNTER = {val_}, - 0x{val_:X}");
    }


    public bool connectivityTest(){
      if(dspDevice.Connect(dspPort, lockMask) == 0)
        {
          Console.WriteLine("Connect to device test successful");
          dspDevice.Disconnect();
          return true;
        }
      else{
        Console.WriteLine("write is unable to connect to device");
        return false;
      }
    }
  }
}

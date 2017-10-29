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

    public bool writeReg(uint addr, uint val){
      Thread.Sleep(500);
      if(dspDevice.Connect(dspPort, lockMask) == 0)
        {
          dspDevice.WriteRegister(addr, val);
          dspDevice.Disconnect();
          Thread.Sleep(500);
          return true;
        }
      else{
        consoleError("Write unable to connect to device");
        return false;
      }
    }

    public uint readReg(uint addr){
      Thread.Sleep(500);
      if(dspDevice.Connect(dspPort, lockMask) == 0)
        {
          uint rval = dspDevice.ReadRegister(addr);
          dspDevice.Disconnect();
          Thread.Sleep(500);
          return rval;
        }
      else{
        consoleError("read is Unable to connect to device");
        return 0xDEAD;
      }
    }


    // Lol no monads
    private bool uploadAndTest(){

      consoleInfo("STARTING TESTS & UPLOAD");

      bool success = true;
      consoleInfo("uploading DSP firmware");
      success = (success && uploadMeameBinary());

      success = (success && test());
      return success;
    }


    // Lol no monads
    public bool test(){
      return pingTest();
    }


    public bool pingTest(){
      Thread.Sleep(200);
      consoleInfo("\n\nTesting basic read and write connectivity");
      Random rnd = new Random();
      uint rval1 = 0x123 + (uint)(rnd.Next(1,10));
      uint rval2 = 0x123 + (uint)(rnd.Next(1,10));
      uint rval3 = 0x123 + (uint)(rnd.Next(1,10));
      uint rval4 = 0x123 + (uint)(rnd.Next(1,10));

      uint cnt1;
      uint cnt2;
      uint cnt3;
      uint cnt4;

      consoleInfo("Writing to DEBUG1 through DEBUG4");

      writeReg(DEBUG1, rval1);
      cnt1 = readReg(COUNTER);
      writeReg(DEBUG2, rval2);
      cnt2 = readReg(COUNTER);
      writeReg(DEBUG3, rval3);
      cnt3 = readReg(COUNTER);
      writeReg(DEBUG4, rval4);
      cnt4 = readReg(COUNTER);

      consoleInfo($"DEBUG1 set to {rval1:X}");
      consoleInfo($"DEBUG2 set to {rval2:X}");
      consoleInfo($"DEBUG3 set to {rval3:X}");
      consoleInfo($"DEBUG4 set to {rval4:X}");

      uint test1 = readReg(DEBUG1);
      uint test2 = readReg(DEBUG2);
      uint test3 = readReg(DEBUG3);
      uint test4 = readReg(DEBUG4);

      consoleInfo($"Reading DEBUG1 through DEBUG4");

      consoleInfo($"DEBUG1 read as {test1:X}");
      consoleInfo($"DEBUG2 read as {test2:X}");
      consoleInfo($"DEBUG3 read as {test3:X}");
      consoleInfo($"DEBUG4 read as {test4:X}");

      if((test1 == rval1) && (test2 == rval2) && (test3 == rval3) && (test4 == rval4)){
        consoleOK("R/W test successful");
      }
      else{
        consoleError("!!!! Ping test failed, device is broken again !!!!");
        consoleError("This is what they call german \"\"\"engineering\"\"\"");
        return false;
      }


      consoleInfo("\n\nTESTING BASIC INTERRUPT HANDLING");
      uint pingTest1 = 0x1234 + (uint)(rnd.Next(1, 100));
      uint pingTest2 = 0x1234 + (uint)(rnd.Next(1, 100));
      uint pingTest3 = 0x1234 + (uint)(rnd.Next(1, 100));
      uint pingTest4 = 0x1234 + (uint)(rnd.Next(1, 100));


      consoleInfo($"Writing {pingTest1} to PING_SEND");
      writeReg(PING_SEND, pingTest1);
      uint pingRecv1 = readReg(PING_READ);
      if(pingRecv1 == pingTest1){ consoleOK($"PING_SEND contained {pingTest1:X} as expected"); }
      else{ consoleError($"PING_SEND contained unexpected value: {pingRecv1:X}"); }

      consoleInfo($"Writing {pingTest1} to PING_SEND");
      writeReg(PING_SEND, pingTest2);
      uint pingRecv2 = readReg(PING_READ);
      if(pingRecv2 == pingTest2){ consoleOK($"PING_SEND contained {pingTest2:X} as expected"); }
      else{ consoleError($"PING_SEND contained unexpected value: {pingRecv2:X}"); }

      consoleInfo($"Writing {pingTest1} to PING_SEND");
      writeReg(PING_SEND, pingTest3);
      uint pingRecv3 = readReg(PING_READ);
      if(pingRecv3 == pingTest3){ consoleOK($"PING_SEND contained {pingTest3:X} as expected"); }
      else{ consoleError($"PING_SEND contained unexpected value: {pingRecv3:X}"); }

      consoleInfo($"Writing {pingTest1} to PING_SEND");
      writeReg(PING_SEND, pingTest4);
      uint pingRecv4 = readReg(PING_READ);
      if(pingRecv4 == pingTest4){ consoleOK($"PING_SEND contained {pingTest4:X} as expected"); }
      else{ consoleError($"PING_SEND contained unexpected value: {pingRecv4:X}"); }

      if(
         (pingTest1 == pingRecv1) &&
         (pingTest2 == pingRecv2) &&
         (pingTest3 == pingRecv3) &&
         (pingTest4 == pingRecv4))
        {
          consoleOK("Basic interrupting working");
          return true;
        } else
        {
          consoleError("!!!! Interrupt handler test error !!!!");
          consoleError("This is what they call german \"\"\"engineering\"\"\"");
          return false;
        }
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


    private void consoleError(String s){
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine($"[DSP Error]: {s}");
      Console.ResetColor();
    }

    private void consoleInfo(String s){
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine($"[DSP Info]: {s}");
      Console.ResetColor();
    }

    private void consoleOK(String s){
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"[DSP Info]: {s}\n\n");
      Console.ResetColor();
    }
  }
}

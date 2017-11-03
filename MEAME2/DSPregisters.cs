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

    enum DspOps : uint {READ=1, WRITE=2, DUMP=3, RESET=4};

    private bool DSPready = false;
    private uint readReqCounter = 0;
    private uint writeReqCounter = 0;

    private uint instructionIndex = 0;
    private uint instructionQsize = 10;
    private uint wordsPerInstruction = 3;


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

    static uint CLEAR           = (MAIL_BASE + 0x60);

    static uint COMMS_BUFFER_MASTER_IDX = (MAIL_BASE + 0x64); // MEAME -> DSP
    static uint COMMS_BUFFER_SLAVE_IDX  = (MAIL_BASE + 0x68); // DSP -> MEAME
    static uint COMMS_BUFFER_START      = (MAIL_BASE + 0x6c);

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

    private void nextCommsBuffer(){
      if(instructionIndex == (instructionQsize - 1))
        instructionIndex = 0;
      else
        instructionIndex++;

      log.info($"instruction index is now {instructionIndex}");
    }

    public void resetMail(){
      log.info("resetting mail");
      if(dspDevice.Connect(dspPort, lockMask) == 0)
        {

          nextCommsBuffer();

          uint opTypeAddress = COMMS_BUFFER_START + (instructionIndex*wordsPerInstruction) + 0x0;
          dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.RESET);

          bool success = false;

          for(int ii = 0; ii < 10; ii++){
            if(dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX) == 0x0){
              success = true;
              instructionIndex = 0;
              break;
            }
            Thread.Sleep(100);
          }

          if(success){
            log.ok("successfully reset device mailbox");
          }
          else{
            log.err("failed to reset device mailbox");
          }

          dspDevice.Disconnect();
        }
      else {
        log.err("failed to connect to device");
      }
    }

    public bool writeReg(uint addr, uint val){
      if(dspDevice.Connect(dspPort, lockMask) == 0)
        {

          nextCommsBuffer();

          uint opTypeAddress           = COMMS_BUFFER_START + ((4*instructionIndex)*wordsPerInstruction) + 0x0;
          uint writeAddressAddress     = COMMS_BUFFER_START + ((4*instructionIndex)*wordsPerInstruction) + 0x4;
          uint valueToBeWrittenAddress = COMMS_BUFFER_START + ((4*instructionIndex)*wordsPerInstruction) + 0x8;

          // addr val
          dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.WRITE);
          dspDevice.WriteRegister(writeAddressAddress, addr);
          dspDevice.WriteRegister(valueToBeWrittenAddress, val);
          dspDevice.WriteRegister(COMMS_BUFFER_MASTER_IDX, instructionIndex);

          var s = $"writing {(uint)DspOps.WRITE:X} to {opTypeAddress:X}\n" +
            $"writing {addr:X} to {writeAddressAddress:X}\n" +
            $"writing {val:X} to {valueToBeWrittenAddress:X}\n" +
            $"writing {instructionIndex:X} to {COMMS_BUFFER_MASTER_IDX:X}";

          log.info(s);

          bool success = false; // me_irl
          for(int ii = 0; ii < 10; ii++){
            uint tmp = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);
            if(tmp == instructionIndex){
              log.info($"the value of {tmp:X} was read from {COMMS_BUFFER_SLAVE_IDX:X} which matched {instructionIndex:X}");
              success = true;
              break;
            }
            Thread.Sleep(100);
            log.info($"read the value {tmp:X} from {COMMS_BUFFER_SLAVE_IDX:X} which did not match {instructionIndex:X}");
            tmp = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);
          }
          dspDevice.Disconnect();
          if(success){
            log.ok($"successfully wrote {val:X} to {addr:X}");
          }
          else{
            log.err("write failure");
          }
          return success;
        }
      else{
        log.err("Write unable to connect to device");
        return false;
      }
    }

    public uint readReg(uint addr){
      if(dspDevice.Connect(dspPort, lockMask) == 0)
        {

          nextCommsBuffer();

          uint opTypeAddress       = COMMS_BUFFER_START + (instructionIndex*4*wordsPerInstruction) + 0x0;
          uint readAddressAddress  = COMMS_BUFFER_START + (instructionIndex*4*wordsPerInstruction) + 0x4;
          uint deviceResultAddress = readAddressAddress;

          dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.READ);
          dspDevice.WriteRegister(readAddressAddress, addr);
          dspDevice.WriteRegister(COMMS_BUFFER_MASTER_IDX, instructionIndex);

          var s = $"writing {(uint)DspOps.READ:X} to {opTypeAddress:X}\n" +
            $"writing {addr:X} to {readAddressAddress:X}\n" +
            $"writing instruction index {instructionIndex:X} to {COMMS_BUFFER_MASTER_IDX:X}";

          log.info(s);

          bool success = false; // me_irl
          uint rval = 0xDEAD;

          for(int ii = 0; ii < 10; ii++){
            uint tmp = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);
            if(tmp == instructionIndex){
              log.info($"the value of {tmp:X} was read from {COMMS_BUFFER_SLAVE_IDX:X} which matched {instructionIndex:X}");
              success = true;
              rval = dspDevice.ReadRegister(deviceResultAddress);
              break;
            }
            log.info($"read the value {tmp:X} from {COMMS_BUFFER_SLAVE_IDX:X} which did not match {instructionIndex:X}");
            Thread.Sleep(100);
          }
          dspDevice.Disconnect();
          if(success){
          log.ok($"successfully read {rval:X} from {addr:X}");
          }
          else{
            log.err("read failure");
          }

          dspDevice.Disconnect();
          return rval;
        }
      else{
        log.err("read is Unable to connect to device");
        return 0xDEAD;
      }
    }
  }
}

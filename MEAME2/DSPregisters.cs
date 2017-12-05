using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

    enum DspOps : uint {READ=1, WRITE=2, DUMP=3, RESET=4, STIMPACK=5, TICK_TEST=6};

    private bool DSPready = false;
    private uint readReqCounter = 0;
    private uint writeReqCounter = 0;

    private uint instructionIndex = 0;
    private uint instructionQsize = 10;
    private uint wordsPerInstruction = 3;

    private uint opTypeAddress = COMMS_BUFFER_START + 0x0;
    private uint op1Address = COMMS_BUFFER_START    + 0x4;
    private uint op2Address = COMMS_BUFFER_START    + 0x8;


    Dictionary<uint, String> registers;
    Dictionary<uint, uint> registerValues;


    static uint MAIL_BASE = 0x1000;

    static uint COMMS1                      = (MAIL_BASE + 0xc);
    static uint COMMS2                      = (MAIL_BASE + 0x10);
    static uint COMMS3                      = (MAIL_BASE + 0x14);
    static uint COMMS4                      = (MAIL_BASE + 0x18);

    static uint COMMS5                      = (MAIL_BASE + 0x1c);
    static uint COMMS6                      = (MAIL_BASE + 0x20);
    static uint ERROR                       = (MAIL_BASE + 0x24);
    static uint ERROR_VAL                   = (MAIL_BASE + 0x28);
    static uint ERROR_OP1                   = (MAIL_BASE + 0x2c);
    static uint ERROR_OP2                   = (MAIL_BASE + 0x30);

    static uint DEBUG1                      = (MAIL_BASE + 0x2c);
    static uint DEBUG2                      = (MAIL_BASE + 0x30);
    static uint DEBUG3                      = (MAIL_BASE + 0x34);
    static uint DEBUG4                      = (MAIL_BASE + 0x38);
    static uint DEBUG5                      = (MAIL_BASE + 0x3c);
    static uint DEBUG6                      = (MAIL_BASE + 0x40);
    static uint DEBUG7                      = (MAIL_BASE + 0x44);
    static uint DEBUG8                      = (MAIL_BASE + 0x48);
    static uint DEBUG9                      = (MAIL_BASE + 0x4c);
    static uint WRITTEN_ADDRESS             = (MAIL_BASE + 0x50);
    static uint COUNTER                     = (MAIL_BASE + 0x54);
    static uint PING_SEND                   = (MAIL_BASE + 0x58);
    static uint PING_READ                   = (MAIL_BASE + 0x5c);

    static uint CLEAR                       = (MAIL_BASE + 0x60);

    static uint COMMS_BUFFER_MASTER_IDX     = (MAIL_BASE + 0x64); // MEAME -> DSP
    static uint COMMS_BUFFER_SLAVE_IDX      = (MAIL_BASE + 0x68); // DSP -> MEAME
    static uint COMMS_BUFFER_START          = (MAIL_BASE + 0x6c);

    static uint COMMS_MSG_BASE              = (MAIL_BASE + 0x200);
    static uint COMMS_INSTRUCTIONS_EXECUTED = (COMMS_MSG_BASE + 0x0);
    static uint COMMS_LAST_OP_TYPE          = (COMMS_MSG_BASE + 0x4);
    static uint COMMS_LAST_OP_1             = (COMMS_MSG_BASE + 0x8);
    static uint COMMS_LAST_OP_2             = (COMMS_MSG_BASE + 0xc);
    static uint COMMS_LAST_ERROR            = (COMMS_MSG_BASE + 0x10);
    static uint COMMS_LAST_ERROR_VAL        = (COMMS_MSG_BASE + 0x14);

    static uint STIMPACK_MSG_BASE           = (MAIL_BASE + 0x300);
    static uint STIMPACK_GROUP_DUMPED_GROUP = (STIMPACK_MSG_BASE + 0x4);
    static uint STIMPACK_GROUP_DAC          = (STIMPACK_MSG_BASE + 0x8);
    static uint STIMPACK_GROUP_ELECTRODES0  = (STIMPACK_MSG_BASE + 0xc);
    static uint STIMPACK_GROUP_ELECTRODES1  = (STIMPACK_MSG_BASE + 0x10);
    static uint STIMPACK_GROUP_PERIOD       = (STIMPACK_MSG_BASE + 0x14);
    static uint STIMPACK_GROUP_TICK         = (STIMPACK_MSG_BASE + 0x18);
    static uint STIMPACK_GROUP_SAMPLE       = (STIMPACK_MSG_BASE + 0x1c);
    static uint STIMPACK_GROUP_FIRES        = (STIMPACK_MSG_BASE + 0x20);
    static uint STIMPACK_SAMPLE             = (STIMPACK_MSG_BASE + 0x24);
    static uint STIMPACK_PERIOD             = (STIMPACK_MSG_BASE + 0x28);
    static uint STIMPACK_ELECTRODES0        = (STIMPACK_MSG_BASE + 0x2c);
    static uint STIMPACK_ELECTRODES1        = (STIMPACK_MSG_BASE + 0x30);


    static uint STIM_BASE               = 0x9000;
    static uint TRIGGER_CTRL_BASE       = 0x0200;


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

      // uploadAndTest();
    }

    private void nextCommsBuffer(){
      if(instructionIndex == (instructionQsize - 1))
        instructionIndex = 0;
      else
        instructionIndex++;

      opTypeAddress = COMMS_BUFFER_START + ((4*instructionIndex)*wordsPerInstruction) + 0x0;
      op1Address = COMMS_BUFFER_START    + ((4*instructionIndex)*wordsPerInstruction) + 0x4;
      op2Address = COMMS_BUFFER_START    + ((4*instructionIndex)*wordsPerInstruction) + 0x8;

      // log.info($"instruction index is now {instructionIndex:X}");
      // log.info($"opTypeAddress is now     {opTypeAddress:X}");
      // log.info($"op1Address is now        {op1Address:X}");
      // log.info($"op1Address now           {op2Address:X}");
    }

    public void resetMail(){
      log.info("resetting mail");
      if(connect())
        {

          uint opTypeAddress = this.opTypeAddress;
          dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.RESET);

          bool success = false;

          for(int ii = 0; ii < 10; ii++){
            if(dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX) == 0x0){
              success = true;
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

          nextCommsBuffer();
          instructionIndex = 0;
          disconnect();
        }
      else {
        log.err("failed to connect to device");
      }
    }


    public bool writeReg(uint addr, uint val){
      if(connect())
        {
          dspDevice.WriteRegister(ERROR, 0x0);
          dspDevice.WriteRegister(ERROR_VAL, 0x0);

          uint opTypeAddress           = this.opTypeAddress;
          uint writeAddressAddress     = this.op1Address;
          uint valueToBeWrittenAddress = this.op2Address;

          // addr val
          dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.WRITE);
          dspDevice.WriteRegister(writeAddressAddress, addr);
          dspDevice.WriteRegister(valueToBeWrittenAddress, val);

          uint oldBufferIdx = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);
          uint dbgRead = dspDevice.ReadRegister(addr);

          nextCommsBuffer();

          dspDevice.WriteRegister(COMMS_BUFFER_MASTER_IDX, instructionIndex);
          bool success = checkRead();

          disconnect();

          if(success){
            // log.ok($"successfully wrote {val:X} to {addr:X}");
            // log.info($"debug val was {dbgRead}");
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
      if(connect())
        {
          // log.info("Performing read reg");

          uint opTypeAddress       = this.opTypeAddress;
          uint readAddressAddress  = this.op1Address;
          uint deviceResultAddress = this.op2Address;

          dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.READ);
          dspDevice.WriteRegister(readAddressAddress, addr);

          uint oldBufferIdx = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);
          uint returnAddress = readAddressAddress;

          nextCommsBuffer();

          dspDevice.WriteRegister(COMMS_BUFFER_MASTER_IDX, instructionIndex);

          uint rval = 0xDEAD;
          bool success = checkRead();

          rval = dspDevice.ReadRegister(deviceResultAddress);

          disconnect();

          if(success){
            // log.ok($"successfully read {rval:X} from {addr:X}");
          }
          else{
            log.err("read failure");
          }

          return rval;
        }
      else{
        log.err("read is Unable to connect to device");
        return 0xDEAD;
      }
    }

    public void tickTest(){
      var first = tickTest_();
      Thread.Sleep(1000);
      var second = tickTest_();
      log.ok($"One second apart: t1: {first}, t2: {second}, elapsed: {second - first}");
    }

    private uint tickTest_(){
      if(connect()){

        uint opTypeAddress       = this.opTypeAddress;
        uint readAddressAddress  = this.op1Address;
        uint deviceResultAddress = this.op2Address;

        dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.TICK_TEST);

        uint oldBufferIdx = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);
        uint returnAddress = readAddressAddress;

        nextCommsBuffer();

        dspDevice.WriteRegister(COMMS_BUFFER_MASTER_IDX, instructionIndex);

        uint rval = 0xDEAD;
        bool success = checkRead();

        rval = dspDevice.ReadRegister(deviceResultAddress);

        disconnect();

        if(success){
        }
        else{
          log.err("tick test read failure");
        }
        return rval;
      }

      else{
        log.err("read is Unable to connect to device");
        return 0xDEAD;
      }
    }


    private bool issueStim(uint stimgroup){
      if(connect())
        {
          dspDevice.WriteRegister(ERROR, 0x0);
          dspDevice.WriteRegister(ERROR_VAL, 0x0);

          uint opTypeAddress           = this.opTypeAddress;
          uint DAC                     = this.op1Address;

          // addr val
          dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.STIMPACK);
          dspDevice.WriteRegister(DAC, stimgroup);

          uint oldBufferIdx = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);

          nextCommsBuffer();

          dspDevice.WriteRegister(COMMS_BUFFER_MASTER_IDX, instructionIndex);
          bool success = checkRead();

          disconnect();

          if(success){
            // log.ok($"successfully wrote {val:X} to {addr:X}");
            // log.info($"debug val was {dbgRead}");
          }
          else{
            log.err("stim failure");
          }
          return success;
        }
      else{
        log.err("Write unable to connect to device");
        return false;
      }
    }


    private bool checkRead(){
      bool success = false;
      for(int ii = 0; ii < 10; ii++){
        uint tmp = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);
        if(tmp == instructionIndex){
          success = true;
          break;
        }
        Thread.Sleep(100);
        log.err($"retrying comms slave read");
      }
      uint err = dspDevice.ReadRegister(ERROR);
      if(err == 0x1){
        uint errVal = dspDevice.ReadRegister(ERROR_VAL);
        uint errOp1 = dspDevice.ReadRegister(ERROR_OP1);
        uint errOp2 = dspDevice.ReadRegister(ERROR_OP2);
        log.err($"DSP reports error {err} with error val {errVal:X}, {errOp1:X}, {errOp2:X}");
        return false;
      }

      return success;
    }
  }
}

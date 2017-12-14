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

    enum DspOps : uint {
      READ             = 1,
      WRITE            = 2,
      DUMP             = 3,
      RESET            = 4,
      STIMPACK         = 5,
      STIM_DEBUG       = 6
    };

    enum LogTags : uint {
      DAC_STATE_CHANGE = 1,
      CONF             = 2,
      CONF_RESET       = 3,
      CONF_START       = 4,
      TRIGGER          = 5,
      STATE_EN_ELEC    = 6,
      STATE_DAC_SEL    = 7,
      STATE_MODE       = 8,
      BOOKING          = 9,
      BOOKING_FOUND    = 12
    };

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

    static uint COMMS1    = (MAIL_BASE + 0xc);
    static uint COMMS2    = (MAIL_BASE + 0x10);
    static uint COMMS3    = (MAIL_BASE + 0x14);
    static uint COMMS4    = (MAIL_BASE + 0x18);

    static uint COMMS5    = (MAIL_BASE + 0x1c);
    static uint COMMS6    = (MAIL_BASE + 0x20);
    static uint ERROR     = (MAIL_BASE + 0x24);
    static uint ERROR_VAL = (MAIL_BASE + 0x28);
    static uint ERROR_OP1 = (MAIL_BASE + 0x2c);
    static uint ERROR_OP2 = (MAIL_BASE + 0x30);

    static uint ENTRIES   = (MAIL_BASE + 0x34);

    static uint DEBUG10   = (MAIL_BASE + 0x400);
    static uint DEBUG11   = (MAIL_BASE + 0x404);
    static uint DEBUG12   = (MAIL_BASE + 0x408);
    static uint DEBUG13   = (MAIL_BASE + 0x40c);
    static uint DEBUG14   = (MAIL_BASE + 0x410);
    static uint DEBUG15   = (MAIL_BASE + 0x414);
    static uint DEBUG16   = (MAIL_BASE + 0x418);
    static uint DEBUG17   = (MAIL_BASE + 0x41c);
    static uint DEBUG18   = (MAIL_BASE + 0x420);
    static uint DEBUG19   = (MAIL_BASE + 0x424);

    static uint DEBUG20   = (MAIL_BASE + 0x428);
    static uint DEBUG21   = (MAIL_BASE + 0x42c);
    static uint DEBUG22   = (MAIL_BASE + 0x430);
    static uint DEBUG23   = (MAIL_BASE + 0x434);
    static uint DEBUG24   = (MAIL_BASE + 0x438);
    static uint DEBUG25   = (MAIL_BASE + 0x43c);
    static uint DEBUG26   = (MAIL_BASE + 0x440);
    static uint DEBUG27   = (MAIL_BASE + 0x444);
    static uint DEBUG28   = (MAIL_BASE + 0x448);
    static uint DEBUG29   = (MAIL_BASE + 0x44c);

    static uint DEBUG30   = (MAIL_BASE + 0x450);
    static uint DEBUG31   = (MAIL_BASE + 0x454);
    static uint DEBUG32   = (MAIL_BASE + 0x458);
    static uint DEBUG33   = (MAIL_BASE + 0x45c);
    static uint DEBUG34   = (MAIL_BASE + 0x460);
    static uint DEBUG35   = (MAIL_BASE + 0x464);
    static uint DEBUG36   = (MAIL_BASE + 0x468);
    static uint DEBUG37   = (MAIL_BASE + 0x46c);
    static uint DEBUG38   = (MAIL_BASE + 0x470);
    static uint DEBUG39   = (MAIL_BASE + 0x474);

    static uint DEBUG40   = (MAIL_BASE + 0x478);
    static uint DEBUG41   = (MAIL_BASE + 0x47c);
    static uint DEBUG42   = (MAIL_BASE + 0x480);
    static uint DEBUG43   = (MAIL_BASE + 0x484);
    static uint DEBUG44   = (MAIL_BASE + 0x488);
    static uint DEBUG45   = (MAIL_BASE + 0x48c);
    static uint DEBUG46   = (MAIL_BASE + 0x490);
    static uint DEBUG47   = (MAIL_BASE + 0x494);
    static uint DEBUG48   = (MAIL_BASE + 0x498);
    static uint DEBUG49   = (MAIL_BASE + 0x49c);

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

    static uint LOG_BASE                    = (MAIL_BASE + 0x600);

    static uint STIM_BASE               = 0x9000;
    static uint TRIGGER_CTRL_BASE       = 0x0200;

    public void resetDebug(){
      if(connect()){
        for(uint ii = 0; ii < 0xFFC; ii+=4){
          dspDevice.WriteRegister(MAIL_BASE + ii, 0x0);
        }
        disconnect();
      }
    }


    private string lookupId(uint id){
      string[] ids = {
        "0  - NOT USED        ",
        "1  - DAC_STATE_CHANGE",
        "2  - CONF            ",
        "3  - CONF_RESET      ",
        "4  - CONF_START      ",
        "5  - TRIGGER         ",
        "6  - STATE_EN_ELEC   ",
        "7  - STATE_DAC_SEL   ",
        "8  - STATE_MODE      ",
        "9  - BOOKING         ",
        "10 - MEAME2 STIM REQ ",
        "11 - COMMS GOT STIM REQ"
      };

      if(id > ids.Length){
        return $"out of bounds: {id}";
      }
      return ids[id];
    }

    // private void handleLogEntry(uint index){
    //   if(connect()){

    //     uint baseAddress = (index*4*4) + LOG_BASE;

    //     uint id1 =  dspDevice.ReadRegister(baseAddress + 0);
    //     uint id2 =  dspDevice.ReadRegister(baseAddress + 4);
    //     uint val1 = dspDevice.ReadRegister(baseAddress + 8);
    //     uint val2 = dspDevice.ReadRegister(baseAddress + 12);

    //     disconnect();

    //     log.info($"Log entry {index}");
    //     log.info($"id1:      {lookupId(id1)}");
    //     log.info($"id2:      {lookupId(id2)}");
    //     log.info($"value1:   {val1:X}");
    //     log.info($"timestep: {val2}\n");
    //   }
    // }


    // public void readLog(){
    //   uint entries = 0;
    //   if(connect()){
    //     entries = dspDevice.ReadRegister(ENTRIES);
    //     log.info($"reading {entries} log entries, or 1000, whatever is smaller:");
    //     if(entries > 1000){ entries = 1000; }
    //     log.info($"reading {entries} log entries:");
    //     disconnect();
    //   }

    //   for(uint ii = 0; ii < entries; ii++){
    //     handleLogEntry(ii);
    //   }
    // }

    public class LogEntry {
      public uint id1 { get; set; }
      public uint id2 { get; set; }
      public uint val1 { get; set; }
      public uint val2 { get; set; }
    }

    public class Log {
      public int idx { get; set; }
      public List<LogEntry> mlog { get; set; }
    }

    private Log getLogEntries(){
      uint entries = 0;
      if(connect()){
        entries = dspDevice.ReadRegister(ENTRIES);
        disconnect();
      }
      List<LogEntry> mlog = new List<LogEntry>();

      for(int ii = 0; ii < entries; ii++){
        addLogEntry(mlog, ii);
      }
      var hurr = new Log();
      hurr.mlog = mlog;
      hurr.idx = 0;

      return hurr;
    }


    private void addLogEntry(List<LogEntry> mlog, int idx){
      if(connect()){

        uint baseAddress = (uint)((idx*4*4) + LOG_BASE);

        var m = new LogEntry();

        m.id1 =  dspDevice.ReadRegister(baseAddress + 0);
        m.id2 =  dspDevice.ReadRegister(baseAddress + 4);
        m.val1 = dspDevice.ReadRegister(baseAddress + 8);
        m.val2 = dspDevice.ReadRegister(baseAddress + 12);

        mlog.Add(m);

        disconnect();
      }
    }


    public void parseLog(){
      Log mlog = getLogEntries();

      while(mlog.idx < mlog.mlog.Count){

        var head = mlog.mlog[mlog.idx].id1;

        if(head == (int)LogTags.STATE_EN_ELEC){
          parseStateLog(mlog);
        }
        else if(head == (int)LogTags.DAC_STATE_CHANGE){
          uint changedPair = mlog.mlog[mlog.idx].id2;
          uint changedTo   = mlog.mlog[mlog.idx].val1;
          log.info($"Log entry {mlog.idx}\tAt timestep {mlog.mlog[mlog.idx].val2}\tDAC state change");
          if(changedTo == 0){ log.info($"Log entry {mlog.idx}\tAt timestep {mlog.mlog[mlog.idx].val2}\tDAC pair {changedPair} changed state to IDLE"); }
          if(changedTo == 1){ log.info($"Log entry {mlog.idx}\tAt timestep {mlog.mlog[mlog.idx].val2}\tDAC pair {changedPair} changed state to PRIMED"); }
          if(changedTo == 2){ log.info($"Log entry {mlog.idx}\tAt timestep {mlog.mlog[mlog.idx].val2}\tDAC pair {changedPair} changed state to FIRING"); }

          mlog.idx++;
        }
        else if(head == (int)LogTags.CONF){
          log.info($"Log entry {mlog.idx}\tAt timestep {mlog.mlog[mlog.idx].val2}\tDAC pair {mlog.mlog[mlog.idx].val1} entered reset DAC pair");
          mlog.idx++;
        }

        else if(head == (int)LogTags.CONF_START){
          log.info($"Log entry {mlog.idx}\tAt timestep {mlog.mlog[mlog.idx].val2}\tDAC pair {mlog.mlog[mlog.idx].id2} entered configure DAC pair with SG{mlog.mlog[mlog.idx].val1}");
          mlog.idx++;
        }

        else if(head == (int)LogTags.TRIGGER){
          log.info($"Log entry {mlog.idx}\tAt timestep {mlog.mlog[mlog.idx].val2}\tDAC pair {mlog.mlog[mlog.idx].id2} was triggered");
          mlog.idx++;
        }

        else if(head == (int)LogTags.BOOKING){
          log.info($"Log entry {mlog.idx}\tAt timestep {mlog.mlog[mlog.idx].val2}\tSG{mlog.mlog[mlog.idx].id2} requested booking");
          mlog.idx++;
        }

        else if(head == (int)LogTags.BOOKING_FOUND){
          log.info($"Log entry {mlog.idx}\tAt timestep {mlog.mlog[mlog.idx].val2}\tSG{mlog.mlog[mlog.idx].id2} was granted booking with DAC pair {mlog.mlog[mlog.idx].val1}");
          mlog.idx++;
        }

        else{
          log.info($"\nLog entry {mlog.idx}");
          log.info($"id1:      {lookupId(mlog.mlog[mlog.idx].id1)}");
          log.info($"id2:      {lookupId(mlog.mlog[mlog.idx].id2)}");
          log.info($"value1:   {mlog.mlog[mlog.idx].val1:X}");
          log.info($"timestep: {mlog.mlog[mlog.idx].val2}\n");
          mlog.idx++;
        }
      }
    }

    private void parseStateLog(Log mlog){
      uint timestep = mlog.mlog[mlog.idx].val2;
      log.info($"Log entry {mlog.idx} - DSP State at timestep {timestep}");

      log.info($"enabled electrodes 0: {mlog.mlog[mlog.idx++].val1:X}");
      log.info($"enabled electrodes 1: {mlog.mlog[mlog.idx++].val1:X}");

      log.info($"dac select 1:         {mlog.mlog[mlog.idx++].val1:X}");
      log.info($"dac select 2:         {mlog.mlog[mlog.idx++].val1:X}");
      log.info($"dac select 3:         {mlog.mlog[mlog.idx++].val1:X}");
      log.info($"dac select 4:         {mlog.mlog[mlog.idx++].val1:X}");

      log.info($"mode select 1:        {mlog.mlog[mlog.idx++].val1:X}");
      log.info($"mode select 2:        {mlog.mlog[mlog.idx++].val1:X}");
      log.info($"mode select 3:        {mlog.mlog[mlog.idx++].val1:X}");
      log.info($"mode select 4:        {mlog.mlog[mlog.idx++].val1:X}\n");
    }
  }
}

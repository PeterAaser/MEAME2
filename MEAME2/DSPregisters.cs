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
  }
}

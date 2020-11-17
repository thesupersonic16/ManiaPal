﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManiaPal
{
    public class ProcessMemory
    {
        // Handle used to Read or Write to
        public IntPtr ProcessHandle;
        public int BaseAddress = 0;

        public ProcessMemory() { }

        public ProcessMemory(Process process) : this(process.Handle) { }

        public ProcessMemory(IntPtr processHandle)
        {
            ProcessHandle = processHandle;
        }

        public void Attach(Process process)
        {
            ProcessHandle = process.Handle;
        }

        public void Attach(string ProcessName)
        {
            try
            {
                Process[] ProcList = Process.GetProcessesByName(ProcessName);
                if (ProcList.Length == 0)
                    return;
                else
                {
                    ProcessHandle = ProcList[0].Handle;
                    BaseAddress = (int)ProcList[0].MainModule.BaseAddress;
                }
            }
            catch
            {
                Thread.Sleep(100);
            }
        }


        public byte[] Read(int address, int length)
        {
            byte[] buffer = new byte[length];
            ReadProcessMemory(ProcessHandle, (IntPtr)(address + BaseAddress), buffer, (UInt32)buffer.Length, out _);
            return buffer;
        }

        public void Write(int address, byte[] bytes)
        {
            WriteProcessMemory(ProcessHandle, (IntPtr)(address + BaseAddress), bytes, (UInt32)bytes.Length, out _);
        }


        // Writing Methods
        // 1 Byte
        public void WriteByte(int address, byte value)
        {
            Write(address, new byte[] { value });
        }

        // 2 Bytes
        public void WriteShort(int address, short value)
        {
            WriteInt16(address, value);
        }

        public void WriteInt16(int address, short value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        public void WriteUInt16(int address, ushort value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        // 4 Bytes
        public void WriteInteger(int address, int value)
        {
            WriteInt32(address, value);
        }

        public void WriteInt32(int address, int value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        public void WriteUInt32(int address, uint value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        public void WriteFloat(int address, float value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        // 8 Bytes
        public void WriteLong(int address, long value)
        {
            WriteInt64(address, value);
        }

        public void WriteDouble(int address, double value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        public void WriteInt64(int address, long value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        public void WriteUInt64(int address, ulong value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        // Other Types
        public void WriteString(int address, string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            WriteProcessMemory(ProcessHandle, (IntPtr)address, buffer, (uint)buffer.Length, out _);
        }

        public void WriteBytes(int address, byte[] bytes)
        {
            WriteProcessMemory(ProcessHandle, (IntPtr)address, bytes, (uint)bytes.Length, out _);
        }

        // X86 Functions
        public void WriteNOP(int address)
        {
            WriteByte(address, 0x90);
        }

        public void WriteNOPs(int address, int size)
        {
            var bytes = new byte[size];
            for (int i = 0; i < size; ++i)
                bytes[i] = 0x90;
            Write(address, bytes);
        }

        public void WriteJMP(int address, int addressTo)
        {
            var bytes = new byte[5];
            bytes[0] = 0xE9; // jmp
            BitConverter.GetBytes(addressTo - address).CopyTo(bytes, 1);
            Write(address, bytes);
        }

        // Reading Methods
        // 1 Byte
        public byte ReadByte(int address)
        {
            return Read(address, 1)[0];
        }

        // 2 Bytes
        public short ReadShort(int address)
        {
            return ReadInt16(address);
        }

        public short ReadInt16(int address)
        {
            return BitConverter.ToInt16(Read(address, 2), 0);
        }

        public ushort ReadUInt16(int address)
        {
            return BitConverter.ToUInt16(Read(address, 2), 0);
        }

        // 4 Bytes
        public int ReadInteger(int address)
        {
            return ReadInt32(address);
        }

        public int ReadInt32(int address)
        {
            return BitConverter.ToInt32(Read(address, 4), 0);
        }

        public uint ReadUInt32(int address)
        {
            return BitConverter.ToUInt32(Read(address, 4), 0);
        }

        public float ReadFloat(int address)
        {
            return BitConverter.ToSingle(Read(address, 4), 0);
        }

        // 8 Bytes
        public long ReadLong(int address)
        {
            return ReadInt64(address);
        }

        public long ReadInt64(int address)
        {
            return BitConverter.ToInt64(Read(address, 8), 0);
        }

        public ulong ReadUInt64(int address)
        {
            return BitConverter.ToUInt32(Read(address, 8), 0);
        }

        public double ReadDouble(int address)
        {
            return BitConverter.ToDouble(Read(address, 8), 0);
        }

        // Other Types
        public string ReadString(int address, int length = 4)
        {
            return Encoding.ASCII.GetString(Read(address, length));
        }

        public string ReadLengthPrefixedString(int address)
        {
            int length = ReadByte(address);
            return Encoding.ASCII.GetString(Read(address + 1, length));
        }

        // Pinvokes
        [DllImport("kernel32.dll")] private static extern int ReadProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")] private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);


    }
}

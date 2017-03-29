using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public static class ClientUtilities {



    public static int HeaderSize { get { return 9; } }
    public static int UpdateSize { get { return sizeof(float) * 7; } } //pos and rotation


    public static Packet SendJoin()
    {
        //generate json
        JoinJsonHandler joinHandler = new JoinJsonHandler("Mattia");
        string join = JsonUtility.ToJson(joinHandler); //create json

        byte[] data = Encoding.UTF8.GetBytes(join); //get json bytes

        Packet packet = new Packet(data, 0);  //create packet
        ClientManager.EnquePacket(packet);
        Debug.Log("Join added: " + packet.TimeStamp);
        return packet;
    }
    public static Packet SendLeave()
    {
        //generate json
        LeaveJsonHandler leaveHandler = new LeaveJsonHandler("Mattia");
        string leave = JsonUtility.ToJson(leaveHandler); //create json

        byte[] data = Encoding.UTF8.GetBytes(leave); //get json bytes

        byte command = 0;
        command = SetBitOnByte(command, 1, true);

        Packet packet = new Packet(data, command);  //create packet
        ClientManager.EnquePacket(packet);

        return packet;
    }
    public static Packet SendUpdate(Vector3 pos, Quaternion rotation)
    {
        //fill data
        byte[] data = new byte[UpdateSize];
        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, data, 0, sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, data, 4, sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, data, 8, sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(rotation.x), 0, data, 12, sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(rotation.y), 0, data, 16, sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(rotation.z), 0, data, 20, sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(rotation.w), 0, data, 24, sizeof(float));
        byte a = 1;
        byte b = 255;
        byte c = (byte)(a | b);

        //packet creation
        byte command = 0;
        command = SetBitOnByte(command, 0, true);
        command = SetBitOnByte(command, 1, true);

        Packet packet = new Packet(data, 3); //generate packet
        ClientManager.EnquePacket(packet);
        return packet;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="lastByte">Bit mask  (bit1 = reliable)</param>
    /// <returns></returns>
    public static byte[] GetPacketHeader(byte lastByte)
    {
        byte[] header = new byte[ClientUtilities.HeaderSize];
        Buffer.BlockCopy(BitConverter.GetBytes(ClientManager.ID), 0, header, 0, sizeof(int));
        Buffer.BlockCopy(BitConverter.GetBytes(ClientManager.TimeStamp), 0, header, 4, sizeof(float));
        header[header.Length - 1] = lastByte;
        return header;
    }




    public static byte SetBitOnByte(byte b, int pos, bool value)
    {
        return value ? (byte)(b | (1 << pos)) : (byte)(b & ~(1 << pos));
    }
    public static bool IsBitSet(byte b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }
}

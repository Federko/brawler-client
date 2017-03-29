using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.IO;


//0 join, 2 leave, 3 update,
public struct Packet
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data">Payload of packet, header will be created automatically</param>
    public Packet(byte[] data, byte command)
    {
        this.Data = new byte[ClientUtilities.HeaderSize + data.Length]; //create packet
        Header = ClientUtilities.GetPacketHeader(command); //save header
        Buffer.BlockCopy(Header, 0, this.Data, 0, ClientUtilities.HeaderSize);  //put header in packet
        Buffer.BlockCopy(data, 0, this.Data, ClientUtilities.HeaderSize, data.Length);  //put payload in packet
        TimeStamp = ClientManager.TimeStamp;  //save timestamp
        this.Command = command;  //save command byte
    }
    public byte[] Data { get; private set; }
    public float TimeStamp { get; private set; }
    public byte[] Header { get; private set; }
    public byte Command { get; private set; }
    public bool IsReliable { get { return ClientUtilities.IsBitSet(Command, 7); } }  //not tested

    /// <summary>
    /// Create a packet from byte array with header and payload
    /// </summary>
    /// <param name="allData"></param>
    public Packet(byte[] allData, int dataReceived)
    {
        Header = new byte[ClientUtilities.HeaderSize];
        Buffer.BlockCopy(allData, 0, Header, 0, ClientUtilities.HeaderSize);

        Data = new byte[dataReceived - ClientUtilities.HeaderSize];
        Buffer.BlockCopy(allData, ClientUtilities.HeaderSize, Data, 0, Data.Length);

        TimeStamp = BitConverter.ToSingle(Header, 4);

        Command = Header[Header.Length - 1];
    }

    public bool IsEmpty
    {
        get
        {
            return (Data == null && Header == null);
        }
    }

    public static Packet Empty
    {
        get
        {
            return new Packet();
        }
    }

}

public static partial class ClientManager
{

    private static Socket socket;
    private static EndPoint endPoint;

    private static List<Packet> packets;

    public static float TimeStamp { get { return Time.time; } }   


    public static int ID { get; private set; }  //is int or uint?
    public static int MaxPacketsSendedPerLoop { get; private set; }
    /// <summary>
    /// Number of packets sended during passed frame
    /// </summary>
    public static int PacketsSendedLastFrame { get; private set; }



    public static void Init(int maxPacketsSendedPerLoop = 256)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        endPoint = new IPEndPoint(IPAddress.Parse("10.0.0.238"), 20234);
        packets = new List<Packet>();
        MaxPacketsSendedPerLoop = maxPacketsSendedPerLoop;
    }

    public static void Update()
    {
        DequePackets();
    }

    public static void Receive()
    {
        for (int i = 0; i < 20; i++)  //make customizable
        {
            Packet packet = Receive(256);
            if(!packet.IsEmpty)
            {
                //parse packet
                //use utilities to update objects ecc.
            }
        }
    }
    public static void EnquePacket(Packet packet)
    {
        packets.Add(packet);
    }
    private static void DequePackets()
    {
        int packetsSended = 0;

        for (int i = 0; i < packets.Count; i++)
        {
            if (packetsSended >= MaxPacketsSendedPerLoop) //packets will be sended during next loop
            {
                PacketsSendedLastFrame = packetsSended;
                break;
            }
            socket.SendTo(packets[i].Data, endPoint);
            packets.RemoveAt(i);
            Debug.Log("Send packet n. " + i);
            packetsSended++;
        }
    }
    private static Packet Receive(int bufferSize)
    {
        byte[] buffer = new byte[bufferSize];
        try
        {
            int dataReceived = socket.Receive(buffer);

            Packet packet = new Packet(buffer,dataReceived); //generate packet structure from data received
            return packet;
        }
        catch
        {
            return Packet.Empty;
        }
    }
}
